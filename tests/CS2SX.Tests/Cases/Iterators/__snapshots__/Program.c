#include <stdlib.h>
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

const CS2SX_ClassInfo CS2SX_Iter_ClassInfo = { "CS2SX_Iter", NULL };

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

const CS2SX_ClassInfo CS2SX_Iter_int_ClassInfo = { "CS2SX_Iter_int", NULL };

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

static CS2SX_Iter_str_vtable CS2SX_Iter_str_vtable_instance =
{
};

void CS2SX_Iter_str_Ctor(CS2SX_Iter_str* self)
{
    CS2SX_TRACE("CS2SX_Iter_str..ctor", "Program.cs");
}

CS2SX_Iter_str* CS2SX_Iter_str_New(void)
{
    CS2SX_Iter_str* self = (CS2SX_Iter_str*)calloc(1, sizeof(CS2SX_Iter_str));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))CS2SX_Iter_str__Destroy;
    self->_gc = CS2SX_Iter_str_Gc;
    self->_class = &CS2SX_Iter_str_ClassInfo;
    self->vtable = &CS2SX_Iter_str_vtable_instance;
    CS2SX_Iter_str_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)CS2SX_Iter_str_Free);
    return self;
}

void CS2SX_Iter_str_Reset(CS2SX_Iter_str* self)
{
    CS2SX_TRACE("CS2SX_Iter_str.Reset", "Program.cs");
    CS2SX_LINE(9);
    CS2SX_Throw((Exception*)(CS2SX_Exc_New(&CS2SX_TI_InvalidOperationException, "Reset wird nicht unterstützt — die Methode neu aufrufen.", NULL, NULL)));
}

void CS2SX_Iter_str_Dispose(CS2SX_Iter_str* self)
{
    CS2SX_TRACE("CS2SX_Iter_str.Dispose", "Program.cs");
}

const CS2SX_ClassInfo CS2SX_Iter_str_ClassInfo = { "CS2SX_Iter_str", NULL };

void CS2SX_Iter_str_Gc(void* _self, CS2SX_GcVisit visit)
{
    CS2SX_Iter_str* self = (CS2SX_Iter_str*)_self; (void)self;
}

static void CS2SX_Iter_str_GcDyn(void* self, CS2SX_GcVisit visit) { ((CS2SX_Iter_str*)self)->_gc(self, visit); }
const CS2SX_GcKind CS2SX_Iter_str_GcKind = { offsetof(CS2SX_Iter_str, _rc), CS2SX_Iter_str_GcDyn, (CS2SX_FreeFn)CS2SX_Iter_str_Free };

void CS2SX_Iter_str__Destroy(CS2SX_Iter_str* self)
{
    CS2SX_StrDrop(self->f_Current); self->f_Current = NULL;
}

