using System;
using System.Collections.Generic;

// Cycles that run through a Dictionary, Stack, Queue or HashSet are found and
// freed like those through fields and lists. Expected output in comments.
public class Hub
{
    public static int Alive;
    public Hub? Owner;
    public Dictionary<string, Hub> ByName = new Dictionary<string, Hub>();
    public Stack<Hub> History = new Stack<Hub>();
    public Queue<Hub> Pending = new Queue<Hub>();
    public HashSet<Hub> Seen = new HashSet<Hub>();

    public Hub() { Alive++; }
    ~Hub() { Alive--; }

    public Hub Child()
    {
        var c = new Hub();
        c.Owner = this;
        return c;
    }
}

public class CyclesCollectionsApp : SwitchApp
{
    private Hub? _kept;
    private long _base;
    private int _frame;

    private static Hub Build()
    {
        var hub = new Hub();
        hub.ByName["c"] = hub.Child();
        hub.History.Push(hub.Child());
        hub.Pending.Enqueue(hub.Child());
        hub.Pending.Enqueue(hub.Child());
        hub.Pending.Dequeue();                 // the queue no longer starts at slot 0
        hub.Seen.Add(hub.Child());
        return hub;
    }

    public override void OnFrame()
    {
        _frame++;
        if (_frame > 60) Build();              // garbage every frame: must not accumulate

        switch (_frame)
        {
            case 1:
                _kept = Build();
                Build();
                Console.WriteLine("built " + Hub.Alive);                       // built 12
                break;
            case 40:
                // The loose one is gone (and the child that left the queue of the kept one).
                Console.WriteLine("kept " + Hub.Alive + " " + (_kept!.ByName["c"].Owner == _kept)
                    + " " + _kept.History.Count + _kept.Pending.Count + _kept.Seen.Count);   // kept 5 True 111
                _kept = null;
                break;
            case 60:
                Console.WriteLine("dropped " + Hub.Alive);                     // dropped 0
                break;
            case 92:                                    // same point of the collector's 16-frame rhythm as 300
                _base = GC.GetTotalMemory(false);
                break;
            case 300:
                long grown = GC.GetTotalMemory(false) - _base;
                Console.WriteLine("stable " + (grown < 8192) + " " + (Hub.Alive < 300));   // stable True True
                Console.WriteLine("done");                                     // done
                break;
        }
    }
}
