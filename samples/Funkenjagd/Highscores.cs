using System.Collections.Generic;
using System.Text.Json;

// The best five rounds, kept as JSON in the app's SaveData folder
// (sdmc:/switch/cs2sx_data/Funkenjagd/scores.txt).

public class Entry
{
    public string Name { get; set; } = "";
    public int Points { get; set; }
    public int Players { get; set; }
}

public class ScoreTable
{
    public int Version { get; set; } = 1;
    public List<Entry> Best { get; set; } = new List<Entry>();
}

public static class Highscores
{
    public const int Max = 5;
    public static ScoreTable Table = new ScoreTable();

    public static void Load()
    {
        SaveData.Mount();
        var text = SaveData.Read("scores");
        if (text.Length == 0) return;
        try
        {
            var loaded = JsonSerializer.Deserialize<ScoreTable>(text);
            if (loaded != null) Table = loaded;
        }
        catch (JsonException)
        {
            Table = new ScoreTable();   // damaged file: start fresh
        }
    }

    /// Would these points make it onto the list?
    public static bool Qualifies(int points)
    {
        if (points <= 0) return false;
        return Table.Best.Count < Max || points > Table.Best[Table.Best.Count - 1].Points;
    }

    /// Adds the round at its place and saves; returns the place (1 = best) or 0.
    public static int Add(string name, int points, int players)
    {
        if (!Qualifies(points)) return 0;
        int at = 0;
        while (at < Table.Best.Count && Table.Best[at].Points >= points) at++;
        Table.Best.Insert(at, new Entry { Name = name, Points = points, Players = players });
        while (Table.Best.Count > Max) Table.Best.RemoveAt(Table.Best.Count - 1);
        SaveData.Write("scores", JsonSerializer.Serialize(Table, new JsonSerializerOptions { WriteIndented = true }));
        SaveData.Commit();
        return at + 1;
    }
}
