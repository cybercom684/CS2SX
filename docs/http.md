# HTTP API

CS2SX kann über WLAN HTTP/HTTPS-Anfragen stellen — z.B. um Daten von einer Web-API zu laden. TLS wird automatisch gewählt (`https://` → Port 443, `http://` → Port 80).

> Die Netzwerk-I/O läuft auf einem Hintergrund-Thread, damit die Frames nicht einfrieren. Es gelten Timeouts (Verbindung ~6 s, I/O ~10 s).

```csharp
if (Http.IsAvailable())
{
    string body = Http.Get("https://api.example.com/status");
    int code = Http.GetLastStatusCode();   // z.B. 200
    if (code == 200)
        Console.WriteLine(body);
}
```

---

## Anfragen

| Methode | Beschreibung |
|---|---|
| `Http.Get(url)` → `string` | GET-Anfrage, gibt den Response-Body zurück (`""` bei Fehler). |
| `Http.Post(url, body)` → `string` | POST mit beliebigem Body. |
| `Http.PostJson(url, json)` → `string` | POST mit `Content-Type: application/json`. |
| `Http.IsAvailable()` → `bool` | Ist eine Netzwerkverbindung verfügbar? Vor Anfragen prüfen. |
| `Http.GetLastStatusCode()` → `int` | HTTP-Statuscode der letzten Anfrage (z.B. 200, 404). |

## JSON auslesen

Kein vollständiger JSON-Parser, aber leichtgewichtige Feld-Extraktoren für flache Objekte:

```csharp
string json = Http.Get("https://api.example.com/user");
int    id    = Http.JsonInt(json, "id", -1);
float  score = Http.JsonFloat(json, "score", 0.0f);
string name  = Http.JsonStr(json, "name");
```

| Methode | Beschreibung |
|---|---|
| `Http.JsonInt(json, field, defVal = 0)` → `int` | Integer-Feld lesen (oder `defVal`). |
| `Http.JsonFloat(json, field, defVal = 0)` → `float` | Float-Feld lesen. |
| `Http.JsonStr(json, field)` → `string` | String-Feld lesen (`""` wenn nicht gefunden). |

## Wetter-Helfer (Open-Meteo)

Bequeme Wrapper für die kostenlose [Open-Meteo](https://open-meteo.com)-API:

```csharp
string temp = Http.WeatherTemp();   // z.B. "12.4"
string wind = Http.WeatherWind();   // Windgeschwindigkeit
int    code = Http.WeatherCode();   // WMO-Wettercode (–1 bei Fehler)
```

| Methode | Beschreibung |
|---|---|
| `Http.WeatherTemp()` → `string` | Aktuelle Temperatur. |
| `Http.WeatherWind()` → `string` | Windgeschwindigkeit. |
| `Http.WeatherCode()` → `int` | WMO-Wettercode (`–1` bei Fehler). |

---

## Hinweise

- **`Http.IsAvailable()` vor jeder Anfrage prüfen** — im Flugmodus oder ohne WLAN schlägt sie sonst still fehl (leerer String).
- Synchrone HTTP-Aufrufe auf dem Haupt-Thread würden die App auf der Switch blockieren — deshalb läuft die Engine threaded. Die `Get`/`Post`-Aufrufe warten bis zur Antwort (mit Timeout), frieren das Bild aber nicht dauerhaft ein.
- Für große oder gestreamte Downloads ist die einfache `Get`-API nicht gedacht.
