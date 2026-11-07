#include <stdlib.h>
#include "Program.h"

void LambdasApp_Init(LambdasApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))LambdasApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))LambdasApp_OnFrame;
}

static int _lambda_0(void* _env, int a, int b)
{
    (void)_env;
    return a + b;
}

static CS2SX_Delegate _dg_0 = CS2SX_DELEGATE_STATIC(_lambda_0);

static int _lambda_1(void* _env, int x)
{
    (void)_env;
    return x > 100;
}

static CS2SX_Delegate _dg_1 = CS2SX_DELEGATE_STATIC(_lambda_1);

struct _cap_2
{
    CS2SX_Env _hdr;
    int multiplier;
};

static int _lambda_2(void* _env, int x)
{
    struct _cap_2* _c = (struct _cap_2*)_env;
    int multiplier = _c->multiplier;
    return x * multiplier;
}

struct _cap_3
{
    CS2SX_Env _hdr;
    LambdasApp* self;
};

static void _lambda_3(void* _env)
{
    struct _cap_3* _c = (struct _cap_3*)_env;
    LambdasApp* self = _c->self;
    self->f_ticks++;
}

static void _lambda_4(void* _env, int v)
{
    (void)_env;
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "got %d", v);
    const char* _ps_1 = _sb1;
    printf("%s\n", _ps_1 ? _ps_1 : "");
}

static CS2SX_Delegate _dg_4 = CS2SX_DELEGATE_STATIC(_lambda_4);

void LambdasApp_OnInit(LambdasApp* self)
{
    Func_int_int_int_t add = (&_dg_0);
    Func_int_int_t isBig = (&_dg_1);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "%d %d", Func_int_int_int_Invoke(add, 2, 3), Func_int_int_Invoke(isBig, 150));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    int multiplier = 3;
    struct _cap_2* _env_2 = CS2SX_ENV_NEW(struct _cap_2);
    _env_2->multiplier = multiplier;
    Func_int_int_t scale = CS2SX_Delegate_New((void*)_lambda_2, _env_2, 1);
    printf("%d\n", Func_int_int_Invoke(scale, 7));
    struct _cap_3* _env_3 = CS2SX_ENV_NEW(struct _cap_3);
    _env_3->self = self;
    self->f_onTick = CS2SX_Delegate_New((void*)_lambda_3, _env_3, 1);
    LambdasApp_Apply(5, (&_dg_4));
}

void LambdasApp_Apply(int value, Action_int_t callback)
{
    Action_int_Invoke(callback, value);
}

void LambdasApp_OnFrame(LambdasApp* self)
{
    Action_Invoke(self->f_onTick);
}

void LambdasApp_Free(LambdasApp* self)
{
    if (!self) return;
    free(self);
}
