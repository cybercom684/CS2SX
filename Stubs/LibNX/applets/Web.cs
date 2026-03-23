// Auto-generated from applets\web.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Applets;

public enum WebShimKind
{
    WebShimKind_Shop = 1,
    WebShimKind_Login = 2,
    WebShimKind_Offline = 3,
    WebShimKind_Share = 4,
    WebShimKind_Web = 5,
    WebShimKind_Wifi = 6,
    WebShimKind_Lobby = 7,
}

public enum WebExitReason
{
    WebExitReason_ExitButton = 0x0,
    WebExitReason_BackButton = 0x1,
    WebExitReason_Requested = 0x2,
    WebExitReason_LastUrl = 0x3,
    WebExitReason_ErrorDialog = 0x7,
    WebExitReason_UnknownE = 0xE,
}

public enum WebFooterButtonId
{
    WebFooterButtonId_None = 0,
    WebFooterButtonId_Type1 = 1,
    WebFooterButtonId_Type2 = 2,
    WebFooterButtonId_Type3 = 3,
    WebFooterButtonId_Type4 = 4,
    WebFooterButtonId_Type5 = 5,
    WebFooterButtonId_Type6 = 6,
    WebFooterButtonId_Max,
}

public enum WebSessionBootMode
{
    WebSessionBootMode_AllForeground = 0,
    WebSessionBootMode_AllForegroundInitiallyHidden = 1,
}

public enum WebSessionSendMessageKind
{
    WebSessionSendMessageKind_BrowserEngineContent = 0x0,
    WebSessionSendMessageKind_SystemMessageAppear = 0x100,
    WebSessionSendMessageKind_Ack = 0x1000,
}

public enum WebSessionReceiveMessageKind
{
    WebSessionReceiveMessageKind_BrowserEngineContent = 0x0,
    WebSessionReceiveMessageKind_AckBrowserEngine = 0x1000,
    WebSessionReceiveMessageKind_AckSystemMessage = 0x1001,
}

public enum WebArgType
{
    WebArgType_Url = 0x1,
    WebArgType_CallbackUrl = 0x3,
    WebArgType_CallbackableUrl = 0x4,
    WebArgType_ApplicationId = 0x5,
    WebArgType_DocumentPath = 0x6,
    WebArgType_DocumentKind = 0x7,
    WebArgType_SystemDataId = 0x8,
    WebArgType_ShareStartPage = 0x9,
    WebArgType_Whitelist = 0xA,
    WebArgType_NewsFlag = 0xB,
    WebArgType_UnknownC = 0xC,
    WebArgType_UnknownD = 0xD,
    WebArgType_Uid = 0xE,
    WebArgType_AlbumEntry0 = 0xF,
    WebArgType_ScreenShot = 0x10,
    WebArgType_EcClientCert = 0x11,
    WebArgType_Unknown12 = 0x12,
    WebArgType_PlayReport = 0x13,
    WebArgType_Unknown14 = 0x14,
    WebArgType_Unknown15 = 0x15,
    WebArgType_BootDisplayKind = 0x17,
    WebArgType_BackgroundKind = 0x18,
    WebArgType_Footer = 0x19,
    WebArgType_Pointer = 0x1A,
    WebArgType_LeftStickMode = 0x1B,
    WebArgType_KeyRepeatFrame0 = 0x1C,
    WebArgType_KeyRepeatFrame1 = 0x1D,
    WebArgType_BootAsMediaPlayerInverted = 0x1E,
    WebArgType_DisplayUrlKind = 0x1F,
    WebArgType_BootAsMediaPlayer = 0x21,
    WebArgType_ShopJump = 0x22,
    WebArgType_MediaPlayerUserGestureRestriction = 0x23,
    WebArgType_MediaAutoPlay = 0x23,
    WebArgType_LobbyParameter = 0x24,
    WebArgType_ApplicationAlbumEntry = 0x26,
    WebArgType_JsExtension = 0x27,
    WebArgType_AdditionalCommentText = 0x28,
    WebArgType_TouchEnabledOnContents = 0x29,
    WebArgType_UserAgentAdditionalString = 0x2A,
    WebArgType_AdditionalMediaData0 = 0x2B,
    WebArgType_MediaPlayerAutoClose = 0x2C,
    WebArgType_PageCache = 0x2D,
    WebArgType_WebAudio = 0x2E,
    WebArgType_2F = 0x2F,
    WebArgType_YouTubeVideoFlag = 0x31,
    WebArgType_FooterFixedKind = 0x32,
    WebArgType_PageFade = 0x33,
    WebArgType_MediaCreatorApplicationRatingAge = 0x34,
    WebArgType_BootLoadingIcon = 0x35,
    WebArgType_PageScrollIndicator = 0x36,
    WebArgType_MediaPlayerSpeedControl = 0x37,
    WebArgType_AlbumEntry1 = 0x38,
    WebArgType_AlbumEntry2 = 0x39,
    WebArgType_AlbumEntry3 = 0x3A,
    WebArgType_AdditionalMediaData1 = 0x3B,
    WebArgType_AdditionalMediaData2 = 0x3C,
    WebArgType_AdditionalMediaData3 = 0x3D,
    WebArgType_BootFooterButton = 0x3E,
    WebArgType_OverrideWebAudioVolume = 0x3F,
    WebArgType_OverrideMediaAudioVolume = 0x40,
    WebArgType_SessionBootMode = 0x41,
    WebArgType_SessionFlag = 0x42,
    WebArgType_MediaPlayerUi = 0x43,
    WebArgType_TransferMemory = 0x44,
}

