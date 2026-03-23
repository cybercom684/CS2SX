// Auto-generated from services\hid.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum HidDebugPadButton
{
    HidDebugPadButton_A = (1 << 0),
    HidDebugPadButton_B = (1 << 1),
    HidDebugPadButton_X = (1 << 2),
    HidDebugPadButton_Y = (1 << 3),
    HidDebugPadButton_L = (1 << 4),
    HidDebugPadButton_R = (1 << 5),
    HidDebugPadButton_ZL = (1 << 6),
    HidDebugPadButton_ZR = (1 << 7),
    HidDebugPadButton_Start = (1 << 8),
    HidDebugPadButton_Select = (1 << 9),
    HidDebugPadButton_Left = (1 << 10),
    HidDebugPadButton_Up = (1 << 11),
    HidDebugPadButton_Right = (1 << 12),
    HidDebugPadButton_Down = (1 << 13),
}

public enum HidTouchScreenModeForNx
{
    HidTouchScreenModeForNx_UseSystemSetting = 0,
    HidTouchScreenModeForNx_Finger = 1,
    HidTouchScreenModeForNx_Heat2 = 2,
}

public enum HidMouseButton
{
    HidMouseButton_Left = (1 << 0),
    HidMouseButton_Right = (1 << 1),
    HidMouseButton_Middle = (1 << 2),
    HidMouseButton_Forward = (1 << 3),
    HidMouseButton_Back = (1 << 4),
}

public enum HidKeyboardKey
{
    HidKeyboardKey_A = 4,
    HidKeyboardKey_B = 5,
    HidKeyboardKey_C = 6,
    HidKeyboardKey_D = 7,
    HidKeyboardKey_E = 8,
    HidKeyboardKey_F = 9,
    HidKeyboardKey_G = 10,
    HidKeyboardKey_H = 11,
    HidKeyboardKey_I = 12,
    HidKeyboardKey_J = 13,
    HidKeyboardKey_K = 14,
    HidKeyboardKey_L = 15,
    HidKeyboardKey_M = 16,
    HidKeyboardKey_N = 17,
    HidKeyboardKey_O = 18,
    HidKeyboardKey_P = 19,
    HidKeyboardKey_Q = 20,
    HidKeyboardKey_R = 21,
    HidKeyboardKey_S = 22,
    HidKeyboardKey_T = 23,
    HidKeyboardKey_U = 24,
    HidKeyboardKey_V = 25,
    HidKeyboardKey_W = 26,
    HidKeyboardKey_X = 27,
    HidKeyboardKey_Y = 28,
    HidKeyboardKey_Z = 29,
    HidKeyboardKey_D1 = 30,
    HidKeyboardKey_D2 = 31,
    HidKeyboardKey_D3 = 32,
    HidKeyboardKey_D4 = 33,
    HidKeyboardKey_D5 = 34,
    HidKeyboardKey_D6 = 35,
    HidKeyboardKey_D7 = 36,
    HidKeyboardKey_D8 = 37,
    HidKeyboardKey_D9 = 38,
    HidKeyboardKey_D0 = 39,
    HidKeyboardKey_Return = 40,
    HidKeyboardKey_Escape = 41,
    HidKeyboardKey_Backspace = 42,
    HidKeyboardKey_Tab = 43,
    HidKeyboardKey_Space = 44,
    HidKeyboardKey_Minus = 45,
    HidKeyboardKey_Plus = 46,
    HidKeyboardKey_OpenBracket = 47,
    HidKeyboardKey_CloseBracket = 48,
    HidKeyboardKey_Pipe = 49,
    HidKeyboardKey_Tilde = 50,
    HidKeyboardKey_Semicolon = 51,
    HidKeyboardKey_Quote = 52,
    HidKeyboardKey_Backquote = 53,
    HidKeyboardKey_Comma = 54,
    HidKeyboardKey_Period = 55,
    HidKeyboardKey_Slash = 56,
    HidKeyboardKey_CapsLock = 57,
    HidKeyboardKey_F1 = 58,
    HidKeyboardKey_F2 = 59,
    HidKeyboardKey_F3 = 60,
    HidKeyboardKey_F4 = 61,
    HidKeyboardKey_F5 = 62,
    HidKeyboardKey_F6 = 63,
    HidKeyboardKey_F7 = 64,
    HidKeyboardKey_F8 = 65,
    HidKeyboardKey_F9 = 66,
    HidKeyboardKey_F10 = 67,
    HidKeyboardKey_F11 = 68,
    HidKeyboardKey_F12 = 69,
    HidKeyboardKey_PrintScreen = 70,
    HidKeyboardKey_ScrollLock = 71,
    HidKeyboardKey_Pause = 72,
    HidKeyboardKey_Insert = 73,
    HidKeyboardKey_Home = 74,
    HidKeyboardKey_PageUp = 75,
    HidKeyboardKey_Delete = 76,
    HidKeyboardKey_End = 77,
    HidKeyboardKey_PageDown = 78,
    HidKeyboardKey_RightArrow = 79,
    HidKeyboardKey_LeftArrow = 80,
    HidKeyboardKey_DownArrow = 81,
    HidKeyboardKey_UpArrow = 82,
    HidKeyboardKey_NumLock = 83,
    HidKeyboardKey_NumPadDivide = 84,
    HidKeyboardKey_NumPadMultiply = 85,
    HidKeyboardKey_NumPadSubtract = 86,
    HidKeyboardKey_NumPadAdd = 87,
    HidKeyboardKey_NumPadEnter = 88,
    HidKeyboardKey_NumPad1 = 89,
    HidKeyboardKey_NumPad2 = 90,
    HidKeyboardKey_NumPad3 = 91,
    HidKeyboardKey_NumPad4 = 92,
    HidKeyboardKey_NumPad5 = 93,
    HidKeyboardKey_NumPad6 = 94,
    HidKeyboardKey_NumPad7 = 95,
    HidKeyboardKey_NumPad8 = 96,
    HidKeyboardKey_NumPad9 = 97,
    HidKeyboardKey_NumPad0 = 98,
    HidKeyboardKey_NumPadDot = 99,
    HidKeyboardKey_Backslash = 100,
    HidKeyboardKey_Application = 101,
    HidKeyboardKey_Power = 102,
    HidKeyboardKey_NumPadEquals = 103,
    HidKeyboardKey_F13 = 104,
    HidKeyboardKey_F14 = 105,
    HidKeyboardKey_F15 = 106,
    HidKeyboardKey_F16 = 107,
    HidKeyboardKey_F17 = 108,
    HidKeyboardKey_F18 = 109,
    HidKeyboardKey_F19 = 110,
    HidKeyboardKey_F20 = 111,
    HidKeyboardKey_F21 = 112,
    HidKeyboardKey_F22 = 113,
    HidKeyboardKey_F23 = 114,
    HidKeyboardKey_F24 = 115,
    HidKeyboardKey_NumPadComma = 133,
    HidKeyboardKey_Ro = 135,
    HidKeyboardKey_KatakanaHiragana = 136,
    HidKeyboardKey_Yen = 137,
    HidKeyboardKey_Henkan = 138,
    HidKeyboardKey_Muhenkan = 139,
    HidKeyboardKey_NumPadCommaPc98 = 140,
    HidKeyboardKey_HangulEnglish = 144,
    HidKeyboardKey_Hanja = 145,
    HidKeyboardKey_Katakana = 146,
    HidKeyboardKey_Hiragana = 147,
    HidKeyboardKey_ZenkakuHankaku = 148,
    HidKeyboardKey_LeftControl = 224,
    HidKeyboardKey_LeftShift = 225,
    HidKeyboardKey_LeftAlt = 226,
    HidKeyboardKey_LeftGui = 227,
    HidKeyboardKey_RightControl = 228,
    HidKeyboardKey_RightShift = 229,
    HidKeyboardKey_RightAlt = 230,
    HidKeyboardKey_RightGui = 231,
}

