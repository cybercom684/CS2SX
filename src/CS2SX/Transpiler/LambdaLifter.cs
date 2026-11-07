// Datei: Transpiler/LambdaLifter.cs
//
// FIXES in dieser Version:
//   FIX-1: O(n²) StringWriter-Rewrite entfernt.
//          Vorher: LiftLambda() schrieb Preludes (Struct-Defs + Funktionsdefs) per
//          sb.Clear() + sb.Append(prelude) + sb.Append(existing) in den Output —
//          O(n) pro Lambda, O(n²) für eine Klasse mit n Lambdas.
//          Neu: Preludes werden in _ctx.PendingLambdaPreludes gesammelt.
//          CSharpToC.VisitMethodDeclaration() ruft _ctx.FlushLambdaPreludes() auf,
//          bevor die Methodensignatur geschrieben wird — einmaliger O(k)-Flush.
//
//   FIX-2: HasPrelude / ConsumePrelude sind intern geblieben (werden von
//          ExpressionWriter nicht mehr für den Rewrite genutzt), aber als
//          interne Hilfsmethoden für LambdaLifter selbst behalten.
//
//   HINWEIS: ExpressionWriter.WriteLambda() muss ebenfalls angepasst werden
//            (siehe ExpressionWriter.cs) — der Rewrite-Block dort entfällt.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using CS2SX.Transpiler.Writers;

namespace CS2SX.Transpiler;

public sealed class LambdaLifter
{
    private readonly TranspilerContext _ctx;
    private readonly IExpressionWriter _expr;
    private StatementWriter? _stmt;

    public LambdaLifter(TranspilerContext ctx, IExpressionWriter expr)
    {
        _ctx = ctx;
        _expr = expr;
    }

    public void SetStatementWriter(StatementWriter stmt) => _stmt = stmt;

    public static bool IsLambda(SyntaxNode? node) =>
        node is LambdaExpressionSyntax;

    // ── Öffentliche API ──────────────────────────────────────────────────────

