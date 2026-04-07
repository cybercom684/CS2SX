using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Behandelt Environment.Exit(), Application.Exit() und ähnliche Exit-Aufrufe.
///
/// Environment.Exit(0)      → Environment_Exit(0)
/// Environment.Exit(1)      → Environment_Exit(1)
/// Application.Exit()       → Environment_Exit(0)
/// </summary>
public sealed class EnvironmentHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        switch (calleeStr)
        {
            case "Environment.Exit":
                result = "Environment_Exit(" + ArgAt(args, 0) + ")";
                return true;

            case "Application.Exit":
            case "Application.Quit":
                result = "Environment_Exit(0)";
                return true;

            case "Environment.FailFast":
                result = "Environment_Exit(1)";
                return true;

            // Console.Clear() → consoleClear() in libnx
            case "Console.Clear":
                result = "consoleClear()";
                return true;

            // Console.SetCursorPosition → ANSI escape
            case "Console.SetCursorPosition":
                if (args.Count >= 2)
                    result = "printf(\"\\033[%d;%dH\", " + args[1] + ", " + args[0] + ")";
                else
                    result = "printf(\"\\033[H\")";
                return true;
        }

        return NotHandled(out result);
    }
}