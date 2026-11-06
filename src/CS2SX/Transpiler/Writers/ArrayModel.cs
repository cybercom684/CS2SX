// ============================================================================
// CS2SX — Transpiler/Writers/ArrayModel.cs
//
// The single place that knows how C# arrays are represented in C.
//
// A C# T[] is a C `T*` (element pointer) preceded by a CS2SX_ArrHdr carrying the
// length (Runtime/switchforms.h). That keeps `a[i]` plain C indexing while
// `a.Length` works wherever the array travels — CS2SX_ArrLen(a).
//
//   new T[n]                 → (T*)CS2SX_ArrNew(n, sizeof(T))            heap
//   new[] { a, b } / {a, b}  → (T*)CS2SX_ArrFromInit((T[]){a, b}, 2, ..) heap
//   local, constant size,    → CS2SX_ARR_LOCAL(T, name, n, a, b)          stack
//   never leaves the method
//   static T[] field         → CS2SX_ARR_STATIC(T, Cls_name, n, ...)      global
//
// Element C types: string → const char*, class → Cls*, everything else MapType.
// ============================================================================

using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Writers;

public static class ArrayModel
{
    /// Stack arrays above this many bytes go to the heap (main-thread stack is small).
    private const int MaxStackBytes = 4096;

    /// C type of one element of a C# array with element type <paramref name="elemCs"/>.
    public static string ElemCType(string elemCs)
    {
        elemCs = elemCs.Trim();
        if (elemCs == "string") return "const char*";
        if (elemCs == "bool") return "int";
        var mapped = TypeRegistry.MapType(elemCs);
        if (!mapped.EndsWith("*") && TypeRegistry.NeedsPointerSuffix(elemCs)) mapped += "*";
        return mapped;
    }

    public static string NewHeap(string elemC, string count) =>
        $"({elemC}*)CS2SX_ArrNew({count}, sizeof({elemC}))";

    public static string FromInit(string elemC, IReadOnlyList<string> elems) =>
        elems.Count == 0
            ? NewHeap(elemC, "0")
            : $"({elemC}*)CS2SX_ArrFromInit(({elemC}[]){{ {string.Join(", ", elems)} }}, "
              + $"{elems.Count}, sizeof({elemC}))";

    public static string LocalDecl(string elemC, string name, string count, IReadOnlyList<string>? elems = null)
    {
        var init = elems is { Count: > 0 } ? ", " + string.Join(", ", elems) : "";
        return $"CS2SX_ARR_LOCAL({elemC}, {name}, {count}{init});";
    }

    public static string StaticDef(string elemC, string fullName, string count, IReadOnlyList<string>? elems = null)
    {
        var init = elems is { Count: > 0 } ? ", " + string.Join(", ", elems) : "";
        return $"CS2SX_ARR_STATIC({elemC}, {fullName}, {count}{init});";
    }

    public static string Length(string arrayExpr) => $"CS2SX_ArrLen({arrayExpr})";

    /// <summary>
    /// Length of an array expression: the compile-time length when this method
    /// declared the (never reassigned) array with a known size, else the runtime header.
    /// </summary>
    public static string LengthOf(string rawExpr, string cExpr, TranspilerContext ctx)
    {
        if (ctx.ArrayLengths.TryGetValue(rawExpr, out var known)) return known;
        return Length(cExpr);
    }

    /// Element C# type of an array expression, via the semantic model first.
    public static string? ElementCsType(ExpressionSyntax expr, TranspilerContext ctx)
    {
        if (ctx.SemanticModel != null)
        {
            try
            {
                if (ctx.SemanticModel.GetTypeInfo(expr).Type is IArrayTypeSymbol at)
                    return TranspilerContext.FormatTypeSymbol(at.ElementType);
            }
            catch { }
        }
        var raw = expr.ToString();
        if (ctx.LocalTypes.TryGetValue(raw, out var lt) && lt.EndsWith("[]")) return lt[..^2];
        if (ctx.FieldTypes.TryGetValue(raw.TrimStart('_'), out var ft) && ft.EndsWith("[]")) return ft[..^2];
        return null;
    }

    public static bool IsArrayExpr(ExpressionSyntax expr, TranspilerContext ctx)
    {
        if (ctx.SemanticModel != null)
        {
            try { return ctx.SemanticModel.GetTypeInfo(expr).Type is IArrayTypeSymbol; }
            catch { }
        }
        var raw = expr.ToString();
        return (ctx.LocalTypes.TryGetValue(raw, out var lt) && lt.EndsWith("[]"))
            || (ctx.FieldTypes.TryGetValue(raw.TrimStart('_'), out var ft) && ft.EndsWith("[]"));
    }

