#include <stdlib.h>
#include "Program.h"

void Label2_Ctor(Label2* self)
{
    CS2SX_TRACE("Label2..ctor", "Program.cs");
    self->f_Name = CS2SX_StrOwn("n");
    self->f_Text = CS2SX_StrOwn("init");
}

Label2* Label2_New(void)
{
    Label2* self = (Label2*)calloc(1, sizeof(Label2));
    if (!self) return NULL;
    self->_rc = 1;
    self->_dtor = (void (*)(void*))Label2_Dispose;
    self->_gc = Label2_Gc;
    Label2_Ctor(self);
    CS2SX_ReleaseLater(self, (CS2SX_FreeFn)Label2_Free);
    return self;
}

void Label2_Gc(void* _self, CS2SX_GcVisit visit)
{
    Label2* self = (Label2*)_self; (void)self;
}

static void Label2_GcDyn(void* self, CS2SX_GcVisit visit) { ((Label2*)self)->_gc(self, visit); }
const CS2SX_GcKind Label2_GcKind = { offsetof(Label2, _rc), Label2_GcDyn, (CS2SX_FreeFn)Label2_Free };

void Label2_Dispose(Label2* self)
{
    CS2SX_StrDrop(self->f_Name); self->f_Name = NULL;
    CS2SX_StrDrop(self->f_Text); self->f_Text = NULL;
}

void Label2_Free(Label2* self)
{
    if (!self) return;
    Label2* _o = (Label2*)self;
    if (--_o->_rc > 0) { CS2SX_GcSuspect(_o, &Label2_GcKind); return; }
    CS2SX_GcForget(_o);
    _o->_dtor(_o);
    free(_o);
}

Label2* Label2_Retain(Label2* self)
{
    if (self) ((Label2*)self)->_rc++;
    return self;
}

const char* StringFieldsApp_s_last = "";
void StringFieldsApp_Init(StringFieldsApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))StringFieldsApp_OnFrame;
    self->f_status = CS2SX_StrOwn("start");
    self->f_log = CS2SX_StrOwn("");
    Label2* _label2_0 = Label2_New();
    CS2SX_STR_STORE(_label2_0->f_Name, "obj");
    self->f_label = CS2SX_RETAIN(Label2, _label2_0);
}

void StringFieldsApp_OnFrame(StringFieldsApp* self)
{
    CS2SX_TRACE("StringFieldsApp.OnFrame", "Program.cs");
    CS2SX_LINE(21);
    self->f_frame++;
    CS2SX_LINE(22);
    if (self->f_frame <= 3)
    {
        CS2SX_LINE(24);
        char* _sb0 = _cs2sx_next_buf();
        snprintf(_sb0, CS2SX_STRBUF_SIZE, "frame %d", self->f_frame);
        CS2SX_STR_STORE(self->f_status, _sb0);
        CS2SX_LINE(25);
        char* _sb1 = _cs2sx_next_buf();
        snprintf(_sb1, CS2SX_STRBUF_SIZE, "%d;", self->f_frame);
        char* _sb2 = _cs2sx_next_buf();
        snprintf(_sb2, CS2SX_STRBUF_SIZE, "%s%s", self->f_log ? self->f_log : "", _sb1 ? _sb1 : "");
        CS2SX_STR_STORE(self->f_log, _sb2);
        CS2SX_LINE(26);
        char* _sb3 = _cs2sx_next_buf();
        snprintf(_sb3, CS2SX_STRBUF_SIZE, "f%d", self->f_frame);
        CS2SX_STR_STORE(StringFieldsApp_s_last, _sb3);
        CS2SX_LINE(27);
        char* _sb4 = _cs2sx_next_buf();
        snprintf(_sb4, CS2SX_STRBUF_SIZE, "text %d", self->f_frame);
        CS2SX_STR_STORE(CS2SX_NN(self->f_label)->f_Text, _sb4);
        CS2SX_LINE(28);
        char* _sb5 = _cs2sx_next_buf();
        snprintf(_sb5, CS2SX_STRBUF_SIZE, "obj%d", self->f_frame);
        CS2SX_STR_STORE(CS2SX_NN(self->f_label)->f_Name, _sb5);
        CS2SX_LINE(29);
        CS2SX_STR_STORE(self->f_status, self->f_status);
    }
    CS2SX_LINE(31);
    if (self->f_frame == 5)
    {
        CS2SX_LINE(33);
        int len = 0;
        CS2SX_LINE(34);
        for (int i = 0; i < 2000; i++)
        {
            CS2SX_LINE(36);
            char* _sb6 = _cs2sx_next_buf();
            snprintf(_sb6, CS2SX_STRBUF_SIZE, "junk %d %d", i, i * i);
            const char* junk = _sb6;
            CS2SX_LINE(37);
            len += strlen(junk);
        }
        CS2SX_LINE(39);
        char* _sb7 = _cs2sx_next_buf();
        snprintf(_sb7, CS2SX_STRBUF_SIZE, "%s | %s | %s | %s | %s", self->f_status, self->f_log, StringFieldsApp_s_last, CS2SX_NN(self->f_label)->f_Text, CS2SX_NN(self->f_label)->f_Name);
        const char* _ps_1 = _sb7;
        printf("%s\n", _ps_1 ? _ps_1 : "");
        CS2SX_LINE(40);
        char* _sb8 = _cs2sx_next_buf();
        snprintf(_sb8, CS2SX_STRBUF_SIZE, "churn %s", CS2SX_BoolStr((len > 0)));
        const char* _ps_2 = _sb8;
        printf("%s\n", _ps_2 ? _ps_2 : "");
    }
}

void StringFieldsApp_Free(StringFieldsApp* self)
{
    if (!self) return;
    CS2SX_StrDrop(self->f_status); self->f_status = NULL;
    CS2SX_StrDrop(self->f_log); self->f_log = NULL;
    if (self->f_label) { Label2_Free(self->f_label); self->f_label = NULL; }
    free(self);
}
