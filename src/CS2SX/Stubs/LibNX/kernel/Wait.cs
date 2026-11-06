// Auto-generated from kernel\wait.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Kernel;

public enum WaiterType
{
    WaiterType_Handle,
    WaiterType_HandleWithClear,
    WaiterType_Waitable,
}

public static class Wait
{
    public static extern Result waitObjects(ref s32 idx_out, ref Waiter objects, s32 num_objects, u64 timeout);
    public static extern Result waitHandles(ref s32 idx_out, ref Handle handles, s32 num_handles, u64 timeout);
}
