#include <stdlib.h>
#include "Program.h"

void ControlFlowApp_Init(ControlFlowApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))ControlFlowApp_OnInit;
}

void ControlFlowApp_OnInit(ControlFlowApp* self)
{
    CS2SX_TRACE("ControlFlowApp.OnInit", "Program.cs");
    CS2SX_LINE(5);
    int sum = 0;
    CS2SX_LINE(6);
    for (int i = 0; i < 20; i++)
    {
        CS2SX_LINE(8);
        if (i == 3)
        {
            CS2SX_LINE(8);
            continue;
        }
        CS2SX_LINE(9);
        if (i > 10)
        {
            CS2SX_LINE(9);
            break;
        }
        CS2SX_LINE(10);
        sum += i;
    }
    CS2SX_LINE(13);
    int n = 5;
    CS2SX_LINE(14);
    while (n > 0)
    {
        CS2SX_LINE(14);
        n--;
    }
    CS2SX_LINE(16);
    int k = 0;
    CS2SX_LINE(17);
    do
    {
        CS2SX_LINE(17);
        k += 2;
    }
    while (k < 7);
    CS2SX_LINE(19);
    Nullable_int maybe = ((Nullable_int){0});
    CS2SX_LINE(20);
    int value = ((maybe).has ? (maybe).value : (-1));
    CS2SX_LINE(21);
    maybe = ((Nullable_int){ 4, 1 });
    CS2SX_LINE(22);
    if ((maybe).has)
    {
        CS2SX_LINE(22);
        value += (maybe).value;
    }
    CS2SX_LINE(24);
    const char* text = NULL;
    CS2SX_LINE(25);
    const char* shown = (text ?: "default");
    CS2SX_LINE(27);
    int flag = sum > 10 && !(k == 0) || value < 0;
    CS2SX_LINE(28);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "%d %d %d %d %s %s", sum, n, k, value, shown, CS2SX_BoolStr(flag));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(30);
    int x = 0;
    CS2SX_LINE(31);
    x += 5;
    CS2SX_LINE(31);
    x -= 1;
    CS2SX_LINE(31);
    x *= 3;
    CS2SX_LINE(31);
    x /= 2;
    CS2SX_LINE(31);
    x %= 5;
    CS2SX_LINE(32);
    x <<= 2;
    CS2SX_LINE(32);
    x >>= 1;
    CS2SX_LINE(32);
    x |= 1;
    CS2SX_LINE(32);
    x &= 7;
    CS2SX_LINE(32);
    x ^= 2;
    CS2SX_LINE(33);
    printf("%d\n", x);
}

void ControlFlowApp_Free(ControlFlowApp* self)
{
    if (!self) return;
    free(self);
}
