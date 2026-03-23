namespace CS2SX.Transpiler;

/// <summary>
/// Bildet C#-Typen, Methoden und Enums auf ihre C-Äquivalente ab.
/// Alle Lookup-Tabellen sind statisch und readonly — keine Allokation zur Laufzeit.
/// </summary>
public static class TypeMapper
{
    // ── Primitive Typen ────────────────────────────────────────────────────────
    private static readonly Dictionary<string, string> s_typeMap = new(StringComparer.Ordinal)
    {
        ["int"] = "int",
        ["uint"] = "unsigned int",
        ["long"] = "long long",
        ["ulong"] = "unsigned long long",
        ["short"] = "short",
        ["ushort"] = "unsigned short",
        ["byte"] = "unsigned char",
        ["sbyte"] = "signed char",
        ["float"] = "float",
        ["double"] = "double",
        ["bool"] = "bool",
        ["char"] = "char",
        ["void"] = "void",
        ["string"] = "const char*",
        ["object"] = "void*",
        // libnx Typen
        ["u8"] = "u8",
        ["u16"] = "u16",
        ["u32"] = "u32",
        ["u64"] = "u64",
        ["s8"] = "s8",
        ["s16"] = "s16",
        ["s32"] = "s32",
        ["s64"] = "s64",
        ["Result"] = "Result",
        ["Handle"] = "Handle",
        // StringBuilder
        ["StringBuilder"] = "StringBuilder",
        // libnx FS-Structs
        ["FsDir"] = "FsDir",
        ["FsFile"] = "FsFile",
        ["FsFileSystem"] = "FsFileSystem",
        ["FsDirectoryEntry"] = "FsDirectoryEntry",
        // libnx sonstige Structs
        ["PadState"] = "PadState",
        ["HidTouchScreenState"] = "HidTouchScreenState",
        ["AccountUid"] = "AccountUid",
        ["PsmChargerType"] = "PsmChargerType",
    };

    // ── Methoden-Mappings ──────────────────────────────────────────────────────
    private static readonly Dictionary<string, string> s_methodMap = new(StringComparer.Ordinal)
    {
        ["Console.WriteLine"] = "printf",
        ["Console.Write"] = "printf",
        ["Math.Abs"] = "abs",
        ["Math.Min"] = "MIN",
        ["Math.Max"] = "MAX",
        ["Math.Sqrt"] = "sqrtf",
        ["Math.Floor"] = "floorf",
        ["Math.Ceil"] = "ceilf",
        ["String.Format"] = "snprintf",  // muss gesondert behandelt werden
    };

    // ── Enum-Mappings ──────────────────────────────────────────────────────────
    private static readonly Dictionary<string, string> s_enumMap = new(StringComparer.Ordinal)
    {
        // Face Buttons
        ["NpadButton.A"] = "HidNpadButton_A",
        ["NpadButton.B"] = "HidNpadButton_B",
        ["NpadButton.X"] = "HidNpadButton_X",
        ["NpadButton.Y"] = "HidNpadButton_Y",
        // Schultertasten
        ["NpadButton.L"] = "HidNpadButton_L",
        ["NpadButton.R"] = "HidNpadButton_R",
        ["NpadButton.ZL"] = "HidNpadButton_ZL",
        ["NpadButton.ZR"] = "HidNpadButton_ZR",
        // System
        ["NpadButton.Plus"] = "HidNpadButton_Plus",
        ["NpadButton.Minus"] = "HidNpadButton_Minus",
        // D-Pad
        ["NpadButton.Up"] = "HidNpadButton_Up",
        ["NpadButton.Down"] = "HidNpadButton_Down",
        ["NpadButton.Left"] = "HidNpadButton_Left",
        ["NpadButton.Right"] = "HidNpadButton_Right",
        // Sticks (Button)
        ["NpadButton.StickL"] = "HidNpadButton_StickL",
        ["NpadButton.StickR"] = "HidNpadButton_StickR",
        // Stick-Richtungen
        ["NpadButton.StickLUp"] = "HidNpadButton_StickLUp",
        ["NpadButton.StickLDown"] = "HidNpadButton_StickLDown",
        ["NpadButton.StickLLeft"] = "HidNpadButton_StickLLeft",
        ["NpadButton.StickLRight"] = "HidNpadButton_StickLRight",
        ["NpadButton.StickRUp"] = "HidNpadButton_StickRUp",
        ["NpadButton.StickRDown"] = "HidNpadButton_StickRDown",
        ["NpadButton.StickRLeft"] = "HidNpadButton_StickRLeft",
        ["NpadButton.StickRRight"] = "HidNpadButton_StickRRight",
        // Boolesche Literale → C
        ["true"] = "1",
        ["false"] = "0",
        ["null"] = "NULL",
    };

