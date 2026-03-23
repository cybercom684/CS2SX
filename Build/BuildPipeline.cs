using CS2SX.Transpiler;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace CS2SX.Build;

public sealed class BuildPipeline
{
    private readonly ProjectConfig _config;
    private readonly string _projectDir;
    private readonly string _buildDir;

    // SwitchForms-Dateien, die NUR für Forward-Declarations genutzt werden,
    // aber NICHT transpiliert werden (Implementierung kommt aus switchforms.h).
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
        // Output-Verzeichnis bereinigen um Stale-Files zu vermeiden
        if (Directory.Exists(_buildDir))
        {
            foreach (var f in Directory.GetFiles(_buildDir, "*.c")) File.Delete(f);
            foreach (var f in Directory.GetFiles(_buildDir, "*.h"))
            {
                var name = Path.GetFileName(f);
                if (name != "switchapp.h" && name != "switchforms.h")
                    File.Delete(f);
            }
        }
        Directory.CreateDirectory(_buildDir);

        var csprojFiles = Directory.GetFiles(_projectDir, "*.csproj");
        if (csprojFiles.Length == 0)
            throw new FileNotFoundException("Keine .csproj-Datei gefunden in: " + _projectDir);

        Log("App: " + _config.Name + " by " + _config.Author + " v" + _config.Version);

        var reader = new ProjectReader();
        reader.Load(csprojFiles[0]);

        if (reader.SourceFiles.Count == 0)
            throw new InvalidOperationException("Keine .cs-Quelldateien gefunden.");

        Log("Found " + reader.SourceFiles.Count + " source file(s)");

        // Schritt 1: Runtime-Header exportieren
        RuntimeExporter.Export(_buildDir);

        // switchforms.c (definiert _cs2sx_strbuf)
        var switchformsCPath = Path.Combine(_buildDir, "switchforms.c");
        WriteSwitchformsC(switchformsCPath);
        var cFiles = new List<string> { switchformsCPath };

        // SwitchForms C#-Quelldateien für Forward-Declaration-Analyse exportieren
        var switchFormsDir = Path.Combine(
            Path.GetTempPath(),
            "cs2sx_" + Path.GetFileName(_projectDir)
        );
        if (Directory.Exists(switchFormsDir))
            Directory.Delete(switchFormsDir, recursive: true);
        Directory.CreateDirectory(switchFormsDir);

        RuntimeExporter.ExportSwitchForms(switchFormsDir);

        // Alle SwitchForms-Dateien für Forward-Declarations
        var switchFormsFilesForFwd = Directory.GetFiles(switchFormsDir, "*.cs").ToList();

        // App-Quelldateien
        var appSourceFiles = reader.SourceFiles.ToList();

        // Forward-Declarations: SwitchForms (alle) + App-Dateien
        var allForFwd = switchFormsFilesForFwd.Concat(appSourceFiles).ToList();
        var forwardPath = Path.Combine(_buildDir, "_forward.h");
        WriteForwardDeclarations(allForFwd, forwardPath);

        // Nur App-Dateien transpilieren — SwitchForms-Implementierungen kommen aus switchforms.h
        var transpiledFiles = appSourceFiles
            .Where(f => !s_switchFormsSkipTranspile.Contains(Path.GetFileName(f)))
            .ToList();

        // allHeaders nur aus tatsächlich transpilierten Dateien
        var allHeaders = transpiledFiles
            .Select(f => Path.GetFileNameWithoutExtension(f) + ".h")
            .ToList();

        // Schritt 3: Transpilieren
        Log("Transpiling...");

        foreach (var csFile in transpiledFiles)
        {
            var source = File.ReadAllText(csFile);
            var baseName = Path.GetFileNameWithoutExtension(csFile);

            Log("Transpiling " + baseName + ".cs...");

            var hTranspiler = new CSharpToC(CSharpToC.TranspileMode.HeaderOnly);
            var hContent = WrapHeader(baseName,
                "#pragma once\n#include \"_forward.h\"\n\n"
                + hTranspiler.Transpile(source));
            File.WriteAllText(Path.Combine(_buildDir, baseName + ".h"), hContent);

            var allIncludes = string.Join("\n", allHeaders.Select(h => $"#include \"{h}\""));
            var cTranspiler = new CSharpToC(CSharpToC.TranspileMode.Implementation);
            var cContent = "#include <stdlib.h>\n"
                         + allIncludes + "\n\n"
                         + cTranspiler.Transpile(source);
            var cPath = Path.Combine(_buildDir, baseName + ".c");
            File.WriteAllText(cPath, cContent);
            cFiles.Add(cPath);
        }

