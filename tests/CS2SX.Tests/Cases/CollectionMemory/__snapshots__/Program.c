#include <stdlib.h>
#include "Program.h"

Inventory* Inventory_New(List_str* Items, int Slots)
{
    Inventory* self = (Inventory*)calloc(1, sizeof(Inventory));
    if (!self) return NULL;
    self->_rc = 1;
    self->f_Items = List_str_Retain(Items);
    self->f_Slots = Slots;
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Inventory_Free);
    return self;
}

void Inventory_Free(Inventory* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    if (self->f_Items) { List_str_Free(self->f_Items); self->f_Items = NULL; }
    free(self);
}

Inventory* Inventory_Retain(Inventory* self)
{
    if (self) self->_rc++;
    return self;
}

const CS2SX_GcKind Inventory_GcKind = { offsetof(Inventory, _rc), CS2SX_GcLeaf, (CS2SX_FreeFn)Inventory_Free };

void Bag_Ctor(Bag* self)
{
    CS2SX_TRACE("Bag..ctor", "Program.cs");
    self->f_Values = List_int_Retain(List_int_New());
    self->f_Log = StringBuilder_Retain(StringBuilder_New(256));
    self->f_Counts = Dict_str_int_Retain(Dict_str_int_New());
}

Bag* Bag_New(void)
{
    Bag* self = (Bag*)calloc(1, sizeof(Bag));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Bag_Dispose;
    self->_gc = Bag_Gc;
    Bag_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Bag_Free);
    return self;
}

void Bag_Gc(void* _self, CS2SX_GcVisit visit)
{
    Bag* self = (Bag*)_self; (void)self;
}

static void Bag_GcDyn(void* self, CS2SX_GcVisit visit) { ((Bag*)self)->_gc(self, visit); }
const CS2SX_GcKind Bag_GcKind = { offsetof(Bag, _rc), Bag_GcDyn, (CS2SX_FreeFn)Bag_Free };

void Bag_Dispose(Bag* self)
{
    if (self->f_Counts) { Dict_str_int_Free(self->f_Counts); self->f_Counts = NULL; }
}

void Bag_Free(Bag* self)
{
    if (!self) return;
    Bag* _o = (Bag*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Bag_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Bag* Bag_Retain(Bag* self)
{
    if (self) ((Bag*)self)->_rc++;
    return self;
}

void CollectionMemoryApp_Init(CollectionMemoryApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))CollectionMemoryApp_OnFrame;
    List_int* _list_0 = List_int_New();
    List_int_Add(_list_0, 5);
    List_int_Add(_list_0, 3);
    List_int_Add(_list_0, 9);
    self->f_scores = List_int_Retain(_list_0);
    self->f_bag = CS2SX_RETAIN(Bag, Bag_New());
    self->f_grid = (List_int**)CS2SX_ArrNew(2, sizeof(List_int*));
}

static int _lambda_0(int s)
{
    return s > 4;
}

static int _lambda_1(int s)
{
    return s;
}

struct _cap_2
{
    CS2SX_Env _hdr;
    List_int* local;
};

static int _lambda_2(void* _env)
{
    CS2SX_TRACE("CollectionMemoryApp.<lambda>", "Program.cs");
    struct _cap_2* _c = (struct _cap_2*)_env;
    List_int* local = _c->local;
    return local->count;
}

static void _envdtor_2(void* _e)
{
    struct _cap_2* _c = (struct _cap_2*)_e;
    CS2SX_ReleaseLater(_c->local, (CS2SX_FreeFn)List_int_Free);
}

