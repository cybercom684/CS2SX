#include <stdlib.h>
#include "_generics.h"
#include "Program.h"

void CS2SX_Yield_Begin(int points)
{
    CS2SX_TRACE("CS2SX_Yield.Begin", "Program.cs");
}

void CS2SX_Yield_Return(int point)
{
    CS2SX_TRACE("CS2SX_Yield.Return", "Program.cs");
}

static CS2SX_Iter_vtable CS2SX_Iter_vtable_instance =
{
};

void CS2SX_Iter_Ctor(CS2SX_Iter* self)
{
    CS2SX_TRACE("CS2SX_Iter..ctor", "Program.cs");
}

CS2SX_Iter* CS2SX_Iter_New(void)
{
    CS2SX_Iter* self = (CS2SX_Iter*)calloc(1, sizeof(CS2SX_Iter));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))CS2SX_Iter__Destroy;
    self->_gc = CS2SX_Iter_Gc;
    self->_class = &CS2SX_Iter_ClassInfo;
    self->vtable = &CS2SX_Iter_vtable_instance;
    CS2SX_Iter_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)CS2SX_Iter_Free);
    return self;
}

void CS2SX_Iter_Reset(CS2SX_Iter* self)
{
    CS2SX_TRACE("CS2SX_Iter.Reset", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Throw((Exception*)(CS2SX_Exc_New(&CS2SX_TI_InvalidOperationException, "Reset wird nicht unterstützt — die Methode neu aufrufen.", NULL, NULL)));
}

void CS2SX_Iter_Dispose(CS2SX_Iter* self)
{
    CS2SX_TRACE("CS2SX_Iter.Dispose", "Program.cs");
}

const CS2SX_ClassInfo CS2SX_Iter_ClassInfo = { "CS2SX_Iter", NULL, NULL };

void CS2SX_Iter_Gc(void* _self, CS2SX_GcVisit visit)
{
    CS2SX_Iter* self = (CS2SX_Iter*)_self; (void)self;
    if (visit) visit((void*)self->f_Current, &CS2SX_Obj_GcKind);
    else if (self->f_Current) { void* _o = (void*)self->f_Current; self->f_Current = NULL; ((void (*)(void*))CS2SX_Obj_Release)(_o); }
}

static void CS2SX_Iter_GcDyn(void* self, CS2SX_GcVisit visit) { ((CS2SX_Iter*)self)->_gc(self, visit); }
const CS2SX_GcKind CS2SX_Iter_GcKind = { offsetof(CS2SX_Iter, _rc), CS2SX_Iter_GcDyn, (CS2SX_FreeFn)CS2SX_Iter_Free };

void CS2SX_Iter__Destroy(CS2SX_Iter* self)
{
    if (self->f_Current) { CS2SX_Obj_Release(self->f_Current); self->f_Current = NULL; }
}

void CS2SX_Iter_Free(CS2SX_Iter* self)
{
    if (!self) return;
    CS2SX_Iter* _o = (CS2SX_Iter*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Iter_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

CS2SX_Iter* CS2SX_Iter_Retain(CS2SX_Iter* self)
{
    if (self) ((CS2SX_Iter*)self)->_rc++;
    return self;
}

static CS2SX_Iter_int_vtable CS2SX_Iter_int_vtable_instance =
{
};

void CS2SX_Iter_int_Ctor(CS2SX_Iter_int* self)
{
    CS2SX_TRACE("CS2SX_Iter_int..ctor", "Program.cs");
}

CS2SX_Iter_int* CS2SX_Iter_int_New(void)
{
    CS2SX_Iter_int* self = (CS2SX_Iter_int*)calloc(1, sizeof(CS2SX_Iter_int));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))CS2SX_Iter_int__Destroy;
    self->_gc = CS2SX_Iter_int_Gc;
    self->_class = &CS2SX_Iter_int_ClassInfo;
    self->vtable = &CS2SX_Iter_int_vtable_instance;
    CS2SX_Iter_int_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)CS2SX_Iter_int_Free);
    return self;
}