        // Schritt 4: Entry-Point
        var appClass = FindSwitchAppClass(appSourceFiles) ?? _config.MainClass;
        var appHeaderFile = FindHeaderForClass(appSourceFiles, appClass) ?? (appClass + ".h");
        var mainPath = EntryPointGenerator.Write(_buildDir, appClass, appHeaderFile, allHeaders);
        cFiles.Add(mainPath);

        // Schritt 5: Custom-Header aus Projektverzeichnis kopieren
        foreach (var hFile in Directory.GetFiles(_projectDir, "*.h"))
        {
            var dest = Path.Combine(_buildDir, Path.GetFileName(hFile));
            File.Copy(hFile, dest, overwrite: true);
            Log("Custom header: " + Path.GetFileName(hFile));
        }

        // Schritt 6: Kompilieren
        Log("Compiling...");
        var elfPath = Path.Combine(_buildDir, _config.Name + ".elf");
        new CCompiler().Compile(cFiles, elfPath, _buildDir, _projectDir);

        // Schritt 7: NACP
        var nacpPath = Path.Combine(_buildDir, _config.Name + ".nacp");
        new NacpBuilder().Build(nacpPath, _config.Name, _config.Author, _config.Version);

        // Schritt 8: NRO
        var nroPath = Path.Combine(_projectDir, _config.Name + ".nro");
        var iconPath = _config.Icon != null ? Path.Combine(_projectDir, _config.Icon) : null;
        new NroBuilder().Build(elfPath, nroPath, nacpPath, iconPath);

        Log("Done! Output: " + nroPath);
    }

    private static void Log(string msg) => Console.WriteLine("[CS2SX] " + msg);

    private static void WriteForwardDeclarations(IReadOnlyList<string> sourceFiles, string outputPath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#pragma once");

        // System-Includes
        sb.AppendLine("#include <switch.h>");
        sb.AppendLine("#include <stdlib.h>");
        sb.AppendLine("#include <string.h>");
        sb.AppendLine("#include <stdio.h>");
        sb.AppendLine("#include <stdbool.h>");
        sb.AppendLine();

        // Action-Typ
        sb.AppendLine("typedef void (*Action)(void*);");
        sb.AppendLine();

        // SwitchForms-Typen VOR switchapp.h forward-declaren
        sb.AppendLine("typedef struct Control    Control;");
        sb.AppendLine("typedef struct Form       Form;");
        sb.AppendLine("typedef struct Label      Label;");
        sb.AppendLine("typedef struct Button     Button;");
        sb.AppendLine("typedef struct ProgressBar ProgressBar;");
        sb.AppendLine("typedef struct SwitchApp  SwitchApp;");
        sb.AppendLine();

        // switchapp.h inkl. switchforms.h — jetzt sind alle Typen forward-declared
        // switchforms.h definiert bereits ALLE List-Makros und ihre Instantiierungen
        sb.AppendLine("#include \"switchapp.h\"");
        sb.AppendLine();

        // KEINE List-Makros oder Instantiierungen hier — switchforms.h hat sie bereits!

        // _cs2sx_strbuf extern
        sb.AppendLine("extern char _cs2sx_strbuf[512];");
        sb.AppendLine();

        // Forward-Declarations nur für App-eigene Klassen (nicht SwitchForms-Typen)
        var alreadyDeclared = new HashSet<string>(StringComparer.Ordinal)
    {
        "Control", "Form", "Label", "Button", "ProgressBar", "SwitchApp",
    };

        foreach (var csFile in sourceFiles)
        {
            var source = File.ReadAllText(csFile);
            var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source);
            foreach (var cls in tree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>())
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
        return "#ifndef " + guard + "\n"
             + "#define " + guard + "\n\n"
             + content + "\n\n"
             + "#endif\n";
    }

    private static string? FindSwitchAppClass(IReadOnlyList<string> sourceFiles)
    {
        foreach (var file in sourceFiles)
        {
            var source = File.ReadAllText(file);
            var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source);
            foreach (var cls in tree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>())
            {
                var baseType = cls.BaseList?.Types.FirstOrDefault()?.ToString().Trim();
                if (baseType == "SwitchApp")
                    return cls.Identifier.Text;
            }
        }
        return null;
    }

    private static string? FindHeaderForClass(IReadOnlyList<string> sourceFiles, string className)
    {
        foreach (var file in sourceFiles)
        {
            var source = File.ReadAllText(file);
            if (source.Contains("class " + className))
                return Path.GetFileNameWithoutExtension(file) + ".h";
        }
        return null;
    }

    private static void WriteSwitchformsC(string path)
    {
        using var w = new StreamWriter(path, append: false, encoding: System.Text.Encoding.UTF8);
        w.WriteLine("#include \"_forward.h\"");
        w.WriteLine();
        w.WriteLine("char _cs2sx_strbuf[512];");
    }
}