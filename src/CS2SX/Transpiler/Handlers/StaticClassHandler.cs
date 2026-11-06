// ============================================================================
// Transpiler/Handlers/StaticClassHandler.cs
//
// PHASE 1 FIX: Klarere Trennung von OwnMethodHandler.
// StaticClassHandler greift NUR wenn:
//   - Receiver beginnt mit Großbuchstaben
//   - Receiver ist keine bekannte System-API
//   - Receiver ist keine lokale Variable und kein Feld
//   - Receiver ist NICHT die eigene Klasse (CurrentClass)
// ============================================================================

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

public sealed class StaticClassHandler : InvocationHandlerBase
{
    private static readonly HashSet<string> s_knownApis = new(StringComparer.Ordinal)
    {
        "Graphics", "Input", "Console", "File", "Directory", "Path",
        "System", "Math", "MathF", "Color", "Form", "LibNX",
        "string", "String", "int", "Int32", "float", "Single",
        "double", "Double", "long", "Int64", "uint", "UInt32",
        "ulong", "UInt64", "bool", "Boolean", "byte", "Byte",
        "char", "Char", "short", "Int16", "ushort", "UInt16",
        "Random", "Environment", "Application",
    };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr, out string result)
    {
        if (inv.Expression is not MemberAccessExpressionSyntax mem)
            return NotHandled(out result);

        var receiverName = mem.Expression.ToString();
        var methodName = mem.Name.Identifier.Text;

        // Kein verschachtelter Member-Zugriff hier (wird separat behandelt)
        if (receiverName.Contains('.'))
            return NotHandled(out result);

        // Muss mit Großbuchstaben beginnen
        if (receiverName.Length == 0 || !char.IsUpper(receiverName[0]))
            return NotHandled(out result);

        // Bekannte System-APIs überspringen
        if (s_knownApis.Contains(receiverName))
            return NotHandled(out result);

        // PHASE 1 FIX: Eigene Klasse nicht als static-Aufruf behandeln
        if (receiverName == ctx.CurrentClass)
            return NotHandled(out result);

        // Lokale Variable → kein static-Aufruf
        if (ctx.LocalTypes.ContainsKey(receiverName))
            return NotHandled(out result);

        // Feld → kein static-Aufruf
        var fieldKey = receiverName.TrimStart('_');
        if (ctx.FieldTypes.ContainsKey(fieldKey))
            return NotHandled(out result);

        // PHASE 1 FIX: SemanticModel-Check wenn verfügbar
        if (ctx.SemanticModel != null && inv.Expression is MemberAccessExpressionSyntax memSem)
        {
            try
            {
                var symbolInfo = ctx.SemanticModel.GetSymbolInfo(memSem.Expression);
                var sym = symbolInfo.Symbol;

                // Wenn Receiver ein Typ (static class) ist → static-Aufruf
                if (sym is Microsoft.CodeAnalysis.INamedTypeSymbol)
                {
                    result = receiverName + "_" + methodName + "(" + JoinArgs(args) + ")";
                    return true;
                }

                // Wenn Receiver eine Variable/Feld ist → nicht static
                if (sym is Microsoft.CodeAnalysis.ILocalSymbol
                    or Microsoft.CodeAnalysis.IFieldSymbol
                    or Microsoft.CodeAnalysis.IParameterSymbol)
                    return NotHandled(out result);
            }
            catch { }
        }

        // Fallback: Großbuchstaben-Receiver → static-Aufruf
        result = receiverName + "_" + methodName + "(" + JoinArgs(args) + ")";
        return true;
    }
}