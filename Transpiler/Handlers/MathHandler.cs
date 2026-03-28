using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

public sealed class MathHandler : InvocationHandlerBase
{
    private static readonly Dictionary<string, string> s_mathMap = new(StringComparer.Ordinal)
    {
        ["Math.Abs"] = "abs",
        ["Math.Min"] = "MIN",
        ["Math.Max"] = "MAX",
        ["Math.Sqrt"] = "sqrtf",
        ["Math.Floor"] = "floorf",
        ["Math.Ceil"] = "ceilf",
        ["Math.Pow"] = "powf",
        ["Math.Sin"] = "sinf",
        ["Math.Cos"] = "cosf",
        ["Math.Clamp"] = "CLAMP",
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