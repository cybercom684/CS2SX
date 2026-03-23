using System.Reflection;

namespace CS2SX.Build;

public static class RuntimeExporter
{
    // ── Public API ─────────────────────────────────────────────────────────────

    public static void Export(string outputDir)
    {
        Directory.CreateDirectory(outputDir);
        ExportResources(outputDir, filter: r => r.Contains(".Runtime."));
    }

    public static void ExportStubs(string outputDir)
    {
        var assembly = Assembly.GetExecutingAssembly();

        foreach (var resource in assembly.GetManifestResourceNames())
        {
            if (!resource.Contains(".Stubs.")) continue;

            var parts = resource.Split('.');
            var stubsIdx = Array.IndexOf(parts, "Stubs");
            if (stubsIdx < 0) continue;

            var relativeParts = parts.Skip(stubsIdx + 1).ToArray();
            if (relativeParts.Length < 2) continue;

            var fileName = relativeParts[^2] + "." + relativeParts[^1];
            var subDirs = relativeParts.Take(relativeParts.Length - 2).ToArray();

            var targetDir = Path.Combine(new[] { outputDir }.Concat(subDirs).ToArray());
            Directory.CreateDirectory(targetDir);

            WriteResource(assembly, resource, Path.Combine(targetDir, fileName));
        }
    }

    /// <summary>
    /// Exportiert SwitchForms C#-Quelldateien für Forward-Declaration-Analyse.
    /// SwitchApp.cs wird NICHT exportiert — sie ist abstrakte Basis und wird
    /// nicht transpiliert (Implementierung liegt in switchapp.h).
    /// </summary>
    public static void ExportSwitchForms(string outputDir)
    {
        var assembly = Assembly.GetExecutingAssembly();

        foreach (var resource in assembly.GetManifestResourceNames())
        {
            if (!resource.Contains(".SwitchFormsLib.")) continue;

            var parts = resource.Split('.');
            var idx = Array.IndexOf(parts, "SwitchFormsLib");
            if (idx < 0 || idx + 2 >= parts.Length) continue;

            var fileName = parts[idx + 1] + "." + parts[idx + 2];

            // SwitchApp.cs überspringen: ist abstrakte Basis, wird nicht transpiliert,
            // aber Forward-Declaration (typedef struct SwitchApp) wird in _forward.h
            // manuell gesetzt.
            if (string.Equals(fileName, "SwitchApp.cs", StringComparison.OrdinalIgnoreCase))
                continue;

            using var stream = assembly.GetManifestResourceStream(resource)!;
            using var reader = new StreamReader(stream);
            File.WriteAllText(Path.Combine(outputDir, fileName), reader.ReadToEnd());
        }
    }

    // ── Private Helpers ────────────────────────────────────────────────────────

    private static void ExportResources(string outputDir, Func<string, bool> filter)
    {
        var assembly = Assembly.GetExecutingAssembly();

        foreach (var resource in assembly.GetManifestResourceNames().Where(filter))
        {
            var fileName = resource.Split('.').TakeLast(2).Aggregate((a, b) => $"{a}.{b}");
            WriteResource(assembly, resource, Path.Combine(outputDir, fileName));
        }
    }

    private static void WriteResource(Assembly assembly, string resourceName, string targetPath)
    {
        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource nicht gefunden: {resourceName}");
        using var reader = new StreamReader(stream);
        File.WriteAllText(targetPath, reader.ReadToEnd());
    }
}