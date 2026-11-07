public class Settings
{
    public int Volume;
}

public class Player
{
    public string Name;
    public int Level;
    public Settings Settings;

    public Player(string name) { Name = name; }
    public Player(string name, int level) : this(name) { Level = level; }

    public void Free() { }
}

public class Diagnostics_FootgunsApp : SwitchApp
{
    public override void OnInit()
    {
        var p = new Player("x");
        Console.WriteLine(p.Name);
    }
}
