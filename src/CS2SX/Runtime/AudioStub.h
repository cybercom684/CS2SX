// ============================================================================
// Runtime/AudioStub.h — Full polyphonic audio + synthesizer engine for CS2SX
//
// Features:
//   • 8-voice polyphonic sine synthesizer        PlayTone()
//   • 8-voice Serum-style wavetable synth        PlayNote() / ReleaseNote()
//     – Oscillators A + B + Sub, each with up to 7 unison voices
//     – Wavetable oscillators: Sine / Saw / Square / Triangle / Noise
//     – Amplitude ADSR envelope
//     – State-Variable Filter (LP / HP / BP / Notch) with resonance
//     – Filter ADSR envelope with octave-scaled modulation
//     – LFO (Sine/Saw/Square/Tri) → Pitch / Volume / Filter / Pan
//     – Pitch bend in semitones
//   • 8-voice WAV sample playback                LoadWav() / PlaySound()
//     – per-voice pitch, volume, stereo pan, loop
//     – linear interpolation resampling (any sample rate → 48 kHz)
//   • Sound bank of 16 loaded WAV files
//   • Effects chain on final mix: low-pass filter + echo / delay
//   • Tanh soft saturation (clean polyphony, no harsh clipping)
//   • Stereo output (independent L + R accumulators)
//
// Call Audio.Update() once per frame in OnFrame().
// Globals are declared extern here; defined once in switchforms.c.
// ============================================================================

#pragma once
#include <switch.h>
#include <math.h>
#include <string.h>
#include <stdlib.h>
#include <stdio.h>

// ── Compile-time constants ────────────────────────────────────────────────────

#define CS2SX_AUDIO_SAMPLE_RATE   48000
#define CS2SX_AUDIO_CHANNELS      2
#define CS2SX_AUDIO_BUF_SAMPLES   1024   // ~21 ms per buffer
#define CS2SX_AUDIO_NUM_BUFS      8      // ~168 ms total DMA capacity
#define CS2SX_MAX_VOICES          8      // PlayTone sine voices
#define CS2SX_MAX_SAMPLE_VOICES   8      // WAV sample voices
#define CS2SX_MAX_SOUNDS          16     // loaded sound bank size
#define CS2SX_ATTACK_SAMPLES      (CS2SX_AUDIO_SAMPLE_RATE * 5 / 1000)  // 5 ms linear attack

// Synth engine
#define CS2SX_MAX_SYNTH_VOICES    8
#define CS2SX_MAX_UNISON          7
#define CS2SX_SYNTH_WT_SIZE       2048   // wavetable entries per waveform

// Wave types (for SetOscA/B and SetLFO)
#define CS2SX_WAVE_SINE     0
#define CS2SX_WAVE_SAW      1
#define CS2SX_WAVE_SQUARE   2
#define CS2SX_WAVE_TRI      3
#define CS2SX_WAVE_NOISE    4

// Filter types (for SetFilter)
#define CS2SX_FILT_OFF      0
#define CS2SX_FILT_LP       1
#define CS2SX_FILT_HP       2
#define CS2SX_FILT_BP       3
#define CS2SX_FILT_NOTCH    4

// LFO targets (for SetLFO)
#define CS2SX_LFO_PITCH     0
#define CS2SX_LFO_VOLUME    1
#define CS2SX_LFO_FILTER    2
#define CS2SX_LFO_PAN       3

// Internal ADSR stage IDs
#define CS2SX_ADSR_ATTACK   0
#define CS2SX_ADSR_DECAY    1
#define CS2SX_ADSR_SUSTAIN  2
#define CS2SX_ADSR_RELEASE  3
#define CS2SX_ADSR_DONE     4

#define CS2SX_TANH_DRIVE    1.2f

// ── Types ─────────────────────────────────────────────────────────────────────

typedef struct {
    AudioOutBuffer libnx_buf;
    s16*           data;
} CS2SX_AudioBuffer;

// PlayTone voice (simple piano-timbre sine)
typedef struct {
    float phase, phaseInc, amplitude;
    float env_cur, env_decay;
    int   remaining, total, active;
} CS2SX_AudioVoice;

