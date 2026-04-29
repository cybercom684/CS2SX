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
        "Sort", "IndexOf", "Reverse",
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
            return NotHandled(out result);

        var cList = ListFuncPrefix(listType);
        var method = mem.Name.Identifier.Text;

        if (method == "Sort" && inv.ArgumentList.Arguments.Count > 0)
        {
            LambdaExpressionSyntax? lambdaNode = null;
            if (inv.ArgumentList.Arguments[0].Expression is LambdaExpressionSyntax lam)
                lambdaNode = lam;

            result = BuildCustomSort(listExpr, listType, args[0], ctx, lambdaNode, writeExpr);
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
        string comparerExpr, TranspilerContext ctx,
        LambdaExpressionSyntax? lambdaNode,
        Func<SyntaxNode?, string> writeExpr)
    {
        var inner = TypeRegistry.GetListInnerType(listType) ?? "int";
        var cInner = inner == "string" ? "const char*" : TypeRegistry.MapType(inner);

        string resolvedComparer = comparerExpr;

        if (lambdaNode != null)
        {
            var adapter = new ExpressionWriter(ctx);
            var lifter = new LambdaLifter(ctx, adapter);
            lifter.SetStatementWriter(new StatementWriter(ctx, adapter));

            // FIX: LiftLambda() schreibt das Prelude (Struct-Def + Hilfsfunktion)
            // direkt in ctx.PendingLambdaPreludes. Es gibt kein HasPrelude /
            // ConsumePrelude mehr. Der Flush passiert automatisch in
            // CSharpToC.VisitMethodDeclaration() vor der nächsten Methodensignatur.
            resolvedComparer = lifter.LiftLambda(lambdaNode,
                hintType: null,
                elementTypeHint: inner);
        }

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