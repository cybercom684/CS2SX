using System;
using System.Collections.Generic;

// Objects that end up nowhere are freed when the next frame starts; whatever keeps
// an object (fields, collections, arrays, generic classes, structs, interfaces,
// lambdas) holds a reference. A counting finalizer makes it visible.
public class Counter
{
    public static int Freed;
}

public class Item
{
    public int V;
    public Item(int v) { V = v; }
    ~Item() { Counter.Freed++; }
}

public interface IHas { int Get(); }

public class Wrapped : IHas
{
    public int V;
    public Wrapped(int v) { V = v; }
    public int Get() => V;
    ~Wrapped() { Counter.Freed += 100; }
}

public class Box<T>
{
    public T Value;
    public Box(T v) { Value = v; }
}

public struct Pair
{
    public Item A;
}

public class FrameMemoryApp : SwitchApp
{
    private readonly Dictionary<string, Item> _dict = new Dictionary<string, Item>();
    private readonly Stack<Item> _stack = new Stack<Item>();
    private readonly Queue<Item> _queue = new Queue<Item>();
    private readonly HashSet<Item> _set = new HashSet<Item>();
    private readonly Item[] _arr = new Item[1];
    private Box<Item> _box = new Box<Item>(new Item(15));
    private Pair _pair;
    private IHas? _iface;
    private Func<int>? _fn;
    private Item? _field;
    private int _frame;

    private static int Garbage()
    {
        int sum = 0;
        for (int i = 0; i < 5; i++)
        {
            var t = new Item(i);          // never stored anywhere
            sum += t.V;
        }
        return sum;
    }

    public override void OnFrame()
    {
        _frame++;
        switch (_frame)
        {
            case 1:
                Garbage();
                _dict["k" + _frame] = new Item(10);   // key built at runtime
                _stack.Push(new Item(11));
                _queue.Enqueue(new Item(12));
                _set.Add(new Item(13));
                _arr[0] = new Item(14);
                _pair.A = new Item(16);
                _iface = new Wrapped(17);
                var cap = new Item(18);
                _fn = () => cap.V;
                _field = new Item(19);
                break;
            case 2:
                Console.WriteLine("garbage " + Counter.Freed);                       // garbage 5
                break;
            case 3:
                Console.WriteLine("kept " + _dict["k1"].V + " " + _stack.Peek().V + " " + _queue.Peek().V
                    + " " + _set.Count + " " + _arr[0].V + " " + _box.Value.V + " " + _pair.A.V
                    + " " + _iface!.Get() + " " + _fn!() + " " + _field!.V + " " + Counter.Freed);
                                                                                     // kept 10 11 12 1 14 15 16 17 18 19 5
                _dict.Remove("k1");
                var popped = _stack.Pop();
                var dequeued = _queue.Dequeue();
                _set.Clear();
                _arr[0] = null!;
                _box.Value = null!;
                _field = null;
                Console.WriteLine("same frame " + popped.V + " " + dequeued.V + " " + Counter.Freed); // same frame 11 12 5
                break;
            case 4:
                Console.WriteLine("released " + Counter.Freed);                      // released 12
                break;
        }
    }
}
