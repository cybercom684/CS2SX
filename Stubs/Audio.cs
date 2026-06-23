// CS2SX Stub — wird nicht transpiliert
public static class Audio
{
    // ── Core ──────────────────────────────────────────────────────────────────

    /// Initialisiert das Audio-System. sampleRate wird ignoriert (immer 48000 Hz).
    public static bool Init(int sampleRate = 48000) => false;

    /// Muss einmal pro Frame in OnFrame() aufgerufen werden — hält den Hardware-Buffer gefüllt.
    public static void Update() { }

    /// Setzt die Master-Lautstärke (0.0 = stumm, 1.0 = voll).
    public static void SetVolume(float volume) { }

    /// Stoppt alle laufenden Töne und Samples sofort.
    public static void Stop() { }

    /// Gibt das Audio-System frei (am App-Ende aufrufen).
    public static void Exit() { }

    // ── Sinuston-Synthesizer ──────────────────────────────────────────────────

    /// Spielt einen Sinuston mit Piano-Timbre (Grundton + Harmonische).
    /// freqHz    : Frequenz in Hz (z. B. 440.0 = A4)
    /// amplitude : Lautstärke 0.0–1.0
    /// duration_ms: Dauer in Millisekunden (exponentielles Decay)
    public static void PlayTone(float freqHz, float amplitude, int duration_ms) { }

    // ── WAV-Datei Wiedergabe ──────────────────────────────────────────────────

    /// Lädt eine WAV-Datei (16-bit PCM, mono oder stereo, beliebige Samplerate).
    /// path: z. B. "romfs:/sfx/jump.wav" oder "/switch/myapp/effect.wav"
    /// Gibt ein Sound-Handle zurück (0–15) oder -1 bei Fehler.
    /// Hinweis: romfs muss zuvor mit romfsInit() gemountet worden sein.
    public static int LoadWav(string path) => -1;

    /// Gibt einen geladenen Sound frei und stoppt alle seine Voices.
    public static void UnloadSound(int handle) { }

    /// Spielt einen geladenen Sound ab.
    /// handle    : Sound-Handle von LoadWav()
    /// volume    : Lautstärke 0.0–1.0
    /// loop      : true = Endlosschleife bis StopInstance/StopSound
    /// pitch     : 1.0 = Originalgeschwindigkeit, 2.0 = Oktave höher, 0.5 = Oktave tiefer
    /// pan       : -1.0 = links, 0.0 = Mitte, 1.0 = rechts (Equal-Power-Panning)
    /// Gibt eine Voice-Instanz-ID zurück (0–7) oder -1 bei Fehler.
    public static int PlaySound(int handle, float volume, bool loop, float pitch, float pan) => -1;

    /// Stoppt eine bestimmte Wiedergabe-Instanz (ID von PlaySound).
    public static void StopInstance(int instanceId) { }

    /// Stoppt alle Voices eines bestimmten Sounds.
    public static void StopSound(int handle) { }

    /// Stoppt alle laufenden Sample-Voices.
    public static void StopAllSounds() { }

    /// Gibt true zurück, wenn die Wiedergabe-Instanz noch aktiv ist.
    public static bool IsPlaying(int instanceId) => false;

    // ── Musik (ganze Dateien: WAV nativ, MP3/FLAC/OGG via Decoder) ───────────────

    /// Lädt eine komplette Audiodatei (WAV/MP3/FLAC/OGG) als Sound-Handle (-1 = Fehler).
    public static int LoadMusic(string path) => -1;
    /// Pausiert eine Wiedergabe-Instanz (Position bleibt erhalten).
    public static void Pause(int instanceId) { }
    /// Setzt eine pausierte Instanz fort.
    public static void Resume(int instanceId) { }
    /// True, wenn die Instanz pausiert ist.
    public static bool IsPaused(int instanceId) => false;
    /// Aktuelle Wiedergabeposition in Quell-Frames.
    public static int GetPositionFrames(int instanceId) => 0;
    /// Springt zu einer Frame-Position.
    public static void Seek(int instanceId, int frame) { }
    /// Gesamtzahl der Frames eines geladenen Sounds.
    public static int GetSoundFrames(int handle) => 0;
    /// Samplerate eines geladenen Sounds (z. B. 44100).
    public static int GetSoundRate(int handle) => 0;
    /// Anzahl der vorgehaltenen Audiopuffer (je ~21 ms), 2–8. Höher = ruckelfreier
    /// bei schwankender Framerate, aber mehr Latenz. Standard 2; für Musik z. B. 6.
    public static void SetLatencyBuffers(int n) { }

    // ── Effekte ───────────────────────────────────────────────────────────────

    /// Tiefpassfilter auf den Gesamtmix.
    /// cutoffHz: Grenzfrequenz in Hz (20–20000). 0 = deaktivieren.
    /// Beispiel: Audio.SetLowPass(800) → dumpfer, unterwasser-artiger Klang.
    public static void SetLowPass(float cutoffHz) { }

    /// Echo-Effekt auf den Gesamtmix.
    /// delayMs: Verzögerung in ms (50–2000).
    /// decay  : Abklingfaktor pro Wiederholung (0.0 = kein Echo, 0.9 = langer Nachhall).
    /// Beispiel: Audio.SetEcho(300, 0.5) → 300 ms Echo bei halber Lautstärke.
    public static void SetEcho(int delayMs, float decay) { }

    /// Deaktiviert alle Effekte (Tiefpass + Echo).
    public static void ClearEffects() { }

