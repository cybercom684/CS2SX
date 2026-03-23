// Auto-generated from runtime\devices\fs_dev.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Runtime.Devices;

public unsafe struct fsdev_dir_t
{
    public u32 magic;
    public FsDir fd;
    public ssize_t index;
    public ulong size;
}

public static class Fs_dev
{
    public static extern Result fsdevMountSdmc();
    public static extern Result fsdevMountSaveData(ref byte name, u64 application_id, AccountUid uid);
    public static extern Result fsdevMountSaveDataReadOnly(ref byte name, u64 application_id, AccountUid uid);
    public static extern Result fsdevMountBcatSaveData(ref byte name, u64 application_id);
    public static extern Result fsdevMountDeviceSaveData(ref byte name, u64 application_id);
    public static extern Result fsdevMountTemporaryStorage(ref byte name);
    public static extern Result fsdevMountCacheStorage(ref byte name, u64 application_id, u16 save_data_index);
    public static extern Result fsdevMountSystemSaveData(ref byte name, FsSaveDataSpaceId save_data_space_id, u64 system_save_data_id, AccountUid uid);
    public static extern Result fsdevMountSystemBcatSaveData(ref byte name, u64 system_save_data_id);
    public static extern int fsdevMountDevice(ref byte name, FsFileSystem fs);
    public static extern int fsdevUnmountDevice(ref byte name);
    public static extern Result fsdevCommitDevice(ref byte name);
    public static extern IntPtr fsdevGetDeviceFileSystem(ref byte name);
    public static extern int fsdevTranslatePath(ref byte path, ref FsFileSystem device, ref byte outpath);
    public static extern Result fsdevSetConcatenationFileAttribute(ref byte path);
    public static extern Result fsdevIsValidSignedSystemPartitionOnSdCard(ref byte name, ref bool @out);
    public static extern Result fsdevCreateFile(ref byte path, ulong size, u32 flags);
    public static extern Result fsdevDeleteDirectoryRecursively(ref byte path);
    public static extern Result fsdevUnmountAll();
    public static extern Result fsdevGetLastResult();
}
