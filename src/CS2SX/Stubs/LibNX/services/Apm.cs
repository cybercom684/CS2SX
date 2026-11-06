// Auto-generated from services\apm.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum ApmPerformanceMode
{
    ApmPerformanceMode_Invalid = -1,
    ApmPerformanceMode_Normal = 0,
    ApmPerformanceMode_Boost = 1,
}

public enum ApmCpuBoostMode
{
    ApmCpuBoostMode_Normal = 0,
    ApmCpuBoostMode_FastLoad = 1,
    ApmCpuBoostMode_Type2 = 2,
}

public static class Apm
{
    public static extern Result apmInitialize();
    public static extern void apmExit();
    public static extern IntPtr apmGetServiceSession();
    public static extern IntPtr apmGetServiceSession_Session();
    public static extern Result apmGetPerformanceMode(ref ApmPerformanceMode out_performanceMode);
    public static extern Result apmSetPerformanceConfiguration(ApmPerformanceMode PerformanceMode, u32 PerformanceConfiguration);
    public static extern Result apmGetPerformanceConfiguration(ApmPerformanceMode PerformanceMode, ref u32 PerformanceConfiguration);
}