public enum WebReplyType
{
    WebReplyType_ExitReason = 0x1,
    WebReplyType_LastUrl = 0x2,
    WebReplyType_LastUrlSize = 0x3,
    WebReplyType_SharePostResult = 0x4,
    WebReplyType_PostServiceName = 0x5,
    WebReplyType_PostServiceNameSize = 0x6,
    WebReplyType_PostId = 0x7,
    WebReplyType_PostIdSize = 0x8,
    WebReplyType_MediaPlayerAutoClosedByCompletion = 0x9,
}

public enum WebDocumentKind
{
    WebDocumentKind_OfflineHtmlPage = 0x1,
    WebDocumentKind_ApplicationLegalInformation = 0x2,
    WebDocumentKind_SystemDataPage = 0x3,
}

public enum WebShareStartPage
{
    WebShareStartPage_Default = 0,
    WebShareStartPage_Settings = 1,
}

public enum WebBootDisplayKind
{
    WebBootDisplayKind_Default = 0,
    WebBootDisplayKind_White = 1,
    WebBootDisplayKind_Black = 2,
    WebBootDisplayKind_Unknown3 = 3,
    WebBootDisplayKind_Unknown4 = 4,
}

public enum WebBackgroundKind
{
    WebBackgroundKind_Default = 0,
    WebBackgroundKind_Unknown1 = 1,
    WebBackgroundKind_Unknown2 = 2,
}

public enum WebLeftStickMode
{
    WebLeftStickMode_Pointer = 0,
    WebLeftStickMode_Cursor = 1,
}

public enum WebFooterFixedKind
{
    WebFooterFixedKind_Default = 0,
    WebFooterFixedKind_Always = 1,
    WebFooterFixedKind_Hidden = 2,
}

public unsafe struct WebWifiPageArg
{
    public u32 unk_x0;
    public byte conntest_url;
    public byte initial_url;
    public Uuid uuid;
    public u32 rev;
}

public unsafe struct WebWifiReturnValue
{
    public u32 unk_x0;
    public Result res;
}

public unsafe struct WebWifiConfig
{
    public WebWifiPageArg arg;
}

public unsafe struct WebCommonTLVStorage
{
    public u8 data;
}

public unsafe struct WebCommonConfig
{
    public WebCommonTLVStorage arg;
    public AppletId appletid;
    public u32 version;
    public AppletHolder holder;
}

public unsafe struct WebCommonReply
{
    public bool type;
    public WebShimKind shimKind;
    public WebCommonReturnValue ret;
    public WebCommonTLVStorage storage;
}

public unsafe struct WebSessionStorageHandleQueue
{
    public s32 read_pos;
    public s32 write_pos;
    public s32 max_storages;
    public bool is_full;
    public AppletStorage storages;
}

public unsafe struct WebSessionMessageHeader
{
    public u32 kind;
    public u32 size;
    public u8 reserved;
}

