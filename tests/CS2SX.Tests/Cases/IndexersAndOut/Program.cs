using System;
using System.Collections.Generic;

// Indexers of own classes, out/ref arguments to own methods, class-typed out
// parameters, `var` in for headers. Expected output in comments.
public class Item { public string Name; public Item(string n) { Name = n; } }

public class Bag
{
    private List<int> _items = new List<int>();
    public int this[int i] { get { return _items[i]; } set { _items[i] = value; } }
    public void Add(int x) { _items.Add(x); }
    public int Count => _items.Count;
    public bool TryFirst(out int v) { if (_items.Count == 0) { v = 0; return false; } v = _items[0]; return true; }
    public void Swap(ref int a, ref int b) { int t = a; a = b; b = t; }
    public bool Find(string name, out Item? found) { found = name == "x" ? null : new Item(name); return found != null; }
}
public class Grid
{
    private Dictionary<string, string> _cells = new Dictionary<string, string>();
    public string this[string key] { get => _cells.ContainsKey(key) ? _cells[key] : "-"; set => _cells[key] = value; }
}
public class BigBag : Bag { }

public class IndexersAndOutApp : SwitchApp
{
    private readonly Bag _field = new Bag();
    private bool Mine(out string s) { s = "mine"; return true; }

    public override void OnInit()
    {
        var b = new Bag();
        b.Add(4); b.Add(5);
        b[1] = 9;
        b[0] += 1;
        _field.Add(3);
        Console.WriteLine("index " + b[0] + " " + b[1] + " " + b.Count + " " + _field[0]);   // index 5 9 2 3
        var g = new Grid { ["a"] = "x" };
        g["b"] = "y";
        Console.WriteLine("grid " + g["a"] + g["b"] + g["c"]);           // grid xy-
        var bb = new BigBag();
        bb.Add(7);
        Console.WriteLine("derived " + bb[0]);                           // derived 7

        int x;
        b.TryFirst(out x);
        var ok = b.TryFirst(out var first);
        if (b.TryFirst(out var f)) Console.WriteLine("out " + x + " " + first + " " + ok + " " + f);   // out 5 5 True 5
        int p = 1, q = 2;
        b.Swap(ref p, ref q);
        if (Mine(out var s)) Console.WriteLine("ref " + p + q + " " + s);   // ref 21 mine
        if (b.Find("ring", out var it) && it != null) Console.WriteLine("class out " + it.Name);   // class out ring
        Console.WriteLine("missing " + b.Find("x", out var none) + " " + (none == null));   // missing False True
        Console.WriteLine(string.CompareOrdinal("a", "b") < 0);             // True

        var sum = 0;
        for (var i = 0; i < b.Count; i++) sum += b[i];
        Console.WriteLine("for " + sum);                                    // for 14
        Console.WriteLine("done");
    }

    public override void OnFrame() { }
}
