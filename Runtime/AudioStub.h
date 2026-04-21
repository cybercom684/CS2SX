// ============================================================================
// Runtime/AudioStub.h
//
// FIX: Audio-State-Variablen sind nicht mehr static — sie werden einmalig
// in switchforms_globals.c definiert und hier als extern deklariert.
// Das verhindert ODR-Konflikte bei mehrfachem Include.
// ============================================================================

#pragma once
#include <switch.h>
#include <math.h>
#include <string.h>
#include <stdlib.h>

// ── Audio State ──────────────────────────────────────────────────────────────

#define CS2SX_AUDIO_SAMPLE_RATE  48000
#define CS2SX_AUDIO_CHANNELS     2
#define CS2SX_AUDIO_BUF_SAMPLES  4096
#define CS2SX_AUDIO_NUM_BUFS     4

typedef struct
{
    AudioOutBuffer libnx_buf;
    s16* data;
} CS2SX_AudioBuffer;

// FIX: extern statt static — Definition in switchforms_globals.c
extern int               _cs2sx_audio_init;
extern float             _cs2sx_audio_volume;
extern float             _cs2sx_audio_phase;
extern CS2SX_AudioBuffer _cs2sx_audio_bufs[CS2SX_AUDIO_NUM_BUFS];
extern int               _cs2sx_audio_buf_idx;
extern int               _cs2sx_audio_submitted;

static inline int CS2SX_Audio_Init(int sampleRate)
{
    (void)sampleRate;
    if (_cs2sx_audio_init) return 1;

    if (R_FAILED(audoutInitialize())) return 0;
    if (R_FAILED(audoutStartAudioOut())) { audoutExit(); return 0; }

    int bufSize = CS2SX_AUDIO_BUF_SAMPLES * CS2SX_AUDIO_CHANNELS * sizeof(s16);
    for (int i = 0; i < CS2SX_AUDIO_NUM_BUFS; i++)
    {
        _cs2sx_audio_bufs[i].data = (s16*)aligned_alloc(0x1000, bufSize);
        if (!_cs2sx_audio_bufs[i].data) continue;
        memset(_cs2sx_audio_bufs[i].data, 0, bufSize);

        _cs2sx_audio_bufs[i].libnx_buf.next = NULL;
        _cs2sx_audio_bufs[i].libnx_buf.buffer = _cs2sx_audio_bufs[i].data;
        _cs2sx_audio_bufs[i].libnx_buf.buffer_size = (u64)bufSize;
        _cs2sx_audio_bufs[i].libnx_buf.data_size = (u64)bufSize;
        _cs2sx_audio_bufs[i].libnx_buf.data_offset = 0;
    }

    _cs2sx_audio_submitted = 0;
    _cs2sx_audio_init = 1;
    return 1;
}

static inline void CS2SX_Audio_SetVolume(float volume)
{
    if (volume < 0.0f) volume = 0.0f;
    if (volume > 1.0f) volume = 1.0f;
    _cs2sx_audio_volume = volume;
}

static inline void CS2SX_Audio_PlayTone(float freqHz, float amplitude, int duration_ms)
{
    if (!_cs2sx_audio_init) return;

    int   totalSamples = (CS2SX_AUDIO_SAMPLE_RATE * duration_ms) / 1000;
    float phaseInc = 2.0f * 3.14159265f * freqHz / (float)CS2SX_AUDIO_SAMPLE_RATE;
    int   processed = 0;

    while (processed < totalSamples)
    {
        int chunk = CS2SX_AUDIO_BUF_SAMPLES;
        if (processed + chunk > totalSamples)
            chunk = totalSamples - processed;

        CS2SX_AudioBuffer* buf = &_cs2sx_audio_bufs[_cs2sx_audio_buf_idx];
        _cs2sx_audio_buf_idx = (_cs2sx_audio_buf_idx + 1) % CS2SX_AUDIO_NUM_BUFS;

        for (int i = 0; i < chunk; i++)
        {
            float sample = sinf(_cs2sx_audio_phase) * amplitude * _cs2sx_audio_volume;
            s16   pcm = (s16)(sample * 32767.0f);
            buf->data[i * 2] = pcm;
            buf->data[i * 2 + 1] = pcm;
            _cs2sx_audio_phase += phaseInc;
            if (_cs2sx_audio_phase > 2.0f * 3.14159265f)
                _cs2sx_audio_phase -= 2.0f * 3.14159265f;
        }

        buf->libnx_buf.data_size = (u64)(chunk * CS2SX_AUDIO_CHANNELS * sizeof(s16));

        if (_cs2sx_audio_submitted >= CS2SX_AUDIO_NUM_BUFS)
        {
            AudioOutBuffer* released = NULL;
            u32             released_count = 0;
            audoutWaitPlayFinish(&released, &released_count, UINT64_MAX);
        }

        audoutAppendAudioOutBuffer(&buf->libnx_buf);
        _cs2sx_audio_submitted++;

        processed += chunk;
    }
}

static inline void CS2SX_Audio_Stop(void)
{
    if (!_cs2sx_audio_init) return;
    audoutStopAudioOut();
    _cs2sx_audio_submitted = 0;
    _cs2sx_audio_init = 0;
}

static inline void CS2SX_Audio_Exit(void)
{
    CS2SX_Audio_Stop();
    for (int i = 0; i < CS2SX_AUDIO_NUM_BUFS; i++)
        if (_cs2sx_audio_bufs[i].data) free(_cs2sx_audio_bufs[i].data);
    audoutExit();
}