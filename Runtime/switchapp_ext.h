#pragma once
// ============================================================================
// CS2SX switchapp_ext.h
// Erweiterungen für switchapp.h — in switchapp.h einbinden mit:
//   #include "switchapp_ext.h"
// ============================================================================

#include "switchapp.h"

// ============================================================================
// Input — Analog-Sticks
// ============================================================================

typedef struct
{
    int x;   // -32767 .. +32767  (links negativ, rechts positiv)
    int y;   // -32767 .. +32767  (oben positiv, unten negativ)
} CS2SX_StickPos;

/// Toten Bereich (Deadzone) — Werte innerhalb werden auf 0 gesetzt.
/// Standard: 3000 (~9% des maximalen Ausschlags).
#define CS2SX_STICK_DEADZONE 3000

static inline CS2SX_StickPos CS2SX_Input_GetStickLeft(PadState* pad)
{
    HidAnalogStickState raw = padGetStickPos(pad, 0);
    CS2SX_StickPos pos;
    pos.x = (raw.x > -CS2SX_STICK_DEADZONE && raw.x < CS2SX_STICK_DEADZONE) ? 0 : raw.x;
    pos.y = (raw.y > -CS2SX_STICK_DEADZONE && raw.y < CS2SX_STICK_DEADZONE) ? 0 : raw.y;
    return pos;
}

static inline CS2SX_StickPos CS2SX_Input_GetStickRight(PadState* pad)
{
    HidAnalogStickState raw = padGetStickPos(pad, 1);
    CS2SX_StickPos pos;
    pos.x = (raw.x > -CS2SX_STICK_DEADZONE && raw.x < CS2SX_STICK_DEADZONE) ? 0 : raw.x;
    pos.y = (raw.y > -CS2SX_STICK_DEADZONE && raw.y < CS2SX_STICK_DEADZONE) ? 0 : raw.y;
    return pos;
}

/// Normiert einen Stick-Wert auf 0..100 (Betrag, ohne Vorzeichen).
/// Nützlich für Fortschrittsbalken, Geschwindigkeitsanzeigen etc.
static inline int CS2SX_StickNorm(int raw)
{
    if (raw < 0) raw = -raw;
    if (raw < CS2SX_STICK_DEADZONE) return 0;
    int v = ((raw - CS2SX_STICK_DEADZONE) * 100) / (32767 - CS2SX_STICK_DEADZONE);
    return v > 100 ? 100 : v;
}

// ============================================================================
// Input — Touch-Screen
// ============================================================================

typedef struct
{
    int     count;          // Anzahl aktiver Touch-Punkte (0 = kein Touch)
    int     x[10];          // X-Koordinate (0..1280)
    int     y[10];          // Y-Koordinate (0..720)
    u32     id[10];         // Touch-ID (für Multi-Touch-Tracking)
} CS2SX_TouchState;

static inline CS2SX_TouchState CS2SX_Input_GetTouch(void)
{
    CS2SX_TouchState state;
    state.count = 0;

    HidTouchScreenState raw = { 0 };
    if (hidGetTouchScreenStates(&raw, 1) == 0)
        return state;

    int count = raw.count;
    if (count > 10) count = 10;
    state.count = count;

    for (int i = 0; i < count; i++)
    {
        // libnx liefert Koordinaten in 1/16-Pixel — durch 16 teilen
        state.x[i] = (int)(raw.touches[i].x);
        state.y[i] = (int)(raw.touches[i].y);
        state.id[i] = raw.touches[i].finger_id;
    }
    return state;
}

/// True wenn der Touch-Punkt (idx) innerhalb eines Rechtecks liegt.
static inline int CS2SX_Touch_HitRect(CS2SX_TouchState* ts, int idx,
    int rx, int ry, int rw, int rh)
{
    if (!ts || idx < 0 || idx >= ts->count) return 0;
    int tx = ts->x[idx];
    int ty = ts->y[idx];
    return tx >= rx && tx < rx + rw && ty >= ry && ty < ry + rh;
}

// ============================================================================
// Graphics — fehlende Primitiven
// ============================================================================

// ── DrawTriangle (Outline) ───────────────────────────────────────────────────

