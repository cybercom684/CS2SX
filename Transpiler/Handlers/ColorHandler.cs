using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Behandelt Color.RGB, Color.RGBA und Color-Konstanten.
/// Color.RGB(r,g,b)     → CS2SX_RGB(r,g,b)
/// Color.RGBA(r,g,b,a)  → CS2SX_RGBA(r,g,b,a)
/// Color.White          → COLOR_WHITE
/// </summary>
public sealed class ColorHandler : InvocationHandlerBase
{
    private static readonly Dictionary<string, string> s_methods =
        new(StringComparer.Ordinal)
        {
            ["Color.RGB"] = "CS2SX_RGB",
            ["Color.RGBA"] = "CS2SX_RGBA",
        };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        if (!s_methods.TryGetValue(calleeStr, out var cFunc))
            return NotHandled(out result);

        result = cFunc + "(" + JoinArgs(args) + ")";
        return true;
    }
}