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
#include "NetStub.h"   // shared, reference-counted socket layer

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
extern char  _nc_error[256];
extern char  _nc_response[NC_BUF_SIZE + 1];
extern int   _nc_response_len;
extern int   _nc_status_code;

extern NcHeader _nc_resp_headers[NC_MAX_RESP_HDR];
extern int      _nc_resp_header_count;

extern NcHeader _nc_req_headers[NC_MAX_REQ_HDR];
extern int      _nc_req_header_count;
extern int      _nc_use_https;
extern int      _nc_connect_timeout;
extern int      _nc_io_timeout;

extern NcMethod _nc_method;
extern char     _nc_req_host[256];
extern char     _nc_req_path[1024];
extern char     _nc_req_body[NC_BUF_SIZE/4];
extern char     _nc_req_ctype[128];
extern char     _nc_download_path[512];

extern Thread   _nc_thread;
// volatile + explicit barriers: the worker thread publishes _nc_state only AFTER
// writing _nc_response et al., and readers must observe those writes first.
extern volatile NcState _nc_state;
extern bool     _nc_thread_open;

extern bool      _nc_sock_init;
extern bool      _nc_ssl_init;
extern SslContext _nc_ctx;

extern char  _nc_weather_temp[32];
extern char  _nc_weather_wind[32];
extern int   _nc_weather_code;

// ── Internal helpers ────────────────────────────────────────────────────────
static inline void _nc_set_err(const char* fmt, ...) {
    va_list ap; va_start(ap, fmt);
    vsnprintf(_nc_error, sizeof(_nc_error)-1, fmt, ap);
    va_end(ap);
}

void _nc_extract_num(const char* json, const char* field, char* dst, int dstsz);

void _nc_parse_weather(void);

void _nc_parse_meta(const char* raw);

int _nc_resolve(const char* host, struct sockaddr_in* out, int port);

int _nc_connect(int fd, struct sockaddr* addr, socklen_t alen, int secs);

int _nc_build_req(char* buf, int bufsz,
                                const char* method_str, const char* host,
                                const char* path, const char* body,
                                const char* content_type);

// ── Core request executors (run in background thread) ───────────────────────
int _nc_do_http(const char* host, const char* path,
                              const char* method_str, const char* body,
                              const char* content_type);

int _nc_do_https(const char* host, const char* path,
                               const char* method_str, const char* body,
                               const char* content_type);

void _nc_thread_fn(void* arg);

void _nc_import_ca(void);

int _nc_start_thread(void);

// ── NetClient public API (kept identical to networkDemo) ────────────────────
int NetClient_Init(void);

void NetClient_Finish(void);

static inline void NetClient_Exit(void) {
    NetClient_Finish();
    if (_nc_ssl_init)  { sslContextClose(&_nc_ctx); sslExit(); _nc_ssl_init = false; }
    if (_nc_sock_init) { CS2SX_Net_Release(); _nc_sock_init = false; }
}

static inline void NetClient_UseHttps(int enable)  { _nc_use_https = enable; }
static inline void NetClient_SetTimeout(int cs, int io) { _nc_connect_timeout = cs; _nc_io_timeout = io; }

void NetClient_SetHeader(const char* name, const char* value);
static inline void NetClient_ClearHeaders(void) { _nc_req_header_count = 0; }

int _nc_begin(NcMethod method, const char* host, const char* path,
                            const char* body, const char* ctype, const char* savepath);

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

const char* NetClient_GetRespHeader(const char* name);

void NetClient_ParseUrl(const char* url, char* host, int hostsz,
                                      char* path, int pathsz, int* use_https);

const char* NetClient_JsonStr(const char* json, const char* field, char* out, int outsz);

static inline int NetClient_JsonInt(const char* json, const char* field, int def_val) {
    char buf[32]; _nc_extract_num(json, field, buf, sizeof(buf));
    return buf[0] ? atoi(buf) : def_val;
}
static inline float NetClient_JsonFloat(const char* json, const char* field, float def_val) {
    char buf[32]; _nc_extract_num(json, field, buf, sizeof(buf));
    return buf[0] ? (float)atof(buf) : def_val;
}
int NetClient_JsonBool(const char* json, const char* field, int def_val);

static inline const char* NetClient_GetWeatherTemp(void) { return _nc_weather_temp; }
static inline const char* NetClient_GetWeatherWind(void) { return _nc_weather_wind; }
static inline int         NetClient_GetWeatherCode(void) { return _nc_weather_code; }

// ============================================================================
// CS2SX bridge — what the C# `Http` class maps to.
// Lazy init + atexit cleanup so the user doesn't have to manage lifecycle.
// ============================================================================

extern bool _cs2sx_http_inited;

void _cs2sx_http_ensure(void);

// Synchronous convenience: starts the threaded request, waits for it (frame
// pauses), returns the response body. Reliable because the I/O runs on the
// background thread — only the wait is synchronous.
const char* _cs2sx_http_sync(int is_post, const char* url,
                                           const char* body, const char* ctype);

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

extern char _cs2sx_http_jsonbuf[512];
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
