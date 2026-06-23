#pragma once
// ============================================================================
// Runtime/HttpStub.h — HTTP/HTTPS client for CS2SX
//
// This is the CS2SX NetClient engine (the same code proven in networkDemo),
// embedded as a built-in stub. The actual network I/O runs on a BACKGROUND
// THREAD — that is what makes it reliable on Switch hardware.
//
// Two ways to use it from C#:
//
//   1. Simple (synchronous) — the frame pauses until the response arrives:
//        string json = Http.Get("http://api.open-meteo.com/v1/forecast?...");
//        float t = Http.JsonFloat(json, "temperature", 0);
//
//   2. Async (no frame freeze) — start, poll each frame, then finish:
//        Http.BeginGet(host, path);
//        // each frame: if (Http.IsComplete()) { ... Http.GetResponse() ... Http.Finish(); }
//
// The URL scheme decides transport: "https://" → TLS/443, "http://" → 80.
// nifm-free: socketInitializeDefault brings up the interface; SSL is optional.
// Cleanup is registered via atexit so Atmosphere doesn't crash on exit.
// ============================================================================

#include <switch.h>
#include <switch/runtime/devices/socket.h>

#include <string.h>
#include <stdio.h>
#include <stdarg.h>
#include <stdlib.h>
#include <ctype.h>
#include <strings.h>
#include <sys/socket.h>
#include <sys/select.h>
#include <fcntl.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <netdb.h>
#include <errno.h>
#include <unistd.h>
#include <math.h>

// ── Constants ───────────────────────────────────────────────────────────────
#define NC_BUF_SIZE     65536
#define NC_REQ_SIZE     4096
#define NC_MAX_REQ_HDR  16
#define NC_MAX_RESP_HDR 32

typedef enum { NC_IDLE=0, NC_RUNNING=1, NC_DONE_OK=2, NC_DONE_ERR=3 } NcState;
typedef struct { char name[64]; char value[256]; } NcHeader;
typedef enum { NC_GET, NC_POST, NC_PUT, NC_DELETE, NC_HEAD, NC_DOWNLOAD } NcMethod;

// ── State (file-scope static → per translation unit) ────────────────────────
static char  _nc_error[256]               = "";
static char  _nc_response[NC_BUF_SIZE + 1] = "";
static int   _nc_response_len             = 0;
static int   _nc_status_code              = 0;

static NcHeader _nc_resp_headers[NC_MAX_RESP_HDR];
static int      _nc_resp_header_count     = 0;

static NcHeader _nc_req_headers[NC_MAX_REQ_HDR];
static int      _nc_req_header_count      = 0;
static int      _nc_use_https             = 0;
static int      _nc_connect_timeout       = 6;
static int      _nc_io_timeout            = 10;

static NcMethod _nc_method                = NC_GET;
static char     _nc_req_host[256]         = "";
static char     _nc_req_path[1024]        = "";
static char     _nc_req_body[NC_BUF_SIZE/4] = "";
static char     _nc_req_ctype[128]        = "";
static char     _nc_download_path[512]    = "";

static Thread   _nc_thread;
// volatile + explicit barriers: the worker thread publishes _nc_state only AFTER
// writing _nc_response et al., and readers must observe those writes first.
static volatile NcState _nc_state         = NC_IDLE;
static bool     _nc_thread_open           = false;

static bool      _nc_sock_init            = false;
static bool      _nc_ssl_init             = false;
static SslContext _nc_ctx;

static char  _nc_weather_temp[32]         = "";
static char  _nc_weather_wind[32]         = "";
static int   _nc_weather_code             = -1;

// ── Internal helpers ────────────────────────────────────────────────────────
static inline void _nc_set_err(const char* fmt, ...) {
    va_list ap; va_start(ap, fmt);
    vsnprintf(_nc_error, sizeof(_nc_error)-1, fmt, ap);
    va_end(ap);
}

static inline void _nc_extract_num(const char* json, const char* field, char* dst, int dstsz) {
    dst[0] = '\0';
    char needle[128];
    snprintf(needle, sizeof(needle), "\"%s\":", field);
    const char* p = strstr(json, needle);
    if (!p) return;
    p += strlen(needle);
    while (*p == ' ') p++;
    int i = 0;
    while (*p && *p != ',' && *p != '}' && *p != '"' && *p != ']' && i < dstsz-1)
        dst[i++] = *p++;
    dst[i] = '\0';
    while (i > 0 && (dst[i-1]==' '||dst[i-1]=='\r'||dst[i-1]=='\n')) dst[--i]='\0';
}

