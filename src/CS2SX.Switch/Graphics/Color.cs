namespace CS2SX.Switch;

/// <summary>
/// 32-bit colors in the framebuffer's RGBA8888 layout (as a little-endian uint:
/// 0xAABBGGRR). Use the named constants or <see cref="RGB"/> / <see cref="RGBA"/>.
/// </summary>
public static class Color
{
    public const uint Black     = 0xFF000000;
    public const uint White     = 0xFFFFFFFF;
    public const uint Red       = 0xFF0000FF;
    public const uint Green     = 0xFF00C800;
    public const uint Blue      = 0xFFFF0000;
    public const uint Yellow    = 0xFF00FFFF;
    public const uint Cyan      = 0xFFFFFF00;
    public const uint Magenta   = 0xFFFF00FF;
    public const uint Gray      = 0xFF808080;
    public const uint DarkGray  = 0xFF404040;
    public const uint LightGray = 0xFFC0C0C0;
    public const uint Orange    = 0xFF00A5FF;
    public const uint Pink      = 0xFFB469FF;
    public const uint Purple    = 0xFF800080;
    public const uint Brown     = 0xFF13458B;
    public const uint Teal      = 0xFF808000;
    public const uint Lime      = 0xFF00FF00;
    public const uint Navy      = 0xFF800000;
    public const uint Silver    = 0xFFC0C0C0;
    public const uint Maroon    = 0xFF000080;
    public const uint Olive     = 0xFF008080;

    public static uint RGBA(byte r, byte g, byte b, byte a) => (uint)((a << 24) | (b << 16) | (g << 8) | r);
    public static uint RGB(byte r, byte g, byte b) => RGBA(r, g, b, 255);
}

public static class ColorExtensions
{
    /// <summary>The same color with a different alpha (0 = transparent, 255 = opaque).</summary>
    public static uint WithAlpha(this uint color, byte alpha) => (color & 0x00FFFFFFu) | ((uint)alpha << 24);
}
