// Datei: Logging/BuildRenderer.cs
//
// REDESIGN in dieser Version:
//   – EnableAnsi()  aktiviert ENABLE_VIRTUAL_TERMINAL_PROCESSING auf Windows,
//     damit ANSI-Codes in cmd.exe und PowerShell 5 korrekt gerendert werden.
//   – Live-Render zeigt nur die Stage-Zeilen (1 Zeile / Stage, kein Balken).
//   – Log-Meldungen werden NICHT mehr live in ein fixes Fenster gerendert,
//     sondern nach Complete() normal mit WriteLine ausgegeben → kein Garbling.
//   – Complete() positioniert den Cursor unterhalb der Stage-Area und druckt
//     Separator + alle gesammelten Log-Zeilen + Summary per WriteLine.

using System.Runtime.InteropServices;

namespace CS2SX.Logging;

public sealed class BuildRenderer : IDisposable
{
    private readonly List<BuildStage> _stages = [];
    private readonly List<(DateTime ts, string level, string msg)> _lines = [];
    private readonly object _lock = new();
    private readonly int _originRow;
    private readonly System.Timers.Timer _ticker;
    private int _disposed;           // 0 = live, 1 = disposed
    private volatile bool _completed;

    // ── ANSI-Codes ────────────────────────────────────────────────────────
    private const string Reset  = "\x1b[0m";
    private const string Dim    = "\x1b[2m";
    private const string Bold   = "\x1b[1m";
    private const string Green  = "\x1b[32m";
    private const string Yellow = "\x1b[33m";
    private const string Red    = "\x1b[31m";
    private const string Cyan   = "\x1b[36m";
    private const string Gray   = "\x1b[90m";
    private const string ClearEol = "\x1b[K";