static inline void _nc_parse_weather(void) {
    const char* block = strstr(_nc_response, "\"current_weather\":");
    if (!block) block = _nc_response;
    _nc_extract_num(block, "temperature", _nc_weather_temp, sizeof(_nc_weather_temp));
    _nc_extract_num(block, "windspeed",   _nc_weather_wind, sizeof(_nc_weather_wind));
    char cb[16]=""; _nc_extract_num(block, "weathercode", cb, sizeof(cb));
    _nc_weather_code = cb[0] ? atoi(cb) : -1;
}

static inline void _nc_parse_meta(const char* raw) {
    _nc_status_code = 0;
    _nc_resp_header_count = 0;
    if (!raw || !raw[0]) return;

    if (strncmp(raw, "HTTP/", 5) == 0) {
        const char* sp = strchr(raw, ' ');
        if (sp) _nc_status_code = atoi(sp + 1);
    }

    const char* p = strchr(raw, '\n');
    if (p) p++;
    while (p && _nc_resp_header_count < NC_MAX_RESP_HDR) {
        if (*p == '\r' || *p == '\n') break;
        const char* colon = strchr(p, ':');
        const char* eol   = strchr(p, '\n');
        if (!colon || !eol || colon > eol) break;
        int nlen = (int)(colon - p);
        if (nlen > 0 && nlen < 63) {
            strncpy(_nc_resp_headers[_nc_resp_header_count].name, p, nlen);
            _nc_resp_headers[_nc_resp_header_count].name[nlen] = '\0';
            const char* val = colon + 1;
            while (*val == ' ') val++;
            int vlen = (int)(eol - val);
            if (vlen > 0 && val[vlen-1] == '\r') vlen--;
            if (vlen > 0 && vlen < 255) {
                strncpy(_nc_resp_headers[_nc_resp_header_count].value, val, vlen);
                _nc_resp_headers[_nc_resp_header_count].value[vlen] = '\0';
                _nc_resp_header_count++;
            }
        }
        p = eol + 1;
    }
}

static inline int _nc_resolve(const char* host, struct sockaddr_in* out, int port) {
    struct addrinfo hints, *res = NULL;
    memset(&hints, 0, sizeof(hints));
    hints.ai_family   = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    char ps[8]; snprintf(ps, sizeof(ps), "%d", port);
    int err = getaddrinfo(host, ps, &hints, &res);
    if (err || !res) { _nc_set_err("DNS failed for %s: %d", host, err); return 0; }
    memcpy(out, res->ai_addr, sizeof(*out));
    freeaddrinfo(res);
    return 1;
}

static inline int _nc_connect(int fd, struct sockaddr* addr, socklen_t alen, int secs) {
    int fl = fcntl(fd, F_GETFL, 0);
    fcntl(fd, F_SETFL, fl | O_NONBLOCK);
    int r = connect(fd, addr, alen);
    if (r == 0) { fcntl(fd, F_SETFL, fl); return 1; }
    if (errno != EINPROGRESS) { _nc_set_err("connect: %d", errno); return 0; }
    struct timeval tv = {.tv_sec=secs, .tv_usec=0};
    fd_set ws; FD_ZERO(&ws); FD_SET(fd, &ws);
    if (select(fd+1, NULL, &ws, NULL, &tv) <= 0) { _nc_set_err("connect timeout"); return 0; }
    int e=0; socklen_t el=sizeof(e);
    getsockopt(fd, SOL_SOCKET, SO_ERROR, &e, &el);
    if (e) { _nc_set_err("connect error: %d", e); return 0; }
    fcntl(fd, F_SETFL, fl);
    return 1;
}