    /// <summary>
    /// Lifts a lambda into a static C function; its definition goes to
    /// _ctx.PendingLambdaPreludes (flushed before the enclosing method).
    ///
    /// Inline mode (default — LINQ/List handlers that call it right away in a loop they
    /// write themselves): returns the function name; captures go through a static
    /// context pointer set just before the call. No allocation.
    ///
    /// Delegate mode (<paramref name="asDelegate"/> — a lambda that becomes a value:
    /// stored, passed to a user method, subscribed to an event): returns an expression
    /// of type CS2SX_Delegate*. Each evaluation gets its own heap environment holding
    /// the captured values, so closures created in a loop keep their own variables.
    /// Lambdas without captures are one static delegate.
    /// </summary>
    public string LiftLambda(
        LambdaExpressionSyntax lambda,
        string? hintType = null,
        string? elementTypeHint = null,
        bool isPredicate = false,
        bool asDelegate = false)
    {
        if (asDelegate) return LiftDelegate(lambda, hintType);

        var id = _ctx.NextLambdaId();
        var name = "_lambda_" + id;
        var caps = FindCaptures(lambda);

        // Derive per-parameter type hints from the delegate hint type (e.g. Action<Window> → ["Window"])
        var effectiveElementHint = elementTypeHint;
        if (effectiveElementHint == null && hintType != null
            && hintType.StartsWith("Action<") && hintType.EndsWith(">"))
        {
            var inner = hintType[7..^1].Trim();
            var typeArgs = SplitGenericArgs(inner);
            if (typeArgs.Count == 1) effectiveElementHint = typeArgs[0];
        }

        // If element type is unknown, try to infer the first parameter's type from SemanticModel.
        // This prevents dead lambdas with "int" fallback type that fail to compile.
        if (effectiveElementHint == null && _ctx.SemanticModel != null)
        {
            try
            {
                ParameterSyntax? firstParam = lambda switch
                {
                    SimpleLambdaExpressionSyntax s => s.Parameter,
                    ParenthesizedLambdaExpressionSyntax p => p.ParameterList.Parameters.FirstOrDefault(),
                    _ => null
                };
                if (firstParam != null)
                {
                    var sym = _ctx.SemanticModel.GetDeclaredSymbol(firstParam);
                    if (sym?.Type != null && sym.Type is not IErrorTypeSymbol)
                        effectiveElementHint = TranspilerContext.FormatTypeSymbol(sym.Type);
                }
            }
            catch { }
        }

        var parms = ApplySemanticParamTypes(lambda, ExtractParams(lambda, effectiveElementHint));
        var retCs = isPredicate ? "bool"
            : hintType != null ? ExtractReturnType(hintType, parms.Count)
            : "void";

        // FIX-1: Prelude in einem lokalen StringBuilder sammeln und dann
        // in _ctx.PendingLambdaPreludes eintragen — kein StringWriter-Rewrite.
        var preludeSb = new System.Text.StringBuilder();
        WriteStructToSb(preludeSb, id, caps);
        WriteFunctionToSb(preludeSb, id, name, lambda, parms, retCs, caps);
        _ctx.PendingLambdaPreludes.Add(preludeSb.ToString());

        // Static-local Closure-Struct statt Stack → kein -Wdangling-pointer.
        // _cs2sx_ctx_N wird VOR dem ersten Aufruf gesetzt und zeigt auf diesen Struct.
        // Safe: Switch ist single-threaded, Lambdas werden immer synchron aufgerufen.
        if (caps.Count > 0)
        {
            var capStruct = "_cap_" + id;
            _ctx.WriteLine($"static struct {capStruct} _ctx_val_{id};");
            foreach (var cap in caps)
                _ctx.WriteLine($"_ctx_val_{id}.{cap.CapName} = {cap.CExpr};");
            _ctx.WriteLine($"_cs2sx_ctx_{id} = &_ctx_val_{id};");
        }

        return name;
    }

    // ── Delegate mode ────────────────────────────────────────────────────────

    private string LiftDelegate(LambdaExpressionSyntax lambda, string? hintType)
    {
        var id = _ctx.NextLambdaId();
        var name = "_lambda_" + id;
        var caps = FindCaptures(lambda);
        var parms = ApplySemanticParamTypes(lambda, ExtractParams(lambda, null));
        var retCs = hintType != null ? ExtractReturnType(hintType, parms.Count) : "void";
        WarnOnMutatedCaptures(lambda, caps);

        var capStruct = "_cap_" + id;
        var sb = new System.Text.StringBuilder();
        if (caps.Count > 0)
        {
            sb.AppendLine($"struct {capStruct}");
            sb.AppendLine("{");
            sb.AppendLine("    CS2SX_Env _hdr;");
            foreach (var cap in caps)
                sb.AppendLine("    " + CapFieldDecl(cap) + ";");
            sb.AppendLine("};");
            sb.AppendLine();
        }

        var retC = DelegateModel.SignatureCType(retCs);
        var paramList = new List<string> { "void* _env" };
        paramList.AddRange(parms.Select(p => DelegateModel.SignatureCType(p.CsType) + " " + p.Name));
        sb.AppendLine($"static {retC} {name}({string.Join(", ", paramList)})");
        sb.AppendLine("{");
        if (caps.Count > 0)
        {
            sb.AppendLine($"    struct {capStruct}* _c = (struct {capStruct}*)_env;");
            foreach (var cap in caps)
                sb.AppendLine("    " + CapFieldDecl(cap) + " = _c->" + cap.CapName + ";");
        }
        else
            sb.AppendLine("    (void)_env;");
        sb.Append(TranspileBody(lambda, retCs, caps, parms));
        sb.AppendLine("}");
        sb.AppendLine();

        if (caps.Count == 0)
        {
            sb.AppendLine($"static CS2SX_Delegate _dg_{id} = CS2SX_DELEGATE_STATIC({name});");
            sb.AppendLine();
            _ctx.PendingLambdaPreludes.Add(sb.ToString());
            return $"(&_dg_{id})";
        }
        _ctx.PendingLambdaPreludes.Add(sb.ToString());

        // A fresh environment per evaluation (inside a loop: per iteration).
        var env = "_env_" + id;
        _ctx.WriteLine($"struct {capStruct}* {env} = CS2SX_ENV_NEW(struct {capStruct});");
        foreach (var cap in caps)
            _ctx.WriteLine($"{env}->{cap.CapName} = {cap.CExpr};");
        return $"CS2SX_Delegate_New((void*){name}, {env}, 1)";
    }

