// Auto-generated from services\irs.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum IrsIrCameraStatus
{
    IrsIrCameraStatus_Available = 0,
    IrsIrCameraStatus_Unsupported = 1,
    IrsIrCameraStatus_Unconnected = 2,
}

public enum IrsIrCameraInternalStatus
{
    IrsIrCameraInternalStatus_Stopped = 0,
    IrsIrCameraInternalStatus_FirmwareUpdateNeeded = 1,
    IrsIrCameraInternalStatus_Unknown2 = 2,
    IrsIrCameraInternalStatus_Unknown3 = 3,
    IrsIrCameraInternalStatus_Unknown4 = 4,
    IrsIrCameraInternalStatus_FirmwareVersionRequested = 5,
    IrsIrCameraInternalStatus_FirmwareVersionIsInvalid = 6,
    IrsIrCameraInternalStatus_Ready = 7,
    IrsIrCameraInternalStatus_Setting = 8,
}

public enum IrsIrSensorMode
{
    IrsIrSensorMode_None = 0,
    IrsIrSensorMode_MomentProcessor = 1,
    IrsIrSensorMode_ClusteringProcessor = 2,
    IrsIrSensorMode_ImageTransferProcessor = 3,
    IrsIrSensorMode_PointingProcessor = 4,
    IrsIrSensorMode_TeraPluginProcessor = 5,
    IrsIrSensorMode_IrLedProcessor = 6,
}

public enum IrsImageProcessorStatus
{
    IrsImageProcessorStatus_Stopped = 0,
    IrsImageProcessorStatus_Running = 1,
}

public enum IrsImageTransferProcessorFormat
{
    IrsImageTransferProcessorFormat_320x240 = 0,
    IrsImageTransferProcessorFormat_160x120 = 1,
    IrsImageTransferProcessorFormat_80x60 = 2,
    IrsImageTransferProcessorFormat_40x30 = 3,
    IrsImageTransferProcessorFormat_20x15 = 4,
}

public enum IrsAdaptiveClusteringMode
{
    IrsAdaptiveClusteringMode_StaticFov = 0,
    IrsAdaptiveClusteringMode_DynamicFov = 1,
}

public enum IrsAdaptiveClusteringTargetDistance
{
    IrsAdaptiveClusteringTargetDistance_Near = 0,
    IrsAdaptiveClusteringTargetDistance_Middle = 1,
    IrsAdaptiveClusteringTargetDistance_Far = 2,
}

public enum IrsHandAnalysisMode
{
    IrsHandAnalysisMode_Silhouette = 1,
    IrsHandAnalysisMode_Image = 2,
    IrsHandAnalysisMode_SilhouetteAndImage = 3,
    IrsHandAnalysisMode_SilhouetteOnly = 4,
}

public unsafe struct IrsIrCameraHandle
{
    public u8 player_number;
    public u8 device_type;
    public u8 reserved;
}

public unsafe struct IrsPackedMcuVersion
{
    public u16 major_version;
    public u16 minor_version;
}

public unsafe struct IrsPackedFunctionLevel
{
    public u8 ir_sensor_function_level;
    public u8 reserved;
}

public unsafe struct IrsRect
{
    public s16 x;
    public s16 y;
    public s16 width;
    public s16 height;
}

public unsafe struct IrsMomentProcessorConfig
{
    public u64 exposure_time;
    public u32 light_target;
    public u32 gain;
    public u8 is_negative_image_used;
    public u8 reserved;
    public IrsRect window_of_interest;
    public u32 preprocess;
    public u32 preprocess_intensity_threshold;
}

public unsafe struct IrsPackedMomentProcessorConfig
{
    public u64 exposure_time;
    public u8 light_target;
    public u8 gain;
    public u8 is_negative_image_used;
    public u8 reserved;
    public IrsRect window_of_interest;
    public IrsPackedMcuVersion required_mcu_version;
    public u8 preprocess;
    public u8 preprocess_intensity_threshold;
    public u8 reserved2;
}

public unsafe struct IrsClusteringProcessorConfig
{
    public u64 exposure_time;
    public u32 light_target;
    public u32 gain;
    public u8 is_negative_image_used;
    public u8 reserved;
    public IrsRect window_of_interest;
    public u32 object_pixel_count_min;
    public u32 object_pixel_count_max;
    public u32 object_intensity_min;
    public u8 is_external_light_filter_enabled;
}

