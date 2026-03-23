// Auto-generated from kernel\thread.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Kernel;

public unsafe struct Thread
{
    public Handle handle;
    public bool owns_stack_mem;
    public IntPtr stack_mem;
    public IntPtr stack_mirror;
    public ulong stack_sz;
    public IntPtr tls_array;
}

public static class Thread
{
    public static extern Result threadCreate(ref Thread t, ThreadFunc entry, IntPtr arg, IntPtr stack_mem, ulong stack_sz, int prio, int cpuid);
    public static extern Result threadStart(ref Thread t);
    public static extern void NX_NORETURN threadExit();
    public static extern Result threadWaitForExit(ref Thread t);
    public static extern Result threadClose(ref Thread t);
    public static extern Result threadPause(ref Thread t);
    public static extern Result threadResume(ref Thread t);
    public static extern Result threadDumpContext(ref ThreadContext ctx, ref Thread t);
    public static extern Handle threadGetCurHandle();
    public static extern IntPtr threadTlsGet(s32 slot_id);
    public static extern void threadTlsSet(s32 slot_id, IntPtr value);
    public static extern void threadTlsFree(s32 slot_id);
}
