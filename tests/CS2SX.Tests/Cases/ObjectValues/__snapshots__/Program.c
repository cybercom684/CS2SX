#include <stdlib.h>
#include "_generics.h"
#include "_interfaces.h"
#include "Program.h"

static const char* Rank__boxstr(void* b) { return Rank_ToString(((CS2SX_Box*)b)->v.i); }
const CS2SX_ClassInfo Rank_BoxInfo = { "Rank", NULL, Rank__boxstr };
void Foo_Ctor(Foo* self)
{
    CS2SX_TRACE("Foo..ctor", "Program.cs");
    self->f_A = 1;
}

Foo* Foo_New(void)
{
    Foo* self = (Foo*)calloc(1, sizeof(Foo));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Foo__Destroy;
    self->_gc = Foo_Gc;
    self->_class = &Foo_ClassInfo;
    Foo_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Foo_Free);
    return self;
}

const CS2SX_ClassInfo Foo_ClassInfo = { "Foo", NULL };

void Foo_Gc(void* _self, CS2SX_GcVisit visit)
{
    Foo* self = (Foo*)_self; (void)self;
}

static void Foo_GcDyn(void* self, CS2SX_GcVisit visit) { ((Foo*)self)->_gc(self, visit); }
const CS2SX_GcKind Foo_GcKind = { offsetof(Foo, _rc), Foo_GcDyn, (CS2SX_FreeFn)Foo_Free };

void Foo__Destroy(Foo* self)
{
}

void Foo_Free(Foo* self)
{
    if (!self) return;
    Foo* _o = (Foo*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Foo_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Foo* Foo_Retain(Foo* self)
{
    if (self) ((Foo*)self)->_rc++;
    return self;
}

void Bar_Ctor(Bar* self)
{
    CS2SX_TRACE("Bar..ctor", "Program.cs");
    Foo_Ctor((Foo*)self);
}

Bar* Bar_New(void)
{
    Bar* self = (Bar*)calloc(1, sizeof(Bar));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Bar__Destroy;
    self->base._gc = Bar_Gc;
    self->base._class = &Bar_ClassInfo;
    Bar_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Bar_Free);
    return self;
}

int Bar_Size(Bar* self)
{
    CS2SX_TRACE("Bar.Size", "Program.cs");
    CS2SX_LINE(10);
    return 7;
}

const CS2SX_ClassInfo Bar_ClassInfo = { "Bar", &Foo_ClassInfo };

void Bar_Gc(void* _self, CS2SX_GcVisit visit)
{
    Bar* self = (Bar*)_self; (void)self;
    Foo_Gc(_self, visit);
}

void Bar__Destroy(Bar* self)
{
    Foo__Destroy((Foo*)self);
}

void Bar_Free(Bar* self)
{
    if (!self) return;
    Foo* _o = (Foo*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Foo_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Bar* Bar_Retain(Bar* self)
{
    if (self) ((Foo*)self)->_rc++;
    return self;
}

IShape_vtable Bar_IShape_vtable_instance =
{
    .Size = (int (*)(void*))Bar_Size,
};


void Other_Ctor(Other* self)
{
    CS2SX_TRACE("Other..ctor", "Program.cs");
}

Other* Other_New(void)
{
    Other* self = (Other*)calloc(1, sizeof(Other));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Other__Destroy;
    self->_gc = Other_Gc;
    self->_class = &Other_ClassInfo;
    Other_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Other_Free);
    return self;
}

const CS2SX_ClassInfo Other_ClassInfo = { "Other", NULL };

void Other_Gc(void* _self, CS2SX_GcVisit visit)
{
    Other* self = (Other*)_self; (void)self;
}

static void Other_GcDyn(void* self, CS2SX_GcVisit visit) { ((Other*)self)->_gc(self, visit); }
const CS2SX_GcKind Other_GcKind = { offsetof(Other, _rc), Other_GcDyn, (CS2SX_FreeFn)Other_Free };

void Other__Destroy(Other* self)
{
}

void Other_Free(Other* self)
{
    if (!self) return;
    Other* _o = (Other*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Other_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Other* Other_Retain(Other* self)
{
    if (self) ((Other*)self)->_rc++;
    return self;
}

void ObjectValuesApp_Init(ObjectValuesApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))ObjectValuesApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))ObjectValuesApp_OnFrame;
    self->f_bag = List_voidptr_Retain(List_voidptr_New());
    self->f_props = Dict_str_voidptr_Retain(Dict_str_voidptr_New());
}

