namespace CS2SX.Transpiler.Writers;

/// <summary>
/// How a value appears in a string when C# formats it without an explicit format
/// (concatenation, interpolation, Console.WriteLine, string.Format "{0}"):
/// bool → "True"/"False", double/float → shortest round-trip text ("2.5", not
/// "2.500000"). printf has no specifier for either, so the value goes through %s.
/// Every place that builds a printf format from C# values calls this.
/// </summary>
internal static class FormatArg
{
    /// A number with a .NET format ("F2", "N0", "X4", "0.00", …) as text: ("%s", call),
    /// or null for other types (DateTime has its own formats).
    public static (string Spec, string Code)? WithFormat(string csType, string format, string code)
    {
        var f = "\"" + format.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
        return csType.Trim() switch
        {
            "double" or "float" or "decimal" => ("%s", $"CS2SX_FmtD((double)({code}), {f})"),
            "DateTime" => ("%s", $"CS2SX_DT_ToString({code}, {f})"),
            "int" => ("%s", $"CS2SX_FmtI((long long)({code}), 32, {f})"),
            "long" => ("%s", $"CS2SX_FmtI((long long)({code}), 64, {f})"),
            "short" => ("%s", $"CS2SX_FmtI((long long)({code}), 16, {f})"),
            "sbyte" => ("%s", $"CS2SX_FmtI((long long)({code}), 8, {f})"),
            "uint" => ("%s", $"CS2SX_FmtU((unsigned long long)({code}), 32, {f})"),
            "ulong" => ("%s", $"CS2SX_FmtU((unsigned long long)({code}), 64, {f})"),
            "ushort" => ("%s", $"CS2SX_FmtU((unsigned long long)({code}), 16, {f})"),
            "byte" => ("%s", $"CS2SX_FmtU((unsigned long long)({code}), 8, {f})"),
            _ => null,
        };
    }

    public static (string Spec, string Code) Adapt(string csType, string spec, string code) =>
        csType switch
        {
            "bool" => ("%s", "CS2SX_BoolStr(" + code + ")"),
            "double" => ("%s", "Double_ToString(" + code + ")"),
            "float" => ("%s", "Float_ToString(" + code + ")"),
            // An enum shows its member name ("Gold", "Read, Write" for [Flags]).
            "object" or "object?" => ("%s", "CS2SX_Obj_ToString(" + code + ")"),
            "char" => ("%s", "CS2SX_CharStr(" + code + ")"),
            "StringBuilder" or "StringBuilder?" or "System.Text.StringBuilder" => ("%s", "StringBuilder_ToString(" + code + ")"),
            "DateTime" => ("%s", "CS2SX_DT_ToString(" + code + ", NULL)"),
            "TimeSpan" => ("%s", "CS2SX_TS_ToString(" + code + ")"),
            "DayOfWeek" => ("%s", "CS2SX_DayOfWeekName(" + code + ")"),
            _ when CS2SX.Core.TypeRegistry.IsRecordClass(csType) => ("%s", csType.Trim().TrimEnd('?') + "_ToString(" + code + ")"),
            _ when CS2SX.Core.TypeRegistry.IsUserDefinedEnum(csType) => ("%s", csType + "_ToString(" + code + ")"),
            _ when ObjectString(csType, code) is { } s => ("%s", s),
            _ when NullableString(csType, code) is { } n => ("%s", n),
            _ => (spec, code),
        };

    /// int? / double? / enum? in a string: the value as it would show, null → "".
    private static string? NullableString(string csType, string code)
    {
        var t = csType.Trim();
        if (!t.EndsWith('?') || !CS2SX.Core.TypeRegistry.IsNullableValueType(t)) return null;
        var inner = t[..^1];
        var (s, c) = Adapt(inner, CS2SX.Core.TypeRegistry.FormatSpecifier(CS2SX.Core.TypeRegistry.MapType(inner)), "_nv.value");
        return "({ __auto_type _nv = (" + code + "); _nv.has ? (const char*)CS2SX_Format(_cs2sx_next_buf(), \"" + s + "\", " + c + ") : \"\"; })";
    }

    /// A user class or struct as a string, as .NET shows it: its (virtual) ToString()
    /// override, else the type name. Objects: null → "". Null for other types.
    public static string? ObjectString(string csType, string code)
    {
        var t = csType.Trim().TrimEnd('?');
        if (CS2SX.Core.TypeRegistry.IsUserStruct(t))
            return CS2SX.Core.TypeRegistry.StructHasToString(t)
                ? "({ __auto_type _ts = (" + code + "); " + t + "_ToString(&_ts); })"
                : "\"" + t + "\"";
        if (CS2SX.Core.TypeRegistry.ArcRoot(t) != null)
            return "CS2SX_Obj_ToString((void*)(" + code + "))";
        // An interface value: the ToString() of the object it holds.
        if (CS2SX.Core.TypeRegistry.IsRegisteredInterface(t))
            return "({ " + t + "* _isv = (" + code + "); _isv ? CS2SX_Obj_ToString(_isv->obj) : \"\"; })";
        return null;
    }
}
