#pragma once
// ============================================================================
// Runtime/NetStub.h — shared socket layer for all network users (HTTP, FTP, ...).
//
// socketInitializeDefault() may only run once per process and socketExit() tears
// the layer down for everyone. Each feature therefore acquires the layer and
// releases it when done; the last release shuts it down.
// ============================================================================

#include <switch.h>

/// Brings up BSD sockets on first use. Returns 1 on success, 0 on failure.
int  CS2SX_Net_Acquire(void);

/// Releases one acquisition; the last release calls socketExit().
void CS2SX_Net_Release(void);
