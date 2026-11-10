using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// async/await: methods become state machines that continue in later frames, on
// the main thread. Locals, parameters and strings survive the waits; exceptions
// end up in the task. Expected output in comments.
public class Loader
{
    private int _calls;
    public int Calls => _calls;

    public async Task<int> Fetch(int id)
    {
        _calls++;
        await Task.Delay(30);
        return id * 10;
    }

    public async Task<string> Name(int id)
    {
        int v = await Fetch(id);
        return "item" + v;
    }
}

public class AsyncAwaitApp : SwitchApp
{
    private readonly Loader _loader = new Loader();
    private Task? _seq;
    private Task<int>? _sum;
    private Task? _outer;
    private Task? _all;
    private bool _fired;
    private bool _reported;
    private int _pulses;
    private int _since;
    private long _base;

    private static async Task<string> Greet(string who)
    {
        await Task.Yield();
        return "hi " + who;
    }

    private async Task<int> Twice(int x) => await _loader.Fetch(x) * 2;

    private async Task<int> Sum(List<int> ids)
    {
        int total = 0;
        foreach (var id in ids)
        {
            total += await _loader.Fetch(id);
        }
        for (int i = 0; i < 2; i++)
        {
            await Task.Yield();
            total++;
        }
        return total;
    }

    private async Task Sequence(string who)
    {
        Console.WriteLine("seq start");
        string greeting = await Greet(who);
        string a = await _loader.Name(1);
        Console.WriteLine("seq " + greeting + " " + a);
        if (a.Length > 3)
        {
            await Task.Delay(20);
            Console.WriteLine("seq waited");
        }
        var ready = Task.FromResult(5);
        int five = await ready;                       // already finished: goes straight on
        Console.WriteLine("seq end " + five + " " + await Twice(2));
    }

    private async Task Fails()
    {
        await Task.Delay(10);
        throw new InvalidOperationException("boom");
    }

    private async Task Outer()
    {
        await Fails();
        Console.WriteLine("not reached");
    }

    private async void Fire()
    {
        await Task.Delay(50);
        await _all!;                                  // a task kept in a field, awaited by a second method
        Console.WriteLine("fire");
        _fired = true;
    }

    private async Task Pulse()
    {
        await Task.Yield();
        _pulses++;
    }

    public override void OnInit()
    {
        Console.WriteLine("init");                                    // init
        _seq = Sequence("sw");                                        // seq start
        _sum = Sum(new List<int> { 1, 2, 3 });
        _outer = Outer();
        _all = Task.WhenAll(_seq, _sum);
        Fire();
        Console.WriteLine("started " + _sum.IsCompleted + " " + _loader.Calls);   // started False 1
    }

    // In between, from the tasks:
    //   seq hi sw item10
    //   seq waited
    //   seq end 5 40
    //   fire
    public override void OnFrame()
    {
        if (!_fired) return;
        if (!_reported)
        {
            _reported = true;
            Console.WriteLine("sum " + _sum!.Result + " calls " + _loader.Calls);          // sum 62 calls 5
            Console.WriteLine("outer " + _outer!.IsCompleted + " " + _outer.IsFaulted);     // outer True True
            try { _outer.Wait(); }
            catch (InvalidOperationException e) { Console.WriteLine("caught " + e.Message); } // caught boom
            Console.WriteLine("all " + _all!.IsCompleted + " " + _seq!.IsCompletedSuccessfully); // all True True
            return;
        }

        // A task every frame: state machines and tasks must not pile up.
        Pulse();
        _since++;
        if (_since == 40) _base = GC.GetTotalMemory(false);
        if (_since == 300)
        {
            long grown = GC.GetTotalMemory(false) - _base;
            Console.WriteLine("stable " + (grown < 8192) + " " + (_pulses > 250));         // stable True True
            Console.WriteLine("done");                                                      // done
        }
    }
}
