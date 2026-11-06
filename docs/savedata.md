# Save Data & Tastatur

## Save Data (Konfiguration speichern)

Ein einfacher Schlüssel-Wert-Speicher auf der SD-Karte — ideal für Einstellungen, Highscores oder den zuletzt geöffneten Pfad. Jeder Schlüssel landet als Datei unter `sdmc:/switch/cs2sx_data/<key>.txt`.

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
| `SaveData.Read(key)` → `string` | Wert lesen — gibt **eine Zeile** (≤ 512 Zeichen) zurück, `""` wenn der Schlüssel fehlt. |
| `SaveData.Write(key, value)` | Wert schreiben (legt das Verzeichnis bei Bedarf an). |
| `SaveData.Commit()` | Schreibvorgänge auf die SD-Karte flushen. |
| `SaveData.Unmount()` | Speicher freigeben (optional, beim App-Ende). |

> **Werte sind einzeilige Strings.** Für Zahlen/Booleans selbst serialisieren (`value.ToString()`) und beim Lesen parsen. Mehrere Felder kannst du mit einem Trennzeichen in **einen** Schlüssel packen (z.B. `"label\tpfad"`) und nach dem Lesen splitten.

> `Read` liefert genau **eine** `fgets`-Zeile (max. 512 Zeichen). Für längere Daten mehrere Schlüssel verwenden.

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
