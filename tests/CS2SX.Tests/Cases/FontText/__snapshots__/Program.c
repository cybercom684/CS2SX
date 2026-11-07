#include <stdlib.h>
#include "Program.h"

void FontTextApp_Init(FontTextApp* self)
{
    if (!self) return;
    ((SwitchApp*)self)->OnInit = (void(*)(SwitchApp*))FontTextApp_OnInit;
    ((SwitchApp*)self)->OnFrame = (void(*)(SwitchApp*))FontTextApp_OnFrame;
    ((SwitchApp*)self)->OnExit = (void(*)(SwitchApp*))FontTextApp_OnExit;
}

void FontTextApp_OnInit(FontTextApp* self)
{
    Graphics_Init(1280, 720);
    self->f_font = (CS2SX_File_Exists("romfs:/font.ttf") ? Font_Load("romfs:/font.ttf") : Font_LoadSystem());
}

void FontTextApp_OnFrame(FontTextApp* self)
{
    self->f_frame++;
    Graphics_FillScreen(CS2SX_RGB(24, 26, 32));
    if (self->f_font == NULL)
    {
        Graphics_DrawText(20, 20, "No font: system font unavailable, no romfs:/font.ttf", COLOR_RED, 2);
        return;
    }
    int y = 16;
    CS2SX_ARR_LOCAL(int, sizes, 6, 12, 16, 24, 32, 48, 72);
    for (int _i_size = 0; _i_size < (int)(6); _i_size++)
    {
        int size = sizes[_i_size];
        char* _sb0 = _cs2sx_next_buf();
        snprintf(_sb0, CS2SX_STRBUF_SIZE, "%d px - The quick brown fox jumps", size);
        char* _sb1 = _cs2sx_next_buf();
        snprintf(_sb1, CS2SX_STRBUF_SIZE, "%d px - The quick brown fox jumps", size);
        char* _sb2 = _cs2sx_next_buf();
        snprintf(_sb2, CS2SX_STRBUF_SIZE, "%d px - The quick brown fox jumps", size);
        Font_Draw(self->f_font, 20, y, _sb2, COLOR_WHITE, size);
        y += Font_LineHeight(self->f_font, size);
    }
    const char* greet = "Grüße, Ünïcödé: äöü ÄÖÜ ß € ©";
    int w = Font_Measure(self->f_font, greet, 32);
    Graphics_FillRect(20, y + 4, w, Font_LineHeight(self->f_font, 32), CS2SX_RGB(60, 70, 120));
    Font_Draw(self->f_font, 20, y + 4, greet, COLOR_YELLOW, 32);
    y += Font_LineHeight(self->f_font, 32) + 12;
    Font_Draw(self->f_font, 20, y, "Multi-line text:\nsecond line\nthird line", COLOR_CYAN, 24);
    Font_Draw(self->f_font, 700, y, "50% alpha text", Color_WithAlpha(COLOR_WHITE, 128), 40);
    y += Font_LineHeight(self->f_font, 24) * 3 + 8;
    char* _sb3 = _cs2sx_next_buf();
    snprintf(_sb3, CS2SX_STRBUF_SIZE, "Frame %d  AV To Wa", self->f_frame);
    char* _sb4 = _cs2sx_next_buf();
    snprintf(_sb4, CS2SX_STRBUF_SIZE, "Frame %d  AV To Wa", self->f_frame);
    char* _sb5 = _cs2sx_next_buf();
    snprintf(_sb5, CS2SX_STRBUF_SIZE, "Frame %d  AV To Wa", self->f_frame);
    Font_Draw(self->f_font, 20, y, _sb5, COLOR_LIME, 36);
    char* _sb6 = _cs2sx_next_buf();
    snprintf(_sb6, CS2SX_STRBUF_SIZE, "w=%d", w);
    char* _sb7 = _cs2sx_next_buf();
    snprintf(_sb7, CS2SX_STRBUF_SIZE, "w=%d", w);
    char* _sb8 = _cs2sx_next_buf();
    snprintf(_sb8, CS2SX_STRBUF_SIZE, "w=%d", w);
    Font_Draw(self->f_font, 1000, 680, _sb8, COLOR_GRAY, 16);
}

void FontTextApp_OnExit(FontTextApp* self)
{
    (self->f_font != NULL ? (void)(Font_Dispose(self->f_font), self->f_font = NULL) : (void)0);
}

void FontTextApp_Free(FontTextApp* self)
{
    if (!self) return;
    if (self->f_font) { Font_Dispose(self->f_font); self->f_font = NULL; }
    free(self);
}
