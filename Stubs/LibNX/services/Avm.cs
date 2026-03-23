// Auto-generated from services\avm.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public unsafe struct AvmVersionListEntry
{
    public u64 application_id;
    public u32 version;
    public u32 required;
}

public unsafe struct AvmRequiredVersionEntry
{
    public u64 application_id;
    public u32 version;
}

public unsafe struct AvmVersionListImporter
{
    public Service s;
}

public static class Avm
{
    public static extern Result avmInitialize();
    public static extern void avmExit();
    public static extern Result avmGetHighestAvailableVersion(u64 id_1, u64 id_2, ref u32 version);
    public static extern Result avmGetHighestRequiredVersion(u64 id_1, u64 id_2, ref u32 version);
    public static extern Result avmGetVersionListEntry(u64 application_id, ref AvmVersionListEntry entry);
    public static extern Result avmGetVersionListImporter(ref AvmVersionListImporter @out);
    public static extern Result avmGetLaunchRequiredVersion(u64 application_id, ref u32 version);
    public static extern Result avmUpgradeLaunchRequiredVersion(u64 application_id, u32 version);
    public static extern Result avmPushLaunchVersion(u64 application_id, u32 version);
    public static extern Result avmListVersionList(ref AvmVersionListEntry buffer, ulong count, ref u32 @out);
    public static extern Result avmListRequiredVersion(ref AvmRequiredVersionEntry buffer, ulong count, ref u32 @out);
    public static extern void avmVersionListImporterClose(ref AvmVersionListImporter srv);
    public static extern Result avmVersionListImporterSetTimestamp(ref AvmVersionListImporter srv, u64 timestamp);
    public static extern Result avmVersionListImporterSetData(ref AvmVersionListImporter srv, ref AvmVersionListEntry entries, u32 count);
    public static extern Result avmVersionListImporterFlush(ref AvmVersionListImporter srv);
}
