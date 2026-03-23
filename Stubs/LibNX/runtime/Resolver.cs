// Auto-generated from runtime\resolver.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Runtime;

public static class Resolver
{
    public static extern Result resolverGetLastResult();
    public static extern u32 resolverGetCancelHandle();
    public static extern bool resolverGetEnableServiceDiscovery();
    public static extern bool resolverGetEnableDnsCache();
    public static extern void resolverSetEnableServiceDiscovery(bool enable);
    public static extern void resolverSetEnableDnsCache(bool enable);
    public static extern Result resolverCancel(u32 handle);
    public static extern Result resolverRemoveHostnameFromCache(ref byte hostname);
    public static extern Result resolverRemoveIpAddressFromCache(u32 ip);
}
