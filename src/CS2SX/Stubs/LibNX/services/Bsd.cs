// Auto-generated from services\bsd.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public unsafe struct BsdInitConfig
{
    public u32 version;
    public IntPtr tmem_buffer;
    public ulong tmem_buffer_size;
    public u32 tcp_tx_buf_size;
    public u32 tcp_rx_buf_size;
    public u32 tcp_tx_buf_max_size;
    public u32 tcp_rx_buf_max_size;
    public u32 udp_tx_buf_size;
    public u32 udp_rx_buf_size;
    public u32 sb_efficiency;
}

public static class Bsd
{
    public static extern Result bsdInitialize(ref BsdInitConfig config, u32 num_sessions, u32 service_type);
    public static extern void bsdExit();
    public static extern IntPtr bsdGetServiceSession();
    public static extern int bsdSocket(int domain, int type, int protocol);
    public static extern int bsdSocketExempt(int domain, int type, int protocol);
    public static extern int bsdOpen(ref byte pathname, int flags);
    public static extern int bsdSelect(int nfds, ref fd_set readfds, ref fd_set writefds, ref fd_set exceptfds, ref struct timeval timeout);
    public static extern int bsdPoll(ref struct pollfd fds, nfds_t nfds, int timeout);
    public static extern int bsdSysctl(ref int name, uint namelen, IntPtr oldp, ref ulong oldlenp, IntPtr newp, ulong newlen);
    public static extern ssize_t bsdRecv(int sockfd, IntPtr buf, ulong len, int flags);
    public static extern ssize_t bsdRecvFrom(int sockfd, IntPtr buf, ulong len, int flags, ref struct sockaddr src_addr, ref socklen_t addrlen);
    public static extern ssize_t bsdSend(int sockfd, IntPtr buf, ulong len, int flags);
    public static extern ssize_t bsdSendTo(int sockfd, IntPtr buf, ulong len, int flags, ref struct sockaddr dest_addr, socklen_t addrlen);
    public static extern int bsdAccept(int sockfd, ref struct sockaddr addr, ref socklen_t addrlen);
    public static extern int bsdBind(int sockfd, ref struct sockaddr addr, socklen_t addrlen);
    public static extern int bsdConnect(int sockfd, ref struct sockaddr addr, socklen_t addrlen);
    public static extern int bsdGetPeerName(int sockfd, ref struct sockaddr addr, ref socklen_t addrlen);
    public static extern int bsdGetSockName(int sockfd, ref struct sockaddr addr, ref socklen_t addrlen);
    public static extern int bsdGetSockOpt(int sockfd, int level, int optname, IntPtr optval, ref socklen_t optlen);
    public static extern int bsdListen(int sockfd, int backlog);
    public static extern int bsdIoctl(int fd, int request, IntPtr data);
    public static extern int bsdFcntl(int fd, int cmd, int flags);
    public static extern int bsdSetSockOpt(int sockfd, int level, int optname, IntPtr optval, socklen_t optlen);
    public static extern int bsdShutdown(int sockfd, int how);
    public static extern int bsdShutdownAllSockets(int how);
    public static extern ssize_t bsdWrite(int fd, IntPtr buf, ulong count);
    public static extern ssize_t bsdRead(int fd, IntPtr buf, ulong count);
    public static extern int bsdClose(int fd);
    public static extern int bsdDuplicateSocket(int sockfd);
    public static extern int bsdRecvMMsg(int sockfd, IntPtr buf, ulong size, uint vlen, int flags, ref struct timespec timeout);
    public static extern int bsdSendMMsg(int sockfd, IntPtr buf, ulong size, uint vlen, int flags);
}
