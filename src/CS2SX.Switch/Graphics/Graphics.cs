namespace CS2SX.Switch;

/// <summary>
/// Immediate-mode 2D drawing in logical screen coordinates (default 1280x720).
/// Call <see cref="BeginFrame"/> / <see cref="EndFrame"/> around the drawing of each frame.
/// </summary>
public static class Graphics
{
    // Initialisierung
    public static void Init(int width, int height)
    {
    }

    /// Rendert in der angegebenen Ausgabeauflösung (z.B. 1920x1080) statt in der
    /// logischen Designgröße. Im Dock-Modus 1:1 scharf. VOR Init() aufrufen.
    public static void SetOutputResolution(int width, int height)
    {
    }

    /// Skalierungs-Zähler/Nenner (physisch = logisch * Num / Den) für eigene
    /// Renderer wie den FreeType-Text.
    public static int GetScaleNum() => 1;
    public static int GetScaleDen() => 1;

    // Frame
    public static void BeginFrame()
    {
    }
    public static void EndFrame()
    {
    }

    // Grundprimitiven
    public static void FillScreen(uint color)
    {
    }
    public static void SetPixel(int x, int y, uint color)
    {
    }
    public static void DrawRect(int x, int y, int w, int h, uint color)
    {
    }
    public static void FillRect(int x, int y, int w, int h, uint color)
    {
    }
    public static void DrawLine(int x0, int y0, int x1, int y1, uint color)
    {
    }
    public static void DrawCircle(int cx, int cy, int r, uint color)
    {
    }
    public static void FillCircle(int cx, int cy, int r, uint color)
    {
    }

    // Text
    public static void DrawText(int x, int y, string text, uint color, int scale)
    {
    }
    public static void DrawChar(int x, int y, char c, uint color, int scale)
    {
    }
    public static int MeasureTextWidth(string text, int scale) => 0;
    public static int MeasureTextHeight(int scale) => 0;

    // Textures
    public static void DrawTexture(Texture tex, int x, int y) { }
    public static Texture? LoadTexture(string path) => null;
    /// Decodes a PNG/JPEG/BMP/GIF/TGA image file into a Texture (null on failure).
    public static Texture? LoadImage(string path) => null;
    /// Decodes + downscales an image to a thumbnail Texture fitting maxSize (null on failure).
    public static Texture? LoadImageThumb(string path, int maxSize) => null;
    /// Bilinear-resamples a texture to dw×dh into a NEW Texture (smooth scaling).
    public static Texture? ScaleTextureSmooth(Texture src, int dw, int dh) => null;
    /// Blits a texture 1:1 at PHYSICAL pixel coordinates (no logical scaling).
    public static void BlitPhysical(Texture tex, int x, int y) { }
    public static void DrawTextureCentered(Texture tex, int rx, int ry, int rw, int rh) { }
    public static void DrawTextureScaled(Texture tex, int x, int y, int tw, int th) { }
    public static void DrawTextureCenteredScaled(Texture tex, int rx, int ry, int rw, int rh, int tw, int th) { }

    // Erweiterte Primitiven
    public static void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, uint color) { }
    public static void FillTriangle(int x0, int y0, int x1, int y1, int x2, int y2, uint color) { }
    public static void DrawEllipse(int cx, int cy, int rx, int ry, uint color) { }
    public static void FillEllipse(int cx, int cy, int rx, int ry, uint color) { }
    public static void DrawRoundedRect(int x, int y, int w, int h, int r, uint color) { }
    public static void FillRoundedRect(int x, int y, int w, int h, int r, uint color) { }
    public static void SetPixelAlpha(int x, int y, uint color, byte alpha) { }
    public static void FillRectAlpha(int x, int y, int w, int h, uint color, byte alpha) { }
    public static void DrawTextAlpha(int x, int y, string text, uint color, int scale, byte alpha) { }
    public static void DrawTextShadow(int x, int y, string text, uint color, uint shadow, int scale) { }
    public static void DrawGrid(int x, int y, int w, int h, int cellW, int cellH, uint color) { }
    /// <summary>Filled polygon through the points (xs[i], ys[i]) for i &lt; count.</summary>
    public static void DrawPolygon(int[] xs, int[] ys, int count, uint color) { }
}
