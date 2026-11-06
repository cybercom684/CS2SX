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

static float _cs2sx_wt[5][CS2SX_SYNTH_WT_SIZE];
static int   _cs2sx_wt_ready = 0;

// Bandlimited wavetables computed once at Audio.Init().
// Uses additive synthesis (Fourier series) so high harmonics alias less.
static inline void _cs2sx_init_wavetables(void)
{
    if (_cs2sx_wt_ready) return;
    const float TWO_PI = 6.28318530f;

    for (int i = 0; i < CS2SX_SYNTH_WT_SIZE; i++)
    {
        float t = (float)i / (float)CS2SX_SYNTH_WT_SIZE;

        // Sine — exact
        _cs2sx_wt[CS2SX_WAVE_SINE][i] = sinf(t * TWO_PI);

        // Bandlimited saw: Σ (-1)^(k+1) sin(k·2πt)/k, k=1..50
        { float s = 0.0f;
          for (int k = 1; k <= 50; k++)
              s += sinf((float)k * TWO_PI * t) / (float)k * (k % 2 == 1 ? 1.0f : -1.0f);
          _cs2sx_wt[CS2SX_WAVE_SAW][i] = s * (2.0f / 3.14159265f); }

        // Bandlimited square: Σ sin((2k-1)·2πt)/(2k-1), k=1..50
        { float s = 0.0f;
          for (int k = 1; k <= 99; k += 2)
              s += sinf((float)k * TWO_PI * t) / (float)k;
          _cs2sx_wt[CS2SX_WAVE_SQUARE][i] = s * (4.0f / 3.14159265f); }

        // Bandlimited triangle: Σ (-1)^k sin((2k+1)·2πt)/(2k+1)², k=0..49
        { float s = 0.0f; int sg = 1;
          for (int k = 1; k <= 99; k += 2) {
              s += (float)sg * sinf((float)k * TWO_PI * t) / ((float)k * (float)k);
              sg = -sg; }
          _cs2sx_wt[CS2SX_WAVE_TRI][i] = s * (8.0f / (3.14159265f * 3.14159265f)); }

        // Noise: pre-seeded pseudo-random (per-voice LCG overrides at runtime)
        { u32 st = (u32)i * 1664525u + 1013904223u;
          _cs2sx_wt[CS2SX_WAVE_NOISE][i] = (float)(s32)st / 2147483648.0f; }
    }
    _cs2sx_wt_ready = 1;
}

// ── Internal helpers ──────────────────────────────────────────────────────────

// Padé tanh approximation — accurate < 1 % for |x| < 4.5, clamped beyond
static inline float _cs2sx_tanh(float x)
{
    if (x >  4.5f) return  1.0f;
    if (x < -4.5f) return -1.0f;
    float x2 = x * x;
    return x * (27.0f + x2) / (27.0f + 9.0f * x2);
}

// Wavetable lookup with linear interpolation; phase_01 in [0, 1)
static inline float _cs2sx_wt_read(int wave, float phase_01)
{
    float fi = phase_01 * (float)CS2SX_SYNTH_WT_SIZE;
    int   i0 = (int)fi & (CS2SX_SYNTH_WT_SIZE - 1);
    int   i1 = (i0 + 1) & (CS2SX_SYNTH_WT_SIZE - 1);
    float fr = fi - (float)(int)fi;
    return _cs2sx_wt[wave][i0] + (_cs2sx_wt[wave][i1] - _cs2sx_wt[wave][i0]) * fr;
}

// Oscillator sample; noise uses per-voice LCG so it's uncorrelated across voices
static inline float _cs2sx_osc_samp(int wave, float phase_01, u32* ns)
{
    if (wave == CS2SX_WAVE_NOISE) {
        *ns = *ns * 1664525u + 1013904223u;
        return (float)(s32)(*ns) / 2147483648.0f;
    }
    return _cs2sx_wt_read(wave, phase_01);
}

// MIDI note + pitch-bend → Hz
static inline float _cs2sx_midi_hz(int note, float pbSemitones)
{
    return 440.0f * powf(2.0f, ((float)(note - 69) + pbSemitones) / 12.0f);
}

// Single ADSR step; returns envelope level. held = 1 while key is down.
static inline float _cs2sx_adsr_tick(
    int* stage, float* level,
    float atkInc, float decMul, float sus, float relMul, int held)
{
    switch (*stage) {
    case CS2SX_ADSR_ATTACK:
        *level += atkInc;
        if (*level >= 1.0f) { *level = 1.0f; *stage = CS2SX_ADSR_DECAY; }
        if (!held) { *stage = CS2SX_ADSR_RELEASE; }
        break;
    case CS2SX_ADSR_DECAY:
        *level = sus + (*level - sus) * decMul;
        if ((*level - sus) < 0.0005f) { *level = sus; *stage = CS2SX_ADSR_SUSTAIN; }
        if (!held) { *stage = CS2SX_ADSR_RELEASE; }
        break;
    case CS2SX_ADSR_SUSTAIN:
        *level = sus;
        if (!held) *stage = CS2SX_ADSR_RELEASE;
        break;
    case CS2SX_ADSR_RELEASE:
        *level *= relMul;
        if (*level < 0.0001f) { *level = 0.0f; *stage = CS2SX_ADSR_DONE; }
        break;
    default: *level = 0.0f; break;
    }
    return *level;
}

// Chamberlin State-Variable Filter step; returns selected output mode
static inline float _cs2sx_svf(float* lo, float* band, float in, float f, float q, int type)
{
    float hi   = in - *lo - q * (*band);
    *band     += f * hi;
    *lo       += f * (*band);
    float notch = hi + *lo;
    switch (type) {
    case CS2SX_FILT_LP:    return *lo;
    case CS2SX_FILT_HP:    return hi;
    case CS2SX_FILT_BP:    return *band;
    case CS2SX_FILT_NOTCH: return notch;
    default:               return in;
    }
}

// ── WAV loader (16-bit PCM, mono or stereo, any sample rate) ─────────────────

