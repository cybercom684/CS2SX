using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Forms of async/await that cannot be turned into a state machine. CS2SX must
// reject each one with a clear error (expect-errors.txt).
public class Diagnostics_AsyncApp : SwitchApp
{
    private async Task<int> Value() { await Task.Delay(1); return 1; }
    private async Task<bool> Ready() { await Task.Delay(1); return true; }

    private async Task InTry()
    {
        try { Console.WriteLine("x"); }
        catch (Exception e) when (e.Message != "") { await Task.Delay(1); }
    }

    private async Task InCondition()
    {
        do { if (DateTime.Now.Second > 99) continue; } while (await Ready());
    }

    private async Task ShortCircuit(bool on)
    {
        Console.WriteLine(on && await Ready());
    }

    private async Task OutVar(string s)
    {
        if (int.TryParse(s, out var n)) Console.WriteLine(n);
        await Task.Delay(1);
        Console.WriteLine(n);
    }

    private async Task OverDictionary(Dictionary<string, int> d)
    {
        foreach (var kv in d) { await Task.Delay(kv.Value); }
    }

    public override void OnInit()
    {
        Action outer = () => { Func<Task> inner = async () => { await Task.Delay(1); }; };
    }
}