    private string CapFieldDecl(CaptureInfo cap)
    {
        if (cap.CapName == "self" && !string.IsNullOrEmpty(_ctx.CurrentClass))
            return _ctx.CurrentClass + "* self";
        return MapFieldType(cap.CsType) + (NeedsPtr(cap.CsType) ? "*" : "") + " " + cap.CapName;
    }

    /// Captured variables are copied when the delegate is created. C# shares them
    /// instead, so assigning one inside the lambda (or after creating it) would not be
    /// seen on the other side — say so instead of silently behaving differently.
    private void WarnOnMutatedCaptures(LambdaExpressionSyntax lambda, List<CaptureInfo> caps)
    {
        if (_ctx.SemanticModel == null || caps.Count == 0) return;
        try
        {
            var flow = _ctx.SemanticModel.AnalyzeDataFlow(lambda);
            if (flow == null || !flow.Succeeded) return;
            foreach (var sym in flow.CapturedInside)
            {
                if (sym is not (ILocalSymbol or IParameterSymbol)) continue;
                if (!caps.Any(c => c.CapName == sym.Name.TrimStart('_'))) continue;
                if (flow.WrittenInside.Contains(sym, SymbolEqualityComparer.Default))
                    _ctx.Warn(lambda, $"Lambda ändert die erfasste Variable '{sym.Name}' — CS2SX kopiert erfasste lokale Variablen beim Erzeugen des Delegates, die Änderung ist außerhalb nicht sichtbar. Ein Feld statt der lokalen Variable verwenden.");
            }
        }
        catch { }
    }

    /// Parameter types from the semantic model (`(a, b) => …` has no syntax types).
    private List<ParamInfo> ApplySemanticParamTypes(LambdaExpressionSyntax lambda, List<ParamInfo> parms)
    {
        if (_ctx.SemanticModel == null) return parms;
        var syntaxParams = lambda switch
        {
            SimpleLambdaExpressionSyntax s => new List<ParameterSyntax> { s.Parameter },
            ParenthesizedLambdaExpressionSyntax p => p.ParameterList.Parameters.ToList(),
            _ => new List<ParameterSyntax>(),
        };
        var result = new List<ParamInfo>(parms.Count);
        for (int i = 0; i < parms.Count; i++)
        {
            var csType = parms[i].CsType;
            try
            {
                if (i < syntaxParams.Count
                    && _ctx.SemanticModel.GetDeclaredSymbol(syntaxParams[i]) is IParameterSymbol ps
                    && ps.Type is not IErrorTypeSymbol)
                    csType = TranspilerContext.FormatTypeSymbol(ps.Type);
            }
            catch { }
            result.Add(parms[i] with { CsType = csType });
        }
        return result;
    }

    // ── Typedef-Generierung ──────────────────────────────────────────────────

