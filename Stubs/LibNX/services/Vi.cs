// Auto-generated from services\vi.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum ViServiceType
{
    ViServiceType_Default = -1,
    ViServiceType_Application = 0,
    ViServiceType_System = 1,
    ViServiceType_Manager = 2,
}

public enum ViLayerFlags
{
    ViLayerFlags_Default = 0x1,
}

public enum ViScalingMode
{
    ViScalingMode_None = 0x0,
    ViScalingMode_FitToLayer = 0x2,
    ViScalingMode_PreserveAspectRatio = 0x4,
    ViScalingMode_Default = ViScalingMode_FitToLayer,
}

public enum ViPowerState
{
    ViPowerState_Off = 0,
    ViPowerState_NotScanning = 1,
    ViPowerState_On = 2,
    ViPowerState_On_Deprecated = 1,
}

public enum ViLayerStack
{
    ViLayerStack_Default = 0,
    ViLayerStack_Lcd = 1,
    ViLayerStack_Screenshot = 2,
    ViLayerStack_Recording = 3,
    ViLayerStack_LastFrame = 4,
    ViLayerStack_Arbitrary = 5,
    ViLayerStack_ApplicationForDebug = 6,
    ViLayerStack_Null = 10,
}

public unsafe struct ViDisplayName
{
    public byte data;
}

public unsafe struct ViDisplay
{
    public u64 display_id;
    public ViDisplayName display_name;
    public bool initialized;
}

public unsafe struct ViLayer
{
    public u64 layer_id;
    public u32 igbp_binder_obj_id;
    public bool initialized : _1;
    public bool stray_layer : _1;
}

public static class Vi
{
    public static extern Result viInitialize(ViServiceType service_type);
    public static extern void viExit();
    public static extern IntPtr viGetSession_IApplicationDisplayService();
    public static extern IntPtr viGetSession_IHOSBinderDriverRelay();
    public static extern IntPtr viGetSession_ISystemDisplayService();
    public static extern IntPtr viGetSession_IManagerDisplayService();
    public static extern IntPtr viGetSession_IHOSBinderDriverIndirect();
    public static extern Result viSetContentVisibility(bool v);
    public static extern Result viOpenDisplay(ref byte display_name, ref ViDisplay display);
    public static extern Result viCloseDisplay(ref ViDisplay display);
    public static extern Result viGetDisplayResolution(ref ViDisplay display, ref s32 width, ref s32 height);
    public static extern Result viGetDisplayLogicalResolution(ref ViDisplay display, ref s32 width, ref s32 height);
    public static extern Result viSetDisplayMagnification(ref ViDisplay display, s32 x, s32 y, s32 width, s32 height);
    public static extern Result viGetDisplayVsyncEvent(ref ViDisplay display, ref Event event_out);
    public static extern Result viSetDisplayPowerState(ref ViDisplay display, ViPowerState state);
    public static extern Result viSetDisplayAlpha(ref ViDisplay display, float alpha);
    public static extern Result viGetZOrderCountMin(ref ViDisplay display, ref s32 z);
    public static extern Result viGetZOrderCountMax(ref ViDisplay display, ref s32 z);
    public static extern Result viCreateLayer(ref ViDisplay display, ref ViLayer layer);
    public static extern Result viCreateManagedLayer(ref ViDisplay display, ViLayerFlags layer_flags, u64 aruid, ref u64 layer_id);
    public static extern Result viSetLayerSize(ref ViLayer layer, s32 width, s32 height);
    public static extern Result viSetLayerZ(ref ViLayer layer, s32 z);
    public static extern Result viSetLayerPosition(ref ViLayer layer, float x, float y);
    public static extern Result viCloseLayer(ref ViLayer layer);
    public static extern Result viDestroyManagedLayer(ref ViLayer layer);
    public static extern Result viSetLayerScalingMode(ref ViLayer layer, ViScalingMode scaling_mode);
    public static extern Result viGetIndirectLayerImageMap(IntPtr buffer, ulong size, s32 width, s32 height, u64 IndirectLayerConsumerHandle, ref u64 out_size, ref u64 out_stride);
    public static extern Result viGetIndirectLayerImageRequiredMemoryInfo(s32 width, s32 height, ref u64 out_size, ref u64 out_alignment);
    public static extern Result viManagerPrepareFatal();
    public static extern Result viManagerShowFatal();
    public static extern Result viManagerDrawFatalRectangle(s32 x, s32 y, s32 end_x, s32 end_y, ViColorRgba4444 color);
    public static extern Result viManagerDrawFatalText32(ref s32 out_advance, s32 x, s32 y, ref u32 utf32_codepoints, ulong num_codepoints, float scale_x, float scale_y, PlSharedFontType font_type, ViColorRgba8888 bg_color, ViColorRgba8888 fg_color, s32 initial_advance);
}
