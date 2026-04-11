// ============================================================================
// Build/WatchCommand.cs
//
// PHASE 4: cs2sx watch <project.csproj>
// Überwacht .cs-Dateien und löst automatisch einen Build aus wenn
// Änderungen erkannt werden. Debounced mit 500ms Verzögerung.
// ============================================================================

using CS2SX.Logging;

namespace CS2SX.Build;

public sealed class WatchCommand
{
    private readonly string _csprojPath;
    private readonly string _projectDir;
    private CancellationTokenSource? _debounceCts;
    private readonly object _lock = new();

    public WatchCommand(string csprojPath)
    {
        _csprojPath = Path.GetFullPath(csprojPath);
        _projectDir = Path.GetDirectoryName(_csprojPath)
            ?? throw new ArgumentException("Ungültiger Pfad", nameof(csprojPath));
    }

    public async Task Run(CancellationToken cancellationToken = default)
    {
        Log.Info($"Watching: {_projectDir}");
        Log.Info("Press Ctrl+C to stop.");
        Console.WriteLine();

        // Initial build
        TriggerBuild();

        using var watcher = new FileSystemWatcher(_projectDir, "*.cs")
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            EnableRaisingEvents = true,
        };

        // Auch .h-Dateien überwachen (Custom Headers)
        using var hWatcher = new FileSystemWatcher(_projectDir, "*.h")
        {
            IncludeSubdirectories = false,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            EnableRaisingEvents = true,
        };

        watcher.Changed += OnFileChanged;
        watcher.Created += OnFileChanged;
        watcher.Deleted += OnFileChanged;
        watcher.Renamed += OnFileRenamed;

        hWatcher.Changed += OnFileChanged;
        hWatcher.Created += OnFileChanged;

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Log.Info("Watch stopped.");
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        // cs2sx_out Verzeichnis ignorieren — generierte Dateien
        if (e.FullPath.Contains("cs2sx_out")) return;
        if (e.FullPath.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar)) return;
        if (e.FullPath.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)) return;

        Log.Info($"Changed: {Path.GetFileName(e.FullPath)}");
        ScheduleBuild();
    }

    private void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        if (e.FullPath.Contains("cs2sx_out")) return;
        Log.Info($"Renamed: {Path.GetFileName(e.FullPath)}");
        ScheduleBuild();
    }

    /// <summary>
    /// Debounced Build — wartet 500ms nach der letzten Änderung
    /// bevor der Build gestartet wird. Verhindert mehrfache Builds
    /// beim Speichern mehrerer Dateien gleichzeitig.
    /// </summary>
    private void ScheduleBuild()
    {
        lock (_lock)
        {
            _debounceCts?.Cancel();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            Task.Delay(500, token).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                    TriggerBuild();
            }, TaskScheduler.Default);
        }
    }

    private void TriggerBuild()
    {
        Console.WriteLine();
        Log.Info($"Building {Path.GetFileNameWithoutExtension(_csprojPath)}...");
        Console.WriteLine(new string('─', 60));

        try
        {
            new BuildPipeline(_csprojPath).Run();
        }
        catch (Exception ex)
        {
            // BuildPipeline loggt bereits den Fehler
            // Wir loggen nur wenn es kein bekannter Build-Fehler ist
            if (ex.Message.Length < 200)
                Log.Error("Build failed: " + ex.Message);
        }

        Console.WriteLine(new string('─', 60));
        Log.Info($"Watching for changes... ({DateTime.Now:HH:mm:ss})");
    }
}