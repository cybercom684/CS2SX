using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Transpiler.Writers;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Handles static Array methods.
///
/// Array.Sort(arr)                      → qsort(arr, n, sizeof(*arr), _cs2sx_cmp_T)
/// Array.Sort(arr, comparer)            → qsort with lambda-lifted compare
/// Array.Copy(src, dst, count)          → memcpy(dst, src, count*sizeof(*dst))
/// Array.Copy(src, si, dst, di, count)  → memcpy(dst+di, src+si, count*sizeof(*src))
/// Array.Fill(arr, val)                 → loop
/// Array.Fill(arr, val, start, count)   → loop with range
/// Array.IndexOf(arr, val)              → linear search
/// Array.Clear(arr, index, length)      → memset
/// Array.Resize(ref arr, newSize)       → realloc
/// Array.Reverse(arr)                   → CS2SX_Array_Reverse inline
/// </summary>
public sealed class ArrayHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        switch (calleeStr)
        {
            case "Array.Sort":
                result = HandleSort(inv, args, ctx, writeExpr);
                return true;

            case "Array.Copy":
                result = HandleCopy(args, ctx);
                return true;

            case "Array.Fill":
                result = HandleFill(inv, args, ctx, writeExpr);
                return true;

            case "Array.IndexOf":
                result = HandleIndexOf(inv, args, ctx, writeExpr);
                return true;

            case "Array.Clear":
                result = HandleClear(args, ctx);
                return true;

            case "Array.Resize":
                result = HandleResize(inv, args, ctx, writeExpr);
                return true;

            case "Array.Reverse":
                result = HandleReverse(inv, args, ctx, writeExpr);
                return true;

            case "Array.Exists":
                result = HandleExists(inv, args, ctx, writeExpr);
                return true;

            case "Array.FindIndex":
                result = HandleFindIndex(inv, args, ctx, writeExpr);
                return true;
        }

        return NotHandled(out result);
    }

    private static string HandleSort(InvocationExpressionSyntax inv, List<string> args,
        TranspilerContext ctx, Func<SyntaxNode?, string> writeExpr)
    {
        if (args.Count == 0) return "/* Array.Sort — no array provided */";

        var arrName = args[0];

        // Determine element type and array length
        var arrRaw = inv.ArgumentList.Arguments[0].Expression.ToString();
        var elemType = InferArrayElementType(arrRaw, ctx);
        var cmpFunc = GetCmpFunction(elemType);
        var lenExpr = ctx.ArrayLengths.TryGetValue(arrRaw, out var len) ? len : "0 /* length unknown */";

        if (args.Count >= 2 && inv.ArgumentList.Arguments[1].Expression is LambdaExpressionSyntax)
        {
            // qsort with lambda-lifted compare — emit inline insertion sort for simplicity
            var idxI = ctx.NextTmp("si");
            var idxJ = ctx.NextTmp("sj");
            var tmp = ctx.NextTmp("stmp");
            var cType = elemType == "string" ? "const char*" : TypeRegistry.MapType(elemType);
            ctx.WriteLine($"for (int {idxI} = 1; {idxI} < {lenExpr}; {idxI}++)");
            ctx.WriteLine("{");
            ctx.Indent();
            ctx.WriteLine($"{cType} {tmp} = {arrName}[{idxI}];");
            ctx.WriteLine($"int {idxJ} = {idxI} - 1;");
            ctx.WriteLine($"while ({idxJ} >= 0 && {args[1]}({arrName}[{idxJ}], {tmp}) > 0)");
            ctx.WriteLine("{");
            ctx.Indent();
            ctx.WriteLine($"{arrName}[{idxJ}+1] = {arrName}[{idxJ}];");
            ctx.WriteLine($"{idxJ}--;");
            ctx.Dedent();
            ctx.WriteLine("}");
            ctx.WriteLine($"{arrName}[{idxJ}+1] = {tmp};");
            ctx.Dedent();
            ctx.WriteLine("}");
            return "/* sorted */";
        }

        return $"qsort({arrName}, {lenExpr}, sizeof(*{arrName}), {cmpFunc})";
    }

    private static string HandleCopy(List<string> args, TranspilerContext ctx)
    {
        if (args.Count == 3)
            // Array.Copy(src, dst, count)
            return $"memcpy({args[1]}, {args[0]}, ({args[2]}) * sizeof(*{args[1]}))";

        if (args.Count == 5)
            // Array.Copy(src, srcOffset, dst, dstOffset, count)
            return $"memcpy({args[2]} + ({args[3]}), {args[0]} + ({args[1]}), ({args[4]}) * sizeof(*{args[2]}))";

        return "/* Array.Copy — unsupported argument count */";
    }

    private static string HandleFill(InvocationExpressionSyntax inv, List<string> args,
        TranspilerContext ctx, Func<SyntaxNode?, string> writeExpr)
    {
        if (args.Count < 2) return "/* Array.Fill — missing arguments */";
        var arrName = args[0];
        var val = args[1];
        var arrRaw = inv.ArgumentList.Arguments[0].Expression.ToString();
        var elemType = InferArrayElementType(arrRaw, ctx);
        var lenExpr = ctx.ArrayLengths.TryGetValue(arrRaw, out var len) ? len : "0 /* length unknown */";

        string startExpr = args.Count >= 3 ? args[2] : "0";
        string countExpr = args.Count >= 4 ? args[3] : lenExpr;

        if (elemType is "int" or "float" or "double" or "byte" or "long" or "short" or "bool"
                     or "uint" or "ulong" or "ushort" or "sbyte")
        {
            // For numeric types: memset only works for 0 and -1; use loop otherwise
            var idxVar = ctx.NextTmp("fi");
            ctx.WriteLine($"for (int {idxVar} = {startExpr}; {idxVar} < ({startExpr}) + ({countExpr}); {idxVar}++)");
            ctx.WriteLine($"    {arrName}[{idxVar}] = {val};");
            return "/* filled */";
        }

        var idx = ctx.NextTmp("fi");
        ctx.WriteLine($"for (int {idx} = {startExpr}; {idx} < ({startExpr}) + ({countExpr}); {idx}++)");
        ctx.WriteLine($"    {arrName}[{idx}] = {val};");
        return "/* filled */";
    }

    private static string HandleIndexOf(InvocationExpressionSyntax inv, List<string> args,
        TranspilerContext ctx, Func<SyntaxNode?, string> writeExpr)
    {
        if (args.Count < 2) return "-1";
        var arrName = args[0];
        var val = args[1];
        var arrRaw = inv.ArgumentList.Arguments[0].Expression.ToString();
        var elemType = InferArrayElementType(arrRaw, ctx);
        var lenExpr = ctx.ArrayLengths.TryGetValue(arrRaw, out var len) ? len : "0";

        var resultVar = ctx.NextTmp("idx");
        var iVar = ctx.NextTmp("ii");
        var cmp = elemType == "string"
            ? $"strcmp({arrName}[{iVar}], {val}) == 0"
            : $"{arrName}[{iVar}] == {val}";

        ctx.WriteLine($"int {resultVar} = -1;");
        ctx.WriteLine($"for (int {iVar} = 0; {iVar} < {lenExpr}; {iVar}++)");
        ctx.WriteLine($"    if ({cmp}) {{ {resultVar} = {iVar}; break; }}");
        return resultVar;
    }

    private static string HandleClear(List<string> args, TranspilerContext ctx)
    {
        if (args.Count < 3) return "/* Array.Clear — missing arguments */";
        // Array.Clear(arr, index, length)
        return $"memset({args[0]} + ({args[1]}), 0, ({args[2]}) * sizeof(*{args[0]}))";
    }

    private static string HandleResize(InvocationExpressionSyntax inv, List<string> args,
        TranspilerContext ctx, Func<SyntaxNode?, string> writeExpr)
    {
        if (args.Count < 2) return "/* Array.Resize — missing arguments */";
        // args[0] is &arr (ref parameter), args[1] is new size
        // Strip & if BuildArg already added it
        var arrPtr = args[0].StartsWith("&") ? args[0][1..] : args[0];
        var arrRaw = inv.ArgumentList.Arguments[0].Expression.ToString().TrimStart('&');
        var elemType = InferArrayElementType(arrRaw, ctx);
        var cType = elemType == "string" ? "const char*" : TypeRegistry.MapType(elemType);
        ctx.ArrayLengths[arrRaw] = args[1];
        return $"({arrPtr} = ({cType}*)realloc({arrPtr}, ({args[1]}) * sizeof({cType})))";
    }

    private static string HandleReverse(InvocationExpressionSyntax inv, List<string> args,
        TranspilerContext ctx, Func<SyntaxNode?, string> writeExpr)
    {
        if (args.Count == 0) return "/* Array.Reverse — no array */";
        var arrName = args[0];
        var arrRaw = inv.ArgumentList.Arguments[0].Expression.ToString();
        var elemType = InferArrayElementType(arrRaw, ctx);
        var cType = elemType == "string" ? "const char*" : TypeRegistry.MapType(elemType);
        var lenExpr = ctx.ArrayLengths.TryGetValue(arrRaw, out var len) ? len : "0";
        var lo = ctx.NextTmp("rlo");
        var hi = ctx.NextTmp("rhi");
        var tmp = ctx.NextTmp("rtmp");

        ctx.WriteLine($"for (int {lo} = 0, {hi} = ({lenExpr}) - 1; {lo} < {hi}; {lo}++, {hi}--)");
        ctx.WriteLine("{");
        ctx.Indent();
        ctx.WriteLine($"{cType} {tmp} = {arrName}[{lo}];");
        ctx.WriteLine($"{arrName}[{lo}] = {arrName}[{hi}];");
        ctx.WriteLine($"{arrName}[{hi}] = {tmp};");
        ctx.Dedent();
        ctx.WriteLine("}");
        return "/* reversed */";
    }

    private static string HandleExists(InvocationExpressionSyntax inv, List<string> args,
        TranspilerContext ctx, Func<SyntaxNode?, string> writeExpr)
    {
        if (args.Count < 2) return "0";
        var arrName = args[0];
        var pred = args[1];
        var arrRaw = inv.ArgumentList.Arguments[0].Expression.ToString();
        var lenExpr = ctx.ArrayLengths.TryGetValue(arrRaw, out var len) ? len : "0";
        var resultVar = ctx.NextTmp("ex");
        var iVar = ctx.NextTmp("ei");

        ctx.WriteLine($"int {resultVar} = 0;");
        ctx.WriteLine($"for (int {iVar} = 0; {iVar} < {lenExpr}; {iVar}++)");
        ctx.WriteLine($"    if ({pred}({arrName}[{iVar}])) {{ {resultVar} = 1; break; }}");
        return resultVar;
    }

    private static string HandleFindIndex(InvocationExpressionSyntax inv, List<string> args,
        TranspilerContext ctx, Func<SyntaxNode?, string> writeExpr)
    {
        if (args.Count < 2) return "-1";
        var arrName = args[0];
        var pred = args[1];
        var arrRaw = inv.ArgumentList.Arguments[0].Expression.ToString();
        var lenExpr = ctx.ArrayLengths.TryGetValue(arrRaw, out var len) ? len : "0";
        var resultVar = ctx.NextTmp("fi");
        var iVar = ctx.NextTmp("fii");

        ctx.WriteLine($"int {resultVar} = -1;");
        ctx.WriteLine($"for (int {iVar} = 0; {iVar} < {lenExpr}; {iVar}++)");
        ctx.WriteLine($"    if ({pred}({arrName}[{iVar}])) {{ {resultVar} = {iVar}; break; }}");
        return resultVar;
    }

    private static string InferArrayElementType(string arrRaw, TranspilerContext ctx)
    {
        if (ctx.LocalTypes.TryGetValue(arrRaw, out var lt) && lt.EndsWith("[]"))
            return lt[..^2];
        if (ctx.FieldTypes.TryGetValue(arrRaw.TrimStart('_'), out var ft) && ft.EndsWith("[]"))
            return ft[..^2];

        // Emit a warning so the user knows the fallback was used instead of silently
        // producing wrong C code (e.g. int comparisons for float/double arrays).
        ctx.Warn($"Array element type for '{arrRaw}' could not be inferred — defaulting to int; add explicit type annotation", "ArrayHandler");
        return "int";
    }

    private static string GetCmpFunction(string csType) => csType switch
    {
        "int"    => "_cs2sx_cmp_int",
        "float"  => "_cs2sx_cmp_float",
        "double" => "_cs2sx_cmp_double",
        "long"   => "_cs2sx_cmp_long",
        "string" => "_cs2sx_cmp_str",
        _        => "_cs2sx_cmp_int",
    };
}
