// Datei: Core/TranspileResult.cs  (neue Datei)

namespace CS2SX.Core;

public sealed class TranspileResult
{
    public string Code
    {
        get;
    }
    public IReadOnlyList<TranspilerDiagnostic> Diagnostics
    {
        get;
    }
    public int WarningCount => Diagnostics.Count(d => d.Severity == DiagnosticSeverity.Warning);
    public int ErrorCount => Diagnostics.Count(d => d.Severity == DiagnosticSeverity.Error);
    public bool IsClean => Diagnostics.Count == 0;

    public TranspileResult(string code, IReadOnlyList<TranspilerDiagnostic> diagnostics)
    {
        Code = code;
        Diagnostics = diagnostics;
    }
}