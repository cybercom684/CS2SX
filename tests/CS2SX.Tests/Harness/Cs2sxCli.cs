using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace CS2SX.Tests.Harness;

/// Result of one cs2sx CLI invocation. Output is ANSI-stripped stdout+stderr.
public sealed record CliResult(int ExitCode, string Output)
{
    public override string ToString() => $"exit={ExitCode}\n{Output}";
}

/// Runs the cs2sx CLI out-of-process. The transpiler keeps global static state
/// (TypeRegistry, OperatorOverloadWriter, …), so every invocation gets its own
/// process — the same way a user runs it.
public static partial class Cs2sxCli
{
    private static readonly string s_cliDll = Path.Combine(AppContext.BaseDirectory, "CS2SX.dll");

    public static CliResult Run(string workingDir, params string[] args)
    {
        if (!File.Exists(s_cliDll))
            throw new FileNotFoundException("CS2SX.dll not found next to the test assembly", s_cliDll);

        var psi = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };
        psi.ArgumentList.Add(s_cliDll);
        foreach (var a in args) psi.ArgumentList.Add(a);
        psi.Environment["NO_COLOR"] = "1";

        using var p = Process.Start(psi)!;
        var stdout = p.StandardOutput.ReadToEndAsync();
        var stderr = p.StandardError.ReadToEndAsync();
        if (!p.WaitForExit(TimeSpan.FromMinutes(5)))
        {
            try { p.Kill(entireProcessTree: true); } catch { }
            throw new TimeoutException($"cs2sx {string.Join(' ', args)} timed out");
        }

        var output = Ansi().Replace(stdout.Result + stderr.Result, "");
        return new CliResult(p.ExitCode, output);
    }

    [GeneratedRegex(@"\x1B\[[0-9;?]*[A-Za-z]")]
    private static partial Regex Ansi();
}
