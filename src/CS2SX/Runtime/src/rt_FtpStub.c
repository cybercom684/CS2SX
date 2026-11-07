// Runtime module for FtpStub.h — generated split: state and non-trivial functions live
// here exactly once (a header `static` would give every translation unit its own copy).
#include "cs2sx_prelude.h"

volatile int g_ftp_run    = 0;
volatile int g_ftp_active = 0;
volatile int g_ftp_clients = 0;
int   g_ftp_port = 5000;
char  g_ftp_ip[32] = "0.0.0.0";
int   g_ftp_sock_init = 0;
Thread g_ftp_thread;
void _ftp_sendf(int s, const char* fmt, ...)
{
    char buf[1100];
    va_list ap; va_start(ap, fmt);
    vsnprintf(buf, sizeof(buf), fmt, ap);
    va_end(ap);
    send(s, buf, strlen(buf), 0);
}

void _ftp_resolve(char* out, size_t cap, const char* cwd, const char* arg)
{
    if (!arg || !arg[0]) { snprintf(out, cap, "%s", cwd); return; }
    if (arg[0] == '/') { snprintf(out, cap, "%s", arg); return; }
    size_t L = strlen(cwd);
    if (L > 0 && cwd[L - 1] == '/') snprintf(out, cap, "%s%s", cwd, arg);
    else                            snprintf(out, cap, "%s/%s", cwd, arg);
}

void _ftp_parent(char* path)
{
    int len = (int)strlen(path);
    while (len > 1 && path[len - 1] == '/') path[--len] = '\0';
    int slash = -1;
    for (int i = len - 1; i >= 0; i--) if (path[i] == '/') { slash = i; break; }
    if (slash <= 0) { path[0] = '/'; path[1] = '\0'; }
    else            { path[slash] = '\0'; }
}

void _ftp_list(int data, const char* dir)
{
    DIR* d = opendir(dir);
    if (!d) return;
    struct dirent* e;
    char full[1100], line[1300], tbuf[24];
    while ((e = readdir(d)) != NULL)
    {
        const char* nm = e->d_name;
        if (nm[0] == '.' && (nm[1] == '\0' || (nm[1] == '.' && nm[2] == '\0'))) continue;
        size_t L = strlen(dir);
        if (L > 0 && dir[L - 1] == '/') snprintf(full, sizeof(full), "%s%s", dir, nm);
        else                            snprintf(full, sizeof(full), "%s/%s", dir, nm);

        struct stat st;
        int isdir = 0; long long sz = 0; time_t mt = 0;
        if (stat(full, &st) == 0) { isdir = S_ISDIR(st.st_mode); sz = (long long)st.st_size; mt = st.st_mtime; }

        struct tm* tm = localtime(&mt);
        if (tm) strftime(tbuf, sizeof(tbuf), "%b %d %H:%M", tm);
        else    snprintf(tbuf, sizeof(tbuf), "Jan 01 00:00");

        snprintf(line, sizeof(line), "%s 1 switch switch %lld %s %s\r\n",
                 isdir ? "drwxr-xr-x" : "-rw-r--r--", sz, tbuf, nm);
        send(data, line, strlen(line), 0);
    }
    closedir(d);
}

int _ftp_pasv_open(int* outPort)
{
    int ls = socket(AF_INET, SOCK_STREAM, 0);
    if (ls < 0) return -1;
    int yes = 1; setsockopt(ls, SOL_SOCKET, SO_REUSEADDR, &yes, sizeof(yes));
    struct sockaddr_in a; memset(&a, 0, sizeof(a));
    a.sin_family = AF_INET; a.sin_addr.s_addr = INADDR_ANY; a.sin_port = 0;
    if (bind(ls, (struct sockaddr*)&a, sizeof(a)) < 0) { close(ls); return -1; }
    if (listen(ls, 1) < 0) { close(ls); return -1; }
    struct sockaddr_in got; socklen_t gl = sizeof(got);
    if (getsockname(ls, (struct sockaddr*)&got, &gl) < 0) { close(ls); return -1; }
    *outPort = ntohs(got.sin_port);
    // a 12s accept timeout so a stalled client can't wedge the session
    struct timeval tv; tv.tv_sec = 12; tv.tv_usec = 0;
    setsockopt(ls, SOL_SOCKET, SO_RCVTIMEO, &tv, sizeof(tv));
    return ls;
}

