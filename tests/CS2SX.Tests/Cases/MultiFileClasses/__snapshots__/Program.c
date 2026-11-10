#include <stdlib.h>
#include "_generics.h"
#include "Program.h"
#include "Shapes.h"
#include "Square.h"

void MultiFileClassesApp_Init(MultiFileClassesApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))MultiFileClassesApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))MultiFileClassesApp_OnFrame;
    self->f_shapes = List_Shape_Retain(List_Shape_New());
    self->f_painter = CS2SX_RETAIN(Painter, Painter_New());
    self->f_timer = CS2SX_Stopwatch_New();
}

void MultiFileClassesApp_OnInit(MultiFileClassesApp* self)
{
    CS2SX_TRACE("MultiFileClassesApp.OnInit", "Program.cs");
    CS2SX_LINE(16);
    List_Shape_Add(self->f_shapes, (Shape*)((Shape*)Square_New(2.0f)));
    CS2SX_LINE(17);
    List_Shape_Add(self->f_shapes, (Shape*)((Shape*)Square_New(3.0f)));
    CS2SX_LINE(18);
    CS2SX_Stopwatch_Start(self->f_timer);
    CS2SX_LINE(19);
    self->f_frameTimer = CS2SX_Stopwatch_StartNew();
}

void MultiFileClassesApp_OnFrame(MultiFileClassesApp* self)
{
    CS2SX_TRACE("MultiFileClassesApp.OnFrame", "Program.cs");
    CS2SX_LINE(24);
    float total = 0;
    CS2SX_LINE(25);
    for (int _i_s = 0; _i_s < (int)(self->f_shapes->count); _i_s++)
    {
        Shape* s = List_Shape_Get(self->f_shapes, _i_s);
        total += ({ __auto_type _cs2sx_r = CS2SX_NN(s); _cs2sx_r->vtable->Area(_cs2sx_r, self->f_painter, 1.0f); });
    }
    CS2SX_LINE(27);
    CS2SX_TouchState t = CS2SX_Input_GetTouch();
    CS2SX_LINE(28);
    int x = (t.count > 0 ? t.x[0] : -1);
    CS2SX_LINE(30);
    double ms = CS2SX_Stopwatch_ElapsedMsDouble(self->f_frameTimer);
    CS2SX_LINE(31);
    CS2SX_Stopwatch_Restart(self->f_frameTimer);
    CS2SX_LINE(32);
    printf("%s %d %d %lld %s\n", Float_ToString(total), 12, x, CS2SX_Stopwatch_ElapsedMs(CS2SX_NN(self->f_timer)), CS2SX_BoolStr(ms > 0));
}

void MultiFileClassesApp_Free(MultiFileClassesApp* self)
{
    if (!self) return;
    if (self->f_painter) { Painter_Free(self->f_painter); self->f_painter = NULL; }
    free(self);
}
