#pragma once
// ============================================================================
// Runtime/SwkbdStub.h — System Software Keyboard (swkbd applet) for CS2SX
//
// Wraps the libnx swkbd library applet.
// Opens the on-screen keyboard overlay — BLOCKING until user confirms/cancels.
// Returns the entered text (empty string on cancel/error).
// Works in standard NRO homebrew mode.
// ============================================================================

#include <switch.h>
#include <string.h>

#define CS2SX_SWKBD_BUF_SIZE 512

extern char _cs2sx_swkbd_buf[CS2SX_SWKBD_BUF_SIZE];

const char* CS2SX_Keyboard_Show(
    const char* prompt, const char* initial);

const char* CS2SX_Keyboard_ShowPassword(const char* prompt);

const char* CS2SX_Keyboard_ShowNumber(
    const char* prompt, const char* initial);
