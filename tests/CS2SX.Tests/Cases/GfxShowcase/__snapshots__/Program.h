#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct GfxShowcaseApp
{
    SwitchApp base;
    Texture* f_checker;
    Texture* f_gradient;
    int f_frame;
};

void GfxShowcaseApp_Init(GfxShowcaseApp* self);
void GfxShowcaseApp_OnInit(GfxShowcaseApp* self);
void GfxShowcaseApp_Label(int x, int y, const char* text);
void GfxShowcaseApp_OnFrame(GfxShowcaseApp* self);
void GfxShowcaseApp_OnExit(GfxShowcaseApp* self);

void GfxShowcaseApp_Free(GfxShowcaseApp* self);


#endif
