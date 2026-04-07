using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Writers;

public static class TypeInferrer
{
    public static string InferCSharpType(SyntaxNode? expr, TranspilerContext ctx)
    {
        if (expr == null) return "int";

        var semantic = ctx.GetSemanticType(expr);
        if (semantic != null && semantic != "object" && semantic != "?")
            return semantic;

        return InferSyntactic(expr, ctx);
    }

    private static string InferSyntactic(SyntaxNode expr, TranspilerContext ctx)
    {
        switch (expr)
        {
            case LiteralExpressionSyntax lit:
                return InferLiteral(lit);

            case IdentifierNameSyntax id:
                return InferIdentifier(id.Identifier.Text, ctx);

            case CastExpressionSyntax cast:
                return cast.Type.ToString().Trim();

            case MemberAccessExpressionSyntax mem:
                return InferMemberAccess(mem, ctx);

            case InvocationExpressionSyntax inv:
                return InferInvocation(inv, ctx);

            case BinaryExpressionSyntax bin:
                return InferBinary(bin, ctx);

            case PrefixUnaryExpressionSyntax pre:
                // FIX 12: !expr → bool
                if (pre.OperatorToken.IsKind(SyntaxKind.ExclamationToken))
                    return "bool";
                return InferSyntactic(pre.Operand, ctx);

            case PostfixUnaryExpressionSyntax post:
                return InferSyntactic(post.Operand, ctx);

            case ConditionalExpressionSyntax cond:
                return InferSyntactic(cond.WhenTrue, ctx);

            case ParenthesizedExpressionSyntax par:
                return InferSyntactic(par.Expression, ctx);

            case ObjectCreationExpressionSyntax oc:
                return oc.Type.ToString().Trim();

            case ElementAccessExpressionSyntax elem:
                return InferElementAccess(elem, ctx);

            case DefaultExpressionSyntax def:
                return def.Type.ToString().Trim();

            case ThisExpressionSyntax:
                return ctx.CurrentClass;

            case InterpolatedStringExpressionSyntax:
                return "string";

            case ConditionalAccessExpressionSyntax:
                return "object";

            default:
                return "int";
        }
    }

    private static string InferLiteral(LiteralExpressionSyntax lit)
    {
        // FIX 12: bool-Literale
        if (lit.IsKind(SyntaxKind.TrueLiteralExpression)) return "bool";
        if (lit.IsKind(SyntaxKind.FalseLiteralExpression)) return "bool";
        if (lit.IsKind(SyntaxKind.NullLiteralExpression)) return "object";

        if (lit.Token.Value is int) return "int";
        if (lit.Token.Value is uint) return "uint";
        if (lit.Token.Value is long) return "long";
        if (lit.Token.Value is ulong) return "ulong";
        if (lit.Token.Value is float) return "float";
        if (lit.Token.Value is double) return "double";
        if (lit.Token.Value is char) return "char";
        if (lit.Token.Value is string) return "string";
        if (lit.Token.Value is bool) return "bool";

        var text = lit.Token.Text;
        if (text.EndsWith("f", StringComparison.OrdinalIgnoreCase) && text.Contains('.'))
            return "float";
        if (text.EndsWith("d", StringComparison.OrdinalIgnoreCase))
            return "double";
        if (text.EndsWith("ul", StringComparison.OrdinalIgnoreCase) ||
            text.EndsWith("lu", StringComparison.OrdinalIgnoreCase))
            return "ulong";
        if (text.EndsWith("u", StringComparison.OrdinalIgnoreCase))
            return "uint";
        if (text.EndsWith("l", StringComparison.OrdinalIgnoreCase))
            return "long";

        return "int";
    }

