// Auto-generated from services\btdrv_types.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum BtdrvBluetoothPropertyType
{
    BtdrvBluetoothPropertyType_Name = 1,
    BtdrvBluetoothPropertyType_Address = 2,
    BtdrvBluetoothPropertyType_Unknown3 = 3,
    BtdrvBluetoothPropertyType_ClassOfDevice = 5,
    BtdrvBluetoothPropertyType_FeatureSet = 6,
}

public enum BtdrvAdapterPropertyType
{
    BtdrvAdapterPropertyType_Address = 0,
    BtdrvAdapterPropertyType_Name = 1,
    BtdrvAdapterPropertyType_ClassOfDevice = 2,
    BtdrvAdapterPropertyType_Unknown3 = 3,
}

public enum BtdrvEventType
{
    BtdrvEventType_InquiryDevice = 0,
    BtdrvEventType_InquiryStatus = 1,
    BtdrvEventType_PairingPinCodeRequest = 2,
    BtdrvEventType_SspRequest = 3,
    BtdrvEventType_Connection = 4,
    BtdrvEventType_Tsi = 5,
    BtdrvEventType_BurstMode = 6,
    BtdrvEventType_SetZeroRetransmission = 7,
    BtdrvEventType_PendingConnections = 8,
    BtdrvEventType_MoveToSecondaryPiconet = 9,
    BtdrvEventType_BluetoothCrash = 10,
    BtdrvEventTypeOld_Unknown0 = 0,
    BtdrvEventTypeOld_InquiryDevice = 3,
    BtdrvEventTypeOld_InquiryStatus = 4,
    BtdrvEventTypeOld_PairingPinCodeRequest = 5,
    BtdrvEventTypeOld_SspRequest = 6,
    BtdrvEventTypeOld_Connection = 7,
    BtdrvEventTypeOld_BluetoothCrash = 13,
}

public enum BtdrvInquiryStatus
{
    BtdrvInquiryStatus_Stopped = 0,
    BtdrvInquiryStatus_Started = 1,
}

public enum BtdrvConnectionEventType
{
    BtdrvConnectionEventType_Status = 0,
    BtdrvConnectionEventType_SspConfirmRequest = 1,
    BtdrvConnectionEventType_Suspended = 2,
}

public enum BtdrvExtEventType
{
    BtdrvExtEventType_SetTsi = 0,
    BtdrvExtEventType_ExitTsi = 1,
    BtdrvExtEventType_SetBurstMode = 2,
    BtdrvExtEventType_ExitBurstMode = 3,
    BtdrvExtEventType_SetZeroRetransmission = 4,
    BtdrvExtEventType_PendingConnections = 5,
    BtdrvExtEventType_MoveToSecondaryPiconet = 6,
}

public enum BtdrvBluetoothHhReportType
{
    BtdrvBluetoothHhReportType_Other = 0,
    BtdrvBluetoothHhReportType_Input = 1,
    BtdrvBluetoothHhReportType_Output = 2,
    BtdrvBluetoothHhReportType_Feature = 3,
}

public enum BtdrvHidEventType
{
    BtdrvHidEventType_Connection = 0,
    BtdrvHidEventType_Data = 1,
    BtdrvHidEventType_SetReport = 2,
    BtdrvHidEventType_GetReport = 3,
    BtdrvHidEventTypeOld_Connection = 0,
    BtdrvHidEventTypeOld_Data = 4,
    BtdrvHidEventTypeOld_Ext = 7,
    BtdrvHidEventTypeOld_SetReport = 8,
    BtdrvHidEventTypeOld_GetReport = 9,
}

public enum BtdrvHidConnectionStatus
{
    BtdrvHidConnectionStatus_Closed = 0,
    BtdrvHidConnectionStatus_Opened = 1,
    BtdrvHidConnectionStatus_Failed = 2,
    BtdrvHidConnectionStatusOld_Opened = 0,
    BtdrvHidConnectionStatusOld_Closed = 2,
    BtdrvHidConnectionStatusOld_Failed = 8,
}

