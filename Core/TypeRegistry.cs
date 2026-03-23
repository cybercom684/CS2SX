namespace CS2SX.Core;

/// <summary>
/// Zentrale Typ-Registry — einzige Quelle der Wahrheit für alle C#→C Typ-Mappings.
///
/// Erweiterung: Einfach einen neuen Eintrag in die entsprechende Kategorie hinzufügen.
/// Keine Änderungen an Transpiler-Code nötig.
/// </summary>
public static class TypeRegistry
{
    // ── Primitive C#→C Typ-Mappings ──────────────────────────────────────────

    private static readonly Dictionary<string, string> s_primitives = new(StringComparer.Ordinal)
    {
        // C# Standard
        ["int"]     = "int",
        ["uint"]    = "unsigned int",
        ["long"]    = "long long",
        ["ulong"]   = "unsigned long long",
        ["short"]   = "short",
        ["ushort"]  = "unsigned short",
        ["byte"]    = "unsigned char",
        ["sbyte"]   = "signed char",
        ["float"]   = "float",
        ["double"]  = "double",
        ["bool"]    = "bool",
        ["char"]    = "char",
        ["void"]    = "void",
        ["string"]  = "const char*",
        ["object"]  = "void*",

        // Delegate-Typ
        ["Action"]  = "Action",

        // StringBuilder
        ["StringBuilder"] = "StringBuilder",

        // libnx Typen
        ["u8"]      = "u8",
        ["u16"]     = "u16",
        ["u32"]     = "u32",
        ["u64"]     = "u64",
        ["s8"]      = "s8",
        ["s16"]     = "s16",
        ["s32"]     = "s32",
        ["s64"]     = "s64",
        ["Result"]  = "Result",
        ["Handle"]  = "Handle",

        // libnx FS-Structs
        ["FsDir"]               = "FsDir",
        ["FsFile"]              = "FsFile",
        ["FsFileSystem"]        = "FsFileSystem",
        ["FsDirectoryEntry"]    = "FsDirectoryEntry",

        // libnx sonstige Structs
        ["PadState"]            = "PadState",
        ["HidTouchScreenState"] = "HidTouchScreenState",
        ["AccountUid"]          = "AccountUid",
        ["PsmChargerType"]      = "PsmChargerType",
    };

    // ── libnx Stack-Structs (kein Pointer, Stack-Allokation) ─────────────────

    private static readonly HashSet<string> s_libNxStructs = new(StringComparer.Ordinal)
    {
        "FsDir", "FsFile", "FsFileSystem", "FsDirectoryEntry",
        "PadState", "HidTouchScreenState", "AccountUid", "PsmChargerType",
    };

    // ── Pointer-Typen (werden mit * deklariert, aber kein extra * nötig) ──────
    // Typen die bereits im Map zu einem Pointer-Typ werden

    private static readonly HashSet<string> s_nativePointerTypes = new(StringComparer.Ordinal)
    {
        "StringBuilder", "Action",
    };

    // ── printf Format-Specifier ───────────────────────────────────────────────

    private static readonly Dictionary<string, string> s_formatSpecifiers = new(StringComparer.Ordinal)
    {
        ["int"]                 = "%d",
        ["short"]               = "%d",
        ["signed char"]         = "%d",
        ["s8"]                  = "%d",
        ["s16"]                 = "%d",
        ["s32"]                 = "%d",
        ["unsigned int"]        = "%u",
        ["unsigned short"]      = "%u",
        ["unsigned char"]       = "%u",
        ["u8"]                  = "%u",
        ["u16"]                 = "%u",
        ["u32"]                 = "%u",
        ["long long"]           = "%lld",
        ["s64"]                 = "%lld",
        ["unsigned long long"]  = "%llu",
        ["u64"]                 = "%llu",
        ["float"]               = "%f",
        ["double"]              = "%lf",
        ["bool"]                = "%d",
        ["const char*"]         = "%s",
    };

    // ── Property-Name Mappings (C# Property → C Feld-Name) ───────────────────

