namespace CS2SX.Cli;

public sealed class CliArgs
{
    // --- Subcommand ---
    public string Command { get; init; } = string.Empty;

    // --- new ---
    public string AppName { get; init; } = string.Empty;

    // --- build ---
    public string BuildTarget { get; init; } = string.Empty;

    // --- check ---
    public string CheckTarget { get; init; } = string.Empty;

    // --- genstubs ---
    public string LibnxInclude { get; init; } = string.Empty;
    public string StubOutput { get; init; } = string.Empty;

    // --- future flags ---
    // public bool Verbose { get; init; }
    // public string Config { get; init; } = string.Empty;
}