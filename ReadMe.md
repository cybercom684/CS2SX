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

# Externe C-Library einbinden
cs2sx addLib <libName> [project.csproj]

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
| `cs2sx addLib <libName>` | Bindet eine externe C-Library ein (Stubs + cs2sx.json-Eintrag) |
| `cs2sx genstubs <include> <out>` | Generiert C#-Stubs aus libnx-Headern |

### Build-Pipeline Stages

```
prepare   → Projektdateien einlesen, Runtime exportieren, veraltete Artefakte bereinigen
fwd-decl  → Forward-Declarations (_forward.h) generieren, Custom-Header einbinden
generics  → Generics/Interfaces/Extension-Methoden sammeln und expandieren
semantic  → Roslyn SemanticModel aufbauen
transpile → C#-Dateien zu .h/.c transpilieren (inkrementell)
compile   → GCC (aarch64-none-elf) kompilieren
package   → nacptool + elf2nro → .nro
```

Beim `clean`-Befehl werden auch verwaiste `.c`/`.h`-Dateien entfernt, die nach dem Umbenennen einer Klasse entstanden sind.

---

## Externe C-Libraries einbinden (`addLib`)

CS2SX kann beliebige C-Libraries direkt in den Build einbinden. Der Workflow:

### 1. Library-Quellen ablegen

```
MeinProjekt/
└── externLibs/
    └── mylib/
        ├── include/
        │   └── mylib.h
        └── src/
            └── mylib.c
```

### 2. addLib ausführen

```bash
cs2sx addLib mylib
```

Das generiert:

- `MylibStubs/` — C#-Stub-Dateien für IDE-Unterstützung (werden **nicht** transpiliert)
- Eintrag in `cs2sx.json` unter `externLibs`

### 3. cs2sx.json anpassen (falls nötig)

Für einfache Libraries reicht der automatisch generierte Eintrag. Komplexe Libraries (wie Freetype) brauchen manuelle Anpassung:

```json
{
  "name": "MeinProjekt",
  "externLibs": [
    {
      "name": "Mylib",
      "includeDir": "externLibs/mylib/include",
      "extraIncludeDirs": [
        "externLibs/mylib/src"
      ],
      "defines": [
        "MYLIB_IMPLEMENTATION"
      ],
      "sources": [
        "externLibs/mylib/mylib_build.c"
      ]
    }
  ]
}
```

| Feld | Beschreibung |
|---|---|
| `includeDir` | Haupt-Include-Verzeichnis (`-I` Flag) |
| `extraIncludeDirs` | Zusätzliche Include-Verzeichnisse (für Libraries mit internen Sub-Includes) |
| `defines` | Präprozessor-Definitionen (`-D` Flags) |
| `sources` | Zu compilierende `.c`-Dateien |

### 4. Shim-Header anlegen (falls nötig)

Wenn die Library komplexe Pointer-Typen hat die der Transpiler nicht kennt, legt man einen Shim-Header im Projektverzeichnis ab. Dieser wird automatisch in `_forward.h` eingebunden und ist damit in jeder Translation Unit sichtbar.

```c
// mylib_shim.h — liegt im Projektverzeichnis
#pragma once
#include "mylib_main.h"

// Wrapper-Funktionen mit einfachen Typen
static inline int Mylib_DoSomething(unsigned long long handle, int param)
{
    return mylib_do_something((mylib_handle_t)(uintptr_t)handle, param);
}
```

In C# nutzt man `ulong` als Ersatz für Pointer-Typen (wird zu `unsigned long long` in C):

```csharp
public class MyApp : SwitchApp
{
    private ulong _handle;   // ← ulong statt IntPtr oder void*

    public override void OnInit()
    {
        Mylib.Init(ref _handle);
    }
}
```

### Warum ulong statt IntPtr oder void*?