static inline int _cs2sx_load_wav(const char* path, CS2SX_Sound* out)
{
    FILE* f = fopen(path, "rb");
    if (!f) return 0;

    char id[4]; u32 chunkSz;
    if (fread(id, 1, 4, f) < 4 || id[0]!='R'||id[1]!='I'||id[2]!='F'||id[3]!='F')
        { fclose(f); return 0; }
    fread(&chunkSz, 4, 1, f);
    if (fread(id, 1, 4, f) < 4 || id[0]!='W'||id[1]!='A'||id[2]!='V'||id[3]!='E')
        { fclose(f); return 0; }

    u16 audioFmt = 0, channels = 0, bitsPerSample = 0;
    u32 sampleRate = 0;

    while (!feof(f)) {
        if (fread(id, 1, 4, f) < 4) break;
        if (fread(&chunkSz, 4, 1, f) < 1) break;

        if (id[0]=='f' && id[1]=='m' && id[2]=='t' && id[3]==' ') {
            fread(&audioFmt,     2, 1, f);
            fread(&channels,     2, 1, f);
            fread(&sampleRate,   4, 1, f);
            fseek(f, 6, SEEK_CUR);  // skip byteRate + blockAlign
            fread(&bitsPerSample, 2, 1, f);
            if (chunkSz > 16) fseek(f, (long)(chunkSz - 16), SEEK_CUR);
        }
        else if (id[0]=='d' && id[1]=='a' && id[2]=='t' && id[3]=='a') {
            if (audioFmt != 1 || bitsPerSample != 16 || channels == 0 || channels > 2)
                { fclose(f); return 0; }

            // chunkSz is attacker/corruption-controlled — guard against a negative
            // or absurd frame count (integer overflow / huge allocation).
            int frames = (int)((u32)chunkSz / 2u / (u32)channels);
            if (frames <= 0 || frames > 48000 * 600 /* ~10 min stereo cap */)
                { fclose(f); return 0; }
            out->data = (s16*)malloc((size_t)frames * 2 * sizeof(s16));
            if (!out->data) { fclose(f); return 0; }

            size_t got;
            if (channels == 2) {
                got = fread(out->data, sizeof(s16), (size_t)frames * 2, f);
                frames = (int)(got / 2);   // clamp to what was actually read
            } else {
                s16* tmp = (s16*)malloc((size_t)frames * sizeof(s16));
                if (!tmp) { free(out->data); out->data = NULL; fclose(f); return 0; }
                got = fread(tmp, sizeof(s16), (size_t)frames, f);
                frames = (int)got;
                for (int i = 0; i < frames; i++)
                    { out->data[i*2] = tmp[i]; out->data[i*2+1] = tmp[i]; }
                free(tmp);
            }

            out->frames     = frames;
            out->sampleRate = (int)sampleRate;
            out->valid      = 1;
            fclose(f);
            return 1;
        }
        else fseek(f, (long)chunkSz, SEEK_CUR);
    }
    fclose(f);
    return 0;
}

// ── Mix: combine all active voices, apply effects, write s16 buffer ───────────

