#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"
#include "_generics.h"

struct MultiFileClassesApp
{
    SwitchApp base;
    List_Shape* f_shapes;
    Painter* f_painter;
    CS2SX_Stopwatch* f_timer;
    CS2SX_Stopwatch* f_frameTimer;
};

void MultiFileClassesApp_Init(MultiFileClassesApp* self);
void MultiFileClassesApp_OnInit(MultiFileClassesApp* self);
void MultiFileClassesApp_OnFrame(MultiFileClassesApp* self);

void MultiFileClassesApp_Free(MultiFileClassesApp* self);


#endif
