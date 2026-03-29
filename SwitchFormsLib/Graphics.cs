using System;

namespace CS2SX.SwitchFormsLib
{
    public static class Graphics
    {
        // Initialisierung
        public static void Init(int width, int height)
        {
        }

        // Frame
        public static void BeginFrame()
        {
        }
        public static void EndFrame()
        {
        }

        // Grundprimitiven
        public static void FillScreen(uint color)
        {
        }
        public static void SetPixel(int x, int y, uint color)
        {
        }
        public static void DrawRect(int x, int y, int w, int h, uint color)
        {
        }
        public static void FillRect(int x, int y, int w, int h, uint color)
        {
        }
        public static void DrawLine(int x0, int y0, int x1, int y1, uint color)
        {
        }
        public static void DrawCircle(int cx, int cy, int r, uint color)
        {
        }
        public static void FillCircle(int cx, int cy, int r, uint color)
        {
        }

        // Text
        public static void DrawText(int x, int y, string text, uint color, int scale)
        {
        }
        public static void DrawChar(int x, int y, char c, uint color, int scale)
        {
        }
        public static int MeasureTextWidth(string text, int scale) => 0;
        public static int MeasureTextHeight(int scale) => 0;

        // Textures
        public static void DrawTexture(Texture tex, int x, int y)
        {
        }
    }

    // Farb-Konstanten
    public static class Color
    {
        public static uint Black = 0xFF000000;
        public static uint White = 0xFFFFFFFF;
        public static uint Red = 0xFF0000FF;
        public static uint Green = 0xFF00C800;
        public static uint Blue = 0xFFFF0000;
        public static uint Yellow = 0xFF00FFFF;
        public static uint Cyan = 0xFFFFFF00;
        public static uint Magenta = 0xFFFF00FF;
        public static uint Gray = 0xFF808080;
        public static uint Orange = 0xFF00A5FF;

        public static uint RGBA(byte r, byte g, byte b, byte a)
            => (uint)((a << 24) | (b << 16) | (g << 8) | r);

        public static uint RGB(byte r, byte g, byte b)
            => RGBA(r, g, b, 255);
    }
}