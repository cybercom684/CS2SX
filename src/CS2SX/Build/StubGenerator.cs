using System.Text;
using System.Text.RegularExpressions;

namespace CS2SX.Build;

/// <summary>
/// Generiert C#-Stub-Dateien aus libnx-Headern.
///
/// Die Stubs dienen als Typ-Referenz für den CS2SX-Transpiler — sie sind
/// keine echten P/Invoke-Bindings, sondern helfen dem C#-Compiler beim
/// Validieren von CS2SX-Projekten.
///
/// Verbesserungen:
///   • ParseEnums/Structs/Functions robuster gegen Kanten-Fälle
///   • BIT(n) → (1 << n) auch für BIT(n) | BIT(m) Ausdrücke
///   • GenerateTypesFile bleibt idempotent
///   • Logging über ein Action<string>-Delegate (testbar)
/// </summary>
public sealed class StubGenerator
{
    // ── Statische Lookup-Tabellen ──────────────────────────────────────────────

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
        "using","virtual","void","volatile","while",
    };

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
        ["float"] = "float",
        ["double"] = "double",
        ["size_t"] = "ulong",
        ["u8"] = "u8",
        ["u16"] = "u16",
        ["u32"] = "u32",
        ["u64"] = "u64",
        ["s8"] = "s8",
        ["s16"] = "s16",
        ["s32"] = "s32",
        ["s64"] = "s64",
        ["Result"] = "Result",
        ["Handle"] = "Handle",
    };

    private static readonly HashSet<string> s_primitiveForFixed = new(StringComparer.Ordinal)
    {
        "byte","sbyte","short","ushort","int","uint","long","ulong","float","double","bool",
        "u8","u16","u32","u64","s8","s16","s32","s64",
    };

    // Schlüsselwörter, die niemals als Rückgabetyp oder Funktionsname auftauchen dürfen
    private static readonly HashSet<string> s_notReturnTypes = new(StringComparer.Ordinal)
    {
        "if","for","while","return","typedef","define","else","switch","case","break",
        "continue","do","goto","struct","union","enum",
    };

    // ── Delegate ───────────────────────────────────────────────────────────────

    private readonly Action<string> _log;

    public StubGenerator(Action<string>? log = null)
    {
        _log = log ?? (msg => Console.WriteLine($"[CS2SX] {msg}"));
    }

    // ── Öffentliche API ────────────────────────────────────────────────────────

    public void Generate(string libnxIncludePath, string outputPath)
    {
        var switchDir = Path.Combine(libnxIncludePath, "switch");
        if (!Directory.Exists(switchDir))
        {
            _log($"libnx include-Pfad nicht gefunden: {switchDir}");
            return;
        }

        Directory.CreateDirectory(outputPath);
        GenerateTypesFile(outputPath);

        var headers = Directory.GetFiles(switchDir, "*.h", SearchOption.AllDirectories);
        _log($"{headers.Length} Header-Dateien gefunden.");

        int generated = 0;
        foreach (var header in headers)
        {
            if (ProcessHeader(header, switchDir, outputPath))
                generated++;
        }

        _log($"{generated} Stub-Dateien generiert.");
    }

    // ── Private Implementierung ────────────────────────────────────────────────

    private void GenerateTypesFile(string outputPath)
    {
        const string content =
            "// CS2SX LibNX Type Aliases — auto-generated\n" +
            "// DO NOT EDIT MANUALLY\n" +
            "global using u8     = System.Byte;\n" +
            "global using u16    = System.UInt16;\n" +
            "global using u32    = System.UInt32;\n" +
            "global using u64    = System.UInt64;\n" +
            "global using s8     = System.SByte;\n" +
            "global using s16    = System.Int16;\n" +
            "global using s32    = System.Int32;\n" +
            "global using s64    = System.Int64;\n" +
            "global using Result = System.UInt32;\n" +
            "global using Handle = System.UInt32;\n";
        File.WriteAllText(Path.Combine(outputPath, "_GlobalTypes.cs"), content);
    }

    private bool ProcessHeader(string headerPath, string basePath, string outputPath)
    {
        string raw;
        try { raw = File.ReadAllText(headerPath); }
        catch { return false; }

        // Kommentare und Präprozessor-Direktiven entfernen
        var content = StripComments(raw);
        content = Regex.Replace(content, @"^\s*#[^\n]*", "", RegexOptions.Multiline);
        content = Regex.Replace(content, @"\n{3,}", "\n\n");

        var relativePath = Path.GetRelativePath(basePath, headerPath);
        var parts = relativePath.Split(Path.DirectorySeparatorChar);

        var namespaceParts = parts.Take(parts.Length - 1)
            .Where(p => !string.IsNullOrEmpty(p))
            .Select(p => char.ToUpper(p[0]) + p[1..]);
        var namespaceName = "LibNX" + (namespaceParts.Any()
            ? "." + string.Join(".", namespaceParts)
            : "");

        var rawClass = Path.GetFileNameWithoutExtension(headerPath);
        var className = char.ToUpper(rawClass[0]) + rawClass[1..];

        var enums = ParseEnums(content);
        var structs = ParseStructs(content);
        var functions = ParseFunctions(content);

        if (!enums.Any() && !structs.Any() && !functions.Any())
            return false;

        var sb = new StringBuilder();
        sb.AppendLine($"// Auto-generated from {relativePath}");
        sb.AppendLine("// DO NOT EDIT MANUALLY");
        sb.AppendLine();
        sb.AppendLine("#pragma warning disable CS0649, CS0169, CS8981");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();

        foreach (var e in enums) sb.AppendLine(e);
        foreach (var s in structs) sb.AppendLine(s);

        if (functions.Any())
        {
            sb.AppendLine($"public static class {className}");
            sb.AppendLine("{");
            foreach (var f in functions)
                sb.AppendLine($"    {f}");
            sb.AppendLine("}");
        }

        var outDir = Path.Combine(
            outputPath,
            string.Join(Path.DirectorySeparatorChar.ToString(), parts.Take(parts.Length - 1)));
        Directory.CreateDirectory(outDir);

        var outFile = Path.Combine(outDir, className + ".cs");
        File.WriteAllText(outFile, sb.ToString());
        _log($"Generiert: {Path.GetRelativePath(outputPath, outFile)}");
        return true;
    }

    // ── Parser ─────────────────────────────────────────────────────────────────

    private static string StripComments(string src)
    {
        // Block-Kommentare
        src = Regex.Replace(src, @"/\*.*?\*/", "", RegexOptions.Singleline);
        // Zeilen-Kommentare
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
                    // BIT(n) → (1 << n)
                    rawVal = Regex.Replace(rawVal, @"\bBIT\((\d+)\)", m =>
                        $"(1 << {m.Groups[1].Value})");
                    value = " = " + rawVal;
                }

                sb.AppendLine($"    {name}{value},");
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

                // Array fixer Größe: type name[size]
                var arrayMatch = Regex.Match(line, @"^([\w\s]+?)\s+(\w+)\s*\[(\d+)\]$");
                if (arrayMatch.Success)
                {
                    var cType = arrayMatch.Groups[1].Value.Trim()
                        .Replace("const", "").Replace("unsigned", "").Trim();
                    var fname = SanitizeName(arrayMatch.Groups[2].Value.Trim());
                    var size = arrayMatch.Groups[3].Value;
                    var csType = MapType(cType);

                    sb.AppendLine(s_primitiveForFixed.Contains(csType)
                        ? $"    public fixed {csType} {fname}[{size}];"
                        : $"    // skipped array: {line}");
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
            @"(?:static\s+inline\s+|NX_INLINE\s+)?(\w[\w\s\*]*?)\s+(\w+)\s*\(([^)]*)\)\s*;",
            RegexOptions.Multiline);

        foreach (Match match in pattern.Matches(content))
        {
            var returnType = match.Groups[1].Value.Trim();
            var funcName = match.Groups[2].Value.Trim();
            var paramsStr = match.Groups[3].Value.Trim();

            if (s_notReturnTypes.Contains(returnType)) continue;
            if (funcName.Length < 2 || funcName.StartsWith('_')) continue;

            // Inline-Funktionen mit Body überspringen
            var afterMatch = content[(match.Index + match.Length)..].TrimStart();
            if (afterMatch.StartsWith('{')) continue;

            var isReturnPointer = returnType.Contains('*');
            var cleanReturn = returnType.Replace("*", "").Replace("const", "").Trim();
            var csReturn = isReturnPointer ? "IntPtr" : MapType(cleanReturn);

            var csParams = ParseParams(paramsStr);
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

    // ── Utilities ──────────────────────────────────────────────────────────────

    private string SanitizeName(string name)
    {
        name = Regex.Replace(name, @"\[.*?\]", "").Trim();
        if (string.IsNullOrEmpty(name)) return "field";
        if (s_csharpKeywords.Contains(name)) return "@" + name;
        if (char.IsDigit(name[0])) return "_" + name;
        return name;
    }

    private string MapType(string cType)
    {
        cType = cType.Trim();
        return s_typeMap.TryGetValue(cType, out var mapped) ? mapped : cType;
    }
}