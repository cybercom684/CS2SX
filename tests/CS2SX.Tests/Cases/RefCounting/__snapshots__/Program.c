#include <stdlib.h>
#include "_generics.h"
#include "Program.h"

int Counter_Disposed;
void Counter_Ctor(Counter* self)
{
}

Counter* Counter_New(void)
{
    Counter* self = (Counter*)calloc(1, sizeof(Counter));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Counter_Dispose;
    Counter_Ctor(self);
    return self;
}

void Counter_Dispose(Counter* self)
{
}

void Counter_Free(Counter* self)
{
    if (!self) return;
    Counter* _o = (Counter*)self;
    if (--_o->_rc > 0) return;
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
    self->f_V = v;
}

Node* Node_New(int v)
{
    Node* self = (Node*)calloc(1, sizeof(Node));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Node_Dispose;
    Node_Ctor(self, v);
    return self;
}

void Node_Dispose(Node* self)
{
    Counter_Disposed++;
}

void Node_Free(Node* self)
{
    if (!self) return;
    Node* _o = (Node*)self;
    if (--_o->_rc > 0) return;
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
}

Shape* Shape_New(void)
{
    Shape* self = (Shape*)calloc(1, sizeof(Shape));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Shape_Dispose;
    Shape_Ctor(self);
    return self;
}

void Shape_Dispose(Shape* self)
{
    Counter_Disposed += 10;
}

void Shape_Free(Shape* self)
{
    if (!self) return;
    Shape* _o = (Shape*)self;
    if (--_o->_rc > 0) return;
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
    self->f_Center = Node_New(7);
    Shape_Ctor((Shape*)self);
}

Circle* Circle_New(void)
{
    Circle* self = (Circle*)calloc(1, sizeof(Circle));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Circle_Dispose;
    Circle_Ctor(self);
    return self;
}

void Circle_Dispose(Circle* self)
{
    Counter_Disposed += 100;
    if (self->f_Center) { Node_Free(self->f_Center); self->f_Center = NULL; }
    Shape_Dispose((Shape*)self);
}

void Circle_Free(Circle* self)
{
    if (!self) return;
    Shape* _o = (Shape*)self;
    if (--_o->_rc > 0) return;
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
}

Holder* Holder_New(void)
{
    Holder* self = (Holder*)calloc(1, sizeof(Holder));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Holder_Dispose;
    Holder_Ctor(self);
    return self;
}

void Holder_Dispose(Holder* self)
{
    if (self->f_Kept) { Node_Free(self->f_Kept); self->f_Kept = NULL; }
}

void Holder_Free(Holder* self)
{
    if (!self) return;
    Holder* _o = (Holder*)self;
    if (--_o->_rc > 0) return;
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

void RefCountingApp_Init(RefCountingApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))RefCountingApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))RefCountingApp_OnFrame;
    self->f_nodes = List_Node_New();
    self->f_shapes = List_Shape_New();
}

void RefCountingApp_OnInit(RefCountingApp* self)
{
    List_Node_AddNew(self->f_nodes, Node_New(1));
    List_Node_Remove(self->f_nodes, 0);
    List_Node_AddNew(self->f_nodes, Node_New(2));
    Node* borrowed = List_Node_Get(self->f_nodes, 0);
    List_Node_Remove(self->f_nodes, 0);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "0 same-frame %d %d", Counter_Disposed, CS2SX_NN(borrowed)->f_V);
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
}

void RefCountingApp_OnFrame(RefCountingApp* self)
{
    self->f_frame++;
    if (self->f_frame > 7)
    {
        return;
    }
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "%d %d", self->f_frame, Counter_Disposed);
    const char* _ps_1 = _sb1;
    printf("%s", _ps_1 ? _ps_1 : "");
    switch (self->f_frame)
    {
        case 1:
            printf(" (2)\n");
            List_Node_AddNew(self->f_nodes, Node_New(3));
            CS2SX_ARC_STORE(Node, self->f_kept, CS2SX_RETAIN(Node, List_Node_Get(self->f_nodes, 0)));
            List_Node_Remove(self->f_nodes, 0);
            break;
        case 2:
            char* _sb2 = _cs2sx_next_buf();
            snprintf(_sb2, CS2SX_STRBUF_SIZE, " (2) kept %d", CS2SX_NN(self->f_kept)->f_V);
            const char* _ps_2 = _sb2;
            printf("%s\n", _ps_2 ? _ps_2 : "");
            CS2SX_ARC_STORE(Node, self->f_kept, Node_New(4));
            break;
        case 3:
            printf(" (3)\n");
            CS2SX_ARC_STORE(Node, self->f_kept, CS2SX_RETAIN(Node, self->f_kept));
            break;
        case 4:
            char* _sb3 = _cs2sx_next_buf();
            snprintf(_sb3, CS2SX_STRBUF_SIZE, " (3) kept %d", CS2SX_NN(self->f_kept)->f_V);
            const char* _ps_3 = _sb3;
            printf("%s\n", _ps_3 ? _ps_3 : "");
            CS2SX_ARC_STORE(Node, self->f_kept, NULL);
            break;
        case 5:
            printf(" (4)\n");
            List_Shape_AddNew(self->f_shapes, (Shape*)(Circle_New()));
            List_Shape_Clear(self->f_shapes);
            break;
        case 6:
            printf(" (115)\n");
            Holder* _holder_4 = Holder_New();
            CS2SX_ARC_STORE(Node, _holder_4->f_Kept, Node_New(5));
            CS2SX_ARC_STORE(Holder, self->f_holder, _holder_4);
            CS2SX_ARC_STORE(Pair, self->f_pair, Pair_New(CS2SX_NN(self->f_holder)->f_Kept, 1));
            CS2SX_ARC_STORE(Holder, self->f_holder, NULL);
            break;
        case 7:
            char* _sb4 = _cs2sx_next_buf();
            snprintf(_sb4, CS2SX_STRBUF_SIZE, " (115) pair %d", CS2SX_NN(CS2SX_NN(self->f_pair)->f_First)->f_V);
            const char* _ps_5 = _sb4;
            printf("%s\n", _ps_5 ? _ps_5 : "");
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
