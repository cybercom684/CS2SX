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
    return v > 8;
}

static int _lambda_6(int v)
{
    return v > 0;
}

static int _lambda_7(int v)
{
    return v > 4;
}

static int _lambda_8(int v)
{
    return v < 5;
}

static int _lambda_9(int v)
{
    return v > 4;
}

static int _lambda_10(int v)
{
    return v < 5;
}

static int _lambda_11(Enemy* e)
{
    return e->f_Hp > 10;
}

static int _lambda_12(Enemy* e)
{
    return e->f_Hp;
}

static int _lambda_13(Enemy* e)
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
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "%d %d %d", evens->count, List_int_Get(doubled, 0), List_int_Get(sorted, 0));
    const char* _ps_15 = _sb1;
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
    char* _sb2 = _cs2sx_next_buf();
    snprintf(_sb2, CS2SX_STRBUF_SIZE, "%d %d %d", _sum_17, _maxv_19, _minv_21);
    int _sum_23 = 0;
    for (int _i_22 = 0; _i_22 < values->count; _i_22++)
      _sum_23 += List_int_Get(values, _i_22);
    int _maxv_25 = (values->count > 0) ? List_int_Get(values, 0) : 0;
    for (int _i_24 = 1; _i_24 < values->count; _i_24++)
      if (List_int_Get(values, _i_24) > _maxv_25) _maxv_25 = List_int_Get(values, _i_24);
    int _minv_27 = (values->count > 0) ? List_int_Get(values, 0) : 0;
    for (int _i_26 = 1; _i_26 < values->count; _i_26++)
      if (List_int_Get(values, _i_26) < _minv_27) _minv_27 = List_int_Get(values, _i_26);
    char* _sb3 = _cs2sx_next_buf();
    snprintf(_sb3, CS2SX_STRBUF_SIZE, "%d %d %d", _sum_23, _maxv_25, _minv_27);
    const char* _ps_28 = _sb3;
    printf("%s\n", _ps_28 ? _ps_28 : "");
    int _any_30 = 0;
    for (int _i_29 = 0; _i_29 < values->count; _i_29++)
      if (_lambda_3(List_int_Get(values, _i_29))) { _any_30 = 1; break; }
    int _all_32 = 1;
    for (int _i_31 = 0; _i_31 < values->count; _i_31++)
      if (!_lambda_4(List_int_Get(values, _i_31))) { _all_32 = 0; break; }
    char* _sb4 = _cs2sx_next_buf();
    snprintf(_sb4, CS2SX_STRBUF_SIZE, "%d %d", _any_30, _all_32);
    int _any_34 = 0;
    for (int _i_33 = 0; _i_33 < values->count; _i_33++)
      if (_lambda_5(List_int_Get(values, _i_33))) { _any_34 = 1; break; }
    int _all_36 = 1;
    for (int _i_35 = 0; _i_35 < values->count; _i_35++)
      if (!_lambda_6(List_int_Get(values, _i_35))) { _all_36 = 0; break; }
    char* _sb5 = _cs2sx_next_buf();
    snprintf(_sb5, CS2SX_STRBUF_SIZE, "%d %d", _any_34, _all_36);
    const char* _ps_37 = _sb5;
    printf("%s\n", _ps_37 ? _ps_37 : "");
    int _first_39 = 0;
    for (int _i_38 = 0; _i_38 < values->count; _i_38++)
    {
        if (_lambda_7(List_int_Get(values, _i_38))) { _first_39 = List_int_Get(values, _i_38); break; }
    }
    int _cnt_41 = 0;
    for (int _i_40 = 0; _i_40 < values->count; _i_40++)
      if (_lambda_8(List_int_Get(values, _i_40))) _cnt_41++;
    char* _sb6 = _cs2sx_next_buf();
    snprintf(_sb6, CS2SX_STRBUF_SIZE, "%d %d", _first_39, _cnt_41);
    int _first_43 = 0;
    for (int _i_42 = 0; _i_42 < values->count; _i_42++)
    {
        if (_lambda_9(List_int_Get(values, _i_42))) { _first_43 = List_int_Get(values, _i_42); break; }
    }
    int _cnt_45 = 0;
    for (int _i_44 = 0; _i_44 < values->count; _i_44++)
      if (_lambda_10(List_int_Get(values, _i_44))) _cnt_45++;
    char* _sb7 = _cs2sx_next_buf();
    snprintf(_sb7, CS2SX_STRBUF_SIZE, "%d %d", _first_43, _cnt_45);
    const char* _ps_46 = _sb7;
    printf("%s\n", _ps_46 ? _ps_46 : "");
    List_Enemy* _list_47 = List_Enemy_New();
    List_Enemy_Add(_list_47, Enemy_New("orc", 30));
    List_Enemy_Add(_list_47, Enemy_New("bat", 5));
    List_Enemy_Add(_list_47, Enemy_New("troll", 80));
    List_Enemy* enemies = _list_47;
    List_Enemy* _where_48 = List_Enemy_New();
    for (int _i_49 = 0; _i_49 < enemies->count; _i_49++)
    {
        Enemy* _e__where_48 = List_Enemy_Get(enemies, _i_49);
        if (_lambda_11(_e__where_48)) List_Enemy_Add(_where_48, _e__where_48);
    }
    List_Enemy* _ord_50 = List_Enemy_New();
    for (int _i_51 = 0; _i_51 < _where_48->count; _i_51++)
      List_Enemy_Add(_ord_50, List_Enemy_Get(_where_48, _i_51));
    for (int _i_51 = 1; _i_51 < _ord_50->count; _i_51++)
    {
        Enemy* _otmp_53 = _ord_50->data[_i_51];
        int _j_52 = _i_51 - 1;
        while (_j_52 >= 0 && (_lambda_12(_ord_50->data[_j_52]) != _lambda_12(_otmp_53) ? (_lambda_12(_ord_50->data[_j_52]) < _lambda_12(_otmp_53)) : 0))
          { _ord_50->data[_j_52+1] = _ord_50->data[_j_52]; _j_52--; }
        _ord_50->data[_j_52+1] = _otmp_53;
    }
    List_Enemy* _tol_54 = List_Enemy_New();
    for (int _i_55 = 0; _i_55 < _ord_50->count; _i_55++)
      List_Enemy_Add(_tol_54, List_Enemy_Get(_ord_50, _i_55));
    List_Enemy* strong = _tol_54;
    printf("%d\n", List_Enemy_Get(strong, 0)->Name);
    List_Enemy* _ord_56 = List_Enemy_New();
    for (int _i_57 = 0; _i_57 < enemies->count; _i_57++)
      List_Enemy_Add(_ord_56, List_Enemy_Get(enemies, _i_57));
    for (int _i_57 = 1; _i_57 < _ord_56->count; _i_57++)
    {
        Enemy* _otmp_59 = _ord_56->data[_i_57];
        int _j_58 = _i_57 - 1;
        while (_j_58 >= 0 && (_lambda_13(_ord_56->data[_j_58]) != _lambda_13(_otmp_59) ? (_lambda_13(_ord_56->data[_j_58]) > _lambda_13(_otmp_59)) : 0))
          { _ord_56->data[_j_58+1] = _ord_56->data[_j_58]; _j_58--; }
        _ord_56->data[_j_58+1] = _otmp_59;
    }
    Enemy* _first_60 = NULL;
    if (_ord_56->count > 0) { _first_60 = List_Enemy_Get(_ord_56, 0); }
    else { fprintf(stderr, "First()/Single(): sequence contains no elements\n"); abort(); }
    int weakest = _first_60;
    printf("%d\n", weakest->Name);
}

void LinqApp_Free(LinqApp* self)
{
    if (!self) return;
    free(self);
}