| Typ | Problem |
|---|---|
| `IntPtr` / `nint` | Wird zu `intptr_t` — funktioniert, aber vorzeichenbehaftet; keine Zeiger-Arithmetik-Semantik |
| `void*` | Wird vom Transpiler als Pointer-Feld behandelt → falscher Destruktor, falsche Signaturen |
| `ulong` | Wird zu `unsigned long long` → 8 Byte auf AArch64, passt für jeden opaken Handle-Wert |

### Wichtige Regeln für addLib-Projekte

**Kein `$"..."` für String-Felder:**
```csharp
// FALSCH — dangling pointer im generierten C
_status = $"Fehler: {errCode}";

// RICHTIG — String-Literal
_status = "Fehler aufgetreten";
```
Interpolierte Strings erzeugen Stack-Buffer (`snprintf`) die nicht in Felder gespeichert werden dürfen. In lokalen Variablen oder direkt in `Graphics.DrawText`/`Console.WriteLine` sind sie weiterhin verwendbar.

**`Input.IsDown()` statt direktem `kDown`:**
```csharp
// FALSCH — kDown ist in Subklassen-Methoden nicht direkt zugänglich
if ((kDown & NpadButton.A) != 0) { }

// RICHTIG
if (Input.IsDown(NpadButton.A)) { }
```

**Single-file Build für komplexe Libraries:**
Libraries wie Freetype erwarten einen Amalgamation-Build — eine einzige `.c`-Datei die alle Module includiert:

```c
// freetype_build.c — in externLibs/freetype/
#include "src/base/ftsystem.c"
#include "src/base/ftinit.c"
// ...alle weiteren Module
```

### Vollständiges Beispiel: Freetype

```
fontSwitch/
├── Program.cs
├── freetype_main.h      ← Shim mit allen Wrapper-Funktionen
├── cs2sx.json           ← extraIncludeDirs + defines + ein Build-File
└── externLibs/
    └── freetype/
        ├── freetype_build.c   ← Single-file Amalgamation
        ├── include/           ← öffentliche Header
        └── src/               ← Quellcode-Module
```

```csharp
public class FontSwitchApp : SwitchApp
{
    private ulong _library;   // FT_Library als ulong
    private ulong _face;      // FT_Face als ulong

    public override void OnInit()
    {
        Graphics.Init(1280, 720);

        int err = Freetype.FT_Init_FreeType(ref _library);
        if (err != 0) { _status = "FT_Init_FreeType fehlgeschlagen"; return; }

        err = Freetype.FT_New_Face(_library, "font.ttf", 0, ref _face);
        if (err != 0) { _status = "font.ttf nicht gefunden"; return; }

        Freetype.FT_Set_Char_Size(_face, 0, 32 * 64, 96, 96);
        _ftReady = true;
    }
}
```

> **Hinweis:** `font.ttf` muss relativ zur NRO-Datei liegen. Freetype öffnet Dateien relativ zum Startverzeichnis der App.

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

### App mit List\<UserClass\>

```csharp
// Item.cs
public class Item
{
    public string Name { get; set; }
    public int Value  { get; set; }

    public Item(string name, int value)
    {
        Name  = name;
        Value = value;
    }

    public string Label() => $"{Name}: {Value}";
}

// Program.cs
public class MyApp : SwitchApp
{
    private List<Item> _items;

    public override void OnInit()
    {
        Graphics.Init(1280, 720);
        _items = new List<Item>();
        _items.Add(new Item("Schwert", 50));
        _items.Add(new Item("Schild", 30));
    }

    public override void OnFrame()
    {
        Graphics.FillScreen(Color.Black);
        for (int i = 0; i < _items.Count; i++)
        {
            Item it = _items[i];
            Graphics.DrawText(50, 50 + i * 30, it.Label(), Color.White, 1);
        }
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
            Audio.PlayTone(440.0f, 0.5f, 300);

        if (Input.IsDown(NpadButton.B))
            Audio.Stop();
    }
}
```

> **Hinweis:** libnx verwendet intern immer 48000 Hz. Der `sampleRate`-Parameter von `Audio.Init()` wird akzeptiert aber ignoriert.

