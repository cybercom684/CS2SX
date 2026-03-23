namespace CS2SX.Transpiler.Writers;

/// <summary>
/// Zentralisierte String-Escaping-Logik für C-Code-Generierung.
///
/// Zwei Modi:
/// - EscapeRaw:    Für normale String-Literale (kein %%-Escaping)
/// - EscapeFormat: Für printf/snprintf Format-Strings (% → %%)
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
                case '\\':   sb.Append("\\\\");  break;
                case '"':    sb.Append("\\\"");  break;
                case '\n':   sb.Append("\\n");   break;
                case '\r':   sb.Append("\\r");   break;
                case '\t':   sb.Append("\\t");   break;
                case '\0':   sb.Append("\\0");   break;
                default:
                    if ((int)c == 27) { sb.Append("\\033"); break; }
                    if ((int)c < 32)  { sb.Append($"\\x{(int)c:X2}"); break; }
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
        // % → %% (aber nicht wenn bereits escaped: \%% bleibt)
        return raw.Replace("%", "%%");
    }

    /// <summary>
    /// Escaped einen Char für C-Char-Literale.
    /// </summary>
    public static string EscapeChar(string s)
        => s.Replace("\\", "\\\\").Replace("'", "\\'");
}