// Loaded WAV sound (always stored as interleaved stereo s16)
typedef struct {
    s16*  data;        // [L0,R0, L1,R1, ...]
    int   frames;      // total stereo frame count
    int   sampleRate;  // original sample rate (used for resampling)
    int   valid;
} CS2SX_Sound;

// WAV sample playback voice
typedef struct {
    int   soundIdx;
    float pos;         // playback position in frames (float for interpolation)
    float posInc;      // frames per output sample (srcRate/48000 * pitch)
    float volL, volR;  // equal-power panning applied to volume
    int   loop, active;
    int   paused;      // 1 = keep position but produce no output / no advance
} CS2SX_SampleVoice;

// Synth: oscillator configuration
typedef struct {
    int   wave;          // CS2SX_WAVE_*
    float level;         // 0.0–1.0
    float detuneCents;   // global detune in cents
    int   unisonCount;   // 1–7 stacked voices
    float unisonDetune;  // cents spread across unison voices (e.g. 20 = ±10 cents)
    float unisonSpread;  // stereo spread 0.0–1.0
    int   active;
} CS2SX_OscCfg;

// Synth: ADSR envelope times
typedef struct {
    float attack_ms, decay_ms, sustain, release_ms;
} CS2SX_EnvCfg;

// Synth: filter parameters
typedef struct {
    int   type;       // CS2SX_FILT_*
    float cutoff;     // Hz
    float resonance;  // 0.0–0.99
} CS2SX_FiltCfg;

// Synth: filter envelope
typedef struct {
    float attack_ms, decay_ms, sustain, release_ms;
    float amount;     // octaves of cutoff modulation
} CS2SX_FilterEnvCfg;

// Synth: LFO configuration
typedef struct {
    int   wave;    // CS2SX_WAVE_SINE/SAW/SQUARE/TRI
    float rate;    // Hz
    int   target;  // CS2SX_LFO_*
    float depth;   // 0.0–1.0
} CS2SX_LfoCfg;

// Synth: per-voice state
typedef struct {
    int   midiNote;   // currently held note (-1 = released, voice is in release stage)
    float velocity;   // 0.0–1.0

    // Osc A: up to CS2SX_MAX_UNISON sub-voices
    float oscAPhase   [CS2SX_MAX_UNISON];
    float oscAPhaseInc[CS2SX_MAX_UNISON];
    float oscAPanL    [CS2SX_MAX_UNISON];
    float oscAPanR    [CS2SX_MAX_UNISON];

    // Osc B
    float oscBPhase   [CS2SX_MAX_UNISON];
    float oscBPhaseInc[CS2SX_MAX_UNISON];
    float oscBPanL    [CS2SX_MAX_UNISON];
    float oscBPanR    [CS2SX_MAX_UNISON];

    // Sub oscillator (sine, one octave below OscA)
    float subPhase, subPhaseInc;

    // Amplitude ADSR
    int   adsrStage;
    float adsrLevel, adsrAtkInc, adsrDecMul, adsrSustain, adsrRelMul;

    // Filter ADSR
    int   fadsrStage;
    float fadsrLevel, fadsrAtkInc, fadsrDecMul, fadsrSustain, fadsrRelMul;

    // Chamberlin SVF state (separate L/R to support stereo unison spread)
    float svfLowL, svfBandL;
    float svfLowR, svfBandR;

    // Per-voice noise LCG states
    u32   noiseStateA, noiseStateB;

    int   active;
} CS2SX_SynthVoice;

// ── Externs (defined in switchforms.c) ────────────────────────────────────────

extern int               _cs2sx_audio_init;
extern float             _cs2sx_audio_volume;
extern CS2SX_AudioBuffer _cs2sx_audio_bufs[CS2SX_AUDIO_NUM_BUFS];
extern int               _cs2sx_audio_buf_idx;
extern int               _cs2sx_audio_submitted;
extern int               _cs2sx_audio_target;
extern int               _cs2sx_audio_outrate;   // actual audout playback rate (Hz)

