using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

public sealed class MotionHandler : InvocationHandlerBase
{
    private static readonly Dictionary<string, string> s_map =
        new(StringComparer.Ordinal)
        {
            ["Motion.IsAvailable"] = "CS2SX_Motion_IsAvailable",
            ["Motion.Get"]         = "CS2SX_Motion_Get",
            ["Motion.ResetAngles"] = "CS2SX_Motion_ResetAngles",
        };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        if (!s_map.TryGetValue(calleeStr, out var cFunc))
            return NotHandled(out result);
        result = cFunc + "(" + JoinArgs(args) + ")";
        return true;
    }
}
