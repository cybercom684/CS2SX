using System;
using System.Collections.Generic;

// Generic methods are specialised per type before transpiling
// (GenericMethodMonomorphizer). Expected output in comments.
public static class Util
{
    public static T Max<T>(T a, T b) where T : IComparable<T> => a.CompareTo(b) > 0 ? a : b;
    public static void Swap<T>(ref T a, ref T b) { T t = a; a = b; b = t; }
    public static T First<T>(List<T> items) => items[0];
    public static T Larger<T>(T a, T b) where T : IComparable<T> => Max(a, b);   // transitive
    public static T OrDefault<T>(List<T> items) => items.Count > 0 ? items[0] : default(T);
    public static int CountWhere<T>(this List<T> items, Func<T, bool> pred)
    {
        int n = 0;
        foreach (var x in items) if (pred(x)) n++;
        return n;
    }
}

public class Node { public int V; public Node(int v) { V = v; } }

public struct Pair
{
    public int A, B;
    public static T Pick<T>(T a, T b, bool first) => first ? a : b;
}

public class GenericMethodsApp : SwitchApp
{
    static List<T> Repeat<T>(T value, int n)
    {
        var l = new List<T>();
        for (int i = 0; i < n; i++) l.Add(value);
        return l;
    }

    public override void OnInit()
    {
        Console.WriteLine("max " + Util.Max(3, 7) + " " + Util.Max(2.5f, 1.5f) + " " + Util.Max("apple", "pear")); // max 7 2.5 pear
        int x = 1, y = 2;
        Util.Swap(ref x, ref y);
        Console.WriteLine("swap " + x + " " + y);                                    // swap 2 1
        Console.WriteLine("pick " + Pair.Pick("a", "b", false) + " " + Pair.Pick<int>(4, 5, true)); // pick b 4
        var nodes = Repeat(new Node(9), 3);
        Console.WriteLine("repeat " + nodes.Count + " " + Util.First(nodes).V);       // repeat 3 9
        Console.WriteLine("larger " + Util.Larger(10, 20));                          // larger 20
        Console.WriteLine("default " + Util.OrDefault(new List<int>()));              // default 0
        var nums = new List<int> { 1, 5, 8, 2 };
        Console.WriteLine("count " + nums.CountWhere(n => n > 3));                   // count 2
    }
}
