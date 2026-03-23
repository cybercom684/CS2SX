// Auto-generated from display\parcel.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Display;

public unsafe struct ParcelHeader
{
    public u32 payload_size;
    public u32 payload_off;
    public u32 objects_size;
    public u32 objects_off;
}

public unsafe struct Parcel
{
    public u8 payload;
    public u32 payload_size;
    public IntPtr objects;
    public u32 objects_size;
    public u32 capacity;
    public u32 pos;
}

public static class Parcel
{
    public static extern void parcelCreate(ref Parcel ctx);
    public static extern Result parcelTransact(ref Binder session, u32 code, ref Parcel in_parcel, ref Parcel reply_parcel);
    public static extern IntPtr parcelWriteData(ref Parcel ctx, IntPtr data, ulong data_size);
    public static extern IntPtr parcelReadData(ref Parcel ctx, IntPtr data, ulong data_size);
    public static extern void parcelWriteInt32(ref Parcel ctx, s32 val);
    public static extern void parcelWriteUInt32(ref Parcel ctx, u32 val);
    public static extern void parcelWriteString16(ref Parcel ctx, ref byte str);
    public static extern s32 parcelReadInt32(ref Parcel ctx);
    public static extern u32 parcelReadUInt32(ref Parcel ctx);
    public static extern void parcelWriteInterfaceToken(ref Parcel ctx, ref byte str);
    public static extern IntPtr parcelReadFlattenedObject(ref Parcel ctx, ref ulong size);
    public static extern IntPtr parcelWriteFlattenedObject(ref Parcel ctx, IntPtr data, ulong size);
}