    private static readonly Dictionary<string, string> s_propertyNames = new(StringComparer.Ordinal)
    {
        // Control-Basisfelder
        ["X"]           = "base.x",
        ["Y"]           = "base.y",
        ["Width"]       = "base.width",
        ["Height"]      = "base.height",
        ["Visible"]     = "base.visible",

        // UI-Controls
        ["Text"]        = "text",
        ["Focused"]     = "focused",
        ["OnClick"]     = "OnClick",

        // ProgressBar
        ["Value"]       = "value",
        ["value"]       = "value",
        ["WidthChars"]  = "width_chars",
        ["width_chars"] = "width_chars",
    };

    // ── Control-Felder (direkt in SwitchApp-Subklassen zugreifbar) ───────────

    public static readonly HashSet<string> ControlFields = new(StringComparer.Ordinal)
    {
        "x", "y", "width", "height", "visible", "focusable",
    };

    // ── NoPrefix-Felder (kein f_ Prefix im generierten C-Code) ───────────────

    public static readonly HashSet<string> NoPrefixFields = new(StringComparer.Ordinal)
    {
        "x", "y", "width", "height", "visible", "focusable",
        "focused", "OnClick", "value", "width_chars", "text",
        "kDown", "kHeld", "Form",
    };

    // ── Enum-Mappings (C# Enum-Member → C Konstante) ─────────────────────────

    private static readonly Dictionary<string, string> s_enums = new(StringComparer.Ordinal)
    {
        // Face Buttons
        ["NpadButton.A"]            = "HidNpadButton_A",
        ["NpadButton.B"]            = "HidNpadButton_B",
        ["NpadButton.X"]            = "HidNpadButton_X",
        ["NpadButton.Y"]            = "HidNpadButton_Y",

        // Schultertasten
        ["NpadButton.L"]            = "HidNpadButton_L",
        ["NpadButton.R"]            = "HidNpadButton_R",
        ["NpadButton.ZL"]           = "HidNpadButton_ZL",
        ["NpadButton.ZR"]           = "HidNpadButton_ZR",

        // System
        ["NpadButton.Plus"]         = "HidNpadButton_Plus",
        ["NpadButton.Minus"]        = "HidNpadButton_Minus",

        // D-Pad
        ["NpadButton.Up"]           = "HidNpadButton_Up",
        ["NpadButton.Down"]         = "HidNpadButton_Down",
        ["NpadButton.Left"]         = "HidNpadButton_Left",
        ["NpadButton.Right"]        = "HidNpadButton_Right",

        // Sticks
        ["NpadButton.StickL"]       = "HidNpadButton_StickL",
        ["NpadButton.StickR"]       = "HidNpadButton_StickR",
        ["NpadButton.StickLUp"]     = "HidNpadButton_StickLUp",
        ["NpadButton.StickLDown"]   = "HidNpadButton_StickLDown",
        ["NpadButton.StickLLeft"]   = "HidNpadButton_StickLLeft",
        ["NpadButton.StickLRight"]  = "HidNpadButton_StickLRight",
        ["NpadButton.StickRUp"]     = "HidNpadButton_StickRUp",
        ["NpadButton.StickRDown"]   = "HidNpadButton_StickRDown",
        ["NpadButton.StickRLeft"]   = "HidNpadButton_StickRLeft",
        ["NpadButton.StickRRight"]  = "HidNpadButton_StickRRight",

        // Literale
        ["true"]  = "1",
        ["false"] = "0",
        ["null"]  = "NULL",
    };

    // ── Öffentliche API ───────────────────────────────────────────────────────

