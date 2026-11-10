using System.Text;
using System.Text.RegularExpressions;
using CS2SX.Logging;
using CS2SX.Transpiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Build;

/// async/await, lowered at the C# level (after the generics are specialised):
///
///   async Task&lt;int&gt; Load(int id) { var raw = await Fetch(id); return raw.Length; }
///
/// becomes a method that creates a state machine object and starts it, and a nested
/// class holding the method's parameters and locals as fields:
///
///   CS2SX_Task_int Load(int id) { var sm = new Load_Async(); …; sm.cs2sx_resume(); return sm.cs2sx_task; }
///   class Load_Async : CS2SX_Step {
///       void cs2sx_run() { CS2SX_Async.Begin(1);
///           { cs2sx_a1 = cs2sx_this.Fetch(id); if (!cs2sx_a1.IsCompleted) { cs2sx_a1.Then(this); CS2SX_Async.Suspend(1); } raw = cs2sx_a1.Result; }
///           { cs2sx_task.SetResult(raw.Length); return; } } }
///
/// The body keeps its control flow: CS2SX_Async.Suspend(n) returns, and the next
/// cs2sx_resume() jumps back behind it (the transpiler emits a label and a goto,
/// which C allows into loops and blocks). Everything stays on the main thread; a
/// task that is not finished continues in a later frame (CS2SX_Task.Tick).
///
/// Task / Task&lt;T&gt; become CS2SX_Task / CS2SX_Task_T, small classes appended to the
/// first source file. Every replacement keeps the number of lines.
public static class AsyncLowering
{
    private sealed record Edit(int Start, int Length, string Text, int Prio, int Seq, bool StatementLevel);

    private sealed class Edits
    {
        public readonly List<Edit> List = new();
        private int _seq;
        public void Replace(int start, int length, string text) =>
            List.Add(new Edit(start, length, text, 4, _seq++, false));
        public void Insert(int pos, string text, int prio, bool statementLevel = true) =>
            List.Add(new Edit(pos, 0, text, prio, _seq++, statementLevel));

        /// The source between from and to with the edits inside applied. An edit
        /// inside a replaced span is dropped (the replacement already contains it).
        public string Render(string source, int from, int to, bool all)
        {
            var sb = new StringBuilder();
            int cursor = from;
            foreach (var e in List
                .Where(e => e.Start >= from && e.Start + e.Length <= to
                            && (all || (!e.StatementLevel && !(e.Length == 0 && e.Start == to))))
                .OrderBy(e => e.Start).ThenBy(e => e.Length == 0 ? 0 : 1).ThenBy(e => e.Prio).ThenBy(e => e.Seq))
            {
                if (e.Start < cursor) continue;
                sb.Append(source, cursor, e.Start - cursor).Append(e.Text);
                cursor = e.Start + e.Length;
            }
            return sb.Append(source, cursor, to - cursor).ToString();
        }
    }

    // Insert order at one position: closing braces first, then what opens.
    private const int PrioAwaitClose = 0, PrioLoopClose = 1, PrioLoopOpen = 2, PrioAwaitOpen = 3, PrioResult = 4, PrioMethod = 5, PrioName = 6;

