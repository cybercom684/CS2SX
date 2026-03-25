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
        if (string.IsNullOrWhiteSpace(cli.BuildTarget)) return Usage();
        var input = Path.GetFullPath(cli.BuildTarget);
        string csprojPath;
        if (Directory.Exists(input))
        {
            var csproj = Directory.GetFiles(input, "*.csproj").FirstOrDefault();
            if (csproj == null) { Log.Error($"No .csproj found in {input}"); return 1; }
            csprojPath = csproj;
        }
        else if (File.Exists(input))
            csprojPath = input;
        else { Log.Error($"Not found: {input}"); return 1; }
        new BuildPipeline(csprojPath).Run();
        return 0;

    case "genstubs":
        new StubGenerator().Generate(cli.LibnxInclude, cli.StubOutput);
        return 0;

    case "":
        return Usage();

    default:
        Log.Error($"Unknown command: {cli.Command}");
        return 1;
}

static int Usage()
{
    Log.Info("Usage:");
    Log.Info("  cs2sx new <AppName>");
    Log.Info("  cs2sx build <path/to/App.csproj or folder>");
    Log.Info("  cs2sx genstubs <libnx-include> <output>");
    return 1;
}