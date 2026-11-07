// ============================================================================
// Runtime module for cs2sx_font.h — TrueType text via FreeType.
// OPTIONAL: compiled and linked (with -lfreetype …) only when the app uses Font.
//
//   • glyphs are rasterized on first use, keyed by (code point, physical size),
//     into 1024×1024 RGBA atlas pages (white, coverage in alpha — the renderer
//     multiplies by the text color); a shelf packer fills a page row by row
//   • a draw first rasterizes every glyph of the text, uploads the changed atlas
//     regions (CS2SX_Gfx_UpdateTextureRect), then emits the quads, snapped to the
//     physical pixel grid so the nearest-filtered glyphs stay sharp
//   • when all pages are full the cache starts over (glyphs re-rasterize lazily)
// ============================================================================
#include "cs2sx_prelude.h"
#include <ft2build.h>
#include FT_FREETYPE_H

#define FONT_PAGE       1024
#define FONT_MAX_PAGES  4
#define FONT_MAX_SIZES  24

typedef struct
{
    u32 cp;          // code point (0 = empty slot)
    u16 size;        // physical pixel size
    u8  page;
    u8  _pad;
    u32 index;       // FreeType glyph index (kerning)
    s16 left, top;   // bitmap offset from the pen position / baseline
    u16 w, h;        // bitmap size
    u16 x, y;        // position in the atlas page
    s32 advance;     // 26.6
} Glyph;

typedef struct { int size; s32 ascender, height; } SizeMetrics;   // 26.6

typedef struct
{
    FT_Face  face;
    u8*      data;                  // file contents (FreeType reads from it); NULL = system font
    Texture* pages[FONT_MAX_PAGES];
    int      pageCount;
    int      penX, penY, rowH;      // shelf packer in the last page
    int      dirty[FONT_MAX_PAGES][4];   // x0, y0, x1, y1 — empty when x1 <= x0
    Glyph*   glyphs;
    int      glyphCap, glyphCount;
    SizeMetrics sizes[FONT_MAX_SIZES];
    int      sizeCount;
    int      curSize;               // size currently set on the face
} FontImpl;

static FT_Library s_ft;
static int s_ftReady;

static FT_Library ft(void)
{
    if (!s_ftReady)
    {
        if (FT_Init_FreeType(&s_ft)) CS2SX_ThrowIO("FreeType could not be initialized.");
        s_ftReady = 1;
    }
    return s_ft;
}

static Font* font_wrap(FT_Face face, u8* data)
{
    FontImpl* f = (FontImpl*)calloc(1, sizeof(FontImpl));
    Font* font = (Font*)calloc(1, sizeof(Font));
    if (!f || !font)
    {
        free(f); free(font);
        FT_Done_Face(face); free(data);
        CS2SX_ThrowNew(&CS2SX_TI_OutOfMemoryException, NULL);
    }
    f->face = face;
    f->data = data;
    f->glyphCap = 256;
    f->glyphs = (Glyph*)calloc((size_t)f->glyphCap, sizeof(Glyph));
    font->_rc = 1;
    font->_impl = f;
    return font;
}

// ── Loading ──────────────────────────────────────────────────────────────────

Font* Font_Load(const char* path)
{
    if (!path) CS2SX_ThrowArgNull("path");
    char p[1024];
    snprintf(p, sizeof p, strchr(path, ':') ? "%s" : "sdmc:%s", path);
    FILE* fp = fopen(p, "rb");
    if (!fp) CS2SX_ThrowFileNotFound(path);
    fseek(fp, 0, SEEK_END);
    long size = ftell(fp);
    fseek(fp, 0, SEEK_SET);
    u8* data = size > 0 ? (u8*)malloc((size_t)size) : NULL;
    if (!data || fread(data, 1, (size_t)size, fp) != (size_t)size)
    {
        fclose(fp); free(data);
        CS2SX_ThrowIO("Could not read the font file.");
    }
    fclose(fp);

    FT_Face face;
    if (FT_New_Memory_Face(ft(), data, (FT_Long)size, 0, &face))
    {
        free(data);
        CS2SX_ThrowIO("The file is not a supported font (TrueType/OpenType).");
    }
    return font_wrap(face, data);
}

