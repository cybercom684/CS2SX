using System.Text.RegularExpressions;
using CS2SX.Logging;

namespace CS2SX.Build;

/// <summary>
/// <c>cs2sx migrate &lt;project&gt; [--dry-run]</c> — moves a project created by an older
/// cs2sx (stub files copied into the project) to the CS2SX.Switch package:
///   • deletes the copied stub files (first line "// CS2SX Stub") and the copied LibNX/ folder
///   • adds the CS2SX.Switch PackageReference (+ nuget.config for the bundled package)
///   • reports source lines that need a manual change because the API was cleaned up
/// Idempotent: running it on a migrated project changes nothing.
/// </summary>
public sealed class MigrateCommand
{
    private readonly string _target;
    private readonly bool _dryRun;

    public MigrateCommand(string target, bool dryRun)
    {
        _target = target;
        _dryRun = dryRun;
    }

    // Source patterns whose meaning changed with the CS2SX.Switch API.
    private static readonly (Regex Pattern, string Hint)[] s_apiChanges =
    {
        (new Regex(@"\bSystem\.(GetBattery|GetTime)\s*\("), "System.GetBattery/GetTime → NX.GetBattery/NX.GetTime (a class named System shadowed the System namespace)"),
        (new Regex(@"\bDirectory\.GetDirectories\s*\("), "Directory.GetDirectories returns string[] now (like .NET): use .Length instead of .Count"),
        (new Regex(@"using\s+CS2SX\.SwitchFormsLib\s*;"), "remove `using CS2SX.SwitchFormsLib;` — the API lives in CS2SX.Switch (imported automatically)"),
        (new Regex(@"\busing\s+System\.IO\s*;"), "remove `using System.IO;` — File/Directory/Path come from CS2SX.Switch (the SD card)"),
    };

    public int Run()
    {
        var csproj = ResolveCsproj(_target);
        if (csproj == null) return 1;
        var projectDir = Path.GetDirectoryName(csproj)!;
        var prefix = _dryRun ? "(dry run) would " : "";
        int changes = 0;

        Log.Info($"Migrating {Path.GetFileName(csproj)} to {AppProjectTemplate.ApiPackageId} {AppProjectTemplate.ToolVersion}");

        // 1) Copied stub files and the LibNX copy.
        foreach (var file in Directory.GetFiles(projectDir, "*.cs"))
        {
            if (!IsLegacyStub(file)) continue;
            Log.Ok($"  {prefix}delete  {Path.GetFileName(file)} (copied stub)");
            if (!_dryRun) File.Delete(file);
            changes++;
        }
        var libnx = Path.Combine(projectDir, "LibNX");
        if (Directory.Exists(libnx) && LooksLikeLibNxCopy(libnx))
        {
            Log.Ok($"  {prefix}delete  LibNX/ (copied libnx stubs)");
            if (!_dryRun) Directory.Delete(libnx, recursive: true);
            changes++;
        }

        // 2) Package reference, package source, gitignore.
        if (!_dryRun)
        {
            if (AppProjectTemplate.EnsurePackageReference(csproj))
            {
                Log.Ok($"  add     PackageReference {AppProjectTemplate.ApiPackageId} {AppProjectTemplate.ToolVersion}");
                changes++;
            }
        }
        else if (!File.ReadAllText(csproj).Contains(AppProjectTemplate.ApiPackageId))
        {
            Log.Ok($"  {prefix}add PackageReference {AppProjectTemplate.ApiPackageId}");
            changes++;
        }

        var nugetConfigPath = Path.Combine(projectDir, "nuget.config");
        var nugetConfig = AppProjectTemplate.NugetConfig();
        if (nugetConfig != null && !File.Exists(nugetConfigPath))
        {
            Log.Ok($"  {prefix}add     nuget.config (bundled {AppProjectTemplate.ApiPackageId} package)");
            if (!_dryRun) File.WriteAllText(nugetConfigPath, nugetConfig);
            changes++;
        }

        var gitIgnorePath = Path.Combine(projectDir, ".gitignore");
        if (!File.Exists(gitIgnorePath))
        {
            Log.Ok($"  {prefix}add     .gitignore");
            if (!_dryRun) File.WriteAllText(gitIgnorePath, AppProjectTemplate.GitIgnore);
            changes++;
        }

        // 3) Lines that need a manual edit.
        int hints = 0;
        var reader = new ProjectReader();
        reader.Load(csproj);
        foreach (var src in reader.SourceFiles)
        {
            var lines = File.ReadAllLines(src);
            for (int i = 0; i < lines.Length; i++)
                foreach (var (pattern, hint) in s_apiChanges)
                    if (pattern.IsMatch(lines[i]))
                    {
                        Log.Warning($"{Path.GetRelativePath(projectDir, src)}({i + 1}): {hint}");
                        hints++;
                    }
        }

        if (changes == 0 && hints == 0)
            Log.Ok("Nothing to migrate — the project already uses the CS2SX.Switch package.");
        else
            Log.Ok($"{changes} change(s){(_dryRun ? " planned" : " applied")}, {hints} line(s) to review.");
        return 0;
    }

    private static bool IsLegacyStub(string file)
    {
        try
        {
            using var r = File.OpenText(file);
            return r.ReadLine()?.StartsWith("// CS2SX Stub", StringComparison.Ordinal) == true;
        }
        catch { return false; }
    }

    // The old `cs2sx new` copied Stubs/LibNX/<area>/*.cs — recognise it by its layout,
    // never delete a user folder that merely happens to be called LibNX.
    private static bool LooksLikeLibNxCopy(string dir)
    {
        var known = new[] { "services", "kernel", "applets", "display" };
        return known.Count(k => Directory.Exists(Path.Combine(dir, k))) >= 3;
    }

    private static string? ResolveCsproj(string input)
    {
        var full = Path.GetFullPath(input);
        if (File.Exists(full) && full.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            return full;
        if (Directory.Exists(full))
        {
            var found = Directory.GetFiles(full, "*.csproj").FirstOrDefault();
            if (found != null) return found;
        }
        Log.Error($"No .csproj found at: {full}");
        return null;
    }
}
