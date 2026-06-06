// Datei: Transpiler/CSharpToC.cs
//
// FIX: VisitMethodDeclaration() ruft _ctx.FlushLambdaPreludes() auf,
//      BEVOR die Methodensignatur in den Output geschrieben wird.
//      Das ist der zentrale Flush-Punkt für alle Lambda-Preludes
//      (Struct-Defs + statische Hilfsfunktionen), die LambdaLifter
//      während der Transpilierung des Methoden-Bodys gesammelt hat.
//
//      Ablauf:
//        1. Methoden-Body wird transpiliert → Lambdas erzeugen Preludes
//           in _ctx.PendingLambdaPreludes (via LambdaLifter.LiftLambda())
//        2. FlushLambdaPreludes() schreibt alle Preludes VOR die Signatur
//        3. Signatur + Body werden normal geschrieben
//
//      Das ersetzt den alten O(n²)-StringWriter-Rewrite in ExpressionWriter.

using System.Xml.Linq;
using CS2SX.Core;
using CS2SX.Logging;
using CS2SX.Transpiler.Handlers;
using CS2SX.Transpiler.Strategies;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

    private readonly GenericInstantiationCollector? _genericCollector;
    private readonly InterfaceExpander? _interfaceExpander;
    private readonly ExtensionMethodHandler _extensionHandler;

    // ── Konstruktoren ─────────────────────────────────────────────────────────

    public CSharpToC(TranspileMode mode = TranspileMode.Implementation)
    {
        _mode = mode;
        _ctx = new TranspilerContext(new StringWriter());
        _extensionHandler = new ExtensionMethodHandler();
        _exprWriter = new ExpressionWriter(_ctx, _extensionHandler);
        _stmtWriter = new StatementWriter(_ctx, _exprWriter);
        _constructorStrategies = BuildStrategies();
    }

    public CSharpToC(
        TranspileMode mode,
        GenericInstantiationCollector collector,
        InterfaceExpander interfaceExpander,
        DiagnosticReporter? sharedDiagnostics = null)
    {
        _mode = mode;
        _ctx = new TranspilerContext(new StringWriter(), sharedDiagnostics);
        _genericCollector = collector;
        _interfaceExpander = interfaceExpander;
        _extensionHandler = new ExtensionMethodHandler(collector.ExtensionMethods);
        _exprWriter = new ExpressionWriter(_ctx, _extensionHandler);
        _stmtWriter = new StatementWriter(_ctx, _exprWriter);
        _constructorStrategies = BuildStrategies();

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

        _ctx.UsingStaticResolver.Collect(tree.GetRoot());
        // Sync type aliases (using X = Y;) into context
        _ctx.TypeAliases.Clear();
        foreach (var kv in _ctx.UsingStaticResolver.UsingAliases)
            _ctx.TypeAliases[kv.Key] = kv.Value;

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
        var enumName = node.Identifier.Text;

        // Always register so EnumDefs can be consulted for pointer-type decisions.
        var memberNames = new List<string>();
        foreach (var member in node.Members)
        {
            var mname = member.Identifier.Text;
            _ctx.EnumMembers.Add(mname);
            memberNames.Add(mname);
        }
        _ctx.EnumDefs[enumName] = memberNames;
        // Register as value type so NeedsPointerSuffix never adds * for enum fields/locals
        TypeRegistry.RegisterUserEnum(enumName);

        if (_mode != TranspileMode.HeaderOnly) return;

        // Determine the underlying C type (default: int)
        string underlyingCType = "int";
        if (node.BaseList?.Types.Count > 0)
        {
            var baseTypeName = node.BaseList.Types[0].ToString().Trim();
            underlyingCType = TypeRegistry.MapType(baseTypeName);
        }

        // typedef form so 'EnumName' is usable without the 'enum' keyword in C.
        _ctx.Out.WriteLine("typedef enum " + enumName);
        _ctx.Out.WriteLine("{");
        _ctx.Indent();
        foreach (var member in node.Members)
        {
            var name = member.Identifier.Text;
            if (member.EqualsValue != null)
                _ctx.Out.WriteLine(name + " = " + _exprWriter.Write(member.EqualsValue.Value) + ",");
            else
                _ctx.Out.WriteLine(name + ",");
        }
        _ctx.Dedent();
        _ctx.Out.WriteLine("} " + enumName + ";");
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

        if (_genericCollector != null
            && node.TypeParameterList?.Parameters.Count > 0)
        {
            Log.Debug($"CSharpToC: Generische Klasse '{node.Identifier.Text}' übersprungen");
            return;
        }

        bool isExtensionClass = node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))
            && node.Members.OfType<MethodDeclarationSyntax>().Any(m =>
                m.ParameterList.Parameters.FirstOrDefault()?.Modifiers
                    .Any(mod => mod.IsKind(SyntaxKind.ThisKeyword)) == true);

        var baseType = _ctx.CurrentBaseType;
        var isSwitchAppChild = baseType == SwitchAppBase;
        var isStaticClass = node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

        if (!string.IsNullOrEmpty(baseType) && baseType != SwitchAppBase)
            LoadBaseFields(baseType);

        // VTable override validation
        if (!string.IsNullOrEmpty(_ctx.CurrentBaseType)
            && _ctx.CurrentBaseType != SwitchAppBase
            && !IsControlSubclass(_ctx.CurrentBaseType)
            && _mode == TranspileMode.Implementation)
        {
            VTableBuilder.ValidateOverrides(node, _ctx.CurrentBaseType, _ctx);
        }

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

        if (_interfaceExpander != null
            && _interfaceExpander.ClassInterfaces.ContainsKey(node.Identifier.Text))
        {
            _ctx.VTableTypes.Add(node.Identifier.Text);
        }

        if (_mode == TranspileMode.HeaderOnly)
        {
            // Hoist nested enum declarations before the class struct so they are
            // declared before any field or method signature that references them.
            foreach (var nestedEnum in node.Members.OfType<EnumDeclarationSyntax>())
                VisitEnumDeclaration(nestedEnum);

            if (!isStaticClass && VTableBuilder.HasVirtualMethods(node))
                VTableBuilder.WriteVTableStruct(node, node.Identifier.Text, _ctx.Out);

            if (!isStaticClass)
                WriteStructDefinition(node, baseType);

            WriteFunctionSignatures(node, isSwitchAppChild, isStaticClass);

      

            // In WriteMethodBodies, also write operator bodies:
            

            if (!isStaticClass)
                WriteDestructor(node, _ctx.CurrentClass, baseType);

            foreach (var prop in node.Members.OfType<PropertyDeclarationSyntax>())
                if (!PropertyWriter.IsAutoProperty(prop))
                    PropertyWriter.WriteSignatures(prop, node.Identifier.Text, _ctx.Out);

            if (_interfaceExpander != null)
            {
                var ifaceDecls = _interfaceExpander.ExpandClassVTableDeclarations(node.Identifier.Text);
                if (!string.IsNullOrEmpty(ifaceDecls))
                    _ctx.Out.WriteLine(ifaceDecls);
            }
        }
        else
        {
            // Register nested enums for pointer-type decisions in method bodies.
            foreach (var nestedEnum in node.Members.OfType<EnumDeclarationSyntax>())
                VisitEnumDeclaration(nestedEnum);

            if (isStaticClass)
                WriteStaticFieldDefinitions(node, _ctx.CurrentClass);  // static classes need field defs
            else
                WriteConstructor(node);

            WriteMethodBodies(node);

            if (!isStaticClass)
                WriteDestructor(node, _ctx.CurrentClass, baseType);

            foreach (var prop in node.Members.OfType<PropertyDeclarationSyntax>())
                if (!PropertyWriter.IsAutoProperty(prop))
                    PropertyWriter.WriteImplementations(prop, node.Identifier.Text, _ctx, _exprWriter, _stmtWriter);

            if (!isStaticClass
                && !string.IsNullOrEmpty(baseType)
                && baseType != SwitchAppBase
                && !IsControlSubclass(baseType)
                && _ctx.VTableTypes.Contains(baseType))
            {
                VTableBuilder.WriteVTableInstance(node, node.Identifier.Text, baseType, _ctx.Out, _ctx.SemanticModel);
            }

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

    private void WriteDestructor(ClassDeclarationSyntax node, string name, string baseType)
    {
        bool isRootClass = string.IsNullOrEmpty(baseType);

        if (_mode == TranspileMode.HeaderOnly)
        {
            _ctx.Out.WriteLine($"void {name}_Free({name}* self);");
            if (isRootClass)
                _ctx.Out.WriteLine($"{name}* {name}_Retain({name}* self);");
            return;
        }

        _ctx.Out.WriteLine($"void {name}_Free({name}* self)");
        _ctx.Out.WriteLine("{");
        _ctx.Indent();
        _ctx.WriteLine("if (!self) return;");
        if (isRootClass)
            _ctx.WriteLine("if (--self->_rc > 0) return;");

        // Benutzerdefinierter Destruktor-Body (falls vorhanden) VOR dem automatischen Cleanup ausführen.
        // C#-Konvention: Finalizer läuft zuerst, danach GC → hier: user-code, dann field-cleanup.
        var customDtor = node.Members.OfType<DestructorDeclarationSyntax>().FirstOrDefault();
        if (customDtor?.Body != null)
        {
            _ctx.CurrentClass = name;
            foreach (var stmt in customDtor.Body.Statements)
                _stmtWriter.Write(stmt);
        }
        else if (customDtor?.ExpressionBody != null)
        {
            _ctx.CurrentClass = name;
            _ctx.WriteLine(_exprWriter.Write(customDtor.ExpressionBody.Expression) + ";");
        }

        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;

            var csType = ResolveFieldType(field);

            // FIX: Nullable-Felder (int? → int*) werden heap-allokiert → müssen free'd werden
            if (NullableHandler.IsNullable(csType))
            {
                foreach (var v in field.Declaration.Variables)
                {
                    var fieldName = v.Identifier.Text.TrimStart('_');
                    var prefix = TypeRegistry.HasNoPrefix(fieldName) ? "" : "f_";
                    _ctx.WriteLine($"if (self->{prefix}{fieldName}) {{ free(self->{prefix}{fieldName}); self->{prefix}{fieldName} = NULL; }}");
                }
                continue;
            }

            // FIX: Array-Felder (T[]) werden per calloc/malloc allokiert → müssen free'd werden
            if (csType.EndsWith("[]"))
            {
                var innerType = csType[..^2].Trim();
                if (!TypeRegistry.IsLibNxStruct(innerType))
                {
                    foreach (var v in field.Declaration.Variables)
                    {
                        var fieldName = v.Identifier.Text.TrimStart('_');
                        var prefix = TypeRegistry.HasNoPrefix(fieldName) ? "" : "f_";
                        _ctx.WriteLine($"if (self->{prefix}{fieldName}) {{ free(self->{prefix}{fieldName}); self->{prefix}{fieldName} = NULL; }}");
                    }
                }
                continue;
            }

            // Interface types are value structs — no heap allocation, nothing to free.
            if (TypeRegistry.IsRegisteredInterface(csType)) continue;

            var needsFree = TypeRegistry.NeedsPointerSuffix(csType)
                         && !TypeRegistry.IsPrimitive(csType)
                         && csType != "string"
                         && !TypeRegistry.IsLibNxStruct(csType);

            if (!needsFree) continue;

            foreach (var v in field.Declaration.Variables)
            {
                var fieldName = v.Identifier.Text.TrimStart('_');
                var prefix = TypeRegistry.HasNoPrefix(fieldName) ? "" : "f_";
                var fieldExpr = $"self->{prefix}{fieldName}";

                if (TypeRegistry.IsList(csType))
                {
                    var inner = TypeRegistry.GetListInnerType(csType) ?? "int";
                    var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
                    _ctx.WriteLine($"if ({fieldExpr}) {{ List_{cInner}_Free({fieldExpr}); {fieldExpr} = NULL; }}");
                }
                else if (TypeRegistry.IsDictionary(csType))
                {
                    var types = TypeRegistry.GetDictionaryTypes(csType);
                    if (types.HasValue)
                    {
                        var ck = types.Value.key == "string" ? "str" : TypeRegistry.MapType(types.Value.key);
                        var cv = types.Value.val == "string" ? "str" : TypeRegistry.MapType(types.Value.val);
                        _ctx.WriteLine($"if ({fieldExpr}) {{ Dict_{ck}_{cv}_Free({fieldExpr}); {fieldExpr} = NULL; }}");
                    }
                }
                else if (TypeRegistry.IsStringBuilder(csType))
                {
                    _ctx.WriteLine($"if ({fieldExpr}) {{ StringBuilder_Free({fieldExpr}); {fieldExpr} = NULL; }}");
                }
                else if (TypeRegistry.IsDisposable(csType))
                {
                    _ctx.WriteLine($"if ({fieldExpr}) {{ {csType}_Dispose({fieldExpr}); {fieldExpr} = NULL; }}");
                }
                else
                {
                    var cType = TypeRegistry.MapType(csType);
                    _ctx.WriteLine($"if ({fieldExpr}) {{ {cType}_Free({fieldExpr}); {fieldExpr} = NULL; }}");
                }
            }
        }

        // When a base _Free is called it chains up to the root class which calls free(self).
        // Calling free(self) again here would be a double-free. Only the root class frees memory.
        bool delegatedToBase = !string.IsNullOrEmpty(baseType)
            && baseType != "SwitchApp"
            && !IsControlSubclass(baseType)
            && !_ctx.InterfaceTypes.Contains(baseType); // interface base has no _Free chain

        if (delegatedToBase)
            _ctx.WriteLine($"{baseType}_Free(({baseType}*)self);");
        else
            _ctx.WriteLine("free(self);");
        _ctx.Dedent();
        _ctx.Out.WriteLine("}");
        _ctx.Out.WriteLine();

        if (isRootClass)
        {
            _ctx.Out.WriteLine($"{name}* {name}_Retain({name}* self)");
            _ctx.Out.WriteLine("{");
            _ctx.Out.WriteLine($"    if (self) self->_rc++;");
            _ctx.Out.WriteLine($"    return self;");
            _ctx.Out.WriteLine("}");
            _ctx.Out.WriteLine();
        }
    }

    public override void VisitStructDeclaration(StructDeclarationSyntax node)
    {
        var structName = node.Identifier.Text;
        _ctx.ValueTypeStructs.Add(structName);

        if (_mode == TranspileMode.HeaderOnly)
        {
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
            bool isStaticProp = prop.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

            var csType = ResolvePropertyType(prop);
            _ctx.PropertyTypes[prop.Identifier.Text] = csType;
            if (PropertyWriter.IsAutoProperty(prop))
            {
                // Instance auto-props only — static auto-props are global C variables, not struct fields
                if (!isStaticProp)
                    _ctx.FieldTypes[prop.Identifier.Text] = csType;
            }
            else
            {
                // Computed properties (static or instance) → call getter function
                _ctx.ComputedPropertyNames.Add(prop.Identifier.Text);
            }
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
        var csType = field.Declaration.Type.ToString().Trim();
        if (TypeRegistry.IsDecimalType(csType))
            _ctx.Warn(field, "decimal field mapped to double (precision loss possible)");
        return csType;
    }

    private void WriteOperatorBody(OperatorDeclarationSyntax op, string className)
    {
        var opToken = op.OperatorToken.Text;
        if (!OperatorOverloadWriter.s_opNames.TryGetValue(opToken, out var suffix))
            suffix = "op_unknown";

        var retType = TypeRegistry.MapType(op.ReturnType.ToString().Trim());
        var paramList = string.Join(", ",
            op.ParameterList.Parameters.Select(p =>
            {
                var decl = BuildParamDecl(p);
                _ctx.LocalTypes[p.Identifier.Text] = p.Type?.ToString().Trim() ?? "int";
                return decl;
            }));

        _ctx.ClearMethodContext();
        var preSigPosOp = _ctx.Out.GetStringBuilder().Length;

        _ctx.Out.WriteLine($"{retType} {className}_{suffix}({paramList})");
        _ctx.Out.WriteLine("{");
        _ctx.Indent();

        if (op.Body != null)
            foreach (var stmt in op.Body.Statements)
                _stmtWriter.Write(stmt);
        else if (op.ExpressionBody != null)
            _ctx.WriteLine($"return {_exprWriter.Write(op.ExpressionBody.Expression)};");

        _ctx.Dedent();
        _ctx.Out.WriteLine("}");
        _ctx.Out.WriteLine();

        if (_ctx.PendingLambdaPreludes.Count > 0)
        {
            var sb = _ctx.Out.GetStringBuilder();
            var methodText = sb.ToString(preSigPosOp, sb.Length - preSigPosOp);
            sb.Remove(preSigPosOp, sb.Length - preSigPosOp);
            _ctx.FlushLambdaPreludes();
            _ctx.Out.Write(methodText);
        }
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

        // Root classes: vtable pointer FIRST (so subclass upcasts work), then _rc.
        // Subclasses: embed BASE struct FIRST — base already contains the vtable pointer.
        // This guarantees (BaseType*)derived correctly addresses the vtable field.
        if (string.IsNullOrEmpty(baseType))
        {
            if (VTableBuilder.HasVirtualMethods(node))
                _ctx.WriteLine(name + "_vtable* vtable;");
            _ctx.WriteLine("int _rc;");
        }
        else
        {
            var embedType = baseType is "Label" or "Button" or "ProgressBar"
                ? "Control"
                : baseType;
            _ctx.WriteLine(embedType + " base;");
        }

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
        // Individual function-pointer fields are only emitted for classes that do NOT
        // use the vtable dispatch mechanism (e.g. plain SwitchApp subclasses, controls).
        // Classes with virtual methods use the vtable struct instead.
        if (!VTableBuilder.HasVirtualMethods(node))
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
            var cType = ResolveConcreteType(csType);
            var needPtr = !cType.EndsWith("*")
                       && !_ctx.EnumDefs.ContainsKey(csType)
                       && (TypeRegistry.NeedsPointerSuffix(csType)
                       || TypeRegistry.IsStringBuilder(csType)
                       || TypeRegistry.IsControlType(csType)
                       || NullableHandler.IsNullable(csType));
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

    private string ResolveConcreteType(string csType)
    {
        if (_genericCollector != null && csType.Contains('<'))
        {
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
            var ptr = !cType.EndsWith("*") && NeedsPtr(csType) ? "*" : "";
            var propPfx = TypeRegistry.HasNoPrefix(prop.Identifier.Text) ? "" : "f_";
            _ctx.WriteLine(cType + ptr + " " + propPfx + prop.Identifier + ";");
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
        string csType;
        if (_ctx.SemanticModel != null)
        {
            try
            {
                var sym = _ctx.SemanticModel.GetDeclaredSymbol(method);
                if (sym != null)
                {
                    csType = TranspilerContext.FormatTypeSymbol(sym.ReturnType);
                    var mapped = TypeRegistry.MapType(csType);
                    if (!mapped.EndsWith("*") && NeedsPtr(csType))
                        return mapped + "*";
                    return mapped;
                }
            }
            catch { }
        }
        csType = method.ReturnType.ToString().Trim();
        {
            var mapped = TypeRegistry.MapType(csType);
            if (!mapped.EndsWith("*") && NeedsPtr(csType))
                return mapped + "*";
            return mapped;
        }
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
        WriteStaticConstructor(node, name);

        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            bool isConst = field.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword));
            bool isStatic = field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
            bool isReadOnly = field.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword));

            if (isConst || isReadOnly)
            {
                // Instance readonly fields are initialized in the constructor — not as globals.
                // Only static readonly and const fields become global C variables here.
                if (isReadOnly && !isStatic && !isConst)
                    continue;

                var csType = ResolveFieldType(field);
                if (csType == "string")
                {
                    foreach (var v in field.Declaration.Variables)
                    {
                        if (v.Initializer == null) continue;
                        var fieldName = v.Identifier.Text.TrimStart('_');
                        var initVal = _exprWriter.Write(v.Initializer.Value);
                        _ctx.Out.WriteLine(
                            $"const char* const {name}_{fieldName} = {initVal};");
                    }
                }
                else if (csType.EndsWith("[]"))
                {
                    // Static readonly T[] → proper C array with count macro
                    var baseType = csType[..^2].Trim();
                    // Strip "const " prefix from element type since we add "const" via the modifier
                    var cBase0 = baseType == "string" ? "const char*" : TypeRegistry.MapType(baseType);
                    var cBaseElem = cBase0.StartsWith("const ") ? cBase0["const ".Length..].Trim() : cBase0;
                    foreach (var v in field.Declaration.Variables)
                    {
                        if (v.Initializer == null) continue;
                        var fieldName = v.Identifier.Text.TrimStart('_');
                        var fullName2 = name + "_" + fieldName;
                        List<string> elems;
                        if (v.Initializer.Value is Microsoft.CodeAnalysis.CSharp.Syntax.InitializerExpressionSyntax initExpr2)
                            elems = initExpr2.Expressions.Select(e => _exprWriter.Write(e)).ToList();
                        else if (v.Initializer.Value is Microsoft.CodeAnalysis.CSharp.Syntax.ArrayCreationExpressionSyntax arrExpr2 && arrExpr2.Initializer != null)
                            elems = arrExpr2.Initializer.Expressions.Select(e => _exprWriter.Write(e)).ToList();
                        else if (v.Initializer.Value is Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitArrayCreationExpressionSyntax implArr2)
                            elems = implArr2.Initializer.Expressions.Select(e => _exprWriter.Write(e)).ToList();
                        else
                            elems = new List<string>();
                        // "const char* arr[]" — no "static" so it matches "extern" declaration in header
                        _ctx.Out.WriteLine($"const {cBaseElem} {fullName2}[] = {{ {string.Join(", ", elems)} }};");
                        _ctx.Out.WriteLine($"#define {fullName2}_count {elems.Count}");
                    }
                }
                else if (!TypeRegistry.IsPrimitive(csType))
                {
                    // Non-primitive const/readonly: emit definition (header has extern decl)
                    var cTypeNP = TypeRegistry.MapType(csType);
                    var needPtrNP = NeedsPtr(csType);
                    var ptrNP = needPtrNP ? "*" : "";
                    // For pointer types: don't add const — header has no const, C functions expect non-const
                    // For non-pointer value types: const is meaningful
                    bool isPointerType = cTypeNP.EndsWith("*") || needPtrNP;
                    foreach (var v in field.Declaration.Variables)
                    {
                        var fieldName = v.Identifier.Text.TrimStart('_');
                        if (v.Initializer != null)
                        {
                            var initVal = _exprWriter.Write(v.Initializer.Value);
                            // Function-call initializers (like Stack_T_New()) are NOT C compile-time constants.
                            // Emit NULL globally and initialize in a generated static constructor.
                            bool isRuntimeInit = initVal.Contains("_New()") || initVal.Contains("_New(")
                                              || (initVal.Contains("(") && !initVal.StartsWith("\"") && !initVal.StartsWith("("));
                            if (isPointerType && isRuntimeInit)
                            {
                                _ctx.Out.WriteLine($"{cTypeNP}{ptrNP} {name}_{fieldName} = NULL;");
                                // Emit GCC constructor to init at runtime
                                _ctx.Out.WriteLine($"__attribute__((constructor)) static void {name}_{fieldName}_Init(void) {{");
                                _ctx.Out.WriteLine($"    {name}_{fieldName} = {initVal};");
                                _ctx.Out.WriteLine($"}}");
                            }
                            else if (isPointerType)
                            {
                                _ctx.Out.WriteLine($"{cTypeNP}{ptrNP} {name}_{fieldName} = {initVal};");
                            }
                            else
                            {
                                var constPrefix = cTypeNP.StartsWith("const ") ? "" : "const ";
                                _ctx.Out.WriteLine($"{constPrefix}{cTypeNP}{ptrNP} {name}_{fieldName} = {initVal};");
                            }
                        }
                        else
                        {
                            // Static field with no initializer → define as NULL/0
                            _ctx.Out.WriteLine($"{cTypeNP}{ptrNP} {name}_{fieldName} = NULL;");
                        }
                    }
                }
                continue;
            }

            if (!isStatic) continue;

            var csTypeFinal = ResolveFieldType(field);

            if (csTypeFinal.EndsWith("[]"))
            {
                WriteStaticArrayFieldDef(field, name, csTypeFinal);
                continue;
            }

            var cType = TypeRegistry.MapType(csTypeFinal);
            var needPtr = NeedsPtr(csTypeFinal);
            var ptr = needPtr ? "*" : "";

            foreach (var v in field.Declaration.Variables)
            {
                var fieldName = v.Identifier.Text.TrimStart('_');
                var init = v.Initializer != null
                    ? " = " + _exprWriter.Write(v.Initializer.Value)
                    : "";
                _ctx.Out.WriteLine($"{cType}{ptr} {name}_{fieldName}{init};");
            }
        }
    }

    private void WriteStaticConstructor(ClassDeclarationSyntax node, string name)
    {
        var staticCtor = node.Members.OfType<ConstructorDeclarationSyntax>()
            .FirstOrDefault(c => c.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)));
        if (staticCtor == null) return;

        _ctx.Out.WriteLine($"__attribute__((constructor))");
        _ctx.Out.WriteLine($"static void {name}_StaticInit(void)");
        _ctx.Out.WriteLine("{");
        _ctx.Indent();
        if (staticCtor.Body != null)
        {
            var stmtWriter = new Writers.StatementWriter(_ctx, _exprWriter);
            foreach (var s in staticCtor.Body.Statements)
                stmtWriter.Write(s);
        }
        _ctx.Dedent();
        _ctx.Out.WriteLine("}");
        _ctx.Out.WriteLine();
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
                // Emit count macro so .Length works via fullName_count
                _ctx.Out.WriteLine($"#define {fullName}_count {elems.Count}");
            }
            else if (v.Initializer?.Value is ImplicitArrayCreationExpressionSyntax implArr)
            {
                var elems = implArr.Initializer.Expressions
                    .Select(e => _exprWriter.Write(e))
                    .ToList();
                var mod = isConst ? "static const " : "static ";
                _ctx.Out.WriteLine(mod + cType + " " + fullName + "[] = { "
                    + string.Join(", ", elems) + " };");
                _ctx.Out.WriteLine($"#define {fullName}_count {elems.Count}");
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
            bool isStatic = field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
            bool isReadOnly = field.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword));

            if (!isStatic && !isConst) continue;

            var csType = ResolveFieldType(field);

            if (isConst || (isStatic && isReadOnly))
            {
                foreach (var v in field.Declaration.Variables)
                {
                    var fieldName = v.Identifier.Text.TrimStart('_');
                    if (v.Initializer == null) continue;

                    var initVal = _exprWriter.Write(v.Initializer.Value);

                    if (csType == "string")
                        _ctx.Out.WriteLine($"extern const char* const {name}_{fieldName};");
                    else if (TypeRegistry.IsPrimitive(csType))
                        _ctx.Out.WriteLine($"#define {name}_{fieldName} ({initVal})");
                    else if (csType.EndsWith("[]"))
                    {
                        // Array: extern declaration must match implementation type (const ElemType arr[])
                        var elemBase = csType[..^2].Trim();
                        var cElem0 = elemBase == "string" ? "const char*" : TypeRegistry.MapType(elemBase);
                        var cElemStrip = cElem0.StartsWith("const ") ? cElem0["const ".Length..].Trim() : cElem0;
                        _ctx.Out.WriteLine($"extern const {cElemStrip} {name}_{fieldName}[];");
                    }
                    else
                    {
                        var cMapped = TypeRegistry.MapType(csType);
                        if (cMapped.EndsWith("*"))
                            _ctx.Out.WriteLine($"extern {cMapped} {name}_{fieldName};");
                        else
                            _ctx.Out.WriteLine($"extern const {cMapped} {name}_{fieldName};");
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
                    _ctx.Out.WriteLine($"extern {cType} {name}_{fieldName}[];");
                }
                continue;
            }

            var cTypeNorm = TypeRegistry.MapType(csType);
            var ptr = NeedsPtr(csType) ? "*" : "";

            foreach (var v in field.Declaration.Variables)
            {
                var fieldName = v.Identifier.Text.TrimStart('_');
                _ctx.Out.WriteLine($"extern {cTypeNorm}{ptr} {name}_{fieldName};");
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
                var initVal = _exprWriter.Write(v.Initializer.Value);
                // Interface-typed field: wrap initializer with as_IFace() upcast
                var fieldCsType = ResolveFieldType(field);
                if (_ctx.InterfaceTypes.Contains(fieldCsType) && _ctx.SemanticModel != null)
                {
                    try
                    {
                        var rightSym = _ctx.SemanticModel.GetTypeInfo(v.Initializer.Value).Type
                            as Microsoft.CodeAnalysis.INamedTypeSymbol;
                        if (rightSym != null && !TypeRegistry.IsRegisteredInterface(rightSym.Name)
                            && TypeRegistry.NeedsPointerSuffix(rightSym.Name))
                            initVal = rightSym.Name + "_as_" + fieldCsType + "(" + initVal + ")";
                    }
                    catch { }
                }
                _ctx.WriteLine("self->" + prefix + fieldName + " = " + initVal + ";");
            }
        }

        // Auto-Property-Initializer: public string Value { get; set; } = "x";
        foreach (var prop in node.Members.OfType<PropertyDeclarationSyntax>())
        {
            if (prop.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;
            if (!PropertyWriter.IsAutoProperty(prop)) continue;
            if (prop.Initializer == null) continue;

            var initPfx = TypeRegistry.HasNoPrefix(prop.Identifier.Text) ? "" : "f_";
            _ctx.WriteLine("self->" + initPfx + prop.Identifier.Text + " = "
                + _exprWriter.Write(prop.Initializer.Value) + ";");
        }

        // Auto-init List<T> properties/fields with no explicit initializer to List_T_New()
        // so that Add() calls don't crash with a null pointer.
        foreach (var prop in node.Members.OfType<PropertyDeclarationSyntax>())
        {
            if (prop.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;
            if (!PropertyWriter.IsAutoProperty(prop)) continue;
            if (prop.Initializer != null) continue;
            var csType = prop.Type.ToString().Trim();
            if (!TypeRegistry.IsList(csType)) continue;
            var inner = TypeRegistry.GetListInnerType(csType) ?? "int";
            var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
            _ctx.WriteLine("self->f_" + prop.Identifier.Text + " = List_" + cInner + "_New();");
        }

        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;
            var csType = field.Declaration.Type.ToString().Trim();
            if (!TypeRegistry.IsList(csType)) continue;
            var inner = TypeRegistry.GetListInnerType(csType) ?? "int";
            var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
            foreach (var v in field.Declaration.Variables)
            {
                if (v.Initializer != null) continue;
                var fieldName = v.Identifier.Text.TrimStart('_');
                var prefix = TypeRegistry.HasNoPrefix(fieldName) ? "" : "f_";
                _ctx.WriteLine("self->" + prefix + fieldName + " = List_" + cInner + "_New();");
            }
        }
    }

    // ── Base-class field initializers for subclass constructors ──────────────

    /// <summary>
    /// Emits property/field initializers from <paramref name="baseType"/>'s declaration
    /// with "self->base." prefix, so subclass _New() functions inherit the base
    /// class's default values (e.g. Visible = true) after memset-to-zero.
    /// </summary>
    internal void WriteInstanceFieldInitializersForBaseClass(string baseType)
    {
        if (string.IsNullOrEmpty(baseType) || _ctx.SemanticModel == null) return;
        try
        {
            foreach (var tree in _ctx.SemanticModel.Compilation.SyntaxTrees)
            {
                var cls = tree.GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .FirstOrDefault(c => c.Identifier.Text == baseType);
                if (cls == null) continue;
                WriteInstanceFieldInitializersWithPrefix(cls, "self->base.");
                return;
            }
        }
        catch { }
    }

    private void WriteInstanceFieldInitializersWithPrefix(
        ClassDeclarationSyntax node, string prefix)
    {
        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            if (field.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;
            foreach (var v in field.Declaration.Variables)
            {
                if (v.Initializer == null) continue;
                var fieldName = v.Identifier.Text.TrimStart('_');
                var fp = TypeRegistry.HasNoPrefix(fieldName) ? "" : "f_";
                _ctx.WriteLine(prefix + fp + fieldName + " = "
                    + _exprWriter.Write(v.Initializer.Value) + ";");
            }
        }
        foreach (var prop in node.Members.OfType<PropertyDeclarationSyntax>())
        {
            if (prop.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;
            if (!PropertyWriter.IsAutoProperty(prop)) continue;
            if (prop.Initializer == null) continue;
            var initPfx = TypeRegistry.HasNoPrefix(prop.Identifier.Text) ? "" : "f_";
            _ctx.WriteLine(prefix + initPfx + prop.Identifier.Text + " = "
                + _exprWriter.Write(prop.Initializer.Value) + ";");
        }
        foreach (var prop in node.Members.OfType<PropertyDeclarationSyntax>())
        {
            if (prop.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))) continue;
            if (!PropertyWriter.IsAutoProperty(prop)) continue;
            if (prop.Initializer != null) continue;
            var csType = prop.Type.ToString().Trim();
            if (!TypeRegistry.IsList(csType)) continue;
            var inner = TypeRegistry.GetListInnerType(csType) ?? "int";
            var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
            _ctx.WriteLine(prefix + "f_" + prop.Identifier.Text + " = List_" + cInner + "_New();");
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

        RegisterClassOverloads(node, name);
        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
            WriteMethodSignature(method, name, isStaticClass);

        // Write operator overload signatures
        foreach (var opDecl in node.Members.OfType<OperatorDeclarationSyntax>())
        {
            var sig = OperatorOverloadWriter.BuildSignature(opDecl, name, BuildParamDecl);
            _ctx.Out.WriteLine(sig + ";");
        }
        // Write conversion operator signatures
        foreach (var convDecl in node.Members.OfType<ConversionOperatorDeclarationSyntax>())
        {
            var sig = OperatorOverloadWriter.BuildConversionSignature(convDecl, name, BuildParamDecl);
            _ctx.Out.WriteLine(sig + ";");
        }

        _ctx.Out.WriteLine();
    }

    // ── Methoden-Überladungen ─────────────────────────────────────────────────

    private void RegisterClassOverloads(TypeDeclarationSyntax node, string className)
    {
        if (_ctx.OverloadedMethods.ContainsKey(className)) return;
        var set = new HashSet<string>(StringComparer.Ordinal);
        foreach (var g in node.Members.OfType<MethodDeclarationSyntax>()
                     .GroupBy(m => m.Identifier.Text)
                     .Where(g => g.Count() > 1))
            set.Add(g.Key);
        _ctx.OverloadedMethods[className] = set;
    }

    // Returns "ClassName_MethodName" for unique methods, "ClassName_MethodName_N" for overloads.
    internal static string BuildCMethodName(string className, string methodName, int userParamCount,
        Dictionary<string, HashSet<string>> overloadedMethods)
    {
        if (overloadedMethods.TryGetValue(className, out var set) && set.Contains(methodName))
            return $"{className}_{methodName}_{userParamCount}";
        return $"{className}_{methodName}";
    }

    // ── Methoden-Signaturen ───────────────────────────────────────────────────

    private void WriteMethodSignature(MethodDeclarationSyntax method,
        string className, bool isStaticClass = false)
    {
        var isAbstract = method.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));
        var isStatic = method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))
                      || isStaticClass;

        bool isExtension = method.ParameterList.Parameters.FirstOrDefault()?.Modifiers
            .Any(m => m.IsKind(SyntaxKind.ThisKeyword)) == true;

        var returnType = ResolveMethodReturnType(method);
        int userParamCount = method.ParameterList.Parameters.Count;
        var name = BuildCMethodName(className, method.Identifier.Text, userParamCount, _ctx.OverloadedMethods);

        var parameters = new List<string>();
        if (!isStatic && !isExtension) parameters.Add(className + "* self");

        foreach (var p in method.ParameterList.Parameters)
        {
            if (p.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword))) continue;
            parameters.Add(BuildParamDecl(p));
        }

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
        var className = node.Identifier.Text;
        RegisterClassOverloads(node, className);
        foreach (var opDecl in node.Members.OfType<OperatorDeclarationSyntax>())
            WriteOperatorBody(opDecl, className);

        foreach (var convDecl in node.Members.OfType<ConversionOperatorDeclarationSyntax>())
            WriteConversionOperatorBody(convDecl, className);

        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
            VisitMethodDeclaration(method);
    }

    private void WriteConversionOperatorBody(ConversionOperatorDeclarationSyntax conv, string className)
    {
        // implicit/explicit operator TargetType(SourceType x)
        // → TargetType ClassName_to_TargetType(SourceType x) { ... }
        var retCsType = conv.Type.ToString().Trim();
        var retTypeMapped = TypeRegistry.MapType(retCsType);
        var retType = retTypeMapped + (NeedsPtr(retCsType) ? "*" : "");
        var isImplicit = conv.ImplicitOrExplicitKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ImplicitKeyword);
        var suffix = isImplicit ? "implicit_" : "explicit_";
        suffix += conv.Type.ToString().Trim().Replace(".", "_");

        var paramList = string.Join(", ",
            conv.ParameterList.Parameters.Select(p =>
            {
                var decl = BuildParamDecl(p);
                _ctx.LocalTypes[p.Identifier.Text] = p.Type?.ToString().Trim() ?? "int";
                return decl;
            }));

        OperatorOverloadWriter.RegisterConversion(className, conv.Type.ToString().Trim(), isImplicit);

        _ctx.ClearMethodContext();
        var preSigPosConv = _ctx.Out.GetStringBuilder().Length;

        _ctx.Out.WriteLine($"{retType} {className}_{suffix}({paramList})");
        _ctx.Out.WriteLine("{");
        _ctx.Indent();

        if (conv.Body != null)
            foreach (var stmt in conv.Body.Statements)
                _stmtWriter.Write(stmt);
        else if (conv.ExpressionBody != null)
            _ctx.WriteLine($"return {_exprWriter.Write(conv.ExpressionBody.Expression)};");

        _ctx.Dedent();
        _ctx.Out.WriteLine("}");
        _ctx.Out.WriteLine();

        if (_ctx.PendingLambdaPreludes.Count > 0)
        {
            var sb = _ctx.Out.GetStringBuilder();
            var methodText = sb.ToString(preSigPosConv, sb.Length - preSigPosConv);
            sb.Remove(preSigPosConv, sb.Length - preSigPosConv);
            _ctx.FlushLambdaPreludes();
            _ctx.Out.Write(methodText);
        }
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
        var isExtern = node.Modifiers.Any(m => m.IsKind(SyntaxKind.ExternKeyword));
        var isStatic = node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

        bool isExtension = node.ParameterList.Parameters.FirstOrDefault()?.Modifiers
            .Any(m => m.IsKind(SyntaxKind.ThisKeyword)) == true;

        string cReturnType;
        if (TypeRegistry.IsTuple(csReturnType))
        {
            var tupleStructName = TypeRegistry.GetTupleStructName(csReturnType);
            _ctx.CurrentTupleReturnType = tupleStructName;
            cReturnType = tupleStructName;

            if (_mode == TranspileMode.HeaderOnly)
            {
                var structDef = TypeRegistry.GenerateTupleStruct(csReturnType);
                if (!string.IsNullOrEmpty(structDef))
                    _ctx.Out.WriteLine(structDef);
            }
        }
        else
        {
            _ctx.CurrentTupleReturnType = null;
            cReturnType = TypeRegistry.MapType(csReturnType);
            if (!cReturnType.EndsWith("*") && NeedsPtr(csReturnType))
                cReturnType += "*";
        }

        int userParamCount = node.ParameterList.Parameters.Count;
        var name = string.IsNullOrEmpty(_ctx.CurrentClass)
            ? node.Identifier.Text
            : BuildCMethodName(_ctx.CurrentClass, node.Identifier.Text, userParamCount, _ctx.OverloadedMethods);

        var parameters = new List<string>();
        if (!string.IsNullOrEmpty(_ctx.CurrentClass) && !isStatic && !isExtension)
            parameters.Add(_ctx.CurrentClass + "* self");

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

        var sig = cReturnType + " " + name + "(" + string.Join(", ", parameters) + ")";

        if (_mode == TranspileMode.HeaderOnly)
        {
            if (isAbstract)
                _ctx.Out.WriteLine("/* abstract: " + sig + " */");
            else if (isExtern)
                // [DllImport] / extern method → emit as C extern declaration (links to native lib)
                _ctx.Out.WriteLine("extern " + sig + ";");
            else
                _ctx.Out.WriteLine(sig + ";");
            return;
        }

        if (isAbstract) return;
        // extern methods have no body — only the header declaration is needed
        if (isExtern) return;

        _ctx.ClearMethodContext();
        _ctx.IsStaticMethod = isStatic;
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
            _ctx.LocalTypes[p.Identifier.Text] =
                isRefParam ? "@ref:" + paramType : paramType;

            if (isParams && paramType.EndsWith("[]"))
            {
                var countVar = p.Identifier.Text + "_count";
                _ctx.LocalTypes[countVar] = "int";
            }
        }

        // FIX: Lambda-Preludes (Struct-Defs + statische Funktionen) müssen VOR der
        // Methodensignatur erscheinen, damit GCC die Funktionsnamen bei ihrer Nutzung
        // als Argument im Body bereits kennt (C99: kein implizites Function-Decl).
        // Da Preludes erst WÄHREND der Body-Verarbeitung gesammelt werden, markieren
        // wir die aktuelle Output-Position und verschieben die Preludes via
        // StringBuilder-Manipulation nachträglich an die richtige Stelle.
        var preSigPos = _ctx.Out.GetStringBuilder().Length;

        _ctx.Out.WriteLine(sig);
        _ctx.Out.WriteLine("{");
        _ctx.Indent();

        if (_ctx.CurrentReturnBuffer != null)
            _ctx.WriteLine("static char _ret_buf[CS2SX_RETURN_BUF_SIZE];");

        if (node.Body != null)
            foreach (var stmt in node.Body.Statements)
                _stmtWriter.Write(stmt);
        else if (node.ExpressionBody != null)
        {
            var exprCode = _exprWriter.Write(node.ExpressionBody.Expression);
            if (cReturnType == "void")
                _ctx.WriteLine(exprCode + ";");
            else
                _ctx.WriteLine("return " + exprCode + ";");
        }

        _ctx.Dedent();
        _ctx.Out.WriteLine("}");
        _ctx.Out.WriteLine();

        // Falls während der Body-Verarbeitung Lambda-Preludes gesammelt wurden:
        // Signatur + Body aus dem Buffer extrahieren, Preludes voranstellen, dann
        // den Method-Text wieder anfügen. So stehen die statischen Lambda-Funktionen
        // immer vor der Methode, die sie referenziert.
        if (_ctx.PendingLambdaPreludes.Count > 0)
        {
            var sb = _ctx.Out.GetStringBuilder();
            var methodText = sb.ToString(preSigPos, sb.Length - preSigPos);
            sb.Remove(preSigPos, sb.Length - preSigPos);
            _ctx.FlushLambdaPreludes();
            _ctx.Out.Write(methodText);
        }
    }

    // ── Records ───────────────────────────────────────────────────────────────

    public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
    {
        // record class → heap-allocated reference type (calloc + _rc ref-counting)
        // record struct → value type (plain C struct, no heap, no _rc)
        bool isRecordStruct = node.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword);

        var name = node.Identifier.Text;
        _ctx.ClearClassContext();
        _ctx.CurrentClass = name;

        if (isRecordStruct)
            _ctx.ValueTypeStructs.Add(name); // treat as value type in type system

        // Collect positional record params as fields
        var paramFields = node.ParameterList?.Parameters
            .Select(p => (
                csType: _ctx.ResolveAlias(p.Type?.ToString().Trim() ?? "int"),
                fieldName: p.Identifier.Text))
            .ToList() ?? new();

        // Also collect any additional properties/fields in the body
        foreach (var (csType, fieldName) in paramFields)
            _ctx.FieldTypes[fieldName] = csType;

        if (_mode == TranspileMode.HeaderOnly)
        {
            _ctx.Out.WriteLine($"struct {name}");
            _ctx.Out.WriteLine("{");
            _ctx.Indent();
            if (!isRecordStruct)
                _ctx.WriteLine("int _rc;");
            foreach (var (csType, fieldName) in paramFields)
            {
                var cType = TypeRegistry.MapType(csType);
                _ctx.WriteLine($"{cType}{(NeedsPtr(csType) ? "*" : "")} f_{fieldName};");
            }
            // Also emit regular members from the record body
            foreach (var field in node.Members.OfType<FieldDeclarationSyntax>()
                .Where(f => !f.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))))
            {
                var csType = _ctx.ResolveAlias(field.Declaration.Type.ToString().Trim());
                var cType = TypeRegistry.MapType(csType);
                foreach (var v in field.Declaration.Variables)
                    _ctx.WriteLine($"{cType}{(NeedsPtr(csType) ? "*" : "")} f_{v.Identifier.Text.TrimStart('_')};");
            }
            foreach (var prop in node.Members.OfType<PropertyDeclarationSyntax>()
                .Where(p => !p.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword))
                         && PropertyWriter.IsAutoProperty(p)))
            {
                var csType = _ctx.ResolveAlias(prop.Type.ToString().Trim());
                var cType = TypeRegistry.MapType(csType);
                _ctx.WriteLine($"{cType}{(NeedsPtr(csType) ? "*" : "")} f_{prop.Identifier.Text.TrimStart('_')};");
            }
            _ctx.Dedent();
            _ctx.Out.WriteLine("};");
            _ctx.Out.WriteLine();

            // Constructor signature
            if (isRecordStruct)
            {
                // record struct: stack-allocatable constructor (returns by value)
                var paramDecls = BuildRecordParamList(paramFields);
                _ctx.Out.WriteLine($"{name} {name}_Make({paramDecls});");
            }
            else
            {
                if (paramFields.Count > 0)
                {
                    var paramDecls = string.Join(", ",
                        paramFields.Select(pf =>
                        {
                            var ct = TypeRegistry.MapType(pf.csType);
                            var ptr = NeedsPtr(pf.csType) ? "*" : "";
                            return $"{ct}{ptr} {pf.fieldName}";
                        }));
                    _ctx.Out.WriteLine($"{name}* {name}_New({paramDecls});");
                }
                else
                {
                    _ctx.Out.WriteLine($"{name}* {name}_New();");
                }
                _ctx.Out.WriteLine($"void {name}_Free({name}* self);");
                _ctx.Out.WriteLine($"{name}* {name}_Retain({name}* self);");
            }

            // Method signatures from body
            RegisterClassOverloads(node, name);
            foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
                WriteMethodSignature(method, name);

            _ctx.Out.WriteLine();
        }
        else if (isRecordStruct)
        {
            // record struct implementation: returns value (no heap)
            _ctx.Out.WriteLine($"{name} {name}_Make({BuildRecordParamList(paramFields)})");
            _ctx.Out.WriteLine("{");
            _ctx.Indent();
            _ctx.WriteLine($"{name} self;");
            _ctx.WriteLine("memset(&self, 0, sizeof(self));");
            foreach (var (csType, fieldName) in paramFields)
                _ctx.WriteLine($"self.f_{fieldName} = {fieldName};");
            _ctx.Dedent();
            _ctx.Out.WriteLine("    return self;");
            _ctx.Out.WriteLine("}");
            _ctx.Out.WriteLine();

            RegisterClassOverloads(node, name);
            foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
                VisitMethodDeclaration(method);
        }
        else
        {
            // record class: heap-allocated, reference-counted
            _ctx.Out.WriteLine($"{name}* {name}_New({BuildRecordParamList(paramFields)})");
            _ctx.Out.WriteLine("{");
            _ctx.Indent();
            _ctx.WriteLine($"{name}* self = ({name}*)calloc(1, sizeof({name}));");
            _ctx.WriteLine("if (!self) return NULL;");
            _ctx.WriteLine("self->_rc = 1;");
            foreach (var (csType, fieldName) in paramFields)
                _ctx.WriteLine($"self->f_{fieldName} = {fieldName};");
            _ctx.Dedent();
            _ctx.Out.WriteLine("    return self;");
            _ctx.Out.WriteLine("}");
            _ctx.Out.WriteLine();

            // Free
            _ctx.Out.WriteLine($"void {name}_Free({name}* self)");
            _ctx.Out.WriteLine("{");
            _ctx.Indent();
            _ctx.WriteLine("if (!self) return;");
            _ctx.WriteLine("if (--self->_rc > 0) return;");
            foreach (var (csType, fieldName) in paramFields)
            {
                var fieldExpr = $"self->f_{fieldName}";
                if (TypeRegistry.IsList(csType))
                {
                    var inner = TypeRegistry.GetListInnerType(csType) ?? "int";
                    var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
                    _ctx.WriteLine($"if ({fieldExpr}) {{ List_{cInner}_Free({fieldExpr}); {fieldExpr} = NULL; }}");
                }
                else if (TypeRegistry.IsDictionary(csType))
                {
                    var types = TypeRegistry.GetDictionaryTypes(csType);
                    if (types.HasValue)
                    {
                        var ck = types.Value.key == "string" ? "str" : TypeRegistry.MapType(types.Value.key);
                        var cv = types.Value.val == "string" ? "str" : TypeRegistry.MapType(types.Value.val);
                        _ctx.WriteLine($"if ({fieldExpr}) {{ Dict_{ck}_{cv}_Free({fieldExpr}); {fieldExpr} = NULL; }}");
                    }
                }
                else if (TypeRegistry.IsStringBuilder(csType))
                {
                    _ctx.WriteLine($"if ({fieldExpr}) {{ StringBuilder_Free({fieldExpr}); {fieldExpr} = NULL; }}");
                }
                else if (NullableHandler.IsNullable(csType))
                {
                    _ctx.WriteLine($"if ({fieldExpr}) {{ free({fieldExpr}); {fieldExpr} = NULL; }}");
                }
                else if (TypeRegistry.NeedsPointerSuffix(csType)
                         && !TypeRegistry.IsPrimitive(csType)
                         && csType != "string"
                         && !TypeRegistry.IsLibNxStruct(csType))
                {
                    var cType = TypeRegistry.MapType(csType);
                    _ctx.WriteLine($"if ({fieldExpr}) {{ {cType}_Free({fieldExpr}); {fieldExpr} = NULL; }}");
                }
            }
            _ctx.WriteLine("free(self);");
            _ctx.Dedent();
            _ctx.Out.WriteLine("}");
            _ctx.Out.WriteLine();

            _ctx.Out.WriteLine($"{name}* {name}_Retain({name}* self)");
            _ctx.Out.WriteLine("{");
            _ctx.Out.WriteLine($"    if (self) self->_rc++;");
            _ctx.Out.WriteLine($"    return self;");
            _ctx.Out.WriteLine("}");
            _ctx.Out.WriteLine();

            // Method bodies
            RegisterClassOverloads(node, name);
            foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
                VisitMethodDeclaration(method);
        }

        _ctx.ClearClassContext();
    }

    private string BuildRecordParamList(List<(string csType, string fieldName)> paramFields)
    {
        if (paramFields.Count == 0) return "";
        return string.Join(", ", paramFields.Select(pf =>
        {
            var ct = TypeRegistry.MapType(pf.csType);
            var ptr = NeedsPtr(pf.csType) ? "*" : "";
            return $"{ct}{ptr} {pf.fieldName}";
        }));
    }

    // ── Indexer ───────────────────────────────────────────────────────────────

    public override void VisitIndexerDeclaration(IndexerDeclarationSyntax node)
    {
        var className = _ctx.CurrentClass;
        if (string.IsNullOrEmpty(className)) return;

        _ctx.IndexerClasses.Add(className);

        var retType = TypeRegistry.MapType(_ctx.ResolveAlias(node.Type.ToString().Trim()));
        var needsPtr = TypeRegistry.NeedsPointerSuffix(node.Type.ToString().Trim())
                    && !retType.EndsWith("*");
        var retDecl = retType + (needsPtr ? "*" : "");

        // Build index parameter declarations
        var indexParams = string.Join(", ",
            node.ParameterList.Parameters.Select(p => BuildParamDecl(p)));

        bool hasGet = node.AccessorList?.Accessors
            .Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)) == true;
        bool hasSet = node.AccessorList?.Accessors
            .Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)) == true;

        if (_mode == TranspileMode.HeaderOnly)
        {
            if (hasGet)
                _ctx.Out.WriteLine($"{retDecl} {className}_get({className}* self, {indexParams});");
            if (hasSet)
                _ctx.Out.WriteLine($"void {className}_set({className}* self, {indexParams}, {retDecl} value);");
            return;
        }

        // Implementation
        var getAccessor = node.AccessorList?.Accessors
            .FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
        if (getAccessor != null)
        {
            _ctx.ClearMethodContext();
            _ctx.CurrentReturnBuffer = null;
            foreach (var p in node.ParameterList.Parameters)
            {
                _ctx.LocalTypes[p.Identifier.Text] = p.Type?.ToString().Trim() ?? "int";
            }
            _ctx.Out.WriteLine($"{retDecl} {className}_get({className}* self, {indexParams})");
            _ctx.Out.WriteLine("{");
            _ctx.Indent();
            if (getAccessor.Body != null)
                foreach (var stmt in getAccessor.Body.Statements)
                    _stmtWriter.Write(stmt);
            else if (getAccessor.ExpressionBody != null)
                _ctx.WriteLine($"return {_exprWriter.Write(getAccessor.ExpressionBody.Expression)};");
            _ctx.Dedent();
            _ctx.Out.WriteLine("}");
            _ctx.Out.WriteLine();
        }

        var setAccessor = node.AccessorList?.Accessors
            .FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));
        if (setAccessor != null)
        {
            _ctx.ClearMethodContext();
            foreach (var p in node.ParameterList.Parameters)
                _ctx.LocalTypes[p.Identifier.Text] = p.Type?.ToString().Trim() ?? "int";
            _ctx.LocalTypes["value"] = node.Type.ToString().Trim();
            _ctx.Out.WriteLine($"void {className}_set({className}* self, {indexParams}, {retDecl} value)");
            _ctx.Out.WriteLine("{");
            _ctx.Indent();
            if (setAccessor.Body != null)
                foreach (var stmt in setAccessor.Body.Statements)
                    _stmtWriter.Write(stmt);
            else if (setAccessor.ExpressionBody != null)
                _ctx.WriteLine($"{_exprWriter.Write(setAccessor.ExpressionBody.Expression)};");
            _ctx.Dedent();
            _ctx.Out.WriteLine("}");
            _ctx.Out.WriteLine();
        }
    }

    // ── Property ─────────────────────────────────────────────────────────────

    public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        var csType = ResolvePropertyType(node);
        _ctx.PropertyTypes[node.Identifier.Text] = csType;
        if (PropertyWriter.IsAutoProperty(node))
            _ctx.FieldTypes[node.Identifier.Text] = csType;
        else
            _ctx.ComputedPropertyNames.Add(node.Identifier.Text);
        base.VisitPropertyDeclaration(node);
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    internal void LoadBaseFields(string baseType)
    {
        if (baseType is "Control" or "Label" or "Button" or "ProgressBar")
        {
            foreach (var f in TypeRegistry.ControlFields)
                _ctx.BaseFieldTypes[f] = "int";
            // PascalCase aliases so C# property access (X, Y, Width, Height) also resolves
            _ctx.BaseFieldTypes["X"] = "int";
            _ctx.BaseFieldTypes["Y"] = "int";
            _ctx.BaseFieldTypes["Width"] = "int";
            _ctx.BaseFieldTypes["Height"] = "int";
            _ctx.BaseFieldTypes["Visible"] = "int";
            _ctx.BaseFieldTypes["Focusable"] = "int";
        }

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

        // Benutzerdefinierte Basisklassen: Felder über das SemanticModel auflösen.
        // Ohne diese Info würde der Transpiler z.B. "self->speed" emittieren statt
        // "self->base.speed" für ein Feld, das in der Elternklasse definiert ist.
        if (_ctx.SemanticModel == null) return;
        if (IsControlSubclass(baseType) || baseType is "SwitchApp") return;

        try
        {
            var compilation = _ctx.SemanticModel.Compilation;
            var baseSym = compilation.GetTypeByMetadataName(baseType)
                       ?? compilation.GlobalNamespace
                              .GetTypeMembers(baseType)
                              .FirstOrDefault();
            if (baseSym == null) return;

            foreach (var member in baseSym.GetMembers())
            {
                string? csType = null;
                string memberName;
                switch (member)
                {
                    case IFieldSymbol f when !f.IsStatic && !f.IsConst:
                        csType = TranspilerContext.FormatTypeSymbol(f.Type);
                        memberName = f.Name.TrimStart('_');
                        _ctx.BaseFieldTypes[memberName] = csType;
                        break;
                    case IPropertySymbol p when !p.IsStatic:
                        csType = TranspilerContext.FormatTypeSymbol(p.Type);
                        memberName = p.Name;
                        _ctx.BaseFieldTypes[memberName] = csType;
                        break;
                }
            }
        }
        catch { }
    }

    internal static bool IsControlSubclass(string baseType) =>
        baseType is "Control" or "Label" or "Button" or "ProgressBar";

    internal static bool NeedsPtr(string csType)
    {
        var cMapped = TypeRegistry.MapType(csType);
        if (cMapped.EndsWith("*")) return false; // MapType already added pointer
        return TypeRegistry.NeedsPointerSuffix(csType)
            || TypeRegistry.IsStringBuilder(csType)
            || TypeRegistry.IsList(csType)
            || TypeRegistry.IsDictionary(csType)
            || TypeRegistry.IsControlType(csType)
            || NullableHandler.IsNullable(csType);
    }

    internal string BuildParamDecl(ParameterSyntax p) =>
        BuildParamDecl(p, skipThis: false);

    internal string BuildParamDecl(ParameterSyntax p, bool skipThis)
    {
        if (p.Type == null) return p.Identifier.Text;

        var csType = _ctx.ResolveAlias(p.Type.ToString().Trim());

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

        bool isRef = p.Modifiers.Any(m =>
            m.IsKind(SyntaxKind.RefKeyword) || m.IsKind(SyntaxKind.OutKeyword));

        if (_genericCollector != null && _genericCollector.GenericClasses.ContainsKey(csType))
            return csType + "* " + p.Identifier;

        var isParams = p.Modifiers.Any(m => m.IsKind(SyntaxKind.ParamsKeyword));
        if (isParams && csType.EndsWith("[]"))
        {
            var baseType = csType[..^2].Trim();
            var cBaseType = baseType == "string" ? "const char*" : TypeRegistry.MapType(baseType);
            return $"{cBaseType}* {p.Identifier}, int {p.Identifier}_count";
        }

        if (NullableHandler.IsNullable(csType))
        {
            var inner = NullableHandler.GetInnerType(csType);
            var cInner = TypeRegistry.MapType(inner);
            return isRef ? $"{cInner}** {p.Identifier}" : $"{cInner}* {p.Identifier}";
        }

        if (csType.EndsWith("[]"))
        {
            var baseType = csType[..^2].Trim();
            var cBase = baseType == "string" ? "const char*" : TypeRegistry.MapType(baseType);
            var arrPtr = TypeRegistry.NeedsPointerSuffix(baseType) ? "**" : "*";
            return $"{cBase}{arrPtr} {p.Identifier}";
        }

        if (_genericCollector != null && _genericCollector.Interfaces.ContainsKey(csType))
            return csType + "* " + p.Identifier;

        var cType = TypeRegistry.MapType(csType);
        var isPrim = TypeRegistry.IsPrimitive(csType)
                  || csType == "string"
                  || _ctx.EnumDefs.ContainsKey(csType)
                  || TypeRegistry.IsDelegate(csType); // function pointer typedefs — no extra *

        if (isRef)
        {
            if (csType == "string")
                return $"const char** {p.Identifier}";
            return $"{cType}* {p.Identifier}";
        }

        // MapType already appends '*' for List<T>, Dictionary<K,V> etc.
        // Do not add another '*' in that case to avoid double-pointer.
        var ptr = (!isPrim && !cType.EndsWith("*")) ? "*" : "";
        return $"{cType}{ptr} {p.Identifier}";
    }
}