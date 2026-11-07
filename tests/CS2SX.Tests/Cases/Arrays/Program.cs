using System;

public class ArraysApp : SwitchApp
{
    private static int[] s_table = new int[8];
    private static readonly string[] s_names = { "one", "two", "three" };
    private float[] _weights = new float[] { 0.5f, 1.5f, 2.5f };

    public override void OnInit()
    {
        var grid = new int[4];
        for (int i = 0; i < grid.Length; i++) grid[i] = i * 10;

        s_table[2] = 7;
        Console.WriteLine(grid[3] + " " + s_table[2] + " " + s_names[1] + " " + s_names.Length);

        var data = new[] { 9, 4, 7, 1 };
        Array.Sort(data);
        Console.WriteLine(data[0] + " " + data[data.Length - 1]);

        float total = 0;
        foreach (var w in _weights) total += w;
        Console.WriteLine(total);
    }
}
