using System.Collections.Generic;

public interface IUpdatable
{
    void Update(float dt);
    bool IsAlive();
}

public class Bullet : IUpdatable
{
    public float Y;
    public void Update(float dt) { Y -= 100 * dt; }
    public bool IsAlive() => Y > 0;
}

public class Timer : IUpdatable
{
    public float Left = 2;
    public void Update(float dt) { Left -= dt; }
    public bool IsAlive() => Left > 0;
}

public class InterfacesApp : SwitchApp
{
    private readonly List<IUpdatable> _things = new List<IUpdatable>();

    public override void OnInit()
    {
        var b = new Bullet();
        b.Y = 50;
        _things.Add(b);
        _things.Add(new Timer());
    }

    public override void OnFrame()
    {
        int alive = 0;
        foreach (var t in _things)
        {
            t.Update(1f / 60f);
            if (t.IsAlive()) alive++;
        }
        Console.WriteLine("alive: " + alive);
    }
}
