// Auto-generated from services\applet.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum AppletType
{
    AppletType_None = -2,
    AppletType_Default = -1,
    AppletType_Application = 0,
    AppletType_SystemApplet = 1,
    AppletType_LibraryApplet = 2,
    AppletType_OverlayApplet = 3,
    AppletType_SystemApplication = 4,
}

public enum AppletOperationMode
{
    AppletOperationMode_Handheld = 0,
    AppletOperationMode_Console = 1,
}

public enum AppletHookType
{
    AppletHookType_OnFocusState = 0,
    AppletHookType_OnOperationMode,
    AppletHookType_OnPerformanceMode,
    AppletHookType_OnExitRequest,
    AppletHookType_OnResume,
    AppletHookType_OnCaptureButtonShortPressed,
    AppletHookType_OnAlbumScreenShotTaken,
    AppletHookType_RequestToDisplay,
    AppletHookType_Max,
}

public enum AppletMessage
{
    AppletMessage_ExitRequest = 4,
    AppletMessage_FocusStateChanged = 15,
    AppletMessage_Resume = 16,
    AppletMessage_OperationModeChanged = 30,
    AppletMessage_PerformanceModeChanged = 31,
    AppletMessage_RequestToDisplay = 51,
    AppletMessage_CaptureButtonShortPressed = 90,
    AppletMessage_AlbumScreenShotTaken = 92,
    AppletMessage_AlbumRecordingSaved = 93,
}

public enum AppletFocusState
{
    AppletFocusState_InFocus = 1,
    AppletFocusState_OutOfFocus = 2,
    AppletFocusState_Background = 3,
}

public enum AppletFocusHandlingMode
{
    AppletFocusHandlingMode_SuspendHomeSleep = 0,
    AppletFocusHandlingMode_NoSuspend,
    AppletFocusHandlingMode_SuspendHomeSleepNotify,
    AppletFocusHandlingMode_AlwaysSuspend,
    AppletFocusHandlingMode_Max,
}

public enum AppletLaunchParameterKind
{
    AppletLaunchParameterKind_UserChannel = 1,
    AppletLaunchParameterKind_PreselectedUser = 2,
    AppletLaunchParameterKind_Unknown = 3,
}

public enum AppletId
{
    AppletId_None = 0x00,
    AppletId_application = 0x01,
    AppletId_OverlayApplet = 0x02,
    AppletId_SystemAppletMenu = 0x03,
    AppletId_SystemApplication = 0x04,
    AppletId_LibraryAppletAuth = 0x0A,
    AppletId_LibraryAppletCabinet = 0x0B,
    AppletId_LibraryAppletController = 0x0C,
    AppletId_LibraryAppletDataErase = 0x0D,
    AppletId_LibraryAppletError = 0x0E,
    AppletId_LibraryAppletNetConnect = 0x0F,
    AppletId_LibraryAppletPlayerSelect = 0x10,
    AppletId_LibraryAppletSwkbd = 0x11,
    AppletId_LibraryAppletMiiEdit = 0x12,
    AppletId_LibraryAppletWeb = 0x13,
    AppletId_LibraryAppletShop = 0x14,
    AppletId_LibraryAppletPhotoViewer = 0x15,
    AppletId_LibraryAppletSet = 0x16,
    AppletId_LibraryAppletOfflineWeb = 0x17,
    AppletId_LibraryAppletLoginShare = 0x18,
    AppletId_LibraryAppletWifiWebAuth = 0x19,
    AppletId_LibraryAppletMyPage = 0x1A,
}

public enum LibAppletMode
{
    LibAppletMode_AllForeground = 0,
    LibAppletMode_Background = 1,
    LibAppletMode_NoUi = 2,
    LibAppletMode_BackgroundIndirect = 3,
    LibAppletMode_AllForegroundInitiallyHidden = 4,
}

public enum LibAppletExitReason
{
    LibAppletExitReason_Normal = 0,
    LibAppletExitReason_Canceled = 1,
    LibAppletExitReason_Abnormal = 2,
    LibAppletExitReason_Unexpected = 10,
}

public enum AppletApplicationExitReason
{
    AppletApplicationExitReason_Normal = 0,
    AppletApplicationExitReason_Unknown1 = 1,
    AppletApplicationExitReason_Unknown2 = 2,
    AppletApplicationExitReason_Unknown3 = 3,
    AppletApplicationExitReason_Unknown4 = 4,
    AppletApplicationExitReason_Unknown5 = 5,
    AppletApplicationExitReason_Unexpected = 100,
}

public enum AppletThemeColorType
{
    AppletThemeColorType_Default = 0,
    AppletThemeColorType_Unknown1 = 1,
    AppletThemeColorType_Unknown2 = 2,
    AppletThemeColorType_Unknown3 = 3,
}

