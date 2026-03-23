using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Behandelt Methoden-Aufrufe auf Felder der eigenen Klasse.
/// _counter.Increment() → Counter_Increment(self->f_counter)
///
/// Erweiterung: Keine nötig — generisch für alle Feld-Typen.
/// </summary>
public sealed class FieldMethodHandler : IInvocationHandler
{
    public bool CanHandle(InvocationExpressionSyntax inv, string calleeStr, TranspilerContext ctx)
    {
        if (inv.Expression is not MemberAccessExpressionSyntax mem) return false;

        var objStr = mem.Expression.ToString();
        if (!objStr.StartsWith('_')) return false;

        var fieldKey = objStr.TrimStart('_');
        return ctx.FieldTypes.ContainsKey(fieldKey);
    }

    public string Handle(InvocationExpressionSyntax inv, string calleeStr, List<string> args,
        TranspilerContext ctx, Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        var mem        = (MemberAccessExpressionSyntax)inv.Expression;
        var objStr     = mem.Expression.ToString();
        var fieldKey   = objStr.TrimStart('_');
        var methodName = mem.Name.Identifier.Text;

        ctx.FieldTypes.TryGetValue(fieldKey, out var fieldType);

        var allArgs = new List<string> { "self->f_" + fieldKey };
        allArgs.AddRange(args);
        return fieldType + "_" + methodName + "(" + string.Join(", ", allArgs) + ")";
    }
}
