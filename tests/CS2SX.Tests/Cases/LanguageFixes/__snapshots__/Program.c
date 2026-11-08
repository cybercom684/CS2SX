#include <stdlib.h>
#include "Program.h"

// ── Struct: Meters ────────────────────────────────────────
int Meters_implicit_int(Meters m)
{
    return m.Value;
}

Meters Meters_explicit_Meters(int v)
{
    return (Meters){ .Value = v };
}

int Meters_Equals(Meters a, Meters b)
{
    return memcmp(&a, &b, sizeof(Meters)) == 0;
}

void Outer_Ctor(Outer* self)
{
}

Outer* Outer_New(void)
{
    Outer* self = (Outer*)calloc(1, sizeof(Outer));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Outer_Dispose;
    Outer_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Outer_Free);
    return self;
}

Inner* Outer_Make(Outer* self)
{
    return Inner_New();
}

void Outer_Dispose(Outer* self)
{
}

void Outer_Free(Outer* self)
{
    if (!self) return;
    Outer* _o = (Outer*)self;
    if (--_o->_rc > 0) return;
    _o->_dtor(_o);
    free(_o);
}

Outer* Outer_Retain(Outer* self)
{
    if (self) ((Outer*)self)->_rc++;
    return self;
}

void Inner_Ctor(Inner* self)
{
    self->f_V = 4;
}

Inner* Inner_New(void)
{
    Inner* self = (Inner*)calloc(1, sizeof(Inner));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Inner_Dispose;
    Inner_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Inner_Free);
    return self;
}

int Inner_Twice(Inner* self)
{
    return self->f_V * 2;
}

void Inner_Dispose(Inner* self)
{
}

void Inner_Free(Inner* self)
{
    if (!self) return;
    Inner* _o = (Inner*)self;
    if (--_o->_rc > 0) return;
    _o->_dtor(_o);
    free(_o);
}

Inner* Inner_Retain(Inner* self)
{
    if (self) ((Inner*)self)->_rc++;
    return self;
}

List_int* Registry_Items;
__attribute__((constructor)) static void Registry_Items_Init(void)
{
    Registry_Items = List_int_New();
}
Dict_str_int* Registry_Names;
__attribute__((constructor)) static void Registry_Names_Init(void)
{
    Registry_Names = Dict_str_int_New();
}
int Registry_Count = 3;
void Registry_Ctor(Registry* self)
{
}

Registry* Registry_New(void)
{
    Registry* self = (Registry*)calloc(1, sizeof(Registry));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Registry_Dispose;
    Registry_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Registry_Free);
    return self;
}

void Registry_Dispose(Registry* self)
{
}

void Registry_Free(Registry* self)
{
    if (!self) return;
    Registry* _o = (Registry*)self;
    if (--_o->_rc > 0) return;
    _o->_dtor(_o);
    free(_o);
}

Registry* Registry_Retain(Registry* self)
{
    if (self) ((Registry*)self)->_rc++;
    return self;
}

void Link_Ctor(Link* self)
{
    self->f_Value = 1;
}

Link* Link_New(void)
{
    Link* self = (Link*)calloc(1, sizeof(Link));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Link_Dispose;
    Link_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Link_Free);
    return self;
}

void Link_Dispose(Link* self)
{
    if (self->f_Next) { Link_Free(self->f_Next); self->f_Next = NULL; }
}

void Link_Free(Link* self)
{
    if (!self) return;
    Link* _o = (Link*)self;
    if (--_o->_rc > 0) return;
    _o->_dtor(_o);
    free(_o);
}

Link* Link_Retain(Link* self)
{
    if (self) ((Link*)self)->_rc++;
    return self;
}

void LanguageFixesApp_Init(LanguageFixesApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))LanguageFixesApp_OnInit;
}

int LanguageFixesApp_AddThree(int v)
{
    return v + 3;
}

_Tuple2_int_str LanguageFixesApp_Pair()
{
    return (_Tuple2_int_str){.item1 = 7, .item2 = "seven"};
}

_Tuple2_int_int LanguageFixesApp_Range(List_int* values)
{
    int lo = List_int_Get(values, 0);
    int hi = List_int_Get(values, 0);
    for (int _i_v = 0; _i_v < (int)(values->count); _i_v++)
    {
        int v = List_int_Get(values, _i_v);
        if (v < lo)
        {
            lo = v;
        }
        if (v > hi)
        {
            hi = v;
        }
    }
    return (_Tuple2_int_int){.item1 = lo, .item2 = hi};
}

