// String fields own their value: written in frames 1–3 from per-frame temporaries
// (interpolation, concatenation, +=), they must still be intact in frame 5 after
// the per-frame string buffers were reused many times. Overwritten values are
// freed (CS2SX_STR_STORE), not leaked.
public class Label2
{
    public string Text { get; set; } = "init";
    public string Name = "n";
}

public class StringFieldsApp : SwitchApp
{
    private string _status = "start";
    private static string s_last = "";
    private string _log = "";
    private readonly Label2 _label = new Label2 { Name = "obj" };
    private int _frame;

    public override void OnFrame()
    {
        _frame++;
        if (_frame <= 3)
        {
            _status = $"frame {_frame}";
            _log += _frame + ";";
            s_last = "f" + _frame;
            _label.Text = $"text {_frame}";
            _label.Name = "obj" + _frame;
            _status = _status;
        }
        if (_frame == 5)
        {
            int len = 0;
            for (int i = 0; i < 2000; i++)
            {
                var junk = $"junk {i} {i * i}";
                len += junk.Length;
            }
            System.Console.WriteLine(_status + " | " + _log + " | " + s_last + " | " + _label.Text + " | " + _label.Name);
            System.Console.WriteLine("churn " + (len > 0));
        }
    }
}
