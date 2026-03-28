using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Orchestriert alle IInvocationHandler in Prioritäts-Reihenfolge.
///
/// Erweiterung: Neuen Handler in s_handlers Liste eintragen.
/// Reihenfolge ist wichtig — spezifischste Handler zuerst.
/// </summary>
public sealed class InvocationDispatcher
{
    private static readonly IReadOnlyList<IInvocationHandler> s_handlers = new List<IInvocationHandler>
    {
        new LibNxHandler(),
        new InputHandler(),
        new FormHandler(),
        new ConsoleHandler(),
        new MathHandler(),
        new FileHandler(),
        new ParseHandler(),
        new StringBuilderHandler(),
        new ListHandler(),
        new DictionaryHandler(),
        new StringMethodHandler(),
        new FieldMethodHandler(),
        new GraphicsHandler(),
        new OwnMethodHandler(),
    };

    private readonly TranspilerContext _ctx;
    private readonly Func<Microsoft.CodeAnalysis.SyntaxNode?, string> _writeExpr;

    public InvocationDispatcher(
        TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        _ctx = ctx;
        _writeExpr = writeExpr;
    }

    /// <summary>
    /// Dispatcht einen Invocation-Ausdruck an den passenden Handler.
    /// Gibt null zurück wenn kein Handler greift (Fallback: direkte C-Funktion).
    /// </summary>
    public string? Dispatch(InvocationExpressionSyntax inv)
    {
        var calleeStr = inv.Expression.ToString();

        var args = inv.ArgumentList.Arguments
            .Select(a => BuildArg(a))
            .ToList();

        foreach (var handler in s_handlers)
        {
            if (handler.TryHandle(inv, calleeStr, args, _ctx, _writeExpr, out var result))
                return result;
        }

        return null;
    }

    private string BuildArg(ArgumentSyntax a)
    {
        var expr = _writeExpr(a.Expression);
        var isRef = a.RefKindKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.RefKeyword);
        var isOut = a.RefKindKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OutKeyword);

        if (!isRef && !isOut) return expr;

        var argName = a.Expression.ToString();

        if (_ctx.LocalTypes.TryGetValue(argName, out var lt) && lt == "char[]")
            return expr;

        if (_ctx.LocalTypes.TryGetValue(argName, out var lst) && TypeRegistry.IsLibNxStruct(lst))
            return "&" + expr;

        var fieldKey = argName.TrimStart('_');
        if (_ctx.FieldTypes.TryGetValue(fieldKey, out var ft) && ft == "string")
            return expr;

        return "&" + expr;
    }
}