static inline void _cs2sx_mix_and_submit(void)
{
    // Clear stereo accumulators
    for (int i = 0; i < CS2SX_AUDIO_BUF_SAMPLES; i++)
        _cs2sx_mix_accumL[i] = _cs2sx_mix_accumR[i] = 0.0f;

    // ── PlayTone voices (piano timbre: fundamental + 2nd + 3rd harmonic) ──────
    for (int v = 0; v < CS2SX_MAX_VOICES; v++) {
        CS2SX_AudioVoice* vo = &_cs2sx_voices[v];
        if (!vo->active) continue;

        int n = CS2SX_AUDIO_BUF_SAMPLES;
        if (n > vo->remaining) n = vo->remaining;
        int played = vo->total - vo->remaining;

        for (int i = 0; i < n; i++) {
            int g = played + i;
            float env;
            if (g < CS2SX_ATTACK_SAMPLES)
                { env = (float)(g + 1) / (float)CS2SX_ATTACK_SAMPLES; vo->env_cur = env; }
            else
                { vo->env_cur *= vo->env_decay; env = vo->env_cur; }

            float ph = vo->phase;
            float s  = (sinf(ph) * 0.70f + sinf(ph * 2.0f) * 0.20f + sinf(ph * 3.0f) * 0.10f)
                       * vo->amplitude * env;

            _cs2sx_mix_accumL[i] += s;
            _cs2sx_mix_accumR[i] += s;

            vo->phase += vo->phaseInc;
            if (vo->phase > 6.28318530f) vo->phase -= 6.28318530f;
        }
        vo->remaining -= n;
        if (vo->remaining <= 0) vo->active = 0;
    }

    // ── WAV sample voices ─────────────────────────────────────────────────────
    for (int v = 0; v < CS2SX_MAX_SAMPLE_VOICES; v++) {
        CS2SX_SampleVoice* sv = &_cs2sx_sample_voices[v];
        if (!sv->active || sv->paused) continue;   // paused: hold position, no output
        CS2SX_Sound* snd = &_cs2sx_sounds[sv->soundIdx];
        if (!snd->valid) { sv->active = 0; continue; }

        for (int i = 0; i < CS2SX_AUDIO_BUF_SAMPLES; i++) {
            int   f0   = (int)sv->pos;
            float frac = sv->pos - (float)f0;

            if (f0 >= snd->frames) {
                if (sv->loop) { sv->pos -= (float)snd->frames; f0 = (int)sv->pos; frac = sv->pos - (float)f0; }
                else { sv->active = 0; break; }
            }
            if (f0 < 0 || f0 >= snd->frames) { sv->active = 0; break; }

            int   f1     = (f0 + 1 < snd->frames) ? f0 + 1 : (sv->loop ? 0 : f0);
            float invMax = 1.0f / 32767.0f;
            float sL = ((float)snd->data[f0*2]   * (1.0f-frac) + (float)snd->data[f1*2]   * frac) * invMax;
            float sR = ((float)snd->data[f0*2+1] * (1.0f-frac) + (float)snd->data[f1*2+1] * frac) * invMax;

            _cs2sx_mix_accumL[i] += sL * sv->volL;
            _cs2sx_mix_accumR[i] += sR * sv->volR;
            sv->pos += sv->posInc;
        }
    }

    // ── Wavetable synth voices ────────────────────────────────────────────────
    if (_cs2sx_wt_ready)
    {
        // LFO: advance by one buffer's worth, evaluate at buffer start
        float lfoVal = _cs2sx_wt_read(_cs2sx_lfo_cfg.wave, _cs2sx_lfo_phase);
        _cs2sx_lfo_phase += _cs2sx_lfo_cfg.rate * (float)CS2SX_AUDIO_BUF_SAMPLES
                            / (float)CS2SX_AUDIO_SAMPLE_RATE;
        if (_cs2sx_lfo_phase >= 1.0f) _cs2sx_lfo_phase -= (float)(int)(_cs2sx_lfo_phase + 1.0f);
        float lfoDep = _cs2sx_lfo_cfg.depth;

        // Pitch factor for LFO_PITCH: ±2 semitones at max depth
        float pitchFac = 1.0f;
        if (_cs2sx_lfo_cfg.target == CS2SX_LFO_PITCH && lfoDep > 0.0f)
            pitchFac = powf(2.0f, lfoVal * lfoDep * 2.0f / 12.0f);

        for (int v = 0; v < CS2SX_MAX_SYNTH_VOICES; v++)
        {
            CS2SX_SynthVoice* sv = &_cs2sx_synth_voices[v];
            if (!sv->active) continue;

            int held = (sv->midiNote >= 0);

            // Per-buffer filter parameters (LFO + filter-env midpoint)
            float fCutoff = _cs2sx_filt_cfg.cutoff;
            if (_cs2sx_filt_cfg.type != CS2SX_FILT_OFF) {
                if (_cs2sx_filt_env.amount != 0.0f)
                    fCutoff *= powf(2.0f, sv->fadsrLevel * _cs2sx_filt_env.amount);
                if (_cs2sx_lfo_cfg.target == CS2SX_LFO_FILTER && lfoDep > 0.0f)
                    fCutoff *= powf(2.0f, lfoVal * lfoDep * 2.0f);  // ±2 octaves
                if (fCutoff > 20000.0f) fCutoff = 20000.0f;
                if (fCutoff < 20.0f)    fCutoff = 20.0f;
            }
            float svff = 2.0f * sinf(3.14159265f * fCutoff / (float)CS2SX_AUDIO_SAMPLE_RATE);
            float svfq = 2.0f - 2.0f * _cs2sx_filt_cfg.resonance;
            if (svff > 1.95f) svff = 1.95f;
            if (svfq < 0.05f) svfq = 0.05f;

            int   uA    = _cs2sx_osc_a.unisonCount;
            int   uB    = _cs2sx_osc_b.unisonCount;
            if (uA < 1) uA = 1; if (uA > CS2SX_MAX_UNISON) uA = CS2SX_MAX_UNISON;
            if (uB < 1) uB = 1; if (uB > CS2SX_MAX_UNISON) uB = CS2SX_MAX_UNISON;
            float normA = _cs2sx_osc_a.level / (float)uA;
            float normB = _cs2sx_osc_b.level / (float)uB;

            // Per-sample processing
            for (int i = 0; i < CS2SX_AUDIO_BUF_SAMPLES; i++)
            {
                // Amplitude ADSR
                float ampEnv = _cs2sx_adsr_tick(
                    &sv->adsrStage, &sv->adsrLevel,
                    sv->adsrAtkInc, sv->adsrDecMul, sv->adsrSustain, sv->adsrRelMul, held);
                if (sv->adsrStage == CS2SX_ADSR_DONE) { sv->active = 0; break; }

                // Filter ADSR (result used next buffer for cutoff modulation)
                _cs2sx_adsr_tick(
                    &sv->fadsrStage, &sv->fadsrLevel,
                    sv->fadsrAtkInc, sv->fadsrDecMul, sv->fadsrSustain, sv->fadsrRelMul, held);

                // Volume LFO: ±50% amplitude at max depth
                float volMod = (_cs2sx_lfo_cfg.target == CS2SX_LFO_VOLUME)
                    ? (1.0f + lfoVal * lfoDep * 0.5f) : 1.0f;

                float sL = 0.0f, sR = 0.0f;

                // Osc A
                if (_cs2sx_osc_a.active && normA > 0.0f) {
                    for (int u = 0; u < uA; u++) {
                        float s = _cs2sx_osc_samp(_cs2sx_osc_a.wave, sv->oscAPhase[u], &sv->noiseStateA) * normA;
                        sv->oscAPhase[u] += sv->oscAPhaseInc[u] * pitchFac;
                        if (sv->oscAPhase[u] >= 1.0f) sv->oscAPhase[u] -= 1.0f;
                        sL += s * sv->oscAPanL[u];
                        sR += s * sv->oscAPanR[u];
                    }
                }

                // Osc B
                if (_cs2sx_osc_b.active && normB > 0.0f) {
                    for (int u = 0; u < uB; u++) {
                        float s = _cs2sx_osc_samp(_cs2sx_osc_b.wave, sv->oscBPhase[u], &sv->noiseStateB) * normB;
                        sv->oscBPhase[u] += sv->oscBPhaseInc[u] * pitchFac;
                        if (sv->oscBPhase[u] >= 1.0f) sv->oscBPhase[u] -= 1.0f;
                        sL += s * sv->oscBPanL[u];
                        sR += s * sv->oscBPanR[u];
                    }
                }

                // Sub oscillator (sine, one octave below)
                if (_cs2sx_sub_level > 0.0f) {
                    float sub = _cs2sx_wt_read(CS2SX_WAVE_SINE, sv->subPhase) * _cs2sx_sub_level;
                    sv->subPhase += sv->subPhaseInc * pitchFac;
                    if (sv->subPhase >= 1.0f) sv->subPhase -= 1.0f;
                    sL += sub; sR += sub;
                }

                // SVF filter (applied to L and R independently)
                if (_cs2sx_filt_cfg.type != CS2SX_FILT_OFF) {
                    sL = _cs2sx_svf(&sv->svfLowL, &sv->svfBandL, sL, svff, svfq, _cs2sx_filt_cfg.type);
                    sR = _cs2sx_svf(&sv->svfLowR, &sv->svfBandR, sR, svff, svfq, _cs2sx_filt_cfg.type);
                }

                float gain = ampEnv * sv->velocity * volMod;
                _cs2sx_mix_accumL[i] += sL * gain;
                _cs2sx_mix_accumR[i] += sR * gain;
            }
        }
    }

    // ── Echo effect ───────────────────────────────────────────────────────────
    if (_cs2sx_echo_active && _cs2sx_echo_bufL && _cs2sx_echo_size > 0) {
        for (int i = 0; i < CS2SX_AUDIO_BUF_SAMPLES; i++) {
            float dL = _cs2sx_echo_bufL[_cs2sx_echo_pos];
            float dR = _cs2sx_echo_bufR[_cs2sx_echo_pos];
            _cs2sx_echo_bufL[_cs2sx_echo_pos] = _cs2sx_mix_accumL[i] + dL * _cs2sx_echo_decay;
            _cs2sx_echo_bufR[_cs2sx_echo_pos] = _cs2sx_mix_accumR[i] + dR * _cs2sx_echo_decay;
            _cs2sx_mix_accumL[i] += dL;
            _cs2sx_mix_accumR[i] += dR;
            _cs2sx_echo_pos = (_cs2sx_echo_pos + 1) % _cs2sx_echo_size;
        }
    }

    // ── Low-pass filter (one-pole IIR) ────────────────────────────────────────
    if (_cs2sx_lpf_active) {
        float a = _cs2sx_lpf_alpha, b = 1.0f - a;
        for (int i = 0; i < CS2SX_AUDIO_BUF_SAMPLES; i++) {
            _cs2sx_lpf_prevL = b * _cs2sx_mix_accumL[i] + a * _cs2sx_lpf_prevL;
            _cs2sx_lpf_prevR = b * _cs2sx_mix_accumR[i] + a * _cs2sx_lpf_prevR;
            _cs2sx_mix_accumL[i] = _cs2sx_lpf_prevL;
            _cs2sx_mix_accumR[i] = _cs2sx_lpf_prevR;
        }
    }

    // ── Tanh soft saturation → s16 PCM ───────────────────────────────────────
    CS2SX_AudioBuffer* buf = &_cs2sx_audio_bufs[_cs2sx_audio_buf_idx];
    _cs2sx_audio_buf_idx   = (_cs2sx_audio_buf_idx + 1) % CS2SX_AUDIO_NUM_BUFS;
    float vol = _cs2sx_audio_volume * CS2SX_TANH_DRIVE;
    for (int i = 0; i < CS2SX_AUDIO_BUF_SAMPLES; i++) {
        buf->data[i * 2]     = (s16)(_cs2sx_tanh(_cs2sx_mix_accumL[i] * vol) * 32767.0f);
        buf->data[i * 2 + 1] = (s16)(_cs2sx_tanh(_cs2sx_mix_accumR[i] * vol) * 32767.0f);
    }

    buf->libnx_buf.data_size = (u64)(CS2SX_AUDIO_BUF_SAMPLES * CS2SX_AUDIO_CHANNELS * sizeof(s16));
    // CRITICAL: the audout DSP reads this buffer from RAM via DMA. Flush the CPU
    // data cache so it sees the samples we just wrote — without this the DSP plays
    // stale/partial cache contents → constant crackling regardless of queue depth.
    armDCacheFlush(buf->libnx_buf.buffer, buf->libnx_buf.buffer_size);
    audoutAppendAudioOutBuffer(&buf->libnx_buf);
    _cs2sx_audio_submitted++;
}

