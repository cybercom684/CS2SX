using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using CS2SX.Transpiler.Handlers;

namespace CS2SX.Transpiler.Writers;

public sealed class ExpressionWriter
{
    private readonly TranspilerContext _ctx;
    private readonly InvocationDispatcher _dispatcher;

    public ExpressionWriter(TranspilerContext ctx)
    {
        _ctx = ctx;
        _dispatcher = new InvocationDispatcher(ctx, Write);
    }

    public string Write(SyntaxNode? node)
    {
        if (node == null) return "";

        return node switch
        {
            BinaryExpressionSyntax bin => WriteBinary(bin),
            LiteralExpressionSyntax lit => WriteLiteral(lit),
            IdentifierNameSyntax id => WriteIdentifier(id),
            PrefixUnaryExpressionSyntax pre => pre.OperatorToken.Text + Write(pre.Operand),
            PostfixUnaryExpressionSyntax post => Write(post.Operand) + post.OperatorToken.Text,
            AssignmentExpressionSyntax assign => WriteAssignment(assign),
            MemberAccessExpressionSyntax mem => WriteMemberAccess(mem),
            InvocationExpressionSyntax inv => WriteInvocation(inv),
            InterpolatedStringExpressionSyntax i => FormatStringBuilder.BuildPrintf(i, false, _ctx, Write),
            ArrayCreationExpressionSyntax arr => WriteArrayCreation(arr),
            ObjectCreationExpressionSyntax obj => WriteObjectCreation(obj),
            ParenthesizedExpressionSyntax par => "(" + Write(par.Expression) + ")",
            ConditionalExpressionSyntax cond => WriteConditional(cond),
            CastExpressionSyntax cast => WriteCast(cast),
            ElementAccessExpressionSyntax elem => WriteElementAccess(elem),
            DefaultExpressionSyntax => "NULL",
            ThisExpressionSyntax => "self",
            _ => node.ToString(),
        };
    }

    private string WriteLiteral(LiteralExpressionSyntax lit)
    {
        if (lit.IsKind(SyntaxKind.NullLiteralExpression)) return "NULL";
        if (lit.IsKind(SyntaxKind.TrueLiteralExpression)) return "1";
        if (lit.IsKind(SyntaxKind.FalseLiteralExpression)) return "0";

        if (lit.Token.IsKind(SyntaxKind.StringLiteralToken))
            return "\"" + StringEscaper.EscapeRaw(lit.Token.ValueText) + "\"";

        if (lit.Token.IsKind(SyntaxKind.CharacterLiteralToken))
            return "'" + StringEscaper.EscapeChar(lit.Token.ValueText) + "'";

        var text = lit.Token.Text;
        if (text.EndsWith("f", StringComparison.OrdinalIgnoreCase) && !text.StartsWith("0x"))
            return text[..^1] + "f";
        if (text.EndsWith("d", StringComparison.OrdinalIgnoreCase))
            return text[..^1];
        if (text.EndsWith("m", StringComparison.OrdinalIgnoreCase))
            return text[..^1];
        if (text.EndsWith("ul", StringComparison.OrdinalIgnoreCase))
            return text[..^2] + "ULL";
        if (text.EndsWith("u", StringComparison.OrdinalIgnoreCase))
            return text[..^1] + "U";
        if (text.EndsWith("l", StringComparison.OrdinalIgnoreCase))
            return text[..^1] + "LL";

        return text;
    }

    private string WriteIdentifier(IdentifierNameSyntax id)
    {
        var name = id.Identifier.Text;

        // Enum-Mapping (NpadButton.A etc.)
        var mapped = TypeRegistry.MapEnum(name);
        if (mapped != name) return mapped;

        // Eigene Enum-Member
        if (_ctx.EnumMembers.Contains(name)) return name;

        // Globale CS2SX-Variablen nie als Felder behandeln
        if (name == "_cs2sx_strbuf") return "_cs2sx_strbuf";

        // Feld-Zugriff: _counter → self->f_counter
        if (_ctx.IsFieldAccess(name))
        {
            var trimmed = name.TrimStart('_');
            var prefix = TypeRegistry.HasNoPrefix(trimmed) ? "" : "f_";
            return "self->" + prefix + trimmed;
        }

        // Control-Felder (x, y, width, height etc.)
        if (TypeRegistry.ControlFields.Contains(name) && !string.IsNullOrEmpty(_ctx.CurrentClass))
            return "self->base." + name;

        return name;
    }

