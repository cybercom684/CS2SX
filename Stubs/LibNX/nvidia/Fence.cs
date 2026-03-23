// Auto-generated from nvidia\fence.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Nvidia;

public unsafe struct NvMultiFence
{
    public u32 num_fences;
    // skipped array: NvFence fences[4]
}

public static class Fence
{
    public static extern Result nvFenceInit();
    public static extern void nvFenceExit();
    public static extern u32 nvFenceGetFd();
    public static extern Result nvFenceWait(ref NvFence f, s32 timeout_us);
    public static extern Result nvMultiFenceWait(ref NvMultiFence mf, s32 timeout_us);
}
