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

# Transpile-only Check (kein GCC)
cs2sx check MeinProjekt/MeinProjekt.csproj

# Datei-Watcher — rebuild bei Änderungen
cs2sx watch MeinProjekt/MeinProjekt.csproj

# Build-Artefakte löschen
cs2sx clean MeinProjekt/MeinProjekt.csproj

# LibNX-Stubs generieren (optional)
cs2sx genstubs <libnx-include> <output>
```

Die fertige `.nro`-Datei liegt danach im Projektverzeichnis und kann direkt auf die Switch SD-Karte kopiert werden.

> Der Build ist **inkrementell** — nur geänderte `.cs`-Dateien werden neu transpiliert. Unveränderte Dateien werden übersprungen. Header-Abhängigkeiten (z.B. Änderungen an `_generics.h` oder `_interfaces.h`) werden automatisch berücksichtigt und triggern bei Bedarf einen vollständigen Rebuild der abhängigen Dateien. Wird eine neuere Version von `cs2sx` selbst erkannt, wird automatisch ein vollständiger Rebuild ausgelöst.

---

## CLI-Befehle

| Befehl | Beschreibung |
|---|---|
| `cs2sx new <AppName>` | Erstellt ein neues Projekt mit Vorlage, Platzhalter-Icon und README |
| `cs2sx build <csproj\|folder>` | Vollständiger Build → `.nro` |
| `cs2sx check <csproj>` | Transpile-only, kein GCC — schnelle Fehlerprüfung |
| `cs2sx watch <csproj\|folder>` | Datei-Watcher mit automatischem Rebuild (500ms Debounce) |
| `cs2sx clean <csproj>` | Löscht `cs2sx_out/` vollständig — behebt Ghost-Symbol-Konflikte nach Klassen-Umbenennungen |
| `cs2sx genstubs <include> <out>` | Generiert C#-Stubs aus libnx-Headern |

### Build-Pipeline Stages

```
prepare   → Projektdateien einlesen, Runtime exportieren, veraltete Artefakte bereinigen
fwd-decl  → Forward-Declarations (_forward.h) generieren
generics  → Generics/Interfaces/Extension-Methoden sammeln und expandieren
semantic  → Roslyn SemanticModel aufbauen
transpile → C#-Dateien zu .h/.c transpilieren (inkrementell)
compile   → GCC (aarch64-none-elf) kompilieren
package   → nacptool + elf2nro → .nro
```

Beim `clean`-Befehl werden auch verwaiste `.c`/`.h`-Dateien entfernt, die nach dem Umbenennen einer Klasse entstanden sind.

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
        Graphics.DrawTextShadow(100, 400, "Shadow!", Color.White, Color.RGB(0, 0, 0), 2);
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
    private MinUiColorPreset _uiPreset;

    public override void OnInit()
    {
        Graphics.Init(1280, 720);
        _uiPreset = new MinUiColorPreset(Color.Gray, Color.White, Color.Cyan);
    }

    public override void OnFrame()
    {
        Graphics.FillScreen(Color.Black);
        MinUI.DrawHeader("Meine App", _uiPreset);
    }
}
```

### Audio

```csharp
public class MyApp : SwitchApp
{
    public override void OnInit()
    {
        Graphics.Init(1280, 720);
        Audio.Init(44100);
    }

    public override void OnFrame()
    {
        if (Input.IsDown(NpadButton.A))
            Audio.PlayTone(440.0f, 0.5f, 300);   // 440 Hz, 50% Lautstärke, 300ms

        if (Input.IsDown(NpadButton.B))
            Audio.Stop();
    }
}
```

> **Hinweis:** libnx verwendet intern immer 48000 Hz. Der `sampleRate`-Parameter von `Audio.Init()` wird akzeptiert aber ignoriert.

### SwitchAppEx — erweiterter App-Loop

