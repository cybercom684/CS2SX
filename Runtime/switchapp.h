#pragma once
#include <switch.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <limits.h>
#include <float.h>
#include <math.h>
#include "switchforms.h"

// ============================================================================
// Farb-Hilfsmakros (RGBA8888)
// ============================================================================

#define CS2SX_RGBA(r,g,b,a) (((u32)(a) << 24) | ((u32)(b) << 16) | ((u32)(g) << 8) | (u32)(r))
#define CS2SX_RGB(r,g,b)    CS2SX_RGBA(r,g,b,255)

#define COLOR_BLACK   CS2SX_RGB(0,   0,   0  )
#define COLOR_WHITE   CS2SX_RGB(255, 255, 255)
#define COLOR_RED     CS2SX_RGB(255, 0,   0  )
#define COLOR_GREEN   CS2SX_RGB(0,   200, 0  )
#define COLOR_BLUE    CS2SX_RGB(0,   0,   255)
#define COLOR_YELLOW  CS2SX_RGB(255, 255, 0  )
#define COLOR_CYAN    CS2SX_RGB(0,   255, 255)
// Fix 9: COLOR_MAGENTA war schon definiert, aber Color.Magenta fehlte im TypeRegistry-Mapping
#define COLOR_MAGENTA CS2SX_RGB(255, 0,   255)
#define COLOR_GRAY    CS2SX_RGB(128, 128, 128)
#define COLOR_DGRAY   CS2SX_RGB(64,  64,  64 )
#define COLOR_LGRAY   CS2SX_RGB(192, 192, 192)
#define COLOR_ORANGE  CS2SX_RGB(255, 165, 0  )

// Fix 9: Fehlende Farben
#define COLOR_PINK    CS2SX_RGB(255, 105, 180)
#define COLOR_PURPLE  CS2SX_RGB(128, 0,   128)
#define COLOR_BROWN   CS2SX_RGB(139, 69,  19 )
#define COLOR_TEAL    CS2SX_RGB(0,   128, 128)
#define COLOR_LIME    CS2SX_RGB(0,   255, 0  )
#define COLOR_NAVY    CS2SX_RGB(0,   0,   128)
#define COLOR_SILVER  CS2SX_RGB(192, 192, 192)
#define COLOR_MAROON  CS2SX_RGB(128, 0,   0  )
#define COLOR_OLIVE   CS2SX_RGB(128, 128, 0  )

// ============================================================================
// Fix 2: Math-Hilfsmakros
// ============================================================================

#ifndef MIN
#define MIN(a, b) ((a) < (b) ? (a) : (b))
#endif

#ifndef MAX
#define MAX(a, b) ((a) > (b) ? (a) : (b))
#endif

#ifndef CLAMP
#define CLAMP(v, lo, hi) ((v) < (lo) ? (lo) : ((v) > (hi) ? (hi) : (v)))
#endif

static inline int CS2SX_Sign(int x) { return (x > 0) - (x < 0); }

// ============================================================================
// Fix 3: Pseudo-Zufallszahlengenerator
// ============================================================================

extern unsigned int _cs2sx_rand_state;

static inline void CS2SX_Rand_Seed(unsigned int seed)
{
    _cs2sx_rand_state = seed ? seed : 12345u;
}

static inline int CS2SX_Rand_Next(int min_val, int max_val)
{
    if (max_val <= min_val) return min_val;
    _cs2sx_rand_state = _cs2sx_rand_state * 1664525u + 1013904223u;
    unsigned int r = (_cs2sx_rand_state >> 16) & 0x7FFFu;
    return min_val + (int)(r % (unsigned int)(max_val - min_val));
}

static inline int CS2SX_Rand_NextMax(int max_val)
{
    return CS2SX_Rand_Next(0, max_val);
}

static inline float CS2SX_Rand_Float(void)
{
    _cs2sx_rand_state = _cs2sx_rand_state * 1664525u + 1013904223u;
    return (float)(_cs2sx_rand_state & 0xFFFFu) / 65535.0f;
}

// ============================================================================
// Fix 15: Environment.Exit
// ============================================================================

static inline void Environment_Exit(int code)
{
    exit(code);
}

#define System_Exit(code) Environment_Exit(code)

// ============================================================================
// Fix 17: Color.WithAlpha
// ============================================================================

static inline u32 Color_WithAlpha(u32 color, u8 alpha)
{
    return (color & 0x00FFFFFFu) | ((u32)alpha << 24);
}

// ============================================================================
// SwitchApp
// ============================================================================

typedef struct SwitchApp SwitchApp;
struct SwitchApp
{
    Form form;
    u64  kDown;
    u64  kHeld;

    void (*OnInit) (SwitchApp* self);
    void (*OnFrame)(SwitchApp* self);
    void (*OnExit) (SwitchApp* self);
};

static inline void SwitchApp_Add(SwitchApp* self, Control* control)
{
    if (!self || !control) return;
    control->context = self;
    Form_Add(&self->form, control);
}

