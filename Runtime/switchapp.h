#pragma once
#include <switch.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "switchforms.h"

// ============================================================================
// SwitchApp
//
// Basis-Struct für alle Switch-Homebrew-Apps, die mit CS2SX transpiliert wurden.
// Subklassen werden von CSharpToC automatisch als:
//
//   typedef struct MyApp MyApp;
//   struct MyApp { SwitchApp base; ... };
//   void MyApp_Init(MyApp* self);
//   void MyApp_OnInit(MyApp* self);
//   void MyApp_OnFrame(MyApp* self);
//   void MyApp_OnExit(MyApp* self);
//
// generiert. Der Entry-Point (main.c) ruft:
//
//   MyApp app;
//   memset(&app, 0, sizeof(MyApp));
//   MyApp_Init(&app);
//   SwitchApp_Run((SwitchApp*)&app);
//
// ============================================================================

typedef struct SwitchApp SwitchApp;
struct SwitchApp
{
    Form form;
    u64  kDown;   // in diesem Frame neu gedrückte Tasten
    u64  kHeld;   // aktuell gehaltene Tasten

    void (*OnInit) (SwitchApp* self);
    void (*OnFrame)(SwitchApp* self);
    void (*OnExit) (SwitchApp* self);
};

// Fügt einen Control zur internen Form hinzu und setzt dessen Context.
static inline void SwitchApp_Add(SwitchApp* self, Control* control)
{
    if (!self || !control) return;
    control->context = self;
    Form_Add(&self->form, control);
}

// Haupt-Loop der App. Gibt erst zurück, wenn + gedrückt wird oder appletMainLoop() false liefert.
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
        // Nach OnInit Fokus setzen, damit das erste fokussierbare Element aktiv ist
        Form_InitFocus(&self->form);
    }

    while (appletMainLoop())
    {
        padUpdate(&pad);
        self->kDown = padGetButtonsDown(&pad);
        self->kHeld = padGetButtons(&pad);

        consoleClear();
        // Explizit den gesamten Bildschirm leeren (ANSI: cursor home + erase display)
        printf("\033[H\033[2J");

        Form_UpdateAll(&self->form, self->kDown, self->kHeld);

        if (self->OnFrame)
            self->OnFrame(self);

        Form_DrawAll(&self->form);

        consoleUpdate(NULL);

        if (self->kDown & HidNpadButton_Plus)
            break;
    }

    if (self->OnExit)
        self->OnExit(self);

    Form_Free(&self->form);
    consoleExit(NULL);
}