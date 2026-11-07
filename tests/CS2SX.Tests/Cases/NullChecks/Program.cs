using System;
using System.Collections.Generic;

// Member access on null throws NullReferenceException like in .NET (instead of
// crashing the console). Expected output in the comments.
public class Node
{
    public int Value;
    public Node? Next;
    public string? Name;
    public int Get() => Value;
    public virtual string Kind() => "node";
}

public interface IShape { int Area(); }
public class Square : IShape { public int Area() => 4; }

public class NullChecksApp : SwitchApp
{
    private static string Try(Action action)
    {
        try { action(); return "none"; }
        catch (NullReferenceException) { return "NRE"; }
        catch (Exception e) { return e.GetType().Name; }
    }

    public override void OnInit()
    {
        var n = new Node { Value = 1 };
        Node? missing = null;
        Console.WriteLine(Try(() => { var v = missing!.Value; }));     // NRE  field read
        Console.WriteLine(Try(() => { missing!.Value = 5; }));         // NRE  field write
        Console.WriteLine(Try(() => missing!.Get()));                  // NRE  method
        Console.WriteLine(Try(() => missing!.Kind()));                 // NRE  virtual method
        Console.WriteLine(Try(() => { var v = n.Next!.Value; }));      // NRE  through a chain
        IShape? shape = null;
        Console.WriteLine(Try(() => shape!.Area()));                   // NRE  interface call
        string? s = null;
        Console.WriteLine(Try(() => { var l = s!.Length; }));          // NRE  string length
        Console.WriteLine(Try(() => s!.ToUpper()));                    // NRE  string method
        List<int>? list = null;
        Console.WriteLine(Try(() => list!.Add(1)));                    // NRE  collection
        Console.WriteLine(Try(() => { var c = list!.Count; }));        // NRE  collection count

        IShape real = new Square();
        Console.WriteLine("ok " + n.Get() + " " + n.Kind() + " " + real.Area() + " "
            + (missing?.Name ?? "none"));                              // ok 1 node 4 none
    }
}
