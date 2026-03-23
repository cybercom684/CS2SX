using CS2SX.SwitchFormsLib;
using System;

namespace CS2SX.SwitchFormsLib;
public class Button : Control
{
    public string text;
    public int focused;
    public System.Action OnClick;

    public Button(string t)
    {
        text = t;
        visible = 1;
        focusable = 1;
    }

    public override void Draw()
    {
        if (base.visible == 0) return;
        if (focused == 1)
            Console.Write($"\x1B[{base.y};{base.x}H> {text} <");
        else
            Console.Write($"\x1B[{base.y};{base.x}H  {text}  ");
    }

    public override void Update(ulong kDown, ulong kHeld)
    {
        if (focused == 1)
        {
            if (Input.IsDown(NpadButton.A))
                OnClick();
        }
    }
}