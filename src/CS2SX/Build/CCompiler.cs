// ============================================================================
// CS2SX — Build/CCompiler.cs
//
// Compiles the generated C and the extern-lib sources to a Switch ELF.
//
//   • every translation unit becomes its own object file in cs2sx_out/obj/
//   • objects are compiled in parallel (one gcc per core)
//   • an object is rebuilt only when its inputs changed: a content hash over the
//     compiler flags, the source and every header gcc reports as a dependency
//     (-MMD). Timestamps are not trusted — generated files are rewritten often.
//     So an extern lib such as FreeType is compiled once, not on every build.
//   • the objects are linked in a separate step
// ============================================================================

using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using CS2SX.Core;
using CS2SX.Logging;

namespace CS2SX.Build;

public sealed class CCompiler
{
    private readonly string _devkitPath;

    public CCompiler()
    {
        _devkitPath = ProcessRunner.GetDevkitPro();
    }

    /// Statistics of the last <see cref="Compile"/> call (for the build summary).
    public int CompiledCount { get; private set; }
    public int CachedCount { get; private set; }

    public void Compile(
        IEnumerable<string> cFiles,
        string outputElf,
        string includeDir,
        string? projectDir = null,
        DiagnosticReporter? diagnostics = null,
        IEnumerable<ExternLib>? externLibs = null)
    {
        var gcc = ProcessRunner.ResolveTool(Path.Combine(_devkitPath, "devkitA64", "bin", "aarch64-none-elf-gcc"));
        var libnxInc = Path.Combine(_devkitPath, "libnx", "include");
        var libnxLib = Path.Combine(_devkitPath, "libnx", "lib");
        var switchSpecs = Path.Combine(_devkitPath, "libnx", "switch.specs");
        // portlibs (curl, zlib, png, freetype, ...) for extern libs that depend on them.
        var portlibsInc = Path.Combine(_devkitPath, "portlibs", "switch", "include");
        var portlibsLib = Path.Combine(_devkitPath, "portlibs", "switch", "lib");

        if (!File.Exists(switchSpecs))
            throw new FileNotFoundException(
                $"Switch specs nicht gefunden: {switchSpecs}\n"
                + "Bitte DevkitPro korrekt installieren: https://devkitpro.org/wiki/Getting_Started",
                switchSpecs);

        // ── Sources: generated TUs + extern-lib sources ──────────────────────
        var units = cFiles.Select(f => new Unit(Path.GetFullPath(f), LibName: null)).ToList();
        if (units.Count == 0)
            throw new ArgumentException("Keine .c-Dateien zum Kompilieren übergeben.", nameof(cFiles));

        var includeArgs = new StringBuilder();
        var defineArgs = new StringBuilder();
        foreach (var lib in externLibs ?? Enumerable.Empty<ExternLib>())
        {
            units.AddRange(lib.Sources.Select(s => new Unit(Path.GetFullPath(s), lib.Name)));
            if (!string.IsNullOrEmpty(lib.IncludeDir)) includeArgs.Append($" -I\"{lib.IncludeDir}\"");
            foreach (var d in lib.ExtraIncludeDirs) includeArgs.Append($" -I\"{d}\"");
            foreach (var d in lib.Defines) defineArgs.Append($" -D{d}");
        }

        // ── Compile flags (identical for every TU → part of each object's hash) ──
        var compileFlags =
              $" -I\"{includeDir}\""
            + (projectDir != null ? $" -I\"{projectDir}\"" : "")
            + includeArgs
            // System headers: excluded from dependency lists, and their internal
            // warnings are not the app's business.
            + $" -isystem \"{libnxInc}\""
            + (Directory.Exists(portlibsInc) ? $" -isystem \"{portlibsInc}\"" : "")
            + defineArgs
            + " -march=armv8-a+crc+crypto -mtune=cortex-a57 -mtp=soft -fPIE"
            + " -ffunction-sections -fdata-sections"
            + " -std=gnu11"   // libnx/extern-lib headers need GNU extensions (typeof, …)
            + " -O2 -Wall -Wextra -Wno-unused-parameter"
            + " -Wno-format-truncation"
            + " -Wno-unused-function"
            + " -Wno-misleading-indentation";

        var objDir = Path.Combine(includeDir, "obj");
        Directory.CreateDirectory(objDir);

        // ── Compile (parallel, cached by content hash) ───────────────────────
        var hashes = new FileHashCache();
        var flagsHash = Hash(compileFlags + "|" + gcc);
        var objects = new string[units.Count];
        var failures = new ConcurrentBag<string>();
        int compiled = 0, cached = 0;

        Parallel.For(0, units.Count,
            new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount },
            i =>
            {
                var unit = units[i];
                var obj = Path.Combine(objDir, ObjectName(unit));
                objects[i] = obj;
                var depFile = obj + ".d";
                var keyFile = obj + ".key";

                if (File.Exists(obj) && File.Exists(keyFile) && File.Exists(depFile)
                    && File.ReadAllText(keyFile) == InputKey(unit.Source, depFile, flagsHash, hashes))
                {
                    Interlocked.Increment(ref cached);
                    return;
                }

                try
                {
                    ProcessRunner.Run(gcc,
                        $"-c \"{unit.Source}\" -o \"{obj}\" -MMD -MF \"{depFile}\"" + compileFlags,
                        "GCC");
                    File.WriteAllText(keyFile, InputKey(unit.Source, depFile, flagsHash, hashes));
                    Interlocked.Increment(ref compiled);
                }
                catch (Exception ex)
                {
                    TryDelete(obj);
                    TryDelete(keyFile);
                    failures.Add(ex.Message);
                }
            });

