#include <stdlib.h>
#include "_generics.h"
#include "Program.h"

int Counter_Disposed;
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

void Node_Ctor(Node* self, int v)
{
    CS2SX_TRACE("Node..ctor", "Program.cs");
    CS2SX_LINE(15);
    self->f_V = v;
}

Node* Node_New(int v)
{
    Node* self = (Node*)calloc(1, sizeof(Node));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Node_Dispose;
    self->_gc = Node_Gc;
    Node_Ctor(self, v);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Node_Free);
    return self;
}

void Node_Gc(void* _self, CS2SX_GcVisit visit)
{
    Node* self = (Node*)_self; (void)self;
}

static void Node_GcDyn(void* self, CS2SX_GcVisit visit) { ((Node*)self)->_gc(self, visit); }
const CS2SX_GcKind Node_GcKind = { offsetof(Node, _rc), Node_GcDyn, (CS2SX_FreeFn)Node_Free };

void Node_Dispose(Node* self)
{
    CS2SX_LINE(16);
    Counter_Disposed++;
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

void Shape_Ctor(Shape* self)
{
    CS2SX_TRACE("Shape..ctor", "Program.cs");
}

Shape* Shape_New(void)
{
    Shape* self = (Shape*)calloc(1, sizeof(Shape));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Shape_Dispose;
    self->_gc = Shape_Gc;
    Shape_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Shape_Free);
    return self;
}

void Shape_Gc(void* _self, CS2SX_GcVisit visit)
{
    Shape* self = (Shape*)_self; (void)self;
}

static void Shape_GcDyn(void* self, CS2SX_GcVisit visit) { ((Shape*)self)->_gc(self, visit); }
const CS2SX_GcKind Shape_GcKind = { offsetof(Shape, _rc), Shape_GcDyn, (CS2SX_FreeFn)Shape_Free };

void Shape_Dispose(Shape* self)
{
    CS2SX_LINE(21);
    Counter_Disposed += 10;
}

void Shape_Free(Shape* self)
{
    if (!self) return;
    Shape* _o = (Shape*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Shape_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Shape* Shape_Retain(Shape* self)
{
    if (self) ((Shape*)self)->_rc++;
    return self;
}

void Circle_Ctor(Circle* self)
{
    CS2SX_TRACE("Circle..ctor", "Program.cs");
    self->f_Center = CS2SX_RETAIN(Node, Node_New(7));
    Shape_Ctor((Shape*)self);
}

Circle* Circle_New(void)
{
    Circle* self = (Circle*)calloc(1, sizeof(Circle));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Circle_Dispose;
    self->base._gc = Circle_Gc;
    Circle_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Circle_Free);
    return self;
}

void Circle_Gc(void* _self, CS2SX_GcVisit visit)
{
    Circle* self = (Circle*)_self; (void)self;
    if (visit) visit(self->f_Center, &Node_GcKind);
    else if (self->f_Center) { void* _o = self->f_Center; self->f_Center = NULL; ((void (*)(void*))Node_Free)(_o); }
    Shape_Gc(_self, visit);
}

void Circle_Dispose(Circle* self)
{
    CS2SX_LINE(27);
    Counter_Disposed += 100;
    if (self->f_Center) { Node_Free(self->f_Center); self->f_Center = NULL; }
    Shape_Dispose((Shape*)self);
}

void Circle_Free(Circle* self)
{
    if (!self) return;
    Shape* _o = (Shape*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Shape_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Circle* Circle_Retain(Circle* self)
{
    if (self) ((Shape*)self)->_rc++;
    return self;
}

void Holder_Ctor(Holder* self)
{
    CS2SX_TRACE("Holder..ctor", "Program.cs");
}

Holder* Holder_New(void)
{
    Holder* self = (Holder*)calloc(1, sizeof(Holder));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Holder_Dispose;
    self->_gc = Holder_Gc;
    Holder_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Holder_Free);
    return self;
}

void Holder_Gc(void* _self, CS2SX_GcVisit visit)
{
    Holder* self = (Holder*)_self; (void)self;
    if (visit) visit(self->f_Kept, &Node_GcKind);
    else if (self->f_Kept) { void* _o = self->f_Kept; self->f_Kept = NULL; ((void (*)(void*))Node_Free)(_o); }
}

static void Holder_GcDyn(void* self, CS2SX_GcVisit visit) { ((Holder*)self)->_gc(self, visit); }
const CS2SX_GcKind Holder_GcKind = { offsetof(Holder, _rc), Holder_GcDyn, (CS2SX_FreeFn)Holder_Free };

void Holder_Dispose(Holder* self)
{
    if (self->f_Kept) { Node_Free(self->f_Kept); self->f_Kept = NULL; }
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

Pair* Pair_New(Node* First, int Weight)
{
    Pair* self = (Pair*)calloc(1, sizeof(Pair));
    if (!self) return NULL;
    self->_rc = 1;
    self->f_First = CS2SX_RETAIN(Node, First);
    self->f_Weight = Weight;
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Pair_Free);
    return self;
}

void Pair_Free(Pair* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    if (self->f_First) { Node_Free(self->f_First); self->f_First = NULL; }
    free(self);
}

Pair* Pair_Retain(Pair* self)
{
    if (self) self->_rc++;
    return self;
}

const CS2SX_GcKind Pair_GcKind = { offsetof(Pair, _rc), CS2SX_GcLeaf, (CS2SX_FreeFn)Pair_Free };

void RefCountingApp_Init(RefCountingApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))RefCountingApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))RefCountingApp_OnFrame;
    self->f_nodes = List_Node_Retain(List_Node_New());
    self->f_shapes = List_Shape_Retain(List_Shape_New());
}

