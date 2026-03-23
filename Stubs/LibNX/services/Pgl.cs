// Auto-generated from services\pgl.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum PglLaunchFlag
{
    PglLaunchFlag_None = 0,
    PglLaunchFlag_EnableDetailedCrashReport = (1 << 0),
    PglLaunchFlag_EnableCrashReportScreenShotForProduction = (1 << 1),
    PglLaunchFlag_EnableCrashReportScreenShotForDevelop = (1 << 2),
}

public enum PglSnapShotDumpType
{
    PglSnapShotDumpType_None = 0,
    PglSnapShotDumpType_Auto = 1,
    PglSnapShotDumpType_Full = 2,
}

public unsafe struct PglContentMetaInfo
{
    public u64 id;
    public u32 version;
    public u8 content_type;
    public u8 id_offset;
    public fixed u8 reserved_0E[2];
}

public static class Pgl
{
    public static extern Result pglInitialize();
    public static extern void pglExit();
    public static extern IntPtr pglGetServiceSessionCmif();
    public static extern IntPtr pglGetServiceSessionTipc();
    public static extern Result pglLaunchProgram(ref u64 out_pid, ref NcmProgramLocation loc, u32 pm_launch_flags, u8 pgl_launch_flags);
    public static extern Result pglTerminateProcess(u64 pid);
    public static extern Result pglLaunchProgramFromHost(ref u64 out_pid, ref byte content_path, u32 pm_launch_flags);
    public static extern Result pglGetHostContentMetaInfo(ref PglContentMetaInfo @out, ref byte content_path);
    public static extern Result pglGetApplicationProcessId(ref u64 @out);
    public static extern Result pglBoostSystemMemoryResourceLimit(u64 size);
    public static extern Result pglIsProcessTracked(ref bool @out, u64 pid);
    public static extern Result pglEnableApplicationCrashReport(bool en);
    public static extern Result pglIsApplicationCrashReportEnabled(ref bool @out);
    public static extern Result pglEnableApplicationAllThreadDumpOnCrash(bool en);
    public static extern Result pglTriggerApplicationSnapShotDumper(PglSnapShotDumpType dump_type, ref byte arg);
    public static extern Result pglGetEventObserver(ref PglEventObserver @out);
    public static extern Result pglEventObserverGetProcessEvent(ref PglEventObserver observer, ref Event @out);
    public static extern Result pglEventObserverGetProcessEventInfo(ref PglEventObserver observer, ref PmProcessEventInfo @out);
    public static extern void pglEventObserverClose(ref PglEventObserver observer);
}
