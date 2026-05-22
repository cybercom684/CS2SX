// Datei: Transpiler/Writers/ExpressionWriter.cs
//
// FIX: WriteLambda() macht keinen O(n)-StringWriter-Rewrite mehr.
//      Vorher:
//        var prelude = lifter.ConsumePrelude();
//        var sb = _ctx.Out.GetStringBuilder();
//        var existing = sb.ToString();   // kopiert ALLES bisherige
//        sb.Clear();
//        sb.Append(prelude);             // schreibt Prelude vorne
//        sb.Append(existing);            // schreibt Rest dahinter — O(n) pro Lambda
//
//      Jetzt:
//        LambdaLifter.LiftLambda() schreibt Preludes in _ctx.PendingLambdaPreludes.
//        CSharpToC.VisitMethodDeclaration() ruft _ctx.FlushLambdaPreludes() einmalig
//        VOR der Methodensignatur auf. WriteLambda() hier tut nichts weiter als
//        LiftLambda() aufrufen und den Funktionsnamen zurückzugeben.

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
        _dispatcher = new InvocationDispatcher(ctx, Write, new ExtensionMethodHandler());
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
            // ^n  →  (len - n)  — must come before the general PrefixUnary arm
            PrefixUnaryExpressionSyntax hatIdx
                when hatIdx.IsKind(SyntaxKind.IndexExpression)
                => "(-1 - " + Write(hatIdx.Operand) + ") /* ^n index — use inside [] */",
            PrefixUnaryExpressionSyntax pre => WritePrefixUnary(pre),
            PostfixUnaryExpressionSyntax post => Write(post.Operand) + post.OperatorToken.Text,
            AssignmentExpressionSyntax assign => WriteAssignment(assign),
            MemberAccessExpressionSyntax mem => WriteMemberAccess(mem),
            InvocationExpressionSyntax inv => WriteInvocation(inv),
            InterpolatedStringExpressionSyntax interp => FormatStringBuilder.BuildToBuffer(interp, _ctx, Write),
            ArrayCreationExpressionSyntax arr => WriteArrayCreation(arr),
            ImplicitArrayCreationExpressionSyntax implArr => WriteImplicitArrayCreation(implArr),
            ObjectCreationExpressionSyntax obj => WriteObjectCreation(obj),
            ImplicitObjectCreationExpressionSyntax implNew => WriteImplicitObjectCreation(implNew),
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
            AwaitExpressionSyntax awaitExpr => WriteAwait(awaitExpr),
            TypeOfExpressionSyntax typeOf => WriteTypeOf(typeOf),
            SizeOfExpressionSyntax sizeOf => "sizeof(" + TypeRegistry.MapType(sizeOf.Type.ToString().Trim()) + ")",
            RangeExpressionSyntax range => WriteRange(range),
            _ => WriteFallback(node),
        };
    }

    private string WriteFallback(SyntaxNode node)
    {
        var text = node.ToString();
        var typeName = node.GetType().Name.Replace("Syntax", "");
        _ctx.Warn($"unsupported expression '{typeName}' — passed through as-is: {(text.Length > 40 ? text[..40] + "…" : text)}",
            typeName);
        return text;
    }

    private string WritePrefixUnary(PrefixUnaryExpressionSyntax pre)
    {
        var operandExpr = Write(pre.Operand);
        var operandType = TypeInferrer.InferCSharpType(pre.Operand, _ctx);
        var overloaded = OperatorOverloadWriter.TryRewriteUnary(pre, operandType, operandExpr);
        if (overloaded != null) return overloaded;
        return pre.OperatorToken.Text + operandExpr;
    }

    private string WriteAwait(AwaitExpressionSyntax awaitExpr)
    {
        _ctx.Warn(awaitExpr, "await — no async support on Switch; executing inner expression synchronously");
        return Write(awaitExpr.Expression);
    }

    private string WriteTypeOf(TypeOfExpressionSyntax typeOf)
    {
        var typeName = typeOf.Type.ToString().Trim();
        // typeof(T) has no runtime representation in C; return a string constant
        return "((void*)0) /* typeof(" + typeName + ") — no runtime type info in C */";
    }

    // ── Lambda ────────────────────────────────────────────────────────────────

    private string WriteLambda(LambdaExpressionSyntax lambda)
    {
        // FIX: Kein StringWriter-Rewrite mehr.
        // LambdaLifter.LiftLambda() schreibt das Prelude (Struct-Def + Funktionsdef)
        // in _ctx.PendingLambdaPreludes. CSharpToC.VisitMethodDeclaration() flusht
        // diese einmalig VOR der Methodensignatur via _ctx.FlushLambdaPreludes().
        var lifter = new LambdaLifter(_ctx, this);
        var stmtWriter = new StatementWriter(_ctx, this);
        lifter.SetStatementWriter(stmtWriter);
        return lifter.LiftLambda(lambda);
    }

    // ── Identifier ────────────────────────────────────────────────────────────

    private string WriteIdentifier(IdentifierNameSyntax id)
    {
        var name = id.Identifier.Text;

        var mapped = TypeRegistry.MapEnum(name);
        if (mapped != name) return mapped;

        if (_ctx.EnumMembers.Contains(name)) return name;
        if (name == "_cs2sx_strbuf") return "_cs2sx_strbuf";

        if (_ctx.LocalTypes.TryGetValue(name, out var localType))
        {
            if (localType.StartsWith("@ref:", StringComparison.Ordinal))
                return "(*" + name + ")";
            if (localType.StartsWith("__kvp__", StringComparison.Ordinal))
                return name + "_Key";
            if (localType == "__exception__")
                return "_ex_msg";
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

    private List<string> WriteArguments(SeparatedSyntaxList<ArgumentSyntax> args)
    {
        return args
            .Select(a => a.Expression is null ? string.Empty : Write(a.Expression))
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList();
    }

    // ── Tuple ─────────────────────────────────────────────────────────────────

    private string WriteTuple(TupleExpressionSyntax tuple)
    {
        var elements = tuple.Arguments.Select(a => Write(a.Expression)).ToList();

        if (!string.IsNullOrEmpty(_ctx.CurrentTupleReturnType))
        {
            var typeName = _ctx.CurrentTupleReturnType;
            // Support up to 8 tuple elements (ValueTuple max); warn on overflow
            var fields = new[] { "item1", "item2", "item3", "item4", "item5", "item6", "item7", "item8" };
            if (elements.Count > fields.Length)
            {
                _ctx.Warn($"Tuple mit {elements.Count} Elementen — maximal {fields.Length} unterstützt; überzählige Felder ignoriert.");
                elements = elements[..fields.Length];
            }
            var assigns = elements
                .Select((e, i) => $".{fields[i]} = {e}")
                .ToList();
            return $"({typeName}){{{string.Join(", ", assigns)}}}";
        }

        return $"{{ {string.Join(", ", elements)} }}";
    }

    // ── Literal ───────────────────────────────────────────────────────────────

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

    // ── Binary ────────────────────────────────────────────────────────────────

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

        // Operator overload check — try left type first
        var leftCsType = TypeInferrer.InferCSharpType(bin.Left, _ctx);
        var overloaded = OperatorOverloadWriter.TryRewriteBinary(bin, leftCsType, left, right);
        if (overloaded != null) return overloaded;
        // Try right type (for symmetric operators like == on value types)
        var rightCsType = TypeInferrer.InferCSharpType(bin.Right, _ctx);
        overloaded = OperatorOverloadWriter.TryRewriteBinary(bin, rightCsType, left, right);
        if (overloaded != null) return overloaded;

        if (op == "==" || op == "!=")
        {
            bool lIsStr = IsStringExpr(bin.Left) || IsStringType(bin.Left);
            bool rIsStr = IsStringExpr(bin.Right) || IsStringType(bin.Right);
            if (lIsStr || rIsStr)
            {
                if (IsNullLiteral(bin.Right))
                    return op == "==" ? "String_IsNullOrEmpty(" + left + ")" : "!String_IsNullOrEmpty(" + left + ")";
                if (IsNullLiteral(bin.Left))
                    return op == "==" ? "String_IsNullOrEmpty(" + right + ")" : "!String_IsNullOrEmpty(" + right + ")";
                return "CS2SX_strcmp_safe(" + left + ", " + right + ") " + op + " 0";
            }
        }

        if (bin.IsKind(SyntaxKind.IsExpression))
        {
            var isTypeName = bin.Right.ToString().Trim();
            if (TypeRegistry.IsPrimitive(isTypeName) && isTypeName != "string")
                return "1";
            return "(" + left + " != NULL)";
        }

        // DateTime - DateTime → TimeSpan
        if (op == "-" && leftCsType == "DateTime" && rightCsType == "DateTime")
            return "CS2SX_DateTime_Subtract((time_t)" + left + ", (time_t)" + right + ")";

        // TimeSpan + TimeSpan / TimeSpan - TimeSpan
        if (leftCsType == "TimeSpan" || rightCsType == "TimeSpan")
        {
            if (op == "+") return "CS2SX_TimeSpan_Add(" + left + ", " + right + ")";
            if (op == "-") return "CS2SX_TimeSpan_Sub(" + left + ", " + right + ")";
        }

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
            var args = WriteArguments(inv.ArgumentList.Arguments);
            accessExpr = receiver + "->" + invMember.Name.Identifier.Text
                       + "(" + string.Join(", ", args) + ")";
        }
        else
            accessExpr = receiver + "->(" + Write(condAccess.WhenNotNull) + ")";
        return NullableHandler.WriteNullConditional(receiver, accessExpr);
    }

    // ── Assignment ────────────────────────────────────────────────────────────

    private string WriteAssignment(AssignmentExpressionSyntax assign)
    {
        var op = assign.OperatorToken.Text;
        var right = Write(assign.Right);

        // Add at the start of WriteAssignment, before existing op checks:
        if (op == "+=" || op == "-=")
        {
            var leftRaw = assign.Left.ToString().Trim();
            var leftKey = leftRaw.TrimStart('_');

            string? fieldType = null;
            _ctx.LocalTypes.TryGetValue(leftRaw, out fieldType);
            if (fieldType == null) _ctx.FieldTypes.TryGetValue(leftKey, out fieldType);

            if (fieldType != null
                && (fieldType == "Action" || fieldType.StartsWith("Action<")
                    || fieldType.StartsWith("Func<") || fieldType == "EventHandler"))
            {
                // Delegate combine/remove — emit a simple assignment for single subscriber,
                // or use a handler list for multiple subscribers.
                // Simple approach: last subscriber wins for single delegates (+=),
                // clear for -=. Emit a warning that multicast is simplified.
                _ctx.Warn($"Delegate {op} on '{leftRaw}': multicast simplified to last-subscriber-wins",
                          leftRaw);

                var leftExpr = Write(assign.Left);
                if (op == "+=")
                {
                    var cDelegateType = LambdaLifter.MapDelegateType(fieldType);
                    return leftExpr + " = (" + cDelegateType + ")" + right;
                }
                else // -=
                    return leftExpr + " = NULL /* unsubscribed */";
            }
        }

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
        var tmpName = _ctx.NextTmp("tup");
        // FIX: __auto_type speichert das Tuple-Ergebnis ohne den exakten C-Struct-Typ zu kennen.
        // Zuvor wurde nur int=0 deklariert und der eigentliche Aufruf/Zuweisung nie emittiert.
        _ctx.Out.WriteLine(_ctx.Tab + "__auto_type " + tmpName + " = " + right + ";");
        var fields = new[] { "item1", "item2", "item3", "item4", "item5", "item6", "item7" };
        for (int i = 0; i < names.Count; i++)
        {
            var varName = names[i];
            if (varName == "_") continue;
            var fieldName = i < fields.Length ? fields[i] : "item" + (i + 1);
            if (!_ctx.LocalTypes.ContainsKey(varName))
            {
                _ctx.Out.WriteLine(_ctx.Tab + "__auto_type " + varName + " = " + tmpName + "." + fieldName + ";");
                _ctx.LocalTypes[varName] = "var";
            }
            else
            {
                _ctx.Out.WriteLine(_ctx.Tab + varName + " = " + tmpName + "." + fieldName + ";");
            }
        }
        return "";
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

    // ── Invocation ────────────────────────────────────────────────────────────

    private string WriteInvocation(InvocationExpressionSyntax inv)
    {
        // Nullable: x.GetValueOrDefault() / x.GetValueOrDefault(defVal)
        if (inv.Expression is MemberAccessExpressionSyntax nvMem
            && nvMem.Name.Identifier.Text == "GetValueOrDefault"
            && IsNullableExpr(nvMem.Expression))
        {
            var nvObj = Write(nvMem.Expression);
            if (inv.ArgumentList.Arguments.Count > 0)
            {
                var defVal = Write(inv.ArgumentList.Arguments[0].Expression);
                return "(" + nvObj + " != NULL ? *" + nvObj + " : " + defVal + ")";
            }
            return "(" + nvObj + " != NULL ? *" + nvObj + " : 0)";
        }

        var result = _dispatcher.Dispatch(inv);
        if (result != null) return result;

        // FIX: base.Method(args) → BaseType_Method((BaseType*)self, args)
        // Zuvor: ((Control*)self)->methodname — hardcodet Control* und lowercaset den Namen
        if (inv.Expression is MemberAccessExpressionSyntax baseMem
            && baseMem.Expression is BaseExpressionSyntax)
        {
            var baseType = string.IsNullOrEmpty(_ctx.CurrentBaseType) ? "Control" : _ctx.CurrentBaseType;
            var methodName = baseMem.Name.Identifier.Text;
            var callArgs = WriteArguments(inv.ArgumentList.Arguments);
            var allArgs = new List<string> { "((" + baseType + "*)self)" };
            allArgs.AddRange(callArgs);
            return baseType + "_" + methodName + "(" + string.Join(", ", allArgs) + ")";
        }

        if (inv.Expression is MemberAccessExpressionSyntax vtableMem)
        {
            var vtableResult = TryWriteVirtualCall(vtableMem, inv);
            if (vtableResult != null) return vtableResult;

            var directResult = TryWriteDirectUserClassCall(vtableMem, inv);
            if (directResult != null) return directResult;
        }

        var args = inv.ArgumentList.Arguments.Select(a => Write(a.Expression)).ToList();
        var calleeStr = inv.Expression.ToString();
        if (!IsSilentCall(calleeStr))
            _ctx.Warn(inv, $"unrecognized call '{calleeStr}' — passed through as-is; verify generated C compiles");
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
        if (receiverType == null)
            receiverType = _ctx.GetSemanticType(mem.Expression);

        if (receiverType != null && receiverType.EndsWith("*"))
            receiverType = receiverType.TrimEnd('*').Trim();

        if (receiverType == null) return null;
        if (TypeRegistry.IsPrimitive(receiverType)) return null;
        if (TypeRegistry.IsLibNxStruct(receiverType)) return null;
        if (TypeRegistry.IsControlType(receiverType)) return null;
        if (receiverType is "string" or "StringBuilder") return null;

        var callArgs = WriteArguments(inv.ArgumentList.Arguments);

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

    private string? TryWriteDirectUserClassCall(MemberAccessExpressionSyntax mem,
        InvocationExpressionSyntax inv)
    {
        var methodName = mem.Name.Identifier.Text;
        var receiverRaw = mem.Expression.ToString();
        var receiverKey = receiverRaw.TrimStart('_');

        string? receiverType = null;
        _ctx.LocalTypes.TryGetValue(receiverRaw, out receiverType);
        if (receiverType == null)
            _ctx.FieldTypes.TryGetValue(receiverKey, out receiverType);
        if (receiverType == null)
            receiverType = _ctx.GetSemanticType(mem.Expression);

        if (receiverType != null && receiverType.EndsWith("*"))
            receiverType = receiverType.TrimEnd('*').Trim();

        if (receiverType == null) return null;
        if (TypeRegistry.IsPrimitive(receiverType)) return null;
        if (TypeRegistry.IsLibNxStruct(receiverType)) return null;
        if (TypeRegistry.IsControlType(receiverType)) return null;
        if (receiverType is "string" or "StringBuilder") return null;

        var callArgs = WriteArguments(inv.ArgumentList.Arguments);
        var recv = Write(mem.Expression);
        var allArgs = new List<string> { recv };
        allArgs.AddRange(callArgs);
        return receiverType + "_" + methodName + "(" + string.Join(", ", allArgs) + ")";
    }

    private string WriteNullCoalescingAssignment(AssignmentExpressionSyntax assign, string right)
    {
        var target = Write(assign.Left);
        _ctx.Out.WriteLine(_ctx.Tab + "if (" + target + " == NULL)");
        _ctx.Out.WriteLine(_ctx.Tab + "    " + target + " = " + right + ";");
        return target;
    }

    private string WriteMemberAssignment(AssignmentExpressionSyntax assign,
        MemberAccessExpressionSyntax mem, string op, string right)
    {
        var obj = Write(mem.Expression);
        var prop = mem.Name.Identifier.Text;
        var objRaw = mem.Expression.ToString();
        var objKey = objRaw.TrimStart('_');

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
        // SemanticModel fallback for multi-level receivers (e.g. player.Stats.Health = 5)
        if (lt == null && ft == null)
        {
            var semType = _ctx.GetSemanticType(mem.Expression);
            if (semType != null) lt = semType;
        }

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

        // Replace just the OnClick block in WriteMemberAssignment:
        if (prop == "OnClick")
        {
            var methodRaw = assign.Right.ToString().Trim();
            // Resolve the actual containing class of the method — could be CurrentClass,
            // or a static class, or a lambda-lifted function.
            string methodExpr;
            if (assign.Right is LambdaExpressionSyntax lambdaRight)
            {
                var lifter = new LambdaLifter(_ctx, this);
                var stmtWriter = new StatementWriter(_ctx, this);
                lifter.SetStatementWriter(stmtWriter);
                methodExpr = lifter.LiftLambda(lambdaRight, hintType: "Action");
            }
            else if (methodRaw.Contains('.'))
            {
                // Fully qualified: SomeClass.Method → SomeClass_Method
                methodExpr = methodRaw.Replace('.', '_');
            }
            else if (_ctx.SemanticModel != null && assign.Right is IdentifierNameSyntax idRight)
            {
                try
                {
                    var sym = _ctx.SemanticModel.GetSymbolInfo(idRight).Symbol;
                    if (sym is IMethodSymbol ms)
                        methodExpr = ms.ContainingType.Name + "_" + ms.Name;
                    else
                        methodExpr = string.IsNullOrEmpty(_ctx.CurrentClass)
                            ? methodRaw
                            : _ctx.CurrentClass + "_" + methodRaw;
                }
                catch
                {
                    methodExpr = string.IsNullOrEmpty(_ctx.CurrentClass)
                        ? methodRaw
                        : _ctx.CurrentClass + "_" + methodRaw;
                }
            }
            else
            {
                methodExpr = string.IsNullOrEmpty(_ctx.CurrentClass)
                    ? methodRaw
                    : _ctx.CurrentClass + "_" + methodRaw;
            }
            return obj + "->OnClick = (void(*)(void*))" + methodExpr;
        }

        var fieldType = lt ?? ft;
        if (fieldType != null && NullableHandler.IsNullable(fieldType) && right != "NULL")
        {
            var inner = NullableHandler.GetInnerType(fieldType);
            var innerC = TypeRegistry.MapType(inner);
            var fieldExpr = obj + arrow + "f_" + objKey;
            // FIX: heap-allokiert pro Instanz statt static (alle Instanzen teilten denselben Speicher)
            _ctx.Out.WriteLine(_ctx.Tab + "if (!" + fieldExpr + ")");
            _ctx.Out.WriteLine(_ctx.Tab + "    " + fieldExpr + " = (" + innerC + "*)malloc(sizeof(" + innerC + "));");
            _ctx.Out.WriteLine(_ctx.Tab + "*(" + fieldExpr + ") = " + right + ";");
            return "";
        }

        var assignReceiverType = lt ?? ft;
        string cProp;
        if (assignReceiverType != null
            && !TypeRegistry.IsLibNxStruct(assignReceiverType)
            && !TypeRegistry.IsControlType(assignReceiverType)
            && assignReceiverType is not ("string" or "int" or "uint" or "float"
                                       or "bool" or "char" or "long" or "ulong"
                                       or "short" or "ushort" or "byte" or "sbyte"
                                       or "double"))
            cProp = TypeRegistry.HasNoPrefix(prop) ? prop : "f_" + prop;
        else
            cProp = TypeRegistry.MapProperty(prop);

        // Interpolated strings produce a stack snprintf-buffer → dangling pointer if stored in a field.
        // Wrap with _cs2sx_heap_strdup() so the field gets a heap copy.
        // Only applies to user-class fields (Control.Text uses Label_SetText, already handled above).
        var finalRight = right;
        if (assign.Right is InterpolatedStringExpressionSyntax
            && assignReceiverType != null
            && !TypeRegistry.IsLibNxStruct(assignReceiverType)
            && !TypeRegistry.IsControlType(assignReceiverType)
            && assignReceiverType is not ("string" or "int" or "uint" or "float"
                                       or "bool" or "char" or "long" or "ulong"
                                       or "short" or "ushort" or "byte" or "sbyte"
                                       or "double"))
        {
            finalRight = "_cs2sx_heap_strdup(" + right + ")";
        }

        return obj + arrow + cProp + " " + op + " " + finalRight;
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

        // User-defined indexer setter → ClassName_set(obj, key, value)
        var objTypeName = (lt ?? ft ?? "").TrimEnd('*').Trim();
        if (!string.IsNullOrEmpty(objTypeName) && _ctx.IndexerClasses.Contains(objTypeName))
            return objTypeName + "_set(" + obj + ", " + key + ", " + right + ")";

        return obj + "[" + key + "] = " + right;
    }

    // ── MemberAccess ──────────────────────────────────────────────────────────

    private string WriteMemberAccess(MemberAccessExpressionSyntax mem)
    {
        var full = mem.ToString();

        // string.* Konstanten
        if (full == "string.Empty") return "\"\"";
        if (full == "string.IsNullOrEmpty") return "String_IsNullOrEmpty";
        if (full == "string.IsNullOrWhiteSpace") return "String_IsNullOrWhiteSpace";
        if (full == "int.MaxValue") return "INT_MAX";
        if (full == "int.MinValue") return "INT_MIN";

        // typeof(T).Name / typeof(T).FullName → string constant
        if (mem.Expression is TypeOfExpressionSyntax typeOfExpr)
        {
            var tName = typeOfExpr.Type.ToString().Trim();
            var typeofProp = mem.Name.Identifier.Text;
            return typeofProp switch
            {
                "Name"      => "\"" + tName + "\"",
                "FullName"  => "\"" + tName + "\"",
                "IsEnum"    => "(0)",
                "IsClass"   => "(1)",
                "IsValueType" => "(0)",
                _           => "\"" + tName + "\" /* typeof." + typeofProp + " */"
            };
        }

        // DateTime.Now.* properties
        if (full is "DateTime.Now" or "DateTime.UtcNow")
            return "_cs2sx_now()";
        if (full.StartsWith("DateTime.Now.", StringComparison.Ordinal)
         || full.StartsWith("DateTime.UtcNow.", StringComparison.Ordinal))
        {
            var dotIdx = full.LastIndexOf('.');
            var part = full[(dotIdx + 1)..];
            return part switch
            {
                "Year"       => "CS2SX_DateTime_Now_Year()",
                "Month"      => "CS2SX_DateTime_Now_Month()",
                "Day"        => "CS2SX_DateTime_Now_Day()",
                "Hour"       => "CS2SX_DateTime_Now_Hour()",
                "Minute"     => "CS2SX_DateTime_Now_Minute()",
                "Second"     => "CS2SX_DateTime_Now_Second()",
                "DayOfWeek"  => "CS2SX_DateTime_Now_DayOfWeek()",
                "DayOfYear"  => "CS2SX_DateTime_Now_DayOfYear()",
                "Ticks"      => "CS2SX_DateTime_Now_Ticks()",
                "Millisecond"=> "0 /* Millisecond not supported */",
                _            => "0 /* DateTime." + part + " not supported */"
            };
        }

        if (IsNumericTypeMember(mem, out var constResult)) return constResult;

        var mapped = TypeRegistry.MapEnum(full);
        if (mapped != full) return mapped;

        if (full.StartsWith("LibNX.", StringComparison.Ordinal))
            return mem.Name.Identifier.Text;

        var obj = Write(mem.Expression);
        var prop = mem.Name.Identifier.Text;

        // Stopwatch.Elapsed.TotalMilliseconds / TotalSeconds / TotalMinutes
        if (prop is "TotalMilliseconds" or "TotalSeconds" or "TotalMinutes")
        {
            if (mem.Expression is MemberAccessExpressionSyntax elapsedMem
                && elapsedMem.Name.Identifier.Text == "Elapsed")
            {
                var swRaw = elapsedMem.Expression.ToString();
                var swType = ResolveReceiverType(swRaw, elapsedMem.Expression);
                if (swType == "Stopwatch")
                {
                    var swObj2 = Write(elapsedMem.Expression);
                    return prop switch
                    {
                        "TotalMilliseconds" => "CS2SX_Stopwatch_ElapsedMsDouble(" + swObj2 + ")",
                        "TotalSeconds"      => "CS2SX_Stopwatch_ElapsedSecDouble(" + swObj2 + ")",
                        "TotalMinutes"      => "(CS2SX_Stopwatch_ElapsedSecDouble(" + swObj2 + ") / 60.0)",
                        _                   => swObj2
                    };
                }
            }
        }

        // Stopwatch instance properties
        if (prop == "ElapsedMilliseconds")
        {
            var swType = ResolveReceiverType(mem.Expression.ToString(), mem.Expression);
            if (swType == "Stopwatch")
                return "CS2SX_Stopwatch_ElapsedMs(" + obj + ")";
        }
        if (prop == "ElapsedTicks")
        {
            var swType = ResolveReceiverType(mem.Expression.ToString(), mem.Expression);
            if (swType == "Stopwatch")
                return "CS2SX_Stopwatch_ElapsedTicks(" + obj + ")";
        }
        if (prop == "IsRunning")
        {
            var swType = ResolveReceiverType(mem.Expression.ToString(), mem.Expression);
            if (swType == "Stopwatch")
                return obj + "->running";
        }

        // TimeSpan properties
        if (prop is "TotalMilliseconds" or "TotalSeconds" or "TotalMinutes" or "TotalHours" or "TotalDays"
            or "Milliseconds" or "Seconds" or "Minutes" or "Hours" or "Days")
        {
            var tsType = ResolveReceiverType(mem.Expression.ToString(), mem.Expression);
            if (tsType == "TimeSpan")
            {
                return prop switch
                {
                    "TotalMilliseconds" => "CS2SX_TimeSpan_TotalMs(" + obj + ")",
                    "TotalSeconds"      => "CS2SX_TimeSpan_TotalSec(" + obj + ")",
                    "TotalMinutes"      => "CS2SX_TimeSpan_TotalMin(" + obj + ")",
                    "TotalHours"        => "CS2SX_TimeSpan_TotalHours(" + obj + ")",
                    "TotalDays"         => "CS2SX_TimeSpan_TotalDays(" + obj + ")",
                    "Milliseconds"      => "CS2SX_TimeSpan_Milliseconds(" + obj + ")",
                    "Seconds"           => "CS2SX_TimeSpan_Seconds(" + obj + ")",
                    "Minutes"           => "CS2SX_TimeSpan_Minutes(" + obj + ")",
                    "Hours"             => "CS2SX_TimeSpan_Hours(" + obj + ")",
                    "Days"              => "CS2SX_TimeSpan_Days(" + obj + ")",
                    _                   => obj + ".ticks",
                };
            }
        }

        // FIX: base.Prop → ((BaseType*)self)->Prop
        // Zuvor: hardcodet ((Control*)self)->prop.toLower() — falsch für alle Nicht-Control-Hierarchien
        if (mem.Expression is BaseExpressionSyntax)
        {
            var baseType = string.IsNullOrEmpty(_ctx.CurrentBaseType) ? "Control" : _ctx.CurrentBaseType;
            return "((" + baseType + "*)self)->" + prop;
        }

        var rawExpr = mem.Expression.ToString();

        if (_ctx.LocalTypes.TryGetValue(rawExpr, out var exType) && exType == "__exception__")
            return "_ex_msg";

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
        if (prop == "Count" && IsStackQueueHashSetExpr(mem.Expression)) return obj + "->count";
        if (prop == "Length" && IsStringBuilderExpr(mem.Expression)) return obj + "->length";
        if (prop == "HasValue" && IsNullableExpr(mem.Expression)) return NullableHandler.WriteHasValue(obj);
        if (prop == "Value" && IsNullableExpr(mem.Expression)) return NullableHandler.WriteGetValue(obj);
        if (prop == "Count" && IsDictExpr(mem.Expression)) return obj + "->count";
        if (prop is "Keys" or "Values" && IsDictExpr(mem.Expression)) return obj;

        var key = rawExpr.TrimStart('_');
        var receiverType = ResolveReceiverType(rawExpr, mem.Expression);

        if (receiverType != null && IsControlSubclassType(receiverType))
        {
            var controlProp = prop.ToLowerInvariant();
            if (TypeRegistry.ControlFields.Contains(controlProp))
                return $"{obj}->base.{controlProp}";

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

        // Tuple named field access: (int x, int y) t → t.x → t.item1
        if (_ctx.SemanticModel != null)
        {
            try
            {
                var typeInfo = _ctx.SemanticModel.GetTypeInfo(mem.Expression);
                var recvSym = typeInfo.ConvertedType ?? typeInfo.Type;
                if (recvSym is Microsoft.CodeAnalysis.INamedTypeSymbol namedSym
                    && namedSym.IsTupleType)
                {
                    var elements = namedSym.TupleElements;
                    for (int ti = 0; ti < elements.Length; ti++)
                    {
                        var elem = elements[ti];
                        if (elem.Name == prop || elem.Name == "Item" + (ti + 1))
                        {
                            return obj + ".item" + (ti + 1);
                        }
                    }
                }
            }
            catch { }
        }

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

    private string? ResolveReceiverType(string rawExpr, ExpressionSyntax? node = null)
    {
        var key = rawExpr.TrimStart('_');
        if (_ctx.LocalTypes.TryGetValue(rawExpr, out var lt)) return lt;
        if (_ctx.FieldTypes.TryGetValue(key, out var ft)) return ft;
        if (_ctx.FieldTypes.TryGetValue(rawExpr, out var ft2)) return ft2;
        // SemanticModel fallback — handles multi-level access like player.Position.X
        if (node != null)
        {
            var semType = _ctx.GetSemanticType(node);
            if (semType != null) return semType;
        }
        return null;
    }

    // ── ObjectCreation ────────────────────────────────────────────────────────

    private string WriteObjectCreation(ObjectCreationExpressionSyntax obj)
    {
        // User-definierter Generic-Typ (AST-Node vorhanden) — nicht List/Dict
        if (obj.Type is GenericNameSyntax gn
            && !TypeRegistry.IsList(obj.Type.ToString())
            && !TypeRegistry.IsDictionary(obj.Type.ToString()))
        {
            var baseName = gn.Identifier.Text;
            var typeArgs = gn.TypeArgumentList.Arguments
                .Select(a => { var t = a.ToString().Trim(); return t == "string" ? "str" : TypeRegistry.MapType(t); })
                .ToList();
            var cName = baseName + "_" + string.Join("_", typeArgs);
            var ctorArgs = obj.ArgumentList?.Arguments.Select(a => Write(a.Expression))
                ?? Enumerable.Empty<string>();
            return cName + "_New(" + string.Join(", ", ctorArgs) + ")";
        }
        return WriteObjectCreationFromTypeName(obj.Type.ToString(), obj.ArgumentList, obj.Initializer);
    }

    private string WriteImplicitObjectCreation(ImplicitObjectCreationExpressionSyntax obj)
    {
        var typeName = _ctx.GetSemanticType(obj);
        if (typeName == null)
        {
            _ctx.Warn(obj, "new() — Zieltyp nicht bestimmbar (kein SemanticModel oder Typ-Fehler)");
            return "NULL /* new() — type not resolvable */";
        }
        return WriteObjectCreationFromTypeName(typeName, obj.ArgumentList, obj.Initializer);
    }

    private string WriteObjectCreationFromTypeName(
        string typeName,
        ArgumentListSyntax? argList,
        InitializerExpressionSyntax? initializer)
    {
        // new string(char c, int count) → CS2SX_RepeatChar(c, count)
        // new string(char[] arr) or new string(char[] arr, int start, int count)
        if (typeName == "string" && argList != null)
        {
            var a = argList.Arguments;
            if (a.Count == 2)
            {
                var ch    = Write(a[0].Expression);
                var count = Write(a[1].Expression);
                return "CS2SX_RepeatChar(" + ch + ", " + count + ")";
            }
            if (a.Count == 3)
            {
                var arr   = Write(a[0].Expression);
                var start = Write(a[1].Expression);
                var count = Write(a[2].Expression);
                return "CS2SX_SubstrFromChars(" + arr + ", " + start + ", " + count + ")";
            }
            if (a.Count == 1)
                return Write(a[0].Expression); // new string(existingCharPtr)
        }

        if (TypeRegistry.IsStringBuilder(typeName))
        {
            var cap = argList?.Arguments.Count > 0
                ? Write(argList.Arguments[0].Expression)
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
        if (TypeRegistry.IsStack(typeName))
        {
            var inner = TypeRegistry.GetStackInnerType(typeName)!;
            var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
            return "Stack_" + cInner + "_New()";
        }
        if (TypeRegistry.IsQueue(typeName))
        {
            var inner = TypeRegistry.GetQueueInnerType(typeName)!;
            var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
            return "Queue_" + cInner + "_New()";
        }
        if (TypeRegistry.IsHashSet(typeName))
        {
            var inner = TypeRegistry.GetHashSetInnerType(typeName)!;
            var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
            return "HashSet_" + cInner + "_New()";
        }
        // Generischer Typ aus String-Repräsentation (z.B. bei target-typed new)
        var angleIdx = typeName.IndexOf('<');
        if (angleIdx > 0 && typeName.EndsWith(">"))
        {
            var baseName = typeName[..angleIdx];
            var innerStr = typeName[(angleIdx + 1)..^1];
            var typeArgs = SplitTopLevelTypeArgs(innerStr)
                .Select(t => { var s = t.Trim(); return s == "string" ? "str" : TypeRegistry.MapType(s); })
                .ToList();
            var cName = baseName + "_" + string.Join("_", typeArgs);
            var ctorArgs = argList?.Arguments.Select(a => Write(a.Expression))
                ?? Enumerable.Empty<string>();
            return cName + "_New(" + string.Join(", ", ctorArgs) + ")";
        }
        if (typeName == "Random")
            return "NULL /* Random — use CS2SX_Rand_Next() directly */";
        if (typeName == "Stopwatch")
            return "CS2SX_Stopwatch_New()";
        if (_ctx.ValueTypeStructs.Contains(typeName))
        {
            var cType = TypeRegistry.MapType(typeName);
            if (initializer?.Expressions.Count > 0)
            {
                var fields = initializer.Expressions
                    .OfType<AssignmentExpressionSyntax>()
                    .Select(a => "." + a.Left + " = " + Write(a.Right));
                return "(" + cType + "){ " + string.Join(", ", fields) + " }";
            }
            if (argList?.Arguments.Count > 0)
            {
                var vals = argList.Arguments.Select(a => Write(a.Expression));
                return "(" + cType + "){ " + string.Join(", ", vals) + " }";
            }
            return "(" + cType + "){0}";
        }

        var args = argList?.Arguments.Select(a => Write(a.Expression))
                          ?? Enumerable.Empty<string>();
        var creation = typeName + "_New(" + string.Join(", ", args) + ")";

        if (initializer?.Expressions.Count > 0)
        {
            var tmp = _ctx.NextTmp(typeName.ToLower());
            var cTypeName = TypeRegistry.MapType(typeName);
            _ctx.Out.WriteLine(_ctx.Tab + cTypeName + "* " + tmp + " = " + creation + ";");
            foreach (var expr in initializer.Expressions)
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

    private static List<string> SplitTopLevelTypeArgs(string s)
    {
        var result = new List<string>();
        var cur = new System.Text.StringBuilder();
        int depth = 0;
        foreach (char c in s)
        {
            if (c == '<') { depth++; cur.Append(c); }
            else if (c == '>') { depth--; cur.Append(c); }
            else if (c == ',' && depth == 0) { result.Add(cur.ToString()); cur.Clear(); }
            else cur.Append(c);
        }
        if (cur.Length > 0) result.Add(cur.ToString());
        return result;
    }

    // ── Array ─────────────────────────────────────────────────────────────────

    private string WriteArrayCreation(ArrayCreationExpressionSyntax arr)
    {
        var elemType = arr.Type.ElementType.ToString().Trim();
        var cType = elemType == "string" ? "const char*" : TypeRegistry.MapType(elemType);
        if (arr.Initializer != null && arr.Initializer.Expressions.Count > 0)
        {
            var elems = arr.Initializer.Expressions.Select(e => Write(e));
            // C99 compound literal — valid as an expression (assignment, return, argument)
            return "(" + cType + "[]){ " + string.Join(", ", elems) + " }";
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
        // Infer element type from first element when possible
        string cType = "int";
        if (implArr.Initializer?.Expressions.Count > 0)
        {
            var firstType = TypeInferrer.InferCSharpType(implArr.Initializer.Expressions[0], _ctx);
            cType = firstType == "string" ? "const char*" : TypeRegistry.MapType(firstType);
        }
        var elems = implArr.Initializer?.Expressions.Select(e => Write(e)) ?? Enumerable.Empty<string>();
        return "(" + cType + "[]){ " + string.Join(", ", elems) + " }";
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

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
        var objRaw = elem.Expression.ToString();
        var objKey = objRaw.TrimStart('_');

        string? lt = null, ft = null;
        _ctx.LocalTypes.TryGetValue(objRaw, out lt);
        _ctx.FieldTypes.TryGetValue(objKey, out ft);

        // Multi-dimensional array: arr[i, j] → arr[i * cols + j]
        if (elem.ArgumentList.Arguments.Count >= 2)
        {
            var strideKey = "__stride__" + objRaw;
            if (_ctx.LocalTypes.TryGetValue(strideKey, out var stride))
            {
                var idx0 = Write(elem.ArgumentList.Arguments[0].Expression);
                var idx1 = Write(elem.ArgumentList.Arguments[1].Expression);
                return objExpr + "[" + idx0 + " * " + stride + " + " + idx1 + "]";
            }
            // Fallback: emit a warning and use flat index
            _ctx.Warn($"Multi-dim array access on '{objRaw}' without known stride — using flat index",
                      elem.ToString());
            var flatParts = elem.ArgumentList.Arguments.Select(a => Write(a.Expression)).ToList();
            return objExpr + "[" + string.Join("][", flatParts) + "]";
        }

        // ^n index-from-end: arr[^1] → arr[len - 1]
        var rawArg = elem.ArgumentList.Arguments[0].Expression;
        string index;
        if (rawArg.IsKind(SyntaxKind.IndexExpression)
            && rawArg is PrefixUnaryExpressionSyntax hatExpr)
        {
            var n = Write(hatExpr.Operand);
            // Length expression: prefer known array length or ->count for collections
            var collType = lt ?? ft ?? "";
            string lenExpr;
            if (TypeRegistry.IsList(collType))        lenExpr = objExpr + "->count";
            else if (TypeRegistry.IsStack(collType))  lenExpr = objExpr + "->count";
            else if (TypeRegistry.IsQueue(collType))  lenExpr = objExpr + "->count";
            else if (collType == "string")            lenExpr = "(int)strlen(" + objExpr + ")";
            else if (_ctx.ArrayLengths.TryGetValue(objRaw, out var kl)) lenExpr = kl;
            else lenExpr = "(int)(sizeof(" + objExpr + ") / sizeof(" + objExpr + "[0]))";
            index = "(" + lenExpr + " - " + n + ")";
        }
        else
        {
            index = Write(rawArg);
        }

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

        // User-defined indexer (this[T]) → ClassName_get(obj, index)
        var objTypeName = (lt ?? ft ?? "").TrimEnd('*').Trim();
        if (!string.IsNullOrEmpty(objTypeName) && _ctx.IndexerClasses.Contains(objTypeName))
            return objTypeName + "_get(" + objExpr + ", " + index + ")";

        return objExpr + "[" + index + "]";
    }

    // Range expression: 1..3, ^3.., ..^1 etc.
    // Maps to a Substring call when used on strings, or emits a comment for arrays.
    private string WriteRange(RangeExpressionSyntax range)
    {
        var left  = range.LeftOperand  != null ? Write(range.LeftOperand)  : "0";
        var right = range.RightOperand != null ? Write(range.RightOperand) : "-1";
        _ctx.Warn(range, "range expression — use String_Substring or manual loop; emitting stub");
        return "/* range " + left + ".." + right + " — use Substring/manual slice */";
    }

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

    private bool IsStringType(SyntaxNode node)
    {
        var t = TypeInferrer.InferCSharpType(node, _ctx);
        return t == "string";
    }

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

    private bool IsStackQueueHashSetExpr(SyntaxNode node)
    {
        var raw = node.ToString();
        var key = raw.TrimStart('_');
        string? t = null;
        _ctx.LocalTypes.TryGetValue(raw, out t);
        if (t == null) _ctx.FieldTypes.TryGetValue(key, out t);
        return t != null && (TypeRegistry.IsStack(t) || TypeRegistry.IsQueue(t) || TypeRegistry.IsHashSet(t));
    }

    private bool IsNullableExpr(SyntaxNode node)
    {
        var raw = node.ToString();
        var key = raw.TrimStart('_');
        if (_ctx.LocalTypes.TryGetValue(raw, out var lt)) return NullableHandler.IsNullable(lt);
        if (_ctx.FieldTypes.TryGetValue(key, out var ft)) return NullableHandler.IsNullable(ft);
        var semantic = _ctx.GetSemanticType(node);
        if (semantic != null) return NullableHandler.IsNullable(semantic);
        return false;
    }

    // Returns true for calls that should NOT generate a warning when unrecognized.
    private static bool IsSilentCall(string calleeStr)
    {
        if (calleeStr.StartsWith("CS2SX_", StringComparison.Ordinal)) return true;
        if (calleeStr.StartsWith("_cs2sx_", StringComparison.Ordinal)) return true;
        // Plain identifier = likely own method, no dot = no namespace prefix
        if (!calleeStr.Contains('.')) return true;
        // Known C stdlib
        return s_knownCBuiltins.Contains(calleeStr);
    }

    private static readonly HashSet<string> s_knownCBuiltins = new(StringComparer.Ordinal)
    {
        "printf", "sprintf", "snprintf", "fprintf", "puts", "putchar",
        "malloc", "calloc", "realloc", "free",
        "memset", "memcpy", "memmove", "memcmp",
        "strlen", "strcmp", "strncmp", "strcpy", "strncpy", "strcat", "strncat",
        "strstr", "strchr", "strrchr", "strtok",
        "abs", "fabs", "sqrtf", "sinf", "cosf", "tanf", "powf", "floorf", "ceilf",
        "atan2f", "fabsf", "fminf", "fmaxf", "roundf",
        "sqrt", "sin", "cos", "tan", "pow", "floor", "ceil", "atan2",
        "rand", "srand", "exit", "abort",
        "atoi", "atof", "strtol", "strtod",
        "setjmp", "longjmp",
        "qsort", "bsearch",
        "fopen", "fclose", "fread", "fwrite", "fgets", "fputs", "fseek", "ftell",
    };
}