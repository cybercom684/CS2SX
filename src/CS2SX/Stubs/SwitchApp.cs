// CS2SX Stub — wird nicht transpiliert, nur für IntelliSense

public class Form
{
    public void Add(Control control)
    {
    }
}

public abstract class Control
{
    public int X
    {
        get; set;
    }
    public int Y
    {
        get; set;
    }
    public int Width
    {
        get; set;
    }
    public int Height
    {
        get; set;
    }
    public bool Visible { get; set; } = true;
}

public abstract class SwitchApp
{
    public Form Form { get; } = new Form();
    public virtual void OnInit()
    {
    }
    public virtual void OnFrame()
    {
    }
    public virtual void OnExit()
    {
    }
}

public class Label : Control
{
    public string Text { get; set; } = "";
    public Label(string text) => Text = text;
}

public class Button : Control
{
    public string Text { get; set; } = "";
    public bool Focused
    {
        get; set;
    }
    public Action OnClick { get; set; } = () => { };
    public Button(string text) => Text = text;
}

public class ProgressBar : Control
{
    public int value
    {
        get; set;
    }
    public int width_chars
    {
        get; set;
    }
    public ProgressBar(int widthChars) => width_chars = widthChars;
}

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
    public static Texture LoadTexture(string path) => null;
    /// Decodes a PNG/JPEG/BMP/GIF/TGA image file into a Texture (null on failure).
    public static Texture LoadImage(string path) => null;
    /// Decodes + downscales an image to a thumbnail Texture fitting maxSize (null on failure).
    public static Texture LoadImageThumb(string path, int maxSize) => null;
    /// Bilinear-resamples a texture to dw×dh into a NEW Texture (smooth scaling).
    public static Texture ScaleTextureSmooth(Texture src, int dw, int dh) => null;
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
}
public static class Color
{
    public static uint Black = 0xFF000000;
    public static uint White = 0xFFFFFFFF;
    public static uint Red = 0xFF0000FF;
    public static uint Green = 0xFF00C800;
    public static uint Blue = 0xFFFF0000;
    public static uint Yellow = 0xFF00FFFF;
    public static uint Cyan = 0xFFFFFF00;
    public static uint Magenta = 0xFFFF00FF;
    public static uint Gray = 0xFF808080;
    public static uint Orange = 0xFF00A5FF;

    public static uint RGBA(byte r, byte g, byte b, byte a)
        => (uint)((a << 24) | (b << 16) | (g << 8) | r);

    public static uint RGB(byte r, byte g, byte b)
        => RGBA(r, g, b, 255);
}

public class Texture : IDisposable
{
    public int Width
    {
        get;
    }
    public int Height
    {
        get;
    }
    public uint[] Pixels
    {
        get;
    }

    public Texture(int width, int height, uint[] pixels)
    {
        Width = width;
        Height = height;
        Pixels = pixels;
    }

    public void Dispose()
    {
        // Wird zu Texture_Dispose(self) übersetzt
    }
}

// Auto-generated by CS2SX — do not edit manually
// Diese Stubs erlauben dem C#-Compiler CS2SX-Projekte zu validieren.
// Zur Laufzeit werden sie vom Transpiler zu libnx-C-Code übersetzt.

namespace CS2SX.Switch
{


    // ── File I/O ──────────────────────────────────────────────────────────────

    public static class File
    {
        public static string ReadAllText(string path) => "";
        public static void WriteAllText(string path, string content)
        {
        }
        public static void AppendAllText(string path, string content)
        {
        }
        public static bool Exists(string path) => false;
        public static void Delete(string path)
        {
        }
        public static void Copy(string src, string dst)
        {
        }
        /// Binary-safe copy (chunked) — preserves images, NRO, archives byte-for-byte.
        public static bool CopyBinary(string src, string dst) => false;
        /// File size in bytes (-1 if missing/unreadable).
        public static long GetSize(string path) => -1;
        /// Last-modified time as a POSIX timestamp (0 if unavailable) — for sorting.
        public static long GetModifiedTime(string path) => 0;
        /// Last-modified time formatted "YYYY-MM-DD HH:MM" ("" if unavailable).
        public static string GetModifiedString(string path) => "";
        /// Formatted hex dump (offset · hex · ascii) of up to maxBytes; handles binary.
        public static string ReadHexDump(string path, int maxBytes) => "";
        /// Begins a chunked copy (for a progress bar). Returns total bytes, or -1 on error.
        public static long CopyBegin(string src, string dst) => -1;
        /// Copies up to chunkBytes more; returns bytes done so far (==total when finished), -1 on error.
        public static long CopyStep(int chunkBytes) => -1;
        /// Closes the in-flight chunked copy.
        public static void CopyEnd() { }
        /// Renames/moves a file within the SD card (no data copy).
        public static bool Rename(string src, string dst) => false;
    }

