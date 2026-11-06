using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

public sealed class VibrationHandler : InvocationHandlerBase
{
    private static readonly Dictionary<string, string> s_map =
        new(StringComparer.Ordinal)
        {
            ["Vibration.Rumble"]       = "CS2SX_Vibration_Rumble",
            ["Vibration.RumbleSimple"] = "CS2SX_Vibration_RumbleSimple",
            ["Vibration.Pulse"]        = "CS2SX_Vibration_Pulse",
            ["Vibration.Stop"]         = "CS2SX_Vibration_Stop",
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
