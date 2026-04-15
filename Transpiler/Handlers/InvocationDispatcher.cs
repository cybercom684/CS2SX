// ============================================================================
// CS2SX — Transpiler/Handlers/InvocationDispatcher.cs
//
// Änderungen gegenüber Original:
//   • ExtensionMethodHandler hinzugefügt (nach AudioHandler)
//   • Handler-Registry ist jetzt instanz-basiert (nicht mehr static readonly)
//     damit ExtensionMethodHandler mit der Registry initialisiert werden kann
//   • Zweiter Konstruktor: InvocationDispatcher(ctx, writeExpr, extensionHandler)
// ============================================================================

using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Orchestriert alle IInvocationHandler in Prioritäts-Reihenfolge.
/// </summary>
public sealed class InvocationDispatcher
{
    private readonly IReadOnlyList<IInvocationHandler> _handlers;
    private readonly TranspilerContext _ctx;
    private readonly Func<SyntaxNode?, string> _writeExpr;

    private static List<IInvocationHandler> BuildHandlers(
        ExtensionMethodHandler? extensionHandler)
    {
        return new List<IInvocationHandler>
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
            new AudioHandler(),
            extensionHandler ?? new ExtensionMethodHandler(), // NEU: Extension-Methods
            new StaticClassHandler(),
            new OwnMethodHandler(),
        };
    }

    /// <summary>
    /// Standard-Konstruktor (Rückwärtskompatibilität, keine Extension-Methods).
    /// </summary>
    public InvocationDispatcher(
        TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr)
    {
        _ctx = ctx;
        _writeExpr = writeExpr;
        _handlers = BuildHandlers(null);
    }

    /// <summary>
    /// Konstruktor mit Extension-Method-Registry.
    /// Wird von CSharpToC verwendet wenn ein GenericInstantiationCollector vorhanden ist.
    /// </summary>
    public InvocationDispatcher(
        TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr,
        ExtensionMethodHandler extensionHandler)
    {
        _ctx = ctx;
        _writeExpr = writeExpr;
        _handlers = BuildHandlers(extensionHandler);
    }

    public string? Dispatch(InvocationExpressionSyntax inv)
    {
        var calleeStr = inv.Expression.ToString();

        var args = inv.ArgumentList.Arguments
            .Select(a => BuildArg(a))
            .ToList();

        foreach (var handler in _handlers)
        {
            if (handler.TryHandle(inv, calleeStr, args, _ctx, _writeExpr, out var result))
                return result;
        }

        return null;
    }

    private string BuildArg(ArgumentSyntax a)
    {
        // PHASE 1 FIX: out var in Argument-Position (z.B. int.TryParse(s, out var n))
        if (a.RefKindKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OutKeyword)
            && a.Expression is DeclarationExpressionSyntax declExpr
            && declExpr.Designation is SingleVariableDesignationSyntax singleDesig)
        {
            var typeName = declExpr.Type.ToString().Trim();
            if (typeName == "var") typeName = "int";
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

        if (_ctx.LocalTypes.TryGetValue(argName, out var lst)
            && TypeRegistry.IsLibNxStruct(lst))
            return "&" + expr;

        var fieldKey = argName.TrimStart('_');
        if (_ctx.FieldTypes.TryGetValue(fieldKey, out var ft) && ft == "string")
            return expr;

        return "&" + expr;
    }
}