    public static class Directory
    {
        public static bool Exists(string path) => false;
        public static void CreateDirectory(string path)
        {
        }
        public static void Delete(string path)
        {
        }
        /// Recursively deletes a directory and all its contents.
        public static bool DeleteRecursive(string path) => false;
        /// Renames/moves a directory within the SD card.
        public static bool Rename(string src, string dst) => false;
        public static string[] GetFiles(string path) => System.Array.Empty<string>();
        public static string[] GetFiles(string path, string pattern) => System.Array.Empty<string>();
        public static string GetCurrentDirectory() => "/switch";
        public static System.Collections.Generic.IEnumerable<string>
                               EnumerateFiles(string path) => System.Array.Empty<string>();
    public static System.Collections.Generic.List<string> GetDirectories(string path)
        => new System.Collections.Generic.List<string>();
    public static System.Collections.Generic.List<string> GetEntries(string path)
        => new System.Collections.Generic.List<string>();
    }

    public static class Path
    {
        public static string Combine(string a, string b) => a + "/" + b;
        public static string GetFileName(string path) => path;
        public static string GetExtension(string path) => "";
        public static string GetDirectoryName(string path) => path;
        public static bool IsDirectory(string path) => false;
    }

    /// ZIP archive extraction / creation (requires the miniz extern-lib).
    public static class Archive
    {
        /// Extracts a .zip into destDir. Returns #files extracted, or -1 on error.
        public static int Extract(string zipPath, string destDir) => -1;
        /// Compresses a file or folder (isDir) into zipPath. Returns #files, or -1.
        public static int Compress(string srcPath, bool isDir, string zipPath) => -1;

        /// Begins a frame-stepped extract. Returns total entries (>=0), or -1.
        public static int BeginExtract(string zipPath, string destDir) => -1;
        /// Begins a frame-stepped multi-item compress. list = lines "<isDir>\t<path>\n". Returns total files, or -1.
        public static int BeginCompress(string list, string zipPath) => -1;
        /// Processes one entry. Returns 1 while more remains, 0 when finished.
        public static int Step() => 0;
        /// Processes entries until ~maxBytes handled (or done). Returns 1 more, 0 done.
        public static int StepBudget(int maxBytes) => 0;
        /// True while a stepped archive op is running.
        public static bool Busy() => false;
        /// Result after Busy() is false: #files, -1 err, -2 cancelled.
        public static int Result() => -1;
        /// Entries/files processed so far (for a progress bar).
        public static int Progress() => 0;
        /// Total entries/files (0 = unknown).
        public static int Total() => 0;
        /// Requests cancellation of the running background op.
        public static void Cancel() { }
    }

    /// SD-card storage information.
    public static class Filesystem
    {
        /// Free space on the SD card in bytes (-1 if unavailable).
        public static long GetFreeSpace() => -1;
        /// Total capacity of the SD card in bytes (-1 if unavailable).
        public static long GetTotalSpace() => -1;
        /// Recursively sums all file sizes under a directory (fast; -1 on error).
        public static long DirSize(string path) => -1;
    }
}


// Parse-Stubs — werden vom Transpiler zu CS2SX_Int_Parse etc. übersetzt

public static class IntParser
{
    // Stubs damit der C#-Compiler int.Parse akzeptiert —
    // int und float sind Schlüsselwörter und können keine statischen Methoden haben,
    // daher sind diese Stubs nur für die Transpiler-Erkennung nötig.
    // Der Transpiler erkennt "int.Parse" direkt als calleeStr.
}

// ── System ────────────────────────────────────────────────────────────────────

public struct BatteryInfo
{
    public int percent;
    public bool charging;
    public bool connected;
}

public struct TimeInfo
{
    public int hour;
    public int minute;
    public int second;
}

// NX class instead of System to avoid shadowing the BCL System namespace
public static class NX
{
    public static BatteryInfo GetBattery() => new BatteryInfo();
    public static TimeInfo GetTime() => new TimeInfo();
}

// ── SwitchAppEx ───────────────────────────────────────────────────────────────

public abstract class SwitchAppEx : SwitchApp
{
    public StickPos _stickL;
    public StickPos _stickR;
    public TouchState _touch;
    public BatteryInfo _battery;
}