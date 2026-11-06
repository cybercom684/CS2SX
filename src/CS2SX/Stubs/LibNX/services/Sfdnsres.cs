// Auto-generated from services\sfdnsres.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public static class Sfdnsres
{
    public static extern Result sfdnsresGetHostByNameRequest(u32 cancel_handle, bool use_nsd, ref byte name, ref u32 h_errno_, ref u32 errno_, IntPtr out_buffer, ulong out_buffer_size, ref u32 out_serialized_size);
    public static extern Result sfdnsresGetHostByAddrRequest(IntPtr in_addr, ulong in_addr_len, u32 type, u32 cancel_handle, ref u32 h_errno_, ref u32 errno_, IntPtr out_buffer, ulong out_buffer_size, ref u32 out_serialized_size);
    public static extern Result sfdnsresGetHostStringErrorRequest(u32 err, ref byte out_str, ulong out_str_size);
    public static extern Result sfdnsresGetGaiStringErrorRequest(u32 err, ref byte out_str, ulong out_str_size);
    public static extern Result sfdnsresGetAddrInfoRequest(u32 cancel_handle, bool use_nsd, ref byte node, ref byte service, IntPtr in_hints, ulong in_hints_size, IntPtr out_buffer, ulong out_buffer_size, ref u32 errno_, ref s32 ret, ref u32 out_serialized_size);
    public static extern Result sfdnsresGetNameInfoRequest(u32 flags, IntPtr in_sa, ulong in_sa_size, ref byte out_host, ulong out_host_size, ref byte out_serv, ulong out_serv_len, u32 cancel_handle, ref u32 errno_, ref s32 ret);
    public static extern Result sfdnsresGetCancelHandleRequest(ref u32 out_handle);
    public static extern Result sfdnsresCancelRequest(u32 handle);
}
