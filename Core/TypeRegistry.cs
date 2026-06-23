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
        ["TimeInfo"] = "CS2SX_TimeInfo",
        ["MotionState"] = "CS2SX_MotionState",
        ["Stopwatch"] = "CS2SX_Stopwatch",
        ["TimeSpan"] = "CS2SX_TimeSpan",
        ["Regex"] = "CS2SX_Regex",
        ["Random"] = "void*",  // opaque handle; methods dispatched by RandomHandler to CS2SX_Rand_* globals
        ["IntPtr"]  = "intptr_t",
        ["UIntPtr"] = "uintptr_t",
        ["nint"]    = "intptr_t",
        ["nuint"]   = "uintptr_t",
        // libnx enum types used as parameter/field types (not as access targets)
        ["NpadButton"] = "HidNpadButton",
        ["HidNpadButton"] = "HidNpadButton",
        ["HidTouchState"] = "HidTouchState",
        ["PadState"] = "PadState",
        ["ValueTuple"] = "void*",  // fallback for unresolved ValueTuple (without type args)
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
        "CS2SX_StickPos", "CS2SX_TouchState", "CS2SX_BatteryInfo", "CS2SX_TimeInfo", "CS2SX_MotionState",
        // libnx enum types (value types — no pointer suffix)
        "NpadButton", "HidNpadButton", "HidTouchState",
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
        ["s64"] = "%ld",
        ["unsigned long long"] = "%llu",
        ["u64"] = "%lu",
        ["float"] = "%f",
        ["double"] = "%lf",
        ["bool"] = "%d",
        ["char"] = "%c",
        ["const char*"] = "%s",
        ["intptr_t"]  = "%zd",
        ["uintptr_t"] = "%zu",
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

    public static bool IsBuiltinControlType(string typeName) =>
        typeName is "Control" or "Label" or "Button" or "ProgressBar";

    // ── NoPrefix-Felder ───────────────────────────────────────────────────────

    public static readonly HashSet<string> NoPrefixFields = new(StringComparer.Ordinal)
    {
        "x", "y", "width", "height", "visible", "focusable",
        "focused", "OnClick", "value", "width_chars", "text",
        "kDown", "kHeld", "Form",
    };

    // ── User-defined enum registry ────────────────────────────────────────────
    private static readonly HashSet<string> s_userEnumTypes = new(StringComparer.Ordinal);
    public static void RegisterUserEnum(string name) => s_userEnumTypes.Add(name);
    public static bool IsUserDefinedEnum(string csType) => s_userEnumTypes.Contains(csType);
    public static void ClearUserEnums() => s_userEnumTypes.Clear();

    // ── Interface type registry ───────────────────────────────────────────────
    // Interfaces in C are value structs {vtable*, obj*} — NOT heap-allocated pointers.
    // NeedsPointerSuffix must return false for interface types so they're passed by value.
    private static readonly HashSet<string> s_interfaceTypes = new(StringComparer.Ordinal);
    public static void RegisterInterfaceType(string name) => s_interfaceTypes.Add(name);
    public static bool IsRegisteredInterface(string csType) => s_interfaceTypes.Contains(csType);

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

    // decimal types are silently mapped to double (precision loss).
    // Callers that can emit a warning should check this first.
    public static bool IsDecimalType(string csType)
    {
        var t = csType.Trim().TrimEnd('?');
        return t is "decimal" or "Decimal" or "System.Decimal";
    }

    public static string MapType(string csType)
    {
        csType = csType.Trim();

        // decimal → double (precision loss — warn at call sites)
        if (IsDecimalType(csType)) return "double";

        if (csType.EndsWith('?'))
            csType = csType[..^1].Trim();

        if (csType.EndsWith("[]"))
        {
            var elemType = csType[..^2].Trim();
            var mapped = MapType(elemType);
            // Reference types (classes): T[] → T** (array of pointers to heap objects)
            if (NeedsPointerSuffix(elemType))
                return mapped + "**";
            return mapped + "*";
        }

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
        {
            // Tuple → proper C struct name (_Tuple2_str_str etc.)
            var structName = GetTupleStructName(csType);
            if (!string.IsNullOrEmpty(structName)) return structName;
            return "void*";
        }

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

        // Task / Task<T> / ValueTask / ValueTask<T> → async is simulated synchronously.
        // Task<T> maps to T (the return value); plain Task maps to void.
        if (csType == "Task" || csType == "ValueTask")
            return "void";
        if (csType.StartsWith("Task<") && csType.EndsWith(">"))
            return MapType(csType[5..^1].Trim());
        if (csType.StartsWith("ValueTask<") && csType.EndsWith(">"))
            return MapType(csType[10..^1].Trim());

        if (csType.StartsWith("Stack<") && csType.EndsWith(">"))
        {
            var inner = csType[6..^1].Trim();
            // Interface types use pointer-based stack (Stack_IFace_ptr) since interfaces are IFace*
            if (s_interfaceTypes.Contains(inner))
                return $"Stack_{inner}_ptr*";
            var cInner = inner == "string" ? "str" : MapListInnerType(inner);
            return $"Stack_{cInner}*";
        }

        if (csType.StartsWith("Queue<") && csType.EndsWith(">"))
        {
            var inner = csType[6..^1].Trim();
            var cInner = inner == "string" ? "str" : MapListInnerType(inner);
            return $"Queue_{cInner}*";
        }

        if (csType.StartsWith("HashSet<") && csType.EndsWith(">"))
        {
            var inner = csType[8..^1].Trim();
            var cInner = inner == "string" ? "str" : MapListInnerType(inner);
            return $"HashSet_{cInner}*";
        }

        // Delegate types → C function pointer typedef names
        if (csType == "Action") return "Action_t";
        if (csType.StartsWith("Action<") && csType.EndsWith(">"))
        {
            var inner = csType[7..^1].Trim();
            var args = SplitGenericArgs(inner);
            var suffix = string.Join("_", args.Select(a => MapListInnerType(a.Trim())));
            return $"Action_{suffix}_t";
        }
        if (csType.StartsWith("Func<") && csType.EndsWith(">"))
        {
            var inner = csType[5..^1].Trim();
            var args = SplitGenericArgs(inner);
            var suffix = string.Join("_", args.Select(a => MapListInnerType(a.Trim())));
            return $"Func_{suffix}_t";
        }
        if (csType == "EventHandler" || csType.StartsWith("EventHandler<"))
            return "Action_t";

        return s_primitives.TryGetValue(csType, out var c) ? c : csType;
    }

    private static List<string> SplitGenericArgs(string s)
    {
        var result = new List<string>();
        var cur = new System.Text.StringBuilder();
        int depth = 0;
        foreach (char c in s)
        {
            if (c == '<' || c == '(') { depth++; cur.Append(c); }
            else if (c == '>' || c == ')') { depth--; cur.Append(c); }
            else if (c == ',' && depth == 0) { result.Add(cur.ToString()); cur.Clear(); }
            else cur.Append(c);
        }
        if (cur.Length > 0) result.Add(cur.ToString());
        return result;
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
    internal static int FindTopLevelComma(string s)
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

    public static string FormatSpecifier(string cType)
    {
        if (s_formatSpecifiers.TryGetValue(cType, out var s)) return s;
        // String-like C types keep %s; everything else (enums, unknown scalars)
        // defaults to %d — safer than %s, which would dereference a non-pointer.
        if (cType != null && cType.Contains("char*")) return "%s";
        return "%d";
    }

    public static bool IsPrimitive(string csType) => s_primitives.ContainsKey(csType);
    public static bool IsLibNxStruct(string csType) => s_libNxStructs.Contains(csType);

    // List-like generic types whose single type argument is the element type.
    // Includes the enumerable interfaces LINQ operators return (e.g. OrderBy →
    // IOrderedEnumerable) so chained LINQ resolves correctly.
    private static readonly string[] s_listPrefixes =
    {
        "List<", "IEnumerable<", "IReadOnlyList<", "IList<",
        "ICollection<", "IReadOnlyCollection<", "IOrderedEnumerable<",
    };

    public static bool IsList(string csType)
    {
        csType = csType.Trim();
        if (!csType.EndsWith(">")) return false;
        foreach (var p in s_listPrefixes)
            if (csType.StartsWith(p, StringComparison.Ordinal)) return true;
        return false;
    }

    public static bool IsDictionary(string csType) =>
        csType.Trim().StartsWith("Dictionary<") && csType.Trim().EndsWith(">");

    public static (string key, string val)? GetDictionaryTypes(string csType)
    {
        if (!IsDictionary(csType)) return null;
        var inner = csType.Trim()[11..^1].Trim();
        var comma = FindTopLevelComma(inner);
        if (comma < 0) return null;
        return (inner[..comma].Trim(), inner[(comma + 1)..].Trim());
    }

    public static bool IsStringBuilder(string csType) => csType.Trim() == "StringBuilder";

    public static bool IsStack(string csType)
    {
        csType = csType.Trim();
        return csType.StartsWith("Stack<") && csType.EndsWith(">");
    }

    public static bool IsQueue(string csType)
    {
        csType = csType.Trim();
        return csType.StartsWith("Queue<") && csType.EndsWith(">");
    }

    public static bool IsHashSet(string csType)
    {
        csType = csType.Trim();
        return csType.StartsWith("HashSet<") && csType.EndsWith(">");
    }

    public static string? GetStackInnerType(string csType)
    {
        csType = csType.Trim();
        return csType.StartsWith("Stack<") && csType.EndsWith(">") ? csType[6..^1].Trim() : null;
    }

    public static string? GetQueueInnerType(string csType)
    {
        csType = csType.Trim();
        return csType.StartsWith("Queue<") && csType.EndsWith(">") ? csType[6..^1].Trim() : null;
    }

    public static string? GetHashSetInnerType(string csType)
    {
        csType = csType.Trim();
        return csType.StartsWith("HashSet<") && csType.EndsWith(">") ? csType[8..^1].Trim() : null;
    }

    public static bool IsNativePointerType(string csType) =>
        s_nativePointerTypes.Contains(csType) || IsList(csType) || IsStack(csType)
        || IsQueue(csType) || IsHashSet(csType);

    public static string? GetListInnerType(string csType)
    {
        csType = csType.Trim();
        if (!csType.EndsWith(">")) return null;
        foreach (var p in s_listPrefixes)
            if (csType.StartsWith(p, StringComparison.Ordinal))
                return csType[p.Length..^1].Trim();
        return null;
    }

    public static bool HasNoPrefix(string fieldName) =>
        NoPrefixFields.Contains(fieldName);

    public static bool IsValueType(string csType) =>
        IsPrimitive(csType) || IsLibNxStruct(csType);

    public static bool IsDelegate(string csType)
    {
        csType = csType.Trim();
        return csType == "Action" || csType == "EventHandler"
            || csType.StartsWith("Action<") || csType.StartsWith("Func<")
            || csType.StartsWith("EventHandler<");
    }

    public static bool NeedsPointerSuffix(string csType) =>
        !IsPrimitive(csType)
        && !IsLibNxStruct(csType)
        && !IsNativePointerType(csType)
        && !IsDictionary(csType)
        && !IsStack(csType)
        && !IsQueue(csType)
        && !IsHashSet(csType)
        && !IsDelegate(csType)
        && !IsUserDefinedEnum(csType)
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

    /// <summary>
    /// Returns the struct definition for a tuple struct by name (e.g. _Tuple2_str_str).
    /// Used by GenericExpander to emit struct typedef before the list macro.
    /// </summary>
    public static string GetTupleStructDefinition(string structName)
    {
        // Parse _TupleN_t1_t2_... back to a fake CS tuple type and generate
        if (!structName.StartsWith("_Tuple")) return "";
        try
        {
            // e.g. _Tuple2_str_str → 2 elements: str, str
            var body = structName["_Tuple".Length..];
            var countEnd = body.IndexOf('_');
            if (countEnd < 0) return "";
            if (!int.TryParse(body[..countEnd], out var count)) return "";
            var typesPart = body[(countEnd + 1)..];
            // Reconstruct fields: str→const char*, int→int etc.
            var fields = new[] { "item1", "item2", "item3", "item4" };
            // Split by _ but rejoin compound names (e.g. const_char might appear)
            // Simple: split assuming single-word type tokens
            var parts = typesPart.Split('_');
            if (parts.Length < count) return "";
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"typedef struct {structName} {{");
            for (int i = 0; i < count && i < parts.Length && i < fields.Length; i++)
            {
                var cType = parts[i] == "str" ? "const char*" : parts[i];
                sb.AppendLine($"    {cType} {fields[i]};");
            }
            sb.AppendLine($"}} {structName};");
            return sb.ToString();
        }
        catch { return ""; }
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