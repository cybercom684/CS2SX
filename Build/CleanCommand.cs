// ============================================================================
// Build/CleanCommand.cs  (NEU)
//
// cs2sx clean <project.csproj>
//
// Löscht das cs2sx_out/-Verzeichnis vollständig.
// Verhindert Ghost-Symbol-Konflikte nach Klassen-Umbenennungen.
// ============================================================================

using CS2SX.Logging;

namespace CS2SX.Build;

public sealed class CleanCommand
{
    private readonly string _projectDir;

    public CleanCommand(string csprojPath)
    {
        _projectDir = Path.GetDirectoryName(Path.GetFullPath(csprojPath))
            ?? throw new ArgumentException("Ungültiger Pfad", nameof(csprojPath));
    }

    public int Run()
    {
        var buildDir = Path.Combine(_projectDir, "cs2sx_out");

        if (!Directory.Exists(buildDir))
        {
            Log.Info("cs2sx_out does not exist — nothing to clean.");
            return 0;
        }

        int deleted = 0;
        int failed = 0;

        foreach (var file in Directory.GetFiles(buildDir, "*", SearchOption.AllDirectories))
        {
            try
            {
                File.Delete(file);
                deleted++;
            }
            catch (Exception ex)
            {
                Log.Warning($"Cannot delete {Path.GetFileName(file)}: {ex.Message}");
                failed++;
            }
        }

        // Leere Unterverzeichnisse entfernen
        foreach (var dir in Directory.GetDirectories(buildDir, "*", SearchOption.AllDirectories)
            .OrderByDescending(d => d.Length)) // tiefste zuerst
        {
            try { Directory.Delete(dir); } catch { }
        }

        if (failed == 0)
        {
            Log.Ok($"Clean complete: {deleted} file(s) removed from cs2sx_out");
            return 0;
        }
        else
        {
            Log.Warning($"Clean: {deleted} removed, {failed} could not be deleted");
            return 1;
        }
    }
}