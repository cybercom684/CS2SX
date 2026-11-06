// Auto-generated from applets\psel.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Applets;

public enum PselUiMode
{
    PselUiMode_UserSelector = 0,
    PselUiMode_UserCreator = 1,
    PselUiMode_EnsureNetworkServiceAccountAvailable = 2,
    PselUiMode_UserIconEditor = 3,
    PselUiMode_UserNicknameEditor = 4,
    PselUiMode_UserCreatorForStarter = 5,
    PselUiMode_NintendoAccountAuthorizationRequestContext = 6,
    PselUiMode_IntroduceExternalNetworkServiceAccount = 7,
    PselUiMode_IntroduceExternalNetworkServiceAccountForRegistration = 8,
    PselUiMode_NintendoAccountNnidLinker = 9,
    PselUiMode_LicenseRequirementsForNetworkService = 10,
    PselUiMode_LicenseRequirementsForNetworkServiceWithUserContextImpl = 11,
    PselUiMode_UserCreatorForImmediateNaLoginTest = 12,
    PselUiMode_UserQualificationPromoter = 13,
}

public enum PselUserSelectionPurpose
{
    PselUserSelectionPurpose_General = 0,
    PselUserSelectionPurpose_GameCardRegistration = 1,
    PselUserSelectionPurpose_EShopLaunch = 2,
    PselUserSelectionPurpose_EShopItemShow = 3,
    PselUserSelectionPurpose_PicturePost = 4,
    PselUserSelectionPurpose_NintendoAccountLinkage = 5,
    PselUserSelectionPurpose_SettingsUpdate = 6,
    PselUserSelectionPurpose_SaveDataDeletion = 7,
    PselUserSelectionPurpose_UserMigration = 8,
    PselUserSelectionPurpose_SaveDataTransfer = 9,
}

public enum PselNintendoAccountStartupDialogType
{
    PselNintendoAccountStartupDialogType_LoginAndCreate = 0,
    PselNintendoAccountStartupDialogType_Login = 1,
    PselNintendoAccountStartupDialogType_Create = 2,
}

public unsafe struct PselUiSettingsV1
{
    public u32 mode;
    public u32 pad;
    public AccountUid invalid_uid_list;
    public u64 application_id;
    public u8 is_network_service_account_required;
    public u8 is_skip_enabled;
    public u8 unk_x92;
    public u8 is_permitted;
    public u8 show_skip_button;
    public u8 additional_select;
    public u8 unk_x96;
    public u8 unk_x97;
}

public unsafe struct PselUiSettings
{
    public PselUiSettingsV1 settings;
    public u32 unk_x98;
    public u8 unk_x9c;
}

public unsafe struct PselUserSelectionSettings
{
    public AccountUid invalid_uid_list;
    public u8 is_skip_enabled;
    public u8 is_network_service_account_required;
    public u8 show_skip_button;
    public u8 additional_select;
    public u8 is_unqualified_user_selectable;
}

public unsafe struct PselUserSelectionSettingsForSystemService
{
    public u32 purpose;
    public u8 enable_user_creation_button;
    public u8 pad;
}

public unsafe struct PselUiReturnArg
{
    public Result res;
    public AccountUid user_id;
}

public static class Psel
{
    public static extern Result pselUiCreate(ref PselUiSettings ui, PselUiMode mode);
    public static extern void pselUiAddUser(ref PselUiSettings ui, AccountUid user_id);
    public static extern Result pselUiShow(ref PselUiSettings ui, ref AccountUid out_user);
    public static extern Result pselShowUserSelectorForSystem(ref AccountUid out_user, ref PselUserSelectionSettings settings, ref PselUserSelectionSettingsForSystemService settings_system);
    public static extern Result pselShowUserSelectorForLauncher(ref AccountUid out_user, ref PselUserSelectionSettings settings, u64 application_id);
    public static extern Result pselShowUserSelector(ref AccountUid out_user, ref PselUserSelectionSettings settings);
    public static extern Result pselShowUserCreator();
    public static extern Result pselShowUserIconEditor(AccountUid user);
    public static extern Result pselShowUserNicknameEditor(AccountUid user);
    public static extern Result pselShowUserCreatorForStarter();
    public static extern Result pselShowNintendoAccountNnidLinker(AccountUid user);
    public static extern Result pselShowUserQualificationPromoter(AccountUid user);
}
