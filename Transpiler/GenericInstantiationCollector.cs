// ============================================================================
// CS2SX — Transpiler/GenericInstantiationCollector.cs
//
// Pass 1 vor dem eigentlichen Transpiler:
// Sammelt alle konkreten Instantiierungen generischer Typen und Methoden.
//
// Beispiele die erkannt werden:
//   new Stack<int>()              → Stack<int>
//   new Pair<string, float>()     → Pair<string, float>
//   Stack<int> x = ...            → Stack<int>
//   void Foo<T>(T x) { }          → Foo<int>, Foo<string> (aus Aufruf-Stellen)
//   class Container<T> { }        → Container<int>, Container<Player> (aus new)
//
// Ergebnis: Dictionary<genericTypeName, HashSet<List<string>>>
//   "Stack" → { ["int"], ["string"] }
//   "Pair"  → { ["string","float"], ["int","int"] }
// ============================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using CS2SX.Logging;

namespace CS2SX.Transpiler;

/// <summary>
/// Repräsentiert eine konkrete Instantiierung eines generischen Typs oder einer Methode.
/// Stack&lt;int&gt;  →  GenericInstantiation("Stack", ["int"])
/// </summary>
public sealed record GenericInstantiation(
    string BaseName,
    IReadOnlyList<string> TypeArguments)
{
    /// <summary>Eindeutiger C-Bezeichner: Stack_int, Pair_str_float</summary>
    public string CName => BaseName + "_" + string.Join("_", TypeArguments.Select(MapToCSuffix));

    /// <summary>Lesbare Darstellung für Debug-Output</summary>
    public override string ToString() =>
        BaseName + "<" + string.Join(", ", TypeArguments) + ">";

    private static string MapToCSuffix(string csType) => csType switch
    {
        "string" => "str",
        "int" => "int",
        "uint" => "uint",
        "long" => "long",
        "ulong" => "ulong",
        "short" => "short",
        "ushort" => "ushort",
        "byte" => "byte",
        "sbyte" => "sbyte",
        "float" => "float",
        "double" => "double",
        "bool" => "bool",
        "char" => "char",
        _ => csType,
    };
}

/// <summary>
/// Sammelt alle konkreten Generic-Instantiierungen aus einer Menge von Quelldateien.
/// Wird vor dem eigentlichen Transpiler-Pass ausgeführt.
/// </summary>
public sealed class GenericInstantiationCollector
{
    // BaseName → Liste aller gesehenen Type-Argument-Kombinationen
    private readonly Dictionary<string, HashSet<string>> _seen =
        new(StringComparer.Ordinal);

    // Alle eindeutigen Instantiierungen
    private readonly List<GenericInstantiation> _instantiations = new();

    // Generic-Klassen die im Projekt definiert sind (Name → Syntax)
    public Dictionary<string, ClassDeclarationSyntax> GenericClasses
    {
        get;
    } =
        new(StringComparer.Ordinal);

    // Generic-Methoden (ClassName.MethodName → Syntax)
    public Dictionary<string, MethodDeclarationSyntax> GenericMethods
    {
        get;
    } =
        new(StringComparer.Ordinal);

    // Interface-Definitionen
    public Dictionary<string, InterfaceDeclarationSyntax> Interfaces
    {
        get;
    } =
        new(StringComparer.Ordinal);

    // Extension-Methoden (erweiterter Typ → Liste von Methoden)
    public Dictionary<string, List<(string className, MethodDeclarationSyntax method)>> ExtensionMethods
    {
        get;
    } =
        new(StringComparer.Ordinal);

    public IReadOnlyList<GenericInstantiation> Instantiations => _instantiations;

    // ── Öffentliche API ───────────────────────────────────────────────────────