    private static string InferIdentifier(string name, TranspilerContext ctx)
    {
        if (ctx.LocalTypes.TryGetValue(name, out var lt))
        {
            // FIX 14: @ref: Marker entfernen für Typ-Inferenz
            if (lt.StartsWith("@ref:", StringComparison.Ordinal))
                return lt["@ref:".Length..];
            return lt;
        }

        var trimmed = name.TrimStart('_');
        if (ctx.FieldTypes.TryGetValue(trimmed, out var ft)) return ft;
        if (ctx.FieldTypes.TryGetValue(name, out var ft2)) return ft2;
        if (ctx.PropertyTypes.TryGetValue(name, out var pt)) return pt;

        return "int";
    }

    private static string InferMemberAccess(MemberAccessExpressionSyntax mem, TranspilerContext ctx)
    {
        var prop = mem.Name.Identifier.Text;
        var objRaw = mem.Expression.ToString();
        var objKey = objRaw.TrimStart('_');

        // Bekannte Typ-Konstanten
        if (prop is "MaxValue" or "MinValue" or "Epsilon")
        {
            var typeName = objRaw;
            return typeName switch
            {
                "float" or "double" => typeName,
                _ => "int",
            };
        }

        if (prop is "Count" or "Length" or "count" or "length")
            return "int";

        string? objType = null;
        ctx.LocalTypes.TryGetValue(objRaw, out objType);
        if (objType == null) ctx.FieldTypes.TryGetValue(objKey, out objType);
        if (objType == null) ctx.PropertyTypes.TryGetValue(objRaw, out objType);

        if (objType != null && TypeRegistry.IsList(objType) && prop == "Count")
            return "int";

        if (objRaw is "this" or "self")
        {
            if (ctx.FieldTypes.TryGetValue(prop, out var selfFt)) return selfFt;
            if (ctx.PropertyTypes.TryGetValue(prop, out var selfPt)) return selfPt;
        }

        if (ctx.PropertyTypes.TryGetValue(prop, out var directPt)) return directPt;
        if (ctx.FieldTypes.TryGetValue(prop, out var directFt)) return directFt;

        return "int";
    }

    private static string InferInvocation(InvocationExpressionSyntax inv, TranspilerContext ctx)
    {
        var callee = inv.Expression.ToString();

        if (callee.EndsWith(".ToString") || callee == "ToString")
            return "string";

        if (callee is "string.Format" or "String.Format"
                   or "string.Concat" or "String.Concat"
                   or "string.Join" or "String.Join")
            return "string";

        if (callee is "string.IsNullOrEmpty" or "String.IsNullOrEmpty"
                   or "string.IsNullOrWhiteSpace" or "String.IsNullOrWhiteSpace"
                   or "string.Contains" or "String.Contains"
                   or "string.StartsWith" or "String.StartsWith"
                   or "string.EndsWith" or "String.EndsWith"
                   or "int.TryParse" or "Int32.TryParse"
                   or "float.TryParse" or "Single.TryParse")
            return "bool";

        if (callee is "int.Parse" or "Int32.Parse" or "CS2SX_Int_Parse")
            return "int";
        if (callee is "float.Parse" or "Single.Parse" or "CS2SX_Float_Parse")
            return "float";

        if (callee is "string.Substring" or "String.Substring"
                   or "string.Replace" or "String.Replace"
                   or "string.Trim" or "String.Trim"
                   or "string.ToUpper" or "String.ToUpper"
                   or "string.ToLower" or "String.ToLower")
            return "string";

        if (callee is "Input.GetTouch" or "CS2SX_Input_GetTouch")
            return "TouchState";

        if (callee is "Input.GetStickLeft" or "_cs2sx_get_stick_left"
                   or "CS2SX_Input_GetStickLeft")
            return "StickPos";

        if (callee is "Input.GetStickRight" or "_cs2sx_get_stick_right"
                   or "CS2SX_Input_GetStickRight")
            return "StickPos";

        if (callee is "System.GetBattery" or "CS2SX_GetBattery")
            return "BatteryInfo";

        // FIX 3: Random Methoden → int/float
        if (callee is "CS2SX_Rand_Next" or "CS2SX_Rand_NextMax")
            return "int";
        if (callee is "CS2SX_Rand_Float")
            return "float";

        // FIX 2: Math-Methoden
        if (callee is "Math.Abs" or "System.Math.Abs"
                   or "Math.Min" or "System.Math.Min"
                   or "Math.Max" or "System.Math.Max"
                   or "Math.Clamp" or "System.Math.Clamp"
                   or "Math.Sign" or "System.Math.Sign"
                   or "Math.Round" or "System.Math.Round")
            return "int";

        if (callee is "Math.Sqrt" or "System.Math.Sqrt"
                   or "Math.Sin" or "System.Math.Sin"
                   or "Math.Cos" or "System.Math.Cos"
                   or "Math.Tan" or "System.Math.Tan"
                   or "Math.Pow" or "System.Math.Pow"
                   or "Math.Floor" or "System.Math.Floor"
                   or "Math.Ceil" or "System.Math.Ceil"
                   or "Math.Ceiling" or "System.Math.Ceiling"
                   or "Math.Atan2" or "System.Math.Atan2")
            return "float";

        if (inv.Expression is IdentifierNameSyntax idName
            && ctx.MethodReturnTypes.TryGetValue(idName.Identifier.Text, out var rt))
            return rt;

        if (inv.Expression is MemberAccessExpressionSyntax memAcc)
        {
            var methodName = memAcc.Name.Identifier.Text;
            if (ctx.MethodReturnTypes.TryGetValue(methodName, out var mrt))
                return mrt;
            if (methodName is "Count" or "GetCount") return "int";
        }

        return "int";
    }

