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
        // Math.*
        ["Math.Min"]      = "MIN",
        ["Math.Max"]      = "MAX",
        ["Math.Sqrt"]     = "sqrtf",
        ["Math.Floor"]    = "floorf",
        ["Math.Ceil"]     = "ceilf",
        ["Math.Ceiling"]  = "ceilf",
        ["Math.Pow"]      = "powf",
        ["Math.Sin"]      = "sinf",
        ["Math.Cos"]      = "cosf",
        ["Math.Tan"]      = "tanf",
        ["Math.Asin"]     = "asinf",
        ["Math.Acos"]     = "acosf",
        ["Math.Atan"]     = "atanf",
        ["Math.Atan2"]    = "atan2f",
        ["Math.Sinh"]     = "sinhf",
        ["Math.Cosh"]     = "coshf",
        ["Math.Tanh"]     = "tanhf",
        ["Math.Exp"]      = "expf",
        ["Math.Log"]      = "logf",
        ["Math.Log2"]     = "log2f",
        ["Math.Log10"]    = "log10f",
        ["Math.Clamp"]    = "CLAMP",
        ["Math.Round"]    = "roundf",
        ["Math.Truncate"] = "truncf",
        ["Math.Cbrt"]     = "cbrtf",
        ["Math.Sign"]     = "CS2SX_Sign",
        ["Math.IEEERemainder"] = "remainder",

        // System.Math.*
        ["System.Math.Min"]      = "MIN",
        ["System.Math.Max"]      = "MAX",
        ["System.Math.Sqrt"]     = "sqrtf",
        ["System.Math.Floor"]    = "floorf",
        ["System.Math.Ceil"]     = "ceilf",
        ["System.Math.Ceiling"]  = "ceilf",
        ["System.Math.Pow"]      = "powf",
        ["System.Math.Sin"]      = "sinf",
        ["System.Math.Cos"]      = "cosf",
        ["System.Math.Tan"]      = "tanf",
        ["System.Math.Asin"]     = "asinf",
        ["System.Math.Acos"]     = "acosf",
        ["System.Math.Atan"]     = "atanf",
        ["System.Math.Atan2"]    = "atan2f",
        ["System.Math.Sinh"]     = "sinhf",
        ["System.Math.Cosh"]     = "coshf",
        ["System.Math.Tanh"]     = "tanhf",
        ["System.Math.Exp"]      = "expf",
        ["System.Math.Log"]      = "logf",
        ["System.Math.Log2"]     = "log2f",
        ["System.Math.Log10"]    = "log10f",
        ["System.Math.Clamp"]    = "CLAMP",
        ["System.Math.Round"]    = "roundf",
        ["System.Math.Truncate"] = "truncf",
        ["System.Math.Cbrt"]     = "cbrtf",
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