// ============================================================================
// Bitmap Font 8x8 (ASCII 32-127, CP437)
// ============================================================================

static const u8 cs2sx_font8x8[96][8] = {
    {0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00}, // ' '
    {0x18,0x3C,0x3C,0x18,0x18,0x00,0x18,0x00}, // '!'
    {0x36,0x36,0x00,0x00,0x00,0x00,0x00,0x00}, // '"'
    {0x36,0x36,0x7F,0x36,0x7F,0x36,0x36,0x00}, // '#'
    {0x0C,0x3E,0x03,0x1E,0x30,0x1F,0x0C,0x00}, // '$'
    {0x00,0x63,0x33,0x18,0x0C,0x66,0x63,0x00}, // '%'
    {0x1C,0x36,0x1C,0x6E,0x3B,0x33,0x6E,0x00}, // '&'
    {0x06,0x06,0x03,0x00,0x00,0x00,0x00,0x00}, // '''
    {0x18,0x0C,0x06,0x06,0x06,0x0C,0x18,0x00}, // '('
    {0x06,0x0C,0x18,0x18,0x18,0x0C,0x06,0x00}, // ')'
    {0x00,0x66,0x3C,0xFF,0x3C,0x66,0x00,0x00}, // '*'
    {0x00,0x0C,0x0C,0x3F,0x0C,0x0C,0x00,0x00}, // '+'
    {0x00,0x00,0x00,0x00,0x00,0x0C,0x0C,0x06}, // ','
    {0x00,0x00,0x00,0x3F,0x00,0x00,0x00,0x00}, // '-'
    {0x00,0x00,0x00,0x00,0x00,0x0C,0x0C,0x00}, // '.'
    {0x60,0x30,0x18,0x0C,0x06,0x03,0x01,0x00}, // '/'
    {0x3E,0x63,0x73,0x7B,0x6F,0x67,0x3E,0x00}, // '0'
    {0x0C,0x0E,0x0C,0x0C,0x0C,0x0C,0x3F,0x00}, // '1'
    {0x1E,0x33,0x30,0x1C,0x06,0x33,0x3F,0x00}, // '2'
    {0x1E,0x33,0x30,0x1C,0x30,0x33,0x1E,0x00}, // '3'
    {0x38,0x3C,0x36,0x33,0x7F,0x30,0x78,0x00}, // '4'
    {0x3F,0x03,0x1F,0x30,0x30,0x33,0x1E,0x00}, // '5'
    {0x1C,0x06,0x03,0x1F,0x33,0x33,0x1E,0x00}, // '6'
    {0x3F,0x33,0x30,0x18,0x0C,0x0C,0x0C,0x00}, // '7'
    {0x1E,0x33,0x33,0x1E,0x33,0x33,0x1E,0x00}, // '8'
    {0x1E,0x33,0x33,0x3E,0x30,0x18,0x0E,0x00}, // '9'
    {0x00,0x0C,0x0C,0x00,0x00,0x0C,0x0C,0x00}, // ':'
    {0x00,0x0C,0x0C,0x00,0x00,0x0C,0x0C,0x06}, // ';'
    {0x18,0x0C,0x06,0x03,0x06,0x0C,0x18,0x00}, // '<'
    {0x00,0x00,0x3F,0x00,0x00,0x3F,0x00,0x00}, // '='
    {0x06,0x0C,0x18,0x30,0x18,0x0C,0x06,0x00}, // '>'
    {0x1E,0x33,0x30,0x18,0x0C,0x00,0x0C,0x00}, // '?'
    {0x3E,0x63,0x7B,0x7B,0x7B,0x03,0x1E,0x00}, // '@'
    {0x0C,0x1E,0x33,0x33,0x3F,0x33,0x33,0x00}, // 'A'
    {0x3F,0x66,0x66,0x3E,0x66,0x66,0x3F,0x00}, // 'B'
    {0x3C,0x66,0x03,0x03,0x03,0x66,0x3C,0x00}, // 'C'
    {0x1F,0x36,0x66,0x66,0x66,0x36,0x1F,0x00}, // 'D'
    {0x7F,0x46,0x16,0x1E,0x16,0x46,0x7F,0x00}, // 'E'
    {0x7F,0x46,0x16,0x1E,0x16,0x06,0x0F,0x00}, // 'F'
    {0x3C,0x66,0x03,0x03,0x73,0x66,0x7C,0x00}, // 'G'
    {0x33,0x33,0x33,0x3F,0x33,0x33,0x33,0x00}, // 'H'
    {0x1E,0x0C,0x0C,0x0C,0x0C,0x0C,0x1E,0x00}, // 'I'
    {0x78,0x30,0x30,0x30,0x33,0x33,0x1E,0x00}, // 'J'
    {0x67,0x66,0x36,0x1E,0x36,0x66,0x67,0x00}, // 'K'
    {0x0F,0x06,0x06,0x06,0x46,0x66,0x7F,0x00}, // 'L'
    {0x63,0x77,0x7F,0x7F,0x6B,0x63,0x63,0x00}, // 'M'
    {0x63,0x67,0x6F,0x7B,0x73,0x63,0x63,0x00}, // 'N'
    {0x1C,0x36,0x63,0x63,0x63,0x36,0x1C,0x00}, // 'O'
    {0x3F,0x66,0x66,0x3E,0x06,0x06,0x0F,0x00}, // 'P'
    {0x1E,0x33,0x33,0x33,0x3B,0x1E,0x38,0x00}, // 'Q'
    {0x3F,0x66,0x66,0x3E,0x36,0x66,0x67,0x00}, // 'R'
    {0x1E,0x33,0x07,0x0E,0x38,0x33,0x1E,0x00}, // 'S'
    {0x3F,0x2D,0x0C,0x0C,0x0C,0x0C,0x1E,0x00}, // 'T'
    {0x33,0x33,0x33,0x33,0x33,0x33,0x3F,0x00}, // 'U'
    {0x33,0x33,0x33,0x33,0x33,0x1E,0x0C,0x00}, // 'V'
    {0x63,0x63,0x63,0x6B,0x7F,0x77,0x63,0x00}, // 'W'
    {0x63,0x63,0x36,0x1C,0x1C,0x36,0x63,0x00}, // 'X'
    {0x33,0x33,0x33,0x1E,0x0C,0x0C,0x1E,0x00}, // 'Y'
    {0x7F,0x63,0x31,0x18,0x4C,0x66,0x7F,0x00}, // 'Z'
    {0x1E,0x06,0x06,0x06,0x06,0x06,0x1E,0x00}, // '['
    {0x03,0x06,0x0C,0x18,0x30,0x60,0x40,0x00}, // '\'
    {0x1E,0x18,0x18,0x18,0x18,0x18,0x1E,0x00}, // ']'
    {0x08,0x1C,0x36,0x63,0x00,0x00,0x00,0x00}, // '^'
    {0x00,0x00,0x00,0x00,0x00,0x00,0x00,0xFF}, // '_'
    {0x0C,0x0C,0x18,0x00,0x00,0x00,0x00,0x00}, // '`'
    {0x00,0x00,0x1E,0x30,0x3E,0x33,0x6E,0x00}, // 'a'
    {0x07,0x06,0x06,0x3E,0x66,0x66,0x3B,0x00}, // 'b'
    {0x00,0x00,0x1E,0x33,0x03,0x33,0x1E,0x00}, // 'c'
    {0x38,0x30,0x30,0x3E,0x33,0x33,0x6E,0x00}, // 'd'
    {0x00,0x00,0x1E,0x33,0x3F,0x03,0x1E,0x00}, // 'e'
    {0x1C,0x36,0x06,0x0F,0x06,0x06,0x0F,0x00}, // 'f'
    {0x00,0x00,0x6E,0x33,0x33,0x3E,0x30,0x1F}, // 'g'
    {0x07,0x06,0x36,0x6E,0x66,0x66,0x67,0x00}, // 'h'
    {0x0C,0x00,0x0E,0x0C,0x0C,0x0C,0x1E,0x00}, // 'i'
    {0x30,0x00,0x30,0x30,0x30,0x33,0x33,0x1E}, // 'j'
    {0x07,0x06,0x66,0x36,0x1E,0x36,0x67,0x00}, // 'k'
    {0x0E,0x0C,0x0C,0x0C,0x0C,0x0C,0x1E,0x00}, // 'l'
    {0x00,0x00,0x33,0x7F,0x7F,0x6B,0x63,0x00}, // 'm'
    {0x00,0x00,0x1F,0x33,0x33,0x33,0x33,0x00}, // 'n'
    {0x00,0x00,0x1E,0x33,0x33,0x33,0x1E,0x00}, // 'o'
    {0x00,0x00,0x3B,0x66,0x66,0x3E,0x06,0x0F}, // 'p'
    {0x00,0x00,0x6E,0x33,0x33,0x3E,0x30,0x78}, // 'q'
    {0x00,0x00,0x3B,0x6E,0x66,0x06,0x0F,0x00}, // 'r'
    {0x00,0x00,0x3E,0x03,0x1E,0x30,0x1F,0x00}, // 's'
    {0x08,0x0C,0x3E,0x0C,0x0C,0x2C,0x18,0x00}, // 't'
    {0x00,0x00,0x33,0x33,0x33,0x33,0x6E,0x00}, // 'u'
    {0x00,0x00,0x33,0x33,0x33,0x1E,0x0C,0x00}, // 'v'
    {0x00,0x00,0x63,0x6B,0x7F,0x7F,0x36,0x00}, // 'w'
    {0x00,0x00,0x63,0x36,0x1C,0x36,0x63,0x00}, // 'x'
    {0x00,0x00,0x33,0x33,0x33,0x3E,0x30,0x1F}, // 'y'
    {0x00,0x00,0x3F,0x19,0x0C,0x26,0x3F,0x00}, // 'z'
    {0x38,0x0C,0x0C,0x07,0x0C,0x0C,0x38,0x00}, // '{'
    {0x18,0x18,0x18,0x00,0x18,0x18,0x18,0x00}, // '|'
    {0x07,0x0C,0x0C,0x38,0x0C,0x0C,0x07,0x00}, // '}'
    {0x6E,0x3B,0x00,0x00,0x00,0x00,0x00,0x00}, // '~'
    {0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF}, // DEL
};

