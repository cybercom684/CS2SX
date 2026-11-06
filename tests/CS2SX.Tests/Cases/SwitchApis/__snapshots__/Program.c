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
    Graphics_Init(1280, 720);
    int r = CS2SX_Ftp_Start(5000);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "FTP %s:%d r=%d running=%d", CS2SX_Ftp_GetIP(), CS2SX_Ftp_GetPort(), r, CS2SX_Ftp_IsRunning());
    const char* _ps_0 = _sb0;
    printf("%s\n", _ps_0 ? _ps_0 : "");
    if (CS2SX_Nsp_Open("/x.nsp") == 1)
    {
        char* _sb1 = _cs2sx_next_buf();
        snprintf(_sb1, CS2SX_STRBUF_SIZE, "%s %lld %d %s", CS2SX_Nsp_TitleId(), CS2SX_Nsp_TotalSize(), CS2SX_Nsp_FileCount(), CS2SX_Nsp_FileName(0));
        const char* _ps_1 = _sb1;
        printf("%s\n", _ps_1 ? _ps_1 : "");
    }
    if (CS2SX_Usb_Init() == 0)
    {
        char* _sb2 = _cs2sx_next_buf();
        snprintf(_sb2, CS2SX_STRBUF_SIZE, "%d %s %lld", CS2SX_Usb_Count(), CS2SX_Usb_Name(0), CS2SX_Usb_Capacity(0));
        const char* _ps_2 = _sb2;
        printf("%s\n", _ps_2 ? _ps_2 : "");
    }
}

void SwitchApisApp_OnFrame(SwitchApisApp* self)
{
    if ((g_cs2sx_kHeld & HidNpadButton_Right))
    {
        self->x += 4;
    }
    if ((g_cs2sx_kHeld & HidNpadButton_Left))
    {
        self->x -= 4;
    }
    if ((g_cs2sx_kDown & HidNpadButton_A))
    {
        CS2SX_App_Quit();
    }
    if ((g_cs2sx_kDown & HidNpadButton_B))
    {
        CS2SX_App_LaunchNro("/switch/hbmenu.nro");
    }
    if ((g_cs2sx_kDown & HidNpadButton_X))
    {
        CS2SX_Ftp_Stop();
    }
    Graphics_BeginFrame();
    Graphics_FillScreen(COLOR_BLACK);
    Graphics_FillRect(self->x, 300, 80, 80, COLOR_ORANGE);
    Graphics_DrawCircle(640, 360, 50, COLOR_CYAN);
    char* _sb3 = _cs2sx_next_buf();
    snprintf(_sb3, CS2SX_STRBUF_SIZE, "x=%d", self->x);
    Graphics_DrawText(20, 20, _sb3, COLOR_WHITE, 2);
    Graphics_EndFrame();
}

void SwitchApisApp_Free(SwitchApisApp* self)
{
    if (!self) return;
    free(self);
}
