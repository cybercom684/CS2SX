namespace CS2SX.Logging;

public sealed class BuildRenderer : IDisposable
{
    private readonly List<BuildStage> _stages = [];
    private readonly List<(DateTime ts, string level, string msg)> _lines = [];
    private readonly object _lock = new();
    private readonly int _originRow;
    private readonly System.Timers.Timer _ticker;
    private bool _disposed;

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
        _ticker.Elapsed += (_, _) => Render();
        _ticker.Start();
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
        lock (_lock)
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

    private static void ClearToEnd() =>
        Console.Write("\x1b[K");

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..(max - 1)] + "…";

    public void Complete(TimeSpan total, int warnings, int errors)
    {
        _ticker.Stop();
        Render();

        var summaryRow = _originRow + 2 + _stages.Count * 3 + 6 + 4;
        Console.SetCursorPosition(0, summaryRow);
        Console.WriteLine();
        if (errors > 0)
            Console.WriteLine($"  {Red}✗{Reset}  Build failed  {Gray}· {errors} error(s){Reset}");
        else
            Console.WriteLine($"  {Green}✓{Reset}  {Bold}Build complete{Reset}  {Gray}· {total.TotalSeconds:F1}s · {warnings} warning(s){Reset}");
        Console.WriteLine();

        // FIX: CursorVisible hier schon wiederherstellen (nicht erst in Dispose)
        // damit Terminal auch bei throw nach Complete() sauber ist
        RestoreTerminal();
    }

    private static void RestoreTerminal()
    {
        try { Console.CursorVisible = true; }
        catch { }
    }

    private static string Repeat(string s, int n) => string.Concat(Enumerable.Repeat(s, n));

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _ticker.Dispose();
        // FIX: immer wiederherstellen, auch wenn Complete() nie aufgerufen wurde
        RestoreTerminal();
    }
}