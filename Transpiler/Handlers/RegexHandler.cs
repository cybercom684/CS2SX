using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Handles System.Text.RegularExpressions.Regex calls.
///
/// Maps to POSIX regex (regex.h / regcomp / regexec) which is available on
/// the Nintendo Switch via musl libc.
///
/// Regex.IsMatch(input, pattern)          → CS2SX_Regex_IsMatch(input, pattern)
/// Regex.Match(input, pattern)            → CS2SX_Regex_Match(input, pattern, buf)
/// Regex.Replace(input, pattern, replace) → CS2SX_Regex_Replace(input, pattern, replace, buf)
/// Regex.Split(input, pattern)            → CS2SX_Regex_Split(input, pattern)   (List_str*)
///
/// Instance form:
///   var rx = new Regex(pattern)
///   rx.IsMatch(input)   → CS2SX_Regex_IsMatch(input, pattern_stored)
///   rx.Match(input)     → CS2SX_Regex_Match(input, pattern_stored, buf)
///   rx.Replace(input, replace) → CS2SX_Regex_Replace(input, pattern_stored, replace, buf)
/// </summary>
public sealed class RegexHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        // ── Static methods ────────────────────────────────────────────────────
        switch (calleeStr)
        {
            case "Regex.IsMatch":
                if (args.Count < 2) { result = "0"; return true; }
                result = $"CS2SX_Regex_IsMatch({args[0]}, {args[1]})";
                return true;

            case "Regex.Match":
            {
                if (args.Count < 2) { result = "\"\""; return true; }
                var buf = ctx.NextStringBuf();
                ctx.WriteLine($"CS2SX_Regex_Match({args[0]}, {args[1]}, {buf}, sizeof({buf}));");
                result = buf;
                return true;
            }

            case "Regex.Replace":
                if (args.Count < 3) { result = args.Count > 0 ? args[0] : "\"\""; return true; }
            {
                var buf = ctx.NextStringBuf(1024);
                ctx.WriteLine($"CS2SX_Regex_Replace({args[0]}, {args[1]}, {args[2]}, {buf}, sizeof({buf}));");
                result = buf;
                return true;
            }

            case "Regex.Split":
                if (args.Count < 2) { result = "NULL"; return true; }
                result = $"CS2SX_Regex_Split({args[0]}, {args[1]})";
                return true;

            case "Regex.Escape":
                if (args.Count < 1) { result = "\"\""; return true; }
                result = args[0];
                return true;
        }

        // ── Instance methods: rx.IsMatch / rx.Match / rx.Replace ─────────────
        if (inv.Expression is not MemberAccessExpressionSyntax mem)
            return NotHandled(out result);

        var method = mem.Name.Identifier.Text;
        if (method is not ("IsMatch" or "Match" or "Replace" or "Split" or "Matches"))
            return NotHandled(out result);

        var rawRecv = mem.Expression.ToString();
        var recvKey = rawRecv.TrimStart('_');
        string? recvType = null;
        ctx.LocalTypes.TryGetValue(rawRecv, out recvType);
        if (recvType == null) ctx.FieldTypes.TryGetValue(recvKey, out recvType);
        if (recvType == null) recvType = ctx.GetSemanticType(mem.Expression);
        if (recvType != "Regex") return NotHandled(out result);

        var rxObj = writeExpr(mem.Expression);
        var input = args.Count > 0 ? args[0] : "\"\"";

        switch (method)
        {
            case "IsMatch":
                result = $"CS2SX_Regex_IsMatch({input}, {rxObj}->pattern)";
                return true;

            case "Match":
            case "Matches":
            {
                var buf = ctx.NextStringBuf();
                ctx.WriteLine($"CS2SX_Regex_Match({input}, {rxObj}->pattern, {buf}, sizeof({buf}));");
                result = buf;
                return true;
            }

            case "Replace":
            {
                var replacement = args.Count > 1 ? args[1] : "\"\"";
                var buf = ctx.NextStringBuf(1024);
                ctx.WriteLine($"CS2SX_Regex_Replace({input}, {rxObj}->pattern, {replacement}, {buf}, sizeof({buf}));");
                result = buf;
                return true;
            }

            case "Split":
                result = $"CS2SX_Regex_Split({input}, {rxObj}->pattern)";
                return true;
        }

        return NotHandled(out result);
    }
}
