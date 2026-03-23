// Auto-generated from services\pm.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum PmLaunchFlag
{
    PmLaunchFlag_None = 0,
    PmLaunchFlag_SignalOnExit = (1 << 0),
    PmLaunchFlag_SignalOnStart = (1 << 1),
    PmLaunchFlag_SignalOnCrash = (1 << 2),
    PmLaunchFlag_SignalOnDebug = (1 << 3),
    PmLaunchFlag_StartSuspended = (1 << 4),
    PmLaunchFlag_DisableAslr = (1 << 5),
    PmLaunchFlagOld_SignalOnExit = (1 << 0),
    PmLaunchFlagOld_StartSuspended = (1 << 1),
    PmLaunchFlagOld_SignalOnCrash = (1 << 2),
    PmLaunchFlagOld_DisableAslr = (1 << 3),
    PmLaunchFlagOld_SignalOnDebug = (1 << 4),
    PmLaunchFlagOld_SignalOnStart = (1 << 5),
}

public enum PmProcessEvent
{
    PmProcessEvent_None = 0,
    PmProcessEvent_Exit = 1,
    PmProcessEvent_Start = 2,
    PmProcessEvent_Crash = 3,
    PmProcessEvent_DebugStart = 4,
    PmProcessEvent_DebugBreak = 5,
}

public enum PmBootMode
{
    PmBootMode_Normal = 0,
    PmBootMode_Maintenance = 1,
    PmBootMode_SafeMode = 2,
}

public unsafe struct PmProcessEventInfo
{
    public PmProcessEvent @event;
    public u64 process_id;
}

public unsafe struct PmResourceLimitValues
{
    public u64 physical_memory;
    public u32 thread_count;
    public u32 event_count;
    public u32 transfer_memory_count;
    public u32 session_count;
}

public static class Pm
{
    public static extern Result pmdmntInitialize();
    public static extern void pmdmntExit();
    public static extern IntPtr pmdmntGetServiceSession();
    public static extern Result pminfoInitialize();
    public static extern void pminfoExit();
    public static extern IntPtr pminfoGetServiceSession();
    public static extern Result pmshellInitialize();
    public static extern void pmshellExit();
    public static extern IntPtr pmshellGetServiceSession();
    public static extern Result pmbmInitialize();
    public static extern void pmbmExit();
    public static extern IntPtr pmbmGetServiceSession();
    public static extern Result pmbmGetBootMode(ref PmBootMode @out);
    public static extern Result pmbmSetMaintenanceBoot();
    public static extern Result pmdmntGetJitDebugProcessIdList(ref u32 out_count, ref u64 out_pids, ulong max_pids);
    public static extern Result pmdmntStartProcess(u64 pid);
    public static extern Result pmdmntGetProcessId(ref u64 pid_out, u64 program_id);
    public static extern Result pmdmntHookToCreateProcess(ref Event @out, u64 program_id);
    public static extern Result pmdmntGetApplicationProcessId(ref u64 pid_out);
    public static extern Result pmdmntHookToCreateApplicationProcess(ref Event @out);
    public static extern Result pmdmntClearHook(u32 which);
    public static extern Result pmdmntGetProgramId(ref u64 program_id_out, u64 pid);
    public static extern Result pminfoGetProgramId(ref u64 program_id_out, u64 pid);
    public static extern Result pminfoGetAppletCurrentResourceLimitValues(ref PmResourceLimitValues @out);
    public static extern Result pminfoGetAppletPeakResourceLimitValues(ref PmResourceLimitValues @out);
    public static extern Result pmshellLaunchProgram(u32 launch_flags, ref NcmProgramLocation location, ref u64 pid);
    public static extern Result pmshellTerminateProcess(u64 processID);
    public static extern Result pmshellTerminateProgram(u64 program_id);
    public static extern Result pmshellGetProcessEventHandle(ref Event @out);
    public static extern Result pmshellGetProcessEventInfo(ref PmProcessEventInfo @out);
    public static extern Result pmshellCleanupProcess(u64 pid);
    public static extern Result pmshellClearJitDebugOccured(u64 pid);
    public static extern Result pmshellNotifyBootFinished();
    public static extern Result pmshellGetApplicationProcessIdForShell(ref u64 pid_out);
    public static extern Result pmshellBoostSystemMemoryResourceLimit(u64 boost_size);
    public static extern Result pmshellBoostApplicationThreadResourceLimit();
    public static extern Result pmshellBoostSystemThreadResourceLimit();
    public static extern Result pmshellGetProcessId(ref u64 pid_out, u64 program_id);
}
