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
    self->f_shapes = List_Shape_New();
    self->f_painter = Painter_New();
    self->f_timer = CS2SX_Stopwatch_New();
}

void MultiFileClassesApp_OnInit(MultiFileClassesApp* self)
{
    List_Shape_AddNew(self->f_shapes, (Shape*)(Square_New(2.0f)));
    List_Shape_AddNew(self->f_shapes, (Shape*)(Square_New(3.0f)));
    CS2SX_Stopwatch_Start(self->f_timer);
    self->f_frameTimer = CS2SX_Stopwatch_StartNew();
}

void MultiFileClassesApp_OnFrame(MultiFileClassesApp* self)
{
    float total = 0;
    for (int _i_s = 0; _i_s < (int)(self->f_shapes->count); _i_s++)
    {
        Shape* s = List_Shape_Get(self->f_shapes, _i_s);
        total += ({ __auto_type _cs2sx_r = CS2SX_NN(s); _cs2sx_r->vtable->Area(_cs2sx_r, self->f_painter, 1.0f); });
    }
    CS2SX_TouchState t = CS2SX_Input_GetTouch();
    int x = (t.count > 0 ? t.x[0] : -1);
    double ms = CS2SX_Stopwatch_ElapsedMsDouble(self->f_frameTimer);
    CS2SX_Stopwatch_Restart(self->f_frameTimer);
    printf("%s %d %d %lld %s\n", Float_ToString(total), 12, x, CS2SX_Stopwatch_ElapsedMs(CS2SX_NN(self->f_timer)), CS2SX_BoolStr(ms > 0));
}

void MultiFileClassesApp_Free(MultiFileClassesApp* self)
{
    if (!self) return;
    if (self->f_painter) { Painter_Free(self->f_painter); self->f_painter = NULL; }
    free(self);
}
