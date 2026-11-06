using System.Reflection;

namespace CS2SX.Build;

/// <summary>Writes the resources embedded in the cs2sx tool to disk.</summary>
public static class RuntimeExporter
{
    /// Resource-name prefix of the embedded Switch API sources (src/CS2SX.Switch).
    public const string ApiResourcePrefix = "CS2SX.Api.";

    /// Sub-directory of the build dir that receives the runtime modules (rt_*.c).
    public const string RuntimeSourceDir = "runtime";

    /// <summary>
    /// The C runtime into a build directory: headers (switchapp.h, switchforms.h, …)
    /// at the top level, module sources (rt_*.c) in <see cref="RuntimeSourceDir"/>/.
    /// Returns the module source paths, which must be compiled and linked.
    /// </summary>
    public static IReadOnlyList<string> Export(string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        var sources = new List<string>();
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var resource in assembly.GetManifestResourceNames().Where(r => r.Contains(".Runtime.")))
        {
            var fileName = resource.Split('.').TakeLast(2).Aggregate((a, b) => $"{a}.{b}");
            if (resource.Contains(".Runtime.src.", StringComparison.Ordinal))
            {
                var dir = Path.Combine(outputDir, RuntimeSourceDir);
                Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, fileName);
                WriteResource(assembly, resource, path);
                sources.Add(path);
            }
            else
                WriteResource(assembly, resource, Path.Combine(outputDir, fileName));
        }
        return sources;
    }

    /// <summary>
    /// The Switch API C# sources, for the passes that scan declarations syntactically
    /// (forward declarations, generic/extension-method collection).
    /// </summary>
    public static void ExportApiSources(string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var resource in assembly.GetManifestResourceNames()
                     .Where(r => r.StartsWith(ApiResourcePrefix, StringComparison.Ordinal)))
            WriteResource(assembly, resource, Path.Combine(outputDir, resource[ApiResourcePrefix.Length..]));
    }

    /// <summary>
    /// Exports the API sources into a fresh temp directory unique to this run and
    /// returns its path; the caller deletes it. Unique per run so concurrent builds —
    /// even of two projects whose folders share a name — never share or delete it.
    /// </summary>
    public static string ExportApiSourcesToTemp()
    {
        var dir = Path.Combine(Path.GetTempPath(), "cs2sx_api_" + Guid.NewGuid().ToString("N"));
        ExportApiSources(dir);
        return dir;
    }

    private static void WriteResource(Assembly assembly, string resourceName, string targetPath)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource nicht gefunden: {resourceName}");
        using var reader = new StreamReader(stream);
        // Unchanged runtime headers keep their timestamp → no needless C recompiles.
        FileUtil.WriteIfChanged(targetPath, reader.ReadToEnd());
    }
}
