// Auto-generated from services\ldn.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum LdnServiceType
{
    LdnServiceType_User = 0,
    LdnServiceType_System = 1,
}

public enum LdnState
{
    LdnState_None = 0,
    LdnState_Initialized = 1,
    LdnState_AccessPoint = 2,
    LdnState_AccessPointCreated = 3,
    LdnState_Station = 4,
    LdnState_StationConnected = 5,
    LdnState_Error = 6,
}

public enum LdnDisconnectReason
{
    LdnDisconnectReason_None = 0,
    LdnDisconnectReason_DisconnectedByUser = 1,
    LdnDisconnectReason_DisconnectedBySystem = 2,
    LdnDisconnectReason_DestroyedByUser = 3,
    LdnDisconnectReason_DestroyedBySystem = 4,
    LdnDisconnectReason_Rejected = 5,
    LdnDisconnectReason_SignalLost = 6,
}

public enum LdnScanFilterFlag
{
    LdnScanFilterFlag_LocalCommunicationId = (1 << 0),
    LdnScanFilterFlag_SessionId = (1 << 1),
    LdnScanFilterFlag_NetworkType = (1 << 2),
    LdnScanFilterFlag_Bssid = (1 << 3),
    LdnScanFilterFlag_Ssid = (1 << 4),
    LdnScanFilterFlag_SceneId = (1 << 5),
    LdnScanFilterFlag_IntentId = LdnScanFilterFlag_LocalCommunicationId | LdnScanFilterFlag_SceneId,
    LdnScanFilterFlag_NetworkId = LdnScanFilterFlag_IntentId | LdnScanFilterFlag_SessionId,
}

public enum LdnAcceptPolicy
{
    LdnAcceptPolicy_AlwaysAccept = 0,
    LdnAcceptPolicy_AlwaysReject = 1,
    LdnAcceptPolicy_BlackList = 2,
    LdnAcceptPolicy_WhiteList = 3,
}

public enum LdnSecurityMode
{
    LdnSecurityMode_Any = 0,
    LdnSecurityMode_Product = 1,
    LdnSecurityMode_Debug = 2,
    LdnSecurityMode_SystemDebug = 3,
}

public enum LdnOperationMode
{
    LdnOperationMode_Stable = 0,
    LdnOperationMode_HighSpeed = 1,
}

public enum LdnWirelessControllerRestriction
{
    LdnWirelessControllerRestriction_Disabled = 0,
    LdnWirelessControllerRestriction_Enabled = 1,
}

public enum LdnProtocol
{
    LdnProtocol_NX = 1,
    LdnProtocol_Unknown3 = 3,
}

public unsafe struct LdnIpv4Address
{
    public u32 addr;
}

public unsafe struct LdnSubnetMask
{
    public u32 mask;
}

public unsafe struct LdnMacAddress
{
    public fixed u8 addr[6];
}

public unsafe struct LdnSsid
{
    public u8 len;
    public byte str;
}

public unsafe struct LdnNodeLatestUpdate
{
    public u8 state_change;
    public u8 reserved;
}

public unsafe struct LdnAddressEntry
{
    public LdnIpv4Address ip_addr;
    public LdnMacAddress mac_addr;
    public u8 reserved;
}

public unsafe struct LdnNodeInfo
{
    public LdnIpv4Address ip_addr;
    public LdnMacAddress mac_addr;
    public s8 node_id;
    public u8 is_connected;
    public byte user_name;
    public u8 platform;
    public s16 local_communication_version;
    public u8 reserved_x30;
}

public unsafe struct LdnUserConfig
{
    public byte user_name;
    public u8 reserved;
}

public unsafe struct LdnIntentId
{
    public s64 local_communication_id;
    public u8 reserved_x8;
    public u16 scene_id;
    public u8 reserved_xC;
}

public unsafe struct LdnSessionId
{
    public u8 random;
}

public unsafe struct LdnNetworkId
{
    public LdnIntentId intent_id;
    public LdnSessionId session_id;
}

public unsafe struct LdnCommonNetworkInfo
{
    public LdnMacAddress bssid;
    public LdnSsid ssid;
    public s16 channel;
    public s8 link_level;
    public u8 network_type;
    public u8 reserved;
}

public unsafe struct LdnNetworkInfo
{
    public LdnNetworkId network_id;
    public LdnCommonNetworkInfo common;
    public u8 server_random;
    public u16 security_mode;
    public u8 station_accept_policy;
    public u8 version;
    public u8 reserved_x14;
    public s8 node_count_max;
    public u8 node_count;
    // skipped array: LdnNodeInfo nodes[8]
    public u8 reserved_x218;
    public u16 advertise_data_size;
    public u8 advertise_data;
    public u8 reserved_x39C;
    public u64 reserved_x428;
}

public unsafe struct LdnScanFilter
{
    public LdnNetworkId network_id;
    public u32 network_type;
    public LdnMacAddress bssid;
    public LdnSsid ssid;
    public u8 reserved;
    public u32 flags;
}

public unsafe struct LdnSecurityConfig
{
    public u16 security_mode;
    public u16 passphrase_size;
    public u8 passphrase;
}

