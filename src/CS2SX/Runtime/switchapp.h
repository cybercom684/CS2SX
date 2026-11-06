#pragma once
#include <switch.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <limits.h>
#include <float.h>
#include <math.h>
#include "switchforms.h"
#include "cs2sx_gfx.h"   // 2D graphics on the GPU (deko3d) — src/rt_gfx.c

// Forward-declare the global pad state so stub headers can call padGetStyleSet.
// The actual definition lives in switchforms.c (emitted alongside SwitchApp_Run).
extern PadState g_cs2sx_pad;

#include "AudioStub.h"
#include "VibrationStub.h"
#include "MotionStub.h"
#include "SwkbdStub.h"
#include "SaveDataStub.h"
#include "HttpStub.h"
#include "FtpStub.h"

// ============================================================================
// Farb-Hilfsmakros (RGBA8888)
// ============================================================================

#define CS2SX_RGBA(r,g,b,a) (((u32)(a) << 24) | ((u32)(b) << 16) | ((u32)(g) << 8) | (u32)(r))
#define CS2SX_RGB(r,g,b)    CS2SX_RGBA(r,g,b,255)

#define COLOR_BLACK   CS2SX_RGB(0,   0,   0  )
#define COLOR_WHITE   CS2SX_RGB(255, 255, 255)
#define COLOR_RED     CS2SX_RGB(255, 0,   0  )
#define COLOR_GREEN   CS2SX_RGB(0,   200, 0  )
#define COLOR_BLUE    CS2SX_RGB(0,   0,   255)
#define COLOR_YELLOW  CS2SX_RGB(255, 255, 0  )
#define COLOR_CYAN    CS2SX_RGB(0,   255, 255)
// Fix 9: COLOR_MAGENTA war schon definiert, aber Color.Magenta fehlte im TypeRegistry-Mapping
#define COLOR_MAGENTA CS2SX_RGB(255, 0,   255)
#define COLOR_GRAY    CS2SX_RGB(128, 128, 128)
#define COLOR_DGRAY   CS2SX_RGB(64,  64,  64 )
#define COLOR_LGRAY   CS2SX_RGB(192, 192, 192)
#define COLOR_ORANGE  CS2SX_RGB(255, 165, 0  )

// Fix 9: Fehlende Farben
#define COLOR_PINK    CS2SX_RGB(255, 105, 180)
#define COLOR_PURPLE  CS2SX_RGB(128, 0,   128)
#define COLOR_BROWN   CS2SX_RGB(139, 69,  19 )
#define COLOR_TEAL    CS2SX_RGB(0,   128, 128)
#define COLOR_LIME    CS2SX_RGB(0,   255, 0  )
#define COLOR_NAVY    CS2SX_RGB(0,   0,   128)
#define COLOR_SILVER  CS2SX_RGB(192, 192, 192)
#define COLOR_MAROON  CS2SX_RGB(128, 0,   0  )
#define COLOR_OLIVE   CS2SX_RGB(128, 128, 0  )

// ============================================================================
// Fix 2: Math-Hilfsmakros
// ============================================================================

#ifndef MIN
#define MIN(a, b) ((a) < (b) ? (a) : (b))
#endif

#ifndef MAX
#define MAX(a, b) ((a) > (b) ? (a) : (b))
#endif

#ifndef CLAMP
#define CLAMP(v, lo, hi) ((v) < (lo) ? (lo) : ((v) > (hi) ? (hi) : (v)))
#endif

static inline int CS2SX_Sign(int x) { return (x > 0) - (x < 0); }

// ============================================================================
// String construction helpers (new string(char, count) / new string(char[], start, count))
// ============================================================================

static inline const char* CS2SX_RepeatChar(char c, int count)
{
    char* buf = _cs2sx_next_buf();
    if (count <= 0) { buf[0] = '\0'; return buf; }
    int n = count < CS2SX_STRBUF_SIZE - 1 ? count : CS2SX_STRBUF_SIZE - 1;
    memset(buf, (unsigned char)c, (size_t)n);
    buf[n] = '\0';
    return buf;
}

