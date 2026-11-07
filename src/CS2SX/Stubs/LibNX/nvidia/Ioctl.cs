// Auto-generated from nvidia\ioctl.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Nvidia;

public enum NvMapParam
{
    NvMapParam_Size = 1,
    NvMapParam_Alignment = 2,
    NvMapParam_Base = 3,
    NvMapParam_Heap = 4,
    NvMapParam_Kind = 5,
}

public enum NvClassNumber
{
    NvClassNumber_2D = 0x902D,
    NvClassNumber_3D = 0xB197,
    NvClassNumber_Compute = 0xB1C0,
    NvClassNumber_Kepler = 0xA140,
    NvClassNumber_DMA = 0xB0B5,
    NvClassNumber_ChannelGpfifo = 0xB06F,
}

public enum NvChannelPriority
{
    NvChannelPriority_Low = 50,
    NvChannelPriority_Medium = 100,
    NvChannelPriority_High = 150,
}

public enum NvZcullConfig
{
    NvZcullConfig_Global = 0,
    NvZcullConfig_NoCtxSwitch = 1,
    NvZcullConfig_SeparateBuffer = 2,
    NvZcullConfig_PartOfRegularBuffer = 3,
}

public enum NvAllocSpaceFlags
{
    NvAllocSpaceFlags_FixedOffset = 1,
    NvAllocSpaceFlags_Sparse = 2,
}

public enum NvMapBufferFlags
{
    NvMapBufferFlags_FixedOffset = 1,
    NvMapBufferFlags_IsCacheable = 4,
    NvMapBufferFlags_Modify = 0x100,
}

public enum NvNotificationType
{
    NvNotificationType_FifoErrorIdleTimeout = 8,
    NvNotificationType_GrErrorSwNotify = 13,
    NvNotificationType_GrSemaphoreTimeout = 24,
    NvNotificationType_GrIllegalNotify = 25,
    NvNotificationType_FifoErrorMmuErrFlt = 31,
    NvNotificationType_PbdmaError = 32,
    NvNotificationType_ResetChannelVerifError = 43,
    NvNotificationType_PbdmaPushbufferCrcMismatch = 80,
}

public unsafe struct nvioctl_zcull_info
{
    public u32 width_align_pixels;
    public u32 height_align_pixels;
    public u32 pixel_squares_by_aliquots;
    public u32 aliquot_total;
    public u32 region_byte_multiplier;
    public u32 region_header_size;
    public u32 subregion_header_size;
    public u32 subregion_width_align_pixels;
    public u32 subregion_height_align_pixels;
    public u32 subregion_count;
}

public unsafe struct nvioctl_zbc_entry
{
    public fixed u32 color_ds[4];
    public fixed u32 color_l2[4];
    public u32 depth;
    public u32 ref_cnt;
    public u32 format;
    public u32 type;
    public u32 size;
}

public unsafe struct nvioctl_gpu_characteristics
{
    public u32 arch;
    public u32 impl;
    public u32 rev;
    public u32 num_gpc;
    public u64 L2_cache_size;
    public u64 on_board_video_memory_size;
    public u32 num_tpc_per_gpc;
    public u32 bus_type;
    public u32 big_page_size;
    public u32 compression_page_size;
    public u32 pde_coverage_bit_count;
    public u32 available_big_page_sizes;
    public u32 gpc_mask;
    public u32 sm_arch_sm_version;
    public u32 sm_arch_spa_version;
    public u32 sm_arch_warp_count;
    public u32 gpu_va_bit_count;
    public u32 reserved;
    public u64 flags;
    public u32 twod_class;
    public u32 threed_class;
    public u32 compute_class;
    public u32 gpfifo_class;
    public u32 inline_to_memory_class;
    public u32 dma_copy_class;
    public u32 max_fbps_count;
    public u32 fbp_en_mask;
    public u32 max_ltc_per_fbp;
    public u32 max_lts_per_ltc;
    public u32 max_tex_per_tpc;
    public u32 max_gpc_count;
    public u32 rop_l2_en_mask_0;
    public u32 rop_l2_en_mask_1;
    public u64 chipname;
    public u64 gr_compbit_store_base_hw;
}

