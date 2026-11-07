// Auto-generated from crypto\cmac.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Crypto;

public unsafe struct Aes128CmacContext
{
    public Aes128Context ctx;
    public u8 subkey;
    public u8 mac;
    public u8 buffer;
    public ulong num_buffered;
    public bool finalized;
}

public unsafe struct Aes192CmacContext
{
    public Aes192Context ctx;
    public u8 subkey;
    public u8 mac;
    public u8 buffer;
    public ulong num_buffered;
    public bool finalized;
}

public unsafe struct Aes256CmacContext
{
    public Aes256Context ctx;
    public u8 subkey;
    public u8 mac;
    public u8 buffer;
    public ulong num_buffered;
    public bool finalized;
}

public static class Cmac
{
    public static extern void cmacAes128ContextCreate(ref Aes128CmacContext @out, IntPtr key);
    public static extern void cmacAes128ContextUpdate(ref Aes128CmacContext ctx, IntPtr src, ulong size);
    public static extern void cmacAes128ContextGetMac(ref Aes128CmacContext ctx, IntPtr dst);
    public static extern void cmacAes128CalculateMac(IntPtr dst, IntPtr key, IntPtr src, ulong size);
    public static extern void cmacAes192ContextCreate(ref Aes192CmacContext @out, IntPtr key);
    public static extern void cmacAes192ContextUpdate(ref Aes192CmacContext ctx, IntPtr src, ulong size);
    public static extern void cmacAes192ContextGetMac(ref Aes192CmacContext ctx, IntPtr dst);
    public static extern void cmacAes192CalculateMac(IntPtr dst, IntPtr key, IntPtr src, ulong size);
    public static extern void cmacAes256ContextCreate(ref Aes256CmacContext @out, IntPtr key);
    public static extern void cmacAes256ContextUpdate(ref Aes256CmacContext ctx, IntPtr src, ulong size);
    public static extern void cmacAes256ContextGetMac(ref Aes256CmacContext ctx, IntPtr dst);
    public static extern void cmacAes256CalculateMac(IntPtr dst, IntPtr key, IntPtr src, ulong size);
}