### SwitchAppEx — erweiterter App-Loop

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

        if (stickL.x > 5000)
            Graphics.DrawText(100, 100, "Stick rechts", Color.Green, 2);

        for (int i = 0; i < touch.count; i++)
            Graphics.FillCircle(touch.x[i], touch.y[i], 20, Color.Red);

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
| `ulong` | ✅ | `unsigned long long` — nützlich als Pointer-Ersatz für externe Libraries |
| `u8`, `u16`, `u32`, `u64`, `s8`–`s64` | ✅ | libnx-Typen |
| `Result`, `Handle` | ✅ | libnx-Typen |
| `T?` Nullable-Typen | ✅ | `HasValue`, `Value`, `??`, `?.` |
| `List<T>` mit Primitiven / Strings | ✅ | `Add`, `Remove`, `Clear`, `Contains`, `Sort`, `Reverse`, `IndexOf`, `Insert` |
| `List<UserClass>` | ✅ | Listen eigener Klassen — Instanzen werden heap-allokiert (`CS2SX_LIST_DEFINE_PTR`) |
| `List<string>` | ✅ | `foreach`, `string.Join`, `string.Split` |
| `Dictionary<K,V>` | ✅ | `Add`, `Remove`, `ContainsKey`, `TryGetValue`, Indexer, `foreach` |
| `Stack<T>` | ✅ | `Push`, `Pop`, `Peek`, `Clear`, `Count` — mit Underflow-Guard |
| `Queue<T>` | ✅ | `Enqueue`, `Dequeue`, `Peek`, `Clear`, `Count` — mit Underflow-Guard |
| `HashSet<T>` | ✅ | `Add`, `Contains`, `Remove`, `Clear`, `UnionWith`, `IntersectWith`, `ExceptWith` |
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
public (int x, int y) GetPos() { return (100, 200); }
var pos = GetPos();
// pos.x, pos.y direkt verfügbar
```

### Generic Methods

Benutzerdefinierte generische Methoden werden am Aufruf-Punkt zu typisierten C-Funktionen spezialisiert:

```csharp
public static T Clamp<T>(T val, T min, T max)
    => val < min ? min : val > max ? max : val;