public enum BtdrvFatalReason
{
    BtdrvFatalReason_Invalid = 0,
    BtdrvFatalReason_Unknown1 = 1,
    BtdrvFatalReason_CommandTimeout = 2,
    BtdrvFatalReason_HardwareError = 3,
    BtdrvFatalReason_Enable = 7,
    BtdrvFatalReason_Audio = 9,
}

public enum BtdrvBleEventType
{
    BtdrvBleEventType_ClientRegistration = 0,
    BtdrvBleEventType_ServerRegistration = 1,
    BtdrvBleEventType_ConnectionUpdate = 2,
    BtdrvBleEventType_PreferredConnectionParameters = 3,
    BtdrvBleEventType_ClientConnection = 4,
    BtdrvBleEventType_ServerConnection = 5,
    BtdrvBleEventType_ScanResult = 6,
    BtdrvBleEventType_ScanFilter = 7,
    BtdrvBleEventType_ClientNotify = 8,
    BtdrvBleEventType_ClientCacheSave = 9,
    BtdrvBleEventType_ClientCacheLoad = 10,
    BtdrvBleEventType_ClientConfigureMtu = 11,
    BtdrvBleEventType_ServerAddAttribute = 12,
    BtdrvBleEventType_ServerAttributeOperation = 13,
}

public enum BtdrvGattAttributeType
{
    BtdrvGattAttributeType_IncludedService = 0,
    BtdrvGattAttributeType_Characteristic = 1,
    BtdrvGattAttributeType_Descriptor = 2,
    BtdrvGattAttributeType_Service = 3,
}

public enum BtdrvGattAttributePermission
{
    BtdrvGattAttributePermission_Read = (1 << 0),
    BtdrvGattAttributePermission_ReadEncrypted = (1 << 1),
    BtdrvGattAttributePermission_ReadEncryptedMitm = (1 << 2),
    BtdrvGattAttributePermission_Write = (1 << 4),
    BtdrvGattAttributePermission_WriteEncrypted = (1 << 5),
    BtdrvGattAttributePermission_WriteEncryptedMitm = (1 << 6),
    BtdrvGattAttributePermission_WriteSigned = (1 << 7),
    BtdrvGattAttributePermission_WriteSignedMitm = (1 << 8),
    BtdrvGattAttributePermission_ReadAllowed = BtdrvGattAttributePermission_Read | BtdrvGattAttributePermission_ReadEncrypted | BtdrvGattAttributePermission_ReadEncryptedMitm,
    BtdrvGattAttributePermission_ReadAuthRequired = BtdrvGattAttributePermission_ReadEncrypted,
    BtdrvGattAttributePermission_ReadMitmRequired = BtdrvGattAttributePermission_ReadEncryptedMitm,
    BtdrvGattAttributePermission_ReadEncryptedRequired = BtdrvGattAttributePermission_ReadEncrypted | BtdrvGattAttributePermission_ReadEncryptedMitm,
    BtdrvGattAttributePermission_WriteAllowed = BtdrvGattAttributePermission_Write | BtdrvGattAttributePermission_WriteEncrypted | BtdrvGattAttributePermission_WriteEncryptedMitm | BtdrvGattAttributePermission_WriteSigned | BtdrvGattAttributePermission_WriteSignedMitm,
    BtdrvGattAttributePermission_WriteAuthRequired = BtdrvGattAttributePermission_WriteEncrypted | BtdrvGattAttributePermission_WriteSigned,
    BtdrvGattAttributePermission_WriteMitmRequired = BtdrvGattAttributePermission_WriteEncryptedMitm | BtdrvGattAttributePermission_WriteSignedMitm,
    BtdrvGattAttributePermission_WriteEncryptedRequired = BtdrvGattAttributePermission_WriteEncrypted | BtdrvGattAttributePermission_WriteEncryptedMitm,
    BtdrvGattAttributePermission_WriteSignedRequired = BtdrvGattAttributePermission_WriteSigned | BtdrvGattAttributePermission_WriteSignedMitm,
}

