namespace CS2SX.SwitchFormsLib;
public class Control
{
    public int x;
    public int y;
    public int width;
    public int height;
    public int visible;
    public int focusable;

    public virtual void Draw()
    {
    }
    public virtual void Update(ulong kDown, ulong kHeld)
    {
    }
}