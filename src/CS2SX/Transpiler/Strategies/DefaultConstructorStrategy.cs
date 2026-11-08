using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Strategies;

/// <summary>
/// Constructors of ordinary classes (not SwitchApp, not Control subclasses).
/// Every instance constructor becomes a pair (names: <see cref="CtorNaming"/>):
/// <code>
///   void Point_Ctor_2(Point* self, int x, int y)   // initialises in place
///   {
///       Point_Ctor_3(self, x, y, "p");   // : this(x, y, "p")      — or, otherwise:
///       field initializers;              //   C# order: initializers, then base ctor,
///       Base_Ctor((Base*)self, …);       //   : base(…) / implicit parameterless base
///       body;
///   }
///   Point* Point_New_2(int x, int y)       // allocates, then runs the Ctor
/// </code>
/// `_New` sets refcount, vtable and (exception classes) the runtime type before the
/// constructor body runs, so virtual calls in constructors dispatch like in C#.
/// </summary>
public sealed class DefaultConstructorStrategy : IConstructorStrategy
{
    public bool Matches(ClassDeclarationSyntax node, string baseType) => true;

    public void Write(ClassDeclarationSyntax node, string name, string baseType,
        TranspilerContext ctx, ExpressionWriter exprWriter, CSharpToC transpiler)
    {
        transpiler.WriteStaticFieldDefinitions(node, name);

        // Record position before vtable/ctor output so lambda preludes from field
        // initializers or constructor bodies can be inserted in front of them.
        var preSigPos = ctx.Out.GetStringBuilder().Length;

        var classSym = ctx.SemanticModel?.GetDeclaredSymbol(node) as INamedTypeSymbol;
        var model = new ClassModel(node, name, baseType, ctx, classSym);

        if (model.BaseVTable)
        {
            // Forward-declared: the full definition follows the method bodies.
            ctx.Out.WriteLine($"static {model.VTableType}_vtable {name}_vtable_instance;");
            ctx.Out.WriteLine();
        }
        else if (model.OwnVTable)
        {
            VTableBuilder.WriteRootVTableInstance(node, name, ctx.Out);
        }

        if (model.IsException)
        {
            ctx.Out.WriteLine(ExceptionModel.TypeInfoDefinition(name, model.ExceptionBase));
            ctx.Out.WriteLine();
        }

        var ctors = CtorNaming.InstanceCtors(node);
        if (ctors.Count == 0)
        {
            WriteCtor(null, "", model, ctx, exprWriter, transpiler);
            WriteNew(null, "", model, ctx, transpiler);
        }
        foreach (var ctor in ctors)
        {
            var sfx = CtorNaming.Suffix(ctors, ctor);
            WriteCtor(ctor, sfx, model, ctx, exprWriter, transpiler);
            WriteNew(ctor, sfx, model, ctx, transpiler);
        }

        if (ctx.PendingLambdaPreludes.Count > 0)
        {
            var sb = ctx.Out.GetStringBuilder();
            var ctorText = sb.ToString(preSigPos, sb.Length - preSigPos);
            sb.Remove(preSigPos, sb.Length - preSigPos);
            ctx.FlushLambdaPreludes();
            ctx.Out.Write(ctorText);
        }
    }

    // ── Header prototypes (called from CSharpToC.WriteFunctionSignatures) ────

    internal static void WritePrototypes(ClassDeclarationSyntax node, string name,
        TranspilerContext ctx, CSharpToC transpiler)
    {
        var classSym = ctx.SemanticModel?.GetDeclaredSymbol(node) as INamedTypeSymbol;
        if (TypeRegistry.IsExceptionType(classSym))
            ctx.Out.WriteLine($"extern const CS2SX_TypeInfo {TypeRegistry.ExceptionTypeInfo(name)};");

        var ctors = CtorNaming.InstanceCtors(node);
        if (ctors.Count == 0)
        {
            ctx.Out.WriteLine($"{name}* {name}_New(void);");
            ctx.Out.WriteLine($"void {name}_Ctor({name}* self);");
            return;
        }
        foreach (var ctor in ctors)
        {
            var sfx = CtorNaming.Suffix(ctors, ctor);
            var decls = ctor.ParameterList.Parameters.Select(p => transpiler.BuildParamDecl(p)).ToList();
            ctx.Out.WriteLine($"{name}* {name}_New{sfx}({(decls.Count > 0 ? string.Join(", ", decls) : "void")});");
            decls.Insert(0, name + "* self");
            ctx.Out.WriteLine($"void {name}_Ctor{sfx}({string.Join(", ", decls)});");
        }
    }