static inline const char* CS2SX_SubstrFromChars(const char* arr, int start, int count)
{
    char* buf = _cs2sx_next_buf();
    if (!arr || count <= 0) { buf[0] = '\0'; return buf; }
    int n = count < CS2SX_STRBUF_SIZE - 1 ? count : CS2SX_STRBUF_SIZE - 1;
    memcpy(buf, arr + start, (size_t)n);
    buf[n] = '\0';
    return buf;
}

// ============================================================================
// NULL-safe string comparison (CS2SX_strcmp_safe)
// ============================================================================

static inline int CS2SX_strcmp_safe(const char* a, const char* b) {
    if (!a && !b) return 0;
    if (!a) return -1;
    if (!b) return 1;
    return strcmp(a, b);
}

static inline int String_CompareIgnoreCase(const char* a, const char* b) {
    if (!a && !b) return 0;
    if (!a) return -1;
    if (!b) return 1;
    return strcasecmp(a, b);
}

// ============================================================================
// String_IsNullOrWhiteSpace — NULL + leer + nur Whitespace
// ============================================================================

static inline int String_IsNullOrWhiteSpace(const char* s) {
    if (!s) return 1;
    while (*s) {
        if ((unsigned char)*s > 0x20) return 0;
        s++;
    }
    return 1;
}

// ============================================================================
// Fix 3: Pseudo-Zufallszahlengenerator
// ============================================================================

extern unsigned int _cs2sx_rand_state;

static inline void CS2SX_Rand_Seed(unsigned int seed)
{
    _cs2sx_rand_state = seed ? seed : 12345u;
}

static inline int CS2SX_Rand_Next(int min_val, int max_val)
{
    if (max_val <= min_val) return min_val;
    _cs2sx_rand_state = _cs2sx_rand_state * 1664525u + 1013904223u;
    unsigned int r = (_cs2sx_rand_state >> 16) & 0x7FFFu;
    return min_val + (int)(r % (unsigned int)(max_val - min_val));
}

static inline int CS2SX_Rand_NextMax(int max_val)
{
    return CS2SX_Rand_Next(0, max_val);
}

static inline long long CS2SX_Rand_NextInt64(void)
{
    // Combine four 15-bit LCG outputs for a ~60-bit random value
    _cs2sx_rand_state = _cs2sx_rand_state * 1664525u + 1013904223u;
    long long a = (long long)((_cs2sx_rand_state >> 16) & 0x7FFF);
    _cs2sx_rand_state = _cs2sx_rand_state * 1664525u + 1013904223u;
    long long b = (long long)((_cs2sx_rand_state >> 16) & 0x7FFF);
    _cs2sx_rand_state = _cs2sx_rand_state * 1664525u + 1013904223u;
    long long c = (long long)((_cs2sx_rand_state >> 16) & 0x7FFF);
    _cs2sx_rand_state = _cs2sx_rand_state * 1664525u + 1013904223u;
    long long d = (long long)((_cs2sx_rand_state >> 16) & 0x7FFF);
    return (a << 45) | (b << 30) | (c << 15) | d;
}

static inline float CS2SX_Rand_Float(void)
{
    _cs2sx_rand_state = _cs2sx_rand_state * 1664525u + 1013904223u;
    return (float)(_cs2sx_rand_state & 0xFFFFu) / 65535.0f;
}

// ============================================================================
// Fix 15: Environment.Exit
// ============================================================================

static inline void Environment_Exit(int code)
{
    exit(code);
}

#define System_Exit(code) Environment_Exit(code)

// ============================================================================
// Fix 17: Color.WithAlpha
// ============================================================================

static inline u32 Color_WithAlpha(u32 color, u8 alpha)
{
    return (color & 0x00FFFFFFu) | ((u32)alpha << 24);
}

// ============================================================================
// SwitchApp
// ============================================================================

typedef struct SwitchApp SwitchApp;
struct SwitchApp
{
    Form form;
    u64  kDown;
    u64  kHeld;

    void (*OnInit) (SwitchApp* self);
    void (*OnFrame)(SwitchApp* self);
    void (*OnExit) (SwitchApp* self);
};

static inline void SwitchApp_Add(SwitchApp* self, Control* control)
{
    if (!self || !control) return;
    control->context = self;
    Form_Add(&self->form, control);
}

