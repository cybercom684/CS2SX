#include <stdlib.h>
#include "_generics.h"
#include "_interfaces.h"
#include "Program.h"

int Item_Alive;
void Item_Ctor(Item* self)
{
    CS2SX_TRACE("Item..ctor", "Program.cs");
    self->f_V = 1;
    CS2SX_LINE(12);
    Item_Alive++;
}

Item* Item_New(void)
{
    Item* self = (Item*)calloc(1, sizeof(Item));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Item__Destroy;
    self->_gc = Item_Gc;
    self->_class = &Item_ClassInfo;
    Item_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Item_Free);
    return self;
}

int Item_Get(Item* self)
{
    CS2SX_TRACE("Item.Get", "Program.cs");
    CS2SX_LINE(14);
    return self->f_V;
}

const CS2SX_ClassInfo Item_ClassInfo = { "Item", NULL };

void Item_Gc(void* _self, CS2SX_GcVisit visit)
{
    Item* self = (Item*)_self; (void)self;
}

static void Item_GcDyn(void* self, CS2SX_GcVisit visit) { ((Item*)self)->_gc(self, visit); }
const CS2SX_GcKind Item_GcKind = { offsetof(Item, _rc), Item_GcDyn, (CS2SX_FreeFn)Item_Free };

void Item__Destroy(Item* self)
{
    CS2SX_LINE(13);
    Item_Alive--;
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

int Thing_Alive;
void Thing_Ctor(Thing* self)
{
    CS2SX_TRACE("Thing..ctor", "Program.cs");
    CS2SX_LINE(22);
    Thing_Alive++;
}

Thing* Thing_New(void)
{
    Thing* self = (Thing*)calloc(1, sizeof(Thing));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Thing__Destroy;
    self->_gc = Thing_Gc;
    self->_class = &Thing_ClassInfo;
    Thing_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Thing_Free);
    return self;
}

int Thing_Value(Thing* self)
{
    CS2SX_TRACE("Thing.Value", "Program.cs");
    CS2SX_LINE(24);
    return 2;
}

const CS2SX_ClassInfo Thing_ClassInfo = { "Thing", NULL };

void Thing_Gc(void* _self, CS2SX_GcVisit visit)
{
    Thing* self = (Thing*)_self; (void)self;
}

static void Thing_GcDyn(void* self, CS2SX_GcVisit visit) { ((Thing*)self)->_gc(self, visit); }
const CS2SX_GcKind Thing_GcKind = { offsetof(Thing, _rc), Thing_GcDyn, (CS2SX_FreeFn)Thing_Free };

void Thing__Destroy(Thing* self)
{
    CS2SX_LINE(23);
    Thing_Alive--;
}