const char* ObjectValuesApp_Describe(void* o)
{
    CS2SX_TRACE("ObjectValuesApp.Describe", "Program.cs");
    CS2SX_LINE(22);
    switch (0) { default:
        if (o == NULL)
        {
            CS2SX_LINE(24);
            return "null";
        }
        else if (CS2SX_Obj_Is(o, &CS2SX_Box_String))
        {
            const char* s = ((CS2SX_Box*)(o))->v.s;
            CS2SX_LINE(25);
            char* _sb0 = _cs2sx_next_buf();
            _sb0 = CS2SX_Format(_sb0, "string %s", s);
            return _sb0;
        }
        else if (CS2SX_Obj_Is(o, &CS2SX_Box_Int32))
        {
            int n = ((int)((CS2SX_Box*)(o))->v.i);
            CS2SX_LINE(26);
            char* _sb1 = _cs2sx_next_buf();
            _sb1 = CS2SX_Format(_sb1, "int %d", (n + 1));
            return _sb1;
        }
        else if (CS2SX_Obj_Is(o, &CS2SX_Box_Double))
        {
            double d = ((CS2SX_Box*)(o))->v.d;
            CS2SX_LINE(27);
            char* _sb2 = _cs2sx_next_buf();
            _sb2 = CS2SX_Format(_sb2, "double %s", Double_ToString(d));
            return _sb2;
        }
        else if (CS2SX_Obj_Is(o, &Rank_BoxInfo))
        {
            Rank r = ((Rank)((CS2SX_Box*)(o))->v.i);
            CS2SX_LINE(28);
            char* _sb3 = _cs2sx_next_buf();
            _sb3 = CS2SX_Format(_sb3, "rank %s", Rank_ToString(r));
            return _sb3;
        }
        else if (CS2SX_Obj_Is(o, &Bar_ClassInfo))
        {
            Bar* b = ((Bar*)(o));
            CS2SX_LINE(29);
            char* _sb4 = _cs2sx_next_buf();
            _sb4 = CS2SX_Format(_sb4, "bar %d", Bar_Size(CS2SX_NN(b)));
            return _sb4;
        }
        else if (CS2SX_Obj_Is(o, &Foo_ClassInfo))
        {
            Foo* f = ((Foo*)(o));
            CS2SX_LINE(30);
            char* _sb5 = _cs2sx_next_buf();
            _sb5 = CS2SX_Format(_sb5, "foo %d", CS2SX_NN(f)->f_A);
            return _sb5;
        }
        else
        {
            CS2SX_LINE(31);
            char* _sb6 = _cs2sx_next_buf();
            _sb6 = CS2SX_Format(_sb6, "other %s", CS2SX_Obj_ToString(o));
            return _sb6;
        }
    }
}

void* ObjectValuesApp_Pick(int i)
{
    CS2SX_TRACE("ObjectValuesApp.Pick", "Program.cs");
    CS2SX_LINE(35);
    return (i == 0 ? CS2SX_Box_Str("zero") : (i == 1 ? CS2SX_Box_D(&CS2SX_Box_Single, (double)(1.5f)) : ((void*)(Foo_New()))));
}

