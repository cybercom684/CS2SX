// Auto-generated from services\btmu.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Btmu
{
    public static extern Result btmuInitialize();
    public static extern void btmuExit();
    public static extern Result btmuGetServiceSession(ref Service srv_out);
    public static extern IntPtr btmuGetServiceSession_IBtmUserCore();
    public static extern Result btmuAcquireBleScanEvent(ref Event out_event);
    public static extern Result btmuGetBleScanFilterParameter(u16 parameter_id, ref BtdrvBleAdvertisePacketParameter @out);
    public static extern Result btmuGetBleScanFilterParameter2(u16 parameter_id, ref BtdrvGattAttributeUuid @out);
    public static extern Result btmuStartBleScanForGeneral(BtdrvBleAdvertisePacketParameter param);
    public static extern Result btmuStopBleScanForGeneral();
    public static extern Result btmuGetBleScanResultsForGeneral(ref BtdrvBleScanResult results, u8 count, ref u8 total_out);
    public static extern Result btmuStartBleScanForPaired(BtdrvBleAdvertisePacketParameter param);
    public static extern Result btmuStopBleScanForPaired();
    public static extern Result btmuStartBleScanForSmartDevice(ref BtdrvGattAttributeUuid uuid);
    public static extern Result btmuStopBleScanForSmartDevice();
    public static extern Result btmuGetBleScanResultsForSmartDevice(ref BtdrvBleScanResult results, u8 count, ref u8 total_out);
    public static extern Result btmuAcquireBleConnectionEvent(ref Event out_event);
    public static extern Result btmuBleConnect(BtdrvAddress addr);
    public static extern Result btmuBleDisconnect(u32 connection_handle);
    public static extern Result btmuBleGetConnectionState(ref BtdrvBleConnectionInfo info, u8 count, ref u8 total_out);
    public static extern Result btmuAcquireBlePairingEvent(ref Event out_event);
    public static extern Result btmuBlePairDevice(u32 connection_handle, BtdrvBleAdvertisePacketParameter param);
    public static extern Result btmuBleUnPairDevice(u32 connection_handle, BtdrvBleAdvertisePacketParameter param);
    public static extern Result btmuBleUnPairDevice2(BtdrvAddress addr, BtdrvBleAdvertisePacketParameter param);
    public static extern Result btmuBleGetPairedDevices(BtdrvBleAdvertisePacketParameter param, ref BtdrvAddress addrs, u8 count, ref u8 total_out);
    public static extern Result btmuAcquireBleServiceDiscoveryEvent(ref Event out_event);
    public static extern Result btmuGetGattServices(u32 connection_handle, ref BtmGattService services, u8 count, ref u8 total_out);
    public static extern Result btmuGetGattService(u32 connection_handle, ref BtdrvGattAttributeUuid uuid, ref BtmGattService service, ref bool flag);
    public static extern Result btmuGetGattIncludedServices(u32 connection_handle, u16 service_handle, ref BtmGattService services, u8 count, ref u8 @out);
    public static extern Result btmuGetBelongingGattService(u32 connection_handle, u16 attribute_handle, ref BtmGattService service, ref bool flag);
    public static extern Result btmuGetGattCharacteristics(u32 connection_handle, u16 service_handle, ref BtmGattCharacteristic characteristics, u8 count, ref u8 total_out);
    public static extern Result btmuGetGattDescriptors(u32 connection_handle, u16 char_handle, ref BtmGattDescriptor descriptors, u8 count, ref u8 total_out);
    public static extern Result btmuAcquireBleMtuConfigEvent(ref Event out_event);
    public static extern Result btmuConfigureBleMtu(u32 connection_handle, u16 mtu);
    public static extern Result btmuGetBleMtu(u32 connection_handle, ref u16 @out);
    public static extern Result btmuRegisterBleGattDataPath(ref BtmBleDataPath path);
    public static extern Result btmuUnregisterBleGattDataPath(ref BtmBleDataPath path);
}
