// Auto-generated from services\caps.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum AlbumImageOrientation
{
    AlbumImageOrientation_Unknown0 = 0,
    AlbumImageOrientation_Unknown1 = 1,
    AlbumImageOrientation_Unknown2 = 2,
    AlbumImageOrientation_Unknown3 = 3,
}

public enum AlbumReportOption
{
    AlbumReportOption_Disable = 0,
    AlbumReportOption_Enable = 1,
}

public enum CapsAlbumStorage
{
    CapsAlbumStorage_Nand = 0,
    CapsAlbumStorage_Sd = 1,
}

public enum CapsContentType
{
    CapsContentType_Screenshot = 0,
    CapsContentType_Movie = 1,
    CapsContentType_ExtraMovie = 3,
}

public enum CapsScreenShotDecoderFlag
{
    CapsScreenShotDecoderFlag_None = 0,
    CapsScreenShotDecoderFlag_EnableFancyUpsampling = (1 << 0),
    CapsScreenShotDecoderFlag_EnableBlockSmoothing = (1 << 1),
}

public enum CapsAlbumFileContents
{
    CapsAlbumFileContents_ScreenShot = 0,
    CapsAlbumFileContents_Movie = 1,
    CapsAlbumFileContents_ExtraScreenShot = 2,
    CapsAlbumFileContents_ExtraMovie = 3,
}

public enum CapsAlbumContentsUsageFlag
{
    CapsAlbumContentsUsageFlag_HasGreaterUsage = (1 << 0),
    CapsAlbumContentsUsageFlag_IsUnknownContents = (1 << 1),
}

public enum CapsAlbumFileContentsFlag
{
    CapsAlbumFileContentsFlag_ScreenShot = (1 << 0),
    CapsAlbumFileContentsFlag_Movie = (1 << 1),
}

public unsafe struct CapsScreenShotAttribute
{
    public u32 unk_x0;
    public u32 orientation;
    public u32 unk_x8;
    public u32 unk_xc;
    public u8 unk_x10;
}

public unsafe struct CapsScreenShotAttributeForApplication
{
    public u32 unk_x0;
    public u8 unk_x4;
    public u8 unk_x5;
    public u8 unk_x6;
    public u8 pad;
    public u32 unk_x8;
    public u32 unk_xc;
    public u32 unk_x10;
    public u32 unk_x14;
    public u32 unk_x18;
    public u32 unk_x1c;
    public u16 unk_x20;
    public u16 unk_x22;
    public u16 unk_x24;
    public u16 unk_x26;
    public u8 reserved;
}

public unsafe struct CapsScreenShotDecodeOption
{
    public u64 flags;
    public u64 reserved;
}

public unsafe struct CapsAlbumFileDateTime
{
    public u16 year;
    public u8 month;
    public u8 day;
    public u8 hour;
    public u8 minute;
    public u8 second;
    public u8 id;
}

public unsafe struct CapsAlbumFileId
{
    public u64 application_id;
    public CapsAlbumFileDateTime datetime;
    public u8 storage;
    public u8 content;
    public u8 unknown_12;
    public u8 unknown_13;
    public u8 pad_x14;
}

public unsafe struct CapsAlbumEntry
{
    public u64 size;
    public CapsAlbumFileId file_id;
}

public unsafe struct CapsApplicationAlbumFileEntry
{
    public CapsApplicationAlbumEntry entry;
    public CapsAlbumFileDateTime datetime;
    public u64 unk_x28;
}

public unsafe struct CapsApplicationData
{
    public u8 userdata;
    public u32 size;
}

public unsafe struct CapsAlbumContentsUsage
{
    public s64 count;
    public s64 size;
    public u32 flags;
    public u8 file_contents;
    public u8 pad_x15;
}

public unsafe struct CapsAlbumUsage2
{
    // skipped array: CapsAlbumContentsUsage usages[2]
}

public unsafe struct CapsAlbumUsage3
{
    // skipped array: CapsAlbumContentsUsage usages[3]
}

public unsafe struct CapsAlbumUsage16
{
    // skipped array: CapsAlbumContentsUsage usages[16]
}

public unsafe struct CapsUserIdList
{
    public AccountUid uids;
    public u8 count;
    public fixed u8 pad[7];
}

public unsafe struct CapsLoadAlbumScreenShotImageOutputForApplication
{
    public s64 width;
    public s64 height;
    public CapsScreenShotAttributeForApplication attr;
    public CapsApplicationData appdata;
    public u8 reserved;
}

public unsafe struct CapsLoadAlbumScreenShotImageOutput
{
    public s64 width;
    public s64 height;
    public CapsScreenShotAttribute attr;
    public u8 unk_x50;
}

public unsafe struct CapsAlbumCache
{
    public u64 count;
    public u64 unk_x8;
}

public static class Caps
{
    public static extern u64 capsGetShimLibraryVersion();
}
