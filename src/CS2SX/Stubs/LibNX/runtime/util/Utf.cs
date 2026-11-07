// Auto-generated from runtime\util\utf.h
// DO NOT EDIT MANUALLY

#pragma warning disable CS0649, CS0169, CS8981

namespace LibNX.Runtime.Util;

public static class Utf
{
    public static extern ssize_t decode_utf8(ref uint32_t @out, ref uint8_t @in);
    public static extern ssize_t decode_utf16(ref uint32_t @out, ref uint16_t @in);
    public static extern ssize_t encode_utf8(ref uint8_t @out, uint32_t @in);
    public static extern ssize_t encode_utf16(ref uint16_t @out, uint32_t @in);
    public static extern ssize_t utf8_to_utf16(ref uint16_t @out, ref uint8_t @in, ulong len);
    public static extern ssize_t utf8_to_utf32(ref uint32_t @out, ref uint8_t @in, ulong len);
    public static extern ssize_t utf16_to_utf8(ref uint8_t @out, ref uint16_t @in, ulong len);
    public static extern ssize_t utf16_to_utf32(ref uint32_t @out, ref uint16_t @in, ulong len);
    public static extern ssize_t utf32_to_utf8(ref uint8_t @out, ref uint32_t @in, ulong len);
    public static extern ssize_t utf32_to_utf16(ref uint16_t @out, ref uint32_t @in, ulong len);
}
