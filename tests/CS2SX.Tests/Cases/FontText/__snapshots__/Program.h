#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct FontTextApp
{
    SwitchApp base;
    Font* f_font;
    int f_frame;
};

void FontTextApp_Init(FontTextApp* self);
void FontTextApp_OnInit(FontTextApp* self);
void FontTextApp_OnFrame(FontTextApp* self);
void FontTextApp_OnExit(FontTextApp* self);

void FontTextApp_Free(FontTextApp* self);


#endif
