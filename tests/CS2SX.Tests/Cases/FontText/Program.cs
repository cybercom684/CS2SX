// Visual check of TrueType text (Font). Uses romfs:/font.ttf when the app ships one,
// otherwise the Switch system font (Ryujinx without firmware cannot provide that).
public class FontTextApp : SwitchApp
{
    private Font? _font;
    private int _frame;

    public override void OnInit()
    {
        Graphics.Init(1280, 720);
        _font = File.Exists("romfs:/font.ttf") ? Font.Load("romfs:/font.ttf") : Font.LoadSystem();
    }

    public override void OnFrame()
    {
        _frame++;
        Graphics.FillScreen(Color.RGB(24, 26, 32));
        if (_font == null)
        {
            Graphics.DrawText(20, 20, "No font: system font unavailable, no romfs:/font.ttf", Color.Red, 2);
            return;
        }

        // sizes
        int y = 16;
        int[] sizes = { 12, 16, 24, 32, 48, 72 };
        foreach (int size in sizes)
        {
            _font.Draw(20, y, size + " px - The quick brown fox jumps", Color.White, size);
            y += _font.LineHeight(size);
        }

        // Unicode, measured background, colors and alpha
        string greet = "Grüße, Ünïcödé: äöü ÄÖÜ ß € ©";
        int w = _font.Measure(greet, 32);
        Graphics.FillRect(20, y + 4, w, _font.LineHeight(32), Color.RGB(60, 70, 120));
        _font.Draw(20, y + 4, greet, Color.Yellow, 32);
        y += _font.LineHeight(32) + 12;

        _font.Draw(20, y, "Multi-line text:\nsecond line\nthird line", Color.Cyan, 24);
        _font.Draw(700, y, "50% alpha text", Color.White.WithAlpha(128), 40);
        y += _font.LineHeight(24) * 3 + 8;

        // changing text every frame (glyph cache) and kerning pairs
        _font.Draw(20, y, "Frame " + _frame + "  AV To Wa", Color.Lime, 36);
        _font.Draw(1000, 680, "w=" + w, Color.Gray, 16);
    }

    public override void OnExit()
    {
        _font?.Dispose();
    }
}