static inline void Graphics_DrawTriangle(
    int x0, int y0, int x1, int y1, int x2, int y2, u32 color)
{
    Graphics_DrawLine(x0, y0, x1, y1, color);
    Graphics_DrawLine(x1, y1, x2, y2, color);
    Graphics_DrawLine(x2, y2, x0, y0, color);
}

// ── FillTriangle (Solid, Scanline-Fill) ──────────────────────────────────────

static inline void Graphics_FillTriangle(
    int x0, int y0, int x1, int y1, int x2, int y2, u32 color)
{
    if (!g_fb_addr) return;

    // Vertices nach Y sortieren (Bubble-Sort, 3 Elemente)
    if (y0 > y1) { int t; t = x0;x0 = x1;x1 = t; t = y0;y0 = y1;y1 = t; }
    if (y1 > y2) { int t; t = x1;x1 = x2;x2 = t; t = y1;y1 = y2;y2 = t; }
    if (y0 > y1) { int t; t = x0;x0 = x1;x1 = t; t = y0;y0 = y1;y1 = t; }

    int total_height = y2 - y0;
    if (total_height == 0) return;

    for (int y = y0; y <= y2; y++)
    {
        int seg_height;
        int xa, xb;

        if (y < y1)
        {
            seg_height = y1 - y0;
            if (seg_height == 0) continue;
            xa = x0 + (x2 - x0) * (y - y0) / total_height;
            xb = x0 + (x1 - x0) * (y - y0) / seg_height;
        }
        else
        {
            seg_height = y2 - y1;
            if (seg_height == 0) continue;
            xa = x0 + (x2 - x0) * (y - y0) / total_height;
            xb = x1 + (x2 - x1) * (y - y1) / seg_height;
        }

        if (xa > xb) { int t = xa; xa = xb; xb = t; }
        for (int x = xa; x <= xb; x++)
            Graphics_SetPixel(x, y, color);
    }
}

// ── DrawEllipse (Midpoint-Algorithmus) ───────────────────────────────────────

static inline void Graphics_DrawEllipse(int cx, int cy, int rx, int ry, u32 color)
{
    if (!g_fb_addr || rx <= 0 || ry <= 0) return;

    // Midpoint-Ellipse
    int x = 0;
    int y = ry;
    long rx2 = (long)rx * rx;
    long ry2 = (long)ry * ry;
    long d = ry2 - rx2 * ry + rx2 / 4;

    while (2 * ry2 * x < 2 * rx2 * y)
    {
        Graphics_SetPixel(cx + x, cy + y, color);
        Graphics_SetPixel(cx - x, cy + y, color);
        Graphics_SetPixel(cx + x, cy - y, color);
        Graphics_SetPixel(cx - x, cy - y, color);
        x++;
        if (d < 0)
            d += ry2 * (2 * x + 1);
        else
        {
            y--;
            d += ry2 * (2 * x + 1) - rx2 * (2 * y);
        }
    }

    d = (long)ry2 * (x * x + x) + rx2 * ((y - 1) * (y - 1) - (long)ry * ry) + (rx2 - ry2);
    while (y >= 0)
    {
        Graphics_SetPixel(cx + x, cy + y, color);
        Graphics_SetPixel(cx - x, cy + y, color);
        Graphics_SetPixel(cx + x, cy - y, color);
        Graphics_SetPixel(cx - x, cy - y, color);
        y--;
        if (d > 0)
            d += rx2 * (1 - 2 * y);
        else
        {
            x++;
            d += ry2 * (2 * x + 1) - rx2 * (2 * y - 1);
        }
    }
}

// ── FillEllipse ───────────────────────────────────────────────────────────────

static inline void Graphics_FillEllipse(int cx, int cy, int rx, int ry, u32 color)
{
    if (!g_fb_addr || rx <= 0 || ry <= 0) return;
    long rx2 = (long)rx * rx;
    long ry2 = (long)ry * ry;

    for (int dy = -ry; dy <= ry; dy++)
    {
        // Berechne Breite der Ellipse auf dieser Y-Zeile
        long dx2 = rx2 * (ry2 - (long)dy * dy) / ry2;
        if (dx2 < 0) dx2 = 0;
        // Ganzzahl-Wurzel via Newton
        int dx = rx;
        while ((long)dx * dx > dx2) dx--;
        for (int x = cx - dx; x <= cx + dx; x++)
            Graphics_SetPixel(x, cy + dy, color);
    }
}