void CS2SX_Iter_str_Free(CS2SX_Iter_str* self)
{
    if (!self) return;
    CS2SX_Iter_str* _o = (CS2SX_Iter_str*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Iter_str_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

CS2SX_Iter_str* CS2SX_Iter_str_Retain(CS2SX_Iter_str* self)
{
    if (self) ((CS2SX_Iter_str*)self)->_rc++;
    return self;
}

void Tree_Ctor(Tree* self, int v, Tree* l, Tree* r)
{
    CS2SX_TRACE("Tree..ctor", "Program.cs");
    CS2SX_LINE(13);
    self->f_Value = v;
    CS2SX_LINE(13);
    CS2SX_ARC_STORE(Tree, self->f_Left, CS2SX_RETAIN(Tree, l));
    CS2SX_LINE(13);
    CS2SX_ARC_STORE(Tree, self->f_Right, CS2SX_RETAIN(Tree, r));
}

Tree* Tree_New(int v, Tree* l, Tree* r)
{
    Tree* self = (Tree*)calloc(1, sizeof(Tree));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Tree__Destroy;
    self->_gc = Tree_Gc;
    self->_class = &Tree_ClassInfo;
    Tree_Ctor(self, v, l, r);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Tree_Free);
    return self;
}

const CS2SX_ClassInfo Tree_ClassInfo = { "Tree", NULL };

void Tree_Gc(void* _self, CS2SX_GcVisit visit)
{
    Tree* self = (Tree*)_self; (void)self;
    if (visit) visit((void*)self->f_Left, &Tree_GcKind);
    else if (self->f_Left) { void* _o = (void*)self->f_Left; self->f_Left = NULL; ((void (*)(void*))Tree_Free)(_o); }
    if (visit) visit((void*)self->f_Right, &Tree_GcKind);
    else if (self->f_Right) { void* _o = (void*)self->f_Right; self->f_Right = NULL; ((void (*)(void*))Tree_Free)(_o); }
}

static void Tree_GcDyn(void* self, CS2SX_GcVisit visit) { ((Tree*)self)->_gc(self, visit); }
const CS2SX_GcKind Tree_GcKind = { offsetof(Tree, _rc), Tree_GcDyn, (CS2SX_FreeFn)Tree_Free };

void Tree__Destroy(Tree* self)
{
    if (self->f_Left) { Tree_Free(self->f_Left); self->f_Left = NULL; }
    if (self->f_Right) { Tree_Free(self->f_Right); self->f_Right = NULL; }
}

void Tree_Free(Tree* self)
{
    if (!self) return;
    Tree* _o = (Tree*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Tree_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Tree* Tree_Retain(Tree* self)
{
    if (self) ((Tree*)self)->_rc++;
    return self;
}

List_int* Tree_get_InOrder(Tree* self)
{
    CS2SX_TRACE("Tree.get_InOrder", "Program.cs");
    CS2SX_LINE(19);
    List_int* cs2sx_yield = List_int_New();
    CS2SX_LINE(20);
    if (self->f_Left != NULL)
    {
        CS2SX_LINE(20);
        for (int _i_x = 0; _i_x < (int)(Tree_get_InOrder(CS2SX_NN(self->f_Left))->count); _i_x++)
        {
            int x = List_int_Get(Tree_get_InOrder(CS2SX_NN(self->f_Left)), _i_x);
            List_int_Add(cs2sx_yield, x);
        }
    }
    CS2SX_LINE(21);
    List_int_Add(cs2sx_yield, self->f_Value);
    CS2SX_LINE(22);
    if (self->f_Right != NULL)
    {
        CS2SX_LINE(22);
        for (int _i_x = 0; _i_x < (int)(Tree_get_InOrder(CS2SX_NN(self->f_Right))->count); _i_x++)
        {
            int x = List_int_Get(Tree_get_InOrder(CS2SX_NN(self->f_Right)), _i_x);
            List_int_Add(cs2sx_yield, x);
        }
    }
    CS2SX_LINE(23);
    return cs2sx_yield;
}

void Blinker_Ctor(Blinker* self)
{
    CS2SX_TRACE("Blinker..ctor", "Program.cs");
    self->f_Name = CS2SX_StrOwn("blink");
}

Blinker* Blinker_New(void)
{
    Blinker* self = (Blinker*)calloc(1, sizeof(Blinker));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Blinker__Destroy;
    self->_gc = Blinker_Gc;
    self->_class = &Blinker_ClassInfo;
    Blinker_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Blinker_Free);
    return self;
}

CS2SX_Iter_int* Blinker_Run(Blinker* self, int times)
{
    CS2SX_TRACE("Blinker.Run", "Program.cs");
    CS2SX_LINE(34);
    Run_Iter* cs2sx_sm = Run_Iter_New();
    CS2SX_LINE(34);
    CS2SX_ARC_STORE(Blinker, cs2sx_sm->f_cs2sx_this, CS2SX_RETAIN(Blinker, self));
    CS2SX_LINE(34);
    cs2sx_sm->f_times = times;
    CS2SX_LINE(34);
    return ((CS2SX_Iter_int*)cs2sx_sm);
}

const CS2SX_ClassInfo Blinker_ClassInfo = { "Blinker", NULL };

void Blinker_Gc(void* _self, CS2SX_GcVisit visit)
{
    Blinker* self = (Blinker*)_self; (void)self;
}

static void Blinker_GcDyn(void* self, CS2SX_GcVisit visit) { ((Blinker*)self)->_gc(self, visit); }
const CS2SX_GcKind Blinker_GcKind = { offsetof(Blinker, _rc), Blinker_GcDyn, (CS2SX_FreeFn)Blinker_Free };

void Blinker__Destroy(Blinker* self)
{
    CS2SX_StrDrop(self->f_Name); self->f_Name = NULL;
}

void Blinker_Free(Blinker* self)
{
    if (!self) return;
    Blinker* _o = (Blinker*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Blinker_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Blinker* Blinker_Retain(Blinker* self)
{
    if (self) ((Blinker*)self)->_rc++;
    return self;
}

static CS2SX_Iter_int_vtable Run_Iter_vtable_instance;

void Run_Iter_Ctor(Run_Iter* self)
{
    CS2SX_TRACE("Run_Iter..ctor", "Program.cs");
    CS2SX_Iter_int_Ctor((CS2SX_Iter_int*)self);
}

Run_Iter* Run_Iter_New(void)
{
    Run_Iter* self = (Run_Iter*)calloc(1, sizeof(Run_Iter));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Run_Iter__Destroy;
    self->base._gc = Run_Iter_Gc;
    self->base._class = &Run_Iter_ClassInfo;
    self->base.vtable = &Run_Iter_vtable_instance;
    Run_Iter_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Run_Iter_Free);
    return self;
}

int Run_Iter_MoveNext(Run_Iter* self)
{
    CS2SX_TRACE("Run_Iter.MoveNext", "Program.cs");
    CS2SX_LINE(34);
    if (self->f_cs2sx_state < 0)
    {
        CS2SX_LINE(34);
        return 0;
    }
    CS2SX_LINE(34);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; case 2: goto _cs2sx_r2_0; default: break; };
    CS2SX_LINE(35);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "%s:", CS2SX_NN(self->f_cs2sx_this)->f_Name);
    CS2SX_STR_STORE(self->f_label, _sb0);
    CS2SX_LINE(36);
    for (self->f_i = 0; self->f_i < self->f_times; self->f_i++)
    {
        CS2SX_LINE(38);
        CS2SX_NN(self->f_cs2sx_this)->f_Steps++;
        {
            CS2SX_LINE(39);
            self->base.f_Current = self->f_i * 10;
            CS2SX_LINE(39);
            self->f_cs2sx_state = 1; return true; _cs2sx_r1_0: self->f_cs2sx_state = 0;
        }
        CS2SX_LINE(40);
        char* _sb1 = _cs2sx_next_buf();
        _sb1 = CS2SX_Format(_sb1, "%s%d", self->f_label ? self->f_label : "", self->f_i);
        CS2SX_STR_STORE(self->f_label, _sb1);
    }
    CS2SX_LINE(42);
    const char* _ps_0 = self->f_label;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    {
        CS2SX_LINE(43);
        self->base.f_Current = -1;
        CS2SX_LINE(43);
        self->f_cs2sx_state = 2; return true; _cs2sx_r2_0: self->f_cs2sx_state = 0;
    }
    CS2SX_LINE(44);
    self->f_cs2sx_state = -1;
    CS2SX_LINE(44);
    return 0;
}

const CS2SX_ClassInfo Run_Iter_ClassInfo = { "Run_Iter", &CS2SX_Iter_int_ClassInfo };

void Run_Iter_Gc(void* _self, CS2SX_GcVisit visit)
{
    Run_Iter* self = (Run_Iter*)_self; (void)self;
    if (visit) visit((void*)self->f_cs2sx_this, &Blinker_GcKind);
    else if (self->f_cs2sx_this) { void* _o = (void*)self->f_cs2sx_this; self->f_cs2sx_this = NULL; ((void (*)(void*))Blinker_Free)(_o); }
    CS2SX_Iter_int_Gc(_self, visit);
}

void Run_Iter__Destroy(Run_Iter* self)
{
    if (self->f_cs2sx_this) { Blinker_Free(self->f_cs2sx_this); self->f_cs2sx_this = NULL; }
    CS2SX_StrDrop(self->f_label); self->f_label = NULL;
    CS2SX_Iter_int__Destroy((CS2SX_Iter_int*)self);
}

void Run_Iter_Free(Run_Iter* self)
{
    if (!self) return;
    CS2SX_Iter_int* _o = (CS2SX_Iter_int*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Iter_int_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Run_Iter* Run_Iter_Retain(Run_Iter* self)
{
    if (self) ((CS2SX_Iter_int*)self)->_rc++;
    return self;
}

static CS2SX_Iter_int_vtable Run_Iter_vtable_instance =
{
    .MoveNext = (int(*)(void*))Run_Iter_MoveNext,
};

void IteratorsApp_Init(IteratorsApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))IteratorsApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))IteratorsApp_OnFrame;
}