public enum AppletTvPowerStateMatchingMode
{
    AppletTvPowerStateMatchingMode_Unknown0 = 0,
    AppletTvPowerStateMatchingMode_Unknown1 = 1,
}

public enum AppletSystemButtonType
{
    AppletSystemButtonType_HomeButtonShortPressing = 1,
    AppletSystemButtonType_HomeButtonLongPressing = 2,
    AppletSystemButtonType_PowerButtonShortPressing = 3,
    AppletSystemButtonType_PowerButtonLongPressing = 4,
    AppletSystemButtonType_Shutdown = 5,
    AppletSystemButtonType_CaptureButtonShortPressing = 6,
    AppletSystemButtonType_CaptureButtonLongPressing = 7,
}

public enum AppletScreenShotPermission
{
    AppletScreenShotPermission_Inherit = 0,
    AppletScreenShotPermission_Enable = 1,
    AppletScreenShotPermission_Disable = 2,
}

public enum AppletIdleTimeDetectionExtension
{
    AppletIdleTimeDetectionExtension_None = 0,
    AppletIdleTimeDetectionExtension_Extended = 1,
    AppletIdleTimeDetectionExtension_ExtendedUnsafe = 2,
}

public enum AppletInputDetectionPolicy
{
    AppletInputDetectionPolicy_Unknown0 = 0,
    AppletInputDetectionPolicy_Unknown1 = 1,
}

public enum AppletWirelessPriorityMode
{
    AppletWirelessPriorityMode_Default = 1,
    AppletWirelessPriorityMode_OptimizedForWlan = 2,
}

public enum AppletCaptureSharedBuffer
{
    AppletCaptureSharedBuffer_LastApplication = 0,
    AppletCaptureSharedBuffer_LastForeground = 1,
    AppletCaptureSharedBuffer_CallerApplet = 2,
}

public enum AppletWindowOriginMode
{
    AppletWindowOriginMode_LowerLeft = 0,
    AppletWindowOriginMode_UpperLeft = 1,
}

public enum AppletProgramSpecifyKind
{
    AppletProgramSpecifyKind_ExecuteProgram = 0,
    AppletProgramSpecifyKind_JumpToSubApplicationProgramForDevelopment = 1,
    AppletProgramSpecifyKind_RestartProgram = 2,
}

public unsafe struct AppletLockAccessor
{
    public Service s;
    public Event @event;
}

public unsafe struct AppletStorage
{
    public Service s;
    public TransferMemory tmem;
}

public unsafe struct AppletHolder
{
    public Service s;
    public Event StateChangedEvent;
    public Event PopInteractiveOutDataEvent;
    public LibAppletMode mode;
    public u64 layer_handle;
    public bool creating_self;
    public LibAppletExitReason exitreason;
}

public unsafe struct AppletApplication
{
    public Service s;
    public Event StateChangedEvent;
    public AppletApplicationExitReason exitreason;
}

public unsafe struct AppletGpuErrorHandler
{
    public Service s;
}

public unsafe struct AppletAttribute
{
    public u8 flag;
    public u8 reserved;
}

public unsafe struct LibAppletInfo
{
    public AppletId appletId;
    public LibAppletMode mode;
}

public unsafe struct AppletProcessLaunchReason
{
    public u8 flag;
    public fixed u8 unk_x1[3];
}

public unsafe struct AppletInfo
{
    public LibAppletInfo info;
    public bool caller_flag;
    public AppletHolder caller;
}

public unsafe struct AppletIdentityInfo
{
    public AppletId appletId;
    public u32 pad;
    public u64 application_id;
}

public unsafe struct AppletApplicationAttributeForQuest
{
    public u32 unk_x0;
    public u32 unk_x4;
    public float volume;
}

public unsafe struct AppletApplicationAttribute
{
    public u32 unk_x0;
    public u32 unk_x4;
    public float volume;
    public u8 unused;
}

public unsafe struct AppletApplicationLaunchProperty
{
    public u64 application_id;
    public u32 version;
    public u8 app_storageId;
    public u8 update_storageId;
    public u8 unk_xa;
    public u8 pad;
}

public unsafe struct AppletApplicationLaunchRequestInfo
{
    public u32 unk_x0;
    public u32 unk_x4;
    public u8 unk_x8;
}

public unsafe struct AppletResourceUsageInfo
{
    public u32 counter0;
    public u32 counter1;
    public u32 counter2;
    public u8 unused;
}