int _ftp_send_file(int data, const char* path)
{
    FILE* f = fopen(path, "rb");
    if (!f) return 0;
    static char buf[16384];
    size_t r; int ok = 1;
    while ((r = fread(buf, 1, sizeof(buf), f)) > 0)
    {
        size_t off = 0;
        while (off < r)
        {
            ssize_t w = send(data, buf + off, r - off, 0);
            if (w <= 0) { ok = 0; break; }
            off += (size_t)w;
        }
        if (!ok) break;
    }
    fclose(f);
    return ok;
}

int _ftp_recv_file(int data, const char* path)
{
    FILE* f = fopen(path, "wb");
    if (!f) return 0;
    static char buf[16384];
    ssize_t r; int ok = 1;
    while ((r = recv(data, buf, sizeof(buf), 0)) > 0)
        if (fwrite(buf, 1, (size_t)r, f) != (size_t)r) { ok = 0; break; }
    fclose(f);
    return ok;
}

int _ftp_recv_line(int s, char* out, int cap)
{
    int n = 0;
    while (n < cap - 1)
    {
        char c;
        ssize_t r = recv(s, &c, 1, 0);
        if (r == 0) return 0;
        if (r < 0)
        {
            if (errno == EAGAIN || errno == EWOULDBLOCK) return (n > 0) ? n : -1;
            return 0;
        }
        if (c == '\n') break;
        if (c != '\r') out[n++] = c;
    }
    out[n] = '\0';
    return n;
}