List_int* IteratorsApp_Evens(int max)
{
    CS2SX_TRACE("IteratorsApp.Evens", "Program.cs");
    CS2SX_LINE(50);
    List_int* cs2sx_yield = List_int_New();
    CS2SX_LINE(51);
    for (int i = 0; i <= max; i += 2)
    {
        CS2SX_LINE(53);
        if (i == 6)
        {
            CS2SX_LINE(53);
            continue;
        }
        CS2SX_LINE(54);
        List_int_Add(cs2sx_yield, i);
    }
    CS2SX_LINE(56);
    if (max > 100)
    {
        CS2SX_LINE(56);
        return cs2sx_yield;
    }
    CS2SX_LINE(57);
    List_int_Add(cs2sx_yield, 1000);
    CS2SX_LINE(58);
    return cs2sx_yield;
}

List_str* IteratorsApp_Words()
{
    CS2SX_TRACE("IteratorsApp.Words", "Program.cs");
    CS2SX_LINE(61);
    List_str* cs2sx_yield = List_str_New();
    CS2SX_LINE(62);
    List_str_Add(cs2sx_yield, "a");
    CS2SX_LINE(63);
    List_str_Add(cs2sx_yield, "bb");
    CS2SX_LINE(64);
    List_str_Add(cs2sx_yield, "ccc");
    CS2SX_LINE(65);
    return cs2sx_yield;
}

