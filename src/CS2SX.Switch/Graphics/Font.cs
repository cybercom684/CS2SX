using System;

namespace CS2SX.Switch;

/// <summary>
/// A TrueType/OpenType font for <see cref="Graphics"/> mode. Glyphs are rendered at the
/// output resolution, so text stays sharp at any size and in 1080p. Sizes are pixel
/// heights in the same logical units as every other coordinate.
/// </summary>
/// <example>
/// <code>
/// _font = Font.Load("romfs:/fonts/Inter.ttf");      // or Font.LoadSystem()
/// _font.Draw(40, 30, "Grüße aus C#!", Color.White, 32);
/// int w = _font.Measure("Score: 120", 24);
/// </code>
/// </example>
public sealed class Font : IDisposable
{
    private Font() { }

    /// <summary>
    /// Loads a .ttf or .otf file — put it in the project's romfs/ folder and load it from
    /// "romfs:/…". Throws FileNotFoundException when it is missing and IOException when it
    /// is not a supported font.
    /// </summary>
    public static Font Load(string path) => new Font();

    /// <summary>
    /// The Switch system font (the standard font of the console UI; covers Latin, Greek,
    /// Cyrillic and Japanese kana). Returns null where it is unavailable, e.g. in an
    /// emulator without installed firmware.
    /// </summary>
    public static Font? LoadSystem() => null;

    /// <summary>Draws text with its top-left corner at (x, y). Supports UTF-8 and '\n'.</summary>
    public void Draw(int x, int y, string text, uint color, int size) { }

    /// <summary>Width of the widest line of <paramref name="text"/> at <paramref name="size"/>.</summary>
    public int Measure(string text, int size) => 0;

    /// <summary>Distance between the baselines of two lines at <paramref name="size"/>.</summary>
    public int LineHeight(int size) => 0;

    /// <summary>Releases the font and its glyph cache. Safe to call more than once.</summary>
    public void Dispose() { }
}
