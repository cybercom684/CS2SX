// Datei: Core/TranspilerContext.cs
// FIX: WriteLineWithMapping ist jetzt in Benutzung — CurrentCFile wird
// in BuildPipeline korrekt gesetzt, sodass GCC-Zeilen auf C#-Zeilen
// zurückgemappt werden können.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CS2SX.Core;

public sealed class TranspilerContext
{
    // ── Output ────────────────────────────────────────────────────────────────

    public StringWriter Out
    {
        get;
    }

    // ── Roslyn Semantic Model ─────────────────────────────────────────────────

    public SemanticModel? SemanticModel
    {
        get; set;
    }

    // ── Diagnose-System ───────────────────────────────────────────────────────

    public DiagnosticReporter Diagnostics { get; } = new();

    // ── Klassen-Kontext ───────────────────────────────────────────────────────

    public string? CurrentJumpBuf
    {
        get; set;
    }
    public string? CurrentReturnBuffer
    {
        get; set;
    }
    public string CurrentClass { get; set; } = string.Empty;
    public string CurrentBaseType { get; set; } = string.Empty;

    public Dictionary<string, string> FieldTypes { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, string> BaseFieldTypes { get; } = new(StringComparer.Ordinal);

    // ── Methoden- und Property-Typen ──────────────────────────────────────────

    public Dictionary<string, string> MethodReturnTypes { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, string> PropertyTypes { get; } = new(StringComparer.Ordinal);
    public HashSet<string> EnumMembers { get; } = new(StringComparer.Ordinal);

    // ── Methoden-Kontext ──────────────────────────────────────────────────────

    public Dictionary<string, string> LocalTypes { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, string> ArrayLengths { get; } = new(StringComparer.Ordinal);

    // ── Value-type und VTable Registries ──────────────────────────────────────

    public HashSet<string> ValueTypeStructs { get; } = new(StringComparer.Ordinal);
    public HashSet<string> VTableTypes { get; } = new(StringComparer.Ordinal);
    public HashSet<string> InterfaceTypes { get; } = new(StringComparer.Ordinal);

    // ── Indentierung ──────────────────────────────────────────────────────────

    private int _indent;
    public string Tab => new(' ', _indent * 4);
    public void Indent() => _indent++;
    public void Dedent() => _indent--;

    // ── Zähler ────────────────────────────────────────────────────────────────

    public int TmpCounter
    {
        get; set;
    }
    public int TmpStringCounter
    {
        get; set;
    }

    // ── Zeilen-Tracking ───────────────────────────────────────────────────────

    public string CurrentFile { get; set; } = string.Empty;
    public int CurrentLine
    {
        get; set;
    }

    // FIX: CurrentCLine wird jetzt hochgezählt — für Diagnostic-Mapping nutzbar
    public int CurrentCLine { get; private set; } = 1;
    public string CurrentCFile { get; set; } = string.Empty;

    // ── Konstruktor ───────────────────────────────────────────────────────────

    public TranspilerContext(StringWriter writer)
    {
        Out = writer;
    }

    // ── Convenience ──────────────────────────────────────────────────────────

    public void WriteLine(string line)
    {
        Out.WriteLine(Tab + line);
        CurrentCLine++;
    }

    public void WriteRaw(string s) => Out.Write(s);

    /// FIX: WriteLineWithMapping wird jetzt verwendet wenn CurrentCFile gesetzt ist.
    /// Der Aufrufer (StatementWriter) setzt ctx.CurrentLine vor dem Schreiben.
    public void WriteLineWithMapping(string line, int csLine, string csSnippet)
    {
        if (!string.IsNullOrEmpty(CurrentCFile) && !string.IsNullOrEmpty(CurrentFile))
        {
            Diagnostics.RegisterSourceMapping(
                CurrentCFile, CurrentCLine,
                CurrentFile, csLine, csSnippet);
        }
        WriteLine(line);
    }

    public void ClearMethodContext()
    {
        LocalTypes.Clear();
        ArrayLengths.Clear();
        TmpCounter = 0;
        TmpStringCounter = 0;
        CurrentLine = 0;
        CurrentReturnBuffer = null;   // ← neu
    }

    public void ClearClassContext()
    {
        CurrentClass = string.Empty;
        CurrentBaseType = string.Empty;
        FieldTypes.Clear();
        BaseFieldTypes.Clear();
        MethodReturnTypes.Clear();
        PropertyTypes.Clear();
        EnumMembers.Clear();
        ClearMethodContext();
    }

    public string? LookupType(string name)
    {
        var trimmed = name.TrimStart('_');
        if (LocalTypes.TryGetValue(name, out var lt)) return lt;
        if (FieldTypes.TryGetValue(trimmed, out var ft)) return ft;
        return null;
    }

    public bool IsFieldAccess(string name) =>
        name.StartsWith('_') && !string.IsNullOrEmpty(CurrentClass);

    public string NextTmp(string prefix = "tmp") =>
        "_" + prefix + "_" + (TmpCounter++);

    public string NextStringBuf(int size = 512)
    {
        var name = "_sb" + (TmpStringCounter++);
        WriteLine($"char {name}[{size}];");
        return name;
    }

    public string PeekNextStringBufName() => "_sb" + TmpStringCounter;

    public string FormatDiagnostic(string message) =>
        string.IsNullOrEmpty(CurrentFile)
            ? message
            : $"{CurrentFile}({CurrentLine}): {message}";

    // ── Diagnose-Shortcuts ────────────────────────────────────────────────────

    public void Warn(string message, string? context = null) =>
        Diagnostics.Warn(CurrentFile, CurrentLine, message, context);

    public void Warn(SyntaxNode node, string message) =>
        Diagnostics.Warn(node, CurrentFile, message);

    // ── SemanticModel Helpers ─────────────────────────────────────────────────

    public string? GetSemanticType(SyntaxNode node)
    {
        if (SemanticModel == null) return null;
        try
        {
            var typeInfo = SemanticModel.GetTypeInfo(node);
            var type = typeInfo.ConvertedType ?? typeInfo.Type;
            if (type == null || type is IErrorTypeSymbol) return null;
            return FormatTypeSymbol(type);
        }
        catch { return null; }
    }

    public string? GetMethodReturnType(SyntaxNode node)
    {
        if (SemanticModel == null) return null;
        try
        {
            var sym = SemanticModel.GetSymbolInfo(node).Symbol;
            if (sym is IMethodSymbol method)
                return FormatTypeSymbol(method.ReturnType);
            return null;
        }
        catch { return null; }
    }

    public static string FormatTypeSymbol(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol named
            && named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
            && named.TypeArguments.Length == 1)
            return FormatTypeSymbol(named.TypeArguments[0]) + "?";

        if (type is IArrayTypeSymbol arr)
            return FormatTypeSymbol(arr.ElementType) + "[]";

        if (type is INamedTypeSymbol listType
            && listType.Name == "List"
            && listType.TypeArguments.Length == 1)
            return "List<" + FormatTypeSymbol(listType.TypeArguments[0]) + ">";

        if (type is INamedTypeSymbol dictType
            && dictType.Name == "Dictionary"
            && dictType.TypeArguments.Length == 2)
            return "Dictionary<"
                + FormatTypeSymbol(dictType.TypeArguments[0]) + ","
                + FormatTypeSymbol(dictType.TypeArguments[1]) + ">";

        if (type.Name == "StringBuilder") return "StringBuilder";

        return type.SpecialType switch
        {
            SpecialType.System_Int32 => "int",
            SpecialType.System_UInt32 => "uint",
            SpecialType.System_Int64 => "long",
            SpecialType.System_UInt64 => "ulong",
            SpecialType.System_Int16 => "short",
            SpecialType.System_UInt16 => "ushort",
            SpecialType.System_Byte => "byte",
            SpecialType.System_SByte => "sbyte",
            SpecialType.System_Single => "float",
            SpecialType.System_Double => "double",
            SpecialType.System_Boolean => "bool",
            SpecialType.System_Char => "char",
            SpecialType.System_String => "string",
            SpecialType.System_Void => "void",
            SpecialType.System_Object => "object",
            _ => type.Name,
        };
    }
}