// ============================================================================
// CS2SX — Build/SemanticModelBuilder.cs  (FIXED)
//
// Fixes:
//   • GetModelForBaseName() neu: findet SemanticModel anhand des Klassennamens
//     (für GenericExpander der synthetische Dateipfade <generic:Foo_int> nutzt)
//   • GetModel() toleriert nun auch synthetische Pfade (<generic:...>)
// ============================================================================

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using CS2SX.Logging;
using System.Reflection;

namespace CS2SX.Transpiler;

public sealed class SemanticModelBuilder
{
    private readonly Dictionary<string, SemanticModel> _models =
        new(StringComparer.OrdinalIgnoreCase);

    // Zusätzliche Suche nach Klassenname (für GenericExpander)
    private readonly Dictionary<string, SemanticModel> _modelsByClass =
        new(StringComparer.Ordinal);

    private readonly CSharpCompilation _compilation;

    // ── Basis-Referenzen ──────────────────────────────────────────────────────

    private static readonly MetadataReference[] s_baseRefs = BuildBaseRefs();

    private static MetadataReference[] BuildBaseRefs()
    {
        var refs = new List<MetadataReference>();

        var trustedAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (trustedAssemblies != null)
        {
            foreach (var path in trustedAssemblies.Split(Path.PathSeparator))
            {
                var name = Path.GetFileNameWithoutExtension(path);
                if (name is "System.Runtime"
                         or "System.Private.CoreLib"
                         or "System.Collections"
                         or "System.Console"
                         or "netstandard")
                {
                    try { refs.Add(MetadataReference.CreateFromFile(path)); }
                    catch { }
                }
            }
        }

        if (refs.Count == 0)
        {
            try { refs.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location)); }
            catch { }
        }

        return refs.ToArray();
    }

    // ── Konstruktor ───────────────────────────────────────────────────────────

    public SemanticModelBuilder(IReadOnlyList<string> sourceFiles)
    {
        var parseOptions = new CSharpParseOptions(
            languageVersion: LanguageVersion.CSharp12,
            preprocessorSymbols: new[] { "CS2SX" });

        var trees = new List<SyntaxTree>();

        var stubTrees = LoadStubTrees(parseOptions);
        trees.AddRange(stubTrees);
        Log.Debug($"SemanticModel: {stubTrees.Count} Stub-Tree(s) geladen");

        foreach (var file in sourceFiles)
        {
            try
            {
                var source = File.ReadAllText(file);
                var tree = CSharpSyntaxTree.ParseText(source, parseOptions, path: file);
                trees.Add(tree);
            }
            catch (Exception ex)
            {
                Log.Warning($"SemanticModel: Datei nicht lesbar: {file} ({ex.Message})");
            }
        }

        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.ConsoleApplication,
            allowUnsafe: true,
            nullableContextOptions: NullableContextOptions.Enable,
            optimizationLevel: OptimizationLevel.Debug);

        _compilation = CSharpCompilation.Create(
            "cs2sx_analysis",
            trees,
            s_baseRefs,
            compilationOptions);

        // SemanticModels für Projektdateien aufbauen (nicht für Stubs)
        foreach (var tree in trees.Skip(stubTrees.Count))
        {
            var path = tree.FilePath;
            if (!string.IsNullOrEmpty(path))
            {
                var model = _compilation.GetSemanticModel(tree, ignoreAccessibility: true);
                _models[Path.GetFullPath(path)] = model;

                // Klassenname → Modell für GenericExpander
                // (synthetische Pfade wie <generic:Stack_int> haben keinen echten Pfad)
                foreach (var cls in tree.GetRoot()
                    .DescendantNodes()
                    .OfType<Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax>())
                {
                    var className = cls.Identifier.Text;
                    if (!_modelsByClass.ContainsKey(className))
                        _modelsByClass[className] = model;
                }
            }
        }

        var errors = _compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Take(5)
            .ToList();

        if (errors.Count > 0)
        {
            Log.Debug($"SemanticModel: {errors.Count} Kompilierungsfehler (ignoriert)");
            foreach (var e in errors)
                Log.Debug($"  {e.Id}: {e.GetMessage()}");
        }
        else
        {
            Log.Debug($"SemanticModel: Compilation erfolgreich ({sourceFiles.Count} Projektdatei(en))");
        }
    }

    // ── Öffentliche API ───────────────────────────────────────────────────────

    /// <summary>
    /// Gibt das SemanticModel für eine Datei zurück.
    /// Toleriert synthetische Pfade (&lt;generic:...&gt;) — gibt null zurück.
    /// </summary>
    public SemanticModel? GetModel(string filePath)
    {
        // Synthetische Pfade (von GenericExpander) → kein Modell
        if (filePath.StartsWith('<') && filePath.EndsWith('>'))
            return null;

        _models.TryGetValue(Path.GetFullPath(filePath), out var model);
        return model;
    }

    /// <summary>
    /// NEU: Sucht ein SemanticModel anhand eines Klassennamens.
    /// Wird von GenericExpander genutzt wenn der Dateipfad synthetisch ist.
    /// Gibt null zurück wenn kein Modell gefunden.
    /// </summary>
    public SemanticModel? GetModelForBaseName(string className)
    {
        _modelsByClass.TryGetValue(className, out var model);
        return model;
    }

    public bool HasModel(string filePath) =>
        _models.ContainsKey(Path.GetFullPath(filePath));

    // ── Stub-Loader ───────────────────────────────────────────────────────────

    private static List<SyntaxTree> LoadStubTrees(CSharpParseOptions parseOptions)
    {
        var result = new List<SyntaxTree>();
        var assembly = Assembly.GetExecutingAssembly();

        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            if (!resourceName.Contains(".Stubs."))
                continue;

            try
            {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) continue;

                using var reader = new StreamReader(stream);
                var source = reader.ReadToEnd();

                var tree = CSharpSyntaxTree.ParseText(
                    source,
                    parseOptions,
                    path: resourceName);

                result.Add(tree);
            }
            catch (Exception ex)
            {
                Log.Debug($"SemanticModel: Stub '{resourceName}' nicht geladen: {ex.Message}");
            }
        }

        return result;
    }
}