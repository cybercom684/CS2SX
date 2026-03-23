using System.Text.Json;
using System.Text.Json.Serialization;

namespace CS2SX.Build;

// ============================================================================
// ProjectConfig
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

    private static readonly JsonSerializerOptions s_opts = new JsonSerializerOptions
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
            Console.Error.WriteLine("[CS2SX] Warnung: cs2sx.json konnte nicht gelesen werden: " + ex.Message);
            return new ProjectConfig();
        }
    }
}