        CompiledCount = compiled;
        CachedCount = cached;

        if (!failures.IsEmpty)
        {
            var all = string.Join("\n", failures);
            throw diagnostics != null
                ? new GccCompileException(diagnostics.MapGccErrors(all, includeDir), new Exception(all))
                : new Exception(all);
        }

        // ── Link ─────────────────────────────────────────────────────────────
        var linkArgs = string.Join(" ", objects.Select(o => $"\"{o}\""))
            + $" -o \"{outputElf}\""
            + " -march=armv8-a+crc+crypto -mtune=cortex-a57 -mtp=soft -fPIE"
            + $" -specs=\"{switchSpecs}\""
            + (Directory.Exists(portlibsLib) ? $" -L\"{portlibsLib}\"" : "")
            + $" -L\"{libnxLib}\" -lnx -lm"
            + " -Wl,--gc-sections";

        // Windows caps a command line at ~32 KB — use a response file for many objects.
        string? responseFile = null;
        if (linkArgs.Length > 28000)
        {
            responseFile = Path.Combine(Path.GetTempPath(), $"cs2sx_link_{Environment.ProcessId}.rsp");
            File.WriteAllText(responseFile, linkArgs.Replace("\\", "/"));
        }
        try
        {
            ProcessRunner.Run(gcc, responseFile != null ? $"@\"{responseFile}\"" : linkArgs, "GCC (link)");
        }
        catch (Exception ex) when (diagnostics != null)
        {
            throw new GccCompileException(diagnostics.MapGccErrors(ex.Message, includeDir), ex);
        }
        finally
        {
            if (responseFile != null) TryDelete(responseFile);
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private sealed record Unit(string Source, string? LibName);

    /// Generated TUs live in cs2sx_out and have unique names; extern-lib sources may
    /// share a file name (two libs with util.c), so they get a lib + path-hash suffix.
    private static string ObjectName(Unit u) =>
        u.LibName == null
            ? Path.GetFileNameWithoutExtension(u.Source) + ".o"
            : $"ext_{Sanitize(u.LibName)}_{Path.GetFileNameWithoutExtension(u.Source)}_{Hash(u.Source)[..8]}.o";

    private static string Sanitize(string s) =>
        new(s.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());

    /// Hash over flags + source + every dependency listed in the .d file.
    private static string InputKey(string source, string depFile, string flagsHash, FileHashCache hashes)
    {
        var sb = new StringBuilder(flagsHash);
        sb.Append('|').Append(hashes.Get(source));
        if (File.Exists(depFile))
            foreach (var dep in ParseDeps(depFile).Where(d => !PathEquals(d, source)))
                sb.Append('|').Append(dep).Append('=').Append(hashes.Get(dep));
        return Hash(sb.ToString());
    }

    /// Parses a make-style dependency file written by gcc -MMD:
    /// "obj.o: src.c dir/a.h \<newline> dir\ with\ space/b.h"
    internal static IEnumerable<string> ParseDeps(string depFile)
    {
        var text = File.ReadAllText(depFile).Replace("\\\r\n", " ").Replace("\\\n", " ");
        // The target ends at the first ": " (a drive letter "C:/" is not followed by a space).
        var colon = text.IndexOf(": ", StringComparison.Ordinal);
        if (colon < 0) yield break;
        var rest = text[(colon + 2)..];

        var cur = new StringBuilder();
        for (int i = 0; i < rest.Length; i++)
        {
            char c = rest[i];
            if (c == '\\' && i + 1 < rest.Length && rest[i + 1] == ' ') { cur.Append(' '); i++; }
            else if (char.IsWhiteSpace(c))
            {
                if (cur.Length > 0) { yield return cur.ToString(); cur.Clear(); }
            }
            else cur.Append(c);
        }
        if (cur.Length > 0) yield return cur.ToString();
    }

    private static bool PathEquals(string a, string b) =>
        string.Equals(Path.GetFullPath(a), Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase);

    private static string Hash(string s) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s)));

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }

    /// Content hashes of files, computed at most once per build.
    private sealed class FileHashCache
    {
        private readonly ConcurrentDictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);

        public string Get(string path) => _cache.GetOrAdd(Path.GetFullPath(path), p =>
        {
            try { return Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))); }
            catch { return "missing"; }   // deleted dependency → key changes → rebuild
        });
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
