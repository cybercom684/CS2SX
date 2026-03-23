// Auto-generated from crypto\sha256.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Crypto;

public unsafe struct Sha256Context
{
    public u32 intermediate_hash[SHA256_HASH_SIZE / sizeof(u32)];
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
