// Auto-generated from services\nfc.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum NfpServiceType
{
    NfpServiceType_User = 0,
    NfpServiceType_Debug = 1,
    NfpServiceType_System = 2,
}

public enum NfcServiceType
{
    NfcServiceType_User = 0,
    NfcServiceType_System = 1,
}

public enum NfcState
{
    NfcState_NonInitialized = 0,
    NfcState_Initialized = 1,
}

public enum NfpDeviceState
{
    NfpDeviceState_Initialized = 0,
    NfpDeviceState_SearchingForTag = 1,
    NfpDeviceState_TagFound = 2,
    NfpDeviceState_TagRemoved = 3,
    NfpDeviceState_TagMounted = 4,
    NfpDeviceState_Unavailable = 5,
}

public enum NfcDeviceState
{
    NfcDeviceState_Initialized = 0,
    NfcDeviceState_SearchingForTag = 1,
    NfcDeviceState_TagFound = 2,
    NfcDeviceState_TagRemoved = 3,
    NfcDeviceState_TagMounted = 4,
}

public enum NfcMifareDeviceState
{
    NfcMifareDeviceState_Initialized = 0,
    NfcMifareDeviceState_SearchingForTag = 1,
    NfcMifareDeviceState_TagFound = 2,
    NfcMifareDeviceState_TagRemoved = 3,
    NfcMifareDeviceState_TagMounted = 4,
    NfcMifareDeviceState_Unavailable = 5,
}

public enum NfpApplicationAreaVersion
{
    NfpApplicationAreaVersion_3DS = 0,
    NfpApplicationAreaVersion_WiiU = 1,
    NfpApplicationAreaVersion_3DSv2 = 2,
    NfpApplicationAreaVersion_Switch = 3,
    NfpApplicationAreaVersion_Invalid = 0xFF,
}

public enum NfpDeviceType
{
    NfpDeviceType_Amiibo = 0,
}

public enum NfpMountTarget
{
    NfpMountTarget_Rom = (1 << 0),
    NfpMountTarget_Ram = (1 << 1),
    NfpMountTarget_All = NfpMountTarget_Rom | NfpMountTarget_Ram,
}

public enum NfcProtocol
{
    NfcProtocol_None = 0,
    NfcProtocol_TypeA = (1 << 0),
    NfcProtocol_TypeB = (1 << 1),
    NfcProtocol_TypeF = (1 << 2),
    NfcProtocol_All = 0xFFFFFFFF,
}

public enum NfcTagType
{
    NfcTagType_None = 0,
    NfcTagType_Type1 = (1 << 0),
    NfcTagType_Type2 = (1 << 1),
    NfcTagType_Type3 = (1 << 2),
    NfcTagType_Type4A = (1 << 3),
    NfcTagType_Type4B = (1 << 4),
    NfcTagType_Type5 = (1 << 5),
    NfcTagType_Mifare = (1 << 6),
    NfcTagType_All = 0xFFFFFFFF,
}

public enum NfcMifareCommand
{
    NfcMifareCommand_Read = 0x30,
    NfcMifareCommand_AuthA = 0x60,
    NfcMifareCommand_AuthB = 0x61,
    NfcMifareCommand_Write = 0xA0,
    NfcMifareCommand_Transfer = 0xB0,
    NfcMifareCommand_Decrement = 0xC0,
    NfcMifareCommand_Increment = 0xC1,
    NfcMifareCommand_Store = 0xC2,
}

public enum NfpAmiiboFlag
{
    NfpAmiiboFlag_Valid = (1 << 0),
    NfpAmiiboFlag_ApplicationAreaExists = (1 << 1),
}

public enum NfpBreakType
{
    NfpBreakType_Flush = 0,
    NfpBreakType_Break1 = 1,
    NfpBreakType_Break2 = 2,
}

public unsafe struct NfpDate
{
    public u16 year;
    public u8 month;
    public u8 day;
}

public unsafe struct NfcTagUid
{
    public fixed u8 uid[10];
    public u8 uid_length;
    public u8 reserved;
}

public unsafe struct NfpTagInfo
{
    public NfcTagUid uid;
    public u32 protocol;
    public u32 tag_type;
    public u8 reserved;
}

public unsafe struct NfcTagInfo
{
    public NfcTagUid uid;
    public u32 protocol;
    public u32 tag_type;
    public u8 reserved;
}

public unsafe struct NfpCommonInfo
{
    public NfpDate last_write_date;
    public u16 write_counter;
    public u16 version;
    public u32 application_area_size;
    public u8 reserved;
}

public unsafe struct NfpRegisterInfo
{
    public MiiCharInfo mii;
    public NfpDate first_write_date;
    public IntPtr amiibo_name;
    public u8 font_region;
    public u8 reserved;
}

public unsafe struct NfpRegisterInfoPrivate
{
    public MiiStoreData mii_store_data;
    public NfpDate first_write_date;
    public IntPtr amiibo_name;
    public u8 font_region;
    public u8 reserved;
}

