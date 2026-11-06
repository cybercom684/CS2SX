#include <stdlib.h>
#include "Program.h"

void ControlFlowApp_Init(ControlFlowApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))ControlFlowApp_OnInit;
}

void ControlFlowApp_OnInit(ControlFlowApp* self)
{
    int sum = 0;
    for (int i = 0; i < 20; i++)
    {
        if (i == 3)
        {
            continue;
        }
        if (i > 10)
        {
            break;
        }
        sum += i;
    }
    int n = 5;
    while (n > 0)
    {
        n--;
    }
    int k = 0;
    do
    {
        k += 2;
    }
    while (k < 7);
    int* maybe = NULL;
    int value = (maybe != NULL ? *maybe : -1);
    maybe = 4;
    if ((maybe != NULL))
    {
        value += (*maybe);
    }
    const char* text = NULL;
    const char* shown = (text != NULL ? text : "default");
    int flag = sum > 10 && !(k == 0) || value < 0;
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "%d %d %d %d %s %d", sum, n, k, value, shown, flag);
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    int x = 0;
    x += 5;
    x -= 1;
    x *= 3;
    x /= 2;
    x %= 5;
    x <<= 2;
    x >>= 1;
    x |= 1;
    x &= 7;
    x ^= 2;
    printf("%d\n", x);
}

void ControlFlowApp_Free(ControlFlowApp* self)
{
    if (!self) return;
    free(self);
}
