using System;

namespace CS2SX.SwitchFormsLib
{
    public static class Graphics
    {
        public static void Init(int width, int height) { }
        public static void BeginFrame() { }
        public static void DrawRect(int x, int y, int w, int h, uint color) { }
        public static void DrawTexture(Texture tex, int x, int y) { }
        public static void EndFrame() { }
    }
}