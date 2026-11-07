using System.Text.RegularExpressions;

namespace CS2SX.Build;

/// <summary>
/// Runtime modules that depend on a large portlib (FreeType: ~1.6 MB of code) are
/// compiled and linked only into apps whose generated C calls them — every other
/// app neither builds them nor links the library.
/// </summary>
internal static class OptionalRuntimeModules
{
    private sealed record Module(string FileName, Regex Usage, string[] LinkLibs);

    private static readonly Module[] s_modules =
    {
        new("rt_font.c",
            new Regex(@"\bFont_(Load|LoadSystem|Draw|Measure|LineHeight|Dispose)\s*\(", RegexOptions.Compiled),
            new[] { "freetype", "harfbuzz", "png", "bz2", "z" }),
    };

    /// <summary>
    /// Drops the unused optional modules from <paramref name="cFiles"/> and returns the
    /// link libraries the used ones need (in link order).
    /// </summary>
    public static List<string> Select(List<string> cFiles)
    {
        var generated = cFiles
            .Where(f => !s_modules.Any(m => IsModule(f, m)))
            .Select(f => File.Exists(f) ? File.ReadAllText(f) : "")
            .ToList();

        var libs = new List<string>();
        foreach (var m in s_modules)
        {
            if (generated.Any(code => m.Usage.IsMatch(code)))
                libs.AddRange(m.LinkLibs.Where(l => !libs.Contains(l)));
            else
                cFiles.RemoveAll(f => IsModule(f, m));
        }
        return libs;
    }

    private static bool IsModule(string path, Module m) =>
        string.Equals(Path.GetFileName(path), m.FileName, StringComparison.OrdinalIgnoreCase)
        && string.Equals(Path.GetFileName(Path.GetDirectoryName(path)), RuntimeExporter.RuntimeSourceDir,
            StringComparison.OrdinalIgnoreCase);
}
