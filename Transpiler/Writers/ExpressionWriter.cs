using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using CS2SX.Transpiler.Handlers;

namespace CS2SX.Transpiler.Writers;

/// <summary>
/// Transpiliert C#-Ausdrücke zu C-Code.
/// Alle Expression-Typen sind hier zentralisiert.
///
/// Erweiterung: Neuen case im WriteExpression-Switch ergänzen.
/// </summary>
public sealed class ExpressionWriter
{
    private readonly TranspilerContext _ctx;
    private readonly InvocationDispatcher _dispatcher;

    public ExpressionWriter(TranspilerContext ctx)
    {
        _ctx        = ctx;
        _dispatcher = new InvocationDispatcher(ctx, Write);
    }

    // ── Öffentliche API ───────────────────────────────────────────────────────

    public string Write(SyntaxNode? node)
    {
        if (node == null) return "";

        return node switch
        {
            BinaryExpressionSyntax bin           => WriteBinary(bin),
            LiteralExpressionSyntax lit          => WriteLiteral(lit),
            IdentifierNameSyntax id              => WriteIdentifier(id),
            PrefixUnaryExpressionSyntax pre      => pre.OperatorToken.Text + Write(pre.Operand),
            PostfixUnaryExpressionSyntax post    => Write(post.Operand) + post.OperatorToken.Text,
            AssignmentExpressionSyntax assign    => WriteAssignment(assign),
            MemberAccessExpressionSyntax mem     => WriteMemberAccess(mem),
            InvocationExpressionSyntax inv       => WriteInvocation(inv),
            InterpolatedStringExpressionSyntax i => FormatStringBuilder.BuildPrintf(i, false, _ctx, Write),
            ArrayCreationExpressionSyntax arr    => WriteArrayCreation(arr),
            ObjectCreationExpressionSyntax obj   => WriteObjectCreation(obj),
            ParenthesizedExpressionSyntax par    => "(" + Write(par.Expression) + ")",
            ConditionalExpressionSyntax cond     => WriteConditional(cond),
            CastExpressionSyntax cast            => WriteCast(cast),
            ElementAccessExpressionSyntax elem   => WriteElementAccess(elem),
            DefaultExpressionSyntax              => "NULL",
            ThisExpressionSyntax                 => "self",
            _                                    => node.ToString(),
        };
    }

    // ── Literale ──────────────────────────────────────────────────────────────

