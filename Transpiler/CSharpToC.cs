using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using CS2SX.Transpiler.Strategies;
using CS2SX.Transpiler.Handlers;
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

    // ── NEU: Generic/Interface/Extension-Support ──────────────────────────────
    private readonly GenericInstantiationCollector? _genericCollector;
    private readonly InterfaceExpander? _interfaceExpander;
    private readonly ExtensionMethodHandler _extensionHandler;

    // ── Konstruktoren ─────────────────────────────────────────────────────────

    /// <summary>
    /// Original-Konstruktor (Rückwärtskompatibilität).
    /// </summary>
    public CSharpToC(TranspileMode mode = TranspileMode.Implementation)
    {
        _mode = mode;
        _ctx = new TranspilerContext(new StringWriter());
        _extensionHandler = new ExtensionMethodHandler();
        _exprWriter = new ExpressionWriter(_ctx, _extensionHandler);
        _stmtWriter = new StatementWriter(_ctx, _exprWriter);
        _constructorStrategies = BuildStrategies();
    }

    /// <summary>
    /// NEU: Konstruktor mit vollständigem Generic/Interface/Extension-Support.
    /// Wird von BuildPipeline nach dem GenericInstantiationCollector-Pass genutzt.
    /// </summary>
    public CSharpToC(
        TranspileMode mode,
        GenericInstantiationCollector collector,
        InterfaceExpander interfaceExpander)
    {
        _mode = mode;
        _ctx = new TranspilerContext(new StringWriter());
        _genericCollector = collector;
        _interfaceExpander = interfaceExpander;
        _extensionHandler = new ExtensionMethodHandler(collector.ExtensionMethods);
        _exprWriter = new ExpressionWriter(_ctx, _extensionHandler);
        _stmtWriter = new StatementWriter(_ctx, _exprWriter);
        _constructorStrategies = BuildStrategies();

        // Interface-Namen in den Context laden damit ExpressionWriter.TryWriteVirtualCall
        // Interface-Variablen korrekt als vtable-Wrapper dispatcht.
        foreach (var ifaceName in collector.Interfaces.Keys)
            _ctx.InterfaceTypes.Add(ifaceName);
    }

    private IConstructorStrategy[] BuildStrategies() =>
    [
        new SwitchAppConstructorStrategy(),
        new ControlSubclassConstructorStrategy(),
        new DefaultConstructorStrategy(),
    ];

    // ── Öffentliche API ───────────────────────────────────────────────────────

    public TranspileResult Transpile(
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
            .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .ToList();

        if (diags.Count > 0)
            foreach (var d in diags)
            {
                var lineSpan = d.Location.GetLineSpan();
                var line = lineSpan.StartLinePosition.Line + 1;
                Log.Error($"{_ctx.CurrentFile}({line}): CS{d.Id}: {d.GetMessage()}");
            }

        Visit(tree.GetRoot());

        return new TranspileResult(
            _ctx.Out.ToString(),
            _ctx.Diagnostics.All);
    }

    // ── Namespace ─────────────────────────────────────────────────────────────

    public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        => base.VisitNamespaceDeclaration(node);

    public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
        => base.VisitFileScopedNamespaceDeclaration(node);

    // ── Enum ──────────────────────────────────────────────────────────────────

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

    // ── Klassen ───────────────────────────────────────────────────────────────

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        _ctx.ClearClassContext();
        _ctx.CurrentClass = node.Identifier.Text;
        _ctx.CurrentBaseType = node.BaseList?.Types.FirstOrDefault()?.ToString().Trim()
                               ?? string.Empty;

        var lineSpan = node.GetLocation().GetLineSpan();
        _ctx.CurrentLine = lineSpan.StartLinePosition.Line + 1;

        // NEU: Generische Klassen überspringen wenn ein Collector vorhanden ist
        // Sie werden vom GenericExpander separat behandelt
        if (_genericCollector != null
            && node.TypeParameterList?.Parameters.Count > 0)
        {
            Log.Debug($"CSharpToC: Generische Klasse '{node.Identifier.Text}' übersprungen (wird von GenericExpander behandelt)");
            return;
        }

        // NEU: Extension-Klassen (static class mit extension methods) im Header
        // als Funktions-Signaturen ausgeben
        bool isExtensionClass = node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))
            && node.Members.OfType<MethodDeclarationSyntax>().Any(m =>
                m.ParameterList.Parameters.FirstOrDefault()?.Modifiers
                    .Any(mod => mod.IsKind(SyntaxKind.ThisKeyword)) == true);

        var baseType = _ctx.CurrentBaseType;
        var isSwitchAppChild = baseType == SwitchAppBase;
        var isStaticClass = node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

        if (!string.IsNullOrEmpty(baseType) && baseType != SwitchAppBase)
            LoadBaseFields(baseType);

        CollectFieldTypes(node);

        if (VTableBuilder.HasVirtualMethods(node))
            _ctx.VTableTypes.Add(node.Identifier.Text);

        if (!string.IsNullOrEmpty(baseType)
            && _ctx.VTableTypes.Contains(baseType)
            && !IsControlSubclass(baseType)
            && baseType != SwitchAppBase)
        {
            _ctx.VTableTypes.Add(node.Identifier.Text);
        }

        // NEU: Interface-Implementierungen in VTableTypes registrieren
        if (_interfaceExpander != null
            && _interfaceExpander.ClassInterfaces.ContainsKey(node.Identifier.Text))
        {
            _ctx.VTableTypes.Add(node.Identifier.Text);
        }

        if (_mode == TranspileMode.HeaderOnly)
        {
            if (!isStaticClass && VTableBuilder.HasVirtualMethods(node))
                VTableBuilder.WriteVTableStruct(node, node.Identifier.Text, _ctx.Out);

            if (!isStaticClass)
                WriteStructDefinition(node, baseType);

            WriteFunctionSignatures(node, isSwitchAppChild, isStaticClass);

            foreach (var prop in node.Members.OfType<PropertyDeclarationSyntax>())
                if (!PropertyWriter.IsAutoProperty(prop))
                    PropertyWriter.WriteSignatures(prop, node.Identifier.Text, _ctx.Out);

            // NEU: Interface-vtable-Deklarationen im Header
            if (_interfaceExpander != null)
            {
                var ifaceDecls = _interfaceExpander.ExpandClassVTableDeclarations(node.Identifier.Text);
                if (!string.IsNullOrEmpty(ifaceDecls))
                    _ctx.Out.WriteLine(ifaceDecls);
            }
        }
        else
        {
            if (!isStaticClass)
                WriteConstructor(node);

            WriteMethodBodies(node);

            foreach (var prop in node.Members.OfType<PropertyDeclarationSyntax>())
                if (!PropertyWriter.IsAutoProperty(prop))
                    PropertyWriter.WriteImplementations(prop, node.Identifier.Text, _ctx, _exprWriter, _stmtWriter);

            if (!isStaticClass
                && !string.IsNullOrEmpty(baseType)
                && baseType != SwitchAppBase
                && !IsControlSubclass(baseType))
            {
                VTableBuilder.WriteVTableInstance(node, node.Identifier.Text, baseType, _ctx.Out);
            }

            // NEU: Interface-vtable-Instanzen nach den Methoden ausgeben
            if (_interfaceExpander != null)
            {
                var ifaceImpls = _interfaceExpander.ExpandClassVTableInstances(
                    node.Identifier.Text, node);
                if (!string.IsNullOrEmpty(ifaceImpls))
                    _ctx.Out.WriteLine(ifaceImpls);
            }
        }

        _ctx.ClearClassContext();
    }

    public override void VisitStructDeclaration(StructDeclarationSyntax node)
    {
        var structName = node.Identifier.Text;

        _ctx.ValueTypeStructs.Add(structName);

        if (_mode == TranspileMode.HeaderOnly)
        {
            _ctx.Out.WriteLine("typedef struct " + structName + " " + structName + ";");
            var sw = new StructWriter(_ctx, _exprWriter, _stmtWriter);
            sw.WriteHeaderDecl(node);
        }
        else
        {
            _ctx.ClearClassContext();
            _ctx.CurrentClass = structName;

            foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
            {
                var csType = field.Declaration.Type.ToString().Trim();
                foreach (var v in field.Declaration.Variables)
                    _ctx.FieldTypes[v.Identifier.Text] = csType;
            }
            foreach (var prop in node.Members.OfType<PropertyDeclarationSyntax>())
                _ctx.FieldTypes[prop.Identifier.Text] = prop.Type.ToString().Trim();

            var sw = new StructWriter(_ctx, _exprWriter, _stmtWriter);
            sw.WriteImpl(node);

            _ctx.ClearClassContext();
        }
    }

    public TranspilerContext GetContext() => _ctx;

    // ── Field-Type-Sammlung ───────────────────────────────────────────────────

    private void CollectFieldTypes(ClassDeclarationSyntax node)
    {
        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;
            if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword))) continue;

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

    // ── Struct-Definition (Header) ────────────────────────────────────────────

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

        // NEU: Interface-Pointer wenn Klasse Interface implementiert
        if (_interfaceExpander != null
            && _interfaceExpander.ClassInterfaces.TryGetValue(name, out var ifaces))
        {
            foreach (var ifaceName in ifaces)
            {
                if (_genericCollector?.Interfaces.ContainsKey(ifaceName) == true)
                    _ctx.WriteLine($"/* implements {ifaceName} */");
            }
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

            // NEU: Generische Typ-Parameter durch konkreten Typ ersetzen
            // (passiert wenn die Klasse selbst generisch ist — sollte nicht vorkommen
            //  weil generische Klassen übersprungen werden, aber zur Sicherheit)
            var cType = ResolveConcreteType(csType);
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

    /// <summary>
    /// NEU: Löst generische Typ-Namen wie Stack&lt;int&gt; zu Stack_int auf.
    /// </summary>
    private string ResolveConcreteType(string csType)
    {
        // Ist es ein generischer Typ der durch den Collector bekannt ist?
        if (_genericCollector != null && csType.Contains('<'))
        {
            // Einfache Heuristik: Foo<Bar> → Foo_Bar wenn Foo generisch ist
            var angleBracket = csType.IndexOf('<');
            var baseName = csType[..angleBracket].Trim();
            if (_genericCollector.GenericClasses.ContainsKey(baseName))
            {
                var inner = csType[(angleBracket + 1)..^1].Trim();
                var innerC = inner == "string" ? "str" : TypeRegistry.MapType(inner);
                return $"{baseName}_{innerC}";
            }
        }
        return TypeRegistry.MapType(csType);
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
            var isVirtual = method.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword));
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

    // ── Typ-Auflösung ─────────────────────────────────────────────────────────

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

    // ── Static-Felder ─────────────────────────────────────────────────────────

    internal void WriteStaticFieldDefinitions(ClassDeclarationSyntax node, string name)
    {
        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            bool isConst = field.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword));
            bool isExplicitlyStatic = field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

            if (isConst) continue;
            if (!isExplicitlyStatic) continue;

            var csType = ResolveFieldType(field);

            if (csType.EndsWith("[]"))
            {
                WriteStaticArrayFieldDef(field, name, csType);
                continue;
            }

            var cType = TypeRegistry.MapType(csType);
            var needPtr = NeedsPtr(csType);
            var ptr = needPtr ? "*" : "";

            foreach (var v in field.Declaration.Variables)
            {
                var fieldName = v.Identifier.Text.TrimStart('_');
                var init = v.Initializer != null
                    ? " = " + _exprWriter.Write(v.Initializer.Value)
                    : "";
                _ctx.Out.WriteLine("static " + cType + ptr + " " + name + "_" + fieldName + init + ";");
            }
        }
    }

    private void WriteStaticArrayFieldDef(FieldDeclarationSyntax field, string className, string csType)
    {
        var baseType = csType[..^2].Trim();
        var cType = baseType == "string" ? "const char*" : TypeRegistry.MapType(baseType);
        var isConst = field.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword))
                   || field.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword));

        foreach (var v in field.Declaration.Variables)
        {
            var fieldName = v.Identifier.Text.TrimStart('_');
            var fullName = className + "_" + fieldName;

            if (v.Initializer?.Value is ArrayCreationExpressionSyntax arr
                && arr.Initializer != null)
            {
                var elems = arr.Initializer.Expressions
                    .Select(e => _exprWriter.Write(e))
                    .ToList();
                var mod = isConst ? "static const " : "static ";
                _ctx.Out.WriteLine(mod + cType + " " + fullName + "[] = { "
                    + string.Join(", ", elems) + " };");
            }
            else if (v.Initializer?.Value is ImplicitArrayCreationExpressionSyntax implArr)
            {
                var elems = implArr.Initializer.Expressions
                    .Select(e => _exprWriter.Write(e))
                    .ToList();
                var mod = isConst ? "static const " : "static ";
                _ctx.Out.WriteLine(mod + cType + " " + fullName + "[] = { "
                    + string.Join(", ", elems) + " };");
            }
            else
            {
                _ctx.Out.WriteLine("static " + cType + " " + fullName + "[1];");
            }
        }
    }

    private void WriteStaticFieldExterns(ClassDeclarationSyntax node, string name)
    {
        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            bool isConst = field.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword));
            bool isExplicitlyStatic = field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
            bool isReadOnly = field.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword));

            if (!isExplicitlyStatic && !isConst) continue;

            var csType = ResolveFieldType(field);

            if (isConst || (isExplicitlyStatic && isReadOnly))
            {
                foreach (var v in field.Declaration.Variables)
                {
                    var fieldName = v.Identifier.Text.TrimStart('_');
                    if (v.Initializer != null)
                    {
                        var initVal = _exprWriter.Write(v.Initializer.Value);
                        _ctx.Out.WriteLine("#define " + name + "_" + fieldName + " (" + initVal + ")");
                    }
                }
                continue;
            }

            if (csType.EndsWith("[]"))
            {
                var baseType = csType[..^2].Trim();
                var cType = baseType == "string" ? "const char*" : TypeRegistry.MapType(baseType);
                foreach (var v in field.Declaration.Variables)
                {
                    var fieldName = v.Identifier.Text.TrimStart('_');
                    _ctx.Out.WriteLine("extern " + cType + " " + name + "_" + fieldName + "[];");
                }
                continue;
            }

            var cTypeNorm = TypeRegistry.MapType(csType);
            var ptr = NeedsPtr(csType) ? "*" : "";

            foreach (var v in field.Declaration.Variables)
            {
                var fieldName = v.Identifier.Text.TrimStart('_');
                _ctx.Out.WriteLine("extern " + cTypeNorm + ptr + " " + name + "_" + fieldName + ";");
            }
        }
    }

    // ── Feld-Initializer ─────────────────────────────────────────────────────

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

    // ── Konstruktor ───────────────────────────────────────────────────────────

    private void WriteConstructor(ClassDeclarationSyntax node)
    {
        var name = node.Identifier.Text;
        var baseType = _ctx.CurrentBaseType;

        var strategy = _constructorStrategies.First(s => s.Matches(node, baseType));
        strategy.Write(node, name, baseType, _ctx, _exprWriter, this);
    }

    // ── Funktions-Signaturen (Header) ─────────────────────────────────────────

    private void WriteFunctionSignatures(ClassDeclarationSyntax node,
        bool isSwitchAppChild, bool isStaticClass)
    {
        var name = node.Identifier.Text;

        if (isStaticClass)
            WriteStaticFieldExterns(node, name);

        if (!isStaticClass)
        {
            if (isSwitchAppChild)
            {
                _ctx.Out.WriteLine("void " + name + "_Init(" + name + "* self);");
            }
            else
            {
                var explicitCtor = node.Members
                    .OfType<ConstructorDeclarationSyntax>()
                    .FirstOrDefault();

                if (explicitCtor != null && explicitCtor.ParameterList.Parameters.Count > 0)
                {
                    var paramDecls = explicitCtor.ParameterList.Parameters
                        .Select(p => BuildParamDecl(p))
                        .ToList();
                    _ctx.Out.WriteLine(name + "* " + name + "_New("
                        + string.Join(", ", paramDecls) + ");");
                }
                else
                {
                    _ctx.Out.WriteLine(name + "* " + name + "_New();");
                }
            }
        }

        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
            WriteMethodSignature(method, name, isStaticClass);

        _ctx.Out.WriteLine();
    }

    // ── Methoden-Signaturen ───────────────────────────────────────────────────

    private void WriteMethodSignature(MethodDeclarationSyntax method,
        string className, bool isStaticClass = false)
    {
        var isAbstract = method.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
        var isStatic = method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))
                      || isStaticClass;

        // NEU: Extension-Methoden korrekt signieren (ohne this-Parameter in C)
        bool isExtension = method.ParameterList.Parameters.FirstOrDefault()?.Modifiers
            .Any(m => m.IsKind(SyntaxKind.ThisKeyword)) == true;

        var returnType = ResolveMethodReturnType(method);
        var name = className + "_" + method.Identifier.Text;

        var parameters = new List<string>();
        if (!isStatic && !isExtension) parameters.Add(className + "* self");

        foreach (var p in method.ParameterList.Parameters)
        {
            // NEU: this-Parameter bei Extension-Methoden überspringen (wird erster Arg)
            if (p.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword))) continue;
            parameters.Add(BuildParamDecl(p));
        }

        // Extension-Methoden: erster Param (this) wird zum ersten normalen Param
        if (isExtension && method.ParameterList.Parameters.Count > 0)
        {
            var thisParam = method.ParameterList.Parameters[0];
            parameters.Insert(0, BuildParamDecl(thisParam, skipThis: true));
        }

        var sig = returnType + " " + name + "(" + string.Join(", ", parameters) + ")";

        if (isAbstract) _ctx.Out.WriteLine("/* abstract: " + sig + " */");
        else _ctx.Out.WriteLine(sig + ";");
    }

    // ── Methoden-Bodies ───────────────────────────────────────────────────────

    private void WriteMethodBodies(ClassDeclarationSyntax node)
    {
        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
            VisitMethodDeclaration(method);
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var lineSpan = node.GetLocation().GetLineSpan();
        _ctx.CurrentLine = lineSpan.StartLinePosition.Line + 1;

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
        var isStatic = node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
        var returnType = TypeRegistry.MapType(csReturnType);

        // NEU: Extension-Methoden erkennen
        bool isExtension = node.ParameterList.Parameters.FirstOrDefault()?.Modifiers
            .Any(m => m.IsKind(SyntaxKind.ThisKeyword)) == true;

        var name = string.IsNullOrEmpty(_ctx.CurrentClass)
            ? node.Identifier.Text
            : _ctx.CurrentClass + "_" + node.Identifier.Text;

        var parameters = new List<string>();
        if (!string.IsNullOrEmpty(_ctx.CurrentClass) && !isStatic && !isExtension)
            parameters.Add(_ctx.CurrentClass + "* self");

        // Extension-Methode: this-Parameter wird erster normaler Parameter
        if (isExtension && node.ParameterList.Parameters.Count > 0)
        {
            var thisParam = node.ParameterList.Parameters[0];
            parameters.Add(BuildParamDecl(thisParam, skipThis: true));
        }

        foreach (var p in node.ParameterList.Parameters)
        {
            if (p.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword))) continue;
            parameters.Add(BuildParamDecl(p));
        }

        var sig = returnType + " " + name + "(" + string.Join(", ", parameters) + ")";

        if (_mode == TranspileMode.HeaderOnly)
        {
            if (isAbstract) _ctx.Out.WriteLine("/* abstract: " + sig + " */");
            else _ctx.Out.WriteLine(sig + ";");
            return;
        }

        if (isAbstract) return;

        _ctx.ClearMethodContext();
        _ctx.CurrentReturnBuffer = null;
        if (csReturnType == "string"
            && ReturnStringFixHelper.HasInterpolatedStringReturn(node))
        {
            _ctx.CurrentReturnBuffer = "_ret_buf";
        }

        foreach (var p in node.ParameterList.Parameters)
        {
            if (p.Type == null) continue;

            var isParams = p.Modifiers.Any(m => m.IsKind(SyntaxKind.ParamsKeyword));
            var isThisParam = p.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword));

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
                catch { paramType = p.Type.ToString().Trim(); }
            }
            else
            {
                paramType = p.Type.ToString().Trim();
            }

            var isRefParam = p.Modifiers.Any(m =>
                m.IsKind(SyntaxKind.RefKeyword) || m.IsKind(SyntaxKind.OutKeyword));
            _ctx.LocalTypes[p.Identifier.Text] = isRefParam ? "@ref:" + paramType : paramType;

            if (isParams && paramType.EndsWith("[]"))
            {
                var countVar = p.Identifier.Text + "_count";
                _ctx.LocalTypes[countVar] = "int";
            }
        }

        _ctx.Out.WriteLine(sig);
        _ctx.Out.WriteLine("{");
        _ctx.Indent();

        if (_ctx.CurrentReturnBuffer != null)
            _ctx.WriteLine("static char _ret_buf[CS2SX_RETURN_BUF_SIZE];");


        if (node.Body != null)
            foreach (var stmt in node.Body.Statements)
                _stmtWriter.Write(stmt);
        else if (node.ExpressionBody != null)
            _ctx.WriteLine("return " + _exprWriter.Write(node.ExpressionBody.Expression) + ";");

        _ctx.Dedent();
        _ctx.Out.WriteLine("}");
        _ctx.Out.WriteLine();
    }

    // ── Property ─────────────────────────────────────────────────────────────

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        var csType = ResolvePropertyType(node);
        _ctx.PropertyTypes[node.Identifier.Text] = csType;
        _ctx.FieldTypes[node.Identifier.Text] = csType;
        base.VisitPropertyDeclaration(node);
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    internal void LoadBaseFields(string baseType)
    {
        if (baseType is "Control" or "Label" or "Button" or "ProgressBar")
            foreach (var f in TypeRegistry.ControlFields)
                _ctx.BaseFieldTypes[f] = "int";

        if (baseType is "Button")
        {
            _ctx.BaseFieldTypes["focused"] = "int";
            _ctx.BaseFieldTypes["OnClick"] = "Action";
            _ctx.BaseFieldTypes["text"] = "string";
        }
        if (baseType is "Label")
            _ctx.BaseFieldTypes["text"] = "string";
        if (baseType is "ProgressBar")
        {
            _ctx.BaseFieldTypes["value"] = "int";
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

    /// <summary>
    /// Original BuildParamDecl (Rückwärtskompatibilität).
    /// </summary>
    internal string BuildParamDecl(ParameterSyntax p) =>
        BuildParamDecl(p, skipThis: false);

    /// <summary>
    /// NEU: BuildParamDecl mit skipThis-Option für Extension-Methoden.
    /// </summary>
    internal string BuildParamDecl(ParameterSyntax p, bool skipThis)
    {
        if (p.Type == null) return p.Identifier.Text;

        var csType = p.Type.ToString().Trim();

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

        // NEU: Generische Typ-Parameter auflösen wenn Collector vorhanden
        // (z.B. T → int bei expandierten Methoden)
        if (_genericCollector != null && _genericCollector.GenericClasses.ContainsKey(csType))
        {
            // Generische Klasse als Parameter → Pointer
            return csType + "* " + p.Identifier;
        }

        var isParams = p.Modifiers.Any(m => m.IsKind(SyntaxKind.ParamsKeyword));
        if (isParams && csType.EndsWith("[]"))
        {
            var baseType = csType[..^2].Trim();
            var cBaseType = baseType == "string" ? "const char*" : TypeRegistry.MapType(baseType);
            return cBaseType + "* " + p.Identifier + ", int " + p.Identifier + "_count";
        }

        if (NullableHandler.IsNullable(csType))
        {
            var inner = NullableHandler.GetInnerType(csType);
            var cInner = TypeRegistry.MapType(inner);
            return cInner + "* " + p.Identifier;
        }

        if (csType.EndsWith("[]"))
        {
            var baseType = csType[..^2].Trim();
            var cBase = baseType == "string" ? "const char*" : TypeRegistry.MapType(baseType);
            return cBase + "* " + p.Identifier;
        }

        // NEU: Interface-Typ als Parameter → Pointer
        if (_genericCollector != null
            && _genericCollector.Interfaces.ContainsKey(csType))
        {
            return csType + "* " + p.Identifier;
        }

        var cType = TypeRegistry.MapType(csType);
        var isPrim = TypeRegistry.IsPrimitive(csType) || csType == "string";
        var isRef = p.Modifiers.Any(m =>
            m.IsKind(SyntaxKind.RefKeyword) || m.IsKind(SyntaxKind.OutKeyword));
        var ptr = (!isPrim || isRef) ? "*" : "";
        return cType + ptr + " " + p.Identifier;
    }
}