public unsafe struct NfpAdminInfo
{
    public u64 application_id;
    public u32 access_id;
    public u16 crc32_change_counter;
    public u8 flags;
    public u8 tag_type;
    public u8 application_area_version;
    public u8 reserved;
}

public unsafe struct NfpData
{
    public u8 tag_magic;
    public u8 reserved1;
    public u16 tag_write_counter;
    public u32 crc32_1;
    public u8 reserved2;
    public NfpDate last_write_date;
    public u16 write_counter;
    public u16 version;
    public u32 application_area_size;
    public u8 reserved3;
    public MiiVer3StoreData mii_v3;
    public u8 pad;
    public u16 mii_v3_crc16;
    public MiiNfpStoreDataExtension mii_store_data_extension;
    public NfpDate first_write_date;
    public u16 amiibo_name;
    public u8 font_region;
    public u8 unknown1;
    public u32 crc32_2;
    public u32 unknown2;
    public u8 reserved4;
    public u64 application_id;
    public u32 access_id;
    public u16 settings_crc32_change_counter;
    public u8 flags;
    public u8 tag_type;
    public u8 application_area_version;
    public u8 application_id_byte;
    public u8 reserved5;
    public u8 application_area;
}

public unsafe struct NfcMifareWriteBlockParameter
{
    public u8 data;
    public u8 sector_number;
    public u8 reserved;
    public NfcSectorKey sector_key;
}

public unsafe struct NfcRequiredMcuVersionData
{
    public u64 version;
    public fixed u64 reserved[3];
}

public unsafe struct NfcDeviceHandle
{
    public u8 handle;
}

