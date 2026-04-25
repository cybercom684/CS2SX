using System.Xml.Linq;

namespace CS2SX.Build;

/// <summary>
/// Liest ein .csproj-Projekt und sammelt alle zu transpilierenden .cs-Quelldateien.
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
            // FIX: Reihenfolge aus der .csproj-Datei beibehalten (bereits deterministisch)
            SourceFiles = explicitFiles;
            return;
        }

        // FIX: OrderBy(f => f) war schon vorhanden aber nur auf string-Ebene.
        // Auf Windows und Linux unterscheidet sich die Groß-/Kleinschreibung —
        // OrdinalIgnoreCase für konsistente Reihenfolge auf beiden Plattformen.
        SourceFiles = Directory
            .EnumerateFiles(ProjectDirectory, "*.cs", SearchOption.AllDirectories)
            .Select(f => Path.GetFullPath(f))
            .Where(f => IsIncluded(f))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)  // FIX: plattformkonsistent
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
            if (s_excludedDirNames.Contains(segments[i]))
                return false;
        }
        return true;
    }
}