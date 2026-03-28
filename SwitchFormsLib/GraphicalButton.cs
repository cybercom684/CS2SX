using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS2SX.SwitchFormsLib
{
    public class GraphicalButton : Button
    {
        public GraphicalButton(string text) : base(text)
        {
        }

        public override void Draw()
        {
            if (Visible == 0) return;
            uint color = (focused == 1) ? 0xFF88FF88 : 0xFF44FF44; // grün
            Graphics.DrawRect(X, Y, Width, Height, color);

            // Text überlagern (falls Console noch aktiv)
            Console.SetCursorPosition(X + 5, Y + Height / 2);
            Console.Write(text);
        }
    }
}