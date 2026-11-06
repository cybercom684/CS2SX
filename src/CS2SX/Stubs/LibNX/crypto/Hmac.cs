// Auto-generated from crypto\hmac.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Crypto;

public unsafe struct HmacSha1Context
{
    public Sha1Context sha_ctx;
    public u32 key[SHA1_BLOCK_SIZE / sizeof(u32)];
    public u32 mac[SHA1_HASH_SIZE / sizeof(u32)];
    public bool finalized;
}

public unsafe struct HmacSha256Context
{
    public Sha256Context sha_ctx;
    public u32 key[SHA256_BLOCK_SIZE / sizeof(u32)];
    public u32 mac[SHA256_HASH_SIZE / sizeof(u32)];
    public bool finalized;
}

public static class Hmac
{
    public static extern void hmacSha256ContextCreate(ref HmacSha256Context @out, IntPtr key, ulong key_size);
    public static extern void hmacSha256ContextUpdate(ref HmacSha256Context ctx, IntPtr src, ulong size);
    public static extern void hmacSha256ContextGetMac(ref HmacSha256Context ctx, IntPtr dst);
    public static extern void hmacSha256CalculateMac(IntPtr dst, IntPtr key, ulong key_size, IntPtr src, ulong size);
    public static extern void hmacSha1ContextCreate(ref HmacSha1Context @out, IntPtr key, ulong key_size);
    public static extern void hmacSha1ContextUpdate(ref HmacSha1Context ctx, IntPtr src, ulong size);
    public static extern void hmacSha1ContextGetMac(ref HmacSha1Context ctx, IntPtr dst);
    public static extern void hmacSha1CalculateMac(IntPtr dst, IntPtr key, ulong key_size, IntPtr src, ulong size);
}
