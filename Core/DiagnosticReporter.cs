// Datei: Core/DiagnosticReporter.cs
//
// FIXES in dieser Version:
//   FIX-1: Deduplizierung von Warnings und Errors.
//          Im Watch-Modus wird bei jeder Dateiänderung der gesamte Transpile-Durchlauf
//          wiederholt. Ohne Deduplizierung akkumuliert jeder Handler-Miss (z.B.
//          InvocationDispatcher "unknown call") für denselben Code-Ort eine neue
//          Warning pro Re-Build. Nach 10 Saves sind es 10x dieselbe Warning.
//          Neu: _seenKeys (HashSet) verhindert Duplikate innerhalb eines Build-Durchlaufs.
//          Flush() leert _seenKeys, sodass der nächste Build sauber beginnt.
//
//   FIX-2: RegisterSourceMapping ist jetzt idempotent.
//          Mehrfaches Registrieren desselben Keys überschreibt statt zu duplizieren
//          (war schon so, explizit dokumentiert).

using CS2SX.Logging;
using Microsoft.CodeAnalysis;

namespace CS2SX.Core;

public sealed class DiagnosticReporter
{
    private readonly List<TranspilerDiagnostic> _diagnostics = new();
    private readonly object _lock = new();

    // FIX-1: Deduplizierungsset — verhindert identische Meldungen
    // (gleiche Datei, gleiche Zeile, gleiche Message) pro Build-Durchlauf.
    // Wird in Flush() geleert damit der nächste Build sauber startet.
    private readonly HashSet<(string csFile, int csLine, string message)> _seenKeys =
        new();

    // ── Source-Map: generierte C-Zeile → C#-Quelldatei + Zeile ──────────────
    // Key:   "baseName.c:lineNumber"   (z.B. "Game.c:42")
    // Value: (csFile, csLine, csSnippet)
    private readonly Dictionary<string, (string csFile, int csLine, string snippet)> _sourceMap =
        new(StringComparer.Ordinal);

    // ── Öffentliche API ───────────────────────────────────────────────────────

    /// <summary>Fügt eine Warning mit Quell-Location hinzu (dedupliziert).</summary>
    public void Warn(string csFile, int csLine, string message, string? context = null)
    {
        lock (_lock)
        {
            // FIX-1: Duplikat? Dann still ignorieren.
            if (!_seenKeys.Add((csFile, csLine, message)))
                return;

            _diagnostics.Add(new TranspilerDiagnostic(
                DiagnosticSeverity.Warning, csFile, csLine, message, context));
        }
    }

    /// <summary>Fügt eine Warning für einen Roslyn-SyntaxNode hinzu (dedupliziert).</summary>
    public void Warn(SyntaxNode node, string csFile, string message)
    {
        var line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        Warn(csFile, line, message, node.ToString().Split('\n')[0].Trim());
    }

    /// <summary>Fügt einen Error hinzu (dedupliziert).</summary>
    public void Error(string csFile, int csLine, string message, string? context = null)
    {
        lock (_lock)
        {
            if (!_seenKeys.Add((csFile, csLine, message)))
                return;

            _diagnostics.Add(new TranspilerDiagnostic(
                DiagnosticSeverity.Error, csFile, csLine, message, context));
        }
    }

    /// <summary>Fügt eine Info-Meldung hinzu (kein Problem, nur Hinweis, dedupliziert).</summary>
    public void Info(string csFile, int csLine, string message)
    {
        lock (_lock)
        {
            if (!_seenKeys.Add((csFile, csLine, message)))
                return;

            _diagnostics.Add(new TranspilerDiagnostic(
                DiagnosticSeverity.Info, csFile, csLine, message, null));
        }
    }

    /// <summary>
    /// Registriert eine C#-Zeile → generierte C-Zeile Zuordnung.
    /// Ist idempotent: mehrfaches Registrieren desselben Keys überschreibt.
    /// </summary>
    public void RegisterSourceMapping(
        string cFileName, int cLineNumber,
        string csFile, int csLine, string csSnippet)
    {
        var key = cFileName + ":" + cLineNumber;
        lock (_lock)
        {
            _sourceMap[key] = (csFile, csLine, csSnippet);
        }
    }

