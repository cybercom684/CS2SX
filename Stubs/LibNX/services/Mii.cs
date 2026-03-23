// Auto-generated from services\mii.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum MiiServiceType
{
    MiiServiceType_System = 0,
    MiiServiceType_User = 1,
}

public enum MiiAge
{
    MiiAge_Young = 0,
    MiiAge_Normal = 1,
    MiiAge_Old = 2,
    MiiAge_All = 3,
}

public enum MiiGender
{
    MiiGender_Male = 0,
    MiiGender_Female = 1,
    MiiGender_All = 2,
}

public enum MiiFaceColor
{
    MiiFaceColor_Black = 0,
    MiiFaceColor_White = 1,
    MiiFaceColor_Asian = 2,
    MiiFaceColor_All = 3,
}

public enum MiiSourceFlag
{
    MiiSourceFlag_Database = (1 << 0),
    MiiSourceFlag_Default = (1 << 1),
    MiiSourceFlag_All = MiiSourceFlag_Database | MiiSourceFlag_Default,
}

public enum MiiSpecialKeyCode
{
    MiiSpecialKeyCode_Normal = 0,
    MiiSpecialKeyCode_Special = 0xA523B78F,
}

public unsafe struct MiiDatabase
{
    public Service s;
}

public unsafe struct MiiCreateId
{
    public Uuid uuid;
}

public unsafe struct MiiCharInfo
{
    public MiiCreateId create_id;
    public u16 mii_name;
    public u8 unk_x26;
    public u8 mii_color;
    public u8 mii_sex;
    public u8 mii_height;
    public u8 mii_width;
    public fixed u8 unk_x2b[2];
    public u8 mii_face_shape;
    public u8 mii_face_color;
    public u8 mii_wrinkles_style;
    public u8 mii_makeup_style;
    public u8 mii_hair_style;
    public u8 mii_hair_color;
    public u8 mii_has_hair_flipped;
    public u8 mii_eye_style;
    public u8 mii_eye_color;
    public u8 mii_eye_size;
    public u8 mii_eye_thickness;
    public u8 mii_eye_angle;
    public u8 mii_eye_pos_x;
    public u8 mii_eye_pos_y;
    public u8 mii_eyebrow_style;
    public u8 mii_eyebrow_color;
    public u8 mii_eyebrow_size;
    public u8 mii_eyebrow_thickness;
    public u8 mii_eyebrow_angle;
    public u8 mii_eyebrow_pos_x;
    public u8 mii_eyebrow_pos_y;
    public u8 mii_nose_style;
    public u8 mii_nose_size;
    public u8 mii_nose_pos;
    public u8 mii_mouth_style;
    public u8 mii_mouth_color;
    public u8 mii_mouth_size;
    public u8 mii_mouth_thickness;
    public u8 mii_mouth_pos;
    public u8 mii_facial_hair_color;
    public u8 mii_beard_style;
    public u8 mii_mustache_style;
    public u8 mii_mustache_size;
    public u8 mii_mustache_pos;
    public u8 mii_glasses_style;
    public u8 mii_glasses_color;
    public u8 mii_glasses_size;
    public u8 mii_glasses_pos;
    public u8 mii_has_mole;
    public u8 mii_mole_size;
    public u8 mii_mole_pos_x;
    public u8 mii_mole_pos_y;
    public u8 unk_x57;
}

public unsafe struct MiiStoreData
{
    public u8 data;
}

public unsafe struct MiiVer3StoreData
{
    public u8 data;
}

public unsafe struct MiiNfpStoreDataExtension
{
    public u8 faceline_color;
    public u8 hair_color;
    public u8 eye_color;
    public u8 eyebrow_color;
    public u8 mouth_color;
    public u8 beard_color;
    public u8 glass_color;
    public u8 glass_type;
}

public static class Mii
{
    public static extern Result miiInitialize(MiiServiceType service_type);
    public static extern void miiExit();
    public static extern IntPtr miiGetServiceSession();
    public static extern Result miiOpenDatabase(ref MiiDatabase @out, MiiSpecialKeyCode key_code);
    public static extern Result miiDatabaseIsUpdated(ref MiiDatabase db, ref bool out_updated, MiiSourceFlag flag);
    public static extern Result miiDatabaseIsFull(ref MiiDatabase db, ref bool out_full);
    public static extern Result miiDatabaseGetCount(ref MiiDatabase db, ref s32 out_count, MiiSourceFlag flag);
    public static extern Result miiDatabaseGet1(ref MiiDatabase db, MiiSourceFlag flag, ref MiiCharInfo out_infos, s32 count, ref s32 total_out);
    public static extern Result miiDatabaseBuildRandom(ref MiiDatabase db, MiiAge age, MiiGender gender, MiiFaceColor face_color, ref MiiCharInfo out_info);
    public static extern void miiDatabaseClose(ref MiiDatabase db);
}