    /// <summary>
    /// Analysiert alle Quelldateien und sammelt:
    /// - Definitionen generischer Klassen/Methoden
    /// - Interface-Definitionen
    /// - Extension-Methoden
    /// - Alle konkreten Instantiierungen
    /// </summary>
    public void Collect(IReadOnlyList<string> sourceFiles, IReadOnlyList<string>? stubFiles = null)
    {
        var allFiles = sourceFiles.ToList();
        if (stubFiles != null) allFiles.AddRange(stubFiles);

        // Pass 1: Definitionen sammeln (generische Klassen, Interfaces, Extensions)
        foreach (var file in allFiles)
        {
            try
            {
                var source = File.ReadAllText(file);
                var tree = CSharpSyntaxTree.ParseText(source,
                    new CSharpParseOptions(LanguageVersion.CSharp12),
                    path: file);
                CollectDefinitions(tree, file);
            }
            catch (Exception ex)
            {
                Log.Warning($"GenericCollector: Datei nicht lesbar: {file} ({ex.Message})");
            }
        }

        // Pass 2: Instantiierungen sammeln (nur Projektdateien, nicht Stubs)
        foreach (var file in sourceFiles)
        {
            try
            {
                var source = File.ReadAllText(file);
                var tree = CSharpSyntaxTree.ParseText(source,
                    new CSharpParseOptions(LanguageVersion.CSharp12),
                    path: file);
                CollectInstantiations(tree);
            }
            catch (Exception ex)
            {
                Log.Warning($"GenericCollector: Instantiierung-Scan fehlgeschlagen: {file} ({ex.Message})");
            }
        }

        Log.Info($"GenericCollector: {GenericClasses.Count} generische Klasse(n), "
               + $"{Interfaces.Count} Interface(s), "
               + $"{_instantiations.Count} Instantiierung(en), "
               + $"{ExtensionMethods.Values.SelectMany(v => v).Count()} Extension-Methode(n)");
    }

    // ── Pass 1: Definitionen sammeln ──────────────────────────────────────────

    private void CollectDefinitions(SyntaxTree tree, string filePath)
    {
        var root = tree.GetRoot();

        // Generische Klassen
        foreach (var cls in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            if (cls.TypeParameterList?.Parameters.Count > 0)
            {
                var name = cls.Identifier.Text;
                GenericClasses[name] = cls;
                Log.Debug($"GenericCollector: Generische Klasse '{name}' gefunden in {Path.GetFileName(filePath)}");
            }

            // Extension-Methoden erkennen (static class mit static-Methoden die this-Parameter haben)
            bool isStaticClass = cls.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
            if (isStaticClass)
            {
                foreach (var method in cls.Members.OfType<MethodDeclarationSyntax>())
                {
                    bool isStatic = method.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
                    if (!isStatic) continue;

                    var firstParam = method.ParameterList.Parameters.FirstOrDefault();
                    if (firstParam == null) continue;

                    bool hasThis = firstParam.Modifiers.Any(m => m.IsKind(SyntaxKind.ThisKeyword));
                    if (!hasThis) continue;

                    var extendedType = firstParam.Type?.ToString().Trim() ?? "";
                    // Generics aus dem Typ entfernen: List<T> → List
                    var baseExtType = StripGenericSuffix(extendedType);

                    if (!ExtensionMethods.TryGetValue(baseExtType, out var list))
                    {
                        list = new List<(string, MethodDeclarationSyntax)>();
                        ExtensionMethods[baseExtType] = list;
                    }
                    list.Add((cls.Identifier.Text, method));

                    Log.Debug($"GenericCollector: Extension-Methode '{cls.Identifier.Text}.{method.Identifier.Text}' für '{baseExtType}'");
                }
            }

            // Generische Methoden in nicht-generischen Klassen
            foreach (var method in cls.Members.OfType<MethodDeclarationSyntax>())
            {
                if (method.TypeParameterList?.Parameters.Count > 0)
                {
                    var key = cls.Identifier.Text + "." + method.Identifier.Text;
                    GenericMethods[key] = method;
                }
            }
        }

        // Interfaces
        foreach (var iface in root.DescendantNodes().OfType<InterfaceDeclarationSyntax>())
        {
            Interfaces[iface.Identifier.Text] = iface;
            Log.Debug($"GenericCollector: Interface '{iface.Identifier.Text}' gefunden");
        }
    }

    // ── Pass 2: Instantiierungen sammeln ──────────────────────────────────────

