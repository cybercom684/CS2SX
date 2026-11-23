using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// yield return: IEnumerable<T> iterators (collected when called, so foreach and
// LINQ work) and IEnumerator<T> / IEnumerator state machines that run one step
// per MoveNext() — coroutines. Expected output in comments.
public class Tree
{
    public int Value;
    public Tree? Left, Right;
    public Tree(int v, Tree? l = null, Tree? r = null) { Value = v; Left = l; Right = r; }

    // recursive iterator in a property
    public IEnumerable<int> InOrder
    {
        get
        {
            if (Left != null) foreach (var x in Left.InOrder) yield return x;
            yield return Value;
            if (Right != null) foreach (var x in Right.InOrder) yield return x;
        }
    }
}

public class Blinker
{
    public string Name = "blink";
    public int Steps;

    // instance coroutine: members, a loop, locals that live across yields
    public IEnumerator<int> Run(int times)
    {
        string label = Name + ":";
        for (int i = 0; i < times; i++)
        {
            Steps++;
            yield return i * 10;
            label += i;
        }
        Console.WriteLine(label);                                  // blink:012
        yield return -1;
    }
}

public class IteratorsApp : SwitchApp
{
    private static IEnumerable<int> Evens(int max)
    {
        for (int i = 0; i <= max; i += 2)
        {
            if (i == 6) continue;
            yield return i;
        }
        if (max > 100) yield break;
        yield return 1000;
    }

    private static IEnumerable<string> Words()
    {
        yield return "a";
        yield return "bb";
        yield return "ccc";
    }

    private static IEnumerator<string> Over(List<string> items, string sep)
    {
        foreach (var s in items)
        {
            yield return s + sep;
            if (s == "stop") yield break;
        }
        yield return "end";
    }

    private static IEnumerator<string> FromSequence()
    {
        foreach (var w in Words()) { var up = w.ToUpper(); yield return up; }
    }

    private static IEnumerator Wait(int frames)
    {
        while (frames > 0) { frames--; yield return null; }
    }

    private IEnumerator<int>? _co;
    private IEnumerator? _wait;
    private int _frame;

    public override void OnInit()
    {
        var evens = new List<int>();
        foreach (var e in Evens(10)) evens.Add(e);
        Console.WriteLine("evens " + string.Join(",", evens));      // evens 0,2,4,8,10,1000
        Console.WriteLine("big " + Evens(200).Count());             // big 100
        Console.WriteLine("words " + Words().Sum(w => w.Length));  // words 6

        var t = new Tree(4, new Tree(2, new Tree(1), new Tree(3)), new Tree(6, null, new Tree(7)));
        Console.WriteLine("tree " + string.Join(" ", t.InOrder));  // tree 1 2 3 4 6 7

        var b = new Blinker();
        var run = b.Run(3);
        Console.WriteLine("lazy " + b.Steps);                       // lazy 0
        var got = "";
        while (run.MoveNext()) got += run.Current + " ";
        Console.WriteLine("run " + got + b.Steps);                  // run 0 10 20 -1 3
        Console.WriteLine("after " + run.MoveNext());               // after False

        var o = Over(new List<string> { "x", "y", "stop", "z" }, "!");
        var parts = new List<string>();
        while (o.MoveNext()) parts.Add(o.Current);
        Console.WriteLine("over " + string.Join(" ", parts));       // over x! y! stop!
        var o2 = Over(new List<string> { "q" }, "?");
        var all = "";
        while (o2.MoveNext()) all += o2.Current;
        Console.WriteLine("over2 " + all);                           // over2 q?end

        var f = FromSequence();
        var caps = "";
        while (f.MoveNext()) caps += f.Current;
        Console.WriteLine("caps " + caps);                           // caps ABBCCC

        // two independent instances of one coroutine
        var c1 = b.Run(2); var c2 = b.Run(2);
        c1.MoveNext(); c1.MoveNext(); c2.MoveNext();
        Console.WriteLine("indep " + c1.Current + " " + c2.Current); // indep 10 0

        _co = new Blinker { Name = "frame" }.Run(2);
        _wait = Wait(3);
    }

    public override void OnFrame()
    {
        _frame++;
        if (_wait != null && !_wait.MoveNext())
        {
            Console.WriteLine("waited " + _frame);                   // waited 4
            _wait = null;
        }
        if (_co != null)
        {
            if (_co.MoveNext()) Console.WriteLine("co " + _frame + " " + _co.Current);
            else { Console.WriteLine("co done " + _frame); _co = null; }
        }
        if (_frame == 6) Console.WriteLine("done");
    }
}
