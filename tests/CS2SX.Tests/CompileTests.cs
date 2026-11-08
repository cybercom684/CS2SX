using CS2SX.Tests.Harness;
using Xunit;

namespace CS2SX.Tests;

/// Full builds through devkitA64 GCC to a packaged .nro.
///
/// A case may contain `known-issue.txt` (why it does not build yet). Such a case is
/// expected to FAIL; once a fix makes it build, the test turns red as a reminder to
/// delete the marker so the case joins the regular suite.
public class CompileTests
{
    public static IEnumerable<object[]> Cases => TestCase.All();

    [ToolchainTheory]
    [MemberData(nameof(Cases))]
    public void Builds_nro(string name)
    {
        var tc = TestCase.Get(name);
        var csproj = tc.CreateProject("compile");
        var projectDir = Path.GetDirectoryName(csproj)!;
        var result = Cs2sxCli.Run(projectDir, "build", csproj);
        var nro = Path.Combine(projectDir, name + ".nro");

        if (tc.ExpectsErrors)
        {
            // Rejected before GCC: no compiler output, no .nro.
            Assert.True(result.ExitCode != 0, $"'{name}' must be rejected:\n{result}");
            Assert.Contains("the C compiler was not run", result.Output);
            Assert.False(File.Exists(nro));
            return;
        }

        var knownIssue = Path.Combine(tc.SourceDir, "known-issue.txt");
        if (File.Exists(knownIssue))
        {
            Assert.True(result.ExitCode != 0,
                $"'{name}' builds now — delete Cases/{name}/known-issue.txt so it is tested normally.");
            return;
        }

        Assert.True(result.ExitCode == 0, $"build failed:\n{result}");
        Assert.True(File.Exists(nro), $"{Path.GetFileName(nro)} was not produced:\n{result}");
    }
}
