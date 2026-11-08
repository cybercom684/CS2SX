#include <stdlib.h>
#include "Program.h"

void SwitchApisApp_Init(SwitchApisApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))SwitchApisApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))SwitchApisApp_OnFrame;
    self->x = 100;
}

void SwitchApisApp_OnInit(SwitchApisApp* self)
{
    CS2SX_TRACE("SwitchApisApp.OnInit", "Program.cs");
    CS2SX_LINE(7);
    Graphics_Init(1280, 720);
    CS2SX_LINE(8);
    int r = CS2SX_Ftp_Start(5000);
    CS2SX_LINE(9);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "FTP %s:%d r=%d running=%s", CS2SX_Ftp_GetIP(), CS2SX_Ftp_GetPort(), r, CS2SX_BoolStr(CS2SX_Ftp_IsRunning()));
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    CS2SX_LINE(10);
    if (CS2SX_Nsp_Open("/x.nsp") == 1)
    {
        CS2SX_LINE(11);
        char* _sb1 = _cs2sx_next_buf();
        snprintf(_sb1, CS2SX_STRBUF_SIZE, "%s %lld %d %s", CS2SX_Nsp_TitleId(), CS2SX_Nsp_TotalSize(), CS2SX_Nsp_FileCount(), CS2SX_Nsp_FileName(0));
        const char* _ps_1 = _sb1;
        printf("%s\n", _ps_1 ? _ps_1 : "");
    }
    CS2SX_LINE(12);
    if (CS2SX_Usb_Init() == 0)
    {
        CS2SX_LINE(13);
        char* _sb2 = _cs2sx_next_buf();
        snprintf(_sb2, CS2SX_STRBUF_SIZE, "%d %s %lld", CS2SX_Usb_Count(), CS2SX_Usb_Name(0), CS2SX_Usb_Capacity(0));
        const char* _ps_2 = _sb2;
        printf("%s\n", _ps_2 ? _ps_2 : "");
    }
}

void SwitchApisApp_OnFrame(SwitchApisApp* self)
{
    CS2SX_TRACE("SwitchApisApp.OnFrame", "Program.cs");
    CS2SX_LINE(18);
    if ((g_cs2sx_kHeld & HidNpadButton_Right))
    {
        CS2SX_LINE(18);
        self->x += 4;
    }
    CS2SX_LINE(19);
    if ((g_cs2sx_kHeld & HidNpadButton_Left))
    {
        CS2SX_LINE(19);
        self->x -= 4;
    }
    CS2SX_LINE(20);
    if ((g_cs2sx_kDown & HidNpadButton_A))
    {
        CS2SX_LINE(20);
        CS2SX_App_Quit();
    }
    CS2SX_LINE(21);
    if ((g_cs2sx_kDown & HidNpadButton_B))
    {
        CS2SX_LINE(21);
        CS2SX_App_LaunchNro("/switch/hbmenu.nro");
    }
    CS2SX_LINE(22);
    if ((g_cs2sx_kDown & HidNpadButton_X))
    {
        CS2SX_LINE(22);
        CS2SX_Ftp_Stop();
    }
    CS2SX_LINE(24);
    Graphics_BeginFrame();
    CS2SX_LINE(25);
    Graphics_FillScreen(COLOR_BLACK);
    CS2SX_LINE(26);
    Graphics_FillRect(self->x, 300, 80, 80, COLOR_ORANGE);
    CS2SX_LINE(27);
    Graphics_DrawCircle(640, 360, 50, COLOR_CYAN);
    CS2SX_LINE(28);
    char* _sb3 = _cs2sx_next_buf();
    snprintf(_sb3, CS2SX_STRBUF_SIZE, "x=%d", self->x);
    Graphics_DrawText(20, 20, _sb3, COLOR_WHITE, 2);
    CS2SX_LINE(29);
    Graphics_EndFrame();
}

void SwitchApisApp_Free(SwitchApisApp* self)
{
    if (!self) return;
    free(self);
}
