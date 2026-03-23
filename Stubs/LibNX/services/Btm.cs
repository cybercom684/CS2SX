// Auto-generated from services\btm.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Btm
{
    public static extern Result btmInitialize();
    public static extern void btmExit();
    public static extern IntPtr btmGetServiceSession();
    public static extern Result btmGetState(ref BtmState @out);
    public static extern Result btmGetHostDeviceProperty(ref BtmHostDeviceProperty @out);
    public static extern Result btmAcquireDeviceConditionEvent(ref Event out_event);
    public static extern Result btmLegacyGetDeviceCondition(ref BtmDeviceCondition @out);
    public static extern Result btmGetDeviceCondition(BtmProfile profile, ref BtmConnectedDeviceV13 @out, ulong count, ref s32 total_out);
    public static extern Result btmSetBurstMode(BtdrvAddress addr, bool flag);
    public static extern Result btmSetSlotMode(ref BtmDeviceSlotModeList list);
    public static extern Result btmSetBluetoothMode(BtmBluetoothMode mode);
    public static extern Result btmSetWlanMode(BtmWlanMode mode);
    public static extern Result btmAcquireDeviceInfoEvent(ref Event out_event);
    public static extern Result btmLegacyGetDeviceInfo(ref BtmDeviceInfoList @out);
    public static extern Result btmGetDeviceInfo(BtmProfile profile, ref BtmDeviceInfoV13 @out, ulong count, ref s32 total_out);
    public static extern Result btmAddDeviceInfo(ref BtmDeviceInfo info);
    public static extern Result btmRemoveDeviceInfo(BtdrvAddress addr);
    public static extern Result btmIncreaseDeviceInfoOrder(BtdrvAddress addr);
    public static extern Result btmLlrNotify(BtdrvAddress addr, s32 unk);
    public static extern Result btmEnableRadio();
    public static extern Result btmDisableRadio();
    public static extern Result btmHidDisconnect(BtdrvAddress addr);
    public static extern Result btmHidSetRetransmissionMode(BtdrvAddress addr, ref BtmZeroRetransmissionList list);
    public static extern Result btmAcquireAwakeReqEvent(ref Event out_event);
    public static extern Result btmAcquireLlrStateEvent(ref Event out_event);
    public static extern Result btmIsLlrStarted(ref bool @out);
    public static extern Result btmEnableSlotSaving(bool flag);
    public static extern Result btmProtectDeviceInfo(BtdrvAddress addr, bool flag);
    public static extern Result btmAcquireBleScanEvent(ref Event out_event);
    public static extern Result btmGetBleScanParameterGeneral(u16 parameter_id, ref BtdrvBleAdvertisePacketParameter @out);
    public static extern Result btmGetBleScanParameterSmartDevice(u16 parameter_id, ref BtdrvGattAttributeUuid @out);
    public static extern Result btmStartBleScanForGeneral(BtdrvBleAdvertisePacketParameter param);
    public static extern Result btmStopBleScanForGeneral();
    public static extern Result btmGetBleScanResultsForGeneral(ref BtdrvBleScanResult results, u8 count, ref u8 total_out);
    public static extern Result btmStartBleScanForPaired(BtdrvBleAdvertisePacketParameter param);
    public static extern Result btmStopBleScanForPaired();
    public static extern Result btmStartBleScanForSmartDevice(ref BtdrvGattAttributeUuid uuid);
    public static extern Result btmStopBleScanForSmartDevice();
    public static extern Result btmGetBleScanResultsForSmartDevice(ref BtdrvBleScanResult results, u8 count, ref u8 total_out);
    public static extern Result btmAcquireBleConnectionEvent(ref Event out_event);
    public static extern Result btmBleConnect(BtdrvAddress addr);
    public static extern Result btmBleOverrideConnection(u32 id);
    public static extern Result btmBleDisconnect(u32 connection_handle);
    public static extern Result btmBleGetConnectionState(ref BtdrvBleConnectionInfo info, u8 count, ref u8 total_out);
    public static extern Result btmBleGetGattClientConditionList(ref BtmGattClientConditionList list);
    public static extern Result btmAcquireBlePairingEvent(ref Event out_event);
    public static extern Result btmBlePairDevice(u32 connection_handle, BtdrvBleAdvertisePacketParameter param);
    public static extern Result btmBleUnpairDeviceOnBoth(u32 connection_handle, BtdrvBleAdvertisePacketParameter param);
    public static extern Result btmBleUnPairDevice(BtdrvAddress addr, BtdrvBleAdvertisePacketParameter param);
    public static extern Result btmBleGetPairedAddresses(BtdrvBleAdvertisePacketParameter param, ref BtdrvAddress addrs, u8 count, ref u8 total_out);
    public static extern Result btmAcquireBleServiceDiscoveryEvent(ref Event out_event);
    public static extern Result btmGetGattServices(u32 connection_handle, ref BtmGattService services, u8 count, ref u8 total_out);
    public static extern Result btmGetGattService(u32 connection_handle, ref BtdrvGattAttributeUuid uuid, ref BtmGattService service, ref bool flag);
    public static extern Result btmGetGattIncludedServices(u32 connection_handle, u16 service_handle, ref BtmGattService services, u8 count, ref u8 @out);
    public static extern Result btmGetBelongingService(u32 connection_handle, u16 attribute_handle, ref BtmGattService service, ref bool flag);
    public static extern Result btmGetGattCharacteristics(u32 connection_handle, u16 service_handle, ref BtmGattCharacteristic characteristics, u8 count, ref u8 total_out);
    public static extern Result btmGetGattDescriptors(u32 connection_handle, u16 char_handle, ref BtmGattDescriptor descriptors, u8 count, ref u8 total_out);
    public static extern Result btmAcquireBleMtuConfigEvent(ref Event out_event);
    public static extern Result btmConfigureBleMtu(u32 connection_handle, u16 mtu);
    public static extern Result btmGetBleMtu(u32 connection_handle, ref u16 @out);
    public static extern Result btmRegisterBleGattDataPath(ref BtmBleDataPath path);
    public static extern Result btmUnregisterBleGattDataPath(ref BtmBleDataPath path);
    public static extern Result btmRegisterAppletResourceUserId(u64 AppletResourceUserId, u32 unk);
    public static extern Result btmUnregisterAppletResourceUserId(u64 AppletResourceUserId);
    public static extern Result btmSetAppletResourceUserId(u64 AppletResourceUserId);
}
