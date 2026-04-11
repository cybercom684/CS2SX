namespace CS2SX.Cli;

public sealed class CliArgs
{
    public string Command { get; init; } = string.Empty;

    // --- new ---
    public string AppName { get; init; } = string.Empty;

    // --- build ---
    public string BuildTarget { get; init; } = string.Empty;

    // --- check ---
    public string CheckTarget { get; init; } = string.Empty;

    // --- watch ---
    public string WatchTarget { get; init; } = string.Empty;

    // --- genstubs ---
    public string LibnxInclude { get; init; } = string.Empty;
    public string StubOutput { get; init; } = string.Empty;

    // --- flags ---
    public bool Verbose
    {
        get; init;
    }
}