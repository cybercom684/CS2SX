// Auto-generated from services\bt.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Bt
{
    public static extern Result btInitialize();
    public static extern void btExit();
    public static extern IntPtr btGetServiceSession();
    public static extern Result btLeClientReadCharacteristic(u32 connection_handle, bool is_primary, ref BtdrvGattId serv_id, ref BtdrvGattId char_id, u8 auth_req);
    public static extern Result btLeClientReadDescriptor(u32 connection_handle, bool is_primary, ref BtdrvGattId serv_id, ref BtdrvGattId char_id, ref BtdrvGattId desc_id, u8 auth_req);
    public static extern Result btLeClientWriteCharacteristic(u32 connection_handle, bool is_primary, ref BtdrvGattId serv_id, ref BtdrvGattId char_id, IntPtr buffer, ulong size, u8 auth_req, bool with_response);
    public static extern Result btLeClientWriteDescriptor(u32 connection_handle, bool is_primary, ref BtdrvGattId serv_id, ref BtdrvGattId char_id, ref BtdrvGattId desc_id, IntPtr buffer, ulong size, u8 auth_req);
    public static extern Result btLeClientRegisterNotification(u32 connection_handle, bool is_primary, ref BtdrvGattId serv_id, ref BtdrvGattId char_id);
    public static extern Result btLeClientDeregisterNotification(u32 connection_handle, bool is_primary, ref BtdrvGattId serv_id, ref BtdrvGattId char_id);
    public static extern Result btSetLeResponse(u8 server_if, ref BtdrvGattAttributeUuid serv_uuid, ref BtdrvGattAttributeUuid char_uuid, IntPtr buffer, ulong size);
    public static extern Result btLeSendIndication(u8 server_if, ref BtdrvGattAttributeUuid serv_uuid, ref BtdrvGattAttributeUuid char_uuid, IntPtr buffer, ulong size, bool noconfirm);
    public static extern Result btGetLeEventInfo(IntPtr buffer, ulong size, ref BtdrvBleEventType type);
    public static extern Result btRegisterBleEvent(ref Event out_event);
}
