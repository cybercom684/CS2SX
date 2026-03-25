using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using CS2SX.Logging;

namespace CS2SX.Transpiler;

public sealed class CSharpToC : CSharpSyntaxWalker
{
    public enum TranspileMode
    {
        HeaderOnly, Implementation
    }

    private readonly TranspileMode _mode;
    private readonly TranspilerContext _ctx;
    private readonly ExpressionWriter _exprWriter;
    private readonly StatementWriter _stmtWriter;

    private const string SwitchAppBase = "SwitchApp";

    public CSharpToC(TranspileMode mode = TranspileMode.Implementation)
    {
        _mode = mode;
        _ctx = new TranspilerContext(new StringWriter());
        _exprWriter = new ExpressionWriter(_ctx);
        _stmtWriter = new StatementWriter(_ctx, _exprWriter);
    }

    // ── Öffentliche API ───────────────────────────────────────────────────────

    public string Transpile(string csharpSource)
    {
        var tree = CSharpSyntaxTree.ParseText(csharpSource);
        var diags = tree.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        if (diags.Count > 0)
        {
            foreach (var d in diags)
            {
                var loc = d.Location.GetLineSpan();
                var path = string.IsNullOrEmpty(loc.Path) ? "unknown" : loc.Path;
                Log.Error($"CS{d.Id}: {d.GetMessage()} at {path}:{loc.StartLinePosition.Line}");
                _ctx.Out.WriteLine("/* PARSE ERROR: " + d.GetMessage() + " */");
            }
        }

        Visit(tree.GetRoot());
        return _ctx.Out.ToString();
    }

    // ── Namespace-Handling ────────────────────────────────────────────────────

    public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        => base.VisitNamespaceDeclaration(node);

    public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
        => base.VisitFileScopedNamespaceDeclaration(node);

