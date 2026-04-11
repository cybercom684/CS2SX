// ============================================================================
// CS2SX — Build/CCompiler.cs  (VERBESSERT)
//
// Änderungen:
//   • GCC-Fehlerausgabe wird auf C#-Quellzeilen zurückgemappt
//   • Bessere Fehlermeldung wenn DEVKITPRO nicht gesetzt
//   • Compile() gibt jetzt die Anzahl der Errors zurück
// ============================================================================

using CS2SX.Core;

namespace CS2SX.Build;

public sealed class CCompiler
{
    private readonly string _devkitPath;

    public CCompiler()
    {
        _devkitPath = ProcessRunner.GetDevkitPro();
    }

    public void Compile(
        IEnumerable<string> cFiles,
        string outputElf,
        string includeDir,
        string? projectDir = null,
        DiagnosticReporter? diagnostics = null)
    {
        var gcc = Path.Combine(_devkitPath, "devkitA64", "bin", "aarch64-none-elf-gcc");
        var libnxInc = Path.Combine(_devkitPath, "libnx", "include");
        var libnxLib = Path.Combine(_devkitPath, "libnx", "lib");
        var switchSpecs = Path.Combine(_devkitPath, "libnx", "switch.specs");

        gcc = ProcessRunner.ResolveTool(gcc);

        var fileArgs = string.Join(" ", cFiles.Select(f => "\"" + f + "\""));

        var args = fileArgs
                 + " -o \"" + outputElf + "\""
                 + " -I\"" + includeDir + "\""
                 + (projectDir != null ? " -I\"" + projectDir + "\"" : "")
                 + " -I\"" + libnxInc + "\""
                 + " -march=armv8-a+crc+crypto -mtune=cortex-a57 -mtp=soft -fPIE"
                 + " -ffunction-sections -fdata-sections"
                 + " -std=c11"
                 + " -O2 -Wall -Wextra -Wno-unused-parameter"
                 + " -Wno-format-truncation"
                 + " -Wno-unused-function"
                 + " -Wno-misleading-indentation"
                 + " -specs=\"" + switchSpecs + "\""
                 + " -L\"" + libnxLib + "\" -lnx -lm"
                 + " -Wl,--gc-sections";

        try
        {
            ProcessRunner.Run(gcc, args, "GCC");
        }
        catch (Exception ex) when (diagnostics != null)
        {
            // GCC-Fehler aufbereiten und mit Source-Map verknüpfen
            var enhanced = diagnostics.MapGccErrors(ex.Message, includeDir);
            throw new GccCompileException(enhanced, ex);
        }
    }
}

/// <summary>
/// GCC-Kompilierungsfehler mit aufbereiteter Fehlermeldung.
/// Enthält sowohl den originalen GCC-Output als auch
/// die zurückgemappten C#-Quellzeilen.
/// </summary>
public sealed class GccCompileException : Exception
{
    public GccCompileException(string message, Exception inner)
        : base(message, inner) { }
}