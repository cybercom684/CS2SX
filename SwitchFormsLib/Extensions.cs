// ============================================================================
// CS2SX — Erweiterungen für SwitchFormsLib
// Datei: SwitchFormsLib/Extensions.cs
//
// Enthält C#-Stubs für alle neuen Features in switchapp_ext.h.
// Der Transpiler mappt diese Aufrufe direkt zu den C-Funktionen.
// ============================================================================

using System.Collections.Generic;

// ============================================================================
// Input — Analog-Sticks
// ============================================================================

/// Repräsentiert die Position eines Analog-Sticks.
/// x/y: -32767 .. +32767 (mit Deadzone bereits herausgefiltert)
public struct StickPos
{
    public int x;
    public int y;

    /// Normiert den X-Wert auf 0..100 (Betrag).
    public int NormX => CS2SX_StickNorm(x < 0 ? -x : x);
    /// Normiert den Y-Wert auf 0..100 (Betrag).
    public int NormY => CS2SX_StickNorm(y < 0 ? -y : y);

    // Transpiler-Hilfsmethode — wird zu CS2SX_StickNorm(v) transpiliert
    private static int CS2SX_StickNorm(int v) => v;
}

public static class Input
{
    // ── Buttons (bereits vorhanden in Input.cs — hier als Referenz) ──────────
    public static bool IsDown(NpadButton btn) => false;
    public static bool IsHeld(NpadButton btn) => false;
    public static bool IsUp(NpadButton btn) => false;

    // ── Analog-Sticks ─────────────────────────────────────────────────────────

    /// Linker Stick. x: links=-32767, rechts=+32767. y: unten=-32767, oben=+32767.
    /// Wird zu CS2SX_Input_GetStickLeft(&pad) transpiliert.
    public static StickPos GetStickLeft() => new StickPos();

    /// Rechter Stick. Gleiche Achsenkonvention.
    /// Wird zu CS2SX_Input_GetStickRight(&pad) transpiliert.
    public static StickPos GetStickRight() => new StickPos();

    // ── Touch ─────────────────────────────────────────────────────────────────

    /// Aktueller Touch-Zustand (bis zu 10 simultane Berührungen).
    /// Wird zu CS2SX_Input_GetTouch() transpiliert.
    public static TouchState GetTouch() => new TouchState();
}

// ============================================================================
// Input — Touch
// ============================================================================

/// Touch-Zustand: count Berührungspunkte, je x[i]/y[i] in Pixel (0..1280 / 0..720).
public struct TouchState
{
    public int count;
    public int[] x;
    public int[] y;
    public uint[] id;

    /// Gibt true zurück wenn Punkt idx innerhalb von (rx,ry,rw,rh) liegt.
    /// Wird zu CS2SX_Touch_HitRect(&touch, idx, rx, ry, rw, rh) transpiliert.
    public bool HitRect(int idx, int rx, int ry, int rw, int rh) => false;

    /// Kurzform: Erster Finger getippt (count > 0)?
    public bool IsTouched => count > 0;

    /// Koordinate des ersten Fingers (oder 0/0 wenn kein Touch).
    public int X0 => count > 0 ? x[0] : 0;
    public int Y0 => count > 0 ? y[0] : 0;
}

// ============================================================================
// Graphics — Neue Primitiven
// ============================================================================

public static class Graphics
{
    // ── Bereits vorhanden (Referenz) ─────────────────────────────────────────
    public static void Init(int width, int height)
    {
    }
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
    public static void DrawText(int x, int y, string text, uint color, int scale)
    {
    }
    public static void DrawChar(int x, int y, char c, uint color, int scale)
    {
    }
    public static int MeasureTextWidth(string text, int scale) => 0;
    public static int MeasureTextHeight(int scale) => 0;
    public static void DrawTexture(Texture tex, int x, int y)
    {
    }

    // ── NEU: Dreiecke ─────────────────────────────────────────────────────────

    /// Dreieck-Outline. Wird zu Graphics_DrawTriangle transpiliert.
    public static void DrawTriangle(int x0, int y0, int x1, int y1,
                                     int x2, int y2, uint color)
    {
    }

    /// Gefülltes Dreieck (Scanline-Fill). Wird zu Graphics_FillTriangle transpiliert.
    public static void FillTriangle(int x0, int y0, int x1, int y1,
                                     int x2, int y2, uint color)
    {
    }

    // ── NEU: Ellipse ──────────────────────────────────────────────────────────

    /// Ellipse-Outline (rx = horizontaler Radius, ry = vertikaler Radius).
    public static void DrawEllipse(int cx, int cy, int rx, int ry, uint color)
    {
    }

    /// Gefüllte Ellipse.
    public static void FillEllipse(int cx, int cy, int rx, int ry, uint color)
    {
    }

    // ── NEU: Abgerundete Rechtecke ────────────────────────────────────────────

