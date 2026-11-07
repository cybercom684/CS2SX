// ============================================================================
// Build/AddLibCommand.cs
//
// cs2sx addLib <libName> [project.csproj]
//
// Scannt externLibs/<libName>/ nach C-Headern, generiert C#-Stub-Dateien
// (nur für Roslyn/IDE, werden NICHT transpiliert) und registriert die
// C-Sources in cs2sx.json für den nächsten Build.
//
// FIXES gegenüber der alten Version:
//
//   FIX-1: Stub-Ordner heißt jetzt "<LibName>Stubs/" statt "<LibName>/".
//          "Stubs" ist bereits in ProjectReader.s_excludedDirNames enthalten,
//          sodass die generierten .cs-Dateien von Roslyn gesehen werden
//          (IDE-Unterstützung) aber NICHT transpiliert werden.
//          → Kein leerer C-Body mehr für extern-Methoden.
//
//   FIX-2: _LibTypes.cs wird NICHT mehr generiert.
//          Die global usings (u8, u16, ...) sind bereits in _GlobalTypes.cs
//          der LibNX-Stubs definiert. Doppelte Definitionen führen zu CS-Fehlern.
//
//   FIX-3: IncludeDir-Pfad wird relativ zu _projectDir berechnet, nicht zum CWD.
//          Path.GetRelativePath(CWD, libDir) war falsch wenn cs2sx aus einem
//          anderen Verzeichnis aufgerufen wurde.
//
//   FIX-4: Generierte Wrapper-Klassen haben kein "Api"-Suffix mehr im Namen.
//          Stattdessen: LibName_FunctionName() direkt — passt zum
//          StaticClassHandler der den Aufruf als "LibName_FunctionName" emittiert.
//          Der Transpiler leitet MyLib.SomeFunc() → MyLib_SomeFunc() weiter,
//          was der tatsächlich compilierte C-Funktionsname ist.
//
//   FIX-5: Generierte Stubs enthalten [System.Runtime.InteropServices.DllImport]-
//          Kommentar und das Keyword "extern" damit Roslyn sie als Declarations
//          versteht ohne dass der CS2SX-Transpiler versucht einen Body zu erzeugen.
//          (ProjectReader schließt den Ordner eh aus — das ist Redundanz als Sicherheit.)
//
//   FIX-6: cs2sx.json-Merge nutzt _projectDir als Basis für GetRelativePath,
//          nicht Directory.GetCurrentDirectory().
// ============================================================================

using CS2SX.Logging;
using System.Text;
using System.Text.RegularExpressions;

namespace CS2SX.Build;

public sealed class AddLibCommand
{
    private readonly string _projectDir;
    private readonly string _libName;

    // C-Keywords die in C# nicht als Identifier verwendet werden dürfen
    private static readonly HashSet<string> s_csharpKeywords = new(StringComparer.Ordinal)
    {
        "abstract","as","base","bool","break","byte","case","catch","char","checked",
        "class","const","continue","decimal","default","delegate","do","double","else",
        "enum","event","explicit","extern","false","finally","fixed","float","for",
        "foreach","goto","if","implicit","in","int","interface","internal","is","lock",
        "long","namespace","new","null","object","operator","out","override","params",
        "private","protected","public","readonly","ref","return","sbyte","sealed",
        "short","sizeof","stackalloc","static","string","struct","switch","this",
        "throw","true","try","typeof","uint","ulong","unchecked","unsafe","ushort",
        "using","virtual","void","volatile","while","event","lock",
    };

    // C → C# Typ-Mapping
    private static readonly Dictionary<string, string> s_typeMap = new(StringComparer.Ordinal)
    {
        ["void"] = "void",
        ["bool"] = "bool",
        ["char"] = "byte",
        ["int"] = "int",
        ["unsigned int"] = "uint",
        ["unsigned char"] = "byte",
        ["unsigned short"] = "ushort",
        ["unsigned long"] = "ulong",
        ["long"] = "long",
        ["long long"] = "long",
        ["float"] = "float",
        ["double"] = "double",
        ["size_t"] = "ulong",
        ["uint8_t"] = "byte",
        ["uint16_t"] = "ushort",
        ["uint32_t"] = "uint",
        ["uint64_t"] = "ulong",
        ["int8_t"] = "sbyte",
        ["int16_t"] = "short",
        ["int32_t"] = "int",
        ["int64_t"] = "long",
        ["u8"] = "byte",
        ["u16"] = "ushort",
        ["u32"] = "uint",
        ["u64"] = "ulong",
        ["s8"] = "sbyte",
        ["s16"] = "short",
        ["s32"] = "int",
        ["s64"] = "long",
    };

