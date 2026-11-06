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
        // Extended palette — must mirror the COLOR_* macros the transpiler maps to.
        public static uint Pink = 0xFFB469FF;
        public static uint Purple = 0xFF800080;
        public static uint Brown = 0xFF13458B;
        public static uint Teal = 0xFF808000;
        public static uint Lime = 0xFF00FF00;
        public static uint Navy = 0xFF800000;
        public static uint Silver = 0xFFC0C0C0;
        public static uint DarkGray = 0xFF404040;
        public static uint LightGray = 0xFFC0C0C0;
        public static uint Maroon = 0xFF000080;
        public static uint Olive = 0xFF008080;

        public static uint RGBA(byte r, byte g, byte b, byte a)
            => (uint)((a << 24) | (b << 16) | (g << 8) | r);

        public static uint RGB(byte r, byte g, byte b)
            => RGBA(r, g, b, 255);
    }
}