    public static string GenerateTypedef(string csType)
    {
        if (csType == "Action")
            return "typedef void (*Action_t)(void);";
        if (csType.StartsWith("Action<") && csType.EndsWith(">"))
        {
            var inner = csType[7..^1];
            var pTypes = SplitGenericArgs(inner).Select(TypeRegistry.MapType).ToList();
            var suffix = string.Join("_", pTypes);
            return $"typedef void (*Action_{suffix}_t)({string.Join(", ", pTypes)});";
        }
        if (csType.StartsWith("Func<") && csType.EndsWith(">"))
        {
            var inner = csType[5..^1];
            var allArgs = SplitGenericArgs(inner);
            var retC = TypeRegistry.MapType(allArgs.Last());
            var pTypes = allArgs.Take(allArgs.Count - 1).Select(TypeRegistry.MapType).ToList();
            var suffix = string.Join("_", allArgs.Select(TypeRegistry.MapType));
            var pList = pTypes.Count > 0 ? string.Join(", ", pTypes) : "void";
            return $"typedef {retC} (*Func_{suffix}_t)({pList});";
        }
        var ident = csType.Replace("<", "_").Replace(">", "").Replace(",", "_").Replace(" ", "");
        return $"typedef void (*{ident}_t)(void);";
    }

    public static string MapDelegateType(string csType)
    {
        // Delegate to TypeRegistry which uses MapListInnerType for proper C identifiers
        // (e.g. Action<string> → Action_str_t, not Action_const char*_t)
        var mapped = TypeRegistry.MapType(csType);
        // If MapType returned a known delegate typedef, return it directly
        if (mapped.EndsWith("_t")) return mapped;
        // Unknown delegate type — sanitize for use as C identifier
        var ident = csType.Replace("<", "_").Replace(">", "").Replace(",", "_").Replace(" ", "");
        return ident + "_t";
    }

    // ── Prelude-Aufbau (in StringBuilder) ────────────────────────────────────

    private void WriteStructToSb(System.Text.StringBuilder sb, int id, List<CaptureInfo> caps)
    {
        if (caps.Count == 0) return;
        var capStruct = "_cap_" + id;
        sb.AppendLine($"struct {capStruct}");
        sb.AppendLine("{");
        foreach (var cap in caps)
        {
            if (cap.CapName == "self" && !string.IsNullOrEmpty(_ctx.CurrentClass))
            {
                sb.AppendLine($"    {_ctx.CurrentClass}* self;");
            }
            else
            {
                var ct = MapFieldType(cap.CsType);
                var ptr = NeedsPtr(cap.CsType) ? "*" : "";
                sb.AppendLine($"    {ct}{ptr} {cap.CapName};");
            }
        }
        sb.AppendLine("};");
        // Static global context pointer — avoids passing void* ctx through every
        // call site (sort comparers, LINQ predicates, etc. only pass the actual
        // element arguments). Safe because Switch homebrew is single-threaded.
        sb.AppendLine($"static struct {capStruct}* _cs2sx_ctx_{id} = NULL;");
        sb.AppendLine();
    }

    private void WriteFunctionToSb(
        System.Text.StringBuilder sb,
        int id, string name,
        LambdaExpressionSyntax lambda,
        List<ParamInfo> parms,
        string retCs,
        List<CaptureInfo> caps)
    {
        var retC = TypeRegistry.MapType(retCs);
        var capStructName = "_cap_" + id;

        // No void* ctx parameter — callers (sort, LINQ, etc.) only pass the element
        // arguments. Captures are accessed via the per-lambda static global pointer.
        var paramList = new List<string>();
        foreach (var p in parms)
        {
            var pt = MapParamType(p.CsType);
            paramList.Add($"{pt} {p.Name}");
        }

        sb.AppendLine($"static {retC} {name}({string.Join(", ", paramList)})");
        sb.AppendLine("{");

        if (caps.Count > 0)
            sb.AppendLine($"    struct {capStructName}* _c = _cs2sx_ctx_{id};");

        foreach (var cap in caps)
        {
            if (cap.CapName == "self" && !string.IsNullOrEmpty(_ctx.CurrentClass))
                sb.AppendLine($"    {_ctx.CurrentClass}* self = _c->self;");
            else
            {
                var ct = MapFieldType(cap.CsType);
                var ptr = NeedsPtr(cap.CsType) ? "*" : "";
                sb.AppendLine($"    {ct}{ptr} {cap.CapName} = _c->{cap.CapName};");
            }
        }

        var bodyContent = TranspileBody(lambda, retCs, caps, parms);
        sb.Append(bodyContent);
        sb.AppendLine("}");
        sb.AppendLine();
    }