static inline int _nc_build_req(char* buf, int bufsz,
                                const char* method_str, const char* host,
                                const char* path, const char* body,
                                const char* content_type)
{
    int pos = snprintf(buf, bufsz,
        "%s %s HTTP/1.1\r\n"
        "Host: %s\r\n"
        "User-Agent: CS2SX-NetClient/1.1\r\n"
        "Accept: */*\r\n",
        method_str, path, host);

    for (int i = 0; i < _nc_req_header_count && pos < bufsz-2; i++)
        pos += snprintf(buf+pos, bufsz-pos, "%s: %s\r\n",
                        _nc_req_headers[i].name, _nc_req_headers[i].value);

    int body_len = (body && body[0]) ? (int)strlen(body) : 0;
    if (body_len > 0) {
        if (content_type && content_type[0])
            pos += snprintf(buf+pos, bufsz-pos, "Content-Type: %s\r\n", content_type);
        pos += snprintf(buf+pos, bufsz-pos, "Content-Length: %d\r\n", body_len);
    }

    pos += snprintf(buf+pos, bufsz-pos, "Connection: close\r\n\r\n");
    if (body_len > 0 && pos + body_len < bufsz) {
        memcpy(buf+pos, body, body_len);
        pos += body_len;
        buf[pos] = '\0';
    }
    return pos;
}

// ── Core request executors (run in background thread) ───────────────────────
static inline int _nc_do_http(const char* host, const char* path,
                              const char* method_str, const char* body,
                              const char* content_type)
{
    struct sockaddr_in addr;
    if (!_nc_resolve(host, &addr, 80)) return 0;

    int fd = socket(AF_INET, SOCK_STREAM, 0);
    if (fd < 0) { _nc_set_err("socket: %d", errno); return 0; }

    struct timeval tv = {.tv_sec=_nc_io_timeout, .tv_usec=0};
    setsockopt(fd, SOL_SOCKET, SO_RCVTIMEO, &tv, sizeof(tv));
    setsockopt(fd, SOL_SOCKET, SO_SNDTIMEO, &tv, sizeof(tv));

    if (!_nc_connect(fd, (struct sockaddr*)&addr, sizeof(addr), _nc_connect_timeout)) {
        close(fd); return 0;
    }

    static char req[NC_REQ_SIZE];
    int rlen = _nc_build_req(req, sizeof(req), method_str, host, path, body, content_type);
    send(fd, req, rlen, 0);

    static char rbuf[NC_BUF_SIZE+1];
    int total=0, n;
    while ((n = recv(fd, rbuf+total, NC_BUF_SIZE-total, 0)) > 0) total += n;
    rbuf[total] = '\0';
    close(fd);

    _nc_parse_meta(rbuf);

    if (_nc_method == NC_HEAD) { _nc_response[0]='\0'; _nc_response_len=0; return 1; }

    char* body_start = strstr(rbuf, "\r\n\r\n");
    body_start = body_start ? body_start + 4 : rbuf;
    int blen = total - (int)(body_start - rbuf);
    if (blen < 0) blen = 0;
    if (blen > NC_BUF_SIZE) blen = NC_BUF_SIZE;

    if (_nc_method == NC_DOWNLOAD && _nc_download_path[0]) {
        FILE* f = fopen(_nc_download_path, "wb");
        if (f) { fwrite(body_start, 1, blen, f); fclose(f); }
        else   { _nc_set_err("fopen: %s", _nc_download_path); return 0; }
        _nc_response[0]='\0'; _nc_response_len=0;
    } else {
        memcpy(_nc_response, body_start, blen);
        _nc_response[blen] = '\0';
        _nc_response_len   = blen;
        _nc_parse_weather();
    }
    return 1;
}