// ── DrawRoundedRect ───────────────────────────────────────────────────────────

static inline void Graphics_DrawRoundedRect(int x, int y, int w, int h, int r, u32 color)
{
    if (!g_fb_addr) return;
    if (r < 0) r = 0;
    if (r > w / 2) r = w / 2;
    if (r > h / 2) r = h / 2;

    // Gerade Seiten
    Graphics_DrawLine(x + r, y, x + w - r, y, color); // oben
    Graphics_DrawLine(x + r, y + h - 1, x + w - r, y + h - 1, color); // unten
    Graphics_DrawLine(x, y + r, x, y + h - r, color); // links
    Graphics_DrawLine(x + w - 1, y + r, x + w - 1, y + h - r, color); // rechts

    // Ecken (Viertelkreise)
    int px, py, d;

    // Oben-links
    px = 0; py = r; d = 3 - 2 * r;
    while (px <= py)
    {
        Graphics_SetPixel(x + r - px, y + r - py, color);
        Graphics_SetPixel(x + r - py, y + r - px, color);
        if (d < 0) d += 4 * px + 6;
        else { d += 4 * (px - py) + 10; py--; }
        px++;
    }
    // Oben-rechts
    px = 0; py = r; d = 3 - 2 * r;
    while (px <= py)
    {
        Graphics_SetPixel(x + w - 1 - r + px, y + r - py, color);
        Graphics_SetPixel(x + w - 1 - r + py, y + r - px, color);
        if (d < 0) d += 4 * px + 6;
        else { d += 4 * (px - py) + 10; py--; }
        px++;
    }
    // Unten-links
    px = 0; py = r; d = 3 - 2 * r;
    while (px <= py)
    {
        Graphics_SetPixel(x + r - px, y + h - 1 - r + py, color);
        Graphics_SetPixel(x + r - py, y + h - 1 - r + px, color);
        if (d < 0) d += 4 * px + 6;
        else { d += 4 * (px - py) + 10; py--; }
        px++;
    }
    // Unten-rechts
    px = 0; py = r; d = 3 - 2 * r;
    while (px <= py)
    {
        Graphics_SetPixel(x + w - 1 - r + px, y + h - 1 - r + py, color);
        Graphics_SetPixel(x + w - 1 - r + py, y + h - 1 - r + px, color);
        if (d < 0) d += 4 * px + 6;
        else { d += 4 * (px - py) + 10; py--; }
        px++;
    }
}

// ── FillRoundedRect ───────────────────────────────────────────────────────────

static inline void Graphics_FillRoundedRect(int x, int y, int w, int h, int r, u32 color)
{
    if (!g_fb_addr) return;
    if (r < 0) r = 0;
    if (r > w / 2) r = w / 2;
    if (r > h / 2) r = h / 2;

    // Mittlerer Bereich (keine Rundung)
    Graphics_FillRect(x, y + r, w, h - 2 * r, color);
    // Oben und unten (Breite = w, aber ohne Ecken — wird durch Kreise gedeckt)
    Graphics_FillRect(x + r, y, w - 2 * r, r, color);
    Graphics_FillRect(x + r, y + h - r, w - 2 * r, r, color);

    // Ecken als gefüllte Viertelkreise
    Graphics_FillCircle(x + r, y + r, r, color);
    Graphics_FillCircle(x + w - 1 - r, y + r, r, color);
    Graphics_FillCircle(x + r, y + h - 1 - r, r, color);
    Graphics_FillCircle(x + w - 1 - r, y + h - 1 - r, r, color);
}

// ── Alpha-Blending ────────────────────────────────────────────────────────────

/// Mischt src-Farbe mit Alpha über dst-Pixel (Porter-Duff over).
/// alpha: 0 = vollständig transparent, 255 = vollständig deckend.
/// WICHTIG: Setzt g_fb_addr voraus (nur im Grafik-Modus aufrufen).
static inline void Graphics_SetPixelAlpha(int x, int y, u32 color, u8 alpha)
{
    if (!g_fb_addr) return;
    if (x < 0 || x >= g_fb_width || y < 0 || y >= g_fb_height) return;

    u32* dst = &g_fb_addr[y * g_fb_width + x];
    u32  bg = *dst;

    // RGBA8888 Layout: A[31:24] B[23:16] G[15:8] R[7:0]
    u32 sr = (color >> 0) & 0xFF;
    u32 sg = (color >> 8) & 0xFF;
    u32 sb = (color >> 16) & 0xFF;

    u32 dr = (bg >> 0) & 0xFF;
    u32 dg = (bg >> 8) & 0xFF;
    u32 db = (bg >> 16) & 0xFF;

    u32 a = alpha;
    u32 ia = 255 - a;

    u32 rr = (sr * a + dr * ia) / 255;
    u32 rg = (sg * a + dg * ia) / 255;
    u32 rb = (sb * a + db * ia) / 255;

    *dst = 0xFF000000 | (rb << 16) | (rg << 8) | rr;
}

