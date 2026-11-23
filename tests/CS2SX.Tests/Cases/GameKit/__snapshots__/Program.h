#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct Score
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    int f_points;
    int f_Changes;
};

Score* Score_New(void);
void Score_Ctor(Score* self);

void Score__Destroy(Score* self);
extern const CS2SX_ClassInfo Score_ClassInfo;
void Score_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Score_GcKind;
void Score_Free(Score* self);
Score* Score_Retain(Score* self);
int Score_get_Points(Score* self);
void Score_set_Points(Score* self, int value);
struct GameKitApp
{
    SwitchApp base;
    Texture* f_sheet;
    int f_frame;
};

void GameKitApp_Init(GameKitApp* self);
void GameKitApp_OnInit(GameKitApp* self);
void GameKitApp_OnFrame(GameKitApp* self);

void GameKitApp_Free(GameKitApp* self);


#endif
