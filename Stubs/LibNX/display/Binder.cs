// Auto-generated from display\binder.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Display;

public unsafe struct Binder
{
    public bool created;
    public bool initialized;
    public s32 id;
    public ulong dummy;
    public IntPtr relay;
}

public static class Binder
{
    public static extern void binderCreate(ref Binder b, s32 id);
    public static extern void binderClose(ref Binder b);
    public static extern Result binderInitSession(ref Binder b, ref Service relay);
    public static extern Result binderTransactParcel(ref Binder b, u32 code, IntPtr parcel_data, ulong parcel_data_size, IntPtr parcel_reply, ulong parcel_reply_size, u32 flags);
    public static extern Result binderConvertErrorCode(s32 code);
    public static extern Result binderAdjustRefcount(ref Binder b, s32 addval, s32 type);
    public static extern Result binderGetNativeHandle(ref Binder b, u32 unk0, ref Event event_out);
}
