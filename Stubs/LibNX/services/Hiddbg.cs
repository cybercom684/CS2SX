// Auto-generated from services\hiddbg.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum HiddbgNpadButton
{
    HiddbgNpadButton_Home = (1 << 18),
    HiddbgNpadButton_Capture = (1 << 19),
}

public enum HiddbgHdlsAttribute
{
    HiddbgHdlsAttribute_HasVirtualSixAxisSensorAcceleration = (1 << 0),
    HiddbgHdlsAttribute_HasVirtualSixAxisSensorAngle = (1 << 1),
}

public unsafe struct HiddbgDebugPadAutoPilotState
{
    public u32 attributes;
    public u32 buttons;
    public HidAnalogStickState analog_stick_l;
    public HidAnalogStickState analog_stick_r;
}

public unsafe struct HiddbgMouseAutoPilotState
{
    public s32 x;
    public s32 y;
    public s32 delta_x;
    public s32 delta_y;
    public s32 wheel_delta;
    public u32 buttons;
    public u32 attributes;
}

public unsafe struct HiddbgKeyboardAutoPilotState
{
    public u64 modifiers;
    public fixed u64 keys[4];
}

public unsafe struct HiddbgSleepButtonAutoPilotState
{
    public u64 buttons;
}

public unsafe struct HiddbgHdlsHandle
{
    public u64 handle;
}

public unsafe struct HiddbgHdlsSessionId
{
    public u64 id;
}

public unsafe struct HiddbgHdlsDeviceInfoV7
{
    public u32 deviceTypeInternal;
    public u32 singleColorBody;
    public u32 singleColorButtons;
    public u8 npadInterfaceType;
    public u8 pad;
}

public unsafe struct HiddbgHdlsDeviceInfo
{
    public u8 deviceType;
    public u8 npadInterfaceType;
    public u8 pad;
    public u32 singleColorBody;
    public u32 singleColorButtons;
    public u32 colorLeftGrip;
    public u32 colorRightGrip;
}

public unsafe struct HiddbgHdlsStateV7
{
    public u8 is_powered;
    public u8 flags;
    public u8 unk_x2;
    public u32 battery_level;
    public u32 buttons;
    public HidAnalogStickState analog_stick_l;
    public HidAnalogStickState analog_stick_r;
    public u8 indicator;
    public u8 padding;
}

public unsafe struct HiddbgHdlsStateV9
{
    public u32 battery_level;
    public u32 flags;
    public u64 buttons;
    public HidAnalogStickState analog_stick_l;
    public HidAnalogStickState analog_stick_r;
    public u8 indicator;
    public u8 padding;
}

public unsafe struct HiddbgHdlsState
{
    public u32 battery_level;
    public u32 flags;
    public u64 buttons;
    public HidAnalogStickState analog_stick_l;
    public HidAnalogStickState analog_stick_r;
    public HidVector six_axis_sensor_acceleration;
    public HidVector six_axis_sensor_angle;
    public u32 attribute;
    public u8 indicator;
    public u8 padding;
}

public unsafe struct HiddbgHdlsNpadAssignmentEntry
{
    public HiddbgHdlsHandle handle;
    public u32 unk_x8;
    public u32 unk_xc;
    public u64 unk_x10;
    public u8 unk_x18;
    public u8 pad;
}

public unsafe struct HiddbgHdlsNpadAssignment
{
    public s32 total_entries;
    public u32 pad;
    public HiddbgHdlsNpadAssignmentEntry entries;
}

public unsafe struct HiddbgHdlsStateListEntryV7
{
    public HiddbgHdlsHandle handle;
    public HiddbgHdlsDeviceInfoV7 device;
    public HiddbgHdlsStateV7 state;
}

public unsafe struct HiddbgHdlsStateListV7
{
    public s32 total_entries;
    public u32 pad;
    public HiddbgHdlsStateListEntryV7 entries;
}

public unsafe struct HiddbgHdlsStateListEntryV9
{
    public HiddbgHdlsHandle handle;
    public HiddbgHdlsDeviceInfo device;
    public alignas(8) HiddbgHdlsStateV9 state;
}

public unsafe struct HiddbgHdlsStateListV9
{
    public s32 total_entries;
    public u32 pad;
    public HiddbgHdlsStateListEntryV9 entries;
}

public unsafe struct HiddbgHdlsStateListEntry
{
    public HiddbgHdlsHandle handle;
    public HiddbgHdlsDeviceInfo device;
    public alignas(8) HiddbgHdlsState state;
}

public unsafe struct HiddbgHdlsStateList
{
    public s32 total_entries;
    public u32 pad;
    public HiddbgHdlsStateListEntry entries;
}

public unsafe struct HiddbgAbstractedPadHandle
{
    public u64 handle;
}

