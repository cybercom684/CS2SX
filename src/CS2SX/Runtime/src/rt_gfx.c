// ============================================================================
// Runtime module for cs2sx_gfx.h — 2D renderer on deko3d.
//
//   • one pipeline: textured, vertex-colored triangles (shaders/gfx2d_*.glsl)
//   • every Graphics_* call appends triangles to a per-frame vertex ring; a draw
//     call is issued only when the bound texture changes. The 8×8 font and a
//     white block share one atlas texture, so text and solid shapes batch together.
//   • GFX_FRAMES frames in flight, each with its own command memory, vertex ring
//     and fence; GPU resources freed during a frame are released once its fence
//     has passed.
//   • textures upload lazily on first draw (apps create them in OnInit, before
//     the GPU exists); the copy is recorded into the current frame.
//   • anti-aliasing: curved shapes and lines get a one-physical-pixel alpha fringe.
// ============================================================================
#include "cs2sx_prelude.h"
#include <deko3d.h>
#include "cs2sx_gfx_shaders.h"

// ── Configuration ────────────────────────────────────────────────────────────

#define GFX_FRAMES      2                      // frames in flight == swapchain images
#define GFX_CMD_BYTES   (1u * 1024 * 1024)     // command memory per frame
#define GFX_VTX_BYTES   (4u * 1024 * 1024)     // vertex ring per frame (~200k vertices)
#define GFX_MAX_TEX     1024                   // texture descriptor slots
#define GFX_UBO_BYTES   256
#define GFX_CODE_BYTES  (64u * 1024)

#define ALIGN_UP(v, a)  (((v) + (a) - 1) & ~((a) - 1))

// ── Global graphics settings (were in the generated globals) ─────────────────

int g_fb_width  = 1280;
int g_fb_height = 720;
int g_out_w     = 0;
int g_out_h     = 0;
int g_sn        = 1;
int g_sd        = 1;
int g_gfx_init  = 0;

static int s_logW = 1280, s_logH = 720;   // logical size (Graphics_Init)

void Graphics_SetOutputResolution(int w, int h) { g_out_w = w; g_out_h = h; }

void Graphics_Init(int width, int height)
{
    if (g_gfx_init || width <= 0 || height <= 0) return;
    s_logW = width;
    s_logH = height;
    g_fb_width  = g_out_w > 0 ? g_out_w : width;
    g_fb_height = g_out_h > 0 ? g_out_h : height;
    g_sn = g_fb_width;    // uniform scale, aspect preserved: physical = logical * sn / sd
    g_sd = width;
    g_gfx_init = 1;
}

// ── GPU state ────────────────────────────────────────────────────────────────

typedef struct { float x, y, u, v; u32 color; } GfxVtx;

typedef struct GfxTexGpu { DkMemBlock mem; DkImage image; int slot; } GfxTexGpu;

typedef struct { DkMemBlock mem; int slot; } GfxDeferred;

static int          s_ready;
static DkDevice     s_dev;
static DkQueue      s_queue;
static DkMemBlock   s_fbMem, s_codeMem, s_dataMem;
static DkSwapchain  s_swap;
static DkImage      s_fb[GFX_FRAMES];
static DkShader     s_vsh, s_fsh;
static DkCmdBuf     s_cmd[GFX_FRAMES];
static DkFence      s_fence[GFX_FRAMES];
static int          s_fenceUsed[GFX_FRAMES];
static GfxDeferred* s_deferred[GFX_FRAMES];
static int          s_deferredCount[GFX_FRAMES], s_deferredCap[GFX_FRAMES];

static uint32_t     s_cmdOffset[GFX_FRAMES], s_vtxOffset[GFX_FRAMES];
static uint32_t     s_descImgOffset, s_descSmpOffset, s_uboOffset;

static unsigned char s_slotUsed[GFX_MAX_TEX];

// per-frame recording state
static int       s_inFrame;
static int       s_frame;           // frame-in-flight index
static int       s_image;           // acquired swapchain image
static DkCmdBuf  s_cb;              // == s_cmd[s_frame]
static GfxVtx*   s_vtx;             // CPU view of this frame's vertex ring
static uint32_t  s_vtxCount, s_vtxCap, s_batchStart;
static int       s_curSlot = -1;
static int       s_overflowWarned;

static float     s_px = 1.0f;       // one physical pixel in logical units

// built-in atlas: font glyphs (16×6 grid of 8×8) + an 8×8 white block below them
#define ATLAS_W 128
#define ATLAS_H 56
static Texture*  s_atlas;
static float     s_whiteU, s_whiteV;

