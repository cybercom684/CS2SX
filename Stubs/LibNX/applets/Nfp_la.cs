// Auto-generated from applets\nfp_la.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Applets;

public enum NfpLaStartParamTypeForAmiiboSettings
{
    NfpLaStartParamTypeForAmiiboSettings_NicknameAndOwnerSettings = 0,
    NfpLaStartParamTypeForAmiiboSettings_GameDataEraser = 1,
    NfpLaStartParamTypeForAmiiboSettings_Restorer = 2,
    NfpLaStartParamTypeForAmiiboSettings_Formatter = 3,
}

public unsafe struct NfpLaAmiiboSettingsStartParam
{
    public u8 unk_x0;
    public u8 unk_x8;
    public u8 unk_x28;
}

public unsafe struct NfpLaStartParamForAmiiboSettings
{
    public u8 unk_x0;
    public u8 type;
    public u8 flags;
    public u8 unk_x3;
    public u8 unk_x4;
    public NfpTagInfo tag_info;
    public NfpRegisterInfo register_info;
    public u8 unk_x164;
    public u8 unk_x184;
}

public unsafe struct NfpLaReturnValueForAmiiboSettings
{
    public u8 flags;
    public fixed u8 pad[3];
    public NfcDeviceHandle handle;
    public NfpTagInfo tag_info;
    public NfpRegisterInfo register_info;
    public u8 unk_x164;
}

public static class Nfp_la
{
    public static extern Result nfpLaStartNicknameAndOwnerSettings(ref NfpLaAmiiboSettingsStartParam in_param, ref NfpTagInfo in_tag_info, ref NfpRegisterInfo in_reg_info, ref NfpTagInfo out_tag_info, ref NfcDeviceHandle handle, ref bool reg_info_flag, ref NfpRegisterInfo out_reg_info);
    public static extern Result nfpLaStartGameDataEraser(ref NfpLaAmiiboSettingsStartParam in_param, ref NfpTagInfo in_tag_info, ref NfpTagInfo out_tag_info, ref NfcDeviceHandle handle);
    public static extern Result nfpLaStartRestorer(ref NfpLaAmiiboSettingsStartParam in_param, ref NfpTagInfo in_tag_info, ref NfpTagInfo out_tag_info, ref NfcDeviceHandle handle);
    public static extern Result nfpLaStartFormatter(ref NfpLaAmiiboSettingsStartParam in_param, ref NfpTagInfo out_tag_info, ref NfcDeviceHandle handle);
}
