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
            sPrepare.Status   = StageStatus.Done;
            sPrepare.Elapsed  = $"{(DateTime.Now - started).TotalMilliseconds:F0}ms";

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
            var appSourceFiles         = reader.SourceFiles.ToList();
            var allForFwd              = switchFormsFilesForFwd.Concat(appSourceFiles).ToList();
            var forwardPath            = Path.Combine(_buildDir, "_forward.h");
            WriteForwardDeclarations(allForFwd, forwardPath);

            sFwd.Progress = 100;
            sFwd.Status   = StageStatus.Done;
            sFwd.Elapsed  = $"{(DateTime.Now - started).TotalMilliseconds:F0}ms";

            // ── semantic ──────────────────────────────────────────────────
            var sSemantic = renderer.GetStage("semantic");
            sSemantic.Status = StageStatus.Running;
            sSemantic.Detail = "building Roslyn compilation…";

            var transpiledFiles = appSourceFiles
                .Where(f => !s_switchFormsSkipTranspile.Contains(Path.GetFileName(f)))
                .ToList();

            // SemanticModelBuilder einmalig für alle Projektdateien erstellen
            var semanticBuilder = new SemanticModelBuilder(transpiledFiles);

            Log.Info($"SemanticModel: {transpiledFiles.Count} file(s) analysed");

            sSemantic.Progress = 100;
            sSemantic.Status   = StageStatus.Done;
            sSemantic.Detail   = $"{transpiledFiles.Count} file(s)";
            sSemantic.Elapsed  = $"{(DateTime.Now - started).TotalMilliseconds:F0}ms";

            // ── transpile (incremental) ───────────────────────────────────
            var sTranspile = renderer.GetStage("transpile");
            sTranspile.Status = StageStatus.Running;

            var allHeaders = transpiledFiles
                .Select(f => Path.GetFileNameWithoutExtension(f) + ".h")
                .ToList();

            var cFiles = new List<string> { switchformsCPath };

            int transpiled = 0;
            int skipped    = 0;

            for (int i = 0; i < transpiledFiles.Count; i++)
            {
                var csFile   = transpiledFiles[i];
                var baseName = Path.GetFileNameWithoutExtension(csFile);
                sTranspile.Detail   = $"{baseName}.cs";
                sTranspile.Progress = (int)((i + 1) / (double)transpiledFiles.Count * 100);

                var hPath = Path.Combine(_buildDir, baseName + ".h");
                var cPath = Path.Combine(_buildDir, baseName + ".c");

                if (IsUpToDate(csFile, hPath, cPath))
                {
                    Log.Info($"→ {baseName}.cs (unchanged)");
                    cFiles.Add(cPath);
                    skipped++;
                    continue;
                }

                Log.Info($"→ {baseName}.cs");

                var source = File.ReadAllText(csFile);

                // SemanticModel für diese Datei
                var semanticModel = semanticBuilder.GetModel(csFile);

                // Header generieren
                var hTranspiler = new CSharpToC(CSharpToC.TranspileMode.HeaderOnly);
                var hContent = WrapHeader(baseName,
                    "#include \"_forward.h\"\n\n"
                    + hTranspiler.Transpile(source, csFile, semanticModel));
                File.WriteAllText(hPath, hContent);

                // Implementation generieren
                var allIncludes = string.Join("\n",
                    allHeaders.Select(h => $"#include \"{h}\""));

                var cTranspiler = new CSharpToC(CSharpToC.TranspileMode.Implementation);
                var cContent = "#include <stdlib.h>\n"
                                + allIncludes + "\n\n"
                                + cTranspiler.Transpile(source, csFile, semanticModel);
                File.WriteAllText(cPath, cContent);
                cFiles.Add(cPath);
                transpiled++;
            }

            sTranspile.Detail = transpiled > 0
                ? $"{transpiled} transpiled, {skipped} unchanged"
                : $"all {skipped} unchanged";

            sTranspile.Progress = 100;
            sTranspile.Status   = StageStatus.Done;
            sTranspile.Elapsed  = $"{(DateTime.Now - started).TotalMilliseconds:F0}ms";

            // ── compile ───────────────────────────────────────────────────
            var sCompile = renderer.GetStage("compile");
            sCompile.Status = StageStatus.Running;

            var appClass      = FindSwitchAppClass(appSourceFiles) ?? _config.MainClass;
            var appHeaderFile = FindHeaderForClass(appSourceFiles, appClass)
                              ?? (appClass + ".h");
            var mainPath = EntryPointGenerator.Write(
                _buildDir, appClass, appHeaderFile, allHeaders);
            cFiles.Add(mainPath);

            foreach (var hFile in Directory.GetFiles(_projectDir, "*.h"))
            {
                var dest = Path.Combine(_buildDir, Path.GetFileName(hFile));
                File.Copy(hFile, dest, overwrite: true);
                Log.Info($"Custom header: {Path.GetFileName(hFile)}");
            }

            sCompile.Detail = $"{cFiles.Count} translation unit(s)";
            var elfPath = Path.Combine(_buildDir, _config.Name + ".elf");
            new CCompiler().Compile(cFiles, elfPath, _buildDir, _projectDir);

            sCompile.Progress = 100;
            sCompile.Status   = StageStatus.Done;
            sCompile.Elapsed  = $"{(DateTime.Now - started).TotalMilliseconds:F0}ms";

            // ── package ───────────────────────────────────────────────────
            var sPackage = renderer.GetStage("package");
            sPackage.Status = StageStatus.Running;
            sPackage.Detail = "nacp + nro";

            var nacpPath = Path.Combine(_buildDir, _config.Name + ".nacp");
            new NacpBuilder().Build(nacpPath, _config.Name, _config.Author, _config.Version);
            sPackage.Progress = 50;

            var nroPath  = Path.Combine(_projectDir, _config.Name + ".nro");
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
            sPackage.Status   = StageStatus.Done;
            sPackage.Detail   = nroPath;
            sPackage.Elapsed  = $"{(DateTime.Now - started).TotalMilliseconds:F0}ms";

            Log.Ok($"→ {nroPath}");
            renderer.Complete(DateTime.Now - started, warnings, errors: 0);
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
            foreach (var stage in new[] { "prepare", "fwd-decl", "semantic", "transpile", "compile", "package" })
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

    // ── Incremental Build ─────────────────────────────────────────────────

    private bool IsUpToDate(string csFile, string hPath, string cPath)
    {
        if (!File.Exists(hPath) || !File.Exists(cPath)) return false;

        var csTime = File.GetLastWriteTimeUtc(csFile);
        var hTime  = File.GetLastWriteTimeUtc(hPath);
        var cTime  = File.GetLastWriteTimeUtc(cPath);

        if (hTime < csTime || cTime < csTime) return false;

        var switchformsH = Path.Combine(_buildDir, "switchforms.h");
        if (File.Exists(switchformsH))
        {
            var sfTime = File.GetLastWriteTimeUtc(switchformsH);
            if (hTime < sfTime || cTime < sfTime) return false;
        }

        var forwardH = Path.Combine(_buildDir, "_forward.h");
        if (File.Exists(forwardH))
        {
            var fwdTime = File.GetLastWriteTimeUtc(forwardH);
            if (hTime < fwdTime || cTime < fwdTime) return false;
        }

        return true;
    }

    // ── Hilfsmethoden ─────────────────────────────────────────────────────

    private static void WriteForwardDeclarations(
        IReadOnlyList<string> sourceFiles, string outputPath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#pragma once");
        sb.AppendLine("#include <switch.h>");
        sb.AppendLine("#include <stdlib.h>");
        sb.AppendLine("#include <string.h>");
        sb.AppendLine("#include <stdio.h>");
        sb.AppendLine("#include <stdbool.h>");
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

        foreach (var csFile in sourceFiles)
        {
            var source = File.ReadAllText(csFile);
            var tree   = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source);
            foreach (var cls in tree.GetRoot()
                .DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                var typeName = cls.Identifier.Text;
                if (alreadyDeclared.Add(typeName))
                    sb.AppendLine($"typedef struct {typeName} {typeName};");
            }
        }

        File.WriteAllText(outputPath, sb.ToString());
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
            var source = File.ReadAllText(file);
            var tree   = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source);
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
            if (File.ReadAllText(file).Contains("class " + className))
                return Path.GetFileNameWithoutExtension(file) + ".h";
        return null;
    }

    private static void WriteSwitchformsC(string path)
    {
        using var w = new StreamWriter(path, append: false, encoding: Encoding.UTF8);
        w.WriteLine("#include \"_forward.h\"");
        w.WriteLine();
        w.WriteLine("char        _cs2sx_strbuf[512];");
        w.WriteLine("Framebuffer g_fb;");
        w.WriteLine("u32*        g_fb_addr   = NULL;");
        w.WriteLine("int         g_fb_width  = 1280;");
        w.WriteLine("int         g_fb_height = 720;");
        w.WriteLine("int         g_gfx_init  = 0;");
    }
}