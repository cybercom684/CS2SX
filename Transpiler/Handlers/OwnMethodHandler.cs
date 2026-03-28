using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

public sealed class OwnMethodHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr, out string result)
    {
        if (string.IsNullOrEmpty(ctx.CurrentClass)
            || inv.Expression is not IdentifierNameSyntax
            || calleeStr.Contains('.')
            || calleeStr.Length == 0
            || !char.IsUpper(calleeStr[0]))
            return NotHandled(out result);

        var allArgs = new List<string> { "self" };
        allArgs.AddRange(args);
        result = ctx.CurrentClass + "_" + calleeStr + "(" + string.Join(", ", allArgs) + ")";
        return true;
    }
}