#pragma once
#ifndef CS2SX_PROGRAM_H
#define CS2SX_PROGRAM_H

#include "_forward.h"

struct Settings
{
    int _rc;
    int f_Volume;
};

Settings* Settings_New(void);
void Settings_Ctor(Settings* self);

void Settings_Free(Settings* self);
Settings* Settings_Retain(Settings* self);
struct Player
{
    int _rc;
    const char* f_Name;
    int f_Level;
    Settings* f_Settings;
};

Player* Player_New_1(const char* name);
void Player_Ctor_1(Player* self, const char* name);
Player* Player_New_2(const char* name, int level);
void Player_Ctor_2(Player* self, const char* name, int level);
void Player_Free(Player* self);

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
