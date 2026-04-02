namespace CS2SX.Core;

/// <summary>
/// Geteilter Zustand während der Transpilierung einer Methode.
/// Wird durch alle Handler weitergegeben — kein globaler Zustand.
///
/// Änderungen gegenüber der alten Version:
///   • TmpStringCounter: pro-Statement eindeutige String-Puffer statt
///     globalem _cs2sx_strbuf — verhindert Überschreiben bei verschachtelten Aufrufen
///   • CurrentLine / CurrentFile: für Fehlermeldungen mit Zeilennummern
///   • NextStringBuf(): erzeugt lokale char[]-Variablen im C-Output
/// </summary>
public sealed class TranspilerContext
{
    // ── Output ────────────────────────────────────────────────────────────────

    public StringWriter Out
    {
        get;
    }

    // ── Klassen-Kontext ───────────────────────────────────────────────────────

    /// <summary>Name der aktuellen jmp_buf Variable für Exception-Handling.</summary>
    public string? CurrentJumpBuf
    {
        get; set;
    }

    /// <summary>Name der aktuell transpilierten Klasse.</summary>
    public string CurrentClass { get; set; } = string.Empty;

    /// <summary>Basisklasse der aktuell transpilierten Klasse.</summary>
    public string CurrentBaseType { get; set; } = string.Empty;

    /// <summary>Felder der aktuellen Klasse: TrimmedName → C#-Typ.</summary>
    public Dictionary<string, string> FieldTypes { get; } = new(StringComparer.Ordinal);

    /// <summary>Felder der Basisklasse.</summary>
    public Dictionary<string, string> BaseFieldTypes { get; } = new(StringComparer.Ordinal);

    // ── Methoden- und Property-Typen ──────────────────────────────────────────

    /// <summary>Rückgabetypen der Methoden: Name → C#-Typ.</summary>
    public Dictionary<string, string> MethodReturnTypes { get; } = new(StringComparer.Ordinal);

    /// <summary>Typen der Properties: Name → C#-Typ.</summary>
    public Dictionary<string, string> PropertyTypes { get; } = new(StringComparer.Ordinal);

    /// <summary>Namen der in der aktuellen Klasse definierten Enums.</summary>
    public HashSet<string> EnumMembers { get; } = new(StringComparer.Ordinal);

    // ── Methoden-Kontext ──────────────────────────────────────────────────────

    /// <summary>Lokale Variablen der aktuellen Methode: Name → C#-Typ.</summary>
    public Dictionary<string, string> LocalTypes { get; } = new(StringComparer.Ordinal);

    // ── Indentierung ──────────────────────────────────────────────────────────

    private int _indent;

    public string Tab => new string(' ', _indent * 4);

    public void Indent() => _indent++;
    public void Dedent() => _indent--;

    // ── Zähler ───────────────────────────────────────────────────────────────

    /// <summary>Allgemeiner Zähler für temporäre Variablen.</summary>
    public int TmpCounter
    {
        get; set;
    }

    /// <summary>
    /// Zähler für temporäre String-Puffer innerhalb einer Methode.
    /// Jeder Aufruf von NextStringBuf() erzeugt einen eindeutigen Namen
    /// wie _sb0, _sb1 etc. — verhindert globale Buffer-Kollisionen.
    /// </summary>
    public int TmpStringCounter
    {
        get; set;
    }

    // ── Zeilen-Info für Fehlermeldungen ───────────────────────────────────────

    /// <summary>Aktuelle C#-Quelldatei (für Fehlermeldungen).</summary>
    public string CurrentFile { get; set; } = string.Empty;

    /// <summary>Aktuelle Zeilennummer in der C#-Quelldatei.</summary>
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

    /// <summary>
    /// Schlägt den Typ eines Bezeichners nach (lokal → Feld → null).
    /// </summary>
    public string? LookupType(string name)
    {
        var trimmed = name.TrimStart('_');
        if (LocalTypes.TryGetValue(name, out var lt)) return lt;
        if (FieldTypes.TryGetValue(trimmed, out var ft)) return ft;
        return null;
    }

    /// <summary>True wenn name auf ein Feld der aktuellen Klasse zeigt.</summary>
    public bool IsFieldAccess(string name) =>
        name.StartsWith('_') && !string.IsNullOrEmpty(CurrentClass);

    /// <summary>Erzeugt eine eindeutige temporäre Variable.</summary>
    public string NextTmp(string prefix = "tmp") =>
        "_" + prefix + "_" + (TmpCounter++);

    /// <summary>
    /// Erzeugt einen eindeutigen lokalen String-Puffer-Namen und
    /// schreibt die Deklaration direkt in den Output.
    ///
    /// Statt des globalen _cs2sx_strbuf wird ein lokales
    ///   char _sbN[512];
    /// erzeugt. Das verhindert Buffer-Kollisionen bei
    /// verschachtelten String-Operationen.
    ///
    /// Gibt den Namen des Puffers zurück ("_sb0", "_sb1", ...).
    /// </summary>
    public string NextStringBuf(int size = 512)
    {
        var name = "_sb" + (TmpStringCounter++);
        WriteLine($"char {name}[{size}];");
        return name;
    }

    /// <summary>
    /// Wie NextStringBuf, aber ohne Deklaration — nur Name.
    /// Verwenden wenn die Deklaration manuell platziert werden soll.
    /// </summary>
    public string PeekNextStringBufName() =>
        "_sb" + TmpStringCounter;

    /// <summary>
    /// Formatiert eine Fehlermeldung mit Datei und Zeilennummer.
    /// </summary>
    public string FormatDiagnostic(string message) =>
        string.IsNullOrEmpty(CurrentFile)
            ? message
            : $"{CurrentFile}({CurrentLine}): {message}";
}
