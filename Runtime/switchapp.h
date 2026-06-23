#pragma once
#include <switch.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <limits.h>
#include <float.h>
#include <math.h>
#include "switchforms.h"

// Forward-declare the global pad state so stub headers can call padGetStyleSet.
// The actual definition lives in switchforms.c (emitted alongside SwitchApp_Run).
extern PadState g_cs2sx_pad;

#include "AudioStub.h"
#include "VibrationStub.h"
#include "MotionStub.h"
#include "SwkbdStub.h"
#include "SaveDataStub.h"
#include "HttpStub.h"

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
// String construction helpers (new string(char, count) / new string(char[], start, count))
// ============================================================================

static inline const char* CS2SX_RepeatChar(char c, int count)
{
    char* buf = _cs2sx_next_buf();
    if (count <= 0) { buf[0] = '\0'; return buf; }
    int n = count < CS2SX_STRBUF_SIZE - 1 ? count : CS2SX_STRBUF_SIZE - 1;
    memset(buf, (unsigned char)c, (size_t)n);
    buf[n] = '\0';
    return buf;
}

static inline const char* CS2SX_SubstrFromChars(const char* arr, int start, int count)
{
    char* buf = _cs2sx_next_buf();
    if (!arr || count <= 0) { buf[0] = '\0'; return buf; }
    int n = count < CS2SX_STRBUF_SIZE - 1 ? count : CS2SX_STRBUF_SIZE - 1;
    memcpy(buf, arr + start, (size_t)n);
    buf[n] = '\0';
    return buf;
}

// ============================================================================
// NULL-safe string comparison (CS2SX_strcmp_safe)
// ============================================================================

static inline int CS2SX_strcmp_safe(const char* a, const char* b) {
    if (!a && !b) return 0;
    if (!a) return -1;
    if (!b) return 1;
    return strcmp(a, b);
}

static inline int String_CompareIgnoreCase(const char* a, const char* b) {
    if (!a && !b) return 0;
    if (!a) return -1;
    if (!b) return 1;
    return strcasecmp(a, b);
}

// ============================================================================
// String_IsNullOrWhiteSpace — NULL + leer + nur Whitespace
// ============================================================================

static inline int String_IsNullOrWhiteSpace(const char* s) {
    if (!s) return 1;
    while (*s) {
        if ((unsigned char)*s > 0x20) return 0;
        s++;
    }
    return 1;
}

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

static inline long long CS2SX_Rand_NextInt64(void)
{
    // Combine four 15-bit LCG outputs for a ~60-bit random value
    _cs2sx_rand_state = _cs2sx_rand_state * 1664525u + 1013904223u;
    long long a = (long long)((_cs2sx_rand_state >> 16) & 0x7FFF);
    _cs2sx_rand_state = _cs2sx_rand_state * 1664525u + 1013904223u;
    long long b = (long long)((_cs2sx_rand_state >> 16) & 0x7FFF);
    _cs2sx_rand_state = _cs2sx_rand_state * 1664525u + 1013904223u;
    long long c = (long long)((_cs2sx_rand_state >> 16) & 0x7FFF);
    _cs2sx_rand_state = _cs2sx_rand_state * 1664525u + 1013904223u;
    long long d = (long long)((_cs2sx_rand_state >> 16) & 0x7FFF);
    return (a << 45) | (b << 30) | (c << 15) | d;
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
    int  f_Width;   // matches transpiler-generated field names (auto-property → f_Name)
    int  f_Height;
    u32* f_Pixels;
};

// Max texture dimension — guards against integer-overflow allocations from
// untrusted/corrupt image headers (width*height*4 must stay well within size_t).
#define CS2SX_TEX_MAX_DIM 16384

static inline Texture* Texture_New(int width, int height, u32* pixels)
{
    if (width <= 0 || height <= 0
        || width > CS2SX_TEX_MAX_DIM || height > CS2SX_TEX_MAX_DIM)
        return NULL;
    Texture* t = (Texture*)malloc(sizeof(Texture));
    if (!t) return NULL;
    t->f_Width  = width;
    t->f_Height = height;
    size_t bytes = (size_t)width * (size_t)height * sizeof(u32);
    t->f_Pixels = (u32*)malloc(bytes);
    if (!t->f_Pixels) { free(t); return NULL; }
    if (pixels) memcpy(t->f_Pixels, pixels, bytes);
    else        memset(t->f_Pixels, 0, bytes);
    return t;
}

static inline void Texture_Dispose(Texture* t)
{
    if (!t) return;
    free(t->f_Pixels);
    free(t);
}