    private static readonly HashSet<string> s_notReturnTypes = new(StringComparer.Ordinal)
    {
        "if","for","while","return","typedef","define","else","switch","case","break",
        "continue","do","goto","struct","union","enum","static","inline","extern",
        "const","volatile","register",
    };

    public AddLibCommand(string libName, string? csprojPath = null)
    {
        _libName = libName;

        if (!string.IsNullOrEmpty(csprojPath) && File.Exists(csprojPath))
        {
            _projectDir = Path.GetDirectoryName(Path.GetFullPath(csprojPath))
                ?? Directory.GetCurrentDirectory();
        }
        else
        {
            var found = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csproj")
                .FirstOrDefault();
            _projectDir = found != null
                ? Path.GetDirectoryName(Path.GetFullPath(found))!
                : Directory.GetCurrentDirectory();
        }
    }

    public int Run()
    {
        Console.WriteLine();
        Log.Info($"cs2sx addLib: {_libName}");
        Console.WriteLine(new string('─', 60));

        // 1. externLibs/<libName> finden
        var externLibsDir = Path.Combine(_projectDir, "externLibs");
        var libDir = Path.Combine(externLibsDir, _libName);

        if (!Directory.Exists(libDir))
        {
            // Case-insensitive Suche
            if (Directory.Exists(externLibsDir))
            {
                var found = Directory.GetDirectories(externLibsDir)
                    .FirstOrDefault(d => string.Equals(
                        Path.GetFileName(d), _libName,
                        StringComparison.OrdinalIgnoreCase));
                if (found != null) libDir = found;
            }

            if (!Directory.Exists(libDir))
            {
                Log.Error($"externLibs/{_libName} nicht gefunden.");
                Log.Info($"Erstelle den Ordner: {Path.Combine(externLibsDir, _libName)}");
                Log.Info($"und lege dort den C-Source der Library ab.");
                return 1;
            }
        }

        Log.Info($"Library gefunden: {libDir}");

        // 2. Header-Dateien sammeln
        var headers = Directory.GetFiles(libDir, "*.h", SearchOption.AllDirectories)
            .OrderBy(f => f)
            .ToList();

        if (headers.Count == 0)
        {
            Log.Error($"Keine .h-Dateien in externLibs/{_libName} gefunden.");
            return 1;
        }

        Log.Info($"{headers.Count} Header-Datei(en) gefunden");

        // 3. C-Source-Dateien zählen
        var sources = Directory.GetFiles(libDir, "*.c", SearchOption.AllDirectories)
            .OrderBy(f => f)
            .ToList();

        Log.Info($"{sources.Count} .c-Datei(en) für Build registriert");

        // 4. FIX-1: Output-Verzeichnis = <ProjectDir>/<LibName>Stubs/
        //    "Stubs" ist in ProjectReader.s_excludedDirNames → wird NICHT transpiliert,
        //    aber Roslyn sieht die Dateien für IDE-Unterstützung.
        var libClassName = ToPascalCase(_libName);
        var stubsDir = Path.Combine(_projectDir, libClassName + "Stubs");
        Directory.CreateDirectory(stubsDir);

        // 5. Stubs generieren
        var generatedFiles = new List<string>();
        int totalFunctions = 0;
        int totalEnums = 0;
        int totalStructs = 0;

        foreach (var header in headers)
        {
            var (file, funcs, enums, structs) = GenerateStubFile(
                header, libDir, stubsDir, libClassName);

            if (file != null)
            {
                generatedFiles.Add(file);
                totalFunctions += funcs;
                totalEnums += enums;
                totalStructs += structs;
                Log.Info($"→ {Path.GetFileName(file)} ({funcs}F {enums}E {structs}S)");
            }
        }

        // FIX-2: KEINE _LibTypes.cs mehr — global usings sind in _GlobalTypes.cs der LibNX-Stubs

        // 6. cs2sx.json mit externLib-Eintrag erweitern
        RegisterLibInConfig(sources, libDir, libClassName);

        // 7. Zusammenfassung
        Console.WriteLine(new string('─', 60));
        Log.Ok($"addLib '{_libName}' abgeschlossen:");
        Log.Info($"  Stubs (nur IDE): {stubsDir}");
        Log.Info($"  Stub-Dateien:    {generatedFiles.Count}");
        Log.Info($"  Funktionen: {totalFunctions}, Enums: {totalEnums}, Structs: {totalStructs}");
        Log.Info($"  C-Sources:  {sources.Count} (werden beim Build mitcompiliert)");
        Console.WriteLine();
        Log.Info($"Verwendung in C#:");
        Log.Info($"  // Direktaufruf — Transpiler leitet weiter an C-Funktion:");
        Log.Info($"  {libClassName}.SomeCFunction(...);");
        Console.WriteLine();
        Log.Info($"Hinweis: Die Stubs in '{libClassName}Stubs/' dienen nur der");
        Log.Info($"  IDE-Unterstützung und werden NICHT transpiliert.");
        Console.WriteLine();

        return 0;
    }