public enum HidKeyboardModifier
{
    HidKeyboardModifier_Control = (1 << 0),
    HidKeyboardModifier_Shift = (1 << 1),
    HidKeyboardModifier_LeftAlt = (1 << 2),
    HidKeyboardModifier_RightAlt = (1 << 3),
    HidKeyboardModifier_Gui = (1 << 4),
    HidKeyboardModifier_CapsLock = (1 << 8),
    HidKeyboardModifier_ScrollLock = (1 << 9),
    HidKeyboardModifier_NumLock = (1 << 10),
    HidKeyboardModifier_Katakana = (1 << 11),
    HidKeyboardModifier_Hiragana = (1 << 12),
}

public enum HidKeyboardLockKeyEvent
{
    HidKeyboardLockKeyEvent_NumLockOn = (1 << 0),
    HidKeyboardLockKeyEvent_NumLockOff = (1 << 1),
    HidKeyboardLockKeyEvent_NumLockToggle = (1 << 2),
    HidKeyboardLockKeyEvent_CapsLockOn = (1 << 3),
    HidKeyboardLockKeyEvent_CapsLockOff = (1 << 4),
    HidKeyboardLockKeyEvent_CapsLockToggle = (1 << 5),
    HidKeyboardLockKeyEvent_ScrollLockOn = (1 << 6),
    HidKeyboardLockKeyEvent_ScrollLockOff = (1 << 7),
    HidKeyboardLockKeyEvent_ScrollLockToggle = (1 << 8),
}

public enum HidNpadIdType
{
    HidNpadIdType_No1 = 0,
    HidNpadIdType_No2 = 1,
    HidNpadIdType_No3 = 2,
    HidNpadIdType_No4 = 3,
    HidNpadIdType_No5 = 4,
    HidNpadIdType_No6 = 5,
    HidNpadIdType_No7 = 6,
    HidNpadIdType_No8 = 7,
    HidNpadIdType_Other = 0x10,
    HidNpadIdType_Handheld = 0x20,
}

public enum HidNpadStyleTag
{
    HidNpadStyleTag_NpadFullKey = (1 << 0),
    HidNpadStyleTag_NpadHandheld = (1 << 1),
    HidNpadStyleTag_NpadJoyDual = (1 << 2),
    HidNpadStyleTag_NpadJoyLeft = (1 << 3),
    HidNpadStyleTag_NpadJoyRight = (1 << 4),
    HidNpadStyleTag_NpadGc = (1 << 5),
    HidNpadStyleTag_NpadPalma = (1 << 6),
    HidNpadStyleTag_NpadLark = (1 << 7),
    HidNpadStyleTag_NpadHandheldLark = (1 << 8),
    HidNpadStyleTag_NpadLucia = (1 << 9),
    HidNpadStyleTag_NpadLagon = (1 << 10),
    HidNpadStyleTag_NpadLager = (1 << 11),
    HidNpadStyleTag_NpadSystemExt = (1 << 29),
    HidNpadStyleTag_NpadSystem = (1 << 30),
    HidNpadStyleSet_NpadFullCtrl = HidNpadStyleTag_NpadFullKey  | HidNpadStyleTag_NpadHandheld | HidNpadStyleTag_NpadJoyDual,
    HidNpadStyleSet_NpadStandard = HidNpadStyleSet_NpadFullCtrl | HidNpadStyleTag_NpadJoyLeft  | HidNpadStyleTag_NpadJoyRight,
}

public enum HidColorAttribute
{
    HidColorAttribute_Ok = 0,
    HidColorAttribute_ReadError = 1,
    HidColorAttribute_NoController = 2,
}

public enum HidNpadButton
{
    HidNpadButton_A = BITL(0),
    HidNpadButton_B = BITL(1),
    HidNpadButton_X = BITL(2),
    HidNpadButton_Y = BITL(3),
    HidNpadButton_StickL = BITL(4),
    HidNpadButton_StickR = BITL(5),
    HidNpadButton_L = BITL(6),
    HidNpadButton_R = BITL(7),
    HidNpadButton_ZL = BITL(8),
    HidNpadButton_ZR = BITL(9),
    HidNpadButton_Plus = BITL(10),
    HidNpadButton_Minus = BITL(11),
    HidNpadButton_Left = BITL(12),
    HidNpadButton_Up = BITL(13),
    HidNpadButton_Right = BITL(14),
    HidNpadButton_Down = BITL(15),
    HidNpadButton_StickLLeft = BITL(16),
    HidNpadButton_StickLUp = BITL(17),
    HidNpadButton_StickLRight = BITL(18),
    HidNpadButton_StickLDown = BITL(19),
    HidNpadButton_StickRLeft = BITL(20),
    HidNpadButton_StickRUp = BITL(21),
    HidNpadButton_StickRRight = BITL(22),
    HidNpadButton_StickRDown = BITL(23),
    HidNpadButton_LeftSL = BITL(24),
    HidNpadButton_LeftSR = BITL(25),
    HidNpadButton_RightSL = BITL(26),
    HidNpadButton_RightSR = BITL(27),
    HidNpadButton_Palma = BITL(28),
    HidNpadButton_Verification = BITL(29),
    HidNpadButton_HandheldLeftB = BITL(30),
    HidNpadButton_LagonCLeft = BITL(31),
    HidNpadButton_LagonCUp = BITL(32),
    HidNpadButton_LagonCRight = BITL(33),
    HidNpadButton_LagonCDown = BITL(34),
    HidNpadButton_AnyLeft = HidNpadButton_Left   | HidNpadButton_StickLLeft  | HidNpadButton_StickRLeft,
    HidNpadButton_AnyUp = HidNpadButton_Up     | HidNpadButton_StickLUp    | HidNpadButton_StickRUp,
    HidNpadButton_AnyRight = HidNpadButton_Right  | HidNpadButton_StickLRight | HidNpadButton_StickRRight,
    HidNpadButton_AnyDown = HidNpadButton_Down   | HidNpadButton_StickLDown  | HidNpadButton_StickRDown,
    HidNpadButton_AnySL = HidNpadButton_LeftSL | HidNpadButton_RightSL,
    HidNpadButton_AnySR = HidNpadButton_LeftSR | HidNpadButton_RightSR,
}

public enum HidDebugPadAttribute
{
    HidDebugPadAttribute_IsConnected = (1 << 0),
}

public enum HidTouchAttribute
{
    HidTouchAttribute_Start = (1 << 0),
    HidTouchAttribute_End = (1 << 1),
}

