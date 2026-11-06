// Auto-generated from services\nim.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public unsafe struct NimSystemUpdateTaskId
{
    public alignas(8) Uuid uuid;
}

public static class Nim
{
    public static extern Result nimInitialize();
    public static extern void nimExit();
    public static extern IntPtr nimGetServiceSession();
    public static extern Result nimListSystemUpdateTask(ref s32 out_count, ref NimSystemUpdateTaskId out_task_ids, ulong max_task_ids);
    public static extern Result nimDestroySystemUpdateTask(ref NimSystemUpdateTaskId task_id);
}
