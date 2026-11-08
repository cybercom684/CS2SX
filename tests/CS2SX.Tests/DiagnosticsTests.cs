using CS2SX.Tests.Harness;
using Xunit;

namespace CS2SX.Tests;

/// Cases with `expect-warnings.txt` list substrings (one per line) that
/// `cs2sx check` must report — guards the early "footgun" diagnostics.
public class DiagnosticsTests
{
    private static string ExpectFile(TestCase c) => Path.Combine(c.SourceDir, "expect-warnings.txt");

    public static IEnumerable<object[]> Cases => TestCase.All(c => File.Exists(ExpectFile(c)));

    [Theory]
    [MemberData(nameof(Cases))]
    public void Check_reports_expected_warnings(string name)
    {
        var tc = TestCase.Get(name);
        var csproj = tc.CreateProject("check");
        var result = Cs2sxCli.Run(Path.GetDirectoryName(csproj)!, "check", csproj);

        var expected = File.ReadAllLines(ExpectFile(tc))
            .Select(l => l.Trim())
            .Where(l => l.Length > 0 && !l.StartsWith('#'));

        foreach (var e in expected)
            Assert.True(result.Output.Contains(e, StringComparison.Ordinal),
                $"expected warning containing \"{e}\" not reported:\n{result}");
    }

    public static IEnumerable<object[]> ErrorCases => TestCase.All(c => c.ExpectsErrors);

    [Theory]
    [MemberData(nameof(ErrorCases))]
    public void Check_reports_expected_errors(string name)
    {
        var tc = TestCase.Get(name);
        var csproj = tc.CreateProject("check");
        var result = Cs2sxCli.Run(Path.GetDirectoryName(csproj)!, "check", csproj);

        Assert.True(result.ExitCode != 0, $"check must fail for '{name}':\n{result}");
        foreach (var e in tc.ExpectedErrors)
            Assert.True(result.Output.Contains(e, StringComparison.Ordinal),
                $"expected error containing \"{e}\" not reported:\n{result}");
    }
}