// Decodes PNG/JPEG/BMP/GIF/TGA into an RGBA buffer. Provided by a project that
// links the stb_image extern-lib (externLibs/stb/stb_image_build.c). Declared
// here so Graphics_LoadImage compiles; the symbol is only required when an app
// actually calls it (the static-inline shim below is otherwise discarded).
extern unsigned int* CS2SX_Image_DecodeRGBA(const char* path, int* w, int* h);

// Loads an image file (PNG/JPEG/BMP/GIF/TGA) into a Texture. Returns NULL on
// failure. Caller owns the Texture and must call Texture_Dispose() / Dispose().
static inline Texture* Graphics_LoadImage(const char* path)
{
    int w = 0, h = 0;
    unsigned int* px = CS2SX_Image_DecodeRGBA(path, &w, &h);
    if (!px) return NULL;
    Texture* t = Texture_New(w, h, (u32*)px);   // Texture_New copies the pixels
    free(px);
    return t;
}

// Decodes + box-downscales an image to a small thumbnail Texture (fits maxSize).
extern unsigned int* CS2SX_Image_DecodeThumb(const char* path, int maxW, int maxH, int* ow, int* oh);

static inline Texture* Graphics_LoadImageThumb(const char* path, int maxSize)
{
    int w = 0, h = 0;
    unsigned int* px = CS2SX_Image_DecodeThumb(path, maxSize, maxSize, &w, &h);
    if (!px) return NULL;
    Texture* t = Texture_New(w, h, (u32*)px);
    free(px);
    return t;
}

// Load a 24-bit or 32-bit BMP from the filesystem (use "romfs:/file.bmp" for embedded assets).
// Returns NULL on failure. Caller owns the returned Texture and must call Texture_Dispose().
static inline Texture* CS2SX_Texture_LoadBMP(const char* path)
{
    FILE* f = fopen(path, "rb");
    if (!f) return NULL;

    // BMP file header (14 bytes)
    u8 hdr[54];
    if (fread(hdr, 1, 54, f) < 54) { fclose(f); return NULL; }
    if (hdr[0] != 'B' || hdr[1] != 'M') { fclose(f); return NULL; }

    u32 dataOffset = hdr[10] | (hdr[11]<<8) | (hdr[12]<<16) | (hdr[13]<<24);
    int width      = (int)(hdr[18] | (hdr[19]<<8) | (hdr[20]<<16) | (hdr[21]<<24));
    int height     = (int)(hdr[22] | (hdr[23]<<8) | (hdr[24]<<16) | (hdr[25]<<24));
    int bpp        = hdr[28] | (hdr[29]<<8);

    if (width <= 0 || height == 0 || (bpp != 24 && bpp != 32)
        || width > CS2SX_TEX_MAX_DIM
        || height > CS2SX_TEX_MAX_DIM || height < -CS2SX_TEX_MAX_DIM)
        { fclose(f); return NULL; }

    int flipped = height > 0; // positive height = bottom-up storage
    if (height < 0) height = -height;

    int bytesPerPx = bpp / 8;
    int rowBytes   = ((width * bytesPerPx + 3) / 4) * 4; // padded to 4 bytes
    u8* rowBuf = (u8*)malloc(rowBytes);
    if (!rowBuf) { fclose(f); return NULL; }

    u32* pixels = (u32*)malloc(width * height * sizeof(u32));
    if (!pixels) { free(rowBuf); fclose(f); return NULL; }

    fseek(f, (long)dataOffset, SEEK_SET);
    for (int row = 0; row < height; row++)
    {
        if (fread(rowBuf, 1, rowBytes, f) < (size_t)rowBytes) break;
        int destRow = flipped ? (height - 1 - row) : row;
        for (int col = 0; col < width; col++)
        {
            u8 b = rowBuf[col * bytesPerPx + 0];
            u8 g = rowBuf[col * bytesPerPx + 1];
            u8 r = rowBuf[col * bytesPerPx + 2];
            u8 a = (bpp == 32) ? rowBuf[col * bytesPerPx + 3] : 0xFF;
            // framebuffer expects RGBA_8888
            pixels[destRow * width + col] = ((u32)a << 24) | ((u32)b << 16) | ((u32)g << 8) | r;
        }
    }

    free(rowBuf);
    fclose(f);

    Texture* tex = (Texture*)malloc(sizeof(Texture));
    if (!tex) { free(pixels); return NULL; }
    tex->f_Width  = width;
    tex->f_Height = height;
    tex->f_Pixels = pixels;
    return tex;
}

