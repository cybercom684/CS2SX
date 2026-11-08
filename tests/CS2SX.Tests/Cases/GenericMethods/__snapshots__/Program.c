#include <stdlib.h>
#include "_generics.h"
#include "Program.h"

int Util_Max_int(int a, int b)
{
    return (({ __auto_type _cmp_a = (a); __auto_type _cmp_b = (b); (_cmp_a > _cmp_b) - (_cmp_a < _cmp_b); }) > 0 ? a : b);
}

float Util_Max_float(float a, float b)
{
    return (({ __auto_type _cmp_a = (a); __auto_type _cmp_b = (b); (_cmp_a > _cmp_b) - (_cmp_a < _cmp_b); }) > 0 ? a : b);
}

const char* Util_Max_str(const char* a, const char* b)
{
    return (String_CompareTo(CS2SX_NN(a), b) > 0 ? a : b);
}

void Util_Swap_int(int* a, int* b)
{
    int t = (*a);
    *a = (*b);
    *b = t;
}

Node* Util_First_Node(List_Node* items)
{
    return List_Node_Get(items, 0);
}

int Util_Larger_int(int a, int b)
{
    return Util_Max_int(a, b);
}

int Util_OrDefault_int(List_int* items)
{
    return (CS2SX_NN(items)->count > 0 ? List_int_Get(items, 0) : 0);
}

int Util_CountWhere_int(List_int* items, Func_int_int_t pred)
{
    int n = 0;
    for (int _i_x = 0; _i_x < (int)(items->count); _i_x++)
    {
        int x = List_int_Get(items, _i_x);
        if (Func_int_int_Invoke(pred, x))
        {
            n++;
        }
    }
    return n;
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

// ── Struct: Pair ────────────────────────────────────────
const char* Pair_Pick_str(const char* a, const char* b, int first)
{
    return (first ? a : b);
}

int Pair_Pick_int(int a, int b, int first)
{
    return (first ? a : b);
}

int Pair_Equals(Pair a, Pair b)
{
    return memcmp(&a, &b, sizeof(Pair)) == 0;
}

void GenericMethodsApp_Init(GenericMethodsApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))GenericMethodsApp_OnInit;
}

static int _lambda_0(void* _env, int n)
{
    (void)_env;
    return n > 3;
}

static CS2SX_Delegate _dg_0 = CS2SX_DELEGATE_STATIC(_lambda_0);

void GenericMethodsApp_OnInit(GenericMethodsApp* self)
{
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "max %d %s %s", Util_Max_int(3, 7), Float_ToString(Util_Max_float(2.5f, 1.5f)), Util_Max_str("apple", "pear"));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    int x = 1;
    int y = 2;
    Util_Swap_int(&x, &y);
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "swap %d %d", x, y);
    const char* _ps_1 = _sb1;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    char* _sb2 = _cs2sx_next_buf();
    snprintf(_sb2, CS2SX_STRBUF_SIZE, "pick %s %d", Pair_Pick_str("a", "b", 0), Pair_Pick_int(4, 5, 1));
    const char* _ps_2 = _sb2;
    printf("%s\n", _ps_2 ? _ps_2 : "");
    List_Node* nodes = GenericMethodsApp_Repeat_Node(Node_New(9), 3);
    char* _sb3 = _cs2sx_next_buf();
    snprintf(_sb3, CS2SX_STRBUF_SIZE, "repeat %d %d", CS2SX_NN(nodes)->count, CS2SX_NN(Util_First_Node(nodes))->f_V);
    const char* _ps_3 = _sb3;
    printf("%s\n", _ps_3 ? _ps_3 : "");
    char* _sb4 = _cs2sx_next_buf();
    snprintf(_sb4, CS2SX_STRBUF_SIZE, "larger %d", Util_Larger_int(10, 20));
    const char* _ps_4 = _sb4;
    printf("%s\n", _ps_4 ? _ps_4 : "");
    char* _sb5 = _cs2sx_next_buf();
    snprintf(_sb5, CS2SX_STRBUF_SIZE, "default %d", Util_OrDefault_int(List_int_New()));
    const char* _ps_5 = _sb5;
    printf("%s\n", _ps_5 ? _ps_5 : "");
    List_int* _list_6 = List_int_New();
    List_int_Add(_list_6, 1);
    List_int_Add(_list_6, 5);
    List_int_Add(_list_6, 8);
    List_int_Add(_list_6, 2);
    List_int* nums = _list_6;
    char* _sb6 = _cs2sx_next_buf();
    snprintf(_sb6, CS2SX_STRBUF_SIZE, "count %d", Util_CountWhere_int(nums, (&_dg_0)));
    const char* _ps_7 = _sb6;
    printf("%s\n", _ps_7 ? _ps_7 : "");
}

List_Node* GenericMethodsApp_Repeat_Node(Node* value, int n)
{
    List_Node* l = List_Node_New();
    for (int i = 0; i < n; i++)
    {
        List_Node_Add(l, value);
    }
    return l;
}

void GenericMethodsApp_Free(GenericMethodsApp* self)
{
    if (!self) return;
    free(self);
}
