using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// async/await around control flow: await in a try block (the catch sees what the
// awaited task threw, finally runs once), await in a while condition,
// Task.WhenAny, Task.Delay(TimeSpan), and a failed task nobody looks at.
// Expected output in comments.
public class Source
{
    private int _left = 3;
    public int Total => 10 - _left;

    public async Task<bool> Next()
    {
        await Task.Yield();
        _left--;
        return _left >= 0;
    }

    public async Task<int> Risky(int n)
    {
        await Task.Delay(5);
        if (n < 0) throw new ArgumentException("negative input");
        return n * 2;
    }
}

public class AsyncFlowApp : SwitchApp
{
    private readonly Source _source = new Source();
    private bool _finished;
    private int _since;

    private async Task<string> Guarded(int n)
    {
        string result = "none";
        try
        {
            Console.WriteLine("try " + n);
            int v = await _source.Risky(n);
            result = "ok " + v;
            await Task.Yield();
            result += "!";
        }
        catch (ArgumentException e)
        {
            result = "caught " + e.Message;
        }
        finally
        {
            Console.WriteLine("finally " + n);
        }
        return result;
    }

    private async Task<int> Nested()
    {
        int steps = 0;
        for (int i = 0; i < 2; i++)
        {
            try
            {
                try
                {
                    steps += await _source.Risky(i == 0 ? 1 : -1);
                }
                finally { steps += 100; }
            }
            catch (ArgumentException) { steps += 1000; }
        }
        return steps;
    }

    private async Task<int> Drain()
    {
        int rounds = 0;
        while (await _source.Next())
        {
            rounds++;
            if (rounds > 10) break;
        }
        return rounds;
    }

    private async Task Lost()
    {
        await Task.Yield();
        throw new InvalidOperationException("nobody waits for this");
    }

    private async Task Script()
    {
        Console.WriteLine(await Guarded(4));              // try 4 / finally 4 / ok 8!
        Console.WriteLine(await Guarded(-1));             // try -1 / finally -1 / caught negative input
        Console.WriteLine("nested " + await Nested());    // nested 1202
        Console.WriteLine("drain " + await Drain() + " " + _source.Total);   // drain 3 11

        var slow = Task.Delay(TimeSpan.FromMilliseconds(600));
        var fast = Task.Delay(20);
        var first = await Task.WhenAny(slow, fast);
        Console.WriteLine("any " + (first == fast) + " " + slow.IsCompleted);   // any True False

        Lost();
        _finished = true;
    }

    public override void OnInit()
    {
        Script();
    }

    public override void OnFrame()
    {
        if (!_finished) return;
        // The lost task is released a few frames later:
        //   Unobserved task exception: nobody waits for this
        if (++_since == 60) Console.WriteLine("done");    // done
    }
}
