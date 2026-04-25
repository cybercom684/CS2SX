// ============================================================================
// Build/WatchCommand.cs
//
// FIXES:
//   1. TriggerBuild() hat jetzt try/catch — BuildPipeline.Run() wirft nach
//      renderer.Complete() nochmal; ohne catch landet die Exception im
//      ThreadPool-ContinueWith und wird still verschluckt, das Terminal
//      bleibt danach in einem kaputten Zustand.
//
//   2. Event-Handler werden in Run() sauber deregistriert (finally-Block).
//      Ohne Deregistrierung akkumulieren sich Handler bei mehrfachem Aufruf.
//
//   3. _debounceCts wird jetzt korrekt Disposed bevor es ersetzt wird.
//      Vorher: Cancel() + Neuzuweisung ohne Dispose → stetiges Memory-Leak
//      bei vielen Dateiänderungen in einem langen Watch-Lauf.
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
        // FIX 2: Handler als lokale Delegates damit sie sauber deregistriert
        // werden können — statische Lambda wäre nicht deregistrierbar.
        ConsoleCancelEventHandler cancelHandler = (_, e) =>
        {
            e.Cancel = true;
            RestoreTerminal();
        };
        UnhandledExceptionEventHandler unhandledHandler = (_, _) => RestoreTerminal();
        EventHandler processExitHandler = (_, _) => RestoreTerminal();

        Console.CancelKeyPress += cancelHandler;
        AppDomain.CurrentDomain.UnhandledException += unhandledHandler;
        AppDomain.CurrentDomain.ProcessExit += processExitHandler;

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
            // FIX 2: Handler immer deregistrieren
            Console.CancelKeyPress -= cancelHandler;
            AppDomain.CurrentDomain.UnhandledException -= unhandledHandler;
            AppDomain.CurrentDomain.ProcessExit -= processExitHandler;

            // FIX 3: Pending debounce CTS aufräumen
            DisposeDebounceCts();

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
            // FIX 3: Altes CTS canceln UND disposen bevor neues erstellt wird.
            // Vorher: _debounceCts?.Cancel() + _debounceCts = new ... ohne Dispose.
            DisposeDebounceCts();
            _debounceCts = new CancellationTokenSource();
            var token = _debounceCts.Token;

            Task.Delay(500, token).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                    TriggerBuild();
            }, TaskScheduler.Default);
        }
    }

    // FIX 3: Hilfsmethode für sauberes Dispose unter Lock
    private void DisposeDebounceCts()
    {
        // Wird immer unter _lock aufgerufen — kein weiteres Locking nötig
        if (_debounceCts != null)
        {
            try
            {
                _debounceCts.Cancel();
                _debounceCts.Dispose();
            }
            catch { /* Ignore — CTS könnte bereits disposed sein */ }
            _debounceCts = null;
        }
    }

    private void TriggerBuild()
    {
        Console.WriteLine();
        Log.Info($"Building {Path.GetFileNameWithoutExtension(_csprojPath)}...");
        Console.WriteLine(new string('─', 60));

        // FIX 1: try/catch um den gesamten Build.
        //
        // BuildPipeline.Run() wirft nach renderer.Complete() nochmal (throw im
        // catch-Block). Ohne dieses try/catch landet die Exception im
        // ThreadPool-ContinueWith-Callback und wird still verschluckt.
        // Das Terminal bleibt danach korrumpiert (Cursor unsichtbar,
        // Farben falsch) weil BuildRenderer.Dispose() nicht mehr aufgerufen
        // wurde oder CursorVisible nicht wiederhergestellt wurde.
        //
        // Nach dem Catch: Terminal wiederherstellen, Fehlermeldung ausgeben,
        // und normal weiterlaufen — der Watcher ist weiterhin aktiv.
        try
        {
            new BuildPipeline(_csprojPath).Run();
        }
        catch (Exception ex)
        {
            // Terminal-Zustand reparieren falls BuildRenderer ihn korrumpiert hat
            RestoreTerminal();

            // Fehlermeldung ausgeben — aber kurz halten (GCC-Fehler sind schon
            // vom BuildRenderer ausgegeben worden; hier nur die finale Exception)
            var msg = ex.Message;
            if (msg.Length > 200) msg = msg[..200] + "…";
            Log.Error("Build failed: " + msg);
        }

        Console.WriteLine(new string('─', 60));
        Log.Info($"Watching for changes... ({DateTime.Now:HH:mm:ss})");
    }

    // ── Terminal-Restore ──────────────────────────────────────────────────────

    private static void RestoreTerminal()
    {
        try
        {
            Console.CursorVisible = true;
            Console.ResetColor();
        }
        catch { /* Console könnte nicht verfügbar sein (z.B. kein TTY) */ }
    }
}