CS2SX_Iter_str* IteratorsApp_Over(List_str* items, const char* sep)
{
    CS2SX_TRACE("IteratorsApp.Over", "Program.cs");
    CS2SX_LINE(68);
    Over_Iter* cs2sx_sm = Over_Iter_New();
    CS2SX_LINE(68);
    CS2SX_COLL_STORE(List_str, cs2sx_sm->f_items, items);
    CS2SX_LINE(68);
    CS2SX_STR_STORE(cs2sx_sm->f_sep, sep);
    CS2SX_LINE(68);
    return ((CS2SX_Iter_str*)cs2sx_sm);
}

CS2SX_Iter_str* IteratorsApp_FromSequence()
{
    CS2SX_TRACE("IteratorsApp.FromSequence", "Program.cs");
    CS2SX_LINE(78);
    FromSequence_Iter* cs2sx_sm = FromSequence_Iter_New();
    CS2SX_LINE(78);
    return ((CS2SX_Iter_str*)cs2sx_sm);
}

CS2SX_Iter* IteratorsApp_Wait(int frames)
{
    CS2SX_TRACE("IteratorsApp.Wait", "Program.cs");
    CS2SX_LINE(83);
    Wait_Iter* cs2sx_sm = Wait_Iter_New();
    CS2SX_LINE(83);
    cs2sx_sm->f_frames = frames;
    CS2SX_LINE(83);
    return ((CS2SX_Iter*)cs2sx_sm);
}

static double _lambda_0(const char* w)
{
    return strlen(CS2SX_NN(w));
}