// ── Public API ────────────────────────────────────────────────────────────────

static inline void CS2SX_Audio_Exit(void);   // forward decl for atexit registration

static inline int CS2SX_Audio_Init(int sampleRate)
{
    (void)sampleRate;
    if (_cs2sx_audio_init) return 1;
    if (R_FAILED(audoutInitialize()))    return 0;
    if (R_FAILED(audoutStartAudioOut())) { audoutExit(); return 0; }

    // Use the device's REAL output rate for resampling. If we assume 48000 but the
    // hardware runs at another rate, every sound plays at the wrong speed.
    {
        u32 hwRate = audoutGetSampleRate();
        _cs2sx_audio_outrate = (hwRate > 0) ? (int)hwRate : CS2SX_AUDIO_SAMPLE_RATE;
    }

    // Ensure audout is torn down even if the app exits via the + button
    // (SwitchApp_Run does not call Audio_Exit). Leaving audout open is a
    // known Atmosphere crash-on-exit vector.
    atexit(CS2SX_Audio_Exit);

    int bufBytes = CS2SX_AUDIO_BUF_SAMPLES * CS2SX_AUDIO_CHANNELS * sizeof(s16);
    int aligned  = (bufBytes + 0xFFF) & ~0xFFF;
    for (int i = 0; i < CS2SX_AUDIO_NUM_BUFS; i++) {
        _cs2sx_audio_bufs[i].data = (s16*)aligned_alloc(0x1000, aligned);
        if (!_cs2sx_audio_bufs[i].data) continue;
        memset(_cs2sx_audio_bufs[i].data, 0, aligned);
        _cs2sx_audio_bufs[i].libnx_buf.next        = NULL;
        _cs2sx_audio_bufs[i].libnx_buf.buffer      = _cs2sx_audio_bufs[i].data;
        _cs2sx_audio_bufs[i].libnx_buf.buffer_size = (u64)aligned;
        _cs2sx_audio_bufs[i].libnx_buf.data_size   = (u64)bufBytes;
        _cs2sx_audio_bufs[i].libnx_buf.data_offset = 0;
    }
    for (int v = 0; v < CS2SX_MAX_VOICES; v++)        _cs2sx_voices[v].active        = 0;
    for (int v = 0; v < CS2SX_MAX_SAMPLE_VOICES; v++) _cs2sx_sample_voices[v].active = 0;
    for (int v = 0; v < CS2SX_MAX_SYNTH_VOICES; v++) {
        _cs2sx_synth_voices[v].active    = 0;
        _cs2sx_synth_voices[v].midiNote  = -1;
        _cs2sx_synth_voices[v].adsrStage = CS2SX_ADSR_DONE;
    }

    // Default synth config: OscA = sine at full level, everything else off
    _cs2sx_osc_a.wave        = CS2SX_WAVE_SINE;
    _cs2sx_osc_a.level       = 1.0f;
    _cs2sx_osc_a.detuneCents = 0.0f;
    _cs2sx_osc_a.unisonCount = 1;
    _cs2sx_osc_a.unisonDetune = 0.0f;
    _cs2sx_osc_a.unisonSpread = 0.0f;
    _cs2sx_osc_a.active      = 1;
    _cs2sx_osc_b.active      = 0;
    _cs2sx_osc_b.level       = 0.0f;
    _cs2sx_sub_level         = 0.0f;
    _cs2sx_amp_env.attack_ms  = 10.0f;
    _cs2sx_amp_env.decay_ms   = 200.0f;
    _cs2sx_amp_env.sustain    = 0.7f;
    _cs2sx_amp_env.release_ms = 300.0f;
    _cs2sx_filt_cfg.type      = CS2SX_FILT_OFF;
    _cs2sx_filt_cfg.cutoff    = 8000.0f;
    _cs2sx_filt_cfg.resonance = 0.5f;
    _cs2sx_filt_env.amount    = 0.0f;
    _cs2sx_filt_env.attack_ms  = 10.0f;
    _cs2sx_filt_env.decay_ms   = 200.0f;
    _cs2sx_filt_env.sustain    = 0.0f;
    _cs2sx_filt_env.release_ms = 200.0f;
    _cs2sx_lfo_cfg.wave   = CS2SX_WAVE_SINE;
    _cs2sx_lfo_cfg.rate   = 5.0f;
    _cs2sx_lfo_cfg.target = CS2SX_LFO_PITCH;
    _cs2sx_lfo_cfg.depth  = 0.0f;
    _cs2sx_lfo_phase      = 0.0f;
    _cs2sx_pitch_bend     = 0.0f;

    _cs2sx_audio_submitted = 0;
    _cs2sx_audio_volume    = 1.0f;
    _cs2sx_audio_init      = 1;

    _cs2sx_init_wavetables();
    return 1;
}

