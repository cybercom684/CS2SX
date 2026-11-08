#include <stdlib.h>
#include "Program.h"

void Settings_Ctor(Settings* self)
{
}

Settings* Settings_New(void)
{
    Settings* self = (Settings*)calloc(1, sizeof(Settings));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Settings_Dispose;
    Settings_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Settings_Free);
    return self;
}

void Settings_Dispose(Settings* self)
{
}

void Settings_Free(Settings* self)
{
    if (!self) return;
    Settings* _o = (Settings*)self;
    if (--_o->_rc > 0) return;
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
    CS2SX_STR_STORE(self->f_Name, name);
}

Player* Player_New_1(const char* name)
{
    Player* self = (Player*)calloc(1, sizeof(Player));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Player_Dispose;
    Player_Ctor_1(self, name);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Player_Free);
    return self;
}

void Player_Ctor_2(Player* self, const char* name, int level)
{
    Player_Ctor_1(self, name);
    self->f_Level = level;
}

Player* Player_New_2(const char* name, int level)
{
    Player* self = (Player*)calloc(1, sizeof(Player));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Player_Dispose;
    Player_Ctor_2(self, name, level);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Player_Free);
    return self;
}

void Player_Free(Player* self)
{
}

void Player_Dispose(Player* self)
{
    CS2SX_StrDrop(self->f_Name); self->f_Name = NULL;
    if (self->f_Settings) { Settings_Free(self->f_Settings); self->f_Settings = NULL; }
}

void Player_Free(Player* self)
{
    if (!self) return;
    Player* _o = (Player*)self;
    if (--_o->_rc > 0) return;
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
    Player* p = Player_New_1("x");
    const char* _ps_0 = p->f_Name;
    printf("%s\n", _ps_0 ? _ps_0 : "");
}

void Diagnostics_FootgunsApp_Free(Diagnostics_FootgunsApp* self)
{
    if (!self) return;
    free(self);
}
