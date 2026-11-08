using CS2SX.Tests.Harness;
using Xunit;

namespace CS2SX.Tests;

/// Builds each case with `expect-output.txt`, runs the .nro in Ryujinx and compares
/// the first lines it prints, in order, with that file.
///
/// Needs devkitPro and CS2SX_RYUJINX (path to Ryujinx.exe); skipped otherwise.
/// CS2SX_UPDATE_OUTPUT=1 records the actual output into expect-output.txt (for every
/// case that prints something) — review it before committing.
public class RuntimeTests
{
    private static bool UpdateMode => Environment.GetEnvironmentVariable("CS2SX_UPDATE_OUTPUT") == "1";

    private static string ExpectFile(TestCase c) => Path.Combine(c.SourceDir, "expect-output.txt");

    public static IEnumerable<object[]> Cases => TestCase.All(c =>
        File.Exists(ExpectFile(c))
        || (UpdateMode && !c.ExpectsErrors && !File.Exists(Path.Combine(c.SourceDir, "known-issue.txt"))));

    [RuntimeTheory]
    [MemberData(nameof(Cases))]
    public void Runs_and_prints_expected_output(string name)
    {
        var tc = TestCase.Get(name);
        var csproj = tc.CreateProject("runtime");
        var projectDir = Path.GetDirectoryName(csproj)!;

        var build = Cs2sxCli.RunWithEnv(projectDir,
            new Dictionary<string, string> { ["CS2SX_TEST_OUTPUT"] = "1" }, "build", csproj);
        Assert.True(build.ExitCode == 0, $"build failed:\n{build}");
        var nro = Path.Combine(projectDir, name + ".nro");

        if (UpdateMode)
        {
            var recorded = Emulator.Run(nro, int.MaxValue, TimeSpan.FromSeconds(20));
            if (recorded.Count > 0)
                File.WriteAllLines(ExpectFile(tc), recorded);
            return;
        }

        // The file holds the first lines of the output: apps keep running in their
        // frame loop, and some print every frame.
        var expected = File.ReadAllLines(ExpectFile(tc)).ToList();
        var actual = Emulator.Run(nro, expected.Count, TimeSpan.FromSeconds(45));
        Assert.True(expected.SequenceEqual(actual.Take(expected.Count)),
            $"output differs\n--- expected ---\n{string.Join('\n', expected)}\n--- actual ---\n{string.Join('\n', actual)}");
    }
}
