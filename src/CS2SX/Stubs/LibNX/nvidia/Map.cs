// Auto-generated from nvidia\map.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Nvidia;

public unsafe struct NvMap
{
    public u32 handle;
    public u32 id;
    public u32 size;
    public IntPtr cpu_addr;
    public NvKind kind;
    public bool has_init;
    public bool is_cpu_cacheable;
}

public static class Map
{
    public static extern Result nvMapInit();
    public static extern u32 nvMapGetFd();
    public static extern void nvMapExit();
    public static extern Result nvMapCreate(ref NvMap m, IntPtr cpu_addr, u32 size, u32 align, NvKind kind, bool is_cpu_cacheable);
    public static extern Result nvMapLoadRemote(ref NvMap m, u32 id);
    public static extern void nvMapClose(ref NvMap m);
}
