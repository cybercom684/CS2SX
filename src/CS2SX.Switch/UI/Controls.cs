using System;

namespace CS2SX.Switch;

/// <summary>Container of the console-mode UI controls of a <see cref="SwitchApp"/>.</summary>
public class Form
{
    public void Add(Control control) { }

    /// <summary>Updates every control (focus navigation, button presses).</summary>
    public void UpdateAll(ulong kDown, ulong kHeld) { }

    /// <summary>Draws every visible control.</summary>
    public void DrawAll() { }
}

/// <summary>Base class of console-mode UI controls. Derive from it for custom controls.</summary>
public abstract class Control
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool Visible { get; set; } = true;
    public bool Focusable { get; set; }

    /// <summary>Draws the control. Override in custom controls.</summary>
    public virtual void Draw() { }

    /// <summary>Handles input for the control. Override in custom controls.</summary>
    public virtual void Update(ulong kDown, ulong kHeld) { }
}

public class Label : Control
{
    public string Text { get; set; } = "";

    public Label() { }
    public Label(string text) => Text = text;

    public void SetText(string text) => Text = text;
    public string GetText() => Text;
}

public class Button : Control
{
    public string Text { get; set; } = "";
    public bool Focused { get; set; }

    /// <summary>Invoked when the focused button is pressed with A.</summary>
    public Action OnClick { get; set; } = () => { };

    public Button() { }
    public Button(string text) => Text = text;
}

public class ProgressBar : Control
{
    /// <summary>Progress in percent (0–100).</summary>
    public int value { get; set; }

    /// <summary>Bar width in characters.</summary>
    public int width_chars { get; set; }

    public ProgressBar(int widthChars) => width_chars = widthChars;
}
