// The game's pixel art, painted in code: no image files needed.
// White parts are tinted when drawn (each player gets their own color).

public static class Sprites
{
    public const int DroneSize = 12, DroneFrames = 4;
    public const int SparkSize = 8, SparkFrames = 2;
    public const int RockSize = 16;

    public static Texture? Drones;   // 4 frames side by side: spinning rotors
    public static Texture? Sparks;   // 2 frames: + and ×
    public static Texture? Rock;

    private static readonly uint Clear = Color.RGBA(0, 0, 0, 0);

    public static void Build()
    {
        Drones = BuildDrones();
        Sparks = BuildSparks();
        Rock = BuildRock();
    }

    private static void Put(uint[] px, int stride, int x, int y, uint c)
    {
        if (x >= 0 && y >= 0 && x < stride && y * stride + x < px.Length) px[y * stride + x] = c;
    }

    private static Texture BuildDrones()
    {
        int w = DroneSize * DroneFrames, h = DroneSize;
        var px = new uint[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = Clear;
        uint body = Color.RGB(255, 255, 255);
        uint shade = Color.RGB(170, 170, 170);
        uint dark = Color.RGB(40, 44, 60);
        uint eye = Color.RGB(120, 230, 255);
        uint rotor = Color.RGB(210, 210, 220);

        for (int f = 0; f < DroneFrames; f++)
        {
            int ox = f * DroneSize;
            // round body with an outline and a lower shade
            for (int y = 0; y < h; y++)
                for (int x = 0; x < DroneSize; x++)
                {
                    int dx = x * 2 - 11, dy = y * 2 - 11;
                    int d = dx * dx + dy * dy;
                    if (d <= 36) Put(px, w, ox + x, y, dy > 2 ? shade : body);
                    else if (d <= 56) Put(px, w, ox + x, y, dark);
                }
            Put(px, w, ox + 6, 5, eye);
            Put(px, w, ox + 7, 5, eye);
            // four rotors in the corners, turning from frame to frame
            for (int c = 0; c < 4; c++)
            {
                int cx = ox + (c % 2 == 0 ? 1 : 10), cy = c < 2 ? 1 : 10;
                if (f % 2 == 0) { Put(px, w, cx - 1, cy, rotor); Put(px, w, cx + 1, cy, rotor); }
                else { Put(px, w, cx, cy - 1, rotor); Put(px, w, cx, cy + 1, rotor); }
                Put(px, w, cx, cy, dark);
            }
        }
        return new Texture(w, h, px);
    }

    private static Texture BuildSparks()
    {
        int w = SparkSize * SparkFrames, h = SparkSize;
        var px = new uint[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = Clear;
        uint core = Color.RGB(255, 255, 255);
        uint glow = Color.RGB(200, 200, 200);
        for (int i = 1; i < 7; i++)
        {
            Put(px, w, 3, i, i == 3 || i == 4 ? core : glow);          // frame 0: +
            Put(px, w, 4, i, i == 3 || i == 4 ? core : glow);
            Put(px, w, i, 3, glow); Put(px, w, i, 4, glow);
            Put(px, w, 8 + i, i, glow);                                 // frame 1: ×
            Put(px, w, 8 + 7 - i, i, glow);
        }
        Put(px, w, 8 + 3, 3, core); Put(px, w, 8 + 4, 4, core);
        return new Texture(w, h, px);
    }

    private static Texture BuildRock()
    {
        int s = RockSize;
        var px = new uint[s * s];
        uint light = Color.RGB(150, 120, 100);
        uint mid = Color.RGB(110, 86, 72);
        uint dark = Color.RGB(70, 54, 46);
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                int dx = x * 2 - 15, dy = y * 2 - 15;
                // a bumpy edge: the radius changes with the direction
                int r = 196 + ((x * 7 + y * 13) % 5) * 12;
                int d = dx * dx + dy * dy;
                px[y * s + x] = d > r ? Clear : dx + dy < -6 ? light : mid;
            }
        // craters
        int[] cx = { 5, 10, 8 }, cy = { 6, 5, 11 };
        for (int i = 0; i < 3; i++)
        {
            Put(px, s, cx[i], cy[i], dark); Put(px, s, cx[i] + 1, cy[i], dark);
            Put(px, s, cx[i], cy[i] + 1, dark); Put(px, s, cx[i] + 1, cy[i] + 1, light);
        }
        return new Texture(s, s, px);
    }
}
