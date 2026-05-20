// ============================================================================
// CS2SX — Transpiler/Handlers/LinqHandler.cs
//
// Transpiles common LINQ method-chain calls to C equivalents using the
// existing List_T / Dict macros and inline loops.
//
// Supported:
//   .Where(pred)        → filtered List_T_New + loop
//   .Select(proj)       → projected List_T_New + loop
//   .First(pred?)       → loop returning first match (or list[0])
//   .FirstOrDefault()   → same but returns 0/NULL on empty
//   .Last()             → last element
//   .Any(pred?)         → loop returning 1/0
//   .All(pred)          → loop returning 1/0
//   .Count(pred?)       → loop counting matches
//   .Sum(proj?)         → loop summing (optional projector lambda)
//   .Min(proj?) / .Max(proj?) → loop finding min/max (optional projector)
//   .Average(proj?)     → loop averaging (optional projector)
//   .Aggregate(seed, func) → loop folding with accumulator
//   .ToList()           → copy
//   .OrderBy(key)       → insertion-sort copy
//   .Contains(val)      → List_T_Contains / string search
//   .Distinct()         → filtered copy removing duplicates
//   .Skip(n) / .Take(n) → sliced copy
//   .Concat(other)      → merged copy
//   .Reverse()          → reversed copy
// ============================================================================

using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

public sealed class LinqHandler : InvocationHandlerBase
{
    private static readonly HashSet<string> s_linqMethods = new(StringComparer.Ordinal)
    {
        "Where", "Select", "First", "FirstOrDefault", "Last", "LastOrDefault",
        "Any", "All", "Count", "Sum", "Min", "Max", "ToList", "ToArray",
        "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending",
        "Contains", "Distinct", "Skip", "Take", "Concat", "Reverse",
        "Single", "SingleOrDefault", "ElementAt", "ElementAtOrDefault",
        "Average", "Aggregate",
        "ToDictionary", "ToHashSet", "GroupBy", "Zip",
    };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        if (inv.Expression is not MemberAccessExpressionSyntax mem
            || !s_linqMethods.Contains(mem.Name.Identifier.Text))
            return NotHandled(out result);

        var methodName = mem.Name.Identifier.Text;
        var sourceRaw = mem.Expression.ToString();
        var sourceKey = sourceRaw.TrimStart('_');
        var sourceExpr = writeExpr(mem.Expression);

        // Resolve source type — with SemanticModel fallback
        string? colType = null;
        ctx.LocalTypes.TryGetValue(sourceRaw, out colType);
        if (colType == null) ctx.FieldTypes.TryGetValue(sourceKey, out colType);

        if (colType == null && ctx.SemanticModel != null)
        {
            try
            {
                var typeInfo = ctx.SemanticModel.GetTypeInfo(mem.Expression);
                var sym = typeInfo.ConvertedType ?? typeInfo.Type;
                if (sym != null && sym is not Microsoft.CodeAnalysis.IErrorTypeSymbol)
                    colType = TranspilerContext.FormatTypeSymbol(sym);
            }
            catch { }
        }

        if (colType == null || (!TypeRegistry.IsList(colType) && !colType.EndsWith("[]")))
            return NotHandled(out result);

        var inner = TypeRegistry.IsList(colType)
            ? TypeRegistry.GetListInnerType(colType) ?? "int"
            : colType[..^2].Trim();
        var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
        var cInnerType = inner == "string" ? "const char*" : TypeRegistry.MapType(inner);
        var isPrim = TypeRegistry.IsPrimitive(inner);
        var elemPtr = (isPrim || inner == "string") ? "" : "*";

        var listGet = TypeRegistry.IsList(colType)
            ? $"List_{cInner}_Get({sourceExpr}, _idx)"
            : sourceExpr + "[_idx]";