    // ── Windows VT enablement ─────────────────────────────────────────────
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr h, out uint mode);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr h, uint mode);
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int n);

    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
    private const int  STD_OUTPUT_HANDLE = -11;

    /// <summary>
    /// Aktiviert ANSI-Escape-Verarbeitung auf Windows (cmd.exe, PS 5.1).
    /// Auf anderen Plattformen ist ANSI immer aktiv – kein-op.
    /// </summary>
    public static void EnableAnsi()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
        try
        {
            var h = GetStdHandle(STD_OUTPUT_HANDLE);
            if (GetConsoleMode(h, out var mode))
                SetConsoleMode(h, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
        }
        catch { /* Nicht-TTY – ignorieren */ }
    }

    // ── Konstruktor ───────────────────────────────────────────────────────

    public BuildRenderer(IEnumerable<BuildStage> stages)
    {
        EnableAnsi();
        _stages.AddRange(stages);
        try { _originRow = Console.CursorTop; }
        catch { _originRow = 0; }
        try { Console.CursorVisible = false; }
        catch { }
        PrintHeader();
        Render();
        _ticker = new System.Timers.Timer(80);
        _ticker.Elapsed += OnTick;
        _ticker.Start();
    }

    // ── Öffentliche API ───────────────────────────────────────────────────

    public BuildStage GetStage(string name) =>
        _stages.First(s => s.Name == name);

    public void MarkFirstRunningAsFailed()
    {
        lock (_lock)
        {
            var running = _stages.FirstOrDefault(s => s.Status == StageStatus.Running);
            if (running != null)
                running.Status = StageStatus.Failed;
        }
    }

    public void Log(string level, string message)
    {
        lock (_lock)
            _lines.Add((DateTime.Now, level, message));
    }

    public void Complete(TimeSpan total, int warnings, int errors)
    {
        _completed = true;
        _ticker.Stop();

        // Letzter Render der Stage-Zeilen mit finalen Zuständen
        Render();

        // Cursor direkt unterhalb der Stage-Area positionieren
        var belowStages = _originRow + 2 + _stages.Count;
        try { Console.SetCursorPosition(0, belowStages); }
        catch { }

        try
        {
            var w = Math.Min(Console.WindowWidth - 4, 60);
            Console.WriteLine();
            Console.WriteLine($"  {Dim}{Repeat("─", w)}{Reset}");
            Console.WriteLine();

            // Alle gesammelten Log-Meldungen ausgeben
            lock (_lock)
            {
                foreach (var (ts, lvl, msg) in _lines)
                    WriteLogLine(ts, lvl, msg);
            }

            if (_lines.Count > 0) Console.WriteLine();

            // Abschluss-Zeile
            if (errors > 0)
            {
                Console.WriteLine($"  {Red}✗{Reset}  {Bold}Build failed{Reset}  " +
                                  $"{Gray}· {errors} error(s){Reset}");
            }
            else if (warnings > 0)
            {
                Console.WriteLine($"  {Yellow}!{Reset}  {Bold}Build complete{Reset}  " +
                                  $"{Gray}· {total.TotalSeconds:F1}s · {warnings} warning(s){Reset}");
            }
            else
            {
                Console.WriteLine($"  {Green}✓{Reset}  {Bold}Build complete{Reset}  " +
                                  $"{Gray}· {total.TotalSeconds:F1}s{Reset}");
            }

            Console.WriteLine();
        }
        catch { }

        RestoreTerminal();
    }

    // ── Interne Render-Logik ──────────────────────────────────────────────

    private void OnTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (Volatile.Read(ref _disposed) == 1 || _completed) return;
        Render();
    }

    private void Render()
    {
        if (Volatile.Read(ref _disposed) == 1) return;

        lock (_lock)
        {
            try
            {
                var row = _originRow + 2;
                foreach (var stage in _stages)
                {
                    Console.SetCursorPosition(0, row++);
                    RenderStage(stage);
                }
            }
            catch { /* Nicht-TTY */ }
        }
    }

    private void PrintHeader()
    {
        int w;
        try { w = Math.Min(Console.WindowWidth, 72); }
        catch { w = 72; }
        Console.WriteLine($"{Bold}{Gray}  cs2sx{Reset}  {Dim}{Repeat("─", w - 10)}{Reset}");
        Console.WriteLine();
    }

    private static void RenderStage(BuildStage s)
    {
        var (icon, col) = s.Status switch
        {
            StageStatus.Done    => ("✓", Green),
            StageStatus.Running => (Spinner(), Cyan),
            StageStatus.Failed  => ("✗", Red),
            StageStatus.Warning => ("!", Yellow),
            _                   => ("○", Gray),
        };

        var nameCol = s.Status == StageStatus.Waiting ? Gray : Reset;
        var elapsed = s.Elapsed.Length > 0 ? $"  {Gray}{s.Elapsed}{Reset}" : string.Empty;
        var detail  = s.Detail.Length  > 0 ? $"  {Gray}{Truncate(s.Detail, 40)}{Reset}" : string.Empty;

        Console.Write($"  {col}{icon}{Reset}  {nameCol}{s.Name,-12}{Reset}{elapsed}{detail}{ClearEol}");
    }

    private static void WriteLogLine(DateTime ts, string level, string msg)
    {
        var (sym, col) = level switch
        {
            "ok"    => ("✓", Green),
            "warn"  => ("!", Yellow),
            "error" => ("✗", Red),
            "debug" => ("~", "\x1b[35m"),
            _       => ("i", Cyan),
        };
        Console.WriteLine($"  {Gray}{ts:HH:mm:ss}{Reset}  {col}{sym}{Reset}  {Dim}{msg}{Reset}");
    }

    // ── Hilfsmethoden ─────────────────────────────────────────────────────

    private static int _spinFrame;
    private static readonly char[] SpinFrames = ['⠋','⠙','⠹','⠸','⠼','⠴','⠦','⠧','⠇','⠏'];
    private static string Spinner() => SpinFrames[_spinFrame++ % SpinFrames.Length].ToString();

    private static string Repeat(string s, int n) => string.Concat(Enumerable.Repeat(s, n));
    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..(max - 1)] + "…";

    private static void RestoreTerminal()
    {
        try { Console.CursorVisible = true; }
        catch { }
    }

    // ── IDisposable ───────────────────────────────────────────────────────

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
        _ticker.Elapsed -= OnTick;
        _ticker.Stop();
        _ticker.Dispose();
        RestoreTerminal();
    }
}
