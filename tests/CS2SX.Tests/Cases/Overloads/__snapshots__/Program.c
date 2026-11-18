#include <stdlib.h>
#include "Program.h"

void Shape_Ctor(Shape* self)
{
    CS2SX_TRACE("Shape..ctor", "Program.cs");
    self->f_Name = CS2SX_StrOwn("shape");
}

Shape* Shape_New(void)
{
    Shape* self = (Shape*)calloc(1, sizeof(Shape));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Shape__Destroy;
    self->_gc = Shape_Gc;
    self->_class = &Shape_ClassInfo;
    Shape_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Shape_Free);
    return self;
}

const CS2SX_ClassInfo Shape_ClassInfo = { "Shape", NULL };

void Shape_Gc(void* _self, CS2SX_GcVisit visit)
{
    Shape* self = (Shape*)_self; (void)self;
}

static void Shape_GcDyn(void* self, CS2SX_GcVisit visit) { ((Shape*)self)->_gc(self, visit); }
const CS2SX_GcKind Shape_GcKind = { offsetof(Shape, _rc), Shape_GcDyn, (CS2SX_FreeFn)Shape_Free };

void Shape__Destroy(Shape* self)
{
    CS2SX_StrDrop(self->f_Name); self->f_Name = NULL;
}

void Shape_Free(Shape* self)
{
    if (!self) return;
    Shape* _o = (Shape*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Shape_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Shape* Shape_Retain(Shape* self)
{
    if (self) ((Shape*)self)->_rc++;
    return self;
}

void Printer_Ctor(Printer* self)
{
    CS2SX_TRACE("Printer..ctor", "Program.cs");
}

Printer* Printer_New(void)
{
    Printer* self = (Printer*)calloc(1, sizeof(Printer));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Printer__Destroy;
    self->_gc = Printer_Gc;
    self->_class = &Printer_ClassInfo;
    Printer_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Printer_Free);
    return self;
}

const char* Printer_Show_1_int(Printer* self, int value)
{
    CS2SX_TRACE("Printer.Show", "Program.cs");
    CS2SX_LINE(17);
    self->f_calls++;
    CS2SX_LINE(17);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "int %d", value);
    return _sb0;
}

const char* Printer_Show_1_float(Printer* self, float value)
{
    CS2SX_TRACE("Printer.Show", "Program.cs");
    CS2SX_LINE(18);
    self->f_calls++;
    CS2SX_LINE(18);
    char* _sb1 = _cs2sx_next_buf();
    _sb1 = CS2SX_Format(_sb1, "float %s", Float_ToString(value));
    return _sb1;
}

const char* Printer_Show_1_str(Printer* self, const char* value)
{
    CS2SX_TRACE("Printer.Show", "Program.cs");
    CS2SX_LINE(19);
    self->f_calls++;
    CS2SX_LINE(19);
    char* _sb2 = _cs2sx_next_buf();
    _sb2 = CS2SX_Format(_sb2, "string %s", value);
    return _sb2;
}

const char* Printer_Show_1_Shape(Printer* self, Shape* value)
{
    CS2SX_TRACE("Printer.Show", "Program.cs");
    CS2SX_LINE(20);
    self->f_calls++;
    CS2SX_LINE(20);
    char* _sb3 = _cs2sx_next_buf();
    _sb3 = CS2SX_Format(_sb3, "shape %s", CS2SX_NN(value)->f_Name);
    return _sb3;
}

const char* Printer_Show_1_intArr(Printer* self, int* values)
{
    CS2SX_TRACE("Printer.Show", "Program.cs");
    CS2SX_LINE(21);
    self->f_calls++;
    CS2SX_LINE(21);
    char* _sb4 = _cs2sx_next_buf();
    _sb4 = CS2SX_Format(_sb4, "array %d", CS2SX_ArrLen(CS2SX_NN(values)));
    return _sb4;
}