float speed = Clamp<float>(speed, 0f, 100f);
// → MyClass_Clamp_float(speed, 0.0f, 100.0f)
```

Unterstützt werden generische Methoden mit einem oder mehreren Typ-Parametern und optionalen `where`-Constraints. Die Spezialisierungen werden automatisch in `_generics.h/.c` emittiert.

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
| `"Hello" + variable.ToString()` | ✅ |
| `IsNullOrEmpty`, `IsNullOrWhiteSpace` | ✅ |
| String-Interpolation `$"..."` | ✅ |
| `String.Length` | ✅ |
| `string.Compare` mit `StringComparison` | ✅ |

### Parsing

| Methode | Status |
|---|---|
| `int.Parse` / `int.TryParse` | ✅ |
| `float.Parse` / `float.TryParse` | ✅ |
| `double.Parse` / `double.TryParse` | ✅ |
| `long.Parse` / `long.TryParse` | ✅ |
| `ulong.Parse` / `ulong.TryParse` | ✅ |
| `uint.Parse` / `uint.TryParse` | ✅ |
| `short`/`ushort`/`byte`/`sbyte`/`bool` Parse+TryParse | ✅ |

### ref / out Parameter

```csharp
public void Swap(ref int a, ref int b) { int tmp = a; a = b; b = tmp; }
int x = 1, y = 2;
Swap(ref x, ref y);

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
```

---

## LINQ

LINQ-Methodenketten werden in äquivalente C-Schleifen expandiert. Alle Ergebnis-Listen werden heap-allokiert und stehen als normale `List<T>*` zur Verfügung.

| Methode | Beschreibung |
|---|---|
| `.Where(pred)` | Gefilterte neue Liste |
| `.Select(proj)` | Projizierte neue Liste |
| `.First(pred?)` | Erstes Element (optional mit Bedingung) |
| `.FirstOrDefault(pred?)` | Erstes Element oder `0`/`NULL` |
| `.Last()` / `.LastOrDefault()` | Letztes Element |
| `.Single(pred?)` / `.SingleOrDefault(pred?)` | Erstes Element (semantisch wie `First`) |
| `.Any(pred?)` | `1` wenn mindestens ein Element (optional) die Bedingung erfüllt |
| `.All(pred)` | `1` wenn alle Elemente die Bedingung erfüllen |
| `.Count(pred?)` | Anzahl (optional mit Bedingung) |
| `.Sum(proj?)` | Summe (optional mit Projektor) |
| `.Min(proj?)` / `.Max(proj?)` | Minimum/Maximum (optional mit Projektor) |
| `.Average(proj?)` | Durchschnitt (optional mit Projektor) |
| `.Aggregate(seed, func)` | Fold mit Akkumulator |
| `.ToList()` / `.ToArray()` | Kopie als neue Liste |
| `.OrderBy(key)` / `.OrderByDescending(key)` | Sortierte Kopie (Insertion-Sort) |
| `.ThenBy(key)` / `.ThenByDescending(key)` | Nachsortierung (neue Kopie) |
| `.Contains(val)` | Enthält-Prüfung |
| `.Distinct()` | Deduplizierte Kopie |
| `.Skip(n)` / `.Take(n)` | Teilmenge |
| `.Concat(other)` | Zusammengeführte Liste |
| `.Reverse()` | Umgekehrte Kopie |
| `.ElementAt(i)` / `.ElementAtOrDefault(i)` | Element per Index |
| `.TakeWhile(pred)` | Elemente solange Bedingung wahr |
| `.SkipWhile(pred)` | Elemente überspringen solange Bedingung wahr |
| `.SelectMany(proj)` | Verschachtelte Listen flachklopfen |
| `.GroupBy(key)` | Gruppierung → `Dictionary<Key, List<T>>` |
| `.Join(inner, outerKey, innerKey, result)` | Zwei Listen verbinden |
| `.Zip(other, (a,b) => …)` | Parallel über zwei Listen iterieren |
| `.ToDictionary(key, val?)` | Liste → `Dictionary<K,V>` |
| `.ToHashSet()` | Liste → `HashSet<T>` |
| `.Except(other)` | Differenz zweier Listen |
| `.Intersect(other)` | Schnittmenge zweier Listen |
| `.Union(other)` | Vereinigung zweier Listen (dedupliziert) |

```csharp
List<int> scores = new List<int>();
scores.Add(42); scores.Add(7); scores.Add(99);