// ============================================================================
// Texture
// ============================================================================

typedef struct Texture Texture;
struct Texture {
    int  width;
    int  height;
    u32* pixels;
};

static inline Texture* Texture_New(int width, int height, u32* pixels)
{
    Texture* t = (Texture*)malloc(sizeof(Texture));
    if (!t) return NULL;
    t->width = width;
    t->height = height;
    t->pixels = (u32*)malloc(width * height * sizeof(u32));
    if (!t->pixels) { free(t); return NULL; }
    if (pixels) memcpy(t->pixels, pixels, width * height * sizeof(u32));
    else        memset(t->pixels, 0, width * height * sizeof(u32));
    return t;
}

static inline void Texture_Dispose(Texture* t)
{
    if (!t) return;
    free(t->pixels);
    free(t);
}

// ============================================================================
// Framebuffer State
// ============================================================================

extern Framebuffer g_fb;
extern u32* g_fb_addr;
extern int         g_fb_width;
extern int         g_fb_height;
extern int         g_gfx_init;
extern PadState    g_cs2sx_pad;

// ============================================================================
// Graphics primitives
// ============================================================================

static inline void Graphics_Init(int width, int height)
{
    if (g_gfx_init) return;
    g_fb_width = width;
    g_fb_height = height;
    g_gfx_init = 1;
}

