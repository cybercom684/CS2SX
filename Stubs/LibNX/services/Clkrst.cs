// Auto-generated from services\clkrst.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public unsafe struct ClkrstSession
{
    public Service s;
}

public static class Clkrst
{
    public static extern Result clkrstInitialize();
    public static extern void clkrstExit();
    public static extern IntPtr clkrstGetServiceSession();
    public static extern Result clkrstOpenSession(ref ClkrstSession session_out, PcvModuleId module_id, u32 unk);
    public static extern void clkrstCloseSession(ref ClkrstSession session);
    public static extern Result clkrstSetClockRate(ref ClkrstSession session, u32 hz);
    public static extern Result clkrstGetClockRate(ref ClkrstSession session, ref u32 out_hz);
    public static extern Result clkrstGetPossibleClockRates(ref ClkrstSession session, ref u32 rates, s32 max_count, ref PcvClockRatesListType out_type, ref s32 out_count);
}
