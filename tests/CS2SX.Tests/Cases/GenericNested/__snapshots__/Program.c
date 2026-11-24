#include <stdlib.h>
#include "_generics.h"
#include "Program.h"

// ── Struct: Pair_int ────────────────────────────────────────
Pair_int Pair_int_New(int a)
{
    Pair_int _self_value = {0};
    Pair_int* self = &_self_value;
    CS2SX_LINE(33);
    self->A = a;
    return _self_value;
}

const char* Pair_int_Map_str(Pair_int* self, Func_int_str_t f)
{
    return Func_int_str_Invoke(f, self->A);
}

int Pair_int_Map_int(Pair_int* self, Func_int_int_t f)
{
    return Func_int_int_Invoke(f, self->A);
}

int Pair_int_Equals(Pair_int a, Pair_int b)
{
    return memcmp(&a, &b, sizeof(Pair_int)) == 0;
}

void GenericNestedApp_Init(GenericNestedApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))GenericNestedApp_OnInit;
}

static int _lambda_0(void* _env, int x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    return x * 2;
}

static CS2SX_Delegate _dg_0 = CS2SX_DELEGATE_STATIC(_lambda_0);

static int _lambda_1(void* _env, int x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    return x * 2;
}

static CS2SX_Delegate _dg_1 = CS2SX_DELEGATE_STATIC(_lambda_1);

static int _lambda_2(void* _env, int x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    return x * 2;
}

static CS2SX_Delegate _dg_2 = CS2SX_DELEGATE_STATIC(_lambda_2);

static const char* _lambda_3(void* _env, int x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "n%d", x);
    return _sb0;
}

static CS2SX_Delegate _dg_3 = CS2SX_DELEGATE_STATIC(_lambda_3);

static const char* _lambda_4(void* _env, int x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    char* _sb1 = _cs2sx_next_buf();
    _sb1 = CS2SX_Format(_sb1, "n%d", x);
    return _sb1;
}

static CS2SX_Delegate _dg_4 = CS2SX_DELEGATE_STATIC(_lambda_4);

static const char* _lambda_5(void* _env, int x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    char* _sb2 = _cs2sx_next_buf();
    _sb2 = CS2SX_Format(_sb2, "n%d", x);
    return _sb2;
}

static CS2SX_Delegate _dg_5 = CS2SX_DELEGATE_STATIC(_lambda_5);

static int _lambda_6(void* _env, const char* x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    return CS2SX_Str16Len(CS2SX_NN(x));
}

static CS2SX_Delegate _dg_6 = CS2SX_DELEGATE_STATIC(_lambda_6);

static int _lambda_7(void* _env, const char* x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    return CS2SX_Str16Len(CS2SX_NN(x));
}

static CS2SX_Delegate _dg_7 = CS2SX_DELEGATE_STATIC(_lambda_7);

static int _lambda_8(void* _env, const char* x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    return CS2SX_Str16Len(CS2SX_NN(x));
}

static CS2SX_Delegate _dg_8 = CS2SX_DELEGATE_STATIC(_lambda_8);

static const char* _lambda_9(void* _env, const char* x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    char* _sb4 = _cs2sx_next_buf();
    _sb4 = CS2SX_Format(_sb4, "%s%s", x, x);
    return _sb4;
}

static CS2SX_Delegate _dg_9 = CS2SX_DELEGATE_STATIC(_lambda_9);

static const char* _lambda_10(void* _env, const char* x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    char* _sb5 = _cs2sx_next_buf();
    _sb5 = CS2SX_Format(_sb5, "%s%s", x, x);
    return _sb5;
}

static CS2SX_Delegate _dg_10 = CS2SX_DELEGATE_STATIC(_lambda_10);

static const char* _lambda_11(void* _env, const char* x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    char* _sb6 = _cs2sx_next_buf();
    _sb6 = CS2SX_Format(_sb6, "%s%s", x, x);
    return _sb6;
}

static CS2SX_Delegate _dg_11 = CS2SX_DELEGATE_STATIC(_lambda_11);

static int _lambda_12(void* _env, int a, const char* b)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    char* _sb8 = _cs2sx_next_buf();
    _sb8 = CS2SX_Format(_sb8, "%d", a);
    return CS2SX_strcmp_safe((_sb8), b) == 0;
}

static CS2SX_Delegate _dg_12 = CS2SX_DELEGATE_STATIC(_lambda_12);