void RefCountingApp_OnInit(RefCountingApp* self)
{
    CS2SX_TRACE("RefCountingApp.OnInit", "Program.cs");
    CS2SX_LINE(49);
    List_Node_Add(self->f_nodes, Node_New(1));
    CS2SX_LINE(50);
    List_Node_Remove(self->f_nodes, 0);
    CS2SX_LINE(51);
    List_Node_Add(self->f_nodes, Node_New(2));
    CS2SX_LINE(52);
    Node* borrowed = List_Node_Get(self->f_nodes, 0);
    CS2SX_LINE(53);
    List_Node_Remove(self->f_nodes, 0);
    CS2SX_LINE(54);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "0 same-frame %d %d", Counter_Disposed, CS2SX_NN(borrowed)->f_V);
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
}

void RefCountingApp_OnFrame(RefCountingApp* self)
{
    CS2SX_TRACE("RefCountingApp.OnFrame", "Program.cs");
    CS2SX_LINE(59);
    self->f_frame++;
    CS2SX_LINE(60);
    if (self->f_frame > 7)
    {
        CS2SX_LINE(60);
        return;
    }
    CS2SX_LINE(61);
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "%d %d", self->f_frame, Counter_Disposed);
    const char* _ps_1 = _sb1;
    printf("%s", _ps_1 ? _ps_1 : "");
    CS2SX_LINE(63);
    switch (self->f_frame)
    {
        case 1:
            CS2SX_LINE(66);
            printf(" (2)\n");
            CS2SX_LINE(67);
            List_Node_Add(self->f_nodes, Node_New(3));
            CS2SX_LINE(68);
            CS2SX_ARC_STORE(Node, self->f_kept, CS2SX_RETAIN(Node, List_Node_Get(self->f_nodes, 0)));
            CS2SX_LINE(69);
            List_Node_Remove(self->f_nodes, 0);
            CS2SX_LINE(70);
            break;
        case 2:
            CS2SX_LINE(72);
            char* _sb2 = _cs2sx_next_buf();
            snprintf(_sb2, CS2SX_STRBUF_SIZE, " (2) kept %d", CS2SX_NN(self->f_kept)->f_V);
            const char* _ps_2 = _sb2;
            printf("%s\n", _ps_2 ? _ps_2 : "");
            CS2SX_LINE(73);
            CS2SX_ARC_STORE(Node, self->f_kept, CS2SX_RETAIN(Node, Node_New(4)));
            CS2SX_LINE(74);
            break;
        case 3:
            CS2SX_LINE(76);
            printf(" (3)\n");
            CS2SX_LINE(77);
            CS2SX_ARC_STORE(Node, self->f_kept, CS2SX_RETAIN(Node, self->f_kept));
            CS2SX_LINE(78);
            break;
        case 4:
            CS2SX_LINE(80);
            char* _sb3 = _cs2sx_next_buf();
            snprintf(_sb3, CS2SX_STRBUF_SIZE, " (3) kept %d", CS2SX_NN(self->f_kept)->f_V);
            const char* _ps_3 = _sb3;
            printf("%s\n", _ps_3 ? _ps_3 : "");
            CS2SX_LINE(81);
            CS2SX_ARC_STORE(Node, self->f_kept, NULL);
            CS2SX_LINE(82);
            break;
        case 5:
            CS2SX_LINE(84);
            printf(" (4)\n");
            CS2SX_LINE(85);
            List_Shape_Add(self->f_shapes, (Shape*)((Shape*)Circle_New()));
            CS2SX_LINE(86);
            List_Shape_Clear(self->f_shapes);
            CS2SX_LINE(87);
            break;
        case 6:
            CS2SX_LINE(89);
            printf(" (115)\n");
            CS2SX_LINE(90);
            Holder* _holder_4 = Holder_New();
            CS2SX_ARC_STORE(Node, _holder_4->f_Kept, CS2SX_RETAIN(Node, Node_New(5)));
            CS2SX_ARC_STORE(Holder, self->f_holder, CS2SX_RETAIN(Holder, _holder_4));
            CS2SX_LINE(91);
            CS2SX_ARC_STORE(Pair, self->f_pair, CS2SX_RETAIN(Pair, Pair_New(CS2SX_NN(self->f_holder)->f_Kept, 1)));
            CS2SX_LINE(92);
            CS2SX_ARC_STORE(Holder, self->f_holder, NULL);
            CS2SX_LINE(93);
            break;
        case 7:
            CS2SX_LINE(95);
            char* _sb4 = _cs2sx_next_buf();
            snprintf(_sb4, CS2SX_STRBUF_SIZE, " (115) pair %d", CS2SX_NN(CS2SX_NN(self->f_pair)->f_First)->f_V);
            const char* _ps_5 = _sb4;
            printf("%s\n", _ps_5 ? _ps_5 : "");
            CS2SX_LINE(96);
            break;
    }
}

void RefCountingApp_Free(RefCountingApp* self)
{
    if (!self) return;
    if (self->f_kept) { Node_Free(self->f_kept); self->f_kept = NULL; }
    if (self->f_holder) { Holder_Free(self->f_holder); self->f_holder = NULL; }
    if (self->f_pair) { Pair_Free(self->f_pair); self->f_pair = NULL; }
    free(self);
}
