# Funkenjagd

Ein kleines Spiel für 1 bis 4 Spieler an einer Konsole: Jeder fliegt eine Drohne mit dem linken Stick (oder dem Steuerkreuz), sammelt Funken und weicht Meteoren aus. Eine Runde dauert 60 Sekunden, die Meteorwellen werden immer dichter. Goldene Funken zählen 5 Punkte, jeder Treffer kostet eines von drei Leben. Die besten fünf Runden landen in der Bestenliste.

| Taste | |
|---|---|
| A | Runde starten / nochmal |
| Y | Controller-Auswahl der Switch (Spieler verbinden) |
| B | nach einer Runde zurück zum Titel |
| + | Beenden |

Wird auf dem Titelbild 10 Sekunden lang nichts gedrückt, spielt der Computer eine Demo-Runde.

## Was das Beispiel zeigt

| Datei | Inhalt |
|---|---|
| `Sprites.cs` | Pixel-Grafik im Code erzeugt, als Sprite-Sheet mit mehreren Bildern |
| `Program.cs` | `Graphics.DrawTextureRegion` mit Animation, Einfärben pro Spieler, Neigen und Spiegeln; `DrawTextureEx` für drehende Meteore; mehrere Controller (`Input.IsConnected`, `GetStickLeft(spieler)`, `ShowControllerSupport`); Countdown, Wellen und Funkenregen als Coroutines; Enums in Texten (Medaillen); Ton und Vibration |
| `Scheduler.cs` | ein kleiner Coroutine-Planer: Methoden mit `IEnumerator<int>`, jeder `yield return` wartet so viele Frames |
| `Highscores.cs` | Bestenliste als JSON (`JsonSerializer`) im SaveData-Ordner der App |
| `Hud.cs` | Text mit TrueType-Schrift oder 8×8-Schrift als Ersatz |

## Bauen

```bash
cs2sx build samples/Funkenjagd/Funkenjagd.csproj
```

Das Spiel nutzt die Systemschrift der Switch. Liegt eine `romfs/font.ttf` im Projekt, wird stattdessen sie verwendet (nötig in Emulatoren ohne Firmware-Schriften).
