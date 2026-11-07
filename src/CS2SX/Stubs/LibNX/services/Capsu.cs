// Auto-generated from services\capsu.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Capsu
{
    public static extern Result capsuInitialize();
    public static extern void capsuExit();
    public static extern IntPtr capsuGetServiceSession();
    public static extern IntPtr capsuGetServiceSession_Accessor();
    public static extern Result capsuGetAlbumFileListDeprecated1(ref CapsApplicationAlbumFileEntry entries, s32 count, CapsContentType type, ref CapsAlbumFileDateTime start_datetime, ref CapsAlbumFileDateTime end_datetime, ref s32 total_entries);
    public static extern Result capsuGetAlbumFileListDeprecated2(ref CapsApplicationAlbumFileEntry entries, s32 count, CapsContentType type, ref CapsAlbumFileDateTime start_datetime, ref CapsAlbumFileDateTime end_datetime, AccountUid uid, ref s32 total_entries);
    public static extern Result capsuGetAlbumFileList3(ref CapsApplicationAlbumEntry entries, s32 count, CapsContentType type, ref CapsAlbumFileDateTime start_datetime, ref CapsAlbumFileDateTime end_datetime, ref s32 total_entries);
    public static extern Result capsuGetAlbumFileList4(ref CapsApplicationAlbumEntry entries, s32 count, CapsContentType type, ref CapsAlbumFileDateTime start_datetime, ref CapsAlbumFileDateTime end_datetime, AccountUid uid, ref s32 total_entries);
    public static extern Result capsuDeleteAlbumFile(CapsContentType type, ref CapsApplicationAlbumFileEntry entry);
    public static extern Result capsuGetAlbumFileSize(ref CapsApplicationAlbumFileEntry entry, ref u64 size);
    public static extern Result capsuLoadAlbumScreenShotImage(ref s32 width, ref s32 height, ref CapsScreenShotAttributeForApplication attr, IntPtr userdata, ulong userdata_maxsize, ref u32 userdata_size, IntPtr image, ulong image_size, IntPtr workbuf, ulong workbuf_size, ref CapsApplicationAlbumFileEntry entry, ref CapsScreenShotDecodeOption option);
    public static extern Result capsuLoadAlbumScreenShotThumbnailImage(ref s32 width, ref s32 height, ref CapsScreenShotAttributeForApplication attr, IntPtr userdata, ulong userdata_maxsize, ref u32 userdata_size, IntPtr image, ulong image_size, IntPtr workbuf, ulong workbuf_size, ref CapsApplicationAlbumFileEntry entry, ref CapsScreenShotDecodeOption option);
    public static extern Result capsuPrecheckToCreateContents(CapsContentType type, u64 unk);
    public static extern Result capsuOpenAlbumMovieStream(ref u64 stream, ref CapsApplicationAlbumFileEntry entry);
    public static extern Result capsuCloseAlbumMovieStream(u64 stream);
    public static extern Result capsuGetAlbumMovieStreamSize(u64 stream, ref u64 size);
    public static extern Result capsuReadAlbumMovieStream(u64 stream, s64 offset, IntPtr buffer, ulong size, ref u64 actual_size);
    public static extern Result capsuGetAlbumMovieStreamBrokenReason(u64 stream);
}
