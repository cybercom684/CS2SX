// Auto-generated from services\async.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public unsafe struct AsyncValue
{
    public Service s;
    public Event @event;
}

public unsafe struct AsyncResult
{
    public Service s;
    public Event @event;
}

public static class Async
{
    public static extern void asyncValueClose(ref AsyncValue a);
    public static extern Result asyncValueWait(ref AsyncValue a, u64 timeout);
    public static extern Result asyncValueGetSize(ref AsyncValue a, ref u64 size);
    public static extern Result asyncValueGet(ref AsyncValue a, IntPtr buffer, ulong size);
    public static extern Result asyncValueCancel(ref AsyncValue a);
    public static extern Result asyncValueGetErrorContext(ref AsyncValue a, ref ErrorContext context);
    public static extern void asyncResultClose(ref AsyncResult a);
    public static extern Result asyncResultWait(ref AsyncResult a, u64 timeout);
    public static extern Result asyncResultGet(ref AsyncResult a);
    public static extern Result asyncResultCancel(ref AsyncResult a);
    public static extern Result asyncResultGetErrorContext(ref AsyncResult a, ref ErrorContext context);
}