`SwitchAppEx` ist eine Erweiterung von `SwitchApp` die Analog-Sticks, Touch und Akku automatisch pro Frame aktualisiert:

```csharp
public class MyApp : SwitchAppEx
{
    public override void OnInit()
    {
        Graphics.Init(1280, 720);
    }

    public override void OnFrame()
    {
        Graphics.FillScreen(Color.Black);

        // Sticks direkt verfügbar (automatisch aktualisiert)
        if (stickL.x > 5000)
            Graphics.DrawText(100, 100, "Stick rechts", Color.Green, 2);

        // Touch direkt verfügbar
        for (int i = 0; i < touch.count; i++)
            Graphics.FillCircle(touch.x[i], touch.y[i], 20, Color.Red);

        // Akku (wird alle ~300 Frames aktualisiert)
        Graphics.DrawText(10, 680, $"Akku: {battery.percent}%", Color.White, 1);
    }
}
```

| Feld | Typ | Beschreibung |
|---|---|---|
| `stickL` | `StickPos` | Linker Analog-Stick (automatisch pro Frame) |
| `stickR` | `StickPos` | Rechter Analog-Stick (automatisch pro Frame) |
| `touch` | `TouchState` | Touch-Screen-Zustand (automatisch pro Frame) |
| `battery` | `BatteryInfo` | Akkustand (alle ~300 Frames aktualisiert) |

---

## Unterstützte Features

### Typen & Collections

| Feature | Status | Hinweis |
|---|---|---|
| `string` | ✅ | als `const char*` |
| `int`, `float`, `bool`, `char` | ✅ | direkt gemappt |
| `u8`, `u16`, `u32`, `u64`, `s8`–`s64` | ✅ | libnx-Typen |
| `Result`, `Handle` | ✅ | libnx-Typen |
| `T?` Nullable-Typen | ✅ | `HasValue`, `Value`, `??`, `?.` |
| `List<T>` | ✅ | `Add`, `Remove`, `Clear`, `Contains`, `Sort`, `Reverse`, `IndexOf` |
| `List<string>` | ✅ | `foreach`, `string.Join`, `string.Split` |
| `Dictionary<K,V>` | ✅ | `Add`, `Remove`, `ContainsKey`, `TryGetValue`, Indexer, `foreach` |
| `StringBuilder` | ✅ | `Append`, `AppendLine`, `Clear`, `ToString`, `Insert`, `Replace`, `IndexOf` |
| `int[]`, `float[]`, `string[]` | ✅ | Stack-Arrays mit Initializer |
| `int[,]` mehrdimensionale Arrays | ✅ | wird als flaches 1D-Array transpiliert |
| `IEnumerable<T>`, `IReadOnlyList<T>` | ✅ | wird als `List<T>*` behandelt |
| `params T[]` | ✅ | wird als Pointer + Count-Parameter transpiliert |
| Tuple-Return `(int, string)` | ✅ | wird als generierter C-Struct transpiliert |
| `StickPos` | ✅ | Analog-Stick-Position (`x`, `y`) |
| `TouchState` | ✅ | Touch-Screen-Zustand (`count`, `x[]`, `y[]`, `id[]`) |
| `BatteryInfo` | ✅ | Akkustand (`percent`, `charging`, `connected`) |
| `Texture` | ✅ | Pixel-Buffer für `Graphics.DrawTexture` |

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
float inf = float.PositiveInfinity; // → INFINITY
```

### Nullable-Typen

```csharp
int? x = null;             // → int* x = NULL;
int? x = 5;                // → int _x_val = 5; int* x = &_x_val;
bool hasVal = x.HasValue;  // → (x != NULL)
int val = x.Value;         // → (*x)
int v = x ?? 0;            // → (x != NULL ? *x : 0)
x?.ToString();             // → (x != NULL ? Int_ToString(*x) : NULL)
```

### Tuple-Rückgabe

```csharp
public (int x, int y) GetPos()
{
    return (100, 200);
}

