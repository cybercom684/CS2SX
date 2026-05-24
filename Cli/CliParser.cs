namespace CS2SX.Cli;

public static class CliParser
{
    private static readonly IReadOnlyDictionary<string, Func<string[], CliArgs>> _commands =
        new Dictionary<string, Func<string[], CliArgs>>(StringComparer.OrdinalIgnoreCase)
        {
            ["new"] = ParseNew,
            ["build"] = ParseBuild,
            ["genstubs"] = ParseGenstubs,
            ["addlib"] = ParseAddLib,
            ["check"] = ParseCheck,
            ["watch"] = ParseWatch,
            ["clean"] = ParseClean,   // FIX: war vergessen
            ["dump"] = ParseDump,
            ["update"] = ParseUpdate,
        };

    public static CliArgs Parse(string[] args)
    {
        if (args.Length < 1)
            return new CliArgs();

        var command = args[0];
        if (!_commands.TryGetValue(command, out var parser))
            return new CliArgs { Command = command };

        return parser(args[1..]);
    }

    private static CliArgs ParseNew(string[] args) => new()
    {
        Command = "new",
        AppName = args.ElementAtOrDefault(0) ?? string.Empty,
    };

    private static CliArgs ParseBuild(string[] args) => new()
    {
        Command = "build",
        BuildTarget = args.ElementAtOrDefault(0) ?? string.Empty,
        Verbose = args.Contains("--verbose") || args.Contains("-v"),
    };

    private static CliArgs ParseGenstubs(string[] args) => new()
    {
        Command = "genstubs",
        LibnxInclude = args.ElementAtOrDefault(0) ?? "C:/devkitPro/libnx/include",
        StubOutput = args.ElementAtOrDefault(1) ?? "./LibNXStubs",
    };

    private static CliArgs ParseAddLib(string[] args) => new()
    {
        Command = "addlib",
        AddLibName = args.ElementAtOrDefault(0) ?? string.Empty,
        BuildTarget = args.ElementAtOrDefault(1) ?? string.Empty,
    };

    private static CliArgs ParseCheck(string[] args) => new()
    {
        Command = "check",
        CheckTarget = args.ElementAtOrDefault(0) ?? string.Empty,
        Verbose = args.Contains("--verbose") || args.Contains("-v"),
    };

    private static CliArgs ParseWatch(string[] args) => new()
    {
        Command = "watch",
        WatchTarget = args.ElementAtOrDefault(0) ?? string.Empty,
    };

    // FIX: clean-Befehl — war komplett vergessen obwohl CleanCommand fertig war
    private static CliArgs ParseClean(string[] args) => new()
    {
        Command = "clean",
        BuildTarget = args.ElementAtOrDefault(0) ?? string.Empty,
    };

    private static CliArgs ParseDump(string[] args) => new()
    {
        Command = "dump",
        // First arg is the .csproj path; remaining args are optional filename filters.
        CheckTarget = args.ElementAtOrDefault(0) ?? string.Empty,
        DumpFilter = args.Length > 1 ? args[1..] : Array.Empty<string>(),
    };

    private static CliArgs ParseUpdate(string[] args) => new()
    {
        Command = "update",
        BuildTarget = args.ElementAtOrDefault(0) ?? string.Empty,
    };
}