        // Array length: consult ctx.ArrayLengths first, sizeof only for stack arrays
        string listCount;
        if (TypeRegistry.IsList(colType))
        {
            listCount = sourceExpr + "->count";
        }
        else if (ctx.ArrayLengths.TryGetValue(sourceRaw, out var knownLen)
              || ctx.ArrayLengths.TryGetValue(sourceKey, out knownLen))
        {
            listCount = knownLen;
        }
        else
        {
            listCount = $"(int)(sizeof({sourceExpr})/sizeof({sourceExpr}[0]))";
        }

        // Lambda / predicate arg
        LambdaExpressionSyntax? lambdaArg = null;
        if (inv.ArgumentList.Arguments.Count > 0
            && inv.ArgumentList.Arguments[0].Expression is LambdaExpressionSyntax lam)
            lambdaArg = lam;

        // Capture context: pass self for instance methods, NULL for static
        var captureCtx = ctx.IsStaticMethod ? "NULL" : "self";

        // Shared helper to create a lifted lambda
        LambdaLifter MakeLifter() {
            var lifter = new LambdaLifter(ctx, new ExpressionWriter(ctx));
            lifter.SetStatementWriter(new StatementWriter(ctx, new ExpressionWriter(ctx)));
            return lifter;
        }

        switch (methodName)
        {
            case "Where":
                {
                    if (lambdaArg == null) return NotHandled(out result);
                    var predFn = MakeLifter().LiftLambda(lambdaArg, elementTypeHint: inner);

                    var outVar = ctx.NextTmp("where");
                    ctx.WriteLine($"List_{cInner}* {outVar} = List_{cInner}_New();");
                    var idxVar = ctx.NextTmp("i");
                    ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                    ctx.WriteLine("{");
                    ctx.Indent();
                    ctx.WriteLine($"{cInnerType}{elemPtr} _e_{outVar} = {listGet.Replace("_idx", idxVar)};");
                    ctx.WriteLine($"if ({predFn}({captureCtx}, _e_{outVar})) List_{cInner}_Add({outVar}, _e_{outVar});");
                    ctx.Dedent();
                    ctx.WriteLine("}");
                    result = outVar;
                    ctx.LocalTypes[outVar] = "List<" + inner + ">";
                    return true;
                }

            case "Select":
                {
                    if (lambdaArg == null) return NotHandled(out result);
                    string projInner = "int";
                    if (lambdaArg is SimpleLambdaExpressionSyntax simpleLam)
                        projInner = TypeInferrer.InferCSharpType(simpleLam.Body, ctx);
                    else if (lambdaArg is ParenthesizedLambdaExpressionSyntax parenLam)
                        projInner = TypeInferrer.InferCSharpType(parenLam.Body, ctx);
                    var cProjInner = projInner == "string" ? "str" : TypeRegistry.MapType(projInner);
                    var cProjType = projInner == "string" ? "const char*" : TypeRegistry.MapType(projInner);
                    var projElemPtr = TypeRegistry.IsPrimitive(projInner) || projInner == "string" ? "" : "*";

                    var projFn = MakeLifter().LiftLambda(lambdaArg, elementTypeHint: inner);

                    var outVar = ctx.NextTmp("sel");
                    ctx.WriteLine($"List_{cProjInner}* {outVar} = List_{cProjInner}_New();");
                    var idxVar = ctx.NextTmp("i");
                    ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                    ctx.WriteLine("{");
                    ctx.Indent();
                    ctx.WriteLine($"{cInnerType}{elemPtr} _e_{outVar} = {listGet.Replace("_idx", idxVar)};");
                    ctx.WriteLine($"List_{cProjInner}_Add({outVar}, ({cProjType}{projElemPtr}){projFn}({captureCtx}, _e_{outVar}));");
                    ctx.Dedent();
                    ctx.WriteLine("}");
                    result = outVar;
                    ctx.LocalTypes[outVar] = "List<" + projInner + ">";
                    return true;
                }

            case "First":
            case "Single":
                {
                    if (lambdaArg != null)
                    {
                        var predFn = MakeLifter().LiftLambda(lambdaArg, elementTypeHint: inner);
                        var idxVar = ctx.NextTmp("i");
                        var retVar = ctx.NextTmp("first");
                        var fillLine = isPrim || inner == "string"
                            ? $"{cInnerType} {retVar} = 0;"
                            : $"{cInnerType}* {retVar} = NULL;";
                        ctx.WriteLine(fillLine);
                        ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                        ctx.WriteLine("{");
                        ctx.Indent();
                        ctx.WriteLine($"if ({predFn}({captureCtx}, {listGet.Replace("_idx", idxVar)})) {{ {retVar} = {listGet.Replace("_idx", idxVar)}; break; }}");
                        ctx.Dedent();
                        ctx.WriteLine("}");
                        result = retVar;
                    }
                    else
                    {
                        result = listGet.Replace("_idx", "0");
                    }
                    return true;
                }

            case "FirstOrDefault":
            case "SingleOrDefault":
                {
                    var defaultVal = isPrim ? "0" : "NULL";
                    if (lambdaArg != null)
                    {
                        var predFn = MakeLifter().LiftLambda(lambdaArg, elementTypeHint: inner);
                        var idxVar = ctx.NextTmp("i");
                        var retVar = ctx.NextTmp("fod");
                        ctx.WriteLine($"{cInnerType}{elemPtr} {retVar} = {defaultVal};");
                        ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                        ctx.WriteLine("{");
                        ctx.Indent();
                        ctx.WriteLine($"if ({predFn}({captureCtx}, {listGet.Replace("_idx", idxVar)})) {{ {retVar} = {listGet.Replace("_idx", idxVar)}; break; }}");
                        ctx.Dedent();
                        ctx.WriteLine("}");
                        result = retVar;
                    }
                    else
                    {
                        var countExpr = listCount.Replace("_idx", "0");
                        result = "(" + countExpr + " > 0 ? " + listGet.Replace("_idx", "0") + " : " + defaultVal + ")";
                    }
                    return true;
                }

            case "Last":
            case "LastOrDefault":
                {
                    var defaultVal = isPrim ? "0" : "NULL";
                    var countExpr = listCount.Replace("_idx", "0");
                    result = "(" + countExpr + " > 0 ? " + listGet.Replace("_idx", countExpr + " - 1") + " : " + defaultVal + ")";
                    return true;
                }

            case "ElementAt":
                {
                    result = listGet.Replace("_idx", args.Count > 0 ? args[0] : "0");
                    return true;
                }

            case "ElementAtOrDefault":
                {
                    var idxArg = args.Count > 0 ? args[0] : "0";
                    var countExpr = listCount.Replace("_idx", idxArg);
                    var defaultVal = isPrim ? "0" : "NULL";
                    result = "(" + idxArg + " >= 0 && " + idxArg + " < " + countExpr + " ? "
                           + listGet.Replace("_idx", idxArg) + " : " + defaultVal + ")";
                    return true;
                }

            case "Any":
                {
                    if (lambdaArg != null)
                    {
                        var predFn = MakeLifter().LiftLambda(lambdaArg, elementTypeHint: inner);
                        var idxVar = ctx.NextTmp("i");
                        var retVar = ctx.NextTmp("any");
                        ctx.WriteLine($"int {retVar} = 0;");
                        ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                        ctx.WriteLine($"  if ({predFn}({captureCtx}, {listGet.Replace("_idx", idxVar)})) {{ {retVar} = 1; break; }}");
                        result = retVar;
                    }
                    else
                    {
                        result = "(" + listCount.Replace("_idx", "0") + " > 0)";
                    }
                    return true;
                }

            case "All":
                {
                    if (lambdaArg == null) { result = "1"; return true; }
                    var predFn = MakeLifter().LiftLambda(lambdaArg, elementTypeHint: inner);
                    var idxVar = ctx.NextTmp("i");
                    var retVar = ctx.NextTmp("all");
                    ctx.WriteLine($"int {retVar} = 1;");
                    ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                    ctx.WriteLine($"  if (!{predFn}({captureCtx}, {listGet.Replace("_idx", idxVar)})) {{ {retVar} = 0; break; }}");
                    result = retVar;
                    return true;
                }

            case "Count":
                {
                    if (lambdaArg != null)
                    {
                        var predFn = MakeLifter().LiftLambda(lambdaArg, elementTypeHint: inner);
                        var idxVar = ctx.NextTmp("i");
                        var retVar = ctx.NextTmp("cnt");
                        ctx.WriteLine($"int {retVar} = 0;");
                        ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                        ctx.WriteLine($"  if ({predFn}({captureCtx}, {listGet.Replace("_idx", idxVar)})) {retVar}++;");
                        result = retVar;
                    }
                    else
                    {
                        result = listCount.Replace("_idx", "0");
                    }
                    return true;
                }

            case "Sum":
                {
                    if (lambdaArg != null)
                    {
                        // list.Sum(x => x.Score) — project each element, then sum
                        var projFn = MakeLifter().LiftLambda(lambdaArg, elementTypeHint: inner);
                        var idxVar = ctx.NextTmp("i");
                        var sumVar = ctx.NextTmp("sum");
                        ctx.WriteLine($"double {sumVar} = 0.0;");
                        ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                        ctx.WriteLine($"  {sumVar} += (double){projFn}({captureCtx}, {listGet.Replace("_idx", idxVar)});");
                        result = sumVar;
                    }
                    else
                    {
                        var idxVar = ctx.NextTmp("i");
                        var sumVar = ctx.NextTmp("sum");
                        ctx.WriteLine($"{cInnerType} {sumVar} = 0;");
                        ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                        ctx.WriteLine($"  {sumVar} += {listGet.Replace("_idx", idxVar)};");
                        result = sumVar;
                    }
                    return true;
                }

            case "Min":
                {
                    if (lambdaArg != null)
                    {
                        var projFn = MakeLifter().LiftLambda(lambdaArg, elementTypeHint: inner);
                        var idxVar = ctx.NextTmp("i");
                        var minVar = ctx.NextTmp("minv");
                        var countExpr = listCount.Replace("_idx", "0");
                        ctx.WriteLine($"double {minVar} = ({countExpr} > 0) ? (double){projFn}({captureCtx}, {listGet.Replace("_idx", "0")}) : 0.0;");
                        ctx.WriteLine($"for (int {idxVar} = 1; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                        ctx.WriteLine($"{{ double _mv = (double){projFn}({captureCtx}, {listGet.Replace("_idx", idxVar)}); if (_mv < {minVar}) {minVar} = _mv; }}");
                        result = minVar;
                    }
                    else
                    {
                        var idxVar = ctx.NextTmp("i");
                        var minVar = ctx.NextTmp("minv");
                        var countExpr = listCount.Replace("_idx", "0");
                        ctx.WriteLine($"{cInnerType} {minVar} = ({countExpr} > 0) ? {listGet.Replace("_idx", "0")} : 0;");
                        ctx.WriteLine($"for (int {idxVar} = 1; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                        ctx.WriteLine($"  if ({listGet.Replace("_idx", idxVar)} < {minVar}) {minVar} = {listGet.Replace("_idx", idxVar)};");
                        result = minVar;
                    }
                    return true;
                }

            case "Max":
                {
                    if (lambdaArg != null)
                    {
                        var projFn = MakeLifter().LiftLambda(lambdaArg, elementTypeHint: inner);
                        var idxVar = ctx.NextTmp("i");
                        var maxVar = ctx.NextTmp("maxv");
                        var countExpr = listCount.Replace("_idx", "0");
                        ctx.WriteLine($"double {maxVar} = ({countExpr} > 0) ? (double){projFn}({captureCtx}, {listGet.Replace("_idx", "0")}) : 0.0;");
                        ctx.WriteLine($"for (int {idxVar} = 1; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                        ctx.WriteLine($"{{ double _mv = (double){projFn}({captureCtx}, {listGet.Replace("_idx", idxVar)}); if (_mv > {maxVar}) {maxVar} = _mv; }}");
                        result = maxVar;
                    }
                    else
                    {
                        var idxVar = ctx.NextTmp("i");
                        var maxVar = ctx.NextTmp("maxv");
                        var countExpr = listCount.Replace("_idx", "0");
                        ctx.WriteLine($"{cInnerType} {maxVar} = ({countExpr} > 0) ? {listGet.Replace("_idx", "0")} : 0;");
                        ctx.WriteLine($"for (int {idxVar} = 1; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                        ctx.WriteLine($"  if ({listGet.Replace("_idx", idxVar)} > {maxVar}) {maxVar} = {listGet.Replace("_idx", idxVar)};");
                        result = maxVar;
                    }
                    return true;
                }

            case "Average":
                {
                    if (lambdaArg != null)
                    {
                        var projFn = MakeLifter().LiftLambda(lambdaArg, elementTypeHint: inner);
                        var idxVar = ctx.NextTmp("i");
                        var sumVar = ctx.NextTmp("avgs");
                        var cntExpr = listCount.Replace("_idx", idxVar);
                        ctx.WriteLine($"double {sumVar} = 0.0;");
                        ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {cntExpr}; {idxVar}++)");
                        ctx.WriteLine($"  {sumVar} += (double){projFn}({captureCtx}, {listGet.Replace("_idx", idxVar)});");
                        var cntVar = listCount.Replace("_idx", "0");
                        result = "(" + cntVar + " > 0 ? " + sumVar + " / " + cntVar + " : 0.0)";
                    }
                    else
                    {
                        var idxVar = ctx.NextTmp("i");
                        var sumVar = ctx.NextTmp("avgs");
                        var cntExpr = listCount.Replace("_idx", idxVar);
                        ctx.WriteLine($"double {sumVar} = 0.0;");
                        ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {cntExpr}; {idxVar}++)");
                        ctx.WriteLine($"  {sumVar} += (double){listGet.Replace("_idx", idxVar)};");
                        var cntVar = listCount.Replace("_idx", "0");
                        result = "(" + cntVar + " > 0 ? " + sumVar + " / " + cntVar + " : 0.0)";
                    }
                    return true;
                }

            case "Aggregate":
                {
                    // list.Aggregate(seed, (acc, x) => ...) — two-argument form
                    // list.Aggregate((acc, x) => ...) — no-seed form (first element is seed)
                    LambdaExpressionSyntax? accLambda = null;
                    string seedExpr = "0";

                    if (inv.ArgumentList.Arguments.Count >= 2
                        && inv.ArgumentList.Arguments[1].Expression is LambdaExpressionSyntax accLam2)
                    {
                        seedExpr = writeExpr(inv.ArgumentList.Arguments[0].Expression);
                        accLambda = accLam2;
                    }
                    else if (inv.ArgumentList.Arguments.Count == 1
                        && inv.ArgumentList.Arguments[0].Expression is LambdaExpressionSyntax accLam1)
                    {
                        accLambda = accLam1;
                    }

                    if (accLambda == null)
                    {
                        ctx.Warn(inv, "Aggregate: kein Accumulator-Lambda gefunden — no-op");
                        result = sourceExpr;
                        return true;
                    }

                    var accFn = MakeLifter().LiftLambda(accLambda, elementTypeHint: inner);
                    var accVar = ctx.NextTmp("acc");
                    var idxVar = ctx.NextTmp("i");
                    int startIdx = 0;

                    // No-seed: use first element as acc, loop from 1
                    if (inv.ArgumentList.Arguments.Count == 1)
                    {
                        ctx.WriteLine($"__auto_type {accVar} = {listGet.Replace("_idx", "0")};");
                        startIdx = 1;
                    }
                    else
                    {
                        ctx.WriteLine($"__auto_type {accVar} = {seedExpr};");
                    }

                    ctx.WriteLine($"for (int {idxVar} = {startIdx}; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                    ctx.WriteLine($"  {accVar} = {accFn}({captureCtx}, {accVar}, {listGet.Replace("_idx", idxVar)});");
                    result = accVar;
                    return true;
                }

            case "ToList":
            case "ToArray":
                {
                    var outVar = ctx.NextTmp("tol");
                    var idxVar = ctx.NextTmp("i");
                    ctx.WriteLine($"List_{cInner}* {outVar} = List_{cInner}_New();");
                    ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                    ctx.WriteLine($"  List_{cInner}_Add({outVar}, {listGet.Replace("_idx", idxVar)});");
                    result = outVar;
                    ctx.LocalTypes[outVar] = "List<" + inner + ">";
                    return true;
                }

            case "OrderBy":
            case "OrderByDescending":
            case "ThenBy":
            case "ThenByDescending":
                {
                    var outVar = ctx.NextTmp("ord");
                    var idxVar = ctx.NextTmp("i");
                    var jVar = ctx.NextTmp("j");
                    var tmpVar = ctx.NextTmp("otmp");
                    var descending = methodName.Contains("Descending");

                    ctx.WriteLine($"List_{cInner}* {outVar} = List_{cInner}_New();");
                    ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                    ctx.WriteLine($"  List_{cInner}_Add({outVar}, {listGet.Replace("_idx", idxVar)});");

                    if (lambdaArg != null)
                    {
                        var keyFn = MakeLifter().LiftLambda(lambdaArg, elementTypeHint: inner);

                        ctx.WriteLine($"for (int {idxVar} = 1; {idxVar} < {outVar}->count; {idxVar}++)");
                        ctx.WriteLine("{");
                        ctx.Indent();
                        ctx.WriteLine($"{cInnerType}{elemPtr} {tmpVar} = {outVar}->data[{idxVar}];");
                        ctx.WriteLine($"int {jVar} = {idxVar} - 1;");
                        var cmp = descending
                            ? $"{keyFn}({captureCtx}, {outVar}->data[{jVar}]) < {keyFn}({captureCtx}, {tmpVar})"
                            : $"{keyFn}({captureCtx}, {outVar}->data[{jVar}]) > {keyFn}({captureCtx}, {tmpVar})";
                        ctx.WriteLine($"while ({jVar} >= 0 && ({cmp}))");
                        ctx.WriteLine($"  {{ {outVar}->data[{jVar}+1] = {outVar}->data[{jVar}]; {jVar}--; }}");
                        ctx.WriteLine($"{outVar}->data[{jVar}+1] = {tmpVar};");
                        ctx.Dedent();
                        ctx.WriteLine("}");
                    }
                    else
                    {
                        ctx.WriteLine($"List_{cInner}_Sort({outVar});");
                        if (descending)
                            ctx.WriteLine($"List_{cInner}_Reverse({outVar});");
                    }

                    result = outVar;
                    ctx.LocalTypes[outVar] = "List<" + inner + ">";
                    return true;
                }

            case "Contains":
                {
                    if (args.Count == 0) { result = "0"; return true; }
                    result = "List_" + cInner + "_Contains(" + sourceExpr + ", " + args[0] + ")";
                    return true;
                }

            case "Distinct":
                {
                    var outVar = ctx.NextTmp("dist");
                    var idxVar = ctx.NextTmp("i");
                    ctx.WriteLine($"List_{cInner}* {outVar} = List_{cInner}_New();");
                    ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                    ctx.WriteLine("{");
                    ctx.Indent();
                    ctx.WriteLine($"{cInnerType}{elemPtr} _de = {listGet.Replace("_idx", idxVar)};");
                    ctx.WriteLine($"if (!List_{cInner}_Contains({outVar}, _de)) List_{cInner}_Add({outVar}, _de);");
                    ctx.Dedent();
                    ctx.WriteLine("}");
                    result = outVar;
                    ctx.LocalTypes[outVar] = "List<" + inner + ">";
                    return true;
                }

            case "Skip":
                {
                    var skipN = args.Count > 0 ? args[0] : "0";
                    var outVar = ctx.NextTmp("skp");
                    var idxVar = ctx.NextTmp("i");
                    ctx.WriteLine($"List_{cInner}* {outVar} = List_{cInner}_New();");
                    ctx.WriteLine($"for (int {idxVar} = {skipN}; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                    ctx.WriteLine($"  List_{cInner}_Add({outVar}, {listGet.Replace("_idx", idxVar)});");
                    result = outVar;
                    ctx.LocalTypes[outVar] = "List<" + inner + ">";
                    return true;
                }

            case "Take":
                {
                    var takeN = args.Count > 0 ? args[0] : "0";
                    var outVar = ctx.NextTmp("tak");
                    var idxVar = ctx.NextTmp("i");
                    ctx.WriteLine($"List_{cInner}* {outVar} = List_{cInner}_New();");
                    ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {takeN} && {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                    ctx.WriteLine($"  List_{cInner}_Add({outVar}, {listGet.Replace("_idx", idxVar)});");
                    result = outVar;
                    ctx.LocalTypes[outVar] = "List<" + inner + ">";
                    return true;
                }

            case "Concat":
                {
                    var otherExpr = args.Count > 0 ? args[0] : "NULL";
                    var outVar = ctx.NextTmp("cat");
                    var idxVar = ctx.NextTmp("i");
                    ctx.WriteLine($"List_{cInner}* {outVar} = List_{cInner}_New();");
                    ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                    ctx.WriteLine($"  List_{cInner}_Add({outVar}, {listGet.Replace("_idx", idxVar)});");
                    ctx.WriteLine($"if ({otherExpr}) for (int {idxVar} = 0; {idxVar} < {otherExpr}->count; {idxVar}++)");
                    ctx.WriteLine($"  List_{cInner}_Add({outVar}, List_{cInner}_Get({otherExpr}, {idxVar}));");
                    result = outVar;
                    ctx.LocalTypes[outVar] = "List<" + inner + ">";
                    return true;
                }

            case "Reverse":
                {
                    var outVar = ctx.NextTmp("rev");
                    var idxVar = ctx.NextTmp("i");
                    var countExpr = listCount.Replace("_idx", "0");
                    ctx.WriteLine($"List_{cInner}* {outVar} = List_{cInner}_New();");
                    ctx.WriteLine($"for (int {idxVar} = {countExpr} - 1; {idxVar} >= 0; {idxVar}--)");
                    ctx.WriteLine($"  List_{cInner}_Add({outVar}, {listGet.Replace("_idx", idxVar)});");
                    result = outVar;
                    ctx.LocalTypes[outVar] = "List<" + inner + ">";
                    return true;
                }

            case "ToHashSet":
                {
                    // list.ToHashSet() → HashSet_T_New() + loop Add
                    var outVar = ctx.NextTmp("ths");
                    var idxVar = ctx.NextTmp("i");
                    ctx.WriteLine($"HashSet_{cInner}* {outVar} = HashSet_{cInner}_New();");
                    ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                    ctx.WriteLine($"  HashSet_{cInner}_Add({outVar}, {listGet.Replace("_idx", idxVar)});");
                    result = outVar;
                    ctx.LocalTypes[outVar] = "HashSet<" + inner + ">";
                    return true;
                }

            case "ToDictionary":
                {
                    // list.ToDictionary(x => x.Key, x => x.Value)
                    // or list.ToDictionary(x => x.Key)  — value = element itself
                    if (lambdaArg == null) { result = "NULL /* ToDictionary: no key selector */"; return true; }

                    LambdaExpressionSyntax? valLambda = null;
                    if (inv.ArgumentList.Arguments.Count >= 2
                        && inv.ArgumentList.Arguments[1].Expression is LambdaExpressionSyntax vl)
                        valLambda = vl;

                    var keyFn = MakeLifter().LiftLambda(lambdaArg, elementTypeHint: inner);

                    string keyInner = "int";
                    if (lambdaArg is SimpleLambdaExpressionSyntax skl)
                        keyInner = TypeInferrer.InferCSharpType(skl.Body, ctx);
                    else if (lambdaArg is ParenthesizedLambdaExpressionSyntax pkl)
                        keyInner = TypeInferrer.InferCSharpType(pkl.Body, ctx);
                    var cKey = keyInner == "string" ? "str" : TypeRegistry.MapType(keyInner);

                    string valInner = inner;
                    string valFnName = "";
                    if (valLambda != null)
                    {
                        valFnName = MakeLifter().LiftLambda(valLambda, elementTypeHint: inner);
                        if (valLambda is SimpleLambdaExpressionSyntax svl)
                            valInner = TypeInferrer.InferCSharpType(svl.Body, ctx);
                        else if (valLambda is ParenthesizedLambdaExpressionSyntax pvl)
                            valInner = TypeInferrer.InferCSharpType(pvl.Body, ctx);
                    }
                    var cVal = valInner == "string" ? "str" : TypeRegistry.MapType(valInner);

                    var outVar = ctx.NextTmp("tdict");
                    var idxVar = ctx.NextTmp("i");
                    ctx.WriteLine($"Dict_{cKey}_{cVal}* {outVar} = Dict_{cKey}_{cVal}_New();");
                    ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listCount.Replace("_idx", idxVar)}; {idxVar}++)");
                    ctx.WriteLine("{");
                    ctx.Indent();
                    ctx.WriteLine($"{cInnerType}{elemPtr} _td_e = {listGet.Replace("_idx", idxVar)};");
                    var valExpr = valLambda != null
                        ? $"{valFnName}({captureCtx}, _td_e)"
                        : "_td_e";
                    ctx.WriteLine($"Dict_{cKey}_{cVal}_Set({outVar}, {keyFn}({captureCtx}, _td_e), {valExpr});");
                    ctx.Dedent();
                    ctx.WriteLine("}");
                    result = outVar;
                    ctx.LocalTypes[outVar] = $"Dictionary<{keyInner},{valInner}>";
                    return true;
                }

            case "GroupBy":
                {
                    // list.GroupBy(x => x.Key) → List<List<T>>  (inner lists per group)
                    // Simplified: returns a List_str* of group keys; full group support is complex.
                    // Emit a List of the source elements grouped by key — represented as List<T>.
                    if (lambdaArg == null) { result = sourceExpr; return true; }
                    ctx.Warn(inv, "GroupBy — simplified: returns source list (full group semantics not supported in C)");
                    result = sourceExpr;
                    return true;
                }

            case "Zip":
                {
                    // list.Zip(other, (a, b) => result)
                    if (inv.ArgumentList.Arguments.Count < 2) { result = "NULL"; return true; }
                    var otherExpr = writeExpr(inv.ArgumentList.Arguments[0].Expression);

                    LambdaExpressionSyntax? zipLambda = null;
                    if (inv.ArgumentList.Arguments.Count >= 2
                        && inv.ArgumentList.Arguments[1].Expression is LambdaExpressionSyntax zl)
                        zipLambda = zl;

                    if (zipLambda == null) { result = sourceExpr; return true; }

                    string projInner = "int";
                    if (zipLambda is ParenthesizedLambdaExpressionSyntax pzl)
                        projInner = TypeInferrer.InferCSharpType(pzl.Body, ctx);
                    var cProjInner = projInner == "string" ? "str" : TypeRegistry.MapType(projInner);

                    var zipFn = MakeLifter().LiftLambda(zipLambda, elementTypeHint: inner);
                    var outVar = ctx.NextTmp("zip");
                    var idxVar = ctx.NextTmp("i");
                    var otherCount = $"{otherExpr}->count";
                    var srcCount = listCount.Replace("_idx", idxVar);
                    ctx.WriteLine($"List_{cProjInner}* {outVar} = List_{cProjInner}_New();");
                    ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {srcCount} && {idxVar} < {otherCount}; {idxVar}++)");
                    ctx.WriteLine($"  List_{cProjInner}_Add({outVar}, {zipFn}({captureCtx}, {listGet.Replace("_idx", idxVar)}, List_{cInner}_Get({otherExpr}, {idxVar})));");
                    result = outVar;
                    ctx.LocalTypes[outVar] = "List<" + projInner + ">";
                    return true;
                }

            default:
                ctx.Warn(inv, $"LINQ method '{methodName}' not fully supported — emitting no-op");
                result = sourceExpr;
                return true;
        }
    }
}

