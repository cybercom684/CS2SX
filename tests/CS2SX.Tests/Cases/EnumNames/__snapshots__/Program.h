#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

#ifndef CS2SX_LIST_Rank_DEFINED
#define CS2SX_LIST_Rank_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(Rank, Rank, CS2SX_ID, CS2SX_NOREF, CS2SX_EQ_MEM)
#endif
#ifndef CS2SX_LIST_Code_DEFINED
#define CS2SX_LIST_Code_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(Code, Code, CS2SX_ID, CS2SX_NOREF, CS2SX_EQ_MEM)
#endif
#ifndef CS2SX_LIST_Access_DEFINED
#define CS2SX_LIST_Access_DEFINED
CS2SX_LIST_DEFINE_HOOK_EQ(Access, Access, CS2SX_ID, CS2SX_NOREF, CS2SX_EQ_MEM)
#endif
struct Player
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    Rank f_Rank;
};

Player* Player_New(void);
void Player_Ctor(Player* self);

void Player__Destroy(Player* self);
extern const CS2SX_ClassInfo Player_ClassInfo;
void Player_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Player_GcKind;
void Player_Free(Player* self);
Player* Player_Retain(Player* self);
struct EnumNamesApp
{
    SwitchApp base;
};

void EnumNamesApp_Init(EnumNamesApp* self);
void EnumNamesApp_OnInit(EnumNamesApp* self);
void EnumNamesApp_OnFrame(EnumNamesApp* self);

void EnumNamesApp_Free(EnumNamesApp* self);


#endif
