// Auto-generated from kernel\rwlock.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Kernel;

public unsafe struct RwLock
{
    public Mutex mutex;
    public CondVar condvar_reader_wait;
    public CondVar condvar_writer_wait;
    public u32 read_lock_count;
    public u32 read_waiter_count;
    public u32 write_lock_count;
    public u32 write_waiter_count;
    public u32 write_owner_tag;
}

public static class Rwlock
{
    public static extern void rwlockInit(ref RwLock r);
    public static extern void rwlockReadLock(ref RwLock r);
    public static extern bool rwlockTryReadLock(ref RwLock r);
    public static extern void rwlockReadUnlock(ref RwLock r);
    public static extern void rwlockWriteLock(ref RwLock r);
    public static extern bool rwlockTryWriteLock(ref RwLock r);
    public static extern void rwlockWriteUnlock(ref RwLock r);
    public static extern bool rwlockIsWriteLockHeldByCurrentThread(ref RwLock r);
    public static extern bool rwlockIsOwnedByCurrentThread(ref RwLock r);
}
