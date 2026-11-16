#include <stdlib.h>
#include "Program.h"

void Note_Ctor(Note* self)
{
    CS2SX_TRACE("Note..ctor", "Program.cs");
    self->f_Text = CS2SX_StrOwn("");
    self->f_Title = CS2SX_StrOwn("");
}

Note* Note_New(void)
{
    Note* self = (Note*)calloc(1, sizeof(Note));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Note_Dispose;
    self->_gc = Note_Gc;
    Note_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Note_Free);
    return self;
}

void Note_Gc(void* _self, CS2SX_GcVisit visit)
{
    Note* self = (Note*)_self; (void)self;
}

static void Note_GcDyn(void* self, CS2SX_GcVisit visit) { ((Note*)self)->_gc(self, visit); }
const CS2SX_GcKind Note_GcKind = { offsetof(Note, _rc), Note_GcDyn, (CS2SX_FreeFn)Note_Free };

void Note_Dispose(Note* self)
{
    CS2SX_StrDrop(self->f_Text); self->f_Text = NULL;
    CS2SX_StrDrop(self->f_Title); self->f_Title = NULL;
}

void Note_Free(Note* self)
{
    if (!self) return;
    Note* _o = (Note*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Note_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Note* Note_Retain(Note* self)
{
    if (self) ((Note*)self)->_rc++;
    return self;
}

void StringsLongApp_Init(StringsLongApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))StringsLongApp_OnFrame;
    self->f_kept = CS2SX_StrOwn("");
    self->f_list = List_str_Retain(List_str_New());
    self->f_map = Dict_str_str_Retain(Dict_str_str_New());
    self->f_array = (const char**)CS2SX_ArrNew(3, sizeof(const char*));
    self->f_note = CS2SX_RETAIN(Note, Note_New());
}

const char* StringsLongApp_Repeat(const char* part, int times)
{
    CS2SX_TRACE("StringsLongApp.Repeat", "Program.cs");
    CS2SX_LINE(25);
    const char* s = "";
    CS2SX_LINE(26);
    for (int i = 0; i < times; i++)
    {
        CS2SX_LINE(26);
        char* _sb0 = _cs2sx_next_buf();
        _sb0 = CS2SX_Format(_sb0, "%s%s", s ? s : "", part ? part : "");
        s = _sb0;
    }
    CS2SX_LINE(27);
    return s;
}

