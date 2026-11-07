// Auto-generated from nvidia\gpu.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Nvidia;

public static class Gpu
{
    public static extern Result nvGpuInit();
    public static extern void nvGpuExit();
    public static extern IntPtr nvGpuGetCharacteristics();
    public static extern u32 nvGpuGetZcullCtxSize();
    public static extern IntPtr nvGpuGetZcullInfo();
    public static extern IntPtr nvGpuGetTpcMasks(ref u32 num_masks_out);
    public static extern Result nvGpuZbcGetActiveSlotMask(ref u32 out_slot, ref u32 out_mask);
    public static extern Result nvGpuZbcAddColor(u32 color_l2, u32 color_ds, u32 format);
    public static extern Result nvGpuZbcAddDepth(float depth);
    public static extern Result nvGpuGetTimestamp(ref u64 ts);
}
