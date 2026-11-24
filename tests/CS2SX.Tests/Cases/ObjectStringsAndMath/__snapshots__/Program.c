#include <stdlib.h>
#include "_generics.h"
#include "Program.h"

void Plain_Ctor(Plain* self)
{
    CS2SX_TRACE("Plain..ctor", "Program.cs");
    self->f_V = 1;
}

Plain* Plain_New(void)
{
    Plain* self = (Plain*)calloc(1, sizeof(Plain));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Plain__Destroy;
    self->_gc = Plain_Gc;
    self->_class = &Plain_ClassInfo;
    Plain_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Plain_Free);
    return self;
}

const CS2SX_ClassInfo Plain_ClassInfo = { "Plain", NULL, NULL };

void Plain_Gc(void* _self, CS2SX_GcVisit visit)
{
    Plain* self = (Plain*)_self; (void)self;
}

static void Plain_GcDyn(void* self, CS2SX_GcVisit visit) { ((Plain*)self)->_gc(self, visit); }
const CS2SX_GcKind Plain_GcKind = { offsetof(Plain, _rc), Plain_GcDyn, (CS2SX_FreeFn)Plain_Free };

void Plain__Destroy(Plain* self)
{
}

void Plain_Free(Plain* self)
{
    if (!self) return;
    Plain* _o = (Plain*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Plain_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Plain* Plain_Retain(Plain* self)
{
    if (self) ((Plain*)self)->_rc++;
    return self;
}

void Named_Ctor(Named* self, const char* n)
{
    CS2SX_TRACE("Named..ctor", "Program.cs");
    CS2SX_LINE(15);
    CS2SX_STR_STORE(self->f_N, n);
}

Named* Named_New(const char* n)
{
    Named* self = (Named*)calloc(1, sizeof(Named));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Named__Destroy;
    self->_gc = Named_Gc;
    self->_class = &Named_ClassInfo;
    Named_Ctor(self, n);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Named_Free);
    return self;
}

const char* Named_ToString(Named* self)
{
    CS2SX_TRACE("Named.ToString", "Program.cs");
    CS2SX_LINE(16);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "Named(%s)", self->f_N);
    return _sb0;
}

const CS2SX_ClassInfo Named_ClassInfo = { "Named", NULL, (const char* (*)(void*))Named_ToString };

void Named_Gc(void* _self, CS2SX_GcVisit visit)
{
    Named* self = (Named*)_self; (void)self;
}

static void Named_GcDyn(void* self, CS2SX_GcVisit visit) { ((Named*)self)->_gc(self, visit); }
const CS2SX_GcKind Named_GcKind = { offsetof(Named, _rc), Named_GcDyn, (CS2SX_FreeFn)Named_Free };

void Named__Destroy(Named* self)
{
    CS2SX_StrDrop(self->f_N); self->f_N = NULL;
}

void Named_Free(Named* self)
{
    if (!self) return;
    Named* _o = (Named*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Named_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Named* Named_Retain(Named* self)
{
    if (self) ((Named*)self)->_rc++;
    return self;
}

static Animal_vtable Animal_vtable_instance =
{
    .Sound = (const char*(*)(void*))Animal_Sound,
};

void Animal_Ctor(Animal* self)
{
    CS2SX_TRACE("Animal..ctor", "Program.cs");
}

Animal* Animal_New(void)
{
    Animal* self = (Animal*)calloc(1, sizeof(Animal));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Animal__Destroy;
    self->_gc = Animal_Gc;
    self->_class = &Animal_ClassInfo;
    self->vtable = &Animal_vtable_instance;
    Animal_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Animal_Free);
    return self;
}

const char* Animal_Sound(Animal* self)
{
    CS2SX_TRACE("Animal.Sound", "Program.cs");
    CS2SX_LINE(21);
    return "...";
}

const char* Animal_ToString(Animal* self)
{
    CS2SX_TRACE("Animal.ToString", "Program.cs");
    CS2SX_LINE(22);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "Animal:%s", self->vtable->Sound(self));
    return _sb0;
}

const CS2SX_ClassInfo Animal_ClassInfo = { "Animal", NULL, (const char* (*)(void*))Animal_ToString };

void Animal_Gc(void* _self, CS2SX_GcVisit visit)
{
    Animal* self = (Animal*)_self; (void)self;
}

