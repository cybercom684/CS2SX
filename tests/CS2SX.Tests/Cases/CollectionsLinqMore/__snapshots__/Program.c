#include <stdlib.h>
#include "_generics.h"
#include "Program.h"
#include "CS2SX_Collections.h"

void Item_Ctor(Item* self, const char* n, int s)
{
    CS2SX_TRACE("Item..ctor", "Program.cs");
    self->f_Name = CS2SX_StrOwn("");
    CS2SX_LINE(10);
    CS2SX_STR_STORE(self->f_Name, n);
    CS2SX_LINE(10);
    self->f_Score = s;
}

Item* Item_New(const char* n, int s)
{
    Item* self = (Item*)calloc(1, sizeof(Item));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Item__Destroy;
    self->_gc = Item_Gc;
    self->_class = &Item_ClassInfo;
    Item_Ctor(self, n, s);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Item_Free);
    return self;
}

const CS2SX_ClassInfo Item_ClassInfo = { "Item", NULL, NULL };

void Item_Gc(void* _self, CS2SX_GcVisit visit)
{
    Item* self = (Item*)_self; (void)self;
}

static void Item_GcDyn(void* self, CS2SX_GcVisit visit) { ((Item*)self)->_gc(self, visit); }
const CS2SX_GcKind Item_GcKind = { offsetof(Item, _rc), Item_GcDyn, (CS2SX_FreeFn)Item_Free };

void Item__Destroy(Item* self)
{
    CS2SX_StrDrop(self->f_Name); self->f_Name = NULL;
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

void CollectionsLinqMoreApp_Init(CollectionsLinqMoreApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))CollectionsLinqMoreApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))CollectionsLinqMoreApp_OnFrame;
}

int CollectionsLinqMoreApp_SumAll(int* xs)
{
    CS2SX_TRACE("CollectionsLinqMoreApp.SumAll", "Program.cs");
    CS2SX_LINE(14);
    int s = 0;
    CS2SX_LINE(14);
    for (int _i_x = 0; _i_x < (int)(CS2SX_ArrLen(xs)); _i_x++)
    {
        int x = xs[_i_x];
        s += x;
    }
    CS2SX_LINE(14);
    return s;
}

static int _lambda_0(int x, int y)
{
    return y - x;
}

static int _lambda_1(int x)
{
    return x == 3;
}

static int _lambda_2(int x)
{
    return x > 4;
}

static int _lambda_3(int x)
{
    return x > 6;
}

static const char* _lambda_4(int x)
{
    char* _sb2 = _cs2sx_next_buf();
    _sb2 = CS2SX_Format(_sb2, "n%d", x);
    return _sb2;
}

static int _lambda_5(void* _env, int x)
{
    CS2SX_TRACE("CollectionsLinqMoreApp.<lambda>", "Program.cs");
    (void)_env;
    return x % 3;
}

static CS2SX_Delegate _dg_5 = CS2SX_DELEGATE_STATIC(_lambda_5);

static const char* _lambda_6(CS2SX_IGrouping_int_int* gr)
{
    char* _sb4 = _cs2sx_next_buf();
    _sb4 = CS2SX_Format(_sb4, "%d:%d", CS2SX_NN(gr)->f_Key, CS2SX_NN(gr)->f_Items->count);
    return _sb4;
}

static int _lambda_7(void* _env, const char* w)
{
    CS2SX_TRACE("CollectionsLinqMoreApp.<lambda>", "Program.cs");
    (void)_env;
    return CS2SX_Str16Len(CS2SX_NN(w));
}

static CS2SX_Delegate _dg_7 = CS2SX_DELEGATE_STATIC(_lambda_7);

static double _lambda_8(const char* w)
{
    return CS2SX_Str16Len(CS2SX_NN(w));
}

static int _lambda_9(Item* i)
{
    return CS2SX_NN(i)->f_Score;
}

static const char* _lambda_10(Item* i)
{
    return CS2SX_NN(i)->f_Name;
}

static const char* _lambda_11(Item* i)
{
    return CS2SX_NN(i)->f_Name;
}