    private static string InferBinary(BinaryExpressionSyntax bin, TranspilerContext ctx)
    {
        // Vergleichsoperatoren → bool
        if (bin.IsKind(SyntaxKind.EqualsExpression)
         || bin.IsKind(SyntaxKind.NotEqualsExpression)
         || bin.IsKind(SyntaxKind.LessThanExpression)
         || bin.IsKind(SyntaxKind.LessThanOrEqualExpression)
         || bin.IsKind(SyntaxKind.GreaterThanExpression)
         || bin.IsKind(SyntaxKind.GreaterThanOrEqualExpression)
         || bin.IsKind(SyntaxKind.LogicalAndExpression)
         || bin.IsKind(SyntaxKind.LogicalOrExpression))
            return "bool";

        if (bin.IsKind(SyntaxKind.AddExpression))
        {
            var lt = InferSyntactic(bin.Left, ctx);
            var rt = InferSyntactic(bin.Right, ctx);
            if (lt == "string" || rt == "string") return "string";
            if (lt == "float" || rt == "float") return "float";
            if (lt == "double" || rt == "double") return "double";
            return lt;
        }

        var leftType = InferSyntactic(bin.Left, ctx);
        var rightType = InferSyntactic(bin.Right, ctx);

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
            var inner = TypeRegistry.GetListInnerType(objType);
            if (inner != null) return inner;

            var dictTypes = TypeRegistry.GetDictionaryTypes(objType);
            if (dictTypes.HasValue) return dictTypes.Value.val;

            if (objType.EndsWith("[]"))
                return objType[..^2];

            if (objType == "string") return "char";

            if (objType is "TouchState") return "int";
            if (objType is "StickPos") return "int";
        }

        return "int";
    }

    public static string FormatSpecifier(SyntaxNode? expr, TranspilerContext ctx)
    {
        if (expr is MemberAccessExpressionSyntax memSpec)
        {
            var prop = memSpec.Name.Identifier.Text;
            if (prop is "Count" or "Length" or "count" or "length")
                return "%d";
        }

        var csType = InferCSharpType(expr, ctx);

        // FIX 12: bool → %d
        if (csType == "bool") return "%d";

        var cType = TypeRegistry.MapType(csType);
        return TypeRegistry.FormatSpecifier(cType);
    }
}