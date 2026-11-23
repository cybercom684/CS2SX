using System;
using System.Collections.Generic;

// `object` holds objects of classes and boxed strings, numbers, bools, chars and
// enums; `is`, patterns, casts, `as` and ToString look at what it really holds.
// Expected output in comments.
public enum Rank { Bronze, Silver, Gold }
public interface IShape { int Size(); }
public class Foo { public int A = 1; }
public class Bar : Foo, IShape { public int Size() => 7; }
public class Other { }

public class ObjectValuesApp : SwitchApp
{
    private object? _held;
    private readonly List<object> _bag = new List<object>();
    private readonly Dictionary<string, object> _props = new Dictionary<string, object>();
    private int _frame;

    private static string Describe(object? o)
    {
        switch (o)
        {
            case null: return "null";
            case string s: return "string " + s;
            case int n: return "int " + (n + 1);
            case double d: return "double " + d;
            case Rank r: return "rank " + r;
            case Bar b: return "bar " + b.Size();
            case Foo f: return "foo " + f.A;
            default: return "other " + o;
        }
    }

    private static object Pick(int i) => i == 0 ? "zero" : i == 1 ? (object)1.5f : new Foo();

    public override void OnInit()
    {
        object a = new Bar();
        object b = "text";
        object c = 5;
        object d = Rank.Gold;
        IShape shape = new Bar();
        object e = shape;
        Console.WriteLine("is " + (a is Foo) + " " + (a is Bar) + " " + (b is Foo) + " " + (b is string)
            + " " + (c is int) + " " + (c is long) + " " + (d is Rank) + " " + (e is Bar));   // is True True False True True False True True

        _bag.Add(a); _bag.Add(b); _bag.Add(c); _bag.Add(d); _bag.Add(new Other()); _bag.Add(true); _bag.Add('x');
        foreach (var o in _bag) Console.WriteLine(Describe(o));
        // bar 7 / string text / int 6 / rank Gold / other Other / other True / other x
        Console.WriteLine(Describe(Pick(1)) + " " + Describe(Pick(2)) + " " + Describe(null));   // other 1.5 foo 1 null

        Console.WriteLine("cast " + ((int)c + 1) + " " + (string)b + " " + ((Foo)a).A + " " + (Rank)d);   // cast 6 text 1 Gold
        Console.WriteLine("as " + ((b as string) ?? "-") + " " + ((a as Foo) != null) + " " + ((b as Foo) == null));   // as text True True
        if (c is int n) Console.WriteLine("bound " + (n * 2));                // bound 10
        if (b is string s && s.Length > 2) Console.WriteLine("bound " + s);  // bound text
        Console.WriteLine(c is int m && m > 3 ? "big " + m : "small");       // big 5
        string t = c switch { string s2 => s2, int i => "Zahl " + i, Foo => "foo", _ => "?" };
        string t2 = a switch { string s3 => s3, int i2 => "Zahl " + i2, Foo f3 => "foo " + f3.A, _ => "?" };
        Console.WriteLine("switch " + t + " " + t2);                         // switch Zahl 5 foo 1
        Console.WriteLine("print " + a + " " + b + " " + c + " " + d + " " + _bag[5] + " " + _bag[6] + " " + c.ToString() + $" {d}");
        // print Bar text 5 Gold True x 5 Gold

        try { var bad = (string)c; Console.WriteLine(bad); }
        catch (InvalidCastException ex) { Console.WriteLine("caught " + ex.Message); }   // caught Unable to cast object of type 'System.Int32' to type 'System.String'.
        try { var bad2 = (long)c; Console.WriteLine(bad2); }
        catch (InvalidCastException) { Console.WriteLine("caught long"); }   // caught long
        Foo plain = new Foo();
        try { var bad3 = (Bar)plain; Console.WriteLine(bad3.Size()); }
        catch (InvalidCastException) { Console.WriteLine("caught bar"); }    // caught bar

        _props["name"] = "Mia";
        _props["level"] = 3;
        _props["owner"] = new Foo();
        int level = (int)_props["level"];
        Console.WriteLine("props " + _props["name"] + " " + (level + 1) + " " + (_props["owner"] is Foo));   // props Mia 4 True

        // kept across frames: the field, the list and the dictionary hold their values
        _held = "frame text " + 41;
    }

    public override void OnFrame()
    {
        _frame++;
        if (_frame == 3)
        {
            Console.WriteLine("held " + _held + " " + _bag.Count + " " + _bag[1] + " " + _props["name"]);   // held frame text 41 7 text Mia
            _held = 7;
        }
        if (_frame == 5)
        {
            Console.WriteLine("held " + _held + " " + (_held is int));   // held 7 True
            Console.WriteLine("done");
        }
    }
}
