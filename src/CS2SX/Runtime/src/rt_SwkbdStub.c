// Runtime module for SwkbdStub.h — generated split: state and non-trivial functions live
// here exactly once (a header `static` would give every translation unit its own copy).
#include "cs2sx_prelude.h"

char _cs2sx_swkbd_buf[CS2SX_SWKBD_BUF_SIZE];
const char* CS2SX_Keyboard_Show(
    const char* prompt, const char* initial)
{
    SwkbdConfig cfg;
    swkbdCreate(&cfg, 0);
    swkbdConfigMakePresetDefault(&cfg);
    swkbdConfigSetStringLenMax(&cfg, CS2SX_SWKBD_BUF_SIZE - 1);
    if (prompt  && prompt[0])  swkbdConfigSetGuideText(&cfg, prompt);
    if (initial && initial[0]) swkbdConfigSetInitialText(&cfg, initial);

    _cs2sx_swkbd_buf[0] = '\0';
    swkbdShow(&cfg, _cs2sx_swkbd_buf, sizeof(_cs2sx_swkbd_buf));
    swkbdClose(&cfg);
    return _cs2sx_swkbd_buf;
}

const char* CS2SX_Keyboard_ShowPassword(const char* prompt)
{
    SwkbdConfig cfg;
    swkbdCreate(&cfg, 0);
    swkbdConfigMakePresetPassword(&cfg);
    swkbdConfigSetStringLenMax(&cfg, CS2SX_SWKBD_BUF_SIZE - 1);
    if (prompt && prompt[0]) swkbdConfigSetGuideText(&cfg, prompt);

    _cs2sx_swkbd_buf[0] = '\0';
    swkbdShow(&cfg, _cs2sx_swkbd_buf, sizeof(_cs2sx_swkbd_buf));
    swkbdClose(&cfg);
    return _cs2sx_swkbd_buf;
}

const char* CS2SX_Keyboard_ShowNumber(
    const char* prompt, const char* initial)
{
    SwkbdConfig cfg;
    swkbdCreate(&cfg, 0);
    swkbdConfigMakePresetDefault(&cfg);
    swkbdConfigSetType(&cfg, SwkbdType_NumPad);
    swkbdConfigSetStringLenMax(&cfg, 20);
    if (prompt  && prompt[0])  swkbdConfigSetGuideText(&cfg, prompt);
    if (initial && initial[0]) swkbdConfigSetInitialText(&cfg, initial);

    _cs2sx_swkbd_buf[0] = '\0';
    swkbdShow(&cfg, _cs2sx_swkbd_buf, sizeof(_cs2sx_swkbd_buf));
    swkbdClose(&cfg);
    return _cs2sx_swkbd_buf;
}

