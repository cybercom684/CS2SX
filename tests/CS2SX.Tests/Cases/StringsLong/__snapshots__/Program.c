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
    self->_dtor = (void (*)(void*))Note__Destroy;
    self->_gc = Note_Gc;
    self->_class = &Note_ClassInfo;
    Note_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Note_Free);
    return self;
}

const CS2SX_ClassInfo Note_ClassInfo = { "Note", NULL };

void Note_Gc(void* _self, CS2SX_GcVisit visit)
{
    Note* self = (Note*)_self; (void)self;
}

static void Note_GcDyn(void* self, CS2SX_GcVisit visit) { ((Note*)self)->_gc(self, visit); }
const CS2SX_GcKind Note_GcKind = { offsetof(Note, _rc), Note_GcDyn, (CS2SX_FreeFn)Note_Free };

void Note__Destroy(Note* self)
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
        _sb1 = CS2SX_Format(_sb1, "concat %d %s", CS2SX_Str16Len(CS2SX_NN(s)), CS2SX_CharStr(CS2SX_Str16At(s, 4999)));
        const char* _ps_0 = _sb1;
        printf("%s\n", _ps_0 ? _ps_0 : "");
        CS2SX_LINE(37);
        char* _sb2 = _cs2sx_next_buf();
        _sb2 = CS2SX_Format(_sb2, "[%s]", s);
        const char* wrapped = _sb2;
        CS2SX_LINE(38);
        char* _sb3 = _cs2sx_next_buf();
        _sb3 = CS2SX_Format(_sb3, "interp %d %s", CS2SX_Str16Len(wrapped), CS2SX_CharStr(CS2SX_Str16At(wrapped, 5001)));
        const char* _ps_1 = _sb3;
        printf("%s\n", _ps_1 ? _ps_1 : "");
        CS2SX_LINE(39);
        char* _sb4 = _cs2sx_next_buf();
        _sb4 = CS2SX_Format(_sb4, "%s-%d", s, 7);
        const char* formatted = _sb4;
        CS2SX_LINE(40);
        char* _sb5 = _cs2sx_next_buf();
        _sb5 = CS2SX_Format(_sb5, "format %d %s", CS2SX_Str16Len(CS2SX_NN(formatted)), CS2SX_CharStr(CS2SX_Str16At(formatted, 5001)));
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
        _sb7 = CS2SX_Format(_sb7, "huge %d %s", CS2SX_Str16Len(CS2SX_NN(huge)), CS2SX_CharStr(CS2SX_Str16At(huge, 89999)));
        const char* _ps_3 = _sb7;
        printf("%s\n", _ps_3 ? _ps_3 : "");
        CS2SX_LINE(50);
        const char* up = CS2SX_TrimChars(CS2SX_NN(String_PadLeft(CS2SX_NN(String_SubstringFrom(CS2SX_NN(String_Replace(CS2SX_NN(String_ToUpper(CS2SX_NN(s))), "9", "nine")), 3)), 7000, '.')), (const char[]){ '.', 0 }, 3);
        CS2SX_LINE(51);
        List_str* _list_4 = List_str_New();
        List_str_Add(_list_4, s);
        List_str_Add(_list_4, s);
        List_str_Add(_list_4, s);
        List_str* parts = _list_4;
        CS2SX_LINE(52);
        char* _sb8 = _cs2sx_next_buf();
        _sb8 = CS2SX_Format(_sb8, "%s%s", String_Join(",", parts), CS2SX_CharRepeat('x', 3000));
        const char* joined = _sb8;
        CS2SX_LINE(53);
        char* _sb9 = _cs2sx_next_buf();
        _sb9 = CS2SX_Format(_sb9, "methods %d %d %d", CS2SX_Str16Len(CS2SX_NN(up)), CS2SX_Str16Len(CS2SX_NN(joined)), CS2SX_Str16Len(CS2SX_NN(CS2SX_TrimChars(CS2SX_NN(s), (const char[]){ '9', 0 }, 2))));
        const char* _ps_5 = _sb9;
        printf("%s\n", _ps_5 ? _ps_5 : "");
        CS2SX_LINE(55);
        char* _sb10 = _cs2sx_next_buf();
        _sb10 = CS2SX_Format(_sb10, "%s!", s);
        CS2SX_STR_STORE(self->f_kept, _sb10);
        CS2SX_LINE(56);
        for (int i = 0; i < 3; i++)
        {
            CS2SX_LINE(58);
            char* _sb11 = _cs2sx_next_buf();
            _sb11 = CS2SX_Format(_sb11, "item%d", i);
            List_str_Add(self->f_list, _sb11);
            CS2SX_LINE(59);
            char* _sb12 = _cs2sx_next_buf();
            _sb12 = CS2SX_Format(_sb12, "value%d", (i * i));
            char* _sb13 = _cs2sx_next_buf();
            _sb13 = CS2SX_Format(_sb13, "key%d", i);
            Dict_str_str_Set(self->f_map, _sb13, _sb12);
            CS2SX_LINE(60);
            char* _sb14 = _cs2sx_next_buf();
            _sb14 = CS2SX_Format(_sb14, "slot%d", (i + 10));
            CS2SX_AT(self->f_array, i) = _sb14;
        }
        CS2SX_LINE(62);
        char* _sb15 = _cs2sx_next_buf();
        _sb15 = CS2SX_Format(_sb15, "text %d", self->f_frame);
        CS2SX_STR_STORE(CS2SX_NN(self->f_note)->f_Text, _sb15);
        CS2SX_LINE(63);
        char* _sb16 = _cs2sx_next_buf();
        _sb16 = CS2SX_Format(_sb16, "title %d", self->f_frame);
        CS2SX_STR_STORE(CS2SX_NN(self->f_note)->f_Title, _sb16);
    }
    CS2SX_LINE(67);
    const char* noise = "";
    CS2SX_LINE(68);
    for (int i = 0; i < 200; i++)
    {
        CS2SX_LINE(68);
        char* _sb17 = _cs2sx_next_buf();
        _sb17 = CS2SX_Format(_sb17, "noise %d %d", i, self->f_frame);
        noise = _sb17;
    }
    CS2SX_LINE(70);
    if (self->f_frame == 40)
    {
        CS2SX_LINE(72);
        char* _sb18 = _cs2sx_next_buf();
        _sb18 = CS2SX_Format(_sb18, "kept %d %s", CS2SX_Str16Len(CS2SX_NN(self->f_kept)), CS2SX_CharStr(CS2SX_Str16At(self->f_kept, 5000)));
        const char* _ps_6 = _sb18;
        printf("%s\n", _ps_6 ? _ps_6 : "");
        CS2SX_LINE(73);
        char* _sb19 = _cs2sx_next_buf();
        _sb19 = CS2SX_Format(_sb19, "%s %s %s %s %s %s", List_str_Get(self->f_list, 0), List_str_Get(self->f_list, 2), Dict_str_str_Get(self->f_map, "key2"), CS2SX_AT(self->f_array, 1), CS2SX_NN(self->f_note)->f_Text, CS2SX_NN(self->f_note)->f_Title);
        const char* _ps_7 = _sb19;
        printf("%s\n", _ps_7 ? _ps_7 : "");
        CS2SX_LINE(75);
        const char* _ps_8 = (CS2SX_Str16Len(CS2SX_NN(noise)) > 0 ? "done" : "?");
        printf("%s\n", _ps_8 ? _ps_8 : "");
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
