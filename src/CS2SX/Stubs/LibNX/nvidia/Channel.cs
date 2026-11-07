// Auto-generated from nvidia\channel.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Nvidia;

public unsafe struct NvChannel
{
    public u32 fd;
    public bool has_init;
}

public static class Channel
{
    public static extern Result nvChannelCreate(ref NvChannel c, ref byte dev);
    public static extern void nvChannelClose(ref NvChannel c);
    public static extern Result nvChannelSetPriority(ref NvChannel c, NvChannelPriority prio);
    public static extern Result nvChannelSetTimeout(ref NvChannel c, u32 timeout);
}
