using System;

namespace CS2SX.Switch;

/// <summary>An image in memory (RGBA8888), drawn with <see cref="Graphics.DrawTexture"/>.</summary>
public class Texture : IDisposable
{
    public int Width { get; }
    public int Height { get; }
    public uint[] Pixels { get; }

    public Texture(int width, int height, uint[] pixels)
    {
        Width = width;
        Height = height;
        Pixels = pixels;
    }

    /// <summary>Releases the pixel memory.</summary>
    public void Dispose() { }
}