const char* Printer_Show_1_List_int(Printer* self, List_int* values)
{
    CS2SX_TRACE("Printer.Show", "Program.cs");
    CS2SX_LINE(22);
    self->f_calls++;
    CS2SX_LINE(22);
    char* _sb5 = _cs2sx_next_buf();
    _sb5 = CS2SX_Format(_sb5, "list %d", CS2SX_NN(values)->count);
    return _sb5;
}

const char* Printer_Show_2_int_int(Printer* self, int a, int b)
{
    CS2SX_TRACE("Printer.Show", "Program.cs");
    CS2SX_LINE(23);
    self->f_calls++;
    CS2SX_LINE(23);
    char* _sb6 = _cs2sx_next_buf();
    _sb6 = CS2SX_Format(_sb6, "two %d", (a + b));
    return _sb6;
}

const char* Printer_Show_2_int_str(Printer* self, int a, const char* b)
{
    CS2SX_TRACE("Printer.Show", "Program.cs");
    CS2SX_LINE(24);
    self->f_calls++;
    CS2SX_LINE(24);
    char* _sb7 = _cs2sx_next_buf();
    _sb7 = CS2SX_Format(_sb7, "mixed %d%s", a, b);
    return _sb7;
}

const char* Printer_Show_0(Printer* self)
{
    CS2SX_TRACE("Printer.Show", "Program.cs");
    CS2SX_LINE(25);
    self->f_calls++;
    CS2SX_LINE(25);
    return "none";
}

int Printer_Twice_1_int(int x)
{
    CS2SX_TRACE("Printer.Twice", "Program.cs");
    CS2SX_LINE(27);
    return x * 2;
}

const char* Printer_Twice_1_str(const char* x)
{
    CS2SX_TRACE("Printer.Twice", "Program.cs");
    CS2SX_LINE(28);
    char* _sb8 = _cs2sx_next_buf();
    _sb8 = CS2SX_Format(_sb8, "%s%s", x, x);
    return _sb8;
}

const char* Printer_All(Printer* self)
{
    CS2SX_TRACE("Printer.All", "Program.cs");
    CS2SX_LINE(30);
    char* _sb9 = _cs2sx_next_buf();
    _sb9 = CS2SX_Format(_sb9, "%s, %s, %s", Printer_Show_1_int(self, 1), Printer_Show_1_str(self, "s"), Printer_Show_0(self));
    return _sb9;
}

const CS2SX_ClassInfo Printer_ClassInfo = { "Printer", NULL };

void Printer_Gc(void* _self, CS2SX_GcVisit visit)
{
    Printer* self = (Printer*)_self; (void)self;
}

static void Printer_GcDyn(void* self, CS2SX_GcVisit visit) { ((Printer*)self)->_gc(self, visit); }
const CS2SX_GcKind Printer_GcKind = { offsetof(Printer, _rc), Printer_GcDyn, (CS2SX_FreeFn)Printer_Free };

void Printer__Destroy(Printer* self)
{
}

void Printer_Free(Printer* self)
{
    if (!self) return;
    Printer* _o = (Printer*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Printer_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Printer* Printer_Retain(Printer* self)
{
    if (self) ((Printer*)self)->_rc++;
    return self;
}

int Printer_get_Calls(Printer* self)
{
    return self->f_calls;
}

void OverloadsApp_Init(OverloadsApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))OverloadsApp_OnInit;
}

double OverloadsApp_Area_1_double(double side)
{
    CS2SX_TRACE("OverloadsApp.Area", "Program.cs");
    CS2SX_LINE(35);
    return side * side;
}

int OverloadsApp_Area_1_int(int side)
{
    CS2SX_TRACE("OverloadsApp.Area", "Program.cs");
    CS2SX_LINE(36);
    return side * side;
}