static inline int _nc_do_https(const char* host, const char* path,
                               const char* method_str, const char* body,
                               const char* content_type)
{
    struct sockaddr_in addr;
    if (!_nc_resolve(host, &addr, 443)) return 0;

    int fd = socket(AF_INET, SOCK_STREAM, 0);
    if (fd < 0) { _nc_set_err("socket: %d", errno); return 0; }

    struct timeval tv = {.tv_sec=_nc_io_timeout, .tv_usec=0};
    setsockopt(fd, SOL_SOCKET, SO_RCVTIMEO, &tv, sizeof(tv));
    setsockopt(fd, SOL_SOCKET, SO_SNDTIMEO, &tv, sizeof(tv));

    if (!_nc_connect(fd, (struct sockaddr*)&addr, sizeof(addr), _nc_connect_timeout)) {
        close(fd); return 0;
    }

    SslConnection conn;
    Result rc = sslContextCreateConnection(&_nc_ctx, &conn);
    if (R_FAILED(rc)) { _nc_set_err("sslContextCreateConnection: 0x%08x", rc); close(fd); return 0; }

    int ssl_fd = socketSslConnectionSetSocketDescriptor(&conn, fd);
    if (ssl_fd < 0 && errno != ENOENT) {
        _nc_set_err("socketSslConnectionSetSocketDescriptor: %d", errno);
        sslConnectionClose(&conn); return 0;
    }
    fd = (ssl_fd >= 0) ? ssl_fd : -1;

    sslConnectionSetHostName(&conn, host, (u32)(strlen(host)+1));
    rc = sslConnectionSetVerifyOption(&conn, SslVerifyOption_PeerCa | SslVerifyOption_HostName);
    if (R_FAILED(rc)) sslConnectionSetVerifyOption(&conn, SslVerifyOption_PeerCa);

    rc = sslConnectionDoHandshake(&conn, NULL, NULL, NULL, 0);
    if (R_FAILED(rc)) {
        _nc_set_err("TLS handshake: 0x%08x", rc);
        sslConnectionClose(&conn); if (fd>=0) close(fd); return 0;
    }

    static char req[NC_REQ_SIZE];
    int rlen = _nc_build_req(req, sizeof(req), method_str, host, path, body, content_type);
    u32 sent = 0;
    rc = sslConnectionWrite(&conn, req, (u32)rlen, &sent);
    if (R_FAILED(rc)) {
        _nc_set_err("sslConnectionWrite: 0x%08x", rc);
        sslConnectionClose(&conn); if (fd>=0) close(fd); return 0;
    }

    static char rbuf[NC_BUF_SIZE+1];
    int total = 0;
    while (total < NC_BUF_SIZE) {
        u32 got = 0;
        rc = sslConnectionRead(&conn, rbuf+total, (u32)(NC_BUF_SIZE-total), &got);
        if (R_FAILED(rc) || got == 0) break;
        total += (int)got;
    }
    rbuf[total] = '\0';
    sslConnectionClose(&conn);
    if (fd >= 0) close(fd);

    _nc_parse_meta(rbuf);

    if (_nc_method == NC_HEAD) { _nc_response[0]='\0'; _nc_response_len=0; return 1; }

    char* body_start = strstr(rbuf, "\r\n\r\n");
    body_start = body_start ? body_start + 4 : rbuf;
    int blen = total - (int)(body_start - rbuf);
    if (blen < 0) blen = 0;
    if (blen > NC_BUF_SIZE) blen = NC_BUF_SIZE;

    if (_nc_method == NC_DOWNLOAD && _nc_download_path[0]) {
        FILE* f = fopen(_nc_download_path, "wb");
        if (f) { fwrite(body_start, 1, blen, f); fclose(f); }
        else   { _nc_set_err("fopen: %s", _nc_download_path); return 0; }
        _nc_response[0]='\0'; _nc_response_len=0;
    } else {
        memcpy(_nc_response, body_start, blen);
        _nc_response[blen] = '\0';
        _nc_response_len   = blen;
        _nc_parse_weather();
    }
    return 1;
}

static void _nc_thread_fn(void* arg) {
    (void)arg;
    _nc_error[0] = '\0';
    _nc_status_code = 0;
    _nc_resp_header_count = 0;

    const char* method_str = "GET";
    if      (_nc_method == NC_POST)     method_str = "POST";
    else if (_nc_method == NC_PUT)      method_str = "PUT";
    else if (_nc_method == NC_DELETE)   method_str = "DELETE";
    else if (_nc_method == NC_HEAD)     method_str = "HEAD";

    int ok = _nc_use_https
        ? _nc_do_https(_nc_req_host, _nc_req_path, method_str, _nc_req_body, _nc_req_ctype)
        : _nc_do_http (_nc_req_host, _nc_req_path, method_str, _nc_req_body, _nc_req_ctype);

    // Ensure all response-buffer writes are visible before the state flips to DONE,
    // so a reader that sees DONE also sees a fully-written _nc_response.
    __sync_synchronize();
    _nc_state = ok ? NC_DONE_OK : NC_DONE_ERR;
}

