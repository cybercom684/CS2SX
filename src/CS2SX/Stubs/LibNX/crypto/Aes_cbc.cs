// Auto-generated from crypto\aes_cbc.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Crypto;

public unsafe struct Aes128CbcContext
{
    public Aes128Context aes_ctx;
    public u8 iv;
    public u8 buffer;
    public ulong num_buffered;
}

public unsafe struct Aes192CbcContext
{
    public Aes192Context aes_ctx;
    public u8 iv;
    public u8 buffer;
    public ulong num_buffered;
}

public unsafe struct Aes256CbcContext
{
    public Aes256Context aes_ctx;
    public u8 iv;
    public u8 buffer;
    public ulong num_buffered;
}

public static class Aes_cbc
{
    public static extern void aes128CbcContextCreate(ref Aes128CbcContext @out, IntPtr key, IntPtr iv, bool is_encryptor);
    public static extern void aes128CbcContextResetIv(ref Aes128CbcContext ctx, IntPtr iv);
    public static extern ulong aes128CbcEncrypt(ref Aes128CbcContext ctx, IntPtr dst, IntPtr src, ulong size);
    public static extern ulong aes128CbcDecrypt(ref Aes128CbcContext ctx, IntPtr dst, IntPtr src, ulong size);
    public static extern void aes192CbcContextCreate(ref Aes192CbcContext @out, IntPtr key, IntPtr iv, bool is_encryptor);
    public static extern void aes192CbcContextResetIv(ref Aes192CbcContext ctx, IntPtr iv);
    public static extern ulong aes192CbcEncrypt(ref Aes192CbcContext ctx, IntPtr dst, IntPtr src, ulong size);
    public static extern ulong aes192CbcDecrypt(ref Aes192CbcContext ctx, IntPtr dst, IntPtr src, ulong size);
    public static extern void aes256CbcContextCreate(ref Aes256CbcContext @out, IntPtr key, IntPtr iv, bool is_encryptor);
    public static extern void aes256CbcContextResetIv(ref Aes256CbcContext ctx, IntPtr iv);
    public static extern ulong aes256CbcEncrypt(ref Aes256CbcContext ctx, IntPtr dst, IntPtr src, ulong size);
    public static extern ulong aes256CbcDecrypt(ref Aes256CbcContext ctx, IntPtr dst, IntPtr src, ulong size);
}
