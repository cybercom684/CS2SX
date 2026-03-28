using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Behandelt int.Parse, int.TryParse, float.Parse, float.TryParse.
///
/// int.Parse(s)              → CS2SX_Int_Parse(s)
/// int.TryParse(s, out val)  → CS2SX_Int_TryParse(s, &val)
/// float.Parse(s)            → CS2SX_Float_Parse(s)
/// float.TryParse(s, out val)→ CS2SX_Float_TryParse(s, &val)
/// </summary>
public sealed class ParseHandler : InvocationHandlerBase
{
    private static readonly Dictionary<string, string> s_map =
        new(StringComparer.Ordinal)
        {
            ["int.Parse"] = "CS2SX_Int_Parse",
            ["Int32.Parse"] = "CS2SX_Int_Parse",
            ["int.TryParse"] = "CS2SX_Int_TryParse",
            ["Int32.TryParse"] = "CS2SX_Int_TryParse",
            ["float.Parse"] = "CS2SX_Float_Parse",
            ["Single.Parse"] = "CS2SX_Float_Parse",
            ["float.TryParse"] = "CS2SX_Float_TryParse",
            ["Single.TryParse"] = "CS2SX_Float_TryParse",
        };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr, out string result)
    {
        if (!s_map.TryGetValue(calleeStr, out var cFunc))
            return NotHandled(out result);

        // TryParse: zweites Argument ist out → BuildArg hängt bereits & an
        result = cFunc + "(" + JoinArgs(args) + ")";
        return true;
    }
}