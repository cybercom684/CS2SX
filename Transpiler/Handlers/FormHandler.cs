using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Behandelt Form-Aufrufe und SwitchApp-Methoden.
///
/// Erweiterung: SwitchApp_RequestExit, Form.Remove etc. hier ergänzen.
/// </summary>
public sealed class FormHandler : IInvocationHandler
{
    public bool CanHandle(InvocationExpressionSyntax inv, string calleeStr, TranspilerContext ctx)
        => calleeStr is "Form.Add" or "Add" or "SwitchApp_RequestExit";

    public string Handle(InvocationExpressionSyntax inv, string calleeStr, List<string> args,
        TranspilerContext ctx, Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        return calleeStr switch
        {
            "Form.Add" or "Add" =>
                "SwitchApp_Add((SwitchApp*)self, (Control*)" + args[0] + ")",
            "SwitchApp_RequestExit" =>
                "SwitchApp_RequestExit((SwitchApp*)self)",
            _ => "",
        };
    }
}
