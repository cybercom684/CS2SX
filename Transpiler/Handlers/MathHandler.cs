using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Behandelt Math-Aufrufe inkl. System.Math.X Varianten.
///
/// Math.Abs(x)         → abs(x)
/// System.Math.Sqrt(x) → sqrtf(x)
/// Math.Clamp(v,lo,hi) → CLAMP(v,lo,hi)
/// </summary>
public sealed class MathHandler : InvocationHandlerBase
{
    private static readonly Dictionary<string, string> s_mathMap = new(StringComparer.Ordinal)
    {
        // Kurzform
        ["Math.Abs"] = "abs",
        ["Math.Min"] = "MIN",
        ["Math.Max"] = "MAX",
        ["Math.Sqrt"] = "sqrtf",
        ["Math.Floor"] = "floorf",
        ["Math.Ceil"] = "ceilf",
        ["Math.Ceiling"] = "ceilf",
        ["Math.Pow"] = "powf",
        ["Math.Sin"] = "sinf",
        ["Math.Cos"] = "cosf",
        ["Math.Tan"] = "tanf",
        ["Math.Atan2"] = "atan2f",
        ["Math.Clamp"] = "CLAMP",
        ["Math.Round"] = "roundf",
        ["Math.Sign"] = "CS2SX_Sign",

        // System.Math.X Varianten
        ["System.Math.Abs"] = "abs",
        ["System.Math.Min"] = "MIN",
        ["System.Math.Max"] = "MAX",
        ["System.Math.Sqrt"] = "sqrtf",
        ["System.Math.Floor"] = "floorf",
        ["System.Math.Ceil"] = "ceilf",
        ["System.Math.Ceiling"] = "ceilf",
        ["System.Math.Pow"] = "powf",
        ["System.Math.Sin"] = "sinf",
        ["System.Math.Cos"] = "cosf",
        ["System.Math.Tan"] = "tanf",
        ["System.Math.Atan2"] = "atan2f",
        ["System.Math.Clamp"] = "CLAMP",
        ["System.Math.Round"] = "roundf",
        ["System.Math.Sign"] = "CS2SX_Sign",
    };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr, out string result)
    {
        if (!s_mathMap.TryGetValue(calleeStr, out var cFunc))
            return NotHandled(out result);

        result = cFunc + "(" + JoinArgs(args) + ")";
        return true;
    }
}