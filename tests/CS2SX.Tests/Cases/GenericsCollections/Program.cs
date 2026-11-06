using System.Collections.Generic;

public class Box<T>
{
    public T Value;
    public Box(T value) { Value = value; }
    public T Get() => Value;
}

public class Item
{
    public string Name;
    public int Count;
    public Item(string name, int count) { Name = name; Count = count; }
}

public class GenericsCollectionsApp : SwitchApp
{
    public override void OnInit()
    {
        var numbers = new List<int>();
        for (int i = 0; i < 10; i++) numbers.Add(i * i);
        numbers.Remove(4);
        Console.WriteLine("count: " + numbers.Count + " first: " + numbers[0]);

        var names = new List<string> { "b", "a", "c" };
        names.Sort();
        Console.WriteLine(string.Join(",", names));

        var stock = new Dictionary<string, int>();
        stock["apple"] = 3;
        stock["pear"] = 5;
        if (stock.ContainsKey("apple")) stock["apple"] += 1;
        foreach (var kv in stock)
            Console.WriteLine(kv.Key + "=" + kv.Value);

        var items = new List<Item>();
        items.Add(new Item("sword", 1));
        items.Add(new Item("potion", 4));
        Console.WriteLine(items[1].Name);

        var box = new Box<int>(42);
        Console.WriteLine("box: " + box.Get());
    }
}
