// ============================================================================
// CS2SX — Transpiler/GenericInstantiationCollector.cs  (FIXED)
//
// Fixes:
//   1. CName für Multi-Typ-Parameter korrekt: Pair<string,float> → Pair_str_float
//      (vorher: Pair_str_float war schon korrekt, aber verschachtelte Generics
//       wie Stack<List<int>> → Stack_List_int_ptr wurden nicht flach gemacht)
//   2. MapToCSuffix kennt jetzt alle primitiven Typen aus TypeRegistry
//   3. Deduplizierung von CName-Kollisionen (zwei verschiedene Instantiierungen
//      die zufällig denselben CName erzeugen würden → Warnung + Skip)
// ============================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using CS2SX.Logging;

namespace CS2SX.Transpiler;

/// <summary>
/// Repräsentiert eine konkrete Instantiierung eines generischen Typs.
/// Stack&lt;int&gt;  →  GenericInstantiation("Stack", ["int"])
/// </summary>
public sealed record GenericInstantiation(
    string BaseName,
    IReadOnlyList<string> TypeArguments)
{
    /// <summary>
    /// Eindeutiger C-Bezeichner: Stack_int, Pair_str_float, Map_str_List_int
    ///
    /// FIX: Verschachtelte Generics werden flach gemacht:
    ///   Stack<List<int>> → Stack_List_int  (nicht Stack_List_int_ptr)
    ///   Pair<string,float> → Pair_str_float
    /// </summary>
    public string CName => BaseName + "_" + string.Join("_", TypeArguments.Select(MapToCSuffix));

    public override string ToString() =>
        BaseName + "<" + string.Join(", ", TypeArguments) + ">";

    /// <summary>
    /// Mappt einen C#-Typ auf einen C-Namens-Suffix.
    /// Verschachtelte Generics werden rekursiv aufgelöst.
    /// </summary>
    internal static string MapToCSuffix(string csType)
    {
        csType = csType.Trim();

        // Direkte Primitive
        var primitive = csType switch
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
            "object" => "obj",
            "void" => "void",
            "u8" => "u8",
            "u16" => "u16",
            "u32" => "u32",
            "u64" => "u64",
            "s8" => "s8",
            "s16" => "s16",
            "s32" => "s32",
            "s64" => "s64",
            _ => null,
        };
        if (primitive != null) return primitive;

        // Array: T[] → T_arr
        if (csType.EndsWith("[]"))
            return MapToCSuffix(csType[..^2]) + "_arr";

        // Nullable: T? → T
        if (csType.EndsWith("?"))
            return MapToCSuffix(csType[..^1]);

        // Generischer Typ: List<T> → List_T, Pair<K,V> → Pair_K_V
        var angleIdx = csType.IndexOf('<');
        if (angleIdx > 0 && csType.EndsWith(">"))
        {
            var outerName = csType[..angleIdx];
            var innerPart = csType[(angleIdx + 1)..^1];
            var innerArgs = SplitTypeArgs(innerPart);
            var suffix = string.Join("_", innerArgs.Select(MapToCSuffix));
            return outerName + "_" + suffix;
        }

        // Unbekannter Typ → direkt verwenden (bereinigt)
        return csType.Replace(".", "_").Replace(" ", "");
    }

    /// <summary>
    /// Teilt Typ-Argumente korrekt auf — respektiert verschachtelte &lt;&gt;.
    /// "string, float" → ["string", "float"]
    /// "int, List&lt;string&gt;" → ["int", "List&lt;string&gt;"]
    /// </summary>
    private static List<string> SplitTypeArgs(string s)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        int depth = 0;

        foreach (char c in s)
        {
            if (c == '<') { depth++; current.Append(c); }
            else if (c == '>') { depth--; current.Append(c); }
            else if (c == ',' && depth == 0)
            {
                result.Add(current.ToString().Trim());
                current.Clear();
            }
            else current.Append(c);
        }

        if (current.Length > 0)
            result.Add(current.ToString().Trim());

        return result;
    }
}

/// <summary>
/// Sammelt alle konkreten Generic-Instantiierungen aus einer Menge von Quelldateien.
/// </summary>
public sealed class GenericInstantiationCollector
{
    private readonly Dictionary<string, HashSet<string>> _seen =
        new(StringComparer.Ordinal);

    // FIX: CName-Deduplizierung — verhindert Kollisionen
    private readonly HashSet<string> _usedCNames =
        new(StringComparer.Ordinal);

    private readonly List<GenericInstantiation> _instantiations = new();

    public Dictionary<string, ClassDeclarationSyntax> GenericClasses
    {
        get;
    } =
        new(StringComparer.Ordinal);

    public Dictionary<string, MethodDeclarationSyntax> GenericMethods
    {
        get;
    } =
        new(StringComparer.Ordinal);

    public Dictionary<string, InterfaceDeclarationSyntax> Interfaces
    {
        get;
    } =
        new(StringComparer.Ordinal);

    public Dictionary<string, List<(string className, MethodDeclarationSyntax method)>> ExtensionMethods
    {
        get;
    } =
        new(StringComparer.Ordinal);

    public IReadOnlyList<GenericInstantiation> Instantiations => _instantiations;

    // ── Öffentliche API ───────────────────────────────────────────────────────

