using System;

namespace CS2SX.Switch;

/// Where Stream.Seek counts from.
public enum SeekOrigin { Begin, Current, End }

/// A sequence of bytes: a file (FileStream, from File.OpenRead / File.Create /
/// File.OpenWrite) or memory (MemoryStream). Dispose closes it — `using` does
/// that at the end of the block.
public abstract class Stream : IDisposable
{
    /// Number of bytes.
    public long Length => 0;

    /// Where the next read or write happens.
    public long Position { get => 0; set { } }

    /// Reads up to count bytes into buffer[offset…]; returns how many it read (0 at the end).
    public int Read(byte[] buffer, int offset, int count) => 0;

    /// The next byte, or -1 at the end.
    public int ReadByte() => -1;

    public void Write(byte[] buffer, int offset, int count) { }
    public void WriteByte(byte value) { }
    public long Seek(long offset, SeekOrigin origin) => 0;
    public void Flush() { }
    public void Close() { }
    public void Dispose() { }
}

/// A file opened as a stream: File.OpenRead(path), File.Create(path), File.OpenWrite(path).
public sealed class FileStream : Stream { private FileStream() { } }

/// Bytes in memory: new MemoryStream() to write, new MemoryStream(bytes) to read.
public sealed class MemoryStream : Stream
{
    public MemoryStream() { }
    public MemoryStream(byte[] buffer) { }

    /// The bytes written so far.
    public byte[] ToArray() => Array.Empty<byte>();
}

/// Reads numbers, strings and bytes in the .NET binary format (little endian;
/// strings with their length in front) — the files BinaryWriter writes. Dispose
/// closes the stream.
public sealed class BinaryReader : IDisposable
{
    public BinaryReader(Stream input) { BaseStream = input; }
    public Stream BaseStream { get; }
    public byte ReadByte() => 0;
    public sbyte ReadSByte() => 0;
    public bool ReadBoolean() => false;
    public short ReadInt16() => 0;
    public ushort ReadUInt16() => 0;
    public int ReadInt32() => 0;
    public uint ReadUInt32() => 0;
    public long ReadInt64() => 0;
    public ulong ReadUInt64() => 0;
    public float ReadSingle() => 0;
    public double ReadDouble() => 0;
    /// Up to count bytes (fewer at the end of the stream).
    public byte[] ReadBytes(int count) => Array.Empty<byte>();
    public string ReadString() => "";
    public void Close() { }
    public void Dispose() { }
}

/// Writes numbers, strings and bytes in the .NET binary format (see BinaryReader).
/// Dispose closes the stream.
public sealed class BinaryWriter : IDisposable
{
    public BinaryWriter(Stream output) { BaseStream = output; }
    public Stream BaseStream { get; }
    public void Write(byte value) { }
    public void Write(sbyte value) { }
    public void Write(bool value) { }
    public void Write(short value) { }
    public void Write(ushort value) { }
    public void Write(int value) { }
    public void Write(uint value) { }
    public void Write(long value) { }
    public void Write(ulong value) { }
    public void Write(float value) { }
    public void Write(double value) { }
    public void Write(string value) { }
    public void Write(byte[] buffer) { }
    public void Write(byte[] buffer, int index, int count) { }
    public void Flush() { }
    public void Close() { }
    public void Dispose() { }
}