void LanguageFixesApp_OnInit(LanguageFixesApp* self)
{
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "static %d", LanguageFixesApp_AddThree(1));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    __auto_type _tup_1 = LanguageFixesApp_Pair();
    int n = _tup_1.item1;
    const char* word = _tup_1.item2;
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "tuple %d %s", n, word);
    const char* _ps_2 = _sb1;
    printf("%s\n", _ps_2 ? _ps_2 : "");
    List_int* _list_3 = List_int_New();
    List_int_Add(_list_3, 5);
    List_int_Add(_list_3, 2);
    List_int_Add(_list_3, 9);
    __auto_type _tup_4 = LanguageFixesApp_Range(_list_3);
    int lo = _tup_4.item1;
    List_int* _list_5 = List_int_New();
    List_int_Add(_list_5, 4);
    List_int_Add(_list_5, 8);
    List_int_Add(_list_5, 6);
    __auto_type _tup_6 = LanguageFixesApp_Range(_list_5);
    self->f_a = _tup_6.item1;
    self->f_b = _tup_6.item2;
    char* _sb2 = _cs2sx_next_buf();
    snprintf(_sb2, CS2SX_STRBUF_SIZE, "range %d %d %d", lo, self->f_a, self->f_b);
    const char* _ps_7 = _sb2;
    printf("%s\n", _ps_7 ? _ps_7 : "");
    List_int_Add(Registry_Items, 10);
    Dict_str_int_Set(Registry_Names, "x", 1);
    char* _sb3 = _cs2sx_next_buf();
    snprintf(_sb3, CS2SX_STRBUF_SIZE, "statics %d %d %d", CS2SX_NN(Registry_Items)->count, CS2SX_NN(Registry_Names)->count, Registry_Count);
    const char* _ps_8 = _sb3;
    printf("%s\n", _ps_8 ? _ps_8 : "");
    Access rights = Access_Read | Access_Exec;
    char* _sb4 = _cs2sx_next_buf();
    snprintf(_sb4, CS2SX_STRBUF_SIZE, "flags %s %s", CS2SX_BoolStr((((rights) & (Access_Exec)) == (Access_Exec))), CS2SX_BoolStr((((rights) & (Access_Write)) == (Access_Write))));
    const char* _ps_9 = _sb4;
    printf("%s\n", _ps_9 ? _ps_9 : "");
    Inner* inner = Inner_New();
    Inner* made = Outer_Make(Outer_New());
    char* _sb5 = _cs2sx_next_buf();
    snprintf(_sb5, CS2SX_STRBUF_SIZE, "nested %d %d", inner->f_V, Inner_Twice((Inner*)CS2SX_NN(made)));
    const char* _ps_10 = _sb5;
    printf("%s\n", _ps_10 ? _ps_10 : "");
    Meters m = (Meters){ .Value = 12 };
    int asInt = Meters_implicit_int(m);
    Meters back = Meters_explicit_Meters(5);
    char* _sb6 = _cs2sx_next_buf();
    snprintf(_sb6, CS2SX_STRBUF_SIZE, "convert %d %d", asInt, back.Value);
    const char* _ps_11 = _sb6;
    printf("%s\n", _ps_11 ? _ps_11 : "");
    Link* head = Link_New();
    int some = ({ __auto_type _nc_12 = (head->f_Next != NULL ? ((Nullable_int){ head->f_Next->f_Value, 1 }) : ((Nullable_int){0})); ((_nc_12).has ? (_nc_12).value : (-1)); });
    Link* _link_13 = Link_New();
    _link_13->f_Value = 9;
    CS2SX_ARC_STORE(Link, head->f_Next, CS2SX_RETAIN(Link, _link_13));
    int other = ({ __auto_type _nc_14 = (head->f_Next != NULL ? ((Nullable_int){ head->f_Next->f_Value, 1 }) : ((Nullable_int){0})); ((_nc_14).has ? (_nc_14).value : (-1)); });
    char* _sb7 = _cs2sx_next_buf();
    snprintf(_sb7, CS2SX_STRBUF_SIZE, "nullcond %d %d", some, other);
    const char* _ps_15 = _sb7;
    printf("%s\n", _ps_15 ? _ps_15 : "");
}

void LanguageFixesApp_Free(LanguageFixesApp* self)
{
    if (!self) return;
    free(self);
}
