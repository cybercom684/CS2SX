// CS2SX Stub — wird nicht transpiliert

/// Zugriff auf die systemeigene Software-Tastatur (Swkbd-Applet) des Nintendo Switch.
/// Öffnet die Overlay-Tastatur — BLOCKIERT bis der Nutzer bestätigt oder abbricht.
/// Gibt bei Abbruch einen leeren String zurück.
public static class Keyboard
{
    /// Öffnet die Standard-Tastatur (alle Zeichen).
    /// prompt:  Hinweistext (Guide-Text im Textfeld), leer = kein Hinweis
    /// initial: Vorausgefüllter Text (optional)
    public static string Show(string prompt, string initial = "") => "";

    /// Öffnet die Tastatur im Passwort-Modus (Zeichen als ● dargestellt).
    public static string ShowPassword(string prompt) => "";

    /// Öffnet die Zifferntastatur (NumPad — nur 0–9 und Punkt).
    public static string ShowNumber(string prompt, string initial = "") => "";
}
