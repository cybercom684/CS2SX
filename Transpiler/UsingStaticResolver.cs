// ============================================================================
// CS2SX — Transpiler/UsingStaticResolver.cs  (NEU)
//
// Löst "using static"-Importe auf.
// using static System.Math;  →  Sqrt → sqrtf, Abs → abs etc.
// using static MyClass;      →  MyMethod() → MyClass_MyMethod(self)
// ============================================================================

using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler;

public sealed class UsingStaticResolver
{
    // Bekannte statische System-Klassen deren Methoden direkt gemappt werden
    private static readonly Dictionary<string, string> s_systemClassMaps =
        new(StringComparer.Ordinal)
        {
            ["System.Math"] = "Math",
            ["System.MathF"] = "MathF",
            ["System.Console"] = "Console",
            ["System.String"] = "String",
            ["System.Convert"] = "Convert",
        };

    // Gesammelte "using static" Imports des aktuellen Files
    private readonly HashSet<string> _usingStaticTypes =
        new(StringComparer.Ordinal);

    public IReadOnlyCollection<string> UsingStaticTypes => _usingStaticTypes;

    /// <summary>
    /// Liest alle "using static" Direktiven aus dem Syntax-Baum.
    /// </summary>
    public void Collect(Microsoft.CodeAnalysis.SyntaxNode root)
    {
        _usingStaticTypes.Clear();

        foreach (var us in root.DescendantNodes()
            .OfType<UsingDirectiveSyntax>()
            .Where(u => u.StaticKeyword.IsKind(
                Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword)))
        {
            var typeName = us.Name?.ToString() ?? "";
            if (!string.IsNullOrEmpty(typeName))
                _usingStaticTypes.Add(typeName);
        }
    }

    /// <summary>
    /// Gibt den qualifizierten Klassen-Prefix zurück wenn der Methoden-Name
    /// aus einem "using static" Import stammt.
    ///
    /// Beispiel: "using static System.Math" + "Sqrt" → "Math"
    /// </summary>
    public string? TryResolveStaticMethod(string methodName)
    {
        foreach (var imported in _usingStaticTypes)
        {
            // Normierter short-Name: "System.Math" → "Math"
            var shortName = imported.Contains('.')
                ? imported[(imported.LastIndexOf('.') + 1)..]
                : imported;

            // Für System-Klassen prüfen ob der Methodenname bekannt ist
            if (s_systemClassMaps.TryGetValue(imported, out var mapped))
                return mapped; // Wird von den bestehenden Handlern aufgelöst

            return shortName;
        }
        return null;
    }
}