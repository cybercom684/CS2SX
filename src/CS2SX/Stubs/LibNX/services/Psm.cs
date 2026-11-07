// Auto-generated from services\psm.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum PsmChargerType
{
    PsmChargerType_Unconnected = 0,
    PsmChargerType_EnoughPower = 1,
    PsmChargerType_LowPower = 2,
    PsmChargerType_NotSupported = 3,
}

public enum PsmBatteryVoltageState
{
    PsmBatteryVoltageState_NeedsShutdown = 0,
    PsmBatteryVoltageState_NeedsSleep = 1,
    PsmBatteryVoltageState_NoPerformanceBoost = 2,
    PsmBatteryVoltageState_Normal = 3,
}

public unsafe struct PsmSession
{
    public Service s;
    public Event StateChangeEvent;
}

public static class Psm
{
    public static extern Result psmInitialize();
    public static extern void psmExit();
    public static extern IntPtr psmGetServiceSession();
    public static extern Result psmGetBatteryChargePercentage(ref u32 @out);
    public static extern Result psmGetChargerType(ref PsmChargerType @out);
    public static extern Result psmGetBatteryVoltageState(ref PsmBatteryVoltageState @out);
    public static extern Result psmGetRawBatteryChargePercentage(ref double @out);
    public static extern Result psmIsEnoughPowerSupplied(ref bool @out);
    public static extern Result psmGetBatteryAgePercentage(ref double @out);
    public static extern Result psmBindStateChangeEvent(ref PsmSession s, bool ChargerType, bool PowerSupply, bool BatteryVoltage);
    public static extern Result psmWaitStateChangeEvent(ref PsmSession s, u64 timeout);
    public static extern Result psmUnbindStateChangeEvent(ref PsmSession s);
}