static inline void CS2SX_Audio_SetVolume(float volume)
{
    if (volume < 0.0f) volume = 0.0f;
    if (volume > 1.0f) volume = 1.0f;
    _cs2sx_audio_volume = volume;
}

// ── PlayTone sine synthesizer ─────────────────────────────────────────────────

static inline void CS2SX_Audio_PlayTone(float freqHz, float amplitude, int duration_ms)
{
    if (!_cs2sx_audio_init) return;
    float phaseInc  = 2.0f * 3.14159265f * freqHz / (float)CS2SX_AUDIO_SAMPLE_RATE;
    int   total     = CS2SX_AUDIO_SAMPLE_RATE * duration_ms / 1000;
    float env_decay = (total > 0) ? expf(-2.302585f / (float)total) : 1.0f;

    int slot = 0, minRem = 0x7FFFFFFF;
    for (int v = 0; v < CS2SX_MAX_VOICES; v++) {
        if (!_cs2sx_voices[v].active)            { slot = v; minRem = -1; break; }
        if (_cs2sx_voices[v].remaining < minRem)   { minRem = _cs2sx_voices[v].remaining; slot = v; }
    }
    _cs2sx_voices[slot].phase     = 0.0f;
    _cs2sx_voices[slot].phaseInc  = phaseInc;
    _cs2sx_voices[slot].amplitude = amplitude;
    _cs2sx_voices[slot].env_cur   = 0.0f;
    _cs2sx_voices[slot].env_decay = env_decay;
    _cs2sx_voices[slot].remaining = total;
    _cs2sx_voices[slot].total     = total;
    _cs2sx_voices[slot].active    = 1;

    if (_cs2sx_audio_submitted < CS2SX_AUDIO_NUM_BUFS)
        _cs2sx_mix_and_submit();
}

// ── Wavetable synth: configuration setters ────────────────────────────────────

static inline void CS2SX_Audio_SetOscA(int wave, float level, float detuneCents,
    int unisonCount, float unisonDetune, float unisonSpread)
{
    _cs2sx_osc_a.wave        = wave;
    _cs2sx_osc_a.level       = level < 0.0f ? 0.0f : level;
    _cs2sx_osc_a.detuneCents = detuneCents;
    _cs2sx_osc_a.unisonCount = unisonCount < 1 ? 1 : (unisonCount > CS2SX_MAX_UNISON ? CS2SX_MAX_UNISON : unisonCount);
    _cs2sx_osc_a.unisonDetune = unisonDetune;
    _cs2sx_osc_a.unisonSpread = unisonSpread < 0.0f ? 0.0f : (unisonSpread > 1.0f ? 1.0f : unisonSpread);
    _cs2sx_osc_a.active      = (level > 0.0f) ? 1 : 0;
}

static inline void CS2SX_Audio_SetOscB(int wave, float level, float detuneCents,
    int unisonCount, float unisonDetune, float unisonSpread)
{
    _cs2sx_osc_b.wave        = wave;
    _cs2sx_osc_b.level       = level < 0.0f ? 0.0f : level;
    _cs2sx_osc_b.detuneCents = detuneCents;
    _cs2sx_osc_b.unisonCount = unisonCount < 1 ? 1 : (unisonCount > CS2SX_MAX_UNISON ? CS2SX_MAX_UNISON : unisonCount);
    _cs2sx_osc_b.unisonDetune = unisonDetune;
    _cs2sx_osc_b.unisonSpread = unisonSpread < 0.0f ? 0.0f : (unisonSpread > 1.0f ? 1.0f : unisonSpread);
    _cs2sx_osc_b.active      = (level > 0.0f) ? 1 : 0;
}

static inline void CS2SX_Audio_SetSub(float level)
{
    _cs2sx_sub_level = level < 0.0f ? 0.0f : (level > 1.0f ? 1.0f : level);
}

static inline void CS2SX_Audio_SetADSR(float attack_ms, float decay_ms, float sustain, float release_ms)
{
    _cs2sx_amp_env.attack_ms  = attack_ms  > 0.0f ? attack_ms  : 1.0f;
    _cs2sx_amp_env.decay_ms   = decay_ms   > 0.0f ? decay_ms   : 1.0f;
    _cs2sx_amp_env.sustain    = sustain < 0.0f ? 0.0f : (sustain > 1.0f ? 1.0f : sustain);
    _cs2sx_amp_env.release_ms = release_ms > 0.0f ? release_ms : 1.0f;
}

static inline void CS2SX_Audio_SetFilter(int type, float cutoffHz, float resonance)
{
    _cs2sx_filt_cfg.type      = type;
    _cs2sx_filt_cfg.cutoff    = cutoffHz < 20.0f ? 20.0f : (cutoffHz > 20000.0f ? 20000.0f : cutoffHz);
    _cs2sx_filt_cfg.resonance = resonance < 0.0f ? 0.0f : (resonance > 0.99f ? 0.99f : resonance);
}

