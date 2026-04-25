#pragma once
// ============================================================================
// CS2SX switchapp_ext.h  (BEREINIGT)
//
// FIX: Alle Definitionen die bereits in switchapp.h vorhanden sind wurden
// entfernt. Die frühere Version hatte massenhafte Duplikate die zu
// ODR-Verletzungen (One Definition Rule) beim Linken führten, sobald
// beide Header included wurden.
//
// Was ENTFERNT wurde (alles bereits in switchapp.h):
//   - CS2SX_StickPos, CS2SX_TouchState, CS2SX_BatteryInfo (Typen)
//   - CS2SX_STICK_DEADZONE (Makro)
//   - CS2SX_Input_GetStickLeft/Right, CS2SX_StickNorm
//   - CS2SX_Input_GetTouch, CS2SX_Touch_HitRect
//   - CS2SX_GetBattery
//   - Graphics_DrawTriangle, Graphics_FillTriangle
//   - Graphics_DrawEllipse, Graphics_FillEllipse
//   - Graphics_DrawRoundedRect, Graphics_FillRoundedRect
//   - Graphics_SetPixelAlpha, Graphics_FillRectAlpha
//   - Graphics_DrawTextAlpha, Graphics_DrawTextShadow, Graphics_DrawGrid
//   - Graphics_DrawPolygon
//   - CS2SX_Dir_GetDirectories, CS2SX_Dir_GetEntries (Duplikate)
//   - CS2SX_Path_* Helfer (alle in switchapp.h)
//
// Was BLEIBT (nur in dieser Datei, nicht in switchapp.h):
//   - SwitchAppEx Struct + SwitchAppEx_Run (erweiterter App-Loop mit
//     automatischem Stick/Touch/Battery-Update pro Frame)
//   - DrawPolygon (war nur in ext, nicht in switchapp.h)
// ============================================================================

#include "switchapp.h"

// ============================================================================
// Graphics_DrawPolygon — war nur in switchapp_ext.h, nicht in switchapp.h
// ============================================================================

static inline void Graphics_DrawPolygon(int* xs, int* ys, int count, u32 color)
{
    if (!g_fb_addr || count < 2) return;
    for (int i = 0; i < count - 1; i++)
        Graphics_DrawLine(xs[i], ys[i], xs[i + 1], ys[i + 1], color);
    Graphics_DrawLine(xs[count - 1], ys[count - 1], xs[0], ys[0], color);
}

// ============================================================================
// SwitchAppEx — Erweiterter App-Loop
//
// Erbt von SwitchApp, aktualisiert Stick/Touch/Battery automatisch pro Frame.
// Nutzung: public class MyApp : SwitchAppEx { ... }
// In OnFrame sind dann _stickL, _stickR, _touch, _battery direkt verfügbar.
// ============================================================================

typedef struct SwitchAppEx SwitchAppEx;
struct SwitchAppEx
{
    // Muss erstes Feld sein — erlaubt Cast zu SwitchApp*
    SwitchApp base;

    // Pro-Frame automatisch aktualisiert von SwitchAppEx_Run
    CS2SX_StickPos   stickL;
    CS2SX_StickPos   stickR;
    CS2SX_TouchState touch;
    CS2SX_BatteryInfo battery;
    int               batteryTimer; // Akku nur alle ~300 Frames abfragen
};

static inline void SwitchAppEx_Run(SwitchAppEx* self)
{
    if (!self) return;

    PadState pad;
    padConfigureInput(1, HidNpadStyleSet_NpadStandard);
    padInitializeDefault(&pad);

    hidInitializeTouchScreen();

    NWindow* win = nwindowGetDefault();
    framebufferCreate(&g_fb, win,
        (u32)g_fb_width, (u32)g_fb_height,
        PIXEL_FORMAT_RGBA_8888, 2);
    framebufferMakeLinear(&g_fb);

    if (self->base.OnInit)
        self->base.OnInit((SwitchApp*)self);

    bool psmOk = R_SUCCEEDED(psmInitialize());

    int use_gfx = g_gfx_init;
    if (!use_gfx)
    {
        framebufferClose(&g_fb);
        consoleInit(NULL);
        Form_InitFocus(&self->base.form);
    }

    self->batteryTimer = 0;

    while (appletMainLoop())
    {
        padUpdate(&pad);
        g_cs2sx_pad = pad;
        self->base.kDown = padGetButtonsDown(&pad);
        self->base.kHeld = padGetButtons(&pad);

        self->stickL = CS2SX_Input_GetStickLeft(&pad);
        self->stickR = CS2SX_Input_GetStickRight(&pad);
        self->touch = CS2SX_Input_GetTouch();

        // Akkustand nur alle ~300 Frames (ca. 5s bei 60fps) — psmGetBattery ist teuer
        self->batteryTimer++;
        if (self->batteryTimer >= 300 && psmOk)
        {
            self->battery = CS2SX_GetBattery();
            self->batteryTimer = 0;
        }

        if (use_gfx)
        {
            u8* fb_raw = framebufferBegin(&g_fb, NULL);
            if (!fb_raw) continue;
            g_fb_addr = (u32*)fb_raw;

            int total = g_fb_width * g_fb_height;
            for (int i = 0; i < total; i++)
                g_fb_addr[i] = COLOR_BLACK;

            Form_UpdateAll(&self->base.form, self->base.kDown, self->base.kHeld);

            if (self->base.OnFrame)
                self->base.OnFrame((SwitchApp*)self);

            Form_DrawAll(&self->base.form);
            framebufferEnd(&g_fb);
            g_fb_addr = NULL;
        }
        else
        {
            consoleClear();
            printf("\033[H\033[2J");
            Form_UpdateAll(&self->base.form, self->base.kDown, self->base.kHeld);
            if (self->base.OnFrame)
                self->base.OnFrame((SwitchApp*)self);
            Form_DrawAll(&self->base.form);
            consoleUpdate(NULL);
        }

        if (self->base.kDown & HidNpadButton_Plus)
            break;
    }

    if (self->base.OnExit)
        self->base.OnExit((SwitchApp*)self);

    Form_Free(&self->base.form);
    if (psmOk) psmExit();

    if (use_gfx)
        framebufferClose(&g_fb);
    else
        consoleExit(NULL);
}