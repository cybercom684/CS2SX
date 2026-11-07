// Auto-generated from services\acc.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum AccountServiceType
{
    AccountServiceType_Application = 0,
    AccountServiceType_System = 1,
    AccountServiceType_Administrator = 2,
}

public unsafe struct AccountProfile
{
    public Service s;
}

public unsafe struct AccountUid
{
    public fixed u64 uid[2];
}

public unsafe struct AccountUserData
{
    public u32 unk_x0;
    public u32 iconID;
    public u8 iconBackgroundColorID;
    public u8 unk_x9;
    public u8 miiID;
    public u8 unk_x20;
}

public unsafe struct AccountProfileBase
{
    public AccountUid uid;
    public u64 lastEditTimestamp;
    public byte nickname;
}

public unsafe struct AccountNetworkServiceAccountId
{
    public u64 id;
}

public static class Acc
{
    public static extern Result accountInitialize(AccountServiceType service_type);
    public static extern void accountExit();
    public static extern IntPtr accountGetServiceSession();
    public static extern Result accountGetUserCount(ref s32 user_count);
    public static extern Result accountListAllUsers(ref AccountUid uids, s32 max_uids, ref s32 actual_total);
    public static extern Result accountGetLastOpenedUser(ref AccountUid uid);
    public static extern Result accountGetProfile(ref AccountProfile @out, AccountUid uid);
    public static extern Result accountIsUserRegistrationRequestPermitted(ref bool @out);
    public static extern Result accountTrySelectUserWithoutInteraction(ref AccountUid uid, bool is_network_service_account_required);
    public static extern void accountProfileClose(ref AccountProfile profile);
    public static extern Result accountProfileGet(ref AccountProfile profile, ref AccountUserData userdata, ref AccountProfileBase profilebase);
    public static extern Result accountProfileGetImageSize(ref AccountProfile profile, ref u32 image_size);
    public static extern Result accountProfileLoadImage(ref AccountProfile profile, IntPtr buf, ulong len, ref u32 image_size);
    public static extern Result accountGetPreselectedUser(ref AccountUid uid);
}