public static class Applet
{
    public static extern Result appletInitialize();
    public static extern void appletExit();
    public static extern IntPtr appletGetServiceSession_Proxy();
    public static extern IntPtr appletGetServiceSession_AppletCommonFunctions();
    public static extern IntPtr appletGetServiceSession_Functions();
    public static extern IntPtr appletGetServiceSession_GlobalStateController();
    public static extern IntPtr appletGetServiceSession_ApplicationCreator();
    public static extern IntPtr appletGetServiceSession_LibraryAppletSelfAccessor();
    public static extern IntPtr appletGetServiceSession_ProcessWindingController();
    public static extern IntPtr appletGetServiceSession_LibraryAppletCreator();
    public static extern IntPtr appletGetServiceSession_CommonStateGetter();
    public static extern IntPtr appletGetServiceSession_SelfController();
    public static extern IntPtr appletGetServiceSession_WindowController();
    public static extern IntPtr appletGetServiceSession_AudioController();
    public static extern IntPtr appletGetServiceSession_DisplayController();
    public static extern IntPtr appletGetServiceSession_DebugFunctions();
    public static extern u64 appletGetAppletResourceUserId();
    public static extern AppletType appletGetAppletType();
    public static extern void appletSetThemeColorType(AppletThemeColorType theme);
    public static extern AppletThemeColorType appletGetThemeColorType();
    public static extern Result appletGetCradleStatus(ref u8 status);
    public static extern Result appletGetBootMode(ref PmBootMode mode);
    public static extern Result appletRequestToAcquireSleepLock();
    public static extern Result appletReleaseSleepLock();
    public static extern Result appletReleaseSleepLockTransiently();
    public static extern Result appletGetWakeupCount(ref u64 @out);
    public static extern Result appletPushToGeneralChannel(ref AppletStorage s);
    public static extern Result appletGetHomeButtonReaderLockAccessor(ref AppletLockAccessor a);
    public static extern Result appletGetReaderLockAccessorEx(ref AppletLockAccessor a, u32 inval);
    public static extern Result appletGetWriterLockAccessorEx(ref AppletLockAccessor a, u32 inval);
    public static extern Result appletGetCradleFwVersion(ref u32 out0, ref u32 out1, ref u32 out2, ref u32 out3);
    public static extern Result appletIsVrModeEnabled(ref bool @out);
    public static extern Result appletSetVrModeEnabled(bool flag);
    public static extern Result appletSetLcdBacklightOffEnabled(bool flag);
    public static extern Result appletIsInControllerFirmwareUpdateSection(ref bool @out);
    public static extern Result appletSetVrPositionForDebug(s32 x, s32 y, s32 width, s32 height);
    public static extern Result appletGetDefaultDisplayResolution(ref s32 width, ref s32 height);
    public static extern Result appletGetDefaultDisplayResolutionChangeEvent(ref Event out_event);
    public static extern Result appletGetHdcpAuthenticationState(ref s32 state);
    public static extern Result appletGetHdcpAuthenticationStateChangeEvent(ref Event out_event);
    public static extern Result appletSetTvPowerStateMatchingMode(AppletTvPowerStateMatchingMode mode);
    public static extern Result appletGetApplicationIdByContentActionName(ref u64 application_id, ref byte name);
    public static extern Result appletSetCpuBoostMode(ApmCpuBoostMode mode);
    public static extern Result appletCancelCpuBoostMode();
    public static extern Result appletGetBuiltInDisplayType(ref s32 @out);
    public static extern Result appletPerformSystemButtonPressingIfInFocus(AppletSystemButtonType type);
    public static extern Result appletSetPerformanceConfigurationChangedNotification(bool flag);
    public static extern Result appletGetCurrentPerformanceConfiguration(ref u32 PerformanceConfiguration);
    public static extern Result appletOpenMyGpuErrorHandler(ref AppletGpuErrorHandler g);
    public static extern Result appletGetOperationModeSystemInfo(ref u32 info);
    public static extern Result appletGetSettingsPlatformRegion(ref SetSysPlatformRegion @out);
    public static extern Result appletActivateMigrationService();
    public static extern Result appletDeactivateMigrationService();
    public static extern Result appletDisableSleepTillShutdown();
    public static extern Result appletSuppressDisablingSleepTemporarily(u64 val);
    public static extern Result appletSetRequestExitToLibraryAppletAtExecuteNextProgramEnabled();
    public static extern void appletGpuErrorHandlerClose(ref AppletGpuErrorHandler g);
    public static extern Result appletGpuErrorHandlerGetManualGpuErrorInfoSize(ref AppletGpuErrorHandler g, ref u64 @out);
    public static extern Result appletGpuErrorHandlerGetManualGpuErrorInfo(ref AppletGpuErrorHandler g, IntPtr buffer, ulong size, ref u64 @out);
    public static extern Result appletGpuErrorHandlerGetManualGpuErrorDetectionSystemEvent(ref AppletGpuErrorHandler g, ref Event out_event);
    public static extern Result appletGpuErrorHandlerFinishManualGpuErrorHandling(ref AppletGpuErrorHandler g);
    public static extern Result appletLockExit();
    public static extern Result appletUnlockExit();
    public static extern Result appletEnterFatalSection();
    public static extern Result appletLeaveFatalSection();
    public static extern Result appletSetScreenShotPermission(AppletScreenShotPermission permission);
    public static extern Result appletSetRestartMessageEnabled(bool flag);
    public static extern Result appletSetScreenShotAppletIdentityInfo(ref AppletIdentityInfo info);
    public static extern Result appletSetControllerFirmwareUpdateSection(bool flag);
    public static extern Result appletSetRequiresCaptureButtonShortPressedMessage(bool flag);
    public static extern Result appletSetAlbumImageOrientation(AlbumImageOrientation orientation);
    public static extern Result appletSetDesirableKeyboardLayout(SetKeyboardLayout layout);
    public static extern Result appletCreateManagedDisplayLayer(ref u64 @out);
    public static extern Result appletIsSystemBufferSharingEnabled();
    public static extern Result appletGetSystemSharedLayerHandle(ref u64 SharedBufferHandle, ref u64 SharedLayerHandle);
    public static extern Result appletGetSystemSharedBufferHandle(ref u64 SharedBufferHandle);
    public static extern Result appletCreateManagedDisplaySeparableLayer(ref u64 display_layer, ref u64 recording_layer);
    public static extern Result appletSetManagedDisplayLayerSeparationMode(u32 mode);
    public static extern Result appletSetHandlesRequestToDisplay(bool flag);
    public static extern Result appletApproveToDisplay();
    public static extern Result appletOverrideAutoSleepTimeAndDimmingTime(s32 inval0, s32 inval1, s32 inval2, s32 inval3);
    public static extern Result appletSetIdleTimeDetectionExtension(AppletIdleTimeDetectionExtension ext);
    public static extern Result appletGetIdleTimeDetectionExtension(ref AppletIdleTimeDetectionExtension ext);
    public static extern Result appletSetInputDetectionSourceSet(u32 val);
    public static extern Result appletReportUserIsActive();
    public static extern Result appletGetCurrentIlluminance(ref float fLux);
    public static extern Result appletIsIlluminanceAvailable(ref bool @out);
    public static extern Result appletSetAutoSleepDisabled(bool flag);
    public static extern Result appletIsAutoSleepDisabled(ref bool @out);
    public static extern Result appletGetCurrentIlluminanceEx(ref bool bOverLimit, ref float fLux);
    public static extern Result appletSetInputDetectionPolicy(AppletInputDetectionPolicy policy);
    public static extern Result appletSetWirelessPriorityMode(AppletWirelessPriorityMode mode);
    public static extern Result appletGetProgramTotalActiveTime(ref u64 activeTime);
    public static extern Result appletSetAlbumImageTakenNotificationEnabled(bool flag);
    public static extern Result appletSetApplicationAlbumUserData(IntPtr buffer, ulong size);
    public static extern Result appletSaveCurrentScreenshot(AlbumReportOption option);
    public static extern Result appletGetAppletResourceUserIdOfCallerApplet(ref u64 @out);
    public static extern Result appletSetAppletWindowVisibility(bool flag);
    public static extern Result appletSetAppletGpuTimeSlice(s64 val);
    public static extern Result appletSetExpectedMasterVolume(float mainAppletVolume, float libraryAppletVolume);
    public static extern Result appletGetExpectedMasterVolume(ref float mainAppletVolume, ref float libraryAppletVolume);
    public static extern Result appletChangeMainAppletMasterVolume(float volume, u64 unk);
    public static extern Result appletSetTransparentVolumeRate(float val);
    public static extern Result appletUpdateLastForegroundCaptureImage();
    public static extern Result appletUpdateCallerAppletCaptureImage();
    public static extern Result appletGetLastForegroundCaptureImageEx(IntPtr buffer, ulong size, ref bool flag);
    public static extern Result appletGetLastApplicationCaptureImageEx(IntPtr buffer, ulong size, ref bool flag);
    public static extern Result appletGetCallerAppletCaptureImageEx(IntPtr buffer, ulong size, ref bool flag);
    public static extern Result appletTakeScreenShotOfOwnLayer(bool flag, AppletCaptureSharedBuffer captureBuf);
    public static extern Result appletCopyBetweenCaptureBuffers(AppletCaptureSharedBuffer dstCaptureBuf, AppletCaptureSharedBuffer srcCaptureBuf);
    public static extern Result appletClearCaptureBuffer(bool flag, AppletCaptureSharedBuffer captureBuf, u32 color);
    public static extern Result appletClearAppletTransitionBuffer(u32 color);
    public static extern Result appletAcquireLastApplicationCaptureSharedBuffer(ref bool flag, ref s32 id);
    public static extern Result appletReleaseLastApplicationCaptureSharedBuffer();
    public static extern Result appletAcquireLastForegroundCaptureSharedBuffer(ref bool flag, ref s32 id);
    public static extern Result appletReleaseLastForegroundCaptureSharedBuffer();
    public static extern Result appletAcquireCallerAppletCaptureSharedBuffer(ref bool flag, ref s32 id);
    public static extern Result appletReleaseCallerAppletCaptureSharedBuffer();
    public static extern Result appletTakeScreenShotOfOwnLayerEx(bool flag0, bool immediately, AppletCaptureSharedBuffer captureBuf);
    public static extern Result appletPushContext(ref AppletStorage s);
    public static extern Result appletPopContext(ref AppletStorage s);
    public static extern void appletLockAccessorClose(ref AppletLockAccessor a);
    public static extern Result appletLockAccessorTryLock(ref AppletLockAccessor a, ref bool flag);
    public static extern Result appletLockAccessorLock(ref AppletLockAccessor a);
    public static extern Result appletLockAccessorUnlock(ref AppletLockAccessor a);
    public static extern Result appletCreateLibraryApplet(ref AppletHolder h, AppletId id, LibAppletMode mode);
    public static extern Result appletCreateLibraryAppletSelf(ref AppletHolder h, AppletId id, LibAppletMode mode);
    public static extern Result appletTerminateAllLibraryApplets();
    public static extern Result appletAreAnyLibraryAppletsLeft(ref bool @out);
    public static extern void appletHolderClose(ref AppletHolder h);
    public static extern bool appletHolderActive(ref AppletHolder h);
    public static extern Result appletHolderGetIndirectLayerConsumerHandle(ref AppletHolder h, ref u64 @out);
    public static extern Result appletHolderStart(ref AppletHolder h);
    public static extern Result appletHolderJump(ref AppletHolder h);
    public static extern Result appletHolderRequestExit(ref AppletHolder h);
    public static extern Result appletHolderTerminate(ref AppletHolder h);
    public static extern Result appletHolderRequestExitOrTerminate(ref AppletHolder h, u64 timeout);
    public static extern void appletHolderJoin(ref AppletHolder h);
    public static extern bool appletHolderCheckFinished(ref AppletHolder h);
    public static extern LibAppletExitReason appletHolderGetExitReason(ref AppletHolder h);
    public static extern Result appletHolderSetOutOfFocusApplicationSuspendingEnabled(ref AppletHolder h, bool flag);
    public static extern Result appletHolderPresetLibraryAppletGpuTimeSliceZero(ref AppletHolder h);
    public static extern Result appletHolderGetPopInteractiveOutDataEvent(ref AppletHolder h, ref Event out_event);
    public static extern bool appletHolderWaitInteractiveOut(ref AppletHolder h);
    public static extern Result appletHolderPushInData(ref AppletHolder h, ref AppletStorage s);
    public static extern Result appletHolderPopOutData(ref AppletHolder h, ref AppletStorage s);
    public static extern Result appletHolderPushExtraStorage(ref AppletHolder h, ref AppletStorage s);
    public static extern Result appletHolderPushInteractiveInData(ref AppletHolder h, ref AppletStorage s);
    public static extern Result appletHolderPopInteractiveOutData(ref AppletHolder h, ref AppletStorage s);
    public static extern Result appletHolderGetLibraryAppletInfo(ref AppletHolder h, ref LibAppletInfo info);
    public static extern Result appletCreateStorage(ref AppletStorage s, s64 size);
    public static extern Result appletCreateTransferMemoryStorage(ref AppletStorage s, IntPtr buffer, s64 size, bool writable);
    public static extern Result appletCreateHandleStorage(ref AppletStorage s, s64 inval, Handle handle);
    public static extern Result appletCreateHandleStorageTmem(ref AppletStorage s, IntPtr buffer, s64 size);
    public static extern void appletStorageClose(ref AppletStorage s);
    public static extern void appletStorageCloseTmem(ref AppletStorage s);
    public static extern Result appletStorageGetSize(ref AppletStorage s, ref s64 size);
    public static extern Result appletStorageWrite(ref AppletStorage s, s64 offset, IntPtr buffer, ulong size);
    public static extern Result appletStorageRead(ref AppletStorage s, s64 offset, IntPtr buffer, ulong size);
    public static extern Result appletStorageGetHandle(ref AppletStorage s, ref s64 @out, ref Handle handle);
    public static extern Result appletStorageMap(ref AppletStorage s, IntPtr addr, ref ulong size);
    public static extern Result appletPopLaunchParameter(ref AppletStorage s, AppletLaunchParameterKind kind);
    public static extern Result appletRequestLaunchApplication(u64 application_id, ref AppletStorage s);
    public static extern Result appletRequestLaunchApplicationForQuest(u64 application_id, ref AppletStorage s, ref AppletApplicationAttributeForQuest attr);
    public static extern Result appletGetDesiredLanguage(ref u64 LanguageCode);
    public static extern Result appletGetDisplayVersion(ref byte displayVersion);
    public static extern Result appletBeginBlockingHomeButtonShortAndLongPressed(s64 val);
    public static extern Result appletEndBlockingHomeButtonShortAndLongPressed();
    public static extern Result appletBeginBlockingHomeButton(s64 val);
    public static extern Result appletEndBlockingHomeButton();
    public static extern void appletNotifyRunning(ref bool @out);
    public static extern Result appletGetPseudoDeviceId(ref Uuid @out);
    public static extern Result appletSetMediaPlaybackState(bool state);
    public static extern Result appletIsGamePlayRecordingSupported(ref bool flag);
    public static extern Result appletSetGamePlayRecordingState(bool state);
    public static extern Result appletInitializeGamePlayRecording();
    public static extern Result appletRequestFlushGamePlayingMovieForDebug();
    public static extern Result appletRequestToShutdown();
    public static extern Result appletRequestToReboot();
    public static extern Result appletRequestToSleep();
    public static extern Result appletExitAndRequestToShowThanksMessage();
    public static extern Result appletInitializeApplicationCopyrightFrameBuffer();
    public static extern Result appletSetApplicationCopyrightImage(IntPtr buffer, ulong size, s32 x, s32 y, s32 width, s32 height, AppletWindowOriginMode mode);
    public static extern Result appletSetApplicationCopyrightVisibility(bool visible);
    public static extern Result appletQueryApplicationPlayStatistics(ref PdmApplicationPlayStatistics stats, ref u64 application_ids, s32 count, ref s32 total_out);
    public static extern Result appletQueryApplicationPlayStatisticsByUid(AccountUid uid, ref PdmApplicationPlayStatistics stats, ref u64 application_ids, s32 count, ref s32 total_out);
    public static extern Result appletExecuteProgram(s32 programIndex, IntPtr buffer, ulong size);
    public static extern Result appletJumpToSubApplicationProgramForDevelopment(u64 application_id, IntPtr buffer, ulong size);
    public static extern Result appletRestartProgram(IntPtr buffer, ulong size);
    public static extern Result appletGetPreviousProgramIndex(ref s32 programIndex);
    public static extern Result appletSetDelayTimeToAbortOnGpuError(u64 val);
    public static extern Result appletGetFriendInvitationStorageChannelEvent(ref Event out_event);
    public static extern Result appletTryPopFromFriendInvitationStorageChannel(ref AppletStorage s);
    public static extern Result appletGetNotificationStorageChannelEvent(ref Event out_event);
    public static extern Result appletTryPopFromNotificationStorageChannel(ref AppletStorage s);
    public static extern Result appletGetHealthWarningDisappearedSystemEvent(ref Event out_event);
    public static extern Result appletSetHdcpAuthenticationActivated(bool flag);
    public static extern Result appletGetLastApplicationExitReason(ref s32 @out);
    public static extern Result appletCreateMovieMaker(ref Service srv_out, ref TransferMemory tmem);
    public static extern Result appletPrepareForJit();
    public static extern Result appletRequestToGetForeground();
    public static extern Result appletLockForeground();
    public static extern Result appletUnlockForeground();
    public static extern Result appletPopFromGeneralChannel(ref AppletStorage s);
    public static extern Result appletGetPopFromGeneralChannelEvent(ref Event out_event);
    public static extern Result appletGetHomeButtonWriterLockAccessor(ref AppletLockAccessor a);
    public static extern Result appletIsSleepEnabled(ref bool @out);
    public static extern Result appletPopRequestLaunchApplicationForDebug(ref AccountUid uids, s32 count, ref u64 application_id, ref s32 total_out);
    public static extern Result appletIsForceTerminateApplicationDisabledForDebug(ref bool @out);
    public static extern Result appletLaunchDevMenu();
    public static extern Result appletSetLastApplicationExitReason(s32 reason);
    public static extern Result appletStartSleepSequence(bool flag);
    public static extern Result appletStartShutdownSequence();
    public static extern Result appletStartRebootSequence();
    public static extern Result appletIsAutoPowerDownRequested(ref bool @out);
    public static extern Result appletLoadAndApplyIdlePolicySettings();
    public static extern Result appletNotifyCecSettingsChanged();
    public static extern Result appletSetDefaultHomeButtonLongPressTime(s64 val);
    public static extern Result appletUpdateDefaultDisplayResolution();
    public static extern Result appletShouldSleepOnBoot(ref bool @out);
    public static extern Result appletGetHdcpAuthenticationFailedEvent(ref Event out_event);
    public static extern Result appletCreateApplication(ref AppletApplication a, u64 application_id);
    public static extern Result appletPopLaunchRequestedApplication(ref AppletApplication a);
    public static extern Result appletCreateSystemApplication(ref AppletApplication a, u64 system_application_id);
    public static extern Result appletPopFloatingApplicationForDevelopment(ref AppletApplication a);
    public static extern void appletApplicationClose(ref AppletApplication a);
    public static extern bool appletApplicationActive(ref AppletApplication a);
    public static extern Result appletApplicationStart(ref AppletApplication a);
    public static extern Result appletApplicationRequestExit(ref AppletApplication a);
    public static extern Result appletApplicationTerminate(ref AppletApplication a);
    public static extern void appletApplicationJoin(ref AppletApplication a);
    public static extern bool appletApplicationCheckFinished(ref AppletApplication a);
    public static extern AppletApplicationExitReason appletApplicationGetExitReason(ref AppletApplication a);
    public static extern Result appletApplicationRequestForApplicationToGetForeground(ref AppletApplication a);
    public static extern Result appletApplicationTerminateAllLibraryApplets(ref AppletApplication a);
    public static extern Result appletApplicationAreAnyLibraryAppletsLeft(ref AppletApplication a, ref bool @out);
    public static extern Result appletApplicationRequestExitLibraryAppletOrTerminate(ref AppletApplication a, u64 timeout);
    public static extern Result appletApplicationGetApplicationId(ref AppletApplication a, ref u64 application_id);
    public static extern Result appletApplicationPushLaunchParameter(ref AppletApplication a, AppletLaunchParameterKind kind, ref AppletStorage s);
    public static extern Result appletApplicationGetApplicationControlProperty(ref AppletApplication a, ref NacpStruct nacp);
    public static extern Result appletApplicationGetApplicationLaunchProperty(ref AppletApplication a, ref AppletApplicationLaunchProperty @out);
    public static extern Result appletApplicationGetApplicationLaunchRequestInfo(ref AppletApplication a, ref AppletApplicationLaunchRequestInfo @out);
    public static extern Result appletApplicationSetUsers(ref AppletApplication a, ref AccountUid uids, s32 count, bool flag);
    public static extern Result appletApplicationCheckRightsEnvironmentAvailable(ref AppletApplication a, ref bool @out);
    public static extern Result appletApplicationGetNsRightsEnvironmentHandle(ref AppletApplication a, ref u64 handle);
    public static extern Result appletApplicationGetDesirableUids(ref AppletApplication a, ref AccountUid uids, s32 count, ref s32 total_out);
    public static extern Result appletApplicationReportApplicationExitTimeout(ref AppletApplication a);
    public static extern Result appletApplicationSetApplicationAttribute(ref AppletApplication a, ref AppletApplicationAttribute attr);
    public static extern Result appletApplicationHasSaveDataAccessPermission(ref AppletApplication a, u64 application_id, ref bool @out);
    public static extern Result appletApplicationPushToFriendInvitationStorageChannel(ref AppletApplication a, AccountUid uid, IntPtr buffer, u64 size);
    public static extern Result appletApplicationPushToNotificationStorageChannel(ref AppletApplication a, IntPtr buffer, u64 size);
    public static extern Result appletApplicationRequestApplicationSoftReset(ref AppletApplication a);
    public static extern Result appletApplicationRestartApplicationTimer(ref AppletApplication a);
    public static extern Result appletPopInData(ref AppletStorage s);
    public static extern Result appletPushOutData(ref AppletStorage s);
    public static extern Result appletPopInteractiveInData(ref AppletStorage s);
    public static extern Result appletPushInteractiveOutData(ref AppletStorage s);
    public static extern Result appletGetPopInDataEvent(ref Event out_event);
    public static extern Result appletGetPopInteractiveInDataEvent(ref Event out_event);
    public static extern Result appletGetLibraryAppletInfo(ref LibAppletInfo info);
    public static extern Result appletGetMainAppletIdentityInfo(ref AppletIdentityInfo info);
    public static extern Result appletCanUseApplicationCore(ref bool @out);
    public static extern Result appletGetCallerAppletIdentityInfo(ref AppletIdentityInfo info);
    public static extern Result appletGetMainAppletApplicationControlProperty(ref NacpStruct nacp);
    public static extern Result appletGetMainAppletStorageId(ref NcmStorageId storageId);
    public static extern Result appletGetCallerAppletIdentityInfoStack(ref AppletIdentityInfo stack, s32 count, ref s32 total_out);
    public static extern Result appletGetNextReturnDestinationAppletIdentityInfo(ref AppletIdentityInfo info);
    public static extern Result appletGetDesirableKeyboardLayout(ref SetKeyboardLayout layout);
    public static extern Result appletPopExtraStorage(ref AppletStorage s);
    public static extern Result appletGetPopExtraStorageEvent(ref Event out_event);
    public static extern Result appletUnpopInData(ref AppletStorage s);
    public static extern Result appletUnpopExtraStorage(ref AppletStorage s);
    public static extern Result appletGetIndirectLayerProducerHandle(ref u64 @out);
    public static extern Result appletGetMainAppletApplicationDesiredLanguage(ref u64 LanguageCode);
    public static extern Result appletGetCurrentApplicationId(ref u64 application_id);
    public static extern Result appletRequestExitToSelf();
    public static extern Result appletCreateGameMovieTrimmer(ref Service srv_out, ref TransferMemory tmem);
    public static extern Result appletReserveResourceForMovieOperation();
    public static extern Result appletUnreserveResourceForMovieOperation();
    public static extern Result appletGetMainAppletAvailableUsers(ref AccountUid uids, s32 count, ref bool flag, ref s32 total_out);
    public static extern Result appletSetApplicationMemoryReservation(u64 val);
    public static extern Result appletShouldSetGpuTimeSliceManually(ref bool @out);
    public static extern Result appletBeginToWatchShortHomeButtonMessage();
    public static extern Result appletEndToWatchShortHomeButtonMessage();
    public static extern Result appletGetApplicationIdForLogo(ref u64 application_id);
    public static extern Result appletSetGpuTimeSliceBoost(u64 val);
    public static extern Result appletSetAutoSleepTimeAndDimmingTimeEnabled(bool flag);
    public static extern Result appletTerminateApplicationAndSetReason(Result reason);
    public static extern Result appletSetScreenShotPermissionGlobally(bool flag);
    public static extern Result appletStartShutdownSequenceForOverlay();
    public static extern Result appletStartRebootSequenceForOverlay();
    public static extern Result appletSetHealthWarningShowingState(bool flag);
    public static extern Result appletIsHealthWarningRequired(ref bool @out);
    public static extern Result appletBeginToObserveHidInputForDevelop();
    public static extern Result appletReadThemeStorage(IntPtr buffer, ulong size, u64 offset, ref u64 transfer_size);
    public static extern Result appletWriteThemeStorage(IntPtr buffer, ulong size, u64 offset);
    public static extern Result appletPushToAppletBoundChannel(ref AppletStorage s);
    public static extern Result appletTryPopFromAppletBoundChannel(ref AppletStorage s);
    public static extern Result appletGetDisplayLogicalResolution(ref s32 width, ref s32 height);
    public static extern Result appletSetDisplayMagnification(float x, float y, float width, float height);
    public static extern Result appletSetHomeButtonDoubleClickEnabled(bool flag);
    public static extern Result appletGetHomeButtonDoubleClickEnabled(ref bool @out);
    public static extern Result appletIsHomeButtonShortPressedBlocked(ref bool @out);
    public static extern Result appletIsVrModeCurtainRequired(ref bool @out);
    public static extern Result appletSetCpuBoostRequestPriority(s32 priority);
    public static extern Result appletOpenMainApplication(ref AppletApplication a);
    public static extern Result appletPerformSystemButtonPressing(AppletSystemButtonType type);
    public static extern Result appletInvalidateTransitionLayer();
    public static extern Result appletRequestLaunchApplicationWithUserAndArgumentForDebug(u64 application_id, ref AccountUid uids, s32 total_uids, bool flag, IntPtr buffer, ulong size);
    public static extern Result appletGetAppletResourceUsageInfo(ref AppletResourceUsageInfo info);
    public static extern Result appletPushToAppletBoundChannelForDebug(ref AppletStorage s, s32 channel);
    public static extern Result appletTryPopFromAppletBoundChannelForDebug(ref AppletStorage s, s32 channel);
    public static extern Result appletAlarmSettingNotificationEnableAppEventReserve(ref AppletStorage s, u64 application_id);
    public static extern Result appletAlarmSettingNotificationDisableAppEventReserve();
    public static extern Result appletAlarmSettingNotificationPushAppEventNotify(IntPtr buffer, u64 size);
    public static extern Result appletFriendInvitationSetApplicationParameter(ref AppletStorage s, u64 application_id);
    public static extern Result appletFriendInvitationClearApplicationParameter();
    public static extern Result appletFriendInvitationPushApplicationParameter(AccountUid uid, IntPtr buffer, u64 size);
    public static extern Result appletSetTerminateResult(Result res);
    public static extern Result appletGetLaunchStorageInfoForDebug(ref NcmStorageId app_storageId, ref NcmStorageId update_storageId);
    public static extern Result appletGetGpuErrorDetectedSystemEvent(ref Event out_event);
    public static extern Result appletSetHandlingHomeButtonShortPressedEnabled(bool flag);
    public static extern Result appletGetMessage(ref u32 msg);
    public static extern bool appletProcessMessage(u32 msg);
    public static extern bool appletMainLoop();
    public static extern void appletHook(ref AppletHookCookie cookie, AppletHookFn callback, IntPtr param);
    public static extern void appletUnhook(ref AppletHookCookie cookie);
    public static extern AppletOperationMode appletGetOperationMode();
    public static extern ApmPerformanceMode appletGetPerformanceMode();
    public static extern AppletFocusState appletGetFocusState();
    public static extern Result appletSetFocusHandlingMode(AppletFocusHandlingMode mode);
}
