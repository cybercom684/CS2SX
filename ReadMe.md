# CS2SX — C# to Nintendo Switch Transpiler

CS2SX transpiliert C#-Quellcode zu C und kompiliert ihn via DevkitPro zu einer Nintendo Switch Homebrew `.nro`-Datei.

---

## Voraussetzungen

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [DevkitPro](https://devkitpro.org/wiki/Getting_Started) mit `devkitA64` und `libnx`
- Umgebungsvariable `DEVKITPRO` muss gesetzt sein

---

## Installation

```bash
dotnet tool install --global --add-source ./bin/Release CS2SX
```

## Update

```bash
dotnet pack -c Release
dotnet tool update --global --add-source ./bin/Release CS2SX
```

---

## Verwendung

```bash
# Neues Projekt erstellen
cs2sx new MeinProjekt

# Projekt bauen
cs2sx build MeinProjekt/MeinProjekt.csproj

# LibNX-Stubs generieren (optional)
cs2sx genstubs <libnx-include> <output>
```

Die fertige `.nro`-Datei liegt danach im Projektverzeichnis und kann direkt auf die Switch SD-Karte kopiert werden.

> Der Build ist **inkrementell** — nur geänderte `.cs`-Dateien werden neu transpiliert. Unveränderte Dateien werden übersprungen.

---

## Beispiel-Apps

### Console-App (Text-UI)

```csharp
public class MyApp : SwitchApp
{
    private Label _label;
    private Button _button;
    private List<int> _values;

    public override void OnInit()
    {
        _values ??= new List<int>();

        if (!Directory.Exists("/switch/MeinProjekt"))
            Directory.CreateDirectory("/switch/MeinProjekt");

        _label = new Label("Hello from C#!");
        _label.X = 5;
        _label.Y = 5;
        Form.Add(_label);

        _button = new Button("Press A");
        _button.X = 5;
        _button.Y = 8;
        _button.OnClick = OnPress;
        Form.Add(_button);
    }

    public override void OnFrame() { }

    public void OnPress()
    {
        _values.Add(_values.Count);
        _label.Text = $"Pressed {_values.Count} times!";
        File.WriteAllText("/switch/MeinProjekt/save.txt", _values.Count.ToString());
    }
}
```

### Grafik-App (Framebuffer)

```csharp
public class MyApp : SwitchApp
{
    public override void OnInit()
    {
        Graphics.Init(1280, 720);
    }

    public override void OnFrame()
    {
        Graphics.FillScreen(Color.Black);
        Graphics.DrawText(100, 100, "Hello Switch!", Color.White, 2);
        Graphics.FillRect(100, 200, 200, 50, Color.Blue);
        Graphics.DrawLine(0, 0, 400, 400, Color.Red);
        Graphics.FillCircle(640, 360, 80, Color.Green);
        Graphics.FillTriangle(200, 100, 300, 300, 100, 300, Color.RGB(255, 128, 0));
        Graphics.FillEllipse(640, 360, 120, 60, Color.RGB(80, 200, 120));
        Graphics.FillRoundedRect(50, 50, 300, 150, 20, Color.RGB(60, 60, 180));
        Graphics.DrawTextShadow(100, 400, "Shadow!", Color.White, Color.RGB(0,0,0), 2);
    }
}
```

### Zufall & Mathematik

```csharp
public class MyApp : SwitchApp
{
    public override void OnInit()
    {
        Graphics.Init(1280, 720);
    }

    public override void OnFrame()
    {
        // Random — beide Schreibweisen funktionieren
        int r1 = Random.Shared.Next(0, 100);
        int r2 = System.Random.Shared.Next(50);
        float rf = Random.Shared.NextSingle();

        // Math — beide Schreibweisen funktionieren
        float d = System.Math.Sqrt(r1 * r1 + r2 * r2);
        float angle = Math.Atan2(r2, r1);
        int clamped = Math.Clamp(r1, 0, 10);

        Graphics.FillScreen(Color.Black);
        Graphics.DrawText(10, 10, "r1=" + r1.ToString(), Color.White, 2);
        Graphics.DrawText(10, 50, "dist=" + d.ToString(), Color.Cyan, 2);
    }
}
```

### Multi-File-App mit statischer Hilfsklasse

```csharp
// MinUI.cs
public static class MinUI
{
    public static void DrawHeader(string title, MinUiColorPreset preset)
    {
        Graphics.FillRect(1, 1, 1278, 50, preset.Background);
        Graphics.DrawRect(0, 0, 1280, 52, preset.Accent);
        Graphics.DrawText(10, 20, title, preset.Foreground, 2);
    }
}

public class MinUiColorPreset
{
    public uint Background { get; set; }
    public uint Foreground { get; set; }
    public uint Accent     { get; set; }

    public MinUiColorPreset(uint background, uint foreground, uint accent)
    {
        Background = background;
        Foreground = foreground;
        Accent     = accent;
    }
}

// Program.cs
public class MyApp : SwitchApp
{
    public MinUiColorPreset uiPreset;

    public override void OnInit()
    {
        Graphics.Init(1280, 720);
        uiPreset = new MinUiColorPreset(Color.Gray, Color.White, Color.Cyan);
    }

    public override void OnFrame()
    {
        Graphics.FillScreen(Color.Black);
        MinUI.DrawHeader("Meine App", uiPreset);
    }
}
```

> **Wichtig:** Wird `Graphics.Init()` in `OnInit()` aufgerufen, wechselt CS2SX automatisch in den Framebuffer-Modus. Ohne `Graphics.Init()` läuft die App im Console/ANSI-Modus.

> **Wichtig:** **Eine Klasse pro `.cs`-Datei.** Der Transpiler verarbeitet jede Datei separat.

---

## Unterstützte Features

### Typen & Collections

| Feature | Status | Hinweis |
|---|---|---|
| `string` | ✅ | als `const char*` |
| `int`, `float`, `bool`, `char` | ✅ | direkt gemappt |
| `u8`, `u16`, `u32`, `u64` | ✅ | libnx-Typen |
| `T?` Nullable-Typen | ✅ | `HasValue`, `Value`, `??`, `?.` |
| `List<T>` | ✅ | `Add`, `Remove`, `Clear`, `Contains`, Index-Zugriff |
| `List<string>` | ✅ | `foreach`, `string.Join`, `string.Split` |
| `Dictionary<K,V>` | ✅ | `Add`, `Remove`, `ContainsKey`, `TryGetValue`, Indexer |
| `StringBuilder` | ✅ | `Append`, `AppendLine`, `Clear`, `ToString`, `Insert`, `Replace`, `IndexOf` |
| `int[]`, `float[]`, `string[]` | ✅ | Stack-Arrays mit Initializer (`new[] { 1, 2, 3 }`) |
| `int[,]` mehrdimensionale Arrays | ✅ | wird als flaches 1D-Array transpiliert |
| `StickPos` | ✅ | Analog-Stick-Position (`x`, `y`) |
| `TouchState` | ✅ | Touch-Screen-Zustand (`count`, `x[]`, `y[]`) |
| `BatteryInfo` | ✅ | Akkustand (`percent`, `charging`, `connected`) |

### Numerische Konstanten

```csharp
int max  = int.MaxValue;    // → INT_MAX
int min  = int.MinValue;    // → INT_MIN
float fm = float.MaxValue;  // → FLT_MAX
float fe = float.Epsilon;   // → FLT_EPSILON
double dm = double.MaxValue;// → DBL_MAX
float pi = Math.PI;         // → (float)M_PI  (auch MathF.PI → 3.14159265f)
float e  = Math.E;          // → (float)M_E
float nan = float.NaN;      // → NAN
```

### Nullable-Typen

```csharp
int? x = null;             // → int* x = NULL;
int? x = 5;                // → int* x = &(int){5};
bool hasVal = x.HasValue;  // → (x != NULL)
int val = x.Value;         // → (*x)
int v = x ?? 0;            // → (x != NULL ? *x : 0)
```

### String-Methoden

| Methode | Status |
|---|---|
| `Trim`, `TrimStart`, `TrimEnd` | ✅ |
| `ToUpper`, `ToLower` | ✅ |
| `Replace`, `Substring`, `IndexOf`, `LastIndexOf` | ✅ |
| `StartsWith`, `EndsWith`, `Contains`, `Equals` | ✅ |
| `PadLeft`, `PadRight` | ✅ |
| `Split`, `string.Join` | ✅ |
| `string.Format`, `string.Concat` | ✅ |
| `"Hello" + variable.ToString()` | ✅ | → `snprintf`-basierte Konkatenation |
| `IsNullOrEmpty`, `IsNullOrWhiteSpace` | ✅ |
| String-Interpolation `$"..."` | ✅ |

### Parsing

| Methode | Status |
|---|---|
| `int.Parse(s)` | ✅ |
| `int.TryParse(s, out val)` | ✅ |
| `float.Parse(s)` | ✅ |
| `float.TryParse(s, out val)` | ✅ |

### ref / out Parameter

```csharp
public void Swap(ref int a, ref int b)
{
    int tmp = a;
    a = b;
    b = tmp;
}

int x = 1, y = 2;
Swap(ref x, ref y);  // → Swap_impl(&x, &y)
```

`ref`- und `out`-Parameter werden korrekt als Zeiger transpiliert und beim Aufruf automatisch mit `&` versehen.

---

## Zufall

CS2SX verwendet einen eingebauten LCG-Zufallsgenerator ohne externe Abhängigkeiten.

```csharp
// Alle folgenden Schreibweisen funktionieren:
int n  = Random.Shared.Next(0, 100);     // Zahl zwischen 0 und 99
int n2 = Random.Shared.Next(50);         // Zahl zwischen 0 und 49
int n3 = System.Random.Shared.Next();    // Beliebige positive Zahl
float f = Random.Shared.NextSingle();    // Float zwischen 0.0 und 1.0

// Instanz-Aufrufe werden ebenso unterstützt:
var rng = new Random();
int n4 = rng.Next(1, 7);                 // Würfelwurf
```

| Methode | C-Ausgabe |
|---|---|
| `Next(min, max)` | `CS2SX_Rand_Next(min, max)` |
| `Next(max)` | `CS2SX_Rand_NextMax(max)` |
| `Next()` | `CS2SX_Rand_Next(0, 32767)` |
| `NextSingle()` / `NextFloat()` | `CS2SX_Rand_Float()` |
| `NextDouble()` | `(double)CS2SX_Rand_Float()` |

---

## Mathematik

Alle Methoden unterstützen sowohl die Kurzform `Math.X` als auch die vollständig qualifizierte Form `System.Math.X`.

```csharp
float d    = Math.Sqrt(x * x + y * y);
float s    = System.Math.Sin(angle);
int   v    = Math.Clamp(value, 0, 100);
int   sign = Math.Sign(delta);
float r    = Math.Round(3.7f);     // → 4.0f
```

| C#-Methode | C-Ausgabe |
|---|---|
| `Math.Abs` / `System.Math.Abs` | `abs(x)` |
| `Math.Min` / `Max` | `MIN(a,b)` / `MAX(a,b)` |
| `Math.Clamp` | `CLAMP(v,lo,hi)` |
| `Math.Sqrt` | `sqrtf(x)` |
| `Math.Floor` / `Ceiling` / `Round` | `floorf` / `ceilf` / `roundf` |
| `Math.Sin` / `Cos` / `Tan` | `sinf` / `cosf` / `tanf` |
| `Math.Atan2` | `atan2f(y,x)` |
| `Math.Pow` | `powf(x,y)` |
| `Math.Sign` | `CS2SX_Sign(x)` |

---

## Farben

```csharp
// Vordefinierte Farben
Color.Black    Color.White    Color.Red      Color.Green
Color.Blue     Color.Yellow   Color.Cyan     Color.Magenta
Color.Gray     Color.Orange   Color.Pink     Color.Purple
Color.Brown    Color.Teal     Color.Lime     Color.Navy
Color.Silver   Color.Maroon   Color.Olive

// Eigene Farben
uint myColor  = Color.RGB(255, 128, 0);
uint myColorA = Color.RGBA(255, 128, 0, 200);

// Alpha-Variante einer bestehenden Farbe
uint halfBlack = Color.Black.WithAlpha(128);  // → Color_WithAlpha(COLOR_BLACK, 128)
uint semiRed   = Color.Red.WithAlpha(200);
```

---

## Grafik (Framebuffer)

Aktivierung: `Graphics.Init(1280, 720)` in `OnInit()` aufrufen.

### Basis-Primitiven

| Methode | Beschreibung |
|---|---|
| `Graphics.Init(w, h)` | Framebuffer-Modus aktivieren |
| `Graphics.FillScreen(color)` | Bildschirm füllen |
| `Graphics.SetPixel(x, y, color)` | Einzelnen Pixel setzen |
| `Graphics.DrawRect(x, y, w, h, color)` | Rechteck-Outline |
| `Graphics.FillRect(x, y, w, h, color)` | Gefülltes Rechteck |
| `Graphics.DrawLine(x0, y0, x1, y1, color)` | Linie (Bresenham) |
| `Graphics.DrawCircle(cx, cy, r, color)` | Kreis-Outline |
| `Graphics.FillCircle(cx, cy, r, color)` | Gefüllter Kreis |
| `Graphics.DrawText(x, y, text, color, scale)` | Text (8×8 Bitmap-Font) |
| `Graphics.DrawChar(x, y, c, color, scale)` | Einzelnes Zeichen |
| `Graphics.MeasureTextWidth(text, scale)` | Text-Breite in Pixeln |
| `Graphics.MeasureTextHeight(scale)` | Text-Höhe in Pixeln |
| `Graphics.DrawTexture(tex, x, y)` | Texture rendern |

### Erweiterte Primitiven

| Methode | Beschreibung |
|---|---|
| `Graphics.DrawTriangle(x0,y0, x1,y1, x2,y2, color)` | Dreieck-Outline |
| `Graphics.FillTriangle(x0,y0, x1,y1, x2,y2, color)` | Gefülltes Dreieck (Scanline-Fill) |
| `Graphics.DrawEllipse(cx, cy, rx, ry, color)` | Ellipse-Outline |
| `Graphics.FillEllipse(cx, cy, rx, ry, color)` | Gefüllte Ellipse |
| `Graphics.DrawRoundedRect(x, y, w, h, r, color)` | Abgerundetes Rechteck |
| `Graphics.FillRoundedRect(x, y, w, h, r, color)` | Gefülltes abgerundetes Rechteck |
| `Graphics.DrawGrid(x, y, w, h, cellW, cellH, color)` | Gitter |
| `Graphics.DrawTextShadow(x, y, text, color, shadow, scale)` | Text mit 1px-Schatten |

### Alpha-Blending

| Methode | Beschreibung |
|---|---|
| `Graphics.SetPixelAlpha(x, y, color, alpha)` | Pixel mit Alpha (0=transparent, 255=deckend) |
| `Graphics.FillRectAlpha(x, y, w, h, color, alpha)` | Rechteck mit Alpha |
| `Graphics.DrawTextAlpha(x, y, text, color, scale, alpha)` | Text mit Alpha |

---

## Input

### Buttons

```csharp
if (Input.IsDown(NpadButton.A))   { /* einmalig beim Drücken  */ }
if (Input.IsHeld(NpadButton.ZR))  { /* solange gehalten       */ }
if (Input.IsUp(NpadButton.B))     { /* einmalig beim Loslassen */ }
```

Verfügbare Buttons: `A`, `B`, `X`, `Y`, `L`, `R`, `ZL`, `ZR`, `Plus`, `Minus`, `Up`, `Down`, `Left`, `Right`, `StickL`, `StickR` sowie alle Stick-Richtungen.

### Analog-Sticks

```csharp
StickPos left  = Input.GetStickLeft();
StickPos right = Input.GetStickRight();

if (left.x > 5000)
    Console.WriteLine("Stick rechts");
```

> Deadzone ±3000 wird automatisch herausgefiltert. X: negativ = links, positiv = rechts. Y: positiv = oben.

### Touch-Screen

```csharp
TouchState touch = Input.GetTouch();

if (touch.count > 0)
{
    int x = touch.x[0];
    int y = touch.y[0];
    Graphics.FillCircle(x, y, 20, Color.Red);
}

for (int i = 0; i < touch.count && i < 10; i++)
    Graphics.FillCircle(touch.x[i], touch.y[i], 15, Color.Green);
```

---

## System

### Akkustand

```csharp
BatteryInfo battery = System.GetBattery();
Graphics.DrawText(10, 10, $"Akku: {battery.percent}%", Color.White, 1);
```

| Feld | Typ | Beschreibung |
|---|---|---|
| `percent` | `int` | Ladezustand 0–100 |
| `charging` | `bool` | `true` wenn geladen wird |
| `connected` | `bool` | `true` wenn Ladegerät angesteckt |

### App beenden

```csharp
Environment.Exit(0);   // Beendet die App sauber (→ exit(0))
Environment.Exit(1);   // Beendet mit Fehlercode
```

---

## File I/O (SD-Karte)

Alle Pfade müssen absolut sein und mit `/switch/` beginnen.

### Dateien

| Methode | Beschreibung |
|---|---|
| `File.ReadAllText(path)` | Datei lesen (max. 8192 Bytes) |
| `File.WriteAllText(path, content)` | Datei schreiben (überschreibt) |
| `File.AppendAllText(path, content)` | An Datei anhängen |
| `File.Exists(path)` | Prüft ob Datei existiert |
| `File.Delete(path)` | Datei löschen |
| `File.Copy(src, dst)` | Datei kopieren |

### Verzeichnisse

| Methode | Beschreibung |
|---|---|
| `Directory.Exists(path)` | Prüft ob Verzeichnis existiert |
| `Directory.CreateDirectory(path)` | Verzeichnis anlegen |
| `Directory.Delete(path)` | Verzeichnis löschen |
| `Directory.GetFiles(path, pattern)` | Dateien auflisten → `List<string>` |
| `Directory.GetDirectories(path)` | Unterverzeichnisse → `List<string>` |
| `Directory.GetEntries(path)` | Dateien + Verzeichnisse → `List<string>` |

### Pfad-Hilfsmethoden

| Methode | Beispiel | Ergebnis |
|---|---|---|
| `Path.GetFileName(path)` | `"/switch/app.nro"` | `"app.nro"` |
| `Path.GetExtension(path)` | `"/switch/app.nro"` | `".nro"` |
| `Path.GetDirectoryName(path)` | `"/switch/app.nro"` | `"/switch"` |
| `Path.Combine(a, b)` | `"/switch"`, `"save.txt"` | `"/switch/save.txt"` |
| `Path.IsDirectory(path)` | `"/switch/mydir"` | `true` |

---

## Kontrollfluss

| Feature | Status |
|---|---|
| `if`, `else if`, `else` | ✅ |
| `for`, `foreach`, `while`, `do...while` | ✅ |
| `switch` (Wert und Pattern) | ✅ |
| `switch` mit `const`-Feldern als case-Labels | ✅ |
| `break`, `continue`, `return` | ✅ |
| `try` / `catch` | ✅ (via `setjmp`) |
| `using` (mit `IDisposable`) | ✅ |
| `??` Null-Coalescing | ✅ |
| `??=` Null-Coalescing-Zuweisung | ✅ |

### const-Felder als case-Labels

```csharp
public class Grid
{
    private const int EMPTY = 0;
    private const int WALL  = 1;
    private const int FOOD  = 2;

    public void Process(int cell)
    {
        switch (cell)
        {
            case WALL:  Graphics.SetPixel(x, y, Color.Gray); break;
            case FOOD:  Graphics.SetPixel(x, y, Color.Green); break;
            case EMPTY: break;
        }
    }
}
```

`const`-Felder werden via Roslyn SemanticModel erkannt und zu `ClassName_WALL` etc. aufgelöst, was in C als `static const int` gültig ist.

---

## Pattern Matching

```csharp
string label = value switch { 0 => "zero", 1 => "one", _ => "other" };

string category = score switch { >= 90 => "A", >= 70 => "B", _ => "C" };

if (obj is Dog d) { d.Bark(); }
if (x is not null) { ... }
```

| Pattern | Status |
|---|---|
| Konstant (`case 1:`, `1 =>`) | ✅ |
| Discard (`_`) | ✅ |
| `is`-Pattern mit Binding (`obj is Dog d`) | ✅ |
| Relational (`>= 5`, `< 10`) | ✅ |
| `not null` / `is null` | ✅ |
| `and` / `or` Pattern | ✅ |
| `when`-Klausel | ✅ |

---

## Klassen & OOP

| Feature | Status | Hinweis |
|---|---|---|
| Klassen mit Feldern und Methoden | ✅ | → C-Structs |
| `static class` | ✅ | → reine C-Funktionen, kein Struct |
| Vererbung (einzeln) | ✅ | `SwitchApp`, `Control` als Basis |
| `abstract`-Klassen | ✅ | → vtable-Infrastruktur |
| `virtual` / `override` | ✅ | → vtable-Funktionszeiger |
| Eigene Controls (erbt von `Control`) | ✅ | `Draw()` + `Update()` |
| `static`-Felder und -Methoden | ✅ | → globale C-Variablen |
| `static readonly` Array-Felder | ✅ | → `static const T ClassName_Feld[] = {...}` |
| Auto-Properties `{ get; set; }` | ✅ | → `f_`-prefixed Struct-Felder |
| Properties mit Body | ✅ | → Getter/Setter-Funktionen |
| `IDisposable` / `using` | ✅ | → `Dispose()`-Aufruf am Blockende |
| Enums mit Werten | ✅ | |
| `interface` | ❌ | |
| Generics | ❌ | |

### Felder: mit und ohne `_` Prefix

Beide Konventionen werden korrekt transpiliert:

```csharp
private uint _bgColor;       // → self->f_bgColor
public  uint Background;     // → self->f_Background
```

### static readonly Array-Felder

```csharp
public class Snake
{
    private static readonly int[] DX = { 1, 0, -1,  0 };
    private static readonly int[] DY = { 0, 1,  0, -1 };
}
// → static const int Snake_DX[] = {1, 0, -1, 0};
//   static const int Snake_DY[] = {0, 1, 0, -1};
```

### static class

```csharp
// MinUI.cs
public static class MinUI
{
    public static void DrawHeader(string title, MinUiColorPreset preset)
    {
        Graphics.FillRect(1, 1, 1278, 50, preset.Background);
        Graphics.DrawText(10, 20, title, preset.Foreground, 2);
    }
}

// Aufruf in Program.cs:
MinUI.DrawHeader("Titel", myPreset);  // → MinUI_DrawHeader("Titel", myPreset)
```

### Methoden-Naming

Eigene Methoden werden unabhängig von Groß-/Kleinschreibung korrekt transpiliert:

```csharp
public void buildHeader(string title) { }   // → MyApp_buildHeader(self, title)
public void UpdateScore(int s) { }          // → MyApp_UpdateScore(self, s)
```

### Eigene Controls

```csharp
public class ValueMeter : Control
{
    private int _value;
    private int _maxValue;
    private int _width;

    public void SetValue(int v)  { _value    = v; }
    public void SetMax(int max)  { _maxValue = max; }
    public void SetWidth(int w)  { _width    = w; }

    public override void Draw()
    {
        int filled = _maxValue > 0 ? (_value * _width) / _maxValue : 0;
        Console.Write(string.Format("\x1b[{0};{1}H[", base.Y, base.X));
        for (int i = 0; i < _width; i++)
            Console.Write(i < filled ? "#" : "-");
        Console.Write("]");
    }

    public override void Update(ulong kDown, ulong kHeld) { }
}
```

---

## Render-Modi

| Modus | Aktivierung | Beschreibung |
|---|---|---|
| **Console** | Standard (kein `Graphics.Init`) | ANSI-Terminal, `Label`, `Button`, `ProgressBar` |
| **Framebuffer** | `Graphics.Init(1280, 720)` in `OnInit()` | Direktes Pixel-Rendering, 1280×720 RGBA8888 |

Im Framebuffer-Modus sind Console-Controls nicht sichtbar.

---

## Projektstruktur

```
MeinProjekt/
├── MeinProjekt.csproj
├── cs2sx.json
├── Program.cs              — Haupt-App (eine Klasse pro Datei!)
├── MeineKlasse.cs
├── cs2sx_out/              — generierter C-Code (nicht manuell bearbeiten)
└── MeinProjekt.nro
```

`cs2sx.json`:

```json
{
    "name": "MeinProjekt",
    "author": "Dein Name",
    "version": "1.0.0",
    "mainClass": "MyApp",
    "icon": "icon.jpg"
}
```

---

## Architektur

```
CS2SX/
├── Core/
│   ├── TypeRegistry.cs         — einzige Quelle aller Typ-Mappings
│   └── TranspilerContext.cs    — geteilter Zustand, kein globaler State
├── Transpiler/
│   ├── Handlers/
│   │   ├── InvocationDispatcher.cs
│   │   ├── LibNxHandler.cs
│   │   ├── InputHandler.cs / InputExtHandler.cs
│   │   ├── ConsoleHandler.cs
│   │   ├── EnvironmentHandler.cs      — Environment.Exit, Console.Clear
│   │   ├── FormHandler.cs
│   │   ├── GraphicsHandler.cs / GraphicsExtHandler.cs
│   │   ├── ColorHandler.cs            — Color.RGB, Color.WithAlpha
│   │   ├── RandomHandler.cs           — Random.Shared.Next, NextSingle
│   │   ├── MathHandler.cs             — Math.X + System.Math.X
│   │   ├── FileHandler.cs
│   │   ├── DirectoryExtHandler.cs
│   │   ├── PathHandler.cs
│   │   ├── SystemExtHandler.cs
│   │   ├── ParseHandler.cs
│   │   ├── ListHandler.cs / DictionaryHandler.cs
│   │   ├── StringBuilderHandler.cs / StringMethodHandler.cs
│   │   ├── StringConcatHandler.cs     — "string" + variable.ToString()
│   │   ├── FieldMethodHandler.cs
│   │   ├── StaticClassHandler.cs      — static class Aufrufe (MinUI.DrawHeader)
│   │   ├── OwnMethodHandler.cs        — eigene Methoden (groß + klein)
│   │   └── ConstFieldAccessWriter.cs  — const-Felder in switch-Labels
│   ├── Strategies/
│   │   ├── SwitchAppConstructorStrategy.cs
│   │   ├── ControlSubclassConstructorStrategy.cs
│   │   └── DefaultConstructorStrategy.cs
│   ├── Writers/
│   │   ├── ExpressionWriter.cs
│   │   ├── StatementWriter.cs
│   │   ├── FormatStringBuilder.cs
│   │   ├── StringEscaper.cs
│   │   ├── TypeInferrer.cs
│   │   └── NullableAndPatternWriter.cs
│   ├── CSharpToC.cs
│   ├── LambdaLifter.cs
│   ├── PropertyWriter.cs
│   └── VTableBuilder.cs
├── Build/
│   ├── BuildPipeline.cs / CCompiler.cs
│   ├── EntryPointGenerator.cs
│   ├── NacpBuilder.cs / NroBuilder.cs
│   └── ProjectConfig.cs / ProjectCreator.cs / ProjectReader.cs
└── Runtime/
    ├── switchforms.h    — UI-Controls, Collections, String-Utils, File I/O
    ├── switchforms.c    — globale Variablendefinitionen
    └── switchapp.h      — SwitchApp-Loop, Framebuffer, Graphics, Input, System
```

### Neuen Feature-Handler hinzufügen

```csharp
// 1. Transpiler/Handlers/MeinHandler.cs anlegen
public sealed class MeinHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        if (calleeStr != "Mein.Methode")
            return NotHandled(out result);

        result = "mein_c_aufruf(" + ArgAt(args, 0) + ")";
        return true;
    }
}

// 2. In InvocationDispatcher.cs vor OwnMethodHandler eintragen
new MeinHandler(),
```

### Neuen Typ hinzufügen

Eintrag in `Core/TypeRegistry.cs` ergänzen — `s_primitives`, `s_controlTypes` oder `s_libNxStructs`. Für Stack-Struct-Rückgabetypen zusätzlich in `TypeInferrer.cs` → `InferInvocation()`.

---

## Bekannte Einschränkungen

- **Ein `SwitchApp`-Subtyp pro Projekt**
- **Eine Klasse pro `.cs`-Datei** — keine verschachtelten Klassen
- **`string`-Puffer 512 Bytes** — Dateipuffer 8192 Bytes
- **Bitmap-Font 8×8** — kein Anti-Aliasing
- **Kein Heap-GC** — allokierte Objekte (`*_New()`) manuell freigeben
- **Lambda-Captures** — nur Werttypen und primitive Captures zuverlässig
- **`is`-Typ-Pattern** — erfordert `TypeName_Is()`-Hilfsfunktion in der Runtime
- **Statische String-Puffer** — verschachtelte String-Methoden können sich überschreiben
- **Mehrdimensionale Arrays** — `int[,]` wird als flaches 1D-Array transpiliert, kein echter 2D-Indexer

---

## Nicht unterstützt

| Feature |
|---|
| `async` / `await` |
| LINQ |
| `params`-Parameter (nur teilweise) |
| Tuple-Return / Dekonstruktion |
| `interface` |
| Generics |
| `Console.ReadLine` / Keyboard-Input |

---

## Lizenz

MIT