var high  = scores.Where(s => s > 40).ToList();
var top   = scores.OrderByDescending(s => s).First();
int total = scores.Sum();
double avg = scores.Average();
bool any  = scores.Any(s => s > 90);
```

---

## Zufall

```csharp
int n  = Random.Shared.Next(0, 100);
float f = Random.Shared.NextSingle();
var rng = new Random();
int n4 = rng.Next(1, 7);
```

| Methode | C-Ausgabe |
|---|---|
| `Next(min, max)` | `CS2SX_Rand_Next(min, max)` |
| `Next(max)` | `CS2SX_Rand_NextMax(max)` |
| `Next()` | `CS2SX_Rand_Next(0, 32767)` |
| `NextInt64()` | `CS2SX_Rand_NextInt64()` |
| `NextSingle()` / `NextFloat()` | `CS2SX_Rand_Float()` |

---

## Mathematik

```csharp
float d = Math.Sqrt(x * x + y * y);
int   v = Math.Clamp(value, 0, 100);
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
uint myColor  = Color.RGB(255, 128, 0);
uint myColorA = Color.RGBA(255, 128, 0, 200);
uint halfBlack = Color.Black.WithAlpha(128);
```

Vordefiniert: `Black`, `White`, `Red`, `Green`, `Blue`, `Yellow`, `Cyan`, `Magenta`, `Gray`, `Orange`, `Pink`, `Purple`, `Brown`, `Teal`, `Lime`, `Navy`, `Silver`, `DarkGray`, `LightGray`, `Maroon`, `Olive`.

---

## Grafik (Framebuffer)

Aktivierung: `Graphics.Init(1280, 720)` in `OnInit()`.

### Basis-Primitiven

| Methode | Beschreibung |
|---|---|
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
| `Graphics.BeginFrame()` / `Graphics.EndFrame()` | Manuelles Frame-Management |

### Erweiterte Primitiven

| Methode | Beschreibung |
|---|---|
| `Graphics.DrawTriangle(x0,y0, x1,y1, x2,y2, color)` | Dreieck-Outline |
| `Graphics.FillTriangle(x0,y0, x1,y1, x2,y2, color)` | Gefülltes Dreieck |
| `Graphics.DrawEllipse(cx, cy, rx, ry, color)` | Ellipse-Outline |
| `Graphics.FillEllipse(cx, cy, rx, ry, color)` | Gefüllte Ellipse |
| `Graphics.DrawRoundedRect(x, y, w, h, r, color)` | Abgerundetes Rechteck-Outline |
| `Graphics.FillRoundedRect(x, y, w, h, r, color)` | Gefülltes abgerundetes Rechteck |
| `Graphics.DrawTextShadow(x, y, text, color, shadow, scale)` | Text mit Schatten |
| `Graphics.SetPixelAlpha(x, y, color, alpha)` | Pixel mit Alpha-Blending |
| `Graphics.FillRectAlpha(x, y, w, h, color, alpha)` | Rechteck mit Alpha |
| `Graphics.DrawTextAlpha(x, y, text, color, scale, alpha)` | Text mit Alpha |
| `Graphics.DrawGrid(x, y, w, h, cols, rows, color)` | Raster zeichnen |
| `Graphics.DrawPolygon(points, count, color)` | Polygon-Outline |

---

## Audio

```csharp
Audio.Init(44100);
Audio.PlayTone(440.0f, 0.5f, 500);
Audio.SetVolume(0.8f);
Audio.Stop();
```

---

## Input

```csharp
if (Input.IsDown(NpadButton.A))  { /* einmalig beim Drücken  */ }
if (Input.IsHeld(NpadButton.ZR)) { /* solange gehalten       */ }
if (Input.IsUp(NpadButton.B))    { /* einmalig beim Loslassen */ }

StickPos left  = Input.GetStickLeft();
StickPos right = Input.GetStickRight();
TouchState touch = Input.GetTouch();

// Touch-Treffer-Test
if (touch.HitRect(100, 200, 80, 40))  // x, y, width, height
    DoSomething();
