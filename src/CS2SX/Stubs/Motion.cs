// CS2SX Stub — wird nicht transpiliert

/// Sensor-Zustand des Gyroscopes und Beschleunigungsmessers.
public struct MotionState
{
    /// Beschleunigung in m/s² (X=rechts, Y=oben, Z=aus dem Screen heraus)
    public float accelX, accelY, accelZ;
    /// Winkelgeschwindigkeit in rad/s
    public float gyroX, gyroY, gyroZ;
    /// Kumulierter Rotationswinkel in Radiant (seit Start oder letztem ResetAngles())
    public float angleX, angleY, angleZ;
}

/// Zugriff auf Gyroscope und Beschleunigungssensor des Nintendo Switch Controllers.
/// Funktioniert mit JoyCons (Dual/Single), Pro Controller und Handheld-Modus.
/// Wird beim ersten Aufruf automatisch initialisiert.
public static class Motion
{
    /// Gibt true zurück wenn ein Bewegungssensor verfügbar ist.
    public static bool IsAvailable() => false;

    /// Liest den aktuellen Sensor-Zustand (Beschleunigung, Gyro, Winkel).
    public static MotionState Get() => new MotionState();

    /// Setzt die kumulierten Winkel (angleX/Y/Z) zurück auf 0.
    public static void ResetAngles() { }
}
