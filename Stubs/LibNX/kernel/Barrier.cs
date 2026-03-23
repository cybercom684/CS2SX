// Auto-generated from kernel\barrier.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Kernel;

public unsafe struct Barrier
{
    public u64 count;
    public u64 total;
    public Mutex mutex;
    public CondVar condvar;
}

public static class Barrier
{
    public static extern void barrierInit(ref Barrier b, u64 thread_count);
    public static extern void barrierWait(ref Barrier b);
}