```

---

## System

```csharp
BatteryInfo battery = System.GetBattery();
Environment.Exit(0);
```

---

## File I/O (SD-Karte)

Alle Pfade müssen absolut sein und mit `/switch/` beginnen — außer beim Start über `addLib`-Libraries, wo relative Pfade zur NRO möglich sind.

| Methode | Beschreibung |
|---|---|
| `File.ReadAllText(path)` | Datei lesen (max. 1 MB) |
| `File.ReadAllLines(path)` | Zeilenweise lesen → `List<string>` |
| `File.WriteAllText(path, content)` | Datei schreiben |
| `File.Exists(path)` | Existenz prüfen |
| `Directory.CreateDirectory(path)` | Verzeichnis anlegen |
| `Directory.GetFiles(path, pattern)` | Dateien → `List<string>` |
| `Path.Combine(a, b)` | Pfade kombinieren |

---

## Kontrollfluss

| Feature | Status |
|---|---|
| `if`, `else if`, `else` | ✅ |
| `for`, `foreach`, `while`, `do...while` | ✅ |
| `foreach` über `List<T>`, Arrays, `string`, `Dictionary<K,V>` | ✅ |
| `switch` (Wert und Pattern) | ✅ |
| `try` / `catch` | ✅ (via `setjmp/longjmp`) |
| `lock` | ✅ | No-op mit Warning — Switch ist single-threaded |
| `using` (mit `IDisposable`) | ✅ |
| `??`, `??=`, `?.` | ✅ |

---

## Pattern Matching

```csharp
string label = value switch { 0 => "zero", 1 => "one", _ => "other" };
string category = score switch { >= 90 => "A", >= 70 => "B", _ => "C" };
if (obj is Dog d) { d.Bark(); }
```

---

## Klassen & OOP

| Feature | Status |
|---|---|
| Klassen mit Feldern und Methoden | ✅ |
| `static class` | ✅ |
| Vererbung (einzeln) | ✅ |
| `abstract`, `virtual`, `override` | ✅ |
| Auto-Properties / Properties mit Body | ✅ |
| Enums | ✅ |
| Value-type `struct` | ✅ |
| Generics (Klassen) | ✅ |
| Generics (Methoden) | ✅ — Typ-Spezialisierung am Aufruf-Punkt |
| `interface` | ✅ |
| Extension-Methoden | ✅ |
| `record` | ✅ | Wird als Klasse mit Auto-Properties transpiliert |
| Named / optionale Parameter | ✅ | Neuordnung per SemanticModel; Defaults automatisch injiziert |
| `using static` | ✅ |
| `async` / `await` | ⚠️ Synchroner Fallback |

---

## Projektstruktur

```
MeinProjekt/
├── MeinProjekt.csproj
├── cs2sx.json
├── icon.jpg
├── Program.cs
├── mylib_main.h           ← optionaler Shim für externe Libraries
├── cs2sx_out/             ← generierter C-Code
│   ├── _forward.h         ← Forward-Declarations + Custom-Header-Includes
│   ├── _generics.h / .c   ← expandierte Generics + List<UserClass>-Typen
│   ├── _interfaces.h
│   ├── switchforms.c / .h
│   ├── switchapp.h
│   └── main.c
└── externLibs/            ← externe C-Libraries
    └── mylib/
        ├── mylib_build.c  ← Amalgamation-Wrapper
        ├── include/
        └── src/
```

`cs2sx.json` mit externer Library:

```json
{
    "name": "MeinProjekt",
    "author": "Dein Name",
    "version": "1.0.0",
    "mainClass": "MyApp",
    "icon": "icon.jpg",
    "externLibs": [
        {
            "name": "Mylib",
            "includeDir": "externLibs/mylib/include",
            "extraIncludeDirs": ["externLibs/mylib/src"],
            "defines": ["MYLIB_IMPLEMENTATION"],
            "sources": ["externLibs/mylib/mylib_build.c"]
        }
    ]
}
```

---

## Bekannte Einschränkungen

| Einschränkung | Details |
|---|---|
| Ein `SwitchApp`-Subtyp pro Projekt | Nur eine Haupt-App-Klasse |
| Eine Klasse pro `.cs`-Datei | Keine verschachtelten Klassen |
| String-Puffer 512 Bytes | Für `snprintf`-basierte Interpolation |
| `$"..."` nicht in Felder speichern | Erzeugt Stack-Buffer → dangling pointer. Nur in lokalen Variablen oder direkt in Ausgabe-Funktionen verwenden |
| `IntPtr` / `nint` | Unterstützt → `intptr_t`; für opake C-Handles ist `ulong` weiterhin die sicherere Wahl |
| `void*` als Felder | Nicht empfohlen — Transpiler behandelt es als Pointer-Feld → falscher Destruktor; `ulong` verwenden |
| `Input.IsDown()` statt `kDown` | Direktes `kDown` in Subklassen-Methoden ist nicht erreichbar — immer `Input.IsDown/IsHeld/IsUp()` verwenden |
| Datei-Lesepuffer max. 1 MB | `File.ReadAllText` |
| Bitmap-Font 8×8 | Kein Anti-Aliasing, kein TrueType (Freetype via `addLib` als Alternative) |
| Kein Heap-GC | Allokierte Objekte (`*_New()`) leben bis `_Free()` |
| `async`/`await` | Synchroner Fallback mit Warning |
| Mehrfachvererbung | Nicht unterstützt |

---

## Lizenz

MIT
