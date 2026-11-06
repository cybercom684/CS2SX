// Auto-generated from crypto\sha256.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Crypto;

public unsafe struct Sha256Context
{
    // 8 u32 words = SHA-256 intermediate hash (256 bits / 32 bits per word)
    public u32 intermediate_hash0;
    public u32 intermediate_hash1;
    public u32 intermediate_hash2;
    public u32 intermediate_hash3;
    public u32 intermediate_hash4;
    public u32 intermediate_hash5;
    public u32 intermediate_hash6;
    public u32 intermediate_hash7;
    public u8 buffer;
    public u64 bits_consumed;
    public ulong num_buffered;
    public bool finalized;
}

public static class Sha256
{
    public static extern void sha256ContextCreate(ref Sha256Context @out);
    public static extern void sha256ContextUpdate(ref Sha256Context ctx, IntPtr src, ulong size);
    public static extern void sha256ContextGetHash(ref Sha256Context ctx, IntPtr dst);
    public static extern void sha256CalculateHash(IntPtr dst, IntPtr src, ulong size);
}
