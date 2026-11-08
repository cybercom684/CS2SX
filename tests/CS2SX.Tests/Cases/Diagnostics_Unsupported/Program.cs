using System;
using System.Collections.Generic;
using System.Linq;

// Every construct here has no C translation. CS2SX must reject each one with a
// clear error before the C compiler runs (expect-errors.txt).
public record Punkt(int X, int Y);

public delegate int Op(int a);

public struct Box<T> { public T V; }

public class Diagnostics_UnsupportedApp : SwitchApp
{
    public virtual T Pick<T>(T a, T b) => a;

    private static IEnumerable<int> Gen() { yield return 1; }

    public override void OnInit()
    {
        int s = 0;
        foreach (var i in 0..4) s += i;

        string text = "abcdef";
        Console.WriteLine(text[1..3]);

        var a = new Punkt(1, 2);
        var b = a with { X = 5 };

        int Sq(int x) => x * x;

        var list = new List<int> { 1, 2, 3 };
        var q = from x in list where x > 1 select x;

        List<int> lit = [1, 2, 3];

        object gate = new object();

        dynamic d = 5;

        Console.WriteLine(a.GetType().Name);

        int n = 0;
    again:
        n++;
        if (n < 3) goto again;
    }
}
