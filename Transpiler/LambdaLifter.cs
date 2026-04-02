using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using CS2SX.Transpiler.Writers;

namespace CS2SX.Transpiler;

/// <summary>
/// Hebt Lambda-Ausdrücke zu benannten statischen C-Funktionen.
///
/// Action a = () => Foo()   → static void _lambda_0(void* _ctx) { ... }
/// Func<int,bool> f = x => x > 0  → static int _lambda_1(void* _ctx, int x) { ... }
///
/// Captures werden als Capture-Struct + void*-Context realisiert.
/// </summary>
public sealed class LambdaLifter
{
    private readonly TranspilerContext _ctx;
    private readonly ExpressionWriter _expr;
    private StatementWriter? _stmt;

    private int _lambdaCounter;
    private readonly List<LambdaInfo> _pending = new();

    public LambdaLifter(TranspilerContext ctx, ExpressionWriter expr)
    {
        _ctx = ctx;
        _expr = expr;
    }

    /// <summary>
    /// StatementWriter nachträglich injizieren (vermeidet zirkuläre Abhängigkeit
    /// zwischen ExpressionWriter und StatementWriter).
    /// </summary>
    public void SetStatementWriter(StatementWriter stmt) => _stmt = stmt;

    // ── Öffentliche API ──────────────────────────────────────────────────

    public static bool IsLambda(SyntaxNode? node) =>
        node is LambdaExpressionSyntax;

    /// <summary>
    /// Erzeugt für ein Lambda eine statische C-Funktion und gibt den Funktionszeiger-
    /// Ausdruck zurück der stattdessen verwendet werden soll.
    /// </summary>
    public string LiftLambda(LambdaExpressionSyntax lambda, string? hintType = null)
    {
        var id = _lambdaCounter++;
        var name = "_lambda_" + id;
        var caps = FindCaptures(lambda);
        var parms = ExtractParams(lambda);
        var retCs = hintType != null ? ExtractReturnType(hintType, parms.Count) : "void";

        _pending.Add(new LambdaInfo(id, name, lambda, parms, retCs, caps));

        // Capture-Struct ausgeben wenn nötig
        if (caps.Count > 0)
        {
            var capStruct = "_cap_" + id;
            _ctx.Out.WriteLine("struct " + capStruct);
            _ctx.Out.WriteLine("{");
            foreach (var (capName, capType) in caps)
            {
                var ct = TypeRegistry.MapType(capType);
                _ctx.Out.WriteLine("    " + ct + " " + capName + ";");
            }
            _ctx.Out.WriteLine("};");

            _ctx.WriteLine("struct " + capStruct + "* _ctx_" + id
                         + " = malloc(sizeof(struct " + capStruct + "));");
            foreach (var (capName, _) in caps)
                _ctx.WriteLine("_ctx_" + id + "->" + capName + " = " + capName + ";");
        }

        return name;
    }

    /// <summary>
    /// Schreibt alle noch ausstehenden Lambda-Definitionen in den Output.
    /// Aufruf am Ende jeder Klasse (nach allen Methoden).
    /// </summary>
    public void FlushPending()
    {
        foreach (var info in _pending)
            WriteLambdaFunction(info);
        _pending.Clear();
    }

    // ── Typedef-Generierung ──────────────────────────────────────────────

    public static string GenerateTypedef(string csType)
    {
        if (csType == "Action")
            return "typedef void (*Action_t)(void*);";

        if (csType.StartsWith("Action<") && csType.EndsWith(">"))
        {
            var inner = csType[7..^1];
            var pTypes = SplitGenericArgs(inner)
                .Select(t => TypeRegistry.MapType(t))
                .ToList();
            var suffix = string.Join("_", pTypes);
            var pList = "void*, " + string.Join(", ", pTypes);
            return "typedef void (*Action_" + suffix + "_t)(" + pList + ");";
        }

        if (csType.StartsWith("Func<") && csType.EndsWith(">"))
        {
            var inner = csType[5..^1];
            var allArgs = SplitGenericArgs(inner);
            var retCs = allArgs.Last();
            var retC = TypeRegistry.MapType(retCs);
            var pTypes = allArgs.Take(allArgs.Count - 1)
                .Select(t => TypeRegistry.MapType(t))
                .ToList();
            var suffix = string.Join("_", allArgs.Select(t => TypeRegistry.MapType(t)));
            var pList = pTypes.Count > 0
                ? "void*, " + string.Join(", ", pTypes)
                : "void*";
            return "typedef " + retC + " (*Func_" + suffix + "_t)(" + pList + ");";
        }

        var ident = csType.Replace("<", "_").Replace(">", "").Replace(",", "_").Replace(" ", "");
        return "typedef void (*" + ident + "_t)(void*);";
    }

