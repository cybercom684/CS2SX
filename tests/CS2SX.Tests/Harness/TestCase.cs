namespace CS2SX.Tests.Harness;

/// A test case is a folder under tests/CS2SX.Tests/Cases/ containing a small
/// Switch program (*.cs, optionally *.h and cs2sx.json). Optional files:
///   __snapshots__/       approved generated C (one .c/.h per source file)
///   expect-warnings.txt  substrings that must appear in `cs2sx check` output
public sealed class TestCase
{
    public string Name { get; }
    public string SourceDir { get; }
    public string SnapshotDir => Path.Combine(SourceDir, "__snapshots__");

    private TestCase(string name, string dir)
    {
        Name = name;
        SourceDir = dir;
    }

    public IEnumerable<string> SourceFiles =>
        Directory.GetFiles(SourceDir, "*.cs").OrderBy(f => f, StringComparer.Ordinal);

    public override string ToString() => Name;

    /// `expect-errors.txt`: the case must be rejected; each non-comment line is a
    /// substring of an error CS2SX reports (the C compiler must not run).
    public string ExpectErrorsFile => Path.Combine(SourceDir, "expect-errors.txt");
    public bool ExpectsErrors => File.Exists(ExpectErrorsFile);
    public IEnumerable<string> ExpectedErrors => !ExpectsErrors ? Array.Empty<string>()
        : File.ReadAllLines(ExpectErrorsFile).Select(l => l.Trim())
            .Where(l => l.Length > 0 && !l.StartsWith('#'));

    public static string CasesRoot
    {
        get
        {
            // Snapshots are written back into the source tree, so resolve the real
            // source folder (not the copy in bin/).
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "CS2SX.sln")))
                dir = dir.Parent;
            if (dir == null)
                throw new DirectoryNotFoundException("Repository root (CS2SX.sln) not found");
            return Path.Combine(dir.FullName, "tests", "CS2SX.Tests", "Cases");
        }
    }

    public static TestCase Get(string name) => new(name, Path.Combine(CasesRoot, name));

    public static IEnumerable<object[]> All(Func<TestCase, bool>? filter = null) =>
        Directory.GetDirectories(CasesRoot)
            .OrderBy(d => d, StringComparer.Ordinal)
            .Select(d => new TestCase(Path.GetFileName(d), d))
            .Where(c => filter == null || filter(c))
            .Select(c => new object[] { c.Name });

    /// Copies the case into a fresh, isolated project folder and returns the .csproj path.
    /// <paramref name="flavor"/> keeps transpile-only and full builds apart.
    public string CreateProject(string flavor)
    {
        var root = Path.Combine(Path.GetTempPath(), "cs2sx-tests", flavor, Name);
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        Directory.CreateDirectory(root);

        foreach (var f in Directory.GetFiles(SourceDir))
        {
            var ext = Path.GetExtension(f);
            if (ext is ".cs" or ".h" or ".json" or ".c")
                File.Copy(f, Path.Combine(root, Path.GetFileName(f)));
        }

        var csproj = Path.Combine(root, Name + ".csproj");
        File.WriteAllText(csproj, """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <Nullable>enable</Nullable>
              </PropertyGroup>
            </Project>
            """);

        var config = Path.Combine(root, "cs2sx.json");
        if (!File.Exists(config))
            File.WriteAllText(config, $$"""
                { "name": "{{Name}}", "author": "cs2sx-tests", "version": "1.0.0" }
                """);

        return csproj;
    }
}