/// Zeichnet ein ausgefülltes Rechteck mit Alpha.
static inline void Graphics_FillRectAlpha(int x, int y, int w, int h, u32 color, u8 alpha)
{
    if (!g_fb_addr || alpha == 0) return;
    if (alpha == 255) { Graphics_FillRect(x, y, w, h, color); return; }
    for (int row = y; row < y + h; row++)
        for (int col = x; col < x + w; col++)
            Graphics_SetPixelAlpha(col, row, color, alpha);
}

/// Zeichnet Text mit Alpha-Transparenz.
static inline void Graphics_DrawTextAlpha(int x, int y, const char* text,
    u32 color, int scale, u8 alpha)
{
    if (!g_fb_addr || !text || alpha == 0) return;
    if (alpha == 255) { Graphics_DrawText(x, y, text, color, scale); return; }
    int ox = x;
    for (int i = 0; text[i] != '\0'; i++)
    {
        if (text[i] == '\n') { y += 8 * scale + 2; x = ox; continue; }
        char c = text[i];
        if (c < 32 || c > 127) c = '?';
        const u8* glyph = cs2sx_font8x8[(int)(c - 32)];
        for (int row = 0; row < 8; row++)
            for (int col = 0; col < 8; col++)
                if (glyph[row] & (1 << (7 - col)))
                    for (int sy = 0; sy < scale; sy++)
                        for (int sx = 0; sx < scale; sx++)
                            Graphics_SetPixelAlpha(
                                x + (7 - col) * scale + sx,
                                y + row * scale + sy,
                                color, alpha);
        x += 8 * scale + 1;
    }
}

// ── DrawPolygon (beliebiges konvexes Polygon) ─────────────────────────────────

/// Zeichnet ein Polygon aus einem Array von X/Y-Koordinaten.
/// count = Anzahl der Punkte.
static inline void Graphics_DrawPolygon(int* xs, int* ys, int count, u32 color)
{
    if (!g_fb_addr || count < 2) return;
    for (int i = 0; i < count - 1; i++)
        Graphics_DrawLine(xs[i], ys[i], xs[i + 1], ys[i + 1], color);
    Graphics_DrawLine(xs[count - 1], ys[count - 1], xs[0], ys[0], color);
}

// ── DrawGrid ─────────────────────────────────────────────────────────────────

/// Zeichnet ein Gitter (nützlich für Debug-Overlays oder Spielfelder).
static inline void Graphics_DrawGrid(int x, int y, int w, int h,
    int cellW, int cellH, u32 color)
{
    if (!g_fb_addr) return;
    for (int gx = x; gx <= x + w; gx += cellW)
        Graphics_DrawLine(gx, y, gx, y + h, color);
    for (int gy = y; gy <= y + h; gy += cellH)
        Graphics_DrawLine(x, gy, x + w, gy, color);
}

// ── DrawTextShadow ────────────────────────────────────────────────────────────

/// Zeichnet Text mit einem 1-Pixel-Schatten (Lesbarkeit auf beliebigem Hintergrund).
static inline void Graphics_DrawTextShadow(int x, int y, const char* text,
    u32 color, u32 shadow, int scale)
{
    Graphics_DrawText(x + scale, y + scale, text, shadow, scale);
    Graphics_DrawText(x, y, text, color, scale);
}

// ============================================================================
// Filesystem — Verzeichnisse listen
// ============================================================================

