using System;
using System.Collections.Generic;

// Phase-4 language features. Expected console output is in the comments.

public enum Mode { None, Fast, Slow }
public enum Level { None, Low, High }

public class Point
{
    public int X;
    public int Y;
    public string Tag;

    public Point() : this(0, 0) { }
    public Point(int v) : this(v, v) { }
    public Point(int x, int y) : this(x, y, "p") { }
    public Point(int x, int y, string tag) { X = x; Y = y; Tag = tag; }
}

public class Point3 : Point
{
    public int Z;
    public Point3(int x, int y, int z) : base(x, y) { Z = z; }
    public Point3(int v) : this(v, v, v) { }
}

public class ParseError : Exception
{
    public int Position;
    public ParseError(string message, int position) : base(message) { Position = position; }
}

public class Counter
{
    public event Action<int> Changed;
    private int _value;

    public void Add(int n)
    {
        _value += n;
        Changed?.Invoke(_value);
    }
}

public class LanguagePhase4App : SwitchApp
{
    private int _sum;

    private static int Parse(string s)
    {
        if (s.Length == 0) throw new ParseError("empty", 0);
        if (s == "x") throw new ArgumentException("bad arg");
        if (s == "!") throw new InvalidOperationException("bad state");
        return s.Length;
    }

    private static string Classify(string s)
    {
        try
        {
            return "ok " + Parse(s);
        }
        catch (ParseError pe)
        {
            return "parse@" + pe.Position + " " + pe.Message;
        }
        catch (ArgumentException ae)
        {
            return "arg " + ae.Message;
        }
        catch (Exception e)
        {
            return "other " + e.Message;
        }
    }

    private static string Rethrow()
    {
        try
        {
            try { Parse("x"); }
            catch (ArgumentException) { throw; }
        }
        catch (Exception e) { return "rethrown " + e.Message; }
        return "none";
    }

    private void OnChangedA(int v) { _sum += v; Console.WriteLine("A " + v); }
    private void OnChangedB(int v) { _sum += 100; Console.WriteLine("B " + v); }

    public override void OnInit()
    {
        // ctor chaining
        var p0 = new Point();          // 0 0 p
        var p1 = new Point(5);         // 5 5 p
        var p3 = new Point3(7);        // 7 7 7 p
        Console.WriteLine(p0.X + " " + p0.Y + " " + p0.Tag);
        Console.WriteLine(p1.X + " " + p1.Y + " " + p1.Tag);
        Console.WriteLine(p3.X + " " + p3.Y + " " + p3.Z + " " + p3.Tag);

        // typed catch
        Console.WriteLine(Classify("abc"));  // ok 3
        Console.WriteLine(Classify(""));     // parse@0 empty
        Console.WriteLine(Classify("x"));    // arg bad arg
        Console.WriteLine(Classify("!"));    // other bad state
        Console.WriteLine(Rethrow());        // rethrown bad arg

        // multicast events
        var c = new Counter();
        c.Changed += OnChangedA;
        c.Changed += OnChangedB;
        c.Add(1);                            // A 1, B 1
        c.Changed -= OnChangedA;
        c.Add(2);                            // B 3
        Console.WriteLine("sum " + _sum);    // sum 201

        Action<string> log = s => Console.WriteLine("1:" + s);
        log += s => Console.WriteLine("2:" + s);
        log("hi");                           // 1:hi, 2:hi

        // enum constants with equal names in two enums
        Mode m = Mode.None;
        Level l = Level.High;
        if (m == Mode.None && l != Level.None) Console.WriteLine("enums " + (int)Mode.Slow + (int)Level.High); // enums 22

        // closures created in a loop capture their own variable
        var fs = new List<Func<int>>();
        for (int i = 0; i < 3; i++)
        {
            int k = i * 10;
            fs.Add(() => k);
        }
        string r = "";
        foreach (var f in fs) r += f() + ",";
        Console.WriteLine("closures " + r);  // closures 0,10,20,
    }
}