public unsafe struct nvioctl_va_region
{
    public u64 offset;
    public u32 page_size;
    public u32 pad;
    public u64 pages;
}

public unsafe struct nvioctl_zbc_slot_mask
{
    public u32 slot;
    public u32 mask;
}

public unsafe struct nvioctl_gpu_time
{
    public u64 timestamp;
    public u64 reserved;
}

public unsafe struct nvioctl_fence
{
    public u32 id;
    public u32 value;
}

public unsafe struct nvioctl_cmdbuf
{
    public u32 mem;
    public u32 offset;
    public u32 words;
}

public unsafe struct nvioctl_reloc
{
    public u32 cmdbuf_mem;
    public u32 cmdbuf_offset;
    public u32 target;
    public u32 target_offset;
}

public unsafe struct nvioctl_reloc_shift
{
    public u32 shift;
}

public unsafe struct nvioctl_syncpt_incr
{
    public u32 syncpt_id;
    public u32 syncpt_incrs;
    public u32 waitbase_id;
    public u32 next;
    public u32 prev;
}

public unsafe struct nvioctl_command_buffer_map
{
    public u32 handle;
    public u32 iova;
}

public unsafe struct nvioctl_clk_rate
{
    public u32 rate;
    public u32 moduleid;
}

public unsafe struct NvNotification
{
    public u64 timestamp;
    public u32 info32;
    public u16 info16;
    public u16 status;
}

public unsafe struct NvError
{
    public u32 type;
    public fixed u32 info[31];
}