static inline void _nc_import_ca(void) {
    u32 all = 0xFFFFFFFF;
    u32 sz  = 0;
    if (R_FAILED(sslGetCertificateBufSize(&all, 1, &sz)) || sz == 0) return;
    void* buf = malloc(sz);
    if (!buf) return;
    u32 n = 0;
    if (R_SUCCEEDED(sslGetCertificates(buf, sz, &all, 1, &n)) && n > 0) {
        u64 id = 0;
        sslContextImportServerPki(&_nc_ctx, buf, sz, SslCertificateFormat_Der, &id);
    }
    free(buf);
}

static inline int _nc_start_thread(void) {
    _nc_state = NC_RUNNING;
    Result rc = threadCreate(&_nc_thread, _nc_thread_fn, NULL, NULL, 0x10000, 0x2C, -2);
    if (R_FAILED(rc)) { _nc_set_err("threadCreate: 0x%08x", rc); _nc_state = NC_IDLE; return 0; }
    rc = threadStart(&_nc_thread);
    if (R_FAILED(rc)) { threadClose(&_nc_thread); _nc_set_err("threadStart: 0x%08x", rc); _nc_state = NC_IDLE; return 0; }
    _nc_thread_open = true;
    return 1;
}

// ── NetClient public API (kept identical to networkDemo) ────────────────────
static inline int NetClient_Init(void) {
    if (_nc_sock_init && _nc_ssl_init) return 1;
    if (!_nc_sock_init) {
        Result rc = socketInitializeDefault();
        if (R_FAILED(rc)) { _nc_set_err("socketInitializeDefault: 0x%08x", rc); return 0; }
        _nc_sock_init = true;
    }
    if (!_nc_ssl_init) {
        Result rc = sslInitialize(2);
        if (R_FAILED(rc)) { _nc_set_err("sslInitialize: 0x%08x", rc); return 0; }
        rc = sslCreateContext(&_nc_ctx, SslVersion_Auto);
        if (R_FAILED(rc)) { sslExit(); _nc_set_err("sslCreateContext: 0x%08x", rc); return 0; }
        _nc_import_ca();
        _nc_ssl_init = true;
    }
    _nc_error[0] = '\0';
    return 1;
}

static inline void NetClient_Finish(void) {
    if (_nc_thread_open) {
        threadWaitForExit(&_nc_thread);
        threadClose(&_nc_thread);
        _nc_thread_open = false;
    }
    _nc_state = NC_IDLE;
}

static inline void NetClient_Exit(void) {
    NetClient_Finish();
    if (_nc_ssl_init)  { sslContextClose(&_nc_ctx); sslExit(); _nc_ssl_init = false; }
    if (_nc_sock_init) { socketExit(); _nc_sock_init = false; }
}

static inline void NetClient_UseHttps(int enable)  { _nc_use_https = enable; }
static inline void NetClient_SetTimeout(int cs, int io) { _nc_connect_timeout = cs; _nc_io_timeout = io; }

static inline void NetClient_SetHeader(const char* name, const char* value) {
    if (!name || !value) return;
    for (int i = 0; i < _nc_req_header_count; i++) {
        if (strcasecmp(_nc_req_headers[i].name, name) == 0) {
            strncpy(_nc_req_headers[i].value, value, 255); return;
        }
    }
    if (_nc_req_header_count < NC_MAX_REQ_HDR) {
        strncpy(_nc_req_headers[_nc_req_header_count].name,  name,  63);
        strncpy(_nc_req_headers[_nc_req_header_count].value, value, 255);
        _nc_req_header_count++;
    }
}
static inline void NetClient_ClearHeaders(void) { _nc_req_header_count = 0; }

static inline int _nc_begin(NcMethod method, const char* host, const char* path,
                            const char* body, const char* ctype, const char* savepath)
{
    if (_nc_state == NC_RUNNING) { _nc_set_err("request already in progress"); return 0; }
    NetClient_Finish();
    strncpy(_nc_req_host, host, sizeof(_nc_req_host)-1);
    strncpy(_nc_req_path, path, sizeof(_nc_req_path)-1);
    _nc_req_body[0]='\0'; _nc_req_ctype[0]='\0'; _nc_download_path[0]='\0';
    if (body)     strncpy(_nc_req_body,      body,    sizeof(_nc_req_body)-1);
    if (ctype)    strncpy(_nc_req_ctype,     ctype,   sizeof(_nc_req_ctype)-1);
    if (savepath) strncpy(_nc_download_path, savepath,sizeof(_nc_download_path)-1);
    _nc_method = method;
    return _nc_start_thread();
}