    /// <summary>
    /// True when a local array may live on the stack: its size is a compile-time
    /// constant, it is small, and the variable never escapes the method — every use
    /// is an element access, <c>.Length</c>, a foreach source, or an argument to a
    /// framework call (Array.*, string.Join, Console.*). Being returned, stored in a
    /// field/collection, passed to user code or captured by a lambda means heap.
    /// </summary>
    public static bool CanLiveOnStack(VariableDeclaratorSyntax v, int count, string elemC, TranspilerContext ctx)
    {
        if (count < 0 || count * EstimateSize(elemC) > MaxStackBytes) return false;
        if (ctx.SemanticModel == null) return false;

        var body = v.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>()?.Body as SyntaxNode
                ?? v.FirstAncestorOrSelf<AccessorDeclarationSyntax>()?.Body;
        if (body == null) return false;

        ISymbol? local;
        try { local = ctx.SemanticModel.GetDeclaredSymbol(v); }
        catch { return false; }
        if (local == null) return false;

        foreach (var id in body.DescendantNodes().OfType<IdentifierNameSyntax>())
        {
            if (id.Identifier.Text != v.Identifier.Text) continue;
            ISymbol? sym;
            try { sym = ctx.SemanticModel.GetSymbolInfo(id).Symbol; }
            catch { return false; }
            if (!SymbolEqualityComparer.Default.Equals(sym, local)) continue;

            if (id.FirstAncestorOrSelf<LambdaExpressionSyntax>() != null
                || id.FirstAncestorOrSelf<LocalFunctionStatementSyntax>() != null)
                return false; // captured — may outlive the frame

            if (!IsNonEscapingUse(id, ctx)) return false;
        }
        return true;
    }

    /// <summary>
    /// True when the local is never reassigned (no <c>a = …</c>, no ref/out argument),
    /// so the length known at its declaration stays valid for the whole method.
    /// </summary>
    public static bool IsNeverReassigned(VariableDeclaratorSyntax v, TranspilerContext ctx)
    {
        var body = v.FirstAncestorOrSelf<BaseMethodDeclarationSyntax>()?.Body as SyntaxNode
                ?? v.FirstAncestorOrSelf<AccessorDeclarationSyntax>()?.Body;
        if (body == null) return false;
        foreach (var id in body.DescendantNodes().OfType<IdentifierNameSyntax>())
        {
            if (id.Identifier.Text != v.Identifier.Text) continue;
            if (id.Parent is AssignmentExpressionSyntax asg && asg.Left == id) return false;
            if (id.Parent is ArgumentSyntax arg && !arg.RefKindKeyword.IsKind(SyntaxKind.None)) return false;
        }
        return true;
    }

    private static bool IsNonEscapingUse(IdentifierNameSyntax id, TranspilerContext ctx)
    {
        switch (id.Parent)
        {
            case ElementAccessExpressionSyntax ea when ea.Expression == id:
                return true;                                     // a[i] (read or write)
            case MemberAccessExpressionSyntax ma when ma.Expression == id:
                return ma.Name.Identifier.Text == "Length";      // a.Length
            case ForEachStatementSyntax fe when fe.Expression == id:
                return true;                                     // foreach (x in a)
            case ArgumentSyntax arg when arg.RefKindKeyword.IsKind(SyntaxKind.None)
                                      && arg.Parent?.Parent is InvocationExpressionSyntax inv:
                return IsFrameworkCall(inv, ctx);
            default:
                return false;
        }
    }

    private static bool IsFrameworkCall(InvocationExpressionSyntax inv, TranspilerContext ctx)
    {
        try
        {
            if (ctx.SemanticModel?.GetSymbolInfo(inv).Symbol is IMethodSymbol m)
            {
                var ns = m.ContainingNamespace?.ToDisplayString() ?? "";
                return ns == "System" || ns.StartsWith("System.", StringComparison.Ordinal);
            }
        }
        catch { }
        return false;
    }

    private static int EstimateSize(string elemC) => elemC switch
    {
        "char" or "u8" or "s8" or "unsigned char" or "signed char" => 1,
        "short" or "u16" or "s16" or "unsigned short" => 2,
        "int" or "float" or "u32" or "s32" or "unsigned int" => 4,
        _ => 8,
    };

    /// Constant integer value of a size expression, or -1.
    public static int ConstantSize(ExpressionSyntax sizeExpr, TranspilerContext ctx)
    {
        try
        {
            var c = ctx.SemanticModel?.GetConstantValue(sizeExpr);
            if (c is { HasValue: true, Value: int i }) return i;
        }
        catch { }
        return sizeExpr is LiteralExpressionSyntax lit && lit.Token.Value is int v ? v : -1;
    }
}
