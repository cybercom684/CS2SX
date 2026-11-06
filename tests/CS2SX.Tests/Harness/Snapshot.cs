using System.Text;
using Xunit;

namespace CS2SX.Tests.Harness;

/// Approval-style snapshots of the generated C for a test case.
///
/// Set CS2SX_UPDATE_SNAPSHOTS=1 to (re)write snapshots from the current output;
/// review the resulting git diff like any other code change. A missing snapshot
/// is written and the test fails once, so new cases never pass unreviewed.
public static class Snapshot
{
    public static bool UpdateMode =>
        Environment.GetEnvironmentVariable("CS2SX_UPDATE_SNAPSHOTS") is "1" or "true";

    public static void Verify(TestCase testCase, string buildDir)
    {
        var failures = new List<string>();
        var created = new List<string>();
        Directory.CreateDirectory(testCase.SnapshotDir);

        foreach (var cs in testCase.SourceFiles)
        {
            var baseName = Path.GetFileNameWithoutExtension(cs);
            foreach (var ext in new[] { ".h", ".c" })
            {
                var generated = Path.Combine(buildDir, baseName + ext);
                Assert.True(File.Exists(generated), $"{baseName}{ext} was not generated");

                var actual = Normalize(File.ReadAllText(generated));
                var snapshotPath = Path.Combine(testCase.SnapshotDir, baseName + ext);

                if (UpdateMode || !File.Exists(snapshotPath))
                {
                    var existed = File.Exists(snapshotPath);
                    if (!existed || Normalize(File.ReadAllText(snapshotPath)) != actual)
                        File.WriteAllText(snapshotPath, actual, new UTF8Encoding(false));
                    if (!existed && !UpdateMode) created.Add(baseName + ext);
                    continue;
                }

                var expected = Normalize(File.ReadAllText(snapshotPath));
                if (expected != actual)
                    failures.Add(FirstDifference(baseName + ext, expected, actual));
            }
        }

        if (created.Count > 0)
            Assert.Fail($"New snapshot(s) written for review: {string.Join(", ", created)}");
        if (failures.Count > 0)
            Assert.Fail("Generated C differs from snapshot (run with CS2SX_UPDATE_SNAPSHOTS=1 "
                + "to accept):\n" + string.Join("\n", failures));
    }

    private static string Normalize(string s) =>
        s.Replace("\r\n", "\n").TrimEnd() + "\n";

    private static string FirstDifference(string file, string expected, string actual)
    {
        var e = expected.Split('\n');
        var a = actual.Split('\n');
        for (int i = 0; i < Math.Max(e.Length, a.Length); i++)
        {
            var el = i < e.Length ? e[i] : "<eof>";
            var al = i < a.Length ? a[i] : "<eof>";
            if (el != al)
                return $"  {file}:{i + 1}\n    expected: {el}\n    actual:   {al}";
        }
        return $"  {file}: differs";
    }
}