static void Animal_GcDyn(void* self, CS2SX_GcVisit visit) { ((Animal*)self)->_gc(self, visit); }
const CS2SX_GcKind Animal_GcKind = { offsetof(Animal, _rc), Animal_GcDyn, (CS2SX_FreeFn)Animal_Free };

void Animal__Destroy(Animal* self)
{
}

void Animal_Free(Animal* self)
{
    if (!self) return;
    Animal* _o = (Animal*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Animal_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Animal* Animal_Retain(Animal* self)
{
    if (self) ((Animal*)self)->_rc++;
    return self;
}

static Animal_vtable Dog_vtable_instance;

void Dog_Ctor(Dog* self)
{
    CS2SX_TRACE("Dog..ctor", "Program.cs");
    Animal_Ctor((Animal*)self);
}

Dog* Dog_New(void)
{
    Dog* self = (Dog*)calloc(1, sizeof(Dog));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Dog__Destroy;
    self->base._gc = Dog_Gc;
    self->base._class = &Dog_ClassInfo;
    self->base.vtable = &Dog_vtable_instance;
    Dog_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Dog_Free);
    return self;
}

const char* Dog_Sound(Dog* self)
{
    CS2SX_TRACE("Dog.Sound", "Program.cs");
    CS2SX_LINE(27);
    return "Wuff";
}

const CS2SX_ClassInfo Dog_ClassInfo = { "Dog", &Animal_ClassInfo, (const char* (*)(void*))Animal_ToString };

void Dog_Gc(void* _self, CS2SX_GcVisit visit)
{
    Dog* self = (Dog*)_self; (void)self;
    Animal_Gc(_self, visit);
}

void Dog__Destroy(Dog* self)
{
    Animal__Destroy((Animal*)self);
}

void Dog_Free(Dog* self)
{
    if (!self) return;
    Animal* _o = (Animal*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Animal_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Dog* Dog_Retain(Dog* self)
{
    if (self) ((Animal*)self)->_rc++;
    return self;
}

static Animal_vtable Dog_vtable_instance =
{
    .Sound = (const char*(*)(void*))Dog_Sound,
};

static Animal_vtable Cat_vtable_instance;

void Cat_Ctor(Cat* self)
{
    CS2SX_TRACE("Cat..ctor", "Program.cs");
    Animal_Ctor((Animal*)self);
}

Cat* Cat_New(void)
{
    Cat* self = (Cat*)calloc(1, sizeof(Cat));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Cat__Destroy;
    self->base._gc = Cat_Gc;
    self->base._class = &Cat_ClassInfo;
    self->base.vtable = &Cat_vtable_instance;
    Cat_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Cat_Free);
    return self;
}

const char* Cat_Sound(Cat* self)
{
    CS2SX_TRACE("Cat.Sound", "Program.cs");
    CS2SX_LINE(32);
    return "Miau";
}

const char* Cat_ToString(Cat* self)
{
    CS2SX_TRACE("Cat.ToString", "Program.cs");
    CS2SX_LINE(33);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "Cat/%s", Animal_ToString((Animal*)self));
    return _sb0;
}

const char* Cat_Self(Cat* self)
{
    CS2SX_TRACE("Cat.Self", "Program.cs");
    CS2SX_LINE(34);
    char* _sb1 = _cs2sx_next_buf();
    _sb1 = CS2SX_Format(_sb1, "self=%s", CS2SX_Obj_ToString((void*)self));
    return _sb1;
}

const CS2SX_ClassInfo Cat_ClassInfo = { "Cat", &Animal_ClassInfo, (const char* (*)(void*))Cat_ToString };

void Cat_Gc(void* _self, CS2SX_GcVisit visit)
{
    Cat* self = (Cat*)_self; (void)self;
    Animal_Gc(_self, visit);
}

void Cat__Destroy(Cat* self)
{
    Animal__Destroy((Animal*)self);
}

void Cat_Free(Cat* self)
{
    if (!self) return;
    Animal* _o = (Animal*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Animal_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Cat* Cat_Retain(Cat* self)
{
    if (self) ((Animal*)self)->_rc++;
    return self;
}

static Animal_vtable Cat_vtable_instance =
{
    .Sound = (const char*(*)(void*))Cat_Sound,
};

// ── Struct: P2 ────────────────────────────────────────
P2 P2_New(int x, int y)
{
    P2 _self_value = {0};
    P2* self = &_self_value;
    CS2SX_LINE(40);
    self->X = x;
    CS2SX_LINE(40);
    self->Y = y;
    return _self_value;
}

const char* P2_ToString(P2* self)
{
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "P%d", (self->X + self->Y));
    return _sb0;
}

int P2_Equals(P2 a, P2 b)
{
    return memcmp(&a, &b, sizeof(P2)) == 0;
}

// ── Struct: Q ────────────────────────────────────────
int Q_Equals(Q a, Q b)
{
    return memcmp(&a, &b, sizeof(Q)) == 0;
}

void ObjectStringsAndMathApp_Init(ObjectStringsAndMathApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))ObjectStringsAndMathApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))ObjectStringsAndMathApp_OnFrame;
}