static inline void Graphics_SetPixel(int x, int y, u32 color)
{
    if (!g_fb_addr) return;
    if (x < 0 || x >= g_fb_width || y < 0 || y >= g_fb_height) return;
    g_fb_addr[y * g_fb_width + x] = color;
}

static inline void Graphics_FillScreen(u32 color)
{
    if (!g_fb_addr) return;
    int total = g_fb_width * g_fb_height;
    for (int i = 0; i < total; i++)
        g_fb_addr[i] = color;
}

static inline void Graphics_DrawRect(int x, int y, int w, int h, u32 color)
{
    if (!g_fb_addr) return;
    for (int i = x; i < x + w; i++)
    {
        Graphics_SetPixel(i, y, color);
        Graphics_SetPixel(i, y + h - 1, color);
    }
    for (int i = y; i < y + h; i++)
    {
        Graphics_SetPixel(x, i, color);
        Graphics_SetPixel(x + w - 1, i, color);
    }
}

static inline void Graphics_FillRect(int x, int y, int w, int h, u32 color)
{
    if (!g_fb_addr) return;
    for (int row = y; row < y + h; row++)
        for (int col = x; col < x + w; col++)
            Graphics_SetPixel(col, row, color);
}

static inline void Graphics_DrawLine(int x0, int y0, int x1, int y1, u32 color)
{
    if (!g_fb_addr) return;
    int dx = abs(x1 - x0);
    int dy = -abs(y1 - y0);
    int sx = x0 < x1 ? 1 : -1;
    int sy = y0 < y1 ? 1 : -1;
    int err = dx + dy;
    while (1)
    {
        Graphics_SetPixel(x0, y0, color);
        if (x0 == x1 && y0 == y1) break;
        int e2 = 2 * err;
        if (e2 >= dy) { err += dy; x0 += sx; }
        if (e2 <= dx) { err += dx; y0 += sy; }
    }
}

static inline void Graphics_DrawCircle(int cx, int cy, int r, u32 color)
{
    if (!g_fb_addr) return;
    int x = 0, y = r, d = 3 - 2 * r;
    while (x <= y)
    {
        Graphics_SetPixel(cx + x, cy + y, color);
        Graphics_SetPixel(cx - x, cy + y, color);
        Graphics_SetPixel(cx + x, cy - y, color);
        Graphics_SetPixel(cx - x, cy - y, color);
        Graphics_SetPixel(cx + y, cy + x, color);
        Graphics_SetPixel(cx - y, cy + x, color);
        Graphics_SetPixel(cx + y, cy - x, color);
        Graphics_SetPixel(cx - y, cy - x, color);
        if (d < 0) d += 4 * x + 6;
        else { d += 4 * (x - y) + 10; y--; }
        x++;
    }
}

static inline void Graphics_FillCircle(int cx, int cy, int r, u32 color)
{
    if (!g_fb_addr) return;
    for (int dy = -r; dy <= r; dy++)
        for (int dx = -r; dx <= r; dx++)
            if (dx * dx + dy * dy <= r * r)
                Graphics_SetPixel(cx + dx, cy + dy, color);
}

static inline void Graphics_DrawChar(int x, int y, char c, u32 color, int scale)
{
    if (!g_fb_addr) return;
    if (c < 32 || c > 127) c = '?';
    const u8* glyph = cs2sx_font8x8[(int)(c - 32)];
    for (int row = 0; row < 8; row++)
        for (int col = 0; col < 8; col++)
            if (glyph[row] & (1 << (7 - col)))
                Graphics_FillRect(x + (7 - col) * scale, y + row * scale, scale, scale, color);
}

