using System;
using System.Collections.Generic;

// Lambdas share captured variables with the method, as in C#.
// Expected output in comments.
public class ClosuresApp : SwitchApp
{
    private Action? _later;

    public override void OnInit()
    {
        int z = 0;
        Action inc = () => z++;
        inc();
        inc();
        Console.WriteLine("count " + z);                                   // count 2

        int seen = 1;
        Func<int> read = () => seen;
        seen = 5;
        Console.WriteLine("read " + read());                               // read 5

        int total = 0;
        var list = new List<int> { 1, 2, 3 };
        list.ForEach(v => total += v);
        Console.WriteLine("sum " + total);                                 // sum 6

        var counters = new List<Func<int>>();
        for (int i = 0; i < 3; i++)
        {
            int k = i;
            counters.Add(() => k * 10);
        }
        Console.WriteLine("loop " + counters[0]() + " " + counters[1]() + " " + counters[2]()); // loop 0 10 20

        Make(7);
        _later!();
        _later();
    }

    private void Make(int start)
    {
        _later = () =>
        {
            start++;
            Console.WriteLine("param " + start);                           // param 8, param 9
        };
    }
}
