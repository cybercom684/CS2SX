using System;
using System.Collections.Generic;
using System.Linq;

// Every construct here has no C translation. CS2SX must reject each one with a
// clear error before the C compiler runs (expect-errors.txt).
public record Punkt(int X, int Y);

public delegate int Op(ref int a);

public class Holder<T> { public struct Box<U> { public U V; } public U Pick<U>(U a) => a; }

public class Diagnostics_UnsupportedApp : SwitchApp
{
    private int _count;

    private static IEnumerable<int> Gen() { yield return 1; }
    private static Punkt Make() => new Punkt(0, 0);

    public override void OnInit()
    {
        int s = 0;
        foreach (var i in 0..^1) s += i;

        int[] numbers = { 1, 2, 3, 4 };
        Console.WriteLine(numbers[1..3].Length);

        var a = new Punkt(1, 2);
        var b = Make() with { X = 5 };

        int Sq(int x) => x * s;

        var list = new List<int> { 1, 2, 3 };
        var q = from x in list where x > 1 select x;

        List<int> lit = [.. list];

        object gate = new object();

        dynamic d = 5;

        Console.WriteLine(a.GetType().Name);

        int n = 0;
    again:
        n++;
        if (n < 3) goto again;
    }
}
