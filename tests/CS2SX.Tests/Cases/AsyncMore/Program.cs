using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// More async forms: await in catch and finally, using with await, await in
// do-while and for headers, async lambdas and the results of Task.WhenAll.
// Expected output in comments.
public class Resource
{
    public static int Open;
    public string Name;
    public Resource(string name) { Name = name; Open++; }
    public void Dispose() { Open--; Console.WriteLine("dispose " + Name); }
}

public class AsyncMoreApp : SwitchApp
{
    private int _ticks;
    private Func<Task>? _later;
    private bool _done;

    private static async Task<int> Value(int v)
    {
        await Task.Yield();
        return v;
    }

    private static async Task<string> Name()
    {
        await Task.Yield();
        return "name";
    }

    private static async Task Fail(string msg)
    {
        await Task.Yield();
        throw new InvalidOperationException(msg);
    }

    private async Task<string> Guarded()
    {
        string log = "";
        try
        {
            await Fail("a");
        }
        catch (ArgumentException)
        {
            log += "wrong ";
        }
        catch (InvalidOperationException e)
        {
            int v = await Value(5);              // await in catch
            log += "caught " + e.Message + v;
        }
        finally
        {
            await Task.Yield();                  // await in finally
            log += " finally";
        }
        return log;
    }

    private async Task<string> Rethrows()
    {
        try
        {
            try { await Fail("b"); }
            catch (InvalidOperationException)
            {
                await Task.Yield();
                throw;                           // rethrow from an awaited catch
            }
        }
        catch (Exception e) { return "outer " + e.Message; }
        return "none";
    }

    private async Task<int> Loops()
    {
        int n = 0;
        do { n++; } while (await Value(n) < 3);                 // await in do-while
        int sum = 0;
        for (int i = await Value(1); i <= await Value(3); i++)  // await in the for header
            sum += i;
        using (var r = new Resource("r1"))
        {
            await Task.Yield();                                  // await in using
            sum += r.Name.Length;
        }
        return n * 100 + sum;
    }

    private async Task Script()
    {
        Console.WriteLine(await Guarded());                     // caught a5 finally
        Console.WriteLine(await Rethrows());                    // outer b
        Console.WriteLine("loops " + await Loops() + " " + Resource.Open);   // dispose r1 / loops 308 0

        bool on = true;
        if (on && await Value(1) == 1) Console.WriteLine("and");               // and
        string? none = null;
        string picked = none ?? await Name();
        int pick = on ? await Value(7) : 0;
        Console.WriteLine(picked + " " + pick);                                   // name 7

        int[] all = await Task.WhenAll(Value(1), Value(2), Value(3));
        Console.WriteLine("all " + all.Length + " " + (all[0] + all[1] + all[2]));   // all 3 6

        int bonus = 10;
        Func<int, Task<int>> add = async x => { await Task.Yield(); return x + bonus; };
        Console.WriteLine("lambda " + await add(5));            // lambda 15

        _later = async () => { await Task.Delay(30); Console.WriteLine("later " + (_ticks > 0)); };
        await _later();                                         // later True
        Action fire = async () => { await Task.Yield(); _done = true; };
        fire();
    }

    public override void OnInit() { Script(); }

    public override void OnFrame()
    {
        _ticks++;
        if (_done) { _done = false; Console.WriteLine("done"); }   // done
    }
}
