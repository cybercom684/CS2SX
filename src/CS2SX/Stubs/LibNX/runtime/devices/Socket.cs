// Auto-generated from runtime\devices\socket.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Runtime.Devices;

public enum BsdServiceType
{
    BsdServiceType_User = (1 << 0),
    BsdServiceType_System = (1 << 1),
    BsdServiceType_Auto = BsdServiceType_User | BsdServiceType_System,
}

public unsafe struct SocketInitConfig
{
    public u32 tcp_tx_buf_size;
    public u32 tcp_rx_buf_size;
    public u32 tcp_tx_buf_max_size;
    public u32 tcp_rx_buf_max_size;
    public u32 udp_tx_buf_size;
    public u32 udp_rx_buf_size;
    public u32 sb_efficiency;
    public u32 num_bsd_sessions;
    public BsdServiceType bsd_service_type;
}

public static class Socket
{
    public static extern Result socketInitialize(ref SocketInitConfig config);
    public static extern Result socketGetLastResult();
    public static extern void socketExit();
    public static extern int socketSslConnectionSetSocketDescriptor(ref SslConnection c, int sockfd);
    public static extern int socketSslConnectionGetSocketDescriptor(ref SslConnection c);
    public static extern int socketSslConnectionSetDtlsSocketDescriptor(ref SslConnection c, int sockfd, ref struct sockaddr addr, socklen_t addrlen);
    public static extern int socketNifmRequestRegisterSocketDescriptor(ref NifmRequest r, int sockfd);
    public static extern int socketNifmRequestUnregisterSocketDescriptor(ref NifmRequest r, int sockfd);
}
