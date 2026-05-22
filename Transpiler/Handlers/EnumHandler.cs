using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Transpiler.Writers;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Handles Enum.* static methods and instance ToString().
///
/// Enum.GetValues&lt;T&gt;() / Enum.GetValues(typeof(T))
///   → emits a static const T array of all members
///
/// Enum.Parse&lt;T&gt;(str) / (T)Enum.Parse(typeof(T), str)
///   → atoi(str) — simple numeric parse
///
/// Enum.GetName(typeof(T), val) / Enum.GetName&lt;T&gt;(val)
///   → switch/if-chain returning the name string
///
/// Enum.IsDefined(typeof(T), val)
///   → range check
///
/// someEnum.ToString() is handled by StringMethodHandler (member access .ToString())
/// </summary>
public sealed class EnumHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        // ── Enum.GetValues ────────────────────────────────────────────────────
        if (calleeStr is "Enum.GetValues" or "Enum.GetValuesAsUnderlyingType")
        {
            result = HandleGetValues(inv, args, ctx);
            return true;
        }

        // ── Enum.Parse ────────────────────────────────────────────────────────
        if (calleeStr is "Enum.Parse" or "Enum.TryParse")
        {
            result = HandleParse(inv, args, ctx, writeExpr, calleeStr == "Enum.TryParse");
            return true;
        }

        // ── Enum.GetName ──────────────────────────────────────────────────────
        if (calleeStr is "Enum.GetName")
        {
            result = HandleGetName(inv, args, ctx);
            return true;
        }

        // ── Enum.IsDefined ────────────────────────────────────────────────────
        if (calleeStr is "Enum.IsDefined")
        {
            string? enumTypeName = null;
            string valArg = args.Count >= 2 ? args[args.Count - 1] : (args.Count >= 1 ? args[0] : "0");
            if (inv.Expression is GenericNameSyntax gnDef && gnDef.TypeArgumentList.Arguments.Count > 0)
                enumTypeName = gnDef.TypeArgumentList.Arguments[0].ToString().Trim();
            else if (inv.ArgumentList.Arguments.Count >= 1
                  && inv.ArgumentList.Arguments[0].Expression is TypeOfExpressionSyntax tofDef)
                enumTypeName = tofDef.Type.ToString().Trim();

            if (enumTypeName != null && ctx.EnumDefs.TryGetValue(enumTypeName, out var defMembers))
            {
                var checks = string.Join(" || ", defMembers.Select(m => $"(({valArg}) == {m})"));
                result = "(" + checks + ")";
            }
            else
            {
                ctx.Warn(inv, "Enum.IsDefined — enum type not found; falling back to range check >= 0");
                result = $"((int)({valArg}) >= 0)";
            }
            return true;
        }

        // ── Enum.GetUnderlyingType ─────────────────────────────────────────────
        if (calleeStr is "Enum.GetUnderlyingType")
        {
            result = "0 /* Enum.GetUnderlyingType — not supported */";
            return true;
        }

        return NotHandled(out result);
    }

    private static string HandleGetValues(InvocationExpressionSyntax inv,
        List<string> args, TranspilerContext ctx)
    {
        // Extract enum type from either generic arg or typeof() arg
        string? enumTypeName = null;

        if (inv.Expression is GenericNameSyntax gn && gn.TypeArgumentList.Arguments.Count > 0)
            enumTypeName = gn.TypeArgumentList.Arguments[0].ToString().Trim();
        else if (inv.ArgumentList.Arguments.Count > 0
              && inv.ArgumentList.Arguments[0].Expression is TypeOfExpressionSyntax tof)
            enumTypeName = tof.Type.ToString().Trim();

        if (enumTypeName == null || !ctx.EnumDefs.TryGetValue(enumTypeName, out var members))
        {
            ctx.Warn($"Enum.GetValues — enum type not found; ensure enum is defined in same file",
                "Enum.GetValues");
            return "NULL /* Enum.GetValues — type not found */";
        }

        // Emit: static const EnumType _vals[] = { A, B, C };
        var arrName = ctx.NextTmp("enum_vals");
        var memberList = string.Join(", ", members);
        ctx.Out.WriteLine(ctx.Tab + $"static const {enumTypeName} {arrName}[] = {{ {memberList} }};");
        ctx.ArrayLengths[arrName] = members.Count.ToString();
        return arrName;
    }

    private static string HandleParse(InvocationExpressionSyntax inv,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, bool isTryParse)
    {
        // Extract the string argument (last arg for both Parse and TryParse)
        // Enum.Parse(typeof(T), str) → args[0]=typeof, args[1]=str
        // Enum.Parse<T>(str)         → args[0]=str
        // Enum.TryParse<T>(str, out result) → args[0]=str, args[1]=&result

        string? enumTypeName = null;
        if (inv.Expression is GenericNameSyntax gn && gn.TypeArgumentList.Arguments.Count > 0)
            enumTypeName = gn.TypeArgumentList.Arguments[0].ToString().Trim();
        else if (args.Count >= 2
              && inv.ArgumentList.Arguments[0].Expression is TypeOfExpressionSyntax tof2)
        {
            enumTypeName = tof2.Type.ToString().Trim();
            args = args.Skip(1).ToList();
        }

        var strArg = ArgAt(args, 0);

        if (isTryParse)
        {
            // TryParse<T>(str, out T result) → lookup by name, return 1 on success, 0 on failure
            var outArg = ArgAt(args, 1);
            if (enumTypeName != null && ctx.EnumDefs.TryGetValue(enumTypeName, out var tryMembers))
            {
                var okVar = ctx.NextTmp("ep_ok");
                ctx.WriteLine($"int {okVar} = 0;");
                foreach (var m in tryMembers)
                    ctx.WriteLine($"if (CS2SX_strcmp_safe({strArg}, \"{m}\") == 0) {{ *{outArg} = {m}; {okVar} = 1; }}");
                return okVar;
            }
            // Fallback: numeric parse always succeeds
            ctx.Out.WriteLine(ctx.Tab + $"*{outArg} = ({enumTypeName ?? "int"})atoi({strArg});");
            return "1";
        }

        // If we have enum defs, build a name-to-value lookup
        if (enumTypeName != null && ctx.EnumDefs.TryGetValue(enumTypeName, out var members))
        {
            var resultVar = ctx.NextTmp("ep");
            ctx.WriteLine($"{enumTypeName} {resultVar} = ({enumTypeName})0;");
            foreach (var m in members)
                ctx.WriteLine($"if (CS2SX_strcmp_safe({strArg}, \"{m}\") == 0) {resultVar} = {m};");
            return resultVar;
        }

        // Fallback: numeric parse
        return $"({enumTypeName ?? "int"})atoi({strArg})";
    }

    private static string HandleGetName(InvocationExpressionSyntax inv,
        List<string> args, TranspilerContext ctx)
    {
        // Enum.GetName(typeof(T), val) or Enum.GetName<T>(val)
        string? enumTypeName = null;
        string valArg;

        if (inv.Expression is GenericNameSyntax gn2 && gn2.TypeArgumentList.Arguments.Count > 0)
        {
            enumTypeName = gn2.TypeArgumentList.Arguments[0].ToString().Trim();
            valArg = ArgAt(args, 0);
        }
        else
        {
            // Enum.GetName(typeof(T), val)
            if (inv.ArgumentList.Arguments.Count >= 1
             && inv.ArgumentList.Arguments[0].Expression is TypeOfExpressionSyntax tof3)
                enumTypeName = tof3.Type.ToString().Trim();
            valArg = ArgAt(args, args.Count - 1);
        }

        if (enumTypeName != null && ctx.EnumDefs.TryGetValue(enumTypeName, out var members))
        {
            var buf = ctx.NextStringBuf();
            ctx.WriteLine($"snprintf({buf}, sizeof({buf}), \"%d\", (int)({valArg}));");
            bool first = true;
            foreach (var m in members)
            {
                var kw = first ? "if" : "else if";
                ctx.WriteLine($"{kw} ({valArg} == {m}) strncpy({buf}, \"{m}\", sizeof({buf}) - 1);");
                first = false;
            }
            return buf;
        }

        // Fallback: numeric string
        var fb = ctx.NextStringBuf();
        ctx.Out.WriteLine(ctx.Tab + $"snprintf({fb}, sizeof({fb}), \"%d\", (int)({valArg}));");
        return fb;
    }
}
