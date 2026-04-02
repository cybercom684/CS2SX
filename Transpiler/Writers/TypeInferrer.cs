using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Writers;

/// <summary>
/// Leitet den C#-Typ eines Ausdrucks her ohne Roslyn-Semantic-Model.
///
/// Verbesserungen gegenüber der alten Version:
///   • MemberAccess schlägt FieldTypes / PropertyTypes korrekt nach
///   • InvocationExpression nutzt MethodReturnTypes
///   • Cast gibt den Ziel-Typ zurück (nicht "int")
///   • BinaryExpression propagiert den Typ des linken Operanden
///   • Literal erkennt float-Suffix (1.0f → "float")
///   • ConditionalExpression propagiert den WhenTrue-Typ
/// </summary>
public static class TypeInferrer
{
    /// <summary>
    /// Gibt den C#-Typ des Ausdrucks zurück (best-effort ohne Semantic-Model).
    /// Fällt nur dann auf "int" zurück wenn wirklich nichts bekannt ist.
    /// </summary>
    public static string InferCSharpType(Microsoft.CodeAnalysis.SyntaxNode? expr, TranspilerContext ctx)
    {
        if (expr == null) return "int";

        switch (expr)
        {
            // ── Literale ──────────────────────────────────────────────────
            case LiteralExpressionSyntax lit:
                return InferLiteral(lit);

            // ── Identifier ────────────────────────────────────────────────
            case IdentifierNameSyntax id:
                return InferIdentifier(id.Identifier.Text, ctx);

            // ── Cast ──────────────────────────────────────────────────────
            case CastExpressionSyntax cast:
                return cast.Type.ToString().Trim();

            // ── Member Access ─────────────────────────────────────────────
            case MemberAccessExpressionSyntax mem:
                return InferMemberAccess(mem, ctx);

            // ── Invocation ────────────────────────────────────────────────
            case InvocationExpressionSyntax inv:
                return InferInvocation(inv, ctx);

            // ── Binär / Unär → Typ des Operanden propagieren ──────────────
            case BinaryExpressionSyntax bin:
                return InferBinary(bin, ctx);

            case PrefixUnaryExpressionSyntax pre:
                return InferCSharpType(pre.Operand, ctx);

            case PostfixUnaryExpressionSyntax post:
                return InferCSharpType(post.Operand, ctx);

            // ── Conditional: Typ des True-Zweigs ──────────────────────────
            case ConditionalExpressionSyntax cond:
                return InferCSharpType(cond.WhenTrue, ctx);

            // ── Parenthesized ─────────────────────────────────────────────
            case ParenthesizedExpressionSyntax par:
                return InferCSharpType(par.Expression, ctx);

            // ── Object creation → Klassen-Name ────────────────────────────
            case ObjectCreationExpressionSyntax oc:
                return oc.Type.ToString().Trim();

            // ── Element access (Liste/Array/Dict) ─────────────────────────
            case ElementAccessExpressionSyntax elem:
                return InferElementAccess(elem, ctx);

            // ── Default / This ────────────────────────────────────────────
            case DefaultExpressionSyntax def:
                return def.Type.ToString().Trim();

            case ThisExpressionSyntax:
                return ctx.CurrentClass;

            default:
                return "int";
        }
    }

    // ── Hilfsmethoden ──────────────────────────────────────────────────────

    private static string InferLiteral(LiteralExpressionSyntax lit)
    {
        if (lit.Token.Value is int) return "int";
        if (lit.Token.Value is uint) return "uint";
        if (lit.Token.Value is long) return "long";
        if (lit.Token.Value is ulong) return "ulong";
        if (lit.Token.Value is float) return "float";
        if (lit.Token.Value is double) return "double";
        if (lit.Token.Value is char) return "char";
        if (lit.Token.Value is string) return "string";
        if (lit.Token.Value is bool) return "bool";

        // Float-Suffix im Token-Text prüfen (z.B. 1.0f)
        var text = lit.Token.Text;
        if (text.EndsWith("f", StringComparison.OrdinalIgnoreCase) && text.Contains('.'))
            return "float";
        if (text.EndsWith("d", StringComparison.OrdinalIgnoreCase))
            return "double";
        if (text.EndsWith("u", StringComparison.OrdinalIgnoreCase))
            return "uint";
        if (text.EndsWith("ul", StringComparison.OrdinalIgnoreCase) ||
            text.EndsWith("lu", StringComparison.OrdinalIgnoreCase))
            return "ulong";
        if (text.EndsWith("l", StringComparison.OrdinalIgnoreCase))
            return "long";

        return "int";
    }