public enum HidMouseAttribute
{
    HidMouseAttribute_Transferable = (1 << 0),
    HidMouseAttribute_IsConnected = (1 << 1),
}

public enum HidNpadAttribute
{
    HidNpadAttribute_IsConnected = (1 << 0),
    HidNpadAttribute_IsWired = (1 << 1),
    HidNpadAttribute_IsLeftConnected = (1 << 2),
    HidNpadAttribute_IsLeftWired = (1 << 3),
    HidNpadAttribute_IsRightConnected = (1 << 4),
    HidNpadAttribute_IsRightWired = (1 << 5),
}

public enum HidSixAxisSensorAttribute
{
    HidSixAxisSensorAttribute_IsConnected = (1 << 0),
    HidSixAxisSensorAttribute_IsInterpolated = (1 << 1),
}

public enum HidGestureAttribute
{
    HidGestureAttribute_IsNewTouch = (1 << 4),
    HidGestureAttribute_IsDoubleTap = (1 << 8),
}

public enum HidGestureDirection
{
    HidGestureDirection_None = 0,
    HidGestureDirection_Left = 1,
    HidGestureDirection_Up = 2,
    HidGestureDirection_Right = 3,
    HidGestureDirection_Down = 4,
}

public enum HidGestureType
{
    HidGestureType_Idle = 0,
    HidGestureType_Complete = 1,
    HidGestureType_Cancel = 2,
    HidGestureType_Touch = 3,
    HidGestureType_Press = 4,
    HidGestureType_Tap = 5,
    HidGestureType_Pan = 6,
    HidGestureType_Swipe = 7,
    HidGestureType_Pinch = 8,
    HidGestureType_Rotate = 9,
}

public enum HidGyroscopeZeroDriftMode
{
    HidGyroscopeZeroDriftMode_Loose = 0,
    HidGyroscopeZeroDriftMode_Standard = 1,
    HidGyroscopeZeroDriftMode_Tight = 2,
}

public enum HidNpadJoyHoldType
{
    HidNpadJoyHoldType_Vertical = 0,
    HidNpadJoyHoldType_Horizontal = 1,
}

public enum HidNpadJoyDeviceType
{
    HidNpadJoyDeviceType_Left = 0,
    HidNpadJoyDeviceType_Right = 1,
}

public enum HidNpadHandheldActivationMode
{
    HidNpadHandheldActivationMode_Dual = 0,
    HidNpadHandheldActivationMode_Single = 1,
    HidNpadHandheldActivationMode_None = 2,
}

public enum HidNpadJoyAssignmentMode
{
    HidNpadJoyAssignmentMode_Dual = 0,
    HidNpadJoyAssignmentMode_Single = 1,
}

public enum HidNpadCommunicationMode
{
    HidNpadCommunicationMode_5ms = 0,
    HidNpadCommunicationMode_10ms = 1,
    HidNpadCommunicationMode_15ms = 2,
    HidNpadCommunicationMode_Default = 3,
}

public enum HidDeviceTypeBits
{
    HidDeviceTypeBits_FullKey = (1 << 0),
    HidDeviceTypeBits_DebugPad = (1 << 1),
    HidDeviceTypeBits_HandheldLeft = (1 << 2),
    HidDeviceTypeBits_HandheldRight = (1 << 3),
    HidDeviceTypeBits_JoyLeft = (1 << 4),
    HidDeviceTypeBits_JoyRight = (1 << 5),
    HidDeviceTypeBits_Palma = (1 << 6),
    HidDeviceTypeBits_LarkHvcLeft = (1 << 7),
    HidDeviceTypeBits_LarkHvcRight = (1 << 8),
    HidDeviceTypeBits_LarkNesLeft = (1 << 9),
    HidDeviceTypeBits_LarkNesRight = (1 << 10),
    HidDeviceTypeBits_HandheldLarkHvcLeft = (1 << 11),
    HidDeviceTypeBits_HandheldLarkHvcRight = (1 << 12),
    HidDeviceTypeBits_HandheldLarkNesLeft = (1 << 13),
    HidDeviceTypeBits_HandheldLarkNesRight = (1 << 14),
    HidDeviceTypeBits_Lucia = (1 << 15),
    HidDeviceTypeBits_Lagon = (1 << 16),
    HidDeviceTypeBits_Lager = (1 << 17),
    HidDeviceTypeBits_System = (1 << 31),
}

public enum HidDeviceType
{
    HidDeviceType_JoyRight1 = 1,
    HidDeviceType_JoyLeft2 = 2,
    HidDeviceType_FullKey3 = 3,
    HidDeviceType_JoyLeft4 = 4,
    HidDeviceType_JoyRight5 = 5,
    HidDeviceType_FullKey6 = 6,
    HidDeviceType_LarkHvcLeft = 7,
    HidDeviceType_LarkHvcRight = 8,
    HidDeviceType_LarkNesLeft = 9,
    HidDeviceType_LarkNesRight = 10,
    HidDeviceType_Lucia = 11,
    HidDeviceType_Palma = 12,
    HidDeviceType_FullKey13 = 13,
    HidDeviceType_FullKey15 = 15,
    HidDeviceType_DebugPad = 17,
    HidDeviceType_System19 = 19,
    HidDeviceType_System20 = 20,
    HidDeviceType_System21 = 21,
    HidDeviceType_Lagon = 22,
    HidDeviceType_Lager = 28,
}

public enum HidAppletFooterUiType
{
    HidAppletFooterUiType_None = 0,
    HidAppletFooterUiType_HandheldNone = 1,
    HidAppletFooterUiType_HandheldJoyConLeftOnly = 2,
    HidAppletFooterUiType_HandheldJoyConRightOnly = 3,
    HidAppletFooterUiType_HandheldJoyConLeftJoyConRight = 4,
    HidAppletFooterUiType_JoyDual = 5,
    HidAppletFooterUiType_JoyDualLeftOnly = 6,
    HidAppletFooterUiType_JoyDualRightOnly = 7,
    HidAppletFooterUiType_JoyLeftHorizontal = 8,
    HidAppletFooterUiType_JoyLeftVertical = 9,
    HidAppletFooterUiType_JoyRightHorizontal = 10,
    HidAppletFooterUiType_JoyRightVertical = 11,
    HidAppletFooterUiType_SwitchProController = 12,
    HidAppletFooterUiType_CompatibleProController = 13,
    HidAppletFooterUiType_CompatibleJoyCon = 14,
    HidAppletFooterUiType_LarkHvc1 = 15,
    HidAppletFooterUiType_LarkHvc2 = 16,
    HidAppletFooterUiType_LarkNesLeft = 17,
    HidAppletFooterUiType_LarkNesRight = 18,
    HidAppletFooterUiType_Lucia = 19,
    HidAppletFooterUiType_Verification = 20,
    HidAppletFooterUiType_Lagon = 21,
}

public enum HidNpadInterfaceType
{
    HidNpadInterfaceType_Bluetooth = 1,
    HidNpadInterfaceType_Rail = 2,
    HidNpadInterfaceType_USB = 3,
    HidNpadInterfaceType_Unknown4 = 4,
}

public enum XcdInterfaceType
{
    XcdInterfaceType_Bluetooth = (1 << 0),
    XcdInterfaceType_Uart = (1 << 1),
    XcdInterfaceType_Usb = (1 << 2),
    XcdInterfaceType_FieldSet = (1 << 7),
}

