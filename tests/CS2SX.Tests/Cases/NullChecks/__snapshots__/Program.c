#include <stdlib.h>
#include "_interfaces.h"
#include "Program.h"

static Node_vtable Node_vtable_instance =
{
    .Kind = (const char*(*)(void*))Node_Kind,
};

void Node_Ctor(Node* self)
{
}

Node* Node_New(void)
{
    Node* self = (Node*)calloc(1, sizeof(Node));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Node_Dispose;
    self->vtable = &Node_vtable_instance;
    Node_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Node_Free);
    return self;
}

int Node_Get(Node* self)
{
    return self->f_Value;
}

const char* Node_Kind(Node* self)
{
    return "node";
}

void Node_Dispose(Node* self)
{
    if (self->f_Next) { Node_Free(self->f_Next); self->f_Next = NULL; }
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

void Square_Ctor(Square* self)
{
}

Square* Square_New(void)
{
    Square* self = (Square*)calloc(1, sizeof(Square));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Square_Dispose;
    Square_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Square_Free);
    return self;
}

int Square_Area(Square* self)
{
    return 4;
}

void Square_Dispose(Square* self)
{
}

void Square_Free(Square* self)
{
    if (!self) return;
    Square* _o = (Square*)self;
    if (--_o->_rc > 0) return;
    _o->_dtor(_o);
    free(_o);
}

Square* Square_Retain(Square* self)
{
    if (self) ((Square*)self)->_rc++;
    return self;
}

IShape_vtable Square_IShape_vtable_instance =
{
    .Area = (int (*)(void*))Square_Area,
};


void NullChecksApp_Init(NullChecksApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))NullChecksApp_OnInit;
}

const char* NullChecksApp_Try(Action_t action)
{
    {
        CS2SX_ExFrame _exf_0;
        CS2SX_ExPush(&_exf_0);
        if (setjmp(_exf_0.buf) == 0)
        {
            Action_Invoke(action);
            {
                __auto_type _ret_1 = "none";
                CS2SX_ExPop(&_exf_0);
                return _ret_1;
            }
            CS2SX_ExPop(&_exf_0);
        }
        else
        {
            Exception* _exf_0_ex = _exf_0.ex;
            if (CS2SX_Exc_Is(_exf_0_ex, &CS2SX_TI_NullReferenceException))
            {
                return "NRE";
            }
            else if (1)
            {
                Exception* e = (Exception*)_exf_0_ex;
                return CS2SX_Exc_TypeName((Exception*)(e));
            }
        }
    }
}

struct _cap_0
{
    CS2SX_Env _hdr;
    Node* missing;
};

static void _lambda_0(void* _env)
{
    struct _cap_0* _c = (struct _cap_0*)_env;
    Node* missing = _c->missing;
    int v = CS2SX_NN(missing)->f_Value;
}

static void _envdtor_0(void* _e)
{
    struct _cap_0* _c = (struct _cap_0*)_e;
    CS2SX_ReleaseLater(_c->missing, (CS2SX_FreeFn)Node_Free);
}

struct _cap_1
{
    CS2SX_Env _hdr;
    Node* missing;
};

static void _lambda_1(void* _env)
{
    struct _cap_1* _c = (struct _cap_1*)_env;
    Node* missing = _c->missing;
    CS2SX_NN(missing)->f_Value = 5;
}

static void _envdtor_1(void* _e)
{
    struct _cap_1* _c = (struct _cap_1*)_e;
    CS2SX_ReleaseLater(_c->missing, (CS2SX_FreeFn)Node_Free);
}

struct _cap_2
{
    CS2SX_Env _hdr;
    Node* missing;
};

static void _lambda_2(void* _env)
{
    struct _cap_2* _c = (struct _cap_2*)_env;
    Node* missing = _c->missing;
    Node_Get(CS2SX_NN(missing));
}

static void _envdtor_2(void* _e)
{
    struct _cap_2* _c = (struct _cap_2*)_e;
    CS2SX_ReleaseLater(_c->missing, (CS2SX_FreeFn)Node_Free);
}

struct _cap_3
{
    CS2SX_Env _hdr;
    Node* missing;
};

