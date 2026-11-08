#include <stdlib.h>
#include "Program.h"

void GfxShowcaseApp_Init(GfxShowcaseApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))GfxShowcaseApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))GfxShowcaseApp_OnFrame;
    ((SwitchApp*)self)->OnExit = (void(*)(SwitchApp*))GfxShowcaseApp_OnExit;
}

void GfxShowcaseApp_OnInit(GfxShowcaseApp* self)
{
    CS2SX_TRACE("GfxShowcaseApp.OnInit", "Program.cs");
    CS2SX_LINE(12);
    Graphics_Init(1280, 720);
    CS2SX_LINE(15);
    unsigned int* px = (unsigned int*)CS2SX_ArrNew(32 * 32, sizeof(unsigned int));
    CS2SX_LINE(16);
    for (int y = 0; y < 32; y++)
    {
        CS2SX_LINE(17);
        for (int x = 0; x < 32; x++)
        {
            CS2SX_LINE(18);
            CS2SX_AT(px, y * 32 + x) = (((x / 8 + y / 8) % 2 == 0) ? COLOR_WHITE : COLOR_RED);
        }
    }
    CS2SX_LINE(19);
    self->f_checker = Texture_New(32, 32, px);
    CS2SX_LINE(21);
    unsigned int* gp = (unsigned int*)CS2SX_ArrNew(64 * 16, sizeof(unsigned int));
    CS2SX_LINE(22);
    for (int y = 0; y < 16; y++)
    {
        CS2SX_LINE(23);
        for (int x = 0; x < 64; x++)
        {
            CS2SX_LINE(24);
            CS2SX_AT(gp, y * 64 + x) = Color_WithAlpha(COLOR_CYAN, ((unsigned char)(x * 4)));
        }
    }
    CS2SX_LINE(25);
    self->f_gradient = Texture_New(64, 16, gp);
}

void GfxShowcaseApp_Label(int x, int y, const char* text)
{
    CS2SX_TRACE("GfxShowcaseApp.Label", "Program.cs");
    CS2SX_LINE(30);
    Graphics_DrawText(x, y, text, COLOR_LGRAY, 1);
}

