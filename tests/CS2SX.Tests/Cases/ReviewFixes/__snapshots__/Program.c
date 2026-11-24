#include <stdlib.h>
#include "_interfaces.h"
#include "Program.h"

void C_Ctor(C* self)
{
    CS2SX_TRACE("C..ctor", "Program.cs");
}

C* C_New(void)
{
    C* self = (C*)calloc(1, sizeof(C));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))C__Destroy;
    self->_gc = C_Gc;
    self->_class = &C_ClassInfo;
    C_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)C_Free);
    return self;
}

const char* C_ToString(C* self)
{
    CS2SX_TRACE("C.ToString", "Program.cs");
    CS2SX_LINE(12);
    return "C!";
}

const char* C_Hello(C* self)
{
    CS2SX_TRACE("C.Hello", "Program.cs");
    CS2SX_LINE(12);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "hi %s", C_get_Name(self));
    return _sb0;
}

const CS2SX_ClassInfo C_ClassInfo = { "C", NULL, (const char* (*)(void*))C_ToString };

void C_Gc(void* _self, CS2SX_GcVisit visit)
{
    C* self = (C*)_self; (void)self;
}

static void C_GcDyn(void* self, CS2SX_GcVisit visit) { ((C*)self)->_gc(self, visit); }
const CS2SX_GcKind C_GcKind = { offsetof(C, _rc), C_GcDyn, (CS2SX_FreeFn)C_Free };

void C__Destroy(C* self)
{
}

void C_Free(C* self)
{
    if (!self) return;
    C* _o = (C*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &C_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

C* C_Retain(C* self)
{
    if (self) ((C*)self)->_rc++;
    return self;
}

const char* C_get_Name(C* self)
{
    return "c";
}

int C_get_N(C* self)
{
    return 1;
}

IB_vtable C_IB_vtable_instance =
{
    .get_N = (int (*)(void*))C_get_N,
};

IA_vtable C_IA_vtable_instance =
{
    .Hello = (const char* (*)(void*))C_Hello,
    .get_Name = (const char* (*)(void*))C_get_Name,
};


void D_Ctor(D* self)
{
    CS2SX_TRACE("D..ctor", "Program.cs");
    C_Ctor((C*)self);
}

D* D_New(void)
{
    D* self = (D*)calloc(1, sizeof(D));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))D__Destroy;
    self->base._gc = D_Gc;
    self->base._class = &D_ClassInfo;
    D_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)D_Free);
    return self;
}

const CS2SX_ClassInfo D_ClassInfo = { "D", &C_ClassInfo, (const char* (*)(void*))C_ToString };

void D_Gc(void* _self, CS2SX_GcVisit visit)
{
    D* self = (D*)_self; (void)self;
    C_Gc(_self, visit);
}

void D__Destroy(D* self)
{
    C__Destroy((C*)self);
}

