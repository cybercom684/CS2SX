// ============================================================================
// CS2SX — Transpiler/Writers/OperatorOverloadWriter.cs
//
// Emits C functions for C# operator overloads.
//
// C#:  public static Vec2 operator+(Vec2 a, Vec2 b) { ... }
// C:   Vec2 Vec2_op_add(Vec2 a, Vec2 b) { ... }
//
// And rewrites binary/unary expressions that use overloaded operators:
// C#:  v1 + v2   →   Vec2_op_add(v1, v2)
// ============================================================================

using CS2SX.Core;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Writers;

public static class OperatorOverloadWriter
{
    // Maps C# operator token text to a C function name suffix
    internal static readonly Dictionary<string, string> s_opNames = new(StringComparer.Ordinal)
    {
        ["+"] = "op_add",
        ["-"] = "op_sub",
        ["*"] = "op_mul",
        ["/"] = "op_div",
        ["%"] = "op_mod",
        ["=="] = "op_eq",
        ["!="] = "op_ne",
        ["<"] = "op_lt",
        [">"] = "op_gt",
        ["<="] = "op_le",
        [">="] = "op_ge",
        ["&"] = "op_and",
        ["|"] = "op_or",
        ["^"] = "op_xor",
        ["<<"] = "op_shl",
        [">>"] = "op_shr",
        ["!"] = "op_not",
        ["~"] = "op_bnot",
        ["++"] = "op_inc",
        ["--"] = "op_dec",
    };

    // Registry: "TypeName.op_add" → true (has overload)
    // Reset() must be called at the start of each build to avoid stale entries
    // from previous builds when running cs2sx watch (multiple builds per process).
    private static readonly HashSet<string> s_registry = new(StringComparer.Ordinal);

    public static void Reset() => s_registry.Clear();

    public static void Register(string typeName, string opToken)
    {
        if (s_opNames.TryGetValue(opToken, out var suffix))
            s_registry.Add(typeName + "." + suffix);
    }

    public static bool HasOverload(string typeName, string opToken) =>
        s_opNames.TryGetValue(opToken, out var suffix)
        && s_registry.Contains(typeName + "." + suffix);

    public static string GetFunctionName(string typeName, string opToken)
    {
        s_opNames.TryGetValue(opToken, out var suffix);
        return typeName + "_" + (suffix ?? "op_unknown");
    }

    /// <summary>
    /// Writes the header signature for an operator overload method.
    /// Called from CSharpToC.WriteFunctionSignatures.
    /// </summary>
    public static string BuildSignature(
        OperatorDeclarationSyntax op,
        string className,
        Func<ParameterSyntax, string> buildParamDecl)
    {
        var opToken = op.OperatorToken.Text;
        if (!s_opNames.TryGetValue(opToken, out var suffix))
            suffix = "op_unknown";

        var retType = TypeRegistry.MapType(op.ReturnType.ToString().Trim());
        var paramList = string.Join(", ",
            op.ParameterList.Parameters.Select(p => buildParamDecl(p)));

        Register(className, opToken);

        return $"{retType} {className}_{suffix}({paramList})";
    }

    /// <summary>
    /// Tries to rewrite a binary expression as an operator-overload call.
    /// Returns null if no overload is registered.
    /// </summary>
    public static string? TryRewriteBinary(
        BinaryExpressionSyntax bin,
        string leftCsType,
        string leftExpr,
        string rightExpr)
    {
        var opToken = bin.OperatorToken.Text;
        if (!HasOverload(leftCsType, opToken)) return null;
        var fnName = GetFunctionName(leftCsType, opToken);
        return $"{fnName}({leftExpr}, {rightExpr})";
    }

    /// <summary>
    /// Tries to rewrite a unary expression as an operator-overload call.
    /// Returns null if no overload is registered.
    /// </summary>
    public static string? TryRewriteUnary(
        PrefixUnaryExpressionSyntax pre,
        string operandCsType,
        string operandExpr)
    {
        var opToken = pre.OperatorToken.Text;
        if (!HasOverload(operandCsType, opToken)) return null;
        var fnName = GetFunctionName(operandCsType, opToken);
        return $"{fnName}({operandExpr})";
    }
}