#include <stdlib.h>
#include "_generics.h"
#include "Program.h"

Enemy* Enemy_New(const char* name, int hp)
{
    Enemy* self = (Enemy*)malloc(sizeof(Enemy));
    if (!self) return NULL;
    memset(self, 0, sizeof(Enemy));
    self->_rc = 1;
    self->f_Name = _cs2sx_heap_strdup(name);
    self->f_Hp = hp;
    return self;
}

void Enemy_Free(Enemy* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    free(self);
}

Enemy* Enemy_Retain(Enemy* self)
{
    if (self) self->_rc++;
    return self;
}

void LinqApp_Init(LinqApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))LinqApp_OnInit;
}

static int _lambda_0(int v)
{
    return v % 2 == 0;
}

static int _lambda_1(int v)
{
    return v * 2;
}

static int _lambda_2(int v)
{
    return v;
}

static int _lambda_3(int v)
{
    return v > 8;
}

static int _lambda_4(int v)
{
    return v > 0;
}

static int _lambda_5(int v)
{
    return v > 4;
}

static int _lambda_6(int v)
{
    return v < 5;
}

static int _lambda_7(Enemy* e)
{
    return e->f_Hp > 10;
}

static int _lambda_8(Enemy* e)
{
    return e->f_Hp;
}

static int _lambda_9(Enemy* e)
{
    return e->f_Hp;
}

