using System.Collections.Generic;
using System.Linq;

public class Enemy
{
    public string Name;
    public int Hp;
    public Enemy(string name, int hp) { Name = name; Hp = hp; }
}

public class LinqApp : SwitchApp
{
    public override void OnInit()
    {
        var values = new List<int> { 5, 3, 8, 1, 9, 2 };

        var evens = values.Where(v => v % 2 == 0).ToList();
        var doubled = values.Select(v => v * 2).ToList();
        var sorted = values.OrderBy(v => v).ToList();
        Console.WriteLine(evens.Count + " " + doubled[0] + " " + sorted[0]);
        Console.WriteLine(values.Sum() + " " + values.Max() + " " + values.Min());
        Console.WriteLine(values.Any(v => v > 8) + " " + values.All(v => v > 0));
        Console.WriteLine(values.First(v => v > 4) + " " + values.Count(v => v < 5));

        var enemies = new List<Enemy>
        {
            new Enemy("orc", 30),
            new Enemy("bat", 5),
            new Enemy("troll", 80),
        };
        var strong = enemies.Where(e => e.Hp > 10).OrderByDescending(e => e.Hp).ToList();
        Console.WriteLine(strong[0].Name);
        var weakest = enemies.OrderBy(e => e.Hp).First();
        Console.WriteLine(weakest.Name);
    }
}
