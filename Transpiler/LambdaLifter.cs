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
    /// Hebt ein Lambda in eine statische C-Funktion und schreibt das Prelude
    /// (Struct-Def + Funktionsdefinition) in _ctx.PendingLambdaPreludes.
    /// CSharpToC.VisitMethodDeclaration() flusht diese VOR der Methodensignatur.
    /// Gibt den C-Funktionsnamen zurück.
    /// </summary>
    public string LiftLambda(
        LambdaExpressionSyntax lambda,
        string? hintType = null,
        string? elementTypeHint = null)
    {
        var id = _ctx.NextLambdaId();
        var name = "_lambda_" + id;
        var caps = FindCaptures(lambda);
        var parms = ExtractParams(lambda, elementTypeHint);
        var retCs = hintType != null ? ExtractReturnType(hintType, parms.Count) : "int";

        // FIX-1: Prelude in einem lokalen StringBuilder sammeln und dann
        // in _ctx.PendingLambdaPreludes eintragen — kein StringWriter-Rewrite.
        var preludeSb = new System.Text.StringBuilder();
        WriteStructToSb(preludeSb, id, caps);
        WriteFunctionToSb(preludeSb, id, name, lambda, parms, retCs, caps);
        _ctx.PendingLambdaPreludes.Add(preludeSb.ToString());

        // Capture-Struct im aktuellen Output befüllen (nach dem Lambda-Ausdruck)
        if (caps.Count > 0)
        {
            var capStruct = "_cap_" + id;
            _ctx.WriteLine($"struct {capStruct}* _ctx_{id} = malloc(sizeof(struct {capStruct}));");
            foreach (var cap in caps)
                _ctx.WriteLine($"_ctx_{id}->{cap.CapName} = {cap.CExpr};");
        }

        return name;
    }

    // ── Typedef-Generierung ──────────────────────────────────────────────────

    public static string GenerateTypedef(string csType)
    {
        if (csType == "Action")
            return "typedef void (*Action_t)(void*);";
        if (csType.StartsWith("Action<") && csType.EndsWith(">"))
        {
            var inner = csType[7..^1];
            var pTypes = SplitGenericArgs(inner).Select(TypeRegistry.MapType).ToList();
            var suffix = string.Join("_", pTypes);
            return $"typedef void (*Action_{suffix}_t)(void*, {string.Join(", ", pTypes)});";
        }
        if (csType.StartsWith("Func<") && csType.EndsWith(">"))
        {
            var inner = csType[5..^1];
            var allArgs = SplitGenericArgs(inner);
            var retC = TypeRegistry.MapType(allArgs.Last());
            var pTypes = allArgs.Take(allArgs.Count - 1).Select(TypeRegistry.MapType).ToList();
            var suffix = string.Join("_", allArgs.Select(TypeRegistry.MapType));
            var pList = pTypes.Count > 0 ? "void*, " + string.Join(", ", pTypes) : "void*";
            return $"typedef {retC} (*Func_{suffix}_t)({pList});";
        }
        var ident = csType.Replace("<", "_").Replace(">", "").Replace(",", "_").Replace(" ", "");
        return $"typedef void (*{ident}_t)(void*);";
    }

    public static string MapDelegateType(string csType)
    {
        if (csType == "Action") return "Action_t";
        if (csType.StartsWith("Action<") && csType.EndsWith(">"))
            return "Action_" + string.Join("_",
                SplitGenericArgs(csType[7..^1]).Select(TypeRegistry.MapType)) + "_t";
        if (csType.StartsWith("Func<") && csType.EndsWith(">"))
            return "Func_" + string.Join("_",
                SplitGenericArgs(csType[5..^1]).Select(TypeRegistry.MapType)) + "_t";
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

        var paramList = new List<string> { "void* _ctx_arg" };
        foreach (var p in parms)
        {
            var pt = MapParamType(p.CsType);
            paramList.Add($"{pt} {p.Name}");
        }

        sb.AppendLine($"static {retC} {name}({string.Join(", ", paramList)})");
        sb.AppendLine("{");

        if (caps.Count > 0)
            sb.AppendLine($"    struct {capStructName}* _c = (struct {capStructName}*)_ctx_arg;");

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
        var tempCtx = new TranspilerContext(tempWriter);

        // Zustand kopieren
        foreach (var kv in _ctx.LocalTypes) tempCtx.LocalTypes[kv.Key] = kv.Value;
        foreach (var kv in _ctx.FieldTypes) tempCtx.FieldTypes[kv.Key] = kv.Value;
        foreach (var kv in _ctx.MethodReturnTypes) tempCtx.MethodReturnTypes[kv.Key] = kv.Value;
        foreach (var kv in _ctx.PropertyTypes) tempCtx.PropertyTypes[kv.Key] = kv.Value;
        foreach (var em in _ctx.EnumMembers) tempCtx.EnumMembers.Add(em);
        foreach (var vt in _ctx.ValueTypeStructs) tempCtx.ValueTypeStructs.Add(vt);
        foreach (var it in _ctx.InterfaceTypes) tempCtx.InterfaceTypes.Add(it);
        foreach (var vt in _ctx.VTableTypes) tempCtx.VTableTypes.Add(vt);

        tempCtx.CurrentClass = _ctx.CurrentClass;
        tempCtx.CurrentBaseType = _ctx.CurrentBaseType;
        tempCtx.SemanticModel = _ctx.SemanticModel;
        tempCtx.CurrentFile = _ctx.CurrentFile;
        tempCtx.TmpCounter = _ctx.TmpCounter;

        // Captures und Parameter in den temp-Kontext eintragen
        foreach (var cap in caps)
        {
            tempCtx.LocalTypes[cap.CapName] = cap.CsType;
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

        // TmpCounter zurücksynchronisieren damit Haupt-Kontext keine doppelten Nummern vergibt
        _ctx.TmpCounter = tempCtx.TmpCounter;
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
                var fieldKey = rawName.TrimStart('_');
                if (_ctx.FieldTypes.TryGetValue(fieldKey, out var ft))
                {
                    csType = ft;
                    var pfx = TypeRegistry.HasNoPrefix(fieldKey) ? "" : "f_";
                    cExpr = "self->" + pfx + fieldKey;
                    needsSelf = true;
                }
                else if (_ctx.FieldTypes.TryGetValue(rawName, out var ft2))
                {
                    csType = ft2;
                    var pfx = TypeRegistry.HasNoPrefix(rawName) ? "" : "f_";
                    cExpr = "self->" + pfx + rawName;
                    needsSelf = true;
                }
                else continue;
            }

            var capName = rawName.TrimStart('_');
            if (captures.Any(c => c.CapName == capName)) continue;
            captures.Add(new CaptureInfo(capName, csType!, cExpr));
        }

        // Prüfen ob eigene Methoden-Aufrufe self benötigen
        if (!needsSelf && !string.IsNullOrEmpty(_ctx.CurrentClass))
        {
            needsSelf = lambda.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Any(inv =>
                    inv.Expression is IdentifierNameSyntax invId
                    && !paramNames.Contains(invId.Identifier.Text)
                    && !_ctx.LocalTypes.ContainsKey(invId.Identifier.Text));
        }

        if (needsSelf
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

    private static string MapParamType(string csType) =>
        csType == "string" ? "const char*" : TypeRegistry.MapType(csType);

    private static bool NeedsPtr(string csType) =>
        csType != "string"
        && (TypeRegistry.NeedsPointerSuffix(csType) || TypeRegistry.IsList(csType));

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