using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Behandelt int.Parse, int.TryParse, float.Parse, float.TryParse.
///
/// int.Parse(s)              → CS2SX_Int_Parse(s)
/// int.TryParse(s, out val)  → CS2SX_Int_TryParse(s, &val)
/// float.Parse(s)            → CS2SX_Float_Parse(s)
/// float.TryParse(s, out val)→ CS2SX_Float_TryParse(s, &val)
/// </summary>
public sealed class ParseHandler : InvocationHandlerBase
{
    private static readonly Dictionary<string, string> s_map =
        new(StringComparer.Ordinal)
        {
            ["int.Parse"]     = "CS2SX_Int_Parse",
            ["Int32.Parse"]   = "CS2SX_Int_Parse",
            ["int.TryParse"]  = "CS2SX_Int_TryParse",
            ["Int32.TryParse"]= "CS2SX_Int_TryParse",

            ["float.Parse"]    = "CS2SX_Float_Parse",
            ["Single.Parse"]   = "CS2SX_Float_Parse",
            ["float.TryParse"] = "CS2SX_Float_TryParse",
            ["Single.TryParse"]= "CS2SX_Float_TryParse",

            ["double.Parse"]    = "CS2SX_Double_Parse",
            ["Double.Parse"]    = "CS2SX_Double_Parse",
            ["double.TryParse"] = "CS2SX_Double_TryParse",
            ["Double.TryParse"] = "CS2SX_Double_TryParse",

            ["long.Parse"]     = "CS2SX_Long_Parse",
            ["Int64.Parse"]    = "CS2SX_Long_Parse",
            ["long.TryParse"]  = "CS2SX_Long_TryParse",
            ["Int64.TryParse"] = "CS2SX_Long_TryParse",

            ["ulong.Parse"]     = "CS2SX_ULong_Parse",
            ["UInt64.Parse"]    = "CS2SX_ULong_Parse",
            ["ulong.TryParse"]  = "CS2SX_ULong_TryParse",
            ["UInt64.TryParse"] = "CS2SX_ULong_TryParse",

            ["uint.Parse"]     = "CS2SX_UInt_Parse",
            ["UInt32.Parse"]   = "CS2SX_UInt_Parse",
            ["uint.TryParse"]  = "CS2SX_UInt_TryParse",
            ["UInt32.TryParse"]= "CS2SX_UInt_TryParse",

            ["bool.Parse"]      = "CS2SX_Bool_Parse",
            ["Boolean.Parse"]   = "CS2SX_Bool_Parse",
            ["bool.TryParse"]   = "CS2SX_Bool_TryParse",
            ["Boolean.TryParse"]= "CS2SX_Bool_TryParse",

            ["short.Parse"]    = "CS2SX_Short_Parse",
            ["Int16.Parse"]    = "CS2SX_Short_Parse",
            ["short.TryParse"] = "CS2SX_Short_TryParse",
            ["Int16.TryParse"] = "CS2SX_Short_TryParse",

            ["ushort.Parse"]    = "CS2SX_UShort_Parse",
            ["UInt16.Parse"]    = "CS2SX_UShort_Parse",
            ["ushort.TryParse"] = "CS2SX_UShort_TryParse",
            ["UInt16.TryParse"] = "CS2SX_UShort_TryParse",

            ["byte.Parse"]    = "CS2SX_Byte_Parse",
            ["Byte.Parse"]    = "CS2SX_Byte_Parse",
            ["byte.TryParse"] = "CS2SX_Byte_TryParse",
            ["Byte.TryParse"] = "CS2SX_Byte_TryParse",

            ["sbyte.Parse"]    = "CS2SX_SByte_Parse",
            ["SByte.Parse"]    = "CS2SX_SByte_Parse",
            ["sbyte.TryParse"] = "CS2SX_SByte_TryParse",
            ["SByte.TryParse"] = "CS2SX_SByte_TryParse",
        };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr, out string result)
    {
        if (!s_map.TryGetValue(calleeStr, out var cFunc))
            return NotHandled(out result);

        // TryParse: zweites Argument ist out → BuildArg hängt bereits & an
        result = cFunc + "(" + JoinArgs(args) + ")";
        return true;
    }
}