// ============================================================================
// Framebuffer State
// ============================================================================

extern Framebuffer g_fb;
extern u32* g_fb_addr;
extern u32* g_sw_backbuf;
extern int         g_fb_width;     // PHYSICAL render width  = output resolution
extern int         g_fb_height;    // PHYSICAL render height = output resolution
extern int         g_out_w;        // requested output width  (0 = auto = logical)
extern int         g_out_h;        // requested output height (0 = auto = logical)
extern int         g_sn;           // coord scale numerator   (logical -> physical)
extern int         g_sd;           // coord scale denominator
extern int         g_gfx_init;
extern PadState    g_cs2sx_pad;
extern u64         g_cs2sx_kDown;
extern u64         g_cs2sx_kHeld;
extern u64         g_cs2sx_kUp;

// ============================================================================
// Graphics primitives
// ============================================================================

// Scales one logical coordinate to physical (output) space. Identity when the
// output resolution equals the logical size (g_sn == g_sd).
#define CS2SX_SCL(v)  ((v) = (v) * g_sn / g_sd)

// Render at a higher OUTPUT resolution than the logical design size. The whole
// scene is drawn 1:1 into an output-sized framebuffer (no downsample), so in
// docked mode the Switch displays it pixel-perfect instead of upscaling 720p.
// Call Graphics_SetOutputResolution(1920,1080) BEFORE Graphics_Init. All app
// coordinates stay logical — primitives scale them to physical via g_sn/g_sd.
static inline void Graphics_SetOutputResolution(int w, int h)
{
    g_out_w = w;
    g_out_h = h;
}
// Scale ratio (numerator/denominator) for callers that render their own pixels
// (e.g. the FreeType text renderer): physical = logical * g_sn / g_sd.
static inline int Graphics_GetScaleNum(void) { return g_sn; }
static inline int Graphics_GetScaleDen(void) { return g_sd; }

static inline void Graphics_Init(int width, int height)
{
    if (g_gfx_init) return;
    int ow = (g_out_w > 0) ? g_out_w : width;
    int oh = (g_out_h > 0) ? g_out_h : height;
    g_fb_width  = ow;
    g_fb_height = oh;
    // Uniform scale (aspect is preserved): physical = logical * ow / width.
    g_sn = ow;
    g_sd = width;
    g_gfx_init  = 1;
}

static int _g_frame_owned = 0;

static inline void Graphics_BeginFrame(void)
{
    if (g_fb_addr) return;  // runtime already manages this frame
    u8* raw = framebufferBegin(&g_fb, NULL);
    g_fb_addr = (u32*)raw;
    _g_frame_owned = 1;
}

static inline void Graphics_EndFrame(void)
{
    if (!_g_frame_owned) return;  // runtime will call framebufferEnd after OnFrame returns
    framebufferEnd(&g_fb);
    g_fb_addr = NULL;
    _g_frame_owned = 0;
}

static inline void Graphics_SetPixel(int x, int y, u32 color)
{
    if (!g_fb_addr) return;
    if (x < 0 || x >= g_fb_width || y < 0 || y >= g_fb_height) return;
    g_fb_addr[y * g_fb_width + x] = color;
}

// Forward declaration: the anti-aliased primitives below (FillCircle, DrawLine)
// blend via Graphics_SetPixelAlpha, whose full definition appears further down.
static inline void Graphics_SetPixelAlpha(int x, int y, u32 color, u8 alpha);

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
    CS2SX_SCL(x); CS2SX_SCL(y); CS2SX_SCL(w); CS2SX_SCL(h);   // logical → physical
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
    CS2SX_SCL(x); CS2SX_SCL(y); CS2SX_SCL(w); CS2SX_SCL(h);   // logical → physical
    // Clip to screen bounds once — eliminates per-pixel bounds checks
    int x0 = x < 0 ? 0 : x;
    int y0 = y < 0 ? 0 : y;
    int x1 = x + w > g_fb_width  ? g_fb_width  : x + w;
    int y1 = y + h > g_fb_height ? g_fb_height : y + h;
    if (x0 >= x1 || y0 >= y1) return;
    for (int row = y0; row < y1; row++)
    {
        u32* dst = &g_fb_addr[row * g_fb_width + x0];
        for (int col = x0; col < x1; col++)
            *dst++ = color;
    }
}

// Anti-aliased line (Xiaolin-Wu style). Axis-aligned lines stay crisp (full
// coverage on one row/column); diagonals get smooth, non-stair-stepped edges.
static inline void _cs2sx_plot_cov(int x, int y, u32 color, float c)
{
    if (c <= 0.0f) return;
    if (c >= 1.0f) Graphics_SetPixel(x, y, color);
    else Graphics_SetPixelAlpha(x, y, color, (u8)(c * 255.0f));
}

