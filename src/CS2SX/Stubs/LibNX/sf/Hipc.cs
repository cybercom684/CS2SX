// Auto-generated from sf\hipc.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Sf;

public enum HipcBufferMode
{
    HipcBufferMode_Normal = 0,
    HipcBufferMode_NonSecure = 1,
    HipcBufferMode_Invalid = 2,
    HipcBufferMode_NonDevice = 3,
}

public unsafe struct HipcMetadata
{
    public u32 type;
    public u32 num_send_statics;
    public u32 num_send_buffers;
    public u32 num_recv_buffers;
    public u32 num_exch_buffers;
    public u32 num_data_words;
    public u32 num_recv_statics;
    public u32 send_pid;
    public u32 num_copy_handles;
    public u32 num_move_handles;
}

public unsafe struct HipcHeader
{
    public u32 type : _16;
    public u32 num_send_statics : _4;
    public u32 num_send_buffers : _4;
    public u32 num_recv_buffers : _4;
    public u32 num_exch_buffers : _4;
    public u32 num_data_words : _10;
    public u32 recv_static_mode : _4;
    public u32 padding : _6;
    public u32 recv_list_offset : _11;
    public u32 has_special_header : _1;
}

public unsafe struct HipcSpecialHeader
{
    public u32 send_pid : _1;
    public u32 num_copy_handles : _4;
    public u32 num_move_handles : _4;
    public u32 padding : _23;
}

public unsafe struct HipcStaticDescriptor
{
    public u32 index : _6;
    public u32 address_high : _6;
    public u32 address_mid : _4;
    public u32 size : _16;
    public u32 address_low;
}

public unsafe struct HipcBufferDescriptor
{
    public u32 size_low;
    public u32 address_low;
    public u32 mode : _2;
    public u32 address_high : _22;
    public u32 size_high : _4;
    public u32 address_mid : _4;
}

public unsafe struct HipcRecvListEntry
{
    public u32 address_low;
    public u32 address_high : _16;
    public u32 size : _16;
}

public unsafe struct HipcRequest
{
    public IntPtr send_statics;
    public IntPtr send_buffers;
    public IntPtr recv_buffers;
    public IntPtr exch_buffers;
    public IntPtr data_words;
    public IntPtr recv_list;
    public IntPtr copy_handles;
    public IntPtr move_handles;
}

public unsafe struct HipcParsedRequest
{
    public HipcMetadata meta;
    public HipcRequest data;
    public u64 pid;
}

public unsafe struct HipcResponse
{
    public u64 pid;
    public u32 num_statics;
    public u32 num_data_words;
    public u32 num_copy_handles;
    public u32 num_move_handles;
    public IntPtr statics;
    public IntPtr data_words;
    public IntPtr copy_handles;
    public IntPtr move_handles;
}

