// Auto-generated from services\pl.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum PlServiceType
{
    PlServiceType_User = 0,
    PlServiceType_System = 1,
}

public enum PlSharedFontType
{
    PlSharedFontType_Standard = 0,
    PlSharedFontType_ChineseSimplified = 1,
    PlSharedFontType_ExtChineseSimplified = 2,
    PlSharedFontType_ChineseTraditional = 3,
    PlSharedFontType_KO = 4,
    PlSharedFontType_NintendoExt = 5,
    PlSharedFontType_Total,
}

public unsafe struct PlFontData
{
    public u32 type;
    public u32 offset;
    public u32 size;
    public IntPtr address;
}

public static class Pl
{
    public static extern Result plInitialize(PlServiceType service_type);
    public static extern void plExit();
    public static extern IntPtr plGetServiceSession();
    public static extern IntPtr plGetSharedmemAddr();
    public static extern Result plGetSharedFontByType(ref PlFontData font, PlSharedFontType SharedFontType);
    public static extern Result plGetSharedFont(u64 LanguageCode, ref PlFontData fonts, s32 max_fonts, ref s32 total_fonts);
}