static void _lambda_3(void* _env)
{
    struct _cap_3* _c = (struct _cap_3*)_env;
    Node* missing = _c->missing;
    ({ __auto_type _cs2sx_r = CS2SX_NN(missing); _cs2sx_r->vtable->Kind(_cs2sx_r); });
}

static void _envdtor_3(void* _e)
{
    struct _cap_3* _c = (struct _cap_3*)_e;
    CS2SX_ReleaseLater(_c->missing, (CS2SX_FreeFn)Node_Free);
}

struct _cap_4
{
    CS2SX_Env _hdr;
    Node* n;
};

static void _lambda_4(void* _env)
{
    struct _cap_4* _c = (struct _cap_4*)_env;
    Node* n = _c->n;
    int v = CS2SX_NN(n->f_Next)->f_Value;
}

static void _envdtor_4(void* _e)
{
    struct _cap_4* _c = (struct _cap_4*)_e;
    CS2SX_ReleaseLater(_c->n, (CS2SX_FreeFn)Node_Free);
}

struct _cap_5
{
    CS2SX_Env _hdr;
    IShape* shape;
};

static void _lambda_5(void* _env)
{
    struct _cap_5* _c = (struct _cap_5*)_env;
    IShape* shape = _c->shape;
    ({ __auto_type _cs2sx_r = CS2SX_NN(shape); _cs2sx_r->vtable->Area(_cs2sx_r->obj); });
}

struct _cap_6
{
    CS2SX_Env _hdr;
    const char* s;
};

static void _lambda_6(void* _env)
{
    struct _cap_6* _c = (struct _cap_6*)_env;
    const char* s = _c->s;
    int l = strlen(CS2SX_NN(s));
}

struct _cap_7
{
    CS2SX_Env _hdr;
    const char* s;
};

static void _lambda_7(void* _env)
{
    struct _cap_7* _c = (struct _cap_7*)_env;
    const char* s = _c->s;
    String_ToUpper(CS2SX_NN(s));
}

struct _cap_8
{
    CS2SX_Env _hdr;
    List_int* list;
};

static void _lambda_8(void* _env)
{
    struct _cap_8* _c = (struct _cap_8*)_env;
    List_int* list = _c->list;
    List_int_Add(list, 1);
}

struct _cap_9
{
    CS2SX_Env _hdr;
    List_int* list;
};

static void _lambda_9(void* _env)
{
    struct _cap_9* _c = (struct _cap_9*)_env;
    List_int* list = _c->list;
    int c = CS2SX_NN(list)->count;
}

struct _cap_10
{
    CS2SX_Env _hdr;
    Node* missing;
};

static void _lambda_10(void* _env)
{
    struct _cap_10* _c = (struct _cap_10*)_env;
    Node* missing = _c->missing;
    const char* t = (CS2SX_RETAIN(Node, missing) != NULL ? ({ char* _sb1 = _cs2sx_next_buf(); snprintf(_sb1, CS2SX_STRBUF_SIZE, "Name: %s", CS2SX_NN(missing)->f_Name); _sb1; }) : "-");
}

static void _envdtor_10(void* _e)
{
    struct _cap_10* _c = (struct _cap_10*)_e;
    CS2SX_ReleaseLater(_c->missing, (CS2SX_FreeFn)Node_Free);
}