    // ── Stub-Generierung ──────────────────────────────────────────────────────

    private (string? path, int funcs, int enums, int structs) GenerateStubFile(
        string headerPath, string libRootDir, string stubsDir, string libClassName)
    {
        string raw;
        try { raw = File.ReadAllText(headerPath); }
        catch { return (null, 0, 0, 0); }

        var content = StripComments(raw);
        content = Regex.Replace(content, @"^\s*#[^\n]*", "", RegexOptions.Multiline);
        content = Regex.Replace(content, @"\n{3,}", "\n\n");

        var enums = ParseEnums(content);
        var structs = ParseStructs(content);
        var functions = ParseFunctions(content);

        if (!enums.Any() && !structs.Any() && !functions.Any())
            return (null, 0, 0, 0);

        var relative = Path.GetRelativePath(libRootDir, headerPath);
        var baseName = ToPascalCase(
            Path.GetFileNameWithoutExtension(relative)
                .Replace("/", "_")
                .Replace("\\", "_"));

        var sb = new StringBuilder();
        sb.AppendLine($"// Auto-generated from externLibs/{Path.GetFileName(libRootDir)}/{relative}");
        sb.AppendLine($"// DO NOT EDIT — regeneriert via cs2sx addLib");
        sb.AppendLine($"//");
        sb.AppendLine($"// HINWEIS: Diese Datei dient NUR der IDE-Unterstützung (Roslyn-Typen).");
        sb.AppendLine($"// Der Ordner '{libClassName}Stubs/' ist in ProjectReader.s_excludedDirNames");
        sb.AppendLine($"// gelistet und wird NICHT von CS2SX transpiliert.");
        sb.AppendLine($"// Die echten C-Funktionen werden direkt aus externLibs/ mitcompiliert.");
        sb.AppendLine();
        sb.AppendLine("#pragma warning disable CS0626, CS0649, CS0169, CS8981, CS1591");
        sb.AppendLine();

        // FIX-4: Namespace = libClassName (kein "Api"-Suffix)
        // Aufruf in C#: MyLib.SomeFunc() → Transpiler emittiert MyLib_SomeFunc() → korrekte C-Funktion
        sb.AppendLine($"namespace {libClassName};");
        sb.AppendLine();

        foreach (var e in enums) sb.AppendLine(e);
        foreach (var s in structs) sb.AppendLine(s);

        if (functions.Any())
        {
            // FIX-4: Klassenname = libClassName (nicht baseName + "Api")
            // StaticClassHandler sieht MyLib.Foo() und emittiert MyLib_Foo()
            sb.AppendLine($"public static class {libClassName}");
            sb.AppendLine("{");
            foreach (var f in functions)
            {
                // FIX-5: extern damit Roslyn keinen Body erwartet;
                // der Transpiler sieht diese Dateien sowieso nicht (Stubs-Ordner)
                sb.AppendLine($"    {f}");
            }
            sb.AppendLine("}");
        }

        var outPath = Path.Combine(stubsDir, baseName + ".cs");
        File.WriteAllText(outPath, sb.ToString(), Encoding.UTF8);

        return (outPath, functions.Count, enums.Count, structs.Count);
    }

