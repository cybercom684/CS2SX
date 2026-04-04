using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using CS2SX.Transpiler.Strategies;
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
    private string _sourceFilePath = string.Empty;

    private const string SwitchAppBase = "SwitchApp";

    private readonly IConstructorStrategy[] _constructorStrategies;

    public CSharpToC(TranspileMode mode = TranspileMode.Implementation)
    {
        _mode = mode;
        _ctx = new TranspilerContext(new StringWriter());
        _exprWriter = new ExpressionWriter(_ctx);
        _stmtWriter = new StatementWriter(_ctx, _exprWriter);

        _constructorStrategies =
        [
            new SwitchAppConstructorStrategy(),
            new ControlSubclassConstructorStrategy(),
            new DefaultConstructorStrategy(),
        ];
    }

    // ── Öffentliche API ───────────────────────────────────────────────────

    /// <summary>
    /// Transpiliert eine C#-Quelldatei.
    /// </summary>
    /// <param name="csharpSource">Quellcode der Datei.</param>
    /// <param name="filePath">Optionaler Dateipfad für Fehlermeldungen.</param>
    /// <param name="semanticModel">
    /// Optionales SemanticModel für exakte Typ-Inferenz.
    /// Wenn null, werden syntaktische Heuristiken verwendet.
    /// </param>
    public string Transpile(
        string csharpSource,
        string? filePath = null,
        SemanticModel? semanticModel = null)
    {
        _sourceFilePath = filePath ?? string.Empty;
        _ctx.CurrentFile = System.IO.Path.GetFileName(_sourceFilePath);
        _ctx.SemanticModel = semanticModel;

        var tree = semanticModel?.SyntaxTree
            ?? CSharpSyntaxTree.ParseText(csharpSource);

        var diags = tree.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        if (diags.Count > 0)
            foreach (var d in diags)
            {
                var lineSpan = d.Location.GetLineSpan();
                var line = lineSpan.StartLinePosition.Line + 1;
                Log.Error($"{_ctx.CurrentFile}({line}): CS{d.Id}: {d.GetMessage()}");
            }

        Visit(tree.GetRoot());
        return _ctx.Out.ToString();
    }

    // ── Namespace ─────────────────────────────────────────────────────────

    public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        => base.VisitNamespaceDeclaration(node);

    public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
        => base.VisitFileScopedNamespaceDeclaration(node);

    // ── Enum ──────────────────────────────────────────────────────────────

    public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        if (_mode != TranspileMode.HeaderOnly) return;

        _ctx.Out.WriteLine("enum " + node.Identifier.Text);
        _ctx.Out.WriteLine("{");
        _ctx.Indent();
        foreach (var member in node.Members)
        {
            var name = member.Identifier.Text;
            _ctx.EnumMembers.Add(name);
            if (member.EqualsValue != null)
                _ctx.Out.WriteLine(name + " = " + _exprWriter.Write(member.EqualsValue.Value) + ",");
            else
                _ctx.Out.WriteLine(name + ",");
        }
        _ctx.Dedent();
        _ctx.Out.WriteLine("};");
        _ctx.Out.WriteLine();
    }

    // ── Klassen ───────────────────────────────────────────────────────────

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        _ctx.ClearClassContext();
        _ctx.CurrentClass = node.Identifier.Text;
        _ctx.CurrentBaseType = node.BaseList?.Types.FirstOrDefault()?.ToString().Trim()
                               ?? string.Empty;

        var lineSpan = node.GetLocation().GetLineSpan();
        _ctx.CurrentLine = lineSpan.StartLinePosition.Line + 1;

        var baseType = _ctx.CurrentBaseType;
        var isSwitchAppChild = baseType == SwitchAppBase;

        if (!string.IsNullOrEmpty(baseType) && baseType != SwitchAppBase)
            LoadBaseFields(baseType);

        CollectFieldTypes(node);

        if (_mode == TranspileMode.HeaderOnly)
        {
            if (VTableBuilder.HasVirtualMethods(node))
                VTableBuilder.WriteVTableStruct(node, node.Identifier.Text, _ctx.Out);

            WriteStructDefinition(node, baseType);
            WriteFunctionSignatures(node, isSwitchAppChild);

            foreach (var prop in node.Members.OfType<PropertyDeclarationSyntax>())
                if (!PropertyWriter.IsAutoProperty(prop))
                    PropertyWriter.WriteSignatures(prop, node.Identifier.Text, _ctx.Out);
        }
        else
        {
            WriteConstructor(node);
            WriteMethodBodies(node);

            foreach (var prop in node.Members.OfType<PropertyDeclarationSyntax>())
                if (!PropertyWriter.IsAutoProperty(prop))
                    PropertyWriter.WriteImplementations(prop, node.Identifier.Text, _ctx, _exprWriter, _stmtWriter);

            if (!string.IsNullOrEmpty(baseType) && baseType != SwitchAppBase
                && !IsControlSubclass(baseType))
            {
                VTableBuilder.WriteVTableInstance(node, node.Identifier.Text, baseType, _ctx.Out);
            }
        }

        _ctx.ClearClassContext();
    }

    // ── Field-Type-Sammlung ───────────────────────────────────────────────

    private void CollectFieldTypes(ClassDeclarationSyntax node)
    {
        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;

            // SemanticModel für Typ-Bestimmung nutzen
            var csType = ResolveFieldType(field);

            foreach (var v in field.Declaration.Variables)
                _ctx.FieldTypes[v.Identifier.Text.TrimStart('_')] = csType;
        }

        foreach (var prop in node.Members.OfType<PropertyDeclarationSyntax>())
        {
            if (prop.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;

            var csType = ResolvePropertyType(prop);
            _ctx.PropertyTypes[prop.Identifier.Text] = csType;
            _ctx.FieldTypes[prop.Identifier.Text] = csType;
        }
    }

    private string ResolveFieldType(FieldDeclarationSyntax field)
    {
        // SemanticModel: exakter Typ
        if (_ctx.SemanticModel != null)
        {
            try
            {
                var typeSymbol = _ctx.SemanticModel.GetTypeInfo(field.Declaration.Type).Type;
                if (typeSymbol != null && typeSymbol is not IErrorTypeSymbol)
                    return TranspilerContext.FormatTypeSymbol(typeSymbol);
            }
            catch { }
        }

        return field.Declaration.Type.ToString().Trim();
    }

    private string ResolvePropertyType(PropertyDeclarationSyntax prop)
    {
        if (_ctx.SemanticModel != null)
        {
            try
            {
                var typeSymbol = _ctx.SemanticModel.GetTypeInfo(prop.Type).Type;
                if (typeSymbol != null && typeSymbol is not IErrorTypeSymbol)
                    return TranspilerContext.FormatTypeSymbol(typeSymbol);
            }
            catch { }
        }

        return prop.Type.ToString().Trim();
    }

    // ── Struct-Definition (Header) ────────────────────────────────────────

    private void WriteStructDefinition(ClassDeclarationSyntax node, string? baseType)
    {
        var name = node.Identifier.Text;

        _ctx.Out.WriteLine("struct " + name);
        _ctx.Out.WriteLine("{");
        _ctx.Indent();

        if (!string.IsNullOrEmpty(baseType) && baseType != SwitchAppBase
            && !IsControlSubclass(baseType))
        {
            _ctx.WriteLine(baseType + "_vtable* vtable;");
        }

        if (!string.IsNullOrEmpty(baseType))
        {
            var embedType = baseType is "Label" or "Button" or "ProgressBar"
                ? "Control"
                : baseType;
            _ctx.WriteLine(embedType + " base;");
        }

        WriteInstanceFieldDeclarations(node);
        WritePropertyDeclarations(node);
        WriteVirtualMethodPointers(node, name);

        _ctx.Dedent();
        _ctx.Out.WriteLine("};");
        _ctx.Out.WriteLine();

        WriteStaticFieldExterns(node, name);
    }

    private void WriteInstanceFieldDeclarations(ClassDeclarationSyntax node)
    {
        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;

            var csType = ResolveFieldType(field);
            var cType = TypeRegistry.MapType(csType);
            var needPtr = TypeRegistry.NeedsPointerSuffix(csType)
                       || TypeRegistry.IsStringBuilder(csType)
                       || TypeRegistry.IsControlType(csType)
                       || NullableHandler.IsNullable(csType);
            var ptr = needPtr ? "*" : "";

            foreach (var v in field.Declaration.Variables)
            {
                var fieldName = v.Identifier.Text.TrimStart('_');
                var prefix = TypeRegistry.HasNoPrefix(fieldName) ? "" : "f_";
                _ctx.FieldTypes[fieldName] = csType;

                if (NullableHandler.IsNullable(csType))
                {
                    var inner = NullableHandler.GetInnerType(csType);
                    var innerC = TypeRegistry.MapType(inner);
                    _ctx.WriteLine(innerC + "* " + prefix + fieldName + ";");
                }
                else
                {
                    _ctx.WriteLine(cType + ptr + " " + prefix + fieldName + ";");
                }
            }
        }
    }

    private void WritePropertyDeclarations(ClassDeclarationSyntax node)
    {
        foreach (var prop in node.Members.OfType<PropertyDeclarationSyntax>())
        {
            if (prop.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;
            if (!PropertyWriter.IsAutoProperty(prop)) continue;

            var csType = ResolvePropertyType(prop);
            var cType = TypeRegistry.MapType(csType);
            _ctx.WriteLine(cType + " f_" + prop.Identifier + ";");
        }
    }

    private void WriteVirtualMethodPointers(ClassDeclarationSyntax node, string name)
    {
        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
        {
            var isAbstract = method.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
            var isVirtual  = method.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword));
            if (!isAbstract && !isVirtual) continue;

            var returnType = ResolveMethodReturnType(method);
            var paramTypes = new List<string> { name + "*" };
            foreach (var p in method.ParameterList.Parameters)
            {
                var pt = ResolveParamType(p);
                var isRef = p.Modifiers.Any(m =>
                    m.IsKind(SyntaxKind.RefKeyword) || m.IsKind(SyntaxKind.OutKeyword));
                paramTypes.Add(isRef ? pt + "*" : pt);
            }
            _ctx.WriteLine(returnType + " (*" + method.Identifier.Text + ")("
                         + string.Join(", ", paramTypes) + ");");
        }
    }

    // ── Typ-Auflösung für Methoden-Signaturen ─────────────────────────────

    private string ResolveMethodReturnType(MethodDeclarationSyntax method)
    {
        if (_ctx.SemanticModel != null)
        {
            try
            {
                var sym = _ctx.SemanticModel.GetDeclaredSymbol(method);
                if (sym != null)
                    return TypeRegistry.MapType(TranspilerContext.FormatTypeSymbol(sym.ReturnType));
            }
            catch { }
        }
        return TypeRegistry.MapType(method.ReturnType.ToString().Trim());
    }

    private string ResolveParamType(ParameterSyntax p)
    {
        if (p.Type == null) return "int";

        if (_ctx.SemanticModel != null)
        {
            try
            {
                var typeInfo = _ctx.SemanticModel.GetTypeInfo(p.Type);
                if (typeInfo.Type != null && typeInfo.Type is not IErrorTypeSymbol)
                    return TypeRegistry.MapType(TranspilerContext.FormatTypeSymbol(typeInfo.Type));
            }
            catch { }
        }
        return TypeRegistry.MapType(p.Type.ToString().Trim());
    }

    // ── Static-Felder ─────────────────────────────────────────────────────

    internal void WriteStaticFieldDefinitions(ClassDeclarationSyntax node, string name)
    {
        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            if (!field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;

            var csType = ResolveFieldType(field);
            var cType = TypeRegistry.MapType(csType);
            var needPtr = NeedsPtr(csType);
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
    }

    private void WriteStaticFieldExterns(ClassDeclarationSyntax node, string name)
    {
        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            if (!field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;

            var csType = ResolveFieldType(field);
            var cType = TypeRegistry.MapType(csType);
            var ptr = NeedsPtr(csType) ? "*" : "";

            foreach (var v in field.Declaration.Variables)
            {
                var fieldName = v.Identifier.Text.TrimStart('_');
                _ctx.Out.WriteLine("extern " + cType + ptr + " " + name + "_" + fieldName + ";");
            }
        }
    }

    // ── Feld-Initializer ─────────────────────────────────────────────────

    internal void WriteInstanceFieldInitializers(ClassDeclarationSyntax node)
    {
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
    }

    // ── Konstruktor ───────────────────────────────────────────────────────

    private void WriteConstructor(ClassDeclarationSyntax node)
    {
        var name = node.Identifier.Text;
        var baseType = _ctx.CurrentBaseType;

        var strategy = _constructorStrategies.First(s => s.Matches(node, baseType));
        strategy.Write(node, name, baseType, _ctx, _exprWriter, this);
    }

    // ── Funktions-Signaturen (Header) ─────────────────────────────────────

    private void WriteFunctionSignatures(ClassDeclarationSyntax node, bool isSwitchAppChild)
    {
        var name = node.Identifier.Text;

        if (isSwitchAppChild)
            _ctx.Out.WriteLine("void " + name + "_Init(" + name + "* self);");
        else
            _ctx.Out.WriteLine(name + "* " + name + "_New();");

        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
            WriteMethodSignature(method, name);

        _ctx.Out.WriteLine();
    }

    // ── Methoden-Signaturen ───────────────────────────────────────────────

    private void WriteMethodSignature(MethodDeclarationSyntax method, string className)
    {
        var isAbstract = method.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
        var isStatic   = method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
        var returnType = ResolveMethodReturnType(method);
        var name       = className + "_" + method.Identifier.Text;

        var parameters = new List<string>();
        if (!isStatic) parameters.Add(className + "* self");
        foreach (var p in method.ParameterList.Parameters)
            parameters.Add(BuildParamDecl(p));

        var sig = returnType + " " + name + "(" + string.Join(", ", parameters) + ")";

        if (isAbstract) _ctx.Out.WriteLine("/* abstract: " + sig + " */");
        else            _ctx.Out.WriteLine(sig + ";");
    }

    // ── Methoden-Bodies ───────────────────────────────────────────────────

    private void WriteMethodBodies(ClassDeclarationSyntax node)
    {
        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
            VisitMethodDeclaration(method);
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var lineSpan = node.GetLocation().GetLineSpan();
        _ctx.CurrentLine = lineSpan.StartLinePosition.Line + 1;

        // Rückgabetyp über SemanticModel bestimmen
        var csReturnType = node.ReturnType.ToString().Trim();
        if (_ctx.SemanticModel != null)
        {
            try
            {
                var sym = _ctx.SemanticModel.GetDeclaredSymbol(node);
                if (sym != null)
                    csReturnType = TranspilerContext.FormatTypeSymbol(sym.ReturnType);
            }
            catch { }
        }
        _ctx.MethodReturnTypes[node.Identifier.Text] = csReturnType;

        var isAbstract = node.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
        var isStatic   = node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
        var returnType = TypeRegistry.MapType(csReturnType);

        var name = string.IsNullOrEmpty(_ctx.CurrentClass)
            ? node.Identifier.Text
            : _ctx.CurrentClass + "_" + node.Identifier.Text;

        var parameters = new List<string>();
        if (!string.IsNullOrEmpty(_ctx.CurrentClass) && !isStatic)
            parameters.Add(_ctx.CurrentClass + "* self");
        foreach (var p in node.ParameterList.Parameters)
            parameters.Add(BuildParamDecl(p));

        var sig = returnType + " " + name + "(" + string.Join(", ", parameters) + ")";

        if (_mode == TranspileMode.HeaderOnly)
        {
            if (isAbstract) _ctx.Out.WriteLine("/* abstract: " + sig + " */");
            else            _ctx.Out.WriteLine(sig + ";");
            return;
        }

        if (isAbstract) return;

        _ctx.ClearMethodContext();

        foreach (var p in node.ParameterList.Parameters)
        {
            if (p.Type == null) continue;

            // SemanticModel für Parameter-Typen
            string paramType;
            if (_ctx.SemanticModel != null)
            {
                try
                {
                    var typeInfo = _ctx.SemanticModel.GetTypeInfo(p.Type);
                    paramType = typeInfo.Type != null && typeInfo.Type is not IErrorTypeSymbol
                        ? TranspilerContext.FormatTypeSymbol(typeInfo.Type)
                        : p.Type.ToString().Trim();
                }
                catch
                {
                    paramType = p.Type.ToString().Trim();
                }
            }
            else
            {
                paramType = p.Type.ToString().Trim();
            }

            _ctx.LocalTypes[p.Identifier.Text] = paramType;
        }

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

    // ── Property ─────────────────────────────────────────────────────────

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        var csType = ResolvePropertyType(node);
        _ctx.PropertyTypes[node.Identifier.Text] = csType;
        _ctx.FieldTypes[node.Identifier.Text] = csType;
        base.VisitPropertyDeclaration(node);
    }

    // ── Utilities ─────────────────────────────────────────────────────────

    internal void LoadBaseFields(string baseType)
    {
        if (baseType is "Control" or "Label" or "Button" or "ProgressBar")
            foreach (var f in TypeRegistry.ControlFields)
                _ctx.BaseFieldTypes[f] = "int";

        if (baseType is "Button")
        {
            _ctx.BaseFieldTypes["focused"] = "int";
            _ctx.BaseFieldTypes["OnClick"] = "Action";
            _ctx.BaseFieldTypes["text"]    = "string";
        }
        if (baseType is "Label")
            _ctx.BaseFieldTypes["text"] = "string";
        if (baseType is "ProgressBar")
        {
            _ctx.BaseFieldTypes["value"]       = "int";
            _ctx.BaseFieldTypes["width_chars"] = "int";
        }
    }

    internal static bool IsControlSubclass(string baseType) =>
        baseType is "Control" or "Label" or "Button" or "ProgressBar";

    internal static bool NeedsPtr(string csType) =>
        TypeRegistry.NeedsPointerSuffix(csType)
     || TypeRegistry.IsStringBuilder(csType)
     || TypeRegistry.IsList(csType)
     || TypeRegistry.IsDictionary(csType)
     || TypeRegistry.IsControlType(csType)
     || NullableHandler.IsNullable(csType);

    internal string BuildParamDecl(ParameterSyntax p)
    {
        if (p.Type == null) return p.Identifier.Text;

        var csType = p.Type.ToString().Trim();

        // SemanticModel für exakten Typ
        if (_ctx.SemanticModel != null)
        {
            try
            {
                var typeInfo = _ctx.SemanticModel.GetTypeInfo(p.Type);
                if (typeInfo.Type != null && typeInfo.Type is not IErrorTypeSymbol)
                    csType = TranspilerContext.FormatTypeSymbol(typeInfo.Type);
            }
            catch { }
        }

        if (NullableHandler.IsNullable(csType))
        {
            var inner  = NullableHandler.GetInnerType(csType);
            var cInner = TypeRegistry.MapType(inner);
            return cInner + "* " + p.Identifier;
        }

        var cType  = TypeRegistry.MapType(csType);
        var isPrim = TypeRegistry.IsPrimitive(csType) || csType == "string";
        var isRef  = p.Modifiers.Any(m =>
            m.IsKind(SyntaxKind.RefKeyword) || m.IsKind(SyntaxKind.OutKeyword));
        var ptr = (!isPrim || isRef) ? "*" : "";
        return cType + ptr + " " + p.Identifier;
    }
}