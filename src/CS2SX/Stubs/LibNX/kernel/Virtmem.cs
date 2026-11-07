// Auto-generated from kernel\virtmem.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Kernel;

public static class Virtmem
{
    public static extern void virtmemLock();
    public static extern void virtmemUnlock();
    public static extern IntPtr virtmemFindAslr(ulong size, ulong guard_size);
    public static extern IntPtr virtmemFindStack(ulong size, ulong guard_size);
    public static extern IntPtr virtmemFindCodeMemory(ulong size, ulong guard_size);
    public static extern IntPtr virtmemAddReservation(IntPtr mem, ulong size);
    public static extern void virtmemRemoveReservation(ref VirtmemReservation rv);
}