var pos = GetPos();
// pos.x, pos.y direkt verfügbar
```

→ Wird als generierter C-Struct `_Tuple2_int_int` transpiliert.

### String-Methoden

| Methode | Status |
|---|---|
| `Trim`, `TrimStart`, `TrimEnd` | ✅ |
| `ToUpper`, `ToLower` | ✅ |
| `Replace`, `Substring`, `IndexOf`, `LastIndexOf` | ✅ |
| `StartsWith`, `EndsWith`, `Contains`, `Equals` | ✅ |
| `PadLeft`, `PadRight` | ✅ |
| `CompareTo` | ✅ |
| `Split`, `string.Join` | ✅ |
| `string.Format`, `string.Concat` | ✅ |
| `"Hello" + variable.ToString()` | ✅ | → `snprintf`-basierte Konkatenation |
| `IsNullOrEmpty`, `IsNullOrWhiteSpace` | ✅ |
| String-Interpolation `$"..."` | ✅ |
| `String.Length` | ✅ | → `strlen()` |
| `string.Compare` mit `StringComparison` | ✅ | `OrdinalIgnoreCase` wird korrekt erkannt |

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

`ref`- und `out`-Parameter werden korrekt als Zeiger transpiliert und beim Aufruf automatisch mit `&` versehen. `out var` in `if`-Bedingungen wird ebenfalls unterstützt:

```csharp
if (int.TryParse(input, out var n))
    Console.WriteLine($"Parsed: {n}");
```

### params-Parameter

```csharp
public static void Log(string prefix, params string[] messages)
{
    for (int i = 0; i < messages_count; i++)
        Console.WriteLine(prefix + messages[i]);
}

Log("INFO", "Start", "Ready");
// → MyClass_Log("INFO", args, 2)
```

`params`-Parameter werden als Pointer + automatisch generierter `_count`-Parameter transpiliert.

---

## Zufall

CS2SX verwendet einen eingebauten LCG-Zufallsgenerator ohne externe Abhängigkeiten.

```csharp
int n  = Random.Shared.Next(0, 100);
int n2 = Random.Shared.Next(50);
int n3 = System.Random.Shared.Next();
float f = Random.Shared.NextSingle();

var rng = new Random();
int n4 = rng.Next(1, 7);
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

Alle Methoden unterstützen sowohl die Kurzform `Math.X` als auch `System.Math.X`.

```csharp
float d    = Math.Sqrt(x * x + y * y);
float s    = System.Math.Sin(angle);
int   v    = Math.Clamp(value, 0, 100);
int   sign = Math.Sign(delta);
float r    = Math.Round(3.7f);
```

| C#-Methode | C-Ausgabe |
|---|---|
| `Math.Abs` | `abs(x)` |
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
Color.Silver   Color.DarkGray Color.LightGray Color.Maroon
Color.Olive

// Eigene Farben
uint myColor  = Color.RGB(255, 128, 0);
uint myColorA = Color.RGBA(255, 128, 0, 200);

// Alpha-Variante einer bestehenden Farbe
uint halfBlack = Color.Black.WithAlpha(128);
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
| `Graphics.DrawPolygon(xs[], ys[], count, color)` | Beliebiges Polygon |

### Alpha-Blending

| Methode | Beschreibung |
|---|---|
| `Graphics.SetPixelAlpha(x, y, color, alpha)` | Pixel mit Alpha (0=transparent, 255=deckend) |
| `Graphics.FillRectAlpha(x, y, w, h, color, alpha)` | Rechteck mit Alpha |
| `Graphics.DrawTextAlpha(x, y, text, color, scale, alpha)` | Text mit Alpha |

---

## Audio

```csharp
Audio.Init(44100);
Audio.PlayTone(440.0f, 0.5f, 500);
Audio.SetVolume(0.8f);
Audio.Stop();
Audio.Exit();
```

