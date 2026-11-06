// Auto-generated from services\ssl.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Services;

public enum SslServiceType
{
    SslServiceType_Default = 0,
    SslServiceType_System = 1,
}

public enum SslCaCertificateId
{
    SslCaCertificateId_All = -1,
    SslCaCertificateId_NintendoCAG3 = 1,
    SslCaCertificateId_NintendoClass2CAG3 = 2,
    SslCaCertificateId_NintendoRootCAG4 = 3,
    SslCaCertificateId_AmazonRootCA1 = 1000,
    SslCaCertificateId_StarfieldServicesRootCertificateAuthorityG2 = 1001,
    SslCaCertificateId_AddTrustExternalCARoot = 1002,
    SslCaCertificateId_COMODOCertificationAuthority = 1003,
    SslCaCertificateId_UTNDATACorpSGC = 1004,
    SslCaCertificateId_UTNUSERFirstHardware = 1005,
    SslCaCertificateId_BaltimoreCyberTrustRoot = 1006,
    SslCaCertificateId_CybertrustGlobalRoot = 1007,
    SslCaCertificateId_VerizonGlobalRootCA = 1008,
    SslCaCertificateId_DigiCertAssuredIDRootCA = 1009,
    SslCaCertificateId_DigiCertAssuredIDRootG2 = 1010,
    SslCaCertificateId_DigiCertGlobalRootCA = 1011,
    SslCaCertificateId_DigiCertGlobalRootG2 = 1012,
    SslCaCertificateId_DigiCertHighAssuranceEVRootCA = 1013,
    SslCaCertificateId_EntrustnetCertificationAuthority2048 = 1014,
    SslCaCertificateId_EntrustRootCertificationAuthority = 1015,
    SslCaCertificateId_EntrustRootCertificationAuthorityG2 = 1016,
    SslCaCertificateId_GeoTrustGlobalCA2 = 1017,
    SslCaCertificateId_GeoTrustGlobalCA = 1018,
    SslCaCertificateId_GeoTrustPrimaryCertificationAuthorityG3 = 1019,
    SslCaCertificateId_GeoTrustPrimaryCertificationAuthority = 1020,
    SslCaCertificateId_GlobalSignRootCA = 1021,
    SslCaCertificateId_GlobalSignRootCAR2 = 1022,
    SslCaCertificateId_GlobalSignRootCAR3 = 1023,
    SslCaCertificateId_GoDaddyClass2CertificationAuthority = 1024,
    SslCaCertificateId_GoDaddyRootCertificateAuthorityG2 = 1025,
    SslCaCertificateId_StarfieldClass2CertificationAuthority = 1026,
    SslCaCertificateId_StarfieldRootCertificateAuthorityG2 = 1027,
    SslCaCertificateId_thawtePrimaryRootCAG3 = 1028,
    SslCaCertificateId_thawtePrimaryRootCA = 1029,
    SslCaCertificateId_VeriSignClass3PublicPrimaryCertificationAuthorityG3 = 1030,
    SslCaCertificateId_VeriSignClass3PublicPrimaryCertificationAuthorityG5 = 1031,
    SslCaCertificateId_VeriSignUniversalRootCertificationAuthority = 1032,
    SslCaCertificateId_DSTRootCAX3 = 1033,
    SslCaCertificateId_USERTrustRsaCertificationAuthority = 1034,
    SslCaCertificateId_ISRGRootX10 = 1035,
    SslCaCertificateId_USERTrustEccCertificationAuthority = 1036,
    SslCaCertificateId_COMODORsaCertificationAuthority = 1037,
    SslCaCertificateId_COMODOEccCertificationAuthority = 1038,
    SslCaCertificateId_AmazonRootCA2 = 1039,
    SslCaCertificateId_AmazonRootCA3 = 1040,
    SslCaCertificateId_AmazonRootCA4 = 1041,
    SslCaCertificateId_DigiCertAssuredIDRootG3 = 1042,
    SslCaCertificateId_DigiCertGlobalRootG3 = 1043,
    SslCaCertificateId_DigiCertTrustedRootG4 = 1044,
    SslCaCertificateId_EntrustRootCertificationAuthorityEC1 = 1045,
    SslCaCertificateId_EntrustRootCertificationAuthorityG4 = 1046,
    SslCaCertificateId_GlobalSignECCRootCAR4 = 1047,
    SslCaCertificateId_GlobalSignECCRootCAR5 = 1048,
    SslCaCertificateId_GlobalSignECCRootCAR6 = 1049,
    SslCaCertificateId_GTSRootR1 = 1050,
    SslCaCertificateId_GTSRootR2 = 1051,
    SslCaCertificateId_GTSRootR3 = 1052,
    SslCaCertificateId_GTSRootR4 = 1053,
    SslCaCertificateId_SecurityCommunicationRootCA = 1054,
    SslCaCertificateId_GlobalSignRootE4 = 1055,
    SslCaCertificateId_GlobalSignRootR4 = 1056,
    SslCaCertificateId_TTeleSecGlobalRootClass2 = 1057,
    SslCaCertificateId_DigiCertTLSECCP384RootG5 = 1058,
    SslCaCertificateId_DigiCertTLSRSA4096RootG5 = 1059,
}

