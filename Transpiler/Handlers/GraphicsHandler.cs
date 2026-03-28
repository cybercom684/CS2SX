using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

public sealed class GraphicsHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        if (!calleeStr.StartsWith("Graphics.", StringComparison.Ordinal))
            return NotHandled(out result);

        var methodName = calleeStr["Graphics.".Length..];
        result = "Graphics_" + methodName + "(" + JoinArgs(args) + ")";
        return true;
    }
}