#include <stdlib.h>
#include "Program.h"

void Settings_Ctor(Settings* self)
{
    CS2SX_TRACE("Settings..ctor", "Program.cs");
}

Settings* Settings_New(void)
{
    Settings* self = (Settings*)calloc(1, sizeof(Settings));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Settings_Dispose;
    self->_gc = Settings_Gc;
    Settings_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Settings_Free);
    return self;
}

void Settings_Gc(void* _self, CS2SX_GcVisit visit)
{
    Settings* self = (Settings*)_self; (void)self;
}

static void Settings_GcDyn(void* self, CS2SX_GcVisit visit) { ((Settings*)self)->_gc(self, visit); }
const CS2SX_GcKind Settings_GcKind = { offsetof(Settings, _rc), Settings_GcDyn, (CS2SX_FreeFn)Settings_Free };

void Settings_Dispose(Settings* self)
{
}

void Settings_Free(Settings* self)
{
    if (!self) return;
    Settings* _o = (Settings*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Settings_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Settings* Settings_Retain(Settings* self)
{
    if (self) ((Settings*)self)->_rc++;
    return self;
}

void Player_Ctor_1(Player* self, const char* name)
{
    CS2SX_TRACE("Player..ctor", "Program.cs");
    CS2SX_LINE(12);
    CS2SX_STR_STORE(self->f_Name, name);
}

Player* Player_New_1(const char* name)
{
    Player* self = (Player*)calloc(1, sizeof(Player));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Player_Dispose;
    self->_gc = Player_Gc;
    Player_Ctor_1(self, name);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Player_Free);
    return self;
}

void Player_Ctor_2(Player* self, const char* name, int level)
{
    CS2SX_TRACE("Player..ctor", "Program.cs");
    Player_Ctor_1(self, name);
    CS2SX_LINE(13);
    self->f_Level = level;
}

Player* Player_New_2(const char* name, int level)
{
    Player* self = (Player*)calloc(1, sizeof(Player));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Player_Dispose;
    self->_gc = Player_Gc;
    Player_Ctor_2(self, name, level);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Player_Free);
    return self;
}

void Player_Free(Player* self)
{
    CS2SX_TRACE("Player.Free", "Program.cs");
}

void Player_Gc(void* _self, CS2SX_GcVisit visit)
{
    Player* self = (Player*)_self; (void)self;
    if (visit) visit(self->f_Settings, &Settings_GcKind);
    else if (self->f_Settings) { void* _o = self->f_Settings; self->f_Settings = NULL; ((void (*)(void*))Settings_Free)(_o); }
}

static void Player_GcDyn(void* self, CS2SX_GcVisit visit) { ((Player*)self)->_gc(self, visit); }
const CS2SX_GcKind Player_GcKind = { offsetof(Player, _rc), Player_GcDyn, (CS2SX_FreeFn)Player_Free };

void Player_Dispose(Player* self)
{
    CS2SX_StrDrop(self->f_Name); self->f_Name = NULL;
    if (self->f_Settings) { Settings_Free(self->f_Settings); self->f_Settings = NULL; }
}

void Player_Free(Player* self)
{
    if (!self) return;
    Player* _o = (Player*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Player_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Player* Player_Retain(Player* self)
{
    if (self) ((Player*)self)->_rc++;
    return self;
}

void Diagnostics_FootgunsApp_Init(Diagnostics_FootgunsApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))Diagnostics_FootgunsApp_OnInit;
}

void Diagnostics_FootgunsApp_OnInit(Diagnostics_FootgunsApp* self)
{
    CS2SX_TRACE("Diagnostics_FootgunsApp.OnInit", "Program.cs");
    CS2SX_LINE(22);
    Player* p = Player_New_1("x");
    CS2SX_LINE(23);
    const char* _ps_0 = p->f_Name;
    printf("%s\n", _ps_0 ? _ps_0 : "");
}

void Diagnostics_FootgunsApp_Free(Diagnostics_FootgunsApp* self)
{
    if (!self) return;
    free(self);
}
