using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

public sealed class SaveDataHandler : InvocationHandlerBase
{
    private static readonly Dictionary<string, string> s_map =
        new(StringComparer.Ordinal)
        {
            ["SaveData.Mount"]   = "CS2SX_SaveData_Mount",
            ["SaveData.Read"]    = "CS2SX_SaveData_Read",
            ["SaveData.Write"]   = "CS2SX_SaveData_Write",
            ["SaveData.Commit"]  = "CS2SX_SaveData_Commit",
            ["SaveData.Unmount"] = "CS2SX_SaveData_Unmount",
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
