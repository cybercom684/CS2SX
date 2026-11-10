#include <stdlib.h>
#include "Program.h"

void Game_Ctor(Game* self)
{
    CS2SX_TRACE("Game..ctor", "Program.cs");
    self->f_Data = (int*)CS2SX_ArrNew(2, sizeof(int));
}

Game* Game_New(void)
{
    Game* self = (Game*)calloc(1, sizeof(Game));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Game_Dispose;
    self->_gc = Game_Gc;
    Game_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Game_Free);
    return self;
}

int Game_Read(Game* self, int i)
{
    CS2SX_TRACE("Game.Read", "Program.cs");
    CS2SX_LINE(4);
    return CS2SX_AT(self->f_Data, i);
}

void Game_Step(Game* self, int i)
{
    CS2SX_TRACE("Game.Step", "Program.cs");
    CS2SX_LINE(7);
    int v = Game_Read(self, i);
    CS2SX_LINE(8);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "v %d", v);
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
}

void Game_Gc(void* _self, CS2SX_GcVisit visit)
{
    Game* self = (Game*)_self; (void)self;
}

static void Game_GcDyn(void* self, CS2SX_GcVisit visit) { ((Game*)self)->_gc(self, visit); }
const CS2SX_GcKind Game_GcKind = { offsetof(Game, _rc), Game_GcDyn, (CS2SX_FreeFn)Game_Free };

void Game_Dispose(Game* self)
{
    CS2SX_ArrFree(self->f_Data); self->f_Data = NULL;
}

void Game_Free(Game* self)
{
    if (!self) return;
    Game* _o = (Game*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Game_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Game* Game_Retain(Game* self)
{
    if (self) ((Game*)self)->_rc++;
    return self;
}

void StackTracesApp_Init(StackTracesApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))StackTracesApp_OnInit;
}

void StackTracesApp_OnInit(StackTracesApp* self)
{
    CS2SX_TRACE("StackTracesApp.OnInit", "Program.cs");
    CS2SX_LINE(17);
    Game* g = Game_New();
    CS2SX_LINE(18);
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            CS2SX_LINE(20);
            Game_Step(g, 5);
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (1)
            {
                Exception* e = (Exception*)_exf_0_ex;
                CS2SX_LINE(24);
                const char* _ps_1 = CS2SX_Exc_TypeName((Exception*)(e));
                printf("%s\n", _ps_1 ? _ps_1 : "");
                CS2SX_LINE(25);
                const char* _ps_2 = (((Exception*)(e))->f_StackTrace ? ((Exception*)(e))->f_StackTrace : "");
                printf("%s", _ps_2 ? _ps_2 : "");
            }
        }
    }
    CS2SX_LINE(27);
    StackTracesApp_Helper();
    CS2SX_LINE(28);
    printf("after\n");
}

void StackTracesApp_Helper()
{
    CS2SX_TRACE("StackTracesApp.Helper", "Program.cs");
    CS2SX_LINE(33);
    {
        CS2SX_ExFrame _exf_3;
        CS2SX_ExPush(&_exf_3);
        if (setjmp(_exf_3.buf) == 0)
        {
            CS2SX_LINE(35);
            CS2SX_Throw((Exception*)(CS2SX_Exc_New(&CS2SX_TI_InvalidOperationException, "boom", NULL, NULL)));
            CS2SX_ExPop(&_exf_3);
        }
        else
        {
            Exception* _exf_3_ex = _exf_3.ex;
            if (CS2SX_Exc_Is(_exf_3_ex, &CS2SX_TI_InvalidOperationException))
            {
                InvalidOperationException* e = (InvalidOperationException*)_exf_3_ex;
                CS2SX_LINE(39);
                const char* _ps_4 = (((Exception*)(e))->f_StackTrace ? ((Exception*)(e))->f_StackTrace : "");
                printf("%s", _ps_4 ? _ps_4 : "");
            }
            else CS2SX_Throw(_exf_3_ex);
        }
    }
}

void StackTracesApp_Free(StackTracesApp* self)
{
    if (!self) return;
    free(self);
}
