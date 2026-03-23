// Auto-generated from services\fs.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum FsDirEntryType
{
    FsDirEntryType_Dir = 0,
    FsDirEntryType_File = 1,
}

public enum FsOpenMode
{
    FsOpenMode_Read = (1 << 0),
    FsOpenMode_Write = (1 << 1),
    FsOpenMode_Append = (1 << 2),
}

public enum FsCreateOption
{
    FsCreateOption_BigFile = (1 << 0),
}

public enum FsDirOpenMode
{
    FsDirOpenMode_ReadDirs = (1 << 0),
    FsDirOpenMode_ReadFiles = (1 << 1),
    FsDirOpenMode_NoFileSize = (1 << 31),
}

public enum FsReadOption
{
    FsReadOption_None = 0,
}

public enum FsWriteOption
{
    FsWriteOption_None = 0,
    FsWriteOption_Flush = (1 << 0),
}

public enum FsContentStorageId
{
    FsContentStorageId_System = 0,
    FsContentStorageId_User = 1,
    FsContentStorageId_SdCard = 2,
    FsContentStorageId_System0 = 3,
}

public enum FsCustomStorageId
{
    FsCustomStorageId_System = 0,
    FsCustomStorageId_SdCard = 1,
}

public enum FsImageDirectoryId
{
    FsImageDirectoryId_Nand = 0,
    FsImageDirectoryId_Sd = 1,
}

public enum FsSaveDataSpaceId
{
    FsSaveDataSpaceId_System = 0,
    FsSaveDataSpaceId_User = 1,
    FsSaveDataSpaceId_SdSystem = 2,
    FsSaveDataSpaceId_Temporary = 3,
    FsSaveDataSpaceId_SdUser = 4,
    FsSaveDataSpaceId_ProperSystem = 100,
    FsSaveDataSpaceId_SafeMode = 101,
    FsSaveDataSpaceId_All = -1,
}

public enum FsSaveDataType
{
    FsSaveDataType_System = 0,
    FsSaveDataType_Account = 1,
    FsSaveDataType_Bcat = 2,
    FsSaveDataType_Device = 3,
    FsSaveDataType_Temporary = 4,
    FsSaveDataType_Cache = 5,
    FsSaveDataType_SystemBcat = 6,
}

public enum FsSaveDataRank
{
    FsSaveDataRank_Primary = 0,
    FsSaveDataRank_Secondary = 1,
}

public enum FsSaveDataFlags
{
    FsSaveDataFlags_KeepAfterResettingSystemSaveData = (1 << 0),
    FsSaveDataFlags_KeepAfterRefurbishment = (1 << 1),
    FsSaveDataFlags_KeepAfterResettingSystemSaveDataWithoutUserSaveData = (1 << 2),
    FsSaveDataFlags_NeedsSecureDelete = (1 << 3),
}

public enum FsSaveDataMetaType
{
    FsSaveDataMetaType_None = 0,
    FsSaveDataMetaType_Thumbnail = 1,
    FsSaveDataMetaType_ExtensionContext = 2,
}

public enum FsGameCardAttribute
{
    FsGameCardAttribute_AutoBootFlag = (1 << 0),
    FsGameCardAttribute_HistoryEraseFlag = (1 << 1),
    FsGameCardAttribute_RepairToolFlag = (1 << 2),
    FsGameCardAttribute_DifferentRegionCupToTerraDeviceFlag = (1 << 3),
    FsGameCardAttribute_DifferentRegionCupToGlobalDeviceFlag = (1 << 4),
}

public enum FsGameCardPartition
{
    FsGameCardPartition_Update = 0,
    FsGameCardPartition_Normal = 1,
    FsGameCardPartition_Secure = 2,
    FsGameCardPartition_Logo = 3,
}

public enum FsOperationId
{
    FsOperationId_Clear,
    FsOperationId_ClearSignature,
    FsOperationId_InvalidateCache,
    FsOperationId_QueryRange,
}