    private string WriteBinary(BinaryExpressionSyntax bin)
    {
        var left = Write(bin.Left);
        var right = Write(bin.Right);
        var op = bin.OperatorToken.Text;

        if ((op == "==" || op == "!=") && IsStringExpr(bin.Left))
            return "strcmp(" + left + ", " + right + ") " + op + " 0";

        if (bin.IsKind(SyntaxKind.IsExpression))
            return "/* is-check: " + bin + " */ 1";

        return left + " " + op + " " + right;
    }

    private string WriteMemberAccess(MemberAccessExpressionSyntax mem)
    {
        var full = mem.ToString();

        var mapped = TypeRegistry.MapEnum(full);
        if (mapped != full) return mapped;

        if (full.StartsWith("LibNX.", StringComparison.Ordinal))
            return mem.Name.Identifier.Text;

        var obj = Write(mem.Expression);
        var prop = mem.Name.Identifier.Text;

        if (prop == "Length" && IsStringExpr(mem.Expression))
            return "strlen(" + obj + ")";

        if (prop == "Count" && IsListExpr(mem.Expression))
            return obj + "->count";

        if (prop == "Length" && IsStringBuilderExpr(mem.Expression))
            return obj + "->length";

        var rawExpr = mem.Expression.ToString();
        var key = rawExpr.TrimStart('_');
        if ((_ctx.LocalTypes.TryGetValue(rawExpr, out var lt) && TypeRegistry.IsLibNxStruct(lt))
         || (_ctx.FieldTypes.TryGetValue(key, out var ft) && TypeRegistry.IsLibNxStruct(ft)))
            return obj + "." + prop;

        return obj + "->" + prop;
    }

    private string WriteAssignment(AssignmentExpressionSyntax assign)
    {
        var op = assign.OperatorToken.Text;
        var right = Write(assign.Right);

        if (assign.Left is MemberAccessExpressionSyntax mem)
        {
            var obj = Write(mem.Expression);
            var prop = mem.Name.Identifier.Text;
            var objRaw = mem.Expression.ToString();
            var objKey = objRaw.TrimStart('_');

            bool isStruct = (_ctx.LocalTypes.TryGetValue(objRaw, out var lt) && TypeRegistry.IsLibNxStruct(lt))
                         || (_ctx.FieldTypes.TryGetValue(objKey, out var ft) && TypeRegistry.IsLibNxStruct(ft));
            var arrow = isStruct ? "." : "->";

            if (prop == "Text")
            {
                if (assign.Right is ConditionalExpressionSyntax cond)
                    return "Label_SetText(" + obj + ", (" + Write(cond.Condition) + ") ? "
                         + Write(cond.WhenTrue) + " : " + Write(cond.WhenFalse) + ")";

                if (assign.Right is InterpolatedStringExpressionSyntax interp)
                    return FormatStringBuilder.BuildLabelSetText(obj, interp, _ctx, Write);

                if (assign.Right is LiteralExpressionSyntax litStr
                    && litStr.Token.IsKind(SyntaxKind.StringLiteralToken))
                    return "Label_SetText(" + obj + ", \"" + StringEscaper.EscapeRaw(litStr.Token.ValueText) + "\")";

                return "Label_SetText(" + obj + ", " + right + ")";
            }

            if (prop == "OnClick")
            {
                var methodName = assign.Right.ToString().Trim();
                return obj + "->OnClick = (void(*)(void*))" + _ctx.CurrentClass + "_" + methodName;
            }

            var cProp = TypeRegistry.MapProperty(prop);
            return obj + arrow + cProp + " " + op + " " + right;
        }

        return Write(assign.Left) + " " + op + " " + right;
    }

