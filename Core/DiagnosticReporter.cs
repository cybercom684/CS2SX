// ============================================================================
// CS2SX — Core/DiagnosticReporter.cs  (NEU)
//
// Zentralisiertes Diagnose-System für den Transpiler.
//
// Aufgaben:
//   • Sammelt Warnings während der Transpilierung (UNSUPPORTED-Nodes, Fallbacks)
//   • Verknüpft GCC-Fehler mit ursprünglichen C#-Quellzeilen via Source-Map
//   • Gibt am Ende des Builds eine verständliche Zusammenfassung aus
//
// Verwendung:
//   // In StatementWriter/ExpressionWriter:
//   _ctx.Diagnostics.Warn(node, "foreach über unbekannten Typ — Fallback zu _count");
//
//   // In BuildPipeline nach GCC-Fehler:
//   var mapped = diagnostics.MapGccError(gccOutput, buildDir);
// ============================================================================

using CS2SX.Logging;
using Microsoft.CodeAnalysis;

namespace CS2SX.Core;

public sealed class DiagnosticReporter
{
    private readonly List<TranspilerDiagnostic> _diagnostics = new();
    private readonly object _lock = new();

    // ── Source-Map: generierte C-Zeile → C#-Quelldatei + Zeile ──────────────
    // Key:   "baseName.c:lineNumber"   (z.B. "Game.c:42")
    // Value: (csFile, csLine, csSnippet)
    private readonly Dictionary<string, (string csFile, int csLine, string snippet)> _sourceMap =
        new(StringComparer.Ordinal);

    // ── Öffentliche API ───────────────────────────────────────────────────────

    /// <summary>Fügt eine Warning mit Quell-Location hinzu.</summary>
    public void Warn(string csFile, int csLine, string message, string? context = null)
    {
        lock (_lock)
        {
            _diagnostics.Add(new TranspilerDiagnostic(
                DiagnosticSeverity.Warning, csFile, csLine, message, context));
        }
    }

    /// <summary>Fügt eine Warning für einen Roslyn-SyntaxNode hinzu.</summary>
    public void Warn(SyntaxNode node, string csFile, string message)
    {
        var line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        Warn(csFile, line, message, node.ToString().Split('\n')[0].Trim());
    }

    /// <summary>Fügt eine Info-Meldung hinzu (kein Problem, nur Hinweis).</summary>
    public void Info(string csFile, int csLine, string message)
    {
        lock (_lock)
        {
            _diagnostics.Add(new TranspilerDiagnostic(
                DiagnosticSeverity.Info, csFile, csLine, message, null));
        }
    }

    /// <summary>Registriert eine C#-Zeile → generierte C-Zeile Zuordnung.</summary>
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

            // GCC-Format: "path/file.c:42:8: error: ..."
            var mapped = TryMapGccLine(line, buildDir);
            if (mapped != null)
                result.AppendLine("    " + mapped);
        }

        return result.ToString().TrimEnd();
    }

    private string? TryMapGccLine(string gccLine, string buildDir)
    {
        // Parst: "/path/Game.c:42:8: error: 'foo' undeclared"
        var parts = gccLine.Split(':');
        if (parts.Length < 3) return null;

        var filePart = parts[0].Trim();
        if (!int.TryParse(parts[1].Trim(), out var lineNum)) return null;

        var fileName = Path.GetFileName(filePart);
        var key = fileName + ":" + lineNum;

        if (_sourceMap.TryGetValue(key, out var loc))
            return $"→ C# {Path.GetFileName(loc.csFile)}({loc.csLine}): {loc.snippet}";

        // Fallback: nur Dateiname ohne Zeilen-Mapping
        if (fileName.EndsWith(".c") && File.Exists(filePart))
        {
            var csName = Path.GetFileNameWithoutExtension(fileName) + ".cs";
            return $"→ (aus {csName} transpiliert)";
        }

        return null;
    }

    // ── Ausgabe ───────────────────────────────────────────────────────────────

    /// <summary>Gibt alle gesammelten Diagnostics aus und gibt die Anzahl Warnings zurück.</summary>
    public int Flush()
    {
        List<TranspilerDiagnostic> snapshot;
        lock (_lock)
        {
            snapshot = new List<TranspilerDiagnostic>(_diagnostics);
            _diagnostics.Clear();
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