void IteratorsApp_OnInit(IteratorsApp* self)
{
    CS2SX_TRACE("IteratorsApp.OnInit", "Program.cs");
    CS2SX_LINE(93);
    List_int* evens = List_int_New();
    CS2SX_LINE(94);
    __auto_type _fe_0 = IteratorsApp_Evens(10);
    for (int _i_e = 0; _i_e < (int)(_fe_0->count); _i_e++)
    {
        int e = List_int_Get(_fe_0, _i_e);
        List_int_Add(evens, e);
    }
    CS2SX_LINE(95);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "evens %s", ({ __auto_type _jl = (evens); int _jn = (_jl ? _jl->count : 0); CS2SX_JOIN_EACH(",", _jn, "%d", (int)(_jl->data[_ji])); }));
    const char* _ps_1 = _sb0;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    CS2SX_LINE(96);
    char* _sb1 = _cs2sx_next_buf();
    _sb1 = CS2SX_Format(_sb1, "big %d", IteratorsApp_Evens(200)->count);
    const char* _ps_2 = _sb1;
    printf("%s\n", _ps_2 ? _ps_2 : "");
    CS2SX_LINE(97);
    __auto_type _src_3 = IteratorsApp_Words();
    int _sum_5 = 0;
    for (int _i_4 = 0; _i_4 < _src_3->count; _i_4++)
      _sum_5 += (int)_lambda_0(List_str_Get(_src_3, _i_4));
    char* _sb2 = _cs2sx_next_buf();
    _sb2 = CS2SX_Format(_sb2, "words %d", _sum_5);
    const char* _ps_6 = _sb2;
    printf("%s\n", _ps_6 ? _ps_6 : "");
    CS2SX_LINE(99);
    Tree* t = Tree_New(4, Tree_New(2, Tree_New(1, NULL, NULL), Tree_New(3, NULL, NULL)), Tree_New(6, NULL, Tree_New(7, NULL, NULL)));
    CS2SX_LINE(100);
    char* _sb3 = _cs2sx_next_buf();
    _sb3 = CS2SX_Format(_sb3, "tree %s", ({ __auto_type _jl = (Tree_get_InOrder(t)); int _jn = (_jl ? _jl->count : 0); CS2SX_JOIN_EACH(" ", _jn, "%d", (int)(_jl->data[_ji])); }));
    const char* _ps_7 = _sb3;
    printf("%s\n", _ps_7 ? _ps_7 : "");
    CS2SX_LINE(102);
    Blinker* b = Blinker_New();
    CS2SX_LINE(103);
    CS2SX_Iter_int* run = Blinker_Run(b, 3);
    CS2SX_LINE(104);
    char* _sb4 = _cs2sx_next_buf();
    _sb4 = CS2SX_Format(_sb4, "lazy %d", b->f_Steps);
    const char* _ps_8 = _sb4;
    printf("%s\n", _ps_8 ? _ps_8 : "");
    CS2SX_LINE(105);
    const char* got = "";
    CS2SX_LINE(106);
    while (({ __auto_type _cs2sx_r = CS2SX_NN(run); _cs2sx_r->vtable->MoveNext(_cs2sx_r); }))
    {
        CS2SX_LINE(106);
        char* _sb5 = _cs2sx_next_buf();
        _sb5 = CS2SX_Format(_sb5, "%d ", CS2SX_NN(run)->f_Current);
        char* _sb6 = _cs2sx_next_buf();
        _sb6 = CS2SX_Format(_sb6, "%s%s", got ? got : "", _sb5 ? _sb5 : "");
        got = _sb6;
    }
    CS2SX_LINE(107);
    char* _sb7 = _cs2sx_next_buf();
    _sb7 = CS2SX_Format(_sb7, "run %s%d", got, b->f_Steps);
    const char* _ps_9 = _sb7;
    printf("%s\n", _ps_9 ? _ps_9 : "");
    CS2SX_LINE(108);
    char* _sb8 = _cs2sx_next_buf();
    _sb8 = CS2SX_Format(_sb8, "after %s", CS2SX_BoolStr(({ __auto_type _cs2sx_r = CS2SX_NN(run); _cs2sx_r->vtable->MoveNext(_cs2sx_r); })));
    const char* _ps_10 = _sb8;
    printf("%s\n", _ps_10 ? _ps_10 : "");
    CS2SX_LINE(110);
    List_str* _list_11 = List_str_New();
    List_str_Add(_list_11, "x");
    List_str_Add(_list_11, "y");
    List_str_Add(_list_11, "stop");
    List_str_Add(_list_11, "z");
    CS2SX_Iter_str* o = IteratorsApp_Over(_list_11, "!");
    CS2SX_LINE(111);
    List_str* parts = List_str_New();
    CS2SX_LINE(112);
    while (({ __auto_type _cs2sx_r = CS2SX_NN(o); _cs2sx_r->vtable->MoveNext(_cs2sx_r); }))
    {
        CS2SX_LINE(112);
        List_str_Add(parts, CS2SX_NN(o)->f_Current);
    }
    CS2SX_LINE(113);
    char* _sb9 = _cs2sx_next_buf();
    _sb9 = CS2SX_Format(_sb9, "over %s", String_Join(" ", parts));
    const char* _ps_12 = _sb9;
    printf("%s\n", _ps_12 ? _ps_12 : "");
    CS2SX_LINE(114);
    List_str* _list_13 = List_str_New();
    List_str_Add(_list_13, "q");
    CS2SX_Iter_str* o2 = IteratorsApp_Over(_list_13, "?");
    CS2SX_LINE(115);
    const char* all = "";
    CS2SX_LINE(116);
    while (({ __auto_type _cs2sx_r = CS2SX_NN(o2); _cs2sx_r->vtable->MoveNext(_cs2sx_r); }))
    {
        CS2SX_LINE(116);
        char* _sb10 = _cs2sx_next_buf();
        _sb10 = CS2SX_Format(_sb10, "%s%s", all ? all : "", CS2SX_NN(o2)->f_Current ? CS2SX_NN(o2)->f_Current : "");
        all = _sb10;
    }
    CS2SX_LINE(117);
    char* _sb11 = _cs2sx_next_buf();
    _sb11 = CS2SX_Format(_sb11, "over2 %s", all);
    const char* _ps_14 = _sb11;
    printf("%s\n", _ps_14 ? _ps_14 : "");
    CS2SX_LINE(119);
    CS2SX_Iter_str* f = IteratorsApp_FromSequence();
    CS2SX_LINE(120);
    const char* caps = "";
    CS2SX_LINE(121);
    while (({ __auto_type _cs2sx_r = CS2SX_NN(f); _cs2sx_r->vtable->MoveNext(_cs2sx_r); }))
    {
        CS2SX_LINE(121);
        char* _sb12 = _cs2sx_next_buf();
        _sb12 = CS2SX_Format(_sb12, "%s%s", caps ? caps : "", CS2SX_NN(f)->f_Current ? CS2SX_NN(f)->f_Current : "");
        caps = _sb12;
    }
    CS2SX_LINE(122);
    char* _sb13 = _cs2sx_next_buf();
    _sb13 = CS2SX_Format(_sb13, "caps %s", caps);
    const char* _ps_15 = _sb13;
    printf("%s\n", _ps_15 ? _ps_15 : "");
    CS2SX_LINE(125);
    CS2SX_Iter_int* c1 = Blinker_Run(b, 2);
    CS2SX_LINE(125);
    CS2SX_Iter_int* c2 = Blinker_Run(b, 2);
    CS2SX_LINE(126);
    ({ __auto_type _cs2sx_r = CS2SX_NN(c1); _cs2sx_r->vtable->MoveNext(_cs2sx_r); });
    CS2SX_LINE(126);
    ({ __auto_type _cs2sx_r = CS2SX_NN(c1); _cs2sx_r->vtable->MoveNext(_cs2sx_r); });
    CS2SX_LINE(126);
    ({ __auto_type _cs2sx_r = CS2SX_NN(c2); _cs2sx_r->vtable->MoveNext(_cs2sx_r); });
    CS2SX_LINE(127);
    char* _sb14 = _cs2sx_next_buf();
    _sb14 = CS2SX_Format(_sb14, "indep %d %d", CS2SX_NN(c1)->f_Current, CS2SX_NN(c2)->f_Current);
    const char* _ps_16 = _sb14;
    printf("%s\n", _ps_16 ? _ps_16 : "");
    CS2SX_LINE(129);
    Blinker* _blinker_17 = Blinker_New();
    CS2SX_STR_STORE(_blinker_17->f_Name, "frame");
    CS2SX_ARC_STORE(CS2SX_Iter_int, self->f_co, CS2SX_RETAIN(CS2SX_Iter_int, Blinker_Run(_blinker_17, 2)));
    CS2SX_LINE(130);
    CS2SX_ARC_STORE(CS2SX_Iter, self->f_wait, CS2SX_RETAIN(CS2SX_Iter, IteratorsApp_Wait(3)));
}

