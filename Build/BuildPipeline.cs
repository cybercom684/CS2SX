using CS2SX.Core;
using CS2SX.Logging;
using CS2SX.Transpiler;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace CS2SX.Build;

public sealed class BuildPipeline
{
    private readonly ProjectConfig _config;
    private readonly string _projectDir;
    private readonly string _buildDir;

    private static readonly HashSet<string> s_switchFormsSkipTranspile =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Button.cs", "Control.cs", "Form.cs",
            "Label.cs", "ProgressBar.cs", "SwitchApp.cs",
        };

    public BuildPipeline(string csprojPath)
    {
        _projectDir = Path.GetDirectoryName(Path.GetFullPath(csprojPath))
            ?? throw new ArgumentException("Ungültiger Pfad", nameof(csprojPath));
        _config = ProjectConfig.Load(_projectDir);
        _buildDir = Path.Combine(_projectDir, "cs2sx_out");
    }

    public void Run()
    {
        var started = DateTime.Now;

        using var renderer = new BuildRenderer([
            new BuildStage { Name = "prepare"   },
            new BuildStage { Name = "fwd-decl"  },
            new BuildStage { Name = "generics"  }, // NEU
            new BuildStage { Name = "semantic"  },
            new BuildStage { Name = "transpile" },
            new BuildStage { Name = "compile"   },
            new BuildStage { Name = "package"   },
        ]);
        Log.AttachRenderer(renderer);

        int warnings = 0;

        try
        {
            // ── prepare ───────────────────────────────────────────────────
            var sPrepare = renderer.GetStage("prepare");
            sPrepare.Status = StageStatus.Running;

            Directory.CreateDirectory(_buildDir);

            var csprojFiles = Directory.GetFiles(_projectDir, "*.csproj");
            if (csprojFiles.Length == 0)
                throw new FileNotFoundException("Keine .csproj-Datei gefunden in: " + _projectDir);

            Log.Info($"{_config.Name} v{_config.Version} by {_config.Author}");

            var reader = new ProjectReader();
            reader.Load(csprojFiles[0]);

            if (reader.SourceFiles.Count == 0)
                throw new InvalidOperationException("Keine .cs-Quelldateien gefunden.");

            Log.Info($"{reader.SourceFiles.Count} source file(s) found");
            RuntimeExporter.Export(_buildDir);

            var switchformsCPath = Path.Combine(_buildDir, "switchforms.c");
            WriteSwitchformsC(switchformsCPath);

            sPrepare.Progress = 100;
            sPrepare.Status = StageStatus.Done;
            sPrepare.Elapsed = $"{(DateTime.Now - started).TotalMilliseconds:F0}ms";

            // ── fwd-decl ──────────────────────────────────────────────────
            var sFwd = renderer.GetStage("fwd-decl");
            sFwd.Status = StageStatus.Running;

            var switchFormsDir = Path.Combine(Path.GetTempPath(),
                "cs2sx_" + Path.GetFileName(_projectDir));
            if (Directory.Exists(switchFormsDir))
                Directory.Delete(switchFormsDir, recursive: true);
            Directory.CreateDirectory(switchFormsDir);
            RuntimeExporter.ExportSwitchForms(switchFormsDir);

            var switchFormsFilesForFwd = Directory.GetFiles(switchFormsDir, "*.cs").ToList();
            var appSourceFiles = reader.SourceFiles.ToList();
            var allForFwd = switchFormsFilesForFwd.Concat(appSourceFiles).ToList();
            var forwardPath = Path.Combine(_buildDir, "_forward.h");
            // Vorläufig ohne expanded types — wird nach dem generics-Pass aktualisiert
            WriteForwardDeclarations(allForFwd, forwardPath, expandedTypeNames: null);

            sFwd.Progress = 100;
            sFwd.Status = StageStatus.Done;
            sFwd.Elapsed = $"{(DateTime.Now - started).TotalMilliseconds:F0}ms";

            // ── generics (NEU) ────────────────────────────────────────────
            var sGenerics = renderer.GetStage("generics");
            sGenerics.Status = StageStatus.Running;
            sGenerics.Detail = "collecting instantiations…";

            var transpiledFiles = appSourceFiles
                .Where(f => !s_switchFormsSkipTranspile.Contains(Path.GetFileName(f)))
                .ToList();

            // Pass 1: Generic/Interface/Extension-Definitionen und Instantiierungen sammeln
            var genericCollector = new GenericInstantiationCollector();
            genericCollector.Collect(transpiledFiles, switchFormsFilesForFwd);

            var interfaceExpander = new InterfaceExpander(genericCollector);
            interfaceExpander.AnalyzeImplementations(transpiledFiles);

            // Interface-Header schreiben
            var ifaceHeaderPath = interfaceExpander.WriteInterfaceHeader(_buildDir);

            // Generic-Code expandieren und in Dateien schreiben
            var genericExpander = new GenericExpander(genericCollector);
            var (genericHeaderPath, genericImplPath) = genericExpander.WriteToFiles(_buildDir);

            // _forward.h neu schreiben mit den jetzt bekannten expandierten Typen.
            // Stack_int, Pair_str_float etc. brauchen forward decls damit
            // andere .h-Dateien sie in Signaturen referenzieren können.
            var expandedNames = genericExpander.GetExpandedTypeNames().ToList();
            if (expandedNames.Count > 0)
            {
                WriteForwardDeclarations(allForFwd, forwardPath, expandedNames);
                Log.Info($"_forward.h aktualisiert mit {expandedNames.Count} expanded type(s)");
            }

            var genericsInfo = new List<string>();
            if (genericCollector.GenericClasses.Count > 0)
                genericsInfo.Add($"{genericCollector.GenericClasses.Count} generic class(es)");
            if (genericCollector.Interfaces.Count > 0)
                genericsInfo.Add($"{genericCollector.Interfaces.Count} interface(s)");
            if (genericCollector.ExtensionMethods.Count > 0)
                genericsInfo.Add($"{genericCollector.ExtensionMethods.Values.SelectMany(v => v).Count()} extension method(s)");
            if (genericCollector.Instantiations.Count > 0)
                genericsInfo.Add($"{genericCollector.Instantiations.Count} instantiation(s)");

            sGenerics.Detail = genericsInfo.Count > 0
                ? string.Join(", ", genericsInfo)
                : "nothing to expand";
            sGenerics.Progress = 100;
            sGenerics.Status = StageStatus.Done;
            sGenerics.Elapsed = $"{(DateTime.Now - started).TotalMilliseconds:F0}ms";

            // ── semantic ──────────────────────────────────────────────────
            var sSemantic = renderer.GetStage("semantic");
            sSemantic.Status = StageStatus.Running;
            sSemantic.Detail = "building Roslyn compilation…";

            var semanticBuilder = new SemanticModelBuilder(transpiledFiles);

            Log.Info($"SemanticModel: {transpiledFiles.Count} file(s) analysed");

            sSemantic.Progress = 100;
            sSemantic.Status = StageStatus.Done;
            sSemantic.Detail = $"{transpiledFiles.Count} file(s)";
            sSemantic.Elapsed = $"{(DateTime.Now - started).TotalMilliseconds:F0}ms";

            // ── transpile (incremental) ───────────────────────────────────
            var sTranspile = renderer.GetStage("transpile");
            sTranspile.Status = StageStatus.Running;

            var allHeaders = transpiledFiles
                .Select(f => Path.GetFileNameWithoutExtension(f) + ".h")
                .ToList();

            // NEU: Generierte Generic/Interface-Header einbeziehen
            if (!string.IsNullOrEmpty(genericHeaderPath))
                allHeaders.Insert(0, Path.GetFileName(genericHeaderPath));
            if (!string.IsNullOrEmpty(ifaceHeaderPath))
                allHeaders.Insert(0, Path.GetFileName(ifaceHeaderPath));

            var cFiles = new List<string> { switchformsCPath };

            // NEU: Generic-Implementierungen als .c-Datei hinzufügen
            if (!string.IsNullOrEmpty(genericImplPath) && File.Exists(genericImplPath))
                cFiles.Add(genericImplPath);

            int transpiled = 0;
            int skipped = 0;

            var allTranspiledHeaders = transpiledFiles
                .Select(f => Path.Combine(_buildDir, Path.GetFileNameWithoutExtension(f) + ".h"))
                .Where(File.Exists)
                .ToList();
            var latestHeaderTime = allTranspiledHeaders.Count > 0
                ? allTranspiledHeaders.Max(h => File.GetLastWriteTimeUtc(h))
                : DateTime.MinValue;

            for (int i = 0; i < transpiledFiles.Count; i++)
            {
                var csFile = transpiledFiles[i];
                var baseName = Path.GetFileNameWithoutExtension(csFile);
                sTranspile.Detail = $"{baseName}.cs";
                sTranspile.Progress = (int)((i + 1) / (double)transpiledFiles.Count * 100);

                var hPath = Path.Combine(_buildDir, baseName + ".h");
                var cPath = Path.Combine(_buildDir, baseName + ".c");

                if (IsUpToDate(csFile, hPath, cPath, latestHeaderTime))
                {
                    Log.Info($"→ {baseName}.cs (unchanged)");
                    cFiles.Add(cPath);
                    skipped++;
                    continue;
                }

                Log.Info($"→ {baseName}.cs");

                var source = System.IO.File.ReadAllText(csFile);
                var semanticModel = semanticBuilder.GetModel(csFile);

                // NEU: Transpiler mit Generic/Interface-Support instanziieren
                var hTranspiler = new CSharpToC(
                    CSharpToC.TranspileMode.HeaderOnly,
                    genericCollector,
                    interfaceExpander);
                var hResult = hTranspiler.Transpile(source, csFile, semanticModel);

                // NEU: Interface-Header im Include-Block
                var interfaceInclude = !string.IsNullOrEmpty(ifaceHeaderPath)
                    ? $"#include \"{Path.GetFileName(ifaceHeaderPath)}\"\n"
                    : "";
                var genericInclude = !string.IsNullOrEmpty(genericHeaderPath)
                    ? $"#include \"{Path.GetFileName(genericHeaderPath)}\"\n"
                    : "";

                var hContent = WrapHeader(baseName,
                    "#include \"_forward.h\"\n"
                    + interfaceInclude
                    + genericInclude
                    + "\n" + hResult.Code);
                System.IO.File.WriteAllText(hPath, hContent);

                var allIncludes = string.Join("\n",
                    allHeaders.Select(h => $"#include \"{h}\""));

                var cTranspiler = new CSharpToC(
                    CSharpToC.TranspileMode.Implementation,
                    genericCollector,
                    interfaceExpander);
                var cResult = cTranspiler.Transpile(source, csFile, semanticModel);
                var cContent = "#include <stdlib.h>\n"
                             + allIncludes + "\n\n"
                             + cResult.Code;
                System.IO.File.WriteAllText(cPath, cContent);

                var allDiags = hResult.Diagnostics
                    .Concat(cResult.Diagnostics)
                    .DistinctBy(d => (d.CsFile, d.CsLine, d.Message))
                    .OrderBy(d => d.CsLine)
                    .ToList();

                foreach (var d in allDiags.Where(d => d.Severity == DiagnosticSeverity.Warning))
                    Log.Warning($"{Path.GetFileName(d.CsFile ?? "")}({d.CsLine}): {d.Message}"
                        + (d.Context != null ? $"\n  code: {d.Context}" : ""));

                foreach (var d in allDiags.Where(d => d.Severity == DiagnosticSeverity.Error))
                    Log.Error($"{Path.GetFileName(d.CsFile ?? "")}({d.CsLine}): {d.Message}");

                warnings += allDiags.Count(d => d.Severity == DiagnosticSeverity.Warning);

                cFiles.Add(cPath);
                transpiled++;
            }

            sTranspile.Detail = transpiled > 0
                ? $"{transpiled} transpiled, {skipped} unchanged"
                : $"all {skipped} unchanged";

            sTranspile.Progress = 100;
            sTranspile.Status = StageStatus.Done;
            sTranspile.Elapsed = $"{(DateTime.Now - started).TotalMilliseconds:F0}ms";

            // ── compile ───────────────────────────────────────────────────
            var sCompile = renderer.GetStage("compile");
            sCompile.Status = StageStatus.Running;

            var appClass = FindSwitchAppClass(appSourceFiles) ?? _config.MainClass;
            var appHeaderFile = FindHeaderForClass(appSourceFiles, appClass)
                              ?? (appClass + ".h");
            var mainPath = EntryPointGenerator.Write(
                _buildDir, appClass, appHeaderFile, allHeaders);
            cFiles.Add(mainPath);

            foreach (var hFile in Directory.GetFiles(_projectDir, "*.h"))
            {
                var dest = Path.Combine(_buildDir, Path.GetFileName(hFile));
                System.IO.File.Copy(hFile, dest, overwrite: true);
                Log.Info($"Custom header: {Path.GetFileName(hFile)}");
            }

            sCompile.Detail = $"{cFiles.Count} translation unit(s)";
            var elfPath = Path.Combine(_buildDir, _config.Name + ".elf");
            new CCompiler().Compile(cFiles, elfPath, _buildDir, _projectDir);

            sCompile.Progress = 100;
            sCompile.Status = StageStatus.Done;
            sCompile.Elapsed = $"{(DateTime.Now - started).TotalMilliseconds:F0}ms";

            // ── package ───────────────────────────────────────────────────
            var sPackage = renderer.GetStage("package");
            sPackage.Status = StageStatus.Running;
            sPackage.Detail = "nacp + nro";

            var nacpPath = Path.Combine(_buildDir, _config.Name + ".nacp");
            new NacpBuilder().Build(nacpPath, _config.Name, _config.Author, _config.Version);
            sPackage.Progress = 50;

            var nroPath = Path.Combine(_projectDir, _config.Name + ".nro");
            var iconPath = _config.Icon != null
                ? Path.Combine(_projectDir, _config.Icon)
                : null;
            if (iconPath == null)
            {
                Log.Warning("No icon found — using default");
                warnings++;
            }
            new NroBuilder().Build(elfPath, nroPath, nacpPath, iconPath);

            sPackage.Progress = 100;
            sPackage.Status = StageStatus.Done;
            sPackage.Detail = nroPath;
            sPackage.Elapsed = $"{(DateTime.Now - started).TotalMilliseconds:F0}ms";

            Log.Ok($"→ {nroPath}");
            renderer.Complete(DateTime.Now - started, warnings, errors: 0);
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
            foreach (var stage in new[] { "prepare", "fwd-decl", "generics", "semantic", "transpile", "compile", "package" })
            {
                var s = renderer.GetStage(stage);
                if (s.Status == StageStatus.Running)
                {
                    s.Status = StageStatus.Failed;
                    break;
                }
            }
            renderer.Complete(DateTime.Now - started, warnings, errors: 1);
            throw;
        }
        finally
        {
            Log.DetachRenderer();
        }
    }

    /// <summary>
    /// Erweiterter Inkremental-Check.
    /// </summary>
    private bool IsUpToDate(string csFile, string hPath, string cPath,
        DateTime latestHeaderTime = default)
    {
        if (!File.Exists(hPath) || !File.Exists(cPath)) return false;

        var csTime = File.GetLastWriteTimeUtc(csFile);
        var hTime = File.GetLastWriteTimeUtc(hPath);
        var cTime = File.GetLastWriteTimeUtc(cPath);

        if (hTime < csTime || cTime < csTime) return false;

        var switchformsH = Path.Combine(_buildDir, "switchforms.h");
        if (File.Exists(switchformsH) && (File.GetLastWriteTimeUtc(switchformsH) > hTime
                                       || File.GetLastWriteTimeUtc(switchformsH) > cTime))
            return false;

        var forwardH = Path.Combine(_buildDir, "_forward.h");
        if (File.Exists(forwardH) && (File.GetLastWriteTimeUtc(forwardH) > hTime
                                   || File.GetLastWriteTimeUtc(forwardH) > cTime))
            return false;

        // NEU: Generics-Header invalidieren
        var genericsH = Path.Combine(_buildDir, "_generics.h");
        if (File.Exists(genericsH) && File.GetLastWriteTimeUtc(genericsH) > hTime)
            return false;

        // NEU: Interface-Header invalidieren
        var ifacesH = Path.Combine(_buildDir, "_interfaces.h");
        if (File.Exists(ifacesH) && File.GetLastWriteTimeUtc(ifacesH) > hTime)
            return false;

        if (latestHeaderTime != default && latestHeaderTime > hTime)
            return false;

        return true;
    }

    /// <summary>
    /// Schreibt _forward.h mit typedef-Vorwärtsdeklarationen für alle Typen.
    ///
    /// expandedTypeNames: expandierte Generic-Typen (Stack_int, Pair_str_float…)
    /// die aus _generics.h kommen und ebenfalls forward-deklariert werden müssen,
    /// damit sie in Signaturen anderer .h-Dateien verwendet werden können bevor
    /// _generics.h inkludiert wird.
    ///
    /// Der Aufruf findet zweimal statt:
    ///   1. Im fwd-decl-Stage: expandedTypeNames = null (Generics noch nicht bekannt)
    ///   2. Im generics-Stage: expandedTypeNames gefüllt → _forward.h wird überschrieben
    /// </summary>
    private static void WriteForwardDeclarations(
        IReadOnlyList<string> sourceFiles,
        string outputPath,
        IEnumerable<string>? expandedTypeNames = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#pragma once");
        sb.AppendLine("#include <switch.h>");
        sb.AppendLine("#include <stdlib.h>");
        sb.AppendLine("#include <string.h>");
        sb.AppendLine("#include <stdio.h>");
        sb.AppendLine("#include <stdbool.h>");
        sb.AppendLine("#include <limits.h>");
        sb.AppendLine("#include <float.h>");
        sb.AppendLine("#include <math.h>");
        sb.AppendLine();
        sb.AppendLine("typedef void (*Action)(void*);");
        sb.AppendLine();
        sb.AppendLine("typedef struct Control    Control;");
        sb.AppendLine("typedef struct Form       Form;");
        sb.AppendLine("typedef struct Label      Label;");
        sb.AppendLine("typedef struct Button     Button;");
        sb.AppendLine("typedef struct ProgressBar ProgressBar;");
        sb.AppendLine("typedef struct SwitchApp  SwitchApp;");
        sb.AppendLine();
        sb.AppendLine("#include \"switchapp.h\"");
        sb.AppendLine();
        sb.AppendLine("extern char _cs2sx_strbuf[512];");
        sb.AppendLine();

        var alreadyDeclared = new HashSet<string>(StringComparer.Ordinal)
        {
            "Control", "Form", "Label", "Button", "ProgressBar", "SwitchApp",
        };

        // Normale Typen aus den Quelldateien
        foreach (var csFile in sourceFiles)
        {
            var source = System.IO.File.ReadAllText(csFile);
            var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source);
            foreach (var typeDecl in tree.GetRoot()
                .DescendantNodes()
                .Where(n => n is ClassDeclarationSyntax or StructDeclarationSyntax))
            {
                // Generische Klassen selbst nicht forward-deklarieren —
                // ihre konkreten Expansionen (Stack_int etc.) kommen weiter unten.
                if (typeDecl is ClassDeclarationSyntax cls
                    && cls.TypeParameterList?.Parameters.Count > 0)
                    continue;

                var typeName = typeDecl is ClassDeclarationSyntax c
                    ? c.Identifier.Text
                    : ((StructDeclarationSyntax)typeDecl).Identifier.Text;

                if (alreadyDeclared.Add(typeName))
                    sb.AppendLine($"typedef struct {typeName} {typeName};");
            }
        }

        // Expandierte Generic-Typen: Stack_int, Pair_str_float, …
        // Diese kommen aus _generics.h und müssen hier als opaque forward decl
        // erscheinen, damit Signaturen in anderen .h-Dateien sie referenzieren können
        // noch bevor _generics.h inkludiert wird.
        if (expandedTypeNames != null)
        {
            var hadAny = false;
            foreach (var expandedName in expandedTypeNames)
            {
                if (!alreadyDeclared.Add(expandedName)) continue;
                if (!hadAny)
                {
                    sb.AppendLine();
                    sb.AppendLine("// Expanded generic types (from _generics.h)");
                    hadAny = true;
                }
                sb.AppendLine($"typedef struct {expandedName} {expandedName};");
            }
        }

        System.IO.File.WriteAllText(outputPath, sb.ToString());
    }

    private static string WrapHeader(string baseName, string content)
    {
        var guard = "CS2SX_" + baseName.ToUpperInvariant() + "_H";
        return "#pragma once\n"
             + "#ifndef " + guard + "\n"
             + "#define " + guard + "\n\n"
             + content
             + "\n\n#endif\n";
    }

    private static string? FindSwitchAppClass(IReadOnlyList<string> sourceFiles)
    {
        foreach (var file in sourceFiles)
        {
            var source = System.IO.File.ReadAllText(file);
            var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source);
            foreach (var cls in tree.GetRoot()
                .DescendantNodes().OfType<ClassDeclarationSyntax>())
                if (cls.BaseList?.Types.FirstOrDefault()?.ToString().Trim() == "SwitchApp")
                    return cls.Identifier.Text;
        }
        return null;
    }

    private static string? FindHeaderForClass(
        IReadOnlyList<string> sourceFiles, string className)
    {
        foreach (var file in sourceFiles)
            if (System.IO.File.ReadAllText(file).Contains("class " + className))
                return Path.GetFileNameWithoutExtension(file) + ".h";
        return null;
    }

    private static void WriteSwitchformsC(string path)
    {
        using var w = new StreamWriter(path, append: false, encoding: Encoding.UTF8);
        w.WriteLine("#include \"_forward.h\"");
        w.WriteLine();
        w.WriteLine("char         _cs2sx_strbuf[512];");
        w.WriteLine("Framebuffer  g_fb;");
        w.WriteLine("u32*         g_fb_addr       = NULL;");
        w.WriteLine("int          g_fb_width      = 1280;");
        w.WriteLine("int          g_fb_height     = 720;");
        w.WriteLine("int          g_gfx_init      = 0;");
        w.WriteLine("PadState     g_cs2sx_pad;");
        w.WriteLine("unsigned int _cs2sx_rand_state = 12345u;");
    }
}