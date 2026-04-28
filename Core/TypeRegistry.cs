namespace CS2SX.Core;

/// <summary>
/// Zentrale Typ-Registry — einzige Quelle der Wahrheit für alle C#→C Typ-Mappings.
/// PHASE 2: Tuple-Support, params-Support, erweiterte Dictionary/List Typen.
/// </summary>
public static class TypeRegistry
{
    // ── Primitive C#→C Typ-Mappings ──────────────────────────────────────────

    private static readonly Dictionary<string, string> s_primitives = new(StringComparer.Ordinal)
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
        ["bool"] = "int",
        ["char"] = "char",
        ["void"] = "void",
        ["string"] = "const char*",
        ["object"] = "void*",
        ["Action"] = "Action",
        ["StringBuilder"] = "StringBuilder",
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
        ["FsDir"] = "FsDir",
        ["FsFile"] = "FsFile",
        ["FsFileSystem"] = "FsFileSystem",
        ["FsDirectoryEntry"] = "FsDirectoryEntry",
        ["PadState"] = "PadState",
        ["HidTouchScreenState"] = "HidTouchScreenState",
        ["AccountUid"] = "AccountUid",
        ["PsmChargerType"] = "PsmChargerType",
        ["StickPos"] = "CS2SX_StickPos",
        ["TouchState"] = "CS2SX_TouchState",
        ["BatteryInfo"] = "CS2SX_BatteryInfo",
        ["Random"] = "void",
        // PHASE 2: Tuple → void* (Tupels werden als temporäre Structs behandelt)
        ["(int, int)"] = "void*",
        ["(string, string)"] = "void*",
        ["(int, string)"] = "void*",
        ["(float, float)"] = "void*",
    };

    // ── SwitchForms Control-Typen ─────────────────────────────────────────────

    public static bool IsControlType(string csType) =>
        s_controlTypes.Contains(csType.Trim());

    private static readonly HashSet<string> s_controlTypes = new(StringComparer.Ordinal)
    {
        "Control", "Label", "Button", "ProgressBar", "Form", "SwitchApp",
    };

    // ── libnx Stack-Structs ───────────────────────────────────────────────────

    private static readonly HashSet<string> s_libNxStructs = new(StringComparer.Ordinal)
    {
        "FsDir", "FsFile", "FsFileSystem", "FsDirectoryEntry",
        "PadState", "HidTouchScreenState", "AccountUid", "PsmChargerType",
        "CS2SX_StickPos", "CS2SX_TouchState", "CS2SX_BatteryInfo",
    };

    // ── Pointer-Typen ─────────────────────────────────────────────────────────

    private static readonly HashSet<string> s_nativePointerTypes = new(StringComparer.Ordinal)
    {
        "StringBuilder", "Action",
    };

    // ── printf Format-Specifier ───────────────────────────────────────────────

    private static readonly Dictionary<string, string> s_formatSpecifiers = new(StringComparer.Ordinal)
    {
        ["int"] = "%d",
        ["short"] = "%d",
        ["signed char"] = "%d",
        ["s8"] = "%d",
        ["s16"] = "%d",
        ["s32"] = "%d",
        ["unsigned int"] = "%u",
        ["unsigned short"] = "%u",
        ["unsigned char"] = "%u",
        ["u8"] = "%u",
        ["u16"] = "%u",
        ["u32"] = "%u",
        ["long long"] = "%lld",
        ["s64"] = "%lld",
        ["unsigned long long"] = "%llu",
        ["u64"] = "%llu",
        ["float"] = "%f",
        ["double"] = "%lf",
        ["bool"] = "%d",
        ["const char*"] = "%s",
    };

    // ── Property-Name Mappings ────────────────────────────────────────────────

    private static readonly Dictionary<string, string> s_propertyNames = new(StringComparer.Ordinal)
    {
        ["X"] = "base.x",
        ["Y"] = "base.y",
        ["Width"] = "base.width",
        ["Height"] = "base.height",
        ["Visible"] = "base.visible",
        ["Text"] = "text",
        ["Focused"] = "focused",
        ["OnClick"] = "OnClick",
        ["Value"] = "value",
        ["value"] = "value",
        ["WidthChars"] = "width_chars",
        ["width_chars"] = "width_chars",
    };

    // ── Control-Felder ────────────────────────────────────────────────────────

    public static readonly HashSet<string> ControlFields = new(StringComparer.Ordinal)
    {
        "x", "y", "width", "height", "visible", "focusable",
    };

    // ── NoPrefix-Felder ───────────────────────────────────────────────────────

    public static readonly HashSet<string> NoPrefixFields = new(StringComparer.Ordinal)
    {
        "x", "y", "width", "height", "visible", "focusable",
        "focused", "OnClick", "value", "width_chars", "text",
        "kDown", "kHeld", "Form",
    };

    // ── Enum-Mappings ─────────────────────────────────────────────────────────

    private static readonly Dictionary<string, string> s_enums = new(StringComparer.Ordinal)
    {
        ["NpadButton.A"] = "HidNpadButton_A",
        ["NpadButton.B"] = "HidNpadButton_B",
        ["NpadButton.X"] = "HidNpadButton_X",
        ["NpadButton.Y"] = "HidNpadButton_Y",
        ["NpadButton.L"] = "HidNpadButton_L",
        ["NpadButton.R"] = "HidNpadButton_R",
        ["NpadButton.ZL"] = "HidNpadButton_ZL",
        ["NpadButton.ZR"] = "HidNpadButton_ZR",
        ["NpadButton.Plus"] = "HidNpadButton_Plus",
        ["NpadButton.Minus"] = "HidNpadButton_Minus",
        ["NpadButton.Up"] = "HidNpadButton_Up",
        ["NpadButton.Down"] = "HidNpadButton_Down",
        ["NpadButton.Left"] = "HidNpadButton_Left",
        ["NpadButton.Right"] = "HidNpadButton_Right",
        ["NpadButton.StickL"] = "HidNpadButton_StickL",
        ["NpadButton.StickR"] = "HidNpadButton_StickR",
        ["NpadButton.StickLUp"] = "HidNpadButton_StickLUp",
        ["NpadButton.StickLDown"] = "HidNpadButton_StickLDown",
        ["NpadButton.StickLLeft"] = "HidNpadButton_StickLLeft",
        ["NpadButton.StickLRight"] = "HidNpadButton_StickLRight",
        ["NpadButton.StickRUp"] = "HidNpadButton_StickRUp",
        ["NpadButton.StickRDown"] = "HidNpadButton_StickRDown",
        ["NpadButton.StickRLeft"] = "HidNpadButton_StickRLeft",
        ["NpadButton.StickRRight"] = "HidNpadButton_StickRRight",
        ["true"] = "1",
        ["false"] = "0",
        ["null"] = "NULL",
        ["Color.Black"] = "COLOR_BLACK",
        ["Color.White"] = "COLOR_WHITE",
        ["Color.Red"] = "COLOR_RED",
        ["Color.Green"] = "COLOR_GREEN",
        ["Color.Blue"] = "COLOR_BLUE",
        ["Color.Yellow"] = "COLOR_YELLOW",
        ["Color.Cyan"] = "COLOR_CYAN",
        ["Color.Magenta"] = "COLOR_MAGENTA",
        ["Color.Gray"] = "COLOR_GRAY",
        ["Color.Orange"] = "COLOR_ORANGE",
        ["Color.Pink"] = "COLOR_PINK",
        ["Color.Purple"] = "COLOR_PURPLE",
        ["Color.Brown"] = "COLOR_BROWN",
        ["Color.Teal"] = "COLOR_TEAL",
        ["Color.Lime"] = "COLOR_LIME",
        ["Color.Navy"] = "COLOR_NAVY",
        ["Color.Silver"] = "COLOR_SILVER",
        ["Color.DarkGray"] = "COLOR_DGRAY",
        ["Color.LightGray"] = "COLOR_LGRAY",
        ["Color.Maroon"] = "COLOR_MAROON",
        ["Color.Olive"] = "COLOR_OLIVE",
    };

    private static readonly HashSet<string> s_disposableTypes = new(StringComparer.Ordinal)
    {
        "Texture",
    };

    public static bool IsDisposable(string csType) => s_disposableTypes.Contains(csType);

    // ── Öffentliche API ───────────────────────────────────────────────────────

    public static string MapType(string csType)
    {
        csType = csType.Trim();

        if (csType.EndsWith('?'))
            csType = csType[..^1].Trim();

        if (csType.EndsWith("[]"))
            return MapType(csType[..^2]) + "*";

        // FIX: Verschachtelte List<List<T>> → List_List_T_ptr*
        if (csType.StartsWith("List<") && csType.EndsWith(">"))
        {
            var inner = csType[5..^1].Trim();
            var cInner = MapListInnerType(inner);
            return $"List_{cInner}*";
        }

        // FIX: Verschachtelte Dictionary<K, List<V>> etc.
        if (csType.StartsWith("Dictionary<") && csType.EndsWith(">"))
        {
            var inner = csType[11..^1].Trim();
            var comma = FindTopLevelComma(inner);
            if (comma >= 0)
            {
                var kType = inner[..comma].Trim();
                var vType = inner[(comma + 1)..].Trim();
                var cKey = MapListInnerType(kType);
                var cVal = MapListInnerType(vType);
                return $"Dict_{cKey}_{cVal}*";
            }
        }

        if (csType.StartsWith("(") && csType.EndsWith(")") && csType.Contains(","))
            return "void*";

        if (csType.StartsWith("IEnumerable<") && csType.EndsWith(">"))
        {
            var inner = csType[12..^1].Trim();
            var cInner = MapListInnerType(inner);
            return $"List_{cInner}*";
        }

        if (csType.StartsWith("IReadOnlyList<") && csType.EndsWith(">"))
        {
            var inner = csType[14..^1].Trim();
            var cInner = MapListInnerType(inner);
            return $"List_{cInner}*";
        }

        return s_primitives.TryGetValue(csType, out var c) ? c : csType;
    }

    /// <summary>
    /// Mappt den inneren Typ einer generischen Collection auf einen C-Suffix.
    /// Behandelt verschachtelte Generics rekursiv.
    /// </summary>
    private static string MapListInnerType(string csType)
    {
        csType = csType.Trim();

        if (csType == "string") return "str";

        // Verschachtelter generischer Typ → rekursiv auflösen
        if (csType.StartsWith("List<") && csType.EndsWith(">"))
        {
            var inner = csType[5..^1].Trim();
            return "List_" + MapListInnerType(inner) + "_ptr";
        }

        if (csType.StartsWith("Dictionary<") && csType.EndsWith(">"))
        {
            var inner = csType[11..^1].Trim();
            var comma = FindTopLevelComma(inner);
            if (comma >= 0)
            {
                var k = MapListInnerType(inner[..comma].Trim());
                var v = MapListInnerType(inner[(comma + 1)..].Trim());
                return $"Dict_{k}_{v}_ptr";
            }
        }

        return MapType(csType).Replace(" ", "_").Replace("*", "ptr");
    }

    /// <summary>
    /// Findet das erste Komma auf der obersten Ebene (nicht in &lt;&gt; verschachtelt).
    /// </summary>
    private static int FindTopLevelComma(string s)
    {
        int depth = 0;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '<' || s[i] == '(') depth++;
            else if (s[i] == '>' || s[i] == ')') depth--;
            else if (s[i] == ',' && depth == 0) return i;
        }
        return -1;
    }

    public static string MapEnum(string csEnum) =>
        s_enums.TryGetValue(csEnum, out var c) ? c : csEnum;

    public static string MapProperty(string prop) =>
        s_propertyNames.TryGetValue(prop, out var c) ? c : "f_" + prop;

    public static string FormatSpecifier(string cType) =>
        s_formatSpecifiers.TryGetValue(cType, out var s) ? s : "%s";

    public static bool IsPrimitive(string csType) => s_primitives.ContainsKey(csType);
    public static bool IsLibNxStruct(string csType) => s_libNxStructs.Contains(csType);

    public static bool IsList(string csType)
    {
        csType = csType.Trim();
        return (csType.StartsWith("List<") && csType.EndsWith(">"))
            || (csType.StartsWith("IEnumerable<") && csType.EndsWith(">"))
            || (csType.StartsWith("IReadOnlyList<") && csType.EndsWith(">"));
    }

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

    public static bool IsStringBuilder(string csType) => csType.Trim() == "StringBuilder";

    public static bool IsNativePointerType(string csType) =>
        s_nativePointerTypes.Contains(csType) || IsList(csType);

    public static string? GetListInnerType(string csType)
    {
        csType = csType.Trim();
        if (csType.StartsWith("List<") && csType.EndsWith(">"))
            return csType[5..^1].Trim();
        if (csType.StartsWith("IEnumerable<") && csType.EndsWith(">"))
            return csType[12..^1].Trim();
        if (csType.StartsWith("IReadOnlyList<") && csType.EndsWith(">"))
            return csType[14..^1].Trim();
        return null;
    }

    public static bool HasNoPrefix(string fieldName) =>
        NoPrefixFields.Contains(fieldName);

    public static bool IsValueType(string csType) =>
        IsPrimitive(csType) || IsLibNxStruct(csType);

    public static bool NeedsPointerSuffix(string csType) =>
        !IsPrimitive(csType)
        && !IsLibNxStruct(csType)
        && !IsNativePointerType(csType)
        && !IsDictionary(csType)
        && csType != "string"
        && !csType.EndsWith("[]");

    // PHASE 2: Tuple-Erkennung
    public static bool IsTuple(string csType)
    {
        csType = csType.Trim();
        return csType.StartsWith("(") && csType.EndsWith(")") && csType.Contains(",");
    }

    /// <summary>
    /// Generiert den C-Struct-Namen für einen Tuple-Typ.
    /// (int, string) → _Tuple2_int_str
    /// </summary>
    public static string GetTupleStructName(string csType)
    {
        if (!IsTuple(csType)) return "void*";
        var inner = csType.Trim()[1..^1];
        var elements = SplitTupleArgs(inner);
        var suffix = string.Join("_", elements.Select(e =>
        {
            var clean = e.Trim();
            // Optionaler Name: "(int x, string y)" → nur Typ nehmen
            var spaceIdx = clean.LastIndexOf(' ');
            if (spaceIdx >= 0) clean = clean[..spaceIdx].Trim();
            return clean == "string" ? "str" : MapType(clean).Replace(" ", "_");
        }));
        return $"_Tuple{elements.Count}_{suffix}";
    }

    /// <summary>
    /// Generiert die C-Struct-Definition für einen Tuple-Typ.
    /// </summary>
    public static string GenerateTupleStruct(string csType)
    {
        if (!IsTuple(csType)) return "";
        var inner = csType.Trim()[1..^1];
        var elements = SplitTupleArgs(inner);
        var name = GetTupleStructName(csType);
        var fields = new[] { "item1", "item2", "item3", "item4", "item5", "item6", "item7" };

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"typedef struct {name} {{");
        for (int i = 0; i < elements.Count; i++)
        {
            var elem = elements[i].Trim();
            var spaceIdx = elem.LastIndexOf(' ');
            string cType;
            if (spaceIdx >= 0)
            {
                // Named tuple: "(int score, string name)" → int, const char*
                var typePart = elem[..spaceIdx].Trim();
                cType = typePart == "string" ? "const char*" : MapType(typePart);
            }
            else
            {
                cType = elem == "string" ? "const char*" : MapType(elem);
            }
            var needPtr = cType != "const char*" && NeedsPointerSuffix(elem);
            sb.AppendLine($"    {cType}{(needPtr ? "*" : "")} {fields[i]};");
        }
        sb.AppendLine($"}} {name};");
        return sb.ToString();
    }

    private static List<string> SplitTupleArgs(string s)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        int depth = 0;
        foreach (char c in s)
        {
            if (c == '(' || c == '<') { depth++; current.Append(c); }
            else if (c == ')' || c == '>') { depth--; current.Append(c); }
            else if (c == ',' && depth == 0) { result.Add(current.ToString()); current.Clear(); }
            else current.Append(c);
        }
        if (current.Length > 0) result.Add(current.ToString());
        return result;
    }

    // PHASE 2: params-Array-Erkennung
    public static bool IsParamsArray(string csType) =>
        csType.EndsWith("[]");
}