// 8×8 bitmap font for ASCII 32..127; bit 0 of each row byte = leftmost pixel.
static const u8 s_font8x8[96][8] = {
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

// ── Small helpers ────────────────────────────────────────────────────────────

static void gfx_defer(DkMemBlock mem, int slot)
{
    int f = s_frame;
    if (s_deferredCount[f] == s_deferredCap[f])
    {
        int cap = s_deferredCap[f] ? s_deferredCap[f] * 2 : 32;
        GfxDeferred* n = (GfxDeferred*)realloc(s_deferred[f], (size_t)cap * sizeof(GfxDeferred));
        if (!n) return;   // leak rather than free too early
        s_deferred[f] = n;
        s_deferredCap[f] = cap;
    }
    s_deferred[f][s_deferredCount[f]++] = (GfxDeferred){ mem, slot };
}

static void gfx_release_deferred(int f)
{
    for (int i = 0; i < s_deferredCount[f]; i++)
    {
        if (s_deferred[f][i].mem) dkMemBlockDestroy(s_deferred[f][i].mem);
        if (s_deferred[f][i].slot >= 0) s_slotUsed[s_deferred[f][i].slot] = 0;
    }
    s_deferredCount[f] = 0;
}

static int gfx_alloc_slot(void)
{
    for (int i = 0; i < GFX_MAX_TEX; i++)
        if (!s_slotUsed[i]) { s_slotUsed[i] = 1; return i; }
    return -1;
}

static inline u32 with_alpha(u32 color, u8 alpha) { return (color & 0x00FFFFFFu) | ((u32)alpha << 24); }
static inline u32 transparent(u32 color) { return color & 0x00FFFFFFu; }

// ── Texture upload ───────────────────────────────────────────────────────────

// Records an upload of t->f_Pixels into t's GPU image (creating it if needed).
static int gfx_upload(Texture* t)
{
    if (!s_inFrame || !t || !t->f_Pixels || t->f_Width <= 0 || t->f_Height <= 0) return -1;

    GfxTexGpu* g = (GfxTexGpu*)t->_gpu;
    if (!g)
    {
        int slot = gfx_alloc_slot();
        if (slot < 0) return -1;

        DkImageLayoutMaker lm;
        dkImageLayoutMakerDefaults(&lm, s_dev);
        lm.format = DkImageFormat_RGBA8_Unorm;
        lm.dimensions[0] = (uint32_t)t->f_Width;
        lm.dimensions[1] = (uint32_t)t->f_Height;
        DkImageLayout layout;
        dkImageLayoutInitialize(&layout, &lm);

        DkMemBlockMaker mm;
        dkMemBlockMakerDefaults(&mm, s_dev, ALIGN_UP((uint32_t)dkImageLayoutGetSize(&layout), DK_MEMBLOCK_ALIGNMENT));
        mm.flags = DkMemBlockFlags_GpuCached | DkMemBlockFlags_Image;
        DkMemBlock mem = dkMemBlockCreate(&mm);
        if (!mem) { s_slotUsed[slot] = 0; return -1; }

        g = (GfxTexGpu*)calloc(1, sizeof(GfxTexGpu));
        if (!g) { dkMemBlockDestroy(mem); s_slotUsed[slot] = 0; return -1; }
        g->mem = mem;
        g->slot = slot;
        dkImageInitialize(&g->image, &layout, mem, 0);

        // descriptor for this slot
        DkImageView view;
        dkImageViewDefaults(&view, &g->image);
        DkImageDescriptor desc;
        dkImageDescriptorInitialize(&desc, &view, false, false);
        memcpy((u8*)dkMemBlockGetCpuAddr(s_dataMem) + s_descImgOffset + (uint32_t)slot * sizeof(DkImageDescriptor),
               &desc, sizeof(desc));
        t->_gpu = g;
    }

    // staging copy, released when this frame has finished on the GPU
    uint32_t bytes = (uint32_t)t->f_Width * (uint32_t)t->f_Height * 4u;
    DkMemBlockMaker sm;
    dkMemBlockMakerDefaults(&sm, s_dev, ALIGN_UP(bytes, DK_MEMBLOCK_ALIGNMENT));
    sm.flags = DkMemBlockFlags_CpuUncached | DkMemBlockFlags_GpuCached;
    DkMemBlock staging = dkMemBlockCreate(&sm);
    if (!staging) return g->slot;
    memcpy(dkMemBlockGetCpuAddr(staging), t->f_Pixels, bytes);

    DkImageView view;
    dkImageViewDefaults(&view, &g->image);
    DkCopyBuf src = { dkMemBlockGetGpuAddr(staging), 0, 0 };
    DkImageRect dst = { 0, 0, 0, (uint32_t)t->f_Width, (uint32_t)t->f_Height, 1 };
    dkCmdBufCopyBufferToImage(s_cb, &src, &view, &dst, 0);
    // make the copy and the (new) descriptor visible to the following draws
    dkCmdBufBarrier(s_cb, DkBarrier_Full, DkInvalidateFlags_Image | DkInvalidateFlags_Descriptors);
    gfx_defer(staging, -1);
    return g->slot;
}

static int gfx_tex_slot(Texture* t)
{
    if (!t) return -1;
    if (t->_gpu) return ((GfxTexGpu*)t->_gpu)->slot;
    return gfx_upload(t);
}

Texture* Texture_New(int width, int height, u32* pixels)
{
    if (width <= 0 || height <= 0 || width > CS2SX_TEX_MAX_DIM || height > CS2SX_TEX_MAX_DIM)
        return NULL;
    Texture* t = (Texture*)calloc(1, sizeof(Texture));
    if (!t) return NULL;
    size_t bytes = (size_t)width * (size_t)height * sizeof(u32);
    t->f_Pixels = (u32*)malloc(bytes);
    if (!t->f_Pixels) { free(t); return NULL; }
    if (pixels) memcpy(t->f_Pixels, pixels, bytes);
    else        memset(t->f_Pixels, 0, bytes);
    t->f_Width = width;
    t->f_Height = height;
    return t;
}

void Texture_Dispose(Texture* t)
{
    if (!t) return;
    GfxTexGpu* g = (GfxTexGpu*)t->_gpu;
    if (g)
    {
        if (s_ready) gfx_defer(g->mem, g->slot);   // the GPU may still sample it this frame
        else { if (g->mem) dkMemBlockDestroy(g->mem); s_slotUsed[g->slot] = 0; }
        free(g);
    }
    free(t->f_Pixels);
    free(t);
}

void Texture_Update(Texture* t)
{
    if (!t || !t->_gpu) return;     // not on the GPU yet → uploads on first draw anyway
    if (s_inFrame) gfx_upload(t);
    else
    {
        // outside a frame: drop the GPU copy, it is re-created on the next draw
        GfxTexGpu* g = (GfxTexGpu*)t->_gpu;
        gfx_defer(g->mem, g->slot);
        free(g);
        t->_gpu = NULL;
    }
}

Texture* CS2SX_Texture_LoadBMP(const char* path)
{
    FILE* f = fopen(path, "rb");
    if (!f) return NULL;
    u8 hdr[54];
    if (fread(hdr, 1, 54, f) < 54 || hdr[0] != 'B' || hdr[1] != 'M') { fclose(f); return NULL; }

    u32 dataOffset = hdr[10] | (hdr[11] << 8) | (hdr[12] << 16) | ((u32)hdr[13] << 24);
    int width  = (int)(hdr[18] | (hdr[19] << 8) | (hdr[20] << 16) | ((u32)hdr[21] << 24));
    int height = (int)(hdr[22] | (hdr[23] << 8) | (hdr[24] << 16) | ((u32)hdr[25] << 24));
    int bpp    = hdr[28] | (hdr[29] << 8);
    if (width <= 0 || height == 0 || (bpp != 24 && bpp != 32) || width > CS2SX_TEX_MAX_DIM
        || height > CS2SX_TEX_MAX_DIM || height < -CS2SX_TEX_MAX_DIM) { fclose(f); return NULL; }

    int flipped = height > 0;            // positive height = bottom-up rows
    if (height < 0) height = -height;
    int bytesPerPx = bpp / 8;
    int rowBytes = ((width * bytesPerPx + 3) / 4) * 4;

    Texture* tex = Texture_New(width, height, NULL);
    u8* row = (u8*)malloc((size_t)rowBytes);
    if (!tex || !row) { free(row); Texture_Dispose(tex); fclose(f); return NULL; }

    fseek(f, (long)dataOffset, SEEK_SET);
    for (int r = 0; r < height; r++)
    {
        if (fread(row, 1, (size_t)rowBytes, f) < (size_t)rowBytes) break;
        u32* dst = &tex->f_Pixels[(flipped ? height - 1 - r : r) * width];
        for (int c = 0; c < width; c++)
        {
            u8 b = row[c * bytesPerPx], g = row[c * bytesPerPx + 1], rr = row[c * bytesPerPx + 2];
            u8 a = bpp == 32 ? row[c * bytesPerPx + 3] : 0xFF;
            dst[c] = ((u32)a << 24) | ((u32)b << 16) | ((u32)g << 8) | rr;
        }
    }
    free(row);
    fclose(f);
    return tex;
}

Texture* Graphics_ScaleTextureSmooth(Texture* src, int dw, int dh)
{
    if (!src || !src->f_Pixels || dw <= 0 || dh <= 0) return NULL;
    int sw = src->f_Width, sh = src->f_Height;
    if (sw <= 0 || sh <= 0) return NULL;
    Texture* t = Texture_New(dw, dh, NULL);
    if (!t) return NULL;
    u32* dpx = t->f_Pixels;
    const u32* spx = src->f_Pixels;
    for (int y = 0; y < dh; y++)
    {
        float fy = ((float)y + 0.5f) * (float)sh / (float)dh - 0.5f;
        int y0 = (int)floorf(fy); float wy = fy - (float)y0; int y1 = y0 + 1;
        if (y0 < 0) y0 = 0; if (y0 > sh - 1) y0 = sh - 1;
        if (y1 < 0) y1 = 0; if (y1 > sh - 1) y1 = sh - 1;
        for (int x = 0; x < dw; x++)
        {
            float fx = ((float)x + 0.5f) * (float)sw / (float)dw - 0.5f;
            int x0 = (int)floorf(fx); float wx = fx - (float)x0; int x1 = x0 + 1;
            if (x0 < 0) x0 = 0; if (x0 > sw - 1) x0 = sw - 1;
            if (x1 < 0) x1 = 0; if (x1 > sw - 1) x1 = sw - 1;
            u32 c00 = spx[y0 * sw + x0], c01 = spx[y0 * sw + x1];
            u32 c10 = spx[y1 * sw + x0], c11 = spx[y1 * sw + x1];
            float w00 = (1 - wx) * (1 - wy), w01 = wx * (1 - wy), w10 = (1 - wx) * wy, w11 = wx * wy;
            u32 out = 0;
            for (int sh8 = 0; sh8 < 32; sh8 += 8)
            {
                float v = ((c00 >> sh8) & 0xFF) * w00 + ((c01 >> sh8) & 0xFF) * w01
                        + ((c10 >> sh8) & 0xFF) * w10 + ((c11 >> sh8) & 0xFF) * w11;
                out |= ((u32)(v + 0.5f) & 0xFF) << sh8;
            }
            dpx[y * dw + x] = out;
        }
    }
    return t;
}

// ── Device setup ─────────────────────────────────────────────────────────────

static void gfx_load_shader(DkShader* shader, const unsigned char* code, uint32_t size, uint32_t* offset)
{
    memcpy((u8*)dkMemBlockGetCpuAddr(s_codeMem) + *offset, code, size);
    DkShaderMaker sm;
    dkShaderMakerDefaults(&sm, s_codeMem, *offset);
    dkShaderInitialize(shader, &sm);
    *offset += ALIGN_UP(size, DK_SHADER_CODE_ALIGNMENT);
}

static void gfx_build_atlas(void)
{
    s_atlas = Texture_New(ATLAS_W, ATLAS_H, NULL);
    if (!s_atlas) return;
    for (int ch = 0; ch < 96; ch++)
    {
        int gx = (ch % 16) * 8, gy = (ch / 16) * 8;
        for (int row = 0; row < 8; row++)
            for (int col = 0; col < 8; col++)
                if (s_font8x8[ch][row] & (1 << col))     // bit 0 = leftmost column
                    s_atlas->f_Pixels[(gy + row) * ATLAS_W + gx + col] = 0xFFFFFFFFu;
    }
    for (int y = 48; y < 56; y++)                         // white block for solid shapes
        for (int x = 0; x < 8; x++)
            s_atlas->f_Pixels[y * ATLAS_W + x] = 0xFFFFFFFFu;
    s_whiteU = 4.0f / ATLAS_W;
    s_whiteV = 52.0f / ATLAS_H;
}

int CS2SX_Gfx_Start(void)
{
    if (s_ready) return 1;

    DkDeviceMaker dm;
    dkDeviceMakerDefaults(&dm);
    s_dev = dkDeviceCreate(&dm);
    if (!s_dev) return 0;

    DkQueueMaker qm;
    dkQueueMakerDefaults(&qm, s_dev);
    qm.flags = DkQueueFlags_Graphics;
    s_queue = dkQueueCreate(&qm);

    // framebuffers at the output resolution
    DkImageLayoutMaker lm;
    dkImageLayoutMakerDefaults(&lm, s_dev);
    lm.flags = DkImageFlags_UsageRender | DkImageFlags_UsagePresent | DkImageFlags_HwCompression;
    lm.format = DkImageFormat_RGBA8_Unorm;
    lm.dimensions[0] = (uint32_t)g_fb_width;
    lm.dimensions[1] = (uint32_t)g_fb_height;
    DkImageLayout fbLayout;
    dkImageLayoutInitialize(&fbLayout, &lm);
    uint32_t fbSize = ALIGN_UP((uint32_t)dkImageLayoutGetSize(&fbLayout), dkImageLayoutGetAlignment(&fbLayout));

    DkMemBlockMaker mm;
    dkMemBlockMakerDefaults(&mm, s_dev, ALIGN_UP(GFX_FRAMES * fbSize, DK_MEMBLOCK_ALIGNMENT));
    mm.flags = DkMemBlockFlags_GpuCached | DkMemBlockFlags_Image;
    s_fbMem = dkMemBlockCreate(&mm);

    DkImage const* images[GFX_FRAMES];
    for (int i = 0; i < GFX_FRAMES; i++)
    {
        dkImageInitialize(&s_fb[i], &fbLayout, s_fbMem, (uint32_t)i * fbSize);
        images[i] = &s_fb[i];
    }
    DkSwapchainMaker swm;
    dkSwapchainMakerDefaults(&swm, s_dev, nwindowGetDefault(), images, GFX_FRAMES);
    s_swap = dkSwapchainCreate(&swm);

    // shader code
    dkMemBlockMakerDefaults(&mm, s_dev, GFX_CODE_BYTES);
    mm.flags = DkMemBlockFlags_CpuUncached | DkMemBlockFlags_GpuCached | DkMemBlockFlags_Code;
    s_codeMem = dkMemBlockCreate(&mm);
    uint32_t codeOffset = 0;
    gfx_load_shader(&s_vsh, cs2sx_gfx2d_vsh, sizeof(cs2sx_gfx2d_vsh), &codeOffset);
    gfx_load_shader(&s_fsh, cs2sx_gfx2d_fsh, sizeof(cs2sx_gfx2d_fsh), &codeOffset);

    // data: [cmd0][vtx0][cmd1][vtx1]...[image descriptors][sampler descriptor][ubo]
    uint32_t off = 0;
    for (int i = 0; i < GFX_FRAMES; i++)
    {
        off = ALIGN_UP(off, DK_CMDMEM_ALIGNMENT);   s_cmdOffset[i] = off; off += GFX_CMD_BYTES;
        off = ALIGN_UP(off, 256);                   s_vtxOffset[i] = off; off += GFX_VTX_BYTES;
    }
    off = ALIGN_UP(off, DK_IMAGE_DESCRIPTOR_ALIGNMENT);   s_descImgOffset = off; off += GFX_MAX_TEX * sizeof(DkImageDescriptor);
    off = ALIGN_UP(off, DK_SAMPLER_DESCRIPTOR_ALIGNMENT); s_descSmpOffset = off; off += sizeof(DkSamplerDescriptor);
    off = ALIGN_UP(off, DK_UNIFORM_BUF_ALIGNMENT);        s_uboOffset = off;     off += GFX_UBO_BYTES;

    dkMemBlockMakerDefaults(&mm, s_dev, ALIGN_UP(off, DK_MEMBLOCK_ALIGNMENT));
    mm.flags = DkMemBlockFlags_CpuUncached | DkMemBlockFlags_GpuCached;
    s_dataMem = dkMemBlockCreate(&mm);
    if (!s_fbMem || !s_codeMem || !s_dataMem || !s_swap) return 0;
    memset((u8*)dkMemBlockGetCpuAddr(s_dataMem) + s_descImgOffset, 0, GFX_MAX_TEX * sizeof(DkImageDescriptor));

    // one sampler: nearest (pixel-exact like the CPU renderer), clamped
    DkSampler smp;
    dkSamplerDefaults(&smp);
    smp.minFilter = DkFilter_Nearest;
    smp.magFilter = DkFilter_Nearest;
    smp.wrapMode[0] = DkWrapMode_ClampToEdge;
    smp.wrapMode[1] = DkWrapMode_ClampToEdge;
    DkSamplerDescriptor smpDesc;
    dkSamplerDescriptorInitialize(&smpDesc, &smp);
    memcpy((u8*)dkMemBlockGetCpuAddr(s_dataMem) + s_descSmpOffset, &smpDesc, sizeof(smpDesc));

    DkCmdBufMaker cbm;
    dkCmdBufMakerDefaults(&cbm, s_dev);
    for (int i = 0; i < GFX_FRAMES; i++)
    {
        s_cmd[i] = dkCmdBufCreate(&cbm);
        s_fenceUsed[i] = 0;
    }

    s_px = (float)g_sd / (float)g_sn;
    gfx_build_atlas();
    s_ready = 1;
    return 1;
}

void CS2SX_Gfx_Shutdown(void)
{
    if (!s_ready) return;
    dkQueueWaitIdle(s_queue);
    for (int i = 0; i < GFX_FRAMES; i++) gfx_release_deferred(i);
    s_ready = 0;
    if (s_atlas) { Texture_Dispose(s_atlas); s_atlas = NULL; }   // s_ready = 0 → freed directly
    for (int i = 0; i < GFX_FRAMES; i++)
    {
        dkCmdBufDestroy(s_cmd[i]);
        free(s_deferred[i]);
        s_deferred[i] = NULL;
        s_deferredCount[i] = s_deferredCap[i] = 0;
    }
    dkQueueDestroy(s_queue);
    dkSwapchainDestroy(s_swap);
    dkMemBlockDestroy(s_dataMem);
    dkMemBlockDestroy(s_codeMem);
    dkMemBlockDestroy(s_fbMem);
    dkDeviceDestroy(s_dev);
}

// ── Frames ───────────────────────────────────────────────────────────────────

static const DkVtxAttribState s_attribs[] = {
    { .bufferId = 0, .isFixed = 0, .offset = offsetof(GfxVtx, x),     .size = DkVtxAttribSize_2x32, .type = DkVtxAttribType_Float, .isBgra = 0 },
    { .bufferId = 0, .isFixed = 0, .offset = offsetof(GfxVtx, u),     .size = DkVtxAttribSize_2x32, .type = DkVtxAttribType_Float, .isBgra = 0 },
    { .bufferId = 0, .isFixed = 0, .offset = offsetof(GfxVtx, color), .size = DkVtxAttribSize_4x8,  .type = DkVtxAttribType_Unorm, .isBgra = 0 },
};
static const DkVtxBufferState s_vtxState[] = { { .stride = sizeof(GfxVtx), .divisor = 0 } };

void CS2SX_Gfx_BeginFrame(void)
{
    if (!s_ready || s_inFrame) return;

    s_frame = (s_frame + 1) % GFX_FRAMES;
    if (s_fenceUsed[s_frame]) dkFenceWait(&s_fence[s_frame], -1);
    gfx_release_deferred(s_frame);

    s_cb = s_cmd[s_frame];
    dkCmdBufClear(s_cb);
    dkCmdBufAddMemory(s_cb, s_dataMem, s_cmdOffset[s_frame], GFX_CMD_BYTES);

    s_image = dkQueueAcquireImage(s_queue, s_swap);

    DkImageView fbView;
    dkImageViewDefaults(&fbView, &s_fb[s_image]);
    dkCmdBufBindRenderTarget(s_cb, &fbView, NULL);

    DkViewport vp = { 0.0f, 0.0f, (float)g_fb_width, (float)g_fb_height, 0.0f, 1.0f };
    DkScissor sc = { 0, 0, (uint32_t)g_fb_width, (uint32_t)g_fb_height };
    dkCmdBufSetViewports(s_cb, 0, &vp, 1);
    dkCmdBufSetScissors(s_cb, 0, &sc, 1);
    dkCmdBufClearColorFloat(s_cb, 0, DkColorMask_RGBA, 0.0f, 0.0f, 0.0f, 1.0f);

    DkShader const* shaders[] = { &s_vsh, &s_fsh };
    dkCmdBufBindShaders(s_cb, DkStageFlag_GraphicsMask, shaders, 2);

    DkRasterizerState rs;
    dkRasterizerStateDefaults(&rs);
    rs.cullMode = DkFace_None;                     // 2D: either winding is fine
    DkColorState cs;
    dkColorStateDefaults(&cs);
    cs.blendEnableMask = 1;                        // alpha blending on render target 0
    DkColorWriteState cws;
    dkColorWriteStateDefaults(&cws);
    DkBlendState bs;
    dkBlendStateDefaults(&bs);                     // src·α + dst·(1-α)
    bs.srcAlphaBlendFactor = DkBlendFactor_One;
    bs.dstAlphaBlendFactor = DkBlendFactor_InvSrcAlpha;
    dkCmdBufBindRasterizerState(s_cb, &rs);
    dkCmdBufBindColorState(s_cb, &cs);
    dkCmdBufBindColorWriteState(s_cb, &cws);
    dkCmdBufBindBlendStates(s_cb, 0, &bs, 1);

    DkGpuAddr data = dkMemBlockGetGpuAddr(s_dataMem);
    dkCmdBufBindImageDescriptorSet(s_cb, data + s_descImgOffset, GFX_MAX_TEX);
    dkCmdBufBindSamplerDescriptorSet(s_cb, data + s_descSmpOffset, 1);

    float xform[4] = { 2.0f / (float)s_logW, -2.0f / (float)s_logH, -1.0f, 1.0f };
    dkCmdBufPushConstants(s_cb, data + s_uboOffset, GFX_UBO_BYTES, 0, sizeof(xform), xform);
    dkCmdBufBindUniformBuffer(s_cb, DkStage_Vertex, 0, data + s_uboOffset, GFX_UBO_BYTES);

    dkCmdBufBindVtxAttribState(s_cb, s_attribs, 3);
    dkCmdBufBindVtxBufferState(s_cb, s_vtxState, 1);
    dkCmdBufBindVtxBuffer(s_cb, 0, data + s_vtxOffset[s_frame], GFX_VTX_BYTES);

    s_vtx = (GfxVtx*)((u8*)dkMemBlockGetCpuAddr(s_dataMem) + s_vtxOffset[s_frame]);
    s_vtxCap = GFX_VTX_BYTES / sizeof(GfxVtx);
    s_vtxCount = s_batchStart = 0;
    s_curSlot = -1;
    s_inFrame = 1;
}

static void gfx_flush(void)
{
    if (s_vtxCount > s_batchStart && s_curSlot >= 0)
    {
        dkCmdBufBindTexture(s_cb, DkStage_Fragment, 0, dkMakeTextureHandle((uint32_t)s_curSlot, 0));
        dkCmdBufDraw(s_cb, DkPrimitive_Triangles, s_vtxCount - s_batchStart, 1, s_batchStart, 0);
    }
    s_batchStart = s_vtxCount;
}

void CS2SX_Gfx_EndFrame(void)
{
    if (!s_inFrame) return;
    gfx_flush();
    s_inFrame = 0;
    dkQueueSubmitCommands(s_queue, dkCmdBufFinishList(s_cb));
    dkQueueSignalFence(s_queue, &s_fence[s_frame], false);
    s_fenceUsed[s_frame] = 1;
    dkQueuePresentImage(s_queue, s_swap, s_image);
}

// ── Vertex emission ──────────────────────────────────────────────────────────

// Selects the texture for the following vertices (flushing on change) and reserves n.
static GfxVtx* gfx_begin(int slot, uint32_t n)
{
    if (!s_inFrame || slot < 0) return NULL;
    if (slot != s_curSlot) { gfx_flush(); s_curSlot = slot; }
    if (s_vtxCount + n > s_vtxCap)
    {
        if (!s_overflowWarned) { s_overflowWarned = 1; printf("[cs2sx] frame vertex buffer full - drawing dropped\n"); }
        return NULL;
    }
    GfxVtx* v = &s_vtx[s_vtxCount];
    s_vtxCount += n;
    return v;
}

static inline void vtx(GfxVtx* v, float x, float y, float u, float w, u32 c)
{
    v->x = x; v->y = y; v->u = u; v->v = w; v->color = c;
}

static GfxVtx* solid(uint32_t n)
{
    return s_atlas ? gfx_begin(gfx_tex_slot(s_atlas), n) : NULL;
}

static void solid_tri(float x0, float y0, u32 c0, float x1, float y1, u32 c1, float x2, float y2, u32 c2)
{
    GfxVtx* v = solid(3);
    if (!v) return;
    vtx(v + 0, x0, y0, s_whiteU, s_whiteV, c0);
    vtx(v + 1, x1, y1, s_whiteU, s_whiteV, c1);
    vtx(v + 2, x2, y2, s_whiteU, s_whiteV, c2);
}

static void solid_rect(float x, float y, float w, float h, u32 c)
{
    if (w <= 0 || h <= 0) return;
    GfxVtx* v = solid(6);
    if (!v) return;
    vtx(v + 0, x,     y,     s_whiteU, s_whiteV, c);
    vtx(v + 1, x + w, y,     s_whiteU, s_whiteV, c);
    vtx(v + 2, x,     y + h, s_whiteU, s_whiteV, c);
    vtx(v + 3, x + w, y,     s_whiteU, s_whiteV, c);
    vtx(v + 4, x + w, y + h, s_whiteU, s_whiteV, c);
    vtx(v + 5, x,     y + h, s_whiteU, s_whiteV, c);
}

static void tex_quad(int slot, float x, float y, float w, float h,
                     float u0, float v0, float u1, float v1, u32 c)
{
    if (w <= 0 || h <= 0) return;
    GfxVtx* v = gfx_begin(slot, 6);
    if (!v) return;
    vtx(v + 0, x,     y,     u0, v0, c);
    vtx(v + 1, x + w, y,     u1, v0, c);
    vtx(v + 2, x,     y + h, u0, v1, c);
    vtx(v + 3, x + w, y,     u1, v0, c);
    vtx(v + 4, x + w, y + h, u1, v1, c);
    vtx(v + 5, x,     y + h, u0, v1, c);
}

// ── Anti-aliased paths ───────────────────────────────────────────────────────

#define MAX_PATH 1024
static float s_pathX[MAX_PATH], s_pathY[MAX_PATH], s_nX[MAX_PATH], s_nY[MAX_PATH];

// Per-vertex outward miter normals of a closed path (unit edge normals averaged,
// scaled so offsets keep a constant distance from both edges).
static void path_normals(int n)
{
    float area = 0;
    for (int i = 0; i < n; i++)
    {
        int j = (i + 1) % n;
        area += s_pathX[i] * s_pathY[j] - s_pathX[j] * s_pathY[i];
    }
    float sign = area > 0 ? 1.0f : -1.0f;   // y-down screen space

    for (int i = 0; i < n; i++)
    {
        int p = (i + n - 1) % n, q = (i + 1) % n;
        float e0x = s_pathX[i] - s_pathX[p], e0y = s_pathY[i] - s_pathY[p];
        float e1x = s_pathX[q] - s_pathX[i], e1y = s_pathY[q] - s_pathY[i];
        float l0 = sqrtf(e0x * e0x + e0y * e0y), l1 = sqrtf(e1x * e1x + e1y * e1y);
        if (l0 > 0) { e0x /= l0; e0y /= l0; }
        if (l1 > 0) { e1x /= l1; e1y /= l1; }
        float n0x = e0y * sign, n0y = -e0x * sign;
        float n1x = e1y * sign, n1y = -e1x * sign;
        float mx = (n0x + n1x) * 0.5f, my = (n0y + n1y) * 0.5f;
        float d2 = mx * mx + my * my;
        if (d2 > 0.000001f)
        {
            float s = 1.0f / d2;
            if (s > 4.0f) s = 4.0f;          // limit miter length on sharp corners
            mx *= s; my *= s;
        }
        s_nX[i] = mx; s_nY[i] = my;
    }
}

// Filled convex path with an anti-aliased outer fringe.
static void fill_convex_aa(int n, u32 color)
{
    if (n < 3) return;
    path_normals(n);
    float cx = 0, cy = 0;
    for (int i = 0; i < n; i++) { cx += s_pathX[i]; cy += s_pathY[i]; }
    cx /= (float)n; cy /= (float)n;

    float hf = s_px * 0.5f;
    u32 clear = transparent(color);
    for (int i = 0; i < n; i++)
    {
        int j = (i + 1) % n;
        float ix0 = s_pathX[i] - s_nX[i] * hf, iy0 = s_pathY[i] - s_nY[i] * hf;
        float ix1 = s_pathX[j] - s_nX[j] * hf, iy1 = s_pathY[j] - s_nY[j] * hf;
        float ox0 = s_pathX[i] + s_nX[i] * hf, oy0 = s_pathY[i] + s_nY[i] * hf;
        float ox1 = s_pathX[j] + s_nX[j] * hf, oy1 = s_pathY[j] + s_nY[j] * hf;
        solid_tri(cx, cy, color, ix0, iy0, color, ix1, iy1, color);
        solid_tri(ix0, iy0, color, ox0, oy0, clear, ox1, oy1, clear);
        solid_tri(ix0, iy0, color, ox1, oy1, clear, ix1, iy1, color);
    }
}

// One-physical-pixel anti-aliased outline of a closed path.
static void stroke_closed_aa(int n, u32 color)
{
    if (n < 2) return;
    path_normals(n);
    float core = s_px * 0.5f, fringe = s_px;
    u32 clear = transparent(color);
    for (int i = 0; i < n; i++)
    {
        int j = (i + 1) % n;
        float ax = s_pathX[i], ay = s_pathY[i], anx = s_nX[i], any = s_nY[i];
        float bx = s_pathX[j], by = s_pathY[j], bnx = s_nX[j], bny = s_nY[j];
        // rails: outer fringe, outer core, inner core, inner fringe
        float ra[4][2] = { { ax + anx * (core + fringe), ay + any * (core + fringe) },
                           { ax + anx * core,            ay + any * core },
                           { ax - anx * core,            ay - any * core },
                           { ax - anx * (core + fringe), ay - any * (core + fringe) } };
        float rb[4][2] = { { bx + bnx * (core + fringe), by + bny * (core + fringe) },
                           { bx + bnx * core,            by + bny * core },
                           { bx - bnx * core,            by - bny * core },
                           { bx - bnx * (core + fringe), by - bny * (core + fringe) } };
        u32 col[4] = { clear, color, color, clear };
        for (int k = 0; k < 3; k++)
        {
            solid_tri(ra[k][0], ra[k][1], col[k], rb[k][0], rb[k][1], col[k], rb[k + 1][0], rb[k + 1][1], col[k + 1]);
            solid_tri(ra[k][0], ra[k][1], col[k], rb[k + 1][0], rb[k + 1][1], col[k + 1], ra[k + 1][0], ra[k + 1][1], col[k + 1]);
        }
    }
}

// Segments for a curve of radius r (logical) so the chord error stays ~0.15 px.
static int curve_segments(float r)
{
    float rp = r / s_px;
    int n = (int)(sqrtf(rp > 0 ? rp : 0) * 6.0f);
    if (n < 12) n = 12;
    if (n > MAX_PATH - 8) n = MAX_PATH - 8;
    return n;
}

static int path_ellipse(float cx, float cy, float rx, float ry)
{
    int n = curve_segments(rx > ry ? rx : ry);
    for (int i = 0; i < n; i++)
    {
        float a = (float)i * 6.28318530718f / (float)n;
        s_pathX[i] = cx + cosf(a) * rx;
        s_pathY[i] = cy + sinf(a) * ry;
    }
    return n;
}

static int path_rounded_rect(float x, float y, float w, float h, float r)
{
    if (r > w * 0.5f) r = w * 0.5f;
    if (r > h * 0.5f) r = h * 0.5f;
    if (r < 0) r = 0;
    int seg = r > 0 ? curve_segments(r) / 4 + 1 : 1;
    int n = 0;
    float cxs[4] = { x + w - r, x + w - r, x + r, x + r };
    float cys[4] = { y + r,     y + h - r, y + h - r, y + r };
    float start[4] = { -1.5707963f, 0.0f, 1.5707963f, 3.1415926f };
    for (int c = 0; c < 4; c++)
        for (int i = 0; i <= seg && n < MAX_PATH; i++)
        {
            float a = start[c] + (float)i * 1.5707963f / (float)seg;
            s_pathX[n] = cxs[c] + cosf(a) * r;
            s_pathY[n] = cys[c] + sinf(a) * r;
            n++;
        }
    return n;
}

// ── Public drawing API ───────────────────────────────────────────────────────

void Graphics_FillScreen(u32 color)
{
    solid_rect(0, 0, (float)s_logW, (float)s_logH, color);
}

void Graphics_SetPixel(int x, int y, u32 color)
{
    solid_rect((float)x * s_px, (float)y * s_px, s_px, s_px, color);
}

void Graphics_SetPixelAlpha(int x, int y, u32 color, u8 alpha)
{
    if (alpha) Graphics_SetPixel(x, y, with_alpha(color, alpha));
}

void Graphics_FillRect(int x, int y, int w, int h, u32 color)
{
    solid_rect((float)x, (float)y, (float)w, (float)h, color);
}

void Graphics_FillRectAlpha(int x, int y, int w, int h, u32 color, u8 alpha)
{
    if (alpha) solid_rect((float)x, (float)y, (float)w, (float)h, with_alpha(color, alpha));
}

void Graphics_DrawRect(int x, int y, int w, int h, u32 color)
{
    if (w <= 0 || h <= 0) return;
    float fx = (float)x, fy = (float)y, fw = (float)w, fh = (float)h, p = s_px;
    solid_rect(fx, fy, fw, p, color);                     // top
    solid_rect(fx, fy + fh - p, fw, p, color);            // bottom
    solid_rect(fx, fy + p, p, fh - 2 * p, color);         // left
    solid_rect(fx + fw - p, fy + p, p, fh - 2 * p, color);// right
}

void Graphics_DrawLine(int x0, int y0, int x1, int y1, u32 color)
{
    float ax = (float)x0, ay = (float)y0, bx = (float)x1, by = (float)y1;
    float dx = bx - ax, dy = by - ay, len = sqrtf(dx * dx + dy * dy);
    if (len < 0.0001f) { Graphics_SetPixel((int)(ax / s_px), (int)(ay / s_px), color); return; }
    dx /= len; dy /= len;
    float h = s_px * 0.5f;                                // half width: 1 physical pixel total
    float nx = -dy * h, ny = dx * h, ex = dx * h, ey = dy * h;
    // a thin rectangle along the segment (extended by half a pixel at both ends)
    s_pathX[0] = ax - ex + nx; s_pathY[0] = ay - ey + ny;
    s_pathX[1] = bx + ex + nx; s_pathY[1] = by + ey + ny;
    s_pathX[2] = bx + ex - nx; s_pathY[2] = by + ey - ny;
    s_pathX[3] = ax - ex - nx; s_pathY[3] = ay - ey - ny;
    fill_convex_aa(4, color);
}

void Graphics_DrawCircle(int cx, int cy, int r, u32 color)
{
    if (r <= 0) return;
    stroke_closed_aa(path_ellipse((float)cx, (float)cy, (float)r, (float)r), color);
}

void Graphics_FillCircle(int cx, int cy, int r, u32 color)
{
    if (r <= 0) return;
    fill_convex_aa(path_ellipse((float)cx, (float)cy, (float)r, (float)r), color);
}

void Graphics_DrawEllipse(int cx, int cy, int rx, int ry, u32 color)
{
    if (rx <= 0 || ry <= 0) return;
    stroke_closed_aa(path_ellipse((float)cx, (float)cy, (float)rx, (float)ry), color);
}

void Graphics_FillEllipse(int cx, int cy, int rx, int ry, u32 color)
{
    if (rx <= 0 || ry <= 0) return;
    fill_convex_aa(path_ellipse((float)cx, (float)cy, (float)rx, (float)ry), color);
}

void Graphics_DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, u32 color)
{
    s_pathX[0] = (float)x0; s_pathY[0] = (float)y0;
    s_pathX[1] = (float)x1; s_pathY[1] = (float)y1;
    s_pathX[2] = (float)x2; s_pathY[2] = (float)y2;
    stroke_closed_aa(3, color);
}

