#include <stdlib.h>
#include "Program.h"

CS2SX_ARR_STATIC(int, ArraysApp_s_table, 8);
CS2SX_ARR_STATIC(const char*, ArraysApp_s_names, 3, "one", "two", "three");
void ArraysApp_Init(ArraysApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))ArraysApp_OnInit;
    self->f_weights = (float*)CS2SX_ArrFromInit((float[]){ 0.5f, 1.5f, 2.5f }, 3, sizeof(float));
}

void ArraysApp_OnInit(ArraysApp* self)
{
    CS2SX_ARR_LOCAL(int, grid, 4);
    for (int i = 0; i < 4; i++)
    {
        CS2SX_AT(grid, i) = i * 10;
    }
    CS2SX_AT(ArraysApp_s_table, 2) = 7;
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "%d %d %s %d", CS2SX_AT(grid, 3), CS2SX_AT(ArraysApp_s_table, 2), CS2SX_AT(ArraysApp_s_names, 1), CS2SX_ArrLen(CS2SX_NN(ArraysApp_s_names)));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_ARR_LOCAL(int, data, 4, 9, 4, 7, 1);
    qsort(data, 4, sizeof(*data), _cs2sx_cmp_int);
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "%d %d", CS2SX_AT(data, 0), CS2SX_AT(data, 4 - 1));
    const char* _ps_1 = _sb1;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    float total = 0;
    for (int _i_w = 0; _i_w < (int)(CS2SX_ArrLen(self->f_weights)); _i_w++)
    {
        float w = self->f_weights[_i_w];
        total += w;
    }
    printf("%s\n", Float_ToString(total));
}

void ArraysApp_Free(ArraysApp* self)
{
    if (!self) return;
    if (self->f_weights) { free(self->f_weights); self->f_weights = NULL; }
    free(self);
}