    // ── Body-Transpilierung in separatem Kontext ─────────────────────────────

    private string TranspileBody(
        LambdaExpressionSyntax lambda,
        string retCsType,
        List<CaptureInfo> caps,
        List<ParamInfo> parms)
    {
        var tempWriter = new System.IO.StringWriter();
        var tempCtx = _ctx.CreateChild(tempWriter);

        // Captures und Parameter in den temp-Kontext eintragen
        foreach (var cap in caps)
        {
            tempCtx.LocalTypes[cap.CapName] = cap.CsType;
            if (!cap.CapName.StartsWith('_'))
                tempCtx.LocalTypes["_" + cap.CapName] = cap.CsType;
        }
        foreach (var p in parms)
            tempCtx.LocalTypes[p.Name] = p.CsType;

        var tempExpr = new ExpressionWriter(tempCtx);
        var tempStmt = new StatementWriter(tempCtx, tempExpr);

        tempCtx.Indent();

        switch (lambda)
        {
            case SimpleLambdaExpressionSyntax simple:
                WriteBodyNode(simple.Body, retCsType, tempCtx, tempExpr, tempStmt);
                break;
            case ParenthesizedLambdaExpressionSyntax paren:
                WriteBodyNode(paren.Body, retCsType, tempCtx, tempExpr, tempStmt);
                break;
        }

        // Zähler zurücksynchronisieren damit Haupt-Kontext keine doppelten Nummern vergibt
        _ctx.TmpCounter = tempCtx.TmpCounter;
        _ctx.TmpStringCounter = tempCtx.TmpStringCounter;
        return tempWriter.ToString();
    }

    private static void WriteBodyNode(
        CSharpSyntaxNode body,
        string retCsType,
        TranspilerContext tempCtx,
        ExpressionWriter tempExpr,
        StatementWriter tempStmt)
    {
        if (body is BlockSyntax block)
        {
            foreach (var stmt in block.Statements)
                tempStmt.Write(stmt);
        }
        else if (body is ExpressionSyntax expr)
        {
            var cExpr = tempExpr.Write(expr);
            if (retCsType is "void" or "")
                tempCtx.WriteLine(cExpr + ";");
            else
                tempCtx.WriteLine("return " + cExpr + ";");
        }
    }

    // ── Capture-Analyse ──────────────────────────────────────────────────────

    private List<CaptureInfo> FindCaptures(LambdaExpressionSyntax lambda)
    {
        var captures = new List<CaptureInfo>();
        var paramNames = new HashSet<string>(ExtractParams(lambda, null).Select(p => p.Name));
        bool needsSelf = false;

        var identifiers = lambda.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Select(id => id.Identifier.Text)
            .Where(n => !paramNames.Contains(n))
            .Distinct()
            .ToList();

        foreach (var rawName in identifiers)
        {
            string? csType;
            string cExpr;

            if (_ctx.LocalTypes.TryGetValue(rawName, out var lt))
            {
                csType = lt.StartsWith("@ref:") ? lt["@ref:".Length..] : lt;
                cExpr = rawName;
            }
            else
            {
                // Instance fields are NOT copied into the closure: C# lambdas see the
                // live field, so the body must read/write it through the captured
                // `self` (a copy would make `() => _count++` silently do nothing).
                if (_ctx.FieldTypes.ContainsKey(rawName.TrimStart('_'))
                    || _ctx.FieldTypes.ContainsKey(rawName))
                    needsSelf = true;
                continue;
            }

            var capName = rawName.TrimStart('_');
            if (captures.Any(c => c.CapName == capName)) continue;
            captures.Add(new CaptureInfo(capName, csType!, cExpr));
        }

        // Prüfen ob eigene Methoden-Aufrufe self benötigen
        // FIX: In statischen Methoden gibt es kein self — Prüfung überspringen
        if (!needsSelf && !string.IsNullOrEmpty(_ctx.CurrentClass) && !_ctx.IsStaticMethod)
        {
            needsSelf = lambda.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(inv =>
                    inv.Expression is IdentifierNameSyntax invId
                    && !paramNames.Contains(invId.Identifier.Text)
                    && !_ctx.LocalTypes.ContainsKey(invId.Identifier.Text));
        }

        if (needsSelf
            && !_ctx.IsStaticMethod
            && !string.IsNullOrEmpty(_ctx.CurrentClass)
            && !captures.Any(c => c.CapName == "self"))
        {
            captures.Insert(0, new CaptureInfo("self", _ctx.CurrentClass, "self"));
        }

        return captures;
    }

