// Auto-generated from kernel\semaphore.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Kernel;

public unsafe struct Semaphore
{
    public CondVar condvar;
    public Mutex mutex;
    public u64 count;
}

public static class Semaphore
{
    public static extern void semaphoreInit(ref Semaphore s, u64 initial_count);
    public static extern void semaphoreSignal(ref Semaphore s);
    public static extern void semaphoreWait(ref Semaphore s);
    public static extern bool semaphoreTryWait(ref Semaphore s);
}
