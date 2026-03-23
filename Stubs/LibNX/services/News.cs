// Auto-generated from services\news.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum NewsServiceType
{
    NewsServiceType_Administrator = 0,
    NewsServiceType_Configuration = 1,
    NewsServiceType_Manager = 2,
    NewsServiceType_Post = 3,
    NewsServiceType_Viewer = 4,
    NewsServiceType_Count,
}

public unsafe struct NewsTopicName
{
    public byte name;
}

public unsafe struct NewsNewlyArrivedEventHolder
{
    public Service s;
}

public unsafe struct NewsDataService
{
    public Service s;
}

public unsafe struct NewsDatabaseService
{
    public Service s;
}

public unsafe struct NewsOverwriteEventHolder
{
    public Service s;
}

public unsafe struct NewsRecordV1
{
    public byte news_id;
    public byte user_id;
    public s64 received_at;
    public s32 read;
    public s32 newly;
    public s32 displayed;
}

public unsafe struct NewsRecord
{
    public byte news_id;
    public byte user_id;
    public NewsTopicName topic_id;
    public s64 received_at;
    public s64 pad_0;
    public s32 decoration_type;
    public s32 read;
    public s32 newly;
    public s32 displayed;
    public s32 feedback;
    public s32 pad_1;
    public s32 extra_1;
    public s32 extra_2;
}

public static class News
{
    public static extern Result newsInitialize(NewsServiceType service_type);
    public static extern void newsExit();
    public static extern Result newsCreateNewlyArrivedEventHolder(ref NewsNewlyArrivedEventHolder @out);
    public static extern Result newsCreateNewsDataService(ref NewsDataService @out);
    public static extern Result newsCreateNewsDatabaseService(ref NewsDatabaseService @out);
    public static extern Result newsCreateOverwriteEventHolder(ref NewsOverwriteEventHolder @out);
    public static extern Result newsPostLocalNews(IntPtr news, ulong size);
    public static extern Result newsSetPassphrase(u64 program_id, ref byte passphrase);
    public static extern Result newsGetSubscriptionStatus(ref byte filter, ref u32 status);
    public static extern Result newsGetTopicList(u32 channel, ref u32 out_count, ref NewsTopicName @out, u32 max_count);
    public static extern Result newsGetSavedataUsage(ref u64 current, ref u64 total);
    public static extern Result newsIsSystemUpdateRequired(ref bool @out);
    public static extern Result newsGetDatabaseVersion(ref u32 version);
    public static extern Result newsRequestImmediateReception(ref byte filter);
    public static extern Result newsSetSubscriptionStatus(ref byte filter, u32 status);
    public static extern Result newsClearStorage();
    public static extern Result newsClearSubscriptionStatusAll();
    public static extern Result newsGetNewsDatabaseDump(IntPtr buffer, u64 size, ref u64 @out);
    public static extern void newsNewlyArrivedEventHolderClose(ref NewsNewlyArrivedEventHolder srv);
    public static extern Result newsNewlyArrivedEventHolderGet(ref NewsNewlyArrivedEventHolder srv, ref Event @out);
    public static extern void newsDataClose(ref NewsDataService srv);
    public static extern Result newsDataOpen(ref NewsDataService srv, ref byte file_name);
    public static extern Result newsDataOpenWithNewsRecordV1(ref NewsDataService srv, ref NewsRecordV1 record);
    public static extern Result newsDataRead(ref NewsDataService srv, ref u64 bytes_read, u64 offset, IntPtr @out, ulong out_size);
    public static extern Result newsDataGetSize(ref NewsDataService srv, ref u64 size);
    public static extern Result newsDataOpenWithNewsRecord(ref NewsDataService srv, ref NewsRecord record);
    public static extern void newsDatabaseClose(ref NewsDatabaseService srv);
    public static extern Result newsDatabaseGetListV1(ref NewsDatabaseService srv, ref NewsRecordV1 @out, u32 max_count, ref byte where, ref byte order, ref u32 count, u32 offset);
    public static extern Result newsDatabaseCount(ref NewsDatabaseService srv, ref byte filter, ref u32 count);
    public static extern Result newsDatabaseGetList(ref NewsDatabaseService srv, ref NewsRecord @out, u32 max_count, ref byte where, ref byte order, ref u32 count, u32 offset);
    public static extern void newsOverwriteEventHolderClose(ref NewsOverwriteEventHolder srv);
    public static extern Result newsOverwriteEventHolderGet(ref NewsOverwriteEventHolder srv, ref Event @out);
}