public unsafe struct IrsPackedClusteringProcessorConfig
{
    public u64 exposure_time;
    public u8 light_target;
    public u8 gain;
    public u8 is_negative_image_used;
    public u8 reserved;
    public IrsRect window_of_interest;
    public IrsPackedMcuVersion required_mcu_version;
    public u32 object_pixel_count_min;
    public u32 object_pixel_count_max;
    public u8 object_intensity_min;
    public u8 is_external_light_filter_enabled;
    public u8 reserved2;
}

public unsafe struct IrsImageTransferProcessorConfig
{
    public u64 exposure_time;
    public u32 light_target;
    public u32 gain;
    public u8 is_negative_image_used;
    public u8 reserved;
    public u32 format;
}

public unsafe struct IrsImageTransferProcessorExConfig
{
    public u64 exposure_time;
    public u32 light_target;
    public u32 gain;
    public u8 is_negative_image_used;
    public u8 reserved;
    public u32 orig_format;
    public u32 trimming_format;
    public u16 trimming_start_x;
    public u16 trimming_start_y;
    public u8 is_external_light_filter_enabled;
}

public unsafe struct IrsPackedImageTransferProcessorConfig
{
    public u64 exposure_time;
    public u8 light_target;
    public u8 gain;
    public u8 is_negative_image_used;
    public u8 reserved;
    public IrsPackedMcuVersion required_mcu_version;
    public u8 format;
    public u8 reserved2;
}

public unsafe struct IrsPackedImageTransferProcessorExConfig
{
    public u64 exposure_time;
    public u8 light_target;
    public u8 gain;
    public u8 is_negative_image_used;
    public u8 reserved;
    public IrsPackedMcuVersion required_mcu_version;
    public u8 orig_format;
    public u8 trimming_format;
    public u16 trimming_start_x;
    public u16 trimming_start_y;
    public u8 is_external_light_filter_enabled;
    public u8 reserved2;
}

public unsafe struct IrsImageTransferProcessorState
{
    public u64 sampling_number;
    public u32 ambient_noise_level;
    public u8 reserved;
}

public unsafe struct IrsPackedPointingProcessorConfig
{
    public IrsRect window_of_interest;
    public IrsPackedMcuVersion required_mcu_version;
}

public unsafe struct IrsTeraPluginProcessorConfig
{
    public u8 mode;
    public u8 unk_x1;
    public u8 unk_x2;
    public u8 unk_x3;
}

public unsafe struct IrsPackedTeraPluginProcessorConfig
{
    public IrsPackedMcuVersion required_mcu_version;
    public u8 mode;
    public u8 unk_x5;
    public u8 unk_x6;
    public u8 unk_x7;
}

public unsafe struct IrsIrLedProcessorConfig
{
    public u32 light_target;
}

public unsafe struct IrsPackedIrLedProcessorConfig
{
    public IrsPackedMcuVersion required_mcu_version;
    public u8 light_target;
    public u8 pad;
}

public unsafe struct IrsAdaptiveClusteringProcessorConfig
{
    public u32 mode;
    public u32 target_distance;
}

public unsafe struct IrsHandAnalysisConfig
{
    public u32 mode;
}

public unsafe struct IrsMomentStatistic
{
    public float average_intensity;
    public float centroid_x;
    public float centroid_y;
}

public unsafe struct IrsMomentProcessorState
{
    public s64 sampling_number;
    public u64 timestamp;
    public u32 ambient_noise_level;
    public u8 reserved;
    public IrsMomentStatistic statistic;
}

public unsafe struct IrsClusteringData
{
    public float average_intensity;
    public float centroid_x;
    public float centroid_y;
    public u32 pixel_count;
    public u16 bound_x;
    public u16 bound_y;
    public u16 boundt_width;
    public u16 bound_height;
}

public unsafe struct IrsClusteringProcessorState
{
    public s64 sampling_number;
    public u64 timestamp;
    public u8 object_count;
    public u8 reserved;
    public u32 ambient_noise_level;
    public IrsClusteringData data;
}

public unsafe struct IrsPointingProcessorState
{
    public s64 sampling_number;
    public u64 timestamp;
    public u32 pointing_status;
    public float position_x;
    public float position_y;
    public u8 reserved;
}

public unsafe struct IrsTeraPluginProcessorState
{
    public s64 sampling_number;
    public u64 timestamp;
    public u32 ambient_noise_level;
    public u8 plugin_data;
}

public unsafe struct IrsProcessorState
{
    public s64 start;
    public u32 count;
    public u32 pad;
    public u8 data;
}

