# JSON

CS2SX übersetzt `JsonSerializer` aus `System.Text.Json`. Der Code bleibt also derselbe wie auf dem PC. Reflection gibt es auf der Switch nicht. Stattdessen schreibt der Build für jeden Typ, der serialisiert wird, eigenen C#-Code zum Schreiben und Lesen und übersetzt ihn mit dem Projekt.

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

public class Spielstand
{
    public string Name { get; set; } = "";
    public int Level { get; set; }
    public List<Gegenstand> Inventar { get; set; } = new List<Gegenstand>();
    [JsonPropertyName("hp")] public float Leben { get; set; } = 100;
    [JsonIgnore] public int Temp { get; set; }
}

// speichern
string text = JsonSerializer.Serialize(stand);
File.WriteAllText("/switch/MeinSpiel/stand.json", text);

// laden
var geladen = JsonSerializer.Deserialize<Spielstand>(File.ReadAllText("/switch/MeinSpiel/stand.json"));
```

## Was geht

| | |
|---|---|
| Aufrufe | `JsonSerializer.Serialize(wert)`, `Serialize(wert, options)`, `Deserialize<T>(text)`, `Deserialize<T>(text, options)` |
| Typen | ganze Zahlen, `float`, `double`, `bool`, `char`, `string`, Enums (als Zahl), `int?` & Co., `List<T>`, Arrays, `Dictionary<string, T>` und `Dictionary<int, T>`, eigene Klassen, Structs und Records, auch verschachtelt |
| Mitglieder | öffentliche Eigenschaften mit `get`; Felder nur mit `[JsonInclude]` oder `IncludeFields = true` (wie in .NET) |
| Attribute | `[JsonPropertyName("…")]`, `[JsonIgnore]`, `[JsonInclude]` |
| Optionen | `WriteIndented`, `PropertyNameCaseInsensitive`, `IncludeFields`, `PropertyNamingPolicy = JsonNamingPolicy.CamelCase` |
| Konstruktoren | ein öffentlicher parameterloser Konstruktor, oder genau ein öffentlicher Konstruktor, dessen Parameter wie die Eigenschaften heißen (Records wie `record Punkt(int X, int Y)`) |

Beim Lesen werden unbekannte Felder übersprungen. Fehlende Felder behalten den Wert, den das Objekt beim Anlegen hat, etwa den Initialwert `= 100`. Ungültiges JSON, ein falscher Werttyp (Text statt Zahl) oder eine Zahl außerhalb des Bereichs werfen eine `JsonException`:

```csharp
try { var s = JsonSerializer.Deserialize<Spielstand>(text); }
catch (JsonException e) { Console.WriteLine("Spielstand beschädigt: " + e.Message); }
```

## Unterschiede zu .NET

- **Strings:** Nur `"`, `\` und Steuerzeichen werden als Escape geschrieben, Umlaute und `<`, `>`, `&` bleiben stehen. .NET schreibt standardmäßig zum Beispiel `ä` als `ä`. Beide Formen sind gültiges JSON und werden beim Lesen gleich verstanden.
- **Typ zur Build-Zeit:** Der Typ muss beim Übersetzen feststehen. `Serialize(object)`, Interfaces und abstrakte Klassen als Eigenschaftstyp gehen nicht. Eine Eigenschaft vom Typ einer Basisklasse schreibt nur deren Eigenschaften, auch wenn ein abgeleitetes Objekt darin steckt (wie in .NET).
- **Nicht unterstützt** (der Build meldet es mit Zeile und Grund): `DateTime`, `Guid` und andere Systemtypen, mehrdimensionale Arrays, `JsonDocument`/`JsonNode`, eigene Konverter, `JsonStringEnumConverter`, Streams.

Für ganz einfache Fälle, etwa ein Feld aus einer Web-Antwort, gibt es weiterhin `Http.JsonInt`/`JsonStr` (siehe [HTTP API](http.md)).