static inline int NetClient_BeginGet(const char* h, const char* p)
    { return _nc_begin(NC_GET, h, p, NULL, NULL, NULL); }
static inline int NetClient_BeginPost(const char* h, const char* p, const char* b, const char* ct)
    { return _nc_begin(NC_POST, h, p, b, ct, NULL); }
static inline int NetClient_BeginPut(const char* h, const char* p, const char* b, const char* ct)
    { return _nc_begin(NC_PUT, h, p, b, ct, NULL); }
static inline int NetClient_BeginDelete(const char* h, const char* p)
    { return _nc_begin(NC_DELETE, h, p, NULL, NULL, NULL); }
static inline int NetClient_BeginHead(const char* h, const char* p)
    { return _nc_begin(NC_HEAD, h, p, NULL, NULL, NULL); }
static inline int NetClient_BeginDownload(const char* h, const char* p, const char* save)
    { return _nc_begin(NC_DOWNLOAD, h, p, NULL, NULL, save); }

static inline int  NetClient_IsComplete(void) {
    NcState s = _nc_state;
    if (s == NC_DONE_OK || s == NC_DONE_ERR) { __sync_synchronize(); return 1; }
    return 0;
}
static inline int  NetClient_WasSuccess(void) { return _nc_state == NC_DONE_OK; }
static inline const char* NetClient_GetResponse(void)    { return _nc_response; }
static inline int         NetClient_GetResponseLen(void) { return _nc_response_len; }
static inline int         NetClient_GetStatusCode(void)  { return _nc_status_code; }
static inline const char* NetClient_GetError(void)       { return _nc_error; }

static inline const char* NetClient_GetRespHeader(const char* name) {
    for (int i = 0; i < _nc_resp_header_count; i++)
        if (strcasecmp(_nc_resp_headers[i].name, name) == 0)
            return _nc_resp_headers[i].value;
    return "";
}

static inline void NetClient_ParseUrl(const char* url, char* host, int hostsz,
                                      char* path, int pathsz, int* use_https)
{
    host[0]='\0'; path[0]='\0';
    if (use_https) *use_https = 0;
    int start = 0;
    if (strncmp(url, "https://", 8) == 0) { start = 8; if (use_https) *use_https = 1; }
    else if (strncmp(url, "http://", 7) == 0) { start = 7; if (use_https) *use_https = 0; }
    const char* slash = strchr(url + start, '/');
    if (slash) {
        int hlen = (int)(slash - url - start);
        if (hlen < hostsz) { strncpy(host, url+start, hlen); host[hlen]='\0'; }
        strncpy(path, slash, pathsz-1); path[pathsz-1]='\0';
    } else {
        strncpy(host, url+start, hostsz-1); host[hostsz-1]='\0';
        strncpy(path, "/", pathsz-1);
    }
}

static inline const char* NetClient_JsonStr(const char* json, const char* field, char* out, int outsz) {
    out[0] = '\0';
    if (!json || !field) return out;
    char needle[128];
    snprintf(needle, sizeof(needle), "\"%s\":\"", field);
    const char* p = strstr(json, needle);
    if (!p) return out;
    p += strlen(needle);
    int i = 0;
    while (*p && *p != '"' && i < outsz-1) {
        if (*p == '\\' && *(p+1)) { p++; }
        out[i++] = *p++;
    }
    out[i] = '\0';
    return out;
}

static inline int NetClient_JsonInt(const char* json, const char* field, int def_val) {
    char buf[32]; _nc_extract_num(json, field, buf, sizeof(buf));
    return buf[0] ? atoi(buf) : def_val;
}
static inline float NetClient_JsonFloat(const char* json, const char* field, float def_val) {
    char buf[32]; _nc_extract_num(json, field, buf, sizeof(buf));
    return buf[0] ? (float)atof(buf) : def_val;
}
static inline int NetClient_JsonBool(const char* json, const char* field, int def_val) {
    char buf[16]; _nc_extract_num(json, field, buf, sizeof(buf));
    if (!buf[0]) return def_val;
    if (strncmp(buf, "true",  4) == 0) return 1;
    if (strncmp(buf, "false", 5) == 0) return 0;
    return atoi(buf) != 0;
}

