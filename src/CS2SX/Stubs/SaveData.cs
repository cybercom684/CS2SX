// CS2SX Stub — wird nicht transpiliert

/// Zugriff auf den Nintendo Switch Save-Data-Speicher.
/// Für NRO-Homebrew werden Saves auf der SD-Karte gespeichert:
///   sdmc:/switch/cs2sx_data/<key>.txt
///
/// Typischer Ablauf:
///   if (SaveData.Mount()) {
///       SaveData.Write("score", _score.ToString());
///       string v = SaveData.Read("score");
///       SaveData.Unmount();
///   }
public static class SaveData
{
    /// Bereitet das Save-Verzeichnis vor (erstellt es falls nötig).
    /// Gibt immer true zurück (SD-Karte ist immer verfügbar).
    public static bool Mount() => false;

    /// Liest einen gespeicherten Wert per Schlüssel.
    /// Gibt leeren String zurück wenn der Schlüssel nicht existiert.
    public static string Read(string key) => "";

    /// Speichert einen Wert unter dem angegebenen Schlüssel.
    public static void Write(string key, string value) { }

    /// Kein-Op: SD-Schreibvorgänge sind sofort persistent.
    public static void Commit() { }

    /// Schliesst den Save-Kontext (nach Commit() aufrufen).
    public static void Unmount() { }
}
