#pragma once
// ============================================================================
// Runtime/SaveDataStub.h — Save Data for CS2SX
//
// For NRO homebrew (no real title ID) all saves go to the SD card via the
// libnx sdmc devoptab (fopen), which commits to the SD reliably:
//   sdmc:/switch/cs2sx_data/<key>.txt
//
// IMPORTANT: Read/Write do NOT gate on _cs2sx_save_mounted. That flag is a
// per-translation-unit static, so a Mount() call in one .c file did not make
// it true in another — which silently disabled saving from any other module
// (e.g. a Settings controller). Read/Write are now self-sufficient.
// ============================================================================

#include <switch.h>
#include <stdio.h>
#include <string.h>
#include <sys/stat.h>

#define CS2SX_SAVE_DIR   "sdmc:/switch/cs2sx_data"
#define CS2SX_SAVE_BUFSZ 512

extern bool _cs2sx_save_mounted;
extern char _cs2sx_save_rbuf[CS2SX_SAVE_BUFSZ];

static inline void _cs2sx_save_ensure_dir(void)
{
    mkdir("sdmc:/switch", 0777);
    mkdir(CS2SX_SAVE_DIR, 0777);
}

static inline int CS2SX_SaveData_Mount(void)
{
    _cs2sx_save_ensure_dir();
    _cs2sx_save_mounted = true;
    return 1;
}

const char* CS2SX_SaveData_Read(const char* key);

void CS2SX_SaveData_Write(const char* key, const char* value);

static inline void CS2SX_SaveData_Commit(void)  { /* fclose already flushes */ }
static inline void CS2SX_SaveData_Unmount(void) { _cs2sx_save_mounted = false; }
