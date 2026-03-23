// Auto-generated from crypto\aes_xts.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Crypto;

public unsafe struct Aes128XtsContext
{
    public Aes128Context aes_ctx;
    public Aes128Context tweak_ctx;
    public u8 tweak;
    public u8 buffer;
    public ulong num_buffered;
}

public unsafe struct Aes192XtsContext
{
    public Aes192Context aes_ctx;
    public Aes192Context tweak_ctx;
    public u8 tweak;
    public u8 buffer;
    public ulong num_buffered;
}

public unsafe struct Aes256XtsContext
{
    public Aes256Context aes_ctx;
    public Aes256Context tweak_ctx;
    public u8 tweak;
    public u8 buffer;
    public ulong num_buffered;
}

public static class Aes_xts
{
    public static extern void aes128XtsContextCreate(ref Aes128XtsContext @out, IntPtr key0, IntPtr key1, bool is_encryptor);
    public static extern void aes128XtsContextResetTweak(ref Aes128XtsContext ctx, IntPtr tweak);
    public static extern void aes128XtsContextResetSector(ref Aes128XtsContext ctx, uint64_t sector, bool is_nintendo);
    public static extern ulong aes128XtsEncrypt(ref Aes128XtsContext ctx, IntPtr dst, IntPtr src, ulong size);
    public static extern ulong aes128XtsDecrypt(ref Aes128XtsContext ctx, IntPtr dst, IntPtr src, ulong size);
    public static extern void aes192XtsContextCreate(ref Aes192XtsContext @out, IntPtr key0, IntPtr key1, bool is_encryptor);
    public static extern void aes192XtsContextResetTweak(ref Aes192XtsContext ctx, IntPtr tweak);
    public static extern void aes192XtsContextResetSector(ref Aes192XtsContext ctx, uint64_t sector, bool is_nintendo);
    public static extern ulong aes192XtsEncrypt(ref Aes192XtsContext ctx, IntPtr dst, IntPtr src, ulong size);
    public static extern ulong aes192XtsDecrypt(ref Aes192XtsContext ctx, IntPtr dst, IntPtr src, ulong size);
    public static extern void aes256XtsContextCreate(ref Aes256XtsContext @out, IntPtr key0, IntPtr key1, bool is_encryptor);
    public static extern void aes256XtsContextResetTweak(ref Aes256XtsContext ctx, IntPtr tweak);
    public static extern void aes256XtsContextResetSector(ref Aes256XtsContext ctx, uint64_t sector, bool is_nintendo);
    public static extern ulong aes256XtsEncrypt(ref Aes256XtsContext ctx, IntPtr dst, IntPtr src, ulong size);
    public static extern ulong aes256XtsDecrypt(ref Aes256XtsContext ctx, IntPtr dst, IntPtr src, ulong size);
}
