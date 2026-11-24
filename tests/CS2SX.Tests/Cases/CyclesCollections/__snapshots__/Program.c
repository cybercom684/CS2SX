#include <stdlib.h>
#include "_generics.h"
#include "Program.h"

int Hub_Alive;
void Hub_Ctor(Hub* self)
{
    CS2SX_TRACE("Hub..ctor", "Program.cs");
    self->f_ByName = Dict_str_Hub_Retain(Dict_str_Hub_New());
    self->f_History = Stack_Hub_Retain(Stack_Hub_New());
    self->f_Pending = Queue_Hub_Retain(Queue_Hub_New());
    self->f_Seen = HashSet_Hub_Retain(HashSet_Hub_New());
    CS2SX_LINE(15);
    Hub_Alive++;
}

Hub* Hub_New(void)
{
    Hub* self = (Hub*)calloc(1, sizeof(Hub));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Hub__Destroy;
    self->_gc = Hub_Gc;
    self->_class = &Hub_ClassInfo;
    Hub_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Hub_Free);
    return self;
}

Hub* Hub_Child(Hub* self)
{
    CS2SX_TRACE("Hub.Child", "Program.cs");
    CS2SX_LINE(20);
    Hub* c = Hub_New();
    CS2SX_LINE(21);
    CS2SX_ARC_STORE(Hub, c->f_Owner, CS2SX_RETAIN(Hub, self));
    CS2SX_LINE(22);
    return c;
}

const CS2SX_ClassInfo Hub_ClassInfo = { "Hub", NULL, NULL };

void Hub_Gc(void* _self, CS2SX_GcVisit visit)
{
    Hub* self = (Hub*)_self; (void)self;
    if (visit) visit((void*)self->f_Owner, &Hub_GcKind);
    else if (self->f_Owner) { void* _o = (void*)self->f_Owner; self->f_Owner = NULL; ((void (*)(void*))Hub_Free)(_o); }
    if (visit) visit((void*)self->f_ByName, &Dict_str_Hub_GcKind);
    else if (self->f_ByName) { void* _o = (void*)self->f_ByName; self->f_ByName = NULL; ((void (*)(void*))Dict_str_Hub_Free)(_o); }
    if (visit) visit((void*)self->f_History, &Stack_Hub_GcKind);
    else if (self->f_History) { void* _o = (void*)self->f_History; self->f_History = NULL; ((void (*)(void*))Stack_Hub_Free)(_o); }
    if (visit) visit((void*)self->f_Pending, &Queue_Hub_GcKind);
    else if (self->f_Pending) { void* _o = (void*)self->f_Pending; self->f_Pending = NULL; ((void (*)(void*))Queue_Hub_Free)(_o); }
    if (visit) visit((void*)self->f_Seen, &HashSet_Hub_GcKind);
    else if (self->f_Seen) { void* _o = (void*)self->f_Seen; self->f_Seen = NULL; ((void (*)(void*))HashSet_Hub_Free)(_o); }
}

static void Hub_GcDyn(void* self, CS2SX_GcVisit visit) { ((Hub*)self)->_gc(self, visit); }
const CS2SX_GcKind Hub_GcKind = { offsetof(Hub, _rc), Hub_GcDyn, (CS2SX_FreeFn)Hub_Free };

void Hub__Destroy(Hub* self)
{
    CS2SX_LINE(16);
    Hub_Alive--;
    if (self->f_Owner) { Hub_Free(self->f_Owner); self->f_Owner = NULL; }
    if (self->f_ByName) { Dict_str_Hub_Free(self->f_ByName); self->f_ByName = NULL; }
    if (self->f_History) { Stack_Hub_Free(self->f_History); self->f_History = NULL; }
    if (self->f_Pending) { Queue_Hub_Free(self->f_Pending); self->f_Pending = NULL; }
    if (self->f_Seen) { HashSet_Hub_Free(self->f_Seen); self->f_Seen = NULL; }
}

void Hub_Free(Hub* self)
{
    if (!self) return;
    Hub* _o = (Hub*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Hub_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Hub* Hub_Retain(Hub* self)
{
    if (self) ((Hub*)self)->_rc++;
    return self;
}

void CyclesCollectionsApp_Init(CyclesCollectionsApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))CyclesCollectionsApp_OnFrame;
}

