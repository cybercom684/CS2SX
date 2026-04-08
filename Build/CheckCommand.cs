// ============================================================================
// CS2SX — Build/CheckCommand.cs  (NEU)
//
// `cs2sx check <project.csproj>`
//
// Transpiliert alle Quelldateien und gibt Warnings aus —
// OHNE GCC aufzurufen. Ideal für schnelles Feedback während der Entwicklung.
//
// Output-Beispiel:
//
//   CS2SX check: MyGame/MyGame.csproj
//   ✓ Game.cs         — 0 warnings
//   ⚠ Physics.cs      — 2 warnings
//     Physics.cs(42): foreach über 'bodies' (Typ '') nicht erkannt — ...
//     Physics.cs(87): Nicht unterstütztes Statement: YieldStatementSyntax
//   ✓ UI.cs           — 0 warnings
//   ─────────────────────────────────────────
//   2 warning(s) in 3 file(s)  [transpile only, no GCC]
// ============================================================================

using CS2SX.Core;
using CS2SX.Logging;
using CS2SX.Transpiler;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Build;

public sealed class CheckCommand
{
    private readonly string _projectDir;

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
        Console.WriteLine(new string('─', 50));

        var reader = new ProjectReader();
        var csprojFiles = Directory.GetFiles(_projectDir, "*.csproj");
        if (csprojFiles.Length == 0)
        {
            Log.Error("Keine .csproj-Datei gefunden.");
            return 1;
        }

        reader.Load(csprojFiles[0]);

        if (reader.SourceFiles.Count == 0)
        {
            Log.Error("Keine .cs-Quelldateien gefunden.");
            return 1;
        }

        var transpiledFiles = reader.SourceFiles
            .Where(f => !ShouldSkip(f))
            .ToList();

        // Semantic Model aufbauen für bessere Typ-Auflösung
        var semanticBuilder = new SemanticModelBuilder(transpiledFiles);

        int totalWarnings = 0;
        int totalFiles = 0;
        var fileResults = new List<(string file, int warnings, List<string> messages)>();

        foreach (var csFile in transpiledFiles)
        {
            totalFiles++;
            var baseName = Path.GetFileName(csFile);

            try
            {
                var source = File.ReadAllText(csFile);
                var semanticModel = semanticBuilder.GetModel(csFile);

                // Transpilieren ohne GCC
                var transpiler = new CSharpToC(CSharpToC.TranspileMode.Implementation);
                transpiler.Transpile(source, csFile, semanticModel);

                // Diagnostics sammeln
                var ctx = transpiler.GetContext();
                var diags = ctx.Diagnostics.All
                    .Where(d => d.Severity == DiagnosticSeverity.Warning)
                    .ToList();

                var messages = diags
                    .Select(d =>
                    {
                        var loc = d.CsLine > 0 ? $"({d.CsLine})" : "";
                        var snippet = d.Context != null ? $"\n      {d.Context}" : "";
                        return $"  {baseName}{loc}: {d.Message}{snippet}";
                    })
                    .ToList();

                fileResults.Add((baseName, diags.Count, messages));
                totalWarnings += diags.Count;
            }
            catch (Exception ex)
            {
                fileResults.Add((baseName, 1,
                    new List<string> { $"  {baseName}: FEHLER: {ex.Message}" }));
                totalWarnings++;
            }
        }

        // Ausgabe
        foreach (var (file, warnings, messages) in fileResults)
        {
            if (warnings == 0)
            {
                Console.WriteLine($"  ✓ {file,-40} 0 warnings");
            }
            else
            {
                Console.WriteLine($"  ⚠ {file,-40} {warnings} warning(s)");
                foreach (var msg in messages)
                    Console.WriteLine(msg);
            }
        }

        Console.WriteLine(new string('─', 50));

        if (totalWarnings == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Keine Warnings in {totalFiles} Datei(en)  [transpile-only, kein GCC]");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠ {totalWarnings} Warning(s) in {totalFiles} Datei(en)  [transpile-only, kein GCC]");
        }

        Console.ResetColor();
        Console.WriteLine();

        return totalWarnings > 0 ? 2 : 0; // Exit-Code 2 = Warnings, 0 = clean
    }

    private static readonly HashSet<string> s_skip = new(StringComparer.OrdinalIgnoreCase)
    {
        "SwitchApp.cs", "Input.cs", "_GlobalTypes.cs",
    };

    private static bool ShouldSkip(string f) =>
        s_skip.Contains(Path.GetFileName(f));
}