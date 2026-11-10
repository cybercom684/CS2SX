#include <stdlib.h>
#include "Program.h"

void Board_Ctor(Board* self, int n)
{
    CS2SX_TRACE("Board..ctor", "Program.cs");
    CS2SX_LINE(7);
    self->f_Cells = (int*)CS2SX_ArrNew(n, sizeof(int));
}

Board* Board_New(int n)
{
    Board* self = (Board*)calloc(1, sizeof(Board));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Board_Dispose;
    self->_gc = Board_Gc;
    Board_Ctor(self, n);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Board_Free);
    return self;
}

int Board_Sum(int* values)
{
    CS2SX_TRACE("Board.Sum", "Program.cs");
    CS2SX_LINE(11);
    int s = 0;
    CS2SX_LINE(12);
    for (int i = 0; i < CS2SX_ArrLen(CS2SX_NN(values)); i++)
    {
        CS2SX_LINE(12);
        s += CS2SX_AT(values, i);
    }
    CS2SX_LINE(13);
    return s;
}

int Board_Count(int* values)
{
    CS2SX_TRACE("Board.Count", "Program.cs");
    CS2SX_LINE(16);
    return CS2SX_ArrLen(CS2SX_NN(values));
}

int* Board_Doubled(Board* self)
{
    CS2SX_TRACE("Board.Doubled", "Program.cs");
    CS2SX_LINE(20);
    int* result = (int*)CS2SX_ArrNew(CS2SX_ArrLen(CS2SX_NN(self->f_Cells)), sizeof(int));
    CS2SX_LINE(21);
    for (int i = 0; i < CS2SX_ArrLen(CS2SX_NN(self->f_Cells)); i++)
    {
        CS2SX_LINE(21);
        CS2SX_AT(result, i) = CS2SX_AT(self->f_Cells, i) * 2;
    }
    CS2SX_LINE(22);
    return result;
}

void Board_Gc(void* _self, CS2SX_GcVisit visit)
{
    Board* self = (Board*)_self; (void)self;
}

static void Board_GcDyn(void* self, CS2SX_GcVisit visit) { ((Board*)self)->_gc(self, visit); }
const CS2SX_GcKind Board_GcKind = { offsetof(Board, _rc), Board_GcDyn, (CS2SX_FreeFn)Board_Free };

void Board_Dispose(Board* self)
{
    CS2SX_ArrFree(self->f_Cells); self->f_Cells = NULL;
}

void Board_Free(Board* self)
{
    if (!self) return;
    Board* _o = (Board*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Board_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Board* Board_Retain(Board* self)
{
    if (self) ((Board*)self)->_rc++;
    return self;
}

void ArraysAdvancedApp_Init(ArraysAdvancedApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))ArraysAdvancedApp_OnInit;
}

void ArraysAdvancedApp_OnInit(ArraysAdvancedApp* self)
{
    CS2SX_TRACE("ArraysAdvancedApp.OnInit", "Program.cs");
    CS2SX_LINE(30);
    Board* b = Board_New(5);
    CS2SX_LINE(31);
    for (int i = 0; i < CS2SX_ArrLen(CS2SX_NN(b->f_Cells)); i++)
    {
        CS2SX_LINE(31);
        CS2SX_AT(b->f_Cells, i) = i;
    }
    CS2SX_LINE(33);
    int* twice = Board_Doubled(b);
    CS2SX_LINE(34);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "%d %d", Board_Sum(twice), CS2SX_ArrLen(CS2SX_NN(twice)));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(36);
    int* nums = (int*)CS2SX_ArrFromInit((int[]){ 3, 1, 2 }, 3, sizeof(int));
    CS2SX_LINE(37);
    CS2SX_ARR_LOCAL(int, _pa_1, 2, 7, 8);
    char* _sb1 = _cs2sx_next_buf();
    snprintf(_sb1, CS2SX_STRBUF_SIZE, "%d %d", Board_Count(nums), Board_Count(_pa_1));
    const char* _ps_2 = _sb1;
    printf("%s\n", _ps_2 ? _ps_2 : "");
    CS2SX_LINE(39);
    int* grow = (int*)CS2SX_ArrFromInit((int[]){ 1, 2 }, 2, sizeof(int));
    CS2SX_LINE(40);
    (grow = (int*)CS2SX_ArrResize(grow, 4, sizeof(int)));
    CS2SX_LINE(41);
    CS2SX_AT(grow, 3) = 9;
    CS2SX_LINE(42);
    char* _sb2 = _cs2sx_next_buf();
    snprintf(_sb2, CS2SX_STRBUF_SIZE, "%d %d", CS2SX_ArrLen(CS2SX_NN(grow)), CS2SX_AT(grow, 3));
    const char* _ps_3 = _sb2;
    printf("%s\n", _ps_3 ? _ps_3 : "");
    CS2SX_LINE(44);
    int* scratch = (int*)CS2SX_ArrNew(2, sizeof(int));
    CS2SX_LINE(45);
    scratch = (int*)CS2SX_ArrNew(6, sizeof(int));
    CS2SX_LINE(46);
    printf("%d\n", CS2SX_ArrLen(scratch));
    CS2SX_LINE(48);
    __auto_type _fe_4 = CS2SX_StrArr_FromList(String_Split("red green blue", " "));
    for (int _i_word = 0; _i_word < (int)(CS2SX_ArrLen(_fe_4)); _i_word++)
    {
        const char* word = _fe_4[_i_word];
        const char* _ps_5 = String_ToUpper(CS2SX_NN(word));
        printf("%s\n", _ps_5 ? _ps_5 : "");
    }
    CS2SX_LINE(51);
    const char** parts = CS2SX_StrArr_FromList(String_Split("a;b", ";"));
    CS2SX_LINE(52);
    char* _sb3 = _cs2sx_next_buf();
    snprintf(_sb3, CS2SX_STRBUF_SIZE, "%s %d", String_JoinArr("+", parts), CS2SX_ArrLen(CS2SX_NN(parts)));
    const char* _ps_6 = _sb3;
    printf("%s\n", _ps_6 ? _ps_6 : "");
}

void ArraysAdvancedApp_Free(ArraysAdvancedApp* self)
{
    if (!self) return;
    free(self);
}
