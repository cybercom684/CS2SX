using System.Text;
using CS2SX.Transpiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Build;

/// Forms of async code the state machines (AsyncLowering) do not handle themselves,
/// rewritten into forms they do — at the C# level, right before AsyncLowering:
///
///   async lambda          → a private async method of the class, called by a plain lambda
///                           (captured variables are passed; mutated ones are already
///                           shared boxes, see SyntaxLowering)
///   await in catch        → one catch that keeps the exception, then if/else on its type
///   await in finally      → catch everything, run the block, throw again
///   using (x) { await }   → try / finally { x?.Dispose(); }
///   do … while (await c)  → while (true) { … if (!(await c)) break; }
///   for (…; await c; …)   → { init; while (true) { if (!(c)) break; … step; } }
///
/// Every edit keeps the number of lines (diagnostics and stack traces keep pointing at
/// the user's lines); the body of an async lambda moves to the end of its class.
/// What cannot be rewritten safely (a `return` that would skip the moved finally, a
/// `continue` in a rewritten loop) is left alone and reported by AsyncLowering.Problems.
public static class AsyncDesugar
{
    private sealed record Edit(int Start, int Length, string Text, int Seq);

    public static List<string> Rewrite(IReadOnlyList<string> files, string outDir)
    {
        static bool Candidate(SyntaxNode n) => n is AwaitExpressionSyntax
            || n is AnonymousFunctionExpressionSyntax { AsyncKeyword.RawKind: not 0 };
        if (!files.Any(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f)).GetRoot().DescendantNodes().Any(Candidate)))
            return files.ToList();

        // Pass 1: async lambdas become methods. Pass 2: statements inside async methods
        // (including the new ones), on the re-parsed result.
        var current = Pass(files, outDir, LiftLambdas);
        return Pass(current, outDir, RewriteStatements);
    }

    private static List<string> Pass(IReadOnlyList<string> files, string outDir,
        Func<SyntaxNode, SemanticModel, List<Edit>, int> step)
    {
        var sem = new SemanticModelBuilder(files);
        var result = new List<string>();
        var dir = Path.Combine(outDir, "_cs");
        Directory.CreateDirectory(dir);
        foreach (var file in files)
        {
            if (sem.GetModel(file) is not { } model) { result.Add(file); continue; }
            var edits = new List<Edit>();
            if (step(model.SyntaxTree.GetRoot(), model, edits) == 0 || edits.Count == 0) { result.Add(file); continue; }
            var text = model.SyntaxTree.GetText().ToString();
            foreach (var e in edits.OrderByDescending(e => e.Start).ThenByDescending(e => e.Seq))
                text = text.Remove(e.Start, e.Length).Insert(e.Start, e.Text);
            var target = Path.Combine(dir, Path.GetFileName(file));
            if (!File.Exists(target) || File.ReadAllText(target) != text)
                File.WriteAllText(target, text);
            result.Add(target);
        }
        return result;
    }

    private static int s_seq;
    private static void Add(List<Edit> edits, int start, int length, string text) =>
        edits.Add(new Edit(start, length, text, s_seq++));
    private static void Insert(List<Edit> edits, int pos, string text) => Add(edits, pos, 0, text);

    /// The text of a node as one line (tokens without trivia; comments dropped).
    private static string OneLine(SyntaxNode node)
    {
        var sb = new StringBuilder();
        SyntaxToken prev = default;
        foreach (var t in node.DescendantTokens())
        {
            if (sb.Length > 0 && NeedsSpace(prev, t)) sb.Append(' ');
            sb.Append(t.Text.Replace("\r", "").Replace("\n", " "));
            prev = t;
        }
        return sb.ToString();
    }

    private static bool NeedsSpace(SyntaxToken a, SyntaxToken b) =>
        (char.IsLetterOrDigit(a.Text[^1]) || a.Text[^1] == '_' || a.Text[^1] == '"')
        && (char.IsLetterOrDigit(b.Text[0]) || b.Text[0] == '_' || b.Text[0] == '@' || b.Text[0] == '$' || b.Text[0] == '"')
        || a.IsKind(SyntaxKind.SemicolonToken) || a.IsKind(SyntaxKind.CommaToken)
        || a.IsKind(SyntaxKind.OpenBraceToken) || b.IsKind(SyntaxKind.CloseBraceToken);

    /// <paramref name="text"/> followed by as many line breaks as <paramref name="span"/>
    /// spans, so the lines after it stay where they were.
    private static string KeepLines(SyntaxNode node, string text)
    {
        var lines = node.GetLocation().GetLineSpan();
        int n = lines.EndLinePosition.Line - lines.StartLinePosition.Line;
        return text + new string('\n', Math.Max(0, n - text.Count(c => c == '\n')));
    }

    // ── Pass 1: async lambdas ─────────────────────────────────────────────────

    private static int LiftLambdas(SyntaxNode root, SemanticModel model, List<Edit> edits)
    {
        int count = 0;
        var perClass = new Dictionary<ClassDeclarationSyntax, StringBuilder>();
        foreach (var lambda in root.DescendantNodes().OfType<AnonymousFunctionExpressionSyntax>())
        {
            if (lambda.AsyncKeyword.RawKind == 0) continue;
            if (lambda.Ancestors().OfType<AnonymousFunctionExpressionSyntax>().Any()) continue;   // one level
            if (lambda.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault() is not { TypeParameterList: null } cls) continue;
            if (model.GetTypeInfo(lambda).ConvertedType is not INamedTypeSymbol { DelegateInvokeMethod: { } invoke }) continue;

            var member = lambda.Ancestors().OfType<MemberDeclarationSyntax>().FirstOrDefault(m => m.Parent == cls);
            if (member is MethodDeclarationSyntax { TypeParameterList: not null }) continue;   // (generic method)
            bool isStatic = member switch
            {
                BaseMethodDeclarationSyntax bm => bm.Modifiers.Any(SyntaxKind.StaticKeyword),
                BasePropertyDeclarationSyntax bp => bp.Modifiers.Any(SyntaxKind.StaticKeyword),
                BaseFieldDeclarationSyntax bf => bf.Modifiers.Any(SyntaxKind.StaticKeyword),
                _ => false,
            };
            if (member is BaseFieldDeclarationSyntax && !isStatic) continue;   // (field initializers cannot use this)

            // Parameters of the lambda (names as written, types from the delegate).
            var names = lambda switch
            {
                SimpleLambdaExpressionSyntax s => new List<string> { s.Parameter.Identifier.Text },
                ParenthesizedLambdaExpressionSyntax p => p.ParameterList.Parameters.Select(x => x.Identifier.Text).ToList(),
                AnonymousMethodExpressionSyntax a when a.ParameterList != null => a.ParameterList.Parameters.Select(x => x.Identifier.Text).ToList(),
                _ => invoke.Parameters.Select((_, i) => "__p" + i).ToList(),
            };
            if (names.Count != invoke.Parameters.Length || invoke.Parameters.Any(p => p.RefKind != RefKind.None)) continue;

            // Variables of the enclosing method that the lambda uses: passed along.
            var captured = new List<(string name, ITypeSymbol type)>();
            bool ok = true;
            foreach (var id in lambda.Body.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
            {
                var sym = model.GetSymbolInfo(id).Symbol;
                if (sym is not (ILocalSymbol or IParameterSymbol)) continue;
                var decl = sym.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                if (decl == null || lambda.Span.Contains(decl.Span)) continue;
                if (IsWritten(id)) { ok = false; break; }
                var type = sym is ILocalSymbol l ? l.Type : ((IParameterSymbol)sym).Type;
                if (captured.All(c => c.name != sym.Name)) captured.Add((sym.Name, type));
            }
            if (!ok) continue;

            // (Named after the line: a character offset differs between LF and CRLF files.)
            var name = "__AsyncLambda" + (++count) + "_L" + (lambda.GetLocation().GetLineSpan().StartLinePosition.Line + 1);
            var ret = invoke.ReturnsVoid ? "void" : invoke.ReturnType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
            var ps = invoke.Parameters.Select((p, i) => Display(p.Type) + " " + names[i])
                .Concat(captured.Select(c => Display(c.type) + " " + c.name));
            var body = lambda.Body is BlockSyntax block ? OneLine(block) : "=> " + OneLine(lambda.Body) + ";";
            var method = $" private {(isStatic ? "static " : "")}async {ret} {name}({string.Join(", ", ps)}) {body}";
            if (!perClass.TryGetValue(cls, out var sb)) perClass[cls] = sb = new StringBuilder();
            sb.Append(method);

            var call = name + "(" + string.Join(", ", names.Concat(captured.Select(c => c.name))) + ")";
            var head = lambda switch
            {
                SimpleLambdaExpressionSyntax s => s.Parameter.Identifier.Text,
                ParenthesizedLambdaExpressionSyntax p => p.ParameterList.ToString(),
                _ => "(" + string.Join(", ", names) + ")",
            };
            Add(edits, lambda.SpanStart, lambda.Span.Length, KeepLines(lambda, head + " => " + call));
        }
        foreach (var (cls, sb) in perClass)
            Insert(edits, cls.CloseBraceToken.SpanStart, sb.ToString() + " ");
        return count;
    }

    private static string Display(ITypeSymbol t) => t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

    private static bool IsWritten(ExpressionSyntax id)
    {
        SyntaxNode node = id;
        while (node.Parent is ParenthesizedExpressionSyntax) node = node.Parent;
        return node.Parent switch
        {
            AssignmentExpressionSyntax a => a.Left == node,
            PrefixUnaryExpressionSyntax p => p.IsKind(SyntaxKind.PreIncrementExpression) || p.IsKind(SyntaxKind.PreDecrementExpression),
            PostfixUnaryExpressionSyntax p => p.IsKind(SyntaxKind.PostIncrementExpression) || p.IsKind(SyntaxKind.PostDecrementExpression),
            ArgumentSyntax arg => arg.RefKindKeyword.RawKind != 0,
            _ => false,
        };
    }

    // ── Pass 2: statements inside async methods ──────────────────────────────

    private static int RewriteStatements(SyntaxNode root, SemanticModel model, List<Edit> edits)
    {
        int count = 0, n = 0;
        foreach (var m in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
        {
            if (!m.Modifiers.Any(SyntaxKind.AsyncKeyword) || m.Body == null) continue;
            bool HasAwait(SyntaxNode? s) => s != null && s.DescendantNodesAndSelf().OfType<AwaitExpressionSyntax>()
                .Any(a => !a.Ancestors().TakeWhile(x => x != s).Any(x => x is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax));

            foreach (var stmt in m.Body.DescendantNodes().OfType<StatementSyntax>())
            {
                if (stmt.Ancestors().TakeWhile(x => x != m).Any(x => x is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax))
                    continue;
                // a && await b, a || await b, a ?? await b, c ? await x : y — in a
                // statement that only consists of it (if, return, declaration,
                // assignment): evaluated into a variable first, keeping short-circuit.
                if (stmt is IfStatementSyntax or ReturnStatementSyntax or LocalDeclarationStatementSyntax or ExpressionStatementSyntax
                    && ConditionalAwait(stmt, model, HasAwait) is { } condExpr)
                {
                    var tmp = "__aw" + (++n);
                    var type = model.GetTypeInfo(condExpr).ConvertedType ?? model.GetTypeInfo(condExpr).Type;
                    if (type != null && type is not IErrorTypeSymbol)
                    {
                        var t = Display(type);
                        string pre = condExpr switch
                        {
                            BinaryExpressionSyntax b when b.IsKind(SyntaxKind.LogicalAndExpression)
                                => $"{t} {tmp} = {OneLine(b.Left)}; if ({tmp}) {tmp} = {OneLine(b.Right)}; ",
                            BinaryExpressionSyntax b when b.IsKind(SyntaxKind.LogicalOrExpression)
                                => $"{t} {tmp} = {OneLine(b.Left)}; if (!{tmp}) {tmp} = {OneLine(b.Right)}; ",
                            BinaryExpressionSyntax b
                                => $"{t} {tmp} = {OneLine(b.Left)}; if ({tmp} == null) {tmp} = {OneLine(b.Right)}; ",
                            ConditionalExpressionSyntax c
                                => $"{t} {tmp}; if ({OneLine(c.Condition)}) {tmp} = {OneLine(c.WhenTrue)}; else {tmp} = {OneLine(c.WhenFalse)}; ",
                            _ => "",
                        };
                        Insert(edits, stmt.SpanStart, pre);
                        Add(edits, condExpr.SpanStart, condExpr.Span.Length, KeepLines(condExpr, tmp));
                        count++;
                    }
                }

                switch (stmt)
                {
                    case TryStatementSyntax t when t.Catches.Any(c => HasAwait(c.Block)) || HasAwait(t.Finally?.Block):
                        if (RewriteTry(t, ++n, edits, HasAwait)) count++;
                        break;
                    case UsingStatementSyntax u when HasAwait(u.Statement) && u.Statement is BlockSyntax:
                        RewriteUsing(u, ++n, edits); count++;
                        break;
                    case DoStatementSyntax d when HasAwait(d.Condition) && !HasContinue(d.Statement):
                        Add(edits, d.DoKeyword.SpanStart, d.DoKeyword.Span.Length, "while (true) {");
                        Add(edits, d.WhileKeyword.SpanStart, d.SemicolonToken.Span.End - d.WhileKeyword.SpanStart,
                            "if (!(" + OneLine(d.Condition) + ")) break; }" + new string('\n', LinesOf(d.WhileKeyword, d.SemicolonToken)));
                        count++;
                        break;
                    case ForStatementSyntax f when (HasAwait(f.Condition) || f.Incrementors.Any(HasAwait) || f.Initializers.Any(HasAwait)
                                                    || HasAwait(f.Declaration))
                                                   && (f.Incrementors.Count == 0 || !HasContinue(f.Statement)):
                        var init = f.Declaration != null ? OneLine(f.Declaration) + ";"
                                 : string.Concat(f.Initializers.Select(i => OneLine(i) + ";"));
                        var cond = f.Condition != null ? "if (!(" + OneLine(f.Condition) + ")) break; " : "";
                        Add(edits, f.ForKeyword.SpanStart, f.CloseParenToken.Span.End - f.ForKeyword.SpanStart,
                            "{ " + init + " while (true) { " + cond + new string('\n', LinesOf(f.ForKeyword, f.CloseParenToken)));
                        Insert(edits, f.Statement.Span.End, " " + string.Concat(f.Incrementors.Select(i => OneLine(i) + "; ")) + "} }");
                        count++;
                        break;
                }
            }
        }
        return count;
    }

    /// The &&, ||, ?? or ?: expression of the statement whose right side or branch
    /// awaits, when the statement is just that expression: if (…), return …;,
    /// T x = …;, x = …; — null otherwise (and when there is more than one).
    private static ExpressionSyntax? ConditionalAwait(StatementSyntax stmt, SemanticModel model, Func<SyntaxNode?, bool> hasAwait)
    {
        ExpressionSyntax? e = stmt switch
        {
            IfStatementSyntax i => i.Condition,
            ReturnStatementSyntax r => r.Expression,
            LocalDeclarationStatementSyntax { Declaration.Variables: [var v] } => v.Initializer?.Value,
            ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax { RawKind: (int)SyntaxKind.SimpleAssignmentExpression } a } => a.Right,
            _ => null,
        };
        while (e is ParenthesizedExpressionSyntax p) e = p.Expression;
        return e switch
        {
            BinaryExpressionSyntax b when (b.IsKind(SyntaxKind.LogicalAndExpression) || b.IsKind(SyntaxKind.LogicalOrExpression)
                                           || b.IsKind(SyntaxKind.CoalesceExpression))
                                          && hasAwait(b.Right) && !hasAwait(b.Left) => b,
            ConditionalExpressionSyntax c when (hasAwait(c.WhenTrue) || hasAwait(c.WhenFalse)) && !hasAwait(c.Condition) => c,
            _ => null,
        };
    }

    private static int LinesOf(SyntaxToken from, SyntaxToken to) =>
        to.GetLocation().GetLineSpan().EndLinePosition.Line - from.GetLocation().GetLineSpan().StartLinePosition.Line;

    /// A `continue` of this loop (not of a loop inside it).
    private static bool HasContinue(StatementSyntax body) =>
        body.DescendantNodesAndSelf().OfType<ContinueStatementSyntax>().Any(c =>
            !c.Ancestors().TakeWhile(x => x != body).Any(x => x is ForStatementSyntax or ForEachStatementSyntax
                or WhileStatementSyntax or DoStatementSyntax or AnonymousFunctionExpressionSyntax));

    /// A statement that leaves the region (return; break/continue of an outer loop; goto).
    private static bool JumpsOut(SyntaxNode region) =>
        region.DescendantNodes().Any(x => x switch
        {
            ReturnStatementSyntax or GotoStatementSyntax => !InLambda(x, region),
            BreakStatementSyntax or ContinueStatementSyntax => !x.Ancestors().TakeWhile(a => a != region)
                .Any(a => a is ForStatementSyntax or ForEachStatementSyntax or WhileStatementSyntax or DoStatementSyntax
                          or SwitchStatementSyntax or AnonymousFunctionExpressionSyntax),
            _ => false,
        });

    private static bool InLambda(SyntaxNode x, SyntaxNode region) =>
        x.Ancestors().TakeWhile(a => a != region).Any(a => a is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax);

    private static bool RewriteTry(TryStatementSyntax t, int n, List<Edit> edits, Func<SyntaxNode?, bool> hasAwait)
    {
        bool catchAwait = t.Catches.Any(c => hasAwait(c.Block));
        bool finallyAwait = t.Finally != null && hasAwait(t.Finally.Block);
        // Moving the finally behind the try is only right when nothing jumps past it.
        if (finallyAwait && (JumpsOut(t.Block) || t.Catches.Any(c => JumpsOut(c.Block)))) return false;
        // (Exception filters stay where they are: not rewritten.)
        if (catchAwait && t.Catches.Any(c => c.Filter != null)) return false;

        var ex = "__ex" + n;
        var fin = "__fin" + n;
        var prefix = new StringBuilder();
        if (finallyAwait) prefix.Append($"Exception? {fin} = null; try {{ ");
        if (catchAwait) prefix.Append($"Exception? {ex} = null; ");
        Insert(edits, t.TryKeyword.SpanStart, prefix.ToString());

        int endOfHandlers = t.Catches.Count > 0 ? t.Catches[^1].Span.End : t.Block.Span.End;
        if (catchAwait)
        {
            // One catch keeps the exception; the handlers become if / else if on its type.
            bool catchesAll = false;
            for (int i = 0; i < t.Catches.Count; i++)
            {
                var c = t.Catches[i];
                var type = c.Declaration?.Type.ToString() ?? "Exception";
                var name = c.Declaration?.Identifier.Text;
                if (string.IsNullOrEmpty(name)) name = null;
                var test = type is "Exception" or "System.Exception" ? $"{ex} != null" : $"{ex} is {type}";
                if (test == $"{ex} != null") catchesAll = true;
                var head = (i == 0 ? $"catch (Exception {ex}__c) {{ {ex} = {ex}__c; }} if" : "else if") + " (" + test + ")";
                Add(edits, c.CatchKeyword.SpanStart, c.Block.OpenBraceToken.SpanStart - c.CatchKeyword.SpanStart,
                    head + new string('\n', LinesOf(c.CatchKeyword, c.Block.OpenBraceToken)) + " ");
                if (name != null) Insert(edits, c.Block.OpenBraceToken.Span.End, $" var {name} = ({type}){ex}!;");
                foreach (var th in c.Block.DescendantNodes().OfType<ThrowStatementSyntax>())
                    if (th.Expression == null && !th.Ancestors().TakeWhile(x => x != c.Block).Any(x => x is CatchClauseSyntax))
                        Add(edits, th.SpanStart, th.Span.Length, $"throw {ex}!;");
                if (i == t.Catches.Count - 1 && !catchesAll)
                    Insert(edits, c.Block.Span.End, $" else if ({ex} != null) throw {ex};");
            }
        }
        if (finallyAwait)
        {
            Insert(edits, endOfHandlers, $" }} catch (Exception {fin}__c) {{ {fin} = {fin}__c; }}");
            Add(edits, t.Finally!.FinallyKeyword.SpanStart, t.Finally.FinallyKeyword.Span.Length, "");
            Insert(edits, t.Finally.Block.Span.End, $" if ({fin} != null) throw {fin};");
        }
        return true;
    }

    private static void RewriteUsing(UsingStatementSyntax u, int n, List<Edit> edits)
    {
        string var;
        string head;
        if (u.Declaration != null)
        {
            var = u.Declaration.Variables[0].Identifier.Text;
            head = OneLine(u.Declaration) + ";";
        }
        else
        {
            var = "__using" + n;
            head = "var " + var + " = " + OneLine(u.Expression!) + ";";
        }
        Add(edits, u.UsingKeyword.SpanStart, u.CloseParenToken.Span.End - u.UsingKeyword.SpanStart,
            "{ " + head + " try" + new string('\n', LinesOf(u.UsingKeyword, u.CloseParenToken)));
        Insert(edits, u.Statement.Span.End, $" finally {{ {var}?.Dispose(); }} }}");
    }
}
