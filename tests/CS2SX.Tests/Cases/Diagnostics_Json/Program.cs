using System;
using System.Collections.Generic;
using System.Text.Json;

// JsonSerializer uses that cannot be turned into generated code. CS2SX must
// reject each one with a clear error (expect-errors.txt).
public interface IShape { int Size { get; } }
public class Holder { public IShape? Shape { get; set; } }
public class NoCtor { public int A { get; } public NoCtor(int a, int b) { A = a; } }
public class WithDate { public DateTime When { get; set; } }

public class Diagnostics_JsonApp : SwitchApp
{
    public override void OnInit()
    {
        object o = new Holder();
        Console.WriteLine(JsonSerializer.Serialize(new Holder()));
        Console.WriteLine(JsonSerializer.Deserialize<NoCtor>("{}"));
        Console.WriteLine(JsonSerializer.Serialize(new WithDate()));
        Console.WriteLine(JsonSerializer.Serialize(o));
        Console.WriteLine(JsonSerializer.Serialize(new int[2, 2]));
    }

    public override void OnFrame() { }
}