extern CS2SX_AudioVoice  _cs2sx_voices[CS2SX_MAX_VOICES];
extern CS2SX_Sound       _cs2sx_sounds[CS2SX_MAX_SOUNDS];
extern CS2SX_SampleVoice _cs2sx_sample_voices[CS2SX_MAX_SAMPLE_VOICES];

extern float             _cs2sx_mix_accumL[CS2SX_AUDIO_BUF_SAMPLES];
extern float             _cs2sx_mix_accumR[CS2SX_AUDIO_BUF_SAMPLES];

extern float             _cs2sx_lpf_alpha;
extern float             _cs2sx_lpf_prevL;
extern float             _cs2sx_lpf_prevR;
extern int               _cs2sx_lpf_active;

extern float*            _cs2sx_echo_bufL;
extern float*            _cs2sx_echo_bufR;
extern int               _cs2sx_echo_size;
extern int               _cs2sx_echo_pos;
extern float             _cs2sx_echo_decay;
extern int               _cs2sx_echo_active;

// Synth engine globals
extern CS2SX_OscCfg       _cs2sx_osc_a;
extern CS2SX_OscCfg       _cs2sx_osc_b;
extern float              _cs2sx_sub_level;
extern CS2SX_EnvCfg       _cs2sx_amp_env;
extern CS2SX_FiltCfg      _cs2sx_filt_cfg;
extern CS2SX_FilterEnvCfg _cs2sx_filt_env;
extern CS2SX_LfoCfg       _cs2sx_lfo_cfg;
extern float              _cs2sx_lfo_phase;
extern float              _cs2sx_pitch_bend;
extern CS2SX_SynthVoice   _cs2sx_synth_voices[CS2SX_MAX_SYNTH_VOICES];

// ── Static wavetable (private to switchforms.c translation unit) ──────────────

extern float _cs2sx_wt[5][CS2SX_SYNTH_WT_SIZE];
extern int   _cs2sx_wt_ready;

// Bandlimited wavetables computed once at Audio.Init().
// Uses additive synthesis (Fourier series) so high harmonics alias less.
void _cs2sx_init_wavetables(void);

// ── Internal helpers ──────────────────────────────────────────────────────────

// Padé tanh approximation — accurate < 1 % for |x| < 4.5, clamped beyond
float _cs2sx_tanh(float x);

// Wavetable lookup with linear interpolation; phase_01 in [0, 1)
float _cs2sx_wt_read(int wave, float phase_01);

// Oscillator sample; noise uses per-voice LCG so it's uncorrelated across voices
float _cs2sx_osc_samp(int wave, float phase_01, u32* ns);

// MIDI note + pitch-bend → Hz
static inline float _cs2sx_midi_hz(int note, float pbSemitones)
{
    return 440.0f * powf(2.0f, ((float)(note - 69) + pbSemitones) / 12.0f);
}

// Single ADSR step; returns envelope level. held = 1 while key is down.
float _cs2sx_adsr_tick(
    int* stage, float* level,
    float atkInc, float decMul, float sus, float relMul, int held);

// Chamberlin State-Variable Filter step; returns selected output mode
float _cs2sx_svf(float* lo, float* band, float in, float f, float q, int type);

// ── WAV loader (16-bit PCM, mono or stereo, any sample rate) ─────────────────

int _cs2sx_load_wav(const char* path, CS2SX_Sound* out);

// ── Mix: combine all active voices, apply effects, write s16 buffer ───────────

void _cs2sx_mix_and_submit(void);

// ── Public API ────────────────────────────────────────────────────────────────

static inline void CS2SX_Audio_Exit(void);   // forward decl for atexit registration

int CS2SX_Audio_Init(int sampleRate);

static inline void CS2SX_Audio_SetVolume(float volume)
{
    if (volume < 0.0f) volume = 0.0f;
    if (volume > 1.0f) volume = 1.0f;
    _cs2sx_audio_volume = volume;
}

// ── PlayTone sine synthesizer ─────────────────────────────────────────────────

void CS2SX_Audio_PlayTone(float freqHz, float amplitude, int duration_ms);