    private static string InferIdentifier(string name, TranspilerContext ctx)
    {
        // 1. Lokale Variable
        if (ctx.LocalTypes.TryGetValue(name, out var lt)) return lt;

        // 2. Feld (mit und ohne _ Prefix)
        var trimmed = name.TrimStart('_');
        if (ctx.FieldTypes.TryGetValue(trimmed, out var ft)) return ft;
        if (ctx.FieldTypes.TryGetValue(name, out var ft2)) return ft2;

        // 3. Property der aktuellen Klasse
        if (ctx.PropertyTypes.TryGetValue(name, out var pt)) return pt;

        return "int";
    }

    private static string InferMemberAccess(MemberAccessExpressionSyntax mem, TranspilerContext ctx)
    {
        var prop = mem.Name.Identifier.Text;
        var objRaw = mem.Expression.ToString();
        var objKey = objRaw.TrimStart('_');

        // .Count / .Length sind immer int
        if (prop is "Count" or "Length" or "count" or "length")
            return "int";

        // Objekt-Typ ermitteln
        string? objType = null;
        ctx.LocalTypes.TryGetValue(objRaw, out objType);
        if (objType == null) ctx.FieldTypes.TryGetValue(objKey, out objType);
        if (objType == null) ctx.PropertyTypes.TryGetValue(objRaw, out objType);

        // List<T>.Count → int
        if (objType != null && TypeRegistry.IsList(objType) && prop == "Count")
            return "int";

        // Eigenes Feld des Objekts → dessen Typ wenn Objekt == self
        if (objRaw is "this" or "self")
        {
            if (ctx.FieldTypes.TryGetValue(prop, out var selfFt)) return selfFt;
            if (ctx.PropertyTypes.TryGetValue(prop, out var selfPt)) return selfPt;
        }

        // Bekannte String-Properties
        if (objType == "string" && prop == "Length") return "int";

        // Property-Typ direkt aus Context
        if (ctx.PropertyTypes.TryGetValue(prop, out var directPt)) return directPt;

        // Feld-Typ direkt aus Context
        if (ctx.FieldTypes.TryGetValue(prop, out var directFt)) return directFt;

        return "int";
    }

    private static string InferInvocation(InvocationExpressionSyntax inv, TranspilerContext ctx)
    {
        var callee = inv.Expression.ToString();

        // ToString() → immer string
        if (callee.EndsWith(".ToString") || callee == "ToString")
            return "string";

        // Bekannte String-Methoden → string
        if (callee is "string.Format" or "String.Format"
                   or "string.Concat" or "String.Concat"
                   or "string.Join" or "String.Join")
            return "string";

        // String.IsNullOrEmpty etc. → bool
        if (callee is "string.IsNullOrEmpty" or "String.IsNullOrEmpty"
                   or "string.IsNullOrWhiteSpace" or "String.IsNullOrWhiteSpace")
            return "bool";

        // Int/Float Parse → int/float
        if (callee is "int.Parse" or "Int32.Parse"
                   or "CS2SX_Int_Parse")
            return "int";
        if (callee is "float.Parse" or "Single.Parse"
                   or "CS2SX_Float_Parse")
            return "float";

        // Eigene Methoden → MethodReturnTypes nachschlagen
        if (inv.Expression is IdentifierNameSyntax idName)
        {
            if (ctx.MethodReturnTypes.TryGetValue(idName.Identifier.Text, out var rt))
                return rt;
        }
        else if (inv.Expression is MemberAccessExpressionSyntax memAcc)
        {
            var methodName = memAcc.Name.Identifier.Text;
            if (ctx.MethodReturnTypes.TryGetValue(methodName, out var rt))
                return rt;

            // Count / Length Methoden-Aufrufe
            if (methodName is "Count" or "GetCount") return "int";
        }

        // Get-Methoden oft int
        if (callee.Contains("Get") && !callee.Contains("GetString")
                                   && !callee.Contains("GetText"))
            return "int";

        return "int";
    }

