// Runtime module for SaveDataStub.h — generated split: state and non-trivial functions live
// here exactly once (a header `static` would give every translation unit its own copy).
#include "cs2sx_prelude.h"

bool _cs2sx_save_mounted = false;
char _cs2sx_save_rbuf[CS2SX_SAVE_BUFSZ];
const char* CS2SX_SaveData_Read(const char* key)
{
    _cs2sx_save_rbuf[0] = '\0';

    char path[256];
    snprintf(path, sizeof(path), CS2SX_SAVE_DIR "/%s.txt", key);

    FILE* f = fopen(path, "r");
    if (!f) return _cs2sx_save_rbuf;
    if (!fgets(_cs2sx_save_rbuf, CS2SX_SAVE_BUFSZ, f))
        _cs2sx_save_rbuf[0] = '\0';
    fclose(f);

    // strip trailing newline / CR
    size_t len = strlen(_cs2sx_save_rbuf);
    while (len > 0 && (_cs2sx_save_rbuf[len - 1] == '\n' || _cs2sx_save_rbuf[len - 1] == '\r'))
        _cs2sx_save_rbuf[--len] = '\0';
    return _cs2sx_save_rbuf;
}

void CS2SX_SaveData_Write(const char* key, const char* value)
{
    if (!value) return;
    _cs2sx_save_ensure_dir();   // self-sufficient — don't rely on Mount()

    char path[256];
    snprintf(path, sizeof(path), CS2SX_SAVE_DIR "/%s.txt", key);

    FILE* f = fopen(path, "w");
    if (!f) return;
    fputs(value, f);
    fflush(f);
    fclose(f);
}