public enum FsBisPartitionId
{
    FsBisPartitionId_BootPartition1Root = 0,
    FsBisPartitionId_BootPartition2Root = 10,
    FsBisPartitionId_UserDataRoot = 20,
    FsBisPartitionId_BootConfigAndPackage2Part1 = 21,
    FsBisPartitionId_BootConfigAndPackage2Part2 = 22,
    FsBisPartitionId_BootConfigAndPackage2Part3 = 23,
    FsBisPartitionId_BootConfigAndPackage2Part4 = 24,
    FsBisPartitionId_BootConfigAndPackage2Part5 = 25,
    FsBisPartitionId_BootConfigAndPackage2Part6 = 26,
    FsBisPartitionId_CalibrationBinary = 27,
    FsBisPartitionId_CalibrationFile = 28,
    FsBisPartitionId_SafeMode = 29,
    FsBisPartitionId_User = 30,
    FsBisPartitionId_System = 31,
    FsBisPartitionId_SystemProperEncryption = 32,
    FsBisPartitionId_SystemProperPartition = 33,
    FsBisPartitionId_SignedSystemPartitionOnSafeMode = 34,
    FsBisPartitionId_DeviceTreeBlob = 35,
    FsBisPartitionId_System0 = 36,
}

public enum FsFileSystemType
{
    FsFileSystemType_Logo = 2,
    FsFileSystemType_ContentControl = 3,
    FsFileSystemType_ContentManual = 4,
    FsFileSystemType_ContentMeta = 5,
    FsFileSystemType_ContentData = 6,
    FsFileSystemType_ApplicationPackage = 7,
    FsFileSystemType_RegisteredUpdate = 8,
}

public enum FsFileSystemQueryId
{
    FsFileSystemQueryId_SetConcatenationFileAttribute = 0,
    FsFileSystemQueryId_IsValidSignedSystemPartitionOnSdCard = 2,
}

public enum FsPriority
{
    FsPriority_Normal = 0,
    FsPriority_Realtime = 1,
    FsPriority_Low = 2,
    FsPriority_Background = 3,
}

public enum FsContentAttributes
{
    FsContentAttributes_None = 0x0,
    FsContentAttributes_All = 0xF,
}

public enum FsMountHostOption
{
    FsMountHostOptionFlag_None = 0,
    FsMountHostOptionFlag_PseudoCaseSensitive = (1 << 0),
}

public unsafe struct FsRightsId
{
    public u8 c;
}

public unsafe struct FsFileSystem
{
    public Service s;
}

public unsafe struct FsFile
{
    public Service s;
}

public unsafe struct FsDir
{
    public Service s;
}

public unsafe struct FsStorage
{
    public Service s;
}

public unsafe struct FsSaveDataInfoReader
{
    public Service s;
}

public unsafe struct FsEventNotifier
{
    public Service s;
}

public unsafe struct FsDeviceOperator
{
    public Service s;
}

public unsafe struct FsDirectoryEntry
{
    public byte name;
    public fixed u8 pad[3];
    public s8 type;
    public fixed u8 pad2[3];
    public s64 file_size;
}

public unsafe struct FsSaveDataAttribute
{
    public u64 application_id;
    public AccountUid uid;
    public u64 system_save_data_id;
    public u8 save_data_type;
    public u8 save_data_rank;
    public u16 save_data_index;
    public u32 pad_x24;
    public u64 unk_x28;
    public u64 unk_x30;
    public u64 unk_x38;
}

public unsafe struct FsSaveDataExtraData
{
    public FsSaveDataAttribute attr;
    public u64 owner_id;
    public u64 timestamp;
    public u32 flags;
    public u32 unk_x54;
    public s64 data_size;
    public s64 journal_size;
    public u64 commit_id;
    public u8 unused;
}

public unsafe struct FsSaveDataMetaInfo
{
    public u32 size;
    public u8 type;
    public u8 reserved;
}

public unsafe struct FsSaveDataCreationInfo
{
    public s64 save_data_size;
    public s64 journal_size;
    public u64 available_size;
    public u64 owner_id;
    public u32 flags;
    public u8 save_data_space_id;
    public u8 unk;
    public u8 padding;
}

public unsafe struct FsSaveDataInfo
{
    public u64 save_data_id;
    public u8 save_data_space_id;
    public u8 save_data_type;
    public fixed u8 pad[6];
    public AccountUid uid;
    public u64 system_save_data_id;
    public u64 application_id;
    public u64 size;
    public u16 save_data_index;
    public u8 save_data_rank;
    public u8 unk_x3b;
}

