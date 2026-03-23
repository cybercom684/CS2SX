// Auto-generated from services\friends.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum FriendsServiceType
{
    FriendsServiceType_User = 0,
    FriendsServiceType_Viewer = 1,
    FriendsServiceType_Manager = 2,
    FriendsServiceType_System = 3,
    FriendsServiceType_Administrator = 4,
}

public unsafe struct FriendsInAppScreenName
{
    public byte name;
    public u64 languageCode;
}

public unsafe struct FriendsFriendInvitationGameModeDescription
{
    public u8 unk_x0;
}

public unsafe struct FriendsFriendInvitationId
{
    public u64 id;
}

public unsafe struct FriendsFriendInvitationGroupId
{
    public u64 id;
}

public unsafe struct FriendsUserSetting
{
    public AccountUid uid;
    public u32 presence_permission;
    public u32 play_log_permission;
    public u64 friend_request_reception;
    public byte friend_code;
    public u64 friend_code_next_issuable_time;
    public u8 unk_x48;
}

public static class Friends
{
    public static extern Result friendsInitialize(FriendsServiceType service_type);
    public static extern void friendsExit();
    public static extern IntPtr friendsGetServiceSession();
    public static extern IntPtr friendsGetServiceSession_IFriendsService();
    public static extern Result friendsGetUserSetting(AccountUid uid, ref FriendsUserSetting user_setting);
    public static extern Result friendsTryPopFriendInvitationNotificationInfo(ref AccountUid uid, IntPtr buffer, u64 size, ref u64 out_size);
}
