using System.Collections.Generic;

// Reference counting of user classes: fields and lists own one reference each.
// A reference they give up (RemoveAt, Clear, overwriting a field) is dropped at
// the start of the next frame, so locals stay valid for the rest of the frame.
// Every frame prints the counter for the previous frame's step, then runs the next.
public class Counter
{
    public static int Disposed;
}

public class Node
{
    public int V;
    public Node(int v) { V = v; }
    ~Node() { Counter.Disposed++; }
}

public class Shape
{
    ~Shape() { Counter.Disposed += 10; }
}

public class Circle : Shape
{
    public Node Center = new Node(7);          // released by Circle's own cleanup
    ~Circle() { Counter.Disposed += 100; }
}

public class Holder
{
    public Node? Kept;
}

public record Pair(Node First, int Weight);

public class RefCountingApp : SwitchApp
{
    private readonly List<Node> _nodes = new List<Node>();
    private readonly List<Shape> _shapes = new List<Shape>();
    private Node? _kept;
    private Holder? _holder;
    private Pair? _pair;
    private int _frame;

    public override void OnInit()
    {
        // Step 0: a list's only reference; and a local that outlives the RemoveAt.
        _nodes.Add(new Node(1));
        _nodes.RemoveAt(0);
        _nodes.Add(new Node(2));
        var borrowed = _nodes[0];
        _nodes.RemoveAt(0);
        Console.WriteLine("0 same-frame " + Counter.Disposed + " " + borrowed.V);   // 0 2
    }

    public override void OnFrame()
    {
        _frame++;
        if (_frame > 7) return;
        Console.Write(_frame + " " + Counter.Disposed);

        switch (_frame)
        {
            case 1:   // after step 0: nodes 1 and 2 are gone
                Console.WriteLine(" (2)");
                _nodes.Add(new Node(3));
                _kept = _nodes[0];               // shared with a field
                _nodes.RemoveAt(0);
                break;
            case 2:   // node 3 still held by _kept
                Console.WriteLine(" (2) kept " + _kept!.V);
                _kept = new Node(4);             // overwrite: node 3 released
                break;
            case 3:
                Console.WriteLine(" (3)");
                _kept = _kept;                   // same object: no change
                break;
            case 4:
                Console.WriteLine(" (3) kept " + _kept!.V);
                _kept = null;                    // node 4 released
                break;
            case 5:
                Console.WriteLine(" (4)");
                _shapes.Add(new Circle());       // virtual destruction:
                _shapes.Clear();                 // Center +1, ~Circle +100, ~Shape +10
                break;
            case 6:
                Console.WriteLine(" (115)");
                _holder = new Holder { Kept = new Node(5) };
                _pair = new Pair(_holder.Kept!, 1);
                _holder = null;                  // node 5 still held by the record
                break;
            case 7:
                Console.WriteLine(" (115) pair " + _pair!.First.V);
                break;
        }
    }
}
