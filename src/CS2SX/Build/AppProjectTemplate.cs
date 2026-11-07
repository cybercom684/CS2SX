using System.Reflection;
using System.Xml.Linq;

namespace CS2SX.Build;

/// <summary>
/// The shape of a CS2SX app project — shared by <c>cs2sx new</c> and <c>cs2sx migrate</c>.
/// An app is a plain SDK-style project referencing the <c>CS2SX.Switch</c> package
/// (the API + MSBuild integration). No stub files are copied into the project.
/// </summary>
public static class AppProjectTemplate
{
    public const string ApiPackageId = "CS2SX.Switch";

    /// Version of this tool — the CS2SX.Switch package version that matches it.
    public static string ToolVersion
    {
        get
        {
            var info = typeof(AppProjectTemplate).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            if (!string.IsNullOrEmpty(info))
                return info.Split('+')[0];
            return typeof(AppProjectTemplate).Assembly.GetName().Version?.ToString(3) ?? "0.5.0";
        }
    }

    /// Folder holding the CS2SX.Switch package bundled with the tool (null if absent).
    public static string? BundledFeed
    {
        get
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "packages");
            return Directory.Exists(dir) && Directory.GetFiles(dir, ApiPackageId + ".*.nupkg").Length > 0
                ? dir : null;
        }
    }

    public static string Csproj() => $"""
        <Project Sdk="Microsoft.NET.Sdk">

          <PropertyGroup>
            <TargetFramework>net8.0</TargetFramework>
          </PropertyGroup>

          <ItemGroup>
            <!-- Switch API + `dotnet build` -> .nro (requires the cs2sx tool on PATH). -->
            <PackageReference Include="{ApiPackageId}" Version="{ToolVersion}" />
          </ItemGroup>

        </Project>
        """;

    /// nuget.config that adds the tool's bundled package folder as a source.
    public static string? NugetConfig()
    {
        var feed = BundledFeed;
        if (feed == null) return null;
        return $"""
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <!-- CS2SX.Switch shipped with the cs2sx tool (matching version). -->
                <add key="cs2sx" value="{feed}" />
                <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
              </packageSources>
              <!-- Explicit mapping, so it also works where package source mapping is enforced. -->
              <packageSourceMapping>
                <packageSource key="cs2sx">
                  <package pattern="{ApiPackageId}" />
                </packageSource>
                <packageSource key="nuget.org">
                  <package pattern="*" />
                </packageSource>
              </packageSourceMapping>
            </configuration>
            """;
    }

    public const string GitIgnore = """
        bin/
        obj/
        cs2sx_out/
        *.nro
        *.nacp
        *.elf
        """;

    /// <summary>
    /// Adds the CS2SX.Switch PackageReference to an existing project file (idempotent).
    /// Returns true when the file was changed.
    /// </summary>
    public static bool EnsurePackageReference(string csprojPath)
    {
        var doc = XDocument.Load(csprojPath, LoadOptions.PreserveWhitespace);
        var root = doc.Root;
        if (root == null) return false;

        var existing = root.Descendants("PackageReference")
            .FirstOrDefault(e => (string?)e.Attribute("Include") == ApiPackageId);
        if (existing != null)
        {
            if ((string?)existing.Attribute("Version") == ToolVersion) return false;
            existing.SetAttributeValue("Version", ToolVersion);
        }
        else
        {
            root.Add(new XText("\n  "),
                new XElement("ItemGroup",
                    new XText("\n    "),
                    new XElement("PackageReference",
                        new XAttribute("Include", ApiPackageId),
                        new XAttribute("Version", ToolVersion)),
                    new XText("\n  ")),
                new XText("\n"));
        }
        doc.Save(csprojPath);
        return true;
    }
}
