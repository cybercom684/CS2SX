// Auto-generated from services\fatal.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum FatalPolicy
{
    FatalPolicy_ErrorReportAndErrorScreen = 0,
    FatalPolicy_ErrorReport = 1,
    FatalPolicy_ErrorScreen = 2,
}

public static class Fatal
{
    public static extern void NX_NORETURN fatalThrow(Result err);
    public static extern void fatalThrowWithPolicy(Result err, FatalPolicy type);
    public static extern void fatalThrowWithContext(Result err, FatalPolicy type, ref FatalCpuContext ctx);
}