static int _lambda_13(void* _env, int a, const char* b)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    char* _sb9 = _cs2sx_next_buf();
    _sb9 = CS2SX_Format(_sb9, "%d", a);
    return CS2SX_strcmp_safe((_sb9), b) == 0;
}

static CS2SX_Delegate _dg_13 = CS2SX_DELEGATE_STATIC(_lambda_13);

static int _lambda_14(void* _env, int a, const char* b)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    char* _sb10 = _cs2sx_next_buf();
    _sb10 = CS2SX_Format(_sb10, "%d", a);
    return CS2SX_strcmp_safe((_sb10), b) == 0;
}

static CS2SX_Delegate _dg_14 = CS2SX_DELEGATE_STATIC(_lambda_14);

static const char* _lambda_15(void* _env, int x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    char* _sb13 = _cs2sx_next_buf();
    _sb13 = CS2SX_Format(_sb13, "p%d", x);
    return _sb13;
}

static CS2SX_Delegate _dg_15 = CS2SX_DELEGATE_STATIC(_lambda_15);

static const char* _lambda_16(void* _env, int x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    char* _sb14 = _cs2sx_next_buf();
    _sb14 = CS2SX_Format(_sb14, "p%d", x);
    return _sb14;
}

static CS2SX_Delegate _dg_16 = CS2SX_DELEGATE_STATIC(_lambda_16);

static const char* _lambda_17(void* _env, int x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    char* _sb15 = _cs2sx_next_buf();
    _sb15 = CS2SX_Format(_sb15, "p%d", x);
    return _sb15;
}

static CS2SX_Delegate _dg_17 = CS2SX_DELEGATE_STATIC(_lambda_17);

static int _lambda_18(void* _env, int x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    return x + 1;
}

static CS2SX_Delegate _dg_18 = CS2SX_DELEGATE_STATIC(_lambda_18);

static int _lambda_19(void* _env, int x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    return x + 1;
}

static CS2SX_Delegate _dg_19 = CS2SX_DELEGATE_STATIC(_lambda_19);

static int _lambda_20(void* _env, int x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    return x + 1;
}

static CS2SX_Delegate _dg_20 = CS2SX_DELEGATE_STATIC(_lambda_20);

void GenericNestedApp_OnInit(GenericNestedApp* self)
{
    CS2SX_TRACE("GenericNestedApp.OnInit", "Program.cs");
    CS2SX_LINE(50);
    Box_int* n = Box_int_New(21);
    CS2SX_LINE(51);
    Box_str* s = Box_str_New("ab");
    CS2SX_LINE(52);
    char* _sb3 = _cs2sx_next_buf();
    _sb3 = CS2SX_Format(_sb3, "%d %s", Box_int_Map_int(n, (&_dg_2)), Box_int_Map_str(n, (&_dg_5)));
    const char* _ps_0 = _sb3;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(53);
    char* _sb7 = _cs2sx_next_buf();
    _sb7 = CS2SX_Format(_sb7, "%d %s", Box_str_Map_int(s, (&_dg_8)), Box_str_Map_str(s, (&_dg_11)));
    const char* _ps_1 = _sb7;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    CS2SX_LINE(54);
    char* _sb11 = _cs2sx_next_buf();
    _sb11 = CS2SX_Format(_sb11, "%s %d", CS2SX_BoolStr(Box_int_Is_str(n, "21", (&_dg_14))), Box_int_Pick_float(n, 1.5f));
    const char* _ps_2 = _sb11;
    printf("%s\n", _ps_2 ? _ps_2 : "");
    CS2SX_LINE(55);
    char* _sb12 = _cs2sx_next_buf();
    _sb12 = CS2SX_Format(_sb12, "%d %s", Box_int_Same(n), Box_str_Same(s));
    const char* _ps_3 = _sb12;
    printf("%s\n", _ps_3 ? _ps_3 : "");
    CS2SX_LINE(56);
    Pair_int pair = Pair_int_New(4);
    CS2SX_LINE(57);
    char* _sb16 = _cs2sx_next_buf();
    _sb16 = CS2SX_Format(_sb16, "%s %d", Pair_int_Map_str(&pair, (&_dg_17)), Pair_int_Map_int(&pair, (&_dg_20)));
    const char* _ps_4 = _sb16;
    printf("%s\n", _ps_4 ? _ps_4 : "");
}

void GenericNestedApp_Free(GenericNestedApp* self)
{
    if (!self) return;
    free(self);
}