void IteratorsApp_OnFrame(IteratorsApp* self)
{
    CS2SX_TRACE("IteratorsApp.OnFrame", "Program.cs");
    CS2SX_LINE(135);
    self->f_frame++;
    CS2SX_LINE(136);
    if (self->f_wait != NULL && !({ __auto_type _cs2sx_r = CS2SX_NN(self->f_wait); _cs2sx_r->vtable->MoveNext(_cs2sx_r); }))
    {
        CS2SX_LINE(138);
        char* _sb15 = _cs2sx_next_buf();
        _sb15 = CS2SX_Format(_sb15, "waited %d", self->f_frame);
        const char* _ps_18 = _sb15;
        printf("%s\n", _ps_18 ? _ps_18 : "");
        CS2SX_LINE(139);
        CS2SX_ARC_STORE(CS2SX_Iter, self->f_wait, NULL);
    }
    CS2SX_LINE(141);
    if (self->f_co != NULL)
    {
        CS2SX_LINE(143);
        if (({ __auto_type _cs2sx_r = CS2SX_NN(self->f_co); _cs2sx_r->vtable->MoveNext(_cs2sx_r); }))
        {
            CS2SX_LINE(143);
            char* _sb16 = _cs2sx_next_buf();
            _sb16 = CS2SX_Format(_sb16, "co %d %d", self->f_frame, CS2SX_NN(self->f_co)->f_Current);
            const char* _ps_19 = _sb16;
            printf("%s\n", _ps_19 ? _ps_19 : "");
        }
        else
        {
            CS2SX_LINE(144);
            char* _sb17 = _cs2sx_next_buf();
            _sb17 = CS2SX_Format(_sb17, "co done %d", self->f_frame);
            const char* _ps_20 = _sb17;
            printf("%s\n", _ps_20 ? _ps_20 : "");
            CS2SX_LINE(144);
            CS2SX_ARC_STORE(CS2SX_Iter_int, self->f_co, NULL);
        }
    }
    CS2SX_LINE(146);
    if (self->f_frame == 6)
    {
        CS2SX_LINE(146);
        printf("done\n");
    }
}

void IteratorsApp_Free(IteratorsApp* self)
{
    if (!self) return;
    if (self->f_co) { CS2SX_Iter_int_Free(self->f_co); self->f_co = NULL; }
    if (self->f_wait) { CS2SX_Iter_Free(self->f_wait); self->f_wait = NULL; }
    free(self);
}

static CS2SX_Iter_str_vtable Over_Iter_vtable_instance;

void Over_Iter_Ctor(Over_Iter* self)
{
    CS2SX_TRACE("Over_Iter..ctor", "Program.cs");
    self->f_items = List_str_Retain(List_str_New());
    self->f_cs2sx_c1 = List_str_Retain(List_str_New());
    CS2SX_Iter_str_Ctor((CS2SX_Iter_str*)self);
}

Over_Iter* Over_Iter_New(void)
{
    Over_Iter* self = (Over_Iter*)calloc(1, sizeof(Over_Iter));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Over_Iter__Destroy;
    self->base._gc = Over_Iter_Gc;
    self->base._class = &Over_Iter_ClassInfo;
    self->base.vtable = &Over_Iter_vtable_instance;
    Over_Iter_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Over_Iter_Free);
    return self;
}

