// Auto-generated from applets\swkbd.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Applets;

public enum SwkbdTextCheckResult
{
    SwkbdTextCheckResult_OK = 0,
    SwkbdTextCheckResult_Bad = 1,
    SwkbdTextCheckResult_Prompt = 2,
    SwkbdTextCheckResult_Silent = 3,
}

public enum SwkbdType
{
    SwkbdType_Normal = 0,
    SwkbdType_NumPad = 1,
    SwkbdType_QWERTY = 2,
    SwkbdType_Unknown3 = 3,
    SwkbdType_Latin = 4,
    SwkbdType_ZhHans = 5,
    SwkbdType_ZhHant = 6,
    SwkbdType_Korean = 7,
    SwkbdType_All = 8,
    SwkbdType_Unknown9 = 9,
}

public enum SwkbdTextDrawType
{
    SwkbdTextDrawType_Line = 0,
    SwkbdTextDrawType_Box = 1,
    SwkbdTextDrawType_DownloadCode = 2,
}

public enum SwkbdRequestCommand
{
    SwkbdRequestCommand_Finalize = 0x4,
    SwkbdRequestCommand_SetUserWordInfo = 0x6,
    SwkbdRequestCommand_SetCustomizeDic = 0x7,
    SwkbdRequestCommand_Calc = 0xA,
    SwkbdRequestCommand_SetCustomizedDictionaries = 0xB,
    SwkbdRequestCommand_UnsetCustomizedDictionaries = 0xC,
    SwkbdRequestCommand_SetChangedStringV2Flag = 0xD,
    SwkbdRequestCommand_SetMovedCursorV2Flag = 0xE,
}

public enum SwkbdReplyType
{
    SwkbdReplyType_FinishedInitialize = 0x0,
    SwkbdReplyType_ChangedString = 0x2,
    SwkbdReplyType_MovedCursor = 0x3,
    SwkbdReplyType_MovedTab = 0x4,
    SwkbdReplyType_DecidedEnter = 0x5,
    SwkbdReplyType_DecidedCancel = 0x6,
    SwkbdReplyType_ChangedStringUtf8 = 0x7,
    SwkbdReplyType_MovedCursorUtf8 = 0x8,
    SwkbdReplyType_DecidedEnterUtf8 = 0x9,
    SwkbdReplyType_UnsetCustomizeDic = 0xA,
    SwkbdReplyType_ReleasedUserWordInfo = 0xB,
    SwkbdReplyType_UnsetCustomizedDictionaries = 0xC,
    SwkbdReplyType_ChangedStringV2 = 0xD,
    SwkbdReplyType_MovedCursorV2 = 0xE,
    SwkbdReplyType_ChangedStringUtf8V2 = 0xF,
    SwkbdReplyType_MovedCursorUtf8V2 = 0x10,
}

public enum SwkbdState
{
    SwkbdState_Inactive = 0x0,
    SwkbdState_Initialized = 0x1,
    SwkbdState_Appearing = 0x2,
    SwkbdState_Shown = 0x3,
    SwkbdState_Disappearing = 0x4,
    SwkbdState_Unknown5 = 0x5,
    SwkbdState_Unknown6 = 0x6,
}

public enum SwkbdInlineMode
{
    SwkbdInlineMode_UserDisplay = 0,
    SwkbdInlineMode_AppletDisplay = 1,
}

public unsafe struct SwkbdDictWord
{
    public u8 unk_x0;
}

public unsafe struct SwkbdCustomizeDicInfo
{
    public u8 unk_x0;
}

public unsafe struct SwkbdArgCommon
{
    public SwkbdType type;
    public u16 okButtonText;
    public u16 leftButtonText;
    public u16 rightButtonText;
    public u8 dicFlag;
    public u8 pad_x1b;
    public u32 keySetDisableBitmask;
    public u32 initialCursorPos;
    public u16 headerText;
    public u16 subText;
    public u16 guideText;
    public u16 pad_x3aa;
    public u32 stringLenMax;
    public u32 stringLenMin;
    public u32 passwordFlag;
    public SwkbdTextDrawType textDrawType;
    public u16 returnButtonFlag;
    public u8 blurBackground;
    public u8 pad_x3bf;
    public u32 initialStringOffset;
    public u32 initialStringSize;
    public u32 userDicOffset;
    public s32 userDicEntries;
    public u8 textCheckFlag;
}

