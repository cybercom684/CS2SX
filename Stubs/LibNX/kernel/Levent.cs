// Auto-generated from kernel\levent.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Kernel;

public unsafe struct LEvent
{
    public u32 counter;
    public bool autoclear;
}

public static class Levent
{
    public static extern bool leventWait(ref LEvent le, u64 timeout_ns);
    public static extern bool leventTryWait(ref LEvent le);
    public static extern void leventSignal(ref LEvent le);
    public static extern void leventClear(ref LEvent le);
}
