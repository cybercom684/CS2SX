using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Behandelt Input-Aufrufe: Input.IsDown, Input.IsHeld, Input.IsUp.
///
/// Erweiterung: Neuen case in Handle() hinzufügen.
/// </summary>
public sealed class InputHandler : IInvocationHandler
{
    public bool CanHandle(InvocationExpressionSyntax inv, string calleeStr, TranspilerContext ctx)
        => calleeStr is "Input.IsDown" or "Input.IsHeld" or "Input.IsUp";

    public string Handle(InvocationExpressionSyntax inv, string calleeStr, List<string> args,
        TranspilerContext ctx, Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        var btn = TypeRegistry.MapEnum(args[0]);
        return calleeStr switch
        {
            "Input.IsDown" => "(((SwitchApp*)self)->kDown & " + btn + ")",
            "Input.IsHeld" => "(((SwitchApp*)self)->kHeld & " + btn + ")",
            "Input.IsUp"   => "(!(((SwitchApp*)self)->kHeld & " + btn + "))",
            _              => "0",
        };
    }
}
