// Auto-generated from services\lr.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public unsafe struct LrLocationResolver
{
    public Service s;
}

public unsafe struct LrRegisteredLocationResolver
{
    public Service s;
}

public static class Lr
{
    public static extern Result lrInitialize();
    public static extern void lrExit();
    public static extern IntPtr lrGetServiceSession();
    public static extern Result lrOpenLocationResolver(NcmStorageId storage, ref LrLocationResolver @out);
    public static extern Result lrOpenRegisteredLocationResolver(ref LrRegisteredLocationResolver @out);
    public static extern Result lrLrResolveProgramPath(ref LrLocationResolver lr, u64 tid, ref byte @out);
    public static extern Result lrLrRedirectProgramPath(ref LrLocationResolver lr, u64 tid, ref byte path);
    public static extern Result lrLrResolveApplicationControlPath(ref LrLocationResolver lr, u64 tid, ref byte @out);
    public static extern Result lrLrResolveApplicationHtmlDocumentPath(ref LrLocationResolver lr, u64 tid, ref byte @out);
    public static extern Result lrLrResolveDataPath(ref LrLocationResolver lr, u64 tid, ref byte @out);
    public static extern Result lrLrRedirectApplicationControlPath(ref LrLocationResolver lr, u64 tid, u64 tid2, ref byte path);
    public static extern Result lrLrRedirectApplicationHtmlDocumentPath(ref LrLocationResolver lr, u64 tid, u64 tid2, ref byte path);
    public static extern Result lrLrResolveApplicationLegalInformationPath(ref LrLocationResolver lr, u64 tid, ref byte @out);
    public static extern Result lrLrRedirectApplicationLegalInformationPath(ref LrLocationResolver lr, u64 tid, u64 tid2, ref byte path);
    public static extern Result lrLrRefresh(ref LrLocationResolver lr);
    public static extern Result lrLrEraseProgramRedirection(ref LrLocationResolver lr, u64 tid);
    public static extern Result lrRegLrResolveProgramPath(ref LrRegisteredLocationResolver reg, u64 tid, ref byte @out);
}
