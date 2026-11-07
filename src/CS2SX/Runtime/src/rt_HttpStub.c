// Runtime module for HttpStub.h — generated split: state and non-trivial functions live
// here exactly once (a header `static` would give every translation unit its own copy).
#include "cs2sx_prelude.h"

char  _nc_error[256]               = "";
char  _nc_response[NC_BUF_SIZE + 1] = "";
int   _nc_response_len             = 0;
int   _nc_status_code              = 0;
NcHeader _nc_resp_headers[NC_MAX_RESP_HDR];
int      _nc_resp_header_count     = 0;
NcHeader _nc_req_headers[NC_MAX_REQ_HDR];
int      _nc_req_header_count      = 0;
int      _nc_use_https             = 0;
int      _nc_connect_timeout       = 6;
int      _nc_io_timeout            = 10;
NcMethod _nc_method                = NC_GET;
char     _nc_req_host[256]         = "";
char     _nc_req_path[1024]        = "";
char     _nc_req_body[NC_BUF_SIZE/4] = "";
char     _nc_req_ctype[128]        = "";
char     _nc_download_path[512]    = "";
Thread   _nc_thread;
volatile NcState _nc_state         = NC_IDLE;
bool     _nc_thread_open           = false;
bool      _nc_sock_init            = false;
bool      _nc_ssl_init             = false;
SslContext _nc_ctx;
char  _nc_weather_temp[32]         = "";
char  _nc_weather_wind[32]         = "";
int   _nc_weather_code             = -1;
void _nc_extract_num(const char* json, const char* field, char* dst, int dstsz)
{
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

void _nc_parse_weather(void)
{
    const char* block = strstr(_nc_response, "\"current_weather\":");
    if (!block) block = _nc_response;
    _nc_extract_num(block, "temperature", _nc_weather_temp, sizeof(_nc_weather_temp));
    _nc_extract_num(block, "windspeed",   _nc_weather_wind, sizeof(_nc_weather_wind));
    char cb[16]=""; _nc_extract_num(block, "weathercode", cb, sizeof(cb));
    _nc_weather_code = cb[0] ? atoi(cb) : -1;
}

void _nc_parse_meta(const char* raw)
{
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

int _nc_resolve(const char* host, struct sockaddr_in* out, int port)
{
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

int _nc_connect(int fd, struct sockaddr* addr, socklen_t alen, int secs)
{
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

int _nc_build_req(char* buf, int bufsz,
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

int _nc_do_http(const char* host, const char* path,
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

int _nc_do_https(const char* host, const char* path,
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

void _nc_thread_fn(void* arg)
{
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

void _nc_import_ca(void)
{
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

int _nc_start_thread(void)
{
    _nc_state = NC_RUNNING;
    Result rc = threadCreate(&_nc_thread, _nc_thread_fn, NULL, NULL, 0x10000, 0x2C, -2);
    if (R_FAILED(rc)) { _nc_set_err("threadCreate: 0x%08x", rc); _nc_state = NC_IDLE; return 0; }
    rc = threadStart(&_nc_thread);
    if (R_FAILED(rc)) { threadClose(&_nc_thread); _nc_set_err("threadStart: 0x%08x", rc); _nc_state = NC_IDLE; return 0; }
    _nc_thread_open = true;
    return 1;
}

int NetClient_Init(void)
{
    if (_nc_sock_init && _nc_ssl_init) return 1;
    if (!_nc_sock_init) {
        if (!CS2SX_Net_Acquire()) { _nc_set_err("socket layer could not be initialised"); return 0; }
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

void NetClient_Finish(void)
{
    if (_nc_thread_open) {
        threadWaitForExit(&_nc_thread);
        threadClose(&_nc_thread);
        _nc_thread_open = false;
    }
    _nc_state = NC_IDLE;
}

void NetClient_SetHeader(const char* name, const char* value)
{
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

int _nc_begin(NcMethod method, const char* host, const char* path,
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

const char* NetClient_GetRespHeader(const char* name)
{
    for (int i = 0; i < _nc_resp_header_count; i++)
        if (strcasecmp(_nc_resp_headers[i].name, name) == 0)
            return _nc_resp_headers[i].value;
    return "";
}

void NetClient_ParseUrl(const char* url, char* host, int hostsz,
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

const char* NetClient_JsonStr(const char* json, const char* field, char* out, int outsz)
{
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

int NetClient_JsonBool(const char* json, const char* field, int def_val)
{
    char buf[16]; _nc_extract_num(json, field, buf, sizeof(buf));
    if (!buf[0]) return def_val;
    if (strncmp(buf, "true",  4) == 0) return 1;
    if (strncmp(buf, "false", 5) == 0) return 0;
    return atoi(buf) != 0;
}

bool _cs2sx_http_inited = false;
void _cs2sx_http_ensure(void)
{
    if (_cs2sx_http_inited) return;
    _cs2sx_http_inited = true;
    atexit(NetClient_Exit);
    NetClient_Init();
}

const char* _cs2sx_http_sync(int is_post, const char* url,
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

char _cs2sx_http_jsonbuf[512];