    /// Rechteck mit abgerundeten Ecken (r = Eckenradius).
    public static void DrawRoundedRect(int x, int y, int w, int h, int r, uint color)
    {
    }

    /// Gefülltes Rechteck mit abgerundeten Ecken.
    public static void FillRoundedRect(int x, int y, int w, int h, int r, uint color)
    {
    }

    // ── NEU: Alpha-Blending ───────────────────────────────────────────────────

    /// Setzt einen Pixel mit Alpha-Blending (alpha: 0=transparent, 255=deckend).
    public static void SetPixelAlpha(int x, int y, uint color, byte alpha)
    {
    }

    /// Rechteck mit Alpha-Transparenz (0=unsichtbar, 255=deckend).
    public static void FillRectAlpha(int x, int y, int w, int h, uint color, byte alpha)
    {
    }

    /// Text mit Alpha-Transparenz.
    public static void DrawTextAlpha(int x, int y, string text, uint color,
                                      int scale, byte alpha)
    {
    }

    // ── NEU: Sonstiges ────────────────────────────────────────────────────────

    /// Text mit 1-Pixel-Schatten für bessere Lesbarkeit auf bunten Hintergründen.
    public static void DrawTextShadow(int x, int y, string text,
                                       uint color, uint shadow, int scale)
    {
    }

    /// Gitter zeichnen (für Debug-Overlays, Spielfelder etc.).
    public static void DrawGrid(int x, int y, int w, int h,
                                 int cellW, int cellH, uint color)
    {
    }
}

// ============================================================================
// Filesystem — Verzeichnisse
// ============================================================================

public static class Directory
{
    // ── Bereits vorhanden ────────────────────────────────────────────────────
    public static bool Exists(string path) => false;
    public static bool CreateDirectory(string path) => false;
    public static bool Delete(string path) => false;
    public static List<string> GetFiles(string path, string pattern)
        => new List<string>();

    // ── NEU ──────────────────────────────────────────────────────────────────

    /// Listet alle Unterverzeichnisse. Wird zu CS2SX_Dir_GetDirectories transpiliert.
    public static List<string> GetDirectories(string path) => new List<string>();

    /// Listet Dateien UND Verzeichnisse. Wird zu CS2SX_Dir_GetEntries transpiliert.
    public static List<string> GetEntries(string path) => new List<string>();
}

// ============================================================================
// Filesystem — Path-Hilfsmethoden
// ============================================================================

public static class Path
{
    /// Dateiname aus Pfad extrahieren (letzter Teil nach '/').
    /// Wird zu CS2SX_Path_GetFileName transpiliert.
    public static string GetFileName(string path) => path;

    /// Extension inkl. Punkt, z.B. ".nro".
    /// Wird zu CS2SX_Path_GetExtension transpiliert.
    public static string GetExtension(string path) => "";

    /// Verzeichnisname (alles vor dem letzten '/').
    /// Wird zu CS2SX_Path_GetDirectoryName transpiliert.
    public static string GetDirectoryName(string path) => path;

    /// Gibt true zurück wenn der Pfad kein '.' im letzten Segment hat.
    /// Wird zu CS2SX_Path_IsDirectory transpiliert.
    public static bool IsDirectory(string path) => false;

    /// Kombiniert zwei Pfad-Teile mit '/'.
    /// Wird zu snprintf(buf, sizeof(buf), "%s/%s", a, b) transpiliert.
    public static string Combine(string a, string b) => a + "/" + b;
}

// ============================================================================
// System — Akkustand
// ============================================================================

public struct BatteryInfo
{
    public int percent;    // 0–100
    public bool charging;
    public bool connected;  // Ladegerät angesteckt
}

public static class System
{
    /// Akkustand abfragen. Braucht psmInitialize() — SwitchAppEx macht das automatisch.
    /// Wird zu CS2SX_GetBattery() transpiliert.
    public static BatteryInfo GetBattery() => new BatteryInfo();
}

// ============================================================================
// SwitchAppEx — Erweiterte Basis-Klasse
// ============================================================================

/// Erweiterung von SwitchApp mit Stick-, Touch- und Battery-Support.
/// Ableiten statt SwitchApp wenn diese Features benötigt werden.
///
/// In C wird SwitchAppEx_Run statt SwitchApp_Run verwendet.
/// Der Transpiler erkennt SwitchAppEx als Basisklasse und generiert
/// die entsprechende _Init-Funktion.
///
/// Beispiel:
///   public class MyApp : SwitchAppEx
///   {
///       public override void OnFrame()
///       {
///           if (_stickL.x > 5000) Console.WriteLine("Links!");
///           if (_touch.count > 0) Console.WriteLine($"Touch: {_touch.X0}");
///       }
///   }
public abstract class SwitchAppEx : SwitchApp
{
    // Diese Felder werden von SwitchAppEx_Run pro Frame befüllt
    public StickPos _stickL;
    public StickPos _stickR;
    public TouchState _touch;
    public BatteryInfo _battery;
}