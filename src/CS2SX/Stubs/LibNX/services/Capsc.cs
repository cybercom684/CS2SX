// Auto-generated from services\capsc.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public unsafe struct CapsApplicationId
{
    public u64 application_id;
    public u8 unknown_08;
    public u8 unknown_09;
    public fixed u8 reserved[6];
}

public static class Capsc
{
    public static extern Result capscInitialize();
    public static extern void capscExit();
    public static extern IntPtr capscGetServiceSession();
    public static extern Result capscNotifyAlbumStorageIsAvailable(CapsAlbumStorage storage);
    public static extern Result capscNotifyAlbumStorageIsUnAvailable(CapsAlbumStorage storage);
    public static extern Result capscRegisterAppletResourceUserId(u64 appletResourceUserId, ref CapsApplicationId application_id);
    public static extern Result capscUnregisterAppletResourceUserId(u64 appletResourceUserId, ref CapsApplicationId application_id);
    public static extern Result capscGetApplicationIdFromAruid(ref CapsApplicationId application_id, u64 aruid);
    public static extern Result capscCheckApplicationIdRegistered(u64 application_id);
    public static extern Result capscGenerateCurrentAlbumFileId(ref CapsApplicationId application_id, CapsAlbumFileContents contents, ref CapsAlbumFileId file_id);
    public static extern Result capscGenerateApplicationAlbumEntry(ref CapsApplicationAlbumEntry appEntry, ref CapsAlbumEntry entry, u64 application_id);
    public static extern Result capscSaveAlbumScreenShotFile(ref CapsAlbumFileId file_id, IntPtr buffer, u64 buffer_size);
    public static extern Result capscSaveAlbumScreenShotFileEx(ref CapsAlbumFileId file_id, u64 version, u64 makernote_offset, u64 makernote_size, IntPtr buffer, u64 buffer_size);
    public static extern Result capscSetOverlayScreenShotThumbnailData(ref CapsAlbumFileId file_id, IntPtr image, u64 image_size);
    public static extern Result capscSetOverlayMovieThumbnailData(ref CapsAlbumFileId file_id, IntPtr image, u64 image_size);
    public static extern Result capscOpenAlbumMovieReadStream(ref u64 stream, ref CapsAlbumFileId file_id);
    public static extern Result capscCloseAlbumMovieStream(u64 stream);
    public static extern Result capscGetAlbumMovieStreamSize(u64 stream, ref u64 size);
    public static extern Result capscReadMovieDataFromAlbumMovieReadStream(u64 stream, u64 offset, IntPtr buffer, ulong size, ref u64 actual_size);
    public static extern Result capscGetAlbumMovieReadStreamBrokenReason(u64 stream);
    public static extern Result capscGetAlbumMovieReadStreamImageDataSize(u64 stream, ref u64 size);
    public static extern Result capscReadImageDataFromAlbumMovieReadStream(u64 stream, u64 offset, IntPtr buffer, ulong size, ref u64 actual_size);
    public static extern Result capscReadFileAttributeFromAlbumMovieReadStream(u64 stream, ref CapsScreenShotAttribute attribute);
    public static extern Result capscOpenAlbumMovieWriteStream(ref u64 stream, ref CapsAlbumFileId file_id);
    public static extern Result capscFinishAlbumMovieWriteStream(u64 stream);
    public static extern Result capscCommitAlbumMovieWriteStream(u64 stream);
    public static extern Result capscDiscardAlbumMovieWriteStream(u64 stream);
    public static extern Result capscDiscardAlbumMovieWriteStreamNoDelete(u64 stream);
    public static extern Result capscCommitAlbumMovieWriteStreamEx(u64 stream, ref CapsAlbumEntry entry);
    public static extern Result capscStartAlbumMovieWriteStreamDataSection(u64 stream);
    public static extern Result capscEndAlbumMovieWriteStreamDataSection(u64 stream);
    public static extern Result capscStartAlbumMovieWriteStreamMetaSection(u64 stream);
    public static extern Result capscEndAlbumMovieWriteStreamMetaSection(u64 stream);
    public static extern Result capscReadDataFromAlbumMovieWriteStream(u64 stream, u64 offset, IntPtr buffer, u64 size, ref u64 actual_size);
    public static extern Result capscWriteDataToAlbumMovieWriteStream(u64 stream, u64 offset, IntPtr buffer, u64 size);
    public static extern Result capscWriteMetaToAlbumMovieWriteStream(u64 stream, u64 offset, IntPtr buffer, u64 size);
    public static extern Result capscGetAlbumMovieWriteStreamBrokenReason(u64 stream);
    public static extern Result capscGetAlbumMovieWriteStreamDataSize(u64 stream, ref u64 size);
    public static extern Result capscSetAlbumMovieWriteStreamDataSize(u64 stream, u64 size);
}
