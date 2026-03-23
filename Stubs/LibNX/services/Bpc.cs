// Auto-generated from services\bpc.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum BpcSleepButtonState
{
    BpcSleepButtonState_Held = 0,
    BpcSleepButtonState_Released = 1,
}

public static class Bpc
{
    public static extern Result bpcInitialize();
    public static extern void bpcExit();
    public static extern IntPtr bpcGetServiceSession();
    public static extern Result bpcShutdownSystem();
    public static extern Result bpcRebootSystem();
    public static extern Result bpcGetSleepButtonState(ref BpcSleepButtonState @out);
    public static extern Result bpcGetPowerButton(ref bool out_is_pushed);
}
