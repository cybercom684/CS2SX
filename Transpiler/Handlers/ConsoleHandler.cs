using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Behandelt Console.Write / Console.WriteLine.
///
/// Erweiterung: Console.Error.Write etc. hier ergänzen.
/// </summary>
public sealed class ConsoleHandler : IInvocationHandler
{
    public bool CanHandle(InvocationExpressionSyntax inv, string calleeStr, TranspilerContext ctx)
        => calleeStr is "Console.Write" or "Console.WriteLine";

    public string Handle(InvocationExpressionSyntax inv, string calleeStr, List<string> args,
        TranspilerContext ctx, Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        var newline = calleeStr == "Console.WriteLine";
        var nl = newline ? "\\n" : "";

        if (args.Count == 0)
            return newline ? "printf(\"\\n\")" : "printf(\"\")";

        var firstArg = inv.ArgumentList.Arguments[0].Expression;

        if (firstArg is LiteralExpressionSyntax lit && lit.Token.IsKind(SyntaxKind.StringLiteralToken))
            return "printf(\"" + StringEscaper.EscapeFormat(lit.Token.ValueText) + nl + "\")";

        if (firstArg is InterpolatedStringExpressionSyntax interp)
            return FormatStringBuilder.BuildPrintf(interp, newline, ctx, writeExpr);

        return "printf(\"%s" + nl + "\", " + writeExpr(firstArg) + ")";
    }
}