    // ── Wellenform-Konstanten (für SetOscA/B und SetLFO) ─────────────────────

    public const int WAVE_SINE   = 0;
    public const int WAVE_SAW    = 1;
    public const int WAVE_SQUARE = 2;
    public const int WAVE_TRI    = 3;
    public const int WAVE_NOISE  = 4;

    // ── Filter-Typ-Konstanten (für SetFilter) ────────────────────────────────

    public const int FILT_OFF   = 0;
    public const int FILT_LP    = 1;   // Tiefpass
    public const int FILT_HP    = 2;   // Hochpass
    public const int FILT_BP    = 3;   // Bandpass
    public const int FILT_NOTCH = 4;   // Kerbfilter

    // ── LFO-Ziel-Konstanten (für SetLFO) ─────────────────────────────────────

    public const int LFO_PITCH  = 0;
    public const int LFO_VOLUME = 1;
    public const int LFO_FILTER = 2;
    public const int LFO_PAN    = 3;

    // ── Wavetable-Synthesizer: Oszillatoren ───────────────────────────────────

    /// Konfiguriert Oszillator A.
    /// wave         : Wellenform (WAVE_SINE / WAVE_SAW / WAVE_SQUARE / WAVE_TRI / WAVE_NOISE)
    /// level        : Lautstärke 0.0–1.0
    /// detuneCents  : Globale Verstimmung in Cent (100 Cent = 1 Halbton)
    /// unisonCount  : Anzahl gestapelter Stimmen (1–7)
    /// unisonDetune : Gesamt-Verstimmungsbreite der Unison-Stimmen in Cent
    /// unisonSpread : Stereo-Spreizung der Unison-Stimmen 0.0–1.0
    public static void SetOscA(int wave, float level, float detuneCents,
        int unisonCount, float unisonDetune, float unisonSpread) { }

    /// Konfiguriert Oszillator B (identische Parameter wie SetOscA).
    public static void SetOscB(int wave, float level, float detuneCents,
        int unisonCount, float unisonDetune, float unisonSpread) { }

    /// Setzt den Pegel des Sub-Oszillators (Sinus eine Oktave tiefer als OscA).
    /// level: 0.0 = aus, 1.0 = volle Lautstärke
    public static void SetSub(float level) { }

    // ── Wavetable-Synthesizer: Hüllkurve ─────────────────────────────────────

    /// Setzt die Amplituden-Hüllkurve (ADSR) für neu gespielte Noten.
    /// attack_ms  : Anstiegszeit in ms
    /// decay_ms   : Abfallzeit zum Sustain-Pegel in ms
    /// sustain    : Haltepegel 0.0–1.0 (solange Taste gedrückt)
    /// release_ms : Ausklingzeit nach Loslassen in ms
    public static void SetADSR(float attack_ms, float decay_ms, float sustain, float release_ms) { }

    // ── Wavetable-Synthesizer: Filter ─────────────────────────────────────────

    /// Aktiviert einen State-Variable-Filter auf dem Synthesizer-Ausgang.
    /// type      : Filter-Typ (FILT_LP / FILT_HP / FILT_BP / FILT_NOTCH / FILT_OFF)
    /// cutoffHz  : Grenzfrequenz in Hz (20–20000)
    /// resonance : Resonanz 0.0–0.99 (hohe Werte = deutlicher Klangfarben-Peak)
    public static void SetFilter(int type, float cutoffHz, float resonance) { }

    /// Steuert die Filterfrequenz per ADSR-Hüllkurve.
    /// attack_ms / decay_ms / sustain / release_ms: wie SetADSR
    /// amount_octaves: Oktaven-Hub der Cutoff-Modulation (z.B. 2.0 = Cutoff × 4 auf Hüllkurven-Peak)
    public static void SetFilterEnv(float attack_ms, float decay_ms, float sustain,
        float release_ms, float amount_octaves) { }

    // ── Wavetable-Synthesizer: LFO ────────────────────────────────────────────

    /// Konfiguriert den Low-Frequency-Oscillator (LFO).
    /// wave  : LFO-Wellenform (WAVE_SINE empfohlen für weiche Modulation)
    /// rateHz: LFO-Geschwindigkeit in Hz (z.B. 5.0 = klassisches Vibrato)
    /// target: Ziel-Parameter (LFO_PITCH / LFO_VOLUME / LFO_FILTER / LFO_PAN)
    /// depth : Modulationstiefe 0.0–1.0
    public static void SetLFO(int wave, float rateHz, int target, float depth) { }

    // ── Wavetable-Synthesizer: Notentrigger ───────────────────────────────────

    /// Löst eine MIDI-Note aus (Attack-Phase beginnt sofort).
    /// midiNote : MIDI-Notennummer 0–127 (z.B. 60 = mittleres C)
    /// velocity : Anschlagstärke 0–127
    public static void PlayNote(int midiNote, int velocity) { }

    /// Lässt eine MIDI-Note los (Release-Phase beginnt).
    public static void ReleaseNote(int midiNote) { }

    /// Lässt alle gehaltenen Noten gleichzeitig los.
    public static void ReleaseAll() { }

    // ── Wavetable-Synthesizer: Tonhöhe ────────────────────────────────────────

    /// Globale Tonhöhen-Verschiebung in Halbtönen (Pitch-Bend-Rad).
    /// semitones: z.B. 2.0 = zwei Halbtöne höher, -12.0 = eine Oktave tiefer
    public static void SetPitchBend(float semitones) { }
}