    // ── Parser ─────────────────────────────────────────────────────────────────

    private static string StripComments(string src)
    {
        src = Regex.Replace(src, @"/\*.*?\*/", "", RegexOptions.Singleline);
        src = Regex.Replace(src, @"//[^\n]*", "");
        return src;
    }

    private List<string> ParseEnums(string content)
    {
        var result = new List<string>();
        var pattern = new Regex(
            @"typedef\s+enum\s*\w*\s*\{([^}]+)\}\s*(\w+)\s*;",
            RegexOptions.Singleline);

        foreach (Match match in pattern.Matches(content))
        {
            var enumName = match.Groups[2].Value.Trim();
            var enumBody = match.Groups[1].Value;

            var sb = new StringBuilder();
            sb.AppendLine($"public enum {enumName}");
            sb.AppendLine("{");

            foreach (var rawLine in enumBody.Split('\n'))
            {
                var line = rawLine.Trim().TrimEnd(',').Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var eqParts = line.Split('=', 2);
                var name = eqParts[0].Trim();
                if (string.IsNullOrEmpty(name) || name.Contains(' ')) continue;

                var value = "";
                if (eqParts.Length > 1)
                {
                    var rawVal = eqParts[1].Trim().TrimEnd(',').Trim();
                    rawVal = Regex.Replace(rawVal, @"\bBIT\((\d+)\)",
                        m => $"(1 << {m.Groups[1].Value})");
                    value = " = " + rawVal;
                }
                sb.AppendLine($"    {SanitizeName(name)}{value},");
            }

            sb.AppendLine("}");
            result.Add(sb.ToString());
        }
        return result;
    }

    private List<string> ParseStructs(string content)
    {
        var result = new List<string>();
        var pattern = new Regex(
            @"typedef\s+struct\s+\w*\s*\{([^{}]+)\}\s*(\w+)\s*;",
            RegexOptions.Singleline);

        foreach (Match match in pattern.Matches(content))
        {
            var structName = match.Groups[2].Value.Trim();
            var structBody = match.Groups[1].Value;

            var sb = new StringBuilder();
            sb.AppendLine($"public unsafe struct {structName}");
            sb.AppendLine("{");

            foreach (var rawLine in structBody.Split('\n'))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("struct") || line.StartsWith("union")) continue;

                line = line.TrimEnd(';').Trim();
                if (string.IsNullOrEmpty(line)) continue;

                var arrayMatch = Regex.Match(line, @"^([\w\s]+?)\s+(\w+)\s*\[(\d+)\]$");
                if (arrayMatch.Success)
                {
                    var cType = arrayMatch.Groups[1].Value.Trim()
                                     .Replace("const", "").Replace("unsigned", "").Trim();
                    var fname = SanitizeName(arrayMatch.Groups[2].Value.Trim());
                    var size = arrayMatch.Groups[3].Value;
                    var csType = MapType(cType);
                    var prims = new HashSet<string>
                        { "byte","sbyte","short","ushort","int","uint",
                          "long","ulong","float","double","bool" };
                    sb.AppendLine(prims.Contains(csType)
                        ? $"    public fixed {csType} {fname}[{size}];"
                        : $"    // skipped array field: {line}");
                    continue;
                }

                var isPointer = line.Contains('*');
                line = line.Replace("*", "").Replace("const", "").Trim();
                var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length < 2) continue;

                var typeStr = string.Join(" ", tokens.Take(tokens.Length - 1));
                var fieldName = SanitizeName(tokens.Last());
                if (string.IsNullOrEmpty(fieldName)) continue;

