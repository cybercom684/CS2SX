// Runtime module for SaveDataStub.h: each app keeps its values in its own folder,
// sdmc:/switch/cs2sx_data/<app>/<key>.txt (<app> = the project name, set by the
// build in g_cs2sx_app_name). Values saved before lie directly in cs2sx_data/;
// Read falls back to them until the app writes the key again.
#include "cs2sx_prelude.h"

bool _cs2sx_save_mounted = false;
char _cs2sx_save_rbuf[CS2SX_SAVE_BUFSZ];

static void save_path(char* out, size_t size, const char* key)
{
    snprintf(out, size, CS2SX_SAVE_DIR "/%s/%s.txt", g_cs2sx_app_name, key ? key : "");
}

static void old_path(char* out, size_t size, const char* key)
{
    snprintf(out, size, CS2SX_SAVE_DIR "/%s.txt", key ? key : "");
}

/// The whole value (any length, several lines), valid until the next frame; ""
/// when the key was never written.
const char* CS2SX_SaveData_Read(const char* key)
{
    char path[512];
    save_path(path, sizeof path, key);
    FILE* f = fopen(path, "rb");
    if (!f)
    {
        old_path(path, sizeof path, key);
        f = fopen(path, "rb");
        if (!f) return "";
    }
    fseek(f, 0, SEEK_END);
    long size = ftell(f);
    fseek(f, 0, SEEK_SET);
    if (size <= 0) { fclose(f); return ""; }
    char* buf = CS2SX_StrAlloc((size_t)size);
    size_t got = fread(buf, 1, (size_t)size, f);
    fclose(f);
    // (a trailing newline is not part of the value)
    while (got > 0 && (buf[got - 1] == '\n' || buf[got - 1] == '\r')) got--;
    buf[got] = '\0';
    return buf;
}

void CS2SX_SaveData_Write(const char* key, const char* value)
{
    if (!value) return;
    _cs2sx_save_ensure_dir();   // self-sufficient — don't rely on Mount()

    char path[512];
    save_path(path, sizeof path, key);
    FILE* f = fopen(path, "wb");
    if (!f) return;
    fputs(value, f);
    fflush(f);
    fclose(f);
    // (The old shared file stays: another app may still read it.)
}