void D_Free(D* self)
{
    if (!self) return;
    C* _o = (C*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &C_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

D* D_Retain(D* self)
{
    if (self) ((C*)self)->_rc++;
    return self;
}

void Item_Ctor(Item* self, const char* l)
{
    CS2SX_TRACE("Item..ctor", "Program.cs");
    CS2SX_LINE(14);
    CS2SX_STR_STORE(self->f_Label, l);
}

Item* Item_New(const char* l)
{
    Item* self = (Item*)calloc(1, sizeof(Item));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Item__Destroy;
    self->_gc = Item_Gc;
    self->_class = &Item_ClassInfo;
    Item_Ctor(self, l);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Item_Free);
    return self;
}

const CS2SX_ClassInfo Item_ClassInfo = { "Item", NULL, NULL };

void Item_Gc(void* _self, CS2SX_GcVisit visit)
{
    Item* self = (Item*)_self; (void)self;
}

static void Item_GcDyn(void* self, CS2SX_GcVisit visit) { ((Item*)self)->_gc(self, visit); }
const CS2SX_GcKind Item_GcKind = { offsetof(Item, _rc), Item_GcDyn, (CS2SX_FreeFn)Item_Free };

void Item__Destroy(Item* self)
{
    CS2SX_StrDrop(self->f_Label); self->f_Label = NULL;
}

void Item_Free(Item* self)
{
    if (!self) return;
    Item* _o = (Item*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Item_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Item* Item_Retain(Item* self)
{
    if (self) ((Item*)self)->_rc++;
    return self;
}

void Holder_Ctor(Holder* self)
{
    CS2SX_TRACE("Holder..ctor", "Program.cs");
    self->f_Thing = CS2SX_RETAIN(Item, Item_New("none"));
    self->f_Tag = CS2SX_StrOwn("");
}

Holder* Holder_New(void)
{
    Holder* self = (Holder*)calloc(1, sizeof(Holder));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Holder__Destroy;
    self->_gc = Holder_Gc;
    self->_class = &Holder_ClassInfo;
    Holder_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Holder_Free);
    return self;
}

const CS2SX_ClassInfo Holder_ClassInfo = { "Holder", NULL, NULL };

void Holder_Gc(void* _self, CS2SX_GcVisit visit)
{
    Holder* self = (Holder*)_self; (void)self;
    if (visit) visit((void*)self->f_Thing, &Item_GcKind);
    else if (self->f_Thing) { void* _o = (void*)self->f_Thing; self->f_Thing = NULL; ((void (*)(void*))Item_Free)(_o); }
}

static void Holder_GcDyn(void* self, CS2SX_GcVisit visit) { ((Holder*)self)->_gc(self, visit); }
const CS2SX_GcKind Holder_GcKind = { offsetof(Holder, _rc), Holder_GcDyn, (CS2SX_FreeFn)Holder_Free };

void Holder__Destroy(Holder* self)
{
    if (self->f_Thing) { Item_Free((Item*)self->f_Thing); self->f_Thing = NULL; }
    CS2SX_StrDrop(self->f_Tag); self->f_Tag = NULL;
}

void Holder_Free(Holder* self)
{
    if (!self) return;
    Holder* _o = (Holder*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Holder_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Holder* Holder_Retain(Holder* self)
{
    if (self) ((Holder*)self)->_rc++;
    return self;
}

static Item* _cs2sx_Holder_IHolder_get_Thing(void* self) { return ((Holder*)self)->f_Thing; }
static void _cs2sx_Holder_IHolder_set_Thing(void* self, Item* value) { CS2SX_ARC_STORE(Item, ((Holder*)self)->f_Thing, (Item*)Item_Retain((Item*)value)); }

static const char* _cs2sx_Holder_IHolder_get_Tag(void* self) { return ((Holder*)self)->f_Tag; }
static void _cs2sx_Holder_IHolder_set_Tag(void* self, const char* value) { CS2SX_STR_STORE(((Holder*)self)->f_Tag, value); }

IHolder_vtable Holder_IHolder_vtable_instance =
{
    .get_Thing = (Item* (*)(void*))_cs2sx_Holder_IHolder_get_Thing,
    .set_Thing = (void (*)(void*, Item*))_cs2sx_Holder_IHolder_set_Thing,
    .get_Tag = (const char* (*)(void*))_cs2sx_Holder_IHolder_get_Tag,
    .set_Tag = (void (*)(void*, const char*))_cs2sx_Holder_IHolder_set_Tag,
};


// ── Struct: Vec ────────────────────────────────────────
int Vec_get_Twice(Vec* self)
{
    return self->X * 2;
}

int Vec_Equals(Vec a, Vec b)
{
    return memcmp(&a, &b, sizeof(Vec)) == 0;
}

// ── Struct: Pair_int ────────────────────────────────────────
const char* Pair_int_Note(Pair_int* self)
{
    return "line1\nline2";
}

int Pair_int_Equals(Pair_int a, Pair_int b)
{
    return memcmp(&a, &b, sizeof(Pair_int)) == 0;
}

void ReviewFixesApp_Init(ReviewFixesApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))ReviewFixesApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))ReviewFixesApp_OnFrame;
}