// ============================================================================
// Input / app state (defined in the generated switchforms.c)
// ============================================================================

extern PadState    g_cs2sx_pad;
extern u64         g_cs2sx_kDown;
extern u64         g_cs2sx_kHeld;
extern u64         g_cs2sx_kUp;
extern int         g_cs2sx_want_exit;   // set to break the main loop (clean exit)

// Requests a clean exit of the app (the main loop breaks after the current frame;
// OnExit + cleanup still run). App.Quit() in C#.
static inline void CS2SX_App_Quit(void) { g_cs2sx_want_exit = 1; }

// Queues another homebrew .nro to be launched after this app exits (via the
// hbloader), then requests a clean exit. App.LaunchNro(path) in C#. The path is
// passed as argv[0]. No-op exit only if next-load isn't available (e.g. not run
// through the hbmenu) — then it just quits.
static inline void CS2SX_App_LaunchNro(const char* path)
{
    if (path && envHasNextLoad())
        envSetNextLoad(path, path);
    g_cs2sx_want_exit = 1;
}

// ============================================================================
// Extension Input
// ============================================================================

#define CS2SX_STICK_DEADZONE 3000

typedef struct { int x; int y; } CS2SX_StickPos;
typedef struct { int count; int x[10]; int y[10]; u32 id[10]; } CS2SX_TouchState;

static inline CS2SX_StickPos CS2SX_Input_GetStickLeft(PadState* pad)
{
    HidAnalogStickState raw = padGetStickPos(pad, 0);
    CS2SX_StickPos pos;
    pos.x = (raw.x > -CS2SX_STICK_DEADZONE && raw.x < CS2SX_STICK_DEADZONE) ? 0 : raw.x;
    pos.y = (raw.y > -CS2SX_STICK_DEADZONE && raw.y < CS2SX_STICK_DEADZONE) ? 0 : raw.y;
    return pos;
}

static inline CS2SX_StickPos CS2SX_Input_GetStickRight(PadState* pad)
{
    HidAnalogStickState raw = padGetStickPos(pad, 1);
    CS2SX_StickPos pos;
    pos.x = (raw.x > -CS2SX_STICK_DEADZONE && raw.x < CS2SX_STICK_DEADZONE) ? 0 : raw.x;
    pos.y = (raw.y > -CS2SX_STICK_DEADZONE && raw.y < CS2SX_STICK_DEADZONE) ? 0 : raw.y;
    return pos;
}

static inline CS2SX_StickPos _cs2sx_get_stick_left(void) { return CS2SX_Input_GetStickLeft(&g_cs2sx_pad); }
static inline CS2SX_StickPos _cs2sx_get_stick_right(void) { return CS2SX_Input_GetStickRight(&g_cs2sx_pad); }

static inline int CS2SX_StickNorm(int raw)
{
    if (raw < 0)raw = -raw;
    if (raw < CS2SX_STICK_DEADZONE)return 0;
    int v = ((raw - CS2SX_STICK_DEADZONE) * 100) / (32767 - CS2SX_STICK_DEADZONE);
    return v > 100 ? 100 : v;
}

static inline CS2SX_TouchState CS2SX_Input_GetTouch(void)
{
    CS2SX_TouchState state = {0};
    HidTouchScreenState raw = { 0 };
    if (hidGetTouchScreenStates(&raw, 1) == 0) return state;
    int count = raw.count;if (count > 10)count = 10;state.count = count;
    for (int i = 0;i < count;i++) { state.x[i] = (int)raw.touches[i].x;state.y[i] = (int)raw.touches[i].y;state.id[i] = raw.touches[i].finger_id; }
    return state;
}

static inline int CS2SX_Touch_HitRect(CS2SX_TouchState* ts, int idx, int rx, int ry, int rw, int rh)
{
    if (!ts || idx < 0 || idx >= ts->count) return 0;
    return ts->x[idx] >= rx && ts->x[idx] < rx + rw && ts->y[idx] >= ry && ts->y[idx] < ry + rh;
}

// ============================================================================
// Extension System — Battery
// ============================================================================

typedef struct { int percent; bool charging; bool connected; } CS2SX_BatteryInfo;

