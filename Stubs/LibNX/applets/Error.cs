// Auto-generated from applets\error.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Applets;

public enum ErrorType
{
    ErrorType_Normal = 0,
    ErrorType_System = 1,
    ErrorType_Application = 2,
    ErrorType_Eula = 3,
    ErrorType_Pctl = 4,
    ErrorType_Record = 5,
    ErrorType_SystemUpdateEula = 8,
}

public enum ErrorContextType
{
    ErrorContextType_None = 0,
    ErrorContextType_Http = 1,
    ErrorContextType_FileSystem = 2,
    ErrorContextType_WebMediaPlayer = 3,
    ErrorContextType_LocalContentShare = 4,
}

public unsafe struct ErrorCode
{
    public u32 low;
    public u32 desc;
}

public unsafe struct ErrorContext
{
    public u8 type;
    public fixed u8 pad[7];
    public u8 data;
    public Result res;
}

public unsafe struct ErrorCommonHeader
{
    public u8 type;
    public u8 jumpFlag;
    public fixed u8 unk_x2[3];
    public u8 contextFlag;
    public u8 resultFlag;
    public u8 contextFlag2;
}

public unsafe struct ErrorCommonArg
{
    public ErrorCommonHeader hdr;
    public ErrorCode errorCode;
    public Result res;
}

public unsafe struct ErrorPctlArg
{
    public ErrorCommonHeader hdr;
    public Result res;
}

public unsafe struct ErrorResultBacktrace
{
    public s32 count;
    public Result backtrace;
}

public unsafe struct ErrorEulaArg
{
    public ErrorCommonHeader hdr;
    public SetRegion regionCode;
}

public unsafe struct ErrorEulaData
{
    public u8 data;
}

public unsafe struct ErrorRecordArg
{
    public ErrorCommonHeader hdr;
    public ErrorCode errorCode;
    public u64 timestamp;
}

public unsafe struct ErrorSystemArg
{
    public ErrorCommonHeader hdr;
    public ErrorCode errorCode;
    public u64 languageCode;
    public byte dialogMessage;
    public byte fullscreenMessage;
}

public unsafe struct ErrorSystemConfig
{
    public ErrorSystemArg arg;
    public ErrorContext ctx;
}

public unsafe struct ErrorApplicationConfig
{
    public ErrorApplicationArg arg;
}

public static class Error
{
    public static extern Result errorResultShow(Result res, bool jumpFlag, ref ErrorContext ctx);
    public static extern Result errorCodeShow(ErrorCode errorCode, bool jumpFlag, ref ErrorContext ctx);
    public static extern Result errorResultBacktraceCreate(ref ErrorResultBacktrace backtrace, s32 count, ref Result entries);
    public static extern Result errorResultBacktraceShow(Result res, ref ErrorResultBacktrace backtrace);
    public static extern Result errorEulaShow(SetRegion RegionCode);
    public static extern Result errorSystemUpdateEulaShow(SetRegion RegionCode, ref ErrorEulaData eula);
    public static extern Result errorCodeRecordShow(ErrorCode errorCode, u64 timestamp);
    public static extern Result errorSystemCreate(ref ErrorSystemConfig c, ref byte dialog_message, ref byte fullscreen_message);
    public static extern Result errorSystemShow(ref ErrorSystemConfig c);
    public static extern void errorSystemSetContext(ref ErrorSystemConfig c, ref ErrorContext ctx);
    public static extern Result errorApplicationCreate(ref ErrorApplicationConfig c, ref byte dialog_message, ref byte fullscreen_message);
    public static extern Result errorApplicationShow(ref ErrorApplicationConfig c);
}