// ── Wavetable synth: configuration setters ────────────────────────────────────

void CS2SX_Audio_SetOscA(int wave, float level, float detuneCents,
    int unisonCount, float unisonDetune, float unisonSpread);

void CS2SX_Audio_SetOscB(int wave, float level, float detuneCents,
    int unisonCount, float unisonDetune, float unisonSpread);

static inline void CS2SX_Audio_SetSub(float level)
{
    _cs2sx_sub_level = level < 0.0f ? 0.0f : (level > 1.0f ? 1.0f : level);
}

void CS2SX_Audio_SetADSR(float attack_ms, float decay_ms, float sustain, float release_ms);

static inline void CS2SX_Audio_SetFilter(int type, float cutoffHz, float resonance)
{
    _cs2sx_filt_cfg.type      = type;
    _cs2sx_filt_cfg.cutoff    = cutoffHz < 20.0f ? 20.0f : (cutoffHz > 20000.0f ? 20000.0f : cutoffHz);
    _cs2sx_filt_cfg.resonance = resonance < 0.0f ? 0.0f : (resonance > 0.99f ? 0.99f : resonance);
}

void CS2SX_Audio_SetFilterEnv(float attack_ms, float decay_ms, float sustain,
    float release_ms, float amount_octaves);

void CS2SX_Audio_SetLFO(int wave, float rateHz, int target, float depth);

void CS2SX_Audio_SetPitchBend(float semitones);

// ── Wavetable synth: note triggering ─────────────────────────────────────────

void CS2SX_Audio_PlayNote(int midiNote, int velocity);

static inline void CS2SX_Audio_ReleaseNote(int midiNote)
{
    for (int v = 0; v < CS2SX_MAX_SYNTH_VOICES; v++)
        if (_cs2sx_synth_voices[v].active && _cs2sx_synth_voices[v].midiNote == midiNote)
            _cs2sx_synth_voices[v].midiNote = -1;
}

static inline void CS2SX_Audio_ReleaseAll(void)
{
    for (int v = 0; v < CS2SX_MAX_SYNTH_VOICES; v++)
        _cs2sx_synth_voices[v].midiNote = -1;
}

// ── WAV sound bank ────────────────────────────────────────────────────────────

int CS2SX_Audio_LoadWav(const char* path);

void CS2SX_Audio_UnloadSound(int handle);

int CS2SX_Audio_PlaySound(int handle, float volume, int loop, float pitch, float pan);

static inline void CS2SX_Audio_StopInstance(int instanceId)
{
    if (instanceId >= 0 && instanceId < CS2SX_MAX_SAMPLE_VOICES)
        _cs2sx_sample_voices[instanceId].active = 0;
}

static inline void CS2SX_Audio_StopSound(int handle)
{
    for (int v = 0; v < CS2SX_MAX_SAMPLE_VOICES; v++)
        if (_cs2sx_sample_voices[v].active && _cs2sx_sample_voices[v].soundIdx == handle)
            _cs2sx_sample_voices[v].active = 0;
}

static inline void CS2SX_Audio_StopAllSounds(void)
{
    for (int v = 0; v < CS2SX_MAX_SAMPLE_VOICES; v++) _cs2sx_sample_voices[v].active = 0;
}

static inline int CS2SX_Audio_IsPlaying(int instanceId)
{
    if (instanceId < 0 || instanceId >= CS2SX_MAX_SAMPLE_VOICES) return 0;
    return _cs2sx_sample_voices[instanceId].active;
}

// ── Music playback (full files: WAV native, MP3/FLAC/OGG via extern decoder) ───

// Decodes a compressed audio file to interleaved stereo s16. Provided by a
// project linking the draudio extern-lib (externLibs/draudio). Declared here so
// CS2SX_Audio_LoadMusic compiles; only required when an app actually calls it.
extern short* CS2SX_Audio_DecodePCM(const char* path, int* outFrames, int* outRate);

