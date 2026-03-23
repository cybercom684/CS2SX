// Auto-generated from services\tc.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Tc
{
    public static extern Result tcInitialize();
    public static extern void tcExit();
    public static extern IntPtr tcGetServiceSession();
    public static extern Result tcEnableFanControl();
    public static extern Result tcDisableFanControl();
    public static extern Result tcIsFanControlEnabled(ref bool status);
    public static extern Result tcGetSkinTemperatureMilliC(ref s32 skinTemp);
}
