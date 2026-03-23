// Auto-generated from applets\mii_la.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Applets;

public enum MiiLaAppletMode
{
    MiiLaAppletMode_ShowMiiEdit = 0,
    MiiLaAppletMode_AppendMii = 1,
    MiiLaAppletMode_AppendMiiImage = 2,
    MiiLaAppletMode_UpdateMiiImage = 3,
    MiiLaAppletMode_CreateMii = 4,
    MiiLaAppletMode_EditMii = 5,
}

public unsafe struct MiiLaAppletOutput
{
    public u32 res;
    public s32 index;
    public u8 unk_x8;
}

public unsafe struct MiiLaAppletOutputForCharInfoEditing
{
    public u32 res;
    public MiiCharInfo char_info;
    public u8 unused;
}

public static class Mii_la
{
    public static extern Result miiLaShowMiiEdit(MiiSpecialKeyCode special_key_code);
    public static extern Result miiLaAppendMii(MiiSpecialKeyCode special_key_code, ref s32 index);
    public static extern Result miiLaAppendMiiImage(MiiSpecialKeyCode special_key_code, ref Uuid valid_uuid_array, s32 count, ref s32 index);
    public static extern Result miiLaUpdateMiiImage(MiiSpecialKeyCode special_key_code, ref Uuid valid_uuid_array, s32 count, Uuid used_uuid, ref s32 index);
    public static extern Result miiLaCreateMii(MiiSpecialKeyCode special_key_code, ref MiiCharInfo out_char);
    public static extern Result miiLaEditMii(MiiSpecialKeyCode special_key_code, ref MiiCharInfo in_char, ref MiiCharInfo out_char);
}
