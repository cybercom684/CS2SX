#include <stdlib.h>
#include "Program.h"

int Shape_Created;
Shape* Shape_New(const char* name)
{
    Shape* self = (Shape*)malloc(sizeof(Shape));
    if (!self) return NULL;
    memset(self, 0, sizeof(Shape));
    self->_rc = 1;
    self->f_Name = _cs2sx_heap_strdup(name);
    Shape_Created++;
    return self;
}

const char* Shape_Describe(Shape* self)
{
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "%s with area %f", self->f_Name, Shape_Area(self));
    return _sb0;
}

void Shape_Free(Shape* self)
{
    if (!self) return;
    if (--self->_rc > 0) return;
    free(self);
}

Shape* Shape_Retain(Shape* self)
{
    if (self) self->_rc++;
    return self;
}

static Shape_vtable Rect_vtable_instance;

Rect* Rect_New(float w, float h)
{
    Rect* self = (Rect*)malloc(sizeof(Rect));
    if (!self) return NULL;
    memset(self, 0, sizeof(Rect));
    self->base._rc = 1;
    self->base.vtable = &Rect_vtable_instance;
    Shape* _base_0 = Shape_New("Rect");
    if (_base_0) { self->base = *_base_0; free(_base_0); }
    self->base.vtable = &Rect_vtable_instance;
    self->base._rc = 1;
    self->f_W = w;
    self->f_H = h;
    return self;
}

float Rect_Area(Rect* self)
{
    return self->f_W * self->f_H;
}

void Rect_Free(Rect* self)
{
    if (!self) return;
    Shape_Free((Shape*)self);
}

static Shape_vtable Rect_vtable_instance =
{
    .Area = (float(*)(void*))Rect_Area,
    .Describe = (const char*(*)(void*))Shape_Describe,
};

static Shape_vtable Square_vtable_instance;

Square* Square_New(float side)
{
    Square* self = (Square*)malloc(sizeof(Square));
    if (!self) return NULL;
    memset(self, 0, sizeof(Square));
    self->base.base._rc = 1;
    self->base.base.vtable = &Square_vtable_instance;
    Rect* _base_0 = Rect_New(side, side);
    if (_base_0) { self->base = *_base_0; free(_base_0); }
    self->base.base.vtable = &Square_vtable_instance;
    self->base.base._rc = 1;
    self->base.f_Name = "Square";
    return self;
}

const char* Square_Describe(Square* self)
{
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "[%s]", Rect_Describe(((Rect*)self)));
    return _sb0;
}

void Square_Free(Square* self)
{
    if (!self) return;
    Rect_Free((Rect*)self);
}

static Shape_vtable Square_vtable_instance =
{
    .Describe = (const char*(*)(void*))Square_Describe,
    .Area = (float(*)(void*))Rect_Area,
};

static Shape_vtable Circle_vtable_instance;

Circle* Circle_New(float r)
{
    Circle* self = (Circle*)malloc(sizeof(Circle));
    if (!self) return NULL;
    memset(self, 0, sizeof(Circle));
    self->base._rc = 1;
    self->base.vtable = &Circle_vtable_instance;
    Shape* _base_0 = Shape_New("Circle");
    if (_base_0) { self->base = *_base_0; free(_base_0); }
    self->base.vtable = &Circle_vtable_instance;
    self->base._rc = 1;
    self->f_r = r;
    return self;
}

float Circle_Area(Circle* self)
{
    return 3.14159f * self->f_r * self->f_r;
}

void Circle_Free(Circle* self)
{
    if (!self) return;
    Shape_Free((Shape*)self);
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
    Shape* a = (Shape*)(Rect_New(2, 3));
    Shape* b = (Shape*)(Circle_New(1));
    Shape* c = (Shape*)(Square_New(4));
    const char* _ps_0 = a->vtable->Describe(a);
    printf("%s\n", _ps_0 ? _ps_0 : "");
    const char* _ps_1 = b->vtable->Describe(b);
    printf("%s\n", _ps_1 ? _ps_1 : "");
    const char* _ps_2 = c->vtable->Describe(c);
    printf("%s\n", _ps_2 ? _ps_2 : "");
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "created: %d", Shape->f_Created);
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "created: %d", Shape->f_Created);
    const char* _ps_3 = _sb1;
    printf("%s\n", _ps_3 ? _ps_3 : "");
}

void ClassesInheritanceApp_Free(ClassesInheritanceApp* self)
{
    if (!self) return;
    free(self);
}
