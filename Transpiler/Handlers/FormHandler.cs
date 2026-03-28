using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

public sealed class FormHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr, out string result)
    {
        if (calleeStr is not ("Form.Add" or "Add" or "SwitchApp_RequestExit"))
            return NotHandled(out result);

        result = calleeStr switch
        {
            "Form.Add" or "Add" =>
                "SwitchApp_Add((SwitchApp*)self, (Control*)" + ArgAt(args, 0) + ")",
            "SwitchApp_RequestExit" =>
                "SwitchApp_RequestExit((SwitchApp*)self)",
            _ => "",
        };
        return true;
    }
}