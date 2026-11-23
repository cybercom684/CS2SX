// Colors and text drawing.

public static class Theme
{
    public static readonly uint Text  = Color.RGB(236, 239, 247);
    public static readonly uint Muted = Color.RGB(140, 148, 172);
    public static readonly uint Card  = Color.RGB(26, 30, 46);
    public static readonly uint Track = Color.RGB(48, 53, 72);
    public static readonly uint Accent = Color.RGB(255, 190, 70);
    public static readonly uint Good  = Color.RGB(72, 214, 140);
    public static readonly uint Warn  = Color.RGB(255, 200, 80);
    public static readonly uint Bad   = Color.RGB(255, 96, 96);
}

/// Text in the TrueType font when one is available, otherwise in the 8×8 font.
public class Hud
{
    private readonly Font? _font;

    public Hud(Font? font)
    {
        _font = font;
    }

    public void Text(int x, int y, string text, uint color, int size)
    {
        if (_font != null) _font.Draw(x, y, text, color, size);
        else Graphics.DrawText(x, y + size / 5, text, color, Scale(size));
    }

    public int Width(string text, int size)
    {
        if (_font != null) return _font.Measure(text, size);
        return Graphics.MeasureTextWidth(text, Scale(size));
    }

    public void TextCentered(int cx, int y, string text, uint color, int size)
    {
        Text(cx - Width(text, size) / 2, y, text, color, size);
    }

    public void TextRight(int right, int y, string text, uint color, int size)
    {
        Text(right - Width(text, size), y, text, color, size);
    }

    private static int Scale(int size)
    {
        int s = size / 12;
        return s < 1 ? 1 : s;
    }
}