| Methode | Beschreibung |
|---|---|
| `Audio.Init(sampleRate)` | PCM-Audio initialisieren |
| `Audio.PlayTone(freq, amp, ms)` | Sinuston erzeugen (Frequenz Hz, Amplitude 0–1, Dauer ms) |
| `Audio.SetVolume(vol)` | Master-Lautstärke setzen (0.0–1.0) |
| `Audio.Stop()` | Wiedergabe stoppen |
| `Audio.Exit()` | Audio-System und Puffer freigeben |

> Intern werden 4 zirkuläre PCM-Puffer à 4096 Samples verwendet (Stereo, 16-bit, 48000 Hz).

---

## Input

### Buttons

```csharp
if (Input.IsDown(NpadButton.A))   { /* einmalig beim Drücken  */ }
if (Input.IsHeld(NpadButton.ZR))  { /* solange gehalten       */ }
if (Input.IsUp(NpadButton.B))     { /* einmalig beim Loslassen */ }
```

Verfügbare Buttons: `A`, `B`, `X`, `Y`, `L`, `R`, `ZL`, `ZR`, `Plus`, `Minus`, `Up`, `Down`, `Left`, `Right`, `StickL`, `StickR` sowie alle Stick-Richtungen (`StickLUp`, `StickLDown`, `StickLLeft`, `StickLRight`, `StickRUp` usw.).

### Analog-Sticks

```csharp
StickPos left  = Input.GetStickLeft();
StickPos right = Input.GetStickRight();

if (left.x > 5000)
    Console.WriteLine("Stick rechts");
```

> Deadzone ±3000 wird automatisch herausgefiltert. X: negativ = links, positiv = rechts. Y: positiv = oben, negativ = unten.

### Touch-Screen

```csharp
TouchState touch = Input.GetTouch();

if (touch.count > 0)
{
    Graphics.FillCircle(touch.x[0], touch.y[0], 20, Color.Red);
}
```

| Feld | Typ | Beschreibung |
|---|---|---|
| `count` | `int` | Anzahl aktiver Touch-Punkte (max. 10) |
| `x[i]`, `y[i]` | `int` | Koordinaten (0–1280, 0–720) |
| `id[i]` | `uint` | Finger-ID für Multi-Touch-Tracking |

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
Environment.Exit(0);
Environment.Exit(1);
```

---

## File I/O (SD-Karte)

Alle Pfade müssen absolut sein und mit `/switch/` beginnen.

### Dateien

| Methode | Beschreibung |
|---|---|
| `File.ReadAllText(path)` | Datei lesen (max. 1 MB) |
| `File.ReadAllLines(path)` | Datei zeilenweise lesen → `List<string>` |
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
| `Directory.GetCurrentDirectory()` | Gibt `"/switch"` zurück |

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
| `foreach` über `List<T>`, Arrays, `string` | ✅ |
| `foreach` über `Dictionary<K,V>` (Key + Value) | ✅ |
| `switch` (Wert und Pattern) | ✅ |
| `switch` mit `const`-Feldern als case-Labels | ✅ |
| `break`, `continue`, `return` | ✅ |
| `try` / `catch` | ✅ (via `setjmp/longjmp`) |
| `using` (mit `IDisposable`) | ✅ |
| `??` Null-Coalescing | ✅ |
| `??=` Null-Coalescing-Zuweisung | ✅ |
| `?.` Null-Conditional | ✅ |

### foreach über Dictionary

```csharp
var scores = new Dictionary<string, int>();
scores.Add("Alice", 100);
scores.Add("Bob", 80);

foreach (var kvp in scores)
{
    Graphics.DrawText(10, y, $"{kvp.Key}: {kvp.Value}", Color.White, 1);
    y += 20;
}
```

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
            case WALL:  Graphics.SetPixel(x, y, Color.Gray);  break;
            case FOOD:  Graphics.SetPixel(x, y, Color.Green); break;
            case EMPTY: break;
        }
    }
}
```

