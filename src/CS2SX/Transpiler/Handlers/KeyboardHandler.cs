using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

public sealed class KeyboardHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        switch (calleeStr)
        {
            case "Keyboard.Show":
                // 2nd arg (initial) defaults to "" when omitted — ArgAt returns "" for missing
                result = "CS2SX_Keyboard_Show(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")";
                return true;

            case "Keyboard.ShowPassword":
                result = "CS2SX_Keyboard_ShowPassword(" + ArgAt(args, 0) + ")";
                return true;

            case "Keyboard.ShowNumber":
                result = "CS2SX_Keyboard_ShowNumber(" + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")";
                return true;
        }
        return NotHandled(out result);
    }
}