public enum BtdrvGattCharacteristicProperty
{
    BtdrvGattCharacteristicProperty_Broadcast = (1 << 0),
    BtdrvGattCharacteristicProperty_Read = (1 << 1),
    BtdrvGattCharacteristicProperty_WriteNoResponse = (1 << 2),
    BtdrvGattCharacteristicProperty_Write = (1 << 3),
    BtdrvGattCharacteristicProperty_Notify = (1 << 4),
    BtdrvGattCharacteristicProperty_Indicate = (1 << 5),
    BtdrvGattCharacteristicProperty_Authentication = (1 << 6),
    BtdrvGattCharacteristicProperty_ExtendedProperties = (1 << 7),
}

public enum BtdrvGattAuthReqType
{
    BtdrvGattAuthReqType_None = 0,
    BtdrvGattAuthReqType_NoMitm = 1,
    BtdrvGattAuthReqType_Mitm = 2,
    BtdrvGattAuthReqType_SignedNoMitm = 3,
    BtdrvGattAuthReqType_SignedMitm = 4,
}

public enum BtdrvBleAdBit
{
    BtdrvBleAdBit_DeviceName = (1 << 0),
    BtdrvBleAdBit_Flags = (1 << 1),
    BtdrvBleAdBit_Manufacturer = (1 << 2),
    BtdrvBleAdBit_TxPower = (1 << 3),
    BtdrvBleAdBit_Service32 = (1 << 4),
    BtdrvBleAdBit_IntRange = (1 << 5),
    BtdrvBleAdBit_Service = (1 << 6),
    BtdrvBleAdBit_ServiceSol = (1 << 7),
    BtdrvBleAdBit_ServiceData = (1 << 8),
    BtdrvBleAdBit_SignData = (1 << 9),
    BtdrvBleAdBit_Service128Sol = (1 << 10),
    BtdrvBleAdBit_Appearance = (1 << 11),
    BtdrvBleAdBit_PublicAddress = (1 << 12),
    BtdrvBleAdBit_RandomAddress = (1 << 13),
    BtdrvBleAdBit_Service32Sol = (1 << 14),
    BtdrvBleAdBit_Proprietary = (1 << 15),
    BtdrvBleAdBit_Service128 = (1 << 16),
}

public enum BtdrvBleAdFlag
{
    BtdrvBleAdFlag_None = 0,
    BtdrvBleAdFlag_LimitedDiscovery = (1 << 0),
    BtdrvBleAdFlag_GeneralDiscovery = (1 << 1),
    BtdrvBleAdFlag_BrEdrNotSupported = (1 << 2),
    BtdrvBleAdFlag_DualModeControllerSupport = (1 << 3),
    BtdrvBleAdFlag_DualModeHostSupport = (1 << 4),
}

public enum BtdrvAudioEventType
{
    BtdrvAudioEventType_None = 0,
    BtdrvAudioEventType_Connection = 1,
}

public enum BtdrvAudioOutState
{
    BtdrvAudioOutState_Stopped = 0,
    BtdrvAudioOutState_Started = 1,
}

public enum BtdrvAudioCodec
{
    BtdrvAudioCodec_Pcm = 0,
}

public unsafe struct BtdrvAddress
{
    public u8 address;
}

public unsafe struct BtdrvClassOfDevice
{
    public u8 class_of_device;
}

public unsafe struct BtdrvAdapterPropertyOld
{
    public BtdrvAddress addr;
    public BtdrvClassOfDevice class_of_device;
    public byte name;
    public u8 feature_set;
}

public unsafe struct BtdrvAdapterProperty
{
    public u8 type;
    public u8 size;
    public u8 data;
}

public unsafe struct BtdrvAdapterPropertySet
{
    public BtdrvAddress addr;
    public BtdrvClassOfDevice class_of_device;
    public byte name;
}

public unsafe struct BtdrvBluetoothPinCode
{
    public byte code;
}

public unsafe struct BtdrvPinCode
{
    public byte code;
    public u8 length;
}