static inline void CS2SX_Audio_SetFilterEnv(float attack_ms, float decay_ms, float sustain,
    float release_ms, float amount_octaves)
{
    _cs2sx_filt_env.attack_ms  = attack_ms  > 0.0f ? attack_ms  : 1.0f;
    _cs2sx_filt_env.decay_ms   = decay_ms   > 0.0f ? decay_ms   : 1.0f;
    _cs2sx_filt_env.sustain    = sustain < 0.0f ? 0.0f : (sustain > 1.0f ? 1.0f : sustain);
    _cs2sx_filt_env.release_ms = release_ms > 0.0f ? release_ms : 1.0f;
    _cs2sx_filt_env.amount     = amount_octaves;
}

static inline void CS2SX_Audio_SetLFO(int wave, float rateHz, int target, float depth)
{
    _cs2sx_lfo_cfg.wave   = wave;
    _cs2sx_lfo_cfg.rate   = rateHz > 0.0f ? rateHz : 0.0f;
    _cs2sx_lfo_cfg.target = target;
    _cs2sx_lfo_cfg.depth  = depth < 0.0f ? 0.0f : (depth > 1.0f ? 1.0f : depth);
}

static inline void CS2SX_Audio_SetPitchBend(float semitones)
{
    _cs2sx_pitch_bend = semitones;
    for (int v = 0; v < CS2SX_MAX_SYNTH_VOICES; v++) {
        CS2SX_SynthVoice* sv = &_cs2sx_synth_voices[v];
        if (!sv->active || sv->midiNote < 0) continue;
        float freq   = _cs2sx_midi_hz(sv->midiNote, _cs2sx_pitch_bend);
        float fsRate = (float)CS2SX_AUDIO_SAMPLE_RATE;
        int uA = _cs2sx_osc_a.unisonCount;
        if (uA < 1) uA = 1; if (uA > CS2SX_MAX_UNISON) uA = CS2SX_MAX_UNISON;
        for (int u = 0; u < uA; u++) {
            float sp = (uA > 1) ? (float)(u * 2 - (uA - 1)) / (float)(uA - 1) : 0.0f;
            float det = _cs2sx_osc_a.detuneCents + sp * _cs2sx_osc_a.unisonDetune * 0.5f;
            sv->oscAPhaseInc[u] = freq * powf(2.0f, det / 1200.0f) / fsRate;
        }
        int uB = _cs2sx_osc_b.unisonCount;
        if (uB < 1) uB = 1; if (uB > CS2SX_MAX_UNISON) uB = CS2SX_MAX_UNISON;
        float freqB = freq * powf(2.0f, _cs2sx_osc_b.detuneCents / 1200.0f);
        for (int u = 0; u < uB; u++) {
            float sp  = (uB > 1) ? (float)(u * 2 - (uB - 1)) / (float)(uB - 1) : 0.0f;
            float det = sp * _cs2sx_osc_b.unisonDetune * 0.5f;
            sv->oscBPhaseInc[u] = freqB * powf(2.0f, det / 1200.0f) / fsRate;
        }
        sv->subPhaseInc = freq * 0.5f / fsRate;
    }
}

// ── Wavetable synth: note triggering ─────────────────────────────────────────

