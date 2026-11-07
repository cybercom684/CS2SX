// Auto-generated from services\capsa.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Capsa
{
    public static extern Result capsaInitialize();
    public static extern void capsaExit();
    public static extern IntPtr capsaGetServiceSession();
    public static extern IntPtr capsaGetServiceSession_Accessor();
    public static extern Result capsaGetAlbumFileCount(CapsAlbumStorage storage, ref u64 count);
    public static extern Result capsaGetAlbumFileList(CapsAlbumStorage storage, ref u64 @out, ref CapsAlbumEntry entries, u64 count);
    public static extern Result capsaLoadAlbumFile(ref CapsAlbumFileId file_id, ref u64 out_size, IntPtr filebuf, u64 filebuf_size);
    public static extern Result capsaDeleteAlbumFile(ref CapsAlbumFileId file_id);
    public static extern Result capsaStorageCopyAlbumFile(ref CapsAlbumFileId file_id, CapsAlbumStorage dst_storage);
    public static extern Result capsaIsAlbumMounted(CapsAlbumStorage storage, ref bool is_mounted);
    public static extern Result capsaGetAlbumUsage(CapsAlbumStorage storage, ref CapsAlbumUsage2 @out);
    public static extern Result capsaGetAlbumFileSize(ref CapsAlbumFileId file_id, ref u64 size);
    public static extern Result capsaLoadAlbumFileThumbnail(ref CapsAlbumFileId file_id, ref u64 out_size, IntPtr image, u64 image_size);
    public static extern Result capsaLoadAlbumScreenShotImage(ref u64 width, ref u64 height, ref CapsAlbumFileId file_id, IntPtr image, u64 image_size, IntPtr workbuf, u64 workbuf_size);
    public static extern Result capsaLoadAlbumScreenShotThumbnailImage(ref u64 width, ref u64 height, ref CapsAlbumFileId file_id, IntPtr image, u64 image_size, IntPtr workbuf, u64 workbuf_size);
    public static extern Result capsaGetAlbumEntryFromApplicationAlbumEntry(ref CapsAlbumEntry entry, ref CapsApplicationAlbumEntry application_entry, u64 application_id);
    public static extern Result capsaLoadAlbumScreenShotImageEx(ref u64 width, ref u64 height, ref CapsAlbumFileId file_id, ref CapsScreenShotDecodeOption opts, IntPtr image, u64 image_size, IntPtr workbuf, u64 workbuf_size);
    public static extern Result capsaLoadAlbumScreenShotThumbnailImageEx(ref u64 width, ref u64 height, ref CapsAlbumFileId file_id, ref CapsScreenShotDecodeOption opts, IntPtr image, u64 image_size, IntPtr workbuf, u64 workbuf_size);
    public static extern Result capsaLoadAlbumScreenShotImageEx0(ref u64 width, ref u64 height, ref CapsScreenShotAttribute attr, ref CapsAlbumFileId file_id, ref CapsScreenShotDecodeOption opts, IntPtr image, u64 image_size, IntPtr workbuf, u64 workbuf_size);
    public static extern Result capsaGetAlbumUsage3(CapsAlbumStorage storage, ref CapsAlbumUsage3 @out);
    public static extern Result capsaGetAlbumMountResult(CapsAlbumStorage storage);
    public static extern Result capsaGetAlbumUsage16(CapsAlbumStorage storage, u8 flags, ref CapsAlbumUsage16 @out);
    public static extern Result capsaGetMinMaxAppletId(ref bool success, ref u64 min, ref u64 max);
    public static extern Result capsaGetAlbumFileCountEx0(CapsAlbumStorage storage, u8 flags, ref u64 count);
    public static extern Result capsaGetAlbumFileListEx0(CapsAlbumStorage storage, u8 flags, ref u64 @out, ref CapsAlbumEntry entries, u64 count);
    public static extern Result capsaGetLastOverlayScreenShotThumbnail(ref CapsAlbumFileId file_id, ref u64 out_size, IntPtr image, u64 image_size);
    public static extern Result capsaGetLastOverlayMovieThumbnail(ref CapsAlbumFileId file_id, ref u64 out_size, IntPtr image, u64 image_size);
    public static extern Result capsaGetAutoSavingStorage(ref CapsAlbumStorage storage);
    public static extern Result capsaGetRequiredStorageSpaceSizeToCopyAll(CapsAlbumStorage dst_storage, CapsAlbumStorage src_storage, ref u64 @out);
    public static extern Result capsLoadAlbumScreenShotThumbnailImageEx0(ref u64 width, ref u64 height, ref CapsScreenShotAttribute attr, ref CapsAlbumFileId file_id, ref CapsScreenShotDecodeOption opts, IntPtr image, u64 image_size, IntPtr workbuf, u64 workbuf_size);
    public static extern Result capsaLoadAlbumScreenShotImageEx1(ref CapsAlbumFileId file_id, ref CapsScreenShotDecodeOption opts, ref CapsLoadAlbumScreenShotImageOutput @out, IntPtr image, u64 image_size, IntPtr workbuf, u64 workbuf_size);
    public static extern Result capsaLoadAlbumScreenShotThumbnailImageEx1(ref CapsAlbumFileId file_id, ref CapsScreenShotDecodeOption opts, ref CapsLoadAlbumScreenShotImageOutput @out, IntPtr image, u64 image_size, IntPtr workbuf, u64 workbuf_size);
    public static extern Result capsaForceAlbumUnmounted(CapsAlbumStorage storage);
    public static extern Result capsaResetAlbumMountStatus(CapsAlbumStorage storage);
    public static extern Result capsaRefreshAlbumCache(CapsAlbumStorage storage);
    public static extern Result capsaGetAlbumCache(CapsAlbumStorage storage, ref CapsAlbumCache cache);
    public static extern Result capsaGetAlbumCacheEx(CapsAlbumStorage storage, CapsAlbumFileContents contents, ref CapsAlbumCache cache);
    public static extern Result capsaGetAlbumEntryFromApplicationAlbumEntryAruid(ref CapsAlbumEntry entry, ref CapsApplicationAlbumEntry application_entry);
    public static extern Result capsaOpenAlbumMovieStream(ref u64 stream, ref CapsAlbumFileId file_id);
    public static extern Result capsaCloseAlbumMovieStream(u64 stream);
    public static extern Result capsaGetAlbumMovieStreamSize(u64 stream, ref u64 size);
    public static extern Result capsaReadMovieDataFromAlbumMovieReadStream(u64 stream, s64 offset, IntPtr buffer, ulong size, ref u64 actual_size);
    public static extern Result capsaGetAlbumMovieReadStreamBrokenReason(u64 stream);
    public static extern Result capsaGetAlbumMovieReadStreamImageDataSize(u64 stream, ref u64 size);
    public static extern Result capsaReadImageDataFromAlbumMovieReadStream(u64 stream, s64 offset, IntPtr buffer, ulong size, ref u64 actual_size);
    public static extern Result capsaReadFileAttributeFromAlbumMovieReadStream(u64 stream, ref CapsScreenShotAttribute attr);
}
