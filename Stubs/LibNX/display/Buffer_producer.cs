// Auto-generated from display\buffer_producer.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Display;

public unsafe struct BqRect
{
    public s32 left;
    public s32 top;
    public s32 right;
    public s32 bottom;
}

public unsafe struct BqBufferOutput
{
    public u32 width;
    public u32 height;
    public u32 transformHint;
    public u32 numPendingBuffers;
}

public unsafe struct BqGraphicBuffer
{
    public u32 width;
    public u32 height;
    public u32 stride;
    public u32 format;
    public u32 usage;
    public IntPtr native_handle;
}

public static class Buffer_producer
{
    public static extern Result bqRequestBuffer(ref Binder b, s32 bufferIdx, ref BqGraphicBuffer buf);
    public static extern Result bqDequeueBuffer(ref Binder b, bool async, u32 width, u32 height, s32 format, u32 usage, ref s32 buf, ref NvMultiFence fence);
    public static extern Result bqDetachBuffer(ref Binder b, s32 slot);
    public static extern Result bqQueueBuffer(ref Binder b, s32 buf, ref BqBufferInput input, ref BqBufferOutput output);
    public static extern Result bqCancelBuffer(ref Binder b, s32 buf, ref NvMultiFence fence);
    public static extern Result bqQuery(ref Binder b, s32 what, ref s32 value);
    public static extern Result bqConnect(ref Binder b, s32 api, bool producerControlledByApp, ref BqBufferOutput output);
    public static extern Result bqDisconnect(ref Binder b, s32 api);
    public static extern Result bqSetPreallocatedBuffer(ref Binder b, s32 buf, ref BqGraphicBuffer input);
}
