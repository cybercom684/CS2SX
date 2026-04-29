// Datei: Core/TranspilerContext.cs
//
// FIXES in dieser Version:
//   FIX-1: TmpCounter und TmpStringCounter sind jetzt per-Klasse, nicht per-Methode.
//          Vorher wurden beide in ClearMethodContext() auf 0 zurückgesetzt. Das führte
//          dazu, dass zwei Methoden in derselben Klasse beide "_tmp_0" oder "_sb0"
//          deklarierten — GCC meldete dann "redefinition of '_tmp_0'".
//          Jetzt steigen die Zähler über die gesamte Klasse monoton an und werden
//          nur in ClearClassContext() zurückgesetzt.
//
//   FIX-2: PendingLambdaPreludes-Liste hinzugefügt.
//          LambdaLifter schreibt erzeugte Struct- und Funktionsdefinitionen nicht mehr
//          per StringWriter-Rewrite in den Output, sondern sammelt sie in dieser Liste.
//          ExpressionWriter.WriteLambda() ruft ConsumeAndWritePreludes() auf, bevor
//          die Methodensignatur geschrieben wird — O(1) statt O(n) pro Lambda.
//
//   FIX-3: _lambdaCounter-Kommentar präzisiert (inhaltlich unverändert, war korrekt).

using CS2SX.Transpiler;
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
    public string? CurrentTupleReturnType
    {
        get; set;
    }

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

    // Löst "using static"-Importe auf
    public UsingStaticResolver UsingStaticResolver { get; } = new();

    // ── Zähler ────────────────────────────────────────────────────────────────

    // FIX-1: _classTmpCounter und _classStringCounter ersetzen die alten
    // TmpCounter / TmpStringCounter Properties. Sie werden NUR in
    // ClearClassContext() zurückgesetzt, nicht in ClearMethodContext().
    // Das verhindert doppelte Variablendeklarationen in .c-Dateien wenn
    // mehrere Methoden einer Klasse beide temporäre Buffer benötigen.
    private int _classTmpCounter;
    private int _classStringCounter;

    // Öffentliche Properties für Legacy-Lesezugriff (z.B. LambdaLifter sync)
    public int TmpCounter
    {
        get => _classTmpCounter;
        set => _classTmpCounter = value;  // LambdaLifter sync bleibt kompatibel
    }

    public int TmpStringCounter
    {
        get => _classStringCounter;
        set => _classStringCounter = value;
    }

    // FIX: Lambda-Zähler gehört in den Context, NICHT in LambdaLifter.
    // Zwei Lambdas in verschiedenen Methoden derselben Klasse müssen
    // unterschiedliche IDs bekommen. Wird nur in ClearClassContext() resettet.
    private int _lambdaCounter;

    /// <summary>
    /// Gibt eine eindeutige, aufsteigende Lambda-ID für die aktuelle Klasse zurück.
    /// </summary>
    public int NextLambdaId() => _lambdaCounter++;

    // FIX-2: Lambda-Preludes (Struct-Defs + Funktionsdefs) werden hier gesammelt
    // statt per O(n)-StringWriter-Rewrite eingefügt zu werden.
    // ExpressionWriter.WriteLambda() fügt via LambdaLifter.ConsumePrelude() hinzu.
    // CSharpToC.VisitMethodDeclaration() schreibt sie einmalig VOR der Signatur.
    public List<string> PendingLambdaPreludes { get; } = new();

    /// <summary>
    /// Schreibt alle gesammelten Lambda-Preludes in den Output und leert die Liste.
    /// Muss von CSharpToC.VisitMethodDeclaration() VOR dem Schreiben der
    /// Methodensignatur aufgerufen werden.
    /// </summary>
    public void FlushLambdaPreludes()
    {
        if (PendingLambdaPreludes.Count == 0) return;
        foreach (var p in PendingLambdaPreludes)
            Out.Write(p);
        PendingLambdaPreludes.Clear();
    }

    // ── Zeilen-Tracking ───────────────────────────────────────────────────────

    public string CurrentFile { get; set; } = string.Empty;
    public int CurrentLine
    {
        get; set;
    }

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

    /// <summary>
    /// Schreibt eine Zeile und registriert das Source-Mapping C-Zeile → C#-Zeile.
    /// </summary>
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

    /// <summary>
    /// Schreibt eine Zeile mit automatischer Source-Map aus dem aktuellen CurrentLine.
    /// </summary>
    public void WriteLineMapped(string line)
    {
        WriteLineWithMapping(line, CurrentLine,
            line.Trim().Length > 60 ? line.Trim()[..60] + "…" : line.Trim());
    }

    public void ClearMethodContext()
    {
        LocalTypes.Clear();
        ArrayLengths.Clear();
        // FIX-1: TmpCounter und TmpStringCounter werden NICHT zurückgesetzt.
        // Sie gelten für die gesamte Klasse (bis ClearClassContext()), damit
        // keine doppelten Variablennamen in der generierten .c-Datei entstehen.
        CurrentLine = 0;
        CurrentTupleReturnType = null;
        CurrentReturnBuffer = null;
        // _lambdaCounter wird ebenfalls NICHT zurückgesetzt (gilt pro Klasse).
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

        // FIX-1: Zähler nur hier zurücksetzen — pro Klasse, nicht pro Methode.
        _classTmpCounter = 0;
        _classStringCounter = 0;
        _lambdaCounter = 0;

        // FIX-2: Offene Preludes beim Klassenwechsel verwerfen (sollte leer sein,
        // aber defensiv sicherheitshalber leeren).
        PendingLambdaPreludes.Clear();

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

    // FIX-1: NextTmp nutzt _classTmpCounter — steigt pro Klasse monoton an.
    public string NextTmp(string prefix = "tmp") =>
        "_" + prefix + "_" + (_classTmpCounter++);

    // FIX-1: NextStringBuf nutzt _classStringCounter — steigt pro Klasse monoton an.
    public string NextStringBuf(int size = 512)
    {
        var name = "_sb" + (_classStringCounter++);
        WriteLine($"char {name}[{size}];");
        return name;
    }

    public string PeekNextStringBufName() => "_sb" + _classStringCounter;

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