    private void CollectInstantiations(SyntaxTree tree)
    {
        var root = tree.GetRoot();

        // new Foo<T>() → Instantiierung
        foreach (var obj in root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
        {
            if (obj.Type is GenericNameSyntax genericName)
                TryRegister(genericName);
        }

        // Foo<T> als Variablen-Typ: Foo<int> x = ...
        foreach (var local in root.DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
        {
            if (local.Declaration.Type is GenericNameSyntax gn)
                TryRegister(gn);
        }

        // Felder: private Foo<T> _field;
        foreach (var field in root.DescendantNodes().OfType<FieldDeclarationSyntax>())
        {
            if (field.Declaration.Type is GenericNameSyntax gn)
                TryRegister(gn);
        }

        // Properties: public Foo<T> Bar { get; set; }
        foreach (var prop in root.DescendantNodes().OfType<PropertyDeclarationSyntax>())
        {
            if (prop.Type is GenericNameSyntax gn)
                TryRegister(gn);
        }

        // Parameter: void Foo(Bar<T> x)
        foreach (var param in root.DescendantNodes().OfType<ParameterSyntax>())
        {
            if (param.Type is GenericNameSyntax gn)
                TryRegister(gn);
        }

        // Rückgabetypen: Bar<T> Foo() { }
        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            if (method.ReturnType is GenericNameSyntax gn)
                TryRegister(gn);
        }

        // Generische Methodenaufrufe: obj.Sort<int>()
        foreach (var inv in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (inv.Expression is MemberAccessExpressionSyntax mem
                && mem.Name is GenericNameSyntax genericMethod)
            {
                TryRegister(genericMethod);
            }
            else if (inv.Expression is GenericNameSyntax gn2)
            {
                TryRegister(gn2);
            }
        }

        // Cast-Ausdrücke: (Foo<int>)x
        foreach (var cast in root.DescendantNodes().OfType<CastExpressionSyntax>())
        {
            if (cast.Type is GenericNameSyntax gn)
                TryRegister(gn);
        }
    }

    // ── Registrierung ──────────────────────────────────────────────────────────

    private void TryRegister(GenericNameSyntax genericName)
    {
        var baseName = genericName.Identifier.Text;

        // Nur Typen registrieren die wir als generische Klassen kennen
        // (nicht List<T>, Dictionary<K,V> etc. — die werden separat behandelt)
        if (!GenericClasses.ContainsKey(baseName)) return;

        var typeArgs = genericName.TypeArgumentList.Arguments
            .Select(a => NormalizeTypeName(a.ToString().Trim()))
            .ToList();

        if (typeArgs.Count == 0) return;

        // Verschachtelte Generics rekursiv registrieren
        foreach (var arg in genericName.TypeArgumentList.Arguments)
        {
            if (arg is GenericNameSyntax nestedGeneric)
                TryRegister(nestedGeneric);
        }

        RegisterInstantiation(baseName, typeArgs);
    }

    private void RegisterInstantiation(string baseName, List<string> typeArgs)
    {
        // Deduplizierung
        if (!_seen.TryGetValue(baseName, out var seenSet))
        {
            seenSet = new HashSet<string>(StringComparer.Ordinal);
            _seen[baseName] = seenSet;
        }

        var key = string.Join(",", typeArgs);
        if (!seenSet.Add(key)) return; // Bereits registriert

        var inst = new GenericInstantiation(baseName, typeArgs);
        _instantiations.Add(inst);
        Log.Debug($"GenericCollector: Neue Instantiierung: {inst}");
    }

    // ── Hilfsmethoden ──────────────────────────────────────────────────────────

    private static string NormalizeTypeName(string csType)
    {
        // Nullable: int? → int (für C-Namen)
        if (csType.EndsWith("?")) return csType[..^1];
        return csType;
    }

    private static string StripGenericSuffix(string typeName)
    {
        var idx = typeName.IndexOf('<');
        return idx >= 0 ? typeName[..idx] : typeName;
    }

    /// <summary>
    /// Gibt alle Instantiierungen einer bestimmten Basis-Klasse zurück.
    /// </summary>
    public IEnumerable<GenericInstantiation> GetInstantiations(string baseName) =>
        _instantiations.Where(i => i.BaseName == baseName);

    /// <summary>
    /// True wenn eine Klasse generisch ist und mindestens eine Instantiierung hat.
    /// </summary>
    public bool IsInstantiated(string baseName) =>
        _seen.ContainsKey(baseName);
}