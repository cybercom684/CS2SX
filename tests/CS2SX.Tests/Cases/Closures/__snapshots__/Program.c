#include <stdlib.h>
#include "_generics.h"
#include "Program.h"

void ClosuresApp_Init(ClosuresApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))ClosuresApp_OnInit;
}

struct _cap_0
{
    CS2SX_Env _hdr;
    __Ref_int* z;
};

static void _lambda_0(void* _env)
{
    CS2SX_TRACE("ClosuresApp.<lambda>", "Program.cs");
    struct _cap_0* _c = (struct _cap_0*)_env;
    __Ref_int* z = _c->z;
    z->f_V++;
}

static void _envdtor_0(void* _e)
{
    struct _cap_0* _c = (struct _cap_0*)_e;
    CS2SX_ReleaseLater(_c->z, (CS2SX_FreeFn)__Ref_int_Free);
}

struct _cap_1
{
    CS2SX_Env _hdr;
    __Ref_int* seen;
};

static int _lambda_1(void* _env)
{
    CS2SX_TRACE("ClosuresApp.<lambda>", "Program.cs");
    struct _cap_1* _c = (struct _cap_1*)_env;
    __Ref_int* seen = _c->seen;
    return seen->f_V;
}

static void _envdtor_1(void* _e)
{
    struct _cap_1* _c = (struct _cap_1*)_e;
    CS2SX_ReleaseLater(_c->seen, (CS2SX_FreeFn)__Ref_int_Free);
}

struct _cap_2
{
    __Ref_int* total;
};
static struct _cap_2* _cs2sx_ctx_2 = NULL;

static void _lambda_2(int v)
{
    struct _cap_2* _c = _cs2sx_ctx_2;
    __Ref_int* total = _c->total;
    total->f_V += v;
}

struct _cap_3
{
    CS2SX_Env _hdr;
    int k;
};

static int _lambda_3(void* _env)
{
    CS2SX_TRACE("ClosuresApp.<lambda>", "Program.cs");
    struct _cap_3* _c = (struct _cap_3*)_env;
    int k = _c->k;
    return k * 10;
}

void ClosuresApp_OnInit(ClosuresApp* self)
{
    CS2SX_TRACE("ClosuresApp.OnInit", "Program.cs");
    CS2SX_LINE(12);
    __Ref_int* z = __Ref_int_New(0);
    CS2SX_LINE(13);
    struct _cap_0* _env_0 = CS2SX_ENV_NEW(struct _cap_0);
    _env_0->z = CS2SX_RETAIN(__Ref_int, z);
    _env_0->_hdr._dtor = _envdtor_0;
    Action_t inc = CS2SX_Delegate_New((void*)_lambda_0, _env_0, 1);
    CS2SX_LINE(14);
    Action_Invoke(inc);
    CS2SX_LINE(15);
    Action_Invoke(inc);
    CS2SX_LINE(16);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "count %d", z->f_V);
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(18);
    __Ref_int* seen = __Ref_int_New(1);
    CS2SX_LINE(19);
    struct _cap_1* _env_1 = CS2SX_ENV_NEW(struct _cap_1);
    _env_1->seen = CS2SX_RETAIN(__Ref_int, seen);
    _env_1->_hdr._dtor = _envdtor_1;
    Func_int_t read = CS2SX_Delegate_New((void*)_lambda_1, _env_1, 1);
    CS2SX_LINE(20);
    seen->f_V = 5;
    CS2SX_LINE(21);
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "read %d", Func_int_Invoke(read));
    const char* _ps_1 = _sb1;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    CS2SX_LINE(23);
    __Ref_int* total = __Ref_int_New(0);
    CS2SX_LINE(24);
    List_int* _list_2 = List_int_New();
    List_int_Add(_list_2, 1);
    List_int_Add(_list_2, 2);
    List_int_Add(_list_2, 3);
    List_int* list = _list_2;
    CS2SX_LINE(25);
    static struct _cap_2 _ctx_val_2;
    _ctx_val_2.total = total;
    _cs2sx_ctx_2 = &_ctx_val_2;
    for (int _fe_i_3 = 0; _fe_i_3 < list->count; _fe_i_3++)
        _lambda_2(list->data[_fe_i_3]);
    CS2SX_LINE(26);
    char* _sb2 = _cs2sx_next_buf();
    snprintf(_sb2, CS2SX_STRBUF_SIZE, "sum %d", total->f_V);
    const char* _ps_4 = _sb2;
    printf("%s\n", _ps_4 ? _ps_4 : "");
    CS2SX_LINE(28);
    List_Func_int_t* counters = List_Func_int_t_New();
    CS2SX_LINE(29);
    for (int i = 0; i < 3; i++)
    {
        CS2SX_LINE(31);
        int k = i;
        CS2SX_LINE(32);
        struct _cap_3* _env_3 = CS2SX_ENV_NEW(struct _cap_3);
        _env_3->k = k;
        List_Func_int_t_Add(counters, CS2SX_Delegate_New((void*)_lambda_3, _env_3, 1));
    }
    CS2SX_LINE(34);
    char* _sb3 = _cs2sx_next_buf();
    snprintf(_sb3, CS2SX_STRBUF_SIZE, "loop %d %d %d", Func_int_Invoke(List_Func_int_t_Get(counters, 0)), Func_int_Invoke(List_Func_int_t_Get(counters, 1)), Func_int_Invoke(List_Func_int_t_Get(counters, 2)));
    const char* _ps_5 = _sb3;
    printf("%s\n", _ps_5 ? _ps_5 : "");
    CS2SX_LINE(36);
    ClosuresApp_Make(self, 7);
    CS2SX_LINE(37);
    Action_Invoke(self->f_later);
    CS2SX_LINE(38);
    Action_Invoke(self->f_later);
}

