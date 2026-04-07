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
            BinaryExpressionSyntax coalesce
                when coalesce.IsKind(SyntaxKind.CoalesceExpression)
                                                               => WriteCoalesce(coalesce),
            BinaryExpressionSyntax bin => WriteBinary(bin),
            LiteralExpressionSyntax lit => WriteLiteral(lit),
            IdentifierNameSyntax id => WriteIdentifier(id),
            PrefixUnaryExpressionSyntax pre => pre.OperatorToken.Text + Write(pre.Operand),
            PostfixUnaryExpressionSyntax post => Write(post.Operand) + post.OperatorToken.Text,
            AssignmentExpressionSyntax assign => WriteAssignment(assign),
            MemberAccessExpressionSyntax mem => WriteMemberAccess(mem),
            InvocationExpressionSyntax inv => WriteInvocation(inv),
            InterpolatedStringExpressionSyntax interp => FormatStringBuilder.BuildToBuffer(interp, _ctx, Write),
            ArrayCreationExpressionSyntax arr => WriteArrayCreation(arr),
            ImplicitArrayCreationExpressionSyntax implArr => WriteImplicitArrayCreation(implArr),
            ObjectCreationExpressionSyntax obj => WriteObjectCreation(obj),
            ParenthesizedExpressionSyntax par => "(" + Write(par.Expression) + ")",
            ConditionalExpressionSyntax cond => WriteConditional(cond),
            CastExpressionSyntax cast => WriteCast(cast),
            ElementAccessExpressionSyntax elem => WriteElementAccess(elem),
            DefaultExpressionSyntax => "NULL",
            ThisExpressionSyntax => "self",
            SwitchExpressionSyntax switchExpr => PatternMatchingWriter.WriteSwitchExpression(switchExpr, _ctx, Write),
            IsPatternExpressionSyntax isPattern => PatternMatchingWriter.WriteIsPattern(isPattern, _ctx, Write),
            ConditionalAccessExpressionSyntax condAccess => WriteConditionalAccess(condAccess),
            _ => node.ToString(),
        };
    }

    // ── Literale ──────────────────────────────────────────────────────────

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
        if (text.EndsWith("f", StringComparison.OrdinalIgnoreCase) && !text.StartsWith("0x")) return text[..^1] + "f";
        if (text.EndsWith("d", StringComparison.OrdinalIgnoreCase)) return text[..^1];
        if (text.EndsWith("m", StringComparison.OrdinalIgnoreCase)) return text[..^1];
        if (text.EndsWith("ul", StringComparison.OrdinalIgnoreCase)) return text[..^2] + "ULL";
        if (text.EndsWith("u", StringComparison.OrdinalIgnoreCase)) return text[..^1] + "U";
        if (text.EndsWith("l", StringComparison.OrdinalIgnoreCase)) return text[..^1] + "LL";
        return text;
    }

    // ── Identifier ────────────────────────────────────────────────────────

    private string WriteIdentifier(IdentifierNameSyntax id)
    {
        var name = id.Identifier.Text;

        // Enum-Mapping zuerst
        var mapped = TypeRegistry.MapEnum(name);
        if (mapped != name) return mapped;

        if (_ctx.EnumMembers.Contains(name)) return name;
        if (name == "_cs2sx_strbuf") return "_cs2sx_strbuf";

        // FIX 13+14: Lokale Variable hat immer absoluten Vorrang —
        // vor Feldzugriffen, vor ControlFields, vor allem.
        if (_ctx.LocalTypes.TryGetValue(name, out var localType))
        {
            // FIX 14: ref/out Parameter → Dereferenzierung beim Lesen
            if (localType.StartsWith("@ref:", StringComparison.Ordinal))
                return "(*" + name + ")";
            return name;
        }

        // Felder MIT _ prefix (klassische Konvention)
        if (_ctx.IsFieldAccess(name))
        {
            var trimmed = name.TrimStart('_');
            if (!_ctx.FieldTypes.ContainsKey(trimmed) && !string.IsNullOrEmpty(_ctx.CurrentClass))
                return _ctx.CurrentClass + "_" + trimmed;
            var prefix = TypeRegistry.HasNoPrefix(trimmed) ? "" : "f_";
            return "self->" + prefix + trimmed;
        }

        // Felder OHNE _ prefix — aber nur wenn der Name in FieldTypes der aktuellen Klasse steht
        if (!string.IsNullOrEmpty(_ctx.CurrentClass)
            && _ctx.FieldTypes.ContainsKey(name))
        {
            var prefix = TypeRegistry.HasNoPrefix(name) ? "" : "f_";
            return "self->" + prefix + name;
        }

        // FIX 8: static const Felder der aktuellen Klasse (z.B. WALL, EMPTY)
        // werden zu ClassName_WALL umgeschrieben damit sie in switch-case funktionieren.
        // Voraussetzung: SemanticModel muss das Symbol als static const kennen.
        if (!string.IsNullOrEmpty(_ctx.CurrentClass) && _ctx.SemanticModel != null)
        {
            try
            {
                var symbolInfo = _ctx.SemanticModel.GetSymbolInfo(id);
                if (symbolInfo.Symbol is Microsoft.CodeAnalysis.IFieldSymbol field
                    && field.IsStatic
                    && (field.IsConst || field.IsReadOnly))
                {
                    var ownerClass = field.ContainingType?.Name ?? _ctx.CurrentClass;
                    return ownerClass + "_" + name;
                }
            }
            catch { }
        }

        // ControlFields (x, y, width, height) NUR als Feld-Zugriff wenn
        // bereits oben keine lokale Variable gefunden wurde
        if (TypeRegistry.ControlFields.Contains(name) && !string.IsNullOrEmpty(_ctx.CurrentClass))
            return "self->base." + name;

        return name;
    }

    // ── Binäre Ausdrücke ──────────────────────────────────────────────────

    private string WriteBinary(BinaryExpressionSyntax bin)
    {
        // FIX 1: String-Konkatenation mit + → snprintf
        if (bin.IsKind(SyntaxKind.AddExpression))
        {
            var concat = StringConcatFixer.TryBuildConcat(bin, _ctx, Write);
            if (concat != null) return concat;
        }

        var left = Write(bin.Left);
        var right = Write(bin.Right);
        var op = bin.OperatorToken.Text;

        if ((op == "==" || op == "!=") && IsStringExpr(bin.Left))
            return "strcmp(" + left + ", " + right + ") " + op + " 0";

        if (bin.IsKind(SyntaxKind.IsExpression))
            return "/* is-check: " + bin + " */ 1";

        return left + " " + op + " " + right;
    }

    // ── ?? Null-Coalescing ────────────────────────────────────────────────

    private string WriteCoalesce(BinaryExpressionSyntax coalesce)
    {
        var left = Write(coalesce.Left);
        var right = Write(coalesce.Right);
        var leftType = TypeInferrer.InferCSharpType(coalesce.Left, _ctx);

        if (NullableHandler.IsNullable(leftType))
        {
            var innerType = NullableHandler.GetInnerType(leftType);
            var isValueType = TypeRegistry.IsPrimitive(innerType);
            return NullableHandler.WriteNullCoalescing(left, right, isValueType);
        }

        var isPrim = TypeRegistry.IsPrimitive(leftType) && leftType != "string";
        if (isPrim) return left;

        return "(" + left + " != NULL ? " + left + " : " + right + ")";
    }

    // ── Conditional Access: x?.Member ────────────────────────────────────

    private string WriteConditionalAccess(ConditionalAccessExpressionSyntax condAccess)
    {
        var receiver = Write(condAccess.Expression);

        string accessExpr;
        if (condAccess.WhenNotNull is MemberBindingExpressionSyntax memberBinding)
        {
            accessExpr = receiver + "->" + memberBinding.Name.Identifier.Text;
        }
        else if (condAccess.WhenNotNull is InvocationExpressionSyntax inv
            && inv.Expression is MemberBindingExpressionSyntax invMember)
        {
            var args = inv.ArgumentList.Arguments.Select(a => Write(a.Expression));
            accessExpr = receiver + "->" + invMember.Name.Identifier.Text
                       + "(" + string.Join(", ", args) + ")";
        }
        else
        {
            accessExpr = receiver + "->(" + Write(condAccess.WhenNotNull) + ")";
        }

        return NullableHandler.WriteNullConditional(receiver, accessExpr);
    }

    // ── Zuweisungen ───────────────────────────────────────────────────────

    private string WriteAssignment(AssignmentExpressionSyntax assign)
    {
        var op = assign.OperatorToken.Text;
        var right = Write(assign.Right);

        if (op == "??=")
            return WriteNullCoalescingAssignment(assign, right);

        if (assign.Left is MemberAccessExpressionSyntax mem)
            return WriteMemberAssignment(assign, mem, op, right);

        if (assign.Left is ElementAccessExpressionSyntax elemLeft)
            return WriteIndexerAssignment(elemLeft, op, right);

        // FIX 14: ref/out Parameter-Zuweisung → *param = value
        if (assign.Left is IdentifierNameSyntax leftId)
        {
            var lname = leftId.Identifier.Text;
            if (_ctx.LocalTypes.TryGetValue(lname, out var lt2)
                && lt2.StartsWith("@ref:", StringComparison.Ordinal))
                return "*" + lname + " " + op + " " + right;
        }

        return Write(assign.Left) + " " + op + " " + right;
    }

    private string WriteNullCoalescingAssignment(
        AssignmentExpressionSyntax assign, string right)
    {
        var target = Write(assign.Left);
        _ctx.Out.WriteLine(_ctx.Tab + "if (" + target + " == NULL)");
        _ctx.Out.WriteLine(_ctx.Tab + "{");
        _ctx.Out.WriteLine(_ctx.Tab + "    " + target + " = " + right + ";");
        _ctx.Out.WriteLine(_ctx.Tab + "}");
        return "";
    }

    private string WriteMemberAssignment(AssignmentExpressionSyntax assign,
        MemberAccessExpressionSyntax mem, string op, string right)
    {
        var obj = Write(mem.Expression);
        var prop = mem.Name.Identifier.Text;
        var objRaw = mem.Expression.ToString();
        var objKey = objRaw.TrimStart('_');

        string? lt = null, ft = null;
        _ctx.LocalTypes.TryGetValue(objRaw, out lt);
        _ctx.FieldTypes.TryGetValue(objKey, out ft);

        bool isStruct = (lt != null && TypeRegistry.IsLibNxStruct(lt))
                     || (ft != null && TypeRegistry.IsLibNxStruct(ft));
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
                return "Label_SetText(" + obj + ", \""
                     + StringEscaper.EscapeRaw(litStr.Token.ValueText) + "\")";
            return "Label_SetText(" + obj + ", " + right + ")";
        }

        if (prop == "OnClick")
        {
            var methodName = assign.Right.ToString().Trim();
            return obj + "->OnClick = (void(*)(void*))" + _ctx.CurrentClass + "_" + methodName;
        }

        var fieldType = lt ?? ft;
        if (fieldType != null && NullableHandler.IsNullable(fieldType) && right != "NULL")
        {
            var tmp = _ctx.NextTmp("nval");
            var inner = NullableHandler.GetInnerType(fieldType);
            var innerC = TypeRegistry.MapType(inner);
            _ctx.Out.WriteLine(_ctx.Tab + "static " + innerC + " " + tmp + " = " + right + ";");
            return obj + arrow + "f_" + objKey + " = &" + tmp;
        }

        var cProp = TypeRegistry.MapProperty(prop);
        return obj + arrow + cProp + " " + op + " " + right;
    }

    private string WriteIndexerAssignment(
        ElementAccessExpressionSyntax elemLeft, string op, string right)
    {
        var obj = Write(elemLeft.Expression);
        var key = Write(elemLeft.ArgumentList.Arguments[0].Expression);
        var objRaw = elemLeft.Expression.ToString();
        var objKey = objRaw.TrimStart('_');

        string? lt = null, ft = null;
        _ctx.LocalTypes.TryGetValue(objRaw, out lt);
        _ctx.FieldTypes.TryGetValue(objKey, out ft);

        bool isDict = (lt != null && TypeRegistry.IsDictionary(lt))
                   || (ft != null && TypeRegistry.IsDictionary(ft));
        if (isDict)
        {
            var dictType = lt ?? ft!;
            var types = TypeRegistry.GetDictionaryTypes(dictType)!.Value;
            var cKey = types.key == "string" ? "str" : TypeRegistry.MapType(types.key);
            var cVal = types.val == "string" ? "str" : TypeRegistry.MapType(types.val);
            return "Dict_" + cKey + "_" + cVal + "_Set(" + obj + ", " + key + ", " + right + ")";
        }

        return obj + "[" + key + "] = " + right;
    }

    // ── Member-Zugriff ────────────────────────────────────────────────────

    private string WriteMemberAccess(MemberAccessExpressionSyntax mem)
    {
        var full = mem.ToString();

        // FIX 10: int.MaxValue / int.MinValue etc.
        if (IsNumericTypeMember(mem, out var constResult))
            return constResult;

        var mapped = TypeRegistry.MapEnum(full);
        if (mapped != full) return mapped;

        if (full.StartsWith("LibNX.", StringComparison.Ordinal))
            return mem.Name.Identifier.Text;

        var obj = Write(mem.Expression);
        var prop = mem.Name.Identifier.Text;

        if (mem.Expression is BaseExpressionSyntax)
            return "((Control*)self)->" + prop.ToLowerInvariant();

        if (prop == "Length" && IsStringExpr(mem.Expression))
            return "strlen(" + obj + ")";

        if (prop == "Count" && IsListExpr(mem.Expression))
            return obj + "->count";

        if (prop == "Length" && IsStringBuilderExpr(mem.Expression))
            return obj + "->length";

        if (prop == "HasValue" && IsNullableExpr(mem.Expression))
            return NullableHandler.WriteHasValue(obj);

        if (prop == "Value" && IsNullableExpr(mem.Expression))
            return NullableHandler.WriteGetValue(obj);

        var rawExpr = mem.Expression.ToString();
        var key = rawExpr.TrimStart('_');

        // LibNx-Structs → dot-Zugriff
        if ((_ctx.LocalTypes.TryGetValue(rawExpr, out var lt) && IsStructType(lt))
         || (_ctx.FieldTypes.TryGetValue(key, out var ft) && IsStructType(ft)))
            return obj + "." + prop;

        // Auto-Properties von eigenen Klassen haben f_-Prefix
        var receiverType = ResolveReceiverType(rawExpr);
        if (receiverType != null
            && !TypeRegistry.IsLibNxStruct(receiverType)
            && !TypeRegistry.IsLibNxStruct(TypeRegistry.MapType(receiverType).TrimEnd('*'))
            && receiverType is not ("string" or "int" or "uint" or "float"
                                 or "bool" or "char" or "long" or "ulong"
                                 or "short" or "ushort" or "byte" or "sbyte"
                                 or "double" or "u8" or "u16" or "u32" or "u64"
                                 or "s8" or "s16" or "s32" or "s64"))
        {
            if (TypeRegistry.HasNoPrefix(prop))
                return obj + "->" + prop;

            return obj + "->f_" + prop;
        }

        return obj + "->" + prop;
    }

    /// <summary>
    /// FIX 10: int.MaxValue, int.MinValue, float.MaxValue etc.
    /// </summary>
    private static bool IsNumericTypeMember(
        MemberAccessExpressionSyntax mem, out string result)
    {
        var typeName = mem.Expression.ToString();
        var memberName = mem.Name.Identifier.Text;

        result = (typeName, memberName) switch
        {
            ("int", "MaxValue") => "INT_MAX",
            ("int", "MinValue") => "INT_MIN",
            ("uint", "MaxValue") => "UINT_MAX",
            ("uint", "MinValue") => "0U",
            ("long", "MaxValue") => "LLONG_MAX",
            ("long", "MinValue") => "LLONG_MIN",
            ("ulong", "MaxValue") => "ULLONG_MAX",
            ("ulong", "MinValue") => "0ULL",
            ("short", "MaxValue") => "SHRT_MAX",
            ("short", "MinValue") => "SHRT_MIN",
            ("byte", "MaxValue") => "255",
            ("byte", "MinValue") => "0",
            ("float", "MaxValue") => "FLT_MAX",
            ("float", "MinValue") => "FLT_MIN",
            ("float", "Epsilon") => "FLT_EPSILON",
            ("double", "MaxValue") => "DBL_MAX",
            ("double", "MinValue") => "DBL_MIN",
            // FIX: Math-Konstanten
            ("Math", "PI") => "(float)M_PI",
            ("Math", "E") => "(float)M_E",
            ("MathF", "PI") => "3.14159265f",
            ("MathF", "E") => "2.71828182f",
            ("float", "NaN") => "NAN",
            ("float", "PositiveInfinity") => "INFINITY",
            ("float", "NegativeInfinity") => "(-INFINITY)",
            _ => null!,
        };

        return result != null;
    }

    private string? ResolveReceiverType(string rawExpr)
    {
        var key = rawExpr.TrimStart('_');
        if (_ctx.LocalTypes.TryGetValue(rawExpr, out var lt)) return lt;
        if (_ctx.FieldTypes.TryGetValue(key, out var ft)) return ft;
        if (_ctx.FieldTypes.TryGetValue(rawExpr, out var ft2)) return ft2;
        return null;
    }

    // ── Objekt-Erstellung ─────────────────────────────────────────────────

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
            var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
            return "List_" + cInner + "_New()";
        }

        if (TypeRegistry.IsDictionary(typeName))
        {
            var types = TypeRegistry.GetDictionaryTypes(typeName)!.Value;
            var cKey = types.key == "string" ? "str" : TypeRegistry.MapType(types.key);
            var cVal = types.val == "string" ? "str" : TypeRegistry.MapType(types.val);
            return "Dict_" + cKey + "_" + cVal + "_New()";
        }

        // FIX 3: new Random() — kein eigenes Objekt nötig, direkt CS2SX_Rand_Next() verwenden
        if (typeName == "Random")
            return "NULL /* Random — use CS2SX_Rand_Next() directly */";

        var args = obj.ArgumentList?.Arguments.Select(a => Write(a.Expression))
                          ?? Enumerable.Empty<string>();
        var creation = typeName + "_New(" + string.Join(", ", args) + ")";

        if (obj.Initializer?.Expressions.Count > 0)
        {
            var tmp = _ctx.NextTmp(typeName.ToLower());
            var cTypeName = TypeRegistry.MapType(typeName);

            _ctx.Out.WriteLine(_ctx.Tab + cTypeName + "* " + tmp + " = " + creation + ";");

            foreach (var expr in obj.Initializer.Expressions)
            {
                if (expr is not AssignmentExpressionSyntax asgn) continue;

                var propName = asgn.Left.ToString().Trim();
                var propVal = Write(asgn.Right);

                if (propName == "Text")
                {
                    _ctx.Out.WriteLine(_ctx.Tab + "Label_SetText(" + tmp + ", " + propVal + ");");
                    continue;
                }
                if (propName == "OnClick")
                {
                    _ctx.Out.WriteLine(_ctx.Tab + tmp + "->OnClick = (void(*)(void*))"
                        + _ctx.CurrentClass + "_" + propVal.Trim() + ";");
                    continue;
                }

                var cp = TypeRegistry.MapProperty(propName);
                _ctx.Out.WriteLine(_ctx.Tab + tmp + "->" + cp + " = " + propVal + ";");
            }

            return tmp;
        }

        return creation;
    }

    // ── Array-Erstellung ─────────────────────────────────────────────────

    private string WriteArrayCreation(ArrayCreationExpressionSyntax arr)
    {
        var elemType = arr.Type.ElementType.ToString().Trim();
        var cType = TypeRegistry.MapType(elemType);

        // FIX 5: Array mit Initializer → Stack-Array-Inhalt
        if (arr.Initializer != null && arr.Initializer.Expressions.Count > 0)
        {
            var elems = arr.Initializer.Expressions.Select(e => Write(e));
            return "{ " + string.Join(", ", elems) + " }";
        }

        if (arr.Type.RankSpecifiers.Count > 0
            && arr.Type.RankSpecifiers[0].Sizes.Count > 0)
        {
            var size = Write(arr.Type.RankSpecifiers[0].Sizes[0]);
            return "(" + cType + "*)malloc(" + size + " * sizeof(" + cType + "))";
        }

        return "(" + cType + "*)malloc(sizeof(" + cType + "))";
    }

    // FIX 5: new[] { 1, 2, 3 } → { 1, 2, 3 }
    private string WriteImplicitArrayCreation(ImplicitArrayCreationExpressionSyntax implArr)
    {
        var elems = implArr.Initializer.Expressions.Select(e => Write(e));
        return "{ " + string.Join(", ", elems) + " }";
    }

    // ── Sonstiges ─────────────────────────────────────────────────────────

    private static bool IsStructType(string csType) =>
        TypeRegistry.IsLibNxStruct(csType)
        || TypeRegistry.IsLibNxStruct(TypeRegistry.MapType(csType).TrimEnd('*'))
        || csType is "TouchState" or "StickPos" or "BatteryInfo";

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
    {
        var targetType = cast.Type.ToString().Trim();
        var cType = TypeRegistry.MapType(targetType);
        var inner = Write(cast.Expression);

        if (TypeRegistry.IsControlType(targetType) || TypeRegistry.NeedsPointerSuffix(targetType))
            return "((" + cType + "*)" + inner + ")";

        return "((" + cType + ")" + inner + ")";
    }

    private string WriteElementAccess(ElementAccessExpressionSyntax elem)
    {
        var objExpr = Write(elem.Expression);
        var index = Write(elem.ArgumentList.Arguments[0].Expression);
        var objRaw = elem.Expression.ToString();
        var objKey = objRaw.TrimStart('_');

        string? lt = null, ft = null;
        _ctx.LocalTypes.TryGetValue(objRaw, out lt);
        _ctx.FieldTypes.TryGetValue(objKey, out ft);

        bool isDict = (lt != null && TypeRegistry.IsDictionary(lt))
                   || (ft != null && TypeRegistry.IsDictionary(ft));
        if (isDict)
        {
            var dictType = lt ?? ft!;
            var types = TypeRegistry.GetDictionaryTypes(dictType)!.Value;
            var cKey = types.key == "string" ? "str" : TypeRegistry.MapType(types.key);
            var cVal = types.val == "string" ? "str" : TypeRegistry.MapType(types.val);
            return "*Dict_" + cKey + "_" + cVal + "_Get(" + objExpr + ", " + index + ")";
        }

        bool isList = (lt != null && TypeRegistry.IsList(lt))
                   || (ft != null && TypeRegistry.IsList(ft));
        if (isList)
        {
            var listType = lt ?? ft!;
            var inner = TypeRegistry.GetListInnerType(listType)!;
            var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
            return "List_" + cInner + "_Get(" + objExpr + ", " + index + ")";
        }

        return objExpr + "[" + index + "]";
    }

    // ── Hilfsmethoden ─────────────────────────────────────────────────────

    private bool IsStringExpr(SyntaxNode node)
    {
        if (node is LiteralExpressionSyntax lit
            && lit.Token.IsKind(SyntaxKind.StringLiteralToken)) return true;
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

    private bool IsNullableExpr(SyntaxNode node)
    {
        var raw = node.ToString();
        var key = raw.TrimStart('_');
        if (_ctx.LocalTypes.TryGetValue(raw, out var lt)) return NullableHandler.IsNullable(lt);
        if (_ctx.FieldTypes.TryGetValue(key, out var ft)) return NullableHandler.IsNullable(ft);
        return false;
    }
}