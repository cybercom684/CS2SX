// Auto-generated from applets\libapplet.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Applets;

public unsafe struct LibAppletArgs
{
    public u32 CommonArgs_version;
    public u32 CommonArgs_size;
    public u32 LaVersion;
    public s32 ExpectedThemeColor;
    public u8 PlayStartupSound;
    public fixed u8 pad[7];
    public u64 tick;
}

public static class Libapplet
{
    public static extern void libappletArgsCreate(ref LibAppletArgs a, u32 version);
    public static extern void libappletArgsSetPlayStartupSound(ref LibAppletArgs a, bool flag);
    public static extern Result libappletCreateWriteStorage(ref AppletStorage s, IntPtr buffer, ulong size);
    public static extern Result libappletReadStorage(ref AppletStorage s, IntPtr buffer, ulong size, ref ulong transfer_size);
    public static extern Result libappletArgsPush(ref LibAppletArgs a, ref AppletHolder h);
    public static extern Result libappletArgsPop(ref LibAppletArgs a);
    public static extern Result libappletPushInData(ref AppletHolder h, IntPtr buffer, ulong size);
    public static extern Result libappletPopOutData(ref AppletHolder h, IntPtr buffer, ulong size, ref ulong transfer_size);
    public static extern void libappletSetJumpFlag(bool flag);
    public static extern Result libappletStart(ref AppletHolder h);
    public static extern Result libappletLaunch(AppletId id, ref LibAppletArgs commonargs, IntPtr arg, ulong arg_size, IntPtr reply, ulong reply_size, ref ulong out_reply_size);
    public static extern Result libappletRequestHomeMenu();
    public static extern Result libappletRequestJumpToSystemUpdate();
    public static extern Result libappletRequestToLaunchApplication(u64 application_id, AccountUid uid, IntPtr buffer, ulong size, u32 sender);
    public static extern Result libappletRequestJumpToStory(AccountUid uid, u64 application_id);
}
