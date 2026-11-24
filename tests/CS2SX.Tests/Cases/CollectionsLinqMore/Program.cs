using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// List range/search methods, GroupBy, ThenBy on text keys, Stack/HashSet/Queue of
// strings, a dictionary of lists, jagged arrays, string.Equals with a comparison,
// StringBuilder chains, params without arguments, ??= on int?, named tuples,
// Predicate<T>, null in strings. Expected output in comments.
public class Item { public string Name = ""; public int Score; public Item(string n, int s) { Name = n; Score = s; } }

public class CollectionsLinqMoreApp : SwitchApp
{
    static int SumAll(params int[] xs) { int s = 0; foreach (var x in xs) s += x; return s; }

    public override void OnInit()
    {
        var a = new List<int> { 5, 3, 8, 1 };
        a.Sort((x, y) => y - x);
        Console.WriteLine("sort " + string.Join(",", a) + " " + a.FindIndex(x => x == 3) + " " + a.FindLastIndex(x => x > 4)); // sort 8,5,3,1 2 1
        Console.WriteLine("rm " + a.RemoveAll(x => x > 6));             // rm 1
        a.InsertRange(0, new List<int> { 9, 9 });
        a.AddRange(new[] { 7 });
        var s = a.ConvertAll(x => "n" + x);
        Console.WriteLine("lst " + string.Join(",", a) + " " + string.Join(",", s)); // lst 9,9,5,3,1,7 n9,n9,n5,n3,n1,n7
        var g = a.GroupBy(x => x % 3).Select(gr => gr.Key + ":" + gr.Count()).ToList();
        Console.WriteLine("grp " + string.Join(",", g));                // grp 0:3,2:1,1:2
        foreach (var grp in new List<string> { "ab", "c", "de" }.GroupBy(w => w.Length))
            Console.WriteLine("g" + grp.Key + " " + string.Join("+", grp) + " " + grp.Sum(w => w.Length)); // g2 ab+de 4 / g1 c 1
        var items = new List<Item> { new Item("b", 2), new Item("a", 2), new Item("c", 1) };
        Console.WriteLine("ord " + string.Join(",", items.OrderBy(i => i.Score).ThenBy(i => i.Name).Select(i => i.Name))
            + " " + string.Join(",", items.OrderByDescending(i => i.Name).Select(i => i.Name))); // ord c,a,b c,b,a
        var hs = new HashSet<string> { "a", "b" };
        hs.Add("a" + "");
        Console.WriteLine("hs " + hs.Count + " " + hs.Contains("b") + " " + hs.Contains("z")); // hs 2 True False
        var st = new Stack<string>(); st.Push("x"); st.Push("y");
        Console.Write("st " + st.Pop() + st.Count);
        Console.WriteLine(st.TryPop(out var top) + top + st.TryPop(out var none)); // st y1TruexFalse
        var q = new Queue<string>(); q.Enqueue("q1");
        if (q.TryDequeue(out var qv)) Console.WriteLine("q " + qv + " " + q.TryPeek(out _)); // q q1 False
        var d = new Dictionary<string, List<int>>();
        d["k"] = new List<int> { 1, 2 };
        d["k"].Add(3);
        var counts = new Dictionary<string, int> { ["y"] = 1 };
        Console.WriteLine("d " + d["k"].Count + " [" + d.GetValueOrDefault("z")?.Count + "] " + counts.GetValueOrDefault("x", 5) + counts.GetValueOrDefault("y")); // d 3 [] 51
        int[][] jag = new int[2][];
        jag[0] = new int[] { 1, 2 };
        jag[1] = new int[3];
        Console.WriteLine("jag " + jag[0][1] + " " + jag[1].Length + " " + jag.Length); // jag 2 3 2
        Console.WriteLine("eq " + string.Equals("A", "a", StringComparison.OrdinalIgnoreCase) + " " + "x".Equals("X") + " " + string.Equals(null, null)); // eq True False True
        var sb = new StringBuilder("hello");
        sb.Insert(0, ">").Replace("l", "LL").Append("!");
        Console.WriteLine("sb " + sb + " " + sb.Length);                 // sb >heLLLLo! 9
        Console.WriteLine("p " + SumAll() + " " + SumAll(1, 2));        // p 0 3
        int? n = null; n ??= 4; n ??= 9;
        int? m = null;
        Console.WriteLine("n " + n + " [" + m + "]");                    // n 4 []
        var t = (name: "x", v: 2);
        Console.WriteLine("t " + t.name + t.v);                          // t x2
        Predicate<int> isEven = x => x % 2 == 0;
        Console.WriteLine("pr " + isEven(4) + isEven(3));                 // pr TrueFalse
        Console.WriteLine("cat " + string.Concat(Enumerable.Repeat("ab", 2)) + " " + string.Concat(new[] { "x", "y" })); // cat abab xy
        var arr = new[] { 3, 1, 3, 2 };
        Console.WriteLine("lq " + string.Join(",", arr.Distinct()) + " " + string.Join(",", arr.Where(x => x > 1)) + " " + arr.Max()); // lq 3,1,2 3,3,2 3
        Console.WriteLine("done");
    }

    public override void OnFrame() { }
}
