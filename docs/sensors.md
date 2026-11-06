# Sensoren & Haptik

Zugriff auf die Bewegungssensoren (Gyroskop + Beschleunigungsmesser) der Joy-Cons sowie auf das HD-Rumble.

---

## Motion (Gyro + Beschleunigung)

Liest den Sechs-Achsen-Sensor (erkennt automatisch Handheld / Joy-Con-Paar / Pro Controller).

```csharp
if (Motion.IsAvailable())
{
    MotionState m = Motion.Get();
    float neigung = m.angleX;     // kumulierter Winkel (rad)
    float drehung = m.gyroZ;      // Winkelgeschwindigkeit (rad/s)
    float gX      = m.accelX;     // Beschleunigung (m/s²)
}
```

| Methode | Beschreibung |
|---|---|
| `Motion.IsAvailable()` → `bool` | Ist ein Bewegungssensor aktiv? |
| `Motion.Get()` → `MotionState` | Aktuellen Sensorzustand abfragen. |
| `Motion.ResetAngles()` | Die kumulierten `angle*`-Werte auf 0 zurücksetzen. |

### `MotionState`

| Feld | Einheit | Bedeutung |
|---|---|---|
| `accelX`, `accelY`, `accelZ` | m/s² | Beschleunigung pro Achse |
| `gyroX`, `gyroY`, `gyroZ` | rad/s | Winkelgeschwindigkeit pro Achse |
| `angleX`, `angleY`, `angleZ` | rad | Aufsummierter Winkel (seit Start / letztem `ResetAngles()`) |

> Die `angle*`-Werte integrieren das Gyroskop über die Zeit und driften daher langsam — für relative Gesten gut, für absolute Orientierung gelegentlich `ResetAngles()` aufrufen.

---

## Vibration (HD-Rumble)

Zwei-Frequenz-Rumble (tiefe + hohe Frequenz mit getrennten Amplituden) oder einfache Wrapper.

```csharp
Vibration.RumbleSimple(0.6f);       // mittlere Stärke (160 Hz + 320 Hz)
Vibration.Pulse(1.0f, 120);         // kurzer kräftiger Puls, 120 ms
Vibration.Rumble(160f, 0.5f, 320f, 0.8f);  // volle Kontrolle
Vibration.Stop();
```

| Methode | Beschreibung |
|---|---|
| `Vibration.RumbleSimple(strength)` | Einfaches Rumble (0.0 – 1.0), 160 Hz + 320 Hz mit gleicher Amplitude. Läuft bis `Stop()`. |
| `Vibration.Rumble(lowFreq, lowAmp, highFreq, highAmp)` | Volle Kontrolle: zwei Frequenzbänder mit eigenen Amplituden. |
| `Vibration.Pulse(strength, durationMs)` | Einmaliger Puls fester Dauer. |
| `Vibration.Stop()` | Rumble beenden. |

> Dauer-Rumble (`Rumble`/`RumbleSimple`) läuft weiter, bis `Stop()` gerufen wird — z.B. nach einer festen Anzahl Frames selbst stoppen. Die Sensoren/Rumble initialisieren sich automatisch für Handheld, Joy-Con-Paar und Pro Controller.
