namespace CS2SX.Logging;

public static class Log
{
    private static BuildRenderer? _renderer;
    private static readonly object _lock = new();
    private static bool _debug = Environment.GetEnvironmentVariable("CS2SX_DEBUG") == "1";

    public static void AttachRenderer(BuildRenderer r)
    {
        lock (_lock) _renderer = r;
    }
    public static void DetachRenderer()
    {
        lock (_lock) _renderer = null;
    }
    public static void EnableDebug() => _debug = true;

    public static void Info(string msg) => Write("info", msg);
    public static void Ok(string msg) => Write("ok", msg);
    public static void Warning(string msg) => Write("warn", msg);
    public static void Error(string msg) => Write("error", msg);
    public static void Debug(string msg)
    {
        if (_debug) Write("debug", msg);
    }

    private static void Write(string level, string msg)
    {
        lock (_lock)
        {
            if (_renderer is not null)
                _renderer.Log(level, msg);
            else
                WriteDirect(level, msg);
        }
    }

    private static void WriteDirect(string level, string msg)
    {
        const string R = "\x1b[0m";
        var (sym, col) = level switch
        {
            "ok" => ("✓", "\x1b[32m"),
            "warn" => ("!", "\x1b[33m"),
            "error" => ("✗", "\x1b[31m"),
            "debug" => ("~", "\x1b[35m"),
            _ => ("i", "\x1b[36m"),
        };
        Console.WriteLine($"\x1b[90m{DateTime.Now:HH:mm:ss}{R}  {col}{sym}{R}  {msg}");
    }
}