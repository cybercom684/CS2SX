using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

public sealed class InputHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr, out string result)
    {
        if (calleeStr is not ("Input.IsDown" or "Input.IsHeld" or "Input.IsUp"))
            return NotHandled(out result);

        var btn = TypeRegistry.MapEnum(ArgAt(args, 0));
        result = calleeStr switch
        {
            "Input.IsDown" => "(g_cs2sx_kDown & " + btn + ")",
            "Input.IsHeld" => "(g_cs2sx_kHeld & " + btn + ")",
            "Input.IsUp"   => "(g_cs2sx_kUp   & " + btn + ")",
            _ => "0",
        };
        return true;
    }
}