void _ftp_serve(int cl)
{
    // 1s recv timeout so a quiet session still polls the stop flag.
    struct timeval tv; tv.tv_sec = 1; tv.tv_usec = 0;
    setsockopt(cl, SOL_SOCKET, SO_RCVTIMEO, &tv, sizeof(tv));

    char cwd[1024]; strcpy(cwd, "/");
    char rnfr[1100]; rnfr[0] = '\0';
    int pasv = -1;

    _ftp_send(cl, "220 CS2SX FTP ready\r\n");

    char line[1200];
    while (g_ftp_run)
    {
        int n = _ftp_recv_line(cl, line, sizeof(line));
        if (n == 0) break;            // disconnect
        if (n < 0) continue;          // timeout → re-check g_ftp_run

        // split "CMD arg..."
        char cmd[8]; int ci = 0;
        int i = 0;
        while (line[i] && line[i] != ' ' && ci < 7) cmd[ci++] = (char)toupper((unsigned char)line[i++]);
        cmd[ci] = '\0';
        while (line[i] == ' ') i++;
        const char* arg = line + i;

        if (!strcmp(cmd, "USER"))      _ftp_send(cl, "331 Any user welcome\r\n");
        else if (!strcmp(cmd, "PASS")) _ftp_send(cl, "230 Logged in\r\n");
        else if (!strcmp(cmd, "SYST")) _ftp_send(cl, "215 UNIX Type: L8\r\n");
        else if (!strcmp(cmd, "FEAT")) _ftp_send(cl, "211-Features\r\n SIZE\r\n211 End\r\n");
        else if (!strcmp(cmd, "NOOP")) _ftp_send(cl, "200 OK\r\n");
        else if (!strcmp(cmd, "TYPE")) _ftp_send(cl, "200 OK\r\n");
        else if (!strcmp(cmd, "PWD") || !strcmp(cmd, "XPWD"))
            _ftp_sendf(cl, "257 \"%s\"\r\n", cwd);
        else if (!strcmp(cmd, "CWD"))
        {
            char np[1100];
            if (!strcmp(arg, "..")) { strcpy(np, cwd); _ftp_parent(np); }
            else _ftp_resolve(np, sizeof(np), cwd, arg);
            struct stat st;
            if (stat(np, &st) == 0 && S_ISDIR(st.st_mode)) { strncpy(cwd, np, sizeof(cwd) - 1); cwd[sizeof(cwd)-1]='\0'; _ftp_send(cl, "250 OK\r\n"); }
            else _ftp_send(cl, "550 No such directory\r\n");
        }
        else if (!strcmp(cmd, "CDUP")) { _ftp_parent(cwd); _ftp_send(cl, "250 OK\r\n"); }
        else if (!strcmp(cmd, "PASV"))
        {
            if (pasv >= 0) { close(pasv); pasv = -1; }
            int port = 0;
            pasv = _ftp_pasv_open(&port);
            if (pasv < 0) { _ftp_send(cl, "425 Cannot open data port\r\n"); continue; }
            u32 ip = gethostid();
            unsigned char* b = (unsigned char*)&ip;
            _ftp_sendf(cl, "227 Entering Passive Mode (%u,%u,%u,%u,%u,%u)\r\n",
                       b[0], b[1], b[2], b[3], (port >> 8) & 0xFF, port & 0xFF);
        }
        else if (!strcmp(cmd, "LIST") || !strcmp(cmd, "NLST"))
        {
            // path = cwd unless a non-flag arg is given
            char dir[1100]; strcpy(dir, cwd);
            if (arg[0] && arg[0] != '-') _ftp_resolve(dir, sizeof(dir), cwd, arg);
            if (pasv < 0) { _ftp_send(cl, "425 Use PASV first\r\n"); continue; }
            _ftp_send(cl, "150 Here comes the listing\r\n");
            int data = accept(pasv, NULL, NULL);
            close(pasv); pasv = -1;
            if (data < 0) { _ftp_send(cl, "426 Data connection failed\r\n"); continue; }
            _ftp_list(data, dir);
            close(data);
            _ftp_send(cl, "226 Directory send OK\r\n");
        }
        else if (!strcmp(cmd, "RETR"))
        {
            char path[1100]; _ftp_resolve(path, sizeof(path), cwd, arg);
            if (pasv < 0) { _ftp_send(cl, "425 Use PASV first\r\n"); continue; }
            _ftp_send(cl, "150 Opening data connection\r\n");
            int data = accept(pasv, NULL, NULL);
            close(pasv); pasv = -1;
            if (data < 0) { _ftp_send(cl, "426 Data connection failed\r\n"); continue; }
            int ok = _ftp_send_file(data, path);
            close(data);
            _ftp_send(cl, ok ? "226 Transfer complete\r\n" : "550 Failed to read file\r\n");
        }
        else if (!strcmp(cmd, "STOR"))
        {
            char path[1100]; _ftp_resolve(path, sizeof(path), cwd, arg);
            if (pasv < 0) { _ftp_send(cl, "425 Use PASV first\r\n"); continue; }
            _ftp_send(cl, "150 Ready to receive\r\n");
            int data = accept(pasv, NULL, NULL);
            close(pasv); pasv = -1;
            if (data < 0) { _ftp_send(cl, "426 Data connection failed\r\n"); continue; }
            int ok = _ftp_recv_file(data, path);
            close(data);
            _ftp_send(cl, ok ? "226 Transfer complete\r\n" : "550 Failed to write file\r\n");
        }
        else if (!strcmp(cmd, "SIZE"))
        {
            char path[1100]; _ftp_resolve(path, sizeof(path), cwd, arg);
            struct stat st;
            if (stat(path, &st) == 0 && !S_ISDIR(st.st_mode)) _ftp_sendf(cl, "213 %lld\r\n", (long long)st.st_size);
            else _ftp_send(cl, "550 Not a file\r\n");
        }
        else if (!strcmp(cmd, "DELE"))
        {
            char path[1100]; _ftp_resolve(path, sizeof(path), cwd, arg);
            _ftp_send(cl, remove(path) == 0 ? "250 File deleted\r\n" : "550 Delete failed\r\n");
        }
        else if (!strcmp(cmd, "MKD") || !strcmp(cmd, "XMKD"))
        {
            char path[1100]; _ftp_resolve(path, sizeof(path), cwd, arg);
            if (mkdir(path, 0777) == 0) _ftp_sendf(cl, "257 \"%s\" created\r\n", path);
            else _ftp_send(cl, "550 Create failed\r\n");
        }
        else if (!strcmp(cmd, "RMD") || !strcmp(cmd, "XRMD"))
        {
            char path[1100]; _ftp_resolve(path, sizeof(path), cwd, arg);
            _ftp_send(cl, rmdir(path) == 0 ? "250 Directory removed\r\n" : "550 Remove failed\r\n");
        }
        else if (!strcmp(cmd, "RNFR"))
        {
            _ftp_resolve(rnfr, sizeof(rnfr), cwd, arg);
            struct stat st;
            _ftp_send(cl, stat(rnfr, &st) == 0 ? "350 Ready for RNTO\r\n" : "550 Not found\r\n");
        }
        else if (!strcmp(cmd, "RNTO"))
        {
            char path[1100]; _ftp_resolve(path, sizeof(path), cwd, arg);
            if (rnfr[0] && rename(rnfr, path) == 0) _ftp_send(cl, "250 Renamed\r\n");
            else _ftp_send(cl, "550 Rename failed\r\n");
            rnfr[0] = '\0';
        }
        else if (!strcmp(cmd, "QUIT")) { _ftp_send(cl, "221 Bye\r\n"); break; }
        else _ftp_send(cl, "502 Command not implemented\r\n");
    }
    if (pasv >= 0) close(pasv);
}

