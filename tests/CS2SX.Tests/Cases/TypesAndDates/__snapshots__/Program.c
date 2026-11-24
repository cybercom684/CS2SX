#include <stdlib.h>
#include "_generics.h"
#include "_interfaces.h"
#include "Program.h"
#include "CS2SX_Collections.h"

void Sq_Ctor(Sq* self)
{
    CS2SX_TRACE("Sq..ctor", "Program.cs");
    self->f_Side = 2;
}

Sq* Sq_New(void)
{
    Sq* self = (Sq*)calloc(1, sizeof(Sq));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Sq__Destroy;
    self->_gc = Sq_Gc;
    self->_class = &Sq_ClassInfo;
    Sq_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Sq_Free);
    return self;
}

int Sq_Corners(Sq* self)
{
    CS2SX_TRACE("Sq.Corners", "Program.cs");
    CS2SX_LINE(17);
    return 4;
}

const char* Sq_Describe(Sq* self)
{
    CS2SX_TRACE("Sq.Describe", "Program.cs");
    CS2SX_LINE(17);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "%s %s", Sq_get_Name(self), Double_ToString(Sq_get_Area(self)));
    return _sb0;
}

const CS2SX_ClassInfo Sq_ClassInfo = { "Sq", NULL, NULL };

void Sq_Gc(void* _self, CS2SX_GcVisit visit)
{
    Sq* self = (Sq*)_self; (void)self;
}

static void Sq_GcDyn(void* self, CS2SX_GcVisit visit) { ((Sq*)self)->_gc(self, visit); }
const CS2SX_GcKind Sq_GcKind = { offsetof(Sq, _rc), Sq_GcDyn, (CS2SX_FreeFn)Sq_Free };

void Sq__Destroy(Sq* self)
{
}

void Sq_Free(Sq* self)
{
    if (!self) return;
    Sq* _o = (Sq*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Sq_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Sq* Sq_Retain(Sq* self)
{
    if (self) ((Sq*)self)->_rc++;
    return self;
}

double Sq_get_Area(Sq* self)
{
    return self->f_Side * self->f_Side;
}

const char* Sq_get_Name(Sq* self)
{
    return "sq";
}

IShape_vtable Sq_IShape_vtable_instance =
{
    .Describe = (const char* (*)(void*))Sq_Describe,
    .Corners = (int (*)(void*))Sq_Corners,
    .get_Area = (double (*)(void*))Sq_get_Area,
    .get_Name = (const char* (*)(void*))Sq_get_Name,
};


void Circle_Ctor(Circle* self)
{
    CS2SX_TRACE("Circle..ctor", "Program.cs");
    self->f_Area = 3;
    self->f_Name = CS2SX_StrOwn("circle");
}

Circle* Circle_New(void)
{
    Circle* self = (Circle*)calloc(1, sizeof(Circle));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Circle__Destroy;
    self->_gc = Circle_Gc;
    self->_class = &Circle_ClassInfo;
    Circle_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Circle_Free);
    return self;
}

const char* Circle_Describe(Circle* self)
{
    CS2SX_TRACE("Circle.Describe", "Program.cs");
    CS2SX_LINE(18);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "round %s", self->f_Name);
    return _sb0;
}

int Circle_Corners(Circle* self)
{
    CS2SX_TRACE("Circle.Corners", "Program.cs");
    CS2SX_LINE(18);
    return 0;
}

const CS2SX_ClassInfo Circle_ClassInfo = { "Circle", NULL, NULL };

void Circle_Gc(void* _self, CS2SX_GcVisit visit)
{
    Circle* self = (Circle*)_self; (void)self;
}

static void Circle_GcDyn(void* self, CS2SX_GcVisit visit) { ((Circle*)self)->_gc(self, visit); }
const CS2SX_GcKind Circle_GcKind = { offsetof(Circle, _rc), Circle_GcDyn, (CS2SX_FreeFn)Circle_Free };

void Circle__Destroy(Circle* self)
{
    CS2SX_StrDrop(self->f_Name); self->f_Name = NULL;
}

