namespace CS2SX.Cli;

public static class CliParser
{
    private static readonly IReadOnlyDictionary<string, Func<string[], CliArgs>> _commands =
        new Dictionary<string, Func<string[], CliArgs>>(StringComparer.OrdinalIgnoreCase)
        {
            ["new"] = ParseNew,
            ["build"] = ParseBuild,
            ["genstubs"] = ParseGenstubs,
        };

    public static CliArgs Parse(string[] args)
    {
        if (args.Length < 1)
            return new CliArgs();

        var command = args[0];
        if (!_commands.TryGetValue(command, out var parser))
            return new CliArgs { Command = command }; // unknown, handled by caller

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
    };

    private static CliArgs ParseGenstubs(string[] args) => new()
    {
        Command = "genstubs",
        LibnxInclude = args.ElementAtOrDefault(0) ?? "C:/devkitPro/libnx/include",
        StubOutput = args.ElementAtOrDefault(1) ?? "./LibNXStubs",
    };
}