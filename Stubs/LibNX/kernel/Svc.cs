// Auto-generated from kernel\svc.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Kernel;

public enum MemoryType
{
    MemType_Unmapped = 0x00,
    MemType_Io = 0x01,
    MemType_Normal = 0x02,
    MemType_CodeStatic = 0x03,
    MemType_CodeMutable = 0x04,
    MemType_Heap = 0x05,
    MemType_SharedMem = 0x06,
    MemType_WeirdMappedMem = 0x07,
    MemType_ModuleCodeStatic = 0x08,
    MemType_ModuleCodeMutable = 0x09,
    MemType_IpcBuffer0 = 0x0A,
    MemType_MappedMemory = 0x0B,
    MemType_ThreadLocal = 0x0C,
    MemType_TransferMemIsolated = 0x0D,
    MemType_TransferMem = 0x0E,
    MemType_ProcessMem = 0x0F,
    MemType_Reserved = 0x10,
    MemType_IpcBuffer1 = 0x11,
    MemType_IpcBuffer3 = 0x12,
    MemType_KernelStack = 0x13,
    MemType_CodeReadOnly = 0x14,
    MemType_CodeWritable = 0x15,
    MemType_Coverage = 0x16,
    MemType_Insecure = 0x17,
}

public enum MemoryState
{
    MemState_Type = 0xFF,
    MemState_PermChangeAllowed = (1 << 8),
    MemState_ForceRwByDebugSyscalls = (1 << 9),
    MemState_IpcSendAllowed_Type0 = (1 << 10),
    MemState_IpcSendAllowed_Type3 = (1 << 11),
    MemState_IpcSendAllowed_Type1 = (1 << 12),
    MemState_ProcessPermChangeAllowed = (1 << 14),
    MemState_MapAllowed = (1 << 15),
    MemState_UnmapProcessCodeMemAllowed = (1 << 16),
    MemState_TransferMemAllowed = (1 << 17),
    MemState_QueryPAddrAllowed = (1 << 18),
    MemState_MapDeviceAllowed = (1 << 19),
    MemState_MapDeviceAlignedAllowed = (1 << 20),
    MemState_IpcBufferAllowed = (1 << 21),
    MemState_IsPoolAllocated = (1 << 22),
    MemState_IsRefCounted = MemState_IsPoolAllocated,
    MemState_MapProcessAllowed = (1 << 23),
    MemState_AttrChangeAllowed = (1 << 24),
    MemState_CodeMemAllowed = (1 << 25),
}

public enum MemoryAttribute
{
    MemAttr_IsBorrowed = (1 << 0),
    MemAttr_IsIpcMapped = (1 << 1),
    MemAttr_IsDeviceMapped = (1 << 2),
    MemAttr_IsUncached = (1 << 3),
    MemAttr_IsPermissionLocked = (1 << 4),
}

public enum Permission
{
    Perm_None = 0,
    Perm_R = (1 << 0),
    Perm_W = (1 << 1),
    Perm_X = (1 << 2),
    Perm_Rw = Perm_R | Perm_W,
    Perm_Rx = Perm_R | Perm_X,
    Perm_DontCare = (1 << 28),
}

public enum BreakReason
{
    BreakReason_Panic = 0,
    BreakReason_Assert = 1,
    BreakReason_User = 2,
    BreakReason_PreLoadDll = 3,
    BreakReason_PostLoadDll = 4,
    BreakReason_PreUnloadDll = 5,
    BreakReason_PostUnloadDll = 6,
    BreakReason_CppException = 7,
    BreakReason_NotificationOnlyFlag = 0x80000000,
}

public enum CodeMapOperation
{
    CodeMapOperation_MapOwner = 0,
    CodeMapOperation_MapSlave = 1,
    CodeMapOperation_UnmapOwner = 2,
    CodeMapOperation_UnmapSlave = 3,
}

public enum LimitableResource
{
    LimitableResource_Memory = 0,
    LimitableResource_Threads = 1,
    LimitableResource_Events = 2,
    LimitableResource_TransferMemories = 3,
    LimitableResource_Sessions = 4,
}

public enum ThreadActivity
{
    ThreadActivity_Runnable = 0,
    ThreadActivity_Paused = 1,
}

public enum ProcessInfoType
{
    ProcessInfoType_ProcessState = 0,
}

