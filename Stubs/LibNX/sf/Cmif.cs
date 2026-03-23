// Auto-generated from sf\cmif.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Sf;

public enum CmifCommandType
{
    CmifCommandType_Invalid = 0,
    CmifCommandType_LegacyRequest = 1,
    CmifCommandType_Close = 2,
    CmifCommandType_LegacyControl = 3,
    CmifCommandType_Request = 4,
    CmifCommandType_Control = 5,
    CmifCommandType_RequestWithContext = 6,
    CmifCommandType_ControlWithContext = 7,
}

public enum CmifDomainRequestType
{
    CmifDomainRequestType_Invalid = 0,
    CmifDomainRequestType_SendMessage = 1,
    CmifDomainRequestType_Close = 2,
}

public unsafe struct CmifInHeader
{
    public u32 magic;
    public u32 version;
    public u32 command_id;
    public u32 token;
}

public unsafe struct CmifOutHeader
{
    public u32 magic;
    public u32 version;
    public Result result;
    public u32 token;
}

public unsafe struct CmifDomainInHeader
{
    public u8 type;
    public u8 num_in_objects;
    public u16 data_size;
    public u32 object_id;
    public u32 padding;
    public u32 token;
}

public unsafe struct CmifDomainOutHeader
{
    public u32 num_out_objects;
    public fixed u32 padding[3];
}

public unsafe struct CmifRequestFormat
{
    public u32 object_id;
    public u32 request_id;
    public u32 context;
    public u32 data_size;
    public u32 server_pointer_size;
    public u32 num_in_auto_buffers;
    public u32 num_out_auto_buffers;
    public u32 num_in_buffers;
    public u32 num_out_buffers;
    public u32 num_inout_buffers;
    public u32 num_in_pointers;
    public u32 num_out_pointers;
    public u32 num_out_fixed_pointers;
    public u32 num_objects;
    public u32 num_handles;
    public u32 send_pid;
}

public unsafe struct CmifRequest
{
    public HipcRequest hipc;
    public IntPtr data;
    public IntPtr out_pointer_sizes;
    public IntPtr objects;
    public u32 server_pointer_size;
    public u32 cur_in_ptr_id;
}

public unsafe struct CmifResponse
{
    public IntPtr data;
    public IntPtr objects;
    public IntPtr copy_handles;
    public IntPtr move_handles;
}