/// Listet alle Unterverzeichnisse in einem Pfad.
/// Entspricht Directory.GetDirectories() in C#.
static inline List_str* CS2SX_Dir_GetDirectories(const char* path)
{
    List_str* result = List_str_New();
    if (!result) return result;

    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return result;

    FsDir d;
    if (R_FAILED(fsFsOpenDirectory(&fs, path, FsDirOpenMode_ReadDirs, &d)))
    {
        fsFsClose(&fs);
        return result;
    }

    static FsDirectoryEntry _subdir_entries[64];
    static char _subdir_paths[64][512];
    s64 count = 0;
    fsDirRead(&d, &count, 64, _subdir_entries);

    for (int i = 0; i < (int)count && i < 64; i++)
    {
        snprintf(_subdir_paths[i], sizeof(_subdir_paths[i]),
            "%s/%s", path, _subdir_entries[i].name);
        List_str_Add(result, _subdir_paths[i]);
    }

    fsDirClose(&d);
    fsFsClose(&fs);
    return result;
}

/// Listet Dateien UND Verzeichnisse zusammen (wie Directory.GetFileSystemEntries).
static inline List_str* CS2SX_Dir_GetEntries(const char* path)
{
    List_str* result = List_str_New();
    if (!result) return result;

    FsFileSystem fs;
    if (R_FAILED(fsOpenSdCardFileSystem(&fs))) return result;

    FsDir d;
    int mode = FsDirOpenMode_ReadFiles | FsDirOpenMode_ReadDirs;
    if (R_FAILED(fsFsOpenDirectory(&fs, path, mode, &d)))
    {
        fsFsClose(&fs);
        return result;
    }

    static FsDirectoryEntry _all_entries[128];
    static char _all_paths[128][512];
    s64 count = 0;
    fsDirRead(&d, &count, 128, _all_entries);

    for (int i = 0; i < (int)count && i < 128; i++)
    {
        snprintf(_all_paths[i], sizeof(_all_paths[i]),
            "%s/%s", path, _all_entries[i].name);
        List_str_Add(result, _all_paths[i]);
    }

    fsDirClose(&d);
    fsFsClose(&fs);
    return result;
}

/// Gibt den Dateinamen aus einem vollständigen Pfad zurück.
/// Path.GetFileName() Äquivalent.
static inline const char* CS2SX_Path_GetFileName(const char* path)
{
    if (!path) return "";
    const char* last = path;
    for (const char* p = path; *p; p++)
        if (*p == '/') last = p + 1;
    return last;
}

/// Gibt die Extension zurück (inklusive Punkt), z.B. ".nro".
/// Path.GetExtension() Äquivalent.
static inline const char* CS2SX_Path_GetExtension(const char* path)
{
    const char* name = CS2SX_Path_GetFileName(path);
    const char* dot = NULL;
    for (const char* p = name; *p; p++)
        if (*p == '.') dot = p;
    return dot ? dot : "";
}

/// Gibt den Pfad ohne Dateinamen zurück.
/// Path.GetDirectoryName() Äquivalent.
static inline const char* CS2SX_Path_GetDirectoryName(const char* path)
{
    static char _dirname_buf[512];
    if (!path) return "";
    int len = (int)strlen(path);
    int slash = -1;
    for (int i = len - 1; i >= 0; i--)
        if (path[i] == '/') { slash = i; break; }
    if (slash <= 0) return "/";
    int copyLen = slash;
    if (copyLen >= 512) copyLen = 511;
    memcpy(_dirname_buf, path, copyLen);
    _dirname_buf[copyLen] = '\0';
    return _dirname_buf;
}

/// Prüft ob ein Eintrag ein Verzeichnis ist (anhand fehlendem Punkt im Namen).
/// Einfache Heuristik — für robustere Prüfung FsDirectoryEntry.type verwenden.
static inline int CS2SX_Path_IsDirectory(const char* path)
{
    const char* ext = CS2SX_Path_GetExtension(path);
    return ext[0] == '\0';
}

// ============================================================================
// System — Akkustand
// ============================================================================

typedef struct
{
    int  percent;     // 0–100
    bool charging;
    bool connected;   // Ladegerät angesteckt
} CS2SX_BatteryInfo;

static inline CS2SX_BatteryInfo CS2SX_GetBattery(void)
{
    CS2SX_BatteryInfo info = { 0, false, false };

    u32 chargePercent = 0;
    if (R_SUCCEEDED(psmGetBatteryChargePercentage(&chargePercent)))
        info.percent = (int)chargePercent;

    PsmChargerType chargerType = PsmChargerType_Unconnected;
    if (R_SUCCEEDED(psmGetChargerType(&chargerType)))
    {
        info.connected = chargerType != PsmChargerType_Unconnected;
        info.charging = info.connected && chargePercent < 100;
    }

    return info;
}