Font* Font_LoadSystem(void)
{
    static int s_plReady;
    if (!s_plReady)
    {
        if (R_FAILED(plInitialize(PlServiceType_User))) return NULL;
        s_plReady = 1;
    }
    PlFontData fd;
    if (R_FAILED(plGetSharedFontByType(&fd, PlSharedFontType_Standard)) || !fd.address || !fd.size)
        return NULL;
    FT_Face face;
    if (FT_New_Memory_Face(ft(), (const FT_Byte*)fd.address, (FT_Long)fd.size, 0, &face))
        return NULL;
    return font_wrap(face, NULL);   // shared memory, owned by the system
}

void Font_Dispose(Font* self)
{
    if (!self) return;
    FontImpl* f = (FontImpl*)self->_impl;
    if (f)
    {
        for (int i = 0; i < f->pageCount; i++) Texture_Dispose(f->pages[i]);
        FT_Done_Face(f->face);
        free(f->data);
        free(f->glyphs);
        free(f);
    }
    free(self);
}

// ── Sizes ────────────────────────────────────────────────────────────────────

static int phys_size(int size)
{
    float px = CS2SX_Gfx_PhysicalPixel();          // logical units per physical pixel
    int p = (int)((float)size / (px > 0 ? px : 1.0f) + 0.5f);
    return p < 1 ? 1 : (p > 1000 ? 1000 : p);
}

static void set_size(FontImpl* f, int phys)
{
    if (f->curSize != phys)
    {
        FT_Set_Pixel_Sizes(f->face, 0, (FT_UInt)phys);
        f->curSize = phys;
    }
}

static const SizeMetrics* metrics(FontImpl* f, int phys)
{
    for (int i = 0; i < f->sizeCount; i++)
        if (f->sizes[i].size == phys) return &f->sizes[i];
    set_size(f, phys);
    SizeMetrics m = { phys, (s32)f->face->size->metrics.ascender, (s32)f->face->size->metrics.height };
    if (f->sizeCount == FONT_MAX_SIZES) f->sizeCount = 0;   // tiny cache, just recycle
    f->sizes[f->sizeCount] = m;
    return &f->sizes[f->sizeCount++];
}

// ── Glyph cache + atlas ──────────────────────────────────────────────────────

static u32 glyph_hash(u32 cp, int size) { return (cp * 2654435761u) ^ ((u32)size * 40503u); }

static void cache_reset(FontImpl* f)
{
    memset(f->glyphs, 0, (size_t)f->glyphCap * sizeof(Glyph));
    f->glyphCount = 0;
    for (int i = 0; i < f->pageCount; i++)
    {
        memset(f->pages[i]->f_Pixels, 0, (size_t)FONT_PAGE * FONT_PAGE * 4);
        f->dirty[i][0] = 0; f->dirty[i][1] = 0;
        f->dirty[i][2] = FONT_PAGE; f->dirty[i][3] = FONT_PAGE;
    }
    // Keep the first page; the others are released (their GPU copies once the
    // frame that may still sample them has finished — Texture_Dispose defers that).
    for (int i = 1; i < f->pageCount; i++) Texture_Dispose(f->pages[i]);
    f->pageCount = f->pageCount > 0 ? 1 : 0;
    f->penX = f->penY = f->rowH = 0;
}

static void cache_grow(FontImpl* f)
{
    int oldCap = f->glyphCap;
    Glyph* old = f->glyphs;
    Glyph* n = (Glyph*)calloc((size_t)oldCap * 2, sizeof(Glyph));
    if (!n) return;
    f->glyphs = n;
    f->glyphCap = oldCap * 2;
    for (int i = 0; i < oldCap; i++)
    {
        if (!old[i].cp) continue;
        u32 h = glyph_hash(old[i].cp, old[i].size) & (u32)(f->glyphCap - 1);
        while (f->glyphs[h].cp) h = (h + 1) & (u32)(f->glyphCap - 1);
        f->glyphs[h] = old[i];
    }
    free(old);
}