`const`-Felder werden via Roslyn SemanticModel erkannt und zu `ClassName_WALL` etc. aufgelöst.

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
| Properties mit Body (Getter/Setter) | ✅ | → `ClassName_get_X()` / `ClassName_set_X()` |
| Expression-body Properties (`=> expr`) | ✅ | → Getter-Funktion |
| `IDisposable` / `using` | ✅ | → `Dispose()`-Aufruf am Blockende |
| Enums mit Werten | ✅ | |
| Value-type `struct` | ✅ | → C Stack-Struct, kein `malloc` |
| `IEquatable<T>` auf Structs | ✅ | → `StructName_Equals(a, b)` via `memcmp` |
| Explizite Konstruktoren mit Parametern | ✅ | → `ClassName_New(params...)` |
| Generics | ✅ | Klassen-Expansion zur Build-Zeit |
| `interface` | ✅ | → vtable-Wrapper-Struct |
| Extension-Methoden | ✅ | → freie C-Funktionen |
| `using static` | ✅ | wird via `UsingStaticResolver` aufgelöst |
| `async` / `await` | ⚠️ | Synchroner Fallback mit Warning |

### Felder: mit und ohne `_` Prefix

```csharp
private uint _bgColor;   // → self->f_bgColor
public  uint Background; // → self->f_Background
```

### static readonly Array-Felder

```csharp
public class Snake
{
    private static readonly int[] DX = { 1, 0, -1,  0 };
    private static readonly int[] DY = { 0, 1,  0, -1 };
}
// → static const int Snake_DX[] = {1, 0, -1, 0};
```

### Value-type Structs

```csharp
public struct Vec2
{
    public float X;
    public float Y;
}

Vec2 pos = new Vec2 { X = 1.0f, Y = 2.0f };
Vec2 arr = new Vec2[10];   // → Vec2 arr[10] = {0};
```

Structs werden als C Stack-Structs erzeugt — kein `malloc`. Methoden auf Structs werden als freie C-Funktionen mit `StructName* self`-Parameter transpiliert. `IEquatable<T>` wird automatisch als `memcmp`-basierte `Equals`-Funktion implementiert.

### Generics

```csharp
public class Stack<T>
{
    private T[] _items;
    private int _top;

    public void Push(T item) { _items[_top++] = item; }
    public T Pop() { return _items[--_top]; }
}

var intStack = new Stack<int>();
var strStack = new Stack<string>();
// → Stack_int_New() / Stack_str_New() in _generics.h
```

### Interfaces

```csharp
public interface IRenderable
{
    void Draw();
    int GetWidth();
}

public class MyButton : IRenderable
{
    public void Draw() { /* ... */ }
    public int GetWidth() { return 100; }
}

IRenderable r = myButton.as_IRenderable();
r.Draw();  // → r->vtable->Draw(r->obj)
```

### Extension-Methoden

```csharp
public static class IntExtensions
{
    public static bool IsEven(this int x) => x % 2 == 0;
    public static int Clamp(this int x, int min, int max) => Math.Clamp(x, min, max);
}

if (score.IsEven()) { ... }   // → IntExtensions_IsEven(score)
int v = speed.Clamp(0, 100); // → IntExtensions_Clamp(speed, 0, 100)
```

### using static

```csharp
using static System.Math;

float d = Sqrt(x * x + y * y);  // → sqrtf(...)
float s = Sin(angle);            // → sinf(...)
```

`using static`-Importe werden via `UsingStaticResolver` aufgelöst und korrekt an die zuständigen Handler weitergeleitet.

### Eigene Controls

