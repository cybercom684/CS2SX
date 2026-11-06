#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct HelloWorldApp
{
    SwitchApp base;
    int f_frames;
};

void HelloWorldApp_Init(HelloWorldApp* self);
void HelloWorldApp_OnInit(HelloWorldApp* self);
void HelloWorldApp_OnFrame(HelloWorldApp* self);

void HelloWorldApp_Free(HelloWorldApp* self);


#endif
