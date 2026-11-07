#include <stdlib.h>
#include "_generics.h"
#include "_interfaces.h"
#include "Program.h"

void Bullet_Ctor(Bullet* self)
{
}

Bullet* Bullet_New(void)
{
    Bullet* self = (Bullet*)calloc(1, sizeof(Bullet));
    if (!self) return NULL;
    self->_rc = 1;
    Bullet_Ctor(self);
    return self;
}

void Bullet_Update(Bullet* self, float dt)
{
    self->f_Y -= 100 * dt;
}

int Bullet_IsAlive(Bullet* self)
{
    return self->f_Y > 0;
}

void Bullet_Free(Bullet* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    free(self);
}

Bullet* Bullet_Retain(Bullet* self)
{
    if (self) self->_rc++;
    return self;
}

IUpdatable_vtable Bullet_IUpdatable_vtable_instance =
{
    .Update = (void (*)(void*, float))Bullet_Update,
    .IsAlive = (int (*)(void*))Bullet_IsAlive,
};


void Timer_Ctor(Timer* self)
{
    self->f_Left = 2;
}

Timer* Timer_New(void)
{
    Timer* self = (Timer*)calloc(1, sizeof(Timer));
    if (!self) return NULL;
    self->_rc = 1;
    Timer_Ctor(self);
    return self;
}

void Timer_Update(Timer* self, float dt)
{
    self->f_Left -= dt;
}

int Timer_IsAlive(Timer* self)
{
    return self->f_Left > 0;
}

void Timer_Free(Timer* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    free(self);
}

Timer* Timer_Retain(Timer* self)
{
    if (self) self->_rc++;
    return self;
}

IUpdatable_vtable Timer_IUpdatable_vtable_instance =
{
    .Update = (void (*)(void*, float))Timer_Update,
    .IsAlive = (int (*)(void*))Timer_IsAlive,
};


void InterfacesApp_Init(InterfacesApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))InterfacesApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))InterfacesApp_OnFrame;
    self->f_things = List_IUpdatable_New();
}

void InterfacesApp_OnInit(InterfacesApp* self)
{
    Bullet* b = Bullet_New();
    b->f_Y = 50;
    List_IUpdatable_Add(self->f_things, Bullet_as_IUpdatable(b));
    List_IUpdatable_Add(self->f_things, Timer_as_IUpdatable(Timer_New()));
}

void InterfacesApp_OnFrame(InterfacesApp* self)
{
    int alive = 0;
    for (int _i_t = 0; _i_t < (int)(self->f_things->count); _i_t++)
    {
        IUpdatable* t = List_IUpdatable_Get(self->f_things, _i_t);
        t->vtable->Update(t->obj, 1.0f / 60.0f);
        if (t->vtable->IsAlive(t->obj))
        {
            alive++;
        }
    }
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "alive: %d", alive);
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
}

void InterfacesApp_Free(InterfacesApp* self)
{
    if (!self) return;
    free(self);
}