int Over_Iter_MoveNext(Over_Iter* self)
{
    CS2SX_TRACE("Over_Iter.MoveNext", "Program.cs");
    CS2SX_LINE(68);
    if (self->f_cs2sx_state < 0)
    {
        CS2SX_LINE(68);
        return 0;
    }
    CS2SX_LINE(68);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; case 2: goto _cs2sx_r2_0; default: break; };
    {
        CS2SX_LINE(69);
        CS2SX_COLL_STORE(List_str, self->f_cs2sx_c1, self->f_items);
        CS2SX_LINE(69);
        for (self->f_cs2sx_i1 = 0; self->f_cs2sx_i1 < CS2SX_NN(self->f_cs2sx_c1)->count; self->f_cs2sx_i1++)
        {
            CS2SX_LINE(70);
            CS2SX_STR_STORE(self->f_s, List_str_Get(self->f_cs2sx_c1, self->f_cs2sx_i1));
            {
                CS2SX_LINE(71);
                char* _sb0 = _cs2sx_next_buf();
                _sb0 = CS2SX_Format(_sb0, "%s%s", self->f_s, self->f_sep);
                CS2SX_STR_STORE(self->base.f_Current, _sb0);
                CS2SX_LINE(71);
                self->f_cs2sx_state = 1; return true; _cs2sx_r1_0: self->f_cs2sx_state = 0;
            }
            CS2SX_LINE(72);
            if (CS2SX_strcmp_safe(self->f_s, "stop") == 0)
            {
                CS2SX_LINE(72);
                self->f_cs2sx_state = -1;
                CS2SX_LINE(72);
                return 0;
            }
        }
    }
    {
        CS2SX_LINE(74);
        CS2SX_STR_STORE(self->base.f_Current, "end");
        CS2SX_LINE(74);
        self->f_cs2sx_state = 2; return true; _cs2sx_r2_0: self->f_cs2sx_state = 0;
    }
    CS2SX_LINE(75);
    self->f_cs2sx_state = -1;
    CS2SX_LINE(75);
    return 0;
}

const CS2SX_ClassInfo Over_Iter_ClassInfo = { "Over_Iter", &CS2SX_Iter_str_ClassInfo };

void Over_Iter_Gc(void* _self, CS2SX_GcVisit visit)
{
    Over_Iter* self = (Over_Iter*)_self; (void)self;
    CS2SX_Iter_str_Gc(_self, visit);
}

void Over_Iter__Destroy(Over_Iter* self)
{
    if (self->f_items) { List_str_Free(self->f_items); self->f_items = NULL; }
    CS2SX_StrDrop(self->f_sep); self->f_sep = NULL;
    CS2SX_StrDrop(self->f_s); self->f_s = NULL;
    if (self->f_cs2sx_c1) { List_str_Free(self->f_cs2sx_c1); self->f_cs2sx_c1 = NULL; }
    CS2SX_Iter_str__Destroy((CS2SX_Iter_str*)self);
}

void Over_Iter_Free(Over_Iter* self)
{
    if (!self) return;
    CS2SX_Iter_str* _o = (CS2SX_Iter_str*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Iter_str_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Over_Iter* Over_Iter_Retain(Over_Iter* self)
{
    if (self) ((CS2SX_Iter_str*)self)->_rc++;
    return self;
}

static CS2SX_Iter_str_vtable Over_Iter_vtable_instance =
{
    .MoveNext = (int(*)(void*))Over_Iter_MoveNext,
};

static CS2SX_Iter_str_vtable FromSequence_Iter_vtable_instance;

void FromSequence_Iter_Ctor(FromSequence_Iter* self)
{
    CS2SX_TRACE("FromSequence_Iter..ctor", "Program.cs");
    self->f_cs2sx_c1 = List_str_Retain(List_str_New());
    CS2SX_Iter_str_Ctor((CS2SX_Iter_str*)self);
}

FromSequence_Iter* FromSequence_Iter_New(void)
{
    FromSequence_Iter* self = (FromSequence_Iter*)calloc(1, sizeof(FromSequence_Iter));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))FromSequence_Iter__Destroy;
    self->base._gc = FromSequence_Iter_Gc;
    self->base._class = &FromSequence_Iter_ClassInfo;
    self->base.vtable = &FromSequence_Iter_vtable_instance;
    FromSequence_Iter_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)FromSequence_Iter_Free);
    return self;
}

int FromSequence_Iter_MoveNext(FromSequence_Iter* self)
{
    CS2SX_TRACE("FromSequence_Iter.MoveNext", "Program.cs");
    CS2SX_LINE(78);
    if (self->f_cs2sx_state < 0)
    {
        CS2SX_LINE(78);
        return 0;
    }
    CS2SX_LINE(78);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; default: break; };
    {
        CS2SX_LINE(79);
        List_str* _src_0 = IteratorsApp_Words();
        List_str* _list_1 = List_str_New();
        if (_src_0) { for (int _cpi = 0; _cpi < _src_0->count; _cpi++) List_str_Add(_list_1, _src_0->data[_cpi]); }
        CS2SX_COLL_STORE(List_str, self->f_cs2sx_c1, _list_1);
        CS2SX_LINE(79);
        for (self->f_cs2sx_i1 = 0; self->f_cs2sx_i1 < CS2SX_NN(self->f_cs2sx_c1)->count; self->f_cs2sx_i1++)
        {
            CS2SX_LINE(79);
            CS2SX_STR_STORE(self->f_w, List_str_Get(self->f_cs2sx_c1, self->f_cs2sx_i1));
            CS2SX_LINE(79);
            CS2SX_STR_STORE(self->f_up, String_ToUpper(CS2SX_NN(self->f_w)));
            {
                CS2SX_LINE(79);
                CS2SX_STR_STORE(self->base.f_Current, self->f_up);
                CS2SX_LINE(79);
                self->f_cs2sx_state = 1; return true; _cs2sx_r1_0: self->f_cs2sx_state = 0;
            }
        }
    }
    CS2SX_LINE(80);
    self->f_cs2sx_state = -1;
    CS2SX_LINE(80);
    return 0;
}

