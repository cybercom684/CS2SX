using System;
using System.Text;

// Strings count characters, not bytes: Length, s[i], Substring, IndexOf, Pad…,
// ToUpper/ToLower and char.* work for umlauts and other letters like in .NET.
// (The output avoids non-ASCII text: the checks compare inside the program.)
public class UnicodeStringsApp : SwitchApp
{
    public override void OnInit()
    {
        string s = "Größe";
        Console.WriteLine("len " + s.Length + " " + "a😀b".Length + " " + "hello".Length);                    // len 5 4 5
        Console.WriteLine("index " + (s[2] == 'ö') + " " + (int)s[3] + " " + (int)"a😀b"[1] + " " + s[4]);   // index True 223 55357 e
        Console.WriteLine("sub " + (s.Substring(0, 3) == "Grö") + " " + (s.Substring(3) == "ße") + " " + "hello".Substring(1, 3));   // sub True True ell
        Console.WriteLine("find " + s.IndexOf("ß") + " " + s.IndexOf('e') + " " + s.LastIndexOf('ö') + " " + "ääx".IndexOf("x"));   // find 3 4 2 2
        Console.WriteLine("case " + (s.ToUpper() == "GRÖßE") + " " + ("ÄÖÜ".ToLower() == "äöü") + " " + "Hello".ToUpper()
            + " " + ("ΑΒΓ".ToLower() == "αβγ") + " " + ("привет".ToUpper() == "ПРИВЕТ"));                         // case True True HELLO True True
        Console.WriteLine("char " + char.IsLetter('ö') + " " + char.IsUpper('Ä') + " " + char.IsLower('ß') + " " + (char.ToUpper('ü') == 'Ü')
            + " " + char.IsWhiteSpace(' ') + " " + char.IsLetter('1') + " " + char.IsDigit('7'));       // char True True True True True False True
        int count = 0, umlauts = 0;
        foreach (char c in s) { count++; if (c > 127) umlauts++; }
        Console.WriteLine("each " + count + " " + umlauts);                                                   // each 5 2
        Console.WriteLine("pad " + "ä".PadRight(3).Length + " " + ("ab".PadLeft(4, '·') == "··ab") + " [" + "ok".PadLeft(4) + "]");   // pad 3 True [  ok]
        Console.WriteLine("edit " + ("Grße".Insert(2, "ö") == s) + " " + (s.Remove(2, 1) == "Grße") + " " + (s.Remove(3) == "Grö"));   // edit True True True
        char[] arr = s.ToCharArray();
        Console.WriteLine("array " + arr.Length + " " + (arr[2] == 'ö') + " " + (new string(arr) == s) + " " + (new string('é', 3) == "ééé") + " " + new string('-', 4));   // array 5 True True True ----
        var sb = new StringBuilder();
        sb.Append('ö').Append("x").Append('ß');
        Console.WriteLine("build " + (sb.ToString() == "öxß") + " " + ("x" + 'ü' == "xü") + " " + ($"{'ä'}" == "ä"));   // build True True True
        try { Console.WriteLine(s[9]); }
        catch (IndexOutOfRangeException) { Console.WriteLine("range ok"); }                                  // range ok
        Console.WriteLine("done");
    }

    public override void OnFrame() { }
}
