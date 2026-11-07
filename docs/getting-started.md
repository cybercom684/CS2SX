# Erste Schritte mit CS2SX

CS2SX transpiliert C#-Code zu C und kompiliert ihn via DevkitPro zur fertigen Nintendo Switch Homebrew-App (`.nro`).

---

## Voraussetzungen

| Tool | Download |
|------|----------|
| [.NET 8 SDK](https://dotnet.microsoft.com/download) | Mindestens .NET 8 |
| [DevkitPro](https://devkitpro.org/wiki/Getting_Started) | Mit `devkitA64` und `libnx` |

Die Umgebungsvariable `DEVKITPRO` muss auf das DevkitPro-Verzeichnis zeigen (z.B. `/opt/devkitpro` auf Linux/macOS oder `C:/devkitPro` auf Windows).

---

## Installation

Die fertigen Downloads liegen unter [Releases](https://github.com/cybercom684/CS2SX/releases).

**Als .NET-Tool (empfohlen):** `CS2SX.<version>.nupkg` in einen Ordner laden, dann:

```bash
dotnet tool install --global --add-source <ordner> CS2SX
```

Nach der Installation ist `cs2sx` in jedem Terminal verfügbar. Ein Update funktioniert genauso, nur mit `dotnet tool update`. Das Tool bringt das passende API-Paket `CS2SX.Switch` mit, neue Projekte bauen also ohne weitere Einrichtung.

**Ohne Installation:** `cs2sx-<version>-<system>.zip` bzw. `.tar.gz` entpacken. Unter Windows `CS2SX.exe` starten, unter Linux und macOS `dotnet CS2SX.dll` (beides braucht nur die .NET-8-Laufzeit, die mit dem .NET SDK ohnehin da ist).

**Aus dem Quellcode:**

```bash
dotnet pack src/CS2SX -c Release -o nupkg
dotnet tool install --global --add-source ./nupkg CS2SX
```

Ein neues Release entsteht, wenn ein Versions-Tag gepusht wird (`git tag -a v0.6.0 -m "…"` und dann `git push origin v0.6.0`). Die GitHub Action `.github/workflows/release.yml` testet, baut alle Pakete und veröffentlicht sie mit der Tag-Nachricht als Release-Text.

---

## Erstes Projekt

### 1. Projekt anlegen

```bash
cs2sx new MeineApp
```

Das erzeugt folgende Struktur:

```
MeineApp/
├── MeineApp.csproj       ← .NET-Projekt, referenziert das Paket CS2SX.Switch
├── nuget.config          ← Paketquelle: das mit cs2sx ausgelieferte CS2SX.Switch
├── cs2sx.json            ← Name, Autor, Version, Icon
├── Program.cs            ← Deine App-Klasse (erbt von SwitchApp)
├── icon.jpg              ← App-Icon (256×256)
├── README.md
└── .gitignore
```

Keine kopierten Stub-Dateien: Die Switch-API (`Graphics`, `Input`, `File`, `Audio`, `Http`, …)
kommt aus dem Paket und ist ohne `using` verfügbar — IntelliSense, Doku-Kommentare und
Typprüfung inklusive.

### 2. App-Klasse

`Program.cs` enthält eine Klasse, die von `SwitchApp` erbt:

```csharp
public class MeineAppApp : SwitchApp
{
    public override void OnInit()
    {
        // Einmalig beim Start
        Console.WriteLine("Hallo Switch!");
    }

    public override void OnFrame()
    {
        // Einmal pro Frame (~60fps)
        if (Input.IsDown(NpadButton.A))
            App.Quit();   // Plus beendet die App ohnehin automatisch
    }
}
```

### 3. Bauen

```bash
dotnet build MeineApp
```

`dotnet build` (oder Build in Visual Studio / Rider) prüft den C#-Code und erzeugt
anschließend `MeineApp.nro` im Projektverzeichnis. Kopiere sie auf die SD-Karte der Switch
(`/switch/MeineApp/MeineApp.nro`). Nur typprüfen ohne `.nro`: `dotnet build -p:CS2SXBuildNro=false`.

---

## CLI-Überblick

| Befehl | Beschreibung |
|--------|-------------|
| `cs2sx new <Name>` | Neues Projekt erstellen |
| `cs2sx build <csproj>` | Vollständiger Build → `.nro` (das macht auch `dotnet build`) |
| `cs2sx build <csproj> --transpile-only` | Nur C erzeugen, ohne GCC |
| `cs2sx check <csproj>` | Nur transpilieren, Warnungen anzeigen (schnelle Fehlerprüfung) |
| `cs2sx watch <csproj>` | Datei-Watcher mit automatischem Rebuild bei Änderungen |
| `cs2sx clean <csproj>` | Build-Artefakte löschen (behebt verwaiste Symbole nach Umbenennen) |
| `cs2sx migrate <csproj>` | Älteres Projekt (mit kopierten Stubs) auf das Paket umstellen |
| `cs2sx addLib <libName>` | Externe C-Library einbinden |

### Ältere Projekte umstellen

Projekte aus cs2sx-Versionen vor 0.5 enthalten kopierte Stub-Dateien (`SwitchApp.cs`, `Input.cs`,
`LibNX/`, …). `cs2sx migrate MeineApp --dry-run` zeigt, was sich ändert; ohne `--dry-run` werden
die Kopien entfernt, die Paketreferenz eingetragen und Stellen gemeldet, die eine Handänderung
brauchen (z. B. `System.GetBattery()` → `NX.GetBattery()`, `Directory.GetDirectories()` liefert
jetzt wie in .NET ein `string[]`).

---

## Projektstruktur verstehen

### cs2sx.json

```json
{
  "name": "MeineApp",
  "author": "Dein Name",
  "version": "1.0.0",
  "icon": "icon.jpg"
}
```

### Was wird transpiliert?

Alle `.cs`-Dateien des Projekts — auch `Program.cs`, und unabhängig vom Dateinamen. Ausgenommen
sind nur Build-Ordner (`bin/`, `obj/`, `cs2sx_out/`) und Stub-Ordner von `addLib` (`*Stubs/`).
### Inkrementeller Build

Nur geänderte Dateien werden neu transpiliert. Bei Änderungen an Generics oder Interfaces triggert CS2SX automatisch einen vollständigen Rebuild der abhängigen Dateien.

---

## Zwei App-Modi

CS2SX unterstützt zwei Ausgabe-Modi:

### Konsolen-Modus (Text-UI)

Standardmodus — gibt Text über den Switch-Konsolentreiber aus. Gut für Tools, Menüs und einfache Apps.

```csharp
public class MeineApp : SwitchApp
{
    public override void OnInit()
    {
        Console.WriteLine("Willkommen!");
    }
}
```

Die Ausgabe bleibt stehen wie ein Log, auch was in `OnInit` geschrieben wird. Hat die App Forms-Controls (`Label`, `Button`, …), wird der Bildschirm jeden Frame neu gezeichnet.

### Grafik-Modus (GPU)

Aktiviert durch `Graphics.Init(width, height)` in `OnInit()`. Ab dann rendert jeder Frame auf der GPU (deko3d), siehe [Grafik](graphics.md).

```csharp
public class MeineApp : SwitchApp
{
    public override void OnInit()
    {
        Graphics.Init(1280, 720);
    }

    public override void OnFrame()
    {
        Graphics.FillScreen(Color.Black);
        Graphics.DrawText(100, 100, "Hallo Grafik!", Color.White, 2);
    }
}
```

---

## Nächste Schritte

- [Sprachunterstützung](language-guide.md) — Welche C#-Features funktionieren
- [Graphics API](graphics.md) — Alle Zeichenfunktionen
- [Input API](input.md) — Controller-Eingaben lesen
- [Forms API](forms.md) — UI-Steuerelemente
- [Beispiel-Apps](examples/) — Fertige Beispiele zum Ausprobieren
- [Externe Libraries](advanced/external-libs.md) — C-Libraries einbinden
