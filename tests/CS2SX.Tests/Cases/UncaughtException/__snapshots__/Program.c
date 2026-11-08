#include <stdlib.h>
#include "Program.h"

void Enemy_Ctor(Enemy* self)
{
    CS2SX_TRACE("Enemy..ctor", "Program.cs");
    self->f_Hp = 3;
}

Enemy* Enemy_New(void)
{
    Enemy* self = (Enemy*)calloc(1, sizeof(Enemy));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Enemy_Dispose;
    Enemy_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Enemy_Free);
    return self;
}

void Enemy_Hit(Enemy* self)
{
    CS2SX_TRACE("Enemy.Hit", "Program.cs");
    CS2SX_LINE(10);
    CS2SX_NN(self->f_Target)->f_Hp--;
}

void Enemy_Dispose(Enemy* self)
{
    if (self->f_Target) { Enemy_Free(self->f_Target); self->f_Target = NULL; }
}

void Enemy_Free(Enemy* self)
{
    if (!self) return;
    Enemy* _o = (Enemy*)self;
    if (--_o->_rc > 0) return;
    _o->_dtor(_o);
    free(_o);
}

Enemy* Enemy_Retain(Enemy* self)
{
    if (self) ((Enemy*)self)->_rc++;
    return self;
}

void UncaughtExceptionApp_Init(UncaughtExceptionApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))UncaughtExceptionApp_OnFrame;
    self->f_enemy = CS2SX_RETAIN(Enemy, Enemy_New());
}

void UncaughtExceptionApp_OnFrame(UncaughtExceptionApp* self)
{
    CS2SX_TRACE("UncaughtExceptionApp.OnFrame", "Program.cs");
    CS2SX_LINE(21);
    self->f_frame++;
    CS2SX_LINE(22);
    if (self->f_frame == 3)
    {
        CS2SX_LINE(23);
        Enemy_Hit(CS2SX_NN(self->f_enemy));
    }
}

void UncaughtExceptionApp_Free(UncaughtExceptionApp* self)
{
    if (!self) return;
    if (self->f_enemy) { Enemy_Free(self->f_enemy); self->f_enemy = NULL; }
    free(self);
}