void LinqApp_OnInit(LinqApp* self)
{
    List_int* _list_0 = List_int_New();
    List_int_Add(_list_0, 5);
    List_int_Add(_list_0, 3);
    List_int_Add(_list_0, 8);
    List_int_Add(_list_0, 1);
    List_int_Add(_list_0, 9);
    List_int_Add(_list_0, 2);
    List_int* values = _list_0;
    List_int* _where_1 = List_int_New();
    for (int _i_2 = 0; _i_2 < values->count; _i_2++)
    {
        int _e__where_1 = List_int_Get(values, _i_2);
        if (_lambda_0(_e__where_1)) List_int_Add(_where_1, _e__where_1);
    }
    List_int* _tol_3 = List_int_New();
    for (int _i_4 = 0; _i_4 < _where_1->count; _i_4++)
      List_int_Add(_tol_3, List_int_Get(_where_1, _i_4));
    List_int* evens = _tol_3;
    List_int* _sel_5 = List_int_New();
    for (int _i_6 = 0; _i_6 < values->count; _i_6++)
    {
        int _e__sel_5 = List_int_Get(values, _i_6);
        List_int_Add(_sel_5, (int)_lambda_1(_e__sel_5));
    }
    List_int* _tol_7 = List_int_New();
    for (int _i_8 = 0; _i_8 < _sel_5->count; _i_8++)
      List_int_Add(_tol_7, List_int_Get(_sel_5, _i_8));
    List_int* doubled = _tol_7;
    List_int* _ord_9 = List_int_New();
    for (int _i_10 = 0; _i_10 < values->count; _i_10++)
      List_int_Add(_ord_9, List_int_Get(values, _i_10));
    for (int _i_10 = 1; _i_10 < _ord_9->count; _i_10++)
    {
        int _otmp_12 = _ord_9->data[_i_10];
        int _j_11 = _i_10 - 1;
        while (_j_11 >= 0 && (_lambda_2(_ord_9->data[_j_11]) != _lambda_2(_otmp_12) ? (_lambda_2(_ord_9->data[_j_11]) > _lambda_2(_otmp_12)) : 0))
          { _ord_9->data[_j_11+1] = _ord_9->data[_j_11]; _j_11--; }
        _ord_9->data[_j_11+1] = _otmp_12;
    }
    List_int* _tol_13 = List_int_New();
    for (int _i_14 = 0; _i_14 < _ord_9->count; _i_14++)
      List_int_Add(_tol_13, List_int_Get(_ord_9, _i_14));
    List_int* sorted = _tol_13;
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "%d %d %d", evens->count, List_int_Get(doubled, 0), List_int_Get(sorted, 0));
    const char* _ps_15 = _sb0;
    printf("%s\n", _ps_15 ? _ps_15 : "");
    int _sum_17 = 0;
    for (int _i_16 = 0; _i_16 < values->count; _i_16++)
      _sum_17 += List_int_Get(values, _i_16);
    int _maxv_19 = (values->count > 0) ? List_int_Get(values, 0) : 0;
    for (int _i_18 = 1; _i_18 < values->count; _i_18++)
      if (List_int_Get(values, _i_18) > _maxv_19) _maxv_19 = List_int_Get(values, _i_18);
    int _minv_21 = (values->count > 0) ? List_int_Get(values, 0) : 0;
    for (int _i_20 = 1; _i_20 < values->count; _i_20++)
      if (List_int_Get(values, _i_20) < _minv_21) _minv_21 = List_int_Get(values, _i_20);
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "%d %d %d", _sum_17, _maxv_19, _minv_21);
    const char* _ps_22 = _sb1;
    printf("%s\n", _ps_22 ? _ps_22 : "");
    int _any_24 = 0;
    for (int _i_23 = 0; _i_23 < values->count; _i_23++)
      if (_lambda_3(List_int_Get(values, _i_23))) { _any_24 = 1; break; }
    int _all_26 = 1;
    for (int _i_25 = 0; _i_25 < values->count; _i_25++)
      if (!_lambda_4(List_int_Get(values, _i_25))) { _all_26 = 0; break; }
    char* _sb2 = _cs2sx_next_buf();
    snprintf(_sb2, CS2SX_STRBUF_SIZE, "%d %d", _any_24, _all_26);
    const char* _ps_27 = _sb2;
    printf("%s\n", _ps_27 ? _ps_27 : "");
    int _first_29 = 0;
    for (int _i_28 = 0; _i_28 < values->count; _i_28++)
    {
        if (_lambda_5(List_int_Get(values, _i_28))) { _first_29 = List_int_Get(values, _i_28); break; }
    }
    int _cnt_31 = 0;
    for (int _i_30 = 0; _i_30 < values->count; _i_30++)
      if (_lambda_6(List_int_Get(values, _i_30))) _cnt_31++;
    char* _sb3 = _cs2sx_next_buf();
    snprintf(_sb3, CS2SX_STRBUF_SIZE, "%d %d", _first_29, _cnt_31);
    const char* _ps_32 = _sb3;
    printf("%s\n", _ps_32 ? _ps_32 : "");
    List_Enemy* _list_33 = List_Enemy_New();
    List_Enemy_Add(_list_33, Enemy_New("orc", 30));
    List_Enemy_Add(_list_33, Enemy_New("bat", 5));
    List_Enemy_Add(_list_33, Enemy_New("troll", 80));
    List_Enemy* enemies = _list_33;
    List_Enemy* _where_34 = List_Enemy_New();
    for (int _i_35 = 0; _i_35 < enemies->count; _i_35++)
    {
        Enemy* _e__where_34 = List_Enemy_Get(enemies, _i_35);
        if (_lambda_7(_e__where_34)) List_Enemy_Add(_where_34, _e__where_34);
    }
    List_Enemy* _ord_36 = List_Enemy_New();
    for (int _i_37 = 0; _i_37 < _where_34->count; _i_37++)
      List_Enemy_Add(_ord_36, List_Enemy_Get(_where_34, _i_37));
    for (int _i_37 = 1; _i_37 < _ord_36->count; _i_37++)
    {
        Enemy* _otmp_39 = _ord_36->data[_i_37];
        int _j_38 = _i_37 - 1;
        while (_j_38 >= 0 && (_lambda_8(_ord_36->data[_j_38]) != _lambda_8(_otmp_39) ? (_lambda_8(_ord_36->data[_j_38]) < _lambda_8(_otmp_39)) : 0))
          { _ord_36->data[_j_38+1] = _ord_36->data[_j_38]; _j_38--; }
        _ord_36->data[_j_38+1] = _otmp_39;
    }
    List_Enemy* _tol_40 = List_Enemy_New();
    for (int _i_41 = 0; _i_41 < _ord_36->count; _i_41++)
      List_Enemy_Add(_tol_40, List_Enemy_Get(_ord_36, _i_41));
    List_Enemy* strong = _tol_40;
    const char* _ps_42 = List_Enemy_Get(strong, 0)->f_Name;
    printf("%s\n", _ps_42 ? _ps_42 : "");
    List_Enemy* _ord_43 = List_Enemy_New();
    for (int _i_44 = 0; _i_44 < enemies->count; _i_44++)
      List_Enemy_Add(_ord_43, List_Enemy_Get(enemies, _i_44));
    for (int _i_44 = 1; _i_44 < _ord_43->count; _i_44++)
    {
        Enemy* _otmp_46 = _ord_43->data[_i_44];
        int _j_45 = _i_44 - 1;
        while (_j_45 >= 0 && (_lambda_9(_ord_43->data[_j_45]) != _lambda_9(_otmp_46) ? (_lambda_9(_ord_43->data[_j_45]) > _lambda_9(_otmp_46)) : 0))
          { _ord_43->data[_j_45+1] = _ord_43->data[_j_45]; _j_45--; }
        _ord_43->data[_j_45+1] = _otmp_46;
    }
    Enemy* _first_47 = NULL;
    if (_ord_43->count > 0) { _first_47 = List_Enemy_Get(_ord_43, 0); }
    else { fprintf(stderr, "First()/Single(): sequence contains no elements\n"); abort(); }
    Enemy* weakest = _first_47;
    const char* _ps_48 = weakest->f_Name;
    printf("%s\n", _ps_48 ? _ps_48 : "");
}

void LinqApp_Free(LinqApp* self)
{
    if (!self) return;
    free(self);
}
