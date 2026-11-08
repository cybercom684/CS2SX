#include <stdlib.h>
#include "Program.h"

int Shape_Created;
static Shape_vtable Shape_vtable_instance =
{
    .Describe = (const char*(*)(void*))Shape_Describe,
};

void Shape_Ctor(Shape* self, const char* name)
{
    CS2SX_TRACE("Shape..ctor", "Program.cs");
    CS2SX_LINE(8);
    CS2SX_STR_STORE(self->f_Name, name);
    CS2SX_LINE(9);
    Shape_Created++;
}

Shape* Shape_New(const char* name)
{
    Shape* self = (Shape*)calloc(1, sizeof(Shape));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Shape_Dispose;
    self->vtable = &Shape_vtable_instance;
    Shape_Ctor(self, name);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Shape_Free);
    return self;
}

const char* Shape_Describe(Shape* self)
{
    CS2SX_TRACE("Shape.Describe", "Program.cs");
    CS2SX_LINE(16);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "%s with area %s", self->f_Name, Float_ToString(self->vtable->Area(self)));
    return _sb0;
}

void Shape_Dispose(Shape* self)
{
    CS2SX_StrDrop(self->f_Name); self->f_Name = NULL;
}

void Shape_Free(Shape* self)
{
    if (!self) return;
    Shape* _o = (Shape*)self;
    if (--_o->_rc > 0) return;
    _o->_dtor(_o);
    free(_o);
}

Shape* Shape_Retain(Shape* self)
{
    if (self) ((Shape*)self)->_rc++;
    return self;
}

static Shape_vtable Rect_vtable_instance;

void Rect_Ctor(Rect* self, float w, float h)
{
    CS2SX_TRACE("Rect..ctor", "Program.cs");
    Shape_Ctor((Shape*)self, "Rect");
    CS2SX_LINE(27);
    self->f_W = w;
    CS2SX_LINE(28);
    self->f_H = h;
}

Rect* Rect_New(float w, float h)
{
    Rect* self = (Rect*)calloc(1, sizeof(Rect));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Rect_Dispose;
    self->base.vtable = &Rect_vtable_instance;
    Rect_Ctor(self, w, h);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Rect_Free);
    return self;
}

float Rect_Area(Rect* self)
{
    CS2SX_TRACE("Rect.Area", "Program.cs");
    CS2SX_LINE(31);
    return self->f_W * self->f_H;
}

void Rect_Dispose(Rect* self)
{
    Shape_Dispose((Shape*)self);
}

void Rect_Free(Rect* self)
{
    if (!self) return;
    Shape* _o = (Shape*)self;
    if (--_o->_rc > 0) return;
    _o->_dtor(_o);
    free(_o);
}

Rect* Rect_Retain(Rect* self)
{
    if (self) ((Shape*)self)->_rc++;
    return self;
}

static Shape_vtable Rect_vtable_instance =
{
    .Area = (float(*)(void*))Rect_Area,
    .Describe = (const char*(*)(void*))Shape_Describe,
};

static Shape_vtable Square_vtable_instance;

void Square_Ctor(Square* self, float side)
{
    CS2SX_TRACE("Square..ctor", "Program.cs");
    Rect_Ctor((Rect*)self, side, side);
    CS2SX_LINE(38);
    CS2SX_STR_STORE(self->base.base.f_Name, "Square");
}

Square* Square_New(float side)
{
    Square* self = (Square*)calloc(1, sizeof(Square));
    if (!self) return NULL;
    self->base.base._rc = 1;
    self->base.base._dtor = (void (*)(void*))Square_Dispose;
    self->base.base.vtable = &Square_vtable_instance;
    Square_Ctor(self, side);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Square_Free);
    return self;
}

const char* Square_Describe(Square* self)
{
    CS2SX_TRACE("Square.Describe", "Program.cs");
    CS2SX_LINE(43);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "[%s]", Shape_Describe(((Shape*)self)));
    return _sb0;
}

void Square_Dispose(Square* self)
{
    Rect_Dispose((Rect*)self);
}

void Square_Free(Square* self)
{
    if (!self) return;
    Shape* _o = (Shape*)self;
    if (--_o->_rc > 0) return;
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
    .Describe = (const char*(*)(void*))Square_Describe,
    .Area = (float(*)(void*))Rect_Area,
};

static Shape_vtable Circle_vtable_instance;

void Circle_Ctor(Circle* self, float r)
{
    CS2SX_TRACE("Circle..ctor", "Program.cs");
    Shape_Ctor((Shape*)self, "Circle");
    CS2SX_LINE(53);
    self->f_r = r;
}

Circle* Circle_New(float r)
{
    Circle* self = (Circle*)calloc(1, sizeof(Circle));
    if (!self) return NULL;
    self->base._rc = 1;
    self->base._dtor = (void (*)(void*))Circle_Dispose;
    self->base.vtable = &Circle_vtable_instance;
    Circle_Ctor(self, r);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Circle_Free);
    return self;
}

float Circle_Area(Circle* self)
{
    CS2SX_TRACE("Circle.Area", "Program.cs");
    CS2SX_LINE(56);
    return 3.14159f * self->f_r * self->f_r;
}

void Circle_Dispose(Circle* self)
{
    Shape_Dispose((Shape*)self);
}

void Circle_Free(Circle* self)
{
    if (!self) return;
    Shape* _o = (Shape*)self;
    if (--_o->_rc > 0) return;
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
    .Area = (float(*)(void*))Circle_Area,
    .Describe = (const char*(*)(void*))Shape_Describe,
};

void ClassesInheritanceApp_Init(ClassesInheritanceApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))ClassesInheritanceApp_OnInit;
}

void ClassesInheritanceApp_OnInit(ClassesInheritanceApp* self)
{
    CS2SX_TRACE("ClassesInheritanceApp.OnInit", "Program.cs");
    CS2SX_LINE(63);
    Shape* a = (Shape*)(Rect_New(2, 3));
    CS2SX_LINE(64);
    Shape* b = (Shape*)(Circle_New(1));
    CS2SX_LINE(65);
    Shape* c = (Shape*)(Square_New(4));
    CS2SX_LINE(66);
    const char* _ps_0 = a->vtable->Describe(a);
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(67);
    const char* _ps_1 = b->vtable->Describe(b);
    printf("%s\n", _ps_1 ? _ps_1 : "");
    CS2SX_LINE(68);
    const char* _ps_2 = c->vtable->Describe(c);
    printf("%s\n", _ps_2 ? _ps_2 : "");
    CS2SX_LINE(69);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "created: %d", Shape_Created);
    const char* _ps_3 = _sb0;
    printf("%s\n", _ps_3 ? _ps_3 : "");
}

void ClassesInheritanceApp_Free(ClassesInheritanceApp* self)
{
    if (!self) return;
    free(self);
}