void Circle_Free(Circle* self)
{
    if (!self) return;
    Circle* _o = (Circle*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Circle_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Circle* Circle_Retain(Circle* self)
{
    if (self) ((Circle*)self)->_rc++;
    return self;
}

static double _cs2sx_Circle_IShape_get_Area(void* self) { return ((Circle*)self)->f_Area; }

static const char* _cs2sx_Circle_IShape_get_Name(void* self) { return ((Circle*)self)->f_Name; }

IShape_vtable Circle_IShape_vtable_instance =
{
    .Describe = (const char* (*)(void*))Circle_Describe,
    .Corners = (int (*)(void*))Circle_Corners,
    .get_Area = (double (*)(void*))_cs2sx_Circle_IShape_get_Area,
    .get_Name = (const char* (*)(void*))_cs2sx_Circle_IShape_get_Name,
};


void Doc_Ctor(Doc* self)
{
    CS2SX_TRACE("Doc..ctor", "Program.cs");
    self->f_Title = CS2SX_StrOwn("a");
}

Doc* Doc_New(void)
{
    Doc* self = (Doc*)calloc(1, sizeof(Doc));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Doc__Destroy;
    self->_gc = Doc_Gc;
    self->_class = &Doc_ClassInfo;
    Doc_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Doc_Free);
    return self;
}

const CS2SX_ClassInfo Doc_ClassInfo = { "Doc", NULL, NULL };

void Doc_Gc(void* _self, CS2SX_GcVisit visit)
{
    Doc* self = (Doc*)_self; (void)self;
}

static void Doc_GcDyn(void* self, CS2SX_GcVisit visit) { ((Doc*)self)->_gc(self, visit); }
const CS2SX_GcKind Doc_GcKind = { offsetof(Doc, _rc), Doc_GcDyn, (CS2SX_FreeFn)Doc_Free };

void Doc__Destroy(Doc* self)
{
    CS2SX_StrDrop(self->f_Title); self->f_Title = NULL;
}

void Doc_Free(Doc* self)
{
    if (!self) return;
    Doc* _o = (Doc*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Doc_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Doc* Doc_Retain(Doc* self)
{
    if (self) ((Doc*)self)->_rc++;
    return self;
}

static const char* _cs2sx_Doc_INamed_get_Title(void* self) { return ((Doc*)self)->f_Title; }
static void _cs2sx_Doc_INamed_set_Title(void* self, const char* value) { CS2SX_STR_STORE(((Doc*)self)->f_Title, value); }

INamed_vtable Doc_INamed_vtable_instance =
{
    .get_Title = (const char* (*)(void*))_cs2sx_Doc_INamed_get_Title,
    .set_Title = (void (*)(void*, const char*))_cs2sx_Doc_INamed_set_Title,
};


void IntRepo_Ctor(IntRepo* self)
{
    CS2SX_TRACE("IntRepo..ctor", "Program.cs");
    self->f_l = List_int_Retain(List_int_New());
}

IntRepo* IntRepo_New(void)
{
    IntRepo* self = (IntRepo*)calloc(1, sizeof(IntRepo));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))IntRepo__Destroy;
    self->_gc = IntRepo_Gc;
    self->_class = &IntRepo_ClassInfo;
    IntRepo_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)IntRepo_Free);
    return self;
}

void IntRepo_Put(IntRepo* self, int x)
{
    CS2SX_TRACE("IntRepo.Put", "Program.cs");
    CS2SX_LINE(23);
    List_int_Add(self->f_l, x);
}

int IntRepo_Get(IntRepo* self, int i)
{
    CS2SX_TRACE("IntRepo.Get", "Program.cs");
    CS2SX_LINE(23);
    return List_int_Get(self->f_l, i);
}

const CS2SX_ClassInfo IntRepo_ClassInfo = { "IntRepo", NULL, NULL };

void IntRepo_Gc(void* _self, CS2SX_GcVisit visit)
{
    IntRepo* self = (IntRepo*)_self; (void)self;
}

static void IntRepo_GcDyn(void* self, CS2SX_GcVisit visit) { ((IntRepo*)self)->_gc(self, visit); }
const CS2SX_GcKind IntRepo_GcKind = { offsetof(IntRepo, _rc), IntRepo_GcDyn, (CS2SX_FreeFn)IntRepo_Free };

void IntRepo__Destroy(IntRepo* self)
{
    if (self->f_l) { List_int_Free(self->f_l); self->f_l = NULL; }
}

void IntRepo_Free(IntRepo* self)
{
    if (!self) return;
    IntRepo* _o = (IntRepo*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &IntRepo_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

IntRepo* IntRepo_Retain(IntRepo* self)
{
    if (self) ((IntRepo*)self)->_rc++;
    return self;
}

int IntRepo_get_Count(IntRepo* self)
{
    CS2SX_TRACE("IntRepo.get_Count", "Program.cs");
    CS2SX_LINE(23);
    return CS2SX_NN(self->f_l)->count;
}

IRepo_int_vtable IntRepo_IRepo_int_vtable_instance =
{
    .Put = (void (*)(void*, int))IntRepo_Put,
    .Get = (int (*)(void*, int))IntRepo_Get,
    .get_Count = (int (*)(void*))IntRepo_get_Count,
};


void StrRepo_Ctor(StrRepo* self)
{
    CS2SX_TRACE("StrRepo..ctor", "Program.cs");
    self->f_l = List_str_Retain(List_str_New());
}

StrRepo* StrRepo_New(void)
{
    StrRepo* self = (StrRepo*)calloc(1, sizeof(StrRepo));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))StrRepo__Destroy;
    self->_gc = StrRepo_Gc;
    self->_class = &StrRepo_ClassInfo;
    StrRepo_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)StrRepo_Free);
    return self;
}