public unsafe struct SwkbdArgV0
{
    public SwkbdArgCommon arg;
    public fixed u8 pad_x3d1[7];
    public SwkbdTextCheckCb textCheckCb;
}

public unsafe struct SwkbdArgV7
{
    public SwkbdArgV0 arg;
    public fixed u32 textGrouping[8];
}

public unsafe struct SwkbdArgVB
{
    public SwkbdArgCommon arg;
    public fixed u8 pad_x3d1[3];
    public fixed u32 textGrouping[8];
    public u64 entries;
    public u8 total_entries;
    public u8 unkFlag;
    public u8 pad_x4b6;
    public u8 trigger;
    public fixed u8 pad_x4c4[4];
}

public unsafe struct SwkbdConfig
{
    public SwkbdArgV7 arg;
    public IntPtr workbuf;
    public ulong workbuf_size;
    public s32 max_dictwords;
    public SwkbdCustomizedDictionarySet customizedDictionarySet;
    public u8 unkFlag;
    public u8 trigger;
    public u32 version;
}

public unsafe struct SwkbdRect
{
    public s16 x;
    public s16 y;
    public s16 width;
    public s16 height;
}

public unsafe struct SwkbdInitializeArg
{
    public u32 unk_x0;
    public u8 mode;
    public u8 unk_x5;
    public fixed u8 pad[2];
}

public unsafe struct SwkbdAppearArg
{
    public SwkbdType type;
    public fixed u16 okButtonText[9];
    public u16 leftButtonText;
    public u16 rightButtonText;
    public u8 dicFlag;
    public u8 unk_x1b;
    public u32 keySetDisableBitmask;
    public s32 stringLenMax;
    public s32 stringLenMin;
    public u8 returnButtonFlag;
    public u8 unk_x29;
    public u8 unk_x2a;
    public u8 unk_x2b;
    public u32 flags;
    public u8 unk_x30;
    public u8 unk_x31;
}

public unsafe struct SwkbdInlineCalcArg
{
    public u32 unk_x0;
    public u16 size;
    public u8 unk_x6;
    public u8 unk_x7;
    public u64 flags;
    public SwkbdInitializeArg initArg;
    public float volume;
    public s32 cursorPos;
    public SwkbdAppearArg appearArg;
    public u16 inputText;
    public u8 utf8Mode;
    public u8 unk_x45d;
    public u8 enableBackspace;
    public fixed u8 unk_x45f[3];
    public u8 keytopAsFloating;
    public u8 footerScalable;
    public u8 alphaEnabledInInputMode;
    public u8 inputModeFadeType;
    public u8 disableTouch;
    public u8 disableHardwareKeyboard;
    public fixed u8 unk_x468[5];
    public u8 unk_x46d;
    public u8 unk_x46e;
    public u8 unk_x46f;
    public float keytopScaleX;
    public float keytopScaleY;
    public float keytopTranslateX;
    public float keytopTranslateY;
    public float keytopBgAlpha;
    public float footerBgAlpha;
    public float balloonScale;
    public float unk_x48c;
    public u8 unk_x490;
    public u8 seGroup;
    public u8 triggerFlag;
    public u8 trigger;
    public u8 pad_x49f;
}

public unsafe struct SwkbdChangedStringArg
{
    public u32 stringLen;
    public s32 dicStartCursorPos;
    public s32 dicEndCursorPos;
    public s32 cursorPos;
}

public unsafe struct SwkbdMovedCursorArg
{
    public u32 stringLen;
    public s32 cursorPos;
}

public unsafe struct SwkbdMovedTabArg
{
    public u32 unk_x0;
    public u32 unk_x4;
}

public unsafe struct SwkbdDecidedEnterArg
{
    public u32 stringLen;
}

