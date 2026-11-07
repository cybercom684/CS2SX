// Auto-generated from crypto\sha1.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Crypto;

public unsafe struct Sha1Context
{
    public u32 intermediate_hash[SHA1_HASH_SIZE / sizeof(u32)];
    public u8 buffer;
    public u64 bits_consumed;
    public ulong num_buffered;
    public bool finalized;
}

public static class Sha1
{
    public static extern void sha1ContextCreate(ref Sha1Context @out);
    public static extern void sha1ContextUpdate(ref Sha1Context ctx, IntPtr src, ulong size);
    public static extern void sha1ContextGetHash(ref Sha1Context ctx, IntPtr dst);
    public static extern void sha1CalculateHash(IntPtr dst, IntPtr src, ulong size);
}
