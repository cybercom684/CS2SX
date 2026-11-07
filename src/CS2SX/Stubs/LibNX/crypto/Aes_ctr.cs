// Auto-generated from crypto\aes_ctr.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Crypto;

public unsafe struct Aes128CtrContext
{
    public Aes128Context aes_ctx;
    public u8 ctr;
    public u8 enc_ctr_buffer;
    public ulong buffer_offset;
}

public unsafe struct Aes192CtrContext
{
    public Aes192Context aes_ctx;
    public u8 ctr;
    public u8 enc_ctr_buffer;
    public ulong buffer_offset;
}

public unsafe struct Aes256CtrContext
{
    public Aes256Context aes_ctx;
    public u8 ctr;
    public u8 enc_ctr_buffer;
    public ulong buffer_offset;
}

public static class Aes_ctr
{
    public static extern void aes128CtrContextCreate(ref Aes128CtrContext @out, IntPtr key, IntPtr ctr);
    public static extern void aes128CtrContextResetCtr(ref Aes128CtrContext ctx, IntPtr ctr);
    public static extern void aes128CtrCrypt(ref Aes128CtrContext ctx, IntPtr dst, IntPtr src, ulong size);
    public static extern void aes192CtrContextCreate(ref Aes192CtrContext @out, IntPtr key, IntPtr ctr);
    public static extern void aes192CtrContextResetCtr(ref Aes192CtrContext ctx, IntPtr ctr);
    public static extern void aes192CtrCrypt(ref Aes192CtrContext ctx, IntPtr dst, IntPtr src, ulong size);
    public static extern void aes256CtrContextCreate(ref Aes256CtrContext @out, IntPtr key, IntPtr ctr);
    public static extern void aes256CtrContextResetCtr(ref Aes256CtrContext ctx, IntPtr ctr);
    public static extern void aes256CtrCrypt(ref Aes256CtrContext ctx, IntPtr dst, IntPtr src, ulong size);
}
