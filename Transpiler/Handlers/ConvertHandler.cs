using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Transpiler.Writers;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Handles Convert.ToXxx() — maps to casts or Parse-style C functions.
///
/// Numeric targets (ToInt32, ToFloat, etc.):
///   - argument is numeric  → (T)(arg)
///   - argument is string   → CS2SX_Xxx_Parse(arg)
///   - argument is bool     → special cast
///
/// Convert.ToString(x) → snprintf to pool buffer with inferred format specifier
///
/// Convert.ToBase64String / FromBase64String → not supported, emits warning
/// </summary>
public sealed class ConvertHandler : InvocationHandlerBase
{
    // Maps Convert.ToXxx → (C-cast, CS-type-name-for-parse-fallback)
    private static readonly Dictionary<string, (string cast, string parseFunc)> s_numericTargets =
        new(StringComparer.Ordinal)
        {
            ["Convert.ToInt32"]   = ("(int)",           "CS2SX_Int_Parse"),
            ["Convert.ToInt"]     = ("(int)",           "CS2SX_Int_Parse"),
            ["Convert.ToInteger"] = ("(int)",           "CS2SX_Int_Parse"),
            ["Convert.ToInt64"]   = ("(long)",          "CS2SX_Long_Parse"),
            ["Convert.ToLong"]    = ("(long)",          "CS2SX_Long_Parse"),
            ["Convert.ToUInt32"]  = ("(unsigned int)",  "CS2SX_UInt_Parse"),
            ["Convert.ToUInt64"]  = ("(unsigned long)", "CS2SX_ULong_Parse"),
            ["Convert.ToSingle"]  = ("(float)",         "CS2SX_Float_Parse"),
            ["Convert.ToFloat"]   = ("(float)",         "CS2SX_Float_Parse"),
            ["Convert.ToDouble"]  = ("(double)",        "CS2SX_Double_Parse"),
            ["Convert.ToByte"]    = ("(uint8_t)",       "CS2SX_Byte_Parse"),
            ["Convert.ToSByte"]   = ("(int8_t)",        "CS2SX_SByte_Parse"),
            ["Convert.ToInt16"]   = ("(short)",         "CS2SX_Short_Parse"),
            ["Convert.ToUInt16"]  = ("(unsigned short)","CS2SX_UShort_Parse"),
            ["Convert.ToBoolean"] = ("(bool)",          "CS2SX_Bool_Parse"),
            ["Convert.ToBool"]    = ("(bool)",          "CS2SX_Bool_Parse"),
            ["Convert.ToChar"]    = ("(char)",          "CS2SX_Char_Parse"),
        };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        // ── Convert.ToString ──────────────────────────────────────────────────
        if (calleeStr == "Convert.ToString")
        {
            if (args.Count == 0) { result = "\"\""; return true; }

            // Infer type from first syntax argument
            var argNode = inv.ArgumentList.Arguments.Count > 0
                ? inv.ArgumentList.Arguments[0].Expression
                : null;
            var csType = argNode != null
                ? TypeInferrer.InferCSharpType(argNode, ctx)
                : "int";
            var spec = TypeRegistry.FormatSpecifier(TypeRegistry.MapType(csType));

            if (csType == "string" || csType == "const char*")
            {
                // Already a string — return as-is
                result = args[0];
                return true;
            }

            if (csType == "bool")
            {
                // bool → "true"/"false"
                result = "(" + args[0] + " ? \"true\" : \"false\")";
                return true;
            }

            var buf = ctx.NextStringBuf();
            ctx.Out.WriteLine(ctx.Tab
                + "snprintf(" + buf + ", sizeof(" + buf + "), \"" + spec + "\", " + args[0] + ");");
            result = buf;
            return true;
        }

        // ── Numeric conversions ───────────────────────────────────────────────
        if (!s_numericTargets.TryGetValue(calleeStr, out var mapping))
            return NotHandled(out result);

        if (args.Count == 0)
        {
            result = mapping.cast + "0";
            return true;
        }

        // Detect if argument is a string → use Parse function instead of cast
        var firstArgNode = inv.ArgumentList.Arguments.Count > 0
            ? inv.ArgumentList.Arguments[0].Expression
            : null;
        var argType = firstArgNode != null
            ? TypeInferrer.InferCSharpType(firstArgNode, ctx)
            : "int";

        // Convert.ToInt32(str, base) — second arg is numeric base (e.g. 16 for hex)
        if (argType == "string" && args.Count >= 2)
        {
            // Only int-like targets support radix parsing
            if (calleeStr is "Convert.ToInt32" or "Convert.ToInt" or "Convert.ToInt64"
                or "Convert.ToUInt32" or "Convert.ToUInt64" or "Convert.ToInt16"
                or "Convert.ToUInt16" or "Convert.ToByte" or "Convert.ToSByte")
            {
                result = calleeStr switch
                {
                    "Convert.ToInt64"  => "(long long)strtoll(" + args[0] + ", NULL, " + args[1] + ")",
                    "Convert.ToUInt64" => "(unsigned long long)strtoull(" + args[0] + ", NULL, " + args[1] + ")",
                    "Convert.ToUInt32" => "(unsigned int)strtoul(" + args[0] + ", NULL, " + args[1] + ")",
                    _                  => "(int)strtol(" + args[0] + ", NULL, " + args[1] + ")"
                };
                return true;
            }
        }

        result = (argType == "string")
            ? mapping.parseFunc + "(" + args[0] + ")"
            : mapping.cast + "(" + args[0] + ")";
        return true;
    }
}