void CS2SX_Iter_int_Reset(CS2SX_Iter_int* self)
{
    CS2SX_TRACE("CS2SX_Iter_int.Reset", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Throw((Exception*)(CS2SX_Exc_New(&CS2SX_TI_InvalidOperationException, "Reset wird nicht unterstützt — die Methode neu aufrufen.", NULL, NULL)));
}

void CS2SX_Iter_int_Dispose(CS2SX_Iter_int* self)
{
    CS2SX_TRACE("CS2SX_Iter_int.Dispose", "Program.cs");
}

const CS2SX_ClassInfo CS2SX_Iter_int_ClassInfo = { "CS2SX_Iter_int", NULL, NULL };

void CS2SX_Iter_int_Gc(void* _self, CS2SX_GcVisit visit)
{
    CS2SX_Iter_int* self = (CS2SX_Iter_int*)_self; (void)self;
}

static void CS2SX_Iter_int_GcDyn(void* self, CS2SX_GcVisit visit) { ((CS2SX_Iter_int*)self)->_gc(self, visit); }
const CS2SX_GcKind CS2SX_Iter_int_GcKind = { offsetof(CS2SX_Iter_int, _rc), CS2SX_Iter_int_GcDyn, (CS2SX_FreeFn)CS2SX_Iter_int_Free };

void CS2SX_Iter_int__Destroy(CS2SX_Iter_int* self)
{
}

void CS2SX_Iter_int_Free(CS2SX_Iter_int* self)
{
    if (!self) return;
    CS2SX_Iter_int* _o = (CS2SX_Iter_int*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Iter_int_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

CS2SX_Iter_int* CS2SX_Iter_int_Retain(CS2SX_Iter_int* self)
{
    if (self) ((CS2SX_Iter_int*)self)->_rc++;
    return self;
}

static const char* Kind__boxstr(void* b) { return Kind_ToString(((CS2SX_Box*)b)->v.i); }
const CS2SX_ClassInfo Kind_BoxInfo = { "Kind", NULL, Kind__boxstr };
int Pair_Equals(Pair* a, Pair* b)
{
    if (a == b) return 1;
    if (!a || !b) return 0;
    if (!(a->f_X == b->f_X)) return 0;
    if (!(a->f_Y == b->f_Y)) return 0;
    return 1;
}

const char* Pair_ToString(Pair* self)
{
    if (!self) return "";
    char* buf = _cs2sx_next_buf();
    return CS2SX_Format(buf, "Pair { X = %d, Y = %d }", self->f_X, self->f_Y);
}

Pair* Pair_New(int X, int Y)
{
    Pair* self = (Pair*)calloc(1, sizeof(Pair));
    if (!self) return NULL;
    self->_rc = 1;
    self->f_X = X;
    self->f_Y = Y;
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Pair_Free);
    return self;
}

void Pair_Free(Pair* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    free(self);
}

Pair* Pair_Retain(Pair* self)
{
    if (self) self->_rc++;
    return self;
}

const CS2SX_GcKind Pair_GcKind = { offsetof(Pair, _rc), CS2SX_GcLeaf, (CS2SX_FreeFn)Pair_Free };
const CS2SX_ClassInfo Pair_ClassInfo = { "Pair", NULL, (const char* (*)(void*))Pair_ToString };

int Hero_Equals(Hero* a, Hero* b)
{
    if (a == b) return 1;
    if (!a || !b) return 0;
    if (!(CS2SX_strcmp_safe(a->f_Name, b->f_Name) == 0)) return 0;
    if (!(a->f_Level == b->f_Level)) return 0;
    if (!(a->f_Element == b->f_Element)) return 0;
    if (!(Pair_Equals(a->f_Pos, b->f_Pos))) return 0;
    if (!(a->f_Ready == b->f_Ready)) return 0;
    return 1;
}

const char* Hero_ToString(Hero* self)
{
    if (!self) return "";
    char* buf = _cs2sx_next_buf();
    return CS2SX_Format(buf, "Hero { Name = %s, Level = %d, Element = %s, Pos = %s, Ready = %s }", (self->f_Name ? self->f_Name : ""), self->f_Level, Kind_ToString(self->f_Element), (self->f_Pos ? Pair_ToString(self->f_Pos) : ""), CS2SX_BoolStr(self->f_Ready));
}

Hero* Hero_New(const char* Name, int Level, Kind Element, Pair* Pos)
{
    Hero* self = (Hero*)calloc(1, sizeof(Hero));
    if (!self) return NULL;
    self->_rc = 1;
    CS2SX_STR_STORE(self->f_Name, Name);
    self->f_Level = Level;
    self->f_Element = Element;
    self->f_Pos = CS2SX_RETAIN(Pair, Pos);
    self->f_Ready = 1;
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Hero_Free);
    return self;
}

void Hero_Free(Hero* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    CS2SX_StrDrop(self->f_Name); self->f_Name = NULL;
    if (self->f_Pos) { Pair_Free(self->f_Pos); self->f_Pos = NULL; }
    free(self);
}

Hero* Hero_Retain(Hero* self)
{
    if (self) self->_rc++;
    return self;
}

const CS2SX_GcKind Hero_GcKind = { offsetof(Hero, _rc), CS2SX_GcLeaf, (CS2SX_FreeFn)Hero_Free };
const CS2SX_ClassInfo Hero_ClassInfo = { "Hero", NULL, (const char* (*)(void*))Hero_ToString };

void Deck_Ctor(Deck* self)
{
    CS2SX_TRACE("Deck..ctor", "Program.cs");
    List_int* _list_0 = List_int_New();
    List_int_Add(_list_0, 3);
    List_int_Add(_list_0, 1);
    List_int_Add(_list_0, 4);
    self->f_cards = List_int_Retain(_list_0);
}

Deck* Deck_New(void)
{
    Deck* self = (Deck*)calloc(1, sizeof(Deck));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Deck__Destroy;
    self->_gc = Deck_Gc;
    self->_class = &Deck_ClassInfo;
    Deck_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Deck_Free);
    return self;
}