void Graphics_FillTriangle(int x0, int y0, int x1, int y1, int x2, int y2, u32 color)
{
    s_pathX[0] = (float)x0; s_pathY[0] = (float)y0;
    s_pathX[1] = (float)x1; s_pathY[1] = (float)y1;
    s_pathX[2] = (float)x2; s_pathY[2] = (float)y2;
    fill_convex_aa(3, color);
}

void Graphics_DrawRoundedRect(int x, int y, int w, int h, int r, u32 color)
{
    if (w <= 0 || h <= 0) return;
    if (r <= 0) { Graphics_DrawRect(x, y, w, h, color); return; }
    float p = s_px * 0.5f;   // keep the outline inside the rectangle like DrawRect
    stroke_closed_aa(path_rounded_rect((float)x + p, (float)y + p, (float)w - 2 * p, (float)h - 2 * p, (float)r), color);
}

void Graphics_FillRoundedRect(int x, int y, int w, int h, int r, u32 color)
{
    if (w <= 0 || h <= 0) return;
    if (r <= 0) { Graphics_FillRect(x, y, w, h, color); return; }
    fill_convex_aa(path_rounded_rect((float)x, (float)y, (float)w, (float)h, (float)r), color);
}

void Graphics_DrawPolygon(int* xs, int* ys, int count, u32 color)
{
    if (!xs || !ys || count < 2) return;
    if (count > MAX_PATH) count = MAX_PATH;
    for (int i = 0; i < count; i++) { s_pathX[i] = (float)xs[i]; s_pathY[i] = (float)ys[i]; }
    stroke_closed_aa(count, color);
}

