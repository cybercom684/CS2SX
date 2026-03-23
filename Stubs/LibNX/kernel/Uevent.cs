// Auto-generated from kernel\uevent.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Kernel;

public static class Uevent
{
    public static extern void ueventCreate(ref UEvent e, bool auto_clear);
    public static extern void ueventClear(ref UEvent e);
    public static extern void ueventSignal(ref UEvent e);
}