    // ── Enum-Handling ─────────────────────────────────────────────────────────

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        if (_mode == TranspileMode.HeaderOnly)
        {
            _ctx.Out.WriteLine("enum " + node.Identifier.Text);
            _ctx.Out.WriteLine("{");
            _ctx.Indent();
            foreach (var member in node.Members)
            {
                var name = member.Identifier.Text;
                _ctx.EnumMembers.Add(name);

                if (member.EqualsValue != null)
                {
                    var val = _exprWriter.Write(member.EqualsValue.Value);
                    _ctx.Out.WriteLine(name + " = " + val + ",");
                }
                else
                {
                    _ctx.Out.WriteLine(name + ",");
                }
            }
            _ctx.Dedent();
            _ctx.Out.WriteLine("};");
            _ctx.Out.WriteLine();
        }
    }

    // ── Klassen-Handling ──────────────────────────────────────────────────────

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        _ctx.ClearClassContext();
        _ctx.CurrentClass = node.Identifier.Text;
        _ctx.CurrentBaseType = node.BaseList?.Types.FirstOrDefault()?.ToString().Trim()
                               ?? string.Empty;

        var baseType = _ctx.CurrentBaseType;
        var isSwitchAppChild = baseType == SwitchAppBase;

        if (!string.IsNullOrEmpty(baseType) && baseType != SwitchAppBase)
            LoadBaseFields(baseType);

        // FieldTypes immer befüllen — wird in beiden Modi gebraucht
        // (im Header-Modus für korrekte Typ-Auflösung in Expressions)
        CollectFieldTypes(node);

        if (_mode == TranspileMode.HeaderOnly)
        {
            WriteStructDefinition(node, baseType);
            WriteFunctionSignatures(node, isSwitchAppChild);
        }
        else
        {
            WriteConstructor(node, isSwitchAppChild);
            WriteMethodBodies(node);
        }

        _ctx.ClearClassContext();
    }

    // ── Field-Type-Sammlung ───────────────────────────────────────────────────

    private void CollectFieldTypes(ClassDeclarationSyntax node)
    {
        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;

            var csType = field.Declaration.Type.ToString().Trim();
            foreach (var v in field.Declaration.Variables)
                _ctx.FieldTypes[v.Identifier.Text.TrimStart('_')] = csType;
        }
    }

    // ── Struct-Definition (Header) ────────────────────────────────────────────

    private void WriteStructDefinition(ClassDeclarationSyntax node, string? baseType)
    {
        var name = node.Identifier.Text;

        _ctx.Out.WriteLine("struct " + name);
        _ctx.Out.WriteLine("{");
        _ctx.Indent();

        // Basisklasse einbetten
        if (!string.IsNullOrEmpty(baseType))
        {
            // Alle Control-Subklassen embedden Control als erstes Feld
            // damit der Cast (Control*)self funktioniert
            var embedType = baseType is "Label" or "Button" or "ProgressBar"
                ? "Control"
                : baseType;
            _ctx.WriteLine(embedType + " base;");
        }

        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;

            var csType = field.Declaration.Type.ToString().Trim();
            var cType = TypeRegistry.MapType(csType);

            // * anhängen wenn:
            // - NeedsPointerSuffix (eigene Klassen-Typen)
            // - StringBuilder (MapType gibt keinen * zurück)
            // - Control-Typen (Label, Button etc. — MapType gibt keinen * zurück)
            // NICHT für List<T>/Dict<K,V> — MapType gibt bereits * zurück
            var needPtr = TypeRegistry.NeedsPointerSuffix(csType)
                       || TypeRegistry.IsStringBuilder(csType)
                       || TypeRegistry.IsControlType(csType);
            var ptr = needPtr ? "*" : "";

            foreach (var v in field.Declaration.Variables)
            {
                var fieldName = v.Identifier.Text.TrimStart('_');
                var prefix = TypeRegistry.HasNoPrefix(fieldName) ? "" : "f_";
                _ctx.FieldTypes[fieldName] = csType;
                _ctx.WriteLine(cType + ptr + " " + prefix + fieldName + ";");
            }
        }

        foreach (var prop in node.Members.OfType<PropertyDeclarationSyntax>())
        {
            if (prop.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;
            var cType = TypeRegistry.MapType(prop.Type.ToString().Trim());
            _ctx.WriteLine(cType + " f_" + prop.Identifier + ";");
        }

        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
        {
            var isAbstract = method.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
            var isVirtual = method.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword));
            if (!isAbstract && !isVirtual) continue;

            var returnType = TypeRegistry.MapType(method.ReturnType.ToString().Trim());
            var paramTypes = new List<string> { name + "*" };
            foreach (var p in method.ParameterList.Parameters)
            {
                var pt = TypeRegistry.MapType(p.Type!.ToString().Trim());
                var isRef = p.Modifiers.Any(m =>
                    m.IsKind(SyntaxKind.RefKeyword) || m.IsKind(SyntaxKind.OutKeyword));
                paramTypes.Add(isRef ? pt + "*" : pt);
            }
            _ctx.WriteLine(returnType + " (*" + method.Identifier.Text + ")("
                         + string.Join(", ", paramTypes) + ");");
        }

        _ctx.Dedent();
        _ctx.Out.WriteLine("};");
        _ctx.Out.WriteLine();

        // Static-Felder
        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            if (!field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;

            var csType = field.Declaration.Type.ToString().Trim();
            var cType = TypeRegistry.MapType(csType);
            var needPtr = TypeRegistry.NeedsPointerSuffix(csType)
                       || TypeRegistry.IsStringBuilder(csType)
                       || TypeRegistry.IsControlType(csType);
            var ptr = needPtr ? "*" : "";

            foreach (var v in field.Declaration.Variables)
            {
                var fieldName = v.Identifier.Text.TrimStart('_');
                _ctx.Out.WriteLine("extern " + cType + ptr + " " + name + "_" + fieldName + ";");
            }
        }
    }

    // ── Funktions-Signaturen (Header) ─────────────────────────────────────────

    private void WriteFunctionSignatures(ClassDeclarationSyntax node, bool isSwitchAppChild)
    {
        var name = node.Identifier.Text;
        var baseType = _ctx.CurrentBaseType;

        var isControlChild = baseType is "Control" or "Label" or "Button" or "ProgressBar";

        if (isSwitchAppChild)
            _ctx.Out.WriteLine("void " + name + "_Init(" + name + "* self);");
        else if (isControlChild)
            // Controls werden mit malloc allokiert wie andere Heap-Objekte
            _ctx.Out.WriteLine(name + "* " + name + "_New();");
        else
            _ctx.Out.WriteLine(name + "* " + name + "_New();");

        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
            WriteMethodSignature(method, name);

        _ctx.Out.WriteLine();
    }

    // ── Konstruktor (Implementation) ──────────────────────────────────────────

    private void WriteConstructor(ClassDeclarationSyntax node, bool isSwitchAppChild)
    {
        var name = node.Identifier.Text;
        var baseType = _ctx.CurrentBaseType;

        var isControlChild = baseType is "Control" or "Label" or "Button" or "ProgressBar";

        if (isSwitchAppChild)
            WriteInitConstructor(node, name, baseType);
        else if (isControlChild)
            WriteControlConstructor(node, name, baseType);
        else
            WriteNewConstructor(node, name);
    }

    private void WriteControlConstructor(ClassDeclarationSyntax node, string name, string baseType)
    {
        // Static-Felder → globale C-Variablen-Definitionen
        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            if (!field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;

            var csType = field.Declaration.Type.ToString().Trim();
            var cType = TypeRegistry.MapType(csType);
            var needPtr = TypeRegistry.NeedsPointerSuffix(csType)
                       || TypeRegistry.IsStringBuilder(csType)
                       || TypeRegistry.IsList(csType)
                       || TypeRegistry.IsDictionary(csType);
            var ptr = needPtr ? "*" : "";

            foreach (var v in field.Declaration.Variables)
            {
                var fieldName = v.Identifier.Text.TrimStart('_');
                var init = v.Initializer != null
                    ? " = " + _exprWriter.Write(v.Initializer.Value)
                    : "";
                _ctx.Out.WriteLine(cType + ptr + " " + name + "_" + fieldName + init + ";");
            }
        }

        _ctx.Out.WriteLine(name + "* " + name + "_New()");
        _ctx.Out.WriteLine("{");
        _ctx.Indent();

        _ctx.WriteLine(name + "* self = (" + name + "*)malloc(sizeof(" + name + "));");
        _ctx.WriteLine("if (!self) return NULL;");
        _ctx.WriteLine("memset(self, 0, sizeof(" + name + "));");

        // Basis-Control Defaults
        _ctx.WriteLine("((Control*)self)->visible = 1;");
        _ctx.WriteLine("((Control*)self)->focusable = 0;");

        // Draw/Update Funktionszeiger verdrahten
        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
        {
            var methodName = method.Identifier.Text;
            var isOverride = method.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword));
            if (!isOverride) continue;

            if (methodName == "Draw")
                _ctx.WriteLine("((Control*)self)->Draw = (void(*)(Control*))"
                    + name + "_Draw;");

            if (methodName == "Update")
            {
                _ctx.WriteLine("((Control*)self)->Update = (void(*)(Control*, u64, u64))"
                    + name + "_Update;");
                // Hat Update → ist fokussierbar
                _ctx.WriteLine("((Control*)self)->focusable = 1;");
            }
        }

        // Feld-Initializer
        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;

            foreach (var v in field.Declaration.Variables)
            {
                if (v.Initializer == null) continue;
                var fieldName = v.Identifier.Text.TrimStart('_');
                var prefix = TypeRegistry.HasNoPrefix(fieldName) ? "" : "f_";
                _ctx.WriteLine("self->" + prefix + fieldName + " = "
                    + _exprWriter.Write(v.Initializer.Value) + ";");
            }
        }

        _ctx.WriteLine("return self;");
        _ctx.Dedent();
        _ctx.Out.WriteLine("}");
        _ctx.Out.WriteLine();
    }

    private void WriteInitConstructor(ClassDeclarationSyntax node, string name, string baseType)
    {
        // Static-Felder → globale C-Variablen-Definitionen
        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            if (!field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;

            var csType = field.Declaration.Type.ToString().Trim();
            var cType = TypeRegistry.MapType(csType);
            var needPtr = TypeRegistry.NeedsPointerSuffix(csType)
                       || TypeRegistry.IsStringBuilder(csType)
                       || TypeRegistry.IsList(csType)
                       || TypeRegistry.IsDictionary(csType);
            var ptr = needPtr ? "*" : "";

            foreach (var v in field.Declaration.Variables)
            {
                var fieldName = v.Identifier.Text.TrimStart('_');
                var init = v.Initializer != null
                    ? " = " + _exprWriter.Write(v.Initializer.Value)
                    : "";
                _ctx.Out.WriteLine(cType + ptr + " " + name + "_" + fieldName + init + ";");
            }
        }

        _ctx.Out.WriteLine("void " + name + "_Init(" + name + "* self)");
        _ctx.Out.WriteLine("{");
        _ctx.Indent();

        // Basis-Init
        if (!string.IsNullOrEmpty(baseType) && baseType != SwitchAppBase)
        {
            // Control-Subklassen: memset + Basis-Felder setzen
            if (baseType is "Control" or "Label" or "Button" or "ProgressBar")
            {
                _ctx.WriteLine("memset(self, 0, sizeof(" + name + "));");
                _ctx.WriteLine("((Control*)self)->visible = 1;");
            }
            else
            {
                _ctx.WriteLine(baseType + "_Init((" + baseType + "*)self);");
            }
        }

        // SwitchApp Lifecycle-Funktionszeiger verdrahten
        if (baseType == SwitchAppBase)
        {
            foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
            {
                var methodName = method.Identifier.Text;
                var isOverride = method.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword));
                var isAbstract = method.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
                if (!isOverride && !isAbstract) continue;

                if (methodName is "OnInit" or "OnFrame" or "OnExit")
                    _ctx.WriteLine("((SwitchApp*)self)->" + methodName
                        + " = (void(*)(SwitchApp*))" + name + "_" + methodName + ";");
            }
        }

        // Control-Subklassen: Draw/Update Funktionszeiger verdrahten
        if (baseType is "Control" or "Label" or "Button" or "ProgressBar")
        {
            foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
            {
                var methodName = method.Identifier.Text;
                var isOverride = method.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword));
                if (!isOverride) continue;

                if (methodName == "Draw")
                    _ctx.WriteLine("((Control*)self)->Draw = (void(*)(Control*))"
                        + name + "_Draw;");

                if (methodName == "Update")
                    _ctx.WriteLine("((Control*)self)->Update = (void(*)(Control*, u64, u64))"
                        + name + "_Update;");
            }
        }

        // Feld-Initializer
        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;

            foreach (var v in field.Declaration.Variables)
            {
                if (v.Initializer == null) continue;
                var fieldName = v.Identifier.Text.TrimStart('_');
                var prefix = TypeRegistry.HasNoPrefix(fieldName) ? "" : "f_";
                _ctx.WriteLine("self->" + prefix + fieldName + " = "
                    + _exprWriter.Write(v.Initializer.Value) + ";");
            }
        }

        _ctx.Dedent();
        _ctx.Out.WriteLine("}");
        _ctx.Out.WriteLine();
    }

    private void WriteNewConstructor(ClassDeclarationSyntax node, string name)
    {
        // Static-Felder → globale C-Variablen-Definitionen (nicht im Konstruktor)
        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            if (!field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;

            var csType = field.Declaration.Type.ToString().Trim();
            var cType = TypeRegistry.MapType(csType);
            var needPtr = TypeRegistry.NeedsPointerSuffix(csType);
            var ptr = needPtr ? "*" : "";

            foreach (var v in field.Declaration.Variables)
            {
                var fieldName = v.Identifier.Text.TrimStart('_');
                var init = v.Initializer != null
                    ? " = " + _exprWriter.Write(v.Initializer.Value)
                    : "";
                _ctx.Out.WriteLine(cType + ptr + " " + name + "_" + fieldName + init + ";");
            }
        }

        _ctx.Out.WriteLine(name + "* " + name + "_New()");
        _ctx.Out.WriteLine("{");
        _ctx.Indent();
        _ctx.WriteLine(name + "* self = (" + name + "*)malloc(sizeof(" + name + "));");
        _ctx.WriteLine("memset(self, 0, sizeof(" + name + "));");

        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            // Static-Felder nicht im Konstruktor initialisieren
            if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;

            foreach (var v in field.Declaration.Variables)
            {
                if (v.Initializer == null) continue;
                var fieldName = v.Identifier.Text.TrimStart('_');
                var prefix = TypeRegistry.HasNoPrefix(fieldName) ? "" : "f_";
                _ctx.WriteLine("self->" + prefix + fieldName + " = "
                    + _exprWriter.Write(v.Initializer.Value) + ";");
            }
        }

        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
        {
            var isOverride = method.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword));
            var isVirtual = method.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword));
            if (!isOverride && !isVirtual) continue;

            var returnType = TypeRegistry.MapType(method.ReturnType.ToString().Trim());
            var paramTypes = new List<string> { name + "*" };
            foreach (var p in method.ParameterList.Parameters)
                paramTypes.Add(TypeRegistry.MapType(p.Type!.ToString().Trim()));

            var castSig = returnType + "(*)(" + string.Join(", ", paramTypes) + ")";
            _ctx.WriteLine("self->base." + method.Identifier.Text
                + " = (" + castSig + ")" + name + "_" + method.Identifier.Text + ";");
        }

        _ctx.WriteLine("return self;");
        _ctx.Dedent();
        _ctx.Out.WriteLine("}");
        _ctx.Out.WriteLine();
    }

    // ── Methoden-Signaturen ───────────────────────────────────────────────────

    private void WriteMethodSignature(MethodDeclarationSyntax method, string className)
    {
        var isAbstract = method.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
        var isStatic = method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
        var returnType = TypeRegistry.MapType(method.ReturnType.ToString().Trim());
        var name = className + "_" + method.Identifier.Text;

        var parameters = new List<string>();
        if (!isStatic) parameters.Add(className + "* self");
        foreach (var p in method.ParameterList.Parameters)
            parameters.Add(BuildParamDecl(p));

        var sig = returnType + " " + name + "(" + string.Join(", ", parameters) + ")";

        if (isAbstract)
            _ctx.Out.WriteLine("/* abstract: " + sig + " */");
        else
            _ctx.Out.WriteLine(sig + ";");
    }

    // ── Methoden-Bodies ───────────────────────────────────────────────────────

    private void WriteMethodBodies(ClassDeclarationSyntax node)
    {
        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
            VisitMethodDeclaration(method);
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        _ctx.MethodReturnTypes[node.Identifier.Text] = node.ReturnType.ToString().Trim();

        var isAbstract = node.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
        var isStatic = node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
        var returnType = TypeRegistry.MapType(node.ReturnType.ToString().Trim());

        var name = string.IsNullOrEmpty(_ctx.CurrentClass)
            ? node.Identifier.Text
            : _ctx.CurrentClass + "_" + node.Identifier.Text;

        var parameters = new List<string>();
        // Static-Methoden bekommen kein self
        if (!string.IsNullOrEmpty(_ctx.CurrentClass) && !isStatic)
            parameters.Add(_ctx.CurrentClass + "* self");
        foreach (var p in node.ParameterList.Parameters)
            parameters.Add(BuildParamDecl(p));

        var sig = returnType + " " + name + "(" + string.Join(", ", parameters) + ")";

        if (_mode == TranspileMode.HeaderOnly)
        {
            if (isAbstract) _ctx.Out.WriteLine("/* abstract: " + sig + " */");
            else _ctx.Out.WriteLine(sig + ";");
            return;
        }

        if (isAbstract) return;

        _ctx.ClearMethodContext();
        _ctx.Out.WriteLine(sig);
        _ctx.Out.WriteLine("{");
        _ctx.Indent();

        if (node.Body != null)
            foreach (var stmt in node.Body.Statements)
                _stmtWriter.Write(stmt);
        else if (node.ExpressionBody != null)
            _ctx.WriteLine("return " + _exprWriter.Write(node.ExpressionBody.Expression) + ";");

        _ctx.Dedent();
        _ctx.Out.WriteLine("}");
        _ctx.Out.WriteLine();
    }

    // ── Property-Handling ─────────────────────────────────────────────────────

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        _ctx.PropertyTypes[node.Identifier.Text] = node.Type.ToString().Trim();
        base.VisitPropertyDeclaration(node);
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    private void LoadBaseFields(string baseType)
    {
        if (baseType is "Control" or "Label" or "Button" or "ProgressBar")
        {
            foreach (var f in TypeRegistry.ControlFields)
                _ctx.BaseFieldTypes[f] = "int";
        }

        if (baseType is "Button")
        {
            _ctx.BaseFieldTypes["focused"] = "int";
            _ctx.BaseFieldTypes["OnClick"] = "Action";
            _ctx.BaseFieldTypes["text"] = "string";
        }

        if (baseType is "Label")
        {
            _ctx.BaseFieldTypes["text"] = "string";
        }

        if (baseType is "ProgressBar")
        {
            _ctx.BaseFieldTypes["value"] = "int";
            _ctx.BaseFieldTypes["width_chars"] = "int";
        }
    }

    private static string BuildParamDecl(ParameterSyntax p)
    {
        if (p.Type == null) return p.Identifier.Text;

        var csType = p.Type.ToString().Trim();
        var cType = TypeRegistry.MapType(csType);
        var isPrim = TypeRegistry.IsPrimitive(csType) || csType == "string";
        var isRef = p.Modifiers.Any(m =>
            m.IsKind(SyntaxKind.RefKeyword) || m.IsKind(SyntaxKind.OutKeyword));
        var ptr = (!isPrim || isRef) ? "*" : "";
        return cType + ptr + " " + p.Identifier;
    }
}