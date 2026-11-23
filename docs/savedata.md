# Save Data & Tastatur

## Save Data (Konfiguration speichern)

Ein einfacher Schlüssel-Wert-Speicher auf der SD-Karte — ideal für Einstellungen, Highscores oder den zuletzt geöffneten Pfad. Jede App hat einen eigenen Ordner (der Projektname), jeder Schlüssel landet als Datei unter `sdmc:/switch/cs2sx_data/<App>/<key>.txt`. Zwei Apps mit demselben Schlüssel stören sich also nicht.

> Bis 0.7.0 lagen alle Apps gemeinsam in `sdmc:/switch/cs2sx_data/<key>.txt`. `Read` findet solche alten Werte weiterhin, bis die App den Schlüssel neu schreibt.

```csharp
SaveData.Mount();                          // einmal beim Start

SaveData.Write("highscore", "1280");
SaveData.Write("lastLevel", "Wald-3");

string hs = SaveData.Read("highscore");    // "1280" (oder "" wenn nicht vorhanden)
int score = 0;
int.TryParse(hs, out score);               // selbst in int wandeln
```

| Methode | Beschreibung |
|---|---|
| `SaveData.Mount()` → `bool` | Speicher vorbereiten (Verzeichnis anlegen). Einmal beim Start aufrufen. |
| `SaveData.Read(key)` → `string` | Wert lesen (beliebig lang, auch mehrzeilig), `""` wenn der Schlüssel fehlt. |
| `SaveData.Write(key, value)` | Wert schreiben (legt das Verzeichnis bei Bedarf an). |
| `SaveData.Commit()` | Schreibvorgänge auf die SD-Karte flushen. |
| `SaveData.Unmount()` | Speicher freigeben (optional, beim App-Ende). |

> **Werte sind Strings.** Für Zahlen/Booleans selbst serialisieren (`value.ToString()`) und beim Lesen parsen. Mehrere Felder kannst du mit einem Trennzeichen in **einen** Schlüssel packen (z.B. `"label\tpfad"`) und nach dem Lesen splitten. Für größere Spielstände eignet sich eine eigene Datei mit [JSON](json.md) oder `BinaryWriter`/`BinaryReader` (siehe [Sprachunterstützung → Datei-I/O](language-guide.md#datei-io)).

---

## Tastatur (Bildschirm-Tastatur)

Öffnet die System-Tastatur der Switch (blockierend — die App pausiert, bis der Nutzer bestätigt oder abbricht).

```csharp
string name = Keyboard.Show("Name eingeben", "Spieler1");
string pin  = Keyboard.ShowNumber("PIN", "");
string pw   = Keyboard.ShowPassword("Passwort");
```

| Methode | Beschreibung |
|---|---|
| `Keyboard.Show(prompt, initial = "")` → `string` | Standard-Tastatur mit Eingabeaufforderung und optionalem Vorgabetext. |
| `Keyboard.ShowNumber(prompt, initial = "")` → `string` | Nur Ziffern. |
| `Keyboard.ShowPassword(prompt)` → `string` | Eingabe wird als `●●●` maskiert. |

> Während die System-Tastatur offen ist, ist die App-Schleife pausiert — Hintergrund-Musik verstummt kurzzeitig. Bricht der Nutzer ab, kommt der ursprüngliche/leere String zurück.