public unsafe struct LdnSecurityParameter
{
    public u8 server_random;
    public LdnSessionId session_id;
}

public unsafe struct LdnNetworkConfig
{
    public LdnIntentId intent_id;
    public s16 channel;
    public s8 node_count_max;
    public u8 reserved_x13;
    public s16 local_communication_version;
    public u8 reserved_x16;
}

public unsafe struct LdnActionFrameSettings
{
    public s64 local_communication_id;
    public u8 reserved;
    public u16 security_mode;
    public u16 passphrase_size;
    public u8 passphrase;
}

public static class Ldn
{
    public static extern Result ldnmInitialize();
    public static extern void ldnmExit();
    public static extern IntPtr ldnmGetServiceSession_MonitorService();
    public static extern Result ldnmGetState(ref LdnState @out);
    public static extern Result ldnmGetNetworkInfo(ref LdnNetworkInfo @out);
    public static extern Result ldnmGetIpv4Address(ref LdnIpv4Address addr, ref LdnSubnetMask mask);
    public static extern Result ldnmGetSecurityParameter(ref LdnSecurityParameter @out);
    public static extern Result ldnmGetNetworkConfig(ref LdnNetworkConfig @out);
    public static extern Result ldnInitialize(LdnServiceType service_type);
    public static extern void ldnExit();
    public static extern IntPtr ldnGetServiceSession_LocalCommunicationService();
    public static extern IntPtr ldnGetServiceSession_IClientProcessMonitor();
    public static extern Result ldnGetState(ref LdnState @out);
    public static extern Result ldnGetNetworkInfo(ref LdnNetworkInfo @out);
    public static extern Result ldnGetIpv4Address(ref LdnIpv4Address addr, ref LdnSubnetMask mask);
    public static extern Result ldnGetDisconnectReason(ref LdnDisconnectReason @out);
    public static extern Result ldnGetSecurityParameter(ref LdnSecurityParameter @out);
    public static extern Result ldnGetNetworkConfig(ref LdnNetworkConfig @out);
    public static extern Result ldnGetStateChangeEvent(ref Event out_event);
    public static extern Result ldnGetNetworkInfoAndHistory(ref LdnNetworkInfo network_info, ref LdnNodeLatestUpdate nodes, s32 count);
    public static extern Result ldnScan(s32 channel, ref LdnScanFilter filter, ref LdnNetworkInfo network_info, s32 count, ref s32 total_out);
    public static extern Result ldnScanPrivate(s32 channel, ref LdnScanFilter filter, ref LdnNetworkInfo network_info, s32 count, ref s32 total_out);
    public static extern Result ldnSetWirelessControllerRestriction(LdnWirelessControllerRestriction restriction);
    public static extern Result ldnSetProtocol(LdnProtocol protocol);
    public static extern Result ldnOpenAccessPoint();
    public static extern Result ldnCloseAccessPoint();
    public static extern Result ldnCreateNetwork(ref LdnSecurityConfig sec_config, ref LdnUserConfig user_config, ref LdnNetworkConfig network_config);
    public static extern Result ldnCreateNetworkPrivate(ref LdnSecurityConfig sec_config, ref LdnSecurityParameter sec_param, ref LdnUserConfig user_config, ref LdnNetworkConfig network_config, ref LdnAddressEntry addrs, s32 count);
    public static extern Result ldnDestroyNetwork();
    public static extern Result ldnReject(LdnIpv4Address addr);
    public static extern Result ldnSetAdvertiseData(IntPtr buffer, ulong size);
    public static extern Result ldnSetStationAcceptPolicy(LdnAcceptPolicy policy);
    public static extern Result ldnAddAcceptFilterEntry(LdnMacAddress addr);
    public static extern Result ldnClearAcceptFilter();
    public static extern Result ldnOpenStation();
    public static extern Result ldnCloseStation();
    public static extern Result ldnConnect(ref LdnSecurityConfig sec_config, ref LdnUserConfig user_config, s32 version, u32 option, ref LdnNetworkInfo network_info);
    public static extern Result ldnConnectPrivate(ref LdnSecurityConfig sec_config, ref LdnSecurityParameter sec_param, ref LdnUserConfig user_config, s32 version, u32 option, ref LdnNetworkConfig network_config);
    public static extern Result ldnDisconnect();
    public static extern Result ldnSetOperationMode(LdnOperationMode mode);
    public static extern Result ldnEnableActionFrame(ref LdnActionFrameSettings settings);
    public static extern Result ldnDisableActionFrame();
    public static extern Result ldnSendActionFrame(IntPtr data, ulong size, LdnMacAddress destination, LdnMacAddress bssid, s16 channel, u32 flags);
    public static extern Result ldnRecvActionFrame(IntPtr data, ulong size, ref LdnMacAddress addr0, ref LdnMacAddress addr1, ref s16 channel, ref u32 out_size, ref s32 link_level, u32 flags);
    public static extern Result ldnSetHomeChannel(s16 channel);
    public static extern Result ldnSetTxPower(s16 power);
    public static extern Result ldnResetTxPower();
}
