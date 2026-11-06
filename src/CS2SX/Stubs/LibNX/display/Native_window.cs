// Auto-generated from display\native_window.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Display;

public unsafe struct NWindow
{
    public u32 magic;
    public Binder bq;
    public Event @event;
    public Mutex mutex;
    public u64 slots_configured;
    public u64 slots_requested;
    public s32 cur_slot;
    public u32 width;
    public u32 height;
    public u32 format;
    public u32 usage;
    public BqRect crop;
    public u32 scaling_mode;
    public u32 transform;
    public u32 sticky_transform;
    public u32 default_width;
    public u32 default_height;
    public u32 swap_interval;
    public bool is_connected;
    public bool producer_controlled_by_app;
    public bool consumer_running_behind;
}

public static class Native_window
{
    public static extern bool nwindowIsValid(ref NWindow nw);
    public static extern IntPtr nwindowGetDefault();
    public static extern Result nwindowCreate(ref NWindow nw, ref Service binder_session, s32 binder_id, bool producer_controlled_by_app);
    public static extern Result nwindowCreateFromLayer(ref NWindow nw, ref ViLayer layer);
    public static extern void nwindowClose(ref NWindow nw);
    public static extern Result nwindowGetDimensions(ref NWindow nw, ref u32 out_width, ref u32 out_height);
    public static extern Result nwindowSetDimensions(ref NWindow nw, u32 width, u32 height);
    public static extern Result nwindowSetCrop(ref NWindow nw, s32 left, s32 top, s32 right, s32 bottom);
    public static extern Result nwindowSetTransform(ref NWindow nw, u32 transform);
    public static extern Result nwindowSetSwapInterval(ref NWindow nw, u32 swap_interval);
    public static extern Result nwindowConfigureBuffer(ref NWindow nw, s32 slot, ref NvGraphicBuffer buf);
    public static extern Result nwindowDequeueBuffer(ref NWindow nw, ref s32 out_slot, ref NvMultiFence out_fence);
    public static extern Result nwindowCancelBuffer(ref NWindow nw, s32 slot, ref NvMultiFence fence);
    public static extern Result nwindowQueueBuffer(ref NWindow nw, s32 slot, ref NvMultiFence fence);
    public static extern Result nwindowReleaseBuffers(ref NWindow nw);
}