CS2SX_Iter_int* Deck_GetEnumerator(Deck* self)
{
    CS2SX_TRACE("Deck.GetEnumerator", "Program.cs");
    CS2SX_LINE(19);
    GetEnumerator_Iter* cs2sx_sm = GetEnumerator_Iter_New();
    CS2SX_LINE(19);
    CS2SX_ARC_STORE(Deck, cs2sx_sm->f_cs2sx_this, CS2SX_RETAIN(Deck, self));
    CS2SX_LINE(19);
    return ((CS2SX_Iter_int*)cs2sx_sm);
}

const CS2SX_ClassInfo Deck_ClassInfo = { "Deck", NULL, NULL };

void Deck_Gc(void* _self, CS2SX_GcVisit visit)
{
    Deck* self = (Deck*)_self; (void)self;
}

static void Deck_GcDyn(void* self, CS2SX_GcVisit visit) { ((Deck*)self)->_gc(self, visit); }
const CS2SX_GcKind Deck_GcKind = { offsetof(Deck, _rc), Deck_GcDyn, (CS2SX_FreeFn)Deck_Free };

void Deck__Destroy(Deck* self)
{
    if (self->f_cards) { List_int_Free(self->f_cards); self->f_cards = NULL; }
}

void Deck_Free(Deck* self)
{
    if (!self) return;
    Deck* _o = (Deck*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Deck_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Deck* Deck_Retain(Deck* self)
{
    if (self) ((Deck*)self)->_rc++;
    return self;
}

static CS2SX_Iter_int_vtable GetEnumerator_Iter_vtable_instance;

void GetEnumerator_Iter_Ctor(GetEnumerator_Iter* self)
{
    CS2SX_TRACE("GetEnumerator_Iter..ctor", "Program.cs");
    self->f_cs2sx_c1 = List_int_Retain(List_int_New());
    CS2SX_Iter_int_Ctor((CS2SX_Iter_int*)self);
}

GetEnumerator_Iter* GetEnumerator_Iter_New(void)
{
    GetEnumerator_Iter* self = (GetEnumerator_Iter*)calloc(1, sizeof(GetEnumerator_Iter));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))GetEnumerator_Iter__Destroy;
    self->base._gc = GetEnumerator_Iter_Gc;
    self->base._class = &GetEnumerator_Iter_ClassInfo;
    self->base.vtable = &GetEnumerator_Iter_vtable_instance;
    GetEnumerator_Iter_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)GetEnumerator_Iter_Free);
    return self;
}

