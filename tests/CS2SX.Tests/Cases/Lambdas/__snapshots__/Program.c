#include <stdlib.h>
#include "Program.h"

void LambdasApp_Init(LambdasApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))LambdasApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))LambdasApp_OnFrame;
}

static int _lambda_0(int a, int b)
{
    return a + b;
}

static int _lambda_1(int x)
{
    return x > 100;
}

struct _cap_2
{
    int multiplier;
};
static struct _cap_2* _cs2sx_ctx_2 = NULL;

static int _lambda_2(int x)
{
    struct _cap_2* _c = _cs2sx_ctx_2;
    int multiplier = _c->multiplier;
    return x * multiplier;
}

struct _cap_3
{
    LambdasApp* self;
};
static struct _cap_3* _cs2sx_ctx_3 = NULL;

static void _lambda_3()
{
    struct _cap_3* _c = _cs2sx_ctx_3;
    LambdasApp* self = _c->self;
    self->f_ticks++;
}

static void _lambda_4(int v)
{
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "got %d", v);
    const char* _ps_1 = _sb1;
    printf("%s\n", _ps_1 ? _ps_1 : "");
}

void LambdasApp_OnInit(LambdasApp* self)
{
    Func_int_int_int_t add = _lambda_0;
    Func_int_int_t isBig = _lambda_1;
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "%d %d", add(2, 3), isBig(150));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    int multiplier = 3;
    static struct _cap_2 _ctx_val_2;
    _ctx_val_2.multiplier = multiplier;
    _cs2sx_ctx_2 = &_ctx_val_2;
    Func_int_int_t scale = _lambda_2;
    printf("%d\n", scale(7));
    static struct _cap_3 _ctx_val_3;
    _ctx_val_3.self = self;
    _cs2sx_ctx_3 = &_ctx_val_3;
    self->f_onTick = _lambda_3;
    LambdasApp_Apply(5, _lambda_4);
}

void LambdasApp_Apply(int value, Action_int_t callback)
{
    callback(value);
}

void LambdasApp_OnFrame(LambdasApp* self)
{
    self->f_onTick();
}

void LambdasApp_Free(LambdasApp* self)
{
    if (!self) return;
    free(self);
}