static inline void Graphics_DrawText(int x, int y, const char* text, u32 color, int scale)
{
    if (!g_fb_addr || !text) return;
    int ox = x;
    for (int i = 0; text[i] != '\0'; i++)
    {
        if (text[i] == '\n') { y += 8 * scale + 2; x = ox; continue; }
        Graphics_DrawChar(x, y, text[i], color, scale);
        x += 8 * scale + 1;
    }
}

static inline void Graphics_DrawTexture(Texture* tex, int x, int y)
{
    if (!tex || !tex->pixels || !g_fb_addr) return;
    for (int row = 0; row < tex->height; row++)
    {
        int py = y + row;
        if (py < 0 || py >= g_fb_height) continue;
        for (int col = 0; col < tex->width; col++)
        {
            int px = x + col;
            if (px < 0 || px >= g_fb_width) continue;
            u32 c = tex->pixels[row * tex->width + col];
            if ((c >> 24) > 0)
                g_fb_addr[py * g_fb_width + px] = c;
        }
    }
}

static inline int Graphics_MeasureTextWidth(const char* text, int scale)
{
    if (!text) return 0;
    int len = 0;
    for (int i = 0; text[i] != '\0'; i++) len++;
    return len * (8 * scale + 1);
}

static inline int Graphics_MeasureTextHeight(int scale)
{
    return 8 * scale;
}

// ============================================================================
// Extension Graphics (merged from switchapp_ext.h)
// ============================================================================

static inline void Graphics_DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, u32 color)
{
    Graphics_DrawLine(x0, y0, x1, y1, color);
    Graphics_DrawLine(x1, y1, x2, y2, color);
    Graphics_DrawLine(x2, y2, x0, y0, color);
}

static inline void Graphics_FillTriangle(int x0, int y0, int x1, int y1, int x2, int y2, u32 color)
{
    if (!g_fb_addr) return;
    if (y0 > y1) { int t;t = x0;x0 = x1;x1 = t;t = y0;y0 = y1;y1 = t; }
    if (y1 > y2) { int t;t = x1;x1 = x2;x2 = t;t = y1;y1 = y2;y2 = t; }
    if (y0 > y1) { int t;t = x0;x0 = x1;x1 = t;t = y0;y0 = y1;y1 = t; }
    int total_height = y2 - y0;
    if (total_height == 0) return;
    for (int y = y0;y <= y2;y++)
    {
        int seg_height, xa, xb;
        if (y < y1) { seg_height = y1 - y0;if (seg_height == 0)continue;xa = x0 + (x2 - x0) * (y - y0) / total_height;xb = x0 + (x1 - x0) * (y - y0) / seg_height; }
        else { seg_height = y2 - y1;if (seg_height == 0)continue;xa = x0 + (x2 - x0) * (y - y0) / total_height;xb = x1 + (x2 - x1) * (y - y1) / seg_height; }
        if (xa > xb) { int t = xa;xa = xb;xb = t; }
        for (int x = xa;x <= xb;x++) Graphics_SetPixel(x, y, color);
    }
}

static inline void Graphics_DrawEllipse(int cx, int cy, int rx, int ry, u32 color)
{
    if (!g_fb_addr || rx <= 0 || ry <= 0) return;
    int x = 0, y = ry;
    long rx2 = (long)rx * rx, ry2 = (long)ry * ry, d = ry2 - rx2 * ry + rx2 / 4;
    while (2 * ry2 * x < 2 * rx2 * y) {
        Graphics_SetPixel(cx + x, cy + y, color);Graphics_SetPixel(cx - x, cy + y, color);
        Graphics_SetPixel(cx + x, cy - y, color);Graphics_SetPixel(cx - x, cy - y, color);
        x++;if (d < 0)d += ry2 * (2 * x + 1);else { y--;d += ry2 * (2 * x + 1) - rx2 * (2 * y); }
    }
    d = (long)ry2 * (x * x + x) + rx2 * ((y - 1) * (y - 1) - (long)ry * ry) + (rx2 - ry2);
    while (y >= 0) {
        Graphics_SetPixel(cx + x, cy + y, color);Graphics_SetPixel(cx - x, cy + y, color);
        Graphics_SetPixel(cx + x, cy - y, color);Graphics_SetPixel(cx - x, cy - y, color);
        y--;if (d > 0)d += rx2 * (1 - 2 * y);else { x++;d += ry2 * (2 * x + 1) - rx2 * (2 * y - 1); }
    }
}

static inline void Graphics_FillEllipse(int cx, int cy, int rx, int ry, u32 color)
{
    if (!g_fb_addr || rx <= 0 || ry <= 0) return;
    long rx2 = (long)rx * rx, ry2 = (long)ry * ry;
    for (int dy = -ry;dy <= ry;dy++) {
        long dx2 = rx2 * (ry2 - (long)dy * dy) / ry2;
        if (dx2 < 0)dx2 = 0;
        int dx = rx;while ((long)dx * dx > dx2)dx--;
        for (int x = cx - dx;x <= cx + dx;x++) Graphics_SetPixel(x, cy + dy, color);
    }
}