    private string WriteLiteral(LiteralExpressionSyntax lit)
    {
        if (lit.IsKind(SyntaxKind.NullLiteralExpression))  return "NULL";
        if (lit.IsKind(SyntaxKind.TrueLiteralExpression))  return "1";
        if (lit.IsKind(SyntaxKind.FalseLiteralExpression)) return "0";

        if (lit.Token.IsKind(SyntaxKind.StringLiteralToken))
            return "\"" + StringEscaper.EscapeRaw(lit.Token.ValueText) + "\"";

        if (lit.Token.IsKind(SyntaxKind.CharacterLiteralToken))
            return "'" + StringEscaper.EscapeChar(lit.Token.ValueText) + "'";

        // Zahlen: C#-Suffixe normalisieren
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

    // ── Bezeichner ────────────────────────────────────────────────────────────

    private string WriteIdentifier(IdentifierNameSyntax id)
    {
        var name = id.Identifier.Text;

        // Enum-Mapping (NpadButton.A etc.)
        var mapped = TypeRegistry.MapEnum(name);
        if (mapped != name) return mapped;

        // Globale CS2SX-Variablen nie als Felder behandeln
        if (name == "_cs2sx_strbuf") return "_cs2sx_strbuf";

        // Feld-Zugriff: _counter → self->f_counter
        if (_ctx.IsFieldAccess(name))
        {
            var trimmed = name.TrimStart('_');
            var prefix  = TypeRegistry.HasNoPrefix(trimmed) ? "" : "f_";
            return "self->" + prefix + trimmed;
        }

        // Control-Felder (x, y, width, height etc.)
        if (TypeRegistry.ControlFields.Contains(name) && !string.IsNullOrEmpty(_ctx.CurrentClass))
            return "self->base." + name;

        return name;
    }

    // ── Binäre Ausdrücke ──────────────────────────────────────────────────────

    private string WriteBinary(BinaryExpressionSyntax bin)
    {
        var left  = Write(bin.Left);
        var right = Write(bin.Right);
        var op    = bin.OperatorToken.Text;

        // String-Vergleich → strcmp
        if ((op == "==" || op == "!=") && IsStringExpr(bin.Left))
            return "strcmp(" + left + ", " + right + ") " + op + " 0";

        if (bin.IsKind(SyntaxKind.IsExpression))
            return "/* is-check: " + bin + " */ 1";

        return left + " " + op + " " + right;
    }

    // ── Member-Zugriff ────────────────────────────────────────────────────────

    private string WriteMemberAccess(MemberAccessExpressionSyntax mem)
    {
        var full = mem.ToString();

        // Enum-Mapping (NpadButton.A etc.)
        var mapped = TypeRegistry.MapEnum(full);
        if (mapped != full) return mapped;

        // LibNX-Namespace → letzter Bezeichner
        if (full.StartsWith("LibNX.", StringComparison.Ordinal))
            return mem.Name.Identifier.Text;

        var obj  = Write(mem.Expression);
        var prop = mem.Name.Identifier.Text;

        // string.Length → strlen(...)
        if (prop == "Length" && IsStringExpr(mem.Expression))
            return "strlen(" + obj + ")";

        // List<T>.Count → ->count
        if (prop == "Count" && IsListExpr(mem.Expression))
            return obj + "->count";

        // StringBuilder.Length → ->length
        if (prop == "Length" && IsStringBuilderExpr(mem.Expression))
            return obj + "->length";

        // libnx Stack-Struct → Punkt-Zugriff
        var rawExpr = mem.Expression.ToString();
        var key     = rawExpr.TrimStart('_');
        if ((_ctx.LocalTypes.TryGetValue(rawExpr, out var lt) && TypeRegistry.IsLibNxStruct(lt))
         || (_ctx.FieldTypes.TryGetValue(key, out var ft) && TypeRegistry.IsLibNxStruct(ft)))
            return obj + "." + prop;

        return obj + "->" + prop;
    }

    // ── Zuweisungen ───────────────────────────────────────────────────────────

    private string WriteAssignment(AssignmentExpressionSyntax assign)
    {
        var op    = assign.OperatorToken.Text;
        var right = Write(assign.Right);

        if (assign.Left is MemberAccessExpressionSyntax mem)
        {
            var obj    = Write(mem.Expression);
            var prop   = mem.Name.Identifier.Text;
            var objRaw = mem.Expression.ToString();
            var objKey = objRaw.TrimStart('_');

            // Struct-Zugriff: . statt ->
            bool isStruct = (_ctx.LocalTypes.TryGetValue(objRaw, out var lt) && TypeRegistry.IsLibNxStruct(lt))
                         || (_ctx.FieldTypes.TryGetValue(objKey, out var ft) && TypeRegistry.IsLibNxStruct(ft));
            var arrow = isStruct ? "." : "->";

            // Text = ... → immer Label_SetText
            if (prop == "Text")
            {
                // Ternary: label.Text = a ? "x" : "y"
                if (assign.Right is ConditionalExpressionSyntax cond)
                    return "Label_SetText(" + obj + ", (" + Write(cond.Condition) + ") ? "
                         + Write(cond.WhenTrue) + " : " + Write(cond.WhenFalse) + ")";

                // Interpolated string
                if (assign.Right is InterpolatedStringExpressionSyntax interp)
                    return FormatStringBuilder.BuildLabelSetText(obj, interp, _ctx, Write);

                // String-Literal
                if (assign.Right is LiteralExpressionSyntax litStr
                    && litStr.Token.IsKind(SyntaxKind.StringLiteralToken))
                    return "Label_SetText(" + obj + ", \"" + StringEscaper.EscapeRaw(litStr.Token.ValueText) + "\")";

                // Alles andere (Variable, sb.ToString(), etc.)
                return "Label_SetText(" + obj + ", " + right + ")";
            }

            // OnClick → Funktionszeiger
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

    // ── Objekt-Erstellung ─────────────────────────────────────────────────────

    private string WriteObjectCreation(ObjectCreationExpressionSyntax obj)
    {
        var typeName = obj.Type.ToString();

        // new StringBuilder(N) → StringBuilder_New(N)
        if (TypeRegistry.IsStringBuilder(typeName))
        {
            var cap = obj.ArgumentList?.Arguments.Count > 0
                ? Write(obj.ArgumentList.Arguments[0].Expression)
                : "256";
            return "StringBuilder_New(" + cap + ")";
        }

        // new List<T>() → List_T_New()
        if (TypeRegistry.IsList(typeName))
        {
            var inner  = TypeRegistry.GetListInnerType(typeName)!;
            var cInner = inner == "string" ? "char" : TypeRegistry.MapType(inner);
            return "List_" + cInner + "_New()";
        }

        var args     = obj.ArgumentList?.Arguments.Select(a => Write(a.Expression))
                       ?? Enumerable.Empty<string>();
        var creation = typeName + "_New(" + string.Join(", ", args) + ")";

        // Object Initializer { X=5, Y=10 } → temp-Variable + Zuweisungen
        if (obj.Initializer?.Expressions.Count > 0)
        {
            var tmp = _ctx.NextTmp(typeName.ToLower());
            _ctx.Out.WriteLine(_ctx.Tab + typeName + "* " + tmp + " = " + creation + ";");
            foreach (var expr in obj.Initializer.Expressions)
            {
                if (expr is AssignmentExpressionSyntax asgn)
                {
                    var p    = asgn.Left.ToString().Trim();
                    var v    = Write(asgn.Right);
                    var cp   = TypeRegistry.MapProperty(p);
                    _ctx.Out.WriteLine(_ctx.Tab + tmp + "->" + cp + " = " + v + ";");
                }
            }
            return tmp;
        }

        return creation;
    }

    // ── Array-Erstellung ──────────────────────────────────────────────────────

    private string WriteArrayCreation(ArrayCreationExpressionSyntax arr)
    {
        var elemType = arr.Type.ElementType.ToString().Trim();
        var cType    = TypeRegistry.MapType(elemType);

        if (arr.Type.RankSpecifiers.Count > 0 && arr.Type.RankSpecifiers[0].Sizes.Count > 0)
        {
            var size = Write(arr.Type.RankSpecifiers[0].Sizes[0]);
            return "(" + cType + "*)malloc(" + size + " * sizeof(" + cType + "))";
        }

        return "(" + cType + "*)malloc(sizeof(" + cType + "))";
    }

    // ── Invocation ────────────────────────────────────────────────────────────

    private string WriteInvocation(InvocationExpressionSyntax inv)
    {
        // Dispatcher prüft alle Handler
        var result = _dispatcher.Dispatch(inv);
        if (result != null) return result;

        // Fallback: direkter C-Funktionsaufruf
        var args     = inv.ArgumentList.Arguments.Select(a => Write(a.Expression)).ToList();
        var calleeStr = inv.Expression.ToString();
        return calleeStr + "(" + string.Join(", ", args) + ")";
    }

    // ── Sonstige Ausdrücke ────────────────────────────────────────────────────

    private string WriteConditional(ConditionalExpressionSyntax cond)
        => "(" + Write(cond.Condition) + " ? " + Write(cond.WhenTrue) + " : " + Write(cond.WhenFalse) + ")";

    private string WriteCast(CastExpressionSyntax cast)
        => "(" + TypeRegistry.MapType(cast.Type.ToString().Trim()) + ")" + Write(cast.Expression);

    private string WriteElementAccess(ElementAccessExpressionSyntax elem)
        => Write(elem.Expression) + "[" + Write(elem.ArgumentList.Arguments[0].Expression) + "]";

    // ── Utilities ─────────────────────────────────────────────────────────────

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
