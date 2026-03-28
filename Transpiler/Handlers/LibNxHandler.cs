using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

public sealed class LibNxHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr, out string result)
    {
        if (!calleeStr.StartsWith("LibNX.", StringComparison.Ordinal))
            return NotHandled(out result);

        var mem = (MemberAccessExpressionSyntax)inv.Expression;
        result = mem.Name.Identifier.Text + "(" + JoinArgs(args) + ")";
        return true;
    }
}