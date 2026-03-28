using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

public sealed class ConsoleHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr, out string result)
    {
        if (calleeStr is not ("Console.Write" or "Console.WriteLine"))
            return NotHandled(out result);

        var newline = calleeStr == "Console.WriteLine";
        var nl = newline ? "\\n" : "";

        if (args.Count == 0)
        {
            result = newline ? "printf(\"\\n\")" : "printf(\"\")";
            return true;
        }

        var firstArg = inv.ArgumentList.Arguments[0].Expression;

        if (firstArg is LiteralExpressionSyntax lit
            && lit.Token.IsKind(SyntaxKind.StringLiteralToken))
        {
            result = "printf(\"" + StringEscaper.EscapeFormat(lit.Token.ValueText) + nl + "\")";
            return true;
        }

        if (firstArg is InterpolatedStringExpressionSyntax interp)
        {
            result = FormatStringBuilder.BuildPrintf(interp, newline, ctx, writeExpr);
            return true;
        }

        result = "printf(\"%s" + nl + "\", " + writeExpr(firstArg) + ")";
        return true;
    }
}