public unsafe struct FsSaveDataFilter
{
    public bool filter_by_application_id;
    public bool filter_by_save_data_type;
    public bool filter_by_user_id;
    public bool filter_by_system_save_data_id;
    public bool filter_by_index;
    public u8 save_data_rank;
    public u8 padding;
    public FsSaveDataAttribute attr;
}

public unsafe struct FsTimeStampRaw
{
    public u64 created;
    public u64 modified;
    public u64 accessed;
    public u8 is_valid;
    public fixed u8 padding[7];
}

public unsafe struct FsArchiveMacKey
{
    public u8 key;
}

public unsafe struct FsGameCardHandle
{
    public u32 value;
}

public unsafe struct FsGameCardUpdatePartitionInfo
{
    public u32 version;
    public u8 pad;
    public u64 id;
}

public unsafe struct FsRangeInfo
{
    public u32 aes_ctr_key_type;
    public u32 speed_emulation_type;
    public u32 reserved;
}

public unsafe struct FsFileSystemAttribute
{
    public bool directory_name_length_max_has_value;
    public bool file_name_length_max_has_value;
    public bool directory_path_length_max_has_value;
    public bool file_path_length_max_has_value;
    public bool utf16_create_directory_path_length_max_has_value;
    public bool utf16_delete_directory_path_length_max_has_value;
    public bool utf16_rename_source_directory_path_length_max_has_value;
    public bool utf16_rename_destination_directory_path_length_max_has_value;
    public bool utf16_open_directory_path_length_max_has_value;
    public bool utf16_directory_name_length_max_has_value;
    public bool utf16_file_name_length_max_has_value;
    public bool utf16_directory_path_length_max_has_value;
    public bool utf16_file_path_length_max_has_value;
    public u8 reserved1;
    public s32 directory_name_length_max;
    public s32 file_name_length_max;
    public s32 directory_path_length_max;
    public s32 file_path_length_max;
    public s32 utf16_create_directory_path_length_max;
    public s32 utf16_delete_directory_path_length_max;
    public s32 utf16_rename_source_directory_path_length_max;
    public s32 utf16_rename_destination_directory_path_length_max;
    public s32 utf16_open_directory_path_length_max;
    public s32 utf16_directory_name_length_max;
    public s32 utf16_file_name_length_max;
    public s32 utf16_directory_path_length_max;
    public s32 utf16_file_path_length_max;
    public u8 reserved2;
}

public unsafe struct FsStorageErrorInfo
{
    public u32 num_activation_failures;
    public u32 num_activation_error_corrections;
    public u32 num_read_write_failures;
    public u32 num_read_write_error_corrections;
}

public unsafe struct FatFatError
{
    public s32 error;
    public s32 extra_error;
    public s32 drive_id;
    public fixed byte name[16];
    public fixed u8 reserved[4];
}

public unsafe struct FatFatReportInfo1
{
    public u16 open_file_peak_count;
    public u16 open_directory_peak_count;
}

public unsafe struct FatFatReportInfo2
{
    public u16 open_unique_file_entry_peak_count;
    public u16 open_unique_directory_entry_peak_count;
}

public unsafe struct FatFatSafeInfo
{
    public u32 result;
    public u32 error_number;
    public u32 safe_error_number;
}

public unsafe struct FsFileSystemProxyErrorInfo
{
    public u32 rom_fs_remount_for_data_corruption_count;
    public u32 rom_fs_unrecoverable_data_corruption_by_remount_count;
    public FatFatError fat_fs_error;
    public u32 rom_fs_recovered_by_invalidate_cache_count;
    public u32 save_data_index_count;
    public FatFatReportInfo1 bis_system_fat_report_info_1;
    public FatFatReportInfo1 bis_user_fat_report_info_1;
    public FatFatReportInfo1 sd_card_fat_report_info_1;
    public FatFatReportInfo2 bis_system_fat_report_info_2;
    public FatFatReportInfo2 bis_user_fat_report_info_2;
    public FatFatReportInfo2 sd_card_fat_report_info_2;
    public u32 rom_fs_deep_retry_start_count;
    public u32 rom_fs_unrecoverable_by_game_card_access_failed_count;
    public FatFatSafeInfo bis_system_fat_safe_info;
    public FatFatSafeInfo bis_user_fat_safe_info;
    public u8 reserved;
}

