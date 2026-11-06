using System;

public class LambdasApp : SwitchApp
{
    private Action _onTick;
    private int _ticks;

    public override void OnInit()
    {
        Func<int, int, int> add = (a, b) => a + b;
        Func<int, bool> isBig = x => x > 100;
        Console.WriteLine(add(2, 3) + " " + isBig(150));

        int multiplier = 3;
        Func<int, int> scale = x => x * multiplier;
        Console.WriteLine(scale(7));

        _onTick = () => _ticks++;
        Apply(5, v => Console.WriteLine("got " + v));
    }

    private static void Apply(int value, Action<int> callback)
    {
        callback(value);
    }

    public override void OnFrame()
    {
        _onTick();
    }
}
