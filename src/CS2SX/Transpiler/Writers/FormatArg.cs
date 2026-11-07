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
    public static (string Spec, string Code) Adapt(string csType, string spec, string code) =>
        csType switch
        {
            "bool" => ("%s", "CS2SX_BoolStr(" + code + ")"),
            "double" => ("%s", "Double_ToString(" + code + ")"),
            "float" => ("%s", "Float_ToString(" + code + ")"),
            _ => (spec, code),
        };
}