public static class Web
{
    public static extern void webWifiCreate(ref WebWifiConfig config, ref byte conntest_url, ref byte initial_url, Uuid uuid, u32 rev);
    public static extern Result webWifiShow(ref WebWifiConfig config, ref WebWifiReturnValue @out);
    public static extern Result webPageCreate(ref WebCommonConfig config, ref byte url);
    public static extern Result webNewsCreate(ref WebCommonConfig config, ref byte url);
    public static extern Result webYouTubeVideoCreate(ref WebCommonConfig config, ref byte url);
    public static extern Result webOfflineCreate(ref WebCommonConfig config, WebDocumentKind docKind, u64 id, ref byte docPath);
    public static extern Result webShareCreate(ref WebCommonConfig config, WebShareStartPage page);
    public static extern Result webLobbyCreate(ref WebCommonConfig config);
    public static extern Result webConfigSetCallbackUrl(ref WebCommonConfig config, ref byte url);
    public static extern Result webConfigSetCallbackableUrl(ref WebCommonConfig config, ref byte url);
    public static extern Result webConfigSetWhitelist(ref WebCommonConfig config, ref byte whitelist);
    public static extern Result webConfigSetUid(ref WebCommonConfig config, AccountUid uid);
    public static extern Result webConfigSetAlbumEntry(ref WebCommonConfig config, ref CapsAlbumEntry entry);
    public static extern Result webConfigSetScreenShot(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigSetEcClientCert(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigSetPlayReport(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigSetBootDisplayKind(ref WebCommonConfig config, WebBootDisplayKind kind);
    public static extern Result webConfigSetBackgroundKind(ref WebCommonConfig config, WebBackgroundKind kind);
    public static extern Result webConfigSetFooter(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigSetPointer(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigSetLeftStickMode(ref WebCommonConfig config, WebLeftStickMode mode);
    public static extern Result webConfigSetKeyRepeatFrame(ref WebCommonConfig config, s32 inval0, s32 inval1);
    public static extern Result webConfigSetDisplayUrlKind(ref WebCommonConfig config, bool kind);
    public static extern Result webConfigSetBootAsMediaPlayer(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigSetShopJump(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigSetMediaPlayerUserGestureRestriction(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigSetMediaAutoPlay(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigSetLobbyParameter(ref WebCommonConfig config, ref byte str);
    public static extern Result webConfigSetApplicationAlbumEntry(ref WebCommonConfig config, ref CapsApplicationAlbumEntry entry);
    public static extern Result webConfigSetJsExtension(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigSetAdditionalCommentText(ref WebCommonConfig config, ref byte str);
    public static extern Result webConfigSetTouchEnabledOnContents(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigSetUserAgentAdditionalString(ref WebCommonConfig config, ref byte str);
    public static extern Result webConfigSetAdditionalMediaData(ref WebCommonConfig config, ref u8 data, ulong size);
    public static extern Result webConfigSetMediaPlayerAutoClose(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigSetPageCache(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigSetWebAudio(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigSetFooterFixedKind(ref WebCommonConfig config, WebFooterFixedKind kind);
    public static extern Result webConfigSetPageFade(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigSetMediaCreatorApplicationRatingAge(ref WebCommonConfig config, ref s8 data);
    public static extern Result webConfigSetBootLoadingIcon(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigSetPageScrollIndicator(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigSetMediaPlayerSpeedControl(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigAddAlbumEntryAndMediaData(ref WebCommonConfig config, ref CapsAlbumEntry entry, ref u8 data, ulong size);
    public static extern Result webConfigSetBootFooterButtonVisible(ref WebCommonConfig config, WebFooterButtonId button, bool visible);
    public static extern Result webConfigSetOverrideWebAudioVolume(ref WebCommonConfig config, float value);
    public static extern Result webConfigSetOverrideMediaAudioVolume(ref WebCommonConfig config, float value);
    public static extern Result webConfigSetBootMode(ref WebCommonConfig config, WebSessionBootMode mode);
    public static extern Result webConfigSetMediaPlayerUi(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigSetTransferMemory(ref WebCommonConfig config, bool flag);
    public static extern Result webConfigShow(ref WebCommonConfig config, ref WebCommonReply @out);
    public static extern Result webConfigRequestExit(ref WebCommonConfig config);
    public static extern Result webReplyGetExitReason(ref WebCommonReply reply, ref WebExitReason exitReason);
    public static extern Result webReplyGetLastUrl(ref WebCommonReply reply, ref byte outstr, ulong outstr_maxsize, ref ulong out_size);
    public static extern Result webReplyGetSharePostResult(ref WebCommonReply reply, ref u32 sharePostResult);
    public static extern Result webReplyGetPostServiceName(ref WebCommonReply reply, ref byte outstr, ulong outstr_maxsize, ref ulong out_size);
    public static extern Result webReplyGetPostId(ref WebCommonReply reply, ref byte outstr, ulong outstr_maxsize, ref ulong out_size);
    public static extern Result webReplyGetMediaPlayerAutoClosedByCompletion(ref WebCommonReply reply, ref bool flag);
    public static extern void webSessionCreate(ref WebSession s, ref WebCommonConfig config);
    public static extern void webSessionClose(ref WebSession s);
    public static extern Result webSessionStart(ref WebSession s, ref Event out_event);
    public static extern Result webSessionWaitForExit(ref WebSession s, ref WebCommonReply @out);
    public static extern Result webSessionRequestExit(ref WebSession s);
    public static extern Result webSessionAppear(ref WebSession s, ref bool flag);
    public static extern Result webSessionTrySendContentMessage(ref WebSession s, ref byte content, u32 size, ref bool flag);
    public static extern Result webSessionTryReceiveContentMessage(ref WebSession s, ref byte content, u64 size, ref u64 out_size, ref bool flag);
}
