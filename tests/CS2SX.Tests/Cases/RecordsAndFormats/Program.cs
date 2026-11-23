using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

// Records compare by value and print their members; number.ToString(format);
// dictionary indexer initializers; StringBuilder with text and character length;
// foreach over an own IEnumerable<T>; ?. on method calls. Expected output in comments.
public enum Kind { Fire, Ice }
public record Pair(int X, int Y);
public record Hero(string Name, int Level, Kind Element, Pair? Pos)
{
    public bool Ready { get; init; } = true;
}

public class Deck : IEnumerable<int>
{
    private readonly List<int> _cards = new List<int> { 3, 1, 4 };
    public IEnumerator<int> GetEnumerator() { foreach (var c in _cards) yield return c; }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class Node
{
    public int V;
    public Node(int v) { V = v; }
    public int Twice() => V * 2;
    public string Name(string prefix) => prefix + V;
}

public class RecordsAndFormatsApp : SwitchApp
{
    public override void OnInit()
    {
        var a = new Pair(1, 2);
        var b = new Pair(1, 2);
        var c = new Pair(2, 1);
        Console.WriteLine("eq " + (a == b) + " " + (a != b) + " " + (a == c) + " " + a.Equals(b) + " " + ReferenceEquals(a, b));   // eq True False False True False
        Console.WriteLine("str " + a + " " + c.ToString());                  // str Pair { X = 1, Y = 2 } Pair { X = 2, Y = 1 }
        var h = new Hero("Mia", 3, Kind.Ice, a);
        Console.WriteLine($"{h}");                                           // Hero { Name = Mia, Level = 3, Element = Ice, Pos = Pair { X = 1, Y = 2 }, Ready = True }
        Console.WriteLine("hero " + (h == new Hero("Mia", 3, Kind.Ice, new Pair(1, 2))) + " " + (h == new Hero("Mia", 4, Kind.Ice, a)));   // hero True False

        Console.WriteLine("fmt " + 3.14159.ToString("F2") + " " + 7.ToString("D3") + " " + 1234567.ToString("N0") + " " + 0.5f.ToString("F1"));   // fmt 3.14 007 1,234,567 0.5

        var dict = new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 };
        var byKind = new Dictionary<Kind, string> { [Kind.Ice] = "kalt" };
        Console.WriteLine("dict " + dict.Count + " " + dict["b"] + " " + byKind[Kind.Ice]);   // dict 2 2 kalt

        var sb = new StringBuilder("Größe");
        sb.Append('!');
        Console.WriteLine("sb " + sb.Length + " " + (sb.ToString() == "Größe!"));   // sb 6 True

        var sum = 0;
        var order = "";
        foreach (var card in new Deck()) { sum += card; order += card; }
        Console.WriteLine("deck " + sum + " " + order);                       // deck 8 314

        Node? none = null;
        var some = new Node(5);
        Console.WriteLine("cond " + (none?.Twice() ?? -1) + " " + (some?.Twice() ?? -1) + " " + (some?.Name("n") ?? "-") + " " + (none?.Name("n") ?? "-"));   // cond -1 10 n5 -
        Console.WriteLine("done");
    }

    public override void OnFrame() { }
}