    private static string InferBinary(BinaryExpressionSyntax bin, TranspilerContext ctx)
    {
        // Vergleiche → bool
        if (bin.IsKind(SyntaxKind.EqualsExpression)
         || bin.IsKind(SyntaxKind.NotEqualsExpression)
         || bin.IsKind(SyntaxKind.LessThanExpression)
         || bin.IsKind(SyntaxKind.LessThanOrEqualExpression)
         || bin.IsKind(SyntaxKind.GreaterThanExpression)
         || bin.IsKind(SyntaxKind.GreaterThanOrEqualExpression)
         || bin.IsKind(SyntaxKind.LogicalAndExpression)
         || bin.IsKind(SyntaxKind.LogicalOrExpression))
            return "bool";

        // String-Konkatenation
        if (bin.IsKind(SyntaxKind.AddExpression))
        {
            var lt = InferCSharpType(bin.Left, ctx);
            var rt = InferCSharpType(bin.Right, ctx);
            if (lt == "string" || rt == "string") return "string";
            // float dominiert über int
            if (lt == "float" || rt == "float") return "float";
            if (lt == "double" || rt == "double") return "double";
            return lt; // linken Typ propagieren
        }

        // Arithmetik → linken Typ propagieren
        var leftType = InferCSharpType(bin.Left, ctx);
        var rightType = InferCSharpType(bin.Right, ctx);

        // Numerische Promotion: float/double dominiert
        if (leftType == "double" || rightType == "double") return "double";
        if (leftType == "float" || rightType == "float") return "float";
        if (leftType == "ulong" || rightType == "ulong") return "ulong";
        if (leftType == "long" || rightType == "long") return "long";
        if (leftType == "uint" || rightType == "uint") return "uint";

        return leftType;
    }

    private static string InferElementAccess(ElementAccessExpressionSyntax elem, TranspilerContext ctx)
    {
        var objRaw = elem.Expression.ToString();
        var objKey = objRaw.TrimStart('_');

        string? objType = null;
        ctx.LocalTypes.TryGetValue(objRaw, out objType);
        if (objType == null) ctx.FieldTypes.TryGetValue(objKey, out objType);

        if (objType != null)
        {
            // List<T>[i] → T
            var inner = TypeRegistry.GetListInnerType(objType);
            if (inner != null) return inner;

            // Dictionary<K,V>[k] → V
            var dictTypes = TypeRegistry.GetDictionaryTypes(objType);
            if (dictTypes.HasValue) return dictTypes.Value.val;

            // T[] → T
            if (objType.EndsWith("[]"))
                return objType[..^2];
        }

        return "int";
    }

    // ── Format-Specifier ───────────────────────────────────────────────────

    /// <summary>
    /// Gibt den printf Format-Specifier für einen Ausdruck zurück.
    /// </summary>
    public static string FormatSpecifier(
        Microsoft.CodeAnalysis.SyntaxNode? expr, TranspilerContext ctx)
    {
        // .Count / .Length → %d
        if (expr is MemberAccessExpressionSyntax memSpec)
        {
            var prop = memSpec.Name.Identifier.Text;
            if (prop is "Count" or "Length" or "count" or "length")
                return "%d";
        }

        var csType = InferCSharpType(expr, ctx);
        var cType = TypeRegistry.MapType(csType);
        return TypeRegistry.FormatSpecifier(cType);
    }
}