struct _cap_4
{
    CS2SX_Env _hdr;
    __Ref_int* start__r;
};

static void _lambda_4(void* _env)
{
    CS2SX_TRACE("ClosuresApp.<lambda>", "Program.cs");
    struct _cap_4* _c = (struct _cap_4*)_env;
    __Ref_int* start__r = _c->start__r;
    CS2SX_LINE(45);
    start__r->f_V++;
    CS2SX_LINE(46);
    char* _sb4 = _cs2sx_next_buf();
    snprintf(_sb4, CS2SX_STRBUF_SIZE, "param %d", start__r->f_V);
    const char* _ps_6 = _sb4;
    printf("%s\n", _ps_6 ? _ps_6 : "");
}

static void _envdtor_4(void* _e)
{
    struct _cap_4* _c = (struct _cap_4*)_e;
    CS2SX_ReleaseLater(_c->start__r, (CS2SX_FreeFn)__Ref_int_Free);
}

void ClosuresApp_Make(ClosuresApp* self, int start)
{
    CS2SX_TRACE("ClosuresApp.Make", "Program.cs");
    CS2SX_LINE(42);
    __Ref_int* start__r = __Ref_int_New(start);
    CS2SX_LINE(43);
    struct _cap_4* _env_4 = CS2SX_ENV_NEW(struct _cap_4);
    _env_4->start__r = CS2SX_RETAIN(__Ref_int, start__r);
    _env_4->_hdr._dtor = _envdtor_4;
    self->f_later = CS2SX_Delegate_New((void*)_lambda_4, _env_4, 1);
}

void ClosuresApp_Free(ClosuresApp* self)
{
    if (!self) return;
    free(self);
}

void __Ref_int_Ctor(__Ref_int* self, int v)
{
    CS2SX_TRACE("__Ref_int..ctor", "Program.cs");
    CS2SX_LINE(51);
    self->f_V = v;
}

__Ref_int* __Ref_int_New(int v)
{
    __Ref_int* self = (__Ref_int*)calloc(1, sizeof(__Ref_int));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))__Ref_int_Dispose;
    __Ref_int_Ctor(self, v);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)__Ref_int_Free);
    return self;
}

void __Ref_int_Dispose(__Ref_int* self)
{
}

void __Ref_int_Free(__Ref_int* self)
{
    if (!self) return;
    __Ref_int* _o = (__Ref_int*)self;
    if (--_o->_rc > 0) return;
    _o->_dtor(_o);
    free(_o);
}

__Ref_int* __Ref_int_Retain(__Ref_int* self)
{
    if (self) ((__Ref_int*)self)->_rc++;
    return self;
}