public enum HidNpadLarkType
{
    HidNpadLarkType_Invalid = 0,
    HidNpadLarkType_H1 = 1,
    HidNpadLarkType_H2 = 2,
    HidNpadLarkType_NL = 3,
    HidNpadLarkType_NR = 4,
}

public enum HidNpadLuciaType
{
    HidNpadLuciaType_Invalid = 0,
    HidNpadLuciaType_J = 1,
    HidNpadLuciaType_E = 2,
    HidNpadLuciaType_U = 3,
}

public enum HidNpadLagerType
{
    HidNpadLagerType_Invalid = 0,
    HidNpadLagerType_J = 1,
    HidNpadLagerType_E = 2,
    HidNpadLagerType_U = 3,
}

public enum HidVibrationDeviceType
{
    HidVibrationDeviceType_Unknown = 0,
    HidVibrationDeviceType_LinearResonantActuator = 1,
    HidVibrationDeviceType_GcErm = 2,
}

public enum HidVibrationDevicePosition
{
    HidVibrationDevicePosition_None = 0,
    HidVibrationDevicePosition_Left = 1,
    HidVibrationDevicePosition_Right = 2,
}

public enum HidVibrationGcErmCommand
{
    HidVibrationGcErmCommand_Stop = 0,
    HidVibrationGcErmCommand_Start = 1,
    HidVibrationGcErmCommand_StopHard = 2,
}

public enum HidPalmaOperationType
{
    HidPalmaOperationType_PlayActivity = 0,
    HidPalmaOperationType_SetFrModeType = 1,
    HidPalmaOperationType_ReadStep = 2,
    HidPalmaOperationType_EnableStep = 3,
    HidPalmaOperationType_ResetStep = 4,
    HidPalmaOperationType_ReadApplicationSection = 5,
    HidPalmaOperationType_WriteApplicationSection = 6,
    HidPalmaOperationType_ReadUniqueCode = 7,
    HidPalmaOperationType_SetUniqueCodeInvalid = 8,
    HidPalmaOperationType_WriteActivityEntry = 9,
    HidPalmaOperationType_WriteRgbLedPatternEntry = 10,
    HidPalmaOperationType_WriteWaveEntry = 11,
    HidPalmaOperationType_ReadDataBaseIdentificationVersion = 12,
    HidPalmaOperationType_WriteDataBaseIdentificationVersion = 13,
    HidPalmaOperationType_SuspendFeature = 14,
    HidPalmaOperationType_ReadPlayLog = 15,
    HidPalmaOperationType_ResetPlayLog = 16,
}

public enum HidPalmaFrModeType
{
    HidPalmaFrModeType_Off = 0,
    HidPalmaFrModeType_B01 = 1,
    HidPalmaFrModeType_B02 = 2,
    HidPalmaFrModeType_B03 = 3,
    HidPalmaFrModeType_Downloaded = 4,
}

public enum HidPalmaWaveSet
{
    HidPalmaWaveSet_Small = 0,
    HidPalmaWaveSet_Medium = 1,
    HidPalmaWaveSet_Large = 2,
}

public enum HidPalmaFeature
{
    HidPalmaFeature_FrMode = (1 << 0),
    HidPalmaFeature_RumbleFeedback = (1 << 1),
    HidPalmaFeature_Step = (1 << 2),
    HidPalmaFeature_MuteSwitch = (1 << 3),
}

public unsafe struct HidAnalogStickState
{
    public s32 x;
    public s32 y;
}

public unsafe struct HidVector
{
    public float x;
    public float y;
    public float z;
}

public unsafe struct HidDirectionState
{
    public float direction;
}

public unsafe struct HidCommonLifoHeader
{
    public u64 unused;
    public u64 buffer_count;
    public u64 tail;
    public u64 count;
}

public unsafe struct HidDebugPadState
{
    public u64 sampling_number;
    public u32 attributes;
    public u32 buttons;
    public HidAnalogStickState analog_stick_r;
    public HidAnalogStickState analog_stick_l;
}

public unsafe struct HidDebugPadStateAtomicStorage
{
    public u64 sampling_number;
    public HidDebugPadState state;
}

public unsafe struct HidDebugPadLifo
{
    public HidCommonLifoHeader header;
    // skipped array: HidDebugPadStateAtomicStorage storage[17]
}

public unsafe struct HidDebugPadSharedMemoryFormat
{
    public HidDebugPadLifo lifo;
    public u8 padding;
}

public unsafe struct HidTouchState
{
    public u64 delta_time;
    public u32 attributes;
    public u32 finger_id;
    public u32 x;
    public u32 y;
    public u32 diameter_x;
    public u32 diameter_y;
    public u32 rotation_angle;
    public u32 reserved;
}

public unsafe struct HidTouchScreenState
{
    public u64 sampling_number;
    public s32 count;
    public u32 reserved;
    // skipped array: HidTouchState touches[16]
}

public unsafe struct HidTouchScreenStateAtomicStorage
{
    public u64 sampling_number;
    public HidTouchScreenState state;
}

public unsafe struct HidTouchScreenLifo
{
    public HidCommonLifoHeader header;
    // skipped array: HidTouchScreenStateAtomicStorage storage[17]
}

public unsafe struct HidTouchScreenSharedMemoryFormat
{
    public HidTouchScreenLifo lifo;
    public u8 padding;
}

public unsafe struct HidTouchScreenConfigurationForNx
{
    public u8 mode;
    public u8 reserved;
}

public unsafe struct HidMouseState
{
    public u64 sampling_number;
    public s32 x;
    public s32 y;
    public s32 delta_x;
    public s32 delta_y;
    public s32 wheel_delta_x;
    public s32 wheel_delta_y;
    public u32 buttons;
    public u32 attributes;
}

public unsafe struct HidMouseStateAtomicStorage
{
    public u64 sampling_number;
    public HidMouseState state;
}

public unsafe struct HidMouseLifo
{
    public HidCommonLifoHeader header;
    // skipped array: HidMouseStateAtomicStorage storage[17]
}

public unsafe struct HidMouseSharedMemoryFormat
{
    public HidMouseLifo lifo;
    public u8 padding;
}

public unsafe struct HidKeyboardState
{
    public u64 sampling_number;
    public u64 modifiers;
    public fixed u64 keys[4];
}

public unsafe struct HidKeyboardStateAtomicStorage
{
    public u64 sampling_number;
    public HidKeyboardState state;
}

public unsafe struct HidKeyboardLifo
{
    public HidCommonLifoHeader header;
    // skipped array: HidKeyboardStateAtomicStorage storage[17]
}

public unsafe struct HidKeyboardSharedMemoryFormat
{
    public HidKeyboardLifo lifo;
    public u8 padding;
}

public unsafe struct HidBasicXpadState
{
    public u64 sampling_number;
    public u32 attributes;
    public u32 buttons;
    public u64 analog_stick_left;
    public u64 analog_stick_right;
}

public unsafe struct HidBasicXpadStateAtomicStorage
{
    public u64 sampling_number;
    public HidBasicXpadState state;
}

