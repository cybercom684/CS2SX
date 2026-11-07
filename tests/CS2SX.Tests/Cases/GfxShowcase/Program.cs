// Visual check of every Graphics API function on the GPU renderer.
// Each cell shows one function with its name; compare against the expectations
// in the label. A: toggle 1080p output (restart needed) · Plus: exit.
public class GfxShowcaseApp : SwitchApp
{
    private Texture? _checker;
    private Texture? _gradient;
    private int _frame;

    public override void OnInit()
    {
        Graphics.Init(1280, 720);

        // Procedural textures (no files needed): checkerboard + alpha gradient.
        var px = new uint[32 * 32];
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
                px[y * 32 + x] = ((x / 8 + y / 8) % 2 == 0) ? Color.White : Color.Red;
        _checker = new Texture(32, 32, px);

        var gp = new uint[64 * 16];
        for (int y = 0; y < 16; y++)
            for (int x = 0; x < 64; x++)
                gp[y * 64 + x] = Color.Cyan.WithAlpha((byte)(x * 4));
        _gradient = new Texture(64, 16, gp);
    }

    private static void Label(int x, int y, string text)
    {
        Graphics.DrawText(x, y, text, Color.LightGray, 1);
    }

    public override void OnFrame()
    {
        _frame++;
        Graphics.FillScreen(Color.RGB(24, 26, 32));
        Graphics.DrawText(20, 12, "CS2SX GPU renderer", Color.White, 3);
        Graphics.DrawTextShadow(560, 16, "shadow text", Color.Yellow, Color.Black, 2);

        // Row 1: rectangles
        Graphics.FillRect(20, 70, 120, 70, Color.Blue);                 Label(20, 146, "FillRect");
        Graphics.DrawRect(160, 70, 120, 70, Color.White);               Label(160, 146, "DrawRect 1px");
        Graphics.FillRoundedRect(300, 70, 120, 70, 18, Color.Green);    Label(300, 146, "FillRoundedRect");
        Graphics.DrawRoundedRect(440, 70, 120, 70, 18, Color.White);    Label(440, 146, "DrawRoundedRect");
        Graphics.FillRect(580, 70, 120, 70, Color.Red);
        Graphics.FillRectAlpha(610, 90, 120, 70, Color.White, 128);    Label(580, 166, "FillRectAlpha 50%");
        Graphics.DrawGrid(760, 70, 120, 70, 20, 14, Color.Gray);       Label(760, 146, "DrawGrid");

        // Row 2: round shapes (edges must be smooth, not stair-stepped)
        Graphics.FillCircle(80, 240, 45, Color.Orange);                 Label(20, 292, "FillCircle");
        Graphics.DrawCircle(220, 240, 45, Color.White);                 Label(160, 292, "DrawCircle");
        Graphics.FillEllipse(360, 240, 60, 30, Color.Purple);           Label(300, 292, "FillEllipse");
        Graphics.DrawEllipse(500, 240, 60, 30, Color.White);            Label(440, 292, "DrawEllipse");
        Graphics.FillTriangle(600, 285, 650, 195, 700, 285, Color.Lime); Label(580, 292, "FillTriangle");
        Graphics.DrawTriangle(740, 285, 790, 195, 840, 285, Color.White); Label(720, 292, "DrawTriangle");
        int[] xs = { 900, 960, 990, 950, 890 };
        int[] ys = { 200, 190, 240, 285, 260 };
        Graphics.DrawPolygon(xs, ys, 5, Color.Yellow);                  Label(880, 292, "DrawPolygon");

        // Row 3: lines at many angles (smooth diagonals)
        for (int i = 0; i < 12; i++)
        {
            int dx = (i * 23) % 120, dy = 110 - (i * 9);
            Graphics.DrawLine(20, 330, 20 + dx + 10, 330 + dy, Color.RGB((byte)(40 + i * 18), 200, 255));
        }
        Label(20, 452, "DrawLine (fan)");

        // Row 3: textures
        if (_checker != null)
        {
            Graphics.DrawTexture(_checker, 200, 340);                   Label(200, 380, "DrawTexture");
            Graphics.DrawTextureScaled(_checker, 300, 330, 96, 96);     Label(300, 432, "Scaled 3x (crisp)");
            Graphics.BlitPhysical(_checker, 600, 500);
        }
        if (_gradient != null)
        {
            Graphics.FillRect(440, 340, 140, 40, Color.Red);
            Graphics.DrawTextureScaled(_gradient, 440, 340, 140, 40);   Label(440, 386, "texture alpha ramp");
        }

        // Row 4: text sizes, alpha text, physical pixels
        Graphics.DrawText(20, 480, "Scale 1: The quick brown fox 0123456789", Color.White, 1);
        Graphics.DrawText(20, 500, "Scale 2: ABC abc !?#", Color.White, 2);
        Graphics.DrawTextAlpha(20, 530, "Alpha text 40%", Color.White, 2, 100);
        for (int i = 0; i < 40; i++) Graphics.SetPixel(700 + i * 2, 520, Color.White);
        Label(700, 528, "SetPixel (physical)");

        // Animation: must move smoothly at 60 fps
        int bx = 20 + (_frame * 4) % 1180;
        Graphics.FillRoundedRect(bx, 640, 80, 40, 12, Color.Cyan);
        Graphics.DrawText(20, 696, "frame " + _frame, Color.Gray, 1);
    }

    public override void OnExit()
    {
        _checker?.Dispose();
        _gradient?.Dispose();
    }
}