int ReviewFixesApp_Dbl(Vec* v)
{
    CS2SX_TRACE("ReviewFixesApp.Dbl", "Program.cs");
    CS2SX_LINE(24);
    return Vec_get_Twice(&(*v));
}

int ReviewFixesApp_Dbl2(Vec v)
{
    CS2SX_TRACE("ReviewFixesApp.Dbl2", "Program.cs");
    CS2SX_LINE(25);
    return Vec_get_Twice(&v);
}

int ReviewFixesApp_IsBig(int x)
{
    CS2SX_TRACE("ReviewFixesApp.IsBig", "Program.cs");
    CS2SX_LINE(26);
    return x > 2;
}

static int _lambda_0(void* _env, int x)
{
    CS2SX_TRACE("ReviewFixesApp.<lambda>", "Program.cs");
    (void)_env;
    return x % 2 == 0;
}

static CS2SX_Delegate _dg_0 = CS2SX_DELEGATE_STATIC(_lambda_0);

static int _lambda_1(void* _env, int x, int y)
{
    CS2SX_TRACE("ReviewFixesApp.<lambda>", "Program.cs");
    (void)_env;
    return y - x;
}

static CS2SX_Delegate _dg_1 = CS2SX_DELEGATE_STATIC(_lambda_1);

static int _mg_ReviewFixesApp_IsBig(void* _t, int a0)
{
    (void)_t;
    return ReviewFixesApp_IsBig(a0);
}
static CS2SX_Delegate _mg_ReviewFixesApp_IsBig_dg = CS2SX_DELEGATE_STATIC(_mg_ReviewFixesApp_IsBig);
#ifndef CS2SX_DELEGATE_Func_int_int_t
#define CS2SX_DELEGATE_Func_int_int_t
typedef CS2SX_Delegate* Func_int_int_t;
static inline int Func_int_int_Invoke(Func_int_int_t d, int a0)
{
    CS2SX_Delegate_CheckNull(d);
    int r = {0};
    for (; d; d = d->next) r = ((int (*)(void*, int))d->fn)(d->target, a0);
    return r;
}
#endif
#ifndef CS2SX_DELEGATE_Func_int_int_t
#define CS2SX_DELEGATE_Func_int_int_t
typedef CS2SX_Delegate* Func_int_int_t;
static inline int Func_int_int_Invoke(Func_int_int_t d, int a0)
{
    CS2SX_Delegate_CheckNull(d);
    int r = {0};
    for (; d; d = d->next) r = ((int (*)(void*, int))d->fn)(d->target, a0);
    return r;
}
#endif
#ifndef CS2SX_DELEGATE_Func_int_int_t
#define CS2SX_DELEGATE_Func_int_int_t
typedef CS2SX_Delegate* Func_int_int_t;
static inline int Func_int_int_Invoke(Func_int_int_t d, int a0)
{
    CS2SX_Delegate_CheckNull(d);
    int r = {0};
    for (; d; d = d->next) r = ((int (*)(void*, int))d->fn)(d->target, a0);
    return r;
}
#endif
static const char* _lambda_2(int x)
{
    char* _sb5 = _cs2sx_next_buf();
    _sb5 = CS2SX_Format(_sb5, "n%d", x);
    return _sb5;
}

