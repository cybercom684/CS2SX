// Auto-generated from services\capssu.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Capssu
{
    public static extern Result capssuInitialize();
    public static extern void capssuExit();
    public static extern IntPtr capssuGetServiceSession();
    public static extern Result capssuSaveScreenShot(IntPtr buffer, ulong size, AlbumReportOption reportoption, AlbumImageOrientation orientation, ref CapsApplicationAlbumEntry @out);
    public static extern Result capssuSaveScreenShotWithUserData(IntPtr buffer, ulong size, AlbumReportOption reportoption, AlbumImageOrientation orientation, IntPtr userdata, ulong userdata_size, ref CapsApplicationAlbumEntry @out);
    public static extern Result capssuSaveScreenShotWithUserIds(IntPtr buffer, ulong size, AlbumReportOption reportoption, AlbumImageOrientation orientation, ref AccountUid uids, ulong uid_count, ref CapsApplicationAlbumEntry @out);
    public static extern Result capssuSaveScreenShotEx0(IntPtr buffer, ulong size, ref CapsScreenShotAttribute attr, AlbumReportOption reportoption, ref CapsApplicationAlbumEntry @out);
    public static extern Result capssuSaveScreenShotEx1(IntPtr buffer, ulong size, ref CapsScreenShotAttribute attr, AlbumReportOption reportoption, ref CapsApplicationData appdata, ref CapsApplicationAlbumEntry @out);
    public static extern Result capssuSaveScreenShotEx2(IntPtr buffer, ulong size, ref CapsScreenShotAttribute attr, AlbumReportOption reportoption, ref CapsUserIdList list, ref CapsApplicationAlbumEntry @out);
}