static inline void Graphics_DrawRoundedRect(int x, int y, int w, int h, int r, u32 color)
{
    if (!g_fb_addr) return;
    if (r < 0)r = 0;if (r > w / 2)r = w / 2;if (r > h / 2)r = h / 2;
    Graphics_DrawLine(x + r, y, x + w - r, y, color);
    Graphics_DrawLine(x + r, y + h - 1, x + w - r, y + h - 1, color);
    Graphics_DrawLine(x, y + r, x, y + h - r, color);
    Graphics_DrawLine(x + w - 1, y + r, x + w - 1, y + h - r, color);
    int px, py, d;
    px = 0;py = r;d = 3 - 2 * r;while (px <= py) { Graphics_SetPixel(x + r - px, y + r - py, color);Graphics_SetPixel(x + r - py, y + r - px, color);if (d < 0)d += 4 * px + 6;else { d += 4 * (px - py) + 10;py--; }px++; }
    px = 0;py = r;d = 3 - 2 * r;while (px <= py) { Graphics_SetPixel(x + w - 1 - r + px, y + r - py, color);Graphics_SetPixel(x + w - 1 - r + py, y + r - px, color);if (d < 0)d += 4 * px + 6;else { d += 4 * (px - py) + 10;py--; }px++; }
    px = 0;py = r;d = 3 - 2 * r;while (px <= py) { Graphics_SetPixel(x + r - px, y + h - 1 - r + py, color);Graphics_SetPixel(x + r - py, y + h - 1 - r + px, color);if (d < 0)d += 4 * px + 6;else { d += 4 * (px - py) + 10;py--; }px++; }
    px = 0;py = r;d = 3 - 2 * r;while (px <= py) { Graphics_SetPixel(x + w - 1 - r + px, y + h - 1 - r + py, color);Graphics_SetPixel(x + w - 1 - r + py, y + h - 1 - r + px, color);if (d < 0)d += 4 * px + 6;else { d += 4 * (px - py) + 10;py--; }px++; }
}

static inline void Graphics_FillRoundedRect(int x, int y, int w, int h, int r, u32 color)
{
    if (!g_fb_addr) return;
    if (r < 0)r = 0;if (r > w / 2)r = w / 2;if (r > h / 2)r = h / 2;
    Graphics_FillRect(x, y + r, w, h - 2 * r, color);
    Graphics_FillRect(x + r, y, w - 2 * r, r, color);
    Graphics_FillRect(x + r, y + h - r, w - 2 * r, r, color);
    Graphics_FillCircle(x + r, y + r, r, color);
    Graphics_FillCircle(x + w - 1 - r, y + r, r, color);
    Graphics_FillCircle(x + r, y + h - 1 - r, r, color);
    Graphics_FillCircle(x + w - 1 - r, y + h - 1 - r, r, color);
}

static inline void Graphics_SetPixelAlpha(int x, int y, u32 color, u8 alpha)
{
    if (!g_fb_addr) return;
    if (x < 0 || x >= g_fb_width || y < 0 || y >= g_fb_height) return;
    u32* dst = &g_fb_addr[y * g_fb_width + x];
    u32 bg = *dst;
    u32 sr = (color >> 0) & 0xFF, sg = (color >> 8) & 0xFF, sb = (color >> 16) & 0xFF;
    u32 dr = (bg >> 0) & 0xFF, dg = (bg >> 8) & 0xFF, db = (bg >> 16) & 0xFF;
    u32 a = alpha, ia = 255 - a;
    *dst = 0xFF000000 | ((sb * a + db * ia) / 255 << 16) | ((sg * a + dg * ia) / 255 << 8) | ((sr * a + dr * ia) / 255);
}

static inline void Graphics_FillRectAlpha(int x, int y, int w, int h, u32 color, u8 alpha)
{
    if (!g_fb_addr || alpha == 0) return;
    if (alpha == 255) { Graphics_FillRect(x, y, w, h, color);return; }
    for (int row = y;row < y + h;row++) for (int col = x;col < x + w;col++) Graphics_SetPixelAlpha(col, row, color, alpha);
}

static inline void Graphics_DrawTextAlpha(int x, int y, const char* text, u32 color, int scale, u8 alpha)
{
    if (!g_fb_addr || !text || alpha == 0) return;
    if (alpha == 255) { Graphics_DrawText(x, y, text, color, scale);return; }
    int ox = x;
    for (int i = 0;text[i] != '\0';i++) {
        if (text[i] == '\n') { y += 8 * scale + 2;x = ox;continue; }
        char c = text[i];if (c < 32 || c>127)c = '?';
        const u8* glyph = cs2sx_font8x8[(int)(c - 32)];
        for (int row = 0;row < 8;row++) for (int col = 0;col < 8;col++)
            if (glyph[row] & (1 << (7 - col)))
                for (int sy = 0;sy < scale;sy++) for (int sx2 = 0;sx2 < scale;sx2++)
                    Graphics_SetPixelAlpha(x + (7 - col) * scale + sx2, y + row * scale + sy, color, alpha);
        x += 8 * scale + 1;
    }
}

