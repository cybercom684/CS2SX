// Auto-generated from sf\service.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Sf;

public enum SfOutHandleAttr
{
    SfOutHandleAttr_None = 0,
    SfOutHandleAttr_HipcCopy = 1,
    SfOutHandleAttr_HipcMove = 2,
}

public unsafe struct Service
{
    public Handle session;
    public u32 own_handle;
    public u32 object_id;
    public u16 pointer_buffer_size;
}

public unsafe struct SfBufferAttrs
{
    public u32 attr0;
    public u32 attr1;
    public u32 attr2;
    public u32 attr3;
    public u32 attr4;
    public u32 attr5;
    public u32 attr6;
    public u32 attr7;
}

public unsafe struct SfBuffer
{
    public IntPtr ptr;
    public ulong size;
}

public unsafe struct SfOutHandleAttrs
{
    public SfOutHandleAttr attr0;
    public SfOutHandleAttr attr1;
    public SfOutHandleAttr attr2;
    public SfOutHandleAttr attr3;
    public SfOutHandleAttr attr4;
    public SfOutHandleAttr attr5;
    public SfOutHandleAttr attr6;
    public SfOutHandleAttr attr7;
}

public unsafe struct SfDispatchParams
{
    public Handle target_session;
    public u32 context;
    public SfBufferAttrs buffer_attrs;
    // skipped array: SfBuffer buffers[8]
    public bool in_send_pid;
    public u32 in_num_objects;
    public IntPtr in_objects;
    public u32 in_num_handles;
    // skipped array: Handle in_handles[8]
    public u32 out_num_objects;
    public IntPtr out_objects;
    public SfOutHandleAttrs out_handle_attrs;
    public IntPtr out_handles;
}

