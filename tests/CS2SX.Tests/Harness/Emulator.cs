using System.Diagnostics;
using System.Text;
using Xunit;

namespace CS2SX.Tests.Harness;

/// Runs a built .nro in Ryujinx (headless) and returns what the app printed.
///
/// The app is built with CS2SX_TEST_OUTPUT=1, so the runtime mirrors every stdout
/// line to svcOutputDebugString; Ryujinx logs those as
/// "KernelSvc OutputDebugString: &lt;line&gt;" on its stdout.
///
/// Set CS2SX_RYUJINX to the path of Ryujinx.exe (1.3+ with --headless) to enable
/// the runtime tests; without it they are skipped.
public static class Emulator
{
    private const string Marker = "KernelSvc OutputDebugString: ";

    // One emulator at a time: several in parallel are slow and flaky.
    private static readonly SemaphoreSlim s_gate = new(1, 1);

    public static string? RyujinxPath
    {
        get
        {
            var p = Environment.GetEnvironmentVariable("CS2SX_RYUJINX");
            return !string.IsNullOrEmpty(p) && File.Exists(p) ? p : null;
        }
    }

    public static bool Available => RyujinxPath != null;

    /// Starts the .nro and collects its output lines until <paramref name="expectedLines"/>
    /// lines have arrived (the app keeps running in its frame loop) or the timeout ends.
    public static List<string> Run(string nroPath, int expectedLines, TimeSpan timeout)
    {
        s_gate.Wait();
        try { return RunCore(nroPath, expectedLines, timeout); }
        finally { s_gate.Release(); }
    }

    private static List<string> RunCore(string nroPath, int expectedLines, TimeSpan timeout)
    {
        var psi = new ProcessStartInfo(RyujinxPath!)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
        };
        psi.ArgumentList.Add("--headless");
        psi.ArgumentList.Add(nroPath);

        var lines = new List<string>();
        var enough = new ManualResetEventSlim();
        using var p = new Process { StartInfo = psi };
        p.OutputDataReceived += (_, e) =>
        {
            if (e.Data == null) return;
            var i = e.Data.IndexOf(Marker, StringComparison.Ordinal);
            if (i < 0) return;
            lock (lines)
            {
                lines.Add(e.Data[(i + Marker.Length)..].TrimEnd());
                if (lines.Count >= expectedLines) enough.Set();
            }
        };
        p.ErrorDataReceived += (_, _) => { };
        p.Start();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        try
        {
            enough.Wait(timeout);
            // Let a few more lines arrive, so extra output shows up as a failure.
            Thread.Sleep(500);
        }
        finally
        {
            try { p.Kill(entireProcessTree: true); } catch { }
            p.WaitForExit(5000);
        }
        lock (lines) return lines.ToList();
    }
}

/// A theory that needs devkitPro and Ryujinx (CS2SX_RYUJINX); skipped otherwise.
public sealed class RuntimeTheoryAttribute : TheoryAttribute
{
    public RuntimeTheoryAttribute()
    {
        if (!Toolchain.Available)
            Skip = "devkitPro (devkitA64) not installed";
        else if (!Emulator.Available)
            Skip = "set CS2SX_RYUJINX to Ryujinx.exe to run the runtime tests";
    }
}
