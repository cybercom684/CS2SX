#include <stdlib.h>
#include "Program.h"

void HelloWorldApp_Init(HelloWorldApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))HelloWorldApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))HelloWorldApp_OnFrame;
}

void HelloWorldApp_OnInit(HelloWorldApp* self)
{
    CS2SX_TRACE("HelloWorldApp.OnInit", "Program.cs");
    CS2SX_LINE(7);
    printf("Hello from CS2SX!\n");
    CS2SX_LINE(8);
    printf("Press + to exit.\n");
}

void HelloWorldApp_OnFrame(HelloWorldApp* self)
{
    CS2SX_TRACE("HelloWorldApp.OnFrame", "Program.cs");
    CS2SX_LINE(13);
    self->f_frames++;
    CS2SX_LINE(14);
    if (self->f_frames % 60 == 0)
    {
        CS2SX_LINE(15);
        char* _sb0 = _cs2sx_next_buf();
        snprintf(_sb0, CS2SX_STRBUF_SIZE, "seconds: %d", (self->f_frames / 60));
        const char* _ps_0 = _sb0;
        printf("%s\n", _ps_0 ? _ps_0 : "");
    }
}

void HelloWorldApp_Free(HelloWorldApp* self)
{
    if (!self) return;
    free(self);
}