static inline const char* NetClient_GetWeatherTemp(void) { return _nc_weather_temp; }
static inline const char* NetClient_GetWeatherWind(void) { return _nc_weather_wind; }
static inline int         NetClient_GetWeatherCode(void) { return _nc_weather_code; }

// ============================================================================
// CS2SX bridge — what the C# `Http` class maps to.
// Lazy init + atexit cleanup so the user doesn't have to manage lifecycle.
// ============================================================================

static bool _cs2sx_http_inited = false;

static inline void _cs2sx_http_ensure(void) {
    if (_cs2sx_http_inited) return;
    _cs2sx_http_inited = true;
    atexit(NetClient_Exit);
    NetClient_Init();
}

// Synchronous convenience: starts the threaded request, waits for it (frame
// pauses), returns the response body. Reliable because the I/O runs on the
// background thread — only the wait is synchronous.
static inline const char* _cs2sx_http_sync(int is_post, const char* url,
                                           const char* body, const char* ctype)
{
    _cs2sx_http_ensure();
    char host[256]; char path[1024]; int https = 0;
    NetClient_ParseUrl(url, host, sizeof(host), path, sizeof(path), &https);
    NetClient_UseHttps(https);

    int ok = is_post ? NetClient_BeginPost(host, path, body, ctype)
                     : NetClient_BeginGet(host, path);
    if (!ok) return "";

    while (!NetClient_IsComplete())
        svcSleepThread(3000000ULL);  // 3 ms — yield to the worker thread

    const char* resp = NetClient_WasSuccess() ? NetClient_GetResponse() : "";
    NetClient_Finish();
    return resp;
}

static inline const char* CS2SX_Http_Get(const char* url)
    { return _cs2sx_http_sync(0, url, NULL, NULL); }
static inline const char* CS2SX_Http_Post(const char* url, const char* body)
    { return _cs2sx_http_sync(1, url, body, "text/plain"); }
static inline const char* CS2SX_Http_PostJson(const char* url, const char* json)
    { return _cs2sx_http_sync(1, url, json, "application/json"); }

static inline int CS2SX_Http_IsAvailable(void)      { _cs2sx_http_ensure(); return _nc_sock_init; }
static inline int CS2SX_Http_GetLastStatusCode(void){ return NetClient_GetStatusCode(); }

// JSON + weather helpers (operate on the last/any response)
static inline int   CS2SX_Http_JsonInt(const char* j, const char* f, int d)   { return NetClient_JsonInt(j, f, d); }
static inline float CS2SX_Http_JsonFloat(const char* j, const char* f, float d){ return NetClient_JsonFloat(j, f, d); }

static char _cs2sx_http_jsonbuf[512];
static inline const char* CS2SX_Http_JsonStr(const char* j, const char* f) {
    return NetClient_JsonStr(j, f, _cs2sx_http_jsonbuf, sizeof(_cs2sx_http_jsonbuf));
}

static inline const char* CS2SX_Http_WeatherTemp(void) { return NetClient_GetWeatherTemp(); }
static inline const char* CS2SX_Http_WeatherWind(void) { return NetClient_GetWeatherWind(); }
static inline int         CS2SX_Http_WeatherCode(void) { return NetClient_GetWeatherCode(); }

// Async passthrough (for UIs that must not freeze)
static inline int  CS2SX_Http_BeginGet(const char* h, const char* p)  { _cs2sx_http_ensure(); return NetClient_BeginGet(h, p); }
static inline int  CS2SX_Http_IsComplete(void) { return NetClient_IsComplete(); }
static inline int  CS2SX_Http_PollSuccess(void){ return NetClient_WasSuccess(); }
static inline const char* CS2SX_Http_Response(void) { return NetClient_GetResponse(); }
static inline void CS2SX_Http_FinishReq(void)  { NetClient_Finish(); }
