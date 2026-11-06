// Auto-generated from applets\friends_la.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Applets;

public enum FriendsLaArgType
{
    FriendsLaArgType_ShowFriendList = 0,
    FriendsLaArgType_ShowUserDetailInfo = 1,
    FriendsLaArgType_StartSendingFriendRequest = 2,
    FriendsLaArgType_ShowMethodsOfSendingFriendRequest = 3,
    FriendsLaArgType_StartFacedFriendRequest = 4,
    FriendsLaArgType_ShowReceivedFriendRequestList = 5,
    FriendsLaArgType_ShowBlockedUserList = 6,
    FriendsLaArgType_ShowMyProfile = 7,
    FriendsLaArgType_StartFriendInvitation = 8,
    FriendsLaArgType_StartSendingFriendInvitation = 9,
    FriendsLaArgType_ShowReceivedInvitationDetail = 10,
}

public unsafe struct FriendsLaArgHeader
{
    public u32 type;
    public u32 pad;
    public AccountUid uid;
}

public unsafe struct FriendsLaArgCommonData
{
    public AccountNetworkServiceAccountId id;
    public FriendsInAppScreenName first_inAppScreenName;
    public FriendsInAppScreenName second_inAppScreenName;
}

public unsafe struct FriendsLaArgV1
{
    public FriendsLaArgHeader hdr;
    public FriendsLaArgCommonData data;
}

public static class Friends_la
{
    public static extern Result friendsLaShowFriendList(AccountUid uid);
    public static extern Result friendsLaShowUserDetailInfo(AccountUid uid, AccountNetworkServiceAccountId id, ref FriendsInAppScreenName first_inAppScreenName, ref FriendsInAppScreenName second_inAppScreenName);
    public static extern Result friendsLaStartSendingFriendRequest(AccountUid uid, AccountNetworkServiceAccountId id, ref FriendsInAppScreenName first_inAppScreenName, ref FriendsInAppScreenName second_inAppScreenName);
    public static extern Result friendsLaShowMethodsOfSendingFriendRequest(AccountUid uid);
    public static extern Result friendsLaStartFacedFriendRequest(AccountUid uid);
    public static extern Result friendsLaShowReceivedFriendRequestList(AccountUid uid);
    public static extern Result friendsLaShowBlockedUserList(AccountUid uid);
    public static extern Result friendsLaShowMyProfile(AccountUid uid);
    public static extern Result friendsLaShowMyProfileForHomeMenu(AccountUid uid);
    public static extern Result friendsLaStartFriendInvitation(AccountUid uid, s32 id_count, ref FriendsFriendInvitationGameModeDescription desc, IntPtr userdata, u64 userdata_size);
    public static extern Result friendsLaStartSendingFriendInvitation(AccountUid uid, ref AccountNetworkServiceAccountId id_list, s32 id_count, ref FriendsFriendInvitationGameModeDescription desc, IntPtr userdata, u64 userdata_size);
    public static extern Result friendsLaShowReceivedInvitationDetail(AccountUid uid, FriendsFriendInvitationId invitation_id, FriendsFriendInvitationGroupId invitation_group_id);
}
