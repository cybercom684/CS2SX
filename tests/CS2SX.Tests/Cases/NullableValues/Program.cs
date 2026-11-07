public enum Mode { Idle, Run }

public struct Size
{
    public int W;
    public int H;
}

public class Profile
{
    public int? BestScore;
    public Mode? LastMode;
    public string? Nickname;          // nullable REFERENCE annotation — stays a plain pointer

    public int? Find(int key) => key > 0 ? key * 10 : null;

    public int Add(int? bonus) => 1 + (bonus ?? 0);
}

public class NullableValuesApp : SwitchApp
{
    public override void OnInit()
    {
        var p = new Profile();
        p.BestScore = 120;
        p.LastMode = Mode.Run;
        p.Nickname = null;

        int? found = p.Find(3);
        int? missing = p.Find(-1);
        int? fallback = missing ?? found;          // nullable ?? nullable → nullable

        bool better = found > 20;                  // lifted comparison
        int? sum = found + p.BestScore;            // lifted arithmetic
        bool same = found == missing;

        Size? box = null;
        if (box == null) box = new Size { W = 4, H = 2 };

        Console.WriteLine(p.Add(5) + " " + p.Add(null));
        Console.WriteLine(fallback.GetValueOrDefault() + " " + missing.GetValueOrDefault(-7));
        Console.WriteLine(better + " " + same + " " + (sum.HasValue ? sum.Value : 0));
        Console.WriteLine((p.LastMode == Mode.Run) + " " + box.Value.W);
        Console.WriteLine(p.Nickname ?? "anon");
    }
}