public unsafe struct HidBasicXpadLifo
{
    public HidCommonLifoHeader header;
    // skipped array: HidBasicXpadStateAtomicStorage storage[17]
}

public unsafe struct HidBasicXpadSharedMemoryEntry
{
    public HidBasicXpadLifo lifo;
    public u8 padding;
}

public unsafe struct HidBasicXpadSharedMemoryFormat
{
    // skipped array: HidBasicXpadSharedMemoryEntry entries[4]
}

public unsafe struct HidDigitizerState
{
    public u64 sampling_number;
    public u32 unk_0x8;
    public u32 unk_0xC;
    public u32 attributes;
    public u32 buttons;
    public u32 unk_0x18;
    public u32 unk_0x1C;
    public u32 unk_0x20;
    public u32 unk_0x24;
    public u32 unk_0x28;
    public u32 unk_0x2C;
    public u32 unk_0x30;
    public u32 unk_0x34;
    public u32 unk_0x38;
    public u32 unk_0x3C;
    public u32 unk_0x40;
    public u32 unk_0x44;
    public u32 unk_0x48;
    public u32 unk_0x4C;
    public u32 unk_0x50;
    public u32 unk_0x54;
}

public unsafe struct HidDigitizerStateAtomicStorage
{
    public u64 sampling_number;
    public HidDigitizerState state;
}

public unsafe struct HidDigitizerLifo
{
    public HidCommonLifoHeader header;
    // skipped array: HidDigitizerStateAtomicStorage storage[17]
}

public unsafe struct HidDigitizerSharedMemoryFormat
{
    public HidDigitizerLifo lifo;
    public u8 padding;
}

public unsafe struct HidHomeButtonState
{
    public u64 sampling_number;
    public u64 buttons;
}

public unsafe struct HidHomeButtonStateAtomicStorage
{
    public u64 sampling_number;
    public HidHomeButtonState state;
}

public unsafe struct HidHomeButtonLifo
{
    public HidCommonLifoHeader header;
    // skipped array: HidHomeButtonStateAtomicStorage storage[17]
}

public unsafe struct HidHomeButtonSharedMemoryFormat
{
    public HidHomeButtonLifo lifo;
    public u8 padding;
}

public unsafe struct HidSleepButtonState
{
    public u64 sampling_number;
    public u64 buttons;
}

public unsafe struct HidSleepButtonStateAtomicStorage
{
    public u64 sampling_number;
    public HidSleepButtonState state;
}

public unsafe struct HidSleepButtonLifo
{
    public HidCommonLifoHeader header;
    // skipped array: HidSleepButtonStateAtomicStorage storage[17]
}

public unsafe struct HidSleepButtonSharedMemoryFormat
{
    public HidSleepButtonLifo lifo;
    public u8 padding;
}

public unsafe struct HidCaptureButtonState
{
    public u64 sampling_number;
    public u64 buttons;
}

public unsafe struct HidCaptureButtonStateAtomicStorage
{
    public u64 sampling_number;
    public HidCaptureButtonState state;
}

public unsafe struct HidCaptureButtonLifo
{
    public HidCommonLifoHeader header;
    // skipped array: HidCaptureButtonStateAtomicStorage storage[17]
}

public unsafe struct HidCaptureButtonSharedMemoryFormat
{
    public HidCaptureButtonLifo lifo;
    public u8 padding;
}

public unsafe struct HidInputDetectorState
{
    public u64 input_source_state;
    public u64 sampling_number;
}

public unsafe struct HidInputDetectorStateAtomicStorage
{
    public u64 sampling_number;
    public HidInputDetectorState state;
}

public unsafe struct HidInputDetectorLifo
{
    public HidCommonLifoHeader header;
    // skipped array: HidInputDetectorStateAtomicStorage storage[2]
}

public unsafe struct HidInputDetectorSharedMemoryEntry
{
    public HidInputDetectorLifo lifo;
    public u8 padding;
}

public unsafe struct HidInputDetectorSharedMemoryFormat
{
    // skipped array: HidInputDetectorSharedMemoryEntry entries[16]
}

public unsafe struct HidUniquePadConfigMutex
{
    public u8 unk_0x0;
}

public unsafe struct HidSixAxisSensorUserCalibrationState
{
    public u32 flags;
    public fixed u8 reserved[4];
    public u64 stage;
    public u64 sampling_number;
}

public unsafe struct HidSixAxisSensorUserCalibrationStateAtomicStorage
{
    public u64 sampling_number;
    public HidSixAxisSensorUserCalibrationState calib_state;
}

public unsafe struct HidSixAxisSensorUserCalibrationStateLifo
{
    public HidCommonLifoHeader header;
    // skipped array: HidSixAxisSensorUserCalibrationStateAtomicStorage storage[2]
}

public unsafe struct HidAnalogStickCalibrationStateImpl
{
    public u64 state;
    public u64 flags;
    public u64 stage;
    public u64 sampling_number;
}

public unsafe struct HidAnalogStickCalibrationStateImplAtomicStorage
{
    public u64 sampling_number;
    public HidAnalogStickCalibrationStateImpl calib_state;
}

public unsafe struct HidAnalogStickCalibrationStateImplLifo
{
    public HidCommonLifoHeader header;
    // skipped array: HidAnalogStickCalibrationStateImplAtomicStorage storage[2]
}

public unsafe struct HidUniquePadConfig
{
    public u32 type;
    public u32 @interface;
    public u8 serial_number;
    public u32 controller_number;
    public bool is_active;
    public fixed u8 reserved[3];
    public u64 sampling_number;
}

public unsafe struct HidUniquePadConfigAtomicStorage
{
    public u64 sampling_number;
    public HidUniquePadConfig config;
}

public unsafe struct HidUniquePadConfigLifo
{
    public HidCommonLifoHeader header;
    // skipped array: HidUniquePadConfigAtomicStorage storage[2]
}

public unsafe struct HidUniquePadLifo
{
    public HidUniquePadConfigLifo config_lifo;
    // skipped array: HidAnalogStickCalibrationStateImplLifo analog_stick_calib_lifo[2]
    public HidSixAxisSensorUserCalibrationStateLifo sixaxis_calib_lifo;
    public HidUniquePadConfigMutex mutex;
}

public unsafe struct HidUniquePadSharedMemoryEntry
{
    public HidUniquePadLifo lifo;
    public u8 padding;
}

public unsafe struct HidUniquePadSharedMemoryFormat
{
    // skipped array: HidUniquePadSharedMemoryEntry entries[16]
}

public unsafe struct HidNpadControllerColor
{
    public u32 main;
    public u32 sub;
}

public unsafe struct HidNpadFullKeyColorState
{
    public u32 attribute;
    public HidNpadControllerColor full_key;
}

public unsafe struct HidNpadJoyColorState
{
    public u32 attribute;
    public HidNpadControllerColor left;
    public HidNpadControllerColor right;
}

public unsafe struct HidNpadCommonState
{
    public u64 sampling_number;
    public u64 buttons;
    public HidAnalogStickState analog_stick_l;
    public HidAnalogStickState analog_stick_r;
    public u32 attributes;
    public u32 reserved;
}