    // ── Öffentliche API ────────────────────────────────────────────────────────

    // libnx-Structs die als Stack-Variable (kein Pointer) deklariert werden
    private static readonly HashSet<string> s_structTypes = new(StringComparer.Ordinal)
    {
        "FsDir", "FsFile", "FsFileSystem", "FsDirectoryEntry",
        "PadState", "HidTouchScreenState", "AccountUid", "PsmChargerType",
    };

    /// <summary>Gibt true zurück, wenn der Typ direkt als Wert (kein Pointer) behandelt wird.</summary>
    public static bool IsPrimitive(string csType) => s_typeMap.ContainsKey(csType);

    /// <summary>Gibt true zurück wenn der Typ ein libnx-Struct ist (Stack-Variable, kein Pointer).</summary>
    public static bool IsLibNxStruct(string csType) => s_structTypes.Contains(csType);

    /// <summary>Bildet einen C#-Typnamen auf den C-Typnamen ab. Unbekannte Typen werden unverändert zurückgegeben.</summary>
    public static string Map(string csType)
    {
        csType = csType.Trim();

        // Nullable<T> / T? → T
        if (csType.EndsWith('?'))
            csType = csType[..^1].Trim();

        // Array T[] → T* (einfacher Zeiger)
        if (csType.EndsWith("[]"))
            return Map(csType[..^2]) + "*";

        // List<T> → List_T* (cs2sx generische Liste)
        if (csType.StartsWith("List<") && csType.EndsWith(">"))
        {
            var inner = csType[5..^1].Trim();
            var cInner = inner == "string" ? "char" : Map(inner);
            return "List_" + cInner + "*";
        }

        return s_typeMap.TryGetValue(csType, out var c) ? c : csType;
    }

    /// <summary>Extrahiert den inneren Typ aus List&lt;T&gt;, z.B. "List&lt;int&gt;" → "int".</summary>
    public static string? GetListInnerType(string csType)
    {
        csType = csType.Trim();
        if (csType.StartsWith("List<") && csType.EndsWith(">"))
            return csType[5..^1].Trim();
        return null;
    }

    /// <summary>Gibt true zurück wenn der Typ ein List&lt;T&gt; ist.</summary>
    public static bool IsList(string csType) =>
        csType.Trim().StartsWith("List<") && csType.Trim().EndsWith(">");

    /// <summary>Gibt true zurück wenn der Typ StringBuilder ist.</summary>
    public static bool IsStringBuilder(string csType) =>
        csType.Trim() == "StringBuilder";

    /// <summary>Bildet einen C#-Methodennamen auf den C-Funktionsnamen ab.</summary>
    public static string MapMethod(string csMethod) =>
        s_methodMap.TryGetValue(csMethod, out var c) ? c : csMethod;

    /// <summary>
    /// Bildet C#-Enum-Member und bekannte Literale ab (true/false/null → 1/0/NULL).
    /// </summary>
    public static string MapEnum(string csEnum) =>
        s_enumMap.TryGetValue(csEnum, out var c) ? c : csEnum;

    /// <summary>Gibt den printf-Formatspezifizierer für einen gemappten C-Typ zurück.</summary>
    public static string FormatSpecifier(string cType) => cType switch
    {
        "int" or "short" or "signed char" or "s8" or "s16" or "s32" => "%d",
        "unsigned int" or "unsigned short" or "unsigned char"
            or "u8" or "u16" or "u32" => "%u",
        "long long" or "s64" => "%lld",
        "unsigned long long" or "u64" => "%llu",
        "float" => "%f",
        "double" => "%lf",
        "bool" => "%d",
        "const char*" => "%s",
        _ => "%s",
    };
}