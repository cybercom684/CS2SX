// Auto-generated from crypto\aes.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Crypto;

public unsafe struct Aes128Context
{
    public u8 round_keys;
}

public unsafe struct Aes192Context
{
    public u8 round_keys;
}

public unsafe struct Aes256Context
{
    public u8 round_keys;
}

public static class Aes
{
    public static extern void aes128ContextCreate(ref Aes128Context @out, IntPtr key, bool is_encryptor);
    public static extern void aes128EncryptBlock(ref Aes128Context ctx, IntPtr dst, IntPtr src);
    public static extern void aes128DecryptBlock(ref Aes128Context ctx, IntPtr dst, IntPtr src);
    public static extern void aes192ContextCreate(ref Aes192Context @out, IntPtr key, bool is_encryptor);
    public static extern void aes192EncryptBlock(ref Aes192Context ctx, IntPtr dst, IntPtr src);
    public static extern void aes192DecryptBlock(ref Aes192Context ctx, IntPtr dst, IntPtr src);
    public static extern void aes256ContextCreate(ref Aes256Context @out, IntPtr key, bool is_encryptor);
    public static extern void aes256EncryptBlock(ref Aes256Context ctx, IntPtr dst, IntPtr src);
    public static extern void aes256DecryptBlock(ref Aes256Context ctx, IntPtr dst, IntPtr src);
}
