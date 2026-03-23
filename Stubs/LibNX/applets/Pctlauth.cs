// Auto-generated from applets\pctlauth.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Applets;

public enum PctlAuthType
{
    PctlAuthType_Show = 0,
    PctlAuthType_RegisterPasscode = 1,
    PctlAuthType_ChangePasscode = 2,
}

public unsafe struct PctlAuthArg
{
    public u32 unk_x0;
    public PctlAuthType type;
    public u8 arg0;
    public u8 arg1;
    public u8 arg2;
    public u8 pad;
}

public static class Pctlauth
{
    public static extern Result pctlauthShow(bool flag);
    public static extern Result pctlauthShowEx(u8 arg0, u8 arg1, u8 arg2);
    public static extern Result pctlauthShowForConfiguration();
    public static extern Result pctlauthRegisterPasscode();
    public static extern Result pctlauthChangePasscode();
}
