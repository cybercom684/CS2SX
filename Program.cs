using CS2SX.Build;
using CS2SX.Cli;
using CS2SX.Logging;

var cli = CliParser.Parse(args);

switch (cli.Command)
{
    case "new":
        if (string.IsNullOrWhiteSpace(cli.AppName)) return Usage();
        new ProjectCreator().Create(cli.AppName);
        return 0;

    case "build":
        {
            if (string.IsNullOrWhiteSpace(cli.BuildTarget)) return Usage();
            var csprojPath = ResolveCsproj(cli.BuildTarget);
            if (csprojPath == null) return 1;
            new BuildPipeline(csprojPath).Run();
            return 0;
        }

    case "check":
        {
            if (string.IsNullOrEmpty(cli.CheckTarget)) return Usage();
            return new CheckCommand(cli.CheckTarget).Run();
        }

    case "watch":
        {
            if (string.IsNullOrWhiteSpace(cli.WatchTarget)) return Usage();
            var csprojPath = ResolveCsproj(cli.WatchTarget);
            if (csprojPath == null) return 1;
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };
            await new WatchCommand(csprojPath).Run(cts.Token);
            return 0;
        }

    case "clean":
        {
            if (string.IsNullOrWhiteSpace(cli.BuildTarget)) return Usage();
            var csprojPath = ResolveCsproj(cli.BuildTarget);
            if (csprojPath == null) return 1;
            return new CleanCommand(csprojPath).Run();
        }

    case "genstubs":
        new StubGenerator().Generate(cli.LibnxInclude, cli.StubOutput);
        return 0;

    case "":
        return Usage();

    default:
        Log.Error($"Unknown command: {cli.Command}");
        return Usage();
}

// ── Hilfsmethoden ─────────────────────────────────────────────────────────────

static string? ResolveCsproj(string input)
{
    var full = Path.GetFullPath(input);

    if (Directory.Exists(full))
    {
        var csproj = Directory.GetFiles(full, "*.csproj").FirstOrDefault();
        if (csproj == null)
        {
            Log.Error($"No .csproj found in: {full}");
            return null;
        }
        return csproj;
    }

    if (File.Exists(full))
        return full;

    Log.Error($"Not found: {full}");
    return null;
}

static int Usage()
{
    Log.Info("CS2SX — C# to Nintendo Switch Transpiler");
    Log.Info("");
    Log.Info("Usage:");
    Log.Info("  cs2sx new    <AppName>                    Create a new project");
    Log.Info("  cs2sx build  <path/to/App.csproj|folder>  Build project to .nro");
    Log.Info("  cs2sx check  <path/to/App.csproj>         Transpile-only check");
    Log.Info("  cs2sx watch  <path/to/App.csproj|folder>  Watch & rebuild on change");
    Log.Info("  cs2sx clean  <path/to/App.csproj|folder>  Delete cs2sx_out/");
    Log.Info("  cs2sx genstubs <libnx-include> <output>   Generate LibNX stubs");
    return 1;
}