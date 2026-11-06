// Auto-generated from kernel\tmem.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Kernel;

public unsafe struct TransferMemory
{
    public Handle handle;
    public ulong size;
    public Permission perm;
    public IntPtr src_addr;
    public IntPtr map_addr;
}

public static class Tmem
{
    public static extern Result tmemCreate(ref TransferMemory t, ulong size, Permission perm);
    public static extern Result tmemCreateFromMemory(ref TransferMemory t, IntPtr buf, ulong size, Permission perm);
    public static extern void tmemLoadRemote(ref TransferMemory t, Handle handle, ulong size, Permission perm);
    public static extern Result tmemMap(ref TransferMemory t);
    public static extern Result tmemUnmap(ref TransferMemory t);
    public static extern Result tmemCloseHandle(ref TransferMemory t);
    public static extern Result tmemWaitForPermission(ref TransferMemory t, Permission perm);
    public static extern Result tmemClose(ref TransferMemory t);
}