void CollectionMemoryApp_OnFrame(CollectionMemoryApp* self)
{
    CS2SX_TRACE("CollectionMemoryApp.OnFrame", "Program.cs");
    CS2SX_LINE(30);
    self->f_frame++;
    CS2SX_LINE(33);
    List_int* _where_1 = List_int_New();
    for (int _i_2 = 0; _i_2 < self->f_scores->count; _i_2++)
    {
        int _e__where_1 = List_int_Get(self->f_scores, _i_2);
        if (_lambda_0(_e__where_1)) List_int_Add(_where_1, _e__where_1);
    }
    List_int* _ord_3 = List_int_New();
    for (int _i_4 = 0; _i_4 < _where_1->count; _i_4++)
      List_int_Add(_ord_3, List_int_Get(_where_1, _i_4));
    for (int _i_4 = 1; _i_4 < _ord_3->count; _i_4++)
    {
        int _otmp_6 = _ord_3->data[_i_4];
        int _j_5 = _i_4 - 1;
        while (_j_5 >= 0 && (_lambda_1(_ord_3->data[_j_5]) != _lambda_1(_otmp_6) ? (_lambda_1(_ord_3->data[_j_5]) > _lambda_1(_otmp_6)) : 0))
          { _ord_3->data[_j_5+1] = _ord_3->data[_j_5]; _j_5--; }
        _ord_3->data[_j_5+1] = _otmp_6;
    }
    List_int* _tol_7 = List_int_New();
    for (int _i_8 = 0; _i_8 < _ord_3->count; _i_8++)
      List_int_Add(_tol_7, List_int_Get(_ord_3, _i_8));
    List_int* top = _tol_7;
    CS2SX_LINE(34);
    StringBuilder* sb = StringBuilder_New(256);
    CS2SX_LINE(35);
    StringBuilder_AppendStr(sb, "x");
    CS2SX_LINE(36);
    Dict_str_int* tmp = Dict_str_int_New();
    CS2SX_LINE(37);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "a%d", self->f_frame);
    Dict_str_int_Set(tmp, _sb0, CS2SX_NN(top)->count);
    CS2SX_LINE(39);
    switch (self->f_frame)
    {
        case 1:
            CS2SX_LINE(42);
            List_str* _list_9 = List_str_New();
            List_str_Add(_list_9, "sword");
            List_str_Add(_list_9, "potion");
            List_str* names = _list_9;
            CS2SX_LINE(43);
            CS2SX_ARC_STORE(Inventory, self->f_inv, CS2SX_RETAIN(Inventory, Inventory_New(names, 4)));
            CS2SX_LINE(44);
            List_int* _list_10 = List_int_New();
            List_int_Add(_list_10, 1);
            List_int_Add(_list_10, 2);
            List_int* local = _list_10;
            CS2SX_LINE(45);
            struct _cap_2* _env_2 = CS2SX_ENV_NEW(struct _cap_2);
            _env_2->local = List_int_Retain(local);
            _env_2->_hdr._dtor = _envdtor_2;
            self->f_counter = CS2SX_Delegate_New((void*)_lambda_2, _env_2, 1);
            CS2SX_LINE(46);
            List_int* _list_11 = List_int_New();
            List_int_Add(_list_11, 7);
            CS2SX_COLL_STORE(List_int, CS2SX_AT(self->f_grid, 0), _list_11);
            CS2SX_LINE(47);
            List_int_Add(CS2SX_NN(self->f_bag)->f_Values, 1);
            CS2SX_LINE(48);
            Dict_str_int_Set(CS2SX_NN(self->f_bag)->f_Counts, "k", 2);
            CS2SX_LINE(49);
            StringBuilder_AppendStr(CS2SX_NN(self->f_bag)->f_Log, "hi");
            CS2SX_LINE(50);
            break;
        case 3:
            CS2SX_LINE(52);
            char* _sb1 = _cs2sx_next_buf();
            snprintf(_sb1, CS2SX_STRBUF_SIZE, "kept %d %s %d %d %d %d %s %d", CS2SX_NN(CS2SX_NN(self->f_inv)->f_Items)->count, List_str_Get(CS2SX_NN(self->f_inv)->f_Items, 1), Func_int_Invoke(self->f_counter), List_int_Get(CS2SX_AT(self->f_grid, 0), 0), CS2SX_NN(CS2SX_NN(self->f_bag)->f_Values)->count, Dict_str_int_Get(CS2SX_NN(self->f_bag)->f_Counts, "k"), StringBuilder_ToString(CS2SX_NN(self->f_bag)->f_Log), CS2SX_NN(top)->count);
            const char* _ps_12 = _sb1;
            printf("%s\n", _ps_12 ? _ps_12 : "");
            CS2SX_LINE(55);
            break;
        case 5:
            CS2SX_LINE(57);
            self->f_base = CS2SX_HeapUsed();
            CS2SX_LINE(58);
            break;
        case 300:
            CS2SX_LINE(60);
            long long grown = CS2SX_HeapUsed() - self->f_base;
            CS2SX_LINE(61);
            char* _sb2 = _cs2sx_next_buf();
            snprintf(_sb2, CS2SX_STRBUF_SIZE, "stable %s", CS2SX_BoolStr((grown < 4096)));
            const char* _ps_13 = _sb2;
            printf("%s\n", _ps_13 ? _ps_13 : "");
            CS2SX_LINE(62);
            CS2SX_ARC_STORE(Inventory, self->f_inv, NULL);
            CS2SX_LINE(63);
            CS2SX_ARC_STORE(Bag, self->f_bag, CS2SX_RETAIN(Bag, Bag_New()));
            CS2SX_LINE(64);
            break;
        case 302:
            CS2SX_LINE(66);
            printf("done\n");
            CS2SX_LINE(67);
            break;
    }
}

void CollectionMemoryApp_Free(CollectionMemoryApp* self)
{
    if (!self) return;
    if (self->f_bag) { Bag_Free(self->f_bag); self->f_bag = NULL; }
    if (self->f_inv) { Inventory_Free(self->f_inv); self->f_inv = NULL; }
    for (int _ai = 0; _ai < CS2SX_ArrLen(self->f_grid); _ai++) List_int_Free(self->f_grid[_ai]);
    CS2SX_ArrFree(self->f_grid); self->f_grid = NULL;
    free(self);
}
