namespace CS2SX.Build;

public class ProjectCreator
{
    public void Create(string appName)
    {
        var projectDir = Path.Combine(Directory.GetCurrentDirectory(), appName);

        if (Directory.Exists(projectDir))
        {
            Console.WriteLine($"[CS2SX] Directory '{appName}' already exists.");
            return;
        }

        Directory.CreateDirectory(projectDir);
        Console.WriteLine($"[CS2SX] Creating project '{appName}'...");

        // .csproj
        File.WriteAllText(Path.Combine(projectDir, $"{appName}.csproj"), $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>net8.0</TargetFramework>
                <Nullable>enable</Nullable>
                <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
              </PropertyGroup>
            </Project>
            """);

        // cs2sx.json
        File.WriteAllText(Path.Combine(projectDir, "cs2sx.json"), $$"""
            {
                "name": "{{appName}}",
                "author": "Unknown",
                "version": "1.0.0",
                "mainClass": "{{appName}}App"
            }
            """);

        // Program.cs
        File.WriteAllText(Path.Combine(projectDir, "Program.cs"), $$"""
            public class {{appName}}App : SwitchApp
            {
                public override void OnInit()
                {
                    Console.WriteLine("{{appName}} started!");
                    Console.WriteLine("Press + to exit.");
                }

                public override void OnFrame()
                {
                }
            }
            """);

        // Stubs kopieren via RuntimeExporter
        RuntimeExporter.ExportStubs(projectDir);

        Console.WriteLine($"[CS2SX] Done! Project created at: {projectDir}");
        Console.WriteLine($"[CS2SX] Build with: cs2sx build {appName}/{appName}.csproj");
    }
}