    /// <summary>C#-Typ → C-Typ. Unbekannte Typen werden unverändert zurückgegeben.</summary>
    public static string MapType(string csType)
    {
        csType = csType.Trim();

        // Nullable<T> / T? → T
        if (csType.EndsWith('?'))
            csType = csType[..^1].Trim();

        // Array T[] → T* (einfacher Zeiger)
        if (csType.EndsWith("[]"))
            return MapType(csType[..^2]) + "*";

        // List<T> → List_T*
        if (csType.StartsWith("List<") && csType.EndsWith(">"))
        {
            var inner  = csType[5..^1].Trim();
            var cInner = inner == "string" ? "char" : MapType(inner);
            return "List_" + cInner + "*";
        }

        // Dictionary<K,V> → Dict_K_V*
        if (csType.StartsWith("Dictionary<") && csType.EndsWith(">"))
        {
            var inner = csType[11..^1].Trim();
            var comma = inner.IndexOf(',');
            var kType = inner[..comma].Trim();
            var vType = inner[(comma + 1)..].Trim();
            var cKey = kType == "string" ? "str" : MapType(kType);
            var cVal = vType == "string" ? "str" : MapType(vType);
            return "Dict_" + cKey + "_" + cVal + "*";
        }

        return s_primitives.TryGetValue(csType, out var c) ? c : csType;
    }

    /// <summary>Enum-Member oder Literal → C-Äquivalent.</summary>
    public static string MapEnum(string csEnum) =>
        s_enums.TryGetValue(csEnum, out var c) ? c : csEnum;

    /// <summary>C# Property-Name → C Feld-Name.</summary>
    public static string MapProperty(string prop) =>
        s_propertyNames.TryGetValue(prop, out var c) ? c : "f_" + prop;

    /// <summary>printf Format-Specifier für einen C-Typ.</summary>
    public static string FormatSpecifier(string cType) =>
        s_formatSpecifiers.TryGetValue(cType, out var s) ? s : "%s";

    /// <summary>True wenn der Typ primitiv ist (direkt als Wert, kein Pointer).</summary>
    public static bool IsPrimitive(string csType) => s_primitives.ContainsKey(csType);

    /// <summary>True wenn der Typ ein libnx Stack-Struct ist.</summary>
    public static bool IsLibNxStruct(string csType) => s_libNxStructs.Contains(csType);

    /// <summary>True wenn der Typ List&lt;T&gt; ist.</summary>
    public static bool IsList(string csType) =>
        csType.Trim().StartsWith("List<") && csType.Trim().EndsWith(">");

    public static bool IsDictionary(string csType) =>
    csType.Trim().StartsWith("Dictionary<") && csType.Trim().EndsWith(">");

    public static (string key, string val)? GetDictionaryTypes(string csType)
    {
        if (!IsDictionary(csType)) return null;
        var inner = csType.Trim()[11..^1].Trim();
        var comma = inner.IndexOf(',');
        if (comma < 0) return null;
        return (inner[..comma].Trim(), inner[(comma + 1)..].Trim());
    }

    /// <summary>True wenn der Typ StringBuilder ist.</summary>
    public static bool IsStringBuilder(string csType) => csType.Trim() == "StringBuilder";

    /// <summary>True wenn der Typ bereits ein Pointer-Typ ist (kein extra * nötig).</summary>
    public static bool IsNativePointerType(string csType) =>
        s_nativePointerTypes.Contains(csType) || IsList(csType);

    /// <summary>Inneren Typ aus List&lt;T&gt; extrahieren.</summary>
    public static string? GetListInnerType(string csType)
    {
        csType = csType.Trim();
        if (csType.StartsWith("List<") && csType.EndsWith(">"))
            return csType[5..^1].Trim();
        return null;
    }

    /// <summary>
    /// True wenn das Feld kein f_ Prefix bekommt.
    /// </summary>
    public static bool HasNoPrefix(string fieldName) =>
        NoPrefixFields.Contains(fieldName);

    /// <summary>
    /// Bestimmt ob ein Feld-Typ als Stack-Variable (kein Pointer) deklariert wird.
    /// </summary>
    public static bool IsValueType(string csType) =>
        IsPrimitive(csType) || IsLibNxStruct(csType);

    /// <summary>
    /// Bestimmt ob für ein Feld kein extra * Suffix nötig ist.
    /// (Weil der Typ bereits ein Pointer ist oder primitiv)
    /// </summary>
    public static bool NeedsPointerSuffix(string csType) =>
        !IsPrimitive(csType)
        && !IsLibNxStruct(csType)
        && !IsNativePointerType(csType)
        && !IsDictionary(csType)      // ← neu
        && csType != "string"
        && !csType.EndsWith("[]");
}
