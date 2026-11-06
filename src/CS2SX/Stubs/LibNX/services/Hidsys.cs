// Auto-generated from services\hidsys.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum HidcfgDigitalButtonAssignment
{
    HidcfgDigitalButtonAssignment_A = 0,
    HidcfgDigitalButtonAssignment_B = 1,
    HidcfgDigitalButtonAssignment_X = 2,
    HidcfgDigitalButtonAssignment_Y = 3,
    HidcfgDigitalButtonAssignment_StickL = 4,
    HidcfgDigitalButtonAssignment_StickR = 5,
    HidcfgDigitalButtonAssignment_L = 6,
    HidcfgDigitalButtonAssignment_R = 7,
    HidcfgDigitalButtonAssignment_ZL = 8,
    HidcfgDigitalButtonAssignment_ZR = 9,
    HidcfgDigitalButtonAssignment_Select = 10,
    HidcfgDigitalButtonAssignment_Start = 11,
    HidcfgDigitalButtonAssignment_Left = 12,
    HidcfgDigitalButtonAssignment_Up = 13,
    HidcfgDigitalButtonAssignment_Right = 14,
    HidcfgDigitalButtonAssignment_Down = 15,
    HidcfgDigitalButtonAssignment_LeftSL = 16,
    HidcfgDigitalButtonAssignment_LeftSR = 17,
    HidcfgDigitalButtonAssignment_RightSL = 18,
    HidcfgDigitalButtonAssignment_RightSR = 19,
    HidcfgDigitalButtonAssignment_HomeButton = 20,
    HidcfgDigitalButtonAssignment_CaptureButton = 21,
    HidcfgDigitalButtonAssignment_Invalid = 22,
}

public enum HidcfgAnalogStickRotation
{
    HidcfgAnalogStickRotation_None = 0,
    HidcfgAnalogStickRotation_Clockwise90 = 1,
    HidcfgAnalogStickRotation_Anticlockwise90 = 2,
}

public enum HidsysUniquePadType
{
    HidsysUniquePadType_Embedded = 0,
    HidsysUniquePadType_FullKeyController = 1,
    HidsysUniquePadType_RightController = 2,
    HidsysUniquePadType_LeftController = 3,
    HidsysUniquePadType_DebugPadController = 4,
}

public unsafe struct HidsysUniquePadId
{
    public u64 id;
}

public unsafe struct HidsysUniquePadSerialNumber
{
    public byte serial_number;
}

public unsafe struct HidsysNotificationLedPatternCycle
{
    public u8 ledIntensity;
    public u8 transitionSteps;
    public u8 finalStepDuration;
    public u8 pad;
}

public unsafe struct HidsysNotificationLedPattern
{
    public u8 baseMiniCycleDuration;
    public u8 totalMiniCycles;
    public u8 totalFullCycles;
    public u8 startIntensity;
    // skipped array: HidsysNotificationLedPatternCycle miniCycles[16]
    public u8 unk_x44;
    public u8 pad_x46;
}

public unsafe struct HidsysButtonConfigEmbedded
{
    public u8 unk_x0;
}

public unsafe struct HidsysButtonConfigFull
{
    public u8 unk_x0;
}

public unsafe struct HidsysButtonConfigLeft
{
    public u8 unk_x0;
}

public unsafe struct HidsysButtonConfigRight
{
    public u8 unk_x0;
}

public unsafe struct HidcfgAnalogStickAssignment
{
    public u32 rotation;
    public u8 is_paired_stick_assigned;
    public fixed u8 reserved[3];
}

