# CS2SX

**Nintendo-Switch-Homebrew in C#.** Du schreibst eine normale C#-App, `dotnet build` erzeugt daraus eine `.nro`, die auf der Switch (und im Emulator) läuft.

CS2SX übersetzt den C#-Code mit Roslyn nach C und kompiliert ihn mit devkitPro/libnx zu nativem AArch64-Code. Auf der Konsole läuft also keine .NET-Laufzeit, kein JIT und kein Garbage Collector, nur kompakter C-Code. Geschrieben wird in Visual Studio, Rider oder VS Code mit voller IntelliSense über das NuGet-Paket `CS2SX.Switch`.

**[Download](https://github.com/cybercom684/CS2SX/releases/latest)** · [Dokumentation](docs/index.md) · [Erste Schritte](docs/getting-started.md) · [Sprachunterstützung](docs/language-guide.md) · [Grafik](docs/graphics.md) · [Beispiele](docs/examples/)

---

## Ein Beispiel

```csharp
public class BallApp : SwitchApp
{
    private float _x = 640, _y = 360, _vx = 6, _vy = 4;
    private int _bounces;

    public override void OnInit()
    {
        Graphics.Init(1280, 720);
    }

    public override void OnFrame()
    {
        _x += _vx;
        _y += _vy;
        if (_x < 40 || _x > 1240) { _vx = -_vx; _bounces++; }
        if (_y < 40 || _y > 680)  { _vy = -_vy; _bounces++; }
        if (Input.IsDown(NpadButton.A)) _bounces = 0;

        Graphics.FillScreen(Color.RGB(20, 22, 30));
        Graphics.FillCircle((int)_x, (int)_y, 40, Color.Orange);
        Graphics.DrawText(40, 30, $"Abpraller: {_bounces}   (A = zuruecksetzen, + = beenden)", Color.White, 2);
    }
}
```

`dotnet build` → `BallApp.nro`. `OnInit` läuft einmal, `OnFrame` 60-mal pro Sekunde. Mit **+** endet die App.

---

## Was CS2SX kann

**C# wie gewohnt**
- Klassen, Vererbung, `virtual`/`abstract`/`override`, Interfaces, Structs, Records, Enums, Properties, Generics und Extension-Methoden.
- Konstruktor-Überladungen mit `: this(…)`/`: base(…)` sowie optionale und benannte Parameter.
- Lambdas mit Closures, `Action`/`Func`, Events mit mehreren Abonnenten.
- LINQ, `List`, `Dictionary`, `Stack`, `Queue`, `HashSet`, Arrays (mit `Length`), `int?`, Tupel, Pattern Matching und `switch`-Ausdrücke.
- Strings mit Interpolation, `StringBuilder`, `string.Format`.

**Exceptions wie in .NET**
- `try`/`catch`/`finally` über Methodengrenzen hinweg, eigene Exception-Klassen und `when`-Filter.
- Die Laufzeit wirft dieselben Exceptions wie .NET, etwa bei `int.Parse("abc")`, `list[99]`, einem Zugriff auf `null`, einer Division durch 0 oder einer fehlenden Datei.
- Eine ungefangene Exception zeigt den Fehlerdialog der Switch, statt die Konsole abstürzen zu lassen.

**Grafik auf der GPU** (deko3d)
- Rechtecke, Kreise, Ellipsen, Dreiecke, Polygone und Linien, kantengeglättet und mit echter Transparenz.
- Texturen: BMP eingebaut, PNG/JPEG über eine Zusatz-Bibliothek.
- **TrueType-Schriften** (`Font.Load("romfs:/font.ttf")`), scharf in jeder Größe.
- 1080p-Ausgabe kostet nichts extra.

**Die Switch-API**
- Buttons, Analog-Sticks und Touchscreen.
- Audio, Bewegungssensoren, HD-Rumble.
- SD-Karte und Spielstände, Bildschirmtastatur.
- HTTP, FTP-Server, Akku und Uhrzeit.
- Eine Text-UI mit Label und Button für Tools.

**Der Build**
- Inkrementell und parallel: Nur Geändertes wird neu übersetzt und kompiliert.
- Dateien in `romfs/` werden automatisch in die `.nro` eingebettet.
- Eigene C-Bibliotheken lassen sich mit `cs2sx addLib` einbinden.

---

## Voraussetzungen

- [.NET 8 SDK](https://dotnet.microsoft.com/download) (oder neuer)
- [devkitPro](https://devkitpro.org/wiki/Getting_Started) mit dem Paket `switch-dev` (devkitA64, libnx, deko3d)
- Für TrueType-Schriften zusätzlich `switch-freetype`

```bash
dkp-pacman -S switch-dev switch-freetype
```

Unter Windows heißt das Programm im devkitPro-MSYS2 einfach `pacman`. Die Umgebungsvariable `DEVKITPRO` setzt der devkitPro-Installer.

---

## Installation

**Als .NET-Tool (empfohlen):** `CS2SX.<version>.nupkg` aus den [Releases](https://github.com/cybercom684/CS2SX/releases/latest) in einen Ordner laden:

```bash
dotnet tool install --global --add-source <ordner> CS2SX
```

Danach steht `cs2sx` in jedem Terminal zur Verfügung. Für ein Update dasselbe mit `dotnet tool update`.

**Ohne Installation:** `cs2sx-<version>-<system>.zip`/`.tar.gz` entpacken und unter Windows `CS2SX.exe`, unter Linux/macOS `dotnet CS2SX.dll` starten. Damit `dotnet build` den Transpiler findet, muss er im `PATH` liegen, oder du gibst ihn im Projekt an: `<CS2SXTool>dotnet /pfad/zu/CS2SX.dll</CS2SXTool>`.

**Aus dem Quellcode:**

```bash
dotnet pack src/CS2SX -c Release -o nupkg
dotnet tool install --global --add-source ./nupkg CS2SX
```

---

## Die erste App

```bash
cs2sx new MeineApp
cd MeineApp
dotnet build
```

Das legt ein Projekt mit `Program.cs` an und baut `MeineApp.nro`. Die Datei kommt auf die SD-Karte nach `/switch/` und wird über das Homebrew-Menü gestartet. Zum Testen am PC lässt sich die `.nro` direkt im Emulator Ryujinx öffnen.

Ein Projekt ist ein normales .NET-Projekt mit dem Paket `CS2SX.Switch`. Statt mit `dotnet build` kannst du auch direkt in Visual Studio oder Rider bauen. Mehr dazu in [Erste Schritte](docs/getting-started.md).

---

## Kommandozeile

| Befehl | Wirkung |
|---|---|
| `cs2sx new <Name>` | Neues Projekt anlegen |
| `cs2sx build <Projekt>` | `.nro` bauen (das macht auch `dotnet build`), `--transpile-only` hört nach dem C-Code auf |
| `cs2sx check <Projekt>` | Nur übersetzen und Warnungen zeigen, ohne GCC, schnell |
| `cs2sx watch <Projekt>` | Bei jeder Änderung neu bauen |
| `cs2sx dump <Projekt> [Datei]` | Den erzeugten C-Code ausgeben |
| `cs2sx clean <Projekt>` | `cs2sx_out/` löschen |
| `cs2sx migrate <Projekt> [--dry-run]` | Ein Projekt einer älteren CS2SX-Version umstellen |
| `cs2sx addLib <Name> [Projekt]` | Eine C-Bibliothek aus `externLibs/<Name>` einbinden ([Anleitung](docs/advanced/external-libs.md)) |

---

## Wie es funktioniert

```
Program.cs ─► Roslyn (Syntax + Semantik) ─► C-Code + C-Laufzeit ─► GCC (devkitA64) ─► .elf ─► .nro
```

Der Transpiler erzeugt pro C#-Datei eine `.h`/`.c`-Datei in `cs2sx_out/` und kompiliert sie zusammen mit der CS2SX-Laufzeit, einer schlanken C-Bibliothek für Collections, Strings, Exceptions, Delegates, Grafik und die Switch-Dienste. Mit `cs2sx dump` lässt sich jederzeit ansehen, was aus dem eigenen Code geworden ist.

---

## Grenzen

- **Kein Garbage Collector:** Objekte werden nicht automatisch freigegeben. Was jeden Frame neu entsteht (`new` in `OnFrame`), sollte wiederverwendet werden.
- **Lambdas kopieren lokale Variablen**, die sie erfassen. Schreibt eine Lambda in eine solche Variable, kommt das außen nicht an; dafür ein Feld verwenden (`cs2sx check` warnt).
- `async`/`await` läuft synchron. Reflection, `dynamic`, eigene `delegate`-Typen und Mehrfachvererbung gibt es nicht.
- **Ryujinx ohne Firmware** hat keine Systemschrift, und `Font.LoadSystem()` lässt den Emulator abstürzen. Zum Testen eine eigene Schrift im `romfs/` mitliefern.

Ausführlich: [Sprachunterstützung](docs/language-guide.md) · [Grenzen und Workarounds](docs/advanced/limitations.md)

---

## Am Projekt mitarbeiten

```
src/CS2SX/          der Transpiler (CLI) und die C-Laufzeit (Runtime/)
src/CS2SX.Switch/   die C#-API, die Apps verwenden (NuGet-Paket)
tests/CS2SX.Tests/  End-to-End-Tests: je ein Ordner in Cases/ ist eine kleine App
docs/               die Dokumentation
```

```bash
dotnet test
```

Die Tests übersetzen jede Beispiel-App, vergleichen den erzeugten C-Code mit gespeicherten Snapshots und bauen sie mit devkitPro zu einer `.nro` (dieser Teil wird ohne devkitPro übersprungen). Mit `CS2SX_RYUJINX=<Pfad zu Ryujinx.exe>` laufen die Apps zusätzlich im Emulator, und ihre Ausgabe wird mit der erwarteten verglichen. Wie die Snapshots aktualisiert werden, steht in [tests/README.md](tests/README.md). Aufbau und Regeln der C-Laufzeit beschreibt [src/CS2SX/Runtime/README.md](src/CS2SX/Runtime/README.md).

**Release:** Die Version steht in `Directory.Build.props`. Ein gepushter Tag startet die GitHub Action, die testet, baut und das Release veröffentlicht:

```bash
git tag -a v0.6.0 -m "Was ist neu"
git push origin v0.6.0
```

---

## Lizenz

MIT