void StrRepo_Put(StrRepo* self, const char* x)
{
    CS2SX_TRACE("StrRepo.Put", "Program.cs");
    CS2SX_LINE(24);
    List_str_Add(self->f_l, x);
}

const char* StrRepo_Get(StrRepo* self, int i)
{
    CS2SX_TRACE("StrRepo.Get", "Program.cs");
    CS2SX_LINE(24);
    return List_str_Get(self->f_l, i);
}

const CS2SX_ClassInfo StrRepo_ClassInfo = { "StrRepo", NULL, NULL };

void StrRepo_Gc(void* _self, CS2SX_GcVisit visit)
{
    StrRepo* self = (StrRepo*)_self; (void)self;
}

static void StrRepo_GcDyn(void* self, CS2SX_GcVisit visit) { ((StrRepo*)self)->_gc(self, visit); }
const CS2SX_GcKind StrRepo_GcKind = { offsetof(StrRepo, _rc), StrRepo_GcDyn, (CS2SX_FreeFn)StrRepo_Free };

void StrRepo__Destroy(StrRepo* self)
{
    if (self->f_l) { List_str_Free(self->f_l); self->f_l = NULL; }
}

void StrRepo_Free(StrRepo* self)
{
    if (!self) return;
    StrRepo* _o = (StrRepo*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &StrRepo_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

StrRepo* StrRepo_Retain(StrRepo* self)
{
    if (self) ((StrRepo*)self)->_rc++;
    return self;
}

int StrRepo_get_Count(StrRepo* self)
{
    CS2SX_TRACE("StrRepo.get_Count", "Program.cs");
    CS2SX_LINE(24);
    return CS2SX_NN(self->f_l)->count;
}

IRepo_str_vtable StrRepo_IRepo_str_vtable_instance =
{
    .Put = (void (*)(void*, const char*))StrRepo_Put,
    .Get = (const char* (*)(void*, int))StrRepo_Get,
    .get_Count = (int (*)(void*))StrRepo_get_Count,
};


static Box_int_vtable Box_int_vtable_instance =
{
};

void Box_int_Ctor(Box_int* self, int v)
{
    CS2SX_TRACE("Box_int..ctor", "Program.cs");
    CS2SX_LINE(25);
    self->f_Value = v;
}

Box_int* Box_int_New(int v)
{
    Box_int* self = (Box_int*)calloc(1, sizeof(Box_int));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Box_int__Destroy;
    self->_gc = Box_int_Gc;
    self->_class = &Box_int_ClassInfo;
    self->vtable = &Box_int_vtable_instance;
    Box_int_Ctor(self, v);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Box_int_Free);
    return self;
}

int Box_int_Get(Box_int* self)
{
    CS2SX_TRACE("Box_int.Get", "Program.cs");
    CS2SX_LINE(25);
    return self->f_Value;
}

const CS2SX_ClassInfo Box_int_ClassInfo = { "Box_int", NULL, NULL };

void Box_int_Gc(void* _self, CS2SX_GcVisit visit)
{
    Box_int* self = (Box_int*)_self; (void)self;
}

static void Box_int_GcDyn(void* self, CS2SX_GcVisit visit) { ((Box_int*)self)->_gc(self, visit); }
const CS2SX_GcKind Box_int_GcKind = { offsetof(Box_int, _rc), Box_int_GcDyn, (CS2SX_FreeFn)Box_int_Free };

void Box_int__Destroy(Box_int* self)
{
}

void Box_int_Free(Box_int* self)
{
    if (!self) return;
    Box_int* _o = (Box_int*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Box_int_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Box_int* Box_int_Retain(Box_int* self)
{
    if (self) ((Box_int*)self)->_rc++;
    return self;
}

static Box_str_vtable Box_str_vtable_instance =
{
};

void Box_str_Ctor(Box_str* self, const char* v)
{
    CS2SX_TRACE("Box_str..ctor", "Program.cs");
    CS2SX_LINE(25);
    CS2SX_STR_STORE(self->f_Value, v);
}

Box_str* Box_str_New(const char* v)
{
    Box_str* self = (Box_str*)calloc(1, sizeof(Box_str));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Box_str__Destroy;
    self->_gc = Box_str_Gc;
    self->_class = &Box_str_ClassInfo;
    self->vtable = &Box_str_vtable_instance;
    Box_str_Ctor(self, v);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Box_str_Free);
    return self;
}

const char* Box_str_Get(Box_str* self)
{
    CS2SX_TRACE("Box_str.Get", "Program.cs");
    CS2SX_LINE(25);
    return self->f_Value;
}

const CS2SX_ClassInfo Box_str_ClassInfo = { "Box_str", NULL, NULL };

void Box_str_Gc(void* _self, CS2SX_GcVisit visit)
{
    Box_str* self = (Box_str*)_self; (void)self;
}

static void Box_str_GcDyn(void* self, CS2SX_GcVisit visit) { ((Box_str*)self)->_gc(self, visit); }
const CS2SX_GcKind Box_str_GcKind = { offsetof(Box_str, _rc), Box_str_GcDyn, (CS2SX_FreeFn)Box_str_Free };

void Box_str__Destroy(Box_str* self)
{
    CS2SX_StrDrop(self->f_Value); self->f_Value = NULL;
}

void Box_str_Free(Box_str* self)
{
    if (!self) return;
    Box_str* _o = (Box_str*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Box_str_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Box_str* Box_str_Retain(Box_str* self)
{
    if (self) ((Box_str*)self)->_rc++;
    return self;
}

static Box_int_vtable IntBox_vtable_instance;

void IntBox_Ctor(IntBox* self, int v)
{
    CS2SX_TRACE("IntBox..ctor", "Program.cs");
    Box_int_Ctor((Box_int*)self, v);
}

IntBox* IntBox_New(int v)
{
    IntBox* self = (IntBox*)calloc(1, sizeof(IntBox));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))IntBox__Destroy;
    self->base._gc = IntBox_Gc;
    self->base._class = &IntBox_ClassInfo;
    self->base.vtable = &IntBox_vtable_instance;
    IntBox_Ctor(self, v);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)IntBox_Free);
    return self;
}