    private static List<ParamInfo> ExtractParams(
        LambdaExpressionSyntax lambda,
        string? elementTypeHint)
    {
        var result = new List<ParamInfo>();
        switch (lambda)
        {
            case SimpleLambdaExpressionSyntax simple:
                result.Add(new ParamInfo(
                    simple.Parameter.Identifier.Text,
                    simple.Parameter.Type?.ToString().Trim() ?? elementTypeHint ?? "int"));
                break;
            case ParenthesizedLambdaExpressionSyntax paren:
                foreach (var p in paren.ParameterList.Parameters)
                    result.Add(new ParamInfo(
                        p.Identifier.Text,
                        p.Type?.ToString().Trim() ?? elementTypeHint ?? "int"));
                break;
        }
        return result;
    }

    private static string ExtractReturnType(string delegateType, int paramCount)
    {
        if (delegateType == "Action") return "void";
        if (delegateType.StartsWith("Action<")) return "void";
        if (delegateType.StartsWith("Func<") && delegateType.EndsWith(">"))
        {
            var args = SplitGenericArgs(delegateType[5..^1]);
            return args.Count > 0 ? args[^1] : "void";
        }
        return "void";
    }

    // ── Typ-Hilfsmethoden ────────────────────────────────────────────────────

    private static string MapFieldType(string csType) =>
        csType == "string" ? "const char*" : TypeRegistry.MapType(csType);

    private string MapParamType(string csType)
    {
        if (csType == "string") return "const char*";
        var mapped = TypeRegistry.MapType(csType);
        if (mapped.EndsWith("*")) return mapped;
        if (TypeRegistry.NeedsPointerSuffix(csType) && !_ctx.EnumDefs.ContainsKey(csType))
            return mapped + "*";
        return mapped;
    }

    private bool NeedsPtr(string csType)
    {
        if (csType == "string") return false;
        if (_ctx.EnumDefs.ContainsKey(csType)) return false;
        var cMapped = TypeRegistry.MapType(csType);
        if (cMapped.EndsWith("*")) return false;
        return TypeRegistry.NeedsPointerSuffix(csType) || TypeRegistry.IsList(csType);
    }

    private static List<string> SplitGenericArgs(string s)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        int depth = 0;
        foreach (char c in s)
        {
            if (c == '<') { depth++; current.Append(c); }
            else if (c == '>') { depth--; current.Append(c); }
            else if (c == ',' && depth == 0) { result.Add(current.ToString().Trim()); current.Clear(); }
            else current.Append(c);
        }
        if (current.Length > 0) result.Add(current.ToString().Trim());
        return result;
    }

    // ── Hilfstypen ────────────────────────────────────────────────────────────

    private sealed record ParamInfo(string Name, string CsType);
    private sealed record CaptureInfo(string CapName, string CsType, string CExpr);
}