// Auto-generated from services\ns.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum NsShellEvent
{
    NsShellEvent_None = 0,
    NsShellEvent_Exit = 1,
    NsShellEvent_Start = 2,
    NsShellEvent_Crash = 3,
    NsShellEvent_Debug = 4,
}

public enum NsApplicationControlSource
{
    NsApplicationControlSource_CacheOnly = 0,
    NsApplicationControlSource_Storage = 1,
    NsApplicationControlSource_StorageOnly = 2,
}

public enum NsBackgroundNetworkUpdateState
{
    NsBackgroundNetworkUpdateState_None = 0,
    NsBackgroundNetworkUpdateState_Downloading = 1,
    NsBackgroundNetworkUpdateState_Ready = 2,
}

public enum NsLatestSystemUpdate
{
    NsLatestSystemUpdate_UpToDate = 0,
    NsLatestSystemUpdate_Downloaded = 1,
    NsLatestSystemUpdate_NeedsDownload = 2,
}

public unsafe struct NsRequestServerStopper
{
    public Service s;
}

public unsafe struct NsProgressMonitorForDeleteUserSaveDataAll
{
    public Service s;
}

public unsafe struct NsProgressAsyncResult
{
    public Service s;
    public Event @event;
}

public unsafe struct NsSystemUpdateControl
{
    public Service s;
    public TransferMemory tmem;
}

public unsafe struct NsApplicationControlData
{
    public NacpStruct nacp;
    public u8 icon;
}

public unsafe struct NsApplicationOccupiedSize
{
    public u8 unk_x0;
}

public unsafe struct NsApplicationContentMetaStatus
{
    public u8 meta_type;
    public u8 storageID;
    public u8 rights_check;
    public u8 reserved;
    public u32 version;
    public u64 application_id;
}

public unsafe struct NsApplicationRecord
{
    public u64 application_id;
    public u8 last_event;
    public u8 attributes;
    public fixed u8 reserved[6];
    public u64 last_updated;
}

public unsafe struct NsProgressForDeleteUserSaveDataAll
{
    public u8 unk_x0;
}

public unsafe struct NsApplicationViewDeprecated
{
    public u64 application_id;
    public u8 unk_x8;
    public u32 flags;
    public u8 unk_x10;
    public u32 unk_x20;
    public u16 unk_x24;
    public u8 unk_x26;
    public u8 unk_x28;
    public u32 unk_x38;
    public u8 unk_x3c;
    public fixed u8 unk_x3d[3];
}

public unsafe struct NsApplicationView
{
    public u64 application_id;
    public u8 unk_x8;
    public u32 flags;
    public u8 unk_x10;
    public u32 unk_x20;
    public u16 unk_x24;
    public u8 unk_x26;
    public u8 unk_x28;
    public u8 unk_x30;
    public u32 unk_x40;
    public u8 unk_x44;
    public u8 unk_x45;
}

public unsafe struct NsPromotionInfo
{
    public u64 start_timestamp;
    public u64 end_timestamp;
    public s64 remaining_time;
    public u8 unk_x18;
    public u8 flags;
    public fixed u8 pad[3];
}

public unsafe struct NsApplicationViewWithPromotionInfo
{
    public NsApplicationView view;
    public NsPromotionInfo promotion;
}

public unsafe struct NsLaunchProperties
{
    public u64 program_id;
    public u32 version;
    public u8 storageID;
    public u8 index;
    public u8 is_application;
}

public unsafe struct NsShellEventInfo
{
    public NsShellEvent @event;
    public u64 process_id;
}

public unsafe struct NsSystemUpdateProgress
{
    public s64 current_size;
    public s64 total_size;
}

public unsafe struct NsEulaDataPath
{
    public byte path;
}

public unsafe struct NsApplicationRightsOnClient
{
    public u64 application_id;
    public AccountUid uid;
    public u8 flags_x18;
    public u8 flags_x19;
    public u8 unk_x1a;
}

public unsafe struct NsDownloadTaskStatus
{
    public u8 unk_x0;
}

