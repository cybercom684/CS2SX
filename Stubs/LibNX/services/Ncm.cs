// Auto-generated from services\ncm.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public unsafe struct NcmContentStorage
{
    public Service s;
}

public unsafe struct NcmContentMetaDatabase
{
    public Service s;
}

public unsafe struct NcmRightsId
{
    public FsRightsId rights_id;
    public u8 key_generation;
    public fixed u8 pad[7];
}

public static class Ncm
{
    public static extern Result ncmInitialize();
    public static extern void ncmExit();
    public static extern IntPtr ncmGetServiceSession();
    public static extern Result ncmCreateContentStorage(NcmStorageId storage_id);
    public static extern Result ncmCreateContentMetaDatabase(NcmStorageId storage_id);
    public static extern Result ncmVerifyContentStorage(NcmStorageId storage_id);
    public static extern Result ncmVerifyContentMetaDatabase(NcmStorageId storage_id);
    public static extern Result ncmOpenContentStorage(ref NcmContentStorage out_content_storage, NcmStorageId storage_id);
    public static extern Result ncmOpenContentMetaDatabase(ref NcmContentMetaDatabase out_content_meta_database, NcmStorageId storage_id);
    public static extern Result ncmCloseContentStorageForcibly(NcmStorageId storage_id);
    public static extern Result ncmCloseContentMetaDatabaseForcibly(NcmStorageId storage_id);
    public static extern Result ncmCleanupContentMetaDatabase(NcmStorageId storage_id);
    public static extern Result ncmActivateContentStorage(NcmStorageId storage_id);
    public static extern Result ncmInactivateContentStorage(NcmStorageId storage_id);
    public static extern Result ncmActivateContentMetaDatabase(NcmStorageId storage_id);
    public static extern Result ncmInactivateContentMetaDatabase(NcmStorageId storage_id);
    public static extern Result ncmInvalidateRightsIdCache();
    public static extern Result ncmActivateFsContentStorage(FsContentStorageId fs_storage_id);
    public static extern void ncmContentStorageClose(ref NcmContentStorage cs);
    public static extern Result ncmContentStorageGeneratePlaceHolderId(ref NcmContentStorage cs, ref NcmPlaceHolderId out_id);
    public static extern Result ncmContentStorageCreatePlaceHolder(ref NcmContentStorage cs, ref NcmContentId content_id, ref NcmPlaceHolderId placeholder_id, s64 size);
    public static extern Result ncmContentStorageDeletePlaceHolder(ref NcmContentStorage cs, ref NcmPlaceHolderId placeholder_id);
    public static extern Result ncmContentStorageHasPlaceHolder(ref NcmContentStorage cs, ref bool @out, ref NcmPlaceHolderId placeholder_id);
    public static extern Result ncmContentStorageWritePlaceHolder(ref NcmContentStorage cs, ref NcmPlaceHolderId placeholder_id, u64 offset, IntPtr data, ulong data_size);
    public static extern Result ncmContentStorageRegister(ref NcmContentStorage cs, ref NcmContentId content_id, ref NcmPlaceHolderId placeholder_id);
    public static extern Result ncmContentStorageDelete(ref NcmContentStorage cs, ref NcmContentId content_id);
    public static extern Result ncmContentStorageHas(ref NcmContentStorage cs, ref bool @out, ref NcmContentId content_id);
    public static extern Result ncmContentStorageGetPath(ref NcmContentStorage cs, ref byte out_path, ulong out_size, ref NcmContentId content_id);
    public static extern Result ncmContentStorageGetPlaceHolderPath(ref NcmContentStorage cs, ref byte out_path, ulong out_size, ref NcmPlaceHolderId placeholder_id);
    public static extern Result ncmContentStorageCleanupAllPlaceHolder(ref NcmContentStorage cs);
    public static extern Result ncmContentStorageListPlaceHolder(ref NcmContentStorage cs, ref NcmPlaceHolderId out_ids, s32 count, ref s32 out_count);
    public static extern Result ncmContentStorageGetContentCount(ref NcmContentStorage cs, ref s32 out_count);
    public static extern Result ncmContentStorageListContentId(ref NcmContentStorage cs, ref NcmContentId out_ids, s32 count, ref s32 out_count, s32 start_offset);
    public static extern Result ncmContentStorageGetSizeFromContentId(ref NcmContentStorage cs, ref s64 out_size, ref NcmContentId content_id);
    public static extern Result ncmContentStorageDisableForcibly(ref NcmContentStorage cs);
    public static extern Result ncmContentStorageRevertToPlaceHolder(ref NcmContentStorage cs, ref NcmPlaceHolderId placeholder_id, ref NcmContentId old_content_id, ref NcmContentId new_content_id);
    public static extern Result ncmContentStorageSetPlaceHolderSize(ref NcmContentStorage cs, ref NcmPlaceHolderId placeholder_id, s64 size);
    public static extern Result ncmContentStorageReadContentIdFile(ref NcmContentStorage cs, IntPtr out_data, ulong out_data_size, ref NcmContentId content_id, s64 offset);
    public static extern Result ncmContentStorageGetRightsIdFromPlaceHolderId(ref NcmContentStorage cs, ref NcmRightsId out_rights_id, ref NcmPlaceHolderId placeholder_id, FsContentAttributes attr);
    public static extern Result ncmContentStorageGetRightsIdFromContentId(ref NcmContentStorage cs, ref NcmRightsId out_rights_id, ref NcmContentId content_id, FsContentAttributes attr);
    public static extern Result ncmContentStorageWriteContentForDebug(ref NcmContentStorage cs, ref NcmContentId content_id, s64 offset, IntPtr data, ulong data_size);
    public static extern Result ncmContentStorageGetFreeSpaceSize(ref NcmContentStorage cs, ref s64 out_size);
    public static extern Result ncmContentStorageGetTotalSpaceSize(ref NcmContentStorage cs, ref s64 out_size);
    public static extern Result ncmContentStorageFlushPlaceHolder(ref NcmContentStorage cs);
    public static extern Result ncmContentStorageGetSizeFromPlaceHolderId(ref NcmContentStorage cs, ref s64 out_size, ref NcmPlaceHolderId placeholder_id);
    public static extern Result ncmContentStorageRepairInvalidFileAttribute(ref NcmContentStorage cs);
    public static extern Result ncmContentStorageGetRightsIdFromPlaceHolderIdWithCache(ref NcmContentStorage cs, ref NcmRightsId out_rights_id, ref NcmPlaceHolderId placeholder_id, ref NcmContentId cache_content_id, FsContentAttributes attr);
    public static extern Result ncmContentStorageRegisterPath(ref NcmContentStorage cs, ref NcmContentId content_id, ref byte path);
    public static extern Result ncmContentStorageClearRegisteredPath(ref NcmContentStorage cs);
    public static extern Result ncmContentStorageGetProgramId(ref NcmContentStorage cs, ref u64 @out, ref NcmContentId content_id, FsContentAttributes attr);
    public static extern void ncmContentMetaDatabaseClose(ref NcmContentMetaDatabase db);
    public static extern Result ncmContentMetaDatabaseSet(ref NcmContentMetaDatabase db, ref NcmContentMetaKey key, IntPtr data, u64 data_size);
    public static extern Result ncmContentMetaDatabaseGet(ref NcmContentMetaDatabase db, ref NcmContentMetaKey key, ref u64 out_size, IntPtr out_data, u64 out_data_size);
    public static extern Result ncmContentMetaDatabaseRemove(ref NcmContentMetaDatabase db, ref NcmContentMetaKey key);
    public static extern Result ncmContentMetaDatabaseGetContentIdByType(ref NcmContentMetaDatabase db, ref NcmContentId out_content_id, ref NcmContentMetaKey key, NcmContentType type);
    public static extern Result ncmContentMetaDatabaseListContentInfo(ref NcmContentMetaDatabase db, ref s32 out_entries_written, ref NcmContentInfo out_info, s32 count, ref NcmContentMetaKey key, s32 start_index);
    public static extern Result ncmContentMetaDatabaseList(ref NcmContentMetaDatabase db, ref s32 out_entries_total, ref s32 out_entries_written, ref NcmContentMetaKey out_keys, s32 count, NcmContentMetaType meta_type, u64 id, u64 id_min, u64 id_max, NcmContentInstallType install_type);
    public static extern Result ncmContentMetaDatabaseGetLatestContentMetaKey(ref NcmContentMetaDatabase db, ref NcmContentMetaKey out_key, u64 id);
    public static extern Result ncmContentMetaDatabaseListApplication(ref NcmContentMetaDatabase db, ref s32 out_entries_total, ref s32 out_entries_written, ref NcmApplicationContentMetaKey out_keys, s32 count, NcmContentMetaType meta_type);
    public static extern Result ncmContentMetaDatabaseHas(ref NcmContentMetaDatabase db, ref bool @out, ref NcmContentMetaKey key);
    public static extern Result ncmContentMetaDatabaseHasAll(ref NcmContentMetaDatabase db, ref bool @out, ref NcmContentMetaKey keys, s32 count);
    public static extern Result ncmContentMetaDatabaseGetSize(ref NcmContentMetaDatabase db, ref u64 out_size, ref NcmContentMetaKey key);
    public static extern Result ncmContentMetaDatabaseGetRequiredSystemVersion(ref NcmContentMetaDatabase db, ref u32 out_version, ref NcmContentMetaKey key);
    public static extern Result ncmContentMetaDatabaseGetPatchContentMetaId(ref NcmContentMetaDatabase db, ref u64 out_patch_id, ref NcmContentMetaKey key);
    public static extern Result ncmContentMetaDatabaseDisableForcibly(ref NcmContentMetaDatabase db);
    public static extern Result ncmContentMetaDatabaseLookupOrphanContent(ref NcmContentMetaDatabase db, ref bool out_orphaned, ref NcmContentId content_ids, s32 count);
    public static extern Result ncmContentMetaDatabaseCommit(ref NcmContentMetaDatabase db);
    public static extern Result ncmContentMetaDatabaseHasContent(ref NcmContentMetaDatabase db, ref bool @out, ref NcmContentMetaKey key, ref NcmContentId content_id);
    public static extern Result ncmContentMetaDatabaseListContentMetaInfo(ref NcmContentMetaDatabase db, ref s32 out_entries_written, IntPtr out_meta_info, s32 count, ref NcmContentMetaKey key, s32 start_index);
    public static extern Result ncmContentMetaDatabaseGetAttributes(ref NcmContentMetaDatabase db, ref NcmContentMetaKey key, ref u8 @out);
    public static extern Result ncmContentMetaDatabaseGetRequiredApplicationVersion(ref NcmContentMetaDatabase db, ref u32 out_version, ref NcmContentMetaKey key);
    public static extern Result ncmContentMetaDatabaseGetContentIdByTypeAndIdOffset(ref NcmContentMetaDatabase db, ref NcmContentId out_content_id, ref NcmContentMetaKey key, NcmContentType type, u8 id_offset);
    public static extern Result ncmContentMetaDatabaseGetPlatform(ref NcmContentMetaDatabase db, ref u8 @out, ref NcmContentMetaKey key);
}
