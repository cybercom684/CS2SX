// Auto-generated from sf\tipc.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Sf;

public enum TipcCommandType
{
    TipcCommandType_Close = 15,
}

public unsafe struct TipcService
{
    public Handle session;
}

public unsafe struct TipcDispatchParams
{
    public SfBufferAttrs buffer_attrs;
    // skipped array: SfBuffer buffers[8]
    public bool in_send_pid;
    public u32 in_num_handles;
    // skipped array: Handle in_handles[8]
    public u32 out_num_objects;
    public IntPtr out_objects;
    public SfOutHandleAttrs out_handle_attrs;
    public IntPtr out_handles;
}

public unsafe struct TipcRequestFormat
{
    public u32 request_id;
    public u32 data_size;
    public u32 num_in_buffers;
    public u32 num_out_buffers;
    public u32 num_inout_buffers;
    public u32 num_handles;
    public u32 send_pid;
}

