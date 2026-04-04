using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CS2SX.Core;

/// <summary>
/// Geteilter Zustand während der Transpilierung einer Methode.
///
/// Änderungen:
///   • NextStringBuf(size) nimmt jetzt eine optionale Puffer-Größe entgegen.
///     Standard bleibt 512 Bytes. Für lange Strings (Pfade, Dateinamen)
///     kann ein größerer Puffer angefordert werden.
///   • NextStringBuf schreibt die Deklaration jetzt als lokale Variable
///     statt als static — verhindert Race Conditions bei rekursiven Aufrufen
///     und macht den generierten Code sicherer.
///
/// Hintergrund zum Warning "snprintf output may be truncated":
///   GCC analysiert Format-Strings statisch und nimmt bei %s das Worst-Case
///   an (maximale String-Länge = 50175 für FS_MAX_PATH). Das führt zu Warnings
///   auch wenn der tatsächliche String immer kurz ist (z.B. eine Extension
///   wie ".txt"). Diese Warnings werden jetzt in CCompiler.cs mit
///   -Wno-format-truncation unterdrückt.
/// </summary>
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

    // ── Klassen-Kontext ───────────────────────────────────────────────────────

    public string? CurrentJumpBuf
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

    // ── Indentierung ──────────────────────────────────────────────────────────

    private int _indent;
    public string Tab => new string(' ', _indent * 4);
    public void Indent() => _indent++;
    public void Dedent() => _indent--;

    // ── Zähler ───────────────────────────────────────────────────────────────

    public int TmpCounter
    {
        get; set;
    }
    public int TmpStringCounter
    {
        get; set;
    }

    // ── Zeilen-Info ───────────────────────────────────────────────────────────

    public string CurrentFile { get; set; } = string.Empty;
    public int CurrentLine
    {
        get; set;
    }

    // ── Konstruktor ───────────────────────────────────────────────────────────

    public TranspilerContext(StringWriter writer)
    {
        Out = writer;
    }

    // ── Convenience ──────────────────────────────────────────────────────────

    public void WriteLine(string line) => Out.WriteLine(Tab + line);
    public void WriteRaw(string s) => Out.Write(s);

    public void ClearMethodContext()
    {
        LocalTypes.Clear();
        TmpCounter = 0;
        TmpStringCounter = 0;
        CurrentLine = 0;
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

    /// <summary>
    /// Erzeugt einen eindeutigen lokalen String-Puffer und schreibt die Deklaration.
    ///
    /// Fix: size-Parameter hinzugefügt (Standard 512).
    /// Für Pfade und andere lange Strings kann ein größerer Puffer angefordert
    /// werden — z.B. NextStringBuf(1024) für Pfad-Konkatenationen.
    ///
    /// Die Variable ist lokal (kein static) — das ist sicherer bei verschachtelten
    /// Aufrufen und verhindert Daten-Überschreibung zwischen Frames.
    /// </summary>
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
        catch
        {
            return null;
        }
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
        catch
        {
            return null;
        }
    }

    public static string FormatTypeSymbol(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol named
            && named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
            && named.TypeArguments.Length == 1)
        {
            return FormatTypeSymbol(named.TypeArguments[0]) + "?";
        }

        if (type is IArrayTypeSymbol arr)
            return FormatTypeSymbol(arr.ElementType) + "[]";

        if (type is INamedTypeSymbol listType
            && listType.Name == "List"
            && listType.TypeArguments.Length == 1)
        {
            return "List<" + FormatTypeSymbol(listType.TypeArguments[0]) + ">";
        }

        if (type is INamedTypeSymbol dictType
            && dictType.Name == "Dictionary"
            && dictType.TypeArguments.Length == 2)
        {
            return "Dictionary<"
                + FormatTypeSymbol(dictType.TypeArguments[0]) + ","
                + FormatTypeSymbol(dictType.TypeArguments[1]) + ">";
        }

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