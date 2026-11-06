using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Behandelt Color.RGB, Color.RGBA, Color-Konstanten und Color.WithAlpha.
///
/// Color.RGB(r,g,b)                → CS2SX_RGB(r,g,b)
/// Color.RGBA(r,g,b,a)             → CS2SX_RGBA(r,g,b,a)
/// Color.White                     → COLOR_WHITE
/// Color.Black.WithAlpha(128)      → Color_WithAlpha(COLOR_BLACK, 128)
/// someColor.WithAlpha(200)        → Color_WithAlpha(someColor, 200)
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
        // Fix 17: someColor.WithAlpha(alpha) → Color_WithAlpha(someColor, alpha)
        if (inv.Expression is MemberAccessExpressionSyntax mem
            && mem.Name.Identifier.Text == "WithAlpha")
        {
            var colorExpr = writeExpr(mem.Expression);
            // Color-Konstante auflösen: Color.Black → COLOR_BLACK
            var mapped = TypeRegistry.MapEnum(mem.Expression.ToString());
            if (mapped != mem.Expression.ToString())
                colorExpr = mapped;

            result = "Color_WithAlpha(" + colorExpr + ", " + ArgAt(args, 0) + ")";
            return true;
        }

        if (!s_methods.TryGetValue(calleeStr, out var cFunc))
            return NotHandled(out result);

        result = cFunc + "(" + JoinArgs(args) + ")";
        return true;
    }
}