using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// The runtime library throws where .NET throws. Expected output in the comments.
public class RuntimeExceptionsApp : SwitchApp
{
    private static string Try(Action action)
    {
        try { action(); return "none"; }
        catch (Exception e) { return e.GetType().Name; }
    }

    public override void OnInit()
    {
        // parse: C# number/bool syntax, whitespace allowed around the value
        Console.WriteLine("parse " + int.Parse(" 42 ") + " " + double.Parse("2.5") + " " + bool.Parse("True")); // parse 42 2.5 True
        Console.WriteLine(Try(() => int.Parse("abc")));             // FormatException
        Console.WriteLine(Try(() => int.Parse("12x")));             // FormatException
        Console.WriteLine(Try(() => int.Parse("99999999999")));     // OverflowException
        Console.WriteLine(Try(() => byte.Parse("300")));             // OverflowException
        Console.WriteLine(Try(() => int.Parse(null)));               // ArgumentNullException
        Console.WriteLine(Try(() => bool.Parse("1")));               // FormatException
        int v;
        bool ok = int.TryParse("12x", out v);
        Console.WriteLine("try " + ok + " " + v);                   // try False 0

        // collections
        var list = new List<int> { 1, 2, 3 };
        Console.WriteLine(Try(() => { var x = list[5]; }));          // ArgumentOutOfRangeException
        Console.WriteLine(Try(() => list.RemoveAt(-1)));             // ArgumentOutOfRangeException
        Console.WriteLine(Try(() => list.Insert(9, 0)));             // ArgumentOutOfRangeException
        var dict = new Dictionary<string, int>();
        dict["a"] = 1;
        dict["a"] += 2;
        Console.WriteLine("dict " + dict["a"]);                     // dict 3
        Console.WriteLine(Try(() => { var y = dict["zzz"]; }));      // KeyNotFoundException
        Console.WriteLine(Try(() => dict.Add("a", 5)));              // ArgumentException
        var stack = new Stack<int>();
        Console.WriteLine(Try(() => stack.Pop()));                   // InvalidOperationException
        var queue = new Queue<int>();
        Console.WriteLine(Try(() => queue.Dequeue()));               // InvalidOperationException

        // arrays
        int[] arr = new int[3];
        Console.WriteLine(Try(() => arr[3] = 1));                    // IndexOutOfRangeException
        int[] none = null;
        Console.WriteLine(Try(() => none[0] = 1));                   // NullReferenceException

        // LINQ on empty sequences
        var empty = new List<int>();
        Console.WriteLine(Try(() => { var f = empty.First(); }));    // InvalidOperationException
        Console.WriteLine(Try(() => { var m = empty.Max(); }));      // InvalidOperationException

        // arithmetic, strings, files
        int zero = 0;
        Console.WriteLine(Try(() => { var d = 10 / zero; }));        // DivideByZeroException
        Console.WriteLine(Try(() => "abc".Substring(5)));            // ArgumentOutOfRangeException
        Console.WriteLine(Try(() => File.ReadAllText("sdmc:/cs2sx/does-not-exist.txt"))); // FileNotFoundException
        // formatting like .NET: shortest round-trip numbers, True/False, placeholder order
        Console.WriteLine($"fmt {0.1} {1e21} {1.5f} {true} {1000000.0} {0.00001}"); // fmt 0.1 1E+21 1.5 True 1000000 1E-05
        Console.WriteLine(string.Format("{1}-{0}-{1}", "a", "b") + " " + 2.25);    // b-a-b 2.25
        Console.WriteLine("{0:F2} {1}", 3.14159, false);                              // 3.14 False
        Console.WriteLine("done");
    }
}