// ============================================================================
// SwitchApp — Erweitertes Update (Sticks + Touch in OnFrame verfügbar)
// ============================================================================

/// Erweiterte App-Basis die Stick- und Touch-State pro Frame bereitstellt.
/// Statt SwitchApp ableiten — dann sind GetStickLeft() etc. direkt verfügbar.
///
/// Nutzung in C#:
///   public class MyApp : SwitchAppEx { ... }
///   // In OnFrame: _stickL.x, _touch.count, _touch.x[0] etc.

typedef struct SwitchAppEx SwitchAppEx;
struct SwitchAppEx
{
    // Muss erstes Feld sein — erlaubt Cast zu SwitchApp*
    SwitchApp base;

    // Pro-Frame aktualisiert von SwitchAppEx_Run
    CS2SX_StickPos  stickL;
    CS2SX_StickPos  stickR;
    CS2SX_TouchState touch;
    CS2SX_BatteryInfo battery;
    int              batteryTimer;   // Akku nur jede ~5s abfragen (teuer)
};

static inline void SwitchAppEx_Run(SwitchAppEx* self)
{
    if (!self) return;

    PadState pad;
    padConfigureInput(1, HidNpadStyleSet_NpadStandard);
    padInitializeDefault(&pad);

    // Touch initialisieren
    hidInitializeTouchScreen();

    NWindow* win = nwindowGetDefault();
    framebufferCreate(&g_fb, win,
        (u32)g_fb_width, (u32)g_fb_height,
        PIXEL_FORMAT_RGBA_8888, 2);
    framebufferMakeLinear(&g_fb);

    if (self->base.OnInit)
        self->base.OnInit((SwitchApp*)self);

    // PSM für Akkustand öffnen
    bool psmOk = R_SUCCEEDED(psmInitialize());

    int use_gfx = g_gfx_init;
    if (!use_gfx)
    {
        framebufferClose(&g_fb);
        consoleInit(NULL);
        Form_InitFocus(&self->base.form);
    }

    self->batteryTimer = 0;

    while (appletMainLoop())
    {
        padUpdate(&pad);
        self->base.kDown = padGetButtonsDown(&pad);
        self->base.kHeld = padGetButtons(&pad);

        // Sticks aktualisieren
        self->stickL = CS2SX_Input_GetStickLeft(&pad);
        self->stickR = CS2SX_Input_GetStickRight(&pad);

        // Touch aktualisieren
        self->touch = CS2SX_Input_GetTouch();

        // Akkustand alle 300 Frames (~5s bei 60fps)
        self->batteryTimer++;
        if (self->batteryTimer >= 300 && psmOk)
        {
            self->battery = CS2SX_GetBattery();
            self->batteryTimer = 0;
        }

        if (use_gfx)
        {
            u8* fb_raw = framebufferBegin(&g_fb, NULL);
            if (!fb_raw) continue;
            g_fb_addr = (u32*)fb_raw;

            int total = g_fb_width * g_fb_height;
            for (int i = 0; i < total; i++)
                g_fb_addr[i] = COLOR_BLACK;

            Form_UpdateAll(&self->base.form, self->base.kDown, self->base.kHeld);

            if (self->base.OnFrame)
                self->base.OnFrame((SwitchApp*)self);

            Form_DrawAll(&self->base.form);
            framebufferEnd(&g_fb);
            g_fb_addr = NULL;
        }
        else
        {
            consoleClear();
            printf("\033[H\033[2J");
            Form_UpdateAll(&self->base.form, self->base.kDown, self->base.kHeld);
            if (self->base.OnFrame)
                self->base.OnFrame((SwitchApp*)self);
            Form_DrawAll(&self->base.form);
            consoleUpdate(NULL);
        }

        if (self->base.kDown & HidNpadButton_Plus)
            break;
    }

    if (self->base.OnExit)
        self->base.OnExit((SwitchApp*)self);

    Form_Free(&self->base.form);
    if (psmOk) psmExit();

    if (use_gfx)
        framebufferClose(&g_fb);
    else
        consoleExit(NULL);
}