const char* IntBox_Show(IntBox* self)
{
    CS2SX_TRACE("IntBox.Show", "Program.cs");
    CS2SX_LINE(26);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "box%d", self->base.f_Value);
    return _sb0;
}

const CS2SX_ClassInfo IntBox_ClassInfo = { "IntBox", &Box_int_ClassInfo, NULL };

void IntBox_Gc(void* _self, CS2SX_GcVisit visit)
{
    IntBox* self = (IntBox*)_self; (void)self;
    Box_int_Gc(_self, visit);
}

void IntBox__Destroy(IntBox* self)
{
    Box_int__Destroy((Box_int*)self);
}

void IntBox_Free(IntBox* self)
{
    if (!self) return;
    Box_int* _o = (Box_int*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Box_int_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

IntBox* IntBox_Retain(IntBox* self)
{
    if (self) ((Box_int*)self)->_rc++;
    return self;
}

static Box_int_vtable IntBox_vtable_instance =
{
    .Show = (const char*(*)(void*))IntBox_Show,
};

static Box_str_vtable StrBox_vtable_instance;

void StrBox_Ctor(StrBox* self, const char* v)
{
    CS2SX_TRACE("StrBox..ctor", "Program.cs");
    Box_str_Ctor((Box_str*)self, v);
}

StrBox* StrBox_New(const char* v)
{
    StrBox* self = (StrBox*)calloc(1, sizeof(StrBox));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))StrBox__Destroy;
    self->base._gc = StrBox_Gc;
    self->base._class = &StrBox_ClassInfo;
    self->base.vtable = &StrBox_vtable_instance;
    StrBox_Ctor(self, v);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)StrBox_Free);
    return self;
}

const char* StrBox_Show(StrBox* self)
{
    CS2SX_TRACE("StrBox.Show", "Program.cs");
    CS2SX_LINE(27);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "s:%s", self->base.f_Value);
    return _sb0;
}

const CS2SX_ClassInfo StrBox_ClassInfo = { "StrBox", &Box_str_ClassInfo, NULL };

void StrBox_Gc(void* _self, CS2SX_GcVisit visit)
{
    StrBox* self = (StrBox*)_self; (void)self;
    Box_str_Gc(_self, visit);
}

void StrBox__Destroy(StrBox* self)
{
    Box_str__Destroy((Box_str*)self);
}

void StrBox_Free(StrBox* self)
{
    if (!self) return;
    Box_str* _o = (Box_str*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Box_str_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

StrBox* StrBox_Retain(StrBox* self)
{
    if (self) ((Box_str*)self)->_rc++;
    return self;
}

static Box_str_vtable StrBox_vtable_instance =
{
    .Show = (const char*(*)(void*))StrBox_Show,
};

// ── Struct: Vec ────────────────────────────────────────
int Vec_Longer(Vec* self, Vec o)
{
    return Vec_get_Len2(self) > Vec_get_Len2(&o);
}

int Vec_get_Len2(Vec* self)
{
    return self->X * self->X + self->Y * self->Y;
}

int Vec_Equals(Vec a, Vec b)
{
    return memcmp(&a, &b, sizeof(Vec)) == 0;
}

__attribute__((constructor))
static void Cfg_StaticInit(void)
{
    CS2SX_LINE(30);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "T%d", 1);
    CS2SX_STR_STORE(Cfg_Title, _sb0);
    CS2SX_LINE(30);
    Cfg_Max = 3 * 4;
}

const char* Cfg_Title;
int Cfg_Max;
void Timer2_Ctor(Timer2* self)
{
    CS2SX_TRACE("Timer2..ctor", "Program.cs");
    self->f_Start = CS2SX_DT_New(2024, 5, 1, 0, 0, 0, 0);
    self->f_Limit = CS2SX_TimeSpan_FromSec((2) * 60.0);
}

Timer2* Timer2_New(void)
{
    Timer2* self = (Timer2*)calloc(1, sizeof(Timer2));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Timer2__Destroy;
    self->_gc = Timer2_Gc;
    self->_class = &Timer2_ClassInfo;
    Timer2_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Timer2_Free);
    return self;
}

