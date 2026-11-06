using System.Xml.Linq;

namespace CS2SX.Build;

/// <summary>
/// Liest ein .csproj-Projekt und sammelt alle zu transpilierenden .cs-Quelldateien.
///
/// FIX (addLib): Alle Ordner die mit "Stubs" enden werden ausgeschlossen.
/// cs2sx addLib generiert IDE-Stubs in "<LibName>Stubs/" — diese sollen von
/// Roslyn/IDE gesehen werden, aber NICHT transpiliert werden, weil sie
/// "extern"-Deklarationen ohne Body enthalten.
/// Die eigentliche C-Library wird direkt aus externLibs/ mitcompiliert.
/// </summary>
public sealed class ProjectReader
{
    private static readonly HashSet<string> s_excludedDirNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "obj", "bin", "Stubs", "LibNX",
        };

    private static readonly HashSet<string> s_excludedFileNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "SwitchApp.cs",
            "Input.cs",
            "_GlobalTypes.cs",
        };

    public string ProjectDirectory { get; private set; } = string.Empty;
    public IReadOnlyList<string> SourceFiles { get; private set; } = Array.Empty<string>();

    public void Load(string csprojPath)
    {
        csprojPath = Path.GetFullPath(csprojPath);
        ProjectDirectory = Path.GetDirectoryName(csprojPath)
            ?? throw new ArgumentException("Ungültiger Pfad: " + csprojPath);

        var xml = XDocument.Load(csprojPath);

        // Explizit definierte <Compile Include="..."/> Einträge
        var explicitFiles = xml.Descendants("Compile")
            .Select(e => e.Attribute("Include")?.Value)
            .Where(v => v != null)
            .Select(v => Path.GetFullPath(Path.Combine(ProjectDirectory, v!)))
            .Where(f => IsIncluded(f))
            .ToList();

        if (explicitFiles.Count > 0)
        {
            SourceFiles = explicitFiles;
            return;
        }

        SourceFiles = Directory
            .EnumerateFiles(ProjectDirectory, "*.cs", SearchOption.AllDirectories)
            .Select(f => Path.GetFullPath(f))
            .Where(f => IsIncluded(f))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private bool IsIncluded(string fullPath)
    {
        var fileName = Path.GetFileName(fullPath);
        if (s_excludedFileNames.Contains(fileName)) return false;

        var relative = Path.GetRelativePath(ProjectDirectory, fullPath);
        var separators = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        var segments = relative.Split(separators, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < segments.Length - 1; i++)
        {
            var seg = segments[i];

            // Exakter Match gegen die Ausschluss-Liste (obj, bin, Stubs, LibNX)
            if (s_excludedDirNames.Contains(seg))
                return false;

            // FIX (addLib): Alle Ordner die mit "Stubs" enden ausschließen.
            // cs2sx addLib erstellt "<LibName>Stubs/" — z.B. "MylibStubs/", "ImGuiStubs/".
            // Diese Ordner enthalten nur IDE-Stubs (extern-Deklarationen ohne Body)
            // und dürfen NICHT transpiliert werden.
            if (seg.EndsWith("Stubs", StringComparison.OrdinalIgnoreCase))
                return false;
        }

        // Dateien die mit "// CS2SX Stub" beginnen sind reine IntelliSense-Stubs
        // und dürfen nicht transpiliert werden (keine echten Implementierungen).
        try
        {
            using var reader = System.IO.File.OpenText(fullPath);
            var firstLine = reader.ReadLine();
            if (firstLine != null && firstLine.StartsWith("// CS2SX Stub", StringComparison.Ordinal))
                return false;
        }
        catch { }

        return true;
    }
}