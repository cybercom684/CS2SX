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
        }

    case "genstubs":
        new StubGenerator().Generate(cli.LibnxInclude, cli.StubOutput);
        return 0;

    case "check":
        {
            if (string.IsNullOrEmpty(cli.CheckTarget))
            {
                Console.Error.WriteLine("Usage: cs2sx check <project.csproj>");
                return 1;
            }
            return new CheckCommand(cli.CheckTarget).Run();
        }

    // PHASE 4: Watch-Modus
    case "watch":
        {
            if (string.IsNullOrWhiteSpace(cli.WatchTarget)) return Usage();

            var input = Path.GetFullPath(cli.WatchTarget);
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

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            await new WatchCommand(csprojPath).Run(cts.Token);
            return 0;
        }

    case "":
        return Usage();

    default:
        Log.Error($"Unknown command: {cli.Command}");
        return 1;
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
    Log.Info("  cs2sx genstubs <libnx-include> <output>   Generate LibNX stubs");
    return 1;
}