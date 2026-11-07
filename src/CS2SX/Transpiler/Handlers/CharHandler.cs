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
    private static readonly Dictionary<string, string> s_map =
        new(StringComparer.Ordinal)
        {
            ["char.IsDigit"]          = "isdigit",
            ["char.IsLetter"]         = "isalpha",
            ["char.IsLetterOrDigit"]  = "isalnum",
            ["char.IsWhiteSpace"]     = "isspace",
            ["char.IsUpper"]          = "isupper",
            ["char.IsLower"]          = "islower",
            ["char.IsPunctuation"]    = "ispunct",
            ["char.IsControl"]        = "iscntrl",
            ["char.ToUpper"]          = "toupper",
            ["char.ToLower"]          = "tolower",
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

        // ctype functions take/return int; the result is used as bool or char in C.
        result = cFunc + "(" + ArgAt(args, 0) + ")";
        return true;
    }
}