static inline void Graphics_DrawLine(int x0, int y0, int x1, int y1, u32 color)
{
    if (!g_fb_addr) return;
    CS2SX_SCL(x0); CS2SX_SCL(y0); CS2SX_SCL(x1); CS2SX_SCL(y1);   // logical → physical

    int steep = abs(y1 - y0) > abs(x1 - x0);
    if (steep) { int t; t = x0; x0 = y0; y0 = t;  t = x1; x1 = y1; y1 = t; }
    if (x0 > x1) { int t; t = x0; x0 = x1; x1 = t;  t = y0; y0 = y1; y1 = t; }

    int   dx   = x1 - x0;
    int   dy   = y1 - y0;
    float grad = (dx == 0) ? 1.0f : (float)dy / (float)dx;
    float yf   = (float)y0;

    for (int x = x0; x <= x1; x++)
    {
        int   iy = (int)floorf(yf);
        float f  = yf - (float)iy;
        if (steep)
        {
            _cs2sx_plot_cov(iy,     x, color, 1.0f - f);
            _cs2sx_plot_cov(iy + 1, x, color, f);
        }
        else
        {
            _cs2sx_plot_cov(x, iy,     color, 1.0f - f);
            _cs2sx_plot_cov(x, iy + 1, color, f);
        }
        yf += grad;
    }
}

// 4×4 supersampled coverage of pixel (px,py) inside the circle (ccx,ccy,r).
// Returns 0..16 (number of sub-samples inside) → 17 smooth gradations, sub-pixel
// accurate, which removes the visible stair-stepping of a 1px coverage ramp.
static inline int _cs2sx_circ_cov16(int px, int py, float ccx, float ccy, float r)
{
    float r2 = r * r;
    int inside = 0;
    for (int sj = 0; sj < 4; sj++)
    {
        float sy = (float)py + ((float)(sj * 2 + 1)) * 0.125f - ccy;   // +0.125,.375,.625,.875
        float sy2 = sy * sy;
        for (int si = 0; si < 4; si++)
        {
            float sx = (float)px + ((float)(si * 2 + 1)) * 0.125f - ccx;
            if (sx * sx + sy2 <= r2) inside++;
        }
    }
    return inside;
}

// Solid, clipped rectangle fill in PHYSICAL coordinates (no scaling). Used by the
// rounded-rect interior so it can scale once and fill its straight parts fast.
static inline void _cs2sx_fillrect_phys(int x, int y, int w, int h, u32 color)
{
    if (w <= 0 || h <= 0) return;
    int x0 = x < 0 ? 0 : x;
    int y0 = y < 0 ? 0 : y;
    int x1 = x + w > g_fb_width  ? g_fb_width  : x + w;
    int y1 = y + h > g_fb_height ? g_fb_height : y + h;
    for (int yy = y0; yy < y1; yy++)
    {
        u32* row = &g_fb_addr[yy * g_fb_width];
        for (int xx = x0; xx < x1; xx++) row[xx] = color;
    }
}

// Anti-aliased quarter-circle corner: box [bx,bx+r)×[by,by+r), arc centered at
// (ccx,ccy). Each pixel gets 4×4 coverage for a smooth, non-pixelated curve.
static inline void _cs2sx_aa_corner(int bx, int by, int r, float ccx, float ccy, u32 color)
{
    for (int py = by; py < by + r; py++)
        for (int px = bx; px < bx + r; px++)
        {
            int cov = _cs2sx_circ_cov16(px, py, ccx, ccy, (float)r);
            if (cov == 0) continue;
            if (cov >= 16) Graphics_SetPixel(px, py, color);
            else Graphics_SetPixelAlpha(px, py, color, (u8)(cov * 255 / 16));
        }
}

