// Auto-generated from services\lp2p.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum Lp2pServiceType
{
    Lp2pServiceType_App = 0,
    Lp2pServiceType_System = 1,
}

public unsafe struct Lp2pMacAddress
{
    public fixed u8 addr[6];
}

public unsafe struct Lp2pGroupId
{
    public u8 id;
}

public unsafe struct Lp2pGroupInfo
{
    public u8 unk_x0;
    public u64 local_communication_id;
    public Lp2pGroupId group_id;
    public byte service_name;
    public s8 flags_count;
    public s8 flags;
    public u8 supported_platform;
    public s8 member_count_max;
    public u8 unk_x82;
    public u8 unk_x83;
    public u16 frequency;
    public s16 channel;
    public u8 network_mode;
    public u8 performance_requirement;
    public u8 security_type;
    public s8 static_aes_key_index;
    public u8 unk_x8C;
    public u8 priority;
    public u8 stealth_enabled;
    public u8 unk_x8F;
    public u8 unk_x90;
    public u8 preshared_key_binary_size;
    public u8 preshared_key;
}

public unsafe struct Lp2pScanResult
{
    public Lp2pGroupInfo group_info;
    public u8 unk_x200;
    public u8 unk_x201;
    public u16 advertise_data_size;
    public u8 advertise_data;
    public u8 unk_x288;
}

public unsafe struct Lp2pNodeInfo
{
    public u8 ip_addr;
    public u8 unk_x20;
    public Lp2pMacAddress mac_addr;
    public u8 unk_x2A;
}

public unsafe struct Lp2pIpConfig
{
    public u8 unk_x0;
    public u8 ip_addr;
    public u8 subnet_mask;
    public u8 gateway;
    public u8 unk_x80;
}

public static class Lp2p
{
    public static extern Result lp2pInitialize(Lp2pServiceType service_type);
    public static extern void lp2pExit();
    public static extern IntPtr lp2pGetServiceSession_INetworkService();
    public static extern IntPtr lp2pGetServiceSession_INetworkServiceMonitor();
    public static extern void lp2pCreateGroupInfo(ref Lp2pGroupInfo info);
    public static extern void lp2pCreateGroupInfoScan(ref Lp2pGroupInfo info);
    public static extern void lp2pGroupInfoSetServiceName(ref Lp2pGroupInfo info, ref byte name);
    public static extern void lp2pGroupInfoSetFlags(ref Lp2pGroupInfo info, ref s8 flags, ulong count);
    public static extern void lp2pGroupInfoSetPresharedKey(ref Lp2pGroupInfo info, IntPtr key, ulong size);
    public static extern Result lp2pGroupInfoSetPassphrase(ref Lp2pGroupInfo info, ref byte passphrase);
    public static extern Result lp2pScan(ref Lp2pGroupInfo info, ref Lp2pScanResult results, s32 count, ref s32 total_out);
    public static extern Result lp2pCreateGroup(ref Lp2pGroupInfo info);
    public static extern Result lp2pDestroyGroup();
    public static extern Result lp2pSetAdvertiseData(IntPtr buffer, ulong size);
    public static extern Result lp2pSendToOtherGroup(IntPtr buffer, ulong size, Lp2pMacAddress addr, Lp2pGroupId group_id, s16 frequency, s16 channel, u32 flags);
    public static extern Result lp2pRecvFromOtherGroup(IntPtr buffer, ulong size, u32 flags, ref Lp2pMacAddress addr, ref u16 unk0, ref s32 unk1, ref u64 out_size, ref s32 unk2);
    public static extern Result lp2pAddAcceptableGroupId(Lp2pGroupId group_id);
    public static extern Result lp2pRemoveAcceptableGroupId();
    public static extern Result lp2pAttachNetworkInterfaceStateChangeEvent(ref Event out_event);
    public static extern Result lp2pGetNetworkInterfaceLastError();
    public static extern Result lp2pGetRole(ref u8 @out);
    public static extern Result lp2pGetAdvertiseData(IntPtr buffer, ulong size, ref u16 transfer_size, ref u16 original_size);
    public static extern Result lp2pGetAdvertiseData2(IntPtr buffer, ulong size, ref u16 transfer_size, ref u16 original_size);
    public static extern Result lp2pGetGroupInfo(ref Lp2pGroupInfo @out);
    public static extern Result lp2pJoin(ref Lp2pGroupInfo @out, ref Lp2pGroupInfo info);
    public static extern Result lp2pGetGroupOwner(ref Lp2pNodeInfo @out);
    public static extern Result lp2pGetIpConfig(ref Lp2pIpConfig @out);
    public static extern Result lp2pLeave(ref u32 @out);
    public static extern Result lp2pAttachJoinEvent(ref Event out_event);
    public static extern Result lp2pGetMembers(ref Lp2pNodeInfo members, s32 count, ref s32 total_out);
}