    // ── T_Ctor ───────────────────────────────────────────────────────────────

    private static void WriteCtor(ConstructorDeclarationSyntax? ctor, string sfx, ClassModel m,
        TranspilerContext ctx, ExpressionWriter exprWriter, CSharpToC transpiler)
    {
        var name = m.Name;
        var decls = new List<string> { name + "* self" };
        var paramNames = new List<string>();
        if (ctor != null)
        {
            foreach (var p in ctor.ParameterList.Parameters)
            {
                decls.Add(transpiler.BuildParamDecl(p));
                paramNames.Add(p.Identifier.Text);
                ctx.LocalTypes[p.Identifier.Text] = p.Type?.ToString().Trim() ?? "int";
            }
        }

        ctx.Out.WriteLine($"void {name}_Ctor{sfx}({string.Join(", ", decls)})");
        ctx.Out.WriteLine("{");
        ctx.Indent();
        transpiler.WriteTraceFrame(".ctor");

        var init = ctor?.Initializer;
        if (init != null && init.ThisOrBaseKeyword.IsKind(SyntaxKind.ThisKeyword))
        {
            // : this(...) — the target constructor runs initializers and the base call.
            var target = ctx.SemanticModel?.GetSymbolInfo(init).Symbol as IMethodSymbol;
            var args = exprWriter.BuildCtorArgs(name, init.ArgumentList);
            args.Insert(0, "self");
            ctx.WriteLine($"{name}_Ctor{CtorNaming.SuffixFor(target)}({string.Join(", ", args)});");
        }
        else
        {
            transpiler.WriteInstanceFieldInitializers(m.Node);
            WriteBaseCall(init, m, ctx, exprWriter);
        }

        if (ctor?.Body != null)
        {
            var stmtWriter = new StatementWriter(ctx, exprWriter);
            foreach (var stmt in ctor.Body.Statements)
                stmtWriter.Write(stmt);
        }
        else if (ctor?.ExpressionBody != null)
        {
            ctx.WriteLine(exprWriter.Write(ctor.ExpressionBody.Expression) + ";");
        }

        ctx.Dedent();
        ctx.Out.WriteLine("}");
        ctx.Out.WriteLine();

        foreach (var p in paramNames)
            ctx.LocalTypes.Remove(p);
    }

    private static void WriteBaseCall(ConstructorInitializerSyntax? init, ClassModel m,
        TranspilerContext ctx, ExpressionWriter exprWriter)
    {
        var baseSym = m.Symbol?.BaseType;
        var target = init != null
            ? ctx.SemanticModel?.GetSymbolInfo(init).Symbol as IMethodSymbol
            : CtorNaming.ImplicitBaseCtor(baseSym);
        var args = init != null
            ? exprWriter.BuildCtorArgs(m.BaseType, init.ArgumentList)
            : new List<string>();

        if (m.UserBase)
        {
            // Implicit base constructor with optional parameters: fill the defaults.
            if (init == null && target != null)
                foreach (var p in target.Parameters)
                    args.Add(ExpressionWriter.DefaultValueLiteral(p));
            args.Insert(0, $"({m.BaseType}*)self");
            ctx.WriteLine($"{m.BaseType}_Ctor{CtorNaming.SuffixFor(target)}({string.Join(", ", args)});");
        }
        else if (m.IsException && TypeRegistry.IsBuiltinException(m.BaseType))
        {
            ctx.WriteLine(ExceptionModel.InitCall("self", target, args) + ";");
        }
    }

    // ── T_New ────────────────────────────────────────────────────────────────

