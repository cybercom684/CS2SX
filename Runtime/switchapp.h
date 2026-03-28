#pragma once
#include <switch.h>
#include <switch/display/framebuffer.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "switchforms.h"

// ============================================================================
// Texture (muss vor den Graphics‑Funktionen definiert sein)
// ============================================================================

typedef struct Texture Texture;
struct Texture {
    int width;
    int height;
    uint32_t* pixels;
};

static inline Texture* Texture_New(int width, int height, uint32_t* pixels) {
    Texture* t = (Texture*)malloc(sizeof(Texture));
    if (!t) return NULL;
    t->width = width;
    t->height = height;
    t->pixels = (uint32_t*)malloc(width * height * sizeof(uint32_t));
    if (pixels) memcpy(t->pixels, pixels, width * height * sizeof(uint32_t));
    else memset(t->pixels, 0, width * height * sizeof(uint32_t));
    return t;
}

static inline void Texture_Dispose(Texture* t) {
    if (t) {
        free(t->pixels);
        free(t);
    }
}

// ============================================================================
// Grafik über libnx Framebuffer (korrigierte API)
// ============================================================================

static Framebuffer g_fb;
static uint32_t* g_fb_addr = NULL;
static uint32_t g_fb_stride;
static int g_fb_width, g_fb_height;

static inline void Graphics_Init(int width, int height) {
    NWindow* win = nwindowGetDefault();
    if (!win) return;
    framebufferCreate(&g_fb, win, width, height, PIXEL_FORMAT_RGBA_8888, 2);
    g_fb_width = width;
    g_fb_height = height;
    g_fb_stride = width; // wird später durch framebufferBegin überschrieben
}

static inline void Graphics_BeginFrame(void) {
    g_fb_addr = framebufferBegin(&g_fb, &g_fb_stride);
    if (!g_fb_addr) return;
    // Optional: Bildschirm löschen (schwarz)
    for (int i = 0; i < g_fb_width * g_fb_height; i++) g_fb_addr[i] = 0;
}

static inline void Graphics_DrawRect(int x, int y, int w, int h, uint32_t color) {
    if (!g_fb_addr) return;
    for (int i = 0; i < h; i++) {
        int row = y + i;
        if (row < 0 || row >= g_fb_height) continue;
        for (int j = 0; j < w; j++) {
            int col = x + j;
            if (col < 0 || col >= g_fb_width) continue;
            g_fb_addr[row * g_fb_stride + col] = color;
        }
    }
}

static inline void Graphics_DrawTexture(Texture* tex, int x, int y) {
    if (!tex || !tex->pixels || !g_fb_addr) return;
    for (int i = 0; i < tex->height; i++) {
        int row = y + i;
        if (row < 0 || row >= g_fb_height) continue;
        for (int j = 0; j < tex->width; j++) {
            int col = x + j;
            if (col < 0 || col >= g_fb_width) continue;
            g_fb_addr[row * g_fb_stride + col] = tex->pixels[i * tex->width + j];
        }
    }
}

static inline void Graphics_EndFrame(void) {
    framebufferEnd(&g_fb);
    g_fb_addr = NULL;
}

// ============================================================================
// SwitchApp (jetzt nach den Grafik-Funktionen)
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

static inline void SwitchApp_Run(SwitchApp* self)
{
    if (!self) return;

    consoleInit(NULL);

    PadState pad;
    padConfigureInput(1, HidNpadStyleSet_NpadStandard);
    padInitializeDefault(&pad);

    if (self->OnInit)
    {
        self->OnInit(self);
        Form_InitFocus(&self->form);
    }

    while (appletMainLoop())
    {
        padUpdate(&pad);
        self->kDown = padGetButtonsDown(&pad);
        self->kHeld = padGetButtons(&pad);

        consoleClear();
        printf("\033[H\033[2J");

        Graphics_BeginFrame();

        Form_UpdateAll(&self->form, self->kDown, self->kHeld);

        if (self->OnFrame)
            self->OnFrame(self);

        Form_DrawAll(&self->form);

        Graphics_EndFrame();

        consoleUpdate(NULL);

        if (self->kDown & HidNpadButton_Plus)
            break;
    }

    if (self->OnExit)
        self->OnExit(self);

    Form_Free(&self->form);
    consoleExit(NULL);
}