    public void Collect(IReadOnlyList<string> sourceFiles, IReadOnlyList<string>? stubFiles = null)
    {
        var allFiles = sourceFiles.ToList();
        if (stubFiles != null) allFiles.AddRange(stubFiles);

        // Pass 1: Definitionen sammeln
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

        // Pass 2: Instantiierungen (nur Projektdateien)
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

    // ── Pass 1: Definitionen ──────────────────────────────────────────────────

    private void CollectDefinitions(SyntaxTree tree, string filePath)
    {
        var root = tree.GetRoot();

        foreach (var cls in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            if (cls.TypeParameterList?.Parameters.Count > 0)
            {
                var name = cls.Identifier.Text;
                GenericClasses[name] = cls;
                Log.Debug($"GenericCollector: Generische Klasse '{name}' in {Path.GetFileName(filePath)}");
            }

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
                    var baseExtType = StripGenericSuffix(extendedType);

                    if (!ExtensionMethods.TryGetValue(baseExtType, out var list))
                    {
                        list = new List<(string, MethodDeclarationSyntax)>();
                        ExtensionMethods[baseExtType] = list;
                    }
                    list.Add((cls.Identifier.Text, method));

                    Log.Debug($"GenericCollector: Extension '{cls.Identifier.Text}.{method.Identifier.Text}' für '{baseExtType}'");
                }
            }

            foreach (var method in cls.Members.OfType<MethodDeclarationSyntax>())
            {
                if (method.TypeParameterList?.Parameters.Count > 0)
                {
                    var key = cls.Identifier.Text + "." + method.Identifier.Text;
                    GenericMethods[key] = method;
                }
            }
        }

        foreach (var iface in root.DescendantNodes().OfType<InterfaceDeclarationSyntax>())
        {
            Interfaces[iface.Identifier.Text] = iface;
            Log.Debug($"GenericCollector: Interface '{iface.Identifier.Text}' gefunden");
        }
    }

    // ── Pass 2: Instantiierungen ──────────────────────────────────────────────

    private void CollectInstantiations(SyntaxTree tree)
    {
        var root = tree.GetRoot();

        foreach (var obj in root.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
            if (obj.Type is GenericNameSyntax gn) TryRegister(gn);

        foreach (var local in root.DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
            if (local.Declaration.Type is GenericNameSyntax gn) TryRegister(gn);

        foreach (var field in root.DescendantNodes().OfType<FieldDeclarationSyntax>())
            if (field.Declaration.Type is GenericNameSyntax gn) TryRegister(gn);

        foreach (var prop in root.DescendantNodes().OfType<PropertyDeclarationSyntax>())
            if (prop.Type is GenericNameSyntax gn) TryRegister(gn);

        foreach (var param in root.DescendantNodes().OfType<ParameterSyntax>())
            if (param.Type is GenericNameSyntax gn) TryRegister(gn);

        foreach (var method in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            if (method.ReturnType is GenericNameSyntax gn) TryRegister(gn);

        foreach (var inv in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (inv.Expression is MemberAccessExpressionSyntax mem
                && mem.Name is GenericNameSyntax gm)
                TryRegister(gm);
            else if (inv.Expression is GenericNameSyntax gn2)
                TryRegister(gn2);
        }

        foreach (var cast in root.DescendantNodes().OfType<CastExpressionSyntax>())
            if (cast.Type is GenericNameSyntax gn) TryRegister(gn);
    }

    // ── Registrierung ──────────────────────────────────────────────────────────

    private void TryRegister(GenericNameSyntax genericName)
    {
        var baseName = genericName.Identifier.Text;
        if (!GenericClasses.ContainsKey(baseName)) return;

        var typeArgs = genericName.TypeArgumentList.Arguments
            .Select(a => NormalizeTypeName(a.ToString().Trim()))
            .ToList();

        if (typeArgs.Count == 0) return;

        // Verschachtelte Generics rekursiv registrieren
        foreach (var arg in genericName.TypeArgumentList.Arguments)
            if (arg is GenericNameSyntax nested) TryRegister(nested);

        RegisterInstantiation(baseName, typeArgs);
    }

    private void RegisterInstantiation(string baseName, List<string> typeArgs)
    {
        if (!_seen.TryGetValue(baseName, out var seenSet))
        {
            seenSet = new HashSet<string>(StringComparer.Ordinal);
            _seen[baseName] = seenSet;
        }

        var key = string.Join(",", typeArgs);
        if (!seenSet.Add(key)) return;

        var inst = new GenericInstantiation(baseName, typeArgs);

        // FIX: CName-Kollision prüfen
        if (!_usedCNames.Add(inst.CName))
        {
            Log.Warning($"GenericCollector: CName-Kollision für '{inst.CName}' — Instantiierung '{inst}' wird übersprungen");
            return;
        }

        _instantiations.Add(inst);
        Log.Debug($"GenericCollector: Neue Instantiierung: {inst} → {inst.CName}");
    }

    // ── Hilfsmethoden ──────────────────────────────────────────────────────────

    private static string NormalizeTypeName(string csType)
    {
        if (csType.EndsWith("?")) return csType[..^1];
        return csType;
    }

    private static string StripGenericSuffix(string typeName)
    {
        var idx = typeName.IndexOf('<');
        return idx >= 0 ? typeName[..idx] : typeName;
    }

    public IEnumerable<GenericInstantiation> GetInstantiations(string baseName) =>
        _instantiations.Where(i => i.BaseName == baseName);

    public bool IsInstantiated(string baseName) =>
        _seen.ContainsKey(baseName);
}