#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct Settings
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    int f_Volume;
};

Settings* Settings_New(void);
void Settings_Ctor(Settings* self);

void Settings__Destroy(Settings* self);
extern const CS2SX_ClassInfo Settings_ClassInfo;
void Settings_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Settings_GcKind;
void Settings_Free(Settings* self);
Settings* Settings_Retain(Settings* self);
struct Player
{
    int _rc;
    void (*_dtor)(void* self);   // __Destroy of the dynamic type
    void (*_gc)(void* self, CS2SX_GcVisit visit);   // _Gc of the dynamic type
    const CS2SX_ClassInfo* _class;   // run-time type (x is T)
    const char* f_Name;
    int f_Level;
    Settings* f_Settings;
};

Player* Player_New_1(const char* name);
void Player_Ctor_1(Player* self, const char* name);
Player* Player_New_2(const char* name, int level);
void Player_Ctor_2(Player* self, const char* name, int level);
void Player_Free(Player* self);

void Player__Destroy(Player* self);
extern const CS2SX_ClassInfo Player_ClassInfo;
void Player_Gc(void* self, CS2SX_GcVisit visit);
extern const CS2SX_GcKind Player_GcKind;
void Player_Free(Player* self);
Player* Player_Retain(Player* self);
struct Diagnostics_FootgunsApp
{
    SwitchApp base;
};

void Diagnostics_FootgunsApp_Init(Diagnostics_FootgunsApp* self);
void Diagnostics_FootgunsApp_OnInit(Diagnostics_FootgunsApp* self);

void Diagnostics_FootgunsApp_Free(Diagnostics_FootgunsApp* self);


#endif
