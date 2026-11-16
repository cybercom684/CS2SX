#include <stdlib.h>
#include "_generics.h"
#include "Program.h"

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
    return strlen(CS2SX_NN(x));
}

static CS2SX_Delegate _dg_6 = CS2SX_DELEGATE_STATIC(_lambda_6);

static int _lambda_7(void* _env, const char* x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    return strlen(CS2SX_NN(x));
}

static CS2SX_Delegate _dg_7 = CS2SX_DELEGATE_STATIC(_lambda_7);

static int _lambda_8(void* _env, const char* x)
{
    CS2SX_TRACE("GenericNestedApp.<lambda>", "Program.cs");
    (void)_env;
    return strlen(CS2SX_NN(x));
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

void GenericNestedApp_OnInit(GenericNestedApp* self)
{
    CS2SX_TRACE("GenericNestedApp.OnInit", "Program.cs");
    CS2SX_LINE(37);
    Box_int* n = Box_int_New(21);
    CS2SX_LINE(38);
    Box_str* s = Box_str_New("ab");
    CS2SX_LINE(39);
    char* _sb3 = _cs2sx_next_buf();
    _sb3 = CS2SX_Format(_sb3, "%d %s", Box_int_Map_int(n, (&_dg_2)), Box_int_Map_str(n, (&_dg_5)));
    const char* _ps_0 = _sb3;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(40);
    char* _sb7 = _cs2sx_next_buf();
    _sb7 = CS2SX_Format(_sb7, "%d %s", Box_str_Map_int(s, (&_dg_8)), Box_str_Map_str(s, (&_dg_11)));
    const char* _ps_1 = _sb7;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    CS2SX_LINE(41);
    char* _sb11 = _cs2sx_next_buf();
    _sb11 = CS2SX_Format(_sb11, "%s %d", CS2SX_BoolStr(Box_int_Is_str(n, "21", (&_dg_14))), Box_int_Pick_float(n, 1.5f));
    const char* _ps_2 = _sb11;
    printf("%s\n", _ps_2 ? _ps_2 : "");
    CS2SX_LINE(42);
    char* _sb12 = _cs2sx_next_buf();
    _sb12 = CS2SX_Format(_sb12, "%d %s", Box_int_Same(n), Box_str_Same(s));
    const char* _ps_3 = _sb12;
    printf("%s\n", _ps_3 ? _ps_3 : "");
}

void GenericNestedApp_Free(GenericNestedApp* self)
{
    if (!self) return;
    free(self);
}
