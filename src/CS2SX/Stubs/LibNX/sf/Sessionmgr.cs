// Auto-generated from sf\sessionmgr.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Sf;

public unsafe struct SessionMgr
{
    public Handle sessions;
    public u32 num_sessions;
    public u32 free_mask;
    public Mutex mutex;
    public CondVar condvar;
    public u32 num_waiters;
}

public static class Sessionmgr
{
    public static extern Result sessionmgrCreate(ref SessionMgr mgr, Handle root_session, u32 num_sessions);
    public static extern void sessionmgrClose(ref SessionMgr mgr);
    public static extern int sessionmgrAttachClient(ref SessionMgr mgr);
    public static extern void sessionmgrDetachClient(ref SessionMgr mgr, int slot);
}