    public static List<string> Rewrite(IReadOnlyList<string> files, string outDir)
    {
        static bool Candidate(SyntaxNode n) => n is AwaitExpressionSyntax
            || n is MethodDeclarationSyntax m && m.Modifiers.Any(SyntaxKind.AsyncKeyword)
            || n is IdentifierNameSyntax { Identifier.Text: "Task" or "ValueTask" }
            || n is GenericNameSyntax { Identifier.Text: "Task" or "ValueTask" };
        if (!files.Any(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f)).GetRoot().DescendantNodes().Any(Candidate)))
            return files.ToList();

        var sem = new SemanticModelBuilder(files);
        var edits = files.ToDictionary(f => f, _ => new Edits());
        var taskTypes = new SortedDictionary<string, string>(StringComparer.Ordinal);   // CS2SX_Task_int → int
        int machines = 0;
        bool any = false;

        foreach (var file in files)
        {
            if (sem.GetModel(file) is not { } model) continue;
            var root = model.SyntaxTree.GetRoot();
            var source = model.SyntaxTree.GetText().ToString();
            var e = edits[file];

            any |= RenameTaskTypes(root, model, e, taskTypes);

            foreach (var m in root.DescendantNodes().OfType<MethodDeclarationSyntax>())
            {
                if (!m.Modifiers.Any(SyntaxKind.AsyncKeyword) || (m.Body == null && m.ExpressionBody == null)) continue;
                if (MethodProblems(m, model).Any()) continue;   // reported by UnsupportedSyntax
                LowerMethod(m, model, source, e, taskTypes);
                machines++;
                any = true;
            }
        }
        if (!any) return files.ToList();

        // The task classes go in front of the first type of the first file — base
        // classes have to come before the state machines — on that type's line, so
        // no line moves.
        {
            var first = files[0];
            var root = CSharpSyntaxTree.ParseText(File.ReadAllText(first)).GetCompilationUnitRoot();
            int membersAt = root.Members.FirstOrDefault()?.SpanStart ?? root.FullSpan.End;
            int usingsAt = root.Usings.FirstOrDefault()?.SpanStart ?? membersAt;
            var runtime = string.Join(" ", RuntimeSource(taskTypes).Split('\n')
                .Select(l => l.Trim()).Where(l => l.Length > 0 && !l.StartsWith("//")));
            edits[first].Insert(usingsAt, "using System; using System.Collections.Generic; ", PrioAwaitClose);
            edits[first].Insert(membersAt, runtime + " ", PrioLoopClose);
        }

        var result = new List<string>();
        var dir = Path.Combine(outDir, "_cs");
        Directory.CreateDirectory(dir);
        foreach (var file in files)
        {
            var e = edits[file];
            if (e.List.Count == 0) { result.Add(file); continue; }
            var source = File.ReadAllText(file);
            var text = e.Render(source, 0, source.Length, all: true);
            var target = Path.Combine(dir, Path.GetFileName(file));
            if (!File.Exists(target) || File.ReadAllText(target) != text)
                File.WriteAllText(target, text);
            result.Add(target);
        }
        Log.Info($"async: {machines} state machine(s)");
        return result;
    }

    // ── Task types ──────────────────────────────────────────────────────────

    private static bool IsTaskLike(ITypeSymbol? t) =>
        t is INamedTypeSymbol n
        && n.ContainingNamespace?.ToDisplayString() == "System.Threading.Tasks"
        && n.Name is "Task" or "ValueTask";

    /// Also true for the classes Task types were replaced by: the problems of a method
    /// are looked for again in the lowered source.
    private static bool IsTaskOrLowered(ITypeSymbol? t) =>
        IsTaskLike(t) || t is INamedTypeSymbol { Name: "CS2SX_Task" } || t?.Name.StartsWith("CS2SX_Task_") == true;

    /// C# text of a type, with Task types replaced by their CS2SX classes.
    private static string TypeText(ITypeSymbol t, IDictionary<string, string> taskTypes)
    {
        if (IsTaskLike(t))
        {
            var n = (INamedTypeSymbol)t;
            if (n.TypeArguments.Length == 0) return "CS2SX_Task";
            var arg = TypeText(n.TypeArguments[0], taskTypes);
            var name = "CS2SX_Task_" + Suffix(arg);
            taskTypes[name] = arg;
            return name;
        }
        if (t is IArrayTypeSymbol a) return TypeText(a.ElementType, taskTypes) + "[]";
        if (t is INamedTypeSymbol { IsGenericType: true, IsTupleType: false } g && g.TypeArguments.Any(ContainsTask)
            && g.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T)
            return g.Name + "<" + string.Join(", ", g.TypeArguments.Select(x => TypeText(x, taskTypes))) + ">";
        return t.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
    }

    private static bool ContainsTask(ITypeSymbol t) => t switch
    {
        _ when IsTaskLike(t) => true,
        IArrayTypeSymbol a => ContainsTask(a.ElementType),
        INamedTypeSymbol n => n.TypeArguments.Any(ContainsTask),
        _ => false,
    };

    private static string Suffix(string t) =>
        t == "string" ? "str" : Regex.Replace(t.Replace("[]", "Arr"), @"[^A-Za-z0-9]+", "_").Trim('_');

    private static bool RenameTaskTypes(SyntaxNode root, SemanticModel model, Edits e, IDictionary<string, string> taskTypes)
    {
        bool any = false;
        foreach (var name in root.DescendantNodes().OfType<SimpleNameSyntax>())
        {
            if (name.Identifier.Text is not ("Task" or "ValueTask")) continue;
            if (name.Ancestors().OfType<UsingDirectiveSyntax>().Any()) continue;
            if (model.GetSymbolInfo(name).Symbol is not INamedTypeSymbol type || !IsTaskLike(type)) continue;

            // System.Threading.Tasks.Task → the whole qualified name
            SyntaxNode span = name;
            if (name.Parent is QualifiedNameSyntax q && q.Right == name) span = q;
            else if (name.Parent is MemberAccessExpressionSyntax ma && ma.Name == name) span = ma;

            // Task.FromResult(x) / Task.Run(() => x) are members of the class of x's type.
            var called = span.Parent is MemberAccessExpressionSyntax { Parent: InvocationExpressionSyntax inv } call && call.Expression == span
                ? model.GetSymbolInfo(inv).Symbol as IMethodSymbol : null;
            if (called is { Name: "FromResult" or "Run", TypeArguments.Length: 1 })
            {
                var of = TypeText(called.ReturnType, taskTypes);
                e.Replace(span.Parent!.SpanStart, span.Parent.Span.Length, of + "." + (called.Name == "FromResult" ? "From" : "Run"));
            }
            // Task.WhenAll(list) — not the params form
            else if (called is { Name: "WhenAll", Parameters: [{ IsParams: false }] })
                e.Replace(span.Parent!.SpanStart, span.Parent.Span.Length, "CS2SX_Task.WhenAllList");
            else
                e.Replace(span.SpanStart, span.Span.Length, TypeText(type, taskTypes));
            any = true;
        }
        return any;
    }

    // ── What cannot be lowered ──────────────────────────────────────────────

    /// Everything in the tree that keeps an async method from being lowered, with
    /// the message for the user. A method with a problem is left as it is.
    public static IEnumerable<(SyntaxNode node, string message)> Problems(SyntaxNode root, SemanticModel model)
    {
        foreach (var node in root.DescendantNodes())
        {
            switch (node)
            {
                case AnonymousFunctionExpressionSyntax { AsyncKeyword.RawKind: not 0 }:
                case LocalFunctionStatementSyntax lf when lf.Modifiers.Any(SyntaxKind.AsyncKeyword):
                    yield return (node, "async-Lambdas und lokale async-Funktionen werden nicht unterstützt — eine async-Methode der Klasse verwenden.");
                    break;
                case MethodDeclarationSyntax m when m.Modifiers.Any(SyntaxKind.AsyncKeyword) && (m.Body != null || m.ExpressionBody != null):
                    foreach (var p in MethodProblems(m, model)) yield return p;
                    break;
            }
        }
    }

    private static bool InNestedFunction(SyntaxNode node, SyntaxNode method) =>
        node.Ancestors().TakeWhile(a => a != method)
            .Any(a => a is AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax);

    private static IEnumerable<AwaitExpressionSyntax> Awaits(SyntaxNode scope, SyntaxNode method) =>
        scope.DescendantNodes().OfType<AwaitExpressionSyntax>().Where(a => !InNestedFunction(a, method));

    private static ExpressionSyntax AwaitOperand(AwaitExpressionSyntax a) =>
        a.Expression is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "ConfigureAwait" } ca }
            ? ca.Expression : a.Expression;

    private static bool IsWhenAll(ExpressionSyntax operand, SemanticModel model) =>
        operand is InvocationExpressionSyntax inv
        && model.GetSymbolInfo(inv).Symbol is IMethodSymbol { Name: "WhenAll" } ms && IsTaskLike(ms.ContainingType);

    private static bool IsListOrArray(ITypeSymbol? t) =>
        t is IArrayTypeSymbol { Rank: 1 }
        || t is INamedTypeSymbol { Name: "List", TypeArguments.Length: 1 } n
           && n.ContainingNamespace?.ToDisplayString() == "System.Collections.Generic";

    private static IEnumerable<(SyntaxNode node, string message)> MethodProblems(MethodDeclarationSyntax m, SemanticModel model)
    {
        if (m.Parent is not ClassDeclarationSyntax cls)
        {
            yield return (m, $"Die async-Methode {m.Identifier.Text} steht nicht in einer Klasse — async geht nur in Klassen (nicht in Structs, Records oder Interfaces mit Rumpf).");
            yield break;
        }
        if (m.TypeParameterList != null || cls.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().Any(t => t.TypeParameterList != null))
        {
            yield return (m, $"Die async-Methode {m.Identifier.Text} ist generisch oder steht in einer generischen Klasse — das wird nicht unterstützt.");
            yield break;
        }
        var ret = model.GetDeclaredSymbol(m)?.ReturnType;
        if (ret == null || !(ret.SpecialType == SpecialType.System_Void || IsTaskOrLowered(ret)))
        {
            yield return (m, $"Die async-Methode {m.Identifier.Text} muss void, Task oder Task<T> zurückgeben.");
            yield break;
        }

        SyntaxNode body = (SyntaxNode?)m.Body ?? m.ExpressionBody!;
        var awaits = Awaits(body, m).ToList();

        foreach (var b in body.DescendantNodes().OfType<BaseExpressionSyntax>())
            yield return (b, "base. in einer async-Methode wird nicht unterstützt — den Aufruf in eine normale Methode auslagern.");

        foreach (var a in awaits)
        {
            string? why = null;
            var operand = AwaitOperand(a);
            var type = model.GetTypeInfo(operand).Type;
            bool isYield = operand is InvocationExpressionSyntax y
                && model.GetSymbolInfo(y).Symbol is IMethodSymbol { Name: "Yield" } ym && IsTaskOrLowered(ym.ContainingType);
            if (!IsTaskOrLowered(type) && !isYield)
                why = "await wird nur auf Task, Task<T> und Task.Yield() unterstützt.";
            else if (IsWhenAll(operand, model) && a.Parent is not ExpressionStatementSyntax)
                why = "Das Ergebnis von await Task.WhenAll(…) wird nicht unterstützt — die Tasks danach einzeln mit await oder .Result auslesen.";

            SyntaxNode child = a;
            foreach (var up in a.Ancestors().TakeWhile(x => x != m))
            {
                if (why != null) break;
                why = up switch
                {
                    TryStatementSyntax => "await in try/catch/finally wird nicht unterstützt — das await vor oder hinter den try-Block ziehen, oder den Block in eine eigene async-Methode auslagern und deren Task prüfen.",
                    UsingStatementSyntax or LockStatementSyntax => "await in using/lock wird nicht unterstützt.",
                    WhileStatementSyntax w when w.Condition == child => "await in einer Schleifenbedingung wird nicht unterstützt — vor der Schleife und am Ende des Rumpfs in eine Variable schreiben.",
                    DoStatementSyntax d when d.Condition == child => "await in einer Schleifenbedingung wird nicht unterstützt — am Ende des Rumpfs in eine Variable schreiben.",
                    ForStatementSyntax f when f.Statement != child => "await im Kopf einer for-Schleife wird nicht unterstützt.",
                    BinaryExpressionSyntax b when b.Right == child && (b.IsKind(SyntaxKind.LogicalAndExpression)
                        || b.IsKind(SyntaxKind.LogicalOrExpression) || b.IsKind(SyntaxKind.CoalesceExpression))
                        => "await rechts von &&, || oder ?? wird nicht unterstützt — das Ergebnis vorher in eine Variable schreiben.",
                    AssignmentExpressionSyntax asg when asg.Right == child && asg.IsKind(SyntaxKind.CoalesceAssignmentExpression)
                        => "await rechts von ??= wird nicht unterstützt — das Ergebnis vorher in eine Variable schreiben.",
                    ConditionalExpressionSyntax c when c.Condition != child => "await in einem Zweig von ?: wird nicht unterstützt — if/else verwenden.",
                    ConditionalAccessExpressionSyntax c when c.WhenNotNull == child => "await hinter ?. wird nicht unterstützt.",
                    SwitchExpressionArmSyntax => "await in einem switch-Ausdruck wird nicht unterstützt — switch-Anweisung oder if/else verwenden.",
                    _ => null,
                };
                child = up;
            }
            if (why != null) yield return (a, why);
        }

        foreach (var fe in body.DescendantNodes().OfType<CommonForEachStatementSyntax>())
        {
            if (InNestedFunction(fe, m)) continue;
            if (fe.AwaitKeyword.RawKind != 0)
                yield return (fe, "await foreach wird nicht unterstützt.");
            else if (Awaits(fe.Statement, m).Any()
                     && (fe is not ForEachStatementSyntax || !IsListOrArray(model.GetTypeInfo(fe.Expression).Type)))
                yield return (fe, "foreach mit await im Rumpf geht nur über eine List<T> oder ein Array mit einer einfachen Schleifenvariable — sonst eine for-Schleife verwenden.");
        }

        // Variables that stay C locals (out var, patterns, deconstruction, using var)
        // do not survive an await.
        const string lost = " behält ihren Wert nicht über ein await — vorher normal deklarieren (z. B. int n; … out n).";
        foreach (var d in body.DescendantNodes().OfType<SingleVariableDesignationSyntax>())
        {
            if (InNestedFunction(d, m)) continue;
            var scope = d.Ancestors().OfType<BlockSyntax>().FirstOrDefault() ?? body;
            if (awaits.Any(a => scope.Span.Contains(a.Span) && a.SpanStart > d.SpanStart))
                yield return (d, $"Die Variable {d.Identifier.Text} (out var, Muster oder Zerlegung)" + lost);
        }
        foreach (var ld in body.DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
        {
            if (InNestedFunction(ld, m) || !(ld.UsingKeyword.RawKind != 0 || ld.Declaration.Type is RefTypeSyntax)) continue;
            var scope = ld.Ancestors().OfType<BlockSyntax>().FirstOrDefault() ?? body;
            if (awaits.Any(a => scope.Span.Contains(a.Span) && a.SpanStart > ld.SpanStart))
                yield return (ld, "using var / ref-Variablen vor einem await werden nicht unterstützt.");
        }
    }

    // ── One async method ────────────────────────────────────────────────────

    private static bool IsHoistable(LocalDeclarationStatementSyntax ld) =>
        !ld.IsConst && ld.UsingKeyword.RawKind == 0 && ld.Declaration.Type is not RefTypeSyntax;

    private static void LowerMethod(MethodDeclarationSyntax m, SemanticModel model, string source, Edits e,
        IDictionary<string, string> taskTypes)
    {
        var symbol = model.GetDeclaredSymbol(m)!;
        var owner = symbol.ContainingType;
        SyntaxNode body = (SyntaxNode?)m.Body ?? m.ExpressionBody!;
        bool isStatic = symbol.IsStatic;
        bool returnsVoid = symbol.ReturnsVoid;
        var taskType = returnsVoid ? "CS2SX_Task" : TypeText(symbol.ReturnType, taskTypes);
        bool hasResult = !returnsVoid && ((INamedTypeSymbol)symbol.ReturnType).TypeArguments.Length == 1;

        // Name of the state machine class, unique in the owner.
        var smName = m.Identifier.Text + "_Async";
        {
            var siblings = ((ClassDeclarationSyntax)m.Parent!).Members.OfType<MethodDeclarationSyntax>()
                .Where(x => x.Identifier.Text == m.Identifier.Text && x.Modifiers.Any(SyntaxKind.AsyncKeyword)).ToList();
            int index = siblings.IndexOf(m);
            if (index > 0) smName += (index + 1);
            while (owner.GetMembers(smName).Any()) smName += "_";
        }

        // Fields: parameters, then every local of the body.
        var fields = new List<(string type, string name)>();
        var fieldTypes = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var p in symbol.Parameters)
        {
            fieldTypes[p.Name] = TypeText(p.Type, taskTypes);
            fields.Add((fieldTypes[p.Name], p.Name));
        }
        var localNames = new Dictionary<ILocalSymbol, string>(SymbolEqualityComparer.Default);
        string FieldFor(ILocalSymbol local)
        {
            if (localNames.TryGetValue(local, out var have)) return have;
            var type = TypeText(local.Type, taskTypes);
            var name = local.Name;
            for (int n = 2; fieldTypes.TryGetValue(name, out var t) && t != type; n++) name = local.Name + "_" + n;
            if (!fieldTypes.ContainsKey(name)) { fieldTypes[name] = type; fields.Add((type, name)); }
            return localNames[local] = name;
        }

        foreach (var ld in body.DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
        {
            if (InNestedFunction(ld, m) || !IsHoistable(ld)) continue;
            var decl = ld.Declaration;
            e.Replace(decl.Type.SpanStart, decl.Variables[0].SpanStart - decl.Type.SpanStart, "");
            for (int i = 0; i < decl.Variables.Count; i++)
            {
                var v = decl.Variables[i];
                if (model.GetDeclaredSymbol(v) is not ILocalSymbol local) continue;
                var name = FieldFor(local);
                if (v.Initializer == null) e.Replace(v.SpanStart, v.Span.Length, "");
                else
                {
                    if (name != local.Name) e.Replace(v.Identifier.SpanStart, v.Identifier.Span.Length, name);
                    if (v.Initializer.Value is InitializerExpressionSyntax arrayInit)   // int[] a = { 1, 2 };
                        e.Insert(arrayInit.SpanStart, "new " + TypeText(local.Type, taskTypes) + " ", PrioName, statementLevel: false);
                }
                if (i < decl.Variables.SeparatorCount)
                {
                    var comma = decl.Variables.GetSeparator(i);
                    e.Replace(comma.SpanStart, comma.Span.Length, ";");
                }
            }
        }
        foreach (var fs in body.DescendantNodes().OfType<ForStatementSyntax>())
        {
            if (InNestedFunction(fs, m) || fs.Declaration is not { } decl) continue;
            e.Replace(decl.Type.SpanStart, decl.Variables[0].SpanStart - decl.Type.SpanStart, "");
            foreach (var v in decl.Variables)
                if (model.GetDeclaredSymbol(v) is ILocalSymbol local && FieldFor(local) is var name && name != local.Name)
                    e.Replace(v.Identifier.SpanStart, v.Identifier.Span.Length, name);
        }

        var loops = body.DescendantNodes().OfType<ForEachStatementSyntax>()
            .Where(fe => !InNestedFunction(fe, m) && Awaits(fe.Statement, m).Any()).ToList();
        foreach (var fe in loops)
            if (model.GetDeclaredSymbol(fe) is ILocalSymbol local) FieldFor(local);

        // Names: renamed locals, this, and members of the owner reached without "this.".
        foreach (var t in body.DescendantNodes().OfType<ThisExpressionSyntax>())
            e.Replace(t.SpanStart, t.Span.Length, "cs2sx_this");
        foreach (var id in body.DescendantNodes().OfType<SimpleNameSyntax>())
        {
            if (id.Parent is MemberAccessExpressionSyntax ma && ma.Name == id) continue;
            if (id.Parent is MemberBindingExpressionSyntax or QualifiedNameSyntax or NameColonSyntax or NameEqualsSyntax) continue;
            if (id.Parent is AssignmentExpressionSyntax { Parent: InitializerExpressionSyntax } init && init.Left == id) continue;
            var info = model.GetSymbolInfo(id);
            var sym = info.Symbol ?? info.CandidateSymbols.FirstOrDefault();
            if (sym is ILocalSymbol local)
            {
                if (localNames.TryGetValue(local, out var name) && name != local.Name)
                    e.Replace(id.SpanStart, id.Span.Length, name);
                continue;
            }
            if (sym is not (IFieldSymbol or IPropertySymbol or IEventSymbol or IMethodSymbol { MethodKind: MethodKind.Ordinary })) continue;
            if (!InheritsFrom(owner, sym.ContainingType)) continue;
            e.Insert(id.SpanStart, sym.IsStatic ? sym.ContainingType.Name + "." : "cs2sx_this.", PrioName, statementLevel: false);
        }

        // return → finish the task.
        foreach (var r in body.DescendantNodes().OfType<ReturnStatementSyntax>())
        {
            if (InNestedFunction(r, m)) continue;
            if (r.Expression == null)
                e.Replace(r.SpanStart, r.Span.Length, "{ cs2sx_task.Finish(); return; }");
            else
            {
                e.Replace(r.SpanStart, r.Expression.SpanStart - r.SpanStart, "{ cs2sx_task.SetResult(");
                e.Replace(r.SemicolonToken.SpanStart, r.SemicolonToken.Span.Length, "); return; }");
            }
        }

        // await → suspend in front of the statement, the result in its place.
        int points = 0;
        var opened = new HashSet<StatementSyntax>();
        foreach (var a in Awaits(body, m).OrderBy(a => a.Span.End))
        {
            int k = ++points;
            var operand = AwaitOperand(a);
            var awaited = model.GetTypeInfo(operand).Type;
            bool whenAll = IsWhenAll(operand, model);
            var holderType = IsTaskLike(awaited) && !whenAll ? TypeText(awaited!, taskTypes) : "CS2SX_Task";
            var holder = "cs2sx_a" + k;
            fields.Add((holderType, holder));
            bool value = a.Parent is not ExpressionStatementSyntax && holderType != "CS2SX_Task"
                         && !(a.Parent is ArrowExpressionClauseSyntax && !hasResult);

            var hoist = $"{holder} = {e.Render(source, operand.SpanStart, operand.Span.End, all: false)}; "
                      + $"if (!{holder}.IsCompleted) {{ {holder}.Then(this); CS2SX_Async.Suspend({k}); }} ";
            e.Replace(a.SpanStart, a.Span.Length, value ? holder + ".Result" : holder + ".Check()");

            var stmt = a.Ancestors().TakeWhile(x => x != m).OfType<StatementSyntax>().FirstOrDefault();
            if (stmt == null)   // expression-bodied method
                e.Insert(body is ArrowExpressionClauseSyntax arrow ? arrow.Expression.SpanStart : body.SpanStart, hoist, PrioAwaitOpen);
            else
            {
                if (opened.Add(stmt))
                {
                    e.Insert(stmt.SpanStart, "{ ", PrioAwaitOpen);
                    e.Insert(stmt.Span.End, " }", PrioAwaitClose);
                }
                e.Insert(stmt.SpanStart, hoist, PrioAwaitOpen);
            }
        }

        // foreach with an await in its body → index loop over fields.
        int loopIndex = 0;
        foreach (var fe in loops)
        {
            int n = ++loopIndex;
            var collType = model.GetTypeInfo(fe.Expression).Type!;
            var elem = localNames[(ILocalSymbol)model.GetDeclaredSymbol(fe)!];
            string c = "cs2sx_c" + n, i = "cs2sx_i" + n;
            fields.Add((TypeText(collType, taskTypes), c));
            fields.Add(("int", i));
            var count = collType is IArrayTypeSymbol ? "Length" : "Count";
            var header = $"{{ {c} = {e.Render(source, fe.Expression.SpanStart, fe.Expression.Span.End, all: false)}; "
                       + $"for ({i} = 0; {i} < {c}.{count}; {i}++)";
            e.Replace(fe.ForEachKeyword.SpanStart, fe.CloseParenToken.Span.End - fe.ForEachKeyword.SpanStart, header);
            var assign = $"{elem} = {c}[{i}]; ";
            if (fe.Statement is BlockSyntax block)
            {
                e.Insert(block.OpenBraceToken.Span.End, " " + assign, PrioLoopOpen);
                e.Insert(fe.Span.End, " }", PrioLoopClose);
            }
            else
            {
                e.Insert(fe.Statement.SpanStart, "{ " + assign, PrioLoopOpen);
                e.Insert(fe.Statement.Span.End, " } }", PrioLoopClose);
            }
        }

        // The method itself: start the machine; the body moves into cs2sx_run().
        var asyncToken = m.Modifiers.First(x => x.IsKind(SyntaxKind.AsyncKeyword));
        e.Replace(asyncToken.SpanStart, asyncToken.FullSpan.End - asyncToken.SpanStart, "");

        var head = new StringBuilder();
        head.Append($" var cs2sx_sm = new {smName}(); ");
        if (!isStatic) head.Append("cs2sx_sm.cs2sx_this = this; ");
        head.Append($"cs2sx_sm.cs2sx_task = new {taskType}(); ");
        foreach (var p in symbol.Parameters) head.Append($"cs2sx_sm.{p.Name} = {p.Name}; ");
        head.Append("cs2sx_sm.cs2sx_resume(); ");
        if (!returnsVoid) head.Append("return cs2sx_sm.cs2sx_task; ");
        head.Append($"}} public sealed class {smName} : CS2SX_Step {{ ");
        if (!isStatic) head.Append($"public {owner.Name} cs2sx_this; ");
        head.Append($"public {taskType} cs2sx_task; public int cs2sx_state; ");
        foreach (var (type, name) in fields) head.Append($"public {type} {name}; ");
        // An exception ends the task; an async void method has nobody to hand it to.
        head.Append(returnsVoid
            ? "public override void cs2sx_resume() { cs2sx_run(); } "
            : "public override void cs2sx_resume() { try { cs2sx_run(); } catch (Exception cs2sx_e) { cs2sx_task.Fail(cs2sx_e); } } ");
        head.Append($"private void cs2sx_run() {{ CS2SX_Async.Begin({points});");

        if (m.Body != null)
        {
            e.Insert(m.Body.OpenBraceToken.Span.End, head.ToString(), PrioLoopOpen);
            e.Insert(m.Body.CloseBraceToken.SpanStart, " cs2sx_task.Finish(); ", PrioMethod);
            e.Insert(m.Body.CloseBraceToken.Span.End, " }", PrioMethod);
        }
        else
        {
            var arrow = m.ExpressionBody!;
            e.Replace(arrow.ArrowToken.SpanStart, arrow.Expression.SpanStart - arrow.ArrowToken.SpanStart, "{" + head + " ");
            if (hasResult) e.Insert(arrow.Expression.SpanStart, "cs2sx_task.SetResult(", PrioResult);
            e.Replace(m.SemicolonToken.SpanStart, m.SemicolonToken.Span.Length,
                (hasResult ? "); }" : "; cs2sx_task.Finish(); }") + " }");
        }
    }

    private static bool InheritsFrom(INamedTypeSymbol type, INamedTypeSymbol? ancestor)
    {
        for (INamedTypeSymbol? t = type; t != null; t = t.BaseType)
            if (SymbolEqualityComparer.Default.Equals(t, ancestor)) return true;
        return false;
    }

    // ── The task classes (C#, transpiled with the project) ──────────────────

    private static string RuntimeSource(IDictionary<string, string> taskTypes)
    {
        var sb = new StringBuilder(@"// async support (CS2SX): tasks run on the main thread, one step per frame.
public abstract class CS2SX_Step { public abstract void cs2sx_resume(); }
public static class CS2SX_Async { public static void Begin(int points) { } public static void Suspend(int point) { } }
public class CS2SX_Task
{
    public bool IsCompleted;
    public Exception? Fault;
    public long Due;
    private List<CS2SX_Step>? waiting;
    private static List<CS2SX_Task> timers = new List<CS2SX_Task>();
    private static long clock;

    public bool IsFaulted => Fault != null;
    public bool IsCompletedSuccessfully => IsCompleted && Fault == null;
    public static CS2SX_Task CompletedTask { get { var t = new CS2SX_Task(); t.IsCompleted = true; return t; } }

    public void Then(CS2SX_Step step)
    {
        if (waiting == null) waiting = new List<CS2SX_Step>();
        waiting.Add(step);
    }
    public void Finish()
    {
        if (IsCompleted) return;
        IsCompleted = true;
        var w = waiting;
        if (w == null) return;
        waiting = null;
        for (int i = 0; i < w.Count; i++) w[i].cs2sx_resume();
    }
    public void Fail(Exception e) { Fault = e; Finish(); }
    public void Check()
    {
        if (!IsCompleted) throw new InvalidOperationException(""Der Task ist noch nicht fertig (await verwenden)."");
        if (Fault != null) throw Fault;
    }
    public void Wait() { Check(); }

    public static CS2SX_Task Delay(int milliseconds)
    {
        var t = new CS2SX_Task();
        if (milliseconds <= 0) { t.IsCompleted = true; return t; }
        t.Due = clock + milliseconds;
        timers.Add(t);
        return t;
    }
    public static CS2SX_Task Yield()
    {
        var t = new CS2SX_Task();
        timers.Add(t);
        return t;
    }
    public static CS2SX_Task Run(Action work)
    {
        var t = new CS2SX_Task();
        try { work(); t.Finish(); }
        catch (Exception e) { t.Fail(e); }
        return t;
    }
    public static CS2SX_Task WhenAll(params CS2SX_Task[] tasks)
    {
        var all = new CS2SX_WhenAll();
        for (int i = 0; i < tasks.Length; i++) all.Tasks.Add(tasks[i]);
        all.cs2sx_resume();
        return all.Done;
    }
    public static CS2SX_Task WhenAllList(List<CS2SX_Task> tasks)
    {
        var all = new CS2SX_WhenAll();
        for (int i = 0; i < tasks.Count; i++) all.Tasks.Add(tasks[i]);
        all.cs2sx_resume();
        return all.Done;
    }
    // Called by the runtime at the start of every frame.
    public static void Tick(long nowMs)
    {
        clock = nowMs;
        int i = 0, n = timers.Count;
        while (i < n)
        {
            var t = timers[i];
            if (t.Due <= nowMs) { timers.RemoveAt(i); n--; t.Finish(); }
            else i++;
        }
    }
}
public sealed class CS2SX_WhenAll : CS2SX_Step
{
    public List<CS2SX_Task> Tasks = new List<CS2SX_Task>();
    public CS2SX_Task Done = new CS2SX_Task();
    public int Next;
    public override void cs2sx_resume()
    {
        while (Next < Tasks.Count)
        {
            var t = Tasks[Next];
            if (!t.IsCompleted) { t.Then(this); return; }
            Next++;
        }
        for (int i = 0; i < Tasks.Count; i++)
        {
            var failed = Tasks[i].Fault;
            if (failed != null) { Done.Fail(failed); return; }
        }
        Done.Finish();
    }
}
");
        foreach (var (name, type) in taskTypes)
        {
            sb.Append($@"public class {name} : CS2SX_Task
{{
    public {type} Value;
    public {type} Result {{ get {{ Check(); return Value; }} }}
    public void SetResult({type} v) {{ Value = v; Finish(); }}
    public static {name} From({type} v) {{ var t = new {name}(); t.Value = v; t.IsCompleted = true; return t; }}
    public static {name} Run(Func<{type}> work)
    {{
        var t = new {name}();
        try {{ t.SetResult(work()); }}
        catch (Exception e) {{ t.Fail(e); }}
        return t;
    }}
}}
");
        }
        return sb.ToString();
    }
}
