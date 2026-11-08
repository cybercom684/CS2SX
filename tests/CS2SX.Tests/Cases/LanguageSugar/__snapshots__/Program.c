#include <stdlib.h>
#include "Program.h"

Punkt* Punkt_New(int X, int Y)
{
    Punkt* self = (Punkt*)calloc(1, sizeof(Punkt));
    if (!self) return NULL;
    self->_rc = 1;
    self->f_X = X;
    self->f_Y = Y;
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Punkt_Free);
    return self;
}

void Punkt_Free(Punkt* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    free(self);
}

Punkt* Punkt_Retain(Punkt* self)
{
    if (self) self->_rc++;
    return self;
}

void LanguageSugarApp_Init(LanguageSugarApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))LanguageSugarApp_OnInit;
    self->f_origin = CS2SX_RETAIN(Punkt, Punkt_New(1, 2));
}

int LanguageSugarApp_Apply(Func_int_int_t op, int v)
{
    CS2SX_TRACE("LanguageSugarApp.Apply", "Program.cs");
    CS2SX_LINE(15);
    return Func_int_int_Invoke(op, v);
}

static int _lambda_0(void* _env, int v)
{
    CS2SX_TRACE("LanguageSugarApp.<lambda>", "Program.cs");
    (void)_env;
    return v * 2;
}

static CS2SX_Delegate _dg_0 = CS2SX_DELEGATE_STATIC(_lambda_0);

static void _lambda_1(void* _env, const char* t)
{
    CS2SX_TRACE("LanguageSugarApp.<lambda>", "Program.cs");
    (void)_env;
    char* _sb5 = _cs2sx_next_buf();
    snprintf(_sb5, CS2SX_STRBUF_SIZE, "notify %s", t);
    const char* _ps_11 = _sb5;
    printf("%s\n", _ps_11 ? _ps_11 : "");
}

static CS2SX_Delegate _dg_1 = CS2SX_DELEGATE_STATIC(_lambda_1);