static inline void Graphics_DrawTextShadow(int x, int y, const char* text, u32 color, u32 shadow, int scale)
{
    Graphics_DrawText(x + scale, y + scale, text, shadow, scale);
    Graphics_DrawText(x, y, text, color, scale);
}

static inline void Graphics_DrawGrid(int x, int y, int w, int h, int cellW, int cellH, u32 color)
{
    if (!g_fb_addr) return;
    for (int gx = x;gx <= x + w;gx += cellW) Graphics_DrawLine(gx, y, gx, y + h, color);
    for (int gy = y;gy <= y + h;gy += cellH) Graphics_DrawLine(x, gy, x + w, gy, color);
}

// ============================================================================
// Extension Input
// ============================================================================

#define CS2SX_STICK_DEADZONE 3000

typedef struct { int x; int y; } CS2SX_StickPos;
typedef struct { int count; int x[10]; int y[10]; u32 id[10]; } CS2SX_TouchState;

static inline CS2SX_StickPos CS2SX_Input_GetStickLeft(PadState* pad)
{
    HidAnalogStickState raw = padGetStickPos(pad, 0);
    CS2SX_StickPos pos;
    pos.x = (raw.x > -CS2SX_STICK_DEADZONE && raw.x < CS2SX_STICK_DEADZONE) ? 0 : raw.x;
    pos.y = (raw.y > -CS2SX_STICK_DEADZONE && raw.y < CS2SX_STICK_DEADZONE) ? 0 : raw.y;
    return pos;
}

static inline CS2SX_StickPos CS2SX_Input_GetStickRight(PadState* pad)
{
    HidAnalogStickState raw = padGetStickPos(pad, 1);
    CS2SX_StickPos pos;
    pos.x = (raw.x > -CS2SX_STICK_DEADZONE && raw.x < CS2SX_STICK_DEADZONE) ? 0 : raw.x;
    pos.y = (raw.y > -CS2SX_STICK_DEADZONE && raw.y < CS2SX_STICK_DEADZONE) ? 0 : raw.y;
    return pos;
}

static inline CS2SX_StickPos _cs2sx_get_stick_left(void) { return CS2SX_Input_GetStickLeft(&g_cs2sx_pad); }
static inline CS2SX_StickPos _cs2sx_get_stick_right(void) { return CS2SX_Input_GetStickRight(&g_cs2sx_pad); }

static inline int CS2SX_StickNorm(int raw)
{
    if (raw < 0)raw = -raw;
    if (raw < CS2SX_STICK_DEADZONE)return 0;
    int v = ((raw - CS2SX_STICK_DEADZONE) * 100) / (32767 - CS2SX_STICK_DEADZONE);
    return v > 100 ? 100 : v;
}

static inline CS2SX_TouchState CS2SX_Input_GetTouch(void)
{
    CS2SX_TouchState state;state.count = 0;
    HidTouchScreenState raw = { 0 };
    if (hidGetTouchScreenStates(&raw, 1) == 0) return state;
    int count = raw.count;if (count > 10)count = 10;state.count = count;
    for (int i = 0;i < count;i++) { state.x[i] = (int)raw.touches[i].x;state.y[i] = (int)raw.touches[i].y;state.id[i] = raw.touches[i].finger_id; }
    return state;
}

static inline int CS2SX_Touch_HitRect(CS2SX_TouchState* ts, int idx, int rx, int ry, int rw, int rh)
{
    if (!ts || idx < 0 || idx >= ts->count) return 0;
    return ts->x[idx] >= rx && ts->x[idx] < rx + rw && ts->y[idx] >= ry && ts->y[idx] < ry + rh;
}

// ============================================================================
// Extension Filesystem (unverändert aus Original)
// ============================================================================

static inline List_str* CS2SX_Dir_GetDirectories(const char* path)
{
    List_str* result = List_str_New();if (!result)return result;
    FsFileSystem fs;if (R_FAILED(fsOpenSdCardFileSystem(&fs)))return result;
    FsDir d;if (R_FAILED(fsFsOpenDirectory(&fs, path, FsDirOpenMode_ReadDirs, &d))) { fsFsClose(&fs);return result; }
    static FsDirectoryEntry _subdir_entries[64];static char _subdir_paths[64][512];
    s64 count = 0;fsDirRead(&d, &count, 64, _subdir_entries);
    for (int i = 0;i < (int)count && i < 64;i++) { snprintf(_subdir_paths[i], sizeof(_subdir_paths[i]), "%s/%s", path, _subdir_entries[i].name);List_str_Add(result, _subdir_paths[i]); }
    fsDirClose(&d);fsFsClose(&fs);return result;
}