const CS2SX_ClassInfo Timer2_ClassInfo = { "Timer2", NULL, NULL };

void Timer2_Gc(void* _self, CS2SX_GcVisit visit)
{
    Timer2* self = (Timer2*)_self; (void)self;
}

static void Timer2_GcDyn(void* self, CS2SX_GcVisit visit) { ((Timer2*)self)->_gc(self, visit); }
const CS2SX_GcKind Timer2_GcKind = { offsetof(Timer2, _rc), Timer2_GcDyn, (CS2SX_FreeFn)Timer2_Free };

void Timer2__Destroy(Timer2* self)
{
}

void Timer2_Free(Timer2* self)
{
    if (!self) return;
    Timer2* _o = (Timer2*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Timer2_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Timer2* Timer2_Retain(Timer2* self)
{
    if (self) ((Timer2*)self)->_rc++;
    return self;
}

CS2SX_DateTime Timer2_get_End(Timer2* self)
{
    return CS2SX_DT_AddTicks(self->f_Start, (self->f_Limit).ticks);
}

void TypesAndDatesApp_Init(TypesAndDatesApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))TypesAndDatesApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))TypesAndDatesApp_OnFrame;
}

int TypesAndDatesApp_Total(IRepo_int* r)
{
    CS2SX_TRACE("TypesAndDatesApp.Total", "Program.cs");
    CS2SX_LINE(35);
    int s = 0;
    CS2SX_LINE(35);
    for (int i = 0; i < IRepo_int_get_Count(r); i++)
    {
        CS2SX_LINE(35);
        s += ({ __auto_type _cs2sx_r = CS2SX_NN(r); _cs2sx_r->vtable->Get(_cs2sx_r->obj, i); });
    }
    CS2SX_LINE(35);
    return s;
}

static const char* _lambda_0(CS2SX_Entry_str_int* kv)
{
    char* _sb6 = _cs2sx_next_buf();
    _sb6 = CS2SX_Format(_sb6, "%s=%d", CS2SX_NN(kv)->f_Key, CS2SX_NN(kv)->f_Value);
    return _sb6;
}

static int _lambda_1(CS2SX_Entry_str_int* kv)
{
    return CS2SX_NN(kv)->f_Value > 1;
}

static int _lambda_2(int x, int y)
{
    return x * y;
}

static List_int* _lambda_3(int x)
{
    List_int* _list_23 = List_int_New();
    List_int_Add(_list_23, x);
    List_int_Add(_list_23, x);
    return _list_23;
}

static int _lambda_4(int acc, int x)
{
    return acc + x;
}

static int _lambda_5(void* _env)
{
    CS2SX_TRACE("TypesAndDatesApp.<lambda>", "Program.cs");
    (void)_env;
    return 1;
}

static CS2SX_Delegate _dg_5 = CS2SX_DELEGATE_STATIC(_lambda_5);

static int _lambda_6(void* _env)
{
    CS2SX_TRACE("TypesAndDatesApp.<lambda>", "Program.cs");
    (void)_env;
    return 2;
}

static CS2SX_Delegate _dg_6 = CS2SX_DELEGATE_STATIC(_lambda_6);

static double _lambda_7(Func_int_t f)
{
    return Func_int_Invoke(f);
}