void ObjectValuesApp_OnInit(ObjectValuesApp* self)
{
    CS2SX_TRACE("ObjectValuesApp.OnInit", "Program.cs");
    CS2SX_LINE(39);
    void* a = ((void*)(Bar_New()));
    CS2SX_LINE(40);
    void* b = CS2SX_Box_Str("text");
    CS2SX_LINE(41);
    void* c = CS2SX_Box_I(&CS2SX_Box_Int32, (long long)(5));
    CS2SX_LINE(42);
    void* d = CS2SX_Box_I(&Rank_BoxInfo, (long long)(Rank_Gold));
    CS2SX_LINE(43);
    IShape* shape = Bar_as_IShape(Bar_New());
    CS2SX_LINE(44);
    void* e = ({ CS2SX_IfaceHdr* _cs2sx_ih = (CS2SX_IfaceHdr*)(shape); _cs2sx_ih ? _cs2sx_ih->obj : NULL; });
    CS2SX_LINE(45);
    char* _sb7 = _cs2sx_next_buf();
    _sb7 = CS2SX_Format(_sb7, "is %s %s %s %s %s %s %s %s", CS2SX_BoolStr((CS2SX_Obj_Is(a, &Foo_ClassInfo))), CS2SX_BoolStr((CS2SX_Obj_Is(a, &Bar_ClassInfo))), CS2SX_BoolStr((CS2SX_Obj_Is(b, &Foo_ClassInfo))), CS2SX_BoolStr((CS2SX_Obj_Is(b, &CS2SX_Box_String))), CS2SX_BoolStr((CS2SX_Obj_Is(c, &CS2SX_Box_Int32))), CS2SX_BoolStr((CS2SX_Obj_Is(c, &CS2SX_Box_Int64))), CS2SX_BoolStr((CS2SX_Obj_Is(d, &Rank_BoxInfo))), CS2SX_BoolStr((CS2SX_Obj_Is(e, &Bar_ClassInfo))));
    const char* _ps_0 = _sb7;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(48);
    List_voidptr_Add(self->f_bag, a);
    CS2SX_LINE(48);
    List_voidptr_Add(self->f_bag, b);
    CS2SX_LINE(48);
    List_voidptr_Add(self->f_bag, c);
    CS2SX_LINE(48);
    List_voidptr_Add(self->f_bag, d);
    CS2SX_LINE(48);
    List_voidptr_Add(self->f_bag, ((void*)(Other_New())));
    CS2SX_LINE(48);
    List_voidptr_Add(self->f_bag, CS2SX_Box_I(&CS2SX_Box_Boolean, (long long)(1)));
    CS2SX_LINE(48);
    List_voidptr_Add(self->f_bag, CS2SX_Box_I(&CS2SX_Box_Char, (long long)('x')));
    CS2SX_LINE(49);
    for (int _i_o = 0; _i_o < (int)(self->f_bag->count); _i_o++)
    {
        void* o = List_voidptr_Get(self->f_bag, _i_o);
        const char* _ps_1 = ObjectValuesApp_Describe(o);
        printf("%s\n", _ps_1 ? _ps_1 : "");
    }
    CS2SX_LINE(51);
    char* _sb8 = _cs2sx_next_buf();
    _sb8 = CS2SX_Format(_sb8, "%s %s %s", ObjectValuesApp_Describe(ObjectValuesApp_Pick(1)), ObjectValuesApp_Describe(ObjectValuesApp_Pick(2)), ObjectValuesApp_Describe(NULL));
    const char* _ps_2 = _sb8;
    printf("%s\n", _ps_2 ? _ps_2 : "");
    CS2SX_LINE(53);
    char* _sb9 = _cs2sx_next_buf();
    _sb9 = CS2SX_Format(_sb9, "cast %d %s %d %s", (((int)CS2SX_Unbox_I(c, &CS2SX_Box_Int32)) + 1), CS2SX_Unbox_Str(b), CS2SX_NN((((Foo*)CS2SX_Obj_Cast(a, &Foo_ClassInfo))))->f_A, Rank_ToString(((Rank)CS2SX_Unbox_I(d, &Rank_BoxInfo))));
    const char* _ps_3 = _sb9;
    printf("%s\n", _ps_3 ? _ps_3 : "");
    CS2SX_LINE(54);
    char* _sb10 = _cs2sx_next_buf();
    _sb10 = CS2SX_Format(_sb10, "as %s %s %s", (((({ void* _cs2sx_as = (b); CS2SX_Obj_Is(_cs2sx_as, &CS2SX_Box_String) ? ((CS2SX_Box*)_cs2sx_as)->v.s : NULL; })) ?: "-")), CS2SX_BoolStr(((({ __auto_type _cs2sx_as = (a); CS2SX_Obj_Is(_cs2sx_as, &Foo_ClassInfo) ? ((Foo*)(_cs2sx_as)) : NULL; })) != NULL)), CS2SX_BoolStr(((({ __auto_type _cs2sx_as = (b); CS2SX_Obj_Is(_cs2sx_as, &Foo_ClassInfo) ? ((Foo*)(_cs2sx_as)) : NULL; })) == NULL)));
    const char* _ps_4 = _sb10;
    printf("%s\n", _ps_4 ? _ps_4 : "");
    CS2SX_LINE(55);
    if (CS2SX_Obj_Is(c, &CS2SX_Box_Int32))
    {
        int n = ((int)((CS2SX_Box*)(c))->v.i);
        char* _sb11 = _cs2sx_next_buf();
        _sb11 = CS2SX_Format(_sb11, "bound %d", (n * 2));
        const char* _ps_5 = _sb11;
        printf("%s\n", _ps_5 ? _ps_5 : "");
    }
    CS2SX_LINE(56);
    const char* s = {0};
    if (({ __auto_type _cs2sx_ps = (b); (CS2SX_Obj_Is(_cs2sx_ps, &CS2SX_Box_String)) ? (s = ((CS2SX_Box*)(_cs2sx_ps))->v.s, 1) : 0; }) && strlen(CS2SX_NN(s)) > 2)
    {
        CS2SX_LINE(56);
        char* _sb12 = _cs2sx_next_buf();
        _sb12 = CS2SX_Format(_sb12, "bound %s", s);
        const char* _ps_6 = _sb12;
        printf("%s\n", _ps_6 ? _ps_6 : "");
    }
    CS2SX_LINE(57);
    int m = {0};
    const char* _ps_7 = (({ __auto_type _cs2sx_ps = (c); (CS2SX_Obj_Is(_cs2sx_ps, &CS2SX_Box_Int32)) ? (m = ((int)((CS2SX_Box*)(_cs2sx_ps))->v.i), 1) : 0; }) && m > 3 ? ({ char* _sb13 = _cs2sx_next_buf(); _sb13 = CS2SX_Format(_sb13, "big %d", m); _sb13; }) : "small");
    printf("%s\n", _ps_7 ? _ps_7 : "");
    CS2SX_LINE(58);
    const char* t = ({ __auto_type _sw_8 = (c); const char* s2 = {0}; int i = {0}; (((CS2SX_Obj_Is(_sw_8, &CS2SX_Box_String)) ? (s2 = ((CS2SX_Box*)(_sw_8))->v.s, 1) : 0) ? s2 : (((CS2SX_Obj_Is(_sw_8, &CS2SX_Box_Int32)) ? (i = ((int)((CS2SX_Box*)(_sw_8))->v.i), 1) : 0) ? ({ char* _sb14 = _cs2sx_next_buf();
    _sb14 = CS2SX_Format(_sb14, "Zahl %d", i); _sb14; }) : (CS2SX_Obj_Is(_sw_8, &Foo_ClassInfo) ? "foo" : "?"))); });
    CS2SX_LINE(59);
    const char* t2 = ({ __auto_type _sw_9 = (a); const char* s3 = {0}; int i2 = {0}; Foo* f3 = {0}; (((CS2SX_Obj_Is(_sw_9, &CS2SX_Box_String)) ? (s3 = ((CS2SX_Box*)(_sw_9))->v.s, 1) : 0) ? s3 : (((CS2SX_Obj_Is(_sw_9, &CS2SX_Box_Int32)) ? (i2 = ((int)((CS2SX_Box*)(_sw_9))->v.i), 1) : 0) ? ({ char* _sb15 = _cs2sx_next_buf();
    _sb15 = CS2SX_Format(_sb15, "Zahl %d", i2); _sb15; }) : (((CS2SX_Obj_Is(_sw_9, &Foo_ClassInfo)) ? (f3 = ((Foo*)(_sw_9)), 1) : 0) ? ({ char* _sb16 = _cs2sx_next_buf();
    _sb16 = CS2SX_Format(_sb16, "foo %d", CS2SX_NN(f3)->f_A); _sb16; }) : "?"))); });
    CS2SX_LINE(60);
    char* _sb17 = _cs2sx_next_buf();
    _sb17 = CS2SX_Format(_sb17, "switch %s %s", t, t2);
    const char* _ps_10 = _sb17;
    printf("%s\n", _ps_10 ? _ps_10 : "");
    CS2SX_LINE(61);
    char* _sb18 = _cs2sx_next_buf();
    _sb18 = CS2SX_Format(_sb18, "print %s %s %s %s %s %s %s %s", CS2SX_Obj_ToString(a), CS2SX_Obj_ToString(b), CS2SX_Obj_ToString(c), CS2SX_Obj_ToString(d), CS2SX_Obj_ToString(List_voidptr_Get(self->f_bag, 5)), CS2SX_Obj_ToString(List_voidptr_Get(self->f_bag, 6)), CS2SX_Obj_ToString(CS2SX_NN(c)), CS2SX_Obj_ToString(d));
    const char* _ps_11 = _sb18;
    printf("%s\n", _ps_11 ? _ps_11 : "");
    CS2SX_LINE(64);
    {
        CS2SX_ExFrame _exf_12;
        CS2SX_ExPush(&_exf_12);
        if (setjmp(_exf_12.buf) == 0)
        {
            CS2SX_LINE(64);
            const char* bad = CS2SX_Unbox_Str(c);
            CS2SX_LINE(64);
            const char* _ps_13 = bad;
            printf("%s\n", _ps_13 ? _ps_13 : "");
            CS2SX_ExPop(&_exf_12);
        }
        else
        {
            Exception* _exf_12_ex = _exf_12.ex;
            if (CS2SX_Exc_Is(_exf_12_ex, &CS2SX_TI_InvalidCastException))
            {
                InvalidCastException* ex = (InvalidCastException*)_exf_12_ex;
                CS2SX_LINE(65);
                char* _sb19 = _cs2sx_next_buf();
                _sb19 = CS2SX_Format(_sb19, "caught %s", ((Exception*)(ex))->f_Message);
                const char* _ps_14 = _sb19;
                printf("%s\n", _ps_14 ? _ps_14 : "");
            }
            else CS2SX_Throw(_exf_12_ex);
        }
    }
    CS2SX_LINE(66);
    {
        CS2SX_ExFrame _exf_15;
        CS2SX_ExPush(&_exf_15);
        if (setjmp(_exf_15.buf) == 0)
        {
            CS2SX_LINE(66);
            long bad2 = ((long long)CS2SX_Unbox_I(c, &CS2SX_Box_Int64));
            CS2SX_LINE(66);
            printf("%lld\n", bad2);
            CS2SX_ExPop(&_exf_15);
        }
        else
        {
            Exception* _exf_15_ex = _exf_15.ex;
            if (CS2SX_Exc_Is(_exf_15_ex, &CS2SX_TI_InvalidCastException))
            {
                CS2SX_LINE(67);
                printf("caught long\n");
                Exception_Free(_exf_15_ex);
            }
            else CS2SX_Throw(_exf_15_ex);
        }
    }
    CS2SX_LINE(68);
    Foo* plain = Foo_New();
    CS2SX_LINE(69);
    {
        CS2SX_ExFrame _exf_16;
        CS2SX_ExPush(&_exf_16);
        if (setjmp(_exf_16.buf) == 0)
        {
            CS2SX_LINE(69);
            Bar* bad3 = ((Bar*)CS2SX_Obj_Cast(plain, &Bar_ClassInfo));
            CS2SX_LINE(69);
            printf("%d\n", Bar_Size(CS2SX_NN(bad3)));
            CS2SX_ExPop(&_exf_16);
        }
        else
        {
            Exception* _exf_16_ex = _exf_16.ex;
            if (CS2SX_Exc_Is(_exf_16_ex, &CS2SX_TI_InvalidCastException))
            {
                CS2SX_LINE(70);
                printf("caught bar\n");
                Exception_Free(_exf_16_ex);
            }
            else CS2SX_Throw(_exf_16_ex);
        }
    }
    CS2SX_LINE(72);
    Dict_str_voidptr_Set(self->f_props, "name", CS2SX_Box_Str("Mia"));
    CS2SX_LINE(73);
    Dict_str_voidptr_Set(self->f_props, "level", CS2SX_Box_I(&CS2SX_Box_Int32, (long long)(3)));
    CS2SX_LINE(74);
    Dict_str_voidptr_Set(self->f_props, "owner", ((void*)(Foo_New())));
    CS2SX_LINE(75);
    int level = ((int)CS2SX_Unbox_I(Dict_str_voidptr_Get(self->f_props, "level"), &CS2SX_Box_Int32));
    CS2SX_LINE(76);
    char* _sb20 = _cs2sx_next_buf();
    _sb20 = CS2SX_Format(_sb20, "props %s %d %s", CS2SX_Obj_ToString(Dict_str_voidptr_Get(self->f_props, "name")), (level + 1), CS2SX_BoolStr((CS2SX_Obj_Is(Dict_str_voidptr_Get(self->f_props, "owner"), &Foo_ClassInfo))));
    const char* _ps_17 = _sb20;
    printf("%s\n", _ps_17 ? _ps_17 : "");
    CS2SX_LINE(79);
    char* _sb21 = _cs2sx_next_buf();
    _sb21 = CS2SX_Format(_sb21, "frame text %d", 41);
    CS2SX_RC_STORE(CS2SX_Obj_Retain, CS2SX_Obj_Release, self->f_held, CS2SX_Box_Str(_sb21));
}

