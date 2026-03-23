using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Behandelt Math-Methoden.
///
/// Erweiterung: Neuen Eintrag in s_mathMap ergänzen.
/// </summary>
public sealed class MathHandler : IInvocationHandler
{
    private static readonly Dictionary<string, string> s_mathMap = new(StringComparer.Ordinal)
    {
        ["Math.Abs"]   = "abs",
        ["Math.Min"]   = "MIN",
        ["Math.Max"]   = "MAX",
        ["Math.Sqrt"]  = "sqrtf",
        ["Math.Floor"] = "floorf",
        ["Math.Ceil"]  = "ceilf",
        ["Math.Pow"]   = "powf",
        ["Math.Sin"]   = "sinf",
        ["Math.Cos"]   = "cosf",
        ["Math.Clamp"] = "CLAMP",
    };

    public bool CanHandle(InvocationExpressionSyntax inv, string calleeStr, TranspilerContext ctx)
        => s_mathMap.ContainsKey(calleeStr);

    public string Handle(InvocationExpressionSyntax inv, string calleeStr, List<string> args,
        TranspilerContext ctx, Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
        => s_mathMap[calleeStr] + "(" + string.Join(", ", args) + ")";
}
