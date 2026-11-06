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

    /// <summary>
    /// Re-uploads <see cref="Pixels"/> to the GPU after you changed them. Textures are
    /// uploaded automatically on first draw; call this only for later modifications.
    /// </summary>
    public void Update() { }

    /// <summary>Releases the texture (CPU and GPU memory). Safe to call more than once.</summary>
    public void Dispose() { }
}