static const char* _mg_Printer_Twice_1_str(void* _t, const char* a0)
{
    (void)_t;
    return Printer_Twice_1_str(a0);
}
static CS2SX_Delegate _mg_Printer_Twice_1_str_dg = CS2SX_DELEGATE_STATIC(_mg_Printer_Twice_1_str);
void OverloadsApp_OnInit(OverloadsApp* self)
{
    CS2SX_TRACE("OverloadsApp.OnInit", "Program.cs");
    CS2SX_LINE(40);
    Printer* p = Printer_New();
    CS2SX_LINE(41);
    const char* _ps_0 = Printer_Show_1_int(p, 7);
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(42);
    const char* _ps_1 = Printer_Show_1_float(p, 1.5f);
    printf("%s\n", _ps_1 ? _ps_1 : "");
    CS2SX_LINE(43);
    const char* _ps_2 = Printer_Show_1_str(p, "x");
    printf("%s\n", _ps_2 ? _ps_2 : "");
    CS2SX_LINE(44);
    const char* _ps_3 = Printer_Show_1_Shape(p, Shape_New());
    printf("%s\n", _ps_3 ? _ps_3 : "");
    CS2SX_LINE(45);
    const char* _ps_4 = Printer_Show_1_intArr(p, (int*)CS2SX_ArrFromInit((int[]){ 1, 2, 3 }, 3, sizeof(int)));
    printf("%s\n", _ps_4 ? _ps_4 : "");
    CS2SX_LINE(46);
    List_int* _list_5 = List_int_New();
    List_int_Add(_list_5, 4);
    List_int_Add(_list_5, 5);
    List_int* _list_6 = List_int_New();
    List_int_Add(_list_6, 4);
    List_int_Add(_list_6, 5);
    List_int* _list_7 = List_int_New();
    List_int_Add(_list_7, 4);
    List_int_Add(_list_7, 5);
    const char* _ps_8 = Printer_Show_1_List_int(p, _list_7);
    printf("%s\n", _ps_8 ? _ps_8 : "");
    CS2SX_LINE(47);
    const char* _ps_9 = Printer_Show_2_int_int(p, 2, 3);
    printf("%s\n", _ps_9 ? _ps_9 : "");
    CS2SX_LINE(48);
    const char* _ps_10 = Printer_Show_2_int_str(p, 2, "b");
    printf("%s\n", _ps_10 ? _ps_10 : "");
    CS2SX_LINE(49);
    const char* _ps_11 = Printer_Show_0(p);
    printf("%s\n", _ps_11 ? _ps_11 : "");
    CS2SX_LINE(50);
    char* _sb0 = _cs2sx_next_buf();
    _sb0 = CS2SX_Format(_sb0, "%s %d", Printer_All(p), Printer_get_Calls(p));
    const char* _ps_12 = _sb0;
    printf("%s\n", _ps_12 ? _ps_12 : "");
    CS2SX_LINE(51);
    char* _sb1 = _cs2sx_next_buf();
    _sb1 = CS2SX_Format(_sb1, "%d %s", Printer_Twice_1_int(4), Printer_Twice_1_str("ab"));
    const char* _ps_13 = _sb1;
    printf("%s\n", _ps_13 ? _ps_13 : "");
    CS2SX_LINE(52);
    char* _sb2 = _cs2sx_next_buf();
    _sb2 = CS2SX_Format(_sb2, "%d %s", OverloadsApp_Area_1_int(3), Double_ToString(OverloadsApp_Area_1_double(1.5)));
    const char* _ps_14 = _sb2;
    printf("%s\n", _ps_14 ? _ps_14 : "");
    CS2SX_LINE(54);
    Func_str_str_t twice = (&_mg_Printer_Twice_1_str_dg);
    CS2SX_LINE(55);
    const char* _ps_15 = Func_str_str_Invoke(twice, "z");
    printf("%s\n", _ps_15 ? _ps_15 : "");
}

void OverloadsApp_Free(OverloadsApp* self)
{
    if (!self) return;
    free(self);
}