void Graphics_DrawGrid(int x, int y, int w, int h, int cellW, int cellH, u32 color)
{
    if (cellW <= 0 || cellH <= 0) return;
    for (int gx = x; gx <= x + w; gx += cellW) solid_rect((float)gx, (float)y, s_px, (float)h, color);
    for (int gy = y; gy <= y + h; gy += cellH) solid_rect((float)x, (float)gy, (float)w, s_px, color);
}

// ── Text (8×8 bitmap font, from the atlas) ───────────────────────────────────

void Graphics_DrawChar(int x, int y, char c, u32 color, int scale)
{
    if (!s_atlas || scale <= 0) return;
    if (c < 32 || c > 127) c = '?';
    int i = c - 32;
    float u0 = (float)((i % 16) * 8) / ATLAS_W, v0 = (float)((i / 16) * 8) / ATLAS_H;
    tex_quad(gfx_tex_slot(s_atlas), (float)x, (float)y, 8.0f * scale, 8.0f * scale,
             u0, v0, u0 + 8.0f / ATLAS_W, v0 + 8.0f / ATLAS_H, color);
}

void Graphics_DrawText(int x, int y, const char* text, u32 color, int scale)
{
    if (!text) return;
    int ox = x;
    for (int i = 0; text[i]; i++)
    {
        if (text[i] == '\n') { y += 8 * scale + 2; x = ox; continue; }
        if (text[i] != ' ') Graphics_DrawChar(x, y, text[i], color, scale);
        x += 8 * scale + 1;
    }
}

