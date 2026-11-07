// Auto-generated from services\ectx.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Ectx
{
    public static extern Result ectxrInitialize();
    public static extern void ectxrExit();
    public static extern IntPtr ectxrGetServiceSession();
    public static extern Result ectxrPullContext(ref s32 out0, ref u32 out_total_size, ref u32 out_size, IntPtr dst, ulong dst_size, u32 descriptor, Result result);
}
