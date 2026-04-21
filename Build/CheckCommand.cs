// ============================================================================
// CS2SX — Build/CheckCommand.cs  (FIXED)
//
// Fixes:
//   • Nutzt jetzt GenericInstantiationCollector + InterfaceExpander identisch
//     zur BuildPipeline — Check-Ergebnisse stimmen mit Build überein.
//   • CSharpToC wird mit vollem Collector instanziiert.
//   • SemanticModelBuilder wird korrekt initialisiert.
// ============================================================================

using CS2SX.Core;
using CS2SX.Logging;
using CS2SX.Transpiler;

namespace CS2SX.Build;

public sealed class CheckCommand
{
    private readonly string _projectDir;

    private static readonly HashSet<string> s_skip =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "SwitchApp.cs", "Input.cs", "_GlobalTypes.cs",
        };

    public CheckCommand(string csprojPath)
    {
        _projectDir = Path.GetDirectoryName(Path.GetFullPath(csprojPath))
            ?? throw new ArgumentException("Ungültiger Pfad", nameof(csprojPath));
    }

    public int Run()
    {
        Console.WriteLine();
        var config = ProjectConfig.Load(_projectDir);
        Console.WriteLine($"CS2SX check: {config.Name}");
        Console.WriteLine(new string('─', 60));

        var csprojFiles = Directory.GetFiles(_projectDir, "*.csproj");
        if (csprojFiles.Length == 0)
        {
            Log.Error("Keine .csproj-Datei gefunden.");
            return 1;
        }

        var reader = new ProjectReader();
        reader.Load(csprojFiles[0]);

        if (reader.SourceFiles.Count == 0)
        {
            Log.Error("Keine .cs-Quelldateien gefunden.");
            return 1;
        }

        var files = reader.SourceFiles
            .Where(f => !s_skip.Contains(Path.GetFileName(f)))
            .ToList();

        // FIX: Gleiche Analyse wie BuildPipeline
        var switchFormsDir = Path.Combine(Path.GetTempPath(),
            "cs2sx_check_" + Path.GetFileName(_projectDir));
        if (Directory.Exists(switchFormsDir))
            Directory.Delete(switchFormsDir, recursive: true);
        Directory.CreateDirectory(switchFormsDir);

        try
        {
            RuntimeExporter.ExportSwitchForms(switchFormsDir);
        }
        catch (Exception ex)
        {
            Log.Warning($"SwitchForms-Export fehlgeschlagen: {ex.Message}");
        }

        var switchFormsFiles = Directory.GetFiles(switchFormsDir, "*.cs").ToList();

        // FIX: Collector + Expander wie BuildPipeline
        var genericCollector = new GenericInstantiationCollector();
        genericCollector.Collect(files, switchFormsFiles);

        var interfaceExpander = new InterfaceExpander(genericCollector);
        interfaceExpander.AnalyzeImplementations(files);

        var semanticBuilder = new SemanticModelBuilder(files);

        int totalWarnings = 0;
        int totalErrors = 0;
        var results = new List<FileResult>();

        foreach (var csFile in files)
        {
            var baseName = Path.GetFileName(csFile);
            var diags = new List<TranspilerDiagnostic>();

            try
            {
                var source = File.ReadAllText(csFile);
                var semanticModel = semanticBuilder.GetModel(csFile);

                // FIX: Mit Collector instanziieren — identisch zu BuildPipeline
                var hResult = new CSharpToC(
                    CSharpToC.TranspileMode.HeaderOnly,
                    genericCollector,
                    interfaceExpander)
                    .Transpile(source, csFile, semanticModel);

                var cResult = new CSharpToC(
                    CSharpToC.TranspileMode.Implementation,
                    genericCollector,
                    interfaceExpander)
                    .Transpile(source, csFile, semanticModel);

                diags.AddRange(hResult.Diagnostics);
                diags.AddRange(cResult.Diagnostics);

                diags = diags
                    .DistinctBy(d => (d.CsFile, d.CsLine, d.Message))
                    .OrderBy(d => d.CsLine)
                    .ToList();
            }
            catch (Exception ex)
            {
                diags.Add(new TranspilerDiagnostic(
                    DiagnosticSeverity.Error, csFile, 0,
                    "Transpiler-Exception: " + ex.Message, null));
                totalErrors++;
            }

            var warnings = diags.Count(d => d.Severity == DiagnosticSeverity.Warning);
            var errors = diags.Count(d => d.Severity == DiagnosticSeverity.Error);

            totalWarnings += warnings;
            totalErrors += errors;

            results.Add(new FileResult(baseName, warnings, errors, diags));
        }

        foreach (var r in results)
        {
            if (r.Warnings == 0 && r.Errors == 0)
            {
                Console.WriteLine($"  ✓ {r.FileName,-44} 0 issues");
                continue;
            }

            var label = r.Errors > 0
                ? $"{r.Errors} error(s), {r.Warnings} warning(s)"
                : $"{r.Warnings} warning(s)";

            Console.ForegroundColor = r.Errors > 0 ? ConsoleColor.Red : ConsoleColor.Yellow;
            Console.WriteLine($"  ⚠ {r.FileName,-44} {label}");
            Console.ResetColor();

            foreach (var d in r.Diagnostics)
            {
                var loc = d.CsLine > 0 ? $"({d.CsLine})" : "";
                var sym = d.Severity == DiagnosticSeverity.Error ? "✗" : "!";
                var color = d.Severity == DiagnosticSeverity.Error
                    ? ConsoleColor.Red
                    : ConsoleColor.Yellow;

                Console.ForegroundColor = color;
                Console.Write($"    {sym} ");
                Console.ResetColor();
                Console.Write($"{r.FileName}{loc}: ");
                Console.WriteLine(d.Message);

                if (d.Context != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"        {d.Context}");
                    Console.ResetColor();
                }
            }
        }

        // Temp-Verzeichnis aufräumen
        try { Directory.Delete(switchFormsDir, recursive: true); }
        catch { }

        Console.WriteLine(new string('─', 60));

        if (totalErrors > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"✗ {totalErrors} error(s), {totalWarnings} warning(s)"
                + $" in {files.Count} file(s)  [transpile-only, kein GCC]");
            Console.ResetColor();
            Console.WriteLine();
            return 1;
        }

        if (totalWarnings > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠ {totalWarnings} warning(s)"
                + $" in {files.Count} file(s)  [transpile-only, kein GCC]");
            Console.ResetColor();
            Console.WriteLine();
            return 2;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓ Keine Issues in {files.Count} Datei(en)  [transpile-only, kein GCC]");
        Console.ResetColor();
        Console.WriteLine();
        return 0;
    }

    private record FileResult(
        string FileName,
        int Warnings,
        int Errors,
        List<TranspilerDiagnostic> Diagnostics);
}