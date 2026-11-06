#include <stdlib.h>
#include "Program.h"

Settings* Settings_New(void)
{
    Settings* self = (Settings*)malloc(sizeof(Settings));
    if (!self) return NULL;
    memset(self, 0, sizeof(Settings));
    self->_rc = 1;
    return self;
}

void Settings_Free(Settings* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    free(self);
}

Settings* Settings_Retain(Settings* self)
{
    if (self) self->_rc++;
    return self;
}

Player* Player_New(const char* name)
{
    Player* self = (Player*)malloc(sizeof(Player));
    if (!self) return NULL;
    memset(self, 0, sizeof(Player));
    self->_rc = 1;
    self->f_Name = _cs2sx_heap_strdup(name);
    return self;
}

void Player_Free(Player* self)
{
}

void Player_Free(Player* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    if (self->f_Settings) { Settings_Free(self->f_Settings); self->f_Settings = NULL; }
    free(self);
}

Player* Player_Retain(Player* self)
{
    if (self) self->_rc++;
    return self;
}

void Diagnostics_FootgunsApp_Init(Diagnostics_FootgunsApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))Diagnostics_FootgunsApp_OnInit;
}

void Diagnostics_FootgunsApp_OnInit(Diagnostics_FootgunsApp* self)
{
    Player* p = Player_New("x");
    const char* _ps_0 = p->f_Name;
    printf("%s\n", _ps_0 ? _ps_0 : "");
}

void Diagnostics_FootgunsApp_Free(Diagnostics_FootgunsApp* self)
{
    if (!self) return;
    free(self);
}