// Reserves a w×h atlas rectangle (1 px gap); returns the page or -1.
static int atlas_alloc(FontImpl* f, int w, int h, int* ox, int* oy)
{
    if (w > FONT_PAGE - 2 || h > FONT_PAGE - 2) return -1;
    for (int attempt = 0; attempt < 3; attempt++)
    {
        if (f->pageCount > 0)
        {
            if (f->penX + w + 1 > FONT_PAGE) { f->penX = 0; f->penY += f->rowH + 1; f->rowH = 0; }
            if (f->penY + h + 1 <= FONT_PAGE)
            {
                *ox = f->penX; *oy = f->penY;
                f->penX += w + 1;
                if (h > f->rowH) f->rowH = h;
                return f->pageCount - 1;
            }
        }
        if (f->pageCount < FONT_MAX_PAGES)
        {
            Texture* t = Texture_New(FONT_PAGE, FONT_PAGE, NULL);
            if (!t) return -1;
            f->pages[f->pageCount] = t;
            memset(f->dirty[f->pageCount], 0, sizeof f->dirty[0]);
            f->pageCount++;
            f->penX = f->penY = f->rowH = 0;
        }
        else
            return -2;   // full: the caller resets the cache
    }
    return -1;
}

static void mark_dirty(FontImpl* f, int page, int x, int y, int w, int h)
{
    int* d = f->dirty[page];
    if (d[2] <= d[0]) { d[0] = x; d[1] = y; d[2] = x + w; d[3] = y + h; return; }
    if (x < d[0]) d[0] = x;
    if (y < d[1]) d[1] = y;
    if (x + w > d[2]) d[2] = x + w;
    if (y + h > d[3]) d[3] = y + h;
}

static const Glyph* glyph(FontImpl* f, u32 cp, int phys)
{
    if (f->glyphCount * 10 >= f->glyphCap * 7) cache_grow(f);
    u32 h = glyph_hash(cp, phys) & (u32)(f->glyphCap - 1);
    while (f->glyphs[h].cp)
    {
        if (f->glyphs[h].cp == cp && f->glyphs[h].size == phys) return &f->glyphs[h];
        h = (h + 1) & (u32)(f->glyphCap - 1);
    }

    set_size(f, phys);
    Glyph g = { .cp = cp, .size = (u16)phys };
    g.index = FT_Get_Char_Index(f->face, cp);
    if (FT_Load_Glyph(f->face, g.index, FT_LOAD_RENDER | FT_LOAD_TARGET_LIGHT) == 0)
    {
        FT_GlyphSlot s = f->face->glyph;
        g.advance = (s32)s->advance.x;
        g.left = (s16)s->bitmap_left;
        g.top = (s16)s->bitmap_top;
        int bw = (int)s->bitmap.width, bh = (int)s->bitmap.rows;
        if (bw > 0 && bh > 0 && s->bitmap.pixel_mode == FT_PIXEL_MODE_GRAY)
        {
            int ax, ay;
            int page = atlas_alloc(f, bw, bh, &ax, &ay);
            if (page == -2)
            {
                cache_reset(f);   // every page is full — start over
                return glyph(f, cp, phys);
            }
            if (page >= 0)
            {
                u32* px = f->pages[page]->f_Pixels;
                for (int r = 0; r < bh; r++)
                {
                    const u8* src = s->bitmap.buffer + r * s->bitmap.pitch;
                    u32* dst = px + (size_t)(ay + r) * FONT_PAGE + ax;
                    for (int c = 0; c < bw; c++)
                        dst[c] = ((u32)src[c] << 24) | 0x00FFFFFFu;
                }
                mark_dirty(f, page, ax, ay, bw, bh);
                g.page = (u8)page; g.x = (u16)ax; g.y = (u16)ay;
                g.w = (u16)bw; g.h = (u16)bh;
            }
        }
    }
    f->glyphs[h] = g;
    f->glyphCount++;
    return &f->glyphs[h];
}

// ── Layout ───────────────────────────────────────────────────────────────────

