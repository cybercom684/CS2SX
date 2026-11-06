#include <stdlib.h>
#include "Program.h"

int ArraysApp_s_table[8] = {0};
#define ArraysApp_s_table_count (8)
const char* ArraysApp_s_names[] = { "one", "two", "three" };
#define ArraysApp_s_names_count 3
void ArraysApp_Init(ArraysApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))ArraysApp_OnInit;
    self->f_weights = (float[]){ 0.5f, 1.5f, 2.5f };
}

void ArraysApp_OnInit(ArraysApp* self)
{
    int grid[4];
    memset(grid, 0, sizeof(grid));
    for (int i = 0; i < grid->f_Length; i++)
    {
        grid[i] = i * 10;
    }
    ArraysApp_s_table[2] = 7;
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "%d %d %s %d", grid[3], ArraysApp_s_table[2], ArraysApp_s_names[1], ArraysApp_s_names_count);
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "%d %d %s %d", grid[3], ArraysApp_s_table[2], ArraysApp_s_names[1], ArraysApp_s_names_count);
    const char* _ps_0 = _sb1;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    int[] data = (int[]){ 9, 4, 7, 1 };
    qsort(data, 0 /* length unknown */, sizeof(*data), _cs2sx_cmp_int);
    char* _sb2 = _cs2sx_next_buf();
    snprintf(_sb2, CS2SX_STRBUF_SIZE, "%d %d", data[0], data[data->f_Length - 1]);
    char* _sb3 = _cs2sx_next_buf();
    snprintf(_sb3, CS2SX_STRBUF_SIZE, "%d %d", data[0], data[data->f_Length - 1]);
    const char* _ps_1 = _sb3;
    printf("%s\n", _ps_1 ? _ps_1 : "");
    float total = 0;
    for (int _i_w = 0; _i_w < (int)(self->f_weights_count); _i_w++)
    {
        float w = self->f_weights[_i_w];
        total += w;
    }
    printf("%f\n", total);
}

void ArraysApp_Free(ArraysApp* self)
{
    if (!self) return;
    if (self->f_weights) { free(self->f_weights); self->f_weights = NULL; }
    free(self);
}
