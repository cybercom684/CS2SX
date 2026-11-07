#pragma once
// ============================================================================
// cs2sx_font.h — TrueType/OpenType text on the GPU renderer. Implementation:
// src/rt_font.c (FreeType). The module and FreeType are linked only into apps that
// use Font — the build adds them when generated code calls Font_* (BuildPipeline).
//
// Glyphs are rasterized on first use at the PHYSICAL output size (sharp at 720p
// and 1080p) into 1024² atlas pages and drawn as textured quads on the physical
// pixel grid. Sizes are pixel heights in logical units, like every coordinate.
// Text is UTF-8; '\n' starts a new line. Kerning from the font's kern table.
// ============================================================================

typedef struct Font Font;
struct Font
{
    int   _rc;
    void* _impl;   // FontImpl (rt_font.c)
};

/// A .ttf/.otf file (e.g. "romfs:/fonts/Inter.ttf"). FileNotFoundException when
/// missing, IOException when the file is not a font.
Font* Font_Load(const char* path);
/// The Switch system font (Nintendo standard font), NULL where it is unavailable
/// (emulators without firmware).
Font* Font_LoadSystem(void);
/// Draws text with its top-left at (x, y); size = pixel height of the font.
void  Font_Draw(Font* self, int x, int y, const char* text, u32 color, int size);
/// Width of the widest line of text at size (logical pixels).
int   Font_Measure(Font* self, const char* text, int size);
/// Distance between two lines of text at size (logical pixels).
int   Font_LineHeight(Font* self, int size);
void  Font_Dispose(Font* self);