public static class Ns
{
    public static extern Result nsInitialize();
    public static extern void nsExit();
    public static extern IntPtr nsGetServiceSession_GetterInterface();
    public static extern IntPtr nsGetServiceSession_ApplicationManagerInterface();
    public static extern Result nsGetDynamicRightsInterface(ref Service srv_out);
    public static extern Result nsGetReadOnlyApplicationControlDataInterface(ref Service srv_out);
    public static extern Result nsGetReadOnlyApplicationRecordInterface(ref Service srv_out);
    public static extern Result nsGetECommerceInterface(ref Service srv_out);
    public static extern Result nsGetApplicationVersionInterface(ref Service srv_out);
    public static extern Result nsGetFactoryResetInterface(ref Service srv_out);
    public static extern Result nsGetAccountProxyInterface(ref Service srv_out);
    public static extern Result nsGetApplicationManagerInterface(ref Service srv_out);
    public static extern Result nsGetDownloadTaskInterface(ref Service srv_out);
    public static extern Result nsGetContentManagementInterface(ref Service srv_out);
    public static extern Result nsGetDocumentInterface(ref Service srv_out);
    public static extern Result nsGetApplicationControlData(NsApplicationControlSource source, u64 application_id, ref NsApplicationControlData buffer, ulong size, ref u64 actual_size);
    public static extern Result nsGetApplicationDesiredLanguage(ref NacpStruct nacp, ref NacpLanguageEntry langentry);
    public static extern Result nsRequestLinkDevice(ref AsyncResult a, AccountUid uid);
    public static extern Result nsRequestSyncRights(ref AsyncResult a);
    public static extern Result nsRequestUnlinkDevice(ref AsyncResult a, AccountUid uid);
    public static extern Result nsResetToFactorySettings();
    public static extern Result nsResetToFactorySettingsWithoutUserSaveData();
    public static extern Result nsResetToFactorySettingsForRefurbishment();
    public static extern Result nsResetToFactorySettingsWithPlatformRegion();
    public static extern Result nsResetToFactorySettingsWithPlatformRegionAuthentication();
    public static extern Result nsListApplicationRecord(ref NsApplicationRecord records, s32 count, s32 entry_offset, ref s32 out_entrycount);
    public static extern Result nsGetApplicationRecordUpdateSystemEvent(ref Event out_event);
    public static extern Result nsGetApplicationViewDeprecated(ref NsApplicationViewDeprecated views, ref u64 application_ids, s32 count);
    public static extern Result nsDeleteApplicationEntity(u64 application_id);
    public static extern Result nsDeleteApplicationCompletely(u64 application_id);
    public static extern Result nsDeleteRedundantApplicationEntity();
    public static extern Result nsIsApplicationEntityMovable(u64 application_id, NcmStorageId storage_id, ref bool @out);
    public static extern Result nsMoveApplicationEntity(u64 application_id, NcmStorageId storage_id);
    public static extern Result nsRequestApplicationUpdateInfo(ref AsyncValue a, u64 application_id);
    public static extern Result nsCancelApplicationDownload(u64 application_id);
    public static extern Result nsResumeApplicationDownload(u64 application_id);
    public static extern Result nsCheckApplicationLaunchVersion(u64 application_id);
    public static extern Result nsCalculateApplicationDownloadRequiredSize(u64 application_id, ref NcmStorageId storage_id, ref s64 size);
    public static extern Result nsCleanupSdCard();
    public static extern Result nsGetSdCardMountStatusChangedEvent(ref Event out_event);
    public static extern Result nsGetGameCardUpdateDetectionEvent(ref Event out_event);
    public static extern Result nsDisableApplicationAutoDelete(u64 application_id);
    public static extern Result nsEnableApplicationAutoDelete(u64 application_id);
    public static extern Result nsSetApplicationTerminateResult(u64 application_id, Result res);
    public static extern Result nsClearApplicationTerminateResult(u64 application_id);
    public static extern Result nsGetLastSdCardMountUnexpectedResult();
    public static extern Result nsGetRequestServerStopper(ref NsRequestServerStopper r);
    public static extern Result nsCancelApplicationApplyDelta(u64 application_id);
    public static extern Result nsResumeApplicationApplyDelta(u64 application_id);
    public static extern Result nsCalculateApplicationApplyDeltaRequiredSize(u64 application_id, ref NcmStorageId storage_id, ref s64 size);
    public static extern Result nsResumeAll();
    public static extern Result nsGetStorageSize(NcmStorageId storage_id, ref s64 total_space_size, ref s64 free_space_size);
    public static extern Result nsRequestUpdateApplication2(ref AsyncResult a, u64 application_id);
    public static extern Result nsDeleteUserSaveDataAll(ref NsProgressMonitorForDeleteUserSaveDataAll p, AccountUid uid);
    public static extern Result nsDeleteUserSystemSaveData(AccountUid uid, u64 system_save_data_id);
    public static extern Result nsDeleteSaveData(FsSaveDataSpaceId save_data_space_id, u64 save_data_id);
    public static extern Result nsUnregisterNetworkServiceAccount(AccountUid uid);
    public static extern Result nsUnregisterNetworkServiceAccountWithUserSaveDataDeletion(AccountUid uid);
    public static extern Result nsRequestDownloadApplicationControlData(ref AsyncResult a, u64 application_id);
    public static extern Result nsListApplicationTitle(ref AsyncValue a, NsApplicationControlSource source, ref u64 application_ids, s32 count, IntPtr buffer, ulong size);
    public static extern Result nsListApplicationIcon(ref AsyncValue a, NsApplicationControlSource source, ref u64 application_ids, s32 count, IntPtr buffer, ulong size);
    public static extern Result nsRequestCheckGameCardRegistration(ref AsyncResult a, u64 application_id);
    public static extern Result nsRequestGameCardRegistrationGoldPoint(ref AsyncValue a, AccountUid uid, u64 application_id);
    public static extern Result nsRequestRegisterGameCard(ref AsyncResult a, AccountUid uid, u64 application_id, s32 inval);
    public static extern Result nsGetGameCardMountFailureEvent(ref Event out_event);
    public static extern Result nsIsGameCardInserted(ref bool @out);
    public static extern Result nsEnsureGameCardAccess();
    public static extern Result nsGetLastGameCardMountFailureResult();
    public static extern Result nsListApplicationIdOnGameCard(ref u64 application_ids, s32 count, ref s32 total_out);
    public static extern Result nsTouchApplication(u64 application_id);
    public static extern Result nsIsApplicationUpdateRequested(u64 application_id, ref bool flag, ref u32 @out);
    public static extern Result nsWithdrawApplicationUpdateRequest(u64 application_id);
    public static extern Result nsRequestVerifyAddOnContentsRights(ref NsProgressAsyncResult a, u64 application_id);
    public static extern Result nsRequestVerifyApplication(ref NsProgressAsyncResult a, u64 application_id, u32 unk, IntPtr buffer, ulong size);
    public static extern Result nsIsAnyApplicationEntityInstalled(u64 application_id, ref bool @out);
    public static extern Result nsCleanupUnavailableAddOnContents(u64 application_id, AccountUid uid);
    public static extern Result nsEstimateSizeToMove(ref u8 storage_ids, s32 count, NcmStorageId storage_id, u32 flags, u64 application_id, ref s64 @out);
    public static extern Result nsFormatSdCard();
    public static extern Result nsNeedsSystemUpdateToFormatSdCard(ref bool @out);
    public static extern Result nsGetLastSdCardFormatUnexpectedResult();
    public static extern Result nsGetApplicationView(ref NsApplicationView views, ref u64 application_ids, s32 count);
    public static extern Result nsGetApplicationViewDownloadErrorContext(u64 application_id, ref ErrorContext context);
    public static extern Result nsGetApplicationViewWithPromotionInfo(ref NsApplicationViewWithPromotionInfo @out, ref u64 application_ids, s32 count);
    public static extern Result nsRequestDownloadApplicationPrepurchasedRights(ref AsyncResult a, u64 application_id);
    public static extern Result nsGetSystemDeliveryInfo(ref NsSystemDeliveryInfo info);
    public static extern Result nsSelectLatestSystemDeliveryInfo(ref NsSystemDeliveryInfo sys_list, s32 sys_count, ref NsSystemDeliveryInfo base_info, ref NsApplicationDeliveryInfo app_list, s32 app_count, ref s32 index);
    public static extern Result nsVerifyDeliveryProtocolVersion(ref NsSystemDeliveryInfo info);
    public static extern Result nsGetApplicationDeliveryInfo(ref NsApplicationDeliveryInfo info, s32 count, u64 application_id, u32 attr, ref s32 total_out);
    public static extern Result nsHasAllContentsToDeliver(ref NsApplicationDeliveryInfo info, s32 count, ref bool @out);
    public static extern Result nsCompareApplicationDeliveryInfo(ref NsApplicationDeliveryInfo info0, s32 count0, ref NsApplicationDeliveryInfo info1, s32 count1, ref s32 @out);
    public static extern Result nsCanDeliverApplication(ref NsApplicationDeliveryInfo info0, s32 count0, ref NsApplicationDeliveryInfo info1, s32 count1, ref bool @out);
    public static extern Result nsListContentMetaKeyToDeliverApplication(ref NcmContentMetaKey meta, s32 meta_count, s32 meta_index, ref NsApplicationDeliveryInfo info, s32 info_count, ref s32 total_out);
    public static extern Result nsNeedsSystemUpdateToDeliverApplication(ref NsApplicationDeliveryInfo info, s32 count, ref NsSystemDeliveryInfo sys_info, ref bool @out);
    public static extern Result nsEstimateRequiredSize(ref NcmContentMetaKey meta, s32 count, ref s64 @out);
    public static extern Result nsRequestReceiveApplication(ref AsyncResult a, u32 addr, u16 port, u64 application_id, ref NcmContentMetaKey meta, s32 count, NcmStorageId storage_id);
    public static extern Result nsCommitReceiveApplication(u64 application_id);
    public static extern Result nsGetReceiveApplicationProgress(u64 application_id, ref NsReceiveApplicationProgress @out);
    public static extern Result nsRequestSendApplication(ref AsyncResult a, u32 addr, u16 port, u64 application_id, ref NcmContentMetaKey meta, s32 count);
    public static extern Result nsGetSendApplicationProgress(u64 application_id, ref NsSendApplicationProgress @out);
    public static extern Result nsCompareSystemDeliveryInfo(ref NsSystemDeliveryInfo info0, ref NsSystemDeliveryInfo info1, ref s32 @out);
    public static extern Result nsListNotCommittedContentMeta(ref NcmContentMetaKey meta, s32 count, u64 application_id, s32 unk, ref s32 total_out);
    public static extern Result nsGetApplicationDeliveryInfoHash(ref NsApplicationDeliveryInfo info, s32 count, ref u8 out_hash);
    public static extern Result nsGetApplicationTerminateResult(u64 application_id, ref Result res);
    public static extern Result nsGetApplicationRightsOnClient(ref NsApplicationRightsOnClient rights, s32 count, u64 application_id, AccountUid uid, u32 flags, ref s32 total_out);
    public static extern Result nsRequestNoDownloadRightsErrorResolution(ref AsyncValue a, u64 application_id);
    public static extern Result nsRequestResolveNoDownloadRightsError(ref AsyncValue a, u64 application_id);
    public static extern Result nsGetPromotionInfo(ref NsPromotionInfo promotion, u64 application_id, AccountUid uid);
    public static extern Result nsClearTaskStatusList();
    public static extern Result nsRequestDownloadTaskList();
    public static extern Result nsRequestEnsureDownloadTask(ref AsyncResult a);
    public static extern Result nsListDownloadTaskStatus(ref NsDownloadTaskStatus tasks, s32 count, ref s32 total_out);
    public static extern Result nsRequestDownloadTaskListData(ref AsyncValue a);
    public static extern Result nsTryCommitCurrentApplicationDownloadTask();
    public static extern Result nsEnableAutoCommit();
    public static extern Result nsDisableAutoCommit();
    public static extern Result nsTriggerDynamicCommitEvent();
    public static extern Result nsCalculateApplicationOccupiedSize(u64 application_id, ref NsApplicationOccupiedSize @out);
    public static extern Result nsCheckSdCardMountStatus();
    public static extern Result nsGetTotalSpaceSize(NcmStorageId storage_id, ref s64 size);
    public static extern Result nsGetFreeSpaceSize(NcmStorageId storage_id, ref s64 size);
    public static extern Result nsCountApplicationContentMeta(u64 application_id, ref s32 @out);
    public static extern Result nsListApplicationContentMetaStatus(u64 application_id, s32 index, ref NsApplicationContentMetaStatus list, s32 count, ref s32 out_entrycount);
    public static extern Result nsIsAnyApplicationRunning(ref bool @out);
    public static extern void nsRequestServerStopperClose(ref NsRequestServerStopper r);
    public static extern Result nsProgressMonitorForDeleteUserSaveDataAllClose(ref NsProgressMonitorForDeleteUserSaveDataAll p);
    public static extern Result nsProgressMonitorForDeleteUserSaveDataAllGetSystemEvent(ref NsProgressMonitorForDeleteUserSaveDataAll p, ref Event out_event);
    public static extern Result nsProgressMonitorForDeleteUserSaveDataAllIsFinished(ref NsProgressMonitorForDeleteUserSaveDataAll p, ref bool @out);
    public static extern Result nsProgressMonitorForDeleteUserSaveDataAllGetResult(ref NsProgressMonitorForDeleteUserSaveDataAll p);
    public static extern Result nsProgressMonitorForDeleteUserSaveDataAllGetProgress(ref NsProgressMonitorForDeleteUserSaveDataAll p, ref NsProgressForDeleteUserSaveDataAll progress);
    public static extern void nsProgressAsyncResultClose(ref NsProgressAsyncResult a);
    public static extern Result nsProgressAsyncResultWait(ref NsProgressAsyncResult a, u64 timeout);
    public static extern Result nsProgressAsyncResultGet(ref NsProgressAsyncResult a);
    public static extern Result nsProgressAsyncResultCancel(ref NsProgressAsyncResult a);
    public static extern Result nsProgressAsyncResultGetProgress(ref NsProgressAsyncResult a, IntPtr buffer, ulong size);
    public static extern Result nsProgressAsyncResultGetDetailResult(ref NsProgressAsyncResult a);
    public static extern Result nsProgressAsyncResultGetErrorContext(ref NsProgressAsyncResult a, ref ErrorContext context);
    public static extern Result nsvmInitialize();
    public static extern void nsvmExit();
    public static extern IntPtr nsvmGetServiceSession();
    public static extern Result nsvmNeedsUpdateVulnerability(ref bool @out);
    public static extern Result nsvmGetSafeSystemVersion(ref NcmContentMetaKey @out);
    public static extern Result nsdevInitialize();
    public static extern void nsdevExit();
    public static extern IntPtr nsdevGetServiceSession();
    public static extern Result nsdevLaunchProgram(ref u64 out_pid, ref NsLaunchProperties properties, u32 flags);
    public static extern Result nsdevTerminateProcess(u64 pid);
    public static extern Result nsdevTerminateProgram(u64 tid);
    public static extern Result nsdevGetShellEvent(ref Event out_event);
    public static extern Result nsdevGetShellEventInfo(ref NsShellEventInfo @out);
    public static extern Result nsdevTerminateApplication();
    public static extern Result nsdevPrepareLaunchProgramFromHost(ref NsLaunchProperties @out, ref byte path, ulong path_len);
    public static extern Result nsdevLaunchApplicationForDevelop(ref u64 out_pid, u64 application_id, u32 flags);
    public static extern Result nsdevLaunchApplicationFromHost(ref u64 out_pid, ref byte path, ulong path_len, u32 flags);
    public static extern Result nsdevLaunchApplicationWithStorageIdForDevelop(ref u64 out_pid, u64 application_id, u32 flags, u8 app_storage_id, u8 patch_storage_id);
    public static extern Result nsdevIsSystemMemoryResourceLimitBoosted(ref bool @out);
    public static extern Result nsdevGetRunningApplicationProcessIdForDevelop(ref u64 out_pid);
    public static extern Result nsdevSetCurrentApplicationRightsEnvironmentCanBeActiveForDevelop(bool can_be_active);
    public static extern Result nssuInitialize();
    public static extern void nssuExit();
    public static extern IntPtr nssuGetServiceSession();
    public static extern Result nssuGetBackgroundNetworkUpdateState(ref NsBackgroundNetworkUpdateState @out);
    public static extern Result nssuOpenSystemUpdateControl(ref NsSystemUpdateControl c);
    public static extern Result nssuNotifyExFatDriverRequired();
    public static extern Result nssuClearExFatDriverStatusForDebug();
    public static extern Result nssuRequestBackgroundNetworkUpdate();
    public static extern Result nssuNotifyBackgroundNetworkUpdate(ref NcmContentMetaKey key);
    public static extern Result nssuNotifyExFatDriverDownloadedForDebug();
    public static extern Result nssuGetSystemUpdateNotificationEventForContentDelivery(ref Event out_event);
    public static extern Result nssuNotifySystemUpdateForContentDelivery();
    public static extern Result nssuPrepareShutdown();
    public static extern Result nssuDestroySystemUpdateTask();
    public static extern Result nssuRequestSendSystemUpdate(ref AsyncResult a, u32 addr, u16 port, ref NsSystemDeliveryInfo info);
    public static extern Result nssuGetSendSystemUpdateProgress(ref NsSystemUpdateProgress @out);
    public static extern void nssuControlClose(ref NsSystemUpdateControl c);
    public static extern Result nssuControlHasDownloaded(ref NsSystemUpdateControl c, ref bool @out);
    public static extern Result nssuControlRequestCheckLatestUpdate(ref NsSystemUpdateControl c, ref AsyncValue a);
    public static extern Result nssuControlRequestDownloadLatestUpdate(ref NsSystemUpdateControl c, ref AsyncResult a);
    public static extern Result nssuControlGetDownloadProgress(ref NsSystemUpdateControl c, ref NsSystemUpdateProgress @out);
    public static extern Result nssuControlApplyDownloadedUpdate(ref NsSystemUpdateControl c);
    public static extern Result nssuControlRequestPrepareCardUpdate(ref NsSystemUpdateControl c, ref AsyncResult a);
    public static extern Result nssuControlGetPrepareCardUpdateProgress(ref NsSystemUpdateControl c, ref NsSystemUpdateProgress @out);
    public static extern Result nssuControlHasPreparedCardUpdate(ref NsSystemUpdateControl c, ref bool @out);
    public static extern Result nssuControlApplyCardUpdate(ref NsSystemUpdateControl c);
    public static extern Result nssuControlGetDownloadedEulaDataSize(ref NsSystemUpdateControl c, ref byte path, ref u64 filesize);
    public static extern Result nssuControlGetDownloadedEulaData(ref NsSystemUpdateControl c, ref byte path, IntPtr buffer, ulong size, ref u64 filesize);
    public static extern Result nssuControlSetupCardUpdate(ref NsSystemUpdateControl c, IntPtr buffer, ulong size);
    public static extern Result nssuControlGetPreparedCardUpdateEulaDataSize(ref NsSystemUpdateControl c, ref byte path, ref u64 filesize);
    public static extern Result nssuControlGetPreparedCardUpdateEulaData(ref NsSystemUpdateControl c, ref byte path, IntPtr buffer, ulong size, ref u64 filesize);
    public static extern Result nssuControlSetupCardUpdateViaSystemUpdater(ref NsSystemUpdateControl c, IntPtr buffer, ulong size);
    public static extern Result nssuControlHasReceived(ref NsSystemUpdateControl c, ref bool @out);
    public static extern Result nssuControlRequestReceiveSystemUpdate(ref NsSystemUpdateControl c, ref AsyncResult a, u32 addr, u16 port, ref NsSystemDeliveryInfo info);
    public static extern Result nssuControlGetReceiveProgress(ref NsSystemUpdateControl c, ref NsSystemUpdateProgress @out);
    public static extern Result nssuControlApplyReceivedUpdate(ref NsSystemUpdateControl c);
    public static extern Result nssuControlGetReceivedEulaDataSize(ref NsSystemUpdateControl c, ref byte path, ref u64 filesize);
    public static extern Result nssuControlGetReceivedEulaData(ref NsSystemUpdateControl c, ref byte path, IntPtr buffer, ulong size, ref u64 filesize);
    public static extern Result nssuControlSetupToReceiveSystemUpdate(ref NsSystemUpdateControl c);
    public static extern Result nssuControlRequestCheckLatestUpdateIncludesRebootlessUpdate(ref NsSystemUpdateControl c, ref AsyncValue a);
}