void NullChecksApp_OnInit(NullChecksApp* self)
{
    Node* _node_2 = Node_New();
    _node_2->f_Value = 1;
    Node* n = _node_2;
    Node* missing = NULL;
    struct _cap_0* _env_0 = CS2SX_ENV_NEW(struct _cap_0);
    _env_0->missing = CS2SX_RETAIN(Node, missing);
    _env_0->_hdr._dtor = _envdtor_0;
    const char* _ps_3 = NullChecksApp_Try(CS2SX_Delegate_New((void*)_lambda_0, _env_0, 1));
    printf("%s\n", _ps_3 ? _ps_3 : "");
    struct _cap_1* _env_1 = CS2SX_ENV_NEW(struct _cap_1);
    _env_1->missing = CS2SX_RETAIN(Node, missing);
    _env_1->_hdr._dtor = _envdtor_1;
    const char* _ps_4 = NullChecksApp_Try(CS2SX_Delegate_New((void*)_lambda_1, _env_1, 1));
    printf("%s\n", _ps_4 ? _ps_4 : "");
    struct _cap_2* _env_2 = CS2SX_ENV_NEW(struct _cap_2);
    _env_2->missing = CS2SX_RETAIN(Node, missing);
    _env_2->_hdr._dtor = _envdtor_2;
    const char* _ps_5 = NullChecksApp_Try(CS2SX_Delegate_New((void*)_lambda_2, _env_2, 1));
    printf("%s\n", _ps_5 ? _ps_5 : "");
    struct _cap_3* _env_3 = CS2SX_ENV_NEW(struct _cap_3);
    _env_3->missing = CS2SX_RETAIN(Node, missing);
    _env_3->_hdr._dtor = _envdtor_3;
    const char* _ps_6 = NullChecksApp_Try(CS2SX_Delegate_New((void*)_lambda_3, _env_3, 1));
    printf("%s\n", _ps_6 ? _ps_6 : "");
    struct _cap_4* _env_4 = CS2SX_ENV_NEW(struct _cap_4);
    _env_4->n = CS2SX_RETAIN(Node, n);
    _env_4->_hdr._dtor = _envdtor_4;
    const char* _ps_7 = NullChecksApp_Try(CS2SX_Delegate_New((void*)_lambda_4, _env_4, 1));
    printf("%s\n", _ps_7 ? _ps_7 : "");
    IShape* shape = NULL;
    struct _cap_5* _env_5 = CS2SX_ENV_NEW(struct _cap_5);
    _env_5->shape = shape;
    const char* _ps_8 = NullChecksApp_Try(CS2SX_Delegate_New((void*)_lambda_5, _env_5, 1));
    printf("%s\n", _ps_8 ? _ps_8 : "");
    const char* s = NULL;
    struct _cap_6* _env_6 = CS2SX_ENV_NEW(struct _cap_6);
    _env_6->s = s;
    const char* _ps_9 = NullChecksApp_Try(CS2SX_Delegate_New((void*)_lambda_6, _env_6, 1));
    printf("%s\n", _ps_9 ? _ps_9 : "");
    struct _cap_7* _env_7 = CS2SX_ENV_NEW(struct _cap_7);
    _env_7->s = s;
    const char* _ps_10 = NullChecksApp_Try(CS2SX_Delegate_New((void*)_lambda_7, _env_7, 1));
    printf("%s\n", _ps_10 ? _ps_10 : "");
    List_int* list = NULL;
    struct _cap_8* _env_8 = CS2SX_ENV_NEW(struct _cap_8);
    _env_8->list = list;
    const char* _ps_11 = NullChecksApp_Try(CS2SX_Delegate_New((void*)_lambda_8, _env_8, 1));
    printf("%s\n", _ps_11 ? _ps_11 : "");
    struct _cap_9* _env_9 = CS2SX_ENV_NEW(struct _cap_9);
    _env_9->list = list;
    const char* _ps_12 = NullChecksApp_Try(CS2SX_Delegate_New((void*)_lambda_9, _env_9, 1));
    printf("%s\n", _ps_12 ? _ps_12 : "");
    IShape* real = Square_as_IShape(CS2SX_RETAIN(Square, Square_New()));
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "ok %d %s %d %s", Node_Get(n), n->vtable->Kind(n), ({ __auto_type _cs2sx_r = real; _cs2sx_r->vtable->Area(_cs2sx_r->obj); }), (((missing != NULL ? missing->f_Name : NULL) ?: "none")));
    const char* _ps_13 = _sb0;
    printf("%s\n", _ps_13 ? _ps_13 : "");
    struct _cap_10* _env_10 = CS2SX_ENV_NEW(struct _cap_10);
    _env_10->missing = CS2SX_RETAIN(Node, missing);
    _env_10->_hdr._dtor = _envdtor_10;
    const char* _ps_14 = NullChecksApp_Try(CS2SX_Delegate_New((void*)_lambda_10, _env_10, 1));
    printf("%s\n", _ps_14 ? _ps_14 : "");
}

void NullChecksApp_Free(NullChecksApp* self)
{
    if (!self) return;
    free(self);
}
