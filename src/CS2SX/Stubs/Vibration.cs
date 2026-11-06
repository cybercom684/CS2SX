// CS2SX Stub — wird nicht transpiliert

/// HD-Rumble-Steuerung für Nintendo Switch JoyCons und Controller.
/// Wird beim ersten Aufruf automatisch initialisiert.
public static class Vibration
{
    /// Volle Rumble-Kontrolle: Frequenz in Hz und Amplitude 0.0–1.0.
    /// Typisch: lowFreq=160, highFreq=320 (JoyCon-Standard).
    public static void Rumble(float lowFreq, float lowAmp, float highFreq, float highAmp) { }

    /// Einfaches Vibrieren — Stärke 0.0 (aus) bis 1.0 (voll).
    public static void RumbleSimple(float strength) { }

    /// Kurzer Vibrationspuls (Stärke 0.0–1.0).
    /// Hinweis: Stop() muss danach manuell (z. B. per Frame-Timer) aufgerufen werden.
    public static void Pulse(float strength, int durationMs) { }

    /// Stoppt alle Vibrationen sofort.
    public static void Stop() { }
}
