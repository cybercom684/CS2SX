using CS2SX.Core;

namespace CS2SX.Transpiler;

/// <summary>
/// Backward-kompatibler Shim über TypeRegistry.
/// Bestehender Code der TypeMapper verwendet läuft weiterhin.
/// Neue Features direkt in TypeRegistry ergänzen.
/// </summary>
public static class TypeMapper
{
    public static readonly HashSet<string> ControlFields  = TypeRegistry.ControlFields;
    public static readonly HashSet<string> NoPrefix       = TypeRegistry.NoPrefixFields;

    public static string  Map(string csType)              => TypeRegistry.MapType(csType);
    public static string  MapMethod(string csMethod)      => csMethod; // direkt weitergeleitet
    public static string  MapEnum(string csEnum)          => TypeRegistry.MapEnum(csEnum);
    public static string  MapProperty(string prop)        => TypeRegistry.MapProperty(prop);
    public static string  FormatSpecifier(string cType)   => TypeRegistry.FormatSpecifier(cType);
    public static bool    IsPrimitive(string csType)      => TypeRegistry.IsPrimitive(csType);
    public static bool    IsLibNxStruct(string csType)    => TypeRegistry.IsLibNxStruct(csType);
    public static bool    IsList(string csType)           => TypeRegistry.IsList(csType);
    public static bool    IsStringBuilder(string csType)  => TypeRegistry.IsStringBuilder(csType);
    public static bool    NeedsPointerSuffix(string t)    => TypeRegistry.NeedsPointerSuffix(t);
    public static string? GetListInnerType(string csType) => TypeRegistry.GetListInnerType(csType);
    public static bool    HasNoPrefix(string fieldName)   => TypeRegistry.HasNoPrefix(fieldName);
}