static inline void Graphics_DrawCircle(int cx, int cy, int r, u32 color)
{
    if (!g_fb_addr) return;
    CS2SX_SCL(cx); CS2SX_SCL(cy); CS2SX_SCL(r);   // logical → physical
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

// Anti-aliased filled circle: the interior is filled solid; the ~1px rim is
// coverage-blended (distance to the edge → alpha) for smooth, non-pixelated
// curves. Also smooths every FillRoundedRect, whose corners are FillCircles.
static inline void Graphics_FillCircle(int cx, int cy, int r, u32 color)
{
    if (!g_fb_addr || r <= 0) return;
    CS2SX_SCL(cx); CS2SX_SCL(cy); CS2SX_SCL(r);   // logical → physical
    int rin  = (r - 1) * (r - 1);   // clearly inside  → solid (no per-pixel cost)
    int rout = (r + 1) * (r + 1);   // clearly outside → skip
    float ccx = (float)cx, ccy = (float)cy;
    for (int dy = -r - 1; dy <= r + 1; dy++)
        for (int dx = -r - 1; dx <= r + 1; dx++)
        {
            int d2 = dx * dx + dy * dy;
            if (d2 <= rin) { Graphics_SetPixel(cx + dx, cy + dy, color); continue; }
            if (d2 > rout) continue;
            int cov = _cs2sx_circ_cov16(cx + dx, cy + dy, ccx, ccy, (float)r);
            if (cov == 0) continue;
            if (cov >= 16) Graphics_SetPixel(cx + dx, cy + dy, color);
            else Graphics_SetPixelAlpha(cx + dx, cy + dy, color, (u8)(cov * 255 / 16));
        }
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
    if (!tex || !tex->f_Pixels || !g_fb_addr) return;
    // Keep the logical size: scale the texture to (W*g_sn/g_sd, H*g_sn/g_sd).
    int dw = tex->f_Width  * g_sn / g_sd;
    int dh = tex->f_Height * g_sn / g_sd;
    CS2SX_SCL(x); CS2SX_SCL(y);
    if (dw <= 0 || dh <= 0) return;
    for (int row = 0; row < dh; row++)
    {
        int srcRow = row * tex->f_Height / dh;
        int py = y + row;
        if (py < 0 || py >= g_fb_height) continue;
        for (int col = 0; col < dw; col++)
        {
            int srcCol = col * tex->f_Width / dw;
            int px = x + col;
            if (px < 0 || px >= g_fb_width) continue;
            u32 c = tex->f_Pixels[srcRow * tex->f_Width + srcCol];
            if ((c >> 24) > 0)
                g_fb_addr[py * g_fb_width + px] = c;
        }
    }
}

// Draws tex centered inside the rectangle (rx, ry, rw, rh) at native size.
static inline void Graphics_DrawTextureCentered(Texture* tex, int rx, int ry, int rw, int rh)
{
    if (!tex) return;
    int x = rx + (rw - tex->f_Width)  / 2;
    int y = ry + (rh - tex->f_Height) / 2;
    Graphics_DrawTexture(tex, x, y);
}

// Draws tex scaled to (dw x dh) pixels at (x, y) using nearest-neighbor interpolation.
static inline void Graphics_DrawTextureScaled(Texture* tex, int x, int y, int dw, int dh)
{
    if (!tex || !tex->f_Pixels || !g_fb_addr || dw <= 0 || dh <= 0) return;
    CS2SX_SCL(x); CS2SX_SCL(y); CS2SX_SCL(dw); CS2SX_SCL(dh);   // logical → physical
    for (int row = 0; row < dh; row++)
    {
        int srcRow = row * tex->f_Height / dh;
        int py = y + row;
        if (py < 0 || py >= g_fb_height) continue;
        for (int col = 0; col < dw; col++)
        {
            int srcCol = col * tex->f_Width / dw;
            int px = x + col;
            if (px < 0 || px >= g_fb_width) continue;
            u32 c = tex->f_Pixels[srcRow * tex->f_Width + srcCol];
            if ((c >> 24) > 0)
                g_fb_addr[py * g_fb_width + px] = c;
        }
    }
}

// Draws tex scaled to (tw x th) and centered inside (rx, ry, rw, rh).
static inline void Graphics_DrawTextureCenteredScaled(Texture* tex,
    int rx, int ry, int rw, int rh, int tw, int th)
{
    if (!tex) return;
    int x = rx + (rw - tw) / 2;
    int y = ry + (rh - th) / 2;
    Graphics_DrawTextureScaled(tex, x, y, tw, th);
}

// Returns a NEW Texture of size dw×dh, bilinear-resampled from src (smooth, no
// nearest-neighbour blockiness). Pure texture→texture; caller owns the result.
// Use this once to pre-scale a photo, then blit the result each frame.
static inline Texture* Graphics_ScaleTextureSmooth(Texture* src, int dw, int dh)
{
    if (!src || !src->f_Pixels || dw <= 0 || dh <= 0) return NULL;
    int sw = src->f_Width, sh = src->f_Height;
    if (sw <= 0 || sh <= 0) return NULL;

    Texture* t = Texture_New(dw, dh, NULL);   // zero-filled; we overwrite every pixel
    if (!t) return NULL;
    u32* dpx = t->f_Pixels;
    u32* spx = src->f_Pixels;

    for (int y = 0; y < dh; y++)
    {
        float fy = ((float)y + 0.5f) * (float)sh / (float)dh - 0.5f;
        int y0 = (int)floorf(fy);
        float wy = fy - (float)y0;
        int y1 = y0 + 1;
        if (y0 < 0) y0 = 0; if (y0 > sh - 1) y0 = sh - 1;
        if (y1 < 0) y1 = 0; if (y1 > sh - 1) y1 = sh - 1;
        float iwy = 1.0f - wy;

        for (int x = 0; x < dw; x++)
        {
            float fx = ((float)x + 0.5f) * (float)sw / (float)dw - 0.5f;
            int x0 = (int)floorf(fx);
            float wx = fx - (float)x0;
            int x1 = x0 + 1;
            if (x0 < 0) x0 = 0; if (x0 > sw - 1) x0 = sw - 1;
            if (x1 < 0) x1 = 0; if (x1 > sw - 1) x1 = sw - 1;
            float iwx = 1.0f - wx;

            u32 c00 = spx[y0 * sw + x0], c01 = spx[y0 * sw + x1];
            u32 c10 = spx[y1 * sw + x0], c11 = spx[y1 * sw + x1];
            float w00 = iwx * iwy, w01 = wx * iwy, w10 = iwx * wy, w11 = wx * wy;

            u32 r = (u32)(( c00        & 0xFF) * w00 + ( c01        & 0xFF) * w01 + ( c10        & 0xFF) * w10 + ( c11        & 0xFF) * w11 + 0.5f);
            u32 g = (u32)(((c00 >>  8) & 0xFF) * w00 + ((c01 >>  8) & 0xFF) * w01 + ((c10 >>  8) & 0xFF) * w10 + ((c11 >>  8) & 0xFF) * w11 + 0.5f);
            u32 b = (u32)(((c00 >> 16) & 0xFF) * w00 + ((c01 >> 16) & 0xFF) * w01 + ((c10 >> 16) & 0xFF) * w10 + ((c11 >> 16) & 0xFF) * w11 + 0.5f);
            u32 a = (u32)(((c00 >> 24) & 0xFF) * w00 + ((c01 >> 24) & 0xFF) * w01 + ((c10 >> 24) & 0xFF) * w10 + ((c11 >> 24) & 0xFF) * w11 + 0.5f);
            dpx[y * dw + x] = (a << 24) | (b << 16) | (g << 8) | r;
        }
    }
    return t;
}

// Blits a texture 1:1 at PHYSICAL pixel coordinates (no g_sn/g_sd scaling). For
// drawing an already-correctly-sized image without re-introducing scaling.
static inline void Graphics_BlitPhysical(Texture* tex, int x, int y)
{
    if (!tex || !tex->f_Pixels || !g_fb_addr) return;
    int w = tex->f_Width, h = tex->f_Height;
    for (int row = 0; row < h; row++)
    {
        int py = y + row;
        if (py < 0 || py >= g_fb_height) continue;
        u32* dst  = &g_fb_addr[py * g_fb_width];
        u32* srow = &tex->f_Pixels[row * w];
        for (int col = 0; col < w; col++)
        {
            int px = x + col;
            if (px < 0 || px >= g_fb_width) continue;
            u32 c = srow[col];
            if ((c >> 24) > 0) dst[px] = c;
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
    CS2SX_SCL(x0); CS2SX_SCL(y0); CS2SX_SCL(x1); CS2SX_SCL(y1); CS2SX_SCL(x2); CS2SX_SCL(y2);
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
    CS2SX_SCL(cx); CS2SX_SCL(cy); CS2SX_SCL(rx); CS2SX_SCL(ry);   // logical → physical
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
    CS2SX_SCL(cx); CS2SX_SCL(cy); CS2SX_SCL(rx); CS2SX_SCL(ry);   // logical → physical
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
    if (!g_fb_addr || w <= 0 || h <= 0) return;
    CS2SX_SCL(x); CS2SX_SCL(y); CS2SX_SCL(w); CS2SX_SCL(h); CS2SX_SCL(r);
    if (r < 0) r = 0;
    if (r > w / 2) r = w / 2;
    if (r > h / 2) r = h / 2;
    if (r == 0) { _cs2sx_fillrect_phys(x, y, w, h, color); return; }

    // Straight interior (no AA needed) — tiles the whole rect with no gaps:
    _cs2sx_fillrect_phys(x,     y + r,     w,         h - 2 * r, color);  // middle band
    _cs2sx_fillrect_phys(x + r, y,         w - 2 * r, r,         color);  // top band
    _cs2sx_fillrect_phys(x + r, y + h - r, w - 2 * r, r,         color);  // bottom band

    // Four anti-aliased corners (centers at the inner-rounding points).
    _cs2sx_aa_corner(x,         y,         r, (float)(x + r),     (float)(y + r),     color); // TL
    _cs2sx_aa_corner(x + w - r, y,         r, (float)(x + w - r), (float)(y + r),     color); // TR
    _cs2sx_aa_corner(x,         y + h - r, r, (float)(x + r),     (float)(y + h - r), color); // BL
    _cs2sx_aa_corner(x + w - r, y + h - r, r, (float)(x + w - r), (float)(y + h - r), color); // BR
}

static inline void Graphics_SetPixelAlpha(int x, int y, u32 color, u8 alpha)
{
    if (!g_fb_addr) return;
    if (x < 0 || x >= g_fb_width || y < 0 || y >= g_fb_height) return;
    u32* dst = &g_fb_addr[y * g_fb_width + x];
    u32 bg = *dst;
    u32 sr = (color >>  0) & 0xFF, sg = (color >>  8) & 0xFF, sb = (color >> 16) & 0xFF;
    u32 dr = (bg    >>  0) & 0xFF, dg = (bg    >>  8) & 0xFF, db = (bg    >> 16) & 0xFF;
    u32 a = alpha, ia = 255 - a;
    // >> 8 instead of / 255: faster on ARM, imperceptible error (≤1/255 per channel)
    *dst = 0xFF000000
        | (((sb * a + db * ia + 128) >> 8) << 16)
        | (((sg * a + dg * ia + 128) >> 8) <<  8)
        |  ((sr * a + dr * ia + 128) >> 8);
}

static inline void Graphics_FillRectAlpha(int x, int y, int w, int h, u32 color, u8 alpha)
{
    if (!g_fb_addr || alpha == 0) return;
    if (alpha == 255) { Graphics_FillRect(x, y, w, h, color); return; }
    CS2SX_SCL(x); CS2SX_SCL(y); CS2SX_SCL(w); CS2SX_SCL(h);   // after the FillRect delegate
    // Clip to screen bounds once — avoids per-pixel bounds check
    int x0 = x < 0 ? 0 : x;
    int y0 = y < 0 ? 0 : y;
    int x1 = x + w > g_fb_width  ? g_fb_width  : x + w;
    int y1 = y + h > g_fb_height ? g_fb_height : y + h;
    if (x0 >= x1 || y0 >= y1) return;
    // Pre-compute source channels and alpha weights once outside the loop
    u32 sr = (color >>  0) & 0xFF;
    u32 sg = (color >>  8) & 0xFF;
    u32 sb = (color >> 16) & 0xFF;
    u32 a  = alpha, ia = 255 - a;
    u32 sr_a = sr * a + 128;
    u32 sg_a = sg * a + 128;
    u32 sb_a = sb * a + 128;
    for (int row = y0; row < y1; row++)
    {
        u32* dst = &g_fb_addr[row * g_fb_width + x0];
        for (int col = x0; col < x1; col++, dst++)
        {
            u32 bg = *dst;
            u32 dr = (bg >>  0) & 0xFF;
            u32 dg = (bg >>  8) & 0xFF;
            u32 db = (bg >> 16) & 0xFF;
            *dst = 0xFF000000
                | (((sb_a + db * ia) >> 8) << 16)
                | (((sg_a + dg * ia) >> 8) <<  8)
                |  ((sr_a + dr * ia) >> 8);
        }
    }
}

static inline void Graphics_DrawTextAlpha(int x, int y, const char* text, u32 color, int scale, u8 alpha)
{
    if (!g_fb_addr || !text || alpha == 0) return;
    if (alpha == 255) { Graphics_DrawText(x, y, text, color, scale);return; }
    CS2SX_SCL(x); CS2SX_SCL(y); CS2SX_SCL(scale);   // after the DrawText delegate
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
    CS2SX_TouchState state = {0};
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
// Extension System — Battery
// ============================================================================

typedef struct { int percent; bool charging; bool connected; } CS2SX_BatteryInfo;

static inline CS2SX_BatteryInfo CS2SX_GetBattery(void)
{
    CS2SX_BatteryInfo info = { 0, false, false };
    u32 chargePercent = 0;
    if (R_SUCCEEDED(psmGetBatteryChargePercentage(&chargePercent))) {
        info.percent = (int)chargePercent;
    } else {
        // psmGetBatteryChargePercentage can silently fail on some firmware/libnx versions;
        // psmGetRawBatteryChargePercentage is the more reliable alternative.
        double rawPercent = 0.0;
        if (R_SUCCEEDED(psmGetRawBatteryChargePercentage(&rawPercent))) {
            info.percent = (int)(rawPercent + 0.5);
            chargePercent = (u32)info.percent;
        }
    }
    PsmChargerType chargerType = PsmChargerType_Unconnected;
    if (R_SUCCEEDED(psmGetChargerType(&chargerType))) {
        info.connected = chargerType != PsmChargerType_Unconnected;
        info.charging = info.connected && chargePercent < 100;
    }
    return info;
}

// ============================================================================
// Extension System — Time
// ============================================================================

typedef struct { int hour; int minute; int second; } CS2SX_TimeInfo;

static inline CS2SX_TimeInfo CS2SX_GetTime(void)
{
    CS2SX_TimeInfo t = { 0, 0, 0 };
    time_t now = time(NULL);
    struct tm* lt = localtime(&now);
    if (lt) { t.hour = lt->tm_hour; t.minute = lt->tm_min; t.second = lt->tm_sec; }
    return t;
}

// ============================================================================
// SwitchApp_Run
// ============================================================================

static inline void SwitchApp_Run(SwitchApp* self)
{
    if (!self) return;

    romfsInit();    // no-op if no romfs was linked; enables romfs:/ paths
    psmInitialize(); // battery service — must stay open for the app lifetime

    PadState pad;
    padConfigureInput(1, HidNpadStyleSet_NpadStandard);
    padInitializeDefault(&pad);

    // OnInit first: lets the user call Graphics.Init() to set g_fb_width/g_fb_height
    // before framebufferCreate, so the buffer is allocated at the correct size.
    if (self->OnInit)
        self->OnInit(self);

    int use_gfx = g_gfx_init;

    if (use_gfx)
    {
        NWindow* win = nwindowGetDefault();
        // Hardware framebuffer at the OUTPUT resolution (g_fb_width/height). When
        // the app requests e.g. 1080p the Switch shows it 1:1 in docked mode
        // instead of upscaling a 720p buffer. Rendering is 1:1 — no downsample.
        framebufferCreate(&g_fb, win,
            (u32)g_fb_width, (u32)g_fb_height,
            PIXEL_FORMAT_RGBA_8888, 2);
        framebufferMakeLinear(&g_fb);
        // Allocate a CPU-cached software backbuffer.  All drawing (including
        // FillRectAlpha reads) goes to this cached buffer; at end of frame we
        // memcpy once to the hardware framebuffer.  This avoids the severe
        // stall penalty of reading back from write-combining VRAM.
        g_sw_backbuf = (u32*)malloc((size_t)g_fb_width * (size_t)g_fb_height * sizeof(u32));
    }
    else
    {
        consoleInit(NULL);
        Form_InitFocus(&self->form);
    }

    while (appletMainLoop())
    {
        cs2sx_frame_begin();
        padUpdate(&pad);
        g_cs2sx_pad  = pad;
        self->kDown  = padGetButtonsDown(&pad);
        self->kHeld  = padGetButtons(&pad);
        g_cs2sx_kDown = self->kDown;
        g_cs2sx_kHeld = self->kHeld;
        g_cs2sx_kUp   = padGetButtonsUp(&pad);

        if (use_gfx)
        {
            u8* fb_raw = framebufferBegin(&g_fb, NULL);
            if (!fb_raw) continue;

            // Point g_fb_addr at the cached backbuffer so all pixel ops are fast
            g_fb_addr = g_sw_backbuf ? g_sw_backbuf : (u32*)fb_raw;

            int total = g_fb_width * g_fb_height;
            for (int i = 0; i < total; i++)
                g_fb_addr[i] = COLOR_BLACK;

            Form_UpdateAll(&self->form, self->kDown, self->kHeld);

            if (self->OnFrame)
                self->OnFrame(self);

            Form_DrawAll(&self->form);

            // Flush backbuffer to hardware framebuffer in one pass (write-only, fast)
            if (g_sw_backbuf)
                memcpy(fb_raw, g_sw_backbuf, (size_t)total * sizeof(u32));

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
    {
        free(g_sw_backbuf);
        g_sw_backbuf = NULL;
        framebufferClose(&g_fb);
    }
    else
        consoleExit(NULL);

    psmExit();
    romfsExit();   // paired with romfsInit() at startup — avoids leaking the mount
}