public enum ProcessState
{
    ProcessState_Created = 0,
    ProcessState_CreatedAttached = 1,
    ProcessState_Running = 2,
    ProcessState_Crashed = 3,
    ProcessState_RunningAttached = 4,
    ProcessState_Exiting = 5,
    ProcessState_Exited = 6,
    ProcessState_DebugSuspended = 7,
}

public enum ProcessActivity
{
    ProcessActivity_Runnable = 0,
    ProcessActivity_Paused = 1,
}

public enum DebugThreadParam
{
    DebugThreadParam_ActualPriority = 0,
    DebugThreadParam_State = 1,
    DebugThreadParam_IdealCore = 2,
    DebugThreadParam_CurrentCore = 3,
    DebugThreadParam_CoreMask = 4,
}

public enum InfoType
{
    InfoType_CoreMask = 0,
    InfoType_PriorityMask = 1,
    InfoType_AliasRegionAddress = 2,
    InfoType_AliasRegionSize = 3,
    InfoType_HeapRegionAddress = 4,
    InfoType_HeapRegionSize = 5,
    InfoType_TotalMemorySize = 6,
    InfoType_UsedMemorySize = 7,
    InfoType_DebuggerAttached = 8,
    InfoType_ResourceLimit = 9,
    InfoType_IdleTickCount = 10,
    InfoType_RandomEntropy = 11,
    InfoType_AslrRegionAddress = 12,
    InfoType_AslrRegionSize = 13,
    InfoType_StackRegionAddress = 14,
    InfoType_StackRegionSize = 15,
    InfoType_SystemResourceSizeTotal = 16,
    InfoType_SystemResourceSizeUsed = 17,
    InfoType_ProgramId = 18,
    InfoType_InitialProcessIdRange = 19,
    InfoType_UserExceptionContextAddress = 20,
    InfoType_TotalNonSystemMemorySize = 21,
    InfoType_UsedNonSystemMemorySize = 22,
    InfoType_IsApplication = 23,
    InfoType_FreeThreadCount = 24,
    InfoType_ThreadTickCount = 25,
    InfoType_IsSvcPermitted = 26,
    InfoType_IoRegionHint = 27,
    InfoType_AliasRegionExtraSize = 28,
    InfoType_TransferMemoryHint = 34,
    InfoType_ThreadTickCountDeprecated = 0xF0000002,
}

public enum SystemInfoType
{
    SystemInfoType_TotalPhysicalMemorySize = 0,
    SystemInfoType_UsedPhysicalMemorySize = 1,
    SystemInfoType_InitialProcessIdRange = 2,
}

public enum TickCountInfo
{
    TickCountInfo_Core0 = 0,
    TickCountInfo_Core1 = 1,
    TickCountInfo_Core2 = 2,
    TickCountInfo_Core3 = 3,
    TickCountInfo_Total = UINT64_MAX,
}

public enum InitialProcessIdRangeInfo
{
    InitialProcessIdRangeInfo_Minimum = 0,
    InitialProcessIdRangeInfo_Maximum = 1,
}

public enum PhysicalMemorySystemInfo
{
    PhysicalMemorySystemInfo_Application = 0,
    PhysicalMemorySystemInfo_Applet = 1,
    PhysicalMemorySystemInfo_System = 2,
    PhysicalMemorySystemInfo_SystemUnsafe = 3,
}

public enum YieldType
{
    YieldType_WithoutCoreMigration = 0l,
    YieldType_WithCoreMigration = -1l,
    YieldType_ToAnyThread = -2l,
}

public enum SignalType
{
    SignalType_Signal = 0,
    SignalType_SignalAndIncrementIfEqual = 1,
    SignalType_SignalAndModifyBasedOnWaitingThreadCountIfEqual = 2,
}

public enum ArbitrationType
{
    ArbitrationType_WaitIfLessThan = 0,
    ArbitrationType_DecrementAndWaitIfLessThan = 1,
    ArbitrationType_WaitIfEqual = 2,
    ArbitrationType_WaitIfEqual64 = 3,
}

public enum MemoryMapping
{
    MemoryMapping_IoRegister = 0,
    MemoryMapping_Uncached = 1,
    MemoryMapping_Memory = 2,
}

public enum IoPoolType
{
    IoPoolType_PcieA2 = 0,
}

public unsafe struct MemoryInfo
{
    public u64 addr;
    public u64 size;
    public u32 type;
    public u32 attr;
    public u32 perm;
    public u32 ipc_refcount;
    public u32 device_refcount;
    public u32 padding;
}

public unsafe struct PhysicalMemoryInfo
{
    public u64 physical_address;
    public u64 virtual_address;
    public u64 size;
}

