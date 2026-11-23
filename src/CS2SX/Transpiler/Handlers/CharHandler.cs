using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Handles static char.* / Char.* classification and conversion methods,
/// mapping them to C &lt;ctype.h&gt; functions. Without this they fall through
/// to a raw passthrough (`char.IsDigit(c)`) that does not compile.
/// </summary>
public sealed class CharHandler : InvocationHandlerBase
{
    // Characters are UTF-16 units (u16): umlauts, Greek and Cyrillic letters too (rt_str16.c).
    private static readonly Dictionary<string, string> s_map =
        new(StringComparer.Ordinal)
        {
            ["char.IsDigit"]          = "CS2SX_CharIsDigit(X)",
            ["char.IsLetter"]         = "CS2SX_CharIsLetter(X)",
            ["char.IsLetterOrDigit"]  = "({ u16 _cs2sx_c = (X); CS2SX_CharIsLetter(_cs2sx_c) || CS2SX_CharIsDigit(_cs2sx_c); })",
            ["char.IsWhiteSpace"]     = "CS2SX_CharIsWhiteSpace(X)",
            ["char.IsUpper"]          = "CS2SX_CharIsUpper(X)",
            ["char.IsLower"]          = "CS2SX_CharIsLower(X)",
            ["char.IsPunctuation"]    = "({ u16 _cs2sx_c = (X); _cs2sx_c < 128 && ispunct(_cs2sx_c); })",
            ["char.IsControl"]        = "({ u16 _cs2sx_c = (X); _cs2sx_c < 32 || (_cs2sx_c >= 127 && _cs2sx_c < 160); })",
            ["char.ToUpper"]          = "CS2SX_CharUpper(X)",
            ["char.ToLower"]          = "CS2SX_CharLower(X)",
            ["char.ToUpperInvariant"] = "CS2SX_CharUpper(X)",
            ["char.ToLowerInvariant"] = "CS2SX_CharLower(X)",
        };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        // Normalize "Char.X" → "char.X"
        var key = calleeStr.StartsWith("Char.", StringComparison.Ordinal)
            ? "char." + calleeStr.Substring(5)
            : calleeStr;

        if (args.Count < 1 || !s_map.TryGetValue(key, out var cFunc))
            return NotHandled(out result);

        result = cFunc.Replace("(X)", "(" + ArgAt(args, 0) + ")");
        return true;
    }
}