public unsafe struct HidNpadGcState
{
    public u64 sampling_number;
    public u64 buttons;
    public HidAnalogStickState analog_stick_l;
    public HidAnalogStickState analog_stick_r;
    public u32 attributes;
    public u32 trigger_l;
    public u32 trigger_r;
    public u32 pad;
}

public unsafe struct HidNpadLarkState
{
    public u64 sampling_number;
    public u64 buttons;
    public HidAnalogStickState analog_stick_l;
    public HidAnalogStickState analog_stick_r;
    public u32 attributes;
    public HidNpadLarkType lark_type_l_and_main;
}

public unsafe struct HidNpadHandheldLarkState
{
    public u64 sampling_number;
    public u64 buttons;
    public HidAnalogStickState analog_stick_l;
    public HidAnalogStickState analog_stick_r;
    public u32 attributes;
    public HidNpadLarkType lark_type_l_and_main;
    public HidNpadLarkType lark_type_r;
    public u32 pad;
}

public unsafe struct HidNpadLuciaState
{
    public u64 sampling_number;
    public u64 buttons;
    public HidAnalogStickState analog_stick_l;
    public HidAnalogStickState analog_stick_r;
    public u32 attributes;
    public HidNpadLuciaType lucia_type;
}

public unsafe struct HidNpadCommonStateAtomicStorage
{
    public u64 sampling_number;
    public HidNpadCommonState state;
}

public unsafe struct HidNpadCommonLifo
{
    public HidCommonLifoHeader header;
    // skipped array: HidNpadCommonStateAtomicStorage storage[17]
}

public unsafe struct HidNpadGcTriggerState
{
    public u64 sampling_number;
    public u32 trigger_l;
    public u32 trigger_r;
}

public unsafe struct HidNpadGcTriggerStateAtomicStorage
{
    public u64 sampling_number;
    public HidNpadGcTriggerState state;
}

public unsafe struct HidNpadGcTriggerLifo
{
    public HidCommonLifoHeader header;
    // skipped array: HidNpadGcTriggerStateAtomicStorage storage[17]
}

public unsafe struct HidSixAxisSensorState
{
    public u64 delta_time;
    public u64 sampling_number;
    public HidVector acceleration;
    public HidVector angular_velocity;
    public HidVector angle;
    public HidDirectionState direction;
    public u32 attributes;
    public u32 reserved;
}

public unsafe struct HidSixAxisSensorStateAtomicStorage
{
    public u64 sampling_number;
    public HidSixAxisSensorState state;
}

public unsafe struct HidNpadSixAxisSensorLifo
{
    public HidCommonLifoHeader header;
    // skipped array: HidSixAxisSensorStateAtomicStorage storage[17]
}

public unsafe struct HidNpadSystemProperties
{
    public u64 is_charging : _3;
    public u64 is_powered : _3;
    public u64 bit6 : _1;
    public u64 bit7 : _1;
    public u64 bit8 : _1;
    public u64 is_unsupported_button_pressed_on_npad_system : _1;
    public u64 is_unsupported_button_pressed_on_npad_system_ext : _1;
    public u64 is_abxy_button_oriented : _1;
    public u64 is_sl_sr_button_oriented : _1;
    public u64 is_plus_available : _1;
    public u64 is_minus_available : _1;
    public u64 is_directional_buttons_available : _1;
    public u64 unused : _48;
}

public unsafe struct HidNpadSystemButtonProperties
{
    public u32 is_unintended_home_button_input_protection_enabled : _1;
}

public unsafe struct HidPowerInfo
{
    public bool is_powered;
    public bool is_charging;
    public fixed u8 reserved[6];
    public u32 battery_level;
}

public unsafe struct XcdDeviceHandle
{
    public u64 handle;
}

public unsafe struct HidNfcXcdDeviceHandleStateImpl
{
    public XcdDeviceHandle handle;
    public u8 is_available;
    public u8 is_activated;
    public fixed u8 reserved[6];
    public u64 sampling_number;
}

public unsafe struct HidNfcXcdDeviceHandleStateImplAtomicStorage
{
    public u64 sampling_number;
    public HidNfcXcdDeviceHandleStateImpl state;
}

public unsafe struct HidNfcXcdDeviceHandleState
{
    public HidCommonLifoHeader header;
    // skipped array: HidNfcXcdDeviceHandleStateImplAtomicStorage storage[2]
}

public unsafe struct HidNpadSharedMemoryEntry
{
    public HidNpadInternalState internal_state;
    public u8 pad;
}

public unsafe struct HidNpadSharedMemoryFormat
{
    // skipped array: HidNpadSharedMemoryEntry entries[10]
}

public unsafe struct HidGesturePoint
{
    public u32 x;
    public u32 y;
}

public unsafe struct HidGestureState
{
    public u64 sampling_number;
    public u64 context_number;
    public u32 type;
    public u32 direction;
    public u32 x;
    public u32 y;
    public s32 delta_x;
    public s32 delta_y;
    public float velocity_x;
    public float velocity_y;
    public u32 attributes;
    public float scale;
    public float rotation_angle;
    public s32 point_count;
    // skipped array: HidGesturePoint points[4]
}

public unsafe struct HidGestureDummyStateAtomicStorage
{
    public u64 sampling_number;
    public HidGestureState state;
}

public unsafe struct HidGestureLifo
{
    public HidCommonLifoHeader header;
    // skipped array: HidGestureDummyStateAtomicStorage storage[17]
}

public unsafe struct HidGestureSharedMemoryFormat
{
    public HidGestureLifo lifo;
    public u8 pad;
}

public unsafe struct HidConsoleSixAxisSensor
{
    public u64 sampling_number;
    public u8 is_seven_six_axis_sensor_at_rest;
    public u8 pad;
    public float verticalization_error;
    public UtilFloat3 gyro_bias;
    public u8 pad2;
}

public unsafe struct HidSevenSixAxisSensorState
{
    public u64 timestamp0;
    public u64 sampling_number;
    public u64 unk_x10;
    public fixed float unk_x18[10];
}

public unsafe struct HidSevenSixAxisSensorStateEntry
{
    public u64 sampling_number;
    public u64 unused;
    public HidSevenSixAxisSensorState state;
}

public unsafe struct HidSevenSixAxisSensorStates
{
    public HidCommonLifoHeader header;
    public HidSevenSixAxisSensorStateEntry storage;
}

public unsafe struct HidVibrationDeviceInfo
{
    public u32 type;
    public u32 position;
}

public unsafe struct HidVibrationValue
{
    public float amp_low;
    public float freq_low;
    public float amp_high;
    public float freq_high;
}

public unsafe struct HidPalmaConnectionHandle
{
    public u64 handle;
}

public unsafe struct HidPalmaOperationInfo
{
    public u32 type;
    public Result res;
    public u8 data;
}

public unsafe struct HidPalmaApplicationSectionAccessBuffer
{
    public u8 data;
}

public unsafe struct HidPalmaActivityEntry
{
    public u16 rgb_led_pattern_index;
    public u16 pad;
    public u32 wave_set;
    public u16 wave_index;
}

