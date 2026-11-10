using System;
using System.Collections.Generic;

// Objects that point at each other (parent ↔ children, a ring, an object that
// points at itself) are found and freed once nothing else refers to them; the
// same shapes stay alive while a field still does. Expected output in comments.
public class TreeNode
{
    public static int Alive;
    public string Name;
    public TreeNode? Parent;
    public List<TreeNode> Children = new List<TreeNode>();

    public TreeNode(string name) { Name = name; Alive++; }
    ~TreeNode() { Alive--; }

    public TreeNode Add(string name)
    {
        var c = new TreeNode(name);
        c.Parent = this;
        Children.Add(c);
        return c;
    }
}

public class Link
{
    public static int Alive;
    public Link? Next { get; set; }
    public Link() { Alive++; }
    ~Link() { Alive--; }
}

public class Boss : Link
{
    public Link? Minion;
}

public class CyclesApp : SwitchApp
{
    private TreeNode? _tree;
    private Link? _ring;
    private long _base;
    private int _frame;

    private static TreeNode BuildTree()
    {
        var root = new TreeNode("root");
        var a = root.Add("a");
        a.Add("a1");
        a.Add("a2");
        root.Add("b");
        return root;
    }

    private static Link BuildRing(int n)
    {
        var first = new Link();
        var cur = first;
        for (int i = 1; i < n; i++)
        {
            var next = new Link();
            cur.Next = next;
            cur = next;
        }
        cur.Next = first;
        return first;
    }

    public override void OnFrame()
    {
        _frame++;

        // Garbage cycles every frame: must not accumulate.
        if (_frame > 40)
        {
            BuildTree();
            BuildRing(3);
        }

        switch (_frame)
        {
            case 1:
                _tree = BuildTree();
                _ring = BuildRing(4);
                var loop = new Link();
                loop.Next = loop;                     // points at itself, kept nowhere
                var boss = new Boss();
                boss.Minion = new Link();
                boss.Minion.Next = boss;                // cycle through a derived class's field
                Console.WriteLine("built " + TreeNode.Alive + " " + Link.Alive);   // built 5 7
                break;
            case 20:
                // Kept by fields: all still there. The loose ones are gone.
                Console.WriteLine("kept " + TreeNode.Alive + " " + Link.Alive
                    + " " + _tree!.Children[0].Children[1].Parent!.Parent!.Name);   // kept 5 4 root
                _tree = null;
                var keep = _ring!.Next;
                _ring = keep;                            // still one way into the ring
                break;
            case 40:
                Console.WriteLine("dropped " + TreeNode.Alive + " " + Link.Alive);  // dropped 0 4
                _ring = null;
                GC.Collect();                            // releases are still pending: nothing yet
                Console.WriteLine("collect " + Link.Alive);                         // collect 4
                break;
            case 60:
                _base = GC.GetTotalMemory(false);
                break;
            case 300:
                long grown = GC.GetTotalMemory(false) - _base;
                Console.WriteLine("stable " + (grown < 4096) + " " + (TreeNode.Alive < 200) + " " + (Link.Alive < 200)); // stable True True True
                break;
            case 302:
                Console.WriteLine("done");               // done
                break;
        }
    }
}