// Next code point of UTF-8 text (invalid bytes → U+FFFD).
static u32 utf8_next(const char** p)
{
    const unsigned char* s = (const unsigned char*)*p;
    u32 c = s[0];
    int n = c < 0x80 ? 0 : (c & 0xE0) == 0xC0 ? 1 : (c & 0xF0) == 0xE0 ? 2 : (c & 0xF8) == 0xF0 ? 3 : -1;
    if (n < 0) { *p += 1; return 0xFFFD; }
    if (n > 0) c &= (0x3Fu >> n);
    for (int i = 1; i <= n; i++)
    {
        if ((s[i] & 0xC0) != 0x80) { *p += i; return 0xFFFD; }
        c = (c << 6) | (s[i] & 0x3F);
    }
    *p += n + 1;
    return c;
}

// Walks the text; for each visible glyph calls emit (when non-NULL) with the
// glyph's top-left in physical pixels. Returns the widest line in 26.6 physical.
static s64 layout(FontImpl* f, const char* text, int phys, float originX, float originY,
                  void (*emit)(FontImpl*, const Glyph*, float, float, void*), void* user)
{
    const SizeMetrics* m = metrics(f, phys);
    int kerning = FT_HAS_KERNING(f->face);
    s64 pen = 0, widest = 0;
    float baseline = originY + (float)m->ascender / 64.0f;
    u32 prevIndex = 0;
    for (const char* p = text; *p; )
    {
        u32 cp = utf8_next(&p);
        if (cp == '\n')
        {
            if (pen > widest) widest = pen;
            pen = 0;
            baseline += (float)m->height / 64.0f;
            prevIndex = 0;
            continue;
        }
        if (cp == '\r') continue;
        const Glyph* g = glyph(f, cp, phys);
        if (kerning && prevIndex && g->index)
        {
            FT_Vector k;
            set_size(f, phys);
            if (FT_Get_Kerning(f->face, prevIndex, g->index, FT_KERNING_DEFAULT, &k) == 0)
                pen += k.x;
        }
        if (emit && g->w && g->h)
            emit(f, g, floorf(originX + (float)pen / 64.0f + 0.5f) + g->left,
                 floorf(baseline + 0.5f) - g->top, user);
        pen += g->advance;
        prevIndex = g->index;
    }
    return pen > widest ? pen : widest;
}

typedef struct { u32 color; float px; } DrawCtx;

static void emit_quad(FontImpl* f, const Glyph* g, float x, float y, void* user)
{
    DrawCtx* c = (DrawCtx*)user;
    CS2SX_Gfx_TexQuad(f->pages[g->page], x * c->px, y * c->px, g->w * c->px, g->h * c->px,
                      g->x, g->y, g->x + g->w, g->y + g->h, c->color);
}

static FontImpl* impl(Font* self)
{
    if (!self) CS2SX_ThrowNullRef();
    return (FontImpl*)self->_impl;
}

void Font_Draw(Font* self, int x, int y, const char* text, u32 color, int size)
{
    FontImpl* f = impl(self);
    if (!text || !*text || size <= 0) return;
    int phys = phys_size(size);
    float px = CS2SX_Gfx_PhysicalPixel();
    float ox = floorf((float)x / px + 0.5f), oy = floorf((float)y / px + 0.5f);

    layout(f, text, phys, ox, oy, NULL, NULL);            // 1: rasterize missing glyphs
    for (int i = 0; i < f->pageCount; i++)                // 2: upload what changed
    {
        int* d = f->dirty[i];
        if (d[2] > d[0] && CS2SX_Gfx_UpdateTextureRect(f->pages[i], d[0], d[1], d[2] - d[0], d[3] - d[1]))
            d[2] = d[0];
    }
    DrawCtx c = { color, px };
    layout(f, text, phys, ox, oy, emit_quad, &c);         // 3: quads
}

int Font_Measure(Font* self, const char* text, int size)
{
    FontImpl* f = impl(self);
    if (!text || size <= 0) return 0;
    s64 w = layout(f, text, phys_size(size), 0, 0, NULL, NULL);
    return (int)ceilf((float)w / 64.0f * CS2SX_Gfx_PhysicalPixel());
}

int Font_LineHeight(Font* self, int size)
{
    FontImpl* f = impl(self);
    if (size <= 0) return 0;
    const SizeMetrics* m = metrics(f, phys_size(size));
    return (int)((float)m->height / 64.0f * CS2SX_Gfx_PhysicalPixel() + 0.5f);
}