public static class Hid
{
    public static extern Result hidInitialize();
    public static extern void hidExit();
    public static extern IntPtr hidGetServiceSession();
    public static extern IntPtr hidGetSharedmemAddr();
    public static extern void hidInitializeTouchScreen();
    public static extern ulong hidGetTouchScreenStates(ref HidTouchScreenState states, ulong count);
    public static extern void hidInitializeMouse();
    public static extern ulong hidGetMouseStates(ref HidMouseState states, ulong count);
    public static extern void hidInitializeKeyboard();
    public static extern ulong hidGetKeyboardStates(ref HidKeyboardState states, ulong count);
    public static extern ulong hidGetHomeButtonStates(ref HidHomeButtonState states, ulong count);
    public static extern ulong hidGetSleepButtonStates(ref HidSleepButtonState states, ulong count);
    public static extern ulong hidGetCaptureButtonStates(ref HidCaptureButtonState states, ulong count);
    public static extern void hidInitializeNpad();
    public static extern u32 hidGetNpadStyleSet(HidNpadIdType id);
    public static extern HidNpadJoyAssignmentMode hidGetNpadJoyAssignment(HidNpadIdType id);
    public static extern Result hidGetNpadControllerColorSingle(HidNpadIdType id, ref HidNpadControllerColor color);
    public static extern Result hidGetNpadControllerColorSplit(HidNpadIdType id, ref HidNpadControllerColor color_left, ref HidNpadControllerColor color_right);
    public static extern u32 hidGetNpadDeviceType(HidNpadIdType id);
    public static extern void hidGetNpadSystemProperties(HidNpadIdType id, ref HidNpadSystemProperties @out);
    public static extern void hidGetNpadSystemButtonProperties(HidNpadIdType id, ref HidNpadSystemButtonProperties @out);
    public static extern void hidGetNpadPowerInfoSingle(HidNpadIdType id, ref HidPowerInfo info);
    public static extern void hidGetNpadPowerInfoSplit(HidNpadIdType id, ref HidPowerInfo info_left, ref HidPowerInfo info_right);
    public static extern u32 hidGetAppletFooterUiAttributesSet(HidNpadIdType id);
    public static extern HidAppletFooterUiType hidGetAppletFooterUiTypes(HidNpadIdType id);
    public static extern HidNpadLagerType hidGetNpadLagerType(HidNpadIdType id);
    public static extern ulong hidGetNpadStatesFullKey(HidNpadIdType id, ref HidNpadFullKeyState states, ulong count);
    public static extern ulong hidGetNpadStatesHandheld(HidNpadIdType id, ref HidNpadHandheldState states, ulong count);
    public static extern ulong hidGetNpadStatesJoyDual(HidNpadIdType id, ref HidNpadJoyDualState states, ulong count);
    public static extern ulong hidGetNpadStatesJoyLeft(HidNpadIdType id, ref HidNpadJoyLeftState states, ulong count);
    public static extern ulong hidGetNpadStatesJoyRight(HidNpadIdType id, ref HidNpadJoyRightState states, ulong count);
    public static extern ulong hidGetNpadStatesGc(HidNpadIdType id, ref HidNpadGcState states, ulong count);
    public static extern ulong hidGetNpadStatesPalma(HidNpadIdType id, ref HidNpadPalmaState states, ulong count);
    public static extern ulong hidGetNpadStatesLark(HidNpadIdType id, ref HidNpadLarkState states, ulong count);
    public static extern ulong hidGetNpadStatesHandheldLark(HidNpadIdType id, ref HidNpadHandheldLarkState states, ulong count);
    public static extern ulong hidGetNpadStatesLucia(HidNpadIdType id, ref HidNpadLuciaState states, ulong count);
    public static extern ulong hidGetNpadStatesLager(HidNpadIdType id, ref HidNpadLagerState states, ulong count);
    public static extern ulong hidGetNpadStatesSystemExt(HidNpadIdType id, ref HidNpadSystemExtState states, ulong count);
    public static extern ulong hidGetNpadStatesSystem(HidNpadIdType id, ref HidNpadSystemState states, ulong count);
    public static extern ulong hidGetSixAxisSensorStates(HidSixAxisSensorHandle handle, ref HidSixAxisSensorState states, ulong count);
    public static extern void hidInitializeGesture();
    public static extern ulong hidGetGestureStates(ref HidGestureState states, ulong count);
    public static extern Result hidSendKeyboardLockKeyEvent(u32 events);
    public static extern Result hidGetSixAxisSensorHandles(ref HidSixAxisSensorHandle handles, s32 total_handles, HidNpadIdType id, HidNpadStyleTag style);
    public static extern Result hidStartSixAxisSensor(HidSixAxisSensorHandle handle);
    public static extern Result hidStopSixAxisSensor(HidSixAxisSensorHandle handle);
    public static extern Result hidIsSixAxisSensorFusionEnabled(HidSixAxisSensorHandle handle, ref bool @out);
    public static extern Result hidEnableSixAxisSensorFusion(HidSixAxisSensorHandle handle, bool flag);
    public static extern Result hidSetSixAxisSensorFusionParameters(HidSixAxisSensorHandle handle, float unk0, float unk1);
    public static extern Result hidGetSixAxisSensorFusionParameters(HidSixAxisSensorHandle handle, ref float unk0, ref float unk1);
    public static extern Result hidResetSixAxisSensorFusionParameters(HidSixAxisSensorHandle handle);
    public static extern Result hidSetGyroscopeZeroDriftMode(HidSixAxisSensorHandle handle, HidGyroscopeZeroDriftMode mode);
    public static extern Result hidGetGyroscopeZeroDriftMode(HidSixAxisSensorHandle handle, ref HidGyroscopeZeroDriftMode mode);
    public static extern Result hidResetGyroscopeZeroDriftMode(HidSixAxisSensorHandle handle);
    public static extern Result hidIsSixAxisSensorAtRest(HidSixAxisSensorHandle handle, ref bool @out);
    public static extern Result hidIsFirmwareUpdateAvailableForSixAxisSensor(HidSixAxisSensorHandle handle, ref bool @out);
    public static extern Result hidSetSupportedNpadStyleSet(u32 style_set);
    public static extern Result hidGetSupportedNpadStyleSet(ref u32 style_set);
    public static extern Result hidSetSupportedNpadIdType(ref HidNpadIdType ids, ulong count);
    public static extern Result hidAcquireNpadStyleSetUpdateEventHandle(HidNpadIdType id, ref Event out_event, bool autoclear);
    public static extern Result hidDisconnectNpad(HidNpadIdType id);
    public static extern Result hidGetPlayerLedPattern(HidNpadIdType id, ref u8 @out);
    public static extern Result hidSetNpadJoyHoldType(HidNpadJoyHoldType type);
    public static extern Result hidGetNpadJoyHoldType(ref HidNpadJoyHoldType type);
    public static extern Result hidSetNpadJoyAssignmentModeSingleByDefault(HidNpadIdType id);
    public static extern Result hidSetNpadJoyAssignmentModeSingle(HidNpadIdType id, HidNpadJoyDeviceType type);
    public static extern Result hidSetNpadJoyAssignmentModeDual(HidNpadIdType id);
    public static extern Result hidMergeSingleJoyAsDualJoy(HidNpadIdType id0, HidNpadIdType id1);
    public static extern Result hidStartLrAssignmentMode();
    public static extern Result hidStopLrAssignmentMode();
    public static extern Result hidSetNpadHandheldActivationMode(HidNpadHandheldActivationMode mode);
    public static extern Result hidGetNpadHandheldActivationMode(ref HidNpadHandheldActivationMode @out);
    public static extern Result hidSwapNpadAssignment(HidNpadIdType id0, HidNpadIdType id1);
    public static extern Result hidEnableUnintendedHomeButtonInputProtection(HidNpadIdType id, bool flag);
    public static extern Result hidSetNpadJoyAssignmentModeSingleWithDestination(HidNpadIdType id, HidNpadJoyDeviceType type, ref bool flag, ref HidNpadIdType dest);
    public static extern Result hidSetNpadAnalogStickUseCenterClamp(bool flag);
    public static extern Result hidSetNpadCaptureButtonAssignment(HidNpadStyleTag style, u64 buttons);
    public static extern Result hidClearNpadCaptureButtonAssignment();
    public static extern Result hidInitializeVibrationDevices(ref HidVibrationDeviceHandle handles, s32 total_handles, HidNpadIdType id, HidNpadStyleTag style);
    public static extern Result hidGetVibrationDeviceInfo(HidVibrationDeviceHandle handle, ref HidVibrationDeviceInfo @out);
    public static extern Result hidSendVibrationValue(HidVibrationDeviceHandle handle, ref HidVibrationValue value);
    public static extern Result hidGetActualVibrationValue(HidVibrationDeviceHandle handle, ref HidVibrationValue @out);
    public static extern Result hidPermitVibration(bool flag);
    public static extern Result hidIsVibrationPermitted(ref bool flag);
    public static extern Result hidSendVibrationValues(ref HidVibrationDeviceHandle handles, ref HidVibrationValue values, s32 count);
    public static extern Result hidSendVibrationGcErmCommand(HidVibrationDeviceHandle handle, HidVibrationGcErmCommand cmd);
    public static extern Result hidGetActualVibrationGcErmCommand(HidVibrationDeviceHandle handle, ref HidVibrationGcErmCommand @out);
    public static extern Result hidBeginPermitVibrationSession();
    public static extern Result hidEndPermitVibrationSession();
    public static extern Result hidIsVibrationDeviceMounted(HidVibrationDeviceHandle handle, ref bool flag);
    public static extern Result hidStartSevenSixAxisSensor();
    public static extern Result hidStopSevenSixAxisSensor();
    public static extern Result hidInitializeSevenSixAxisSensor();
    public static extern Result hidFinalizeSevenSixAxisSensor();
    public static extern Result hidSetSevenSixAxisSensorFusionStrength(float strength);
    public static extern Result hidGetSevenSixAxisSensorFusionStrength(ref float strength);
    public static extern Result hidResetSevenSixAxisSensorTimestamp();
    public static extern Result hidGetSevenSixAxisSensorStates(ref HidSevenSixAxisSensorState states, ulong count, ref ulong total_out);
    public static extern Result hidIsSevenSixAxisSensorAtRest(ref bool @out);
    public static extern Result hidGetSensorFusionError(ref float @out);
    public static extern Result hidGetGyroBias(ref UtilFloat3 @out);
    public static extern Result hidIsUsbFullKeyControllerEnabled(ref bool @out);
    public static extern Result hidEnableUsbFullKeyController(bool flag);
    public static extern Result hidIsUsbFullKeyControllerConnected(HidNpadIdType id, ref bool @out);
    public static extern Result hidGetNpadInterfaceType(HidNpadIdType id, ref u8 @out);
    public static extern Result hidGetNpadOfHighestBatteryLevel(ref HidNpadIdType ids, ulong count, ref HidNpadIdType @out);
    public static extern Result hidGetPalmaConnectionHandle(HidNpadIdType id, ref HidPalmaConnectionHandle @out);
    public static extern Result hidInitializePalma(HidPalmaConnectionHandle handle);
    public static extern Result hidAcquirePalmaOperationCompleteEvent(HidPalmaConnectionHandle handle, ref Event out_event, bool autoclear);
    public static extern Result hidGetPalmaOperationInfo(HidPalmaConnectionHandle handle, ref HidPalmaOperationInfo @out);
    public static extern Result hidPlayPalmaActivity(HidPalmaConnectionHandle handle, u16 val);
    public static extern Result hidSetPalmaFrModeType(HidPalmaConnectionHandle handle, HidPalmaFrModeType type);
    public static extern Result hidReadPalmaStep(HidPalmaConnectionHandle handle);
    public static extern Result hidEnablePalmaStep(HidPalmaConnectionHandle handle, bool flag);
    public static extern Result hidResetPalmaStep(HidPalmaConnectionHandle handle);
    public static extern Result hidReadPalmaApplicationSection(HidPalmaConnectionHandle handle, s32 inval0, u64 size);
    public static extern Result hidWritePalmaApplicationSection(HidPalmaConnectionHandle handle, s32 inval0, u64 size, ref HidPalmaApplicationSectionAccessBuffer buf);
    public static extern Result hidReadPalmaUniqueCode(HidPalmaConnectionHandle handle);
    public static extern Result hidSetPalmaUniqueCodeInvalid(HidPalmaConnectionHandle handle);
    public static extern Result hidWritePalmaActivityEntry(HidPalmaConnectionHandle handle, u16 unk, ref HidPalmaActivityEntry entry);
    public static extern Result hidWritePalmaRgbLedPatternEntry(HidPalmaConnectionHandle handle, u16 unk, IntPtr buffer, ulong size);
    public static extern Result hidWritePalmaWaveEntry(HidPalmaConnectionHandle handle, HidPalmaWaveSet wave_set, u16 unk, IntPtr buffer, ulong tmem_size, ulong size);
    public static extern Result hidSetPalmaDataBaseIdentificationVersion(HidPalmaConnectionHandle handle, s32 version);
    public static extern Result hidGetPalmaDataBaseIdentificationVersion(HidPalmaConnectionHandle handle);
    public static extern Result hidSuspendPalmaFeature(HidPalmaConnectionHandle handle, u32 features);
    public static extern Result hidReadPalmaPlayLog(HidPalmaConnectionHandle handle, u16 unk);
    public static extern Result hidResetPalmaPlayLog(HidPalmaConnectionHandle handle, u16 unk);
    public static extern Result hidSetIsPalmaAllConnectable(bool flag);
    public static extern Result hidSetIsPalmaPairedConnectable(bool flag);
    public static extern Result hidPairPalma(HidPalmaConnectionHandle handle);
    public static extern Result hidCancelWritePalmaWaveEntry(HidPalmaConnectionHandle handle);
    public static extern Result hidEnablePalmaBoostMode(bool flag);
    public static extern Result hidGetPalmaBluetoothAddress(HidPalmaConnectionHandle handle, ref BtdrvAddress @out);
    public static extern Result hidSetDisallowedPalmaConnection(ref BtdrvAddress addrs, s32 count);
    public static extern Result hidSetNpadCommunicationMode(HidNpadCommunicationMode mode);
    public static extern Result hidGetNpadCommunicationMode(ref HidNpadCommunicationMode @out);
    public static extern Result hidSetTouchScreenConfiguration(ref HidTouchScreenConfigurationForNx config);
    public static extern Result hidIsFirmwareUpdateNeededForNotification(ref bool @out);
}