public unsafe struct BtdrvHidData
{
    public u16 size;
    public u8 data;
}

public unsafe struct BtdrvHidReport
{
    public u16 size;
    public u8 data;
}

public unsafe struct BtdrvPlrStatistics
{
    public u8 unk_x0;
}

public unsafe struct BtdrvPlrList
{
    public u8 unk_x0;
}

public unsafe struct BtdrvChannelMapList
{
    public u8 unk_x0;
}

public unsafe struct BtdrvGattAttributeUuid
{
    public u32 size;
    public u8 uuid;
}

public unsafe struct BtdrvGattId
{
    public u8 instance_id;
    public fixed u8 pad[3];
    public BtdrvGattAttributeUuid uuid;
}

public unsafe struct BtdrvGattAttribute
{
    public BtdrvGattId id;
    public u16 type;
    public u16 handle;
    public u16 group_end_handle;
    public u8 property;
    public bool is_primary;
}

public unsafe struct BtdrvLeConnectionParams
{
    public BtdrvAddress addr;
    public u16 min_conn_interval;
    public u16 max_conn_interval;
    public u16 min_ce_length;
    public u16 max_ce_length;
    public u16 slave_latency;
    public u16 supervision_tout;
    public u8 preference;
    public u8 pad;
}

public unsafe struct BtdrvBleConnectionParameter
{
    public u16 min_conn_interval;
    public u16 max_conn_interval;
    public u16 min_ce_length;
    public u16 max_ce_length;
    public u16 slave_latency;
    public u16 supervision_tout;
}

public unsafe struct BtdrvBleAdvertisePacketData
{
    public u32 adv_data_mask;
    public u8 flag;
    public u8 manu_data_len;
    public u8 manu_data;
    public fixed u8 pad[1];
    public u16 appearance_data;
    public u8 num_service;
    public fixed u8 pad2[3];
    public BtdrvGattAttributeUuid uuid_val;
    public u8 service_data_len;
    public fixed u8 pad3[1];
    public u16 service_data_uuid;
    public u8 service_data;
    public bool is_scan_rsp;
    public u8 tx_power;
    public fixed u8 pad4[3];
}

public unsafe struct BtdrvBleAdvertisement
{
    public u8 size;
    public u8 type;
    public u8 data;
}

public unsafe struct BtdrvBleAdvertiseFilter
{
    public u8 index;
    public BtdrvBleAdvertisement adv;
    public u8 mask;
    public u8 mask_size;
}

public unsafe struct BtdrvBleAdvertisePacketParameter
{
    public u16 company_id;
    public fixed u8 pattern_data[6];
}

public unsafe struct BtdrvBleScanResult
{
    public u8 unk_x0;
    public BtdrvAddress addr;
    public u8 unk_x7;
    public s32 count;
    public s32 unk_x144;
}

public unsafe struct BtdrvBleConnectionInfo
{
    public u32 connection_handle;
    public BtdrvAddress addr;
    public fixed u8 pad[2];
}

public unsafe struct BtdrvLeEventInfo
{
    public u32 unk_x0;
    public u32 unk_x4;
    public u8 unk_x8;
    public fixed u8 pad[3];
    public BtdrvGattAttributeUuid uuid0;
    public BtdrvGattAttributeUuid uuid1;
    public BtdrvGattAttributeUuid uuid2;
    public u16 size;
    public u8 data;
}

public unsafe struct BtdrvBleClientGattOperationInfo
{
    public u8 unk_x0;
    public fixed u8 pad[3];
    public u32 unk_x4;
    public u8 unk_x8;
    public fixed u8 pad2[3];
    public BtdrvGattAttributeUuid uuid0;
    public BtdrvGattAttributeUuid uuid1;
    public BtdrvGattAttributeUuid uuid2;
    public u64 size;
    public u8 data;
}

public unsafe struct BtdrvPcmParameter
{
    public u32 unk_x0;
    public s32 sample_rate;
    public u32 bits_per_sample;
}

public unsafe struct BtdrvAudioControlButtonState
{
    public u8 unk_x0;
}

