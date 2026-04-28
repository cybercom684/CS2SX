using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using CS2SX.Transpiler.Handlers;

namespace CS2SX.Transpiler.Writers;

public sealed class ExpressionWriter : IExpressionWriter
{
    private readonly TranspilerContext _ctx;
    private readonly InvocationDispatcher _dispatcher;

    public ExpressionWriter(TranspilerContext ctx)
    {
        _ctx = ctx;
        _dispatcher = new InvocationDispatcher(ctx, Write);
    }

    public ExpressionWriter(TranspilerContext ctx, ExtensionMethodHandler extensionHandler)
    {
        _ctx = ctx;
        _dispatcher = new InvocationDispatcher(ctx, Write, extensionHandler);
    }

    public string Write(SyntaxNode? node)
    {
        if (node == null) return "";

        if (node is InvocationExpressionSyntax nameofInv
            && nameofInv.Expression.ToString() == "nameof")
            return WriteNameOf(nameofInv);

        if (node is DeclarationExpressionSyntax declExpr
            && declExpr.Designation is SingleVariableDesignationSyntax singleDesig)
        {
            var typeName = declExpr.Type.ToString().Trim();
            if (typeName != "var")
                _ctx.LocalTypes[singleDesig.Identifier.Text] = typeName;
            return singleDesig.Identifier.Text;
        }

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
            DefaultExpressionSyntax def => WriteDefault(def),
            ThisExpressionSyntax => "self",
            SwitchExpressionSyntax switchExpr => PatternMatchingWriter.WriteSwitchExpression(switchExpr, _ctx, Write),
            IsPatternExpressionSyntax isPattern => PatternMatchingWriter.WriteIsPattern(isPattern, _ctx, Write),
            ConditionalAccessExpressionSyntax condAccess => WriteConditionalAccess(condAccess),
            LambdaExpressionSyntax lambda => WriteLambda(lambda),
            TupleExpressionSyntax tuple => WriteTuple(tuple),
            _ => node.ToString(),
        };
    }

    // Datei: Transpiler/Writers/ExpressionWriter.cs
    // NUR WriteLambda UND WriteIdentifier ERSETZEN

    private string WriteLambda(LambdaExpressionSyntax lambda)
    {
        var lifter = new LambdaLifter(_ctx, this);
        var stmtWriter = new StatementWriter(_ctx, this);
        lifter.SetStatementWriter(stmtWriter);

        var funcName = lifter.LiftLambda(lambda);

        // FIX: Prelude wird per ConsumePrelude() geholt und VOR den bisherigen
        //      Output eingefügt — ein einziger O(n)-Rewrite pro Lambda statt
        //      pro Zeile, und korrekt bei mehreren Lambdas in einer Methode weil
        //      die Preludes in _pendingPreludes geordnet akkumuliert werden.
        if (lifter.HasPrelude)
        {
            var prelude = lifter.ConsumePrelude();
            var sb = _ctx.Out.GetStringBuilder();
            var existing = sb.ToString();
            sb.Clear();
            sb.Append(prelude);
            sb.Append(existing);
        }

        return funcName;
    }

    private string WriteIdentifier(IdentifierNameSyntax id)
    {
        var name = id.Identifier.Text;

        var mapped = TypeRegistry.MapEnum(name);
        if (mapped != name) return mapped;

        if (_ctx.EnumMembers.Contains(name)) return name;
        if (name == "_cs2sx_strbuf") return "_cs2sx_strbuf";

        // LocalTypes VOR IsFieldAccess
        if (_ctx.LocalTypes.TryGetValue(name, out var localType))
        {
            if (localType.StartsWith("@ref:", StringComparison.Ordinal))
                return "(*" + name + ")";
            if (localType.StartsWith("__kvp__", StringComparison.Ordinal))
                return name + "_Key";
            if (localType == "__exception__")
                return "_ex_msg";
            // _-prefixed Capture: "_currentPath" → "currentPath"
            if (name.StartsWith('_') && !name.StartsWith("__"))
            {
                var clean = name.TrimStart('_');
                if (_ctx.LocalTypes.ContainsKey(clean))
                    return clean;
            }
            return name;
        }

        if (!string.IsNullOrEmpty(_ctx.CurrentClass) && _ctx.SemanticModel != null)
        {
            try
            {
                var sym = _ctx.SemanticModel.GetSymbolInfo(id).Symbol;
                if (sym is IFieldSymbol field && field.IsStatic
                    && (field.IsConst || field.IsReadOnly))
                    return (field.ContainingType?.Name ?? _ctx.CurrentClass) + "_" + name;
            }
            catch { }
        }

        if (_ctx.IsFieldAccess(name))
        {
            var trimmed = name.TrimStart('_');
            if (!_ctx.FieldTypes.ContainsKey(trimmed) && !string.IsNullOrEmpty(_ctx.CurrentClass))
                return _ctx.CurrentClass + "_" + trimmed;
            var prefix = TypeRegistry.HasNoPrefix(trimmed) ? "" : "f_";
            return "self->" + prefix + trimmed;
        }

        if (!string.IsNullOrEmpty(_ctx.CurrentClass) && _ctx.FieldTypes.ContainsKey(name))
        {
            var prefix = TypeRegistry.HasNoPrefix(name) ? "" : "f_";
            return "self->" + prefix + name;
        }

        if (TypeRegistry.ControlFields.Contains(name) && !string.IsNullOrEmpty(_ctx.CurrentClass))
            return "self->base." + name;

        return name;
    }

    private string WriteTuple(TupleExpressionSyntax tuple)
    {
        var elements = tuple.Arguments.Select(a => Write(a.Expression)).ToList();

        if (!string.IsNullOrEmpty(_ctx.CurrentTupleReturnType))
        {
            var typeName = _ctx.CurrentTupleReturnType;
            var fields = new[] { "item1", "item2", "item3", "item4", "item5", "item6", "item7" };
            var assigns = elements
                .Select((e, i) => $".{fields[i]} = {e}")
                .ToList();
            return $"({typeName}){{{string.Join(", ", assigns)}}}";
        }

        return $"{{ {string.Join(", ", elements)} }}";
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
        if (text.EndsWith("f", StringComparison.OrdinalIgnoreCase) && !text.StartsWith("0x")) return text[..^1] + "f";
        if (text.EndsWith("d", StringComparison.OrdinalIgnoreCase)) return text[..^1];
        if (text.EndsWith("m", StringComparison.OrdinalIgnoreCase)) return text[..^1];
        if (text.EndsWith("ul", StringComparison.OrdinalIgnoreCase)) return text[..^2] + "ULL";
        if (text.EndsWith("u", StringComparison.OrdinalIgnoreCase)) return text[..^1] + "U";
        if (text.EndsWith("l", StringComparison.OrdinalIgnoreCase)) return text[..^1] + "LL";
        return text;
    }

    private string WriteBinary(BinaryExpressionSyntax bin)
    {
        if (bin.IsKind(SyntaxKind.AddExpression))
        {
            var concat = StringConcatFixer.TryBuildConcat(bin, _ctx, Write);
            if (concat != null) return concat;
        }

        var left = Write(bin.Left);
        var right = Write(bin.Right);
        var op = bin.OperatorToken.Text;

        // String-Vergleich mit strcmp
        if ((op == "==" || op == "!=") && IsStringExpr(bin.Left))
            return "strcmp(" + left + ", " + right + ") " + op + " 0";

        // FIX: string != null / == null → IsNullOrEmpty statt strcmp(..., NULL)
        if (op == "==" && IsNullLiteral(bin.Right) && IsStringType(bin.Left))
            return "String_IsNullOrEmpty(" + left + ")";
        if (op == "!=" && IsNullLiteral(bin.Right) && IsStringType(bin.Left))
            return "!String_IsNullOrEmpty(" + left + ")";
        if (op == "==" && IsNullLiteral(bin.Left) && IsStringType(bin.Right))
            return "String_IsNullOrEmpty(" + right + ")";
        if (op == "!=" && IsNullLiteral(bin.Left) && IsStringType(bin.Right))
            return "!String_IsNullOrEmpty(" + right + ")";

        if (bin.IsKind(SyntaxKind.IsExpression))
            return "/* is-check: " + bin + " */ 1";

        return left + " " + op + " " + right;
    }

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

    private string WriteConditionalAccess(ConditionalAccessExpressionSyntax condAccess)
    {
        var receiver = Write(condAccess.Expression);
        string accessExpr;
        if (condAccess.WhenNotNull is MemberBindingExpressionSyntax memberBinding)
            accessExpr = receiver + "->" + memberBinding.Name.Identifier.Text;
        else if (condAccess.WhenNotNull is InvocationExpressionSyntax inv
            && inv.Expression is MemberBindingExpressionSyntax invMember)
        {
            var args = inv.ArgumentList.Arguments.Select(a => Write(a.Expression));
            accessExpr = receiver + "->" + invMember.Name.Identifier.Text
                       + "(" + string.Join(", ", args) + ")";
        }
        else
            accessExpr = receiver + "->(" + Write(condAccess.WhenNotNull) + ")";
        return NullableHandler.WriteNullConditional(receiver, accessExpr);
    }

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

        if (assign.Left is IdentifierNameSyntax leftId)
        {
            var lname = leftId.Identifier.Text;
            if (_ctx.LocalTypes.TryGetValue(lname, out var lt2)
                && lt2.StartsWith("@ref:", StringComparison.Ordinal))
                return "*" + lname + " " + op + " " + right;

            string? ltIface = null;
            _ctx.LocalTypes.TryGetValue(lname, out ltIface);
            if (ltIface == null) _ctx.FieldTypes.TryGetValue(lname.TrimStart('_'), out ltIface);

            if (ltIface != null && _ctx.InterfaceTypes.Contains(ltIface))
            {
                var rightRaw = assign.Right.ToString().Trim();
                var wrapped = TryWrapAsInterface(rightRaw, right, ltIface);
                if (wrapped != null)
                    return Write(assign.Left) + " " + op + " " + wrapped;
            }
        }

        if (assign.Left is TupleExpressionSyntax tupleLeft)
            return WriteTupleDeconstruction(tupleLeft, right);

        return Write(assign.Left) + " " + op + " " + right;
    }

    private string? TryWrapAsInterface(string exprRaw, string exprCode, string targetIfaceName)
    {
        if (!_ctx.InterfaceTypes.Contains(targetIfaceName)) return null;
        var key = exprRaw.TrimStart('_');
        string? csType = null;
        _ctx.LocalTypes.TryGetValue(exprRaw, out csType);
        if (csType == null) _ctx.FieldTypes.TryGetValue(key, out csType);
        if (csType == null) return null;
        var bareType = csType.TrimEnd('*').Trim();
        if (bareType == targetIfaceName) return null;
        return bareType + "_as_" + targetIfaceName + "(" + exprCode + ")";
    }

    private string WriteTupleDeconstruction(TupleExpressionSyntax tupleLeft, string right)
    {
        var names = tupleLeft.Arguments.Select(a => a.Expression.ToString()).ToList();
        _ctx.Out.WriteLine(_ctx.Tab + "/* tuple deconstruction */");
        for (int i = 0; i < names.Count; i++)
        {
            var varName = names[i];
            if (varName != "_")
            {
                if (!_ctx.LocalTypes.ContainsKey(varName))
                {
                    _ctx.WriteLine("int " + varName + " = 0;");
                    _ctx.LocalTypes[varName] = "int";
                }
            }
        }
        return "/* tuple: " + string.Join(", ", names) + " = " + right + " */";
    }

    private static string WriteNameOf(InvocationExpressionSyntax inv)
    {
        if (inv.ArgumentList.Arguments.Count == 0) return "\"\"";
        var arg = inv.ArgumentList.Arguments[0].Expression;
        var name = arg is MemberAccessExpressionSyntax mem
            ? mem.Name.Identifier.Text
            : arg.ToString().Trim();
        return "\"" + name + "\"";
    }

    private static string WriteDefault(DefaultExpressionSyntax def)
    {
        var csType = def.Type.ToString().Trim();
        if (TypeRegistry.IsPrimitive(csType) && csType != "string") return "0";
        if (csType == "bool") return "0";
        return "NULL";
    }

    private string WriteInvocation(InvocationExpressionSyntax inv)
    {
        var result = _dispatcher.Dispatch(inv);
        if (result != null) return result;

        if (inv.Expression is MemberAccessExpressionSyntax vtableMem)
        {
            var vtableResult = TryWriteVirtualCall(vtableMem, inv);
            if (vtableResult != null) return vtableResult;
        }

        var args = inv.ArgumentList.Arguments.Select(a => Write(a.Expression)).ToList();
        var calleeStr = inv.Expression.ToString();
        return calleeStr + "(" + string.Join(", ", args) + ")";
    }

    private string? TryWriteVirtualCall(MemberAccessExpressionSyntax mem,
        InvocationExpressionSyntax inv)
    {
        var methodName = mem.Name.Identifier.Text;
        var receiverRaw = mem.Expression.ToString();
        var receiverKey = receiverRaw.TrimStart('_');

        string? receiverType = null;
        _ctx.LocalTypes.TryGetValue(receiverRaw, out receiverType);
        if (receiverType == null)
            _ctx.FieldTypes.TryGetValue(receiverKey, out receiverType);

        if (receiverType != null && receiverType.EndsWith("*"))
            receiverType = receiverType.TrimEnd('*').Trim();

        if (receiverType == null) return null;
        if (TypeRegistry.IsPrimitive(receiverType)) return null;
        if (TypeRegistry.IsLibNxStruct(receiverType)) return null;
        if (TypeRegistry.IsControlType(receiverType)) return null;
        if (receiverType is "string" or "StringBuilder") return null;

        var callArgs = inv.ArgumentList.Arguments.Select(a => Write(a.Expression)).ToList();

        if (_ctx.InterfaceTypes.Contains(receiverType))
        {
            var receiver = Write(mem.Expression);
            var ifaceArgs = new List<string> { receiver + "->obj" };
            ifaceArgs.AddRange(callArgs);
            return receiver + "->vtable->" + methodName
                 + "(" + string.Join(", ", ifaceArgs) + ")";
        }

        if (!_ctx.VTableTypes.Contains(receiverType)) return null;

        var recv = Write(mem.Expression);
        var vtableArgs = new List<string> { recv };
        vtableArgs.AddRange(callArgs);
        return recv + "->vtable->" + methodName
             + "(" + string.Join(", ", vtableArgs) + ")";
    }

    private string WriteNullCoalescingAssignment(AssignmentExpressionSyntax assign, string right)
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

        // FIX: ex.Message = ... → _ex_msg = ...
        if (_ctx.LocalTypes.TryGetValue(objRaw, out var exType) && exType == "__exception__")
            return "_ex_msg " + op + " " + right;

        if (_ctx.LocalTypes.TryGetValue(objRaw, out var kvpType)
            && kvpType.StartsWith("__kvp__", StringComparison.Ordinal))
        {
            var kvpBase = kvpType["__kvp__".Length..];
            return kvpBase + "_" + prop + " = " + right;
        }

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

    private string WriteIndexerAssignment(ElementAccessExpressionSyntax elemLeft, string op, string right)
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

    private string WriteMemberAccess(MemberAccessExpressionSyntax mem)
    {
        var full = mem.ToString();

        if (IsNumericTypeMember(mem, out var constResult)) return constResult;

        var mapped = TypeRegistry.MapEnum(full);
        if (mapped != full) return mapped;

        if (full.StartsWith("LibNX.", StringComparison.Ordinal))
            return mem.Name.Identifier.Text;

        var obj = Write(mem.Expression);
        var prop = mem.Name.Identifier.Text;

        if (mem.Expression is BaseExpressionSyntax)
            return "((Control*)self)->" + prop.ToLowerInvariant();

        var rawExpr = mem.Expression.ToString();

        // Exception-Variable: ex.Message / ex.ToString() → _ex_msg
        if (_ctx.LocalTypes.TryGetValue(rawExpr, out var exType) && exType == "__exception__")
            return "_ex_msg";

        // Dictionary KVP-Zugriff
        if (_ctx.LocalTypes.TryGetValue(rawExpr, out var kvpType)
            && kvpType.StartsWith("__kvp__", StringComparison.Ordinal))
        {
            var baseVar = kvpType["__kvp__".Length..];
            return prop switch
            {
                "Key" => $"{baseVar}_Key",
                "Value" => $"{baseVar}_Value",
                _ => $"{baseVar}_{prop}",
            };
        }

        // Length / Count
        if (prop == "Length")
        {
            if (IsStringExpr(mem.Expression))
                return "strlen(" + obj + ")";
            var rk2 = mem.Expression.ToString();
            var rkey2 = rk2.TrimStart('_');
            if ((_ctx.LocalTypes.TryGetValue(rk2, out var rlt) && rlt is "string" or "char[]" or "const char*")
             || (_ctx.FieldTypes.TryGetValue(rkey2, out var rft) && rft is "string" or "char[]" or "const char*"))
                return "strlen(" + obj + ")";
        }
        if (prop == "Count" && IsListExpr(mem.Expression)) return obj + "->count";
        if (prop == "Length" && IsStringBuilderExpr(mem.Expression)) return obj + "->length";
        if (prop == "HasValue" && IsNullableExpr(mem.Expression)) return NullableHandler.WriteHasValue(obj);
        if (prop == "Value" && IsNullableExpr(mem.Expression)) return NullableHandler.WriteGetValue(obj);
        if (prop == "Count" && IsDictExpr(mem.Expression)) return obj + "->count";
        if (prop is "Keys" or "Values" && IsDictExpr(mem.Expression)) return obj;

        var key = rawExpr.TrimStart('_');
        var receiverType = ResolveReceiverType(rawExpr);

        // FIX: Zugriff auf Basisklassen-Properties/-Felder korrekt mappen
        // Beispiel: button.Focused → ((Button*)button)->focused
        //           label.Text    → korrekt über Label_SetText / Label_Text
        if (receiverType != null && IsControlSubclassType(receiverType))
        {
            var controlProp = prop.ToLowerInvariant();
            if (TypeRegistry.ControlFields.Contains(controlProp))
                return $"{obj}->base.{controlProp}";

            // Button-spezifische Felder
            if (receiverType is "Button" && prop is "Focused" or "focused")
                return $"{obj}->focused";
            if (receiverType is "Button" && prop is "Text" or "text")
                return $"{obj}->text";
            if (receiverType is "Button" && prop is "OnClick")
                return $"{obj}->OnClick";
            if (receiverType is "Label" && prop is "Text" or "text")
                return $"((Label*){obj})->text";
            if (receiverType is "ProgressBar" && prop is "Value" or "value")
                return $"{obj}->value";
            if (receiverType is "ProgressBar" && prop is "WidthChars" or "width_chars")
                return $"{obj}->width_chars";
        }

        // Struct-Typen: . statt ->
        if ((_ctx.LocalTypes.TryGetValue(rawExpr, out var lt) && IsStructType(lt))
         || (_ctx.FieldTypes.TryGetValue(key, out var ft) && IsStructType(ft)))
            return obj + "." + prop;

        if ((_ctx.LocalTypes.TryGetValue(rawExpr, out var vlt) && _ctx.ValueTypeStructs.Contains(vlt))
         || (_ctx.FieldTypes.TryGetValue(key, out var vft) && _ctx.ValueTypeStructs.Contains(vft)))
            return obj + "." + prop;

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

    // Hilfsmethode: prüft ob ein Typ eine bekannte Control-Subklasse ist
    private static bool IsControlSubclassType(string csType) =>
        csType is "Button" or "Label" or "ProgressBar" or "Control" or "Form";

    private static bool IsNumericTypeMember(MemberAccessExpressionSyntax mem, out string result)
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
        if (obj.Type is GenericNameSyntax genericTypeName)
        {
            var baseName = genericTypeName.Identifier.Text;
            var typeArgs = genericTypeName.TypeArgumentList.Arguments
                .Select(a =>
                {
                    var t = a.ToString().Trim();
                    return t == "string" ? "str" : TypeRegistry.MapType(t);
                }).ToList();
            var cName = baseName + "_" + string.Join("_", typeArgs);
            var ctorArgs = obj.ArgumentList?.Arguments.Select(a => Write(a.Expression))
                ?? Enumerable.Empty<string>();
            return cName + "_New(" + string.Join(", ", ctorArgs) + ")";
        }
        if (typeName == "Random")
            return "NULL /* Random — use CS2SX_Rand_Next() directly */";
        if (_ctx.ValueTypeStructs.Contains(typeName))
        {
            var cType = TypeRegistry.MapType(typeName);
            if (obj.Initializer?.Expressions.Count > 0)
            {
                var fields = obj.Initializer.Expressions
                    .OfType<AssignmentExpressionSyntax>()
                    .Select(a => "." + a.Left + " = " + Write(a.Right));
                return "(" + cType + "){ " + string.Join(", ", fields) + " }";
            }
            if (obj.ArgumentList?.Arguments.Count > 0)
            {
                var vals = obj.ArgumentList.Arguments.Select(a => Write(a.Expression));
                return "(" + cType + "){ " + string.Join(", ", vals) + " }";
            }
            return "(" + cType + "){0}";
        }

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

    private string WriteArrayCreation(ArrayCreationExpressionSyntax arr)
    {
        var elemType = arr.Type.ElementType.ToString().Trim();
        var cType = TypeRegistry.MapType(elemType);
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

    private string WriteImplicitArrayCreation(ImplicitArrayCreationExpressionSyntax implArr)
    {
        var elems = implArr.Initializer.Expressions.Select(e => Write(e));
        return "{ " + string.Join(", ", elems) + " }";
    }

    private static bool IsStructType(string csType) =>
        TypeRegistry.IsLibNxStruct(csType)
        || TypeRegistry.IsLibNxStruct(TypeRegistry.MapType(csType).TrimEnd('*'))
        || csType is "TouchState" or "StickPos" or "BatteryInfo";

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

    // ── Hilfsmethoden ─────────────────────────────────────────────────────────

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

    // FIX: string-Typ-Erkennung für != null Check
    private bool IsStringType(SyntaxNode node)
    {
        var t = TypeInferrer.InferCSharpType(node, _ctx);
        return t == "string";
    }

    // FIX: null-Literal-Erkennung
    private static bool IsNullLiteral(SyntaxNode node) =>
        node is LiteralExpressionSyntax lit
        && lit.IsKind(SyntaxKind.NullLiteralExpression);

    private bool IsListExpr(SyntaxNode node)
    {
        var raw = node.ToString();
        var key = raw.TrimStart('_');
        return (_ctx.LocalTypes.TryGetValue(raw, out var lt) && TypeRegistry.IsList(lt))
            || (_ctx.FieldTypes.TryGetValue(key, out var ft) && TypeRegistry.IsList(ft));
    }

    private bool IsDictExpr(SyntaxNode node)
    {
        var raw = node.ToString();
        var key = raw.TrimStart('_');
        return (_ctx.LocalTypes.TryGetValue(raw, out var lt) && TypeRegistry.IsDictionary(lt))
            || (_ctx.FieldTypes.TryGetValue(key, out var ft) && TypeRegistry.IsDictionary(ft));
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