// Auto-generated from services\ncm_types.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum NcmStorageId
{
    NcmStorageId_None = 0,
    NcmStorageId_Host = 1,
    NcmStorageId_GameCard = 2,
    NcmStorageId_BuiltInSystem = 3,
    NcmStorageId_BuiltInUser = 4,
    NcmStorageId_SdCard = 5,
    NcmStorageId_Any = 6,
}

public enum NcmContentType
{
    NcmContentType_Meta = 0,
    NcmContentType_Program = 1,
    NcmContentType_Data = 2,
    NcmContentType_Control = 3,
    NcmContentType_HtmlDocument = 4,
    NcmContentType_LegalInformation = 5,
    NcmContentType_DeltaFragment = 6,
}

public enum NcmContentMetaType
{
    NcmContentMetaType_Unknown = 0x0,
    NcmContentMetaType_SystemProgram = 0x1,
    NcmContentMetaType_SystemData = 0x2,
    NcmContentMetaType_SystemUpdate = 0x3,
    NcmContentMetaType_BootImagePackage = 0x4,
    NcmContentMetaType_BootImagePackageSafe = 0x5,
    NcmContentMetaType_Application = 0x80,
    NcmContentMetaType_Patch = 0x81,
    NcmContentMetaType_AddOnContent = 0x82,
    NcmContentMetaType_Delta = 0x83,
    NcmContentMetaType_DataPatch = 0x84,
}

public enum NcmContentMetaAttribute
{
    NcmContentMetaAttribute_None = 0,
    NcmContentMetaAttribute_IncludesExFatDriver = (1 << 0),
    NcmContentMetaAttribute_Rebootless = (1 << 1),
    NcmContentMetaAttribute_Compacted = (1 << 2),
}

public enum NcmContentInstallType
{
    NcmContentInstallType_Full = 0,
    NcmContentInstallType_FragmentOnly = 1,
    NcmContentInstallType_Unknown = 7,
}

public enum NcmContentMetaPlatform
{
    NcmContentMetaPlatform_Nx = 0,
}

public unsafe struct NcmContentId
{
    public u8 c;
}

public unsafe struct NcmPlaceHolderId
{
    public Uuid uuid;
}

public unsafe struct NcmContentMetaKey
{
    public u64 id;
    public u32 version;
    public u8 type;
    public u8 install_type;
    public fixed u8 padding[2];
}

public unsafe struct NcmApplicationContentMetaKey
{
    public NcmContentMetaKey key;
    public u64 application_id;
}

public unsafe struct NcmContentInfo
{
    public NcmContentId content_id;
    public u32 size_low;
    public u8 size_high;
    public u8 attr;
    public u8 content_type;
    public u8 id_offset;
}

public unsafe struct NcmPackagedContentInfo
{
    public u8 hash;
    public NcmContentInfo info;
}

public unsafe struct NcmContentMetaInfo
{
    public u64 id;
    public u32 version;
    public u8 type;
    public u8 attr;
    public fixed u8 padding[2];
}

public unsafe struct NcmContentMetaHeader
{
    public u16 extended_header_size;
    public u16 content_count;
    public u16 content_meta_count;
    public u8 attributes;
    public u8 storage_id;
}

public unsafe struct NcmApplicationMetaExtendedHeader
{
    public u64 patch_id;
    public u32 required_system_version;
    public u32 required_application_version;
}

public unsafe struct NcmPatchMetaExtendedHeader
{
    public u64 application_id;
    public u32 required_system_version;
    public u32 extended_data_size;
    public u8 reserved;
}

public unsafe struct NcmAddOnContentMetaExtendedHeader
{
    public u64 application_id;
    public u32 required_application_version;
    public u8 content_accessibilities;
    public fixed u8 padding[3];
    public u64 data_patch_id;
}

public unsafe struct NcmLegacyAddOnContentMetaExtendedHeader
{
    public u64 application_id;
    public u32 required_application_version;
    public u32 padding;
}

public unsafe struct NcmDataPatchMetaExtendedHeader
{
    public u64 data_id;
    public u64 application_id;
    public u32 required_application_version;
    public u32 extended_data_size;
    public u64 padding;
}

public unsafe struct NcmSystemUpdateMetaExtendedHeader
{
    public u32 extended_data_size;
}

public unsafe struct NcmProgramLocation
{
    public u64 program_id;
    public u8 storageID;
    public fixed u8 pad[7];
}

