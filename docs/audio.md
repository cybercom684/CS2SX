# Audio API

CS2SX bringt eine vollständige Audio-Engine mit: Töne, WAV-Sample-Wiedergabe, Musik mit Pause/Seek, einen polyphonen Synthesizer und eine Effekt-Kette. Alles läuft mit **48 kHz Stereo**.

> Rufe **einmal pro Frame** `Audio.Update()` auf — das hält den Mixer gefüllt. Ohne `Update()` bricht die Wiedergabe ab.

```csharp
public override void OnInit()
{
    Audio.Init();                 // sampleRate-Parameter wird ignoriert (intern immer 48000)
}

public override void OnFrame()
{
    Audio.Update();               // Pflicht: jeden Frame aufrufen
    // ... restliche Logik ...
}
```

---

## Grundlagen

| Methode | Beschreibung |
|---|---|
| `Audio.Init(sampleRate = 48000)` | Engine starten. Der Parameter wird ignoriert (libnx nutzt intern immer 48000 Hz). |
| `Audio.Update()` | Mixer pro Frame füttern — **Pflicht** für Hintergrund-Wiedergabe. |
| `Audio.SetVolume(volume)` | Master-Lautstärke (0.0 – 1.0). |
| `Audio.Stop()` | Alles stoppen. |
| `Audio.Exit()` | Engine beenden (beim App-Ende). |
| `Audio.SetLatencyBuffers(n)` | Anzahl der gepufferten Audio-Buffer (Latenz vs. Stabilität). |

## Töne

```csharp
Audio.PlayTone(440.0f, 0.4f, 250);   // A4, Amplitude 0.4, 250 ms (Klavier-Timbre)
```

| Methode | Beschreibung |
|---|---|
| `Audio.PlayTone(freqHz, amplitude, duration_ms)` | Einzelnen Ton mit Hüllkurve abspielen. |

## WAV-Samples

```csharp
int snd = Audio.LoadWav("romfs:/sprung.wav");        // Handle laden
int id  = Audio.PlaySound(snd, 0.8f, false, 1.0f, 0.0f); // vol, loop, pitch, pan → Instanz-ID
// ...
if (Audio.IsPlaying(id)) Audio.StopInstance(id);
Audio.UnloadSound(snd);
```

| Methode | Beschreibung |
|---|---|
| `Audio.LoadWav(path)` → `int` | WAV laden (mono/stereo, beliebige Rate → 48 kHz). Gibt ein Handle zurück (–1 = Fehler). |
| `Audio.UnloadSound(handle)` | Sample-Speicher freigeben. |
| `Audio.PlaySound(handle, volume, loop, pitch, pan)` → `int` | Abspielen → Instanz-ID. `pitch` 1.0 = normal, `pan` –1..1. |
| `Audio.StopInstance(id)` / `StopSound(handle)` / `StopAllSounds()` | Einzelne Instanz / alle Instanzen eines Handles / alles stoppen. |
| `Audio.IsPlaying(id)` → `bool` | Läuft die Instanz noch? |

## Musik (mit Pause / Seek)

```csharp
int music = Audio.LoadMusic("/switch/MeineApp/song.wav");
int id    = Audio.PlaySound(music, 0.7f, true, 1.0f, 0.0f);
Audio.Pause(id);
Audio.Resume(id);
int pos   = Audio.GetPositionFrames(id);
int total = Audio.GetSoundFrames(music);
Audio.Seek(id, total / 2);     // zur Mitte springen
```

| Methode | Beschreibung |
|---|---|
| `Audio.LoadMusic(path)` → `int` | Wie `LoadWav`, optimiert für lange Dateien (Streaming-freundliches Handle). |
| `Audio.Pause(id)` / `Resume(id)` / `IsPaused(id)` | Wiedergabe pausieren / fortsetzen / Status. |
| `Audio.GetPositionFrames(id)` → `int` | Aktuelle Position in Samples. |
| `Audio.Seek(id, frame)` | Auf Sample-Position springen. |
| `Audio.GetSoundFrames(handle)` / `GetSoundRate(handle)` | Länge in Samples / Sample-Rate des geladenen Sounds. |

> **Nur WAV ist eingebaut.** MP3/OGG/FLAC benötigen Decoder-Extern-Libs (`dr_libs`, `stb_vorbis`) — siehe [Externe Bibliotheken](advanced/external-libs.md).

---

## Synthesizer

Ein 8-stimmiger Wavetable-Synth (Oszillatoren A/B + Sub, Filter, ADSR, LFO). Konfigurieren, dann mit MIDI-Noten spielen:

```csharp
Audio.SetOscA(1, 0.8f, 0.0f, 1);          // wave=Saw, level, detuneCents, unison
Audio.SetADSR(5.0f, 120.0f, 0.7f, 200.0f); // attack, decay, sustain, release
Audio.SetFilter(0, 1200.0f, 0.3f);         // type=LowPass, cutoff, resonance

Audio.PlayNote(60, 100);   // MIDI 60 (C4), velocity 100
Audio.ReleaseNote(60);     // loslassen → Release-Phase
```

| Methode | Beschreibung |
|---|---|
| `Audio.SetOscA(wave, level, detuneCents, unison)` / `SetOscB(...)` | Oszillator A/B. `wave`: 0=Sine, 1=Saw, 2=Square, 3=Triangle, 4=Noise. |
| `Audio.SetADSR(attack_ms, decay_ms, sustain, release_ms)` | Amplituden-Hüllkurve. |
| `Audio.SetFilter(type, cutoffHz, resonance)` | Filter. `type`: 0=LP, 1=HP, 2=BP, 3=Notch. |
| `Audio.SetFilterEnv(attack_ms, decay_ms, sustain, release_ms)` | Filter-Hüllkurve. |
| `Audio.SetLFO(wave, rateHz, target, depth)` | LFO. `target`: 0=Pitch, 1=Volume, 2=Filter, 3=Pan. |
| `Audio.SetPitchBend(semitones)` | Pitch-Bend in Halbtönen. |
| `Audio.PlayNote(midiNote, velocity)` / `ReleaseNote(midiNote)` | Note anschlagen / loslassen. |

## Effekte

```csharp
Audio.SetLowPass(800.0f);     // globaler Tiefpass
Audio.SetEcho(180, 0.4f);     // Echo: 180 ms Delay, 0.4 Rückkopplung
Audio.ClearEffects();         // Effekt-Kette zurücksetzen
```

| Methode | Beschreibung |
|---|---|
| `Audio.SetLowPass(cutoffHz)` | Globaler Tiefpass über den Master. |
| `Audio.SetEcho(delayMs, decay)` | Echo/Delay. |
| `Audio.ClearEffects()` | Alle Effekte aus. |

---

## Tipps

- **`Audio.Update()` immer** pro Frame aufrufen — auch während langer Operationen (Kopieren, Zippen), sonst stockt die Musik.
- Sample-Rate-Mismatch ist kein Problem — die Engine resampled automatisch auf 48 kHz.
- Für knackfreie Wiedergabe bei vielen Stimmen begrenzt eine `tanh`-Sättigung die Summe.
