using System;
using System.Collections.Generic;
using System.Text;

// Strings built by concatenation, interpolation and string.Format are not cut
// off at 1023 characters, and strings put into collections, arrays and fields
// keep their value across frames. Expected output in comments.
public class Note
{
    public string Text = "";
    public string Title { get; set; } = "";
}

public class StringsLongApp : SwitchApp
{
    private string _kept = "";
    private readonly List<string> _list = new List<string>();
    private readonly Dictionary<string, string> _map = new Dictionary<string, string>();
    private readonly string[] _array = new string[3];
    private readonly Note _note = new Note();
    private int _frame;

    private static string Repeat(string part, int times)
    {
        string s = "";
        for (int i = 0; i < times; i++) s += part;
        return s;
    }

    public override void OnFrame()
    {
        _frame++;
        if (_frame == 1)
        {
            string s = Repeat("0123456789", 500);
            Console.WriteLine("concat " + s.Length + " " + s[4999]);              // concat 5000 9
            string wrapped = $"[{s}]";
            Console.WriteLine("interp " + wrapped.Length + " " + wrapped[5001]);  // interp 5002 ]
            string formatted = string.Format("{0}-{1}", s, 7);
            Console.WriteLine("format " + formatted.Length + " " + formatted[5001]);   // format 5002 7

            var sb = new StringBuilder();
            sb.Append(s);
            sb.Append(s);
            string big = sb.ToString();
            string huge = big + big + big + big + big + big + big + big + big;    // 90000: its own block
            Console.WriteLine("huge " + huge.Length + " " + huge[89999]);          // huge 90000 9

            _kept = s + "!";
            for (int i = 0; i < 3; i++)
            {
                _list.Add("item" + i);
                _map["key" + i] = "value" + (i * i);
                _array[i] = "slot" + (i + 10);
            }
            _note.Text = "text " + _frame;
            _note.Title = "title " + _frame;
        }

        // Lots of other strings in between.
        string noise = "";
        for (int i = 0; i < 200; i++) noise = "noise " + i + " " + _frame;

        if (_frame == 40)
        {
            Console.WriteLine("kept " + _kept.Length + " " + _kept[5000]);         // kept 5001 !
            Console.WriteLine(_list[0] + " " + _list[2] + " " + _map["key2"] + " " + _array[1]
                + " " + _note.Text + " " + _note.Title);                           // item0 item2 value4 slot11 text 1 title 1
            Console.WriteLine(noise.Length > 0 ? "done" : "?");                    // done
        }
    }
}
