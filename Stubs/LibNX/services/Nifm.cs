// Auto-generated from services\nifm.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum NifmServiceType
{
    NifmServiceType_User = 0,
    NifmServiceType_System = 1,
    NifmServiceType_Admin = 2,
}

public enum NifmInternetConnectionType
{
    NifmInternetConnectionType_WiFi = 1,
    NifmInternetConnectionType_Ethernet = 2,
}

public enum NifmInternetConnectionStatus
{
    NifmInternetConnectionStatus_ConnectingUnknown1 = 0,
    NifmInternetConnectionStatus_ConnectingUnknown2 = 1,
    NifmInternetConnectionStatus_ConnectingUnknown3 = 2,
    NifmInternetConnectionStatus_ConnectingUnknown4 = 3,
    NifmInternetConnectionStatus_Connected = 4,
}

public enum NifmRequestState
{
    NifmRequestState_Invalid = 0,
    NifmRequestState_Unknown1 = 1,
    NifmRequestState_OnHold = 2,
    NifmRequestState_Available = 3,
    NifmRequestState_Unknown4 = 4,
    NifmRequestState_Unknown5 = 5,
}

public unsafe struct NifmRequest
{
    public Service s;
    public Event event_request_state;
    public Event event1;
    public NifmRequestState request_state;
    public Result res;
}

public unsafe struct NifmClientId
{
    public u32 id;
}

public unsafe struct NifmIpV4Address
{
    public fixed u8 addr[4];
}

public unsafe struct NifmIpAddressSetting
{
    public u8 is_automatic;
    public NifmIpV4Address current_addr;
    public NifmIpV4Address subnet_mask;
    public NifmIpV4Address gateway;
}

public unsafe struct NifmDnsSetting
{
    public u8 is_automatic;
    public NifmIpV4Address primary_dns_server;
    public NifmIpV4Address secondary_dns_server;
}

public unsafe struct NifmProxySetting
{
    public u8 enabled;
    public u8 pad;
    public u16 port;
    public byte server;
    public u8 auto_auth_enabled;
    public byte user;
    public byte password;
    public u8 pad2;
}

public unsafe struct NifmIpSettingData
{
    public NifmIpAddressSetting ip_address_setting;
    public NifmDnsSetting dns_setting;
    public NifmProxySetting proxy_setting;
    public u16 mtu;
}

public unsafe struct NifmWirelessSettingData
{
    public u8 ssid_len;
    public byte ssid;
    public u8 unk_x22;
    public u8 pad;
    public u32 unk_x24;
    public u32 unk_x28;
    public u8 passphrase;
    public u8 pad2;
}

public unsafe struct NifmSfWirelessSettingData
{
    public u8 ssid_len;
    public byte ssid;
    public u8 unk_x21;
    public u8 unk_x22;
    public u8 unk_x23;
    public u8 passphrase;
}

public unsafe struct NifmSfNetworkProfileData
{
    public NifmIpSettingData ip_setting_data;
    public Uuid uuid;
    public byte network_name;
    public u8 unk_x112;
    public u8 unk_x113;
    public u8 unk_x114;
    public u8 unk_x115;
    public NifmSfWirelessSettingData wireless_setting_data;
    public u8 pad;
}

public unsafe struct NifmNetworkProfileData
{
    public Uuid uuid;
    public byte network_name;
    public u32 unk_x50;
    public u32 unk_x54;
    public u8 unk_x58;
    public u8 unk_x59;
    public fixed u8 pad[2];
    public NifmWirelessSettingData wireless_setting_data;
    public NifmIpSettingData ip_setting_data;
}

public static class Nifm
{
    public static extern Result nifmInitialize(NifmServiceType service_type);
    public static extern void nifmExit();
    public static extern IntPtr nifmGetServiceSession_StaticService();
    public static extern IntPtr nifmGetServiceSession_GeneralService();
    public static extern NifmClientId nifmGetClientId();
    public static extern Result nifmCreateRequest(ref NifmRequest r, bool autoclear);
    public static extern Result nifmGetCurrentNetworkProfile(ref NifmNetworkProfileData profile);
    public static extern Result nifmGetNetworkProfile(Uuid uuid, ref NifmNetworkProfileData profile);
    public static extern Result nifmSetNetworkProfile(ref NifmNetworkProfileData profile, ref Uuid uuid);
    public static extern Result nifmGetCurrentIpAddress(ref u32 @out);
    public static extern Result nifmGetCurrentIpConfigInfo(ref u32 current_addr, ref u32 subnet_mask, ref u32 gateway, ref u32 primary_dns_server, ref u32 secondary_dns_server);
    public static extern Result nifmSetWirelessCommunicationEnabled(bool enable);
    public static extern Result nifmIsWirelessCommunicationEnabled(ref bool @out);
    public static extern Result nifmGetInternetConnectionStatus(ref NifmInternetConnectionType connectionType, ref u32 wifiStrength, ref NifmInternetConnectionStatus connectionStatus);
    public static extern Result nifmIsEthernetCommunicationEnabled(ref bool @out);
    public static extern bool nifmIsAnyInternetRequestAccepted(NifmClientId id);
    public static extern Result nifmIsAnyForegroundRequestAccepted(ref bool @out);
    public static extern Result nifmPutToSleep();
    public static extern Result nifmWakeUp();
    public static extern Result nifmSetWowlDelayedWakeTime(s32 val);
    public static extern void nifmRequestClose(ref NifmRequest r);
    public static extern Result nifmGetRequestState(ref NifmRequest r, ref NifmRequestState @out);
    public static extern Result nifmGetResult(ref NifmRequest r);
    public static extern Result nifmRequestCancel(ref NifmRequest r);
    public static extern Result nifmRequestSubmit(ref NifmRequest r);
    public static extern Result nifmRequestSubmitAndWait(ref NifmRequest r);
    public static extern Result nifmRequestGetAppletInfo(ref NifmRequest r, u32 theme_color, IntPtr buffer, ulong size, ref u32 applet_id, ref u32 mode, ref u32 out_size);
    public static extern Result nifmRequestSetKeptInSleep(ref NifmRequest r, bool flag);
    public static extern Result nifmRequestRegisterSocketDescriptor(ref NifmRequest r, int sockfd);
    public static extern Result nifmRequestUnregisterSocketDescriptor(ref NifmRequest r, int sockfd);
}
