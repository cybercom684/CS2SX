using System;

namespace CS2SX.SwitchFormsLib
{
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

        public void Dispose()
        {
            // Wird zu Texture_Dispose(self) übersetzt
        }
    }
}