    private string WriteObjectCreation(ObjectCreationExpressionSyntax obj)
    {
        var typeName = obj.Type.ToString();

        if (TypeRegistry.IsStringBuilder(typeName))
        {
            var cap = obj.ArgumentList?.Arguments.Count > 0
                ? Write(obj.ArgumentList.Arguments[0].Expression)
                : "256";
            return "StringBuilder_New(" + cap + ")";
        }

        if (TypeRegistry.IsList(typeName))
        {
            var inner = TypeRegistry.GetListInnerType(typeName)!;
            var cInner = inner == "string" ? "char" : TypeRegistry.MapType(inner);
            return "List_" + cInner + "_New()";
        }

        if (TypeRegistry.IsDictionary(typeName))
        {
            var types = TypeRegistry.GetDictionaryTypes(typeName)!.Value;
            var cKey = types.key == "string" ? "str" : TypeRegistry.MapType(types.key);
            var cVal = types.val == "string" ? "str" : TypeRegistry.MapType(types.val);
            return "Dict_" + cKey + "_" + cVal + "_New()";
        }

        var args = obj.ArgumentList?.Arguments.Select(a => Write(a.Expression))
                       ?? Enumerable.Empty<string>();
        var creation = typeName + "_New(" + string.Join(", ", args) + ")";

        if (obj.Initializer?.Expressions.Count > 0)
        {
            var tmp = _ctx.NextTmp(typeName.ToLower());
            _ctx.Out.WriteLine(_ctx.Tab + typeName + "* " + tmp + " = " + creation + ";");
            foreach (var expr in obj.Initializer.Expressions)
            {
                if (expr is AssignmentExpressionSyntax asgn)
                {
                    var p = asgn.Left.ToString().Trim();
                    var v = Write(asgn.Right);
                    var cp = TypeRegistry.MapProperty(p);
                    _ctx.Out.WriteLine(_ctx.Tab + tmp + "->" + cp + " = " + v + ";");
                }
            }
            return tmp;
        }

        return creation;
    }

    private string WriteArrayCreation(ArrayCreationExpressionSyntax arr)
    {
        var elemType = arr.Type.ElementType.ToString().Trim();
        var cType = TypeRegistry.MapType(elemType);

        if (arr.Type.RankSpecifiers.Count > 0 && arr.Type.RankSpecifiers[0].Sizes.Count > 0)
        {
            var size = Write(arr.Type.RankSpecifiers[0].Sizes[0]);
            return "(" + cType + "*)malloc(" + size + " * sizeof(" + cType + "))";
        }

        return "(" + cType + "*)malloc(sizeof(" + cType + "))";
    }

    private string WriteInvocation(InvocationExpressionSyntax inv)
    {
        var result = _dispatcher.Dispatch(inv);
        if (result != null) return result;

        var args = inv.ArgumentList.Arguments.Select(a => Write(a.Expression)).ToList();
        var calleeStr = inv.Expression.ToString();
        return calleeStr + "(" + string.Join(", ", args) + ")";
    }

    private string WriteConditional(ConditionalExpressionSyntax cond)
        => "(" + Write(cond.Condition) + " ? " + Write(cond.WhenTrue) + " : " + Write(cond.WhenFalse) + ")";

    private string WriteCast(CastExpressionSyntax cast)
        => "(" + TypeRegistry.MapType(cast.Type.ToString().Trim()) + ")" + Write(cast.Expression);

    private string WriteElementAccess(ElementAccessExpressionSyntax elem)
        => Write(elem.Expression) + "[" + Write(elem.ArgumentList.Arguments[0].Expression) + "]";

    private bool IsStringExpr(SyntaxNode node)
    {
        if (node is LiteralExpressionSyntax lit && lit.Token.IsKind(SyntaxKind.StringLiteralToken))
            return true;
        if (node is IdentifierNameSyntax id)
        {
            var key = id.Identifier.Text.TrimStart('_');
            return (_ctx.LocalTypes.TryGetValue(id.Identifier.Text, out var lt) && lt == "string")
                || (_ctx.FieldTypes.TryGetValue(key, out var ft) && ft == "string");
        }
        return false;
    }

    private bool IsListExpr(SyntaxNode node)
    {
        var raw = node.ToString();
        var key = raw.TrimStart('_');
        return (_ctx.LocalTypes.TryGetValue(raw, out var lt) && TypeRegistry.IsList(lt))
            || (_ctx.FieldTypes.TryGetValue(key, out var ft) && TypeRegistry.IsList(ft));
    }

    private bool IsStringBuilderExpr(SyntaxNode node)
    {
        var raw = node.ToString();
        var key = raw.TrimStart('_');
        return (_ctx.LocalTypes.TryGetValue(raw, out var lt) && TypeRegistry.IsStringBuilder(lt))
            || (_ctx.FieldTypes.TryGetValue(key, out var ft) && TypeRegistry.IsStringBuilder(ft));
    }
}