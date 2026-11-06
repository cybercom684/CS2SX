#pragma once
// ============================================================================
// cs2sx_gfx.h — 2D graphics (GPU, deko3d). Implementation: src/rt_gfx.c
//
// Coordinates are LOGICAL (the size passed to Graphics_Init, default 1280x720),
// y pointing down. The swapchain runs at the OUTPUT resolution
// (Graphics_SetOutputResolution, e.g. 1920x1080); the GPU scales logical → output
// for free, so 1080p costs nothing extra. Exceptions working in PHYSICAL output
// pixels: Graphics_SetPixel, Graphics_SetPixelAlpha, Graphics_BlitPhysical.
//
// Colors are u32 RGBA8888 as stored in memory (little-endian 0xAABBGGRR),
// alpha-blended. Outlines are one physical pixel wide; curved shapes and lines
// are anti-aliased with a one-pixel alpha fringe.
//
// All drawing happens between the runtime's frame begin/end (inside OnFrame);
// Graphics_BeginFrame/EndFrame are kept for source compatibility and do nothing.
// ============================================================================

#include <switch.h>

// ── Textures ─────────────────────────────────────────────────────────────────

typedef struct Texture Texture;
struct Texture {
    int  f_Width;    // field names match the transpiler's auto-property naming (f_Name)
    int  f_Height;
    u32* f_Pixels;   // CPU copy (RGBA8888); uploaded to the GPU on first draw
    void* _gpu;      // GPU image + descriptor slot (rt_gfx.c), NULL until first use
};

// Max texture dimension — guards against overflowing allocations from corrupt headers.
#define CS2SX_TEX_MAX_DIM 16384

/// New texture of width×height, copying `pixels` (or zero-filled when NULL).
Texture* Texture_New(int width, int height, u32* pixels);
/// Frees the texture (the GPU copy is released once the GPU no longer uses it).
void     Texture_Dispose(Texture* t);
/// Re-uploads f_Pixels after the app modified them.
void     Texture_Update(Texture* t);

/// 24/32-bit BMP (e.g. "romfs:/sprite.bmp"); NULL on failure.
Texture* CS2SX_Texture_LoadBMP(const char* path);

// PNG/JPEG/... decoding comes from the optional stb_image extern lib. These shims
// stay static inline here so apps that never call them don't need the lib.
extern unsigned int* CS2SX_Image_DecodeRGBA(const char* path, int* w, int* h);
extern unsigned int* CS2SX_Image_DecodeThumb(const char* path, int maxW, int maxH, int* ow, int* oh);

static inline Texture* Graphics_LoadImage(const char* path)
{
    int w = 0, h = 0;
    unsigned int* px = CS2SX_Image_DecodeRGBA(path, &w, &h);
    if (!px) return NULL;
    Texture* t = Texture_New(w, h, (u32*)px);   // copies the pixels
    free(px);
    return t;
}

static inline Texture* Graphics_LoadImageThumb(const char* path, int maxSize)
{
    int w = 0, h = 0;
    unsigned int* px = CS2SX_Image_DecodeThumb(path, maxSize, maxSize, &w, &h);
    if (!px) return NULL;
    Texture* t = Texture_New(w, h, (u32*)px);
    free(px);
    return t;
}

/// Bilinear-resampled copy of src at dw×dh (CPU); caller owns the result.
Texture* Graphics_ScaleTextureSmooth(Texture* src, int dw, int dh);

// ── Setup / frame (driven by SwitchApp_Run) ──────────────────────────────────

extern int g_fb_width;     // PHYSICAL output width
extern int g_fb_height;    // PHYSICAL output height
extern int g_out_w;        // requested output width  (0 = logical size)
extern int g_out_h;        // requested output height (0 = logical size)
extern int g_sn;           // logical → physical scale numerator
extern int g_sd;           // logical → physical scale denominator
extern int g_gfx_init;     // Graphics_Init was called → graphics mode

void Graphics_SetOutputResolution(int w, int h);   // before Graphics_Init
void Graphics_Init(int width, int height);
static inline int Graphics_GetScaleNum(void) { return g_sn; }
static inline int Graphics_GetScaleDen(void) { return g_sd; }
static inline void Graphics_BeginFrame(void) { }   // frames are managed by the runtime
static inline void Graphics_EndFrame(void)   { }

/// Creates the GPU device and swapchain (once, after OnInit). Returns 1 on success.
int  CS2SX_Gfx_Start(void);
/// Starts recording a frame (clears to black).
void CS2SX_Gfx_BeginFrame(void);
/// Submits and presents the frame.
void CS2SX_Gfx_EndFrame(void);
/// Waits for the GPU and releases every GPU resource.
void CS2SX_Gfx_Shutdown(void);

// ── Drawing ──────────────────────────────────────────────────────────────────

void Graphics_FillScreen(u32 color);
void Graphics_SetPixel(int x, int y, u32 color);                  // physical
void Graphics_SetPixelAlpha(int x, int y, u32 color, u8 alpha);   // physical
void Graphics_DrawRect(int x, int y, int w, int h, u32 color);
void Graphics_FillRect(int x, int y, int w, int h, u32 color);
void Graphics_FillRectAlpha(int x, int y, int w, int h, u32 color, u8 alpha);
void Graphics_DrawLine(int x0, int y0, int x1, int y1, u32 color);
void Graphics_DrawCircle(int cx, int cy, int r, u32 color);
void Graphics_FillCircle(int cx, int cy, int r, u32 color);
void Graphics_DrawEllipse(int cx, int cy, int rx, int ry, u32 color);
void Graphics_FillEllipse(int cx, int cy, int rx, int ry, u32 color);
void Graphics_DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, u32 color);
void Graphics_FillTriangle(int x0, int y0, int x1, int y1, int x2, int y2, u32 color);
void Graphics_DrawRoundedRect(int x, int y, int w, int h, int r, u32 color);
void Graphics_FillRoundedRect(int x, int y, int w, int h, int r, u32 color);
void Graphics_DrawPolygon(int* xs, int* ys, int count, u32 color);   // closed outline
void Graphics_DrawGrid(int x, int y, int w, int h, int cellW, int cellH, u32 color);

void Graphics_DrawChar(int x, int y, char c, u32 color, int scale);
void Graphics_DrawText(int x, int y, const char* text, u32 color, int scale);
void Graphics_DrawTextAlpha(int x, int y, const char* text, u32 color, int scale, u8 alpha);
void Graphics_DrawTextShadow(int x, int y, const char* text, u32 color, u32 shadow, int scale);
int  Graphics_MeasureTextWidth(const char* text, int scale);
int  Graphics_MeasureTextHeight(int scale);

void Graphics_DrawTexture(Texture* tex, int x, int y);
void Graphics_DrawTextureScaled(Texture* tex, int x, int y, int dw, int dh);
void Graphics_DrawTextureCentered(Texture* tex, int rx, int ry, int rw, int rh);
void Graphics_DrawTextureCenteredScaled(Texture* tex, int rx, int ry, int rw, int rh, int tw, int th);
void Graphics_BlitPhysical(Texture* tex, int x, int y);              // physical, 1:1