static int _mg_LanguageSugarApp_Sq2(void* _t, int a0)
{
    (void)_t;
    return LanguageSugarApp_Sq2(a0);
}
static CS2SX_Delegate _mg_LanguageSugarApp_Sq2_dg = CS2SX_DELEGATE_STATIC(_mg_LanguageSugarApp_Sq2);
void LanguageSugarApp_OnInit(LanguageSugarApp* self)
{
    CS2SX_TRACE("LanguageSugarApp.OnInit", "Program.cs");
    CS2SX_LINE(24);
    LanguageSugarApp_OnInit__Count(self);
    CS2SX_LINE(24);
    LanguageSugarApp_OnInit__Count(self);
    CS2SX_LINE(25);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "local %d %d %d %d", LanguageSugarApp_OnInit__Sq(self, 4), LanguageSugarApp_OnInit__Fact(self, 5), self->f_calls, LanguageSugarApp_OnInit__SqPlus(self, 3));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(28);
    List_int* _list_1 = List_int_New();
    List_int_Add(_list_1, 3);
    List_int_Add(_list_1, 1);
    List_int_Add(_list_1, 2);
    List_int* list = _list_1;
    CS2SX_LINE(29);
    CS2SX_ARR_LOCAL(int, arr, 3, 7, 8, 9);
    CS2SX_LINE(30);
    HashSet_int* _set_2 = HashSet_int_New();
    HashSet_int_Add(_set_2, 4);
    HashSet_int_Add(_set_2, 5);
    HashSet_int_Add(_set_2, 4);
    HashSet_int* set = _set_2;
    CS2SX_LINE(31);
    List_int* empty = List_int_New();
    CS2SX_LINE(32);
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "collections %d %d %d %d", list->count, CS2SX_AT(arr, 2), set->count, empty->count);
    const char* _ps_3 = _sb1;
    printf("%s\n", _ps_3 ? _ps_3 : "");
    CS2SX_LINE(35);
    Punkt* p = Punkt_New(10, CS2SX_NN(self->f_origin)->f_Y);
    CS2SX_LINE(36);
    Punkt* q = Punkt_New(p->f_X, p->f_Y + 5);
    CS2SX_LINE(37);
    char* _sb2 = _cs2sx_next_buf();
    snprintf(_sb2, CS2SX_STRBUF_SIZE, "with %d,%d %d,%d %d", p->f_X, p->f_Y, q->f_X, q->f_Y, CS2SX_NN(self->f_origin)->f_X);
    const char* _ps_4 = _sb2;
    printf("%s\n", _ps_4 ? _ps_4 : "");
    CS2SX_LINE(40);
    int sum = 0;
    CS2SX_LINE(41);
    for (int i = (2); i < (6); i++)
    {
        CS2SX_LINE(41);
        sum += i;
    }
    CS2SX_LINE(42);
    char* _sb3 = _cs2sx_next_buf();
    snprintf(_sb3, CS2SX_STRBUF_SIZE, "range-loop %d", sum);
    const char* _ps_5 = _sb3;
    printf("%s\n", _ps_5 ? _ps_5 : "");
    CS2SX_LINE(45);
    const char* s = "abcdef";
    CS2SX_LINE(46);
    List_int* _list_6 = List_int_New();
    List_int_Add(_list_6, 10);
    List_int_Add(_list_6, 20);
    List_int_Add(_list_6, 30);
    List_int_Add(_list_6, 40);
    List_int_Add(_list_6, 50);
    List_int* nums = _list_6;
    CS2SX_LINE(47);
    List_int* _rng_7 = List_int_New();
    int _rs_8 = (1);
    if (((3)) - ((1)) < 0 || _rs_8 < 0) CS2SX_ThrowArgOutOfRange("count");
    for (int _ri_9 = 0; _ri_9 < ((3)) - ((1)); _ri_9++)
        List_int_Add(_rng_7, List_int_Get(nums, _rs_8 + _ri_9));
    char* _sb4 = _cs2sx_next_buf();
    snprintf(_sb4, CS2SX_STRBUF_SIZE, "slices %s %s %s %c %d %d %d", String_Substring(s, (1), ((3)) - ((1))), String_Substring(s, strlen(s) - (2), (strlen(s)) - (strlen(s) - (2))), String_Substring(s, 0, ((2)) - (0)), s[strlen(s) - (1)], CS2SX_NN(_rng_7)->count, List_int_Get(nums, nums->count - (1)), CS2SX_AT(arr, 3 - (1)));
    const char* _ps_10 = _sb4;
    printf("%s\n", _ps_10 ? _ps_10 : "");
    CS2SX_LINE(51);
    Func_int_int_t twice = (&_dg_0);
    CS2SX_LINE(52);
    Action_str_t say = (&_dg_1);
    CS2SX_LINE(53);
    Action_str_Invoke(say, "hi");
    CS2SX_LINE(54);
    char* _sb6 = _cs2sx_next_buf();
    snprintf(_sb6, CS2SX_STRBUF_SIZE, "delegate %d %d", LanguageSugarApp_Apply(twice, 21), LanguageSugarApp_Apply((&_mg_LanguageSugarApp_Sq2_dg), 5));
    const char* _ps_12 = _sb6;
    printf("%s\n", _ps_12 ? _ps_12 : "");
}

int LanguageSugarApp_Sq2(int x)
{
    CS2SX_TRACE("LanguageSugarApp.Sq2", "Program.cs");
    CS2SX_LINE(57);
    return x * x;
}

int LanguageSugarApp_OnInit__Sq(LanguageSugarApp* self, int x)
{
    CS2SX_TRACE("LanguageSugarApp.OnInit__Sq", "Program.cs");
    CS2SX_LINE(59);
    return x * x;
}

int LanguageSugarApp_OnInit__Fact(LanguageSugarApp* self, int n)
{
    CS2SX_TRACE("LanguageSugarApp.OnInit__Fact", "Program.cs");
    CS2SX_LINE(61);
    return (n <= 1 ? 1 : n * LanguageSugarApp_OnInit__Fact(self, n - 1));
}

void LanguageSugarApp_OnInit__Count(LanguageSugarApp* self)
{
    CS2SX_TRACE("LanguageSugarApp.OnInit__Count", "Program.cs");
    CS2SX_LINE(63);
    self->f_calls++;
}

int LanguageSugarApp_OnInit__SqPlus(LanguageSugarApp* self, int x)
{
    CS2SX_TRACE("LanguageSugarApp.OnInit__SqPlus", "Program.cs");
    CS2SX_LINE(65);
    return LanguageSugarApp_OnInit__Sq(self, x) + 1;
}

void LanguageSugarApp_Free(LanguageSugarApp* self)
{
    if (!self) return;
    if (self->f_origin) { Punkt_Free(self->f_origin); self->f_origin = NULL; }
    free(self);
}
