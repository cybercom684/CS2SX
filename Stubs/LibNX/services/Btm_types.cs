// Auto-generated from services\btm_types.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum BtmState
{
    BtmState_NotInitialized = 0,
    BtmState_RadioOff = 1,
    BtmState_MinorSlept = 2,
    BtmState_RadioOffMinorSlept = 3,
    BtmState_Slept = 4,
    BtmState_RadioOffSlept = 5,
    BtmState_Initialized = 6,
    BtmState_Working = 7,
}

public enum BtmBluetoothMode
{
    BtmBluetoothMode_Dynamic2Slot = 0,
    BtmBluetoothMode_StaticJoy = 1,
}

public enum BtmWlanMode
{
    BtmWlanMode_Local4 = 0,
    BtmWlanMode_Local8 = 1,
    BtmWlanMode_None = 2,
}

public enum BtmTsiMode
{
    BtmTsiMode_0Fd3Td3Si10 = 0,
    BtmTsiMode_1Fd1Td1Si5 = 1,
    BtmTsiMode_2Fd1Td3Si10 = 2,
    BtmTsiMode_3Fd1Td5Si15 = 3,
    BtmTsiMode_4Fd3Td1Si10 = 4,
    BtmTsiMode_5Fd3Td3Si15 = 5,
    BtmTsiMode_6Fd5Td1Si15 = 6,
    BtmTsiMode_7Fd1Td3Si15 = 7,
    BtmTsiMode_8Fd3Td1Si15 = 8,
    BtmTsiMode_9Fd1Td1Si10 = 9,
    BtmTsiMode_10Fd1Td1Si15 = 10,
    BtmTsiMode_Active = 255,
}

public enum BtmSlotMode
{
    BtmSlotMode_2 = 0,
    BtmSlotMode_4 = 1,
    BtmSlotMode_6 = 2,
    BtmSlotMode_Active = 3,
}

public enum BtmProfile
{
    BtmProfile_None = 0,
    BtmProfile_Hid = 1,
    BtmProfile_Audio = 2,
}

public unsafe struct BtmBdName
{
    public byte name;
}

public unsafe struct BtmClassOfDevice
{
    public u8 class_of_device;
}

public unsafe struct BtmLinkKey
{
    public u8 link_key;
}

public unsafe struct BtmHidDeviceInfo
{
    public u16 vid;
    public u16 pid;
}

public unsafe struct BtmConnectedDeviceV1
{
    public BtdrvAddress address;
    public fixed u8 pad[2];
    public u32 unk_x8;
    public byte name;
    public u8 unk_x2C;
    public u16 vid;
    public u16 pid;
    public u8 unk_x4C;
}

public unsafe struct BtmConnectedDeviceV13
{
    public BtdrvAddress address;
    public fixed u8 pad[2];
    public u32 profile;
    public u8 unk_xC;
    public byte name;
    public u8 unk_x6C;
    public fixed u8 pad2[3];
}

public unsafe struct BtmDeviceConditionV100
{
    public u32 unk_x0;
    public u32 unk_x4;
    public u8 unk_x8;
    public u8 unk_x9;
    public u8 max_count;
    public u8 connected_count;
    // skipped array: BtmConnectedDeviceV1 devices[8]
}

public unsafe struct BtmDeviceConditionV510
{
    public u32 unk_x0;
    public u32 unk_x4;
    public u8 unk_x8;
    public fixed u8 unk_x9[2];
    public u8 max_count;
    public u8 connected_count;
    public fixed u8 pad[3];
    // skipped array: BtmConnectedDeviceV1 devices[8]
}

public unsafe struct BtmDeviceConditionV800
{
    public u32 unk_x0;
    public u32 unk_x4;
    public u8 unk_x8;
    public u8 unk_x9;
    public u8 max_count;
    public u8 connected_count;
    // skipped array: BtmConnectedDeviceV1 devices[8]
}

public unsafe struct BtmDeviceConditionV900
{
    public u32 unk_x0;
    public u8 unk_x4;
    public u8 unk_x5;
    public u8 max_count;
    public u8 connected_count;
    // skipped array: BtmConnectedDeviceV1 devices[8]
}

public unsafe struct BtmDeviceSlotMode
{
    public BtdrvAddress addr;
    public fixed u8 reserved[2];
    public u32 slot_mode;
}

public unsafe struct BtmDeviceSlotModeList
{
    public u8 device_count;
    public fixed u8 reserved[3];
    // skipped array: BtmDeviceSlotMode devices[8]
}

public unsafe struct BtmDeviceInfoList
{
    public u8 device_count;
    public fixed u8 reserved[3];
    // skipped array: BtmDeviceInfoV1 devices[10]
}

public unsafe struct BtmDeviceProperty
{
    public BtdrvAddress addr;
    public BtmClassOfDevice class_of_device;
    public BtmBdName name;
}

public unsafe struct BtmDevicePropertyList
{
    public u8 device_count;
    // skipped array: BtmDeviceProperty devices[15]
}

public unsafe struct BtmZeroRetransmissionList
{
    public u8 enabled_report_id_count;
    public u8 enabled_report_id;
}

public unsafe struct BtmGattClientConditionList
{
    public u8 unk_x0;
}

public unsafe struct BtmGattService
{
    public u8 unk_x0;
    public BtdrvGattAttributeUuid uuid;
    public u16 handle;
    public u8 unk_x1A;
    public u16 instance_id;
    public u16 end_group_handle;
    public u8 primary_service;
    public fixed u8 pad[3];
}

public unsafe struct BtmGattCharacteristic
{
    public u8 unk_x0;
    public BtdrvGattAttributeUuid uuid;
    public u16 handle;
    public u8 unk_x1A;
    public u16 instance_id;
    public u8 properties;
    public u8 unk_x1F;
}

public unsafe struct BtmGattDescriptor
{
    public u8 unk_x0;
    public BtdrvGattAttributeUuid uuid;
    public u16 handle;
    public u8 unk_x1A;
}

public unsafe struct BtmBleDataPath
{
    public u8 unk_x0;
    public fixed u8 pad[3];
    public BtdrvGattAttributeUuid uuid;
}

public unsafe struct BtmAudioDevice
{
    public BtdrvAddress addr;
    public byte name;
}

