// Datei: Transpiler/Handlers/ListHandler.cs
//
// FIX: BuildCustomSort() nutzt nicht mehr lifter.HasPrelude / lifter.ConsumePrelude.
//      Diese Methoden existieren im neuen LambdaLifter nicht mehr.
//      Stattdessen: LiftLambda() schreibt Preludes direkt in _ctx.PendingLambdaPreludes.
//      CSharpToC.VisitMethodDeclaration() flusht diese vor der Methodensignatur.
//      Hier in BuildCustomSort() ist kein manueller Flush nötig — die Preludes
//      werden automatisch beim nächsten Methoden-Flush ausgegeben.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using CS2SX.Transpiler;
using CS2SX.Transpiler.Writers;

namespace CS2SX.Transpiler.Handlers;

public sealed class ListHandler : InvocationHandlerBase
{
    private static readonly HashSet<string> s_methods = new(StringComparer.Ordinal)
    {
        "Add", "Clear", "RemoveAt", "Remove", "Contains", "Insert",
        "Sort", "IndexOf", "Reverse", "ForEach",
        "FindAll", "TrueForAll", "ConvertAll", "Find", "Exists",
    };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        if (inv.Expression is not MemberAccessExpressionSyntax mem
            || !s_methods.Contains(mem.Name.Identifier.Text))
            return NotHandled(out result);

        var objStr = mem.Expression.ToString();

        if (!TryResolveList(objStr, ctx, out var listType, out var listExpr))
        {
            // Fallback: chained member access like _taskBar.OpenTasks — resolve via SemanticModel
            var semType = ctx.GetSemanticType(mem.Expression);
            if (semType != null && TypeRegistry.IsList(semType))
            {
                listType = semType;
                listExpr = writeExpr(mem.Expression);
            }
            else
                return NotHandled(out result);
        }

        var cList = ListFuncPrefix(listType);
        var method = mem.Name.Identifier.Text;
        var inner = TypeRegistry.GetListInnerType(listType) ?? "int";
        bool isUserClass = !TypeRegistry.IsPrimitive(inner) && inner != "string";

