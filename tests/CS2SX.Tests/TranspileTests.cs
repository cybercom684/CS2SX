using CS2SX.Tests.Harness;
using Xunit;

namespace CS2SX.Tests;

/// Every case must transpile without errors, and the generated C must match its
/// approved snapshot. Runs without devkitPro.
public class TranspileTests
{
    public static IEnumerable<object[]> Cases => TestCase.All();

    [Theory]
    [MemberData(nameof(Cases))]
    public void Transpiles_and_matches_snapshot(string name)
    {
        var tc = TestCase.Get(name);
        var csproj = tc.CreateProject("transpile");
        var projectDir = Path.GetDirectoryName(csproj)!;

        var result = Cs2sxCli.Run(projectDir, "build", csproj, "--transpile-only");
        Assert.True(result.ExitCode == 0, $"transpile failed:\n{result}");

        var buildDir = Path.Combine(projectDir, "cs2sx_out");
        foreach (var cs in tc.SourceFiles)
        {
            var c = File.ReadAllText(Path.Combine(buildDir, Path.GetFileNameWithoutExtension(cs) + ".c"));
            Assert.DoesNotContain("UNSUPPORTED", c);
        }

        Snapshot.Verify(tc, buildDir);
    }
}