const CS2SX_ClassInfo FromSequence_Iter_ClassInfo = { "FromSequence_Iter", &CS2SX_Iter_str_ClassInfo };

void FromSequence_Iter_Gc(void* _self, CS2SX_GcVisit visit)
{
    FromSequence_Iter* self = (FromSequence_Iter*)_self; (void)self;
    CS2SX_Iter_str_Gc(_self, visit);
}

void FromSequence_Iter__Destroy(FromSequence_Iter* self)
{
    CS2SX_StrDrop(self->f_up); self->f_up = NULL;
    CS2SX_StrDrop(self->f_w); self->f_w = NULL;
    if (self->f_cs2sx_c1) { List_str_Free(self->f_cs2sx_c1); self->f_cs2sx_c1 = NULL; }
    CS2SX_Iter_str__Destroy((CS2SX_Iter_str*)self);
}

void FromSequence_Iter_Free(FromSequence_Iter* self)
{
    if (!self) return;
    CS2SX_Iter_str* _o = (CS2SX_Iter_str*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Iter_str_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

FromSequence_Iter* FromSequence_Iter_Retain(FromSequence_Iter* self)
{
    if (self) ((CS2SX_Iter_str*)self)->_rc++;
    return self;
}

static CS2SX_Iter_str_vtable FromSequence_Iter_vtable_instance =
{
    .MoveNext = (int(*)(void*))FromSequence_Iter_MoveNext,
};

static CS2SX_Iter_vtable Wait_Iter_vtable_instance;

void Wait_Iter_Ctor(Wait_Iter* self)
{
    CS2SX_TRACE("Wait_Iter..ctor", "Program.cs");
    CS2SX_Iter_Ctor((CS2SX_Iter*)self);
}

Wait_Iter* Wait_Iter_New(void)
{
    Wait_Iter* self = (Wait_Iter*)calloc(1, sizeof(Wait_Iter));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Wait_Iter__Destroy;
    self->base._gc = Wait_Iter_Gc;
    self->base._class = &Wait_Iter_ClassInfo;
    self->base.vtable = &Wait_Iter_vtable_instance;
    Wait_Iter_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Wait_Iter_Free);
    return self;
}

int Wait_Iter_MoveNext(Wait_Iter* self)
{
    CS2SX_TRACE("Wait_Iter.MoveNext", "Program.cs");
    CS2SX_LINE(83);
    if (self->f_cs2sx_state < 0)
    {
        CS2SX_LINE(83);
        return 0;
    }
    CS2SX_LINE(83);
    switch (self->f_cs2sx_state) { case 1: goto _cs2sx_r1_0; default: break; };
    CS2SX_LINE(84);
    while (self->f_frames > 0)
    {
        CS2SX_LINE(84);
        self->f_frames--;
        {
            CS2SX_LINE(84);
            CS2SX_RC_STORE(CS2SX_Obj_Retain, CS2SX_Obj_Release, self->base.f_Current, NULL);
            CS2SX_LINE(84);
            self->f_cs2sx_state = 1; return true; _cs2sx_r1_0: self->f_cs2sx_state = 0;
        }
    }
    CS2SX_LINE(85);
    self->f_cs2sx_state = -1;
    CS2SX_LINE(85);
    return 0;
}

const CS2SX_ClassInfo Wait_Iter_ClassInfo = { "Wait_Iter", &CS2SX_Iter_ClassInfo };

void Wait_Iter_Gc(void* _self, CS2SX_GcVisit visit)
{
    Wait_Iter* self = (Wait_Iter*)_self; (void)self;
    CS2SX_Iter_Gc(_self, visit);
}

void Wait_Iter__Destroy(Wait_Iter* self)
{
    CS2SX_Iter__Destroy((CS2SX_Iter*)self);
}

void Wait_Iter_Free(Wait_Iter* self)
{
    if (!self) return;
    CS2SX_Iter* _o = (CS2SX_Iter*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &CS2SX_Iter_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Wait_Iter* Wait_Iter_Retain(Wait_Iter* self)
{
    if (self) ((CS2SX_Iter*)self)->_rc++;
    return self;
}

static CS2SX_Iter_vtable Wait_Iter_vtable_instance =
{
    .MoveNext = (int(*)(void*))Wait_Iter_MoveNext,
};