public static class Ioctl
{
    public static extern Result nvioctlNvhostCtrl_SyncptRead(u32 fd, u32 id, ref u32 @out);
    public static extern Result nvioctlNvhostCtrl_SyncptIncr(u32 fd, u32 id);
    public static extern Result nvioctlNvhostCtrl_SyncptWait(u32 fd, u32 id, u32 threshold, u32 timeout);
    public static extern Result nvioctlNvhostCtrl_EventSignal(u32 fd, u32 event_id);
    public static extern Result nvioctlNvhostCtrl_EventWait(u32 fd, u32 syncpt_id, u32 threshold, s32 timeout, u32 event_id, ref u32 @out);
    public static extern Result nvioctlNvhostCtrl_EventWaitAsync(u32 fd, u32 syncpt_id, u32 threshold, s32 timeout, u32 event_id);
    public static extern Result nvioctlNvhostCtrl_EventRegister(u32 fd, u32 event_id);
    public static extern Result nvioctlNvhostCtrl_EventUnregister(u32 fd, u32 event_id);
    public static extern Result nvioctlNvhostCtrlGpu_ZCullGetCtxSize(u32 fd, ref u32 @out);
    public static extern Result nvioctlNvhostCtrlGpu_ZCullGetInfo(u32 fd, ref nvioctl_zcull_info @out);
    public static extern Result nvioctlNvhostCtrlGpu_ZbcSetTable(u32 fd, u32 color_ds, u32 color_l2, u32 depth, u32 format, u32 type);
    public static extern Result nvioctlNvhostCtrlGpu_ZbcQueryTable(u32 fd, u32 index, ref nvioctl_zbc_entry @out);
    public static extern Result nvioctlNvhostCtrlGpu_GetCharacteristics(u32 fd, ref nvioctl_gpu_characteristics @out);
    public static extern Result nvioctlNvhostCtrlGpu_GetTpcMasks(u32 fd, IntPtr buffer, ulong size);
    public static extern Result nvioctlNvhostCtrlGpu_ZbcGetActiveSlotMask(u32 fd, ref nvioctl_zbc_slot_mask @out);
    public static extern Result nvioctlNvhostCtrlGpu_GetGpuTime(u32 fd, ref nvioctl_gpu_time @out);
    public static extern Result nvioctlNvhostAsGpu_BindChannel(u32 fd, u32 channel_fd);
    public static extern Result nvioctlNvhostAsGpu_AllocSpace(u32 fd, u32 pages, u32 page_size, u32 flags, u64 align_or_offset, ref u64 offset);
    public static extern Result nvioctlNvhostAsGpu_FreeSpace(u32 fd, u64 offset, u32 pages, u32 page_size);
    public static extern Result nvioctlNvhostAsGpu_MapBufferEx(u32 fd, u32 flags, u32 kind, u32 nvmap_handle, u32 page_size, u64 buffer_offset, u64 mapping_size, u64 input_offset, ref u64 offset);
    public static extern Result nvioctlNvhostAsGpu_UnmapBuffer(u32 fd, u64 offset);
    public static extern Result nvioctlNvhostAsGpu_GetVARegions(u32 fd, nvioctl_va_region regions);
    public static extern Result nvioctlNvhostAsGpu_InitializeEx(u32 fd, u32 flags, u32 big_page_size);
    public static extern Result nvioctlNvmap_Create(u32 fd, u32 size, ref u32 nvmap_handle);
    public static extern Result nvioctlNvmap_FromId(u32 fd, u32 id, ref u32 nvmap_handle);
    public static extern Result nvioctlNvmap_Alloc(u32 fd, u32 nvmap_handle, u32 heapmask, u32 flags, u32 align, u8 kind, IntPtr addr);
    public static extern Result nvioctlNvmap_Free(u32 fd, u32 nvmap_handle);
    public static extern Result nvioctlNvmap_Param(u32 fd, u32 nvmap_handle, NvMapParam param, ref u32 result);
    public static extern Result nvioctlNvmap_GetId(u32 fd, u32 nvmap_handle, ref u32 id);
    public static extern Result nvioctlChannel_SetNvmapFd(u32 fd, u32 nvmap_fd);
    public static extern Result nvioctlChannel_SubmitGpfifo(u32 fd, ref nvioctl_gpfifo_entry entries, u32 num_entries, u32 flags, ref nvioctl_fence fence_inout);
    public static extern Result nvioctlChannel_KickoffPb(u32 fd, ref nvioctl_gpfifo_entry entries, u32 num_entries, u32 flags, ref nvioctl_fence fence_inout);
    public static extern Result nvioctlChannel_AllocObjCtx(u32 fd, u32 class_num, u32 flags, ref u64 id_out);
    public static extern Result nvioctlChannel_ZCullBind(u32 fd, u64 gpu_va, u32 mode);
    public static extern Result nvioctlChannel_SetErrorNotifier(u32 fd, u32 enable);
    public static extern Result nvioctlChannel_GetErrorInfo(u32 fd, ref NvError @out);
    public static extern Result nvioctlChannel_GetErrorNotification(u32 fd, ref NvNotification @out);
    public static extern Result nvioctlChannel_SetPriority(u32 fd, u32 priority);
    public static extern Result nvioctlChannel_SetTimeout(u32 fd, u32 timeout);
    public static extern Result nvioctlChannel_AllocGpfifoEx2(u32 fd, u32 num_entries, u32 flags, u32 unk0, u32 unk1, u32 unk2, u32 unk3, ref nvioctl_fence fence_out);
    public static extern Result nvioctlChannel_SetUserData(u32 fd, IntPtr addr);
    public static extern Result nvioctlChannel_Submit(u32 fd, ref nvioctl_cmdbuf cmdbufs, u32 num_cmdbufs, ref nvioctl_reloc relocs, ref nvioctl_reloc_shift reloc_shifts, u32 num_relocs, ref nvioctl_syncpt_incr syncpt_incrs, u32 num_syncpt_incrs, ref nvioctl_fence fences, u32 num_fences);
    public static extern Result nvioctlChannel_GetSyncpt(u32 fd, u32 module_id, ref u32 syncpt);
    public static extern Result nvioctlChannel_GetModuleClockRate(u32 fd, u32 module_id, ref u32 freq);
    public static extern Result nvioctlChannel_SetModuleClockRate(u32 fd, u32 module_id, u32 freq);
    public static extern Result nvioctlChannel_MapCommandBuffer(u32 fd, ref nvioctl_command_buffer_map maps, u32 num_maps, bool compressed);
    public static extern Result nvioctlChannel_UnmapCommandBuffer(u32 fd, ref nvioctl_command_buffer_map maps, u32 num_maps, bool compressed);
    public static extern Result nvioctlChannel_SetSubmitTimeout(u32 fd, u32 timeout);
}
