using System.Text;

public class StringsApp : SwitchApp
{
    public override void OnInit()
    {
        string name = "Switch";
        int score = 1234;
        float ratio = 0.75f;

        Console.WriteLine($"Hello {name}, score={score}, ratio={ratio}");
        Console.WriteLine(name.ToUpper() + " " + name.ToLower() + " " + name.Length);
        Console.WriteLine(name.Substring(1, 3));
        Console.WriteLine(name.Contains("wit") ? "contains" : "missing");
        Console.WriteLine(name.Replace("S", "Tw"));
        Console.WriteLine("  padded  ".Trim());
        Console.WriteLine(name.StartsWith("Sw") && name.EndsWith("ch"));
        Console.WriteLine(name.IndexOf('t'));

        var parts = "a,b,c".Split(',');
        Console.WriteLine(parts.Length + " parts, last=" + parts[2]);
        Console.WriteLine(string.Join("-", parts));
        Console.WriteLine(string.IsNullOrEmpty("") ? "empty" : "not empty");

        int parsed = int.Parse("77");
        Console.WriteLine(parsed.ToString() + " " + score.ToString());

        var sb = new StringBuilder();
        for (int i = 0; i < 3; i++) sb.Append(i).Append(';');
        Console.WriteLine(sb.ToString());
    }
}
