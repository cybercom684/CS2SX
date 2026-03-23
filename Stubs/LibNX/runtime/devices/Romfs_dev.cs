// Auto-generated from runtime\devices\romfs_dev.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Runtime.Devices;

public unsafe struct romfs_header
{
    public u64 headerSize;
    public u64 dirHashTableOff;
    public u64 dirHashTableSize;
    public u64 dirTableOff;
    public u64 dirTableSize;
    public u64 fileHashTableOff;
    public u64 fileHashTableSize;
    public u64 fileTableOff;
    public u64 fileTableSize;
    public u64 fileDataOff;
}

public unsafe struct romfs_dir
{
    public u32 parent;
    public u32 sibling;
    public u32 childDir;
    public u32 childFile;
    public u32 nextHash;
    public u32 nameLen;
    public uint8_t name;
}

public unsafe struct romfs_file
{
    public u32 parent;
    public u32 sibling;
    public u64 dataOff;
    public u64 dataSize;
    public u32 nextHash;
    public u32 nameLen;
    public uint8_t name;
}

public static class Romfs_dev
{
    public static extern Result romfsMountSelf(ref byte name);
    public static extern Result romfsMountFromFile(FsFile file, u64 offset, ref byte name);
    public static extern Result romfsMountFromStorage(FsStorage storage, u64 offset, ref byte name);
    public static extern Result romfsMountFromCurrentProcess(ref byte name);
    public static extern Result romfsMountDataStorageFromProgram(u64 program_id, ref byte name);
    public static extern Result romfsMountFromFsdev(ref byte path, u64 offset, ref byte name);
    public static extern Result romfsMountFromDataArchive(u64 dataId, NcmStorageId storageId, ref byte name);
    public static extern Result romfsUnmount(ref byte name);
}
