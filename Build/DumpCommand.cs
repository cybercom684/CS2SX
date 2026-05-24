// ============================================================================
// CS2SX — Build/DumpCommand.cs
//
// Transpiles every (or a filtered subset of) project source files and prints
// the generated C code to stdout.  Useful for inspecting transpiler output
// without having to run a full build.
//
// Usage:
//   dotnet run -- dump <path/to/App.csproj>            # all files
//   dotnet run -- dump <path/to/App.csproj> Foo.cs     # one specific file
//   dotnet run -- dump <path/to/App.csproj> Foo.cs Bar.cs ...
// ============================================================================

using CS2SX.Core;
using CS2SX.Logging;
using CS2SX.Transpiler;

namespace CS2SX.Build;

public sealed class DumpCommand
{
    private readonly string _projectDir;

    // Files that are injected by the build pipeline — not user sources.
    private static readonly HashSet<string> s_skip =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "SwitchApp.cs", "Input.cs", "_GlobalTypes.cs",
        };

    /// <param name="csprojPath">Path to the .csproj file (or its containing directory).</param>
    public DumpCommand(string csprojPath)
    {
        _projectDir = Path.GetDirectoryName(Path.GetFullPath(csprojPath))
            ?? throw new ArgumentException("Invalid path", nameof(csprojPath));
    }

    /// <param name="filter">
    /// Optional list of base filenames (e.g. "EditorController.cs") to restrict
    /// the dump.  When empty, all project source files are dumped.
    /// </param>
    public int Run(IReadOnlyList<string> filter)
    {
        var csprojFiles = Directory.GetFiles(_projectDir, "*.csproj");
        if (csprojFiles.Length == 0)
        {
            Log.Error("No .csproj file found.");
            return 1;
        }

        var reader = new ProjectReader();
        reader.Load(csprojFiles[0]);

        if (reader.SourceFiles.Count == 0)
        {
            Log.Error("No .cs source files found.");
            return 1;
        }

        // All project files (used for semantic analysis and generic collection)
        var allFiles = reader.SourceFiles
            .Where(f => !s_skip.Contains(Path.GetFileName(f)))
            .ToList();

        // Subset to actually dump
        var filesToDump = filter.Count == 0
            ? allFiles
            : allFiles
                .Where(f => filter.Any(name =>
                    string.Equals(Path.GetFileName(f), name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

        if (filesToDump.Count == 0)
        {
            Log.Error($"None of the requested file(s) found in the project: {string.Join(", ", filter)}");
            return 1;
        }

        // ── Same analysis pipeline as CheckCommand / BuildPipeline ─────────────

        var switchFormsDir = Path.Combine(Path.GetTempPath(),
            "cs2sx_dump_" + Path.GetFileName(_projectDir));
        if (Directory.Exists(switchFormsDir))
            Directory.Delete(switchFormsDir, recursive: true);
        Directory.CreateDirectory(switchFormsDir);

        try
        {
            RuntimeExporter.ExportSwitchForms(switchFormsDir);
        }
        catch (Exception ex)
        {
            Log.Warning($"SwitchForms export failed: {ex.Message}");
        }

        var switchFormsFiles = Directory.GetFiles(switchFormsDir, "*.cs").ToList();

        var genericCollector = new GenericInstantiationCollector();
        genericCollector.Collect(allFiles, switchFormsFiles);

        var interfaceExpander = new InterfaceExpander(genericCollector);
        interfaceExpander.AnalyzeImplementations(allFiles);

        var semanticBuilder = new SemanticModelBuilder(allFiles);

        // Pre-scan all files for VTable types so cross-file virtual dispatch works.
        var sharedVTableTypes = BuildPipeline.PreScanVTableTypes(allFiles);

        // ── Transpile and print ────────────────────────────────────────────────

        int exitCode = 0;

        foreach (var csFile in filesToDump)
        {
            var baseName = Path.GetFileName(csFile);

            try
            {
                var source = File.ReadAllText(csFile);
                var semanticModel = semanticBuilder.GetModel(csFile);

                // ── Header (.h equivalent) ─────────────────────────────────────
                var hTranspiler = new CSharpToC(
                    CSharpToC.TranspileMode.HeaderOnly,
                    genericCollector,
                    interfaceExpander);
                foreach (var vt in sharedVTableTypes)
                    hTranspiler.GetContext().VTableTypes.Add(vt);
                var hResult = hTranspiler.Transpile(source, csFile, semanticModel);

                Console.WriteLine(new string('=', 72));
                Console.WriteLine($"// FILE: {baseName}  [HeaderOnly]");
                Console.WriteLine(new string('=', 72));
                Console.WriteLine(hResult.Code);

                if (hResult.Diagnostics.Count > 0)
                    PrintDiagnostics(hResult.Diagnostics, baseName, "H");

                // ── Implementation (.c equivalent) ─────────────────────────────
                var cTranspiler = new CSharpToC(
                    CSharpToC.TranspileMode.Implementation,
                    genericCollector,
                    interfaceExpander);
                foreach (var vt in sharedVTableTypes)
                    cTranspiler.GetContext().VTableTypes.Add(vt);
                var cResult = cTranspiler.Transpile(source, csFile, semanticModel);

                Console.WriteLine(new string('=', 72));
                Console.WriteLine($"// FILE: {baseName}  [Implementation]");
                Console.WriteLine(new string('=', 72));
                Console.WriteLine(cResult.Code);

                if (cResult.Diagnostics.Count > 0)
                    PrintDiagnostics(cResult.Diagnostics, baseName, "C");

                if (hResult.ErrorCount > 0 || cResult.ErrorCount > 0)
                    exitCode = 1;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"ERROR transpiling {baseName}: {ex.Message}");
                exitCode = 1;
            }
        }

        // Cleanup temp dir
        try { Directory.Delete(switchFormsDir, recursive: true); }
        catch { }

        return exitCode;
    }

    private static void PrintDiagnostics(
        IReadOnlyList<TranspilerDiagnostic> diags, string fileName, string stage)
    {
        foreach (var d in diags)
        {
            var loc = d.CsLine > 0 ? $"({d.CsLine})" : "";
            var prefix = d.Severity == DiagnosticSeverity.Error ? "ERROR" : "WARN";
            Console.Error.WriteLine($"[{stage}] {prefix} {fileName}{loc}: {d.Message}");
            if (d.Context != null)
                Console.Error.WriteLine($"    {d.Context}");
        }
    }
}
