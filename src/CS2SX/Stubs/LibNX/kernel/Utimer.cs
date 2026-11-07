// Auto-generated from kernel\utimer.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Kernel;

public enum TimerType
{
    TimerType_OneShot,
    TimerType_Repeating,
}

public static class Utimer
{
    public static extern void utimerCreate(ref UTimer t, u64 interval, TimerType type);
    public static extern void utimerStart(ref UTimer t);
    public static extern void utimerStop(ref UTimer t);
}
