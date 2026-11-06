// Runtime module for NetStub.h — the reference-counted socket layer.
#include "cs2sx_prelude.h"

static int   s_net_refs = 0;
static Mutex s_net_lock = 0;   // FTP runs on its own thread

int CS2SX_Net_Acquire(void)
{
    int ok = 1;
    mutexLock(&s_net_lock);
    if (s_net_refs == 0 && R_FAILED(socketInitializeDefault()))
        ok = 0;
    if (ok) s_net_refs++;
    mutexUnlock(&s_net_lock);
    return ok;
}

void CS2SX_Net_Release(void)
{
    mutexLock(&s_net_lock);
    if (s_net_refs > 0 && --s_net_refs == 0)
        socketExit();
    mutexUnlock(&s_net_lock);
}
