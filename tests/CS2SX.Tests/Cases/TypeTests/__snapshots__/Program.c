#include <stdlib.h>
#include "_generics.h"
#include "_interfaces.h"
#include "Program.h"

static Shape_vtable Shape_vtable_instance =
{
};

void Shape_Ctor(Shape* self)
{
    CS2SX_TRACE("Shape..ctor", "Program.cs");
}

Shape* Shape_New(void)
{
    Shape* self = (Shape*)calloc(1, sizeof(Shape));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Shape__Destroy;
    self->_gc = Shape_Gc;
    self->_class = &Shape_ClassInfo;
    self->vtable = &Shape_vtable_instance;
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

static const char* _cs2sx_Shape_IShape_Kind_virt(void* self) { return ((Shape*)self)->vtable->Kind((Shape*)self); }
IShape_vtable Shape_IShape_vtable_instance =
{
    .Kind = _cs2sx_Shape_IShape_Kind_virt,
};


static Shape_vtable Circle_vtable_instance;

void Circle_Ctor(Circle* self)
{
    CS2SX_TRACE("Circle..ctor", "Program.cs");
    self->f_R = 2;
    Shape_Ctor((Shape*)self);
}

Circle* Circle_New(void)
{
    Circle* self = (Circle*)calloc(1, sizeof(Circle));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Circle__Destroy;
    self->base._gc = Circle_Gc;
    self->base._class = &Circle_ClassInfo;
    self->base.vtable = &Circle_vtable_instance;
    Circle_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Circle_Free);
    return self;
}

const char* Circle_Kind(Circle* self)
{
    CS2SX_TRACE("Circle.Kind", "Program.cs");
    CS2SX_LINE(17);
    return "circle";
}

const CS2SX_ClassInfo Circle_ClassInfo = { "Circle", &Shape_ClassInfo };

void Circle_Gc(void* _self, CS2SX_GcVisit visit)
{
    Circle* self = (Circle*)_self; (void)self;
    Shape_Gc(_self, visit);
}

void Circle__Destroy(Circle* self)
{
    Shape__Destroy((Shape*)self);
}

