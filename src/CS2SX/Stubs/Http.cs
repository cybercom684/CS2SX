// CS2SX Stub — wird nicht transpiliert

/// Einfacher HTTP- und HTTPS-Client für Nintendo Switch.
/// Das URL-Schema entscheidet: "https://" nutzt TLS (Port 443),
/// "http://" nutzt Klartext (Port 80).
///
/// Alle Aufrufe sind SYNCHRON/BLOCKIEREND — der Frame pausiert bis zur
/// Antwort. Timeouts sind begrenzt (Verbindung 6s, I/O 10s), eine Anfrage
/// kann also nie für immer hängen. Für eine durchgehend reaktionsfähige UI
/// stattdessen die async NetClient-Bibliothek verwenden (cs2sx addLib NetClient).
///
/// Initialisiert nifm + Socket + SSL automatisch beim ersten Aufruf.
///
/// Beispiel:
///   if (Http.IsAvailable()) {
///       string json = Http.Get("https://api.example.com/data");
///       // json verarbeiten...
///   }
public static class Http
{
    /// HTTP/HTTPS GET — gibt den Response-Body als String zurück (leer bei Fehler).
    /// Synchron: der Frame pausiert bis zur Antwort (max ~10s).
    public static string Get(string url) => "";

    /// HTTP/HTTPS POST — sendet body als text/plain, gibt Response-Body zurück.
    public static string Post(string url, string body) => "";

    /// HTTP/HTTPS POST mit Content-Type: application/json.
    public static string PostJson(string url, string json) => "";

    /// Gibt true zurück wenn die Netzwerk-Initialisierung erfolgreich war.
    /// Hinweis: prüft nicht ob WLAN aktiv ist — einzelne Requests können dennoch fehlschlagen.
    public static bool IsAvailable() => false;

    /// HTTP-Statuscode der letzten Anfrage (200, 404, 0 bei Verbindungsfehler etc.).
    public static int GetLastStatusCode() => 0;

    // ── JSON-Helfer (auf beliebigem JSON-String) ──────────────────────────────

    /// Extrahiert einen Integer-Wert: "field":42 → 42 (oder defVal).
    public static int JsonInt(string json, string field, int defVal = 0) => defVal;

    /// Extrahiert einen Float-Wert: "field":3.14 → 3.14f (oder defVal).
    public static float JsonFloat(string json, string field, float defVal = 0) => defVal;

    /// Extrahiert einen String-Wert: "field":"text" → "text".
    /// Zeigt auf einen rotierenden statischen Puffer — sofort verwenden.
    public static string JsonStr(string json, string field) => "";

    // ── Wetter-Helfer (Open-Meteo) ────────────────────────────────────────────
    // Nach einem Get() auf die Open-Meteo-API automatisch geparst.

    /// Temperatur aus der letzten Open-Meteo-Antwort (z.B. "12.3").
    public static string WeatherTemp() => "";

    /// Windgeschwindigkeit aus der letzten Open-Meteo-Antwort (z.B. "8.1").
    public static string WeatherWind() => "";

    /// WMO-Wettercode aus der letzten Open-Meteo-Antwort (-1 wenn keiner).
    public static int WeatherCode() => -1;
}
