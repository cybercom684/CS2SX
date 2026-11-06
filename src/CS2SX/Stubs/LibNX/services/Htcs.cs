// Auto-generated from services\htcs.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum HtcsSocketError
{
    HTCS_ENONE = 0,
    HTCS_EACCES = 2,
    HTCS_EADDRINUSE = 3,
    HTCS_EADDRNOTAVAIL = 4,
    HTCS_EAGAIN = 6,
    HTCS_EALREADY = 7,
    HTCS_EBADF = 8,
    HTCS_EBUSY = 10,
    HTCS_ECONNABORTED = 13,
    HTCS_ECONNREFUSED = 14,
    HTCS_ECONNRESET = 15,
    HTCS_EDESTADDRREQ = 17,
    HTCS_EFAULT = 21,
    HTCS_EINPROGRESS = 26,
    HTCS_EINTR = 27,
    HTCS_EINVAL = 28,
    HTCS_EIO = 29,
    HTCS_EISCONN = 30,
    HTCS_EMFILE = 33,
    HTCS_EMSGSIZE = 35,
    HTCS_ENETDOWN = 38,
    HTCS_ENETRESET = 39,
    HTCS_ENOBUFS = 42,
    HTCS_ENOMEM = 49,
    HTCS_ENOTCONN = 56,
    HTCS_ETIMEDOUT = 76,
    HTCS_EUNKNOWN = 79,
    HTCS_EWOULDBLOCK = HTCS_EAGAIN,
}

public enum HtcsMessageFlag
{
    HTCS_MSG_PEEK = 1,
    HTCS_MSG_WAITALL = 2,
}

public enum HtcsShutdownType
{
    HTCS_SHUT_RD = 0,
    HTCS_SHUT_WR = 1,
    HTCS_SHUT_RDWR = 2,
}

public enum HtcsFcntlOperation
{
    HTCS_F_GETFL = 3,
    HTCS_F_SETFL = 4,
}

public enum HtcsFcntlFlag
{
    HTCS_O_NONBLOCK = 4,
}

public enum HtcsAddressFamily
{
    HTCS_AF_HTCS = 0,
}

public unsafe struct HtcsPeerName
{
    public byte name;
}

public unsafe struct HtcsPortName
{
    public byte name;
}

public unsafe struct HtcsSockAddr
{
    public HtcsAddressFamilyType family;
    public HtcsPeerName peer_name;
    public HtcsPortName port_name;
}

public unsafe struct HtcsTimeVal
{
    public s64 tv_sec;
    public s64 tv_usec;
}

public unsafe struct HtcsFdSet
{
    public int fds;
}

public unsafe struct HtcsSocket
{
    public Service s;
}

public static class Htcs
{
    public static extern Result htcsInitialize(u32 num_sessions);
    public static extern void htcsExit();
    public static extern IntPtr htcsGetManagerServiceSession();
    public static extern IntPtr htcsGetMonitorServiceSession();
    public static extern Result htcsGetPeerNameAny(ref HtcsPeerName @out);
    public static extern Result htcsGetDefaultHostName(ref HtcsPeerName @out);
    public static extern Result htcsCreateSocket(ref s32 out_err, ref HtcsSocket @out, bool enable_disconnection_emulation);
    public static extern Result htcsStartSelect(ref u32 out_task_id, ref Handle out_event_handle, ref s32 read, ulong num_read, ref s32 write, ulong num_write, ref s32 except, ulong num_except, s64 tv_sec, s64 tv_usec);
    public static extern Result htcsEndSelect(ref s32 out_err, ref s32 out_count, ref s32 read, ulong num_read, ref s32 write, ulong num_write, ref s32 except, ulong num_except, u32 task_id);
    public static extern Result htcsSocketClose(ref HtcsSocket s, ref s32 out_err, ref s32 out_res);
    public static extern Result htcsSocketConnect(ref HtcsSocket s, ref s32 out_err, ref s32 out_res, ref HtcsSockAddr address);
    public static extern Result htcsSocketBind(ref HtcsSocket s, ref s32 out_err, ref s32 out_res, ref HtcsSockAddr address);
    public static extern Result htcsSocketListen(ref HtcsSocket s, ref s32 out_err, ref s32 out_res, s32 backlog_count);
    public static extern Result htcsSocketShutdown(ref HtcsSocket s, ref s32 out_err, ref s32 out_res, s32 how);
    public static extern Result htcsSocketFcntl(ref HtcsSocket s, ref s32 out_err, ref s32 out_res, s32 command, s32 value);
    public static extern Result htcsSocketAcceptStart(ref HtcsSocket s, ref u32 out_task_id, ref Handle out_event_handle);
    public static extern Result htcsSocketAcceptResults(ref HtcsSocket s, ref s32 out_err, ref HtcsSocket out_socket, ref HtcsSockAddr out_address, u32 task_id);
    public static extern Result htcsSocketRecvStart(ref HtcsSocket s, ref u32 out_task_id, ref Handle out_event_handle, s32 mem_size, s32 flags);
    public static extern Result htcsSocketRecvResults(ref HtcsSocket s, ref s32 out_err, ref s64 out_size, IntPtr buffer, ulong buffer_size, u32 task_id);
    public static extern Result htcsSocketSendStart(ref HtcsSocket s, ref u32 out_task_id, ref Handle out_event_handle, IntPtr buffer, ulong buffer_size, s32 flags);
    public static extern Result htcsSocketSendResults(ref HtcsSocket s, ref s32 out_err, ref s64 out_size, u32 task_id);
    public static extern Result htcsSocketStartSend(ref HtcsSocket s, ref u32 out_task_id, ref Handle out_event_handle, ref s64 out_max_size, s64 size, s32 flags);
    public static extern Result htcsSocketContinueSend(ref HtcsSocket s, ref s64 out_size, ref bool out_wait, IntPtr buffer, ulong buffer_size, u32 task_id);
    public static extern Result htcsSocketEndSend(ref HtcsSocket s, ref s32 out_err, ref s64 out_size, u32 task_id);
    public static extern Result htcsSocketStartRecv(ref HtcsSocket s, ref u32 out_task_id, ref Handle out_event_handle, s64 size, s32 flags);
    public static extern Result htcsSocketEndRecv(ref HtcsSocket s, ref s32 out_err, ref s64 out_size, IntPtr buffer, ulong buffer_size, u32 task_id);
    public static extern Result htcsSocketGetPrimitive(ref HtcsSocket s, ref s32 @out);
    public static extern void htcsCloseSocket(ref HtcsSocket s);
}