static inline List_str* CS2SX_Dir_GetEntries(const char* path)
{
    List_str* result = List_str_New();if (!result)return result;
    FsFileSystem fs;if (R_FAILED(fsOpenSdCardFileSystem(&fs)))return result;
    FsDir d;int mode = FsDirOpenMode_ReadFiles | FsDirOpenMode_ReadDirs;
    if (R_FAILED(fsFsOpenDirectory(&fs, path, mode, &d))) { fsFsClose(&fs);return result; }
    static FsDirectoryEntry _all_entries[128];static char _all_paths[128][512];
    s64 count = 0;fsDirRead(&d, &count, 128, _all_entries);
    for (int i = 0;i < (int)count && i < 128;i++) { snprintf(_all_paths[i], sizeof(_all_paths[i]), "%s/%s", path, _all_entries[i].name);List_str_Add(result, _all_paths[i]); }
    fsDirClose(&d);fsFsClose(&fs);return result;
}

static inline const char* CS2SX_Path_GetFileName(const char* path)
{
    if (!path)return"";const char* last = path;
    for (const char* p = path;*p;p++) if (*p == '/')last = p + 1;
    return last;
}

static inline const char* CS2SX_Path_GetExtension(const char* path)
{
    const char* name = CS2SX_Path_GetFileName(path);const char* dot = NULL;
    for (const char* p = name;*p;p++) if (*p == '.')dot = p;
    return dot ? dot : "";
}

static inline const char* CS2SX_Path_GetDirectoryName(const char* path)
{
    static char _dirname_buf[512];if (!path)return"";
    int len = (int)strlen(path), slash = -1;
    for (int i = len - 1;i >= 0;i--) if (path[i] == '/') { slash = i;break; }
    if (slash <= 0)return"/";int copyLen = slash;if (copyLen >= 512)copyLen = 511;
    memcpy(_dirname_buf, path, copyLen);_dirname_buf[copyLen] = '\0';return _dirname_buf;
}

static inline int CS2SX_Path_IsDirectory(const char* path)
{
    return CS2SX_Path_GetExtension(path)[0] == '\0';
}

// ============================================================================
// Extension System — Battery
// ============================================================================

typedef struct { int percent; bool charging; bool connected; } CS2SX_BatteryInfo;

static inline CS2SX_BatteryInfo CS2SX_GetBattery(void)
{
    CS2SX_BatteryInfo info = { 0,false,false };
    u32 chargePercent = 0;
    if (R_SUCCEEDED(psmGetBatteryChargePercentage(&chargePercent))) info.percent = (int)chargePercent;
    PsmChargerType chargerType = PsmChargerType_Unconnected;
    if (R_SUCCEEDED(psmGetChargerType(&chargerType))) {
        info.connected = chargerType != PsmChargerType_Unconnected;
        info.charging = info.connected && chargePercent < 100;
    }
    return info;
}

// ============================================================================
// SwitchApp_Run
// ============================================================================

static inline void SwitchApp_Run(SwitchApp* self)
{
    if (!self) return;

    PadState pad;
    padConfigureInput(1, HidNpadStyleSet_NpadStandard);
    padInitializeDefault(&pad);

    NWindow* win = nwindowGetDefault();
    framebufferCreate(&g_fb, win,
        (u32)g_fb_width, (u32)g_fb_height,
        PIXEL_FORMAT_RGBA_8888, 2);
    framebufferMakeLinear(&g_fb);

    if (self->OnInit)
        self->OnInit(self);

    int use_gfx = g_gfx_init;

    if (!use_gfx)
    {
        framebufferClose(&g_fb);
        consoleInit(NULL);
        Form_InitFocus(&self->form);
    }

    while (appletMainLoop())
    {
        padUpdate(&pad);
        g_cs2sx_pad = pad;
        self->kDown = padGetButtonsDown(&pad);
        self->kHeld = padGetButtons(&pad);

        if (use_gfx)
        {
            u8* fb_raw = framebufferBegin(&g_fb, NULL);
            if (!fb_raw) continue;

            g_fb_addr = (u32*)fb_raw;

            int total = g_fb_width * g_fb_height;
            for (int i = 0; i < total; i++)
                g_fb_addr[i] = COLOR_BLACK;

            Form_UpdateAll(&self->form, self->kDown, self->kHeld);

            if (self->OnFrame)
                self->OnFrame(self);

            Form_DrawAll(&self->form);

            framebufferEnd(&g_fb);
            g_fb_addr = NULL;
        }
        else
        {
            consoleClear();
            printf("\033[H\033[2J");

            Form_UpdateAll(&self->form, self->kDown, self->kHeld);

            if (self->OnFrame)
                self->OnFrame(self);

            Form_DrawAll(&self->form);
            consoleUpdate(NULL);
        }

        if (self->kDown & HidNpadButton_Plus)
            break;
    }

    if (self->OnExit)
        self->OnExit(self);

    Form_Free(&self->form);

    if (use_gfx)
        framebufferClose(&g_fb);
    else
        consoleExit(NULL);
}