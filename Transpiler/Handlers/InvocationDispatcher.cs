using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using Microsoft.CodeAnalysis;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Orchestriert alle IInvocationHandler in Prioritäts-Reihenfolge.
///
/// Erweiterung: Neuen Handler in s_handlers Liste eintragen.
/// Reihenfolge ist wichtig — spezifischste Handler zuerst.
/// </summary>
public sealed class InvocationDispatcher
{
    // Handler werden in dieser Reihenfolge geprüft.
    // Spezifischste/exklusivste zuerst.
    private static readonly IReadOnlyList<IInvocationHandler> s_handlers = new List<IInvocationHandler>
    {
        new LibNxHandler(),         // LibNX.Services.X.y() — immer eindeutig
        new InputHandler(),         // Input.IsDown/IsHeld/IsUp
        new FormHandler(),          // Form.Add, SwitchApp_RequestExit
        new ConsoleHandler(),       // Console.Write/WriteLine
        new MathHandler(),          // Math.Abs/Min/Max etc.
        new StringBuilderHandler(), // sb.Append/AppendLine/Clear/ToString
        new ListHandler(),          // list.Add/Clear/Remove/Contains
        new DictionaryHandler(),    // dict.Add/Remove/ContainsKey/TryGetValue/Clear
        new StringMethodHandler(),  // string.IsNullOrEmpty, s.Contains etc.
        new FieldMethodHandler(),   // _counter.Increment() etc.
        new OwnMethodHandler(),     // CalcSum(), RefreshScores() etc.
    };

    private readonly TranspilerContext _ctx;
    private readonly Func<Microsoft.CodeAnalysis.SyntaxNode?, string> _writeExpr;

    public InvocationDispatcher(
        TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        _ctx       = ctx;
        _writeExpr = writeExpr;
    }

    /// <summary>
    /// Dispatcht einen Invocation-Ausdruck an den passenden Handler.
    /// Gibt null zurück wenn kein Handler greift (Fallback: direkte C-Funktion).
    /// </summary>
    public string? Dispatch(InvocationExpressionSyntax inv)
    {
        var calleeStr = inv.Expression.ToString();

        // ref/out Argumente → & Prefix
        var args = inv.ArgumentList.Arguments
            .Select(a => BuildArg(a))
            .ToList();

        foreach (var handler in s_handlers)
        {
            if (handler.CanHandle(inv, calleeStr, _ctx))
                return handler.Handle(inv, calleeStr, args, _ctx, _writeExpr);
        }

        return null; // Fallback
    }

    private string BuildArg(ArgumentSyntax a)
    {
        var expr = _writeExpr(a.Expression);

        var isRef = a.RefKindKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.RefKeyword);
        var isOut = a.RefKindKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OutKeyword);

        if (!isRef && !isOut)
            return expr;

        var argName = a.Expression.ToString();

        // char[]-Puffer verfällt zu Pointer — kein &
        if (_ctx.LocalTypes.TryGetValue(argName, out var lt) && lt == "char[]")
            return expr;

        // libnx-Struct → &
        if (_ctx.LocalTypes.TryGetValue(argName, out var lst) && TypeRegistry.IsLibNxStruct(lst))
            return "&" + expr;

        // string-Feld → kein &
        var fieldKey = argName.TrimStart('_');
        if (_ctx.FieldTypes.TryGetValue(fieldKey, out var ft) && ft == "string")
            return expr;

        // out-Parameter für primitive Typen (int, float etc.) → &
        // Der DictionaryHandler darf dann KEIN weiteres & hinzufügen.
        return "&" + expr;
    }
}
