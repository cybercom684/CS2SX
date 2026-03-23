using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Behandelt Aufrufe an Methoden der eigenen Klasse.
/// CalcSum() → ClassName_CalcSum(self)
/// RefreshScores() → ClassName_RefreshScores(self)
///
/// Erweiterung: Keine nötig — generisch für alle eigenen Methoden.
/// </summary>
public sealed class OwnMethodHandler : IInvocationHandler
{
    public bool CanHandle(InvocationExpressionSyntax inv, string calleeStr, TranspilerContext ctx)
    {
        if (string.IsNullOrEmpty(ctx.CurrentClass)) return false;
        if (inv.Expression is not IdentifierNameSyntax id) return false;
        if (calleeStr.Contains('.')) return false;

        var name = id.Identifier.Text;
        // Erste Buchstabe großgeschrieben = Methode (nicht Variable/Funktion)
        return name.Length > 0 && char.IsUpper(name[0]);
    }

    public string Handle(InvocationExpressionSyntax inv, string calleeStr, List<string> args,
        TranspilerContext ctx, Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        var allArgs = new List<string> { "self" };
        allArgs.AddRange(args);
        return ctx.CurrentClass + "_" + calleeStr + "(" + string.Join(", ", allArgs) + ")";
    }
}