public unsafe struct HiddbgAbstractedPadState
{
    public u32 type;
    public u8 flags;
    public u8 pad;
    public u32 singleColorBody;
    public u32 singleColorButtons;
    public u8 npadInterfaceType;
    public u8 pad2;
    public HiddbgHdlsStateV7 state;
    public u8 unused;
}

public static class Hiddbg
{
    public static extern Result hiddbgInitialize();
    public static extern void hiddbgExit();
    public static extern IntPtr hiddbgGetServiceSession();
    public static extern Result hiddbgSetDebugPadAutoPilotState(ref HiddbgDebugPadAutoPilotState state);
    public static extern Result hiddbgUnsetDebugPadAutoPilotState();
    public static extern Result hiddbgSetTouchScreenAutoPilotState(ref HidTouchState states, s32 count);
    public static extern Result hiddbgUnsetTouchScreenAutoPilotState();
    public static extern Result hiddbgSetMouseAutoPilotState(ref HiddbgMouseAutoPilotState state);
    public static extern Result hiddbgUnsetMouseAutoPilotState();
    public static extern Result hiddbgSetKeyboardAutoPilotState(ref HiddbgKeyboardAutoPilotState state);
    public static extern Result hiddbgUnsetKeyboardAutoPilotState();
    public static extern Result hiddbgDeactivateHomeButton();
    public static extern Result hiddbgSetSleepButtonAutoPilotState(ref HiddbgSleepButtonAutoPilotState state);
    public static extern Result hiddbgUnsetSleepButtonAutoPilotState();
    public static extern Result hiddbgUpdateControllerColor(u32 colorBody, u32 colorButtons, HidsysUniquePadId unique_pad_id);
    public static extern Result hiddbgUpdateDesignInfo(u32 colorBody, u32 colorButtons, u32 colorLeftGrip, u32 colorRightGrip, u8 inval, HidsysUniquePadId unique_pad_id);
    public static extern Result hiddbgAcquireOperationEventHandle(ref Event out_event, bool autoclear, HidsysUniquePadId unique_pad_id);
    public static extern Result hiddbgReadSerialFlash(u32 offset, IntPtr buffer, ulong size, HidsysUniquePadId unique_pad_id);
    public static extern Result hiddbgWriteSerialFlash(u32 offset, IntPtr buffer, ulong tmem_size, ulong size, HidsysUniquePadId unique_pad_id);
    public static extern Result hiddbgGetOperationResult(HidsysUniquePadId unique_pad_id);
    public static extern Result hiddbgGetUniquePadDeviceTypeSetInternal(HidsysUniquePadId unique_pad_id, ref u32 @out);
    public static extern Result hiddbgGetAbstractedPadHandles(ref HiddbgAbstractedPadHandle handles, s32 count, ref s32 total_out);
    public static extern Result hiddbgGetAbstractedPadState(HiddbgAbstractedPadHandle handle, ref HiddbgAbstractedPadState state);
    public static extern Result hiddbgGetAbstractedPadsState(ref HiddbgAbstractedPadHandle handles, ref HiddbgAbstractedPadState states, s32 count, ref s32 total_out);
    public static extern Result hiddbgSetAutoPilotVirtualPadState(s8 AbstractedVirtualPadId, ref HiddbgAbstractedPadState state);
    public static extern Result hiddbgUnsetAutoPilotVirtualPadState(s8 AbstractedVirtualPadId);
    public static extern Result hiddbgUnsetAllAutoPilotVirtualPadState();
    public static extern Result hiddbgAttachHdlsWorkBuffer(ref HiddbgHdlsSessionId session_id, IntPtr buffer, ulong size);
    public static extern Result hiddbgReleaseHdlsWorkBuffer(HiddbgHdlsSessionId session_id);
    public static extern Result hiddbgIsHdlsVirtualDeviceAttached(HiddbgHdlsSessionId session_id, HiddbgHdlsHandle handle, ref bool @out);
    public static extern Result hiddbgDumpHdlsNpadAssignmentState(HiddbgHdlsSessionId session_id, ref HiddbgHdlsNpadAssignment state);
    public static extern Result hiddbgDumpHdlsStates(HiddbgHdlsSessionId session_id, ref HiddbgHdlsStateList state);
    public static extern Result hiddbgApplyHdlsNpadAssignmentState(HiddbgHdlsSessionId session_id, ref HiddbgHdlsNpadAssignment state, bool flag);
    public static extern Result hiddbgApplyHdlsStateList(HiddbgHdlsSessionId session_id, ref HiddbgHdlsStateList state);
    public static extern Result hiddbgAttachHdlsVirtualDevice(ref HiddbgHdlsHandle handle, ref HiddbgHdlsDeviceInfo info);
    public static extern Result hiddbgDetachHdlsVirtualDevice(HiddbgHdlsHandle handle);
    public static extern Result hiddbgSetHdlsState(HiddbgHdlsHandle handle, ref HiddbgHdlsState state);
}
