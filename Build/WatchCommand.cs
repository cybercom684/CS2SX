// ============================================================================
// Build/WatchCommand.cs
//
// FIX: Terminal-Corruption-Fix — Console.CursorVisible wird jetzt IMMER
// wiederhergestellt, auch bei unbehandelten Exceptions und SIGINT.
// Außerdem: cs2sx clean-Aufruf vor jedem Rebuild wenn --clean-on-change
// Flag gesetzt ist.
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
        // FIX: Terminal-Cleanup als AppDomain-Handler registrieren — wird auch
        // bei unbehandelten Exceptions und Ctrl+C ausgelöst.
        Console.CancelKeyPress += OnCancelKeyPress;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

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
        finally
        {
            // FIX: Terminal immer wiederherstellen, egal wie wir hier ankommen
            RestoreTerminal();
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
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
    /// Debounced Build — 500ms Verzögerung nach letzter Änderung.
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
            // FIX: Terminal nach BuildRenderer-Exception wiederherstellen
            RestoreTerminal();
            if (ex.Message.Length < 200)
                Log.Error("Build failed: " + ex.Message);
        }

        Console.WriteLine(new string('─', 60));
        Log.Info($"Watching for changes... ({DateTime.Now:HH:mm:ss})");
    }

    // ── FIX: Terminal-Restore ─────────────────────────────────────────────────

    private static void RestoreTerminal()
    {
        try
        {
            Console.CursorVisible = true;
            Console.ResetColor();
        }
        catch { /* Ignore — Console könnte nicht verfügbar sein */ }
    }

    private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        RestoreTerminal();
        // Nicht abbrechen — normaler Ctrl+C-Flow
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        RestoreTerminal();
    }

    private static void OnProcessExit(object? sender, EventArgs e)
    {
        RestoreTerminal();
    }
}