public unsafe struct HidcfgButtonConfigEmbedded
{
    public u32 hardware_button_left;
    public u32 hardware_button_up;
    public u32 hardware_button_right;
    public u32 hardware_button_down;
    public u32 hardware_button_a;
    public u32 hardware_button_b;
    public u32 hardware_button_x;
    public u32 hardware_button_y;
    public u32 hardware_button_stick_l;
    public u32 hardware_button_stick_r;
    public u32 hardware_button_l;
    public u32 hardware_button_r;
    public u32 hardware_button_zl;
    public u32 hardware_button_zr;
    public u32 hardware_button_select;
    public u32 hardware_button_start;
    public u32 hardware_button_capture;
    public HidcfgAnalogStickAssignment hardware_stick_l;
    public HidcfgAnalogStickAssignment hardware_stick_r;
}

public unsafe struct HidcfgButtonConfigFull
{
    public u32 hardware_button_left;
    public u32 hardware_button_up;
    public u32 hardware_button_right;
    public u32 hardware_button_down;
    public u32 hardware_button_a;
    public u32 hardware_button_b;
    public u32 hardware_button_x;
    public u32 hardware_button_y;
    public u32 hardware_button_stick_l;
    public u32 hardware_button_stick_r;
    public u32 hardware_button_l;
    public u32 hardware_button_r;
    public u32 hardware_button_zl;
    public u32 hardware_button_zr;
    public u32 hardware_button_select;
    public u32 hardware_button_start;
    public u32 hardware_button_capture;
    public HidcfgAnalogStickAssignment hardware_stick_l;
    public HidcfgAnalogStickAssignment hardware_stick_r;
}

public unsafe struct HidcfgButtonConfigLeft
{
    public u32 hardware_button_left;
    public u32 hardware_button_up;
    public u32 hardware_button_right;
    public u32 hardware_button_down;
    public u32 hardware_button_stick_l;
    public u32 hardware_button_l;
    public u32 hardware_button_zl;
    public u32 hardware_button_select;
    public u32 hardware_button_left_sl;
    public u32 hardware_button_left_sr;
    public u32 hardware_button_capture;
    public HidcfgAnalogStickAssignment hardware_stick_l;
}

public unsafe struct HidcfgButtonConfigRight
{
    public u32 hardware_button_a;
    public u32 hardware_button_b;
    public u32 hardware_button_x;
    public u32 hardware_button_y;
    public u32 hardware_button_stick_r;
    public u32 hardware_button_r;
    public u32 hardware_button_zr;
    public u32 hardware_button_start;
    public u32 hardware_button_right_sl;
    public u32 hardware_button_right_sr;
    public HidcfgAnalogStickAssignment hardware_stick_r;
}

public unsafe struct HidcfgStorageName
{
    public u8 name;
}