static inline void CS2SX_Audio_PlayNote(int midiNote, int velocity)
{
    if (!_cs2sx_audio_init || !_cs2sx_wt_ready) return;
    if (midiNote < 0 || midiNote > 127) return;
    if (velocity < 0) velocity = 0;
    if (velocity > 127) velocity = 127;

    // Prefer a free voice; otherwise steal same-note first, then deepest-release
    int slot = -1;
    for (int v = 0; v < CS2SX_MAX_SYNTH_VOICES; v++) {
        if (_cs2sx_synth_voices[v].active && _cs2sx_synth_voices[v].midiNote == midiNote)
            { slot = v; break; }
        if (!_cs2sx_synth_voices[v].active && slot < 0)
            slot = v;
    }
    if (slot < 0) {
        // Steal the voice farthest into its release stage
        float minLvl = 2.0f;
        for (int v = 0; v < CS2SX_MAX_SYNTH_VOICES; v++) {
            if (_cs2sx_synth_voices[v].adsrStage == CS2SX_ADSR_RELEASE &&
                _cs2sx_synth_voices[v].adsrLevel < minLvl) {
                minLvl = _cs2sx_synth_voices[v].adsrLevel;
                slot   = v;
            }
        }
        if (slot < 0) slot = 0;  // last resort: overwrite slot 0
    }

    CS2SX_SynthVoice* sv = &_cs2sx_synth_voices[slot];
    float freq   = _cs2sx_midi_hz(midiNote, _cs2sx_pitch_bend);
    float fsRate = (float)CS2SX_AUDIO_SAMPLE_RATE;

    // Precompute ADSR multipliers from current global config
    float atkSamp  = _cs2sx_amp_env.attack_ms  * (fsRate / 1000.0f);
    float decSamp  = _cs2sx_amp_env.decay_ms   * (fsRate / 1000.0f);
    float relSamp  = _cs2sx_amp_env.release_ms * (fsRate / 1000.0f);
    float fAtkSamp = _cs2sx_filt_env.attack_ms  * (fsRate / 1000.0f);
    float fDecSamp = _cs2sx_filt_env.decay_ms   * (fsRate / 1000.0f);
    float fRelSamp = _cs2sx_filt_env.release_ms * (fsRate / 1000.0f);

    sv->midiNote     = midiNote;
    sv->velocity     = (float)velocity / 127.0f;
    sv->adsrStage    = CS2SX_ADSR_ATTACK;
    sv->adsrLevel    = 0.0f;
    sv->adsrAtkInc   = atkSamp  > 0.0f ? 1.0f / atkSamp  : 1.0f;
    sv->adsrDecMul   = decSamp  > 0.0f ? expf(-6.908f / decSamp)  : 0.001f;
    sv->adsrSustain  = _cs2sx_amp_env.sustain;
    sv->adsrRelMul   = relSamp  > 0.0f ? expf(-6.908f / relSamp)  : 0.001f;
    sv->fadsrStage   = CS2SX_ADSR_ATTACK;
    sv->fadsrLevel   = 0.0f;
    sv->fadsrAtkInc  = fAtkSamp > 0.0f ? 1.0f / fAtkSamp : 1.0f;
    sv->fadsrDecMul  = fDecSamp > 0.0f ? expf(-6.908f / fDecSamp) : 0.001f;
    sv->fadsrSustain = _cs2sx_filt_env.sustain;
    sv->fadsrRelMul  = fRelSamp > 0.0f ? expf(-6.908f / fRelSamp) : 0.001f;
    sv->svfLowL = sv->svfBandL = sv->svfLowR = sv->svfBandR = 0.0f;
    sv->noiseStateA  = (u32)midiNote * 1664525u + 1013904223u;
    sv->noiseStateB  = sv->noiseStateA ^ 0xDEADBEEFu;

    // OscA: compute phase increments and stereo panning per unison voice
    int uA = _cs2sx_osc_a.unisonCount;
    if (uA < 1) uA = 1; if (uA > CS2SX_MAX_UNISON) uA = CS2SX_MAX_UNISON;
    for (int u = 0; u < uA; u++) {
        float sp  = (uA > 1) ? (float)(u * 2 - (uA - 1)) / (float)(uA - 1) : 0.0f;
        float det = _cs2sx_osc_a.detuneCents + sp * _cs2sx_osc_a.unisonDetune * 0.5f;
        sv->oscAPhaseInc[u] = freq * powf(2.0f, det / 1200.0f) / fsRate;
        sv->oscAPhase[u]    = (float)u / (float)(uA > 1 ? uA : 1);  // stagger phases for fat start
        float panT  = sp * _cs2sx_osc_a.unisonSpread;
        float angle = (panT + 1.0f) * (3.14159265f * 0.25f);
        sv->oscAPanL[u] = cosf(angle);
        sv->oscAPanR[u] = sinf(angle);
    }
    for (int u = uA; u < CS2SX_MAX_UNISON; u++)
        sv->oscAPhaseInc[u] = sv->oscAPanL[u] = sv->oscAPanR[u] = 0.0f;

    // OscB
    int uB = _cs2sx_osc_b.unisonCount;
    if (uB < 1) uB = 1; if (uB > CS2SX_MAX_UNISON) uB = CS2SX_MAX_UNISON;
    float freqB = freq * powf(2.0f, _cs2sx_osc_b.detuneCents / 1200.0f);
    for (int u = 0; u < uB; u++) {
        float sp  = (uB > 1) ? (float)(u * 2 - (uB - 1)) / (float)(uB - 1) : 0.0f;
        float det = sp * _cs2sx_osc_b.unisonDetune * 0.5f;
        sv->oscBPhaseInc[u] = freqB * powf(2.0f, det / 1200.0f) / fsRate;
        sv->oscBPhase[u]    = (float)u / (float)(uB > 1 ? uB : 1);
        float panT  = sp * _cs2sx_osc_b.unisonSpread;
        float angle = (panT + 1.0f) * (3.14159265f * 0.25f);
        sv->oscBPanL[u] = cosf(angle);
        sv->oscBPanR[u] = sinf(angle);
    }
    for (int u = uB; u < CS2SX_MAX_UNISON; u++)
        sv->oscBPhaseInc[u] = sv->oscBPanL[u] = sv->oscBPanR[u] = 0.0f;

    // Sub: one octave below
    sv->subPhase    = 0.0f;
    sv->subPhaseInc = freq * 0.5f / fsRate;

    sv->active = 1;

    if (_cs2sx_audio_submitted < CS2SX_AUDIO_NUM_BUFS)
        _cs2sx_mix_and_submit();
}

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

static inline int CS2SX_Audio_LoadWav(const char* path)
{
    if (!_cs2sx_audio_init) return -1;
    for (int i = 0; i < CS2SX_MAX_SOUNDS; i++) {
        if (!_cs2sx_sounds[i].valid) {
            if (_cs2sx_load_wav(path, &_cs2sx_sounds[i])) return i;
            return -1;
        }
    }
    return -1;
}

static inline void CS2SX_Audio_UnloadSound(int handle)
{
    if (handle < 0 || handle >= CS2SX_MAX_SOUNDS) return;
    for (int v = 0; v < CS2SX_MAX_SAMPLE_VOICES; v++)
        if (_cs2sx_sample_voices[v].active && _cs2sx_sample_voices[v].soundIdx == handle)
            _cs2sx_sample_voices[v].active = 0;
    free(_cs2sx_sounds[handle].data);
    _cs2sx_sounds[handle].data  = NULL;
    _cs2sx_sounds[handle].valid = 0;
}

static inline int CS2SX_Audio_PlaySound(int handle, float volume, int loop, float pitch, float pan)
{
    if (!_cs2sx_audio_init || handle < 0 || handle >= CS2SX_MAX_SOUNDS) return -1;
    CS2SX_Sound* snd = &_cs2sx_sounds[handle];
    if (!snd->valid) return -1;

    if (pitch <= 0.0f) pitch = 1.0f;
    if (pan < -1.0f) pan = -1.0f;
    if (pan >  1.0f) pan =  1.0f;

    float panAngle = (pan + 1.0f) * (3.14159265f * 0.25f);
    float pL = cosf(panAngle);
    float pR = sinf(panAngle);

    int slot = -1; float maxRel = -1.0f;
    for (int v = 0; v < CS2SX_MAX_SAMPLE_VOICES; v++) {
        if (!_cs2sx_sample_voices[v].active) { slot = v; break; }
        int sf = _cs2sx_sounds[_cs2sx_sample_voices[v].soundIdx].frames;
        float rel = sf > 0 ? _cs2sx_sample_voices[v].pos / (float)sf : 1.0f;
        if (rel > maxRel) { maxRel = rel; slot = v; }
    }
    if (slot < 0) return -1;

    _cs2sx_sample_voices[slot].soundIdx = handle;
    _cs2sx_sample_voices[slot].pos      = 0.0f;
    _cs2sx_sample_voices[slot].posInc   = (float)snd->sampleRate / (float)_cs2sx_audio_outrate * pitch;
    _cs2sx_sample_voices[slot].volL     = volume * pL;
    _cs2sx_sample_voices[slot].volR     = volume * pR;
    _cs2sx_sample_voices[slot].loop     = loop;
    _cs2sx_sample_voices[slot].active   = 1;
    _cs2sx_sample_voices[slot].paused   = 0;

    if (_cs2sx_audio_submitted < CS2SX_AUDIO_NUM_BUFS)
        _cs2sx_mix_and_submit();
    return slot;
}

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

