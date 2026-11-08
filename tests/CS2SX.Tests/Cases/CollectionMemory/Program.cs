using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Collections are reference-counted like objects: temporaries made every frame
// (LINQ results, builders, dictionaries) are freed, whatever keeps a collection
// (fields, properties, records, closures, arrays) holds a reference.
public record Inventory(List<string> Items, int Slots);

public class Bag
{
    public List<int> Values = new List<int>();
    public Dictionary<string, int> Counts { get; set; } = new Dictionary<string, int>();
    public StringBuilder Log = new StringBuilder();
}

public class CollectionMemoryApp : SwitchApp
{
    private readonly List<int> _scores = new List<int> { 5, 3, 9 };
    private Bag _bag = new Bag();
    private Inventory? _inv;
    private Func<int>? _counter;
    private readonly List<int>[] _grid = new List<int>[2];
    private long _base;
    private int _frame;

    public override void OnFrame()
    {
        _frame++;

        // Temporaries every frame: must not accumulate.
        var top = _scores.Where(s => s > 4).OrderBy(s => s).ToList();
        var sb = new StringBuilder();
        sb.Append("x");
        var tmp = new Dictionary<string, int>();
        tmp["a" + _frame] = top.Count;

        switch (_frame)
        {
            case 1:
                var names = new List<string> { "sword", "potion" };
                _inv = new Inventory(names, 4);          // the record keeps the list
                var local = new List<int> { 1, 2 };
                _counter = () => local.Count;             // the closure keeps the list
                _grid[0] = new List<int> { 7 };          // the array keeps the list
                _bag.Values.Add(1);
                _bag.Counts["k"] = 2;
                _bag.Log.Append("hi");
                break;
            case 3:
                Console.WriteLine("kept " + _inv!.Items.Count + " " + _inv.Items[1] + " " + _counter!()
                    + " " + _grid[0][0] + " " + _bag.Values.Count + " " + _bag.Counts["k"]
                    + " " + _bag.Log.ToString() + " " + top.Count);   // kept 2 potion 2 7 1 2 hi 2
                break;
            case 5:
                _base = GC.GetTotalMemory(false);
                break;
            case 300:
                long grown = GC.GetTotalMemory(false) - _base;
                Console.WriteLine("stable " + (grown < 4096));         // stable True
                _inv = null;                                           // record and its list released
                _bag = new Bag();                                      // old bag and its collections released
                break;
            case 302:
                Console.WriteLine("done");                             // done
                break;
        }
    }
}