void StringsLongApp_OnFrame(StringsLongApp* self)
{
    CS2SX_TRACE("StringsLongApp.OnFrame", "Program.cs");
    CS2SX_LINE(32);
    self->f_frame++;
    CS2SX_LINE(33);
    if (self->f_frame == 1)
    {
        CS2SX_LINE(35);
        const char* s = StringsLongApp_Repeat("0123456789", 500);
        CS2SX_LINE(36);
        char* _sb1 = _cs2sx_next_buf();
        _sb1 = CS2SX_Format(_sb1, "concat %d %c", strlen(CS2SX_NN(s)), s[4999]);
        const char* _ps_0 = _sb1;
        printf("%s\n", _ps_0 ? _ps_0 : "");
        CS2SX_LINE(37);
        char* _sb2 = _cs2sx_next_buf();
        _sb2 = CS2SX_Format(_sb2, "[%s]", s);
        const char* wrapped = _sb2;
        CS2SX_LINE(38);
        char* _sb3 = _cs2sx_next_buf();
        _sb3 = CS2SX_Format(_sb3, "interp %d %c", strlen(wrapped), wrapped[5001]);
        const char* _ps_1 = _sb3;
        printf("%s\n", _ps_1 ? _ps_1 : "");
        CS2SX_LINE(39);
        char* _sb4 = _cs2sx_next_buf();
        _sb4 = CS2SX_Format(_sb4, "%s-%d", s, 7);
        const char* formatted = _sb4;
        CS2SX_LINE(40);
        char* _sb5 = _cs2sx_next_buf();
        _sb5 = CS2SX_Format(_sb5, "format %d %c", strlen(CS2SX_NN(formatted)), formatted[5001]);
        const char* _ps_2 = _sb5;
        printf("%s\n", _ps_2 ? _ps_2 : "");
        CS2SX_LINE(42);
        StringBuilder* sb = StringBuilder_New(256);
        CS2SX_LINE(43);
        StringBuilder_AppendStr(sb, s);
        CS2SX_LINE(44);
        StringBuilder_AppendStr(sb, s);
        CS2SX_LINE(45);
        const char* big = StringBuilder_ToString(sb);
        CS2SX_LINE(46);
        char* _sb6 = _cs2sx_next_buf();
        _sb6 = CS2SX_Format(_sb6, "%s%s%s%s%s%s%s%s%s", big, big, big, big, big, big, big, big, big);
        const char* huge = _sb6;
        CS2SX_LINE(47);
        char* _sb7 = _cs2sx_next_buf();
        _sb7 = CS2SX_Format(_sb7, "huge %d %c", strlen(CS2SX_NN(huge)), huge[89999]);
        const char* _ps_3 = _sb7;
        printf("%s\n", _ps_3 ? _ps_3 : "");
        CS2SX_LINE(49);
        char* _sb8 = _cs2sx_next_buf();
        _sb8 = CS2SX_Format(_sb8, "%s!", s);
        CS2SX_STR_STORE(self->f_kept, _sb8);
        CS2SX_LINE(50);
        for (int i = 0; i < 3; i++)
        {
            CS2SX_LINE(52);
            char* _sb9 = _cs2sx_next_buf();
            _sb9 = CS2SX_Format(_sb9, "item%d", i);
            List_str_Add(self->f_list, _sb9);
            CS2SX_LINE(53);
            char* _sb10 = _cs2sx_next_buf();
            _sb10 = CS2SX_Format(_sb10, "value%d", (i * i));
            char* _sb11 = _cs2sx_next_buf();
            _sb11 = CS2SX_Format(_sb11, "key%d", i);
            Dict_str_str_Set(self->f_map, _sb11, _sb10);
            CS2SX_LINE(54);
            char* _sb12 = _cs2sx_next_buf();
            _sb12 = CS2SX_Format(_sb12, "slot%d", (i + 10));
            CS2SX_AT(self->f_array, i) = _sb12;
        }
        CS2SX_LINE(56);
        char* _sb13 = _cs2sx_next_buf();
        _sb13 = CS2SX_Format(_sb13, "text %d", self->f_frame);
        CS2SX_STR_STORE(CS2SX_NN(self->f_note)->f_Text, _sb13);
        CS2SX_LINE(57);
        char* _sb14 = _cs2sx_next_buf();
        _sb14 = CS2SX_Format(_sb14, "title %d", self->f_frame);
        CS2SX_STR_STORE(CS2SX_NN(self->f_note)->f_Title, _sb14);
    }
    CS2SX_LINE(61);
    const char* noise = "";
    CS2SX_LINE(62);
    for (int i = 0; i < 200; i++)
    {
        CS2SX_LINE(62);
        char* _sb15 = _cs2sx_next_buf();
        _sb15 = CS2SX_Format(_sb15, "noise %d %d", i, self->f_frame);
        noise = _sb15;
    }
    CS2SX_LINE(64);
    if (self->f_frame == 40)
    {
        CS2SX_LINE(66);
        char* _sb16 = _cs2sx_next_buf();
        _sb16 = CS2SX_Format(_sb16, "kept %d %c", strlen(CS2SX_NN(self->f_kept)), self->f_kept[5000]);
        const char* _ps_4 = _sb16;
        printf("%s\n", _ps_4 ? _ps_4 : "");
        CS2SX_LINE(67);
        char* _sb17 = _cs2sx_next_buf();
        _sb17 = CS2SX_Format(_sb17, "%s %s %s %s %s %s", List_str_Get(self->f_list, 0), List_str_Get(self->f_list, 2), Dict_str_str_Get(self->f_map, "key2"), CS2SX_AT(self->f_array, 1), CS2SX_NN(self->f_note)->f_Text, CS2SX_NN(self->f_note)->f_Title);
        const char* _ps_5 = _sb17;
        printf("%s\n", _ps_5 ? _ps_5 : "");
        CS2SX_LINE(69);
        const char* _ps_6 = (strlen(CS2SX_NN(noise)) > 0 ? "done" : "?");
        printf("%s\n", _ps_6 ? _ps_6 : "");
    }
}

void StringsLongApp_Free(StringsLongApp* self)
{
    if (!self) return;
    CS2SX_StrDrop(self->f_kept); self->f_kept = NULL;
    if (self->f_list) { List_str_Free(self->f_list); self->f_list = NULL; }
    if (self->f_map) { Dict_str_str_Free(self->f_map); self->f_map = NULL; }
    CS2SX_ArrFree(self->f_array); self->f_array = NULL;
    if (self->f_note) { Note_Free(self->f_note); self->f_note = NULL; }
    free(self);
}
