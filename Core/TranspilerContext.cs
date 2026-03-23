namespace CS2SX.Core;

/// <summary>
/// Geteilter Zustand während der Transpilierung einer Methode.
/// Wird durch alle Handler weitergegeben — kein globaler Zustand.
/// </summary>
public sealed class TranspilerContext
{
    // ── Output ────────────────────────────────────────────────────────────────

    public StringWriter Out { get; }

    // ── Klassen-Kontext ───────────────────────────────────────────────────────

    /// <summary>Name der aktuell transpilierten Klasse.</summary>
    public string CurrentClass { get; set; } = string.Empty;

    /// <summary>Basisklasse der aktuell transpilierten Klasse.</summary>
    public string CurrentBaseType { get; set; } = string.Empty;

    /// <summary>Felder der aktuellen Klasse: TrimmedName → C#-Typ.</summary>
    public Dictionary<string, string> FieldTypes { get; } = new(StringComparer.Ordinal);

    /// <summary>Felder der Basisklasse.</summary>
    public Dictionary<string, string> BaseFieldTypes { get; } = new(StringComparer.Ordinal);

    // ── Methoden-Kontext ──────────────────────────────────────────────────────

    /// <summary>Lokale Variablen der aktuellen Methode: Name → C#-Typ.</summary>
    public Dictionary<string, string> LocalTypes { get; } = new(StringComparer.Ordinal);

    // ── Indentierung ──────────────────────────────────────────────────────────

    private int _indent;

    public string Tab => new string(' ', _indent * 4);

    public void Indent()  => _indent++;
    public void Dedent()  => _indent--;

    // ── Hilfszähler ───────────────────────────────────────────────────────────

    public int TmpCounter { get; set; }

    // ── Konstruktor ───────────────────────────────────────────────────────────

    public TranspilerContext(StringWriter writer)
    {
        Out = writer;
    }

    // ── Convenience ──────────────────────────────────────────────────────────

    public void WriteLine(string line) => Out.WriteLine(Tab + line);
    public void WriteRaw(string s)     => Out.Write(s);

    public void ClearMethodContext()
    {
        LocalTypes.Clear();
        TmpCounter = 0;
    }

    public void ClearClassContext()
    {
        CurrentClass    = string.Empty;
        CurrentBaseType = string.Empty;
        FieldTypes.Clear();
        BaseFieldTypes.Clear();
        ClearMethodContext();
    }

    /// <summary>
    /// Schlägt den Typ eines Bezeichners nach (lokal → Feld → null).
    /// </summary>
    public string? LookupType(string name)
    {
        var trimmed = name.TrimStart('_');
        if (LocalTypes.TryGetValue(name, out var lt))   return lt;
        if (FieldTypes.TryGetValue(trimmed, out var ft)) return ft;
        return null;
    }

    /// <summary>
    /// True wenn name auf ein Feld der aktuellen Klasse zeigt.
    /// </summary>
    public bool IsFieldAccess(string name) =>
        name.StartsWith('_') && !string.IsNullOrEmpty(CurrentClass);

    /// <summary>
    /// Erzeugt eine eindeutige temporäre Variable.
    /// </summary>
    public string NextTmp(string prefix = "tmp") =>
        "_" + prefix + "_" + (TmpCounter++);
}