public unsafe struct LastThreadContext
{
    public u64 fp;
    public u64 sp;
    public u64 lr;
    public u64 pc;
}

public static class Svc
{
    public static extern Result svcSetHeapSize(IntPtr out_addr, u64 size);
    public static extern Result svcSetMemoryPermission(IntPtr addr, u64 size, u32 perm);
    public static extern Result svcSetMemoryAttribute(IntPtr addr, u64 size, u32 val0, u32 val1);
    public static extern Result svcMapMemory(IntPtr dst_addr, IntPtr src_addr, u64 size);
    public static extern Result svcUnmapMemory(IntPtr dst_addr, IntPtr src_addr, u64 size);
    public static extern Result svcQueryMemory(ref MemoryInfo meminfo_ptr, ref u32 pageinfo, u64 addr);
    public static extern void NX_NORETURN svcExitProcess();
    public static extern Result svcCreateThread(ref Handle @out, IntPtr entry, IntPtr arg, IntPtr stack_top, int prio, int cpuid);
    public static extern Result svcStartThread(Handle handle);
    public static extern void NX_NORETURN svcExitThread();
    public static extern void svcSleepThread(s64 nano);
    public static extern Result svcGetThreadPriority(ref s32 priority, Handle handle);
    public static extern Result svcSetThreadPriority(Handle handle, u32 priority);
    public static extern Result svcGetThreadCoreMask(ref s32 preferred_core, ref u64 affinity_mask, Handle handle);
    public static extern Result svcSetThreadCoreMask(Handle handle, s32 preferred_core, u32 affinity_mask);
    public static extern u32 svcGetCurrentProcessorNumber();
    public static extern Result svcSignalEvent(Handle handle);
    public static extern Result svcClearEvent(Handle handle);
    public static extern Result svcMapSharedMemory(Handle handle, IntPtr addr, ulong size, u32 perm);
    public static extern Result svcUnmapSharedMemory(Handle handle, IntPtr addr, ulong size);
    public static extern Result svcCreateTransferMemory(ref Handle @out, IntPtr addr, ulong size, u32 perm);
    public static extern Result svcCloseHandle(Handle handle);
    public static extern Result svcResetSignal(Handle handle);
    public static extern Result svcWaitSynchronization(ref s32 index, ref Handle handles, s32 handleCount, u64 timeout);
    public static extern Result svcCancelSynchronization(Handle thread);
    public static extern Result svcArbitrateLock(u32 wait_tag, ref u32 tag_location, u32 self_tag);
    public static extern Result svcArbitrateUnlock(ref u32 tag_location);
    public static extern Result svcWaitProcessWideKeyAtomic(ref u32 key, ref u32 tag_location, u32 self_tag, u64 timeout);
    public static extern void svcSignalProcessWideKey(ref u32 key, s32 num);
    public static extern u64 svcGetSystemTick();
    public static extern Result svcConnectToNamedPort(ref Handle session, ref byte name);
    public static extern Result svcSendSyncRequestLight(Handle session);
    public static extern Result svcSendSyncRequest(Handle session);
    public static extern Result svcSendSyncRequestWithUserBuffer(IntPtr usrBuffer, u64 size, Handle session);
    public static extern Result svcSendAsyncRequestWithUserBuffer(ref Handle handle, IntPtr usrBuffer, u64 size, Handle session);
    public static extern Result svcGetProcessId(ref u64 processID, Handle handle);
    public static extern Result svcGetThreadId(ref u64 threadID, Handle handle);
    public static extern Result svcBreak(u32 breakReason, uintptr_t address, uintptr_t size);
    public static extern Result svcOutputDebugString(ref byte str, u64 size);
    public static extern void NX_NORETURN svcReturnFromException(Result res);
    public static extern Result svcGetInfo(ref u64 @out, u32 id0, Handle handle, u64 id1);
    public static extern void svcFlushEntireDataCache();
    public static extern Result svcFlushDataCache(IntPtr address, ulong size);
    public static extern Result svcMapPhysicalMemory(IntPtr address, u64 size);
    public static extern Result svcUnmapPhysicalMemory(IntPtr address, u64 size);
    public static extern Result svcGetDebugFutureThreadInfo(ref LastThreadContext out_context, ref u64 out_thread_id, Handle debug, s64 ns);
    public static extern Result svcGetLastThreadInfo(ref LastThreadContext out_context, ref u64 out_tls_address, ref u32 out_flags);
    public static extern Result svcGetResourceLimitLimitValue(ref s64 @out, Handle reslimit_h, LimitableResource which);
    public static extern Result svcGetResourceLimitCurrentValue(ref s64 @out, Handle reslimit_h, LimitableResource which);
    public static extern Result svcSetThreadActivity(Handle thread, ThreadActivity paused);
    public static extern Result svcGetThreadContext3(ref ThreadContext ctx, Handle thread);
    public static extern Result svcWaitForAddress(IntPtr address, u32 arb_type, s64 value, s64 timeout);
    public static extern Result svcSignalToAddress(IntPtr address, u32 signal_type, s32 value, s32 count);
    public static extern void svcSynchronizePreemptionState();
    public static extern Result svcGetResourceLimitPeakValue(ref s64 @out, Handle reslimit_h, LimitableResource which);
    public static extern Result svcCreateIoPool(ref Handle out_handle, u32 pool_type);
    public static extern Result svcCreateIoRegion(ref Handle out_handle, Handle io_pool_h, u64 physical_address, u64 size, u32 memory_mapping, u32 perm);
    public static extern void svcDumpInfo(u32 dump_info_type, u64 arg0);
    public static extern void svcKernelDebug(u32 kern_debug_type, u64 arg0, u64 arg1, u64 arg2);
    public static extern void svcChangeKernelTraceState(u32 kern_trace_state);
    public static extern Result svcCreateSession(ref Handle server_handle, ref Handle client_handle, u32 unk0, u64 unk1);
    public static extern Result svcAcceptSession(ref Handle session_handle, Handle port_handle);
    public static extern Result svcReplyAndReceiveLight(Handle handle);
    public static extern Result svcReplyAndReceive(ref s32 index, ref Handle handles, s32 handleCount, Handle replyTarget, u64 timeout);
    public static extern Result svcReplyAndReceiveWithUserBuffer(ref s32 index, IntPtr usrBuffer, u64 size, ref Handle handles, s32 handleCount, Handle replyTarget, u64 timeout);
    public static extern Result svcCreateEvent(ref Handle server_handle, ref Handle client_handle);
    public static extern Result svcMapIoRegion(Handle io_region_h, IntPtr address, u64 size, u32 perm);
    public static extern Result svcUnmapIoRegion(Handle io_region_h, IntPtr address, u64 size);
    public static extern Result svcMapPhysicalMemoryUnsafe(IntPtr address, u64 size);
    public static extern Result svcUnmapPhysicalMemoryUnsafe(IntPtr address, u64 size);
    public static extern Result svcSetUnsafeLimit(u64 size);
    public static extern Result svcCreateCodeMemory(ref Handle code_handle, IntPtr src_addr, u64 size);
    public static extern Result svcControlCodeMemory(Handle code_handle, CodeMapOperation op, IntPtr dst_addr, u64 size, u64 perm);
    public static extern void svcSleepSystem();
    public static extern Result svcReadWriteRegister(ref u32 outVal, u64 regAddr, u32 rwMask, u32 inVal);
    public static extern Result svcSetProcessActivity(Handle process, ProcessActivity paused);
    public static extern Result svcCreateSharedMemory(ref Handle @out, ulong size, u32 local_perm, u32 other_perm);
    public static extern Result svcMapTransferMemory(Handle tmem_handle, IntPtr addr, ulong size, u32 perm);
    public static extern Result svcUnmapTransferMemory(Handle tmem_handle, IntPtr addr, ulong size);
    public static extern Result svcCreateInterruptEvent(ref Handle handle, u64 irqNum, u32 flag);
    public static extern Result svcQueryPhysicalAddress(ref PhysicalMemoryInfo @out, u64 virtaddr);
    public static extern Result svcQueryMemoryMapping(ref u64 virtaddr, ref u64 out_size, u64 physaddr, u64 size);
    public static extern Result svcLegacyQueryIoMapping(ref u64 virtaddr, u64 physaddr, u64 size);
    public static extern Result svcCreateDeviceAddressSpace(ref Handle handle, u64 dev_addr, u64 dev_size);
    public static extern Result svcAttachDeviceAddressSpace(u64 device, Handle handle);
    public static extern Result svcDetachDeviceAddressSpace(u64 device, Handle handle);
    public static extern Result svcMapDeviceAddressSpaceByForce(Handle handle, Handle proc_handle, u64 map_addr, u64 dev_size, u64 dev_addr, u32 option);
    public static extern Result svcMapDeviceAddressSpaceAligned(Handle handle, Handle proc_handle, u64 map_addr, u64 dev_size, u64 dev_addr, u32 option);
    public static extern Result svcMapDeviceAddressSpace(ref u64 out_mapped_size, Handle handle, Handle proc_handle, u64 map_addr, u64 dev_size, u64 dev_addr, u32 perm);
    public static extern Result svcUnmapDeviceAddressSpace(Handle handle, Handle proc_handle, u64 map_addr, u64 map_size, u64 dev_addr);
    public static extern Result svcInvalidateProcessDataCache(Handle process, uintptr_t address, ulong size);
    public static extern Result svcStoreProcessDataCache(Handle process, uintptr_t address, ulong size);
    public static extern Result svcFlushProcessDataCache(Handle process, uintptr_t address, ulong size);
    public static extern Result svcDebugActiveProcess(ref Handle debug, u64 processID);
    public static extern Result svcBreakDebugProcess(Handle debug);
    public static extern Result svcTerminateDebugProcess(Handle debug);
    public static extern Result svcGetDebugEvent(IntPtr event_out, Handle debug);
    public static extern Result svcContinueDebugEvent(Handle debug, u32 flags, ref u64 tid_list, u32 num_tids);
    public static extern Result svcLegacyContinueDebugEvent(Handle debug, u32 flags, u64 threadID);
    public static extern Result svcGetDebugThreadContext(ref ThreadContext ctx, Handle debug, u64 threadID, u32 flags);
    public static extern Result svcSetDebugThreadContext(Handle debug, u64 threadID, ref ThreadContext ctx, u32 flags);
    public static extern Result svcGetProcessList(ref s32 num_out, ref u64 pids_out, u32 max_pids);
    public static extern Result svcGetThreadList(ref s32 num_out, ref u64 tids_out, u32 max_tids, Handle debug);
    public static extern Result svcQueryDebugProcessMemory(ref MemoryInfo meminfo_ptr, ref u32 pageinfo, Handle debug, u64 addr);
    public static extern Result svcReadDebugProcessMemory(IntPtr buffer, Handle debug, u64 addr, u64 size);
    public static extern Result svcWriteDebugProcessMemory(Handle debug, IntPtr buffer, u64 addr, u64 size);
    public static extern Result svcSetHardwareBreakPoint(u32 which, u64 flags, u64 value);
    public static extern Result svcGetDebugThreadParam(ref u64 out_64, ref u32 out_32, Handle debug, u64 threadID, DebugThreadParam param);
    public static extern Result svcGetSystemInfo(ref u64 @out, u64 id0, Handle handle, u64 id1);
    public static extern Result svcCreatePort(ref Handle portServer, ref Handle portClient, s32 max_sessions, bool is_light, ref byte name);
    public static extern Result svcManageNamedPort(ref Handle portServer, ref byte name, s32 maxSessions);
    public static extern Result svcConnectToPort(ref Handle session, Handle port);
    public static extern Result svcSetProcessMemoryPermission(Handle proc, u64 addr, u64 size, u32 perm);
    public static extern Result svcMapProcessMemory(IntPtr dst, Handle proc, u64 src, u64 size);
    public static extern Result svcUnmapProcessMemory(IntPtr dst, Handle proc, u64 src, u64 size);
    public static extern Result svcQueryProcessMemory(ref MemoryInfo meminfo_ptr, ref u32 pageinfo, Handle proc, u64 addr);
    public static extern Result svcMapProcessCodeMemory(Handle proc, u64 dst, u64 src, u64 size);
    public static extern Result svcUnmapProcessCodeMemory(Handle proc, u64 dst, u64 src, u64 size);
    public static extern Result svcCreateProcess(ref Handle @out, IntPtr proc_info, ref u32 caps, u64 cap_num);
    public static extern Result svcStartProcess(Handle proc, s32 main_prio, s32 default_cpu, u32 stack_size);
    public static extern Result svcTerminateProcess(Handle proc);
    public static extern Result svcGetProcessInfo(ref s64 @out, Handle proc, ProcessInfoType which);
    public static extern Result svcCreateResourceLimit(ref Handle @out);
    public static extern Result svcSetResourceLimitLimitValue(Handle reslimit, LimitableResource which, u64 value);
    public static extern void svcCallSecureMonitor(ref SecmonArgs regs);
    public static extern Result svcMapInsecurePhysicalMemory(IntPtr address, u64 size);
    public static extern Result svcUnmapInsecurePhysicalMemory(IntPtr address, u64 size);
}
