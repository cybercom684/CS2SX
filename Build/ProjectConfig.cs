using System.Text.Json;
using System.Text.Json.Serialization;

namespace CS2SX.Build;

// ============================================================================
// ProjectConfig
//
// FIX (Freetype / komplexe Libs): ExternLibConfig unterstützt jetzt:
//   - extraIncludeDirs: zusätzliche -I Pfade über includeDir hinaus
//   - defines:         -D Präprozessor-Definitionen (z.B. FT2_BUILD_LIBRARY)
//
// Damit können Libraries die mehrere interne Include-Verzeichnisse und
// Build-Defines brauchen (Freetype, libpng, etc.) korrekt eingebunden werden.
// ============================================================================

public sealed class ProjectConfig
{
    [JsonPropertyName("mainClass")]
    public string MainClass { get; set; } = "MyApp";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "MyApp";

    [JsonPropertyName("author")]
    public string Author { get; set; } = "Unknown";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";

    [JsonPropertyName("icon")]
    public string? Icon
    {
        get; set;
    }

    [JsonPropertyName("externLibs")]
    public List<ExternLibConfig> ExternLibs { get; set; } = new();

    public sealed class ExternLibConfig
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("includeDir")]
        public string? IncludeDir
        {
            get; set;
        }

        /// <summary>
        /// Zusätzliche Include-Verzeichnisse über includeDir hinaus.
        /// Nötig für Libraries mit internen Sub-Includes (Freetype, libpng, ...).
        /// Jeder Eintrag wird als eigener -I Flag an GCC übergeben.
        /// </summary>
        [JsonPropertyName("extraIncludeDirs")]
        public List<string> ExtraIncludeDirs { get; set; } = new();

        /// <summary>
        /// Präprozessor-Definitionen (-D flags).
        /// Beispiel: ["FT2_BUILD_LIBRARY", "FT_CONFIG_OPTION_SYSTEM_ZLIB"]
        /// </summary>
        [JsonPropertyName("defines")]
        public List<string> Defines { get; set; } = new();

        [JsonPropertyName("sources")]
        public List<string> Sources { get; set; } = new();
    }

    private static readonly JsonSerializerOptions s_opts = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    public static ProjectConfig Load(string projectDir)
    {
        var configPath = Path.Combine(projectDir, "cs2sx.json");
        if (!File.Exists(configPath)) return new ProjectConfig();

        try
        {
            var json = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<ProjectConfig>(json, s_opts)
                ?? new ProjectConfig();
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine("[CS2SX] Warnung: cs2sx.json konnte nicht gelesen werden: "
                + ex.Message);
            return new ProjectConfig();
        }
    }
}