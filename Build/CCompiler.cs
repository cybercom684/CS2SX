// ============================================================================
// CS2SX — Build/CCompiler.cs
//
// FIX (Freetype / komplexe Libs):
//   ExternLib bekommt ExtraIncludeDirs und Defines.
//   Freetype braucht z.B.:
//     -I externLibs/freetype/include
//     -I externLibs/freetype/include/freetype
//     -I externLibs/freetype/src
//     -DFT2_BUILD_LIBRARY
//   Das war vorher nicht möglich — nur ein einziges IncludeDir pro Lib.
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
        DiagnosticReporter? diagnostics = null,
        IEnumerable<ExternLib>? externLibs = null)
    {
        var gcc = Path.Combine(_devkitPath, "devkitA64", "bin", "aarch64-none-elf-gcc");
        var libnxInc = Path.Combine(_devkitPath, "libnx", "include");
        var libnxLib = Path.Combine(_devkitPath, "libnx", "lib");
        var switchSpecs = Path.Combine(_devkitPath, "libnx", "switch.specs");

        if (!File.Exists(switchSpecs))
            throw new FileNotFoundException(
                $"Switch specs nicht gefunden: {switchSpecs}\n"
                + "Bitte DevkitPro korrekt installieren: https://devkitpro.org/wiki/Getting_Started",
                switchSpecs);

        gcc = ProcessRunner.ResolveTool(gcc);

        var allCFiles = cFiles.ToList();
        if (allCFiles.Count == 0)
            throw new ArgumentException("Keine .c-Dateien zum Kompilieren übergeben.", nameof(cFiles));
        var extraIncludeArgs = new System.Text.StringBuilder();
        var defineArgs = new System.Text.StringBuilder();

        if (externLibs != null)
        {
            foreach (var lib in externLibs)
            {
                // .c-Dateien anhängen
                allCFiles.AddRange(lib.Sources);

                // Haupt-IncludeDir
                if (!string.IsNullOrEmpty(lib.IncludeDir))
                    extraIncludeArgs.Append($" -I\"{lib.IncludeDir}\"");

                // FIX: Zusätzliche Include-Verzeichnisse (für Freetype, libpng etc.)
                foreach (var extraDir in lib.ExtraIncludeDirs)
                    extraIncludeArgs.Append($" -I\"{extraDir}\"");

                // FIX: Präprozessor-Defines (-D flags)
                foreach (var define in lib.Defines)
                    defineArgs.Append($" -D{define}");
            }
        }

        var fileArgs = string.Join(" ", allCFiles.Select(f => $"\"{f}\""));

        var args = fileArgs
                 + $" -o \"{outputElf}\""
                 + $" -I\"{includeDir}\""
                 + (projectDir != null ? $" -I\"{projectDir}\"" : "")
                 + $" -I\"{libnxInc}\""
                 + extraIncludeArgs
                 + defineArgs
                 + " -march=armv8-a+crc+crypto -mtune=cortex-a57 -mtp=soft -fPIE"
                 + " -ffunction-sections -fdata-sections"
                 + " -std=c11"
                 + " -O2 -Wall -Wextra -Wno-unused-parameter"
                 + " -Wno-format-truncation"
                 + " -Wno-unused-function"
                 + " -Wno-misleading-indentation"
                 + $" -specs=\"{switchSpecs}\""
                 + $" -L\"{libnxLib}\" -lnx -lm"
                 + " -Wl,--gc-sections";

        try
        {
            ProcessRunner.Run(gcc, args, "GCC");
        }
        catch (Exception ex) when (diagnostics != null)
        {
            var enhanced = diagnostics.MapGccErrors(ex.Message, includeDir);
            throw new GccCompileException(enhanced, ex);
        }
    }

    /// <summary>
    /// Repräsentiert eine externe C-Library für den Build.
    /// </summary>
    public sealed record ExternLib(
        string Name,
        List<string> Sources,
        string? IncludeDir,
        List<string> ExtraIncludeDirs,
        List<string> Defines);
}

public sealed class GccCompileException : Exception
{
    public GccCompileException(string message, Exception inner)
        : base(message, inner) { }
}