public enum SslTrustedCertStatus
{
    SslTrustedCertStatus_Invalid = -1,
    SslTrustedCertStatus_Removed = 0,
    SslTrustedCertStatus_EnabledTrusted = 1,
    SslTrustedCertStatus_EnabledNotTrusted = 2,
    SslTrustedCertStatus_Revoked = 3,
}

public enum SslFlushSessionCacheOptionType
{
    SslFlushSessionCacheOptionType_SingleHost = 0,
    SslFlushSessionCacheOptionType_AllHosts = 1,
}

public enum SslDebugOptionType
{
    SslDebugOptionType_AllowDisableVerifyOption = 0,
}

public enum SslVersion
{
    SslVersion_Auto = (1 << 0),
    SslVersion_TlsV10 = (1 << 3),
    SslVersion_TlsV11 = (1 << 4),
    SslVersion_TlsV12 = (1 << 5),
    SslVersion_TlsV13 = (1 << 6),
    SslVersion_Auto24 = (1 << 24),
}

public enum SslCertificateFormat
{
    SslCertificateFormat_Pem = 1,
    SslCertificateFormat_Der = 2,
}

public enum SslInternalPki
{
    SslInternalPki_DeviceClientCertDefault = 1,
}

public enum SslContextOption
{
    SslContextOption_CrlImportDateCheckEnable = 1,
}

public enum SslVerifyOption
{
    SslVerifyOption_PeerCa = (1 << 0),
    SslVerifyOption_HostName = (1 << 1),
    SslVerifyOption_DateCheck = (1 << 2),
    SslVerifyOption_EvCertPartial = (1 << 3),
    SslVerifyOption_EvPolicyOid = (1 << 4),
    SslVerifyOption_EvCertFingerprint = (1 << 5),
}

public enum SslIoMode
{
    SslIoMode_Blocking = 1,
    SslIoMode_NonBlocking = 2,
}

public enum SslPollEvent
{
    SslPollEvent_Read = (1 << 0),
    SslPollEvent_Write = (1 << 1),
    SslPollEvent_Except = (1 << 2),
}

public enum SslSessionCacheMode
{
    SslSessionCacheMode_None = 0,
    SslSessionCacheMode_SessionId = 1,
    SslSessionCacheMode_SessionTicket = 2,
}

public enum SslRenegotiationMode
{
    SslRenegotiationMode_None = 0,
    SslRenegotiationMode_Secure = 1,
}

public enum SslOptionType
{
    SslOptionType_DoNotCloseSocket = 0,
    SslOptionType_GetServerCertChain = 1,
    SslOptionType_SkipDefaultVerify = 2,
    SslOptionType_EnableAlpn = 3,
}

public enum SslPrivateOptionType
{
    SslPrivateOptionType_DtlsSession = 1,
    SslPrivateOptionType_SetCipher = 2,
}

public enum SslAlpnProtoState
{
    SslAlpnProtoState_NoSupport = 0,
    SslAlpnProtoState_Negotiated = 1,
    SslAlpnProtoState_NoOverlap = 2,
    SslAlpnProtoState_Selected = 3,
    SslAlpnProtoState_EarlyValue = 4,
}

