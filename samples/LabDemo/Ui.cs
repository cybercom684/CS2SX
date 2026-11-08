// Colors and drawing helpers shared by every page.

public static class Theme
{
    public static readonly uint Bg      = Color.RGB(16, 18, 26);
    public static readonly uint Header  = Color.RGB(24, 27, 38);
    public static readonly uint Card    = Color.RGB(32, 36, 50);
    public static readonly uint Track   = Color.RGB(48, 53, 72);
    public static readonly uint Text    = Color.RGB(236, 239, 247);
    public static readonly uint Muted   = Color.RGB(140, 148, 172);
    public static readonly uint Accent  = Color.RGB(92, 156, 255);
    public static readonly uint Good    = Color.RGB(72, 214, 140);
    public static readonly uint Warn    = Color.RGB(255, 190, 70);
    public static readonly uint Bad     = Color.RGB(255, 96, 96);
}

/// Text in the TrueType font when one is available, otherwise in the 8×8 font.
public class Ui
{
    private readonly Font? _font;

    public Ui(Font? font)
    {
        _font = font;
    }

    public string FontName()
    {
        return _font != null ? "TrueType" : "8x8 (keine Schrift gefunden)";
    }

    public void Text(int x, int y, string text, uint color, int size)
    {
        if (_font != null)
            _font.Draw(x, y, text, color, size);
        else
            Graphics.DrawText(x, y + size / 5, text, color, Scale(size));
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

    /// Rounded panel with a soft shadow and a small title.
    public void Card(int x, int y, int w, int h, string title)
    {
        Graphics.FillRoundedRect(x + 3, y + 6, w, h, 18, Color.RGBA(0, 0, 0, 90));
        Graphics.FillRoundedRect(x, y, w, h, 18, Theme.Card);
        if (title.Length > 0)
            Text(x + 22, y + 16, title, Theme.Muted, 20);
    }

    /// Horizontal progress bar, value 0..1.
    public void Bar(int x, int y, int w, int h, float value, uint color)
    {
        if (value < 0) value = 0;
        if (value > 1) value = 1;
        Graphics.FillRoundedRect(x, y, w, h, h / 2, Theme.Track);
        int fill = (int)(w * value);
        if (fill >= h)
            Graphics.FillRoundedRect(x, y, fill, h, h / 2, color);
    }

    private static int Scale(int size)
    {
        int s = size / 12;
        return s < 1 ? 1 : s;
    }
}

/// One screen of the app.
public abstract class Page
{
    public abstract void Frame(Ui ui, float dt);
}
