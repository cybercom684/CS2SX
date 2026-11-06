namespace CS2SX.SwitchFormsLib;
public class ProgressBar : Control
{
    public int value;
    public int width_chars;

    public ProgressBar(int w)
    {
        width_chars = w;
        visible = 1;
        focusable = 0;
    }

    public override void Draw()
    {
        if (base.visible == 0) return;
        int fill = (value * width_chars) / 100;
        Console.Write($"\x1B[{base.y};{base.x}H[");
        int i = 0;
        while (i < width_chars)
        {
            if (i < fill)
                Console.Write("#");
            else
                Console.Write("-");
            i++;
        }
        Console.Write($"] {value}%%");
    }
}