        // For user-class lists: free elements before Clear / on RemoveAt
        if (method == "Clear" && isUserClass)
        {
            var idxVar = ctx.NextTmp("fi");
            var cInner = TypeRegistry.MapType(inner);
            ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listExpr}->count; {idxVar}++)");
            ctx.WriteLine($"    if ({listExpr}->data[{idxVar}]) {cInner}_Free({listExpr}->data[{idxVar}]);");
            result = cList + "_Clear(" + listExpr + ")";
            return true;
        }

        if (method == "RemoveAt" && isUserClass && args.Count > 0)
        {
            var cInner = TypeRegistry.MapType(inner);
            ctx.WriteLine($"if ({args[0]} >= 0 && {args[0]} < {listExpr}->count && {listExpr}->data[{args[0]}])");
            ctx.WriteLine($"    {cInner}_Free({listExpr}->data[{args[0]}]);");
            result = cList + "_Remove(" + listExpr + ", " + args[0] + ")";
            return true;
        }

        if (method == "ForEach")
        {
            // list.ForEach(action) → for loop calling action on each element
            var cInner = inner == "string" ? "const char*" : TypeRegistry.MapType(inner);
            var actionExpr = args.Count > 0 ? args[0] : "NULL /* ForEach: missing action */";
            var iVar = ctx.NextTmp("fe_i");
            ctx.WriteLine($"for (int {iVar} = 0; {iVar} < {listExpr}->count; {iVar}++)");
            ctx.WriteLine($"    {actionExpr}({listExpr}->data[{iVar}]);");
            result = "";
            return true;
        }

        if (method is "FindAll" or "Find" or "Exists" or "TrueForAll" or "ConvertAll")
        {
            if (inv.ArgumentList.Arguments.Count == 0)
            {
                result = listExpr;
                return true;
            }

            var lambdaNode = inv.ArgumentList.Arguments[0].Expression as LambdaExpressionSyntax;
            if (lambdaNode == null) { result = listExpr; return true; }

            var lifter = new LambdaLifter(ctx, new ExpressionWriter(ctx));
            lifter.SetStatementWriter(new StatementWriter(ctx, new ExpressionWriter(ctx)));
            var predFn = lifter.LiftLambda(lambdaNode, elementTypeHint: inner);

            var cInnerType = inner == "string" ? "const char*" : TypeRegistry.MapType(inner);
            var isPrim = TypeRegistry.IsPrimitive(inner);
            var elemPtr = (isPrim || inner == "string") ? "" : "*";
            var idxVar = ctx.NextTmp("li");

            if (method == "Exists")
            {
                var retVar = ctx.NextTmp("ex");
                ctx.WriteLine($"int {retVar} = 0;");
                ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listExpr}->count; {idxVar}++)");
                ctx.WriteLine($"    if ({predFn}({listExpr}->data[{idxVar}])) {{ {retVar} = 1; break; }}");
                result = retVar;
                return true;
            }

            if (method == "TrueForAll")
            {
                var retVar = ctx.NextTmp("tfa");
                ctx.WriteLine($"int {retVar} = 1;");
                ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listExpr}->count; {idxVar}++)");
                ctx.WriteLine($"    if (!{predFn}({listExpr}->data[{idxVar}])) {{ {retVar} = 0; break; }}");
                result = retVar;
                return true;
            }

            if (method == "Find")
            {
                var retVar = ctx.NextTmp("find");
                var fillLine = isPrim || inner == "string"
                    ? $"{cInnerType} {retVar} = 0;"
                    : $"{cInnerType}* {retVar} = NULL;";
                ctx.WriteLine(fillLine);
                ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listExpr}->count; {idxVar}++)");
                ctx.WriteLine($"    if ({predFn}({listExpr}->data[{idxVar}])) {{ {retVar} = {listExpr}->data[{idxVar}]; break; }}");
                result = retVar;
                return true;
            }

            if (method == "FindAll")
            {
                var outVar = ctx.NextTmp("fa");
                ctx.WriteLine($"List_{cList[5..]}* {outVar} = {cList}_New();");
                ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listExpr}->count; {idxVar}++)");
                ctx.WriteLine($"    if ({predFn}({listExpr}->data[{idxVar}])) {cList}_Add({outVar}, {listExpr}->data[{idxVar}]);");
                ctx.LocalTypes[outVar] = "List<" + inner + ">";
                result = outVar;
                return true;
            }

            if (method == "ConvertAll")
            {
                // ConvertAll(x => expr) — Ergebnis-Typ aus Lambda inferieren
                string projInner = "int";
                if (lambdaNode is SimpleLambdaExpressionSyntax sl)
                    projInner = TypeInferrer.InferCSharpType(sl.Body, ctx);
                else if (lambdaNode is ParenthesizedLambdaExpressionSyntax pl)
                    projInner = TypeInferrer.InferCSharpType(pl.Body, ctx);
                var cProj = projInner == "string" ? "str" : TypeRegistry.MapType(projInner);
                var projLifter = new LambdaLifter(ctx, new ExpressionWriter(ctx));
                projLifter.SetStatementWriter(new StatementWriter(ctx, new ExpressionWriter(ctx)));
                var projFn = projLifter.LiftLambda(lambdaNode, elementTypeHint: inner);
                var outVar = ctx.NextTmp("ca");
                ctx.WriteLine($"List_{cProj}* {outVar} = List_{cProj}_New();");
                ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < {listExpr}->count; {idxVar}++)");
                ctx.WriteLine($"    List_{cProj}_Add({outVar}, {projFn}({listExpr}->data[{idxVar}]));");
                ctx.LocalTypes[outVar] = "List<" + projInner + ">";
                result = outVar;
                return true;
            }
        }

        if (method == "Sort" && inv.ArgumentList.Arguments.Count > 0)
        {
            // args[0] is already the transpiled comparer expression.
            // If the argument was a lambda, InvocationDispatcher already lifted it via
            // writeExpr → ExpressionWriter.WriteLambda → LambdaLifter.LiftLambda.
            // We must NOT re-lift here or the same lambda ends up in PendingLambdaPreludes
            // twice, emitting duplicate struct/function definitions in the C output.
            result = BuildCustomSort(listExpr, listType, args[0], ctx);
            return true;
        }

        result = method switch
        {
            "Add" => cList + "_Add(" + listExpr + ", " + JoinArgs(args) + ")",
            "Clear" => cList + "_Clear(" + listExpr + ")",
            "RemoveAt" => cList + "_Remove(" + listExpr + ", " + JoinArgs(args) + ")",
            "Remove" => cList + "_RemoveValue(" + listExpr + ", " + JoinArgs(args) + ")",
            "Contains" => cList + "_Contains(" + listExpr + ", " + JoinArgs(args) + ")",
            "Insert" => cList + "_Insert(" + listExpr + ", " + JoinArgs(args) + ")",
            "Sort" => cList + "_Sort(" + listExpr + ")",
            "Reverse" => cList + "_Reverse(" + listExpr + ")",
            "IndexOf" => cList + "_IndexOf(" + listExpr + ", " + JoinArgs(args) + ")",
            _ => listExpr + "->" + method + "(" + JoinArgs(args) + ")",
        };
        return true;
    }

    private static string BuildCustomSort(
        string listExpr, string listType,
        string comparerExpr, TranspilerContext ctx)
    {
        var inner = TypeRegistry.GetListInnerType(listType) ?? "int";
        var cInner = inner == "string" ? "const char*" : TypeRegistry.MapType(inner);

        // comparerExpr is already the correct C expression:
        //   - lambda arg  → lifted function name (e.g. "_lambda_3"), produced by
        //                   InvocationDispatcher → writeExpr → WriteLambda → LiftLambda
        //   - method-ref  → the transpiled identifier (e.g. "MyClass_Compare")
        // Re-lifting here would add a second identical prelude and capture-init block.
        string resolvedComparer = comparerExpr;

        var idxI = ctx.NextTmp("si");
        var idxJ = ctx.NextTmp("sj");
        var tmpVar = ctx.NextTmp("stmp");

        ctx.WriteLine($"for (int {idxI} = 1; {idxI} < {listExpr}->count; {idxI}++)");
        ctx.WriteLine("{");
        ctx.Indent();
        ctx.WriteLine($"{cInner} {tmpVar} = {listExpr}->data[{idxI}];");
        ctx.WriteLine($"int {idxJ} = {idxI} - 1;");
        ctx.WriteLine($"while ({idxJ} >= 0 && {resolvedComparer}({listExpr}->data[{idxJ}], {tmpVar}) > 0)");
        ctx.WriteLine("{");
        ctx.Indent();
        ctx.WriteLine($"{listExpr}->data[{idxJ}+1] = {listExpr}->data[{idxJ}];");
        ctx.WriteLine($"{idxJ}--;");
        ctx.Dedent();
        ctx.WriteLine("}");
        ctx.WriteLine($"{listExpr}->data[{idxJ}+1] = {tmpVar};");
        ctx.Dedent();
        ctx.WriteLine("}");

        return "/* sorted */";
    }
}