void GfxShowcaseApp_OnFrame(GfxShowcaseApp* self)
{
    CS2SX_TRACE("GfxShowcaseApp.OnFrame", "Program.cs");
    CS2SX_LINE(35);
    self->f_frame++;
    CS2SX_LINE(36);
    Graphics_FillScreen(CS2SX_RGB(24, 26, 32));
    CS2SX_LINE(37);
    Graphics_DrawText(20, 12, "CS2SX GPU renderer", COLOR_WHITE, 3);
    CS2SX_LINE(38);
    Graphics_DrawTextShadow(560, 16, "shadow text", COLOR_YELLOW, COLOR_BLACK, 2);
    CS2SX_LINE(41);
    Graphics_FillRect(20, 70, 120, 70, COLOR_BLUE);
    CS2SX_LINE(41);
    GfxShowcaseApp_Label(20, 146, "FillRect");
    CS2SX_LINE(42);
    Graphics_DrawRect(160, 70, 120, 70, COLOR_WHITE);
    CS2SX_LINE(42);
    GfxShowcaseApp_Label(160, 146, "DrawRect 1px");
    CS2SX_LINE(43);
    Graphics_FillRoundedRect(300, 70, 120, 70, 18, COLOR_GREEN);
    CS2SX_LINE(43);
    GfxShowcaseApp_Label(300, 146, "FillRoundedRect");
    CS2SX_LINE(44);
    Graphics_DrawRoundedRect(440, 70, 120, 70, 18, COLOR_WHITE);
    CS2SX_LINE(44);
    GfxShowcaseApp_Label(440, 146, "DrawRoundedRect");
    CS2SX_LINE(45);
    Graphics_FillRect(580, 70, 120, 70, COLOR_RED);
    CS2SX_LINE(46);
    Graphics_FillRectAlpha(610, 90, 120, 70, COLOR_WHITE, 128);
    CS2SX_LINE(46);
    GfxShowcaseApp_Label(580, 166, "FillRectAlpha 50%");
    CS2SX_LINE(47);
    Graphics_DrawGrid(760, 70, 120, 70, 20, 14, COLOR_GRAY);
    CS2SX_LINE(47);
    GfxShowcaseApp_Label(760, 146, "DrawGrid");
    CS2SX_LINE(50);
    Graphics_FillCircle(80, 240, 45, COLOR_ORANGE);
    CS2SX_LINE(50);
    GfxShowcaseApp_Label(20, 292, "FillCircle");
    CS2SX_LINE(51);
    Graphics_DrawCircle(220, 240, 45, COLOR_WHITE);
    CS2SX_LINE(51);
    GfxShowcaseApp_Label(160, 292, "DrawCircle");
    CS2SX_LINE(52);
    Graphics_FillEllipse(360, 240, 60, 30, COLOR_PURPLE);
    CS2SX_LINE(52);
    GfxShowcaseApp_Label(300, 292, "FillEllipse");
    CS2SX_LINE(53);
    Graphics_DrawEllipse(500, 240, 60, 30, COLOR_WHITE);
    CS2SX_LINE(53);
    GfxShowcaseApp_Label(440, 292, "DrawEllipse");
    CS2SX_LINE(54);
    Graphics_FillTriangle(600, 285, 650, 195, 700, 285, COLOR_LIME);
    CS2SX_LINE(54);
    GfxShowcaseApp_Label(580, 292, "FillTriangle");
    CS2SX_LINE(55);
    Graphics_DrawTriangle(740, 285, 790, 195, 840, 285, COLOR_WHITE);
    CS2SX_LINE(55);
    GfxShowcaseApp_Label(720, 292, "DrawTriangle");
    CS2SX_LINE(56);
    CS2SX_ARR_LOCAL(int, xs, 5, 900, 960, 990, 950, 890);
    CS2SX_LINE(57);
    CS2SX_ARR_LOCAL(int, ys, 5, 200, 190, 240, 285, 260);
    CS2SX_LINE(58);
    Graphics_DrawPolygon(xs, ys, 5, COLOR_YELLOW);
    CS2SX_LINE(58);
    GfxShowcaseApp_Label(880, 292, "DrawPolygon");
    CS2SX_LINE(61);
    for (int i = 0; i < 12; i++)
    {
        CS2SX_LINE(63);
        int dx = (i * 23) % 120;
        int dy = 110 - (i * 9);
        CS2SX_LINE(64);
        Graphics_DrawLine(20, 330, 20 + dx + 10, 330 + dy, CS2SX_RGB(((unsigned char)(40 + i * 18)), 200, 255));
    }
    CS2SX_LINE(66);
    GfxShowcaseApp_Label(20, 452, "DrawLine (fan)");
    CS2SX_LINE(69);
    if (self->f_checker != NULL)
    {
        CS2SX_LINE(71);
        Graphics_DrawTexture(self->f_checker, 200, 340);
        CS2SX_LINE(71);
        GfxShowcaseApp_Label(200, 380, "DrawTexture");
        CS2SX_LINE(72);
        Graphics_DrawTextureScaled(self->f_checker, 300, 330, 96, 96);
        CS2SX_LINE(72);
        GfxShowcaseApp_Label(300, 432, "Scaled 3x (crisp)");
        CS2SX_LINE(73);
        Graphics_BlitPhysical(self->f_checker, 600, 500);
    }
    CS2SX_LINE(75);
    if (self->f_gradient != NULL)
    {
        CS2SX_LINE(77);
        Graphics_FillRect(440, 340, 140, 40, COLOR_RED);
        CS2SX_LINE(78);
        Graphics_DrawTextureScaled(self->f_gradient, 440, 340, 140, 40);
        CS2SX_LINE(78);
        GfxShowcaseApp_Label(440, 386, "texture alpha ramp");
    }
    CS2SX_LINE(82);
    Graphics_DrawText(20, 480, "Scale 1: The quick brown fox 0123456789", COLOR_WHITE, 1);
    CS2SX_LINE(83);
    Graphics_DrawText(20, 500, "Scale 2: ABC abc !?#", COLOR_WHITE, 2);
    CS2SX_LINE(84);
    Graphics_DrawTextAlpha(20, 530, "Alpha text 40%", COLOR_WHITE, 2, 100);
    CS2SX_LINE(85);
    for (int i = 0; i < 40; i++)
    {
        CS2SX_LINE(85);
        Graphics_SetPixel(700 + i * 2, 520, COLOR_WHITE);
    }
    CS2SX_LINE(86);
    GfxShowcaseApp_Label(700, 528, "SetPixel (physical)");
    CS2SX_LINE(89);
    int bx = 20 + (self->f_frame * 4) % 1180;
    CS2SX_LINE(90);
    Graphics_FillRoundedRect(bx, 640, 80, 40, 12, COLOR_CYAN);
    CS2SX_LINE(91);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "frame %d", self->f_frame);
    Graphics_DrawText(20, 696, _sb0, COLOR_GRAY, 1);
}

void GfxShowcaseApp_OnExit(GfxShowcaseApp* self)
{
    CS2SX_TRACE("GfxShowcaseApp.OnExit", "Program.cs");
    CS2SX_LINE(96);
    (self->f_checker != NULL ? (void)(Texture_Dispose(self->f_checker), self->f_checker = NULL) : (void)0);
    CS2SX_LINE(97);
    (self->f_gradient != NULL ? (void)(Texture_Dispose(self->f_gradient), self->f_gradient = NULL) : (void)0);
}

void GfxShowcaseApp_Free(GfxShowcaseApp* self)
{
    if (!self) return;
    if (self->f_checker) { Texture_Dispose(self->f_checker); self->f_checker = NULL; }
    if (self->f_gradient) { Texture_Dispose(self->f_gradient); self->f_gradient = NULL; }
    free(self);
}
