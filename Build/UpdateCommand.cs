using CS2SX.Logging;

namespace CS2SX.Build;

/// <summary>
/// cs2sx update &lt;project&gt; — overwrites SwitchApp.cs, Input.cs and LibNX stubs
/// with the versions embedded in the current tool binary.
/// Safe to run on any existing CS2SX project.
/// </summary>
public sealed class UpdateCommand
{
    private readonly string _target;

    public UpdateCommand(string target)
    {
        _target = target;
    }

    public int Run()
    {
        var projectDir = ResolveProjectDir(_target);
        if (projectDir == null) return 1;

        Log.Info($"Updating stubs in: {projectDir}");

        IReadOnlyList<string> added, updated;
        try
        {
            (added, updated) = RuntimeExporter.ExportStubs(projectDir);
        }
        catch (Exception ex)
        {
            Log.Error($"Stub export failed: {ex.Message}");
            return 1;
        }

        foreach (var f in updated)
            Log.Ok($"  updated  {Path.GetRelativePath(projectDir, f)}");
        foreach (var f in added)
            Log.Ok($"  added    {Path.GetRelativePath(projectDir, f)}");

        Log.Ok($"Stubs updated ({updated.Count + added.Count} file(s)).");
        Log.Info("Run 'cs2sx build' to rebuild with the updated APIs.");
        return 0;
    }

    private static string? ResolveProjectDir(string input)
    {
        var full = Path.GetFullPath(input);

        if (Directory.Exists(full))
            return full;

        if (File.Exists(full) && full.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            return Path.GetDirectoryName(full);

        Log.Error($"Not found: {full}");
        return null;
    }
}