public static class Hidsys
{
    public static extern Result hidsysInitialize();
    public static extern void hidsysExit();
    public static extern IntPtr hidsysGetServiceSession();
    public static extern Result hidsysSendKeyboardLockKeyEvent(u32 events);
    public static extern Result hidsysAcquireHomeButtonEventHandle(ref Event out_event, bool autoclear);
    public static extern Result hidsysActivateHomeButton();
    public static extern Result hidsysAcquireSleepButtonEventHandle(ref Event out_event, bool autoclear);
    public static extern Result hidsysActivateSleepButton();
    public static extern Result hidsysAcquireCaptureButtonEventHandle(ref Event out_event, bool autoclear);
    public static extern Result hidsysActivateCaptureButton();
    public static extern Result hidsysApplyNpadSystemCommonPolicy();
    public static extern Result hidsysGetLastActiveNpad(ref u32 @out);
    public static extern Result hidsysGetSupportedNpadStyleSetOfCallerApplet(ref u32 @out);
    public static extern Result hidsysGetNpadInterfaceType(HidNpadIdType id, ref u8 @out);
    public static extern Result hidsysGetNpadLeftRightInterfaceType(HidNpadIdType id, ref u8 out0, ref u8 out1);
    public static extern Result hidsysHasBattery(HidNpadIdType id, ref bool @out);
    public static extern Result hidsysHasLeftRightBattery(HidNpadIdType id, ref bool out0, ref bool out1);
    public static extern Result hidsysGetUniquePadsFromNpad(HidNpadIdType id, ref HidsysUniquePadId unique_pad_ids, s32 count, ref s32 total_out);
    public static extern Result hidsysEnableAppletToGetInput(bool enable);
    public static extern Result hidsysEnableHandheldHids();
    public static extern Result hidsysDisableHandheldHids();
    public static extern Result hidsysSetJoyConRailEnabled(bool enable);
    public static extern Result hidsysIsJoyConRailEnabled(ref bool @out);
    public static extern Result hidsysIsHandheldHidsEnabled(ref bool @out);
    public static extern Result hidsysIsJoyConAttachedOnAllRail(ref bool @out);
    public static extern Result hidsysIsInvertedControllerConnectedOnRail(ref bool @out);
    public static extern Result hidsysAcquireUniquePadConnectionEventHandle(ref Event out_event);
    public static extern Result hidsysGetUniquePadIds(ref HidsysUniquePadId unique_pad_ids, s32 count, ref s32 total_out);
    public static extern Result hidsysAcquireJoyDetachOnBluetoothOffEventHandle(ref Event out_event, bool autoclear);
    public static extern Result hidsysGetUniquePadBluetoothAddress(HidsysUniquePadId unique_pad_id, ref BtdrvAddress address);
    public static extern Result hidsysDisconnectUniquePad(HidsysUniquePadId unique_pad_id);
    public static extern Result hidsysGetUniquePadType(HidsysUniquePadId unique_pad_id, ref HidsysUniquePadType pad_type);
    public static extern Result hidsysGetUniquePadInterface(HidsysUniquePadId unique_pad_id, ref HidNpadInterfaceType @interface);
    public static extern Result hidsysGetUniquePadSerialNumber(HidsysUniquePadId unique_pad_id, ref HidsysUniquePadSerialNumber serial);
    public static extern Result hidsysGetUniquePadControllerNumber(HidsysUniquePadId unique_pad_id, ref u64 number);
    public static extern Result hidsysSetNotificationLedPattern(ref HidsysNotificationLedPattern pattern, HidsysUniquePadId unique_pad_id);
    public static extern Result hidsysSetNotificationLedPatternWithTimeout(ref HidsysNotificationLedPattern pattern, HidsysUniquePadId unique_pad_id, u64 timeout);
    public static extern Result hidsysIsUsbFullKeyControllerEnabled(ref bool @out);
    public static extern Result hidsysEnableUsbFullKeyController(bool flag);
    public static extern Result hidsysIsUsbConnected(HidsysUniquePadId unique_pad_id, ref bool @out);
    public static extern Result hidsysGetTouchScreenDefaultConfiguration(ref HidTouchScreenConfigurationForNx touch_screen_configuration);
    public static extern Result hidsysIsFirmwareUpdateNeededForNotification(HidsysUniquePadId unique_pad_id, ref bool @out);
    public static extern Result hidsysLegacyIsButtonConfigSupported(HidsysUniquePadId unique_pad_id, ref bool @out);
    public static extern Result hidsysIsButtonConfigSupported(BtdrvAddress addr, ref bool @out);
    public static extern Result hidsysIsButtonConfigEmbeddedSupported(ref bool @out);
    public static extern Result hidsysLegacyDeleteButtonConfig(HidsysUniquePadId unique_pad_id);
    public static extern Result hidsysDeleteButtonConfig(BtdrvAddress addr);
    public static extern Result hidsysDeleteButtonConfigEmbedded();
    public static extern Result hidsysLegacySetButtonConfigEnabled(HidsysUniquePadId unique_pad_id, bool flag);
    public static extern Result hidsysSetButtonConfigEnabled(BtdrvAddress addr, bool flag);
    public static extern Result hidsysSetButtonConfigEmbeddedEnabled(bool flag);
    public static extern Result hidsysLegacyIsButtonConfigEnabled(HidsysUniquePadId unique_pad_id, ref bool @out);
    public static extern Result hidsysIsButtonConfigEnabled(BtdrvAddress addr, ref bool @out);
    public static extern Result hidsysIsButtonConfigEmbeddedEnabled(ref bool @out);
    public static extern Result hidsysLegacySetButtonConfigEmbedded(HidsysUniquePadId unique_pad_id, ref HidsysButtonConfigEmbedded config);
    public static extern Result hidsysSetButtonConfigEmbedded(ref HidsysButtonConfigEmbedded config);
    public static extern Result hidsysLegacySetButtonConfigFull(HidsysUniquePadId unique_pad_id, ref HidsysButtonConfigFull config);
    public static extern Result hidsysSetButtonConfigFull(BtdrvAddress addr, ref HidsysButtonConfigFull config);
    public static extern Result hidsysLegacySetButtonConfigLeft(HidsysUniquePadId unique_pad_id, ref HidsysButtonConfigLeft config);
    public static extern Result hidsysSetButtonConfigLeft(BtdrvAddress addr, ref HidsysButtonConfigLeft config);
    public static extern Result hidsysLegacySetButtonConfigRight(HidsysUniquePadId unique_pad_id, ref HidsysButtonConfigRight config);
    public static extern Result hidsysSetButtonConfigRight(BtdrvAddress addr, ref HidsysButtonConfigRight config);
    public static extern Result hidsysLegacyGetButtonConfigEmbedded(HidsysUniquePadId unique_pad_id, ref HidsysButtonConfigEmbedded config);
    public static extern Result hidsysGetButtonConfigEmbedded(ref HidsysButtonConfigEmbedded config);
    public static extern Result hidsysLegacyGetButtonConfigFull(HidsysUniquePadId unique_pad_id, ref HidsysButtonConfigFull config);
    public static extern Result hidsysGetButtonConfigFull(BtdrvAddress addr, ref HidsysButtonConfigFull config);
    public static extern Result hidsysLegacyGetButtonConfigLeft(HidsysUniquePadId unique_pad_id, ref HidsysButtonConfigLeft config);
    public static extern Result hidsysGetButtonConfigLeft(BtdrvAddress addr, ref HidsysButtonConfigLeft config);
    public static extern Result hidsysLegacyGetButtonConfigRight(HidsysUniquePadId unique_pad_id, ref HidsysButtonConfigRight config);
    public static extern Result hidsysGetButtonConfigRight(BtdrvAddress addr, ref HidsysButtonConfigRight config);
    public static extern Result hidsysIsCustomButtonConfigSupported(HidsysUniquePadId unique_pad_id, ref bool @out);
    public static extern Result hidsysIsDefaultButtonConfigEmbedded(ref HidcfgButtonConfigEmbedded config, ref bool @out);
    public static extern Result hidsysIsDefaultButtonConfigFull(ref HidcfgButtonConfigFull config, ref bool @out);
    public static extern Result hidsysIsDefaultButtonConfigLeft(ref HidcfgButtonConfigLeft config, ref bool @out);
    public static extern Result hidsysIsDefaultButtonConfigRight(ref HidcfgButtonConfigRight config, ref bool @out);
    public static extern Result hidsysIsButtonConfigStorageEmbeddedEmpty(s32 index, ref bool @out);
    public static extern Result hidsysIsButtonConfigStorageFullEmpty(s32 index, ref bool @out);
    public static extern Result hidsysIsButtonConfigStorageLeftEmpty(s32 index, ref bool @out);
    public static extern Result hidsysIsButtonConfigStorageRightEmpty(s32 index, ref bool @out);
    public static extern Result hidsysGetButtonConfigStorageEmbeddedDeprecated(s32 index, ref HidcfgButtonConfigEmbedded config);
    public static extern Result hidsysGetButtonConfigStorageFullDeprecated(s32 index, ref HidcfgButtonConfigFull config);
    public static extern Result hidsysGetButtonConfigStorageLeftDeprecated(s32 index, ref HidcfgButtonConfigLeft config);
    public static extern Result hidsysGetButtonConfigStorageRightDeprecated(s32 index, ref HidcfgButtonConfigRight config);
    public static extern Result hidsysSetButtonConfigStorageEmbeddedDeprecated(s32 index, ref HidcfgButtonConfigEmbedded config);
    public static extern Result hidsysSetButtonConfigStorageFullDeprecated(s32 index, ref HidcfgButtonConfigFull config);
    public static extern Result hidsysSetButtonConfigStorageLeftDeprecated(s32 index, ref HidcfgButtonConfigLeft config);
    public static extern Result hidsysSetButtonConfigStorageRightDeprecated(s32 index, ref HidcfgButtonConfigRight config);
    public static extern Result hidsysDeleteButtonConfigStorageEmbedded(s32 index);
    public static extern Result hidsysDeleteButtonConfigStorageFull(s32 index);
    public static extern Result hidsysDeleteButtonConfigStorageLeft(s32 index);
    public static extern Result hidsysDeleteButtonConfigStorageRight(s32 index);
    public static extern Result hidsysIsUsingCustomButtonConfig(HidsysUniquePadId unique_pad_id, ref bool @out);
    public static extern Result hidsysIsAnyCustomButtonConfigEnabled(ref bool @out);
    public static extern Result hidsysSetAllCustomButtonConfigEnabled(u64 AppletResourceUserId, bool flag);
    public static extern Result hidsysSetAllDefaultButtonConfig();
    public static extern Result hidsysSetHidButtonConfigEmbedded(HidsysUniquePadId unique_pad_id, ref HidcfgButtonConfigEmbedded config);
    public static extern Result hidsysSetHidButtonConfigFull(HidsysUniquePadId unique_pad_id, ref HidcfgButtonConfigFull config);
    public static extern Result hidsysSetHidButtonConfigLeft(HidsysUniquePadId unique_pad_id, ref HidcfgButtonConfigLeft config);
    public static extern Result hidsysSetHidButtonConfigRight(HidsysUniquePadId unique_pad_id, ref HidcfgButtonConfigRight config);
    public static extern Result hidsysGetHidButtonConfigEmbedded(HidsysUniquePadId unique_pad_id, ref HidcfgButtonConfigEmbedded config);
    public static extern Result hidsysGetHidButtonConfigFull(HidsysUniquePadId unique_pad_id, ref HidcfgButtonConfigFull config);
    public static extern Result hidsysGetHidButtonConfigLeft(HidsysUniquePadId unique_pad_id, ref HidcfgButtonConfigLeft config);
    public static extern Result hidsysGetHidButtonConfigRight(HidsysUniquePadId unique_pad_id, ref HidcfgButtonConfigRight config);
    public static extern Result hidsysGetButtonConfigStorageEmbedded(s32 index, ref HidcfgButtonConfigEmbedded config, ref HidcfgStorageName name);
    public static extern Result hidsysGetButtonConfigStorageFull(s32 index, ref HidcfgButtonConfigFull config, ref HidcfgStorageName name);
    public static extern Result hidsysGetButtonConfigStorageLeft(s32 index, ref HidcfgButtonConfigLeft config, ref HidcfgStorageName name);
    public static extern Result hidsysGetButtonConfigStorageRight(s32 index, ref HidcfgButtonConfigRight config, ref HidcfgStorageName name);
    public static extern Result hidsysSetButtonConfigStorageEmbedded(s32 index, ref HidcfgButtonConfigEmbedded config, ref HidcfgStorageName name);
    public static extern Result hidsysSetButtonConfigStorageFull(s32 index, ref HidcfgButtonConfigFull config, ref HidcfgStorageName name);
    public static extern Result hidsysSetButtonConfigStorageLeft(s32 index, ref HidcfgButtonConfigLeft config, ref HidcfgStorageName name);
    public static extern Result hidsysSetButtonConfigStorageRight(s32 index, ref HidcfgButtonConfigRight config, ref HidcfgStorageName name);
}
