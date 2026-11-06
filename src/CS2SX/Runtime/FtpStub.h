#pragma once
// ============================================================================
// Runtime/FtpStub.h — minimal FTP server for CS2SX (built-in runtime stub).
//
// Lets a PC connect over Wi-Fi (FileZilla, WinSCP, …) and transfer files to/from
// the console. The server runs on a BACKGROUND THREAD so the UI stays responsive.
// Passive mode (PASV) only — works behind the usual home-router NAT. One control
// client at a time (set the desktop client to a single connection).
//
// Rooted at "/" → the SD card (libnx maps "/" to sdmc via the default devoptab).
// File ops go through C stdio/dirent, so this serves whatever "/" resolves to.
//
// C# surface (see Stubs + handler):  Ftp.Start(port) · Ftp.Stop() · Ftp.IsRunning()
//   · Ftp.GetIP() · Ftp.GetPort() · Ftp.Clients()
//
// Sockets come from the shared, reference-counted layer in NetStub.h, so the FTP
// server and the HTTP client can be used together.
// ============================================================================

#include <switch.h>
#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include <stdarg.h>
#include <ctype.h>
#include <errno.h>
#include <unistd.h>
#include <fcntl.h>
#include <dirent.h>
#include <time.h>
#include <sys/stat.h>
#include <sys/socket.h>
#include <sys/select.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include "NetStub.h"   // shared, reference-counted socket layer

// ── shared state (one server; all calls come from a single C# class) ─────────
extern volatile int g_ftp_run;
extern volatile int g_ftp_active;
extern volatile int g_ftp_clients;
extern int   g_ftp_port;
extern char  g_ftp_ip[32];
extern int   g_ftp_sock_init;
extern Thread g_ftp_thread;

static inline void _ftp_send(int s, const char* str)
{
    send(s, str, strlen(str), 0);
}
void _ftp_sendf(int s, const char* fmt, ...);

// Resolve an FTP argument (absolute "/x" or relative "x") against the session cwd.
void _ftp_resolve(char* out, size_t cap, const char* cwd, const char* arg);

// Collapse a trailing "/.." by dropping the last path component (for CDUP/CWD ..).
void _ftp_parent(char* path);

// Writes an `ls -l`-style listing of `dir` to the data socket. Clients parse this.
void _ftp_list(int data, const char* dir);

// Opens a passive listening socket; fills *outPort. Returns the socket or -1.
int _ftp_pasv_open(int* outPort);

// Sends a whole file over the data socket. Returns 1 on success.
int _ftp_send_file(int data, const char* path);

// Receives a whole file from the data socket. Returns 1 on success.
int _ftp_recv_file(int data, const char* path);

// Reads one CRLF-terminated command line. Returns length, 0 on disconnect,
// -1 on a (recoverable) timeout so the loop can re-check the stop flag.
int _ftp_recv_line(int s, char* out, int cap);

void _ftp_serve(int cl);

void _ftp_thread_fn(void* arg);

// ── public API (mapped to C# Ftp.*) ──────────────────────────────────────────
int CS2SX_Ftp_Start(int port);
void CS2SX_Ftp_Stop(void);
static inline int         CS2SX_Ftp_IsRunning(void) { return g_ftp_active; }
static inline const char* CS2SX_Ftp_GetIP(void)     { return g_ftp_ip; }
static inline int         CS2SX_Ftp_GetPort(void)   { return g_ftp_port; }
static inline int         CS2SX_Ftp_Clients(void)   { return g_ftp_clients; }