public unsafe struct SwkbdInline
{
    public u32 version;
    public AppletHolder holder;
    public SwkbdInlineCalcArg calcArg;
    public bool directionalButtonAssignFlag;
    public SwkbdState state;
    public bool dicCustomInitialized;
    public bool customizedDictionariesInitialized;
    public AppletStorage dicStorage;
    public bool wordInfoInitialized;
    public AppletStorage wordInfoStorage;
    public IntPtr interactive_tmpbuf;
    public ulong interactive_tmpbuf_size;
    public IntPtr interactive_strbuf;
    public ulong interactive_strbuf_size;
    public VoidFn finishedInitializeCb;
    public VoidFn decidedCancelCb;
    public SwkbdChangedStringCb changedStringCb;
    public SwkbdChangedStringV2Cb changedStringV2Cb;
    public SwkbdMovedCursorCb movedCursorCb;
    public SwkbdMovedCursorV2Cb movedCursorV2Cb;
    public SwkbdMovedTabCb movedTabCb;
    public SwkbdDecidedEnterCb decidedEnterCb;
    public VoidFn releasedUserWordInfoCb;
}

public static class Swkbd
{
    public static extern Result swkbdCreate(ref SwkbdConfig c, s32 max_dictwords);
    public static extern void swkbdClose(ref SwkbdConfig c);
    public static extern void swkbdConfigMakePresetDefault(ref SwkbdConfig c);
    public static extern void swkbdConfigMakePresetPassword(ref SwkbdConfig c);
    public static extern void swkbdConfigMakePresetUserName(ref SwkbdConfig c);
    public static extern void swkbdConfigMakePresetDownloadCode(ref SwkbdConfig c);
    public static extern void swkbdConfigSetOkButtonText(ref SwkbdConfig c, ref byte str);
    public static extern void swkbdConfigSetLeftOptionalSymbolKey(ref SwkbdConfig c, ref byte str);
    public static extern void swkbdConfigSetRightOptionalSymbolKey(ref SwkbdConfig c, ref byte str);
    public static extern void swkbdConfigSetHeaderText(ref SwkbdConfig c, ref byte str);
    public static extern void swkbdConfigSetSubText(ref SwkbdConfig c, ref byte str);
    public static extern void swkbdConfigSetGuideText(ref SwkbdConfig c, ref byte str);
    public static extern void swkbdConfigSetInitialText(ref SwkbdConfig c, ref byte str);
    public static extern void swkbdConfigSetDictionary(ref SwkbdConfig c, ref SwkbdDictWord input, s32 entries);
    public static extern Result swkbdConfigSetCustomizedDictionaries(ref SwkbdConfig c, ref SwkbdCustomizedDictionarySet dic);
    public static extern void swkbdConfigSetTextCheckCallback(ref SwkbdConfig c, SwkbdTextCheckCb cb);
    public static extern Result swkbdShow(ref SwkbdConfig c, ref byte out_string, ulong out_string_size);
    public static extern Result swkbdInlineCreate(ref SwkbdInline s);
    public static extern Result swkbdInlineClose(ref SwkbdInline s);
    public static extern Result swkbdInlineLaunch(ref SwkbdInline s);
    public static extern Result swkbdInlineLaunchForLibraryApplet(ref SwkbdInline s, u8 mode, u8 unk_x5);
    public static extern Result swkbdInlineGetImageMemoryRequirement(ref u64 out_size, ref u64 out_alignment);
    public static extern Result swkbdInlineGetImage(ref SwkbdInline s, IntPtr buffer, u64 size, ref bool data_available);
    public static extern s32 swkbdInlineGetMaxHeight(ref SwkbdInline s);
    public static extern s32 swkbdInlineGetMiniaturizedHeight(ref SwkbdInline s);
    public static extern s32 swkbdInlineGetTouchRectangles(ref SwkbdInline s, ref SwkbdRect keytop, ref SwkbdRect footer);
    public static extern bool swkbdInlineIsUsedTouchPointByKeyboard(ref SwkbdInline s, s32 x, s32 y);
    public static extern Result swkbdInlineUpdate(ref SwkbdInline s, ref SwkbdState out_state);
    public static extern void swkbdInlineSetFinishedInitializeCallback(ref SwkbdInline s, VoidFn cb);
    public static extern void swkbdInlineSetDecidedCancelCallback(ref SwkbdInline s, VoidFn cb);
    public static extern void swkbdInlineSetChangedStringCallback(ref SwkbdInline s, SwkbdChangedStringCb cb);
    public static extern void swkbdInlineSetChangedStringV2Callback(ref SwkbdInline s, SwkbdChangedStringV2Cb cb);
    public static extern void swkbdInlineSetMovedCursorCallback(ref SwkbdInline s, SwkbdMovedCursorCb cb);
    public static extern void swkbdInlineSetMovedCursorV2Callback(ref SwkbdInline s, SwkbdMovedCursorV2Cb cb);
    public static extern void swkbdInlineSetMovedTabCallback(ref SwkbdInline s, SwkbdMovedTabCb cb);
    public static extern void swkbdInlineSetDecidedEnterCallback(ref SwkbdInline s, SwkbdDecidedEnterCb cb);
    public static extern void swkbdInlineSetReleasedUserWordInfoCallback(ref SwkbdInline s, VoidFn cb);
    public static extern void swkbdInlineAppear(ref SwkbdInline s, ref SwkbdAppearArg arg);
    public static extern void swkbdInlineAppearEx(ref SwkbdInline s, ref SwkbdAppearArg arg, u8 trigger);
    public static extern void swkbdInlineDisappear(ref SwkbdInline s);
    public static extern void swkbdInlineMakeAppearArg(ref SwkbdAppearArg arg, SwkbdType type);
    public static extern void swkbdInlineAppearArgSetOkButtonText(ref SwkbdAppearArg arg, ref byte str);
    public static extern void swkbdInlineAppearArgSetLeftButtonText(ref SwkbdAppearArg arg, ref byte str);
    public static extern void swkbdInlineAppearArgSetRightButtonText(ref SwkbdAppearArg arg, ref byte str);
    public static extern void swkbdInlineSetVolume(ref SwkbdInline s, float volume);
    public static extern void swkbdInlineSetInputText(ref SwkbdInline s, ref byte str);
    public static extern void swkbdInlineSetCursorPos(ref SwkbdInline s, s32 pos);
    public static extern Result swkbdInlineSetUserWordInfo(ref SwkbdInline s, ref SwkbdDictWord input, s32 entries);
    public static extern Result swkbdInlineUnsetUserWordInfo(ref SwkbdInline s);
    public static extern void swkbdInlineSetUtf8Mode(ref SwkbdInline s, bool flag);
    public static extern Result swkbdInlineSetCustomizeDic(ref SwkbdInline s, IntPtr buffer, ulong size, ref SwkbdCustomizeDicInfo info);
    public static extern void swkbdInlineUnsetCustomizeDic(ref SwkbdInline s);
    public static extern Result swkbdInlineSetCustomizedDictionaries(ref SwkbdInline s, ref SwkbdCustomizedDictionarySet dic);
    public static extern Result swkbdInlineUnsetCustomizedDictionaries(ref SwkbdInline s);
    public static extern void swkbdInlineSetInputModeFadeType(ref SwkbdInline s, u8 type);
    public static extern void swkbdInlineSetAlphaEnabledInInputMode(ref SwkbdInline s, bool flag);
    public static extern void swkbdInlineSetKeytopBgAlpha(ref SwkbdInline s, float alpha);
    public static extern void swkbdInlineSetFooterBgAlpha(ref SwkbdInline s, float alpha);
    public static extern void swkbdInlineSetKeytopScale(ref SwkbdInline s, float scale);
    public static extern void swkbdInlineSetKeytopTranslate(ref SwkbdInline s, float x, float y);
    public static extern void swkbdInlineSetKeytopAsFloating(ref SwkbdInline s, bool flag);
    public static extern void swkbdInlineSetFooterScalable(ref SwkbdInline s, bool flag);
    public static extern void swkbdInlineSetTouchFlag(ref SwkbdInline s, bool flag);
    public static extern void swkbdInlineSetHardwareKeyboardFlag(ref SwkbdInline s, bool flag);
    public static extern void swkbdInlineSetDirectionalButtonAssignFlag(ref SwkbdInline s, bool flag);
    public static extern void swkbdInlineSetSeGroup(ref SwkbdInline s, u8 seGroup, bool flag);
    public static extern void swkbdInlineSetBackspaceFlag(ref SwkbdInline s, bool flag);
}
