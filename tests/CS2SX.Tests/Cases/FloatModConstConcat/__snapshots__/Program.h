#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

extern const char* const P_Root;
extern const char* const P_Sub;
extern const char* const P_Deep;

struct FloatModConstConcatApp
{
    SwitchApp base;
};

extern const char* const FloatModConstConcatApp_Local;
void FloatModConstConcatApp_Init(FloatModConstConcatApp* self);
void FloatModConstConcatApp_OnInit(FloatModConstConcatApp* self);
void FloatModConstConcatApp_OnFrame(FloatModConstConcatApp* self);

void FloatModConstConcatApp_Free(FloatModConstConcatApp* self);


#endif