public unsafe struct FsMemoryReportInfo
{
    public u64 pooled_buffer_peak_free_size;
    public u64 pooled_buffer_retried_count;
    public u64 pooled_buffer_reduce_allocation_count;
    public u64 buffer_manager_peak_free_size;
    public u64 buffer_manager_retried_count;
    public u64 exp_heap_peak_free_size;
    public u64 buffer_pool_peak_free_size;
    public u64 patrol_read_allocate_buffer_success_count;
    public u64 patrol_read_allocate_buffer_failure_count;
    public u64 buffer_manager_peak_total_allocatable_size;
    public u64 buffer_pool_max_allocate_size;
    public u64 pooled_buffer_failed_ideal_allocation_count_on_async_access;
    public u8 reserved;
}

public unsafe struct FsGameCardErrorReportInfo
{
    public u16 game_card_crc_error_num;
    public u16 reserved1;
    public u16 asic_crc_error_num;
    public u16 reserved2;
    public u16 refresh_num;
    public u16 reserved3;
    public u16 retry_limit_out_num;
    public u16 timeout_retry_num;
    public u16 asic_reinitialize_failure_detail;
    public u16 insertion_count;
    public u16 removal_count;
    public u16 asic_reinitialize_num;
    public u32 initialize_count;
    public u16 asic_reinitialize_failure_num;
    public u16 awaken_failure_num;
    public u16 reserved4;
    public u16 refresh_succeeded_count;
    public u32 last_read_error_page_address;
    public u32 last_read_error_page_count;
    public u32 awaken_count;
    public u32 read_count_from_insert;
    public u32 read_count_from_awaken;
    public fixed u8 reserved5[8];
}

