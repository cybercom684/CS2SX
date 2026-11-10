#include <stdlib.h>
#include "_generics.h"
#include "_interfaces.h"
#include "Program.h"

int Counter_Freed;
void Counter_Ctor(Counter* self)
{
    CS2SX_TRACE("Counter..ctor", "Program.cs");
}

Counter* Counter_New(void)
{
    Counter* self = (Counter*)calloc(1, sizeof(Counter));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Counter_Dispose;
    self->_gc = Counter_Gc;
    Counter_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Counter_Free);
    return self;
}

void Counter_Gc(void* _self, CS2SX_GcVisit visit)
{
    Counter* self = (Counter*)_self; (void)self;
}

static void Counter_GcDyn(void* self, CS2SX_GcVisit visit) { ((Counter*)self)->_gc(self, visit); }
const CS2SX_GcKind Counter_GcKind = { offsetof(Counter, _rc), Counter_GcDyn, (CS2SX_FreeFn)Counter_Free };

void Counter_Dispose(Counter* self)
{
}

void Counter_Free(Counter* self)
{
    if (!self) return;
    Counter* _o = (Counter*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Counter_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Counter* Counter_Retain(Counter* self)
{
    if (self) ((Counter*)self)->_rc++;
    return self;
}

void Item_Ctor(Item* self, int v)
{
    CS2SX_TRACE("Item..ctor", "Program.cs");
    CS2SX_LINE(15);
    self->f_V = v;
}

Item* Item_New(int v)
{
    Item* self = (Item*)calloc(1, sizeof(Item));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Item_Dispose;
    self->_gc = Item_Gc;
    Item_Ctor(self, v);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Item_Free);
    return self;
}

void Item_Gc(void* _self, CS2SX_GcVisit visit)
{
    Item* self = (Item*)_self; (void)self;
}

static void Item_GcDyn(void* self, CS2SX_GcVisit visit) { ((Item*)self)->_gc(self, visit); }
const CS2SX_GcKind Item_GcKind = { offsetof(Item, _rc), Item_GcDyn, (CS2SX_FreeFn)Item_Free };

void Item_Dispose(Item* self)
{
    CS2SX_LINE(16);
    Counter_Freed++;
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

void Wrapped_Ctor(Wrapped* self, int v)
{
    CS2SX_TRACE("Wrapped..ctor", "Program.cs");
    CS2SX_LINE(24);
    self->f_V = v;
}

Wrapped* Wrapped_New(int v)
{
    Wrapped* self = (Wrapped*)calloc(1, sizeof(Wrapped));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Wrapped_Dispose;
    self->_gc = Wrapped_Gc;
    Wrapped_Ctor(self, v);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Wrapped_Free);
    return self;
}

int Wrapped_Get(Wrapped* self)
{
    CS2SX_TRACE("Wrapped.Get", "Program.cs");
    CS2SX_LINE(25);
    return self->f_V;
}

void Wrapped_Gc(void* _self, CS2SX_GcVisit visit)
{
    Wrapped* self = (Wrapped*)_self; (void)self;
}

static void Wrapped_GcDyn(void* self, CS2SX_GcVisit visit) { ((Wrapped*)self)->_gc(self, visit); }
const CS2SX_GcKind Wrapped_GcKind = { offsetof(Wrapped, _rc), Wrapped_GcDyn, (CS2SX_FreeFn)Wrapped_Free };

void Wrapped_Dispose(Wrapped* self)
{
    CS2SX_LINE(26);
    Counter_Freed += 100;
}

void Wrapped_Free(Wrapped* self)
{
    if (!self) return;
    Wrapped* _o = (Wrapped*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Wrapped_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Wrapped* Wrapped_Retain(Wrapped* self)
{
    if (self) ((Wrapped*)self)->_rc++;
    return self;
}

IHas_vtable Wrapped_IHas_vtable_instance =
{
    .Get = (int (*)(void*))Wrapped_Get,
};


// ── Struct: Pair ────────────────────────────────────────
int Pair_Equals(Pair a, Pair b)
{
    return memcmp(&a, &b, sizeof(Pair)) == 0;
}

void FrameMemoryApp_Init(FrameMemoryApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))FrameMemoryApp_OnFrame;
    self->f_dict = Dict_str_Item_Retain(Dict_str_Item_New());
    self->f_stack = Stack_Item_Retain(Stack_Item_New());
    self->f_queue = Queue_Item_Retain(Queue_Item_New());
    self->f_set = HashSet_Item_Retain(HashSet_Item_New());
    self->f_arr = (Item**)CS2SX_ArrNew(1, sizeof(Item*));
    self->f_box = Box_Item_New(Item_New(15));
}

int FrameMemoryApp_Garbage()
{
    CS2SX_TRACE("FrameMemoryApp.Garbage", "Program.cs");
    CS2SX_LINE(56);
    int sum = 0;
    CS2SX_LINE(57);
    for (int i = 0; i < 5; i++)
    {
        CS2SX_LINE(59);
        Item* t = Item_New(i);
        CS2SX_LINE(60);
        sum += t->f_V;
    }
    CS2SX_LINE(62);
    return sum;
}

struct _cap_0
{
    CS2SX_Env _hdr;
    Item* cap;
};

static int _lambda_0(void* _env)
{
    CS2SX_TRACE("FrameMemoryApp.<lambda>", "Program.cs");
    struct _cap_0* _c = (struct _cap_0*)_env;
    Item* cap = _c->cap;
    return cap->f_V;
}

static void _envdtor_0(void* _e)
{
    struct _cap_0* _c = (struct _cap_0*)_e;
    CS2SX_ReleaseLater(_c->cap, (CS2SX_FreeFn)Item_Free);
}

void FrameMemoryApp_OnFrame(FrameMemoryApp* self)
{
    CS2SX_TRACE("FrameMemoryApp.OnFrame", "Program.cs");
    CS2SX_LINE(67);
    self->f_frame++;
    CS2SX_LINE(68);
    switch (self->f_frame)
    {
        case 1:
            CS2SX_LINE(71);
            FrameMemoryApp_Garbage();
            CS2SX_LINE(72);
            char* _sb0 = _cs2sx_next_buf();
            snprintf(_sb0, CS2SX_STRBUF_SIZE, "k%d", self->f_frame);
            Dict_str_Item_Set(self->f_dict, _sb0, Item_New(10));
            CS2SX_LINE(73);
            Stack_Item_Push(self->f_stack, Item_New(11));
            CS2SX_LINE(74);
            Queue_Item_Enqueue(self->f_queue, Item_New(12));
            CS2SX_LINE(75);
            HashSet_Item_Add(self->f_set, Item_New(13));
            CS2SX_LINE(76);
            CS2SX_ARC_STORE(Item, CS2SX_AT(self->f_arr, 0), CS2SX_RETAIN(Item, Item_New(14)));
            CS2SX_LINE(77);
            self->f_pair.A = CS2SX_RETAIN(Item, Item_New(16));
            CS2SX_LINE(78);
            self->f_iface = Wrapped_as_IHas(CS2SX_RETAIN(Wrapped, Wrapped_New(17)));
            CS2SX_LINE(79);
            Item* cap = Item_New(18);
            CS2SX_LINE(80);
            struct _cap_0* _env_0 = CS2SX_ENV_NEW(struct _cap_0);
            _env_0->cap = CS2SX_RETAIN(Item, cap);
            _env_0->_hdr._dtor = _envdtor_0;
            self->f_fn = CS2SX_Delegate_New((void*)_lambda_0, _env_0, 1);
            CS2SX_LINE(81);
            CS2SX_ARC_STORE(Item, self->f_field, CS2SX_RETAIN(Item, Item_New(19)));
            CS2SX_LINE(82);
            break;
        case 2:
            CS2SX_LINE(84);
            char* _sb1 = _cs2sx_next_buf();
            snprintf(_sb1, CS2SX_STRBUF_SIZE, "garbage %d", Counter_Freed);
            const char* _ps_0 = _sb1;
            printf("%s\n", _ps_0 ? _ps_0 : "");
            CS2SX_LINE(85);
            break;
        case 3:
            CS2SX_LINE(87);
            char* _sb2 = _cs2sx_next_buf();
            snprintf(_sb2, CS2SX_STRBUF_SIZE, "kept %d %d %d %d %d %d %d %d %d %d %d", CS2SX_NN(Dict_str_Item_Get(self->f_dict, "k1"))->f_V, CS2SX_NN(Stack_Item_Peek(self->f_stack))->f_V, CS2SX_NN(Queue_Item_Peek(self->f_queue))->f_V, CS2SX_NN(self->f_set)->count, CS2SX_NN(CS2SX_AT(self->f_arr, 0))->f_V, CS2SX_NN(CS2SX_NN(self->f_box)->f_Value)->f_V, CS2SX_NN(self->f_pair.A)->f_V, ({ __auto_type _cs2sx_r = CS2SX_NN(self->f_iface); _cs2sx_r->vtable->Get(_cs2sx_r->obj); }), Func_int_Invoke(self->f_fn), CS2SX_NN(self->f_field)->f_V, Counter_Freed);
            const char* _ps_1 = _sb2;
            printf("%s\n", _ps_1 ? _ps_1 : "");
            CS2SX_LINE(91);
            Dict_str_Item_Remove(self->f_dict, "k1");
            CS2SX_LINE(92);
            Item* popped = Stack_Item_Pop(self->f_stack);
            CS2SX_LINE(93);
            Item* dequeued = Queue_Item_Dequeue(self->f_queue);
            CS2SX_LINE(94);
            HashSet_Item_Clear(self->f_set);
            CS2SX_LINE(95);
            CS2SX_ARC_STORE(Item, CS2SX_AT(self->f_arr, 0), NULL);
            CS2SX_LINE(96);
            CS2SX_ARC_STORE(Item, CS2SX_NN(self->f_box)->f_Value, NULL);
            CS2SX_LINE(97);
            CS2SX_ARC_STORE(Item, self->f_field, NULL);
            CS2SX_LINE(98);
            char* _sb3 = _cs2sx_next_buf();
            snprintf(_sb3, CS2SX_STRBUF_SIZE, "same frame %d %d %d", CS2SX_NN(popped)->f_V, CS2SX_NN(dequeued)->f_V, Counter_Freed);
            const char* _ps_2 = _sb3;
            printf("%s\n", _ps_2 ? _ps_2 : "");
            CS2SX_LINE(99);
            break;
        case 4:
            CS2SX_LINE(101);
            char* _sb4 = _cs2sx_next_buf();
            snprintf(_sb4, CS2SX_STRBUF_SIZE, "released %d", Counter_Freed);
            const char* _ps_3 = _sb4;
            printf("%s\n", _ps_3 ? _ps_3 : "");
            CS2SX_LINE(102);
            break;
    }
}

void FrameMemoryApp_Free(FrameMemoryApp* self)
{
    if (!self) return;
    for (int _ai = 0; _ai < CS2SX_ArrLen(self->f_arr); _ai++) Item_Free((Item*)self->f_arr[_ai]);
    CS2SX_ArrFree(self->f_arr); self->f_arr = NULL;
    if (self->f_box) { Box_Item_Free(self->f_box); self->f_box = NULL; }
    if (self->f_field) { Item_Free(self->f_field); self->f_field = NULL; }
    free(self);
}