static const char* _lambda_12(Item* i)
{
    return CS2SX_NN(i)->f_Name;
}

static const char* _lambda_13(Item* i)
{
    return CS2SX_NN(i)->f_Name;
}

static int _lambda_14(void* _env, int x)
{
    CS2SX_TRACE("CollectionsLinqMoreApp.<lambda>", "Program.cs");
    (void)_env;
    return x % 2 == 0;
}

static CS2SX_Delegate _dg_14 = CS2SX_DELEGATE_STATIC(_lambda_14);

static int _lambda_15(int x)
{
    return x > 1;
}

void CollectionsLinqMoreApp_OnInit(CollectionsLinqMoreApp* self)
{
    CS2SX_TRACE("CollectionsLinqMoreApp.OnInit", "Program.cs");
    CS2SX_LINE(18);
    List_int* _list_0 = List_int_New();
    List_int_Add(_list_0, 5);
    List_int_Add(_list_0, 3);
    List_int_Add(_list_0, 8);
    List_int_Add(_list_0, 1);
    List_int* a = _list_0;
    CS2SX_LINE(19);
    for (int _si_1 = 1; _si_1 < a->count; _si_1++)
    {
        int _stmp_3 = a->data[_si_1];
        int _sj_2 = _si_1 - 1;
        while (_sj_2 >= 0 && _lambda_0(a->data[_sj_2], _stmp_3) > 0)
        {
            a->data[_sj_2+1] = a->data[_sj_2];
            _sj_2--;
        }
        a->data[_sj_2+1] = _stmp_3;
    }
    /* sorted */;
    CS2SX_LINE(20);
    int _fix_5 = -1;
    for (int _li_4 = 0; _li_4 < a->count; _li_4++)
        if (_lambda_1(a->data[_li_4])) { _fix_5 = _li_4; break; }
    int _fix_7 = -1;
    for (int _li_6 = a->count - 1; _li_6 >= 0; _li_6--)
        if (_lambda_2(a->data[_li_6])) { _fix_7 = _li_6; break; }
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "sort %s %d %d", ({ __auto_type _jl = (a); int _jn = (_jl ? _jl->count : 0); CS2SX_JOIN_EACH(",", _jn, "%d", (int)(_jl->data[_ji])); }), _fix_5, _fix_7);
    const char* _ps_8 = _sb0;
    printf("%s\n", _ps_8 ? _ps_8 : "");
    CS2SX_LINE(21);
    int _ra_10 = 0;
    for (int _li_9 = a->count - 1; _li_9 >= 0; _li_9--)
        if (_lambda_3(a->data[_li_9])) { List_int_Remove(a, _li_9); _ra_10++; }
    char* _sb1 = _cs2sx_next_buf();
    _sb1 = CS2SX_Format(_sb1, "rm %d", _ra_10);
    const char* _ps_11 = _sb1;
    printf("%s\n", _ps_11 ? _ps_11 : "");
    CS2SX_LINE(22);
    List_int* _list_12 = List_int_New();
    List_int_Add(_list_12, 9);
    List_int_Add(_list_12, 9);
    __auto_type _ars_13 = _list_12;
    int _arn_14 = _ars_13 ? _ars_13->count : 0;
    int _ara_16 = 0;
    for (int _ari_15 = 0; _ari_15 < _arn_14; _ari_15++) List_int_Insert(a, _ara_16 + _ari_15, _ars_13->data[_ari_15]);
    CS2SX_LINE(23);
    __auto_type _ars_17 = (int*)CS2SX_ArrFromInit((int[]){ 7 }, 1, sizeof(int));
    int _arn_18 = CS2SX_ArrLen(_ars_17);
    for (int _ari_19 = 0; _ari_19 < _arn_18; _ari_19++) List_int_Add(a, _ars_17[_ari_19]);
    CS2SX_LINE(24);
    List_str* _ca_21 = List_str_New();
    for (int _li_20 = 0; _li_20 < a->count; _li_20++)
        List_str_Add(_ca_21, _lambda_4(a->data[_li_20]));
    List_str* s = _ca_21;
    CS2SX_LINE(25);
    char* _sb3 = _cs2sx_next_buf();
    _sb3 = CS2SX_Format(_sb3, "lst %s %s", ({ __auto_type _jl = (a); int _jn = (_jl ? _jl->count : 0); CS2SX_JOIN_EACH(",", _jn, "%d", (int)(_jl->data[_ji])); }), String_Join(",", s));
    const char* _ps_22 = _sb3;
    printf("%s\n", _ps_22 ? _ps_22 : "");
    CS2SX_LINE(26);
    __auto_type _src_23 = CS2SX_IGrouping_int_int_By(a, (&_dg_5));
    List_str* _sel_24 = List_str_New();
    for (int _i_25 = 0; _i_25 < _src_23->count; _i_25++)
    {
        CS2SX_IGrouping_int_int* _e__sel_24 = List_CS2SX_IGrouping_int_int_Get(_src_23, _i_25);
        List_str_Add(_sel_24, (const char*)_lambda_6(_e__sel_24));
    }
    List_str* _tol_26 = List_str_New();
    for (int _i_27 = 0; _i_27 < _sel_24->count; _i_27++)
      List_str_Add(_tol_26, List_str_Get(_sel_24, _i_27));
    List_str* g = _tol_26;
    CS2SX_LINE(27);
    char* _sb5 = _cs2sx_next_buf();
    _sb5 = CS2SX_Format(_sb5, "grp %s", String_Join(",", g));
    const char* _ps_28 = _sb5;
    printf("%s\n", _ps_28 ? _ps_28 : "");
    CS2SX_LINE(28);
    List_str* _list_29 = List_str_New();
    List_str_Add(_list_29, "ab");
    List_str_Add(_list_29, "c");
    List_str_Add(_list_29, "de");
    __auto_type _fe_30 = CS2SX_IGrouping_int_str_By(_list_29, (&_dg_7));
    for (int _i_grp = 0; _i_grp < (int)(_fe_30->count); _i_grp++)
    {
        CS2SX_IGrouping_int_str* grp = List_CS2SX_IGrouping_int_str_Get(_fe_30, _i_grp);
        int _sum_32 = 0;
        for (int _i_31 = 0; _i_31 < CS2SX_NN(grp)->f_Items->count; _i_31++)
          _sum_32 += (int)_lambda_8(List_str_Get(CS2SX_NN(grp)->f_Items, _i_31));
        char* _sb6 = _cs2sx_next_buf();
        _sb6 = CS2SX_Format(_sb6, "g%d %s %d", CS2SX_NN(grp)->f_Key, String_Join("+", CS2SX_NN(grp)->f_Items), _sum_32);
        const char* _ps_33 = _sb6;
        printf("%s\n", _ps_33 ? _ps_33 : "");
    }
    CS2SX_LINE(30);
    List_Item* _list_34 = List_Item_New();
    List_Item_Add(_list_34, Item_New("b", 2));
    List_Item_Add(_list_34, Item_New("a", 2));
    List_Item_Add(_list_34, Item_New("c", 1));
    List_Item* items = _list_34;
    CS2SX_LINE(31);
    List_Item* _ord_35 = List_Item_New();
    for (int _i_36 = 0; _i_36 < items->count; _i_36++)
      List_Item_Add(_ord_35, List_Item_Get(items, _i_36));
    for (int _i_36 = 1; _i_36 < _ord_35->count; _i_36++)
    {
        Item* _otmp_38 = _ord_35->data[_i_36];
        int _j_37 = _i_36 - 1;
        while (_j_37 >= 0 && (_lambda_9(_ord_35->data[_j_37]) != _lambda_9(_otmp_38) ? (_lambda_9(_ord_35->data[_j_37]) > _lambda_9(_otmp_38)) : 0))
          { _ord_35->data[_j_37+1] = _ord_35->data[_j_37]; _j_37--; }
        _ord_35->data[_j_37+1] = _otmp_38;
    }
    List_Item* _ord_39 = List_Item_New();
    for (int _i_40 = 0; _i_40 < _ord_35->count; _i_40++)
      List_Item_Add(_ord_39, List_Item_Get(_ord_35, _i_40));
    for (int _i_40 = 1; _i_40 < _ord_39->count; _i_40++)
    {
        Item* _otmp_42 = _ord_39->data[_i_40];
        int _j_41 = _i_40 - 1;
        while (_j_41 >= 0 && (_lambda_9(_ord_39->data[_j_41]) != _lambda_9(_otmp_42) ? (_lambda_9(_ord_39->data[_j_41]) > _lambda_9(_otmp_42)) : (CS2SX_strcmp_safe(_lambda_10(_ord_39->data[_j_41]), _lambda_10(_otmp_42)) != 0 ? (CS2SX_strcmp_safe(_lambda_10(_ord_39->data[_j_41]), _lambda_10(_otmp_42)) > 0) : 0)))
          { _ord_39->data[_j_41+1] = _ord_39->data[_j_41]; _j_41--; }
        _ord_39->data[_j_41+1] = _otmp_42;
    }
    List_str* _sel_43 = List_str_New();
    for (int _i_44 = 0; _i_44 < _ord_39->count; _i_44++)
    {
        Item* _e__sel_43 = List_Item_Get(_ord_39, _i_44);
        List_str_Add(_sel_43, (const char*)_lambda_11(_e__sel_43));
    }
    List_Item* _ord_45 = List_Item_New();
    for (int _i_46 = 0; _i_46 < items->count; _i_46++)
      List_Item_Add(_ord_45, List_Item_Get(items, _i_46));
    for (int _i_46 = 1; _i_46 < _ord_45->count; _i_46++)
    {
        Item* _otmp_48 = _ord_45->data[_i_46];
        int _j_47 = _i_46 - 1;
        while (_j_47 >= 0 && (CS2SX_strcmp_safe(_lambda_12(_ord_45->data[_j_47]), _lambda_12(_otmp_48)) != 0 ? (CS2SX_strcmp_safe(_lambda_12(_ord_45->data[_j_47]), _lambda_12(_otmp_48)) < 0) : 0))
          { _ord_45->data[_j_47+1] = _ord_45->data[_j_47]; _j_47--; }
        _ord_45->data[_j_47+1] = _otmp_48;
    }
    List_str* _sel_49 = List_str_New();
    for (int _i_50 = 0; _i_50 < _ord_45->count; _i_50++)
    {
        Item* _e__sel_49 = List_Item_Get(_ord_45, _i_50);
        List_str_Add(_sel_49, (const char*)_lambda_13(_e__sel_49));
    }
    char* _sb7 = _cs2sx_next_buf();
    _sb7 = CS2SX_Format(_sb7, "ord %s %s", String_Join(",", _sel_43), String_Join(",", _sel_49));
    const char* _ps_51 = _sb7;
    printf("%s\n", _ps_51 ? _ps_51 : "");
    CS2SX_LINE(33);
    HashSet_str* _set_52 = HashSet_str_New();
    HashSet_str_Add(_set_52, "a");
    HashSet_str_Add(_set_52, "b");
    HashSet_str* hs = _set_52;
    CS2SX_LINE(34);
    HashSet_str_Add(hs, "a");
    CS2SX_LINE(35);
    char* _sb8 = _cs2sx_next_buf();
    _sb8 = CS2SX_Format(_sb8, "hs %d %s %s", hs->count, CS2SX_BoolStr(HashSet_str_Contains(hs, "b")), CS2SX_BoolStr(HashSet_str_Contains(hs, "z")));
    const char* _ps_53 = _sb8;
    printf("%s\n", _ps_53 ? _ps_53 : "");
    CS2SX_LINE(36);
    Stack_str* st = Stack_str_New();
    CS2SX_LINE(36);
    Stack_str_Push(st, "x");
    CS2SX_LINE(36);
    Stack_str_Push(st, "y");
    CS2SX_LINE(37);
    char* _sb9 = _cs2sx_next_buf();
    _sb9 = CS2SX_Format(_sb9, "st %s%d", Stack_str_Pop(st), st->count);
    const char* _ps_54 = _sb9;
    printf("%s", _ps_54 ? _ps_54 : "");
    CS2SX_LINE(38);
    const char* top = {0};
    const char* none = {0};
    char* _sb10 = _cs2sx_next_buf();
    _sb10 = CS2SX_Format(_sb10, "%s%s%s", CS2SX_BoolStr(({ __auto_type _tc = (st); int _tok = _tc && _tc->count > 0; if (_tok) *(&top) = Stack_str_Pop(_tc); _tok; })), top, CS2SX_BoolStr(({ __auto_type _tc = (st); int _tok = _tc && _tc->count > 0; if (_tok) *(&none) = Stack_str_Pop(_tc); _tok; })));
    const char* _ps_55 = _sb10;
    printf("%s\n", _ps_55 ? _ps_55 : "");
    CS2SX_LINE(39);
    Queue_str* q = Queue_str_New();
    CS2SX_LINE(39);
    Queue_str_Enqueue(q, "q1");
    CS2SX_LINE(40);
    const char* qv = {0};
    if (({ __auto_type _tc = (q); int _tok = _tc && _tc->count > 0; if (_tok) *(&qv) = Queue_str_Dequeue(_tc); _tok; }))
    {
        CS2SX_LINE(40);
        char* _sb11 = _cs2sx_next_buf();
        _sb11 = CS2SX_Format(_sb11, "q %s %s", qv, CS2SX_BoolStr(({ __auto_type _tc = (q); int _tok = _tc && _tc->count > 0; if (_tok) (void)Queue_str_Peek(_tc); _tok; })));
        const char* _ps_56 = _sb11;
        printf("%s\n", _ps_56 ? _ps_56 : "");
    }
    CS2SX_LINE(41);
    Dict_str_List_int* d = Dict_str_List_int_New();
    CS2SX_LINE(42);
    List_int* _list_57 = List_int_New();
    List_int_Add(_list_57, 1);
    List_int_Add(_list_57, 2);
    Dict_str_List_int_Set(d, "k", _list_57);
    CS2SX_LINE(43);
    List_int_Add(Dict_str_List_int_Get(d, "k"), 3);
    CS2SX_LINE(44);
    Dict_str_int* _dict_58 = Dict_str_int_New();
    Dict_str_int_Set(_dict_58, "y", 1);
    Dict_str_int* counts = _dict_58;
    CS2SX_LINE(45);
    char* _sb12 = _cs2sx_next_buf();
    _sb12 = CS2SX_Format(_sb12, "d %d [%s] %d%d", CS2SX_NN(Dict_str_List_int_Get(d, "k"))->count, ({ __auto_type _nv = ((({ __auto_type _gd = (d); __auto_type _gk = ("z"); __typeof__(Dict_str_List_int_Get(_gd, _gk)) _gv = {0}; if (Dict_str_List_int_ContainsKey(_gd, _gk)) _gv = Dict_str_List_int_Get(_gd, _gk); _gv; }) != NULL ? ((Nullable_int){ ({ __auto_type _gd = (d); __auto_type _gk = ("z"); __typeof__(Dict_str_List_int_Get(_gd, _gk)) _gv = {0}; if (Dict_str_List_int_ContainsKey(_gd, _gk)) _gv = Dict_str_List_int_Get(_gd, _gk); _gv; })->count, 1 }) : ((Nullable_int){0}))); _nv.has ? (const char*)CS2SX_Format(_cs2sx_next_buf(), "%d", _nv.value) : ""; }), ({ __auto_type _gd = (counts); __auto_type _gk = ("x"); __typeof__(Dict_str_int_Get(_gd, _gk)) _gv = {0}; _gv = (5); if (Dict_str_int_ContainsKey(_gd, _gk)) _gv = Dict_str_int_Get(_gd, _gk); _gv; }), ({ __auto_type _gd = (counts); __auto_type _gk = ("y"); __typeof__(Dict_str_int_Get(_gd, _gk)) _gv = {0}; if (Dict_str_int_ContainsKey(_gd, _gk)) _gv = Dict_str_int_Get(_gd, _gk); _gv; }));
    const char* _ps_59 = _sb12;
    printf("%s\n", _ps_59 ? _ps_59 : "");
    CS2SX_LINE(46);
    int** jag = (int**)CS2SX_ArrNew(2, sizeof(int*));
    CS2SX_LINE(47);
    CS2SX_AT(jag, 0) = (int*)CS2SX_ArrFromInit((int[]){ 1, 2 }, 2, sizeof(int));
    CS2SX_LINE(48);
    CS2SX_AT(jag, 1) = (int*)CS2SX_ArrNew(3, sizeof(int));
    CS2SX_LINE(49);
    char* _sb13 = _cs2sx_next_buf();
    _sb13 = CS2SX_Format(_sb13, "jag %d %d %d", CS2SX_AT(CS2SX_AT(jag, 0), 1), CS2SX_ArrLen(CS2SX_NN(CS2SX_AT(jag, 1))), CS2SX_ArrLen(jag));
    const char* _ps_60 = _sb13;
    printf("%s\n", _ps_60 ? _ps_60 : "");
    CS2SX_LINE(50);
    char* _sb14 = _cs2sx_next_buf();
    _sb14 = CS2SX_Format(_sb14, "eq %s %s %s", CS2SX_BoolStr(String_EqualsIgnoreCase("A", "a")), CS2SX_BoolStr((CS2SX_strcmp_safe("x", "X") == 0)), CS2SX_BoolStr((CS2SX_strcmp_safe(NULL, NULL) == 0)));
    const char* _ps_61 = _sb14;
    printf("%s\n", _ps_61 ? _ps_61 : "");
    CS2SX_LINE(51);
    StringBuilder* sb = StringBuilder_AppendStr(StringBuilder_New(256), "hello");
    CS2SX_LINE(52);
    StringBuilder_AppendStr(StringBuilder_Replace(StringBuilder_Insert(sb, 0, ">"), "l", "LL"), "!");
    CS2SX_LINE(53);
    char* _sb15 = _cs2sx_next_buf();
    _sb15 = CS2SX_Format(_sb15, "sb %s %d", StringBuilder_ToString(sb), ({ StringBuilder* _cs2sx_sbl = CS2SX_NN(sb); _cs2sx_sbl->buf ? CS2SX_Str16Len(_cs2sx_sbl->buf) : 0; }));
    const char* _ps_62 = _sb15;
    printf("%s\n", _ps_62 ? _ps_62 : "");
    CS2SX_LINE(54);
    CS2SX_ARR_LOCAL(int, _pa_63, 0);
    CS2SX_ARR_LOCAL(int, _pa_64, 2, 1, 2);
    char* _sb16 = _cs2sx_next_buf();
    _sb16 = CS2SX_Format(_sb16, "p %d %d", CollectionsLinqMoreApp_SumAll(_pa_63), CollectionsLinqMoreApp_SumAll(_pa_64));
    const char* _ps_65 = _sb16;
    printf("%s\n", _ps_65 ? _ps_65 : "");
    CS2SX_LINE(55);
    Nullable_int n = ((Nullable_int){0});
    CS2SX_LINE(55);
    if (!(n).has)
        n = ((Nullable_int){ 4, 1 });
    n;
    CS2SX_LINE(55);
    if (!(n).has)
        n = ((Nullable_int){ 9, 1 });
    n;
    CS2SX_LINE(56);
    Nullable_int m = ((Nullable_int){0});
    CS2SX_LINE(57);
    char* _sb17 = _cs2sx_next_buf();
    _sb17 = CS2SX_Format(_sb17, "n %s [%s]", ({ __auto_type _nv = (n); _nv.has ? (const char*)CS2SX_Format(_cs2sx_next_buf(), "%d", _nv.value) : ""; }), ({ __auto_type _nv = (m); _nv.has ? (const char*)CS2SX_Format(_cs2sx_next_buf(), "%d", _nv.value) : ""; }));
    const char* _ps_66 = _sb17;
    printf("%s\n", _ps_66 ? _ps_66 : "");
    CS2SX_LINE(58);
    _Tuple2_str_int t = (_Tuple2_str_int){.item1 = "x", .item2 = 2};
    CS2SX_LINE(59);
    char* _sb18 = _cs2sx_next_buf();
    _sb18 = CS2SX_Format(_sb18, "t %s%d", t.item1, t.item2);
    const char* _ps_67 = _sb18;
    printf("%s\n", _ps_67 ? _ps_67 : "");
    CS2SX_LINE(60);
    Func_int_int_t isEven = (&_dg_14);
    CS2SX_LINE(61);
    char* _sb19 = _cs2sx_next_buf();
    _sb19 = CS2SX_Format(_sb19, "pr %s%s", CS2SX_BoolStr(Func_int_int_Invoke(isEven, 4)), CS2SX_BoolStr(Func_int_int_Invoke(isEven, 3)));
    const char* _ps_68 = _sb19;
    printf("%s\n", _ps_68 ? _ps_68 : "");
    CS2SX_LINE(62);
    List_str* _rep_69 = List_str_New();
    for (int _repi_70 = 0; _repi_70 < 2; _repi_70++)
        List_str_Add(_rep_69, "ab");
    char* _sb20 = _cs2sx_next_buf();
    _sb20 = CS2SX_Format(_sb20, "cat %s %s", String_Join("", _rep_69), String_JoinArr("", (const char**)CS2SX_ArrFromInit((const char*[]){ "x", "y" }, 2, sizeof(const char*))));
    const char* _ps_71 = _sb20;
    printf("%s\n", _ps_71 ? _ps_71 : "");
    CS2SX_LINE(63);
    int* arr = (int*)CS2SX_ArrFromInit((int[]){ 3, 1, 3, 2 }, 4, sizeof(int));
    CS2SX_LINE(64);
    List_int* _dist_72 = List_int_New();
    for (int _i_73 = 0; _i_73 < 4; _i_73++)
    {
        int _de = arr[_i_73];
        if (!List_int_Contains(_dist_72, _de)) List_int_Add(_dist_72, _de);
    }
    List_int* _where_74 = List_int_New();
    for (int _i_75 = 0; _i_75 < 4; _i_75++)
    {
        int _e__where_74 = arr[_i_75];
        if (_lambda_15(_e__where_74)) List_int_Add(_where_74, _e__where_74);
    }
    int _maxv_77 = (4 > 0) ? arr[0] : 0;
    for (int _i_76 = 1; _i_76 < 4; _i_76++)
      if (arr[_i_76] > _maxv_77) _maxv_77 = arr[_i_76];
    char* _sb21 = _cs2sx_next_buf();
    _sb21 = CS2SX_Format(_sb21, "lq %s %s %d", ({ __auto_type _jl = (_dist_72); int _jn = (_jl ? _jl->count : 0); CS2SX_JOIN_EACH(",", _jn, "%d", (int)(_jl->data[_ji])); }), ({ __auto_type _jl = (_where_74); int _jn = (_jl ? _jl->count : 0); CS2SX_JOIN_EACH(",", _jn, "%d", (int)(_jl->data[_ji])); }), (4 > 0 ? _maxv_77 : (CS2SX_ThrowNoElements(), _maxv_77)));
    const char* _ps_78 = _sb21;
    printf("%s\n", _ps_78 ? _ps_78 : "");
    CS2SX_LINE(65);
    printf("done\n");
}

void CollectionsLinqMoreApp_OnFrame(CollectionsLinqMoreApp* self)
{
    CS2SX_TRACE("CollectionsLinqMoreApp.OnFrame", "Program.cs");
}

void CollectionsLinqMoreApp_Free(CollectionsLinqMoreApp* self)
{
    if (!self) return;
    free(self);
}
