using CS2SX.Build;

if (args.Length < 2)
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  cs2sx new <AppName>");
    Console.WriteLine("  cs2sx build <path/to/App.csproj or folder>");
    Console.WriteLine("  cs2sx genstubs <libnx-include> <output>");
    return 1;
}

if (args[0] == "new")
{
    new ProjectCreator().Create(args[1]);
    return 0;
}

if (args[0] == "genstubs")
{
    var libnxPath = args.Length > 1 ? args[1] : "C:/devkitPro/libnx/include";
    var outputPath = args.Length > 2 ? args[2] : "./LibNXStubs";
    new StubGenerator().Generate(libnxPath, outputPath);
    return 0;
}

if (args[0] == "build")
{
    var input = Path.GetFullPath(args[1]);
    string csprojPath;

    if (Directory.Exists(input))
    {
        var csproj = Directory.GetFiles(input, "*.csproj").FirstOrDefault();
        if (csproj == null)
        {
            Console.WriteLine($"[CS2SX] No .csproj found in {input}");
            return 1;
        }
        csprojPath = csproj;
    }
    else if (File.Exists(input))
        csprojPath = input;
    else
    {
        Console.WriteLine($"[CS2SX] Not found: {input}");
        return 1;
    }

    new BuildPipeline(csprojPath).Run();
    return 0;
}

Console.WriteLine($"Unknown command: {args[0]}");
return 1;