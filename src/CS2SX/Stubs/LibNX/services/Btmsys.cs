// Auto-generated from services\btmsys.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Btmsys
{
    public static extern Result btmsysInitialize();
    public static extern void btmsysExit();
    public static extern Result btmsysGetServiceSession(ref Service srv_out);
    public static extern IntPtr btmsysGetServiceSession_IBtmSystemCore();
    public static extern Result btmsysStartGamepadPairing();
    public static extern Result btmsysCancelGamepadPairing();
    public static extern Result btmsysClearGamepadPairingDatabase();
    public static extern Result btmsysGetPairedGamepadCount(ref u8 @out);
    public static extern Result btmsysEnableRadio();
    public static extern Result btmsysDisableRadio();
    public static extern Result btmsysGetRadioOnOff(ref bool @out);
    public static extern Result btmsysAcquireRadioEvent(ref Event out_event);
    public static extern Result btmsysAcquireGamepadPairingEvent(ref Event out_event);
    public static extern Result btmsysIsGamepadPairingStarted(ref bool @out);
    public static extern Result btmsysStartAudioDeviceDiscovery();
    public static extern Result btmsysStopAudioDeviceDiscovery();
    public static extern Result btmsysIsDiscoveryingAudioDevice(ref bool @out);
    public static extern Result btmsysGetDiscoveredAudioDevice(ref BtmAudioDevice @out, s32 count, ref s32 total_out);
    public static extern Result btmsysAcquireAudioDeviceConnectionEvent(ref Event out_event);
    public static extern Result btmsysConnectAudioDevice(BtdrvAddress addr);
    public static extern Result btmsysIsConnectingAudioDevice(ref bool @out);
    public static extern Result btmsysGetConnectedAudioDevices(ref BtmAudioDevice @out, s32 count, ref s32 total_out);
    public static extern Result btmsysDisconnectAudioDevice(BtdrvAddress addr);
    public static extern Result btmsysAcquirePairedAudioDeviceInfoChangedEvent(ref Event out_event);
    public static extern Result btmsysGetPairedAudioDevices(ref BtmAudioDevice @out, s32 count, ref s32 total_out);
    public static extern Result btmsysRemoveAudioDevicePairing(BtdrvAddress addr);
    public static extern Result btmsysRequestAudioDeviceConnectionRejection();
    public static extern Result btmsysCancelAudioDeviceConnectionRejection();
}