void ReviewFixesApp_OnInit(ReviewFixesApp* self)
{
    CS2SX_TRACE("ReviewFixesApp.OnInit", "Program.cs");
    CS2SX_LINE(30);
    IA* a = D_as_IA(D_New());
    CS2SX_LINE(31);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "iface %s %s %s", ({ __auto_type _cs2sx_r = a; _cs2sx_r->vtable->Hello(_cs2sx_r->obj); }), ({ IA* _isv = (a); _isv ? CS2SX_Obj_ToString(_isv->obj) : ""; }), ({ IA* _isv = (a); _isv ? CS2SX_Obj_ToString(_isv->obj) : ""; }));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(32);
    IHolder* h = Holder_as_IHolder(Holder_New());
    CS2SX_LINE(33);
    char* _sb1 = _cs2sx_next_buf();
    _sb1 = CS2SX_Format(_sb1, "x%d", 1);
    IHolder_set_Thing(h, Item_New(_sb1));
    CS2SX_LINE(34);
    char* _sb2 = _cs2sx_next_buf();
    _sb2 = CS2SX_Format(_sb2, "t%d", 2);
    IHolder_set_Tag(h, _sb2);
    CS2SX_LINE(35);
    char* _sb3 = _cs2sx_next_buf();
    _sb3 = CS2SX_Format(_sb3, "set %s %s", CS2SX_NN(IHolder_get_Thing(h))->f_Label, IHolder_get_Tag(h));
    const char* _ps_1 = _sb3;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    CS2SX_LINE(37);
    List_int* _list_2 = List_int_New();
    List_int_Add(_list_2, 1);
    List_int_Add(_list_2, 2);
    List_int_Add(_list_2, 3);
    List_int_Add(_list_2, 4);
    List_int* l = _list_2;
    CS2SX_LINE(38);
    Func_int_int_t even = (&_dg_0);
    CS2SX_LINE(39);
    Func_int_int_int_t desc = (&_dg_1);
    CS2SX_LINE(40);
    __auto_type _cb_3 = desc;
    for (int _si_4 = 1; _si_4 < l->count; _si_4++)
    {
        int _stmp_6 = l->data[_si_4];
        int _sj_5 = _si_4 - 1;
        while (_sj_5 >= 0 && Func_int_int_int_Invoke(_cb_3, l->data[_sj_5], _stmp_6) > 0)
        {
            l->data[_sj_5+1] = l->data[_sj_5];
            _sj_5--;
        }
        l->data[_sj_5+1] = _stmp_6;
    }
    /* sorted */;
    CS2SX_LINE(41);
    __auto_type _cb_7 = even;
    List_int* _fa_9 = List_int_New();
    for (int _li_8 = 0; _li_8 < l->count; _li_8++)
        if (Func_int_int_Invoke(_cb_7, l->data[_li_8])) List_int_Add(_fa_9, l->data[_li_8]);
    __auto_type _cb_10 = even;
    int _ex_12 = 0;
    for (int _li_11 = 0; _li_11 < l->count; _li_11++)
        if (Func_int_int_Invoke(_cb_10, l->data[_li_11])) { _ex_12 = 1; break; }
    __auto_type _cb_13 = (&_mg_ReviewFixesApp_IsBig_dg);
    int _find_15 = 0;
    for (int _li_14 = 0; _li_14 < l->count; _li_14++)
        if (Func_int_int_Invoke(_cb_13, l->data[_li_14])) { _find_15 = l->data[_li_14]; break; }
    __auto_type _cb_16 = (&_mg_ReviewFixesApp_IsBig_dg);
    int _fix_18 = -1;
    for (int _li_17 = 0; _li_17 < l->count; _li_17++)
        if (Func_int_int_Invoke(_cb_16, l->data[_li_17])) { _fix_18 = _li_17; break; }
    __auto_type _cb_19 = even;
    int _tfa_21 = 1;
    for (int _li_20 = 0; _li_20 < l->count; _li_20++)
        if (!Func_int_int_Invoke(_cb_19, l->data[_li_20])) { _tfa_21 = 0; break; }
    char* _sb4 = _cs2sx_next_buf();
    _sb4 = CS2SX_Format(_sb4, "dlg %s %s %s %d %d %s", ({ __auto_type _jl = (_fa_9); int _jn = (_jl ? _jl->count : 0); CS2SX_JOIN_EACH(",", _jn, "%d", (int)(_jl->data[_ji])); }), ({ __auto_type _jl = (l); int _jn = (_jl ? _jl->count : 0); CS2SX_JOIN_EACH(",", _jn, "%d", (int)(_jl->data[_ji])); }), CS2SX_BoolStr(_ex_12), _find_15, _fix_18, CS2SX_BoolStr(_tfa_21));
    const char* _ps_22 = _sb4;
    printf("%s\n", _ps_22 ? _ps_22 : "");
    CS2SX_LINE(43);
    List_int* _src_23 = l;
    List_int* _list_24 = List_int_New();
    if (_src_23) { for (int _cpi = 0; _cpi < _src_23->count; _cpi++) List_int_Add(_list_24, _src_23->data[_cpi]); }
    List_int* copy = _list_24;
    CS2SX_LINE(44);
    __auto_type _cb_25 = (&_mg_ReviewFixesApp_IsBig_dg);
    int _ra_27 = 0;
    for (int _li_26 = copy->count - 1; _li_26 >= 0; _li_26--)
        if (Func_int_int_Invoke(_cb_25, copy->data[_li_26])) { List_int_Remove(copy, _li_26); _ra_27++; }
    List_str* _ca_29 = List_str_New();
    for (int _li_28 = 0; _li_28 < l->count; _li_28++)
        List_str_Add(_ca_29, _lambda_2(l->data[_li_28]));
    char* _sb6 = _cs2sx_next_buf();
    _sb6 = CS2SX_Format(_sb6, "rm %d %s %s", _ra_27, ({ __auto_type _jl = (copy); int _jn = (_jl ? _jl->count : 0); CS2SX_JOIN_EACH(",", _jn, "%d", (int)(_jl->data[_ji])); }), String_Join(",", _ca_29));
    const char* _ps_30 = _sb6;
    printf("%s\n", _ps_30 ? _ps_30 : "");
    CS2SX_LINE(46);
    List_str* _list_31 = List_str_New();
    List_str_Add(_list_31, "a");
    List_str_Add(_list_31, "b");
    List_str* names = _list_31;
    CS2SX_LINE(47);
    const char** _toa_32 = (const char**)CS2SX_ArrNew(names->count, sizeof(const char*));
    for (int _i_33 = 0; _i_33 < names->count; _i_33++)
      _toa_32[_i_33] = _cs2sx_heap_strdup(List_str_Get(names, _i_33));
    const char** arr = _toa_32;
    CS2SX_LINE(48);
    List_str_Clear(names);
    CS2SX_LINE(49);
    List_str_Add(names, "z");
    CS2SX_LINE(50);
    char* _sb7 = _cs2sx_next_buf();
    _sb7 = CS2SX_Format(_sb7, "arr %d%s%s %s", CS2SX_ArrLen(CS2SX_NN(arr)), CS2SX_AT(arr, 0), CS2SX_AT(arr, 1), List_str_Get(names, 0));
    const char* _ps_34 = _sb7;
    printf("%s\n", _ps_34 ? _ps_34 : "");
    CS2SX_LINE(52);
    Vec v = (Vec){ .X = 4 };
    CS2SX_LINE(53);
    char* _sb8 = _cs2sx_next_buf();
    _sb8 = CS2SX_Format(_sb8, "vec %d %d", ReviewFixesApp_Dbl(&v), ReviewFixesApp_Dbl2(v));
    const char* _ps_35 = _sb8;
    printf("%s\n", _ps_35 ? _ps_35 : "");
    CS2SX_LINE(54);
    int big = INT_MAX;
    CS2SX_LINE(55);
    {
        CS2SX_ExFrame _exf_36;
        CS2SX_ExPush(&_exf_36);
        if (setjmp(_exf_36.buf) == 0)
        {
            CS2SX_LINE(55);
            {
                CS2SX_LINE(55);
                ({ int _ckp = big; big = CS2SX_CHECKED(add, int, _ckp, 1); _ckp; });
            }
            CS2SX_LINE(55);
            printf("no\n");
            CS2SX_ExPop(&_exf_36);
        }
        else
        {
            Exception* _exf_36_ex = _exf_36.ex;
            if (CS2SX_Exc_Is(_exf_36_ex, &CS2SX_TI_OverflowException))
            {
                CS2SX_LINE(55);
                printf("ovf\n");
                Exception_Free(_exf_36_ex);
            }
            else CS2SX_Throw(_exf_36_ex);
        }
    }
    CS2SX_LINE(56);
    int small = INT_MIN;
    CS2SX_LINE(57);
    {
        CS2SX_ExFrame _exf_37;
        CS2SX_ExPush(&_exf_37);
        if (setjmp(_exf_37.buf) == 0)
        {
            CS2SX_LINE(57);
            {
                CS2SX_LINE(57);
                (small = CS2SX_CHECKED(sub, int, small, 1));
            }
            CS2SX_LINE(57);
            printf("no\n");
            CS2SX_ExPop(&_exf_37);
        }
        else
        {
            Exception* _exf_37_ex = _exf_37.ex;
            if (CS2SX_Exc_Is(_exf_37_ex, &CS2SX_TI_OverflowException))
            {
                CS2SX_LINE(57);
                printf("ovf2\n");
                Exception_Free(_exf_37_ex);
            }
            else CS2SX_Throw(_exf_37_ex);
        }
    }
    CS2SX_LINE(58);
    char* _sb9 = _cs2sx_next_buf();
    snprintf(_sb9, CS2SX_STRBUF_SIZE, "%d", 5);
    char* _sb10 = _cs2sx_next_buf();
    _sb10 = CS2SX_Format(_sb10, "conv %s %s", _sb9, CS2SX_ToBase((long long)(((signed char)-1)), 16, 16));
    const char* _ps_38 = _sb10;
    printf("%s\n", _ps_38 ? _ps_38 : "");
    CS2SX_LINE(59);
    char* _sb11 = _cs2sx_next_buf();
    _sb11 = CS2SX_Format(_sb11, "note %d", CS2SX_ArrLen(CS2SX_NN(CS2SX_StrArr_FromList(String_Split(CS2SX_NN(({ Pair_int _sv_39 = (Pair_int){ .A = 1 }; Pair_int_Note(&_sv_39); })), "\n")))));
    const char* _ps_40 = _sb11;
    printf("%s\n", _ps_40 ? _ps_40 : "");
    CS2SX_LINE(61);
    StringBuilder* sb = StringBuilder_AppendStr(StringBuilder_New(256), "äbc");
    CS2SX_LINE(62);
    StringBuilder_Replace(StringBuilder_Insert(sb, 1, (const char*)CS2SX_Format(_cs2sx_next_buf(), "%d", 5)), CS2SX_CharStr('b'), CS2SX_CharStr('B'));
    CS2SX_LINE(64);
    char* _sb12 = _cs2sx_next_buf();
    _sb12 = CS2SX_Format(_sb12, "sb %s %d %d", CS2SX_BoolStr((CS2SX_strcmp_safe(StringBuilder_ToString(sb), "ä5Bc") == 0)), ({ StringBuilder* _cs2sx_sbl = CS2SX_NN(sb); _cs2sx_sbl->buf ? CS2SX_Str16Len(_cs2sx_sbl->buf) : 0; }), String_IndexOf(CS2SX_NN(StringBuilder_ToString(sb)), "c"));
    const char* _ps_41 = _sb12;
    printf("%s\n", _ps_41 ? _ps_41 : "");
    CS2SX_LINE(65);
    char* _sb13 = _cs2sx_next_buf();
    _sb13 = CS2SX_Format(_sb13, "dt %s %d %s", CS2SX_DT_ToString(CS2SX_DT_New(2024, 1, 1, 10, 0, 30, 0), "ss.FFF"), CS2SX_DT_Part(CS2SX_DT_Add(CS2SX_DT_New(2024, 1, 1, 0, 0, 0, 0), 0.5, CS2SX_TICKS_PER_DAY), CS2SX_DT_HOUR), CS2SX_DT_ToString(CS2SX_DT_Add(CS2SX_DT_New(2024, 1, 1, 0, 0, 0, 0), 1.5, CS2SX_TICKS_PER_MS), "fffffff"));
    const char* _ps_42 = _sb13;
    printf("%s\n", _ps_42 ? _ps_42 : "");
    CS2SX_LINE(67);
    printf("done\n");
}

void ReviewFixesApp_OnFrame(ReviewFixesApp* self)
{
    CS2SX_TRACE("ReviewFixesApp.OnFrame", "Program.cs");
}

void ReviewFixesApp_Free(ReviewFixesApp* self)
{
    if (!self) return;
    free(self);
}