static int _lambda_0(int x)
{
    return x > 2;
}

static int _lambda_1(int x)
{
    return x < 2;
}

static int _lambda_2(int x)
{
    return x * 3;
}

static double _lambda_3(int x)
{
    return x * 1.5;
}

static double _lambda_4(int x)
{
    return x * 2;
}

static int _lambda_5(int x)
{
    return x % 2 == 1;
}

static int _lambda_6(int x)
{
    return x > 1;
}

void ObjectStringsAndMathApp_OnInit(ObjectStringsAndMathApp* self)
{
    CS2SX_TRACE("ObjectStringsAndMathApp.OnInit", "Program.cs");
    CS2SX_LINE(50);
    Plain* p = Plain_New();
    CS2SX_LINE(51);
    Named* n = Named_New("x");
    CS2SX_LINE(52);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "plain %s %s", CS2SX_Obj_ToString((void*)(p)), CS2SX_Obj_ToString((void*)(p)));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(53);
    char* _sb1 = _cs2sx_next_buf();
    _sb1 = CS2SX_Format(_sb1, "named %s %s %s", CS2SX_Obj_ToString((void*)(n)), CS2SX_Obj_ToString((void*)(n)), CS2SX_Obj_ToString((void*)(n)));
    const char* _ps_1 = _sb1;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    CS2SX_LINE(54);
    const char* _ps_2 = CS2SX_Obj_ToString((void*)(n));
    printf("%s\n", _ps_2 ? _ps_2 : "");
    CS2SX_LINE(55);
    Animal* a = (Animal*)(Dog_New());
    CS2SX_LINE(56);
    Animal* c = (Animal*)(Cat_New());
    CS2SX_LINE(57);
    char* _sb2 = _cs2sx_next_buf();
    _sb2 = CS2SX_Format(_sb2, "virt %s %s", CS2SX_Obj_ToString((void*)(a)), CS2SX_Obj_ToString((void*)(c)));
    const char* _ps_3 = _sb2;
    printf("%s\n", _ps_3 ? _ps_3 : "");
    CS2SX_LINE(58);
    const char* _ps_4 = Cat_Self(CS2SX_NN((((Cat*)CS2SX_Obj_Cast(c, &Cat_ClassInfo)))));
    printf("%s\n", _ps_4 ? _ps_4 : "");
    CS2SX_LINE(59);
    void* o = ((void*)(n));
    CS2SX_LINE(60);
    char* _sb3 = _cs2sx_next_buf();
    _sb3 = CS2SX_Format(_sb3, "obj %s", CS2SX_Obj_ToString(o));
    const char* _ps_5 = _sb3;
    printf("%s\n", _ps_5 ? _ps_5 : "");
    CS2SX_LINE(61);
    P2 pt = P2_New(1, 3);
    CS2SX_LINE(62);
    char* _sb4 = _cs2sx_next_buf();
    _sb4 = CS2SX_Format(_sb4, "struct %s %s %s %s", ({ __auto_type _ts = (pt); P2_ToString(&_ts); }), ({ __auto_type _ts = (pt); P2_ToString(&_ts); }), ({ __auto_type _ts = (pt); P2_ToString(&_ts); }), "Q");
    const char* _ps_6 = _sb4;
    printf("%s\n", _ps_6 ? _ps_6 : "");
    CS2SX_LINE(63);
    List_Named* _list_7 = List_Named_New();
    List_Named_Add(_list_7, Named_New("a"));
    List_Named_Add(_list_7, Named_New("b"));
    List_Named* list = _list_7;
    CS2SX_LINE(64);
    char* _sb5 = _cs2sx_next_buf();
    _sb5 = CS2SX_Format(_sb5, "join %s", ({ __auto_type _jl = (list); int _jn = (_jl ? _jl->count : 0); CS2SX_JOIN_EACH(",", _jn, "%s", CS2SX_Obj_ToString((void*)((_jl->data[_ji])))); }));
    const char* _ps_8 = _sb5;
    printf("%s\n", _ps_8 ? _ps_8 : "");
    CS2SX_LINE(65);
    Named* none = NULL;
    CS2SX_LINE(66);
    char* _sb6 = _cs2sx_next_buf();
    _sb6 = CS2SX_Format(_sb6, "null [%s]", CS2SX_Obj_ToString((void*)(none)));
    const char* _ps_9 = _sb6;
    printf("%s\n", _ps_9 ? _ps_9 : "");
    CS2SX_LINE(67);
    List_Animal* _list_10 = List_Animal_New();
    List_Animal_Add(_list_10, (Animal*)Dog_New());
    List_Animal_Add(_list_10, (Animal*)Cat_New());
    __auto_type _fe_11 = _list_10;
    for (int _i_an = 0; _i_an < (int)(_fe_11->count); _i_an++)
    {
        Animal* an = List_Animal_Get(_fe_11, _i_an);
        char* _sb7 = _cs2sx_next_buf();
        _sb7 = CS2SX_Format(_sb7, "%s;", CS2SX_Obj_ToString((void*)(an)));
        const char* _ps_12 = _sb7;
        printf("%s", _ps_12 ? _ps_12 : "");
    }
    CS2SX_LINE(69);
    printf("\n");
    CS2SX_LINE(71);
    char* _sb8 = _cs2sx_next_buf();
    _sb8 = CS2SX_Format(_sb8, "io %s %s %s %s", CS2SX_BoolStr(CS2SX_File_Exists("sdmc:/cs2sx-nope.txt")), CS2SX_Path_GetFileName("sdmc:/a/b.txt"), CS2SX_Path_GetExtension("x.png"), CS2SX_Path_GetFileNameWithoutExt("a/b.c"));
    const char* _ps_13 = _sb8;
    printf("%s\n", _ps_13 ? _ps_13 : "");
    CS2SX_LINE(74);
    List_int* _list_14 = List_int_New();
    List_int_Add(_list_14, 1);
    List_int_Add(_list_14, 2);
    List_int_Add(_list_14, 3);
    List_int_Add(_list_14, 4);
    List_int_Add(_list_14, 5);
    List_int* v = _list_14;
    CS2SX_LINE(75);
    int _cnt_16 = 0;
    for (int _i_15 = 0; _i_15 < v->count; _i_15++)
      if (_lambda_0(List_int_Get(v, _i_15))) _cnt_16++;
    int _cnt_18 = 0;
    for (int _i_17 = 0; _i_17 < v->count; _i_17++)
      if (_lambda_1(List_int_Get(v, _i_17))) _cnt_18++;
    int _maxv_20 = (v->count > 0) ? (int)_lambda_2(List_int_Get(v, 0)) : 0;
    for (int _i_19 = 1; _i_19 < v->count; _i_19++)
    { int _mv = (int)_lambda_2(List_int_Get(v, _i_19)); if (_mv > _maxv_20) _maxv_20 = _mv; }
    double _minv_22 = (v->count > 0) ? (double)_lambda_3(List_int_Get(v, 0)) : 0;
    for (int _i_21 = 1; _i_21 < v->count; _i_21++)
    { double _mv = (double)_lambda_3(List_int_Get(v, _i_21)); if (_mv < _minv_22) _minv_22 = _mv; }
    int _sum_24 = 0;
    for (int _i_23 = 0; _i_23 < v->count; _i_23++)
      _sum_24 += (int)_lambda_4(List_int_Get(v, _i_23));
    printf("linq %d %d %d %s %d\n", _cnt_16, _cnt_18, (v->count > 0 ? _maxv_20 : (CS2SX_ThrowNoElements(), _maxv_20)), Double_ToString((v->count > 0 ? _minv_22 : (CS2SX_ThrowNoElements(), _minv_22))), _sum_24);
    CS2SX_LINE(76);
    int* _toa_25 = (int*)CS2SX_ArrNew(v->count, sizeof(int));
    for (int _i_26 = 0; _i_26 < v->count; _i_26++)
      _toa_25[_i_26] = List_int_Get(v, _i_26);
    int* arr = _toa_25;
    CS2SX_LINE(77);
    List_int* _where_27 = List_int_New();
    for (int _i_28 = 0; _i_28 < v->count; _i_28++)
    {
        int _e__where_27 = List_int_Get(v, _i_28);
        if (_lambda_5(_e__where_27)) List_int_Add(_where_27, _e__where_27);
    }
    int* _toa_29 = (int*)CS2SX_ArrNew(_where_27->count, sizeof(int));
    for (int _i_30 = 0; _i_30 < _where_27->count; _i_30++)
      _toa_29[_i_30] = List_int_Get(_where_27, _i_30);
    int* odd = _toa_29;
    CS2SX_LINE(78);
    int* _toa_31 = (int*)CS2SX_ArrNew(v->count, sizeof(int));
    for (int _i_32 = 0; _i_32 < v->count; _i_32++)
      _toa_31[_i_32] = List_int_Get(v, _i_32);
    List_int* _where_33 = List_int_New();
    for (int _i_34 = 0; _i_34 < CS2SX_ArrLen(_toa_31); _i_34++)
    {
        int _e__where_33 = _toa_31[_i_34];
        if (_lambda_6(_e__where_33)) List_int_Add(_where_33, _e__where_33);
    }
    char* _sb9 = _cs2sx_next_buf();
    _sb9 = CS2SX_Format(_sb9, "arr %d %d %d %d %d", CS2SX_ArrLen(CS2SX_NN(arr)), CS2SX_AT(arr, 4), CS2SX_ArrLen(CS2SX_NN(odd)), CS2SX_AT(odd, 2), _where_33->count);
    const char* _ps_35 = _sb9;
    printf("%s\n", _ps_35 ? _ps_35 : "");
    CS2SX_LINE(81);
    char* _sb10 = _cs2sx_next_buf();
    _sb10 = CS2SX_Format(_sb10, "round %s %s %s %s %s", Double_ToString(rint(2.5)), Double_ToString(rint(3.5)), Double_ToString(rint(-2.5)), Double_ToString(round(2.5)), Double_ToString((rint((1.2345) * pow(10, (2))) / pow(10, (2)))));
    const char* _ps_36 = _sb10;
    printf("%s\n", _ps_36 ? _ps_36 : "");
    CS2SX_LINE(83);
    char* _sb11 = _cs2sx_next_buf();
    _sb11 = CS2SX_Format(_sb11, "base %s %s %s %s", CS2SX_ToBase((long long)(255), 32, 16), CS2SX_ToBase((long long)(5), 32, 2), CS2SX_ToBase((long long)(-1), 32, 16), CS2SX_ToBase((long long)(64), 32, 8));
    const char* _ps_37 = _sb11;
    printf("%s\n", _ps_37 ? _ps_37 : "");
    CS2SX_LINE(85);
    char* _sb12 = _cs2sx_next_buf();
    _sb12 = CS2SX_Format(_sb12, "date %s", CS2SX_BoolStr((CS2SX_Str16Len(CS2SX_NN(CS2SX_DT_ToString(CS2SX_DT_Now(), "yyyy-MM-dd HH:mm:ss"))) == 19)));
    const char* _ps_38 = _sb12;
    printf("%s\n", _ps_38 ? _ps_38 : "");
    CS2SX_LINE(87);
    int big = INT_MAX;
    CS2SX_LINE(88);
    {
        CS2SX_ExFrame _exf_39;
        CS2SX_ExPush(&_exf_39);
        if (setjmp(_exf_39.buf) == 0)
        {
            CS2SX_LINE(88);
            int r = (CS2SX_CHECKED(add, int, big, 1));
            CS2SX_LINE(88);
            char* _sb13 = _cs2sx_next_buf();
            _sb13 = CS2SX_Format(_sb13, "no %d", r);
            const char* _ps_40 = _sb13;
            printf("%s\n", _ps_40 ? _ps_40 : "");
            CS2SX_ExPop(&_exf_39);
        }
        else
        {
            Exception* _exf_39_ex = _exf_39.ex;
            if (CS2SX_Exc_Is(_exf_39_ex, &CS2SX_TI_OverflowException))
            {
                CS2SX_LINE(89);
                printf("ovf1 ");
                Exception_Free(_exf_39_ex);
            }
            else CS2SX_Throw(_exf_39_ex);
        }
    }
    CS2SX_LINE(90);
    {
        CS2SX_ExFrame _exf_41;
        CS2SX_ExPush(&_exf_41);
        if (setjmp(_exf_41.buf) == 0)
        {
            CS2SX_LINE(90);
            /* checked/unchecked — no overflow checking in C */
            {
                CS2SX_LINE(90);
                int t = big;
                CS2SX_LINE(90);
                t = CS2SX_CHECKED(add, int, t, 1);
                CS2SX_LINE(90);
                char* _sb14 = _cs2sx_next_buf();
                _sb14 = CS2SX_Format(_sb14, "no %d", t);
                const char* _ps_42 = _sb14;
                printf("%s\n", _ps_42 ? _ps_42 : "");
            }
            CS2SX_ExPop(&_exf_41);
        }
        else
        {
            Exception* _exf_41_ex = _exf_41.ex;
            if (CS2SX_Exc_Is(_exf_41_ex, &CS2SX_TI_OverflowException))
            {
                CS2SX_LINE(91);
                printf("ovf2 ");
                Exception_Free(_exf_41_ex);
            }
            else CS2SX_Throw(_exf_41_ex);
        }
    }
    CS2SX_LINE(92);
    long long l = 5000000000LL;
    CS2SX_LINE(93);
    {
        CS2SX_ExFrame _exf_43;
        CS2SX_ExPush(&_exf_43);
        if (setjmp(_exf_43.buf) == 0)
        {
            CS2SX_LINE(93);
            int x = (CS2SX_CHECKED(add, int, l, 0));
            CS2SX_LINE(93);
            char* _sb15 = _cs2sx_next_buf();
            _sb15 = CS2SX_Format(_sb15, "no %d", x);
            const char* _ps_44 = _sb15;
            printf("%s\n", _ps_44 ? _ps_44 : "");
            CS2SX_ExPop(&_exf_43);
        }
        else
        {
            Exception* _exf_43_ex = _exf_43.ex;
            if (CS2SX_Exc_Is(_exf_43_ex, &CS2SX_TI_OverflowException))
            {
                CS2SX_LINE(94);
                printf("ovf3 ");
                Exception_Free(_exf_43_ex);
            }
            else CS2SX_Throw(_exf_43_ex);
        }
    }
    CS2SX_LINE(95);
    {
        CS2SX_ExFrame _exf_45;
        CS2SX_ExPush(&_exf_45);
        if (setjmp(_exf_45.buf) == 0)
        {
            CS2SX_LINE(95);
            /* checked/unchecked — no overflow checking in C */
            {
                CS2SX_LINE(95);
                int m = CS2SX_CHECKED(mul, int, big, 2);
                CS2SX_LINE(95);
                char* _sb16 = _cs2sx_next_buf();
                _sb16 = CS2SX_Format(_sb16, "no %d", m);
                const char* _ps_46 = _sb16;
                printf("%s\n", _ps_46 ? _ps_46 : "");
            }
            CS2SX_ExPop(&_exf_45);
        }
        else
        {
            Exception* _exf_45_ex = _exf_45.ex;
            if (CS2SX_Exc_Is(_exf_45_ex, &CS2SX_TI_OverflowException))
            {
                OverflowException* e = (OverflowException*)_exf_45_ex;
                CS2SX_LINE(96);
                char* _sb17 = _cs2sx_next_buf();
                _sb17 = CS2SX_Format(_sb17, "ovf4 %s", ((Exception*)(e))->f_Message);
                const char* _ps_47 = _sb17;
                printf("%s\n", _ps_47 ? _ps_47 : "");
            }
            else CS2SX_Throw(_exf_45_ex);
        }
    }
    CS2SX_LINE(97);
    char* _sb18 = _cs2sx_next_buf();
    _sb18 = CS2SX_Format(_sb18, "unchecked %d %d", (big + 1), (CS2SX_CHECKED(sub, int, big, 1)));
    const char* _ps_48 = _sb18;
    printf("%s\n", _ps_48 ? _ps_48 : "");
    CS2SX_LINE(98);
    printf("done\n");
}

void ObjectStringsAndMathApp_OnFrame(ObjectStringsAndMathApp* self)
{
    CS2SX_TRACE("ObjectStringsAndMathApp.OnFrame", "Program.cs");
}

void ObjectStringsAndMathApp_Free(ObjectStringsAndMathApp* self)
{
    if (!self) return;
    free(self);
}
