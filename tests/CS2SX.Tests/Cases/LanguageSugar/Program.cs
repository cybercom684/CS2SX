using System;
using System.Collections.Generic;

// C# conveniences lowered at the C# level (SyntaxLowering). Expected output in comments.
public record Punkt(int X, int Y);

public delegate int Op(int a);
public delegate void Notify(string text);

public class LanguageSugarApp : SwitchApp
{
    private int _calls;
    private readonly Punkt _origin = new Punkt(1, 2);

    private static int Apply(Op op, int v) => op(v);

    public override void OnInit()
    {
        // local functions (static, using fields, recursive, calling each other)
        int Sq(int x) => x * x;
        int Fact(int n) => n <= 1 ? 1 : n * Fact(n - 1);
        void Count() { _calls++; }
        int SqPlus(int x) => Sq(x) + 1;
        Count(); Count();
        Console.WriteLine("local " + Sq(4) + " " + Fact(5) + " " + _calls + " " + SqPlus(3));   // local 16 120 2 10

        // collection expressions
        List<int> list = [3, 1, 2];
        int[] arr = [7, 8, 9];
        HashSet<int> set = [4, 5, 4];
        List<int> empty = [];
        Console.WriteLine("collections " + list.Count + " " + arr[2] + " " + set.Count + " " + empty.Count);  // collections 3 9 2 0

        // with on records
        var p = _origin with { X = 10 };
        var q = p with { Y = p.Y + 5 };
        Console.WriteLine("with " + p.X + "," + p.Y + " " + q.X + "," + q.Y + " " + _origin.X);  // with 10,2 10,7 1

        // foreach over a range
        int sum = 0;
        foreach (var i in 2..6) sum += i;
        Console.WriteLine("range-loop " + sum);                                    // range-loop 14

        // ranges and ^ on strings, lists, arrays
        string s = "abcdef";
        var nums = new List<int> { 10, 20, 30, 40, 50 };
        Console.WriteLine("slices " + s[1..3] + " " + s[^2..] + " " + s[..2] + " " + s[^1]
            + " " + nums[1..3].Count + " " + nums[^1] + " " + arr[^1]);            // slices bc ef ab f 2 50 9

        // delegate types
        Op twice = v => v * 2;
        Notify say = t => Console.WriteLine("notify " + t);
        say("hi");                                                                 // notify hi
        Console.WriteLine("delegate " + Apply(twice, 21) + " " + Apply(Sq2, 5));  // delegate 42 25
    }

    private static int Sq2(int x) => x * x;
}