void _ftp_thread_fn(void* arg)
{
    (void)arg;
    int srv = socket(AF_INET, SOCK_STREAM, 0);
    if (srv < 0) { g_ftp_active = 0; g_ftp_run = 0; return; }
    int yes = 1; setsockopt(srv, SOL_SOCKET, SO_REUSEADDR, &yes, sizeof(yes));
    struct sockaddr_in a; memset(&a, 0, sizeof(a));
    a.sin_family = AF_INET; a.sin_addr.s_addr = INADDR_ANY; a.sin_port = htons((u16)g_ftp_port);
    if (bind(srv, (struct sockaddr*)&a, sizeof(a)) < 0) { close(srv); g_ftp_active = 0; g_ftp_run = 0; return; }
    if (listen(srv, 2) < 0) { close(srv); g_ftp_active = 0; g_ftp_run = 0; return; }

    while (g_ftp_run)
    {
        fd_set rf; FD_ZERO(&rf); FD_SET(srv, &rf);
        struct timeval tv; tv.tv_sec = 0; tv.tv_usec = 200000;   // 200ms → poll stop flag
        int sel = select(srv + 1, &rf, NULL, NULL, &tv);
        if (sel <= 0) continue;
        int cl = accept(srv, NULL, NULL);
        if (cl < 0) continue;
        g_ftp_clients = 1;
        _ftp_serve(cl);
        close(cl);
        g_ftp_clients = 0;
    }
    close(srv);
    g_ftp_active = 0;
}

int CS2SX_Ftp_Start(int port)
{
    if (g_ftp_active) return 1;
    if (!g_ftp_sock_init)
    {
        if (!CS2SX_Net_Acquire()) return 0;   // shared with HTTP (NetStub.h)
        g_ftp_sock_init = 1;
    }
    g_ftp_port = (port > 0) ? port : 5000;

    u32 ip = gethostid();
    struct in_addr ia; ia.s_addr = ip;
    snprintf(g_ftp_ip, sizeof(g_ftp_ip), "%s", inet_ntoa(ia));

    g_ftp_run = 1; g_ftp_active = 1; g_ftp_clients = 0;
    Result rc = threadCreate(&g_ftp_thread, _ftp_thread_fn, NULL, NULL, 0x20000, 0x2C, -2);
    if (R_FAILED(rc)) { g_ftp_run = 0; g_ftp_active = 0; return 0; }
    if (R_FAILED(threadStart(&g_ftp_thread))) { threadClose(&g_ftp_thread); g_ftp_run = 0; g_ftp_active = 0; return 0; }
    return 1;
}

void CS2SX_Ftp_Stop(void)
{
    if (!g_ftp_active && !g_ftp_run) return;
    g_ftp_run = 0;
    threadWaitForExit(&g_ftp_thread);
    threadClose(&g_ftp_thread);
    g_ftp_active = 0; g_ftp_clients = 0;
    if (g_ftp_sock_init) { CS2SX_Net_Release(); g_ftp_sock_init = 0; }
}