static inline void CS2SX_Audio_Seek(int instanceId, int frame)
{
    if (instanceId < 0 || instanceId >= CS2SX_MAX_SAMPLE_VOICES) return;
    CS2SX_SampleVoice* sv = &_cs2sx_sample_voices[instanceId];
    if (!sv->active) return;
    int total = _cs2sx_sounds[sv->soundIdx].frames;
    if (frame < 0) frame = 0;
    if (frame > total - 1) frame = total - 1;
    sv->pos = (float)frame;
}

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

static inline void CS2SX_Audio_SetLowPass(float cutoffHz)
{
    if (cutoffHz <= 0.0f) { _cs2sx_lpf_active = 0; return; }
    float omega = 2.0f * 3.14159265f * cutoffHz / (float)CS2SX_AUDIO_SAMPLE_RATE;
    _cs2sx_lpf_alpha  = expf(-omega);
    _cs2sx_lpf_prevL  = 0.0f;
    _cs2sx_lpf_prevR  = 0.0f;
    _cs2sx_lpf_active = 1;
}

static inline void CS2SX_Audio_SetEcho(int delayMs, float decay)
{
    int size = CS2SX_AUDIO_SAMPLE_RATE * delayMs / 1000;
    if (size <= 0) { _cs2sx_echo_active = 0; return; }

    if (size != _cs2sx_echo_size || !_cs2sx_echo_bufL) {
        free(_cs2sx_echo_bufL);
        free(_cs2sx_echo_bufR);
        _cs2sx_echo_bufL = (float*)calloc((size_t)size, sizeof(float));
        _cs2sx_echo_bufR = (float*)calloc((size_t)size, sizeof(float));
        _cs2sx_echo_size = size;
        _cs2sx_echo_pos  = 0;
    }
    _cs2sx_echo_decay  = decay;
    _cs2sx_echo_active = (_cs2sx_echo_bufL != NULL);
}

static inline void CS2SX_Audio_ClearEffects(void)
{
    _cs2sx_lpf_active  = 0;
    _cs2sx_echo_active = 0;
}

// ── Per-frame update ──────────────────────────────────────────────────────────

static inline void CS2SX_Audio_Update(void)
{
    if (!_cs2sx_audio_init) return;

    // Reclaim ALL finished buffers (loop, not once): if the frame rate drops below
    // ~47 fps, more than one buffer finishes per frame — reclaiming only one would
    // desync the in-flight count and starve the queue → crackle.
    {
        AudioOutBuffer* rel; u32 cnt;
        while (_cs2sx_audio_submitted > 0
            && R_SUCCEEDED(audoutGetReleasedAudioOutBuffer(&rel, &cnt)) && cnt > 0)
        {
            _cs2sx_audio_submitted -= (int)cnt;
            if (_cs2sx_audio_submitted < 0) _cs2sx_audio_submitted = 0;
        }
    }

    int anyActive = 0;
    for (int v = 0; v < CS2SX_MAX_VOICES        && !anyActive; v++) if (_cs2sx_voices[v].active)        anyActive = 1;
    for (int v = 0; v < CS2SX_MAX_SAMPLE_VOICES && !anyActive; v++) if (_cs2sx_sample_voices[v].active) anyActive = 1;
    for (int v = 0; v < CS2SX_MAX_SYNTH_VOICES  && !anyActive; v++) if (_cs2sx_synth_voices[v].active)  anyActive = 1;
    if (!anyActive) return;

    // Keep the DMA queue topped up to _cs2sx_audio_target buffers. A deeper queue
    // tolerates frame-time jitter (heavy rendering, decode hitches) without the
    // underruns that cause crackling/stuttering. Default 2 (~42 ms, low latency);
    // raise via Audio.SetLatencyBuffers for music-style playback.
    int target = _cs2sx_audio_target;
    if (target < 2) target = 2;
    if (target > CS2SX_AUDIO_NUM_BUFS) target = CS2SX_AUDIO_NUM_BUFS;
    while (_cs2sx_audio_submitted < target)
        _cs2sx_mix_and_submit();
}

// Sets how many audio buffers (each ~21 ms) to keep queued. Higher = more robust
// against frame stalls (no crackling), at the cost of audio latency. Range 2..8.
static inline void CS2SX_Audio_SetLatencyBuffers(int n)
{
    if (n < 2) n = 2;
    if (n > CS2SX_AUDIO_NUM_BUFS) n = CS2SX_AUDIO_NUM_BUFS;
    _cs2sx_audio_target = n;
}

// ── Cleanup ───────────────────────────────────────────────────────────────────

static inline void CS2SX_Audio_Stop(void)
{
    if (!_cs2sx_audio_init) return;
    for (int v = 0; v < CS2SX_MAX_VOICES;        v++) _cs2sx_voices[v].active        = 0;
    for (int v = 0; v < CS2SX_MAX_SAMPLE_VOICES; v++) _cs2sx_sample_voices[v].active = 0;
    for (int v = 0; v < CS2SX_MAX_SYNTH_VOICES;  v++) { _cs2sx_synth_voices[v].active = 0; _cs2sx_synth_voices[v].midiNote = -1; }
    audoutStopAudioOut();
    _cs2sx_audio_submitted = 0;
    _cs2sx_audio_init      = 0;
}

static inline void CS2SX_Audio_Exit(void)
{
    CS2SX_Audio_Stop();
    for (int i = 0; i < CS2SX_AUDIO_NUM_BUFS; i++)
        if (_cs2sx_audio_bufs[i].data)
            { free(_cs2sx_audio_bufs[i].data); _cs2sx_audio_bufs[i].data = NULL; }
    for (int i = 0; i < CS2SX_MAX_SOUNDS; i++)
        if (_cs2sx_sounds[i].valid)
            { free(_cs2sx_sounds[i].data); _cs2sx_sounds[i].data = NULL; _cs2sx_sounds[i].valid = 0; }
    free(_cs2sx_echo_bufL); _cs2sx_echo_bufL = NULL;
    free(_cs2sx_echo_bufR); _cs2sx_echo_bufR = NULL;
    audoutExit();
}
