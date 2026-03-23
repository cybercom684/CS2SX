// Auto-generated from kernel\shmem.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Kernel;

public unsafe struct SharedMemory
{
    public Handle handle;
    public ulong size;
    public Permission perm;
    public IntPtr map_addr;
}

public static class Shmem
{
    public static extern Result shmemCreate(ref SharedMemory s, ulong size, Permission local_perm, Permission remote_perm);
    public static extern void shmemLoadRemote(ref SharedMemory s, Handle handle, ulong size, Permission perm);
    public static extern Result shmemMap(ref SharedMemory s);
    public static extern Result shmemUnmap(ref SharedMemory s);
    public static extern Result shmemClose(ref SharedMemory s);
}