public static class Fs
{
    public static extern Result fsInitialize();
    public static extern void fsExit();
    public static extern IntPtr fsGetServiceSession();
    public static extern void fsSetPriority(FsPriority prio);
    public static extern Result fsOpenFileSystem(ref FsFileSystem @out, FsFileSystemType fsType, ref byte contentPath);
    public static extern Result fsOpenDataFileSystemByCurrentProcess(ref FsFileSystem @out);
    public static extern Result fsOpenFileSystemWithPatch(ref FsFileSystem @out, u64 id, FsFileSystemType fsType);
    public static extern Result fsOpenFileSystemWithId(ref FsFileSystem @out, u64 id, FsFileSystemType fsType, ref byte contentPath, FsContentAttributes attr);
    public static extern Result fsOpenDataFileSystemByProgramId(ref FsFileSystem @out, u64 program_id);
    public static extern Result fsOpenBisFileSystem(ref FsFileSystem @out, FsBisPartitionId partitionId, ref byte @string);
    public static extern Result fsOpenBisStorage(ref FsStorage @out, FsBisPartitionId partitionId);
    public static extern Result fsOpenSdCardFileSystem(ref FsFileSystem @out);
    public static extern Result fsOpenHostFileSystem(ref FsFileSystem @out, ref byte path);
    public static extern Result fsOpenHostFileSystemWithOption(ref FsFileSystem @out, ref byte path, u32 flags);
    public static extern Result fsDeleteSaveDataFileSystem(u64 application_id);
    public static extern Result fsCreateSaveDataFileSystem(ref FsSaveDataAttribute attr, ref FsSaveDataCreationInfo creation_info, ref FsSaveDataMetaInfo meta);
    public static extern Result fsCreateSaveDataFileSystemBySystemSaveDataId(ref FsSaveDataAttribute attr, ref FsSaveDataCreationInfo creation_info);
    public static extern Result fsDeleteSaveDataFileSystemBySaveDataSpaceId(FsSaveDataSpaceId save_data_space_id, u64 saveID);
    public static extern Result fsDeleteSaveDataFileSystemBySaveDataAttribute(FsSaveDataSpaceId save_data_space_id, ref FsSaveDataAttribute attr);
    public static extern Result fsIsExFatSupported(ref bool @out);
    public static extern Result fsOpenGameCardFileSystem(ref FsFileSystem @out, ref FsGameCardHandle handle, FsGameCardPartition partition);
    public static extern Result fsExtendSaveDataFileSystem(FsSaveDataSpaceId save_data_space_id, u64 saveID, s64 dataSize, s64 journalSize);
    public static extern Result fsOpenSaveDataFileSystem(ref FsFileSystem @out, FsSaveDataSpaceId save_data_space_id, ref FsSaveDataAttribute attr);
    public static extern Result fsOpenSaveDataFileSystemBySystemSaveDataId(ref FsFileSystem @out, FsSaveDataSpaceId save_data_space_id, ref FsSaveDataAttribute attr);
    public static extern Result fsOpenReadOnlySaveDataFileSystem(ref FsFileSystem @out, FsSaveDataSpaceId save_data_space_id, ref FsSaveDataAttribute attr);
    public static extern Result fsReadSaveDataFileSystemExtraDataBySaveDataSpaceId(IntPtr buf, ulong len, FsSaveDataSpaceId save_data_space_id, u64 saveID);
    public static extern Result fsReadSaveDataFileSystemExtraData(IntPtr buf, ulong len, u64 saveID);
    public static extern Result fsWriteSaveDataFileSystemExtraData(IntPtr buf, ulong len, FsSaveDataSpaceId save_data_space_id, u64 saveID);
    public static extern Result fsOpenSaveDataInfoReader(ref FsSaveDataInfoReader @out, FsSaveDataSpaceId save_data_space_id);
    public static extern Result fsOpenSaveDataInfoReaderWithFilter(ref FsSaveDataInfoReader @out, FsSaveDataSpaceId save_data_space_id, ref FsSaveDataFilter save_data_filter);
    public static extern Result fsOpenImageDirectoryFileSystem(ref FsFileSystem @out, FsImageDirectoryId image_directory_id);
    public static extern Result fsOpenContentStorageFileSystem(ref FsFileSystem @out, FsContentStorageId content_storage_id);
    public static extern Result fsOpenCustomStorageFileSystem(ref FsFileSystem @out, FsCustomStorageId custom_storage_id);
    public static extern Result fsOpenDataStorageByCurrentProcess(ref FsStorage @out);
    public static extern Result fsOpenDataStorageByProgramId(ref FsStorage @out, u64 program_id);
    public static extern Result fsOpenDataStorageByDataId(ref FsStorage @out, u64 dataId, NcmStorageId storageId);
    public static extern Result fsOpenPatchDataStorageByCurrentProcess(ref FsStorage @out);
    public static extern Result fsOpenDeviceOperator(ref FsDeviceOperator @out);
    public static extern Result fsOpenSdCardDetectionEventNotifier(ref FsEventNotifier @out);
    public static extern Result fsIsSignedSystemPartitionOnSdCardValid(ref bool @out);
    public static extern Result fsGetProgramId(ref u64 @out, ref byte path, FsContentAttributes attr);
    public static extern Result fsGetRightsIdByPath(ref byte path, ref FsRightsId out_rights_id);
    public static extern Result fsGetRightsIdAndKeyGenerationByPath(ref byte path, FsContentAttributes attr, ref u8 out_key_generation, ref FsRightsId out_rights_id);
    public static extern Result fsGetContentStorageInfoIndex(ref s32 @out);
    public static extern Result fsDisableAutoSaveDataCreation();
    public static extern Result fsSetGlobalAccessLogMode(u32 mode);
    public static extern Result fsGetGlobalAccessLogMode(ref u32 out_mode);
    public static extern Result fsOutputAccessLogToSdCard(ref byte log, ulong size);
    public static extern Result fsGetAndClearErrorInfo(ref FsFileSystemProxyErrorInfo @out);
    public static extern Result fsGetAndClearMemoryReportInfo(ref FsMemoryReportInfo @out);
    public static extern Result fsGetProgramIndexForAccessLog(ref u32 out_program_index, ref u32 out_program_count);
    public static extern Result fsCreate_TemporaryStorage(u64 application_id, u64 owner_id, s64 size, u32 flags);
    public static extern Result fsCreate_SystemSaveDataWithOwner(FsSaveDataSpaceId save_data_space_id, u64 system_save_data_id, AccountUid uid, u64 owner_id, s64 size, s64 journal_size, u32 flags);
    public static extern Result fsCreate_SystemSaveData(FsSaveDataSpaceId save_data_space_id, u64 system_save_data_id, s64 size, s64 journal_size, u32 flags);
    public static extern Result fsOpen_SaveData(ref FsFileSystem @out, u64 application_id, AccountUid uid);
    public static extern Result fsOpen_SaveDataReadOnly(ref FsFileSystem @out, u64 application_id, AccountUid uid);
    public static extern Result fsOpen_BcatSaveData(ref FsFileSystem @out, u64 application_id);
    public static extern Result fsOpen_DeviceSaveData(ref FsFileSystem @out, u64 application_id);
    public static extern Result fsOpen_TemporaryStorage(ref FsFileSystem @out);
    public static extern Result fsOpen_CacheStorage(ref FsFileSystem @out, u64 application_id, u16 save_data_index);
    public static extern Result fsOpen_SystemSaveData(ref FsFileSystem @out, FsSaveDataSpaceId save_data_space_id, u64 system_save_data_id, AccountUid uid);
    public static extern Result fsOpen_SystemBcatSaveData(ref FsFileSystem @out, u64 system_save_data_id);
    public static extern Result fsFsCreateFile(ref FsFileSystem fs, ref byte path, s64 size, u32 option);
    public static extern Result fsFsDeleteFile(ref FsFileSystem fs, ref byte path);
    public static extern Result fsFsCreateDirectory(ref FsFileSystem fs, ref byte path);
    public static extern Result fsFsDeleteDirectory(ref FsFileSystem fs, ref byte path);
    public static extern Result fsFsDeleteDirectoryRecursively(ref FsFileSystem fs, ref byte path);
    public static extern Result fsFsRenameFile(ref FsFileSystem fs, ref byte cur_path, ref byte new_path);
    public static extern Result fsFsRenameDirectory(ref FsFileSystem fs, ref byte cur_path, ref byte new_path);
    public static extern Result fsFsGetEntryType(ref FsFileSystem fs, ref byte path, ref FsDirEntryType @out);
    public static extern Result fsFsOpenFile(ref FsFileSystem fs, ref byte path, u32 mode, ref FsFile @out);
    public static extern Result fsFsOpenDirectory(ref FsFileSystem fs, ref byte path, u32 mode, ref FsDir @out);
    public static extern Result fsFsCommit(ref FsFileSystem fs);
    public static extern Result fsFsGetFreeSpace(ref FsFileSystem fs, ref byte path, ref s64 @out);
    public static extern Result fsFsGetTotalSpace(ref FsFileSystem fs, ref byte path, ref s64 @out);
    public static extern Result fsFsGetFileTimeStampRaw(ref FsFileSystem fs, ref byte path, ref FsTimeStampRaw @out);
    public static extern Result fsFsCleanDirectoryRecursively(ref FsFileSystem fs, ref byte path);
    public static extern Result fsFsQueryEntry(ref FsFileSystem fs, IntPtr @out, ulong out_size, IntPtr @in, ulong in_size, ref byte path, FsFileSystemQueryId query_id);
    public static extern Result fsFsGetFileSystemAttribute(ref FsFileSystem fs, ref FsFileSystemAttribute @out);
    public static extern void fsFsClose(ref FsFileSystem fs);
    public static extern Result fsFsSetConcatenationFileAttribute(ref FsFileSystem fs, ref byte path);
    public static extern Result fsFsIsValidSignedSystemPartitionOnSdCard(ref FsFileSystem fs, ref bool @out);
    public static extern Result fsFileRead(ref FsFile f, s64 off, IntPtr buf, u64 read_size, u32 option, ref u64 bytes_read);
    public static extern Result fsFileWrite(ref FsFile f, s64 off, IntPtr buf, u64 write_size, u32 option);
    public static extern Result fsFileFlush(ref FsFile f);
    public static extern Result fsFileSetSize(ref FsFile f, s64 sz);
    public static extern Result fsFileGetSize(ref FsFile f, ref s64 @out);
    public static extern Result fsFileOperateRange(ref FsFile f, FsOperationId op_id, s64 off, s64 len, ref FsRangeInfo @out);
    public static extern void fsFileClose(ref FsFile f);
    public static extern Result fsDirRead(ref FsDir d, ref s64 total_entries, ulong max_entries, ref FsDirectoryEntry buf);
    public static extern Result fsDirGetEntryCount(ref FsDir d, ref s64 count);
    public static extern void fsDirClose(ref FsDir d);
    public static extern Result fsStorageRead(ref FsStorage s, s64 off, IntPtr buf, u64 read_size);
    public static extern Result fsStorageWrite(ref FsStorage s, s64 off, IntPtr buf, u64 write_size);
    public static extern Result fsStorageFlush(ref FsStorage s);
    public static extern Result fsStorageSetSize(ref FsStorage s, s64 sz);
    public static extern Result fsStorageGetSize(ref FsStorage s, ref s64 @out);
    public static extern Result fsStorageOperateRange(ref FsStorage s, FsOperationId op_id, s64 off, s64 len, ref FsRangeInfo @out);
    public static extern void fsStorageClose(ref FsStorage s);
    public static extern Result fsSaveDataInfoReaderRead(ref FsSaveDataInfoReader s, ref FsSaveDataInfo buf, ulong max_entries, ref s64 total_entries);
    public static extern void fsSaveDataInfoReaderClose(ref FsSaveDataInfoReader s);
    public static extern Result fsEventNotifierGetEventHandle(ref FsEventNotifier e, ref Event @out, bool autoclear);
    public static extern void fsEventNotifierClose(ref FsEventNotifier e);
    public static extern Result fsDeviceOperatorIsSdCardInserted(ref FsDeviceOperator d, ref bool @out);
    public static extern Result fsDeviceOperatorGetSdCardSpeedMode(ref FsDeviceOperator d, ref s64 @out);
    public static extern Result fsDeviceOperatorGetSdCardCid(ref FsDeviceOperator d, IntPtr dst, ulong dst_size, s64 size);
    public static extern Result fsDeviceOperatorGetSdCardUserAreaSize(ref FsDeviceOperator d, ref s64 @out);
    public static extern Result fsDeviceOperatorGetSdCardProtectedAreaSize(ref FsDeviceOperator d, ref s64 @out);
    public static extern Result fsDeviceOperatorGetAndClearSdCardErrorInfo(ref FsDeviceOperator d, ref FsStorageErrorInfo @out, ref s64 out_log_size, IntPtr dst, ulong dst_size, s64 size);
    public static extern Result fsDeviceOperatorGetMmcCid(ref FsDeviceOperator d, IntPtr dst, ulong dst_size, s64 size);
    public static extern Result fsDeviceOperatorGetMmcSpeedMode(ref FsDeviceOperator d, ref s64 @out);
    public static extern Result fsDeviceOperatorGetMmcPatrolCount(ref FsDeviceOperator d, ref u32 @out);
    public static extern Result fsDeviceOperatorGetAndClearMmcErrorInfo(ref FsDeviceOperator d, ref FsStorageErrorInfo @out, ref s64 out_log_size, IntPtr dst, ulong dst_size, s64 size);
    public static extern Result fsDeviceOperatorGetMmcExtendedCsd(ref FsDeviceOperator d, IntPtr dst, ulong dst_size, s64 size);
    public static extern Result fsDeviceOperatorIsGameCardInserted(ref FsDeviceOperator d, ref bool @out);
    public static extern Result fsDeviceOperatorGetGameCardHandle(ref FsDeviceOperator d, ref FsGameCardHandle @out);
    public static extern Result fsDeviceOperatorGetGameCardUpdatePartitionInfo(ref FsDeviceOperator d, ref FsGameCardHandle handle, ref FsGameCardUpdatePartitionInfo @out);
    public static extern Result fsDeviceOperatorGetGameCardAttribute(ref FsDeviceOperator d, ref FsGameCardHandle handle, ref u8 @out);
    public static extern Result fsDeviceOperatorGetGameCardDeviceCertificate(ref FsDeviceOperator d, ref FsGameCardHandle handle, IntPtr dst, ulong dst_size, ref s64 out_size, s64 size);
    public static extern Result fsDeviceOperatorGetGameCardIdSet(ref FsDeviceOperator d, IntPtr dst, ulong dst_size, s64 size);
    public static extern Result fsDeviceOperatorGetGameCardErrorReportInfo(ref FsDeviceOperator d, ref FsGameCardErrorReportInfo @out);
    public static extern Result fsDeviceOperatorGetGameCardDeviceId(ref FsDeviceOperator d, IntPtr dst, ulong dst_size, s64 size);
    public static extern Result fsDeviceOperatorChallengeCardExistence(ref FsDeviceOperator d, ref FsGameCardHandle handle, IntPtr dst, ulong dst_size, IntPtr seed, ulong seed_size, IntPtr value, ulong value_size);
    public static extern void fsDeviceOperatorClose(ref FsDeviceOperator d);
}
