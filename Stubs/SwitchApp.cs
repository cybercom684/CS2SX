// CS2SX Stub — wird nicht transpiliert, nur für IntelliSense

public class Form
{
    public void Add(Control control)
    {
    }
}

public abstract class Control
{
    public int X
    {
        get; set;
    }
    public int Y
    {
        get; set;
    }
    public int Width
    {
        get; set;
    }
    public int Height
    {
        get; set;
    }
    public bool Visible { get; set; } = true;
}

public abstract class SwitchApp
{
    public Form Form { get; } = new Form();
    public virtual void OnInit()
    {
    }
    public virtual void OnFrame()
    {
    }
    public virtual void OnExit()
    {
    }
}

public class Label : Control
{
    public string Text { get; set; } = "";
    public Label(string text) => Text = text;
}

public class Button : Control
{
    public string Text { get; set; } = "";
    public bool Focused
    {
        get; set;
    }
    public Action OnClick { get; set; } = () => { };
    public Button(string text) => Text = text;
}

public class ProgressBar : Control
{
    public int value
    {
        get; set;
    }
    public int width_chars
    {
        get; set;
    }
    public ProgressBar(int widthChars) => width_chars = widthChars;
}