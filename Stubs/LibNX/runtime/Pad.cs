// Auto-generated from runtime\pad.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Runtime;

public unsafe struct PadState
{
    public u8 id_mask;
    public u8 active_id_mask;
    public bool read_handheld;
    public bool active_handheld;
    public u32 style_set;
    public u32 attributes;
    public u64 buttons_cur;
    public u64 buttons_old;
    // skipped array: HidAnalogStickState sticks[2]
    public fixed u32 gc_triggers[2];
}

public unsafe struct PadRepeater
{
    public u64 button_mask;
    public s32 counter;
    public u16 delay;
    public u16 repeat;
}

public static class Pad
{
    public static extern void padConfigureInput(u32 max_players, u32 style_set);
    public static extern void padInitializeWithMask(ref PadState pad, u64 mask);
    public static extern void padUpdate(ref PadState pad);
    public static extern void padRepeaterUpdate(ref PadRepeater r, u64 button_mask);
}