void Graphics_DrawTextAlpha(int x, int y, const char* text, u32 color, int scale, u8 alpha)
{
    if (alpha) Graphics_DrawText(x, y, text, with_alpha(color, alpha), scale);
}

void Graphics_DrawTextShadow(int x, int y, const char* text, u32 color, u32 shadow, int scale)
{
    Graphics_DrawText(x + scale, y + scale, text, shadow, scale);
    Graphics_DrawText(x, y, text, color, scale);
}

int Graphics_MeasureTextWidth(const char* text, int scale)
{
    if (!text) return 0;
    return (int)strlen(text) * (8 * scale + 1);
}

int Graphics_MeasureTextHeight(int scale) { return 8 * scale; }

// ── Textures ─────────────────────────────────────────────────────────────────

void Graphics_DrawTextureScaled(Texture* tex, int x, int y, int dw, int dh)
{
    if (!tex || dw <= 0 || dh <= 0) return;
    tex_quad(gfx_tex_slot(tex), (float)x, (float)y, (float)dw, (float)dh, 0, 0, 1, 1, 0xFFFFFFFFu);
}

void Graphics_DrawTexture(Texture* tex, int x, int y)
{
    if (tex) Graphics_DrawTextureScaled(tex, x, y, tex->f_Width, tex->f_Height);
}

void Graphics_DrawTextureCentered(Texture* tex, int rx, int ry, int rw, int rh)
{
    if (tex) Graphics_DrawTexture(tex, rx + (rw - tex->f_Width) / 2, ry + (rh - tex->f_Height) / 2);
}

void Graphics_DrawTextureCenteredScaled(Texture* tex, int rx, int ry, int rw, int rh, int tw, int th)
{
    if (tex) Graphics_DrawTextureScaled(tex, rx + (rw - tw) / 2, ry + (rh - th) / 2, tw, th);
}

void Graphics_BlitPhysical(Texture* tex, int x, int y)
{
    if (!tex) return;
    tex_quad(gfx_tex_slot(tex), (float)x * s_px, (float)y * s_px,
             (float)tex->f_Width * s_px, (float)tex->f_Height * s_px, 0, 0, 1, 1, 0xFFFFFFFFu);
}
