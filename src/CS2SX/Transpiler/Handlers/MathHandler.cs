using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using CS2SX.Transpiler.Writers;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Handles Math.* / MathF.* / System.Math.* calls.
/// Math.Abs is type-aware: float/double → fabsf/fabs, int → abs.
/// </summary>
public sealed class MathHandler : InvocationHandlerBase
{
    // Methods that are type-neutral (always same C function regardless of arg type)
    private static readonly Dictionary<string, string> s_mathMap = new(StringComparer.Ordinal)
    {
        // Math.* — System.Math is double-precision in C#, so use the double C
        // functions (sqrt/floor/...), NOT the single-precision *f variants.
        ["Math.Min"]      = "MIN",
        ["Math.Max"]      = "MAX",
        ["Math.Sqrt"]     = "sqrt",
        ["Math.Floor"]    = "floor",
        ["Math.Ceil"]     = "ceil",
        ["Math.Ceiling"]  = "ceil",
        ["Math.Pow"]      = "pow",
        ["Math.Sin"]      = "sin",
        ["Math.Cos"]      = "cos",
        ["Math.Tan"]      = "tan",
        ["Math.Asin"]     = "asin",
        ["Math.Acos"]     = "acos",
        ["Math.Atan"]     = "atan",
        ["Math.Atan2"]    = "atan2",
        ["Math.Sinh"]     = "sinh",
        ["Math.Cosh"]     = "cosh",
        ["Math.Tanh"]     = "tanh",
        ["Math.Exp"]      = "exp",
        ["Math.Log"]      = "log",
        ["Math.Log2"]     = "log2",
        ["Math.Log10"]    = "log10",
        ["Math.Clamp"]    = "CLAMP",
        ["Math.Round"]    = "round",
        ["Math.Truncate"] = "trunc",
        ["Math.Cbrt"]     = "cbrt",
        ["Math.Sign"]     = "CS2SX_Sign",
        ["Math.IEEERemainder"] = "remainder",

        // System.Math.*
        ["System.Math.Min"]      = "MIN",
        ["System.Math.Max"]      = "MAX",
        ["System.Math.Sqrt"]     = "sqrt",
        ["System.Math.Floor"]    = "floor",
        ["System.Math.Ceil"]     = "ceil",
        ["System.Math.Ceiling"]  = "ceil",
        ["System.Math.Pow"]      = "pow",
        ["System.Math.Sin"]      = "sin",
        ["System.Math.Cos"]      = "cos",
        ["System.Math.Tan"]      = "tan",
        ["System.Math.Asin"]     = "asin",
        ["System.Math.Acos"]     = "acos",
        ["System.Math.Atan"]     = "atan",
        ["System.Math.Atan2"]    = "atan2",
        ["System.Math.Sinh"]     = "sinh",
        ["System.Math.Cosh"]     = "cosh",
        ["System.Math.Tanh"]     = "tanh",
        ["System.Math.Exp"]      = "exp",
        ["System.Math.Log"]      = "log",
        ["System.Math.Log2"]     = "log2",
        ["System.Math.Log10"]    = "log10",
        ["System.Math.Clamp"]    = "CLAMP",
        ["System.Math.Round"]    = "round",
        ["System.Math.Truncate"] = "trunc",
        ["System.Math.Cbrt"]     = "cbrt",
        ["System.Math.Sign"]     = "CS2SX_Sign",

        // MathF.* (single-precision variants — same C functions)
        ["MathF.Sqrt"]    = "sqrtf",
        ["MathF.Floor"]   = "floorf",
        ["MathF.Ceil"]    = "ceilf",
        ["MathF.Ceiling"] = "ceilf",
        ["MathF.Pow"]     = "powf",
        ["MathF.Sin"]     = "sinf",
        ["MathF.Cos"]     = "cosf",
        ["MathF.Tan"]     = "tanf",
        ["MathF.Asin"]    = "asinf",
        ["MathF.Acos"]    = "acosf",
        ["MathF.Atan"]    = "atanf",
        ["MathF.Atan2"]   = "atan2f",
        ["MathF.Sinh"]    = "sinhf",
        ["MathF.Cosh"]    = "coshf",
        ["MathF.Tanh"]    = "tanhf",
        ["MathF.Exp"]     = "expf",
        ["MathF.Log"]     = "logf",
        ["MathF.Log2"]    = "log2f",
        ["MathF.Log10"]   = "log10f",
        ["MathF.Round"]   = "roundf",
        ["MathF.Truncate"]= "truncf",
        ["MathF.Cbrt"]    = "cbrtf",
        ["MathF.Min"]     = "MIN",
        ["MathF.Max"]     = "MAX",
        ["MathF.Clamp"]   = "CLAMP",
        ["MathF.Sign"]    = "CS2SX_Sign",
    };

    // Abs needs type inference: int→abs, float→fabsf, double→fabs, long→llabs
    private static readonly HashSet<string> s_absNames = new(StringComparer.Ordinal)
    {
        "Math.Abs", "System.Math.Abs", "MathF.Abs",
    };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        // Type-aware Abs
        if (s_absNames.Contains(calleeStr))
        {
            result = WriteAbs(inv, args, ctx);
            return true;
        }

        // Math.Round(value, digits): C round() takes one arg, so scale manually.
        if ((calleeStr is "Math.Round" or "System.Math.Round" or "MathF.Round")
            && args.Count == 2)
        {
            var roundFn = calleeStr == "MathF.Round" ? "roundf" : "round";
            var powFn   = calleeStr == "MathF.Round" ? "powf"   : "pow";
            result = $"({roundFn}(({args[0]}) * {powFn}(10, ({args[1]}))) / {powFn}(10, ({args[1]})))";
            return true;
        }

        if (!s_mathMap.TryGetValue(calleeStr, out var cFunc))
            return NotHandled(out result);

        result = cFunc + "(" + JoinArgs(args) + ")";
        return true;
    }

    private static string WriteAbs(InvocationExpressionSyntax inv,
        List<string> args, TranspilerContext ctx)
    {
        var argExpr = inv.ArgumentList.Arguments.Count > 0
            ? inv.ArgumentList.Arguments[0].Expression : null;
        var csType = argExpr != null ? TypeInferrer.InferCSharpType(argExpr, ctx) : "int";

        var fn = csType switch
        {
            "float"  => "fabsf",
            "double" => "fabs",
            "long"   => "llabs",
            _        => "abs",
        };
        return fn + "(" + ArgAt(args, 0) + ")";
    }
}
