using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Orchestriert alle IInvocationHandler in Prioritäts-Reihenfolge.
///
/// PHASE 3: AudioHandler hinzugefügt.
/// PHASE 1 FIX: StaticClassHandler / OwnMethodHandler Konfliktreihenfolge korrigiert.
/// </summary>
public sealed class InvocationDispatcher
{
    private static readonly IReadOnlyList<IInvocationHandler> s_handlers = new List<IInvocationHandler>
    {
        new LibNxHandler(),
        new EnvironmentHandler(),
        new InputHandler(),
        new FormHandler(),
        new ConsoleHandler(),
        new MathHandler(),
        new RandomHandler(),
        new FileHandler(),
        new ParseHandler(),
        new ColorHandler(),
        new StringBuilderHandler(),
        new ListHandler(),
        new DictionaryHandler(),
        new StringMethodHandler(),
        new FieldMethodHandler(),
        new GraphicsHandler(),
        new GraphicsExtHandler(),
        new InputExtHandler(),
        new DirectoryExtHandler(),
        new PathHandler(),
        new SystemExtHandler(),
        new AudioHandler(),       // PHASE 3: Audio-Unterstützung
        new StaticClassHandler(), // PHASE 1 FIX: Vor OwnMethodHandler, aber nach allen API-Handlern
        new OwnMethodHandler(),   // PHASE 1 FIX: Zuletzt — nur für eigene Methoden
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
        // PHASE 1 FIX: out var Deklarationen korrekt behandeln
        if (a.RefKindKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OutKeyword)
            && a.Expression is DeclarationExpressionSyntax declExpr
            && declExpr.Designation is SingleVariableDesignationSyntax singleDesig)
        {
            // out var n → registriere Typ und gib &n zurück
            var typeName = declExpr.Type.ToString().Trim();
            if (typeName == "var")
            {
                // Typ-Inferenz aus dem Aufruf-Kontext ist hier schwierig
                // Standard: int für TryParse-Aufrufe
                typeName = "int";
            }
            _ctx.LocalTypes[singleDesig.Identifier.Text] = typeName;
            return "&" + singleDesig.Identifier.Text;
        }

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