public static class Nfc
{
    public static extern Result nfpInitialize(NfpServiceType service_type);
    public static extern void nfpExit();
    public static extern Result nfcInitialize(NfcServiceType service_type);
    public static extern void nfcExit();
    public static extern Result nfcMfInitialize();
    public static extern void nfcMfExit();
    public static extern IntPtr nfpGetServiceSession();
    public static extern IntPtr nfpGetServiceSession_Interface();
    public static extern IntPtr nfcGetServiceSession();
    public static extern IntPtr nfcGetServiceSession_Interface();
    public static extern IntPtr nfcMfGetServiceSession();
    public static extern IntPtr nfcMfGetServiceSession_Interface();
    public static extern Result nfpListDevices(ref s32 total_out, ref NfcDeviceHandle @out, s32 count);
    public static extern Result nfpStartDetection(ref NfcDeviceHandle handle);
    public static extern Result nfpStopDetection(ref NfcDeviceHandle handle);
    public static extern Result nfpMount(ref NfcDeviceHandle handle, NfpDeviceType device_type, NfpMountTarget mount_target);
    public static extern Result nfpUnmount(ref NfcDeviceHandle handle);
    public static extern Result nfcListDevices(ref s32 total_out, ref NfcDeviceHandle @out, s32 count);
    public static extern Result nfcStartDetection(ref NfcDeviceHandle handle, NfcProtocol protocol);
    public static extern Result nfcStopDetection(ref NfcDeviceHandle handle);
    public static extern Result nfcMfListDevices(ref s32 total_out, ref NfcDeviceHandle @out, s32 count);
    public static extern Result nfcMfStartDetection(ref NfcDeviceHandle handle);
    public static extern Result nfcMfStopDetection(ref NfcDeviceHandle handle);
    public static extern Result nfpOpenApplicationArea(ref NfcDeviceHandle handle, u32 app_id);
    public static extern Result nfpGetApplicationArea(ref NfcDeviceHandle handle, IntPtr buf, ulong buf_size, ref u32 out_size);
    public static extern Result nfpSetApplicationArea(ref NfcDeviceHandle handle, IntPtr buf, ulong buf_size);
    public static extern Result nfpFlush(ref NfcDeviceHandle handle);
    public static extern Result nfpRestore(ref NfcDeviceHandle handle);
    public static extern Result nfpCreateApplicationArea(ref NfcDeviceHandle handle, u32 app_id, IntPtr buf, ulong buf_size);
    public static extern Result nfpRecreateApplicationArea(ref NfcDeviceHandle handle, u32 app_id, IntPtr buf, ulong buf_size);
    public static extern Result nfpGetApplicationAreaSize(ref NfcDeviceHandle handle, ref u32 out_app_area_size);
    public static extern Result nfpDeleteApplicationArea(ref NfcDeviceHandle handle);
    public static extern Result nfpExistsApplicationArea(ref NfcDeviceHandle handle, ref bool @out);
    public static extern Result nfpGetTagInfo(ref NfcDeviceHandle handle, ref NfpTagInfo @out);
    public static extern Result nfpGetRegisterInfo(ref NfcDeviceHandle handle, ref NfpRegisterInfo @out);
    public static extern Result nfpGetCommonInfo(ref NfcDeviceHandle handle, ref NfpCommonInfo @out);
    public static extern Result nfpGetModelInfo(ref NfcDeviceHandle handle, ref NfpModelInfo @out);
    public static extern Result nfpGetAdminInfo(ref NfcDeviceHandle handle, ref NfpAdminInfo @out);
    public static extern Result nfcGetTagInfo(ref NfcDeviceHandle handle, ref NfcTagInfo @out);
    public static extern Result nfcMfGetTagInfo(ref NfcDeviceHandle handle, ref NfcTagInfo @out);
    public static extern Result nfpAttachActivateEvent(ref NfcDeviceHandle handle, ref Event out_event);
    public static extern Result nfpAttachDeactivateEvent(ref NfcDeviceHandle handle, ref Event out_event);
    public static extern Result nfcAttachActivateEvent(ref NfcDeviceHandle handle, ref Event out_event);
    public static extern Result nfcAttachDeactivateEvent(ref NfcDeviceHandle handle, ref Event out_event);
    public static extern Result nfcMfAttachActivateEvent(ref NfcDeviceHandle handle, ref Event out_event);
    public static extern Result nfcMfAttachDeactivateEvent(ref NfcDeviceHandle handle, ref Event out_event);
    public static extern Result nfpGetState(ref NfcState @out);
    public static extern Result nfpGetDeviceState(ref NfcDeviceHandle handle, ref NfpDeviceState @out);
    public static extern Result nfpGetNpadId(ref NfcDeviceHandle handle, ref u32 @out);
    public static extern Result nfcGetState(ref NfcState @out);
    public static extern Result nfcGetDeviceState(ref NfcDeviceHandle handle, ref NfcDeviceState @out);
    public static extern Result nfcGetNpadId(ref NfcDeviceHandle handle, ref u32 @out);
    public static extern Result nfcMfGetState(ref NfcState @out);
    public static extern Result nfcMfGetDeviceState(ref NfcDeviceHandle handle, ref NfcMifareDeviceState @out);
    public static extern Result nfcMfGetNpadId(ref NfcDeviceHandle handle, ref u32 @out);
    public static extern Result nfpAttachAvailabilityChangeEvent(ref Event out_event);
    public static extern Result nfcAttachAvailabilityChangeEvent(ref Event out_event);
    public static extern Result nfcMfAttachAvailabilityChangeEvent(ref Event out_event);
    public static extern Result nfpFormat(ref NfcDeviceHandle handle);
    public static extern Result nfpGetRegisterInfoPrivate(ref NfcDeviceHandle handle, ref NfpRegisterInfoPrivate @out);
    public static extern Result nfpSetRegisterInfoPrivate(ref NfcDeviceHandle handle, ref NfpRegisterInfoPrivate register_info_private);
    public static extern Result nfpDeleteRegisterInfo(ref NfcDeviceHandle handle);
    public static extern Result nfpGetAll(ref NfcDeviceHandle handle, ref NfpData @out);
    public static extern Result nfpSetAll(ref NfcDeviceHandle handle, ref NfpData nfp_data);
    public static extern Result nfpFlushDebug(ref NfcDeviceHandle handle);
    public static extern Result nfpBreakTag(ref NfcDeviceHandle handle, NfpBreakType break_type);
    public static extern Result nfpReadBackupData(ref NfcDeviceHandle handle, IntPtr out_buf, ulong buf_size, ref u32 out_size);
    public static extern Result nfpWriteBackupData(ref NfcDeviceHandle handle, IntPtr buf, ulong buf_size);
    public static extern Result nfpWriteNtf(ref NfcDeviceHandle handle, u32 write_type, IntPtr buf, ulong buf_size);
    public static extern Result nfcIsNfcEnabled(ref bool @out);
    public static extern Result nfcReadMifare(ref NfcDeviceHandle handle, ref NfcMifareReadBlockData out_block_data, ref NfcMifareReadBlockParameter read_block_parameter, s32 count);
    public static extern Result nfcWriteMifare(ref NfcDeviceHandle handle, ref NfcMifareWriteBlockParameter write_block_parameter, s32 count);
    public static extern Result nfcMfReadMifare(ref NfcDeviceHandle handle, ref NfcMifareReadBlockData out_block_data, ref NfcMifareReadBlockParameter read_block_parameter, s32 count);
    public static extern Result nfcMfWriteMifare(ref NfcDeviceHandle handle, ref NfcMifareWriteBlockParameter write_block_parameter, s32 count);
    public static extern Result nfcSendCommandByPassThrough(ref NfcDeviceHandle handle, u64 timeout, IntPtr cmd_buf, ulong cmd_buf_size, IntPtr reply_buf, ulong reply_buf_size, ref u64 out_size);
    public static extern Result nfcKeepPassThroughSession(ref NfcDeviceHandle handle);
    public static extern Result nfcReleasePassThroughSession(ref NfcDeviceHandle handle);
}
