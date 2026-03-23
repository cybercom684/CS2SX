// Auto-generated from services\fsldr.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public unsafe struct FsCodeInfo
{
    public u8 signature;
    public u8 hash;
    public bool is_signed;
    public fixed u8 reserved[3];
}

public static class Fsldr
{
    public static extern Result fsldrInitialize();
    public static extern void fsldrExit();
    public static extern IntPtr fsldrGetServiceSession();
    public static extern Result fsldrOpenCodeFileSystem(ref FsCodeInfo out_code_info, u64 tid, NcmStorageId storage_id, ref byte path, FsContentAttributes attr, ref FsFileSystem @out);
    public static extern Result fsldrIsArchivedProgram(u64 pid, ref bool @out);
}
