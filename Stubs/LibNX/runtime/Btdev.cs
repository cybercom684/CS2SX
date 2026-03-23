// Auto-generated from runtime\btdev.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Runtime;

public unsafe struct BtdevGattAttribute
{
    public u8 type;
    public BtdrvGattAttributeUuid uuid;
    public u16 handle;
    public u32 connection_handle;
}

public unsafe struct BtdevGattService
{
    public BtdevGattAttribute attr;
    public u16 instance_id;
    public u16 end_group_handle;
    public bool primary_service;
}

public unsafe struct BtdevGattCharacteristic
{
    public BtdevGattAttribute attr;
    public u16 instance_id;
    public u8 properties;
    public u64 value_size;
    public u8 value;
}

public unsafe struct BtdevGattDescriptor
{
    public BtdevGattAttribute attr;
    public u64 value_size;
    public u8 value;
}

public static class Btdev
{
    public static extern Result btdevInitialize();
    public static extern void btdevExit();
    public static extern bool btdevGattAttributeUuidIsSame(ref BtdrvGattAttributeUuid a, ref BtdrvGattAttributeUuid b);
    public static extern Result btdevAcquireBleScanEvent(ref Event out_event);
    public static extern Result btdevGetBleScanParameter(u16 parameter_id, ref BtdrvBleAdvertisePacketParameter @out);
    public static extern Result btdevGetBleScanParameter2(u16 parameter_id, ref BtdrvGattAttributeUuid @out);
    public static extern Result btdevStartBleScanGeneral(BtdrvBleAdvertisePacketParameter param);
    public static extern Result btdevStopBleScanGeneral();
    public static extern Result btdevGetBleScanResult(ref BtdrvBleScanResult results, u8 count, ref u8 total_out);
    public static extern Result btdevEnableBleAutoConnection(BtdrvBleAdvertisePacketParameter param);
    public static extern Result btdevDisableBleAutoConnection();
    public static extern Result btdevStartBleScanSmartDevice(ref BtdrvGattAttributeUuid uuid);
    public static extern Result btdevStopBleScanSmartDevice();
    public static extern Result btdevAcquireBleConnectionStateChangedEvent(ref Event out_event);
    public static extern Result btdevConnectToGattServer(BtdrvAddress addr);
    public static extern Result btdevDisconnectFromGattServer(u32 connection_handle);
    public static extern Result btdevGetBleConnectionInfoList(ref BtdrvBleConnectionInfo info, u8 count, ref u8 total_out);
    public static extern Result btdevAcquireBleServiceDiscoveryEvent(ref Event out_event);
    public static extern Result btdevGetGattServices(u32 connection_handle, ref BtdevGattService services, u8 count, ref u8 total_out);
    public static extern Result btdevGetGattService(u32 connection_handle, ref BtdrvGattAttributeUuid uuid, ref BtdevGattService service, ref bool flag);
    public static extern Result btdevAcquireBlePairingEvent(ref Event out_event);
    public static extern Result btdevPairGattServer(u32 connection_handle, BtdrvBleAdvertisePacketParameter param);
    public static extern Result btdevUnpairGattServer(u32 connection_handle, BtdrvBleAdvertisePacketParameter param);
    public static extern Result btdevUnpairGattServer2(BtdrvAddress addr, BtdrvBleAdvertisePacketParameter param);
    public static extern Result btdevGetPairedGattServerAddress(BtdrvBleAdvertisePacketParameter param, ref BtdrvAddress addrs, u8 count, ref u8 total_out);
    public static extern Result btdevAcquireBleMtuConfigEvent(ref Event out_event);
    public static extern Result btdevConfigureBleMtu(u32 connection_handle, u16 mtu);
    public static extern Result btdevGetBleMtu(u32 connection_handle, ref u16 @out);
    public static extern Result btdevAcquireBleGattOperationEvent(ref Event out_event);
    public static extern Result btdevRegisterGattOperationNotification(ref BtdrvGattAttributeUuid uuid);
    public static extern Result btdevUnregisterGattOperationNotification(ref BtdrvGattAttributeUuid uuid);
    public static extern Result btdevGetGattOperationResult(ref BtdrvBleClientGattOperationInfo @out);
    public static extern Result btdevReadGattCharacteristic(ref BtdevGattCharacteristic c);
    public static extern Result btdevWriteGattCharacteristic(ref BtdevGattCharacteristic c);
    public static extern Result btdevEnableGattCharacteristicNotification(ref BtdevGattCharacteristic c, bool flag);
    public static extern Result btdevReadGattDescriptor(ref BtdevGattDescriptor d);
    public static extern Result btdevWriteGattDescriptor(ref BtdevGattDescriptor d);
    public static extern void btdevGattAttributeCreate(ref BtdevGattAttribute a, ref BtdrvGattAttributeUuid uuid, u16 handle, u32 connection_handle);
    public static extern void btdevGattServiceCreate(ref BtdevGattService s, ref BtdrvGattAttributeUuid uuid, u16 handle, u32 connection_handle, u16 instance_id, u16 end_group_handle, bool primary_service);
    public static extern Result btdevGattServiceGetIncludedServices(ref BtdevGattService s, ref BtdevGattService services, u8 count, ref u8 total_out);
    public static extern Result btdevGattServiceGetCharacteristics(ref BtdevGattService s, ref BtdevGattCharacteristic characteristics, u8 count, ref u8 total_out);
    public static extern Result btdevGattServiceGetCharacteristic(ref BtdevGattService s, ref BtdrvGattAttributeUuid uuid, ref BtdevGattCharacteristic characteristic, ref bool flag);
    public static extern void btdevGattCharacteristicCreate(ref BtdevGattCharacteristic c, ref BtdrvGattAttributeUuid uuid, u16 handle, u32 connection_handle, u16 instance_id, u8 properties);
    public static extern Result btdevGattCharacteristicGetService(ref BtdevGattCharacteristic c, ref BtdevGattService service);
    public static extern Result btdevGattCharacteristicGetDescriptors(ref BtdevGattCharacteristic c, ref BtdevGattDescriptor descriptors, u8 count, ref u8 total_out);
    public static extern Result btdevGattCharacteristicGetDescriptor(ref BtdevGattCharacteristic c, ref BtdrvGattAttributeUuid uuid, ref BtdevGattDescriptor descriptor, ref bool flag);
    public static extern void btdevGattCharacteristicSetValue(ref BtdevGattCharacteristic c, IntPtr buffer, ulong size);
    public static extern u64 btdevGattCharacteristicGetValue(ref BtdevGattCharacteristic c, IntPtr buffer, ulong size);
    public static extern void btdevGattDescriptorCreate(ref BtdevGattDescriptor d, ref BtdrvGattAttributeUuid uuid, u16 handle, u32 connection_handle);
    public static extern Result btdevGattDescriptorGetService(ref BtdevGattDescriptor d, ref BtdevGattService service);
    public static extern Result btdevGattDescriptorGetCharacteristic(ref BtdevGattDescriptor d, ref BtdevGattCharacteristic characteristic);
    public static extern void btdevGattDescriptorSetValue(ref BtdevGattDescriptor d, IntPtr buffer, ulong size);
    public static extern u64 btdevGattDescriptorGetValue(ref BtdevGattDescriptor d, IntPtr buffer, ulong size);
}