void TypesAndDatesApp_OnInit(TypesAndDatesApp* self)
{
    CS2SX_TRACE("TypesAndDatesApp.OnInit", "Program.cs");
    CS2SX_LINE(39);
    List_IShape* _list_0 = List_IShape_New();
    List_IShape_Add(_list_0, Sq_as_IShape(Sq_New()));
    List_IShape_Add(_list_0, Circle_as_IShape(Circle_New()));
    __auto_type _fe_1 = _list_0;
    for (int _i_s = 0; _i_s < (int)(_fe_1->count); _i_s++)
    {
        IShape* s = List_IShape_Get(_fe_1, _i_s);
        char* _sb0 = _cs2sx_next_buf();
        _sb0 = CS2SX_Format(_sb0, "%s %s [%s] %d", IShape_get_Name(s), Double_ToString(IShape_get_Area(s)), ({ __auto_type _cs2sx_r = CS2SX_NN(s); _cs2sx_r->vtable->Describe(_cs2sx_r->obj); }), ({ __auto_type _cs2sx_r = CS2SX_NN(s); _cs2sx_r->vtable->Corners(_cs2sx_r->obj); }));
        const char* _ps_2 = _sb0;
        printf("%s\n", _ps_2 ? _ps_2 : "");
    }
    CS2SX_LINE(41);
    INamed* n = Doc_as_INamed(Doc_New());
    CS2SX_LINE(42);
    INamed_set_Title(n, "b");
    CS2SX_LINE(43);
    char* _sb1 = _cs2sx_next_buf();
    _sb1 = CS2SX_Format(_sb1, "title %s", INamed_get_Title(n));
    const char* _ps_3 = _sb1;
    printf("%s\n", _ps_3 ? _ps_3 : "");
    CS2SX_LINE(45);
    IRepo_int* r = IntRepo_as_IRepo_int(IntRepo_New());
    CS2SX_LINE(45);
    ({ __auto_type _cs2sx_r = r; _cs2sx_r->vtable->Put(_cs2sx_r->obj, 4); });
    CS2SX_LINE(45);
    ({ __auto_type _cs2sx_r = r; _cs2sx_r->vtable->Put(_cs2sx_r->obj, 5); });
    CS2SX_LINE(46);
    IRepo_str* sr = StrRepo_as_IRepo_str(StrRepo_New());
    CS2SX_LINE(46);
    ({ __auto_type _cs2sx_r = sr; _cs2sx_r->vtable->Put(_cs2sx_r->obj, "x"); });
    CS2SX_LINE(47);
    char* _sb2 = _cs2sx_next_buf();
    _sb2 = CS2SX_Format(_sb2, "repo %d %d %d %s", IRepo_int_get_Count(r), ({ __auto_type _cs2sx_r = r; _cs2sx_r->vtable->Get(_cs2sx_r->obj, 1); }), TypesAndDatesApp_Total(r), ({ __auto_type _cs2sx_r = sr; _cs2sx_r->vtable->Get(_cs2sx_r->obj, 0); }));
    const char* _ps_4 = _sb2;
    printf("%s\n", _ps_4 ? _ps_4 : "");
    CS2SX_LINE(48);
    Box_int* b = (Box_int*)(IntBox_New(7));
    CS2SX_LINE(49);
    List_Box_int* _list_5 = List_Box_int_New();
    List_Box_int_Add(_list_5, b);
    List_Box_int_Add(_list_5, (Box_int*)IntBox_New(2));
    List_Box_int* boxes = _list_5;
    CS2SX_LINE(50);
    char* _sb3 = _cs2sx_next_buf();
    _sb3 = CS2SX_Format(_sb3, "box %s %d %d %s %s", b->vtable->Show(b), b->f_Value, Box_int_Get(b), ({ __auto_type _cs2sx_r = StrBox_New("q"); _cs2sx_r->base.vtable->Show(_cs2sx_r); }), ({ __auto_type _cs2sx_r = CS2SX_NN(List_Box_int_Get(boxes, 1)); _cs2sx_r->vtable->Show(_cs2sx_r); }));
    const char* _ps_6 = _sb3;
    printf("%s\n", _ps_6 ? _ps_6 : "");
    CS2SX_LINE(52);
    Vec v = (Vec){ .X = 3, .Y = 4 };
    CS2SX_LINE(53);
    char* _sb4 = _cs2sx_next_buf();
    _sb4 = CS2SX_Format(_sb4, "vec %d %s %s %d", Vec_get_Len2(&v), CS2SX_BoolStr(Vec_Longer(&v, (Vec){ .X = 1, .Y = 1 })), Cfg_Title, Cfg_Max);
    const char* _ps_7 = _sb4;
    printf("%s\n", _ps_7 ? _ps_7 : "");
    CS2SX_LINE(54);
    char* _sb5 = _cs2sx_next_buf();
    _sb5 = CS2SX_Format(_sb5, "num %s %s %s %s %d", Float_ToString((-FLT_MAX)), Double_ToString(__DBL_DENORM_MIN__), Double_ToString(M_PI), CS2SX_BoolStr(((-DBL_MAX) < 0)), INT_MAX);
    const char* _ps_8 = _sb5;
    printf("%s\n", _ps_8 ? _ps_8 : "");
    CS2SX_LINE(56);
    Dict_str_int* _dict_9 = Dict_str_int_New();
    Dict_str_int_Set(_dict_9, "a", 1);
    Dict_str_int_Set(_dict_9, "b", 2);
    Dict_str_int* d = _dict_9;
    CS2SX_LINE(57);
    __auto_type _src_10 = CS2SX_DictLinq_str_int_Entries(d);
    List_str* _sel_11 = List_str_New();
    for (int _i_12 = 0; _i_12 < _src_10->count; _i_12++)
    {
        CS2SX_Entry_str_int* _e__sel_11 = List_CS2SX_Entry_str_int_Get(_src_10, _i_12);
        List_str_Add(_sel_11, (const char*)_lambda_0(_e__sel_11));
    }
    __auto_type _src_13 = CS2SX_DictLinq_str_int_Values(d);
    int _sum_15 = 0;
    for (int _i_14 = 0; _i_14 < _src_13->count; _i_14++)
      _sum_15 += List_int_Get(_src_13, _i_14);
    __auto_type _src_16 = CS2SX_DictLinq_str_int_Entries(d);
    List_CS2SX_Entry_str_int* _where_17 = List_CS2SX_Entry_str_int_New();
    for (int _i_18 = 0; _i_18 < _src_16->count; _i_18++)
    {
        CS2SX_Entry_str_int* _e__where_17 = List_CS2SX_Entry_str_int_Get(_src_16, _i_18);
        if (_lambda_1(_e__where_17)) List_CS2SX_Entry_str_int_Add(_where_17, _e__where_17);
    }
    char* _sb7 = _cs2sx_next_buf();
    _sb7 = CS2SX_Format(_sb7, "dict %s %d %s %d", String_Join(",", _sel_11), _sum_15, String_Join("", CS2SX_DictLinq_str_int_Keys(d)), _where_17->count);
    const char* _ps_19 = _sb7;
    printf("%s\n", _ps_19 ? _ps_19 : "");
    CS2SX_LINE(59);
    List_int* _list_20 = List_int_New();
    List_int_Add(_list_20, 1);
    List_int_Add(_list_20, 2);
    List_int_Add(_list_20, 3);
    List_int* a = _list_20;
    CS2SX_LINE(60);
    List_int* _zip_21 = List_int_New();
    for (int _i_22 = 0; _i_22 < a->count && _i_22 < a->count; _i_22++)
      List_int_Add(_zip_21, _lambda_2(List_int_Get(a, _i_22), List_int_Get(a, _i_22)));
    List_int* _sm_24 = List_int_New();
    for (int _i_25 = 0; _i_25 < a->count; _i_25++)
    {
        int _e__sm_24 = List_int_Get(a, _i_25);
        List_int* _sub_26 = _lambda_3(_e__sm_24);
        if (_sub_26)
        {
            for (int _j_27 = 0; _j_27 < _sub_26->count; _j_27++)
                List_int_Add(_sm_24, List_int_Get(_sub_26, _j_27));
        }
    }
    __auto_type _acc_28 = 0;
    for (int _i_29 = 0; _i_29 < a->count; _i_29++)
      _acc_28 = _lambda_4(_acc_28, List_int_Get(a, _i_29));
    List_Func_int_t* _list_30 = List_Func_int_t_New();
    List_Func_int_t_Add(_list_30, (&_dg_5));
    List_Func_int_t_Add(_list_30, (&_dg_6));
    int _sum_32 = 0;
    for (int _i_31 = 0; _i_31 < _list_30->count; _i_31++)
      _sum_32 += (int)_lambda_7(List_Func_int_t_Get(_list_30, _i_31));
    char* _sb8 = _cs2sx_next_buf();
    _sb8 = CS2SX_Format(_sb8, "linq %s %s %d %d", ({ __auto_type _jl = (_zip_21); int _jn = (_jl ? _jl->count : 0); CS2SX_JOIN_EACH(",", _jn, "%d", (int)(_jl->data[_ji])); }), ({ __auto_type _jl = (_sm_24); int _jn = (_jl ? _jl->count : 0); CS2SX_JOIN_EACH(",", _jn, "%d", (int)(_jl->data[_ji])); }), _acc_28, _sum_32);
    const char* _ps_33 = _sb8;
    printf("%s\n", _ps_33 ? _ps_33 : "");
    CS2SX_LINE(63);
    CS2SX_DateTime dt = CS2SX_DT_Add(CS2SX_DT_New(2024, 2, 28, 0, 0, 0, 0), 2, CS2SX_TICKS_PER_DAY);
    CS2SX_LINE(64);
    char* _sb9 = _cs2sx_next_buf();
    _sb9 = CS2SX_Format(_sb9, "date %s %s %d", CS2SX_DT_ToString(dt, "yyyy-MM-dd"), CS2SX_DayOfWeekName(CS2SX_DT_Part(dt, CS2SX_DT_DOW)), CS2SX_DT_Part(dt, CS2SX_DT_DOY));
    const char* _ps_34 = _sb9;
    printf("%s\n", _ps_34 ? _ps_34 : "");
    CS2SX_LINE(65);
    CS2SX_DateTime t = CS2SX_DT_New(2023, 12, 31, 23, 59, 30, 0);
    CS2SX_LINE(66);
    char* _sb10 = _cs2sx_next_buf();
    _sb10 = CS2SX_Format(_sb10, "add %s %s %d %s %d", CS2SX_DT_ToString(CS2SX_DT_Add(t, 45, CS2SX_TICKS_PER_SEC), "yyyy-MM-dd HH:mm:ss"), CS2SX_DT_ToString(CS2SX_DT_AddMonths(t, 2), "d"), CS2SX_DT_Part(CS2SX_DT_AddMonths(CS2SX_DT_New(2024, 1, 31, 0, 0, 0, 0), 1), CS2SX_DT_DAY), CS2SX_BoolStr(CS2SX_DT_IsLeapYear(2100)), CS2SX_DT_DaysInMonth(2024, 2));
    const char* _ps_35 = _sb10;
    printf("%s\n", _ps_35 ? _ps_35 : "");
    CS2SX_LINE(68);
    CS2SX_TimeSpan span = CS2SX_TimeSpan_FromTicks((CS2SX_DT_New(2024, 3, 1, 0, 0, 0, 0)) - (CS2SX_DT_New(2024, 2, 1, 0, 0, 0, 0)));
    CS2SX_LINE(69);
    char* _sb11 = _cs2sx_next_buf();
    _sb11 = CS2SX_Format(_sb11, "span %s %s %s %s %s", Double_ToString(CS2SX_TimeSpan_TotalDays(span)), CS2SX_TS_ToString(span), CS2SX_BoolStr((((span).ticks > (CS2SX_TimeSpan_FromSec((28) * 86400.0)).ticks))), CS2SX_TS_ToString(CS2SX_TimeSpan_FromSec(90.5)), CS2SX_TS_ToString(CS2SX_TimeSpan_New(1, 2, 3, 4, 0)));
    const char* _ps_36 = _sb11;
    printf("%s\n", _ps_36 ? _ps_36 : "");
    CS2SX_LINE(70);
    CS2SX_DateTime later = CS2SX_DT_AddTicks(t, (CS2SX_TimeSpan_FromSec((1) * 3600.0)).ticks);
    CS2SX_LINE(71);
    char* _sb12 = _cs2sx_next_buf();
    _sb12 = CS2SX_Format(_sb12, "fmt %s %s %s %s %s", CS2SX_DT_ToString(later, "dddd, dd MMM yyyy h:mm tt"), CS2SX_DT_ToString(t, "s"), CS2SX_DT_ToString(t, NULL), CS2SX_TS_ToString(CS2SX_TimeSpan_FromTicks((t) % CS2SX_TICKS_PER_DAY)), CS2SX_BoolStr((CS2SX_DT_Part(t, CS2SX_DT_DOW) == 0)));
    const char* _ps_37 = _sb12;
    printf("%s\n", _ps_37 ? _ps_37 : "");
    CS2SX_LINE(72);
    Timer2* tm = Timer2_New();
    CS2SX_LINE(73);
    char* _sb13 = _cs2sx_next_buf();
    _sb13 = CS2SX_Format(_sb13, "timer %s %s %s", CS2SX_DT_ToString(Timer2_get_End(tm), "HH:mm"), CS2SX_DT_ToString(tm->f_Start, "yyyy"), CS2SX_BoolStr((CS2SX_DT_Today() <= CS2SX_DT_Now())));
    const char* _ps_38 = _sb13;
    printf("%s\n", _ps_38 ? _ps_38 : "");
    CS2SX_LINE(74);
    {
        CS2SX_ExFrame _exf_39;
        CS2SX_ExPush(&_exf_39);
        if (setjmp(_exf_39.buf) == 0)
        {
            CS2SX_LINE(74);
            CS2SX_DateTime bad = CS2SX_DT_New(2023, 2, 29, 0, 0, 0, 0);
            CS2SX_LINE(74);
            char* _sb14 = _cs2sx_next_buf();
            _sb14 = CS2SX_Format(_sb14, "no %d", CS2SX_DT_Part(bad, CS2SX_DT_DAY));
            const char* _ps_40 = _sb14;
            printf("%s\n", _ps_40 ? _ps_40 : "");
            CS2SX_ExPop(&_exf_39);
        }
        else
        {
            Exception* _exf_39_ex = _exf_39.ex;
            if (CS2SX_Exc_Is(_exf_39_ex, &CS2SX_TI_ArgumentOutOfRangeException))
            {
                CS2SX_LINE(75);
                printf("range\n");
                Exception_Free(_exf_39_ex);
            }
            else CS2SX_Throw(_exf_39_ex);
        }
    }
    CS2SX_LINE(77);
    CS2SX_File_WriteAllText("sdmc:/cs2sx_types.txt", "a\nb\nc");
    CS2SX_LINE(78);
    char* _sb15 = _cs2sx_next_buf();
    _sb15 = CS2SX_Format(_sb15, "file %d %s", CS2SX_ArrLen(CS2SX_StrArr_FromList(CS2SX_File_ReadAllLines("sdmc:/cs2sx_types.txt"))), CS2SX_BoolStr((CS2SX_ArrLen(CS2SX_StrArr_FromList(CS2SX_Dir_GetFiles("sdmc:/", "*"))) > 0)));
    const char* _ps_41 = _sb15;
    printf("%s\n", _ps_41 ? _ps_41 : "");
    CS2SX_LINE(79);
    CS2SX_File_Delete("sdmc:/cs2sx_types.txt");
    CS2SX_LINE(80);
    printf("done\n");
}

void TypesAndDatesApp_OnFrame(TypesAndDatesApp* self)
{
    CS2SX_TRACE("TypesAndDatesApp.OnFrame", "Program.cs");
}

void TypesAndDatesApp_Free(TypesAndDatesApp* self)
{
    if (!self) return;
    free(self);
}