public unsafe struct SslContext
{
    public Service s;
}

public unsafe struct SslConnection
{
    public Service s;
}

public unsafe struct SslBuiltInCertificateInfo
{
    public u32 cert_id;
    public u32 status;
    public u64 cert_size;
    public IntPtr cert_data;
}

public unsafe struct SslServerCertDetailHeader
{
    public u64 magicnum;
    public u32 cert_total;
    public u32 pad;
}

public unsafe struct SslServerCertDetailEntry
{
    public u32 size;
    public u32 offset;
}

public unsafe struct SslCipherInfo
{
    public byte cipher;
    public byte protocol_version;
}

public unsafe struct SslKeyAndCertParams
{
    public u32 unk_x0;
    public s32 key_size;
    public u64 public_exponent;
    public byte common_name;
    public u32 common_name_len;
}

public static class Ssl
{
    public static extern Result sslInitialize(u32 num_sessions);
    public static extern void sslExit();
    public static extern IntPtr sslGetServiceSession();
    public static extern Result sslCreateContext(ref SslContext c, u32 ssl_version);
    public static extern Result sslGetContextCount(ref u32 @out);
    public static extern Result sslGetCertificates(IntPtr buffer, u32 size, ref u32 ca_cert_ids, u32 count, ref u32 total_out);
    public static extern Result sslGetCertificateBufSize(ref u32 ca_cert_ids, u32 count, ref u32 @out);
    public static extern Result sslFlushSessionCache(ref byte str, ulong str_bufsize, SslFlushSessionCacheOptionType type, ref u32 @out);
    public static extern Result sslSetDebugOption(IntPtr buffer, ulong size, SslDebugOptionType type);
    public static extern Result sslGetDebugOption(IntPtr buffer, ulong size, SslDebugOptionType type);
    public static extern Result sslClearTls12FallbackFlag();
    public static extern Result sslSetThreadCoreMask(u64 mask);
    public static extern Result sslGetThreadCoreMask(ref u64 @out);
    public static extern void sslContextClose(ref SslContext c);
    public static extern Result sslContextSetOption(ref SslContext c, SslContextOption option, s32 value);
    public static extern Result sslContextGetOption(ref SslContext c, SslContextOption option, ref s32 @out);
    public static extern Result sslContextCreateConnection(ref SslContext c, ref SslConnection conn);
    public static extern Result sslContextGetConnectionCount(ref SslContext c, ref u32 @out);
    public static extern Result sslContextImportServerPki(ref SslContext c, IntPtr buffer, u32 size, SslCertificateFormat format, ref u64 id);
    public static extern Result sslContextImportClientPki(ref SslContext c, IntPtr pkcs12, u32 pkcs12_size, ref byte pw, u32 pw_size, ref u64 id);
    public static extern Result sslContextRemovePki(ref SslContext c, u64 id);
    public static extern Result sslContextRegisterInternalPki(ref SslContext c, SslInternalPki internal_pki, ref u64 id);
    public static extern Result sslContextAddPolicyOid(ref SslContext c, ref byte str, u32 str_bufsize);
    public static extern Result sslContextImportCrl(ref SslContext c, IntPtr buffer, u32 size, ref u64 id);
    public static extern Result sslContextImportClientCertKeyPki(ref SslContext c, IntPtr cert, u32 cert_size, IntPtr key, u32 key_size, SslCertificateFormat format, ref u64 id);
    public static extern Result sslContextGeneratePrivateKeyAndCert(ref SslContext c, IntPtr cert, u32 cert_size, IntPtr key, u32 key_size, u32 val, ref SslKeyAndCertParams @params, ref u32 out_certsize, ref u32 out_keysize);
    public static extern Result sslContextCreateConnectionForSystem(ref SslContext c, ref SslConnection conn);
    public static extern void sslConnectionClose(ref SslConnection c);
    public static extern Result sslConnectionSetSocketDescriptor(ref SslConnection c, int sockfd, ref int out_sockfd);
    public static extern Result sslConnectionSetHostName(ref SslConnection c, ref byte str, u32 str_bufsize);
    public static extern Result sslConnectionSetVerifyOption(ref SslConnection c, u32 verify_option);
    public static extern Result sslConnectionSetIoMode(ref SslConnection c, SslIoMode mode);
    public static extern Result sslConnectionGetSocketDescriptor(ref SslConnection c, ref int sockfd);
    public static extern Result sslConnectionGetHostName(ref SslConnection c, ref byte str, u32 str_bufsize, ref u32 @out);
    public static extern Result sslConnectionGetVerifyOption(ref SslConnection c, ref u32 @out);
    public static extern Result sslConnectionGetIoMode(ref SslConnection c, ref SslIoMode @out);
    public static extern Result sslConnectionDoHandshake(ref SslConnection c, ref u32 out_size, ref u32 total_certs, IntPtr server_certbuf, u32 server_certbuf_size);
    public static extern Result sslConnectionGetServerCertDetail(IntPtr certbuf, u32 certbuf_size, u32 cert_index, IntPtr cert, ref u32 cert_size);
    public static extern Result sslConnectionRead(ref SslConnection c, IntPtr buffer, u32 size, ref u32 out_size);
    public static extern Result sslConnectionWrite(ref SslConnection c, IntPtr buffer, u32 size, ref u32 out_size);
    public static extern Result sslConnectionPending(ref SslConnection c, ref s32 @out);
    public static extern Result sslConnectionPeek(ref SslConnection c, IntPtr buffer, u32 size, ref u32 out_size);
    public static extern Result sslConnectionPoll(ref SslConnection c, u32 in_pollevent, ref u32 out_pollevent, u32 timeout);
    public static extern Result sslConnectionGetVerifyCertError(ref SslConnection c);
    public static extern Result sslConnectionGetNeededServerCertBufferSize(ref SslConnection c, ref u32 @out);
    public static extern Result sslConnectionSetSessionCacheMode(ref SslConnection c, SslSessionCacheMode mode);
    public static extern Result sslConnectionGetSessionCacheMode(ref SslConnection c, ref SslSessionCacheMode @out);
    public static extern Result sslConnectionFlushSessionCache(ref SslConnection c);
    public static extern Result sslConnectionSetRenegotiationMode(ref SslConnection c, SslRenegotiationMode mode);
    public static extern Result sslConnectionGetRenegotiationMode(ref SslConnection c, ref SslRenegotiationMode @out);
    public static extern Result sslConnectionSetOption(ref SslConnection c, SslOptionType option, bool flag);
    public static extern Result sslConnectionGetOption(ref SslConnection c, SslOptionType option, ref bool @out);
    public static extern Result sslConnectionGetVerifyCertErrors(ref SslConnection c, ref u32 out0, ref u32 out1, ref Result errors, u32 count);
    public static extern Result sslConnectionGetCipherInfo(ref SslConnection c, ref SslCipherInfo @out);
    public static extern Result sslConnectionSetNextAlpnProto(ref SslConnection c, ref u8 buffer, u32 size);
    public static extern Result sslConnectionGetNextAlpnProto(ref SslConnection c, ref SslAlpnProtoState state, ref u32 @out, ref u8 buffer, u32 size);
    public static extern Result sslConnectionSetDtlsSocketDescriptor(ref SslConnection c, int sockfd, IntPtr buf, ulong size, ref int out_sockfd);
    public static extern Result sslConnectionGetDtlsHandshakeTimeout(ref SslConnection c, ref u64 @out);
    public static extern Result sslConnectionSetPrivateOption(ref SslConnection c, SslPrivateOptionType option, u32 value);
    public static extern Result sslConnectionSetSrtpCiphers(ref SslConnection c, ref u16 ciphers, u32 count);
    public static extern Result sslConnectionGetSrtpCipher(ref SslConnection c, ref u16 @out);
    public static extern Result sslConnectionExportKeyingMaterial(ref SslConnection c, ref u8 outbuf, u32 outbuf_size, ref byte label, u32 label_size, IntPtr context, u32 context_size);
    public static extern Result sslConnectionSetIoTimeout(ref SslConnection c, u32 timeout);
    public static extern Result sslConnectionGetIoTimeout(ref SslConnection c, ref u32 @out);
}