    /// <summary>
    /// Versucht GCC-Fehlerausgabe auf C#-Quellzeilen zurückzuverfolgen.
    /// Gibt eine lesbare Fehlermeldung zurück.
    /// </summary>
    public string MapGccErrors(string gccOutput, string buildDir)
    {
        if (string.IsNullOrWhiteSpace(gccOutput)) return gccOutput;

        var lines = gccOutput.Split('\n');
        var result = new System.Text.StringBuilder();

        foreach (var line in lines)
        {
            result.AppendLine(line);

            var mapped = TryMapGccLine(line, buildDir);
            if (mapped != null)
                result.AppendLine("    " + mapped);
        }

        return result.ToString().TrimEnd();
    }

    private string? TryMapGccLine(string gccLine, string buildDir)
    {
        var parts = gccLine.Split(':');
        if (parts.Length < 3) return null;

        var filePart = parts[0].Trim();
        if (!int.TryParse(parts[1].Trim(), out var lineNum)) return null;

        var fileName = Path.GetFileName(filePart);
        var key = fileName + ":" + lineNum;

        if (_sourceMap.TryGetValue(key, out var loc))
            return $"→ C# {Path.GetFileName(loc.csFile)}({loc.csLine}): {loc.snippet}";

        if (fileName.EndsWith(".c") && File.Exists(filePart))
        {
            var csName = Path.GetFileNameWithoutExtension(fileName) + ".cs";
            return $"→ (aus {csName} transpiliert)";
        }

        return null;
    }

    // ── Ausgabe ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Gibt alle gesammelten Diagnostics aus, leert den Puffer (inkl. Deduplizierungsset)
    /// und gibt die Anzahl Warnings zurück.
    /// </summary>
    public int Flush()
    {
        List<TranspilerDiagnostic> snapshot;
        lock (_lock)
        {
            snapshot = new List<TranspilerDiagnostic>(_diagnostics);
            _diagnostics.Clear();
            // FIX-1: Deduplizierungsset leeren — nächster Build startet sauber.
            _seenKeys.Clear();
        }

        if (snapshot.Count == 0) return 0;

        int warnings = 0;
        foreach (var d in snapshot.OrderBy(d => d.CsFile).ThenBy(d => d.CsLine))
        {
            var loc = string.IsNullOrEmpty(d.CsFile)
                ? ""
                : $"{Path.GetFileName(d.CsFile)}({d.CsLine}): ";

            switch (d.Severity)
            {
                case DiagnosticSeverity.Warning:
                    Log.Warning(loc + d.Message
                        + (d.Context != null ? $"\n  code: {d.Context}" : ""));
                    warnings++;
                    break;
                case DiagnosticSeverity.Error:
                    Log.Error(loc + d.Message
                        + (d.Context != null ? $"\n  code: {d.Context}" : ""));
                    break;
                case DiagnosticSeverity.Info:
                    Log.Info(loc + d.Message);
                    break;
            }
        }

        return warnings;
    }

    /// <summary>Gibt eine kompakte Zusammenfassung für den Build-Abschluss zurück.</summary>
    public string Summary()
    {
        lock (_lock)
        {
            var w = _diagnostics.Count(d => d.Severity == DiagnosticSeverity.Warning);
            return w == 0 ? "" : $"{w} transpiler warning(s)";
        }
    }

    public bool HasWarnings
    {
        get
        {
            lock (_lock) return _diagnostics.Any(d => d.Severity == DiagnosticSeverity.Warning);
        }
    }

    public IReadOnlyList<TranspilerDiagnostic> All
    {
        get
        {
            lock (_lock) return _diagnostics.ToList();
        }
    }
}

// ── Diagnostic-Eintrag ────────────────────────────────────────────────────────

public enum DiagnosticSeverity
{
    Info, Warning, Error
}

public sealed record TranspilerDiagnostic(
    DiagnosticSeverity Severity,
    string CsFile,
    int CsLine,
    string Message,
    string? Context);