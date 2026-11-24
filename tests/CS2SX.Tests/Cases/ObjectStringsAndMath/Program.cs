using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Objects in strings go through their (virtual) ToString(); API results keep their
// type in strings even with `using System.IO`; LINQ selectors keep their type and
// ToArray() is an array; Math.Round rounds halves to even; Convert.ToString(v, base);
// checked arithmetic throws OverflowException. Expected output in comments.
public class Plain { public int V = 1; }

public class Named
{
    public string N;
    public Named(string n) { N = n; }
    public override string ToString() => "Named(" + N + ")";
}

public class Animal
{
    public virtual string Sound() => "...";
    public override string ToString() => "Animal:" + Sound();
}

public class Dog : Animal
{
    public override string Sound() => "Wuff";
}

public class Cat : Animal
{
    public override string Sound() => "Miau";
    public override string ToString() => "Cat/" + base.ToString();
    public string Self() => "self=" + ToString();
}

public struct P2
{
    public int X, Y;
    public P2(int x, int y) { X = x; Y = y; }
    public override string ToString() => "P" + (X + Y);
}

public struct Q { public int A; }

public class ObjectStringsAndMathApp : SwitchApp
{
    public override void OnInit()
    {
        var p = new Plain();
        var n = new Named("x");
        Console.WriteLine("plain " + p + $" {p}");                      // plain Plain Plain
        Console.WriteLine("named " + n + $" {n} " + n.ToString());      // named Named(x) Named(x) Named(x)
        Console.WriteLine(n);                                            // Named(x)
        Animal a = new Dog();
        Animal c = new Cat();
        Console.WriteLine("virt " + a + " " + c.ToString());            // virt Animal:Wuff Cat/Animal:Miau
        Console.WriteLine(((Cat)c).Self());                              // self=Cat/Animal:Miau
        object o = n;
        Console.WriteLine("obj " + o);                                   // obj Named(x)
        var pt = new P2(1, 3);
        Console.WriteLine("struct " + pt + " " + pt.ToString() + $" {pt} " + new Q()); // struct P4 P4 P4 Q
        var list = new List<Named> { new Named("a"), new Named("b") };
        Console.WriteLine("join " + string.Join(",", list));            // join Named(a),Named(b)
        Named none = null;
        Console.WriteLine("null [" + none + "]");                       // null []
        foreach (var an in new List<Animal> { new Dog(), new Cat() })
            Console.Write(an + ";");                                     // Animal:Wuff;Cat/Animal:Miau;
        Console.WriteLine();

        Console.WriteLine("io " + File.Exists("sdmc:/cs2sx-nope.txt") + $" {Path.GetFileName("sdmc:/a/b.txt")} "
            + Path.GetExtension("x.png") + " " + Path.GetFileNameWithoutExtension("a/b.c"));   // io False b.txt .png b

        var v = new List<int> { 1, 2, 3, 4, 5 };
        Console.WriteLine($"linq {v.Count(x => x > 2)} {v.Count(x => x < 2)} {v.Max(x => x * 3)} {v.Min(x => x * 1.5)} {v.Sum(x => x * 2)}"); // linq 3 1 15 1.5 30
        int[] arr = v.ToArray();
        var odd = v.Where(x => x % 2 == 1).ToArray();
        Console.WriteLine("arr " + arr.Length + " " + arr[4] + " " + odd.Length + " " + odd[2]
            + " " + v.ToArray().Where(x => x > 1).Count());              // arr 5 5 3 5 4

        Console.WriteLine("round " + Math.Round(2.5) + " " + Math.Round(3.5) + " " + Math.Round(-2.5)
            + " " + Math.Round(2.5, MidpointRounding.AwayFromZero) + " " + Math.Round(1.2345, 2)); // round 2 4 -2 3 1.23
        Console.WriteLine("base " + Convert.ToString(255, 16) + " " + Convert.ToString(5, 2) + " "
            + Convert.ToString(-1, 16) + " " + Convert.ToString(64, 8));  // base ff 101 ffffffff 100
        Console.WriteLine("date " + (DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Length == 19)); // date True

        int big = int.MaxValue;
        try { int r = checked(big + 1); Console.WriteLine("no " + r); }
        catch (OverflowException) { Console.Write("ovf1 "); }
        try { checked { int t = big; t += 1; Console.WriteLine("no " + t); } }
        catch (OverflowException) { Console.Write("ovf2 "); }
        long l = 5000000000L;
        try { int x = checked((int)l); Console.WriteLine("no " + x); }
        catch (OverflowException) { Console.Write("ovf3 "); }
        try { checked { int m = big * 2; Console.WriteLine("no " + m); } }
        catch (OverflowException e) { Console.WriteLine("ovf4 " + e.Message); } // ovf1 ovf2 ovf3 ovf4 Arithmetic operation resulted in an overflow.
        Console.WriteLine("unchecked " + unchecked(big + 1) + " " + checked(big - 1)); // unchecked -2147483648 2147483646
        Console.WriteLine("done");
    }

    public override void OnFrame() { }
}
