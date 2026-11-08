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

void RefCountingApp_Init(RefCountingApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))RefCountingApp_OnInit;
    self->f_nodes = List_Node_New();
    self->f_shapes = List_Shape_New();
}

void RefCountingApp_OnInit(RefCountingApp* self)
{
    List_Node_AddNew(self->f_nodes, Node_New(1));
    List_Node_Remove(self->f_nodes, 0);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "removed-only %d", Counter_Disposed);
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    List_Node_AddNew(self->f_nodes, Node_New(2));
    self->f_kept = CS2SX_RETAIN(Node, List_Node_Get(self->f_nodes, 0));
    List_Node_Remove(self->f_nodes, 0);
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "shared %d %d", Counter_Disposed, CS2SX_NN(self->f_kept)->f_V);
    const char* _ps_1 = _sb1;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    Holder* holder = Holder_New();
    List_Node_AddNew(self->f_nodes, Node_New(3));
    holder->f_Kept = CS2SX_RETAIN(Node, List_Node_Get(self->f_nodes, 0));
    List_Node_Clear(self->f_nodes);
    char* _sb2 = _cs2sx_next_buf();
    snprintf(_sb2, CS2SX_STRBUF_SIZE, "cleared %d %d", Counter_Disposed, CS2SX_NN(holder->f_Kept)->f_V);
    const char* _ps_2 = _sb2;
    printf("%s\n", _ps_2 ? _ps_2 : "");
    List_Node* other = List_Node_New();
    List_Node_AddNew(self->f_nodes, Node_New(4));
    List_Node_Add(other, List_Node_Get(self->f_nodes, 0));
    List_Node_Remove(self->f_nodes, 0);
    char* _sb3 = _cs2sx_next_buf();
    snprintf(_sb3, CS2SX_STRBUF_SIZE, "two-lists %d %d", Counter_Disposed, CS2SX_NN(List_Node_Get(other, 0))->f_V);
    const char* _ps_3 = _sb3;
    printf("%s\n", _ps_3 ? _ps_3 : "");
    List_Node_Remove(other, 0);
    char* _sb4 = _cs2sx_next_buf();
    snprintf(_sb4, CS2SX_STRBUF_SIZE, "last-ref %d", Counter_Disposed);
    const char* _ps_4 = _sb4;
    printf("%s\n", _ps_4 ? _ps_4 : "");
    List_Shape_AddNew(self->f_shapes, (Shape*)(Circle_New()));
    List_Shape_Remove(self->f_shapes, 0);
    char* _sb5 = _cs2sx_next_buf();
    snprintf(_sb5, CS2SX_STRBUF_SIZE, "virtual %d", Counter_Disposed);
    const char* _ps_5 = _sb5;
    printf("%s\n", _ps_5 ? _ps_5 : "");
}

void RefCountingApp_Free(RefCountingApp* self)
{
    if (!self) return;
    if (self->f_kept) { Node_Free(self->f_kept); self->f_kept = NULL; }
    free(self);
}