void Circle_Free(Circle* self)
{
    if (!self) return;
    Shape* _o = (Shape*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Shape_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Circle* Circle_Retain(Circle* self)
{
    if (self) ((Shape*)self)->_rc++;
    return self;
}

static Shape_vtable Circle_vtable_instance =
{
    .Kind = (const char*(*)(void*))Circle_Kind,
};

static Shape_vtable Square_vtable_instance;

void Square_Ctor(Square* self)
{
    CS2SX_TRACE("Square..ctor", "Program.cs");
    self->f_Side = 3;
    Shape_Ctor((Shape*)self);
}

Square* Square_New(void)
{
    Square* self = (Square*)calloc(1, sizeof(Square));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Square__Destroy;
    self->base._gc = Square_Gc;
    self->base._class = &Square_ClassInfo;
    self->base.vtable = &Square_vtable_instance;
    Square_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Square_Free);
    return self;
}

const char* Square_Kind(Square* self)
{
    CS2SX_TRACE("Square.Kind", "Program.cs");
    CS2SX_LINE(23);
    return "square";
}

const CS2SX_ClassInfo Square_ClassInfo = { "Square", &Shape_ClassInfo };

void Square_Gc(void* _self, CS2SX_GcVisit visit)
{
    Square* self = (Square*)_self; (void)self;
    Shape_Gc(_self, visit);
}

void Square__Destroy(Square* self)
{
    Shape__Destroy((Shape*)self);
}

void Square_Free(Square* self)
{
    if (!self) return;
    Shape* _o = (Shape*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Shape_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Square* Square_Retain(Square* self)
{
    if (self) ((Shape*)self)->_rc++;
    return self;
}

static Shape_vtable Square_vtable_instance =
{
    .Kind = (const char*(*)(void*))Square_Kind,
};

static Shape_vtable Cube_vtable_instance;

void Cube_Ctor(Cube* self)
{
    CS2SX_TRACE("Cube..ctor", "Program.cs");
    Square_Ctor((Square*)self);
}

Cube* Cube_New(void)
{
    Cube* self = (Cube*)calloc(1, sizeof(Cube));
    if (!self) return NULL;
    self->base.base._rc = 1;
    self->base.base._dtor = (void (*)(void*))Cube__Destroy;
    self->base.base._gc = Cube_Gc;
    self->base.base._class = &Cube_ClassInfo;
    self->base.base.vtable = &Cube_vtable_instance;
    Cube_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Cube_Free);
    return self;
}

const char* Cube_Kind(Cube* self)
{
    CS2SX_TRACE("Cube.Kind", "Program.cs");
    CS2SX_LINE(28);
    return "cube";
}

const CS2SX_ClassInfo Cube_ClassInfo = { "Cube", &Square_ClassInfo };

void Cube_Gc(void* _self, CS2SX_GcVisit visit)
{
    Cube* self = (Cube*)_self; (void)self;
    Square_Gc(_self, visit);
}

void Cube__Destroy(Cube* self)
{
    Square__Destroy((Square*)self);
}

void Cube_Free(Cube* self)
{
    if (!self) return;
    Shape* _o = (Shape*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Shape_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Cube* Cube_Retain(Cube* self)
{
    if (self) ((Shape*)self)->_rc++;
    return self;
}

static Shape_vtable Cube_vtable_instance =
{
    .Kind = (const char*(*)(void*))Cube_Kind,
};

void TypeTestsApp_Init(TypeTestsApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))TypeTestsApp_OnInit;
}

const char* TypeTestsApp_Describe(Shape* s)
{
    CS2SX_TRACE("TypeTestsApp.Describe", "Program.cs");
    CS2SX_LINE(35);
    if (({ Shape* _cs2sx_to = (Shape*)(s); _cs2sx_to && CS2SX_IsA(_cs2sx_to->_class, &Circle_ClassInfo); }))
    {
        Circle* c = (Circle*)s;
        char* _sb0 = _cs2sx_next_buf();
        _sb0 = CS2SX_Format(_sb0, "circle r%d", CS2SX_NN(c)->f_R);
        return _sb0;
    }
    CS2SX_LINE(36);
    if (({ Shape* _cs2sx_to = (Shape*)(s); _cs2sx_to && CS2SX_IsA(_cs2sx_to->_class, &Square_ClassInfo); }))
    {
        Square* q = (Square*)s;
        char* _sb1 = _cs2sx_next_buf();
        _sb1 = CS2SX_Format(_sb1, "%s side%d", ((({ Shape* _cs2sx_to = (Shape*)(s); _cs2sx_to && CS2SX_IsA(_cs2sx_to->_class, &Cube_ClassInfo); }) ? "cube" : "square")), CS2SX_NN(q)->f_Side);
        return _sb1;
    }
    CS2SX_LINE(37);
    return "?";
}

const char* TypeTestsApp_Switch(Shape* s)
{
    CS2SX_TRACE("TypeTestsApp.Switch", "Program.cs");
    CS2SX_LINE(42);
    switch (0) { default:
        if (({ Shape* _cs2sx_to = (Shape*)(s); _cs2sx_to && CS2SX_IsA(_cs2sx_to->_class, &Cube_ClassInfo); }))
        {
            CS2SX_LINE(44);
            return "C";
        }
        else if (({ Shape* _cs2sx_to = (Shape*)(s); _cs2sx_to && CS2SX_IsA(_cs2sx_to->_class, &Square_ClassInfo); }))
        {
            Square* sq = (Square*)s;
            CS2SX_LINE(45);
            char* _sb2 = _cs2sx_next_buf();
            _sb2 = CS2SX_Format(_sb2, "S%d", CS2SX_NN(sq)->f_Side);
            return _sb2;
        }
        else if (({ Shape* _cs2sx_to = (Shape*)(s); _cs2sx_to && CS2SX_IsA(_cs2sx_to->_class, &Circle_ClassInfo); }))
        {
            CS2SX_LINE(46);
            return "O";
        }
        else
        {
            CS2SX_LINE(47);
            return "-";
        }
    }
}

void TypeTestsApp_OnInit(TypeTestsApp* self)
{
    CS2SX_TRACE("TypeTestsApp.OnInit", "Program.cs");
    CS2SX_LINE(53);
    List_Shape* _list_0 = List_Shape_New();
    List_Shape_Add(_list_0, (Shape*)Circle_New());
    List_Shape_Add(_list_0, (Shape*)Square_New());
    List_Shape_Add(_list_0, (Shape*)Cube_New());
    List_Shape* shapes = _list_0;
    CS2SX_LINE(54);
    for (int _i_s = 0; _i_s < (int)(shapes->count); _i_s++)
    {
        Shape* s = List_Shape_Get(shapes, _i_s);
        char* _sb3 = _cs2sx_next_buf();
        _sb3 = CS2SX_Format(_sb3, "%s %s %s", TypeTestsApp_Describe(s), TypeTestsApp_Switch(s), CS2SX_BoolStr((({ Shape* _cs2sx_to = (Shape*)(s); _cs2sx_to && CS2SX_IsA(_cs2sx_to->_class, &Square_ClassInfo); }))));
        const char* _ps_1 = _sb3;
        printf("%s\n", _ps_1 ? _ps_1 : "");
    }
    CS2SX_LINE(57);
    IShape* i = Cube_as_IShape(Cube_New());
    CS2SX_LINE(58);
    char* _sb4 = _cs2sx_next_buf();
    _sb4 = CS2SX_Format(_sb4, "%s %s", CS2SX_BoolStr((({ CS2SX_IfaceHdr* _cs2sx_ti = (CS2SX_IfaceHdr*)(i); _cs2sx_ti && CS2SX_IsA(_cs2sx_ti->cls, &Square_ClassInfo); }))), CS2SX_BoolStr((({ CS2SX_IfaceHdr* _cs2sx_ti = (CS2SX_IfaceHdr*)(i); _cs2sx_ti && CS2SX_IsA(_cs2sx_ti->cls, &Circle_ClassInfo); }))));
    const char* _ps_2 = _sb4;
    printf("%s\n", _ps_2 ? _ps_2 : "");
    CS2SX_LINE(59);
    if (({ CS2SX_IfaceHdr* _cs2sx_ti = (CS2SX_IfaceHdr*)(i); _cs2sx_ti && CS2SX_IsA(_cs2sx_ti->cls, &Square_ClassInfo); }))
    {
        Square* sq = (Square*)((CS2SX_IfaceHdr*)(i))->obj;
        char* _sb5 = _cs2sx_next_buf();
        _sb5 = CS2SX_Format(_sb5, "side %d %s", CS2SX_NN(sq)->f_Side, ({ __auto_type _cs2sx_r = CS2SX_NN(sq); _cs2sx_r->base.vtable->Kind(_cs2sx_r); }));
        const char* _ps_3 = _sb5;
        printf("%s\n", _ps_3 ? _ps_3 : "");
    }
    CS2SX_LINE(61);
    Exception* e = (Exception*)(CS2SX_Exc_New(&CS2SX_TI_ArgumentNullException, NULL, "p", NULL));
    CS2SX_LINE(62);
    char* _sb6 = _cs2sx_next_buf();
    _sb6 = CS2SX_Format(_sb6, "%s %s", CS2SX_BoolStr((CS2SX_Exc_Is((Exception*)(e), &CS2SX_TI_ArgumentException))), CS2SX_BoolStr((CS2SX_Exc_Is((Exception*)(e), &CS2SX_TI_InvalidOperationException))));
    const char* _ps_4 = _sb6;
    printf("%s\n", _ps_4 ? _ps_4 : "");
    CS2SX_LINE(63);
    Shape* none = NULL;
    CS2SX_LINE(64);
    char* _sb7 = _cs2sx_next_buf();
    _sb7 = CS2SX_Format(_sb7, "%s done", CS2SX_BoolStr((({ Shape* _cs2sx_to = (Shape*)(none); _cs2sx_to && CS2SX_IsA(_cs2sx_to->_class, &Circle_ClassInfo); }))));
    const char* _ps_5 = _sb7;
    printf("%s\n", _ps_5 ? _ps_5 : "");
}

void TypeTestsApp_Free(TypeTestsApp* self)
{
    if (!self) return;
    free(self);
}