Hub* CyclesCollectionsApp_Build()
{
    CS2SX_TRACE("CyclesCollectionsApp.Build", "Program.cs");
    CS2SX_LINE(34);
    Hub* hub = Hub_New();
    CS2SX_LINE(35);
    Dict_str_Hub_Set(hub->f_ByName, "c", Hub_Child(hub));
    CS2SX_LINE(36);
    Stack_Hub_Push(hub->f_History, Hub_Child(hub));
    CS2SX_LINE(37);
    Queue_Hub_Enqueue(hub->f_Pending, Hub_Child(hub));
    CS2SX_LINE(38);
    Queue_Hub_Enqueue(hub->f_Pending, Hub_Child(hub));
    CS2SX_LINE(39);
    Queue_Hub_Dequeue(hub->f_Pending);
    CS2SX_LINE(40);
    HashSet_Hub_Add(hub->f_Seen, Hub_Child(hub));
    CS2SX_LINE(41);
    return hub;
}

void CyclesCollectionsApp_OnFrame(CyclesCollectionsApp* self)
{
    CS2SX_TRACE("CyclesCollectionsApp.OnFrame", "Program.cs");
    CS2SX_LINE(46);
    self->f_frame++;
    CS2SX_LINE(47);
    if (self->f_frame > 60)
    {
        CS2SX_LINE(47);
        CyclesCollectionsApp_Build();
    }
    CS2SX_LINE(49);
    switch (self->f_frame)
    {
        case 1:
            CS2SX_LINE(52);
            CS2SX_ARC_STORE(Hub, self->f_kept, CS2SX_RETAIN(Hub, CyclesCollectionsApp_Build()));
            CS2SX_LINE(53);
            CyclesCollectionsApp_Build();
            CS2SX_LINE(54);
            char* _sb0 = _cs2sx_next_buf();
            _sb0 = CS2SX_Format(_sb0, "built %d", Hub_Alive);
            const char* _ps_0 = _sb0;
            printf("%s\n", _ps_0 ? _ps_0 : "");
            CS2SX_LINE(55);
            break;
        case 40:
            CS2SX_LINE(58);
            char* _sb1 = _cs2sx_next_buf();
            _sb1 = CS2SX_Format(_sb1, "kept %d %s %d%d%d", Hub_Alive, CS2SX_BoolStr((CS2SX_NN(Dict_str_Hub_Get(CS2SX_NN(self->f_kept)->f_ByName, "c"))->f_Owner == self->f_kept)), CS2SX_NN(CS2SX_NN(self->f_kept)->f_History)->count, CS2SX_NN(CS2SX_NN(self->f_kept)->f_Pending)->count, CS2SX_NN(CS2SX_NN(self->f_kept)->f_Seen)->count);
            const char* _ps_1 = _sb1;
            printf("%s\n", _ps_1 ? _ps_1 : "");
            CS2SX_LINE(60);
            CS2SX_ARC_STORE(Hub, self->f_kept, NULL);
            CS2SX_LINE(61);
            break;
        case 60:
            CS2SX_LINE(63);
            char* _sb2 = _cs2sx_next_buf();
            _sb2 = CS2SX_Format(_sb2, "dropped %d", Hub_Alive);
            const char* _ps_2 = _sb2;
            printf("%s\n", _ps_2 ? _ps_2 : "");
            CS2SX_LINE(64);
            break;
        case 92:
            CS2SX_LINE(66);
            self->f_base = CS2SX_HeapUsed();
            CS2SX_LINE(67);
            break;
        case 300:
            CS2SX_LINE(69);
            long long grown = CS2SX_HeapUsed() - self->f_base;
            CS2SX_LINE(70);
            char* _sb3 = _cs2sx_next_buf();
            _sb3 = CS2SX_Format(_sb3, "stable %s %s", CS2SX_BoolStr((grown < 8192)), CS2SX_BoolStr((Hub_Alive < 300)));
            const char* _ps_3 = _sb3;
            printf("%s\n", _ps_3 ? _ps_3 : "");
            CS2SX_LINE(71);
            printf("done\n");
            CS2SX_LINE(72);
            break;
    }
}

void CyclesCollectionsApp_Free(CyclesCollectionsApp* self)
{
    if (!self) return;
    if (self->f_kept) { Hub_Free(self->f_kept); self->f_kept = NULL; }
    free(self);
}