    private static void WriteNew(ConstructorDeclarationSyntax? ctor, string sfx, ClassModel m,
        TranspilerContext ctx, CSharpToC transpiler)
    {
        var name = m.Name;
        var decls = ctor?.ParameterList.Parameters.Select(p => transpiler.BuildParamDecl(p)).ToList()
                    ?? new List<string>();
        var names = ctor?.ParameterList.Parameters.Select(p => p.Identifier.Text).ToList()
                    ?? new List<string>();

        ctx.Out.WriteLine($"{name}* {name}_New{sfx}({(decls.Count > 0 ? string.Join(", ", decls) : "void")})");
        ctx.Out.WriteLine("{");
        ctx.Indent();
        ctx.WriteLine($"{name}* self = ({name}*)calloc(1, sizeof({name}));");
        ctx.WriteLine("if (!self) return NULL;");
        if (m.RcPath != null)
            ctx.WriteLine($"self->{m.RcPath}_rc = 1;");
        if (m.RcPath != null && Arc.UsesDispose(m.Symbol))
            ctx.WriteLine($"self->{m.RcPath}_dtor = (void (*)(void*)){name}_Dispose;");
        if (m.BaseVTable || m.OwnVTable)
            ctx.WriteLine($"self->{m.RcPath}vtable = &{name}_vtable_instance;");
        if (m.IsException)
            ctx.WriteLine($"((Exception*)self)->_type = &{TypeRegistry.ExceptionTypeInfo(name)};");
        names.Insert(0, "self");
        ctx.WriteLine($"{name}_Ctor{sfx}({string.Join(", ", names)});");
        // The initial reference belongs to the current frame: whatever keeps the
        // object (field, collection, …) takes its own; otherwise it is freed when
        // the next frame starts.
        if (m.RcPath != null && Arc.UsesDispose(m.Symbol))
            ctx.WriteLine($"CS2SX_ReleaseLater(self, (CS2SX_FreeFn){name}_Free);");
        ctx.WriteLine("return self;");
        ctx.Dedent();
        ctx.Out.WriteLine("}");
        ctx.Out.WriteLine();
    }

    // ── What the class derives from ───────────────────────────────────────────

    private sealed class ClassModel
    {
        public ClassDeclarationSyntax Node { get; }
        public string Name { get; }
        public string BaseType { get; }
        public INamedTypeSymbol? Symbol { get; }

        /// Base is a user class (has a T_Ctor to chain to).
        public bool UserBase { get; }
        public bool IsException { get; }
        /// Name of the exception parent (user class or System type) for the type info.
        public string ExceptionBase { get; } = "Exception";
        /// Path from self to the struct holding `_rc`/`vtable` ("" = self, "base." …);
        /// null when the class has no refcount (Control-like bases).
        public string? RcPath { get; }
        public bool BaseVTable { get; }
        public bool OwnVTable { get; }
        public string VTableType { get; }

        public ClassModel(ClassDeclarationSyntax node, string name, string baseType,
            TranspilerContext ctx, INamedTypeSymbol? sym)
        {
            Node = node; Name = name; BaseType = baseType; Symbol = sym;
            bool hasBase = !string.IsNullOrEmpty(baseType);
            bool runtimeBase = baseType == "SwitchApp" || CSharpToC.IsControlSubclass(baseType)
                               || ctx.InterfaceTypes.Contains(baseType);
            IsException = TypeRegistry.IsExceptionType(sym);
            if (IsException && sym?.BaseType != null) ExceptionBase = sym.BaseType.Name;
            UserBase = hasBase && !runtimeBase && !TypeRegistry.IsBuiltinException(baseType);

            int hops = ctx.RootHops.TryGetValue(name, out var rh) && rh > 0 ? rh : 1;
            var chain = string.Concat(Enumerable.Repeat("base.", hops));
            RcPath = !hasBase ? "" : runtimeBase ? null : chain;

            BaseVTable = hasBase && !runtimeBase && ctx.VTableTypes.Contains(baseType);
            OwnVTable = !hasBase && VTableBuilder.HasVirtualMethods(node);
            VTableType = ctx.VTableRoot.TryGetValue(name, out var vr) ? vr : baseType;
        }
    }
}
