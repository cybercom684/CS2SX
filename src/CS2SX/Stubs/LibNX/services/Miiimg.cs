// Auto-generated from services\miiimg.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public unsafe struct MiiimgImageId
{
    public Uuid uuid;
}

public static class Miiimg
{
    public static extern Result miiimgInitialize();
    public static extern void miiimgExit();
    public static extern IntPtr miiimgGetServiceSession();
    public static extern Result miiimgReload();
    public static extern Result miiimgGetCount(ref s32 out_count);
    public static extern Result miiimgIsEmpty(ref bool out_empty);
    public static extern Result miiimgIsFull(ref bool out_full);
    public static extern Result miiimgGetAttribute(s32 index, ref MiiimgImageAttribute out_attr);
    public static extern Result miiimgLoadImage(MiiimgImageId id, IntPtr out_image, ulong out_image_size);
}
