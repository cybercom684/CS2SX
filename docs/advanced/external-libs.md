# Externe C-Libraries einbinden

CS2SX kann beliebige C-Libraries direkt in den Build einbinden. Der Workflow besteht aus drei Schritten.

---

## Schritt 1: Library-Quellen ablegen

```
MeinProjekt/
└── externLibs/
    └── mylib/
        ├── include/
        │   └── mylib.h
        └── src/
            └── mylib.c
```

## Schritt 2: addLib ausführen

```bash
cs2sx addLib mylib
```

Das generiert automatisch:
- `MylibStubs/` — C#-Stub-Klassen für IDE-Unterstützung (werden **nicht** transpiliert)
- Eintrag in `cs2sx.json` unter `externLibs`

## Schritt 3: cs2sx.json prüfen / anpassen

```json
{
  "name": "MeinProjekt",
  "externLibs": [
    {
      "name": "Mylib",
      "includeDir": "externLibs/mylib/include",
      "sources": [
        "externLibs/mylib/src/mylib.c"
      ]
    }
  ]
}
```

| Feld | Beschreibung |
|------|-------------|
| `includeDir` | Haupt-Include-Verzeichnis (`-I` Flag) |
| `extraIncludeDirs` | Zusätzliche Include-Verzeichnisse |
| `defines` | Präprozessor-Definitionen (`-D` Flags) |
| `sources` | Zu kompilierende `.c`-Dateien |

---

## Opake Handle-Typen

Viele C-Libraries verwenden opake Pointer-Typen (`void*`, `FT_Library`, custom structs). In C# verwendet man dafür `ulong`:

```csharp
public class MeineApp : SwitchApp
{
    private ulong _handle;   // FT_Library, SDL_Window o.ä. als ulong

    public override void OnInit()
    {
        MyLib.Init(ref _handle);
    }
}
```

| Typ | Problem |
|-----|---------|
| `IntPtr`/`nint` | Vorzeichenbehaftet; keine Pointer-Arithmetik-Semantik |
| `void*` | Transpiler behandelt es als Pointer-Feld → falscher Destruktor |
| `ulong` | 8 Byte auf AArch64, passt für jeden opaken Handle-Wert ✓ |

---

## Shim-Header für komplexe Libraries

Wenn die Library komplexe Pointer-Typen hat die der Transpiler nicht kennt, legt man einen Shim-Header im Projektverzeichnis ab:

```c
// mylib_shim.h  (liegt im Projektverzeichnis, wird automatisch eingebunden)
#pragma once
#include "mylib.h"

// Wrapper mit einfachen Typen statt komplexer C-Typen
static inline int Mylib_DoSomething(unsigned long long handle, int param)
{
    return mylib_do_something((mylib_handle_t)(uintptr_t)handle, param);
}
```

Dateien mit dem Suffix `_shim.h` oder `_main.h` im Projektverzeichnis werden automatisch in `_forward.h` eingebunden und sind in jeder Translation Unit sichtbar.

---

## Vollständiges Beispiel: Freetype

### Verzeichnisstruktur

```
fontSwitch/
├── Program.cs
├── FontSwitchApp.cs
├── freetype_main.h          ← Shim mit Wrapper-Funktionen
├── cs2sx.json
└── externLibs/
    └── freetype/
        ├── freetype_build.c ← Single-file Amalgamation
        ├── include/         ← öffentliche FreeType-Header
        └── src/             ← Quellcode-Module
```

### freetype_main.h (Shim)

```c
#pragma once
#include "freetype/include/freetype/freetype.h"

// Einfache Wrapper damit C#-Code nur ulong braucht
static inline int Freetype_Init(unsigned long long* lib)
{
    return FT_Init_FreeType((FT_Library*)(uintptr_t*)lib);
}

static inline int Freetype_NewFace(unsigned long long lib,
    const char* path, long idx, unsigned long long* face)
{
    return FT_New_Face((FT_Library)(uintptr_t)lib, path, idx,
                       (FT_Face*)(uintptr_t*)face);
}
```

### cs2sx.json

```json
{
  "name": "fontSwitch",
  "externLibs": [
    {
      "name": "Freetype",
      "includeDir": "externLibs/freetype/include",
      "extraIncludeDirs": [
        "externLibs/freetype/src"
      ],
      "defines": [
        "FT2_BUILD_LIBRARY"
      ],
      "sources": [
        "externLibs/freetype/freetype_build.c"
      ]
    }
  ]
}
```

### freetype_build.c (Amalgamation)

```c
// Alle FreeType-Module in einer Datei
#include "src/base/ftsystem.c"
#include "src/base/ftinit.c"
#include "src/base/ftdebug.c"
#include "src/base/ftbase.c"
#include "src/base/ftbbox.c"
#include "src/base/ftglyph.c"
#include "src/truetype/truetype.c"
#include "src/sfnt/sfnt.c"
#include "src/smooth/smooth.c"
```

### C#-App

```csharp
public class FontSwitchApp : SwitchApp
{
    private ulong _library;
    private ulong _face;
    private bool _bereit = false;

    public override void OnInit()
    {
        Graphics.Init(1280, 720);

        if (Freetype.Init(ref _library) != 0) return;
        if (Freetype.NewFace(_library, "/switch/fontSwitch/font.ttf", 0, ref _face) != 0) return;

        _bereit = true;
    }

    public override void OnFrame()
    {
        Graphics.FillScreen(Color.Black);

        if (!_bereit)
        {
            Graphics.DrawText(100, 100, "FreeType konnte nicht initialisiert werden", Color.Red, 2);
        }
        else
        {
            Graphics.DrawText(100, 100, "FreeType bereit!", Color.Green, 2);
            // Hier eigene Schriftrendering-Logik
        }

        if (Input.IsDown(NpadButton.Plus))
            Environment.Exit(0);
    }
}
```

---

## Wichtige Regeln

**Kein `$"..."` in Feldern speichern:**
```csharp
// FALSCH — dangling pointer im generierten C
_status = $"Fehler: {errCode}";

// RICHTIG — direkt ausgeben oder String-Literal
Console.WriteLine($"Fehler: {errCode}");
_status = "Fehler aufgetreten";
```

**Library-Ressourcen in OnExit() freigeben:**
```csharp
public override void OnExit()
{
    if (_face != 0)    Freetype.DoneFace(_face);
    if (_library != 0) Freetype.Done(_library);
}
```