void Thing_Free(Thing* self)
{
    if (!self) return;
    Thing* _o = (Thing*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Thing_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Thing* Thing_Retain(Thing* self)
{
    if (self) ((Thing*)self)->_rc++;
    return self;
}

IThing_vtable Thing_IThing_vtable_instance =
{
    .Value = (int (*)(void*))Thing_Value,
};


// ── Struct: Slot ────────────────────────────────────────
int Slot_Equals(Slot a, Slot b)
{
    return memcmp(&a, &b, sizeof(Slot)) == 0;
}

void Slot_Keep(Slot* s)
{
    if (s->Held) Item_Retain((Item*)s->Held);
    s->Label = CS2SX_StrOwn(s->Label);
}

void Slot_Drop(Slot* s)
{
    CS2SX_ReleaseLater((void*)s->Held, (CS2SX_FreeFn)Item_Free);
    CS2SX_StrDrop(s->Label);
}

void Slot_Gc(Slot* s, CS2SX_GcVisit visit)
{
    if (visit) visit((void*)s->Held, &Item_GcKind); else if (s->Held) { void* _o = (void*)s->Held; s->Held = NULL; Item_Free((Item*)_o); }
    (void)s; (void)visit;
}

int Holder_Alive;
void Holder_Ctor(Holder* self)
{
    CS2SX_TRACE("Holder..ctor", "Program.cs");
    self->f_Calls = List_Func_int_t_Retain(List_Func_int_t_New());
    self->f_Things = List_IThing_Retain(List_IThing_New());
    self->f_Slots = List_Slot_Retain(List_Slot_New());
    CS2SX_LINE(47);
    Holder_Alive++;
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

struct _cap_0
{
    CS2SX_Env _hdr;
    Holder* self;
};

static void _lambda_0(void* _env)
{
    CS2SX_TRACE("Holder.<lambda>", "Program.cs");
    struct _cap_0* _c = (struct _cap_0*)_env;
    Holder* self = _c->self;
    self->f_Count++;
}

static void _envdtor_0(void* _e)
{
    struct _cap_0* _c = (struct _cap_0*)_e;
    CS2SX_ReleaseLater((void*)_c->self, (CS2SX_FreeFn)Holder_Free);
}

static void _envgc_0(void* _e, CS2SX_GcVisit visit)
{
    struct _cap_0* _c = (struct _cap_0*)_e;
    if (visit) visit((void*)_c->self, &Holder_GcKind);
    else if (_c->self) { void* _o = (void*)_c->self; _c->self = NULL; ((CS2SX_FreeFn)Holder_Free)(_o); }
    (void)_c; (void)visit;
}

struct _cap_1
{
    CS2SX_Env _hdr;
    Holder* self;
};

static int _lambda_1(void* _env)
{
    CS2SX_TRACE("Holder.<lambda>", "Program.cs");
    struct _cap_1* _c = (struct _cap_1*)_env;
    Holder* self = _c->self;
    return self->f_Count;
}

static void _envdtor_1(void* _e)
{
    struct _cap_1* _c = (struct _cap_1*)_e;
    CS2SX_ReleaseLater((void*)_c->self, (CS2SX_FreeFn)Holder_Free);
}

static void _envgc_1(void* _e, CS2SX_GcVisit visit)
{
    struct _cap_1* _c = (struct _cap_1*)_e;
    if (visit) visit((void*)_c->self, &Holder_GcKind);
    else if (_c->self) { void* _o = (void*)_c->self; _c->self = NULL; ((CS2SX_FreeFn)Holder_Free)(_o); }
    (void)_c; (void)visit;
}

void Holder_Wire(Holder* self)
{
    CS2SX_TRACE("Holder.Wire", "Program.cs");
    CS2SX_LINE(50);
    struct _cap_0* _env_0 = CS2SX_ENV_NEW(struct _cap_0);
    _env_0->self = self; ((void* (*)(void*))Holder_Retain)((void*)_env_0->self);
    _env_0->_hdr._dtor = _envdtor_0;
    _env_0->_hdr._gc = _envgc_0;
    CS2SX_RC_STORE(CS2SX_Delegate_Retain, CS2SX_Delegate_Release, self->f_Changed, CS2SX_Delegate_Combine(self->f_Changed, CS2SX_Delegate_New((void*)_lambda_0, _env_0, 1)));
    CS2SX_LINE(50);
    struct _cap_1* _env_1 = CS2SX_ENV_NEW(struct _cap_1);
    _env_1->self = self; ((void* (*)(void*))Holder_Retain)((void*)_env_1->self);
    _env_1->_hdr._dtor = _envdtor_1;
    _env_1->_hdr._gc = _envgc_1;
    CS2SX_RC_STORE(CS2SX_Delegate_Retain, CS2SX_Delegate_Release, self->f_Getter, CS2SX_Delegate_New((void*)_lambda_1, _env_1, 1));
}

void Holder_Fire(Holder* self)
{
    CS2SX_TRACE("Holder.Fire", "Program.cs");
    CS2SX_LINE(51);
    (self->f_Changed != NULL ? Action_Invoke(self->f_Changed) : (void)0);
}

const CS2SX_ClassInfo Holder_ClassInfo = { "Holder", NULL };

void Holder_Gc(void* _self, CS2SX_GcVisit visit)
{
    Holder* self = (Holder*)_self; (void)self;
    if (visit) visit((void*)self->f_Getter, &CS2SX_Delegate_GcKind);
    else if (self->f_Getter) { void* _o = (void*)self->f_Getter; self->f_Getter = NULL; ((void (*)(void*))CS2SX_Delegate_Release)(_o); }
    if (visit) visit((void*)self->f_Thing, &CS2SX_Iface_GcKind);
    else if (self->f_Thing) { void* _o = (void*)self->f_Thing; self->f_Thing = NULL; ((void (*)(void*))CS2SX_Iface_Free)(_o); }
    if (visit) visit((void*)self->f_Calls, &List_Func_int_t_GcKind);
    else if (self->f_Calls) { void* _o = (void*)self->f_Calls; self->f_Calls = NULL; ((void (*)(void*))List_Func_int_t_Free)(_o); }
    if (visit) visit((void*)self->f_Things, &List_IThing_GcKind);
    else if (self->f_Things) { void* _o = (void*)self->f_Things; self->f_Things = NULL; ((void (*)(void*))List_IThing_Free)(_o); }
    if (visit) visit((void*)self->f_Changed, &CS2SX_Delegate_GcKind);
    else if (self->f_Changed) { void* _o = (void*)self->f_Changed; self->f_Changed = NULL; ((void (*)(void*))CS2SX_Delegate_Release)(_o); }
    Slot_Gc(&self->f_Slot, visit);
    _Tuple2_Item_int_Gc(&self->f_Pair, visit);
}

static void Holder_GcDyn(void* self, CS2SX_GcVisit visit) { ((Holder*)self)->_gc(self, visit); }
const CS2SX_GcKind Holder_GcKind = { offsetof(Holder, _rc), Holder_GcDyn, (CS2SX_FreeFn)Holder_Free };

void Holder__Destroy(Holder* self)
{
    CS2SX_LINE(48);
    Holder_Alive--;
    if (self->f_Getter) { CS2SX_Delegate_Release(self->f_Getter); self->f_Getter = NULL; }
    if (self->f_Thing) { CS2SX_Iface_Free(self->f_Thing); self->f_Thing = NULL; }
    Slot_Drop(&self->f_Slot);
    _Tuple2_Item_int_Drop(&self->f_Pair);
    if (self->f_Calls) { List_Func_int_t_Free(self->f_Calls); self->f_Calls = NULL; }
    if (self->f_Things) { List_IThing_Free(self->f_Things); self->f_Things = NULL; }
    if (self->f_Slots) { List_Slot_Free(self->f_Slots); self->f_Slots = NULL; }
    if (self->f_Changed) { CS2SX_Delegate_Release(self->f_Changed); self->f_Changed = NULL; }
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

void HeldValuesApp_Init(HeldValuesApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))HeldValuesApp_OnFrame;
    self->f_h = CS2SX_RETAIN(Holder, Holder_New());
}

struct _cap_2
{
    CS2SX_Env _hdr;
    Item* it;
};

static int _lambda_2(void* _env)
{
    CS2SX_TRACE("HeldValuesApp.<lambda>", "Program.cs");
    struct _cap_2* _c = (struct _cap_2*)_env;
    Item* it = _c->it;
    return it->f_V;
}

static void _envdtor_2(void* _e)
{
    struct _cap_2* _c = (struct _cap_2*)_e;
    CS2SX_ReleaseLater((void*)_c->it, (CS2SX_FreeFn)Item_Free);
}

static void _envgc_2(void* _e, CS2SX_GcVisit visit)
{
    struct _cap_2* _c = (struct _cap_2*)_e;
    if (visit) visit((void*)_c->it, &Item_GcKind);
    else if (_c->it) { void* _o = (void*)_c->it; _c->it = NULL; ((CS2SX_FreeFn)Item_Free)(_o); }
    (void)_c; (void)visit;
}

void HeldValuesApp_Churn(HeldValuesApp* self)
{
    CS2SX_TRACE("HeldValuesApp.Churn", "Program.cs");
    CS2SX_LINE(63);
    Item* it = Item_New();
    CS2SX_LINE(64);
    CS2SX_RC_STORE(CS2SX_Delegate_Retain, CS2SX_Delegate_Release, CS2SX_NN(self->f_h)->f_Getter, CS2SX_Delegate_NewBound((void*)Item_Get, it, (void* (*)(void*))Item_Retain, (CS2SX_FreeFn)Item_Free, &Item_GcKind));
    CS2SX_LINE(65);
    CS2SX_RC_STORE(CS2SX_Iface_Retain, CS2SX_Iface_Free, CS2SX_NN(self->f_h)->f_Thing, Thing_as_IThing(Thing_New()));
    CS2SX_LINE(66);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "s%d", self->f_frame);
    ({ Slot* _cs2sx_sp = &(CS2SX_NN(self->f_h)->f_Slot); Slot _cs2sx_sn = (Slot){ .Held = Item_New(), .Label = _sb0, .N = self->f_frame }; Slot_Keep(&_cs2sx_sn); Slot _cs2sx_so = *_cs2sx_sp; *_cs2sx_sp = _cs2sx_sn; Slot_Drop(&_cs2sx_so); _cs2sx_sn; });
    CS2SX_LINE(67);
    ({ _Tuple2_Item_int* _cs2sx_sp = &(CS2SX_NN(self->f_h)->f_Pair); _Tuple2_Item_int _cs2sx_sn = (_Tuple2_Item_int){.item1 = Item_New(), .item2 = self->f_frame}; _Tuple2_Item_int_Keep(&_cs2sx_sn); _Tuple2_Item_int _cs2sx_so = *_cs2sx_sp; *_cs2sx_sp = _cs2sx_sn; _Tuple2_Item_int_Drop(&_cs2sx_so); _cs2sx_sn; });
    CS2SX_LINE(68);
    struct _cap_2* _env_2 = CS2SX_ENV_NEW(struct _cap_2);
    _env_2->it = it; ((void* (*)(void*))Item_Retain)((void*)_env_2->it);
    _env_2->_hdr._dtor = _envdtor_2;
    _env_2->_hdr._gc = _envgc_2;
    List_Func_int_t_Add(CS2SX_NN(self->f_h)->f_Calls, CS2SX_Delegate_New((void*)_lambda_2, _env_2, 1));
    CS2SX_LINE(69);
    List_IThing_Add(CS2SX_NN(self->f_h)->f_Things, Thing_as_IThing(Thing_New()));
    CS2SX_LINE(70);
    char* _sb1 = _cs2sx_next_buf();
    _sb1 = CS2SX_Format(_sb1, "l%d", self->f_frame);
    List_Slot_Add(CS2SX_NN(self->f_h)->f_Slots, (Slot){ .Held = it, .Label = _sb1 });
    CS2SX_LINE(71);
    if (CS2SX_NN(CS2SX_NN(self->f_h)->f_Calls)->count > 3)
    {
        CS2SX_LINE(71);
        List_Func_int_t_Remove(CS2SX_NN(self->f_h)->f_Calls, 0);
        CS2SX_LINE(71);
        List_IThing_Remove(CS2SX_NN(self->f_h)->f_Things, 0);
        CS2SX_LINE(71);
        List_Slot_Remove(CS2SX_NN(self->f_h)->f_Slots, 0);
    }
    CS2SX_LINE(72);
    self->f_sink += Func_int_Invoke(CS2SX_NN(self->f_h)->f_Getter) + ({ __auto_type _cs2sx_r = CS2SX_NN(CS2SX_NN(self->f_h)->f_Thing); _cs2sx_r->vtable->Value(_cs2sx_r->obj); }) + Func_int_Invoke(List_Func_int_t_Get(CS2SX_NN(self->f_h)->f_Calls, 0)) + ({ __auto_type _cs2sx_r = CS2SX_NN(List_IThing_Get(CS2SX_NN(self->f_h)->f_Things, 0)); _cs2sx_r->vtable->Value(_cs2sx_r->obj); }) + CS2SX_NN(self->f_h)->f_Slot.N - self->f_frame + CS2SX_NN(self->f_h)->f_Pair.item2 - self->f_frame;
    CS2SX_LINE(74);
    Holder* loose = Holder_New();
    CS2SX_LINE(75);
    Holder_Wire(loose);
    CS2SX_LINE(76);
    Holder_Fire(loose);
    CS2SX_LINE(77);
    self->f_sink += Func_int_Invoke(loose->f_Getter);
}

void HeldValuesApp_OnFrame(HeldValuesApp* self)
{
    CS2SX_TRACE("HeldValuesApp.OnFrame", "Program.cs");
    CS2SX_LINE(82);
    self->f_frame++;
    CS2SX_LINE(83);
    if (self->f_frame > 2)
    {
        CS2SX_LINE(83);
        HeldValuesApp_Churn(self);
    }
    CS2SX_LINE(84);
    if (self->f_frame == 3)
    {
        CS2SX_LINE(85);
        char* _sb2 = _cs2sx_next_buf();
        _sb2 = CS2SX_Format(_sb2, "first %d %s %s", self->f_sink, CS2SX_NN(self->f_h)->f_Slot.Label, List_Slot_Get(CS2SX_NN(self->f_h)->f_Slots, 0).Label);
        const char* _ps_0 = _sb2;
        printf("%s\n", _ps_0 ? _ps_0 : "");
    }
    CS2SX_LINE(86);
    if (self->f_frame == 70)
    {
        CS2SX_LINE(86);
        self->f_base = CS2SX_HeapUsed();
    }
    CS2SX_LINE(87);
    if (self->f_frame == 70 + 256)
    {
        CS2SX_LINE(89);
        long long grown = CS2SX_HeapUsed() - self->f_base;
        CS2SX_LINE(90);
        char* _sb3 = _cs2sx_next_buf();
        _sb3 = CS2SX_Format(_sb3, "stable %s %s %s %s", CS2SX_BoolStr((grown < 8192)), CS2SX_BoolStr((Item_Alive < 40)), CS2SX_BoolStr((Thing_Alive < 10)), CS2SX_BoolStr((Holder_Alive < 40)));
        const char* _ps_1 = _sb3;
        printf("%s\n", _ps_1 ? _ps_1 : "");
        CS2SX_LINE(92);
        printf("done\n");
    }
}

void HeldValuesApp_Free(HeldValuesApp* self)
{
    if (!self) return;
    if (self->f_h) { Holder_Free(self->f_h); self->f_h = NULL; }
    free(self);
}