void ObjectValuesApp_OnFrame(ObjectValuesApp* self)
{
    CS2SX_TRACE("ObjectValuesApp.OnFrame", "Program.cs");
    CS2SX_LINE(84);
    self->f_frame++;
    CS2SX_LINE(85);
    if (self->f_frame == 3)
    {
        CS2SX_LINE(87);
        char* _sb22 = _cs2sx_next_buf();
        _sb22 = CS2SX_Format(_sb22, "held %s %d %s %s", CS2SX_Obj_ToString(self->f_held), CS2SX_NN(self->f_bag)->count, CS2SX_Obj_ToString(List_voidptr_Get(self->f_bag, 1)), CS2SX_Obj_ToString(Dict_str_voidptr_Get(self->f_props, "name")));
        const char* _ps_18 = _sb22;
        printf("%s\n", _ps_18 ? _ps_18 : "");
        CS2SX_LINE(88);
        CS2SX_RC_STORE(CS2SX_Obj_Retain, CS2SX_Obj_Release, self->f_held, CS2SX_Box_I(&CS2SX_Box_Int32, (long long)(7)));
    }
    CS2SX_LINE(90);
    if (self->f_frame == 5)
    {
        CS2SX_LINE(92);
        char* _sb23 = _cs2sx_next_buf();
        _sb23 = CS2SX_Format(_sb23, "held %s %s", CS2SX_Obj_ToString(self->f_held), CS2SX_BoolStr((CS2SX_Obj_Is(self->f_held, &CS2SX_Box_Int32))));
        const char* _ps_19 = _sb23;
        printf("%s\n", _ps_19 ? _ps_19 : "");
        CS2SX_LINE(93);
        printf("done\n");
    }
}

void ObjectValuesApp_Free(ObjectValuesApp* self)
{
    if (!self) return;
    if (self->f_held) { CS2SX_Obj_Release(self->f_held); self->f_held = NULL; }
    if (self->f_bag) { List_voidptr_Free(self->f_bag); self->f_bag = NULL; }
    if (self->f_props) { Dict_str_voidptr_Free(self->f_props); self->f_props = NULL; }
    free(self);
}
