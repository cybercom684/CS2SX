using System;

public class Board
{
    public int[] Cells;

    public Board(int n) { Cells = new int[n]; }

    public static int Sum(int[] values)
    {
        int s = 0;
        for (int i = 0; i < values.Length; i++) s += values[i];
        return s;
    }

    public static int Count(params int[] values) => values.Length;

    public int[] Doubled()
    {
        var result = new int[Cells.Length];
        for (int i = 0; i < Cells.Length; i++) result[i] = Cells[i] * 2;
        return result;
    }
}

public class ArraysAdvancedApp : SwitchApp
{
    public override void OnInit()
    {
        var b = new Board(5);
        for (int i = 0; i < b.Cells.Length; i++) b.Cells[i] = i;

        int[] twice = b.Doubled();
        Console.WriteLine(Board.Sum(twice) + " " + twice.Length);

        var nums = new[] { 3, 1, 2 };
        Console.WriteLine(Board.Count(nums) + " " + Board.Count(7, 8));

        int[] grow = { 1, 2 };
        Array.Resize(ref grow, 4);
        grow[3] = 9;
        Console.WriteLine(grow.Length + " " + grow[3]);

        int[] scratch = new int[2];
        scratch = new int[6];                      // reassigned: length must follow
        Console.WriteLine(scratch.Length);

        foreach (var word in "red green blue".Split(' '))
            Console.WriteLine(word.ToUpper());

        string[] parts = "a;b".Split(';');
        Console.WriteLine(string.Join("+", parts) + " " + parts.Length);
    }
}