int GetEnumerator_Iter_MoveNext(GetEnumerator_Iter* self)
{
    CS2SX_TRACE("GetEnumerator_Iter.MoveNext", "Program.cs");
    CS2SX_LINE(19);
    if (self->f_cs2sx_state < 0)
    {
        CS2SX_LINE(19);
        return 0;
    }
    CS2SX_LINE(19);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; default: break; };
    {
        CS2SX_LINE(19);
        CS2SX_COLL_STORE(List_int, self->f_cs2sx_c1, CS2SX_NN(self->f_cs2sx_this)->f_cards);
        CS2SX_LINE(19);
        for (self->f_cs2sx_i1 = 0; self->f_cs2sx_i1 < CS2SX_NN(self->f_cs2sx_c1)->count; self->f_cs2sx_i1++)
        {
            CS2SX_LINE(19);
            self->f_c = List_int_Get(self->f_cs2sx_c1, self->f_cs2sx_i1);
            {
                CS2SX_LINE(19);
                self->base.f_Current = self->f_c;
                CS2SX_LINE(19);
                self->f_cs2sx_state = 1; return true; _cs2sx_r1_0: self->f_cs2sx_state = 0;
            }
        }
    }
    CS2SX_LINE(19);
    self->f_cs2sx_state = -1;
    CS2SX_LINE(19);
    return 0;
}

const CS2SX_ClassInfo GetEnumerator_Iter_ClassInfo = { "GetEnumerator_Iter", &CS2SX_Iter_int_ClassInfo, NULL };

void GetEnumerator_Iter_Gc(void* _self, CS2SX_GcVisit visit)
{
    GetEnumerator_Iter* self = (GetEnumerator_Iter*)_self; (void)self;
    if (visit) visit((void*)self->f_cs2sx_this, &Deck_GcKind);
    else if (self->f_cs2sx_this) { void* _o = (void*)self->f_cs2sx_this; self->f_cs2sx_this = NULL; ((void (*)(void*))Deck_Free)(_o); }
    CS2SX_Iter_int_Gc(_self, visit);
}

void GetEnumerator_Iter__Destroy(GetEnumerator_Iter* self)
{
    if (self->f_cs2sx_this) { Deck_Free(self->f_cs2sx_this); self->f_cs2sx_this = NULL; }
    if (self->f_cs2sx_c1) { List_int_Free(self->f_cs2sx_c1); self->f_cs2sx_c1 = NULL; }
    CS2SX_Iter_int__Destroy((CS2SX_Iter_int*)self);
}

void GetEnumerator_Iter_Free(GetEnumerator_Iter* self)
{
    if (!self) return;
    CS2SX_Iter_int* _o = (CS2SX_Iter_int*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Iter_int_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

GetEnumerator_Iter* GetEnumerator_Iter_Retain(GetEnumerator_Iter* self)
{
    if (self) ((CS2SX_Iter_int*)self)->_rc++;
    return self;
}

static CS2SX_Iter_int_vtable GetEnumerator_Iter_vtable_instance =
{
    .MoveNext = (int(*)(void*))GetEnumerator_Iter_MoveNext,
};

void Node_Ctor(Node* self, int v)
{
    CS2SX_TRACE("Node..ctor", "Program.cs");
    CS2SX_LINE(26);
    self->f_V = v;
}

Node* Node_New(int v)
{
    Node* self = (Node*)calloc(1, sizeof(Node));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Node__Destroy;
    self->_gc = Node_Gc;
    self->_class = &Node_ClassInfo;
    Node_Ctor(self, v);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Node_Free);
    return self;
}

int Node_Twice(Node* self)
{
    CS2SX_TRACE("Node.Twice", "Program.cs");
    CS2SX_LINE(27);
    return self->f_V * 2;
}

const char* Node_Name(Node* self, const char* prefix)
{
    CS2SX_TRACE("Node.Name", "Program.cs");
    CS2SX_LINE(28);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "%s%d", prefix, self->f_V);
    return _sb0;
}

