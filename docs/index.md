# CS2SX Dokumentation

Willkommen bei CS2SX — dem C#-zu-C-Transpiler für Nintendo Switch Homebrew.

---

## Einstieg

| Dokument | Beschreibung |
|----------|-------------|
| [Erste Schritte](getting-started.md) | Installation, erstes Projekt, CLI-Überblick |
| [Sprachunterstützung](language-guide.md) | Welche C#-Features funktionieren |

## API-Referenz

| Dokument | Beschreibung |
|----------|-------------|
| [Graphics API](graphics.md) | 2D-Zeichenfunktionen, Farben, Text, Texturen |
| [Input API](input.md) | Controller-Buttons lesen |
| [Forms API](forms.md) | Text-UI mit Label, Button, ProgressBar |

## Beispiele

| Beispiel | Beschreibung |
|----------|-------------|
| [Hello World](examples/01-hello-world.md) | Minimales erstes Programm |
| [Input-Demo](examples/02-input-demo.md) | Alle Buttons in Echtzeit anzeigen |
| [Ball fangen](examples/03-simple-game.md) | Vollständiges Spiel mit Physik und Highscore |
| [Datei-Browser](examples/04-file-manager.md) | Text-UI-Tool mit Verzeichnis-Navigation |

## Fortgeschrittenes

| Dokument | Beschreibung |
|----------|-------------|
| [Externe Libraries](advanced/external-libs.md) | C-Libraries einbinden (addLib, Shim-Header) |
| [Grenzen & Workarounds](advanced/limitations.md) | Was nicht geht und wie man es umgeht |

---

## Schnellstart

```bash
# 1. Installieren
dotnet pack -c Release
dotnet tool install --global --add-source ./bin/Release CS2SX

# 2. Projekt erstellen
cs2sx new MeineApp

# 3. Bauen
cs2sx build MeineApp/MeineApp.csproj
```

Die fertige `.nro` auf die Switch SD-Karte kopieren: `/switch/MeineApp/MeineApp.nro`
