using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Writers;

/// <summary>
/// The C model of C# delegates (runtime: cs2sx_delegate.h). Every delegate type is
/// a typedef of <c>CS2SX_Delegate*</c> named by <see cref="TypeRegistry.MapType"/>
/// (Func&lt;int, int&gt; → Func_int_int_t) with a typed invoker next to it
/// (Func_int_int_Invoke). Lifted functions take the bound object first:
/// <c>R fn(void* target, A…)</c>.
/// </summary>
internal static class DelegateModel
{
    public static bool IsDelegate(ITypeSymbol? type) => type?.TypeKind == TypeKind.Delegate;

    /// C typedef of a delegate type: Action_int_t.
    public static string CTypedef(ITypeSymbol type) =>
        TypeRegistry.MapType(TranspilerContext.FormatTypeSymbol(type));

    public static string InvokeName(string cTypedef) =>
        (cTypedef.EndsWith("_t") ? cTypedef[..^2] : cTypedef) + "_Invoke";

    public static string InvokeName(ITypeSymbol type) => InvokeName(CTypedef(type));

    /// `typedef CS2SX_Delegate* Func_int_int_t;` plus its invoker, for _forward.h.
    /// <paramref name="paramCTypes"/>/<paramref name="retCType"/> are the C types of the
    /// delegate signature ("void" for Action).
    public static string TypedefDecl(string cTypedef, IReadOnlyList<string> paramCTypes, string retCType)
    {
        var invoke = InvokeName(cTypedef);
        var parms = new List<string> { cTypedef + " d" };
        var args = new List<string> { "d->target" };
        for (int i = 0; i < paramCTypes.Count; i++)
        {
            parms.Add(paramCTypes[i] + " a" + i);
            args.Add("a" + i);
        }
        var fnType = retCType + " (*)(" + string.Join(", ", new[] { "void*" }.Concat(paramCTypes)) + ")";
        var call = "((" + fnType + ")d->fn)(" + string.Join(", ", args) + ")";
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"#ifndef CS2SX_DELEGATE_{cTypedef}");
        sb.AppendLine($"#define CS2SX_DELEGATE_{cTypedef}");
        sb.AppendLine($"typedef CS2SX_Delegate* {cTypedef};");
        sb.AppendLine($"static inline {retCType} {invoke}({string.Join(", ", parms)})");
        sb.AppendLine("{");
        sb.AppendLine("    CS2SX_Delegate_CheckNull(d);");
        if (retCType == "void")
            sb.AppendLine($"    for (; d; d = d->next) {call};");
        else
        {
            sb.AppendLine($"    {retCType} r = {{0}};");
            sb.AppendLine($"    for (; d; d = d->next) r = {call};");
            sb.AppendLine("    return r;");
        }
        sb.AppendLine("}");
        sb.AppendLine("#endif");
        return sb.ToString();
    }

    /// C type of a delegate parameter / return type (reference types get their `*`).
    public static string SignatureCType(string csType)
    {
        if (csType == "bool") return "int";
        var mapped = TypeRegistry.MapType(csType);
        if (!mapped.EndsWith("*") && CSharpToC.NeedsPtr(csType)) mapped += "*";
        return mapped;
    }

    // ── Method group → delegate ──────────────────────────────────────────────

    /// <c>OnChanged</c> / <c>Helper.Parse</c> converted to a delegate. Instance methods
    /// are bound to their receiver and called directly (their C signature already
    /// starts with the object pointer); static methods get a thunk that drops it.
    public static string MethodGroup(IMethodSymbol method, ExpressionSyntax expr,
        TranspilerContext ctx, Func<SyntaxNode?, string> write)
    {
        var owner = method.ContainingType?.Name ?? ctx.CurrentClass;
        var cName = CSharpToC.BuildCMethodName(owner, method.Name, method.Parameters.Length,
            ctx.OverloadedMethods);

        if (!method.IsStatic && !method.IsExtensionMethod)
        {
            string target = expr is MemberAccessExpressionSyntax ma
                ? write(ma.Expression)
                : "self";
            if (method.ContainingType?.Name != ctx.CurrentClass && target == "self")
                target = "(" + owner + "*)self";
            // The delegate keeps its target object alive (reference-counted classes).
            if (TypeRegistry.ArcRoot(owner) is { } root)
                target = "CS2SX_RETAIN(" + root + ", " + target + ")";
            return "CS2SX_Delegate_New((void*)" + cName + ", " + target + ", 0)";
        }

        var thunk = "_mg_" + cName;
        if (ctx.EmittedThunks.Add(thunk))
        {
            var ret = SignatureCType(TranspilerContext.FormatTypeSymbol(method.ReturnType));
            var ps = method.Parameters
                .Select((p, i) => SignatureCType(TranspilerContext.FormatTypeSymbol(p.Type)) + " a" + i)
                .ToList();
            var callArgs = string.Join(", ", method.Parameters.Select((_, i) => "a" + i));
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"static {ret} {thunk}({string.Join(", ", new[] { "void* _t" }.Concat(ps))})");
            sb.AppendLine("{");
            sb.AppendLine("    (void)_t;");
            sb.AppendLine(ret == "void" ? $"    {cName}({callArgs});" : $"    return {cName}({callArgs});");
            sb.AppendLine("}");
            sb.AppendLine($"static CS2SX_Delegate {thunk}_dg = CS2SX_DELEGATE_STATIC({thunk});");
            ctx.PendingLambdaPreludes.Add(sb.ToString());
        }
        return "(&" + thunk + "_dg)";
    }

    // ── Callbacks of inline handlers (List.ForEach, Sort, Array.Sort) ──────────

    /// A callable for a callback argument a handler invokes in a loop it writes
    /// itself: a lambda is lifted inline (static function, no allocation); any other
    /// delegate value is evaluated once and called through its invoker.
    /// Returns a function building the call from the argument list.
    public static Func<string, string> InlineCallable(ExpressionSyntax arg, TranspilerContext ctx,
        string? elementTypeHint, Func<SyntaxNode?, string> write)
    {
        if (arg is LambdaExpressionSyntax lambda)
        {
            var lifter = new LambdaLifter(ctx, new ExpressionWriter(ctx));
            lifter.SetStatementWriter(new StatementWriter(ctx, new ExpressionWriter(ctx)));
            string? hint = null;
            try
            {
                var t = ctx.SemanticModel?.GetTypeInfo(lambda).ConvertedType;
                if (t != null && t is not IErrorTypeSymbol) hint = TranspilerContext.FormatTypeSymbol(t);
            }
            catch { }
            var fn = lifter.LiftLambda(lambda, hintType: hint, elementTypeHint: elementTypeHint);
            return a => fn + "(" + a + ")";
        }

        var type = ctx.SemanticModel?.GetTypeInfo(arg).ConvertedType;
        var value = write(arg);
        if (!IsDelegate(type)) return a => value + "(" + a + ")";
        var tmp = ctx.NextTmp("cb");
        ctx.WriteLine("__auto_type " + tmp + " = " + value + ";");
        var invoke = InvokeName(type!);
        return a => invoke + "(" + tmp + (a.Length > 0 ? ", " + a : "") + ")";
    }
}
