namespace CS2SX.SwitchFormsLib;
public class Label : Control
{
    public string text;

    public Label(string t)
    {
        text = t;
        visible = 1;
        focusable = 0;
    }

    public override void Draw()
    {
        if (base.visible == 0) return;
        Console.Write($"\x1B[{base.y};{base.x}H{text}");
    }

    public void SetText(string t)
    {
        text = t;
    }

    public string GetText()
    {
        return text;
    }
}