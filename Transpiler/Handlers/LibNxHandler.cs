using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Behandelt LibNX-Namespace-qualifizierte Aufrufe.
/// LibNX.Services.Psm.psmFoo(args) → psmFoo(args)
///
/// Erweiterung: Keine nötig — alle LibNX-Funktionen werden automatisch erkannt.
/// </summary>
public sealed class LibNxHandler : IInvocationHandler
{
    public bool CanHandle(InvocationExpressionSyntax inv, string calleeStr, TranspilerContext ctx)
        => calleeStr.StartsWith("LibNX.", StringComparison.Ordinal);

    public string Handle(InvocationExpressionSyntax inv, string calleeStr, List<string> args,
        TranspilerContext ctx, Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        // LibNX.Services.Psm.psmFoo → letzter Bezeichner = Funktionsname
        var mem = (MemberAccessExpressionSyntax)inv.Expression;
        return mem.Name.Identifier.Text + "(" + string.Join(", ", args) + ")";
    }
}