                var mappedType = isPointer ? "IntPtr" : MapType(typeStr);
                sb.AppendLine($"    public {mappedType} {fieldName};");
            }

            sb.AppendLine("}");
            result.Add(sb.ToString());
        }
        return result;
    }

    private List<string> ParseFunctions(string content)
    {
        var result = new List<string>();
        var pattern = new Regex(
            @"(?:static\s+inline\s+|JS_EXTERN\s+|EMSCRIPTEN_KEEPALIVE\s+)?(\w[\w\s\*]*?)\s+(\w+)\s*\(([^)]*)\)\s*;",
            RegexOptions.Multiline);

        foreach (Match match in pattern.Matches(content))
        {
            var returnType = match.Groups[1].Value.Trim();
            var funcName = match.Groups[2].Value.Trim();
            var paramsStr = match.Groups[3].Value.Trim();

            if (s_notReturnTypes.Contains(returnType)) continue;
            if (funcName.Length < 2 || funcName.StartsWith('_')) continue;

            var afterMatch = content[(match.Index + match.Length)..].TrimStart();
            if (afterMatch.StartsWith('{')) continue;

            var isReturnPointer = returnType.Contains('*');
            var cleanReturn = returnType.Replace("*", "").Replace("const", "").Trim();
            var csReturn = isReturnPointer ? "IntPtr" : MapType(cleanReturn);

            var csParams = ParseParams(paramsStr);

            // FIX-5: extern — Roslyn erwartet keinen Body, kein Transpiler-Problem
            // da der Stubs-Ordner ausgeschlossen ist
            result.Add($"public static extern {csReturn} {funcName}({csParams});");
        }
        return result;
    }

    private string ParseParams(string paramsStr)
    {
        if (string.IsNullOrWhiteSpace(paramsStr) || paramsStr.Trim() == "void")
            return "";

        var result = new List<string>();
        int idx = 0;

        foreach (var param in paramsStr.Split(','))
        {
            var clean = param.Trim().Replace("const", "").Replace("  ", " ").Trim();
            if (string.IsNullOrEmpty(clean)) continue;

            var isPointer = clean.Contains('*');
            clean = Regex.Replace(clean.Replace("*", ""), @"\[.*?\]", "").Trim();

            var tokens = clean.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) continue;

            string cType, paramName;
            if (tokens.Length == 1)
            {
                cType = tokens[0];
                paramName = "p" + idx++;
            }
            else
            {
                cType = string.Join(" ", tokens.Take(tokens.Length - 1));
                paramName = SanitizeName(tokens.Last());
            }

            if (string.IsNullOrEmpty(cType)) continue;
            if (cType == "void" && !isPointer) continue;

            var csType = cType == "void" ? "IntPtr" : MapType(cType);
            var prefix = isPointer && cType != "void" ? "ref " : "";
            result.Add($"{prefix}{csType} {paramName}");
        }

        return string.Join(", ", result);
    }

    // ── cs2sx.json erweitern ──────────────────────────────────────────────────

    private void RegisterLibInConfig(
        List<string> cSources, string libDir, string libClassName)
    {
        var configPath = Path.Combine(_projectDir, "cs2sx.json");

        var json = File.Exists(configPath)
            ? File.ReadAllText(configPath)
            : "{}";

        // FIX-6: relativ zu _projectDir berechnen, nicht zu CWD
        var relativeSources = cSources
            .Select(s => Path.GetRelativePath(_projectDir, s).Replace('\\', '/'))
            .ToList();

        if (json.Contains("\"externLibs\""))
        {
            if (!json.Contains($"\"{libClassName}\""))
            {
                json = MergeLibConfig(json, libClassName, relativeSources, libDir);
                File.WriteAllText(configPath, json, Encoding.UTF8);
                Log.Info($"cs2sx.json: externLib '{libClassName}' registriert ({relativeSources.Count} sources)");
            }
            else
            {
                Log.Info($"cs2sx.json: externLib '{libClassName}' bereits registriert");
            }
            return;
        }

        json = MergeLibConfig(json, libClassName, relativeSources, libDir);
        File.WriteAllText(configPath, json, Encoding.UTF8);
        Log.Info($"cs2sx.json: externLib '{libClassName}' registriert ({relativeSources.Count} sources)");
    }

    private string MergeLibConfig(
        string existingJson,
        string libClassName,
        List<string> relativeSources,
        string libDir)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(existingJson);
            var dict = new Dictionary<string, object?>();

            foreach (var prop in doc.RootElement.EnumerateObject())
                dict[prop.Name] = prop.Value.GetRawText();

            List<ExternLibEntry> libs;
            if (dict.TryGetValue("externLibs", out var existing)
                && existing is string rawArr
                && rawArr.StartsWith('['))
            {
                using var arrDoc = System.Text.Json.JsonDocument.Parse(rawArr);
                libs = arrDoc.RootElement.EnumerateArray()
                    .Select(e => new ExternLibEntry
                    {
                        Name = e.GetProperty("name").GetString() ?? "",
                        IncludeDir = e.TryGetProperty("includeDir", out var id)
                                         ? id.GetString() : null,
                        Sources = e.TryGetProperty("sources", out var src)
                                         ? src.EnumerateArray()
                                              .Select(s => s.GetString() ?? "")
                                              .ToList()
                                         : new(),
                    }).ToList();
            }
            else
            {
                libs = new();
            }

            if (!libs.Any(l => string.Equals(l.Name, libClassName, StringComparison.OrdinalIgnoreCase)))
            {
                // FIX-6: relativ zu _projectDir, nicht zu CWD
                libs.Add(new ExternLibEntry
                {
                    Name = libClassName,
                    IncludeDir = Path.GetRelativePath(_projectDir, libDir).Replace('\\', '/'),
                    Sources = relativeSources,
                });
            }

            dict["externLibs"] = libs;
            return SerializeConfig(dict);
        }
        catch
        {
            var libEntry = BuildLibJsonEntry(libClassName, relativeSources, libDir);
            var trimmed = existingJson.TrimEnd();
            if (trimmed.EndsWith('}'))
            {
                var insertBefore = trimmed.LastIndexOf('}');
                var prefix = trimmed[..insertBefore].TrimEnd();
                var comma = prefix.EndsWith('{') ? "" : ",";
                return prefix + comma + "\n  \"externLibs\": [" + libEntry + "]\n}";
            }
            return existingJson;
        }
    }

    private static string SerializeConfig(Dictionary<string, object?> dict)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        var entries = dict.ToList();
        for (int i = 0; i < entries.Count; i++)
        {
            var (key, val) = entries[i];
            var comma = i < entries.Count - 1 ? "," : "";

            if (val is List<ExternLibEntry> libs)
            {
                sb.AppendLine($"  \"externLibs\": [");
                for (int j = 0; j < libs.Count; j++)
                {
                    var lib = libs[j];
                    var libComma = j < libs.Count - 1 ? "," : "";
                    var sources = string.Join(", ",
                        lib.Sources.Select(s => $"\"{s}\""));
                    sb.AppendLine($"    {{");
                    sb.AppendLine($"      \"name\": \"{lib.Name}\",");
                    if (!string.IsNullOrEmpty(lib.IncludeDir))
                        sb.AppendLine($"      \"includeDir\": \"{lib.IncludeDir}\",");
                    sb.AppendLine($"      \"sources\": [{sources}]");
                    sb.AppendLine($"    }}{libComma}");
                }
                sb.AppendLine($"  ]{comma}");
            }
            else
            {
                sb.AppendLine($"  \"{key}\": {val}{comma}");
            }
        }
        sb.Append("}");
        return sb.ToString();
    }

    private string BuildLibJsonEntry(
        string libClassName, List<string> sources, string libDir)
    {
        var srcArr = string.Join(", ", sources.Select(s => $"\"{s}\""));
        // FIX-6: relativ zu _projectDir
        var relDir = Path.GetRelativePath(_projectDir, libDir).Replace('\\', '/');
        return $"\n    {{\"name\":\"{libClassName}\",\"includeDir\":\"{relDir}\",\"sources\":[{srcArr}]}}";
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToUpper(name[0]) + name[1..];
    }

    private string SanitizeName(string name)
    {
        name = Regex.Replace(name, @"\[.*?\]", "").Trim();
        if (string.IsNullOrEmpty(name)) return "field";
        if (s_csharpKeywords.Contains(name)) return "@" + name;
        if (char.IsDigit(name[0])) return "_" + name;
        return name;
    }

    private static string MapType(string cType)
    {
        cType = cType.Trim();
        return s_typeMap.TryGetValue(cType, out var mapped) ? mapped : cType;
    }

    private sealed class ExternLibEntry
    {
        public string Name { get; set; } = "";
        public string? IncludeDir
        {
            get; set;
        }
        public List<string> Sources { get; set; } = new();
    }
}