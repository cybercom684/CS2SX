using System;
using System.Collections.Generic;

// Delegates, interface values, structs and tuples are kept by what holds them
// (fields, events, lists, closures) and released when they are let go — also
// in cycles (an object whose event handler captures the object). Values made
// every frame must not pile up. Expected output in comments.
public class Item
{
    public static int Alive;
    public int V = 1;
    public Item() { Alive++; }
    ~Item() { Alive--; }
    public int Get() => V;
}

public interface IThing { int Value(); }

public class Thing : IThing
{
    public static int Alive;
    public Thing() { Alive++; }
    ~Thing() { Alive--; }
    public int Value() => 2;
}

public struct Slot
{
    public Item? Held;
    public string Label;
    public int N;
}

public class Holder
{
    public static int Alive;
    public Func<int>? Getter;
    public event Action? Changed;
    public IThing? Thing;
    public Slot Slot;
    public (Item, int) Pair;
    public List<Func<int>> Calls = new List<Func<int>>();
    public List<IThing> Things = new List<IThing>();
    public List<Slot> Slots = new List<Slot>();
    public int Count;

    public Holder() { Alive++; }
    ~Holder() { Alive--; }

    public void Wire() { Changed += () => Count++; Getter = () => Count; }   // the handlers capture this
    public void Fire() { Changed?.Invoke(); }
}

public class HeldValuesApp : SwitchApp
{
    private readonly Holder _h = new Holder();
    private long _base;
    private int _frame;
    private int _sink;

    private void Churn()
    {
        var it = new Item();
        _h.Getter = it.Get;                                   // a method of the item
        _h.Thing = new Thing();
        _h.Slot = new Slot { Held = new Item(), Label = "s" + _frame, N = _frame };
        _h.Pair = (new Item(), _frame);
        _h.Calls.Add(() => it.V);
        _h.Things.Add(new Thing());
        _h.Slots.Add(new Slot { Held = it, Label = "l" + _frame });
        if (_h.Calls.Count > 3) { _h.Calls.RemoveAt(0); _h.Things.RemoveAt(0); _h.Slots.RemoveAt(0); }
        _sink += _h.Getter() + _h.Thing.Value() + _h.Calls[0]() + _h.Things[0].Value() + _h.Slot.N - _frame + _h.Pair.Item2 - _frame;

        var loose = new Holder();                             // a cycle through its own handlers
        loose.Wire();
        loose.Fire();
        _sink += loose.Getter!();
    }

    public override void OnFrame()
    {
        _frame++;
        if (_frame > 2) Churn();
        if (_frame == 3)
            Console.WriteLine("first " + _sink + " " + _h.Slot.Label + " " + _h.Slots[0].Label);   // first 7 s3 l3
        if (_frame == 70) _base = GC.GetTotalMemory(false);
        if (_frame == 70 + 256)
        {
            long grown = GC.GetTotalMemory(false) - _base;
            Console.WriteLine("stable " + (grown < 8192) + " " + (Item.Alive < 40) + " " + (Thing.Alive < 10)
                + " " + (Holder.Alive < 40));                    // stable True True True True
            Console.WriteLine("done");                           // done
        }
    }
}
