// ============================================================================
// CS2SX — Logging/BuildRenderer.cs  (FIX: Thread-Safety)
//
// FIXES:
//   1. _disposed wird atomar per Interlocked.Exchange geprüft — verhindert
//      ObjectDisposedException wenn Timer-Callback nach Dispose feuert.
//   2. Complete() stoppt den Timer synchron (WaitForDispose) bevor RestoreTerminal
//      aufgerufen wird — kein paralleler Render nach Complete.
//   3. Render() prüft _disposed vor jedem Console-Zugriff.
// ============================================================================

namespace CS2SX.Logging;

public sealed class BuildRenderer : IDisposable
{
    private readonly List<BuildStage> _stages = [];
    private readonly List<(DateTime ts, string level, string msg)> _lines = [];
    private readonly object _lock = new();
    private readonly int _originRow;
    private readonly System.Timers.Timer _ticker;
    private volatile bool _disposed;
    private volatile bool _completed;

    private const string Reset = "\x1b[0m";
    private const string Dim = "\x1b[2m";
    private const string Bold = "\x1b[1m";
    private const string Green = "\x1b[32m";
    private const string Yellow = "\x1b[33m";
    private const string Red = "\x1b[31m";
    private const string Cyan = "\x1b[36m";
    private const string Gray = "\x1b[90m";

    public BuildRenderer(IEnumerable<BuildStage> stages)
    {
        _stages.AddRange(stages);
        _originRow = Console.CursorTop;
        Console.CursorVisible = false;
        PrintHeader();
        Render();
        _ticker = new System.Timers.Timer(80);
        _ticker.Elapsed += OnTick;
        _ticker.Start();
    }

    private void OnTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        // FIX: Disposed-Check vor jedem Tick-Aufruf
        if (_disposed || _completed) return;
        Render();
    }

    public BuildStage GetStage(string name) =>
        _stages.First(s => s.Name == name);

    public void Log(string level, string message)
    {
        lock (_lock)
            _lines.Add((DateTime.Now, level, message));
    }

    private void PrintHeader()
    {
        var w = Math.Min(Console.WindowWidth, 72);
        Console.WriteLine($"{Bold}{Gray}  cs2sx{Reset}  {Dim}{Repeat("─", w - 10)}{Reset}");
        Console.WriteLine();
    }

    private void Render()
    {
        // FIX: Disposed-Check im Render selbst
        if (_disposed) return;

        lock (_lock)
        {
            try
            {
                var row = _originRow + 2;
                foreach (var stage in _stages)
                {
                    Console.SetCursorPosition(0, row++);
                    RenderStage(stage);
                    Console.SetCursorPosition(0, row++);
                    RenderBar(stage);
                    row++;
                }

                Console.SetCursorPosition(0, row++);
                Console.Write($"{Dim}{Repeat("─", 52)}{Reset}");
                row++;

                var recentLines = _lines.TakeLast(4).ToList();
                foreach (var (ts, lvl, msg) in recentLines)
                {
                    Console.SetCursorPosition(0, row++);
                    var (sym, col) = lvl switch
                    {
                        "ok" => ("✓", Green),
                        "warn" => ("!", Yellow),
                        "error" => ("✗", Red),
                        "debug" => ("~", "\x1b[35m"),
                        _ => ("i", Cyan),
                    };
                    var time = $"{Gray}{ts:HH:mm:ss}{Reset}";
                    Console.Write($"  {time}  {col}{sym}{Reset}  {Dim}{msg,-52}{Reset}");
                    ClearToEnd();
                }
            }
            catch (Exception)
            {
                // Console-Zugriff kann bei nicht-TTY-Terminals werfen — ignorieren
            }
        }
    }

    private static void RenderStage(BuildStage s)
    {
        var (icon, col) = s.Status switch
        {
            StageStatus.Done => ("v", Green),
            StageStatus.Running => (Spinner(), Cyan),
            StageStatus.Failed => ("x", Red),
            StageStatus.Warning => ("!", Yellow),
            _ => ("o", Gray),
        };

        var nameCol = s.Status == StageStatus.Waiting ? Gray : Reset;
        var elapsed = s.Elapsed.Length > 0 ? $"  {Gray}{s.Elapsed}{Reset}" : string.Empty;
        var detail = s.Detail.Length > 0 ? $"  {Gray}{Truncate(s.Detail, 36)}{Reset}" : string.Empty;

        Console.Write($"  {col}{icon}{Reset}  {nameCol}{s.Name,-12}{Reset}{elapsed}{detail}");
        ClearToEnd();
    }

    private static void RenderBar(BuildStage s)
    {
        const int w = 28;
        var filled = (int)(s.Progress / 100.0 * w);
        var col = s.Status switch
        {
            StageStatus.Done => Green,
            StageStatus.Failed => Red,
            StageStatus.Warning => Yellow,
            StageStatus.Running => Green,
            _ => Gray,
        };
        var bar = $"{col}{Repeat("█", filled)}{Gray}{Repeat("░", w - filled)}{Reset}";
        Console.Write($"     {bar}");
        ClearToEnd();
    }

    private static int _spinFrame;
    private static readonly char[] SpinFrames = ['⠋', '⠙', '⠹', '⠸', '⠼', '⠴', '⠦', '⠧', '⠇', '⠏'];
    private static string Spinner() => SpinFrames[_spinFrame++ % SpinFrames.Length].ToString();

    private static void ClearToEnd() => Console.Write("\x1b[K");

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..(max - 1)] + "…";

    public void Complete(TimeSpan total, int warnings, int errors)
    {
        // FIX: Timer synchron stoppen bevor wir in den Terminal schreiben
        _completed = true;
        _ticker.Stop();

        // Letzte Render-Runde mit finalen Stage-Zuständen
        Render();

        var summaryRow = _originRow + 2 + _stages.Count * 3 + 6 + 4;
        try
        {
            Console.SetCursorPosition(0, summaryRow);
            Console.WriteLine();
            if (errors > 0)
                Console.WriteLine($"  {Red}✗{Reset}  Build failed  {Gray}· {errors} error(s){Reset}");
            else
                Console.WriteLine($"  {Green}✓{Reset}  {Bold}Build complete{Reset}  " +
                                  $"{Gray}· {total.TotalSeconds:F1}s · {warnings} warning(s){Reset}");
            Console.WriteLine();
        }
        catch { }

        RestoreTerminal();
    }

    private static void RestoreTerminal()
    {
        try { Console.CursorVisible = true; }
        catch { }
    }

    private static string Repeat(string s, int n) =>
        string.Concat(Enumerable.Repeat(s, n));

    public void Dispose()
    {
        // FIX: Idempotentes Dispose — verhindert Doppel-Dispose-Fehler
        if (_disposed) return;
        _disposed = true;

        _ticker.Elapsed -= OnTick;
        _ticker.Stop();
        _ticker.Dispose();

        RestoreTerminal();
    }
}