static inline CS2SX_BatteryInfo CS2SX_GetBattery(void)
{
    CS2SX_BatteryInfo info = { 0, false, false };
    u32 chargePercent = 0;
    if (R_SUCCEEDED(psmGetBatteryChargePercentage(&chargePercent))) {
        info.percent = (int)chargePercent;
    } else {
        // psmGetBatteryChargePercentage can silently fail on some firmware/libnx versions;
        // psmGetRawBatteryChargePercentage is the more reliable alternative.
        double rawPercent = 0.0;
        if (R_SUCCEEDED(psmGetRawBatteryChargePercentage(&rawPercent))) {
            info.percent = (int)(rawPercent + 0.5);
            chargePercent = (u32)info.percent;
        }
    }
    PsmChargerType chargerType = PsmChargerType_Unconnected;
    if (R_SUCCEEDED(psmGetChargerType(&chargerType))) {
        info.connected = chargerType != PsmChargerType_Unconnected;
        info.charging = info.connected && chargePercent < 100;
    }
    return info;
}

// ============================================================================
// Extension System — Time
// ============================================================================

typedef struct { int hour; int minute; int second; } CS2SX_TimeInfo;

static inline CS2SX_TimeInfo CS2SX_GetTime(void)
{
    CS2SX_TimeInfo t = { 0, 0, 0 };
    time_t now = time(NULL);
    struct tm* lt = localtime(&now);
    if (lt) { t.hour = lt->tm_hour; t.minute = lt->tm_min; t.second = lt->tm_sec; }
    return t;
}

// ============================================================================
// SwitchApp_Run
// ============================================================================

static inline void SwitchApp_Run(SwitchApp* self)
{
    if (!self) return;

    romfsInit();    // no-op if no romfs was linked; enables romfs:/ paths
    psmInitialize(); // battery service — must stay open for the app lifetime

    PadState pad;
    padConfigureInput(1, HidNpadStyleSet_NpadStandard);
    padInitializeDefault(&pad);

    // OnInit first: lets the user call Graphics.Init() to set g_fb_width/g_fb_height
    // before framebufferCreate, so the buffer is allocated at the correct size.
    if (self->OnInit)
        self->OnInit(self);

    // Graphics mode (Graphics.Init called in OnInit): GPU swapchain at the output
    // resolution. If the GPU can't be brought up, fall back to the text console.
    int use_gfx = g_gfx_init && CS2SX_Gfx_Start();

    if (!use_gfx)
    {
        consoleInit(NULL);
        if (g_gfx_init) printf("[cs2sx] GPU initialisation failed - running in text mode\n");
        Form_InitFocus(&self->form);
    }

    while (appletMainLoop())
    {
        cs2sx_frame_begin();
        padUpdate(&pad);
        g_cs2sx_pad  = pad;
        self->kDown  = padGetButtonsDown(&pad);
        self->kHeld  = padGetButtons(&pad);
        g_cs2sx_kDown = self->kDown;
        g_cs2sx_kHeld = self->kHeld;
        g_cs2sx_kUp   = padGetButtonsUp(&pad);

        if (use_gfx)
        {
            CS2SX_Gfx_BeginFrame();          // clears to black
            Form_UpdateAll(&self->form, self->kDown, self->kHeld);
            if (self->OnFrame)
                self->OnFrame(self);
            Form_DrawAll(&self->form);
            CS2SX_Gfx_EndFrame();            // submit + present
        }
        else
        {
            consoleClear();
            printf("\033[H\033[2J");

            Form_UpdateAll(&self->form, self->kDown, self->kHeld);

            if (self->OnFrame)
                self->OnFrame(self);

            Form_DrawAll(&self->form);
            consoleUpdate(NULL);
        }

        if (self->kDown & HidNpadButton_Plus)
            break;
        if (g_cs2sx_want_exit)
            break;
    }

    if (self->OnExit)
        self->OnExit(self);

    Form_Free(&self->form);

    if (use_gfx)
        CS2SX_Gfx_Shutdown();
    else
        consoleExit(NULL);

    psmExit();
    romfsExit();   // paired with romfsInit() at startup — avoids leaking the mount
}