const CS2SX_ClassInfo Node_ClassInfo = { "Node", NULL, NULL };

void Node_Gc(void* _self, CS2SX_GcVisit visit)
{
    Node* self = (Node*)_self; (void)self;
}

static void Node_GcDyn(void* self, CS2SX_GcVisit visit) { ((Node*)self)->_gc(self, visit); }
const CS2SX_GcKind Node_GcKind = { offsetof(Node, _rc), Node_GcDyn, (CS2SX_FreeFn)Node_Free };

void Node__Destroy(Node* self)
{
}

void Node_Free(Node* self)
{
    if (!self) return;
    Node* _o = (Node*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Node_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Node* Node_Retain(Node* self)
{
    if (self) ((Node*)self)->_rc++;
    return self;
}

void RecordsAndFormatsApp_Init(RecordsAndFormatsApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))RecordsAndFormatsApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))RecordsAndFormatsApp_OnFrame;
}

void RecordsAndFormatsApp_OnInit(RecordsAndFormatsApp* self)
{
    CS2SX_TRACE("RecordsAndFormatsApp.OnInit", "Program.cs");
    CS2SX_LINE(35);
    Pair* a = Pair_New(1, 2);
    CS2SX_LINE(36);
    Pair* b = Pair_New(1, 2);
    CS2SX_LINE(37);
    Pair* c = Pair_New(2, 1);
    CS2SX_LINE(38);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "eq %s %s %s %s %s", CS2SX_BoolStr((Pair_Equals(a, b))), CS2SX_BoolStr((!Pair_Equals(a, b))), CS2SX_BoolStr((Pair_Equals(a, c))), CS2SX_BoolStr(Pair_Equals(a, b)), CS2SX_BoolStr(((const void*)(a) == (const void*)(b))));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(39);
    char* _sb1 = _cs2sx_next_buf();
    _sb1 = CS2SX_Format(_sb1, "str %s %s", Pair_ToString(a), Pair_ToString(c));
    const char* _ps_1 = _sb1;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    CS2SX_LINE(40);
    Hero* h = Hero_New("Mia", 3, Kind_Ice, a);
    CS2SX_LINE(41);
    printf("%s\n", Hero_ToString(h));
    CS2SX_LINE(42);
    char* _sb2 = _cs2sx_next_buf();
    _sb2 = CS2SX_Format(_sb2, "hero %s %s", CS2SX_BoolStr((Hero_Equals(h, Hero_New("Mia", 3, Kind_Ice, Pair_New(1, 2))))), CS2SX_BoolStr((Hero_Equals(h, Hero_New("Mia", 4, Kind_Ice, a)))));
    const char* _ps_2 = _sb2;
    printf("%s\n", _ps_2 ? _ps_2 : "");
    CS2SX_LINE(44);
    char* _sb3 = _cs2sx_next_buf();
    _sb3 = CS2SX_Format(_sb3, "fmt %s %s %s %s", CS2SX_FmtD((double)(3.14159), "F2"), CS2SX_FmtI((long long)(7), 32, "D3"), CS2SX_FmtI((long long)(1234567), 32, "N0"), CS2SX_FmtD((double)(0.5f), "F1"));
    const char* _ps_3 = _sb3;
    printf("%s\n", _ps_3 ? _ps_3 : "");
    CS2SX_LINE(46);
    Dict_str_int* _dict_4 = Dict_str_int_New();
    Dict_str_int_Set(_dict_4, "a", 1);
    Dict_str_int_Set(_dict_4, "b", 2);
    Dict_str_int* dict = _dict_4;
    CS2SX_LINE(47);
    Dict_Kind_str* _dict_5 = Dict_Kind_str_New();
    Dict_Kind_str_Set(_dict_5, Kind_Ice, "kalt");
    Dict_Kind_str* byKind = _dict_5;
    CS2SX_LINE(48);
    char* _sb4 = _cs2sx_next_buf();
    _sb4 = CS2SX_Format(_sb4, "dict %d %d %s", dict->count, Dict_str_int_Get(dict, "b"), Dict_Kind_str_Get(byKind, Kind_Ice));
    const char* _ps_6 = _sb4;
    printf("%s\n", _ps_6 ? _ps_6 : "");
    CS2SX_LINE(50);
    StringBuilder* sb = StringBuilder_AppendStr(StringBuilder_New(256), "Größe");
    CS2SX_LINE(51);
    StringBuilder_AppendChar(sb, '!');
    CS2SX_LINE(52);
    char* _sb5 = _cs2sx_next_buf();
    _sb5 = CS2SX_Format(_sb5, "sb %d %s", ({ StringBuilder* _cs2sx_sbl = CS2SX_NN(sb); _cs2sx_sbl->buf ? CS2SX_Str16Len(_cs2sx_sbl->buf) : 0; }), CS2SX_BoolStr((CS2SX_strcmp_safe(StringBuilder_ToString(sb), "Größe!") == 0)));
    const char* _ps_7 = _sb5;
    printf("%s\n", _ps_7 ? _ps_7 : "");
    CS2SX_LINE(54);
    int sum = 0;
    CS2SX_LINE(55);
    const char* order = "";
    {
        CS2SX_LINE(56);
        CS2SX_Iter_int* cs2sx_en1 = Deck_GetEnumerator((Deck_New()));
        CS2SX_LINE(56);
        while (({ __auto_type _cs2sx_r = CS2SX_NN(cs2sx_en1); _cs2sx_r->vtable->MoveNext(_cs2sx_r); }))
        {
            CS2SX_LINE(56);
            int card = CS2SX_NN(cs2sx_en1)->f_Current;
            CS2SX_LINE(56);
            sum += card;
            CS2SX_LINE(56);
            char* _sb6 = _cs2sx_next_buf();
            _sb6 = CS2SX_Format(_sb6, "%s%d", order ? order : "", card);
            order = _sb6;
        }
    }
    CS2SX_LINE(57);
    char* _sb7 = _cs2sx_next_buf();
    _sb7 = CS2SX_Format(_sb7, "deck %d %s", sum, order);
    const char* _ps_8 = _sb7;
    printf("%s\n", _ps_8 ? _ps_8 : "");
    CS2SX_LINE(59);
    Node* none = NULL;
    CS2SX_LINE(60);
    Node* some = Node_New(5);
    CS2SX_LINE(61);
    char* _sb8 = _cs2sx_next_buf();
    _sb8 = CS2SX_Format(_sb8, "cond %d %d %s %s", (({ __auto_type _nc_9 = (none != NULL ? ((Nullable_int){ Node_Twice((Node*)none), 1 }) : ((Nullable_int){0})); ((_nc_9).has ? (_nc_9).value : (-1)); })), (({ __auto_type _nc_10 = (some != NULL ? ((Nullable_int){ Node_Twice((Node*)some), 1 }) : ((Nullable_int){0})); ((_nc_10).has ? (_nc_10).value : (-1)); })), (((some != NULL ? Node_Name((Node*)some, "n") : NULL) ?: "-")), (((none != NULL ? Node_Name((Node*)none, "n") : NULL) ?: "-")));
    const char* _ps_11 = _sb8;
    printf("%s\n", _ps_11 ? _ps_11 : "");
    CS2SX_LINE(62);
    printf("done\n");
}

void RecordsAndFormatsApp_OnFrame(RecordsAndFormatsApp* self)
{
    CS2SX_TRACE("RecordsAndFormatsApp.OnFrame", "Program.cs");
}

void RecordsAndFormatsApp_Free(RecordsAndFormatsApp* self)
{
    if (!self) return;
    free(self);
}
