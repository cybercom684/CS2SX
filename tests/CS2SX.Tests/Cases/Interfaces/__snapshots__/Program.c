#include <stdlib.h>
#include "_generics.h"
#include "_interfaces.h"
#include "Program.h"

void Bullet_Ctor(Bullet* self)
{
    CS2SX_TRACE("Bullet..ctor", "Program.cs");
}

Bullet* Bullet_New(void)
{
    Bullet* self = (Bullet*)calloc(1, sizeof(Bullet));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Bullet_Dispose;
    self->_gc = Bullet_Gc;
    Bullet_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Bullet_Free);
    return self;
}

void Bullet_Update(Bullet* self, float dt)
{
    CS2SX_TRACE("Bullet.Update", "Program.cs");
    CS2SX_LINE(12);
    self->f_Y -= 100 * dt;
}

int Bullet_IsAlive(Bullet* self)
{
    CS2SX_TRACE("Bullet.IsAlive", "Program.cs");
    CS2SX_LINE(13);
    return self->f_Y > 0;
}

void Bullet_Gc(void* _self, CS2SX_GcVisit visit)
{
    Bullet* self = (Bullet*)_self; (void)self;
}

static void Bullet_GcDyn(void* self, CS2SX_GcVisit visit) { ((Bullet*)self)->_gc(self, visit); }
const CS2SX_GcKind Bullet_GcKind = { offsetof(Bullet, _rc), Bullet_GcDyn, (CS2SX_FreeFn)Bullet_Free };

void Bullet_Dispose(Bullet* self)
{
}

void Bullet_Free(Bullet* self)
{
    if (!self) return;
    Bullet* _o = (Bullet*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Bullet_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Bullet* Bullet_Retain(Bullet* self)
{
    if (self) ((Bullet*)self)->_rc++;
    return self;
}

IUpdatable_vtable Bullet_IUpdatable_vtable_instance =
{
    .Update = (void (*)(void*, float))Bullet_Update,
    .IsAlive = (int (*)(void*))Bullet_IsAlive,
};


void Timer_Ctor(Timer* self)
{
    CS2SX_TRACE("Timer..ctor", "Program.cs");
    self->f_Left = 2;
}

Timer* Timer_New(void)
{
    Timer* self = (Timer*)calloc(1, sizeof(Timer));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Timer_Dispose;
    self->_gc = Timer_Gc;
    Timer_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Timer_Free);
    return self;
}

void Timer_Update(Timer* self, float dt)
{
    CS2SX_TRACE("Timer.Update", "Program.cs");
    CS2SX_LINE(19);
    self->f_Left -= dt;
}

int Timer_IsAlive(Timer* self)
{
    CS2SX_TRACE("Timer.IsAlive", "Program.cs");
    CS2SX_LINE(20);
    return self->f_Left > 0;
}

void Timer_Gc(void* _self, CS2SX_GcVisit visit)
{
    Timer* self = (Timer*)_self; (void)self;
}

static void Timer_GcDyn(void* self, CS2SX_GcVisit visit) { ((Timer*)self)->_gc(self, visit); }
const CS2SX_GcKind Timer_GcKind = { offsetof(Timer, _rc), Timer_GcDyn, (CS2SX_FreeFn)Timer_Free };

void Timer_Dispose(Timer* self)
{
}

void Timer_Free(Timer* self)
{
    if (!self) return;
    Timer* _o = (Timer*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Timer_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Timer* Timer_Retain(Timer* self)
{
    if (self) ((Timer*)self)->_rc++;
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
    self->f_things = List_IUpdatable_Retain(List_IUpdatable_New());
}

void InterfacesApp_OnInit(InterfacesApp* self)
{
    CS2SX_TRACE("InterfacesApp.OnInit", "Program.cs");
    CS2SX_LINE(29);
    Bullet* b = Bullet_New();
    CS2SX_LINE(30);
    b->f_Y = 50;
    CS2SX_LINE(31);
    List_IUpdatable_Add(self->f_things, Bullet_as_IUpdatable(CS2SX_RETAIN(Bullet, b)));
    CS2SX_LINE(32);
    List_IUpdatable_Add(self->f_things, Timer_as_IUpdatable(CS2SX_RETAIN(Timer, Timer_New())));
}

void InterfacesApp_OnFrame(InterfacesApp* self)
{
    CS2SX_TRACE("InterfacesApp.OnFrame", "Program.cs");
    CS2SX_LINE(37);
    int alive = 0;
    CS2SX_LINE(38);
    for (int _i_t = 0; _i_t < (int)(self->f_things->count); _i_t++)
    {
        IUpdatable* t = List_IUpdatable_Get(self->f_things, _i_t);
        CS2SX_LINE(40);
        ({ __auto_type _cs2sx_r = CS2SX_NN(t); _cs2sx_r->vtable->Update(_cs2sx_r->obj, 1.0f / 60.0f); });
        CS2SX_LINE(41);
        if (({ __auto_type _cs2sx_r = CS2SX_NN(t); _cs2sx_r->vtable->IsAlive(_cs2sx_r->obj); }))
        {
            CS2SX_LINE(41);
            alive++;
        }
    }
    CS2SX_LINE(43);
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
