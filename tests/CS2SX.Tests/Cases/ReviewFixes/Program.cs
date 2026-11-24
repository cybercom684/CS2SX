using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

// Delegate values in List methods, interface values in strings, default methods via
// a derived interface, interface setters, ToArray ownership, StringBuilder positions,
// checked ++, Convert overloads, DateTime precision and formats. Expected output in comments.
public interface IA { string Name { get; } string Hello() => "hi " + Name; }
public interface IB : IA { int N { get; } }
public class C : IB { public string Name => "c"; public int N => 1; public override string ToString() => "C!"; }
public class D : C { }
public class Item { public string Label; public Item(string l) { Label = l; } }
public interface IHolder { Item Thing { get; set; } string Tag { get; set; } }
public class Holder : IHolder { public Item Thing { get; set; } = new Item("none"); public string Tag { get; set; } = ""; }
public struct Vec { public int X; public int Twice => X * 2; }
public struct Pair<T> { public T A; public string Note() => @"line1
line2"; }

public class ReviewFixesApp : SwitchApp
{
    static int Dbl(ref Vec v) => v.Twice;
    static int Dbl2(in Vec v) => v.Twice;
    static bool IsBig(int x) => x > 2;

    public override void OnInit()
    {
        IA a = new D();
        Console.WriteLine("iface " + a.Hello() + " " + a + $" {a}");          // iface hi c C! C!
        IHolder h = new Holder();
        h.Thing = new Item("x" + 1);
        h.Tag = "t" + 2;
        Console.WriteLine("set " + h.Thing.Label + " " + h.Tag);              // set x1 t2

        var l = new List<int> { 1, 2, 3, 4 };
        Predicate<int> even = x => x % 2 == 0;
        Comparison<int> desc = (x, y) => y - x;
        l.Sort(desc);
        Console.WriteLine("dlg " + string.Join(",", l.FindAll(even)) + " " + string.Join(",", l) + " " + l.Exists(even)
            + " " + l.Find(IsBig) + " " + l.FindIndex(IsBig) + " " + l.TrueForAll(even)); // dlg 4,2 4,3,2,1 True 4 0 False
        var copy = new List<int>(l);
        Console.WriteLine("rm " + copy.RemoveAll(IsBig) + " " + string.Join(",", copy) + " " + string.Join(",", l.ConvertAll(x => "n" + x))); // rm 2 2,1 n4,n3,n2,n1

        var names = new List<string> { "a", "b" };
        var arr = names.ToArray();
        names.Clear();
        names.Add("z");
        Console.WriteLine("arr " + arr.Length + arr[0] + arr[1] + " " + names[0]); // arr 2ab z

        var v = new Vec { X = 4 };
        Console.WriteLine("vec " + Dbl(ref v) + " " + Dbl2(in v));              // vec 8 8
        int big = int.MaxValue;
        try { checked { big++; } Console.WriteLine("no"); } catch (OverflowException) { Console.WriteLine("ovf"); } // ovf
        int small = int.MinValue;
        try { checked { --small; } Console.WriteLine("no"); } catch (OverflowException) { Console.WriteLine("ovf2"); } // ovf2
        Console.WriteLine("conv " + Convert.ToString(5, CultureInfo.InvariantCulture) + " " + Convert.ToString((sbyte)-1, 16)); // conv 5 ffff
        Console.WriteLine("note " + new Pair<int> { A = 1 }.Note().Split('\n').Length); // note 2

        var sb = new StringBuilder("äbc");
        sb.Insert(1, 5).Replace('b', 'B');
        // (compared, not printed: the emulator's debug log does not carry UTF-8)
        Console.WriteLine("sb " + (sb.ToString() == "ä5Bc") + " " + sb.Length + " " + sb.ToString().IndexOf("c")); // sb True 4 3
        Console.WriteLine("dt " + new DateTime(2024, 1, 1, 10, 0, 30).ToString("ss.FFF") + " " + new DateTime(2024, 1, 1).AddDays(0.5).Hour
            + " " + new DateTime(2024, 1, 1).AddMilliseconds(1.5).ToString("fffffff")); // dt 30 12 0015000
        Console.WriteLine("done");
    }

    public override void OnFrame() { }
}
