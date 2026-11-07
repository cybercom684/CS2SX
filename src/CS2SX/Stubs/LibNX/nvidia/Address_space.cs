// Auto-generated from nvidia\address_space.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Nvidia;

public unsafe struct NvAddressSpace
{
    public u32 fd;
    public u32 page_size;
    public bool has_init;
}

public static class Address_space
{
    public static extern Result nvAddressSpaceCreate(ref NvAddressSpace a, u32 page_size);
    public static extern void nvAddressSpaceClose(ref NvAddressSpace a);
    public static extern Result nvAddressSpaceAlloc(ref NvAddressSpace a, bool sparse, u64 size, ref iova_t iova_out);
    public static extern Result nvAddressSpaceAllocFixed(ref NvAddressSpace a, bool sparse, u64 size, iova_t iova);
    public static extern Result nvAddressSpaceFree(ref NvAddressSpace a, iova_t iova, u64 size);
    public static extern Result nvAddressSpaceMap(ref NvAddressSpace a, u32 nvmap_handle, bool is_gpu_cacheable, NvKind kind, ref iova_t iova_out);
    public static extern Result nvAddressSpaceMapFixed(ref NvAddressSpace a, u32 nvmap_handle, bool is_gpu_cacheable, NvKind kind, iova_t iova);
    public static extern Result nvAddressSpaceModify(ref NvAddressSpace a, iova_t iova, u64 offset, u64 size, NvKind kind);
    public static extern Result nvAddressSpaceUnmap(ref NvAddressSpace a, iova_t iova);
}