    public static string MapDelegateType(string csType)
    {
        if (csType == "Action") return "Action_t";

        if (csType.StartsWith("Action<") && csType.EndsWith(">"))
        {
            var inner = csType[7..^1];
            var suffix = string.Join("_", SplitGenericArgs(inner).Select(t => TypeRegistry.MapType(t)));
            return "Action_" + suffix + "_t";
        }

        if (csType.StartsWith("Func<") && csType.EndsWith(">"))
        {
            var inner = csType[5..^1];
            var suffix = string.Join("_", SplitGenericArgs(inner).Select(t => TypeRegistry.MapType(t)));
            return "Func_" + suffix + "_t";
        }

        var ident = csType.Replace("<", "_").Replace(">", "").Replace(",", "_").Replace(" ", "");
        return ident + "_t";
    }

    // ── Private Implementierung ──────────────────────────────────────────

    private void WriteLambdaFunction(LambdaInfo info)
    {
        var retC = TypeRegistry.MapType(info.ReturnTypeCSharp);
        var hasCtx = info.Captures.Count > 0;
        var capStructName = "_cap_" + info.Id;

        var parms = new List<string> { "void* _ctx_arg" };
        foreach (var (pName, pCsType) in info.Params)
        {
            var pt = TypeRegistry.MapType(pCsType);
            parms.Add(pt + " " + pName);
        }

        _ctx.Out.WriteLine("static " + retC + " " + info.Name
                         + "(" + string.Join(", ", parms) + ")");
        _ctx.Out.WriteLine("{");
        _ctx.Indent();

        // Capture-Struct casten
        if (hasCtx)
            _ctx.WriteLine("struct " + capStructName + "* _c = (struct " + capStructName + "*)_ctx_arg;");

        // Capture-Variablen als lokale Aliase
        foreach (var (capName, capType) in info.Captures)
        {
            var ct = TypeRegistry.MapType(capType);
            _ctx.WriteLine(ct + " " + capName + " = _c->" + capName + ";");
            _ctx.LocalTypes[capName] = capType;
        }

        // Parameter in LocalTypes registrieren
        foreach (var (pName, pCsType) in info.Params)
            _ctx.LocalTypes[pName] = pCsType;

        WriteBody(info);

        _ctx.Dedent();
        _ctx.Out.WriteLine("}");
        _ctx.Out.WriteLine();
    }

    private void WriteBody(LambdaInfo info)
    {
        switch (info.Lambda)
        {
            case SimpleLambdaExpressionSyntax simple:
                WriteLambdaBodyNode(simple.Body, info.ReturnTypeCSharp);
                break;
            case ParenthesizedLambdaExpressionSyntax paren:
                WriteLambdaBodyNode(paren.Body, info.ReturnTypeCSharp);
                break;
        }
    }

    private void WriteLambdaBodyNode(CSharpSyntaxNode body, string retCsType)
    {
        if (body is BlockSyntax block)
        {
            // Vollständiger Block — StatementWriter nutzen wenn verfügbar
            if (_stmt != null)
            {
                foreach (var stmt in block.Statements)
                    _stmt.Write(stmt);
            }
            else
            {
                // Fallback ohne StatementWriter (sollte in der Praxis nicht vorkommen)
                foreach (var stmt in block.Statements)
                    _ctx.WriteLine("/* stmt: " + stmt.ToString().Replace("\n", " ").Replace("\r", "") + " */");
            }
        }
        else if (body is ExpressionSyntax expr)
        {
            var cExpr = _expr.Write(expr);
            if (retCsType == "void" || retCsType == "")
                _ctx.WriteLine(cExpr + ";");
            else
                _ctx.WriteLine("return " + cExpr + ";");
        }
    }

    // ── Capture-Analyse ──────────────────────────────────────────────────

    private List<(string name, string csType)> FindCaptures(LambdaExpressionSyntax lambda)
    {
        var captures = new List<(string, string)>();
        var paramNames = new HashSet<string>(ExtractParams(lambda).Select(p => p.name));

        var identifiers = lambda.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Select(id => id.Identifier.Text)
            .Where(n => !paramNames.Contains(n))
            .Distinct();

        foreach (var name in identifiers)
        {
            var type = _ctx.LookupType(name);
            if (type != null)
                captures.Add((name, type));
        }

        return captures;
    }

    private static List<(string name, string csType)> ExtractParams(LambdaExpressionSyntax lambda)
    {
        var result = new List<(string, string)>();

        switch (lambda)
        {
            case SimpleLambdaExpressionSyntax simple:
                result.Add((simple.Parameter.Identifier.Text,
                    simple.Parameter.Type?.ToString().Trim() ?? "int"));
                break;

            case ParenthesizedLambdaExpressionSyntax paren:
                foreach (var p in paren.ParameterList.Parameters)
                    result.Add((p.Identifier.Text, p.Type?.ToString().Trim() ?? "int"));
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
            var inner = delegateType[5..^1];
            var args = SplitGenericArgs(inner);
            return args.Count > 0 ? args[^1] : "void";
        }
        return "void";
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
            else if (c == ',' && depth == 0)
            {
                result.Add(current.ToString().Trim());
                current.Clear();
            }
            else current.Append(c);
        }

        if (current.Length > 0)
            result.Add(current.ToString().Trim());

        return result;
    }

    private record LambdaInfo(
        int Id,
        string Name,
        LambdaExpressionSyntax Lambda,
        List<(string name, string csType)> Params,
        string ReturnTypeCSharp,
        List<(string name, string csType)> Captures);
}