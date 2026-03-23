// Auto-generated from nvidia\gpu_channel.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Nvidia;

public unsafe struct NvGpuChannel
{
    public NvChannel @base;
    public Event error_event;
    public u64 object_id;
    public NvFence fence;
    public u32 fence_incr;
    public nvioctl_gpfifo_entry entries;
    public u32 num_entries;
}

public static class Gpu_channel
{
    public static extern Result nvGpuChannelCreate(ref NvGpuChannel c, ref struct NvAddressSpace @as, NvChannelPriority prio);
    public static extern void nvGpuChannelClose(ref NvGpuChannel c);
    public static extern Result nvGpuChannelZcullBind(ref NvGpuChannel c, iova_t iova);
    public static extern Result nvGpuChannelAppendEntry(ref NvGpuChannel c, iova_t start, ulong num_cmds, u32 flags, u32 flush_threshold);
    public static extern Result nvGpuChannelKickoff(ref NvGpuChannel c);
    public static extern Result nvGpuChannelGetErrorNotification(ref NvGpuChannel c, ref NvNotification notif);
    public static extern Result nvGpuChannelGetErrorInfo(ref NvGpuChannel c, ref NvError error);
}
