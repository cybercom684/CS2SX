// Auto-generated from kernel\mutex.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Kernel;

public static class Mutex
{
    public static extern void mutexLock(ref Mutex m);
    public static extern bool mutexTryLock(ref Mutex m);
    public static extern void mutexUnlock(ref Mutex m);
    public static extern bool mutexIsLockedByCurrentThread(ref Mutex m);
    public static extern void rmutexLock(ref RMutex m);
    public static extern bool rmutexTryLock(ref RMutex m);
    public static extern void rmutexUnlock(ref RMutex m);
}
