using System.Collections.Generic;

// Reference counting of user classes (Phase 1): fields and lists own one reference
// each; removing an object from a list only destroys it when nothing else holds it,
// and destruction runs the dynamic type's cleanup.
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

public class RefCountingApp : SwitchApp
{
    private readonly List<Node> _nodes = new List<Node>();
    private readonly List<Shape> _shapes = new List<Shape>();
    private Node? _kept;

    public override void OnInit()
    {
        // 1. A list's only reference: RemoveAt destroys the object.
        _nodes.Add(new Node(1));
        _nodes.RemoveAt(0);
        Console.WriteLine("removed-only " + Counter.Disposed);            // 1

        // 2. Shared with a field: RemoveAt keeps it alive.
        _nodes.Add(new Node(2));
        _kept = _nodes[0];
        _nodes.RemoveAt(0);
        Console.WriteLine("shared " + Counter.Disposed + " " + _kept.V);   // 1 2

        // 3. Shared with another object's field, then the list is cleared.
        var holder = new Holder();
        _nodes.Add(new Node(3));
        holder.Kept = _nodes[0];
        _nodes.Clear();
        Console.WriteLine("cleared " + Counter.Disposed + " " + holder.Kept!.V);  // 1 3

        // 4. Two lists share an element: removing from one keeps it.
        var other = new List<Node>();
        _nodes.Add(new Node(4));
        other.Add(_nodes[0]);
        _nodes.RemoveAt(0);
        Console.WriteLine("two-lists " + Counter.Disposed + " " + other[0].V);   // 1 4
        other.RemoveAt(0);
        Console.WriteLine("last-ref " + Counter.Disposed);                 // 2

        // 5. Virtual destruction: a Circle removed from a List<Shape> runs Circle's
        //    cleanup (its Center node: +1, ~Circle: +100) and Shape's (+10).
        _shapes.Add(new Circle());
        _shapes.RemoveAt(0);
        Console.WriteLine("virtual " + Counter.Disposed);                  // 113
    }
}
