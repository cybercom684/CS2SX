// Auto-generated from applets\album_la.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Applets;

public enum AlbumLaArg
{
    AlbumLaArg_ShowAlbumFiles = 0,
    AlbumLaArg_ShowAllAlbumFiles = 1,
    AlbumLaArg_ShowAllAlbumFilesForHomeMenu = 2,
}

public static class Album_la
{
    public static extern Result albumLaShowAlbumFiles();
    public static extern Result albumLaShowAllAlbumFiles();
    public static extern Result albumLaShowAllAlbumFilesForHomeMenu();
}
