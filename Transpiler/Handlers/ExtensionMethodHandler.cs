// ============================================================================
// CS2SX — Transpiler/Handlers/ExtensionMethodHandler.cs
//
// Behandelt Extension-Methoden-Aufrufe.
//
// C#:
//   static class IntExtensions {
//       public static bool IsEven(this int x) => x % 2 == 0;
//       public static int Clamp(this int x, int min, int max) => Math.Clamp(x, min, max);
//   }
//   n.IsEven()          → IntExtensions_IsEven(n)
//   speed.Clamp(0, 100) → IntExtensions_Clamp(speed, 0, 100)
//
// C#:
//   static class ListExtensions {
//       public static void AddRange<T>(this List<T> list, T[] arr) { ... }
//   }
//   myList.AddRange(items)  → ListExtensions_AddRange(myList, items)
//
// Erkennung:
//   1. SemanticModel wenn verfügbar: exakte Symbol-Auflösung
//   2. Fallback: Typ des Receivers aus LocalTypes/FieldTypes + Extension-Registry
// ============================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

public sealed class ExtensionMethodHandler : InvocationHandlerBase
{
    // Typ → Liste von (ClassName, MethodSyntax)
    private readonly Dictionary<string, List<(string className, MethodDeclarationSyntax method)>> _registry;

    public ExtensionMethodHandler(
        Dictionary<string, List<(string className, MethodDeclarationSyntax method)>>? registry = null)
    {
        _registry = registry ?? new Dictionary<string, List<(string, MethodDeclarationSyntax)>>(
            StringComparer.Ordinal);
    }

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        if (inv.Expression is not MemberAccessExpressionSyntax mem)
            return NotHandled(out result);

        var methodName = mem.Name.Identifier.Text;
        var receiverRaw = mem.Expression.ToString();
        var receiverExpr = writeExpr(mem.Expression);

        // 1. SemanticModel-Prüfung (bevorzugt)
        if (ctx.SemanticModel != null)
        {
            try
            {
                var symbolInfo = ctx.SemanticModel.GetSymbolInfo(inv);
                var symbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();

                if (symbol is IMethodSymbol method && method.IsExtensionMethod)
                {
                    var className = method.ContainingType.Name;
                    var allArgs = new List<string> { receiverExpr };
                    allArgs.AddRange(args);

                    result = $"{className}_{methodName}({string.Join(", ", allArgs)})";
                    return true;
                }
            }
            catch { }
        }

        // 2. Registry-Lookup (Fallback ohne SemanticModel)
        var receiverType = ResolveReceiverType(receiverRaw, ctx);
        if (receiverType == null) return NotHandled(out result);

        // Basis-Typ für generische Typen: List<int> → List
        var baseType = StripGenericSuffix(receiverType);

        if (!_registry.TryGetValue(baseType, out var candidates))
        {
            // Auch primitiven Typ-Namen versuchen
            var cType = TypeRegistry.MapType(receiverType);
            if (!_registry.TryGetValue(cType, out candidates))
                return NotHandled(out result);
        }

        var match = candidates.FirstOrDefault(c =>
            c.method.Identifier.Text == methodName);

        if (match.method == null) return NotHandled(out result);

        var allArgsFallback = new List<string> { receiverExpr };
        allArgsFallback.AddRange(args);

        result = $"{match.className}_{methodName}({string.Join(", ", allArgsFallback)})";
        return true;
    }

    private static string? ResolveReceiverType(string receiverRaw, TranspilerContext ctx)
    {
        var key = receiverRaw.TrimStart('_');
        if (ctx.LocalTypes.TryGetValue(receiverRaw, out var lt)) return lt;
        if (ctx.FieldTypes.TryGetValue(key, out var ft)) return ft;
        return null;
    }

    private static string StripGenericSuffix(string typeName)
    {
        var idx = typeName.IndexOf('<');
        return idx >= 0 ? typeName[..idx] : typeName;
    }
}