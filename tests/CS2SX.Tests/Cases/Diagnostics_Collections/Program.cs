using System;
using System.Collections.Generic;
using System.Linq;

// Uses of the sorted / linked collections that cannot be generated. CS2SX must
// reject each one with a clear error (expect-errors.txt).
public struct Cell { public int X; }

public class Diagnostics_CollectionsApp : SwitchApp
{
    private static void Show(IEnumerable<KeyValuePair<string, int>> items) { }

    public override void OnInit()
    {
        var byLength = new SortedSet<string>(Comparer<string>.Create((a, b) => a.Length - b.Length));
        var cells = new SortedSet<Cell>();
        var pq = new PriorityQueue<string, int>();
        Console.WriteLine(pq.UnorderedItems);
        var d = new SortedDictionary<string, int>();
        Console.WriteLine(d.Comparer);
        Show(d);
    }

    public override void OnFrame() { }
}
