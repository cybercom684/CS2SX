// Auto-generated from audio\audio.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Audio;

public enum PcmFormat
{
    PcmFormat_Invalid = 0,
    PcmFormat_Int8 = 1,
    PcmFormat_Int16 = 2,
    PcmFormat_Int24 = 3,
    PcmFormat_Int32 = 4,
    PcmFormat_Float = 5,
    PcmFormat_Adpcm = 6,
}

public unsafe struct AudioDeviceName
{
    public byte name;
}

