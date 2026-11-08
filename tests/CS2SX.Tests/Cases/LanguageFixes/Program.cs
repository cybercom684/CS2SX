using System;
using System.Collections.Generic;

// Ordinary C# that used to end in GCC errors (expected output in comments).
[Flags]
public enum Access { None = 0, Read = 1, Write = 2, Exec = 4 }

public struct Meters
{
    public int Value;
    public static implicit operator int(Meters m) => m.Value;
    public static explicit operator Meters(int v) => new Meters { Value = v };
}

public class Outer
{
    public class Inner
    {
        public int V = 4;
        public int Twice() => V * 2;
    }

    public Inner Make() => new Inner();
}

public class Registry
{
    public static List<int> Items = new List<int>();
    public static Dictionary<string, int> Names = new Dictionary<string, int>();
    public static int Count = 3;
}

public class Link
{
    public Link? Next;
    public int Value = 1;
}

public class LanguageFixesApp : SwitchApp
{
    private int _a, _b;

    static int AddThree(int v) => v + 3;

    static (int, string) Pair() => (7, "seven");

    static (int min, int max) Range(List<int> values)
    {
        int lo = values[0], hi = values[0];
        foreach (var v in values) { if (v < lo) lo = v; if (v > hi) hi = v; }
        return (lo, hi);
    }

    public override void OnInit()
    {
        // 1. static method called through the class name
        Console.WriteLine("static " + LanguageFixesApp.AddThree(1));               // static 4

        // 2. tuple deconstruction (new variables, discards, existing fields)
        var (n, word) = Pair();
        Console.WriteLine("tuple " + n + " " + word);                              // tuple 7 seven
        (int lo, _) = Range(new List<int> { 5, 2, 9 });
        (_a, _b) = Range(new List<int> { 4, 8, 6 });
        Console.WriteLine("range " + lo + " " + _a + " " + _b);                    // range 2 4 8

        // 3. static fields with runtime initializers
        Registry.Items.Add(10);
        Registry.Names["x"] = 1;
        Console.WriteLine("statics " + Registry.Items.Count + " " + Registry.Names.Count + " " + Registry.Count); // statics 1 1 3

        // 4. Enum.HasFlag
        var rights = Access.Read | Access.Exec;
        Console.WriteLine("flags " + rights.HasFlag(Access.Exec) + " " + rights.HasFlag(Access.Write)); // flags True False

        // 5. nested classes
        var inner = new Outer.Inner();
        Outer.Inner made = new Outer().Make();
        Console.WriteLine("nested " + inner.V + " " + made.Twice());               // nested 4 8

        // 6. user-defined conversions
        var m = new Meters { Value = 12 };
        int asInt = m;
        Meters back = (Meters)5;
        Console.WriteLine("convert " + asInt + " " + back.Value);                  // convert 12 5

        // 7. ?. on a value member, combined with ??
        var head = new Link();
        int some = head.Next?.Value ?? -1;
        head.Next = new Link { Value = 9 };
        int other = head.Next?.Value ?? -1;
        Console.WriteLine("nullcond " + some + " " + other);                       // nullcond -1 9
    }
}