```csharp
public class ValueMeter : Control
{
    private int _value;
    private int _maxValue;
    private int _width;

    public override void Draw()
    {
        int filled = _maxValue > 0 ? (_value * _width) / _maxValue : 0;
        Console.Write($"\x1b[{base.Y};{base.X}H[");
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

Im Framebuffer-Modus sind Console-Controls nicht sichtbar. Im Console-Modus stehen `Label`, `Button` (mit Fokus-Navigation via D-Pad) und `ProgressBar` zur Verfügung.

---

## Projektstruktur

```
MeinProjekt/
├── MeinProjekt.csproj
├── cs2sx.json
├── icon.jpg                    — App-Icon (256x256 JPEG)
├── README.md
├── Program.cs
├── MeineKlasse.cs
└── cs2sx_out/                  — generierter C-Code (nicht manuell bearbeiten)
    ├── _forward.h              — Forward-Declarations aller Typen
    ├── _generics.h / .c        — expandierte Generic-Klassen
    ├── _interfaces.h           — vtable-Structs für Interfaces
    ├── switchforms.c           — Runtime-Globals
    ├── switchforms.h           — Runtime: UI, Collections, String-Utils, File I/O
    ├── switchapp.h             — Runtime: SwitchApp-Loop, Grafik, Input, Audio
    ├── switchapp_ext.h         — SwitchAppEx + DrawPolygon
    ├── main.c                  — Auto-generierter Einstiegspunkt
    ├── MeineKlasse.h/.c        — Transpilierter Code
    └── MeinProjekt.elf
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

## Diagnostics & Fehlermeldungen

Der Transpiler gibt Warnings aus wenn Konstrukte nicht vollständig unterstützt werden:

```
Game.cs(42): unknown call 'Foo.Bar' — passed through as-is, verify generated C
Game.cs(17): Task.Run — executed synchronously (no threading on Switch)
```

GCC-Fehlermeldungen werden automatisch auf die ursprünglichen C#-Quellzeilen zurückverfolgt:

```
Game.c:88:5: error: 'foo' undeclared
    → C# Game.cs(42): someMethod()
```

---

## Architektur