public unsafe struct IrsDeviceFormat
{
    public u32 ir_camera_status;
    public u32 ir_camera_internal_status;
    public u32 ir_sensor_mode;
    public u32 pad;
    public IrsProcessorState processor_state;
}

public unsafe struct IrsAruidFormat
{
    public u64 ir_sensor_aruid;
    public u32 ir_sensor_aruid_status;
    public u32 pad;
}

public unsafe struct IrsStatusManager
{
    public IrsDeviceFormat device_format;
    public IrsAruidFormat aruid_format;
}

public static class Irs
{
    public static extern Result irsInitialize();
    public static extern void irsExit();
    public static extern IntPtr irsGetServiceSession();
    public static extern IntPtr irsGetSharedmemAddr();
    public static extern Result irsGetIrCameraHandle(ref IrsIrCameraHandle handle, HidNpadIdType id);
    public static extern Result irsGetIrCameraStatus(IrsIrCameraHandle handle, ref IrsIrCameraStatus @out);
    public static extern Result irsCheckFirmwareUpdateNecessity(IrsIrCameraHandle handle, ref bool @out);
    public static extern Result irsGetImageProcessorStatus(IrsIrCameraHandle handle, ref IrsImageProcessorStatus @out);
    public static extern Result irsStopImageProcessor(IrsIrCameraHandle handle);
    public static extern Result irsStopImageProcessorAsync(IrsIrCameraHandle handle);
    public static extern Result irsRunMomentProcessor(IrsIrCameraHandle handle, ref IrsMomentProcessorConfig config);
    public static extern Result irsGetMomentProcessorStates(IrsIrCameraHandle handle, ref IrsMomentProcessorState states, s32 count, ref s32 total_out);
    public static extern IrsMomentStatistic irsCalculateMomentRegionStatistic(ref IrsMomentProcessorState state, IrsRect rect, s32 region_x, s32 region_y, s32 region_width, s32 region_height);
    public static extern Result irsRunClusteringProcessor(IrsIrCameraHandle handle, ref IrsClusteringProcessorConfig config);
    public static extern Result irsGetClusteringProcessorStates(IrsIrCameraHandle handle, ref IrsClusteringProcessorState states, s32 count, ref s32 total_out);
    public static extern Result irsRunImageTransferProcessor(IrsIrCameraHandle handle, ref IrsImageTransferProcessorConfig config, ulong size);
    public static extern Result irsRunImageTransferExProcessor(IrsIrCameraHandle handle, ref IrsImageTransferProcessorExConfig config, ulong size);
    public static extern Result irsGetImageTransferProcessorState(IrsIrCameraHandle handle, IntPtr buffer, ulong size, ref IrsImageTransferProcessorState state);
    public static extern Result irsRunPointingProcessor(IrsIrCameraHandle handle);
    public static extern Result irsGetPointingProcessorMarkerStates(IrsIrCameraHandle handle, ref IrsPointingProcessorMarkerState states, s32 count, ref s32 total_out);
    public static extern Result irsGetPointingProcessorStates(IrsIrCameraHandle handle, ref IrsPointingProcessorState states, s32 count, ref s32 total_out);
    public static extern Result irsRunTeraPluginProcessor(IrsIrCameraHandle handle, ref IrsTeraPluginProcessorConfig config);
    public static extern Result irsGetTeraPluginProcessorStates(IrsIrCameraHandle handle, ref IrsTeraPluginProcessorState states, s32 count, s64 sampling_number, u32 prefix_data, u32 prefix_bitcount, ref s32 total_out);
    public static extern Result irsRunIrLedProcessor(IrsIrCameraHandle handle, ref IrsIrLedProcessorConfig config);
    public static extern Result irsRunAdaptiveClusteringProcessor(IrsIrCameraHandle handle, ref IrsAdaptiveClusteringProcessorConfig config);
    public static extern Result irsRunHandAnalysis(IrsIrCameraHandle handle, ref IrsHandAnalysisConfig config);
    public static extern void irsGetMomentProcessorDefaultConfig(ref IrsMomentProcessorConfig config);
    public static extern void irsGetClusteringProcessorDefaultConfig(ref IrsClusteringProcessorConfig config);
    public static extern void irsGetDefaultImageTransferProcessorConfig(ref IrsImageTransferProcessorConfig config);
    public static extern void irsGetDefaultImageTransferProcessorExConfig(ref IrsImageTransferProcessorExConfig config);
}
