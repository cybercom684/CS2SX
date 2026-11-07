namespace CS2SX.Transpiler.Writers;

/// <summary>
/// Zentralisierte String-Escaping-Logik für C-Code-Generierung.
///
/// Zwei Modi:
/// - EscapeRaw:    Für normale String-Literale (kein %%-Escaping)
/// - EscapeFormat: Für printf/snprintf Format-Strings (% → %%)
///
/// Bug-Fix: EscapeChar behandelte nur \\ und \' — nicht \n, \r, \t, \0 etc.
/// Das führte zu "missing terminating ' character" in GCC wenn
/// s[i] == '\n' transpiliert wurde.
/// </summary>
public static class StringEscaper
{
    /// <summary>
    /// Escaped einen C#-String für die Verwendung als C-String-Literal.
    /// KEIN %%-Escaping — für direkte String-Zuweisung.
    /// </summary>
    public static string EscapeRaw(string s)
    {
        var sb = new System.Text.StringBuilder(s.Length + 8);
        foreach (char c in s)
        {
            switch (c)
            {
                case '\x1B': sb.Append("\\033"); break;  // ESC
                case '\\': sb.Append("\\\\"); break;
                case '"': sb.Append("\\\""); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                case '\0': sb.Append("\\0"); break;
                default:
                    if ((int)c == 27) { sb.Append("\\033"); break; }
                    if ((int)c < 32) { sb.Append($"\\x{(int)c:X2}"); break; }
                    sb.Append(c);
                    break;
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// Escaped einen String für printf/snprintf Format-Strings.
    /// % wird zu %% escaped.
    /// </summary>
    public static string EscapeFormat(string s)
    {
        var raw = EscapeRaw(s);
        return raw.Replace("%", "%%");
    }

    /// <summary>
    /// Escaped einen einzelnen Char für C-Char-Literale.
    ///
    /// Bug-Fix v2: Vorher nur \\ und \' behandelt.
    /// Jetzt alle C-Escape-Sequenzen korrekt — verhindert
    /// "missing terminating ' character" bei '\n', '\r', '\t' etc.
    ///
    /// Eingabe:  der Wert des Char-Tokens (bereits unescaped von Roslyn),
    ///           z.B. "\n" für das Literal '\n' im C#-Quellcode.
    /// Ausgabe:  der escaped Inhalt für 'X' in C, z.B. "\\n"
    /// </summary>
    public static string EscapeChar(string s)
    {
        // s ist der ValueText des Char-Tokens — bereits ein einzelnes Zeichen
        // (Roslyn hat z.B. '\n' bereits zu "\n" aufgelöst, also char 10)
        if (s.Length == 1)
        {
            char c = s[0];
            return c switch
            {
                '\\' => "\\\\",
                '\'' => "\\'",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                '\0' => "\\0",
                '\a' => "\\a",
                '\b' => "\\b",
                '\f' => "\\f",
                '\v' => "\\v",
                '\x1B' => "\\033",
                _ when (int)c < 32 => $"\\x{(int)c:X2}",
                _ => s,
            };
        }

        // Fallback für mehrteilige Eingabe (sollte nicht vorkommen)
        return s.Replace("\\", "\\\\").Replace("'", "\\'");
    }
}