// Loads a full music file into a sound slot. .wav uses the built-in loader;
// other formats go through the extern decoder. Returns a sound handle or -1.
static inline int CS2SX_Audio_LoadMusic(const char* path)
{
    if (!_cs2sx_audio_init || !path) return -1;
    int slot = -1;
    for (int i = 0; i < CS2SX_MAX_SOUNDS; i++)
        if (!_cs2sx_sounds[i].valid) { slot = i; break; }
    if (slot < 0) return -1;

    // Extension check (case-insensitive) — .wav handled natively.
    int n = 0; while (path[n]) n++;
    int isWav = 0;
    if (n >= 4)
    {
        char a = path[n-3], b = path[n-2], c = path[n-1];
        if ((a=='w'||a=='W') && (b=='a'||b=='A') && (c=='v'||c=='V')) isWav = 1;
    }

    if (isWav)
    {
        if (_cs2sx_load_wav(path, &_cs2sx_sounds[slot])) return slot;
        return -1;
    }

    int frames = 0, rate = 0;
    short* pcm = CS2SX_Audio_DecodePCM(path, &frames, &rate);   // stereo interleaved s16
    if (!pcm || frames <= 0 || rate <= 0) { if (pcm) free(pcm); return -1; }
    _cs2sx_sounds[slot].data       = (s16*)pcm;   // sound takes ownership
    _cs2sx_sounds[slot].frames     = frames;
    _cs2sx_sounds[slot].sampleRate = rate;
    _cs2sx_sounds[slot].valid      = 1;
    return slot;
}

static inline void CS2SX_Audio_Pause(int instanceId)
{
    if (instanceId >= 0 && instanceId < CS2SX_MAX_SAMPLE_VOICES)
        _cs2sx_sample_voices[instanceId].paused = 1;
}

static inline void CS2SX_Audio_Resume(int instanceId)
{
    if (instanceId >= 0 && instanceId < CS2SX_MAX_SAMPLE_VOICES)
        _cs2sx_sample_voices[instanceId].paused = 0;
}

static inline int CS2SX_Audio_IsPaused(int instanceId)
{
    if (instanceId < 0 || instanceId >= CS2SX_MAX_SAMPLE_VOICES) return 0;
    return _cs2sx_sample_voices[instanceId].paused;
}

// Current playback position (in source frames) of a playing instance.
static inline int CS2SX_Audio_GetPositionFrames(int instanceId)
{
    if (instanceId < 0 || instanceId >= CS2SX_MAX_SAMPLE_VOICES) return 0;
    return (int)_cs2sx_sample_voices[instanceId].pos;
}

void CS2SX_Audio_Seek(int instanceId, int frame);

static inline int CS2SX_Audio_GetSoundFrames(int handle)
{
    if (handle < 0 || handle >= CS2SX_MAX_SOUNDS || !_cs2sx_sounds[handle].valid) return 0;
    return _cs2sx_sounds[handle].frames;
}

static inline int CS2SX_Audio_GetSoundRate(int handle)
{
    if (handle < 0 || handle >= CS2SX_MAX_SOUNDS || !_cs2sx_sounds[handle].valid) return 0;
    return _cs2sx_sounds[handle].sampleRate;
}

// ── Effects ───────────────────────────────────────────────────────────────────

void CS2SX_Audio_SetLowPass(float cutoffHz);

void CS2SX_Audio_SetEcho(int delayMs, float decay);

static inline void CS2SX_Audio_ClearEffects(void)
{
    _cs2sx_lpf_active  = 0;
    _cs2sx_echo_active = 0;
}

// ── Per-frame update ──────────────────────────────────────────────────────────

void CS2SX_Audio_Update(void);

// Sets how many audio buffers (each ~21 ms) to keep queued. Higher = more robust
// against frame stalls (no crackling), at the cost of audio latency. Range 2..8.
static inline void CS2SX_Audio_SetLatencyBuffers(int n)
{
    if (n < 2) n = 2;
    if (n > CS2SX_AUDIO_NUM_BUFS) n = CS2SX_AUDIO_NUM_BUFS;
    _cs2sx_audio_target = n;
}

// ── Cleanup ───────────────────────────────────────────────────────────────────

void CS2SX_Audio_Stop(void);

void CS2SX_Audio_Exit(void);
