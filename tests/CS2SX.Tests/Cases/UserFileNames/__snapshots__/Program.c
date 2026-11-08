#include <stdlib.h>
#include "Button.h"
#include "Input.h"
#include "Program.h"

void UserFileNamesApp_Init(UserFileNamesApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))UserFileNamesApp_OnFrame;
    self->f_input = CS2SX_RETAIN(InputMapper, InputMapper_New());
    self->f_entry = CS2SX_RETAIN(MenuEntry, MenuEntry_New("Start"));
}

void UserFileNamesApp_OnFrame(UserFileNamesApp* self)
{
    CS2SX_TRACE("UserFileNamesApp.OnFrame", "Program.cs");
    CS2SX_LINE(8);
    InputMapper_Poll(CS2SX_NN(self->f_input));
    CS2SX_LINE(9);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "%s %d", CS2SX_NN(self->f_entry)->f_Title, CS2SX_NN(self->f_input)->f_Presses);
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
}

void UserFileNamesApp_Free(UserFileNamesApp* self)
{
    if (!self) return;
    if (self->f_input) { InputMapper_Free(self->f_input); self->f_input = NULL; }
    if (self->f_entry) { MenuEntry_Free(self->f_entry); self->f_entry = NULL; }
    free(self);
}
