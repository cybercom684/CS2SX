using System;
using System.Collections.Generic;
using System.Linq;

// SortedDictionary, SortedSet, PriorityQueue and LinkedList. Expected output in comments.
public class Task1 : IComparable<Task1>
{
    public string Name;
    public int Weight;
    public Task1(string name, int weight) { Name = name; Weight = weight; }
    public int CompareTo(Task1 other) => Weight - other.Weight;
}

public class SortedCollectionsApp : SwitchApp
{
    private SortedDictionary<string, int> _scores = new SortedDictionary<string, int>();

    public override void OnInit()
    {
        // SortedDictionary: keys in order
        _scores["mia"] = 30; _scores["anna"] = 50; _scores["zoe"] = 10;
        _scores.Add("ben", 20);
        _scores["mia"] += 5;
        var keys = "";
        foreach (var kv in _scores) keys += kv.Key + "=" + kv.Value + " ";
        Console.WriteLine(keys);                                                // anna=50 ben=20 mia=35 zoe=10
        Console.WriteLine(_scores.ContainsKey("ben") + " " + _scores.Remove("ben") + " " + _scores.ContainsKey("ben") + " " + _scores.Count);   // True True False 3
        if (_scores.TryGetValue("zoe", out var z)) Console.WriteLine("zoe " + z);   // zoe 10
        Console.WriteLine("missing " + _scores.TryGetValue("x", out var nope) + " " + nope);   // missing False 0
        foreach (var (name, points) in _scores) if (points > 40) Console.WriteLine("top " + name);   // top anna
        Console.WriteLine("keys " + string.Join(",", _scores.Keys) + " best " + _scores.OrderByDescending(p => p.Value).First().Key);   // keys anna,mia,zoe best anna
        var lit = new SortedDictionary<int, string> { { 3, "c" }, { 1, "a" }, { 2, "b" } };
        Console.WriteLine("lit " + string.Join("", lit.Values));             // lit abc
        try { _scores.Add("mia", 1); } catch (ArgumentException) { Console.WriteLine("dup"); }   // dup
        try { Console.WriteLine(_scores["nobody"]); } catch (KeyNotFoundException) { Console.WriteLine("nokey"); }   // nokey

        // SortedSet
        var set = new SortedSet<int> { 5, 1, 9, 1, 3 };
        Console.WriteLine("set " + set.Count + " " + set.Min + " " + set.Max + " " + string.Join(",", set));   // set 4 1 9 1,3,5,9
        Console.WriteLine(set.Add(4) + " " + set.Add(4) + " " + set.Contains(9) + " " + set.Remove(9));   // True False True True
        Console.WriteLine("where " + string.Join(",", set.Where(x => x > 2)));   // where 3,4,5
        var words = new SortedSet<string>(new List<string> { "pear", "apple", "fig" });
        foreach (var w in words) Console.Write(w + " ");
        Console.WriteLine();                                                      // apple fig pear

        // PriorityQueue: smallest priority first
        var pq = new PriorityQueue<string, int>();
        pq.Enqueue("low", 5); pq.Enqueue("urgent", 1); pq.Enqueue("mid", 3); pq.Enqueue("later", 9);
        var order = "";
        while (pq.TryDequeue(out var item, out var prio)) order += item + prio + " ";
        Console.WriteLine(order);                                                 // urgent1 mid3 low5 later9
        try { pq.Dequeue(); } catch (InvalidOperationException) { Console.WriteLine("empty"); }   // empty
        var jobs = new PriorityQueue<string, Task1>();
        jobs.Enqueue("b", new Task1("b", 7)); jobs.Enqueue("a", new Task1("a", 2));
        Console.WriteLine("job " + jobs.Peek() + " " + jobs.Count);              // job a 2

        // LinkedList
        var list = new LinkedList<int>();
        list.AddLast(2); list.AddLast(3); list.AddFirst(1);
        var mid = list.Find(2)!;
        list.AddAfter(mid, 25);
        list.AddBefore(mid, 15);
        list.Remove(3);
        var walk = "";
        foreach (var x in list) walk += x + " ";
        Console.WriteLine(walk + list.Count + " " + list.First!.Value + " " + list.Last!.Value);   // 1 15 2 25 4 1 25
        var back = "";
        for (var n = list.Last; n != null; n = n.Previous) back += n.Value;
        Console.WriteLine("back " + back + " " + list.Contains(15) + " " + list.Sum());   // back 252151 True 43
        list.RemoveFirst(); list.RemoveLast();
        Console.WriteLine("left " + string.Join(",", list));                      // left 15,2
        Console.WriteLine("done");
    }

    public override void OnFrame() { }
}
