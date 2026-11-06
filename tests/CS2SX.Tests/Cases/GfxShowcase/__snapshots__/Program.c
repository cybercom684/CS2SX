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
    Graphics_Init(1280, 720);
    unsigned int* px = (unsigned int*)CS2SX_ArrNew(32 * 32, sizeof(unsigned int));
    for (int y = 0; y < 32; y++)
    {
        for (int x = 0; x < 32; x++)
        {
            px[y * 32 + x] = (((x / 8 + y / 8) % 2 == 0) ? COLOR_WHITE : COLOR_RED);
        }
    }
    self->f_checker = Texture_New(32, 32, px);
    unsigned int* gp = (unsigned int*)CS2SX_ArrNew(64 * 16, sizeof(unsigned int));
    for (int y = 0; y < 16; y++)
    {
        for (int x = 0; x < 64; x++)
        {
            gp[y * 64 + x] = Color_WithAlpha(COLOR_CYAN, ((unsigned char)(x * 4)));
        }
    }
    self->f_gradient = Texture_New(64, 16, gp);
}

void GfxShowcaseApp_Label(int x, int y, const char* text)
{
    Graphics_DrawText(x, y, text, COLOR_LGRAY, 1);
}

void GfxShowcaseApp_OnFrame(GfxShowcaseApp* self)
{
    self->f_frame++;
    Graphics_FillScreen(CS2SX_RGB(24, 26, 32));
    Graphics_DrawText(20, 12, "CS2SX GPU renderer", COLOR_WHITE, 3);
    Graphics_DrawTextShadow(560, 16, "shadow text", COLOR_YELLOW, COLOR_BLACK, 2);
    Graphics_FillRect(20, 70, 120, 70, COLOR_BLUE);
    GfxShowcaseApp_Label(20, 146, "FillRect");
    Graphics_DrawRect(160, 70, 120, 70, COLOR_WHITE);
    GfxShowcaseApp_Label(160, 146, "DrawRect 1px");
    Graphics_FillRoundedRect(300, 70, 120, 70, 18, COLOR_GREEN);
    GfxShowcaseApp_Label(300, 146, "FillRoundedRect");
    Graphics_DrawRoundedRect(440, 70, 120, 70, 18, COLOR_WHITE);
    GfxShowcaseApp_Label(440, 146, "DrawRoundedRect");
    Graphics_FillRect(580, 70, 120, 70, COLOR_RED);
    Graphics_FillRectAlpha(610, 90, 120, 70, COLOR_WHITE, 128);
    GfxShowcaseApp_Label(580, 166, "FillRectAlpha 50%");
    Graphics_DrawGrid(760, 70, 120, 70, 20, 14, COLOR_GRAY);
    GfxShowcaseApp_Label(760, 146, "DrawGrid");
    Graphics_FillCircle(80, 240, 45, COLOR_ORANGE);
    GfxShowcaseApp_Label(20, 292, "FillCircle");
    Graphics_DrawCircle(220, 240, 45, COLOR_WHITE);
    GfxShowcaseApp_Label(160, 292, "DrawCircle");
    Graphics_FillEllipse(360, 240, 60, 30, COLOR_PURPLE);
    GfxShowcaseApp_Label(300, 292, "FillEllipse");
    Graphics_DrawEllipse(500, 240, 60, 30, COLOR_WHITE);
    GfxShowcaseApp_Label(440, 292, "DrawEllipse");
    Graphics_FillTriangle(600, 285, 650, 195, 700, 285, COLOR_LIME);
    GfxShowcaseApp_Label(580, 292, "FillTriangle");
    Graphics_DrawTriangle(740, 285, 790, 195, 840, 285, COLOR_WHITE);
    GfxShowcaseApp_Label(720, 292, "DrawTriangle");
    CS2SX_ARR_LOCAL(int, xs, 5, 900, 960, 990, 950, 890);
    CS2SX_ARR_LOCAL(int, ys, 5, 200, 190, 240, 285, 260);
    Graphics_DrawPolygon(xs, ys, 5, COLOR_YELLOW);
    GfxShowcaseApp_Label(880, 292, "DrawPolygon");
    for (int i = 0; i < 12; i++)
    {
        int dx = (i * 23) % 120;
        int dy = 110 - (i * 9);
        Graphics_DrawLine(20, 330, 20 + dx + 10, 330 + dy, CS2SX_RGB(((unsigned char)(40 + i * 18)), 200, 255));
    }
    GfxShowcaseApp_Label(20, 452, "DrawLine (fan)");
    if (self->f_checker != NULL)
    {
        Graphics_DrawTexture(self->f_checker, 200, 340);
        GfxShowcaseApp_Label(200, 380, "DrawTexture");
        Graphics_DrawTextureScaled(self->f_checker, 300, 330, 96, 96);
        GfxShowcaseApp_Label(300, 432, "Scaled 3x (crisp)");
        Graphics_BlitPhysical(self->f_checker, 600, 500);
    }
    if (self->f_gradient != NULL)
    {
        Graphics_FillRect(440, 340, 140, 40, COLOR_RED);
        Graphics_DrawTextureScaled(self->f_gradient, 440, 340, 140, 40);
        GfxShowcaseApp_Label(440, 386, "texture alpha ramp");
    }
    Graphics_DrawText(20, 480, "Scale 1: The quick brown fox 0123456789", COLOR_WHITE, 1);
    Graphics_DrawText(20, 500, "Scale 2: ABC abc !?#", COLOR_WHITE, 2);
    Graphics_DrawTextAlpha(20, 530, "Alpha text 40%", COLOR_WHITE, 2, 100);
    for (int i = 0; i < 40; i++)
    {
        Graphics_SetPixel(700 + i * 2, 520, COLOR_WHITE);
    }
    GfxShowcaseApp_Label(700, 528, "SetPixel (physical)");
    int bx = 20 + (self->f_frame * 4) % 1180;
    Graphics_FillRoundedRect(bx, 640, 80, 40, 12, COLOR_CYAN);
    char* _sb0 = _cs2sx_next_buf();
    snprintf(_sb0, CS2SX_STRBUF_SIZE, "frame %d", self->f_frame);
    Graphics_DrawText(20, 696, _sb0, COLOR_GRAY, 1);
}

void GfxShowcaseApp_OnExit(GfxShowcaseApp* self)
{
    (self->f_checker != NULL ? (void)(Texture_Dispose(self->f_checker), self->f_checker = NULL) : (void)0);
    (self->f_gradient != NULL ? (void)(Texture_Dispose(self->f_gradient), self->f_gradient = NULL) : (void)0);
}

void GfxShowcaseApp_Free(GfxShowcaseApp* self)
{
    if (!self) return;
    if (self->f_checker) { Texture_Dispose(self->f_checker); self->f_checker = NULL; }
    if (self->f_gradient) { Texture_Dispose(self->f_gradient); self->f_gradient = NULL; }
    free(self);
}