```
CS2SX/
├── Core/
│   ├── TypeRegistry.cs              — einzige Quelle aller Typ-Mappings
│   ├── TranspilerContext.cs         — geteilter Zustand, kein globaler State
│   ├── TranspileResult.cs           — Rückgabeobjekt mit Code + Diagnostics
│   └── DiagnosticReporter.cs        — Warnings/Errors + GCC Source-Mapping
├── Transpiler/
│   ├── Handlers/
│   │   ├── InvocationDispatcher.cs  — orchestriert alle Handler
│   │   ├── AsyncHandler.cs          — Task.Run/Delay → synchroner Fallback
│   │   ├── AudioHandler.cs          — Audio.Init/PlayTone/Stop
│   │   ├── ColorHandler.cs          — Color.RGB, Color.WithAlpha
│   │   ├── ConsoleHandler.cs        — Console.Write/WriteLine
│   │   ├── DictionaryHandler.cs     — Dictionary<K,V> Methoden
│   │   ├── DirectoryExtHandler.cs   — GetDirectories, GetEntries
│   │   ├── EnvironmentHandler.cs    — Environment.Exit, Console.Clear
│   │   ├── ExtensionMethodHandler.cs— Extension-Methoden via SemanticModel
│   │   ├── FieldMethodHandler.cs    — _field.Method() Aufrufe
│   │   ├── FileHandler.cs           — File.X + Directory.X
│   │   ├── FormHandler.cs           — Form.Add
│   │   ├── GraphicsExtHandler.cs    — Dreieck, Ellipse, Alpha, Grid
│   │   ├── GraphicsHandler.cs       — Graphics.X Basis-Primitiven
│   │   ├── InputExtHandler.cs       — Sticks, Touch
│   │   ├── InputHandler.cs          — Input.IsDown/IsHeld/IsUp
│   │   ├── LibNxHandler.cs          — LibNX.X() Aufrufe
│   │   ├── ListHandler.cs           — List<T> Methoden
│   │   ├── MathHandler.cs           — Math.X + System.Math.X
│   │   ├── OwnMethodHandler.cs      — eigene Methoden
│   │   ├── ParseHandler.cs          — int.Parse, float.TryParse
│   │   ├── PathHandler.cs           — Path.GetFileName, Combine
│   │   ├── RandomHandler.cs         — Random.Shared.Next, NextSingle
│   │   ├── StaticClassHandler.cs    — static class Aufrufe
│   │   ├── StringBuilderHandler.cs  — StringBuilder Methoden
│   │   ├── StringConcatHandler.cs   — "string" + variable → snprintf
│   │   ├── StringMethodHandler.cs   — String.X + Instanz-Methoden
│   │   └── SystemExtHandler.cs      — System.GetBattery
│   ├── Strategies/
│   │   ├── SwitchAppConstructorStrategy.cs
│   │   ├── ControlSubclassConstructorStrategy.cs
│   │   └── DefaultConstructorStrategy.cs
│   ├── Writers/
│   │   ├── ExpressionWriter.cs
│   │   ├── FormatStringBuilder.cs
│   │   ├── NullableAndPatternWriter.cs
│   │   ├── StatementWriter.cs
│   │   ├── StringEscaper.cs
│   │   ├── StructWriter.cs
│   │   └── TypeInferrer.cs
│   ├── CSharpToC.cs
│   ├── GenericExpander.cs
│   ├── GenericInstantiationCollector.cs
│   ├── InterfaceExpander.cs
│   ├── LambdaLifter.cs
│   ├── PropertyWriter.cs
│   ├── UsingStaticResolver.cs
│   └── VTableBuilder.cs
├── Build/
│   ├── BuildPipeline.cs
│   ├── CCompiler.cs
│   ├── CheckCommand.cs
│   ├── CleanCommand.cs
│   ├── EntryPointGenerator.cs
│   ├── NacpBuilder.cs
│   ├── NroBuilder.cs
│   ├── ProjectConfig.cs
│   ├── ProjectCreator.cs
│   ├── ProjectReader.cs
│   ├── RuntimeExporter.cs
│   ├── SemanticModelBuilder.cs
│   ├── StubGenerator.cs
│   └── WatchCommand.cs
└── Runtime/
    ├── switchforms.h      — UI-Controls, Collections, String-Utils, File I/O
    ├── switchforms.c      — ODR-sichere Globaldefinitionen
    ├── switchapp.h        — SwitchApp-Loop, Framebuffer, Graphics, Input, System
    ├── switchapp_ext.h    — SwitchAppEx, DrawPolygon
    └── AudioStub.h        — PCM-Audio via libnx audout
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

| Einschränkung | Details |
|---|---|
| Ein `SwitchApp`-Subtyp pro Projekt | Nur eine Haupt-App-Klasse |
| Eine Klasse pro `.cs`-Datei | Keine verschachtelten Klassen |
| String-Puffer 512 Bytes | Für `snprintf`-basierte Interpolation |
| Datei-Lesepuffer max. 1 MB | `File.ReadAllText` |
| Bitmap-Font 8×8 | Kein Anti-Aliasing, kein TrueType |
| Kein Heap-GC | Allokierte Objekte (`*_New()`) leben bis `_Free()` |
| Lambda-Captures | Nur Werttypen und primitive Captures zuverlässig |
| `is`-Typ-Pattern | Erfordert `TypeName_Is()`-Funktion in der Runtime |
| Mehrdimensionale Arrays | `int[,]` wird als flaches 1D-Array transpiliert |
| `async`/`await` | Synchroner Fallback mit Warning, kein echtes Threading |
| LINQ | Nicht unterstützt |
| Mehrfachvererbung | Nicht unterstützt |
| `Console.ReadLine` | Nicht unterstützt (kein Keyboard-Input auf Switch) |

---

## Lizenz

MIT