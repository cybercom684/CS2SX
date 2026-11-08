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
using CS2SX.Transpiler;
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
        if (node is ExpressionSyntax mg && TryWriteMethodGroupConversion(mg) is { } dg)
            return dg;
        if (node is CastExpressionSyntax userCast && UserConversion(_ctx.SemanticModel?.GetSymbolInfo(userCast).Symbol) is { } castFn)
            return castFn + "(" + Write(userCast.Expression) + ")";
        var code = WriteCore(node);
        if (node is not ExpressionSyntax expr) return code;
        code = ApplyUserConversion(expr, code);
        code = ApplyEscapeRetain(expr, code);
        return ApplyNullableConversion(expr, code);
    }

    /// A reference-counted object that is kept by something that does not count —
    /// an array initializer element, an interface value or an `object` — takes a
    /// reference here, so releases elsewhere cannot destroy it under them.
    private string ApplyEscapeRetain(ExpressionSyntax expr, string code)
    {
        if (_ctx.SemanticModel == null || code.Length == 0) return code;
        try
        {
            var ti = _ctx.SemanticModel.GetTypeInfo(expr);
            if (Arc.Root(ti.Type) == null) return code;
            bool arrayElement = expr.Parent is InitializerExpressionSyntax init
                                && init.IsKind(SyntaxKind.ArrayInitializerExpression);
            bool boxed = ti.ConvertedType is { TypeKind: TypeKind.Interface }
                         || ti.ConvertedType?.SpecialType == SpecialType.System_Object;
            // (node, 1) — a tuple element (not a deconstruction target)
            bool tupleElement = expr.Parent is ArgumentSyntax { Parent: TupleExpressionSyntax te }
                                && !(te.Parent is AssignmentExpressionSyntax a && a.Left == te);
            // holder.Node = x / new Holder { Node = x } on a struct member
            bool structMember = expr.Parent is AssignmentExpressionSyntax { Left: var left } asg
                                && asg.Right == expr
                                && _ctx.SemanticModel.GetSymbolInfo(left).Symbol is IFieldSymbol or IPropertySymbol
                                && _ctx.SemanticModel.GetSymbolInfo(left).Symbol!.ContainingType?.TypeKind == TypeKind.Struct;
            return arrayElement || boxed || tupleElement || structMember
                ? Arc.RetainIfShared(expr, code, _ctx) : code;
        }
        catch { return code; }
    }

    /// C function of a user-defined conversion operator (`implicit operator int(M m)`
    /// → M_implicit_int), or null when <paramref name="symbol"/> is none.
    private static string? UserConversion(ISymbol? symbol) =>
        symbol is IMethodSymbol { MethodKind: MethodKind.Conversion } m && TranspilerContext.IsUserCode(m)
            ? OperatorOverloadWriter.ConversionFunctionName(m.ContainingType.Name,
                TranspilerContext.FormatTypeSymbol(m.ReturnType), m.Name == "op_Implicit")
            : null;

    /// Implicit user-defined conversions where C# applies them (initializer,
    /// assignment, argument, return): `int x = m;` → `int x = M_implicit_int(m);`.
    private string ApplyUserConversion(ExpressionSyntax expr, string code)
    {
        if (_ctx.SemanticModel == null || code.Length == 0) return code;
        try
        {
            var conv = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetConversion(_ctx.SemanticModel, expr);
            return conv.IsUserDefined && UserConversion(conv.MethodSymbol) is { } fn
                ? fn + "(" + code + ")"
                : code;
        }
        catch { return code; }
    }

    /// A method group converted to a delegate (`ev += OnChanged`, `Run(Helper.Parse)`)
    /// becomes a delegate object bound to its receiver — see DelegateModel.MethodGroup.
    private string? TryWriteMethodGroupConversion(ExpressionSyntax expr)
    {
        if (_ctx.SemanticModel == null
            || expr is not (IdentifierNameSyntax or MemberAccessExpressionSyntax))
            return null;
        try
        {
            if (!DelegateModel.IsDelegate(_ctx.SemanticModel.GetTypeInfo(expr).ConvertedType))
                return null;
            var info = _ctx.SemanticModel.GetSymbolInfo(expr);
            var method = info.Symbol as IMethodSymbol
                ?? info.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
            if (method == null || method.MethodKind != MethodKind.Ordinary) return null;
            return DelegateModel.MethodGroup(method, expr, _ctx, Write);
        }
        catch { return null; }
    }

    /// <summary>
    /// The implicit conversions T → T? and null → T? (for nullable VALUE types), applied
    /// wherever C# applies them — assignment, initializer, argument, return — because the
    /// semantic model reports them on exactly that node: <c>5</c> → <c>((Nullable_int){ 5, 1 })</c>,
    /// <c>null</c> → <c>((Nullable_int){0})</c>.
    /// </summary>
    private string ApplyNullableConversion(ExpressionSyntax expr, string code)
    {
        if (_ctx.SemanticModel == null || code.Length == 0) return code;
        try
        {
            var ti = _ctx.SemanticModel.GetTypeInfo(expr);
            if (ti.ConvertedType is not INamedTypeSymbol
                { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } target)
                return code;
            if (ti.Type != null && SymbolEqualityComparer.Default.Equals(ti.Type, ti.ConvertedType))
                return code; // already T?
            bool isNullOrDefault = expr.IsKind(SyntaxKind.NullLiteralExpression)
                                || expr.IsKind(SyntaxKind.DefaultLiteralExpression);
            // No natural type (target-typed `c ? 1 : null`, switch expression, new()):
            // its branches are converted individually — don't wrap the whole again.
            if (ti.Type == null && !isNullOrDefault) return code;

            var inner = TranspilerContext.FormatTypeSymbol(target.TypeArguments[0]);
            if (!TypeRegistry.IsNullableValueInner(inner)) return code;
            var cName = TypeRegistry.NullableCName(inner);

            return isNullOrDefault
                ? NullableHandler.Null(cName)
                : NullableHandler.WrapValue(cName, code);
        }
        catch { return code; }
    }

    private string WriteCore(SyntaxNode? node)
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
            // Predefined type keywords (string, int, bool ...) used as receiver — pass through as-is.
            // The invocation dispatcher / member access handler recognises "string.X", "int.X" etc.
            PredefinedTypeSyntax pred => pred.Keyword.Text,
            IdentifierNameSyntax id => WriteIdentifier(id),
            // ^n outside [] — System.Index has no standalone C equivalent.
            // When used as arr[^n], WriteElementAccess handles it correctly via len-n.
            // Here we can only warn; the expression is meaningless without an array context.
            PrefixUnaryExpressionSyntax hatIdx
                when hatIdx.IsKind(SyntaxKind.IndexExpression)
                => WarnAndReturn(hatIdx, "^n (System.Index) used outside indexer — use arr[^n] form; emitting 0", "0"),
            PrefixUnaryExpressionSyntax pre => WritePrefixUnary(pre),
            // x! (null-forgiving) only silences a compiler warning — no runtime effect.
            PostfixUnaryExpressionSyntax forgive
                when forgive.IsKind(SyntaxKind.SuppressNullableWarningExpression) => Write(forgive.Operand),
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
            InitializerExpressionSyntax initz => WriteArrayInitializerLiteral(initz),
            ThrowExpressionSyntax throwExpr => WriteThrowExpression(throwExpr),
            _ => WriteFallback(node),
        };
    }

    // A bare array/collection initializer used as an expression: `{ 1, 2, 3 }` →
    // a C99 compound literal `(int[]){ 1, 2, 3 }`. Object initializers (`{ A = 1 }`)
    // are consumed by object-creation, not here, so defer those to the fallback.
    private string WriteArrayInitializerLiteral(InitializerExpressionSyntax init)
    {
        if (init.Expressions.Any(e => e is AssignmentExpressionSyntax))
            return WriteFallback(init);

        var elems = init.Expressions.Select(e => Write(e)).ToList();

        string? csElem = null;
        if (_ctx.SemanticModel != null)
        {
            try
            {
                var ti = _ctx.SemanticModel.GetTypeInfo(init);
                if ((ti.ConvertedType ?? ti.Type) is IArrayTypeSymbol ats)
                    csElem = TranspilerContext.FormatTypeSymbol(ats.ElementType);
            }
            catch { }
        }
        if (csElem == null && init.Expressions.Count > 0)
            csElem = TypeInferrer.InferCSharpType(init.Expressions[0], _ctx);
        return ArrayModel.FromInit(ArrayModel.ElemCType(csElem ?? "int"), elems);
    }

    private string WriteFallback(SyntaxNode node)
    {
        var text = node.ToString();
        var typeName = node.GetType().Name.Replace("Syntax", "");
        _ctx.Warn($"unsupported expression '{typeName}' — passed through as-is: {(text.Length > 40 ? text[..40] + "…" : text)}",
            typeName);
        return text;
    }

    private string WarnAndReturn(SyntaxNode node, string message, string replacement)
    {
        _ctx.Warn(node, message);
        return replacement;
    }

    private string WritePrefixUnary(PrefixUnaryExpressionSyntax pre)
    {
        var operandExpr = Write(pre.Operand);
        var operandType = TypeInferrer.InferCSharpType(pre.Operand, _ctx);
        var overloaded = OperatorOverloadWriter.TryRewriteUnary(pre, operandType, operandExpr);
        if (overloaded != null) return overloaded;
        return pre.OperatorToken.Text + operandExpr;
    }

    /// `x ?? throw …`, `c ? v : throw …`, `=> throw …`: an lvalue of the target type
    /// that never yields (cs2sx_exc.h: CS2SX_ThrowExpr does not return).
    private string WriteThrowExpression(ThrowExpressionSyntax throwExpr)
    {
        var ex = "(Exception*)(" + Write(throwExpr.Expression) + ")";
        string cType = "int";
        if (_ctx.SemanticModel != null)
        {
            try
            {
                var target = _ctx.SemanticModel.GetTypeInfo(throwExpr).ConvertedType;
                if (target != null && target is not IErrorTypeSymbol && target.SpecialType != SpecialType.System_Void)
                {
                    var cs = TranspilerContext.FormatTypeSymbol(target);
                    cType = TypeRegistry.MapType(cs);
                    if (!cType.EndsWith("*") && CS2SX.Transpiler.CSharpToC.NeedsPtr(cs)) cType += "*";
                }
            }
            catch { }
        }
        return "(*(" + cType + "*)CS2SX_ThrowExpr(" + ex + "))";
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
        string? hintType = null;
        if (_ctx.SemanticModel != null)
        {
            try
            {
                var typeInfo = _ctx.SemanticModel.GetTypeInfo(lambda);
                var delegateType = typeInfo.ConvertedType ?? typeInfo.Type;
                if (delegateType != null && delegateType is not IErrorTypeSymbol)
                    hintType = TranspilerContext.FormatTypeSymbol(delegateType);
            }
            catch { }
        }
        var lifter = new LambdaLifter(_ctx, this);
        var stmtWriter = new StatementWriter(_ctx, this);
        lifter.SetStatementWriter(stmtWriter);
        // A lambda written as an expression is a value — a delegate object. Handlers
        // that call a lambda inline (LINQ, List.ForEach…) lift it themselves.
        return lifter.LiftLambda(lambda, hintType: hintType, asDelegate: true);
    }

    // ── Identifier ────────────────────────────────────────────────────────────

    private string WriteIdentifier(IdentifierNameSyntax id)
    {
        var name = id.Identifier.Text;

        var mapped = TypeRegistry.MapEnum(name);
        if (mapped != name) return mapped;

        if (_ctx.EnumMembers.Contains(name) && !_ctx.LocalTypes.ContainsKey(name))
        {
            // Bare enum member (`using static`, or inside the enum itself).
            if (_ctx.SemanticModel != null)
            {
                try
                {
                    if (_ctx.SemanticModel.GetSymbolInfo(id).Symbol is IFieldSymbol ef
                        && ef.ContainingType?.TypeKind == TypeKind.Enum
                        && TypeRegistry.IsUserDefinedEnum(ef.ContainingType.Name))
                        return TypeRegistry.EnumMemberCName(ef.ContainingType.Name, ef.Name);
                }
                catch { }
            }
            return name;
        }
        if (name == "_cs2sx_strbuf") return "_cs2sx_strbuf";

        if (_ctx.LocalTypes.TryGetValue(name, out var localType))
        {
            if (localType.StartsWith("@ref:", StringComparison.Ordinal))
                return "(*" + name + ")";
            if (localType.StartsWith("__kvp__", StringComparison.Ordinal))
                return name + "_Key";
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
                if (sym is IFieldSymbol field && field.IsStatic)
                    // Static field access: ClassName_fieldName (strip leading _ from field name)
                    return (field.ContainingType?.Name ?? _ctx.CurrentClass) + "_" + field.Name.TrimStart('_');

                // Static computed property (e.g. static IScreen Current => ...) → call static getter
                if (sym is IPropertySymbol staticProp && staticProp.IsStatic
                    && staticProp.ContainingType?.Name == _ctx.CurrentClass
                    && _ctx.ComputedPropertyNames.Contains(name))
                    return _ctx.CurrentClass + "_get_" + name + "()";

                // Own-class computed property (expression-body) → call getter function
                if (sym is IPropertySymbol ownProp && !ownProp.IsStatic
                    && ownProp.ContainingType?.Name == _ctx.CurrentClass
                    && _ctx.ComputedPropertyNames.Contains(name))
                    return _ctx.CurrentClass + "_get_" + name + "(self)";

                // Inherited instance property → self->base.*
                if (sym is IPropertySymbol prop && !prop.IsStatic
                    && prop.ContainingType?.Name is { } pCont && pCont != _ctx.CurrentClass)
                {
                    // Only use the built-in Control lowercase field names (x, y, width, height)
                    // for the actual runtime Control struct — user-defined classes use f_ prefix.
                    var lower = name.ToLowerInvariant();
                    if (TypeRegistry.IsBuiltinControlType(pCont) && TypeRegistry.ControlFields.Contains(lower))
                        return SelfBaseChain(id, pCont) + lower;
                    var pfx = TypeRegistry.HasNoPrefix(name) ? "" : "f_";
                    return SelfBaseChain(id, pCont) + pfx + name;
                }

                // Inherited instance field → self->base.*
                if (sym is IFieldSymbol baseField && !baseField.IsStatic
                    && baseField.ContainingType?.Name is { } fCont && fCont != _ctx.CurrentClass)
                {
                    var lname = baseField.Name.TrimStart('_');
                    var lower = lname.ToLowerInvariant();
                    if (TypeRegistry.IsBuiltinControlType(fCont) && TypeRegistry.ControlFields.Contains(lower))
                        return SelfBaseChain(id, fCont) + lower;
                    var pfx = TypeRegistry.HasNoPrefix(lname) ? "" : "f_";
                    return SelfBaseChain(id, fCont) + pfx + lname;
                }
            }
            catch { }
        }

        // Computed property on current class → call the generated getter function
        if (!string.IsNullOrEmpty(_ctx.CurrentClass) && _ctx.ComputedPropertyNames.Contains(name))
        {
            // Static class: no self parameter
            var selfArg = _ctx.IsStaticMethod ? "" : "self";
            return _ctx.CurrentClass + "_get_" + name + "(" + selfArg + ")";
        }

        // Inside a user struct's methods/constructor: fields keep their C# names
        // (struct members are emitted unprefixed, like libnx structs) and `self`
        // is a pointer to the struct value.
        if (!string.IsNullOrEmpty(_ctx.CurrentClass)
            && TypeRegistry.IsUserStruct(_ctx.CurrentClass)
            && !_ctx.IsStaticMethod
            && _ctx.FieldTypes.ContainsKey(name)
            && !_ctx.LocalTypes.ContainsKey(name))
            return "self->" + name;

        if (_ctx.IsFieldAccess(name))
        {
            var trimmed = name.TrimStart('_');
            // Static method context: use ClassName_fieldName (no self pointer)
            if (_ctx.IsStaticMethod && !string.IsNullOrEmpty(_ctx.CurrentClass))
                return _ctx.CurrentClass + "_" + trimmed;
            if (!_ctx.FieldTypes.ContainsKey(trimmed) && !string.IsNullOrEmpty(_ctx.CurrentClass))
                return _ctx.CurrentClass + "_" + trimmed;
            var prefix = TypeRegistry.HasNoPrefix(trimmed) ? "" : "f_";
            return "self->" + prefix + trimmed;
        }

        if (!string.IsNullOrEmpty(_ctx.CurrentClass) && _ctx.FieldTypes.ContainsKey(name))
        {
            // Static method context: use ClassName_fieldName
            if (_ctx.IsStaticMethod)
                return _ctx.CurrentClass + "_" + name.TrimStart('_');
            var prefix = TypeRegistry.HasNoPrefix(name) ? "" : "f_";
            return "self->" + prefix + name;
        }

        // Fallback: inherited field via BaseFieldTypes (no semantic model)
        if (!string.IsNullOrEmpty(_ctx.CurrentClass) && _ctx.BaseFieldTypes.ContainsKey(name))
        {
            var lower = name.ToLowerInvariant();
            if (TypeRegistry.ControlFields.Contains(lower))
                return "self->base." + lower;
            var pfx = TypeRegistry.HasNoPrefix(name) ? "" : "f_";
            return "self->base." + pfx + name;
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

        // No explicit return-type context: try to infer tuple type from SemanticModel
        // and generate a proper C compound literal (_TupleN_... struct).
        if (_ctx.SemanticModel != null)
        {
            try
            {
                var typeInfo = _ctx.SemanticModel.GetTypeInfo(tuple);
                var tupleType = (typeInfo.ConvertedType ?? typeInfo.Type)?.ToString() ?? "";
                // Get the CS short-form tuple type
                var csShort = TranspilerContext.FormatTypeSymbol(typeInfo.ConvertedType ?? typeInfo.Type!);
                if (TypeRegistry.IsTuple(csShort))
                {
                    var structName = TypeRegistry.GetTupleStructName(csShort);
                    if (!string.IsNullOrEmpty(structName))
                    {
                        // Struct is defined in _generics.h — no need to emit it here
                        var fields = new[] { "item1", "item2", "item3", "item4", "item5", "item6", "item7", "item8" };
                        var assigns = elements.Take(fields.Length).Select((e, i) => $".{fields[i]} = {e}").ToList();
                        return $"({structName}){{{string.Join(", ", assigns)}}}";
                    }
                }
            }
            catch { }
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
        return lit.Token.IsKind(SyntaxKind.NumericLiteralToken)
            ? CNumericLiteral(lit.Token.Text)
            : lit.Token.Text;
    }

    /// <summary>
    /// C# numeric literal → equivalent C literal.
    ///   1f → 1.0f   2.5F → 2.5f   5d → 5.0   3m → 3.0 (decimal → double)
    ///   10u → 10U   7L → 7LL      9UL → 9ULL  1_000 → 1000
    ///   0xFD / 0x1F stay hex — 'D'/'F' are hex digits there, not suffixes.
    /// </summary>
    internal static string CNumericLiteral(string text)
    {
        text = text.Replace("_", "");   // digit separators are not valid C
        bool isHexOrBin = text.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                       || text.StartsWith("0b", StringComparison.OrdinalIgnoreCase);

        // Integer suffixes (valid for hex/binary too).
        var lower = text.ToLowerInvariant();
        if (lower.EndsWith("ul") || lower.EndsWith("lu")) return text[..^2] + "ULL";
        if (lower.EndsWith("u")) return text[..^1] + "U";
        if (lower.EndsWith("l")) return text[..^1] + "LL";
        if (isHexOrBin) return text;

        // Real suffixes: make sure the C literal is floating-point, not an int.
        static string Real(string digits) =>
            digits.IndexOfAny(['.', 'e', 'E']) >= 0 ? digits : digits + ".0";
        if (lower.EndsWith("f")) return Real(text[..^1]) + "f";
        if (lower.EndsWith("d") || lower.EndsWith("m")) return Real(text[..^1]);
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

        var lifted = TryWriteLiftedNullable(bin, left, right, op);
        if (lifted != null) return lifted;

        // Operator overload check — try left type first
        var leftCsType = TypeInferrer.InferCSharpType(bin.Left, _ctx);
        var overloaded = OperatorOverloadWriter.TryRewriteBinary(bin, leftCsType, left, right);
        if (overloaded != null) return overloaded;
        // Try right type (for symmetric operators like == on value types)
        var rightCsType = TypeInferrer.InferCSharpType(bin.Right, _ctx);
        overloaded = OperatorOverloadWriter.TryRewriteBinary(bin, rightCsType, left, right);
        if (overloaded != null) return overloaded;

        if ((op == "/" || op == "%") && NeedsZeroDivisorCheck(bin))
            return (op == "/" ? "CS2SX_IDIV(" : "CS2SX_IMOD(") + left + ", " + right + ")";

        if (op == "==" || op == "!=")
        {
            bool lIsStr = IsStringExpr(bin.Left) || IsStringType(bin.Left);
            bool rIsStr = IsStringExpr(bin.Right) || IsStringType(bin.Right);
            if (lIsStr || rIsStr)
            {
                // str == null → str == NULL (proper pointer check, not empty-string check)
                if (IsNullLiteral(bin.Right))
                    return left + " " + op + " NULL";
                if (IsNullLiteral(bin.Left))
                    return right + " " + op + " NULL";
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

    /// <summary>
    /// Operators on nullable value types follow C#'s lifting rules. After the implicit
    /// T → T? conversion both operands are Nullable_T structs, each evaluated once:
    ///   x == null / x != null → !(x).has / (x).has
    ///   a == b                → both null, or both set and equal
    ///   a &lt; b (etc.)          → both set and a.value &lt; b.value (false otherwise)
    ///   a + b (etc.)          → null if either is null, else (Nullable_T){ a+b, 1 }
    /// </summary>
    private string? TryWriteLiftedNullable(BinaryExpressionSyntax bin, string left, string right, string op)
    {
        if (!IsNullableExpr(bin.Left) && !IsNullableExpr(bin.Right)) return null;

        if (op is "==" or "!=")
        {
            if (IsNullLiteral(bin.Right)) return op == "==" ? "!(" + left + ").has" : "(" + left + ").has";
            if (IsNullLiteral(bin.Left)) return op == "==" ? "!(" + right + ").has" : "(" + right + ").has";
        }

        var a = _ctx.NextTmp("na");
        var b = _ctx.NextTmp("nb");
        var bind = "__auto_type " + a + " = " + left + "; __auto_type " + b + " = " + right + "; ";
        switch (op)
        {
            case "==":
                return "({ " + bind + a + ".has == " + b + ".has && (!" + a + ".has || " + a + ".value == " + b + ".value); })";
            case "!=":
                return "({ " + bind + "!(" + a + ".has == " + b + ".has && (!" + a + ".has || " + a + ".value == " + b + ".value)); })";
            case "<" or ">" or "<=" or ">=":
                return "({ " + bind + a + ".has && " + b + ".has && " + a + ".value " + op + " " + b + ".value; })";
            case "+" or "-" or "*" or "/" or "%" or "&" or "|" or "^" or "<<" or ">>":
            {
                var resultC = TypeRegistry.MapType(_ctx.GetSemanticType(bin) ?? "int?");
                return "({ " + bind + "(" + a + ".has && " + b + ".has) ? ((" + resultC + "){ "
                     + a + ".value " + op + " " + b + ".value, 1 }) : ((" + resultC + "){0}); })";
            }
            default:
                return null;
        }
    }

    private string WriteCoalesce(BinaryExpressionSyntax coalesce)
    {
        var left = Write(coalesce.Left);
        var right = Write(coalesce.Right);

        if (IsNullableExpr(coalesce.Left))
        {
            // The result is T? only when the fallback is itself nullable (a ?? b with b: T?).
            var resultIsNullable = _ctx.GetSemanticType(coalesce) is { } rt && NullableHandler.IsNullable(rt);
            if (coalesce.Left is IdentifierNameSyntax or MemberAccessExpressionSyntax)
                return NullableHandler.WriteNullCoalescing(left, right, resultIsNullable);
            var t = _ctx.NextTmp("nc");
            return "({ __auto_type " + t + " = " + left + "; "
                 + NullableHandler.WriteNullCoalescing(t, right, resultIsNullable) + "; })";
        }

        // Only skip the null-check when the semantic model positively confirms a
        // non-nullable primitive — the "int" fallback from InferInvocation is not
        // trustworthy enough to suppress ?? (e.g. FirstOrDefault infers "int" but
        // actually returns a nullable pointer).
        var semanticType = _ctx.GetSemanticType(coalesce.Left);
        var confirmedPrim = semanticType != null
            && TypeRegistry.IsPrimitive(semanticType) && semanticType != "string";
        if (confirmedPrim) return left;

        // Reference types: GNU `a ?: b` evaluates `a` exactly once (`GetName() ?? "x"`
        // must not call GetName twice).
        return "(" + left + " ?: " + right + ")";
    }

    private string WriteConditionalAccess(ConditionalAccessExpressionSyntax condAccess)
    {
        var receiver = Write(condAccess.Expression);
        string accessExpr;
        if (condAccess.WhenNotNull is MemberBindingExpressionSyntax memberBinding)
            accessExpr = receiver + "->" + (StoredMemberPath(condAccess.Expression, memberBinding)
                                            ?? memberBinding.Name.Identifier.Text);
        else if (condAccess.WhenNotNull is InvocationExpressionSyntax inv
            && inv.Expression is MemberBindingExpressionSyntax invMember)
        {
            var methodName = invMember.Name.Identifier.Text;
            var invArgs = WriteArguments(inv.ArgumentList.Arguments);
            var argStr = string.Join(", ", invArgs);
            // ?.Invoke() on a delegate → its typed invoker (every entry of a multicast list)
            ITypeSymbol? recvSym = null;
            try { recvSym = _ctx.SemanticModel?.GetTypeInfo(condAccess.Expression).Type; } catch { }
            if (methodName == "Invoke" && DelegateModel.IsDelegate(recvSym))
            {
                accessExpr = DelegateModel.InvokeName(recvSym!) + "(" + receiver
                    + (argStr.Length > 0 ? ", " + argStr : "") + ")";
            }
            else if (methodName == "Invoke")
            {
                accessExpr = receiver + "(" + argStr + ")";
            }
            else
            {
                // Check if receiver is an interface type → dispatch via vtable
                var receiverRaw = condAccess.Expression.ToString().Trim();
                var receiverType = ResolveReceiverType(receiverRaw, condAccess.Expression as ExpressionSyntax);
                var cleanType = receiverType?.TrimEnd('*');
                if (cleanType != null && TypeRegistry.IsRegisteredInterface(cleanType))
                {
                    // Interface method dispatch: iface->vtable->Method(iface->obj, args)
                    var fullArgs = argStr.Length > 0 ? receiver + "->obj, " + argStr : receiver + "->obj";
                    var ifaceCall = receiver + "->vtable->" + methodName + "(" + fullArgs + ")";
                    accessExpr = ifaceCall;
                }
                else
                {
                    var receiverRaw2 = condAccess.Expression.ToString().Trim();
                    var recvType2 = ResolveReceiverType(receiverRaw2, condAccess.Expression as ExpressionSyntax);
                    if (recvType2 == null)
                        recvType2 = _ctx.GetSemanticType(condAccess.Expression);
                    var cleanType2 = recvType2?.TrimEnd('*').Trim();

                    if (cleanType2 != null && _ctx.VTableTypes.Contains(cleanType2))
                    {
                        var vtArgs2 = argStr.Length > 0 ? receiver + ", " + argStr : receiver;
                        accessExpr = receiver + "->vtable->" + methodName + "(" + vtArgs2 + ")";
                    }
                    else if (cleanType2 != null
                             && !TypeRegistry.IsPrimitive(cleanType2)
                             && !TypeRegistry.IsLibNxStruct(cleanType2)
                             && !TypeRegistry.IsControlType(cleanType2)
                             && cleanType2 is not ("string" or "StringBuilder"))
                    {
                        var dcArgs = argStr.Length > 0 ? receiver + ", " + argStr : receiver;
                        accessExpr = cleanType2 + "_" + methodName + "(" + dcArgs + ")";
                    }
                    else
                    {
                        accessExpr = receiver + "->" + methodName + "(" + argStr + ")";
                    }
                }
            }
        }
        else
            accessExpr = receiver + "->(" + Write(condAccess.WhenNotNull) + ")";

        // x?.Dispose() also releases x (see ReleaseAfterDispose)
        if (condAccess.WhenNotNull is InvocationExpressionSyntax { Expression: MemberBindingExpressionSyntax disposeBinding } disposeInv)
            accessExpr = ReleaseAfterDispose(condAccess.Expression, disposeBinding.Name.Identifier.Text,
                disposeInv.ArgumentList.Arguments.Count, accessExpr);

        // Void-returning null-conditional: use (void)0 as null arm to avoid C11 type violation
        bool isVoidCall = false;
        if (_ctx.SemanticModel != null && condAccess.WhenNotNull is InvocationExpressionSyntax voidInv)
        {
            try
            {
                var si = _ctx.SemanticModel.GetSymbolInfo(voidInv);
                var ms = (si.Symbol ?? si.CandidateSymbols.FirstOrDefault()) as IMethodSymbol;
                isVoidCall = ms?.ReturnsVoid == true;
            }
            catch { }
        }
        if (isVoidCall)
            return "(" + receiver + " != NULL ? " + accessExpr + " : (void)0)";

        // x?.Count where Count is a value type: C# makes it int? — a Nullable_int
        // that is empty when x is null (`x?.Count ?? 0` then reads .has/.value).
        try
        {
            if (_ctx.SemanticModel?.GetTypeInfo(condAccess).Type is INamedTypeSymbol
                {
                    OriginalDefinition.SpecialType: SpecialType.System_Nullable_T,
                } nt
                && _ctx.SemanticModel.GetTypeInfo(condAccess.WhenNotNull).Type is { } inner
                && inner.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T)
            {
                var innerCs = TranspilerContext.FormatTypeSymbol(nt.TypeArguments[0]);
                if (TypeRegistry.IsNullableValueInner(innerCs))
                {
                    var cName = TypeRegistry.NullableCName(innerCs);
                    return "(" + receiver + " != NULL ? " + NullableHandler.WrapValue(cName, accessExpr)
                         + " : " + NullableHandler.Null(cName) + ")";
                }
            }
        }
        catch { }

        return NullableHandler.WriteNullConditional(receiver, accessExpr);
    }

    // ── Assignment ────────────────────────────────────────────────────────────

    // Wraps a string RHS in _cs2sx_heap_strdup so a field gets a stable, owned
    // copy. String literals (static) and already-heap/NULL values are left as-is.
    private string StableStringRhs(SyntaxNode? rightExpr, string right)
    {
        if (rightExpr is LiteralExpressionSyntax) return right;
        var t = right.Trim();
        if (t == "NULL" || t == "\"\"" || t.StartsWith("_cs2sx_heap_strdup", StringComparison.Ordinal))
            return right;
        return "_cs2sx_heap_strdup(" + right + ")";
    }

    private bool IsDelegateTyped(ExpressionSyntax expr)
    {
        if (_ctx.SemanticModel != null)
        {
            try
            {
                var sym = _ctx.SemanticModel.GetSymbolInfo(expr).Symbol;
                if (sym is IEventSymbol) return true;
                var t = _ctx.SemanticModel.GetTypeInfo(expr).Type;
                if (t != null) return DelegateModel.IsDelegate(t);
            }
            catch { }
        }
        var raw = expr.ToString().Trim();
        string? type = null;
        _ctx.LocalTypes.TryGetValue(raw, out type);
        if (type == null) _ctx.FieldTypes.TryGetValue(raw.TrimStart('_'), out type);
        return type != null && TypeRegistry.IsDelegate(type);
    }

    private string WriteAssignment(AssignmentExpressionSyntax assign)
    {
        var op = assign.OperatorToken.Text;
        var right = Write(assign.Right);

        // A field of a reference-counted class owns one reference (see Arc): take one
        // for the new value, give up the old one (deferred to the next frame).
        if (op == "=" && OwningStoreRoot(assign.Left) is { } storeRoot)
            return "CS2SX_ARC_STORE(" + storeRoot + ", " + Write(assign.Left) + ", "
                 + Arc.RetainIfShared(assign.Right, right, _ctx) + ")";

        // Delegates/events: `d += h` / `d -= h` build a new invocation list (multicast).
        if ((op == "+=" || op == "-=") && IsDelegateTyped(assign.Left))
        {
            var leftExpr = Write(assign.Left);
            var fn = op == "+=" ? "CS2SX_Delegate_Combine" : "CS2SX_Delegate_Remove";
            return leftExpr + " = " + fn + "(" + leftExpr + ", " + right + ")";
        }

        if (op == "??=")
            return WriteNullCoalescingAssignment(assign, right);

        // x /= y, x %= y on integers: divide-by-zero check like the binary operator.
        if ((op == "/=" || op == "%=") && assign.Left is IdentifierNameSyntax or MemberAccessExpressionSyntax
            && IsIntegralWithRuntimeDivisor(assign.Left, assign.Right))
        {
            var target = Write(assign.Left);
            return target + " = " + (op == "/=" ? "CS2SX_IDIV(" : "CS2SX_IMOD(") + target + ", " + right + ")";
        }

        // string s += "x"  →  s = <concat into pooled buffer>(s, "x")
        // C has no `+=` for const char*, so lower it to a concat + reassign.
        if (op == "+=" && TypeInferrer.InferCSharpType(assign.Left, _ctx) == "string")
        {
            var leftStr = Write(assign.Left);
            var buf = _ctx.NextStringBuf();
            _ctx.Out.WriteLine(_ctx.Tab
                + "snprintf(" + buf + ", CS2SX_STRBUF_SIZE, \"%s%s\", "
                + leftStr + " ? " + leftStr + " : \"\", "
                + right + " ? " + right + " : \"\");");
            return leftStr + " = " + buf;
        }

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

            // A `string` FIELD must own a stable heap copy: the RHS may be a
            // frame-arena buffer (string method result) or a value freed later
            // (e.g. a List<string> element). Without this the field dangles.
            if (op == "=" && !_ctx.LocalTypes.ContainsKey(lname)
                && _ctx.FieldTypes.TryGetValue(lname.TrimStart('_'), out var lfld)
                && lfld == "string")
            {
                return Write(assign.Left) + " = " + StableStringRhs(assign.Right, right);
            }

            string? ltIface = null;
            _ctx.LocalTypes.TryGetValue(lname, out ltIface);
            if (ltIface == null) _ctx.FieldTypes.TryGetValue(lname.TrimStart('_'), out ltIface);

            if (ltIface != null && _ctx.InterfaceTypes.Contains(ltIface))
            {
                var rightRaw = assign.Right.ToString().Trim();
                var wrapped = TryWrapAsInterface(rightRaw, right, ltIface);
                // SemanticModel fallback: when right is a function call (not in LocalTypes/FieldTypes)
                if (wrapped == null && _ctx.SemanticModel != null)
                {
                    try
                    {
                        var rightTypeSym = _ctx.SemanticModel.GetTypeInfo(assign.Right).Type as INamedTypeSymbol;
                        if (rightTypeSym != null && !TypeRegistry.IsRegisteredInterface(rightTypeSym.Name)
                            && TypeRegistry.NeedsPointerSuffix(rightTypeSym.Name))
                            wrapped = rightTypeSym.Name + "_as_" + ltIface + "(" + right + ")";
                    }
                    catch { }
                }
                if (wrapped != null)
                    return Write(assign.Left) + " " + op + " " + wrapped;
            }
        }

        if (assign.Left is TupleExpressionSyntax or DeclarationExpressionSyntax)
            return WriteDeconstruction(assign.Left, assign.Right, right);

        return Write(assign.Left) + " " + op + " " + right;
    }

    /// Root class of the element type when <paramref name="elem"/> indexes an array of a
    /// reference-counted class.
    private string? ArcElementRoot(ElementAccessExpressionSyntax elem)
    {
        try
        {
            return _ctx.SemanticModel?.GetTypeInfo(elem.Expression).Type is IArrayTypeSymbol at
                ? Arc.Root(at.ElementType) : null;
        }
        catch { return null; }
    }

    /// Root class when the assignment target is a field or auto-property of a class
    /// (storage that owns a reference) typed as a reference-counted class; null for
    /// locals, parameters, properties with their own setter and struct members
    /// (struct copies would share the reference).
    private string? OwningStoreRoot(ExpressionSyntax left)
    {
        if (_ctx.SemanticModel == null) return null;
        try
        {
            var sym = _ctx.SemanticModel.GetSymbolInfo(left).Symbol;
            if (sym?.ContainingType is not { TypeKind: TypeKind.Class }) return null;
            if (sym is IFieldSymbol f && !f.IsConst) return Arc.Root(f.Type);
            if (sym is IPropertySymbol p
                && p.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is PropertyDeclarationSyntax pd
                && PropertyWriter.IsAutoProperty(pd))
                return Arc.Root(p.Type);
        }
        catch { }
        return null;
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

    /// `(a, b) = t`, `var (a, b) = t`, `(var a, int b) = t`, discards with `_`:
    /// the tuple is evaluated once into a temporary, then each target gets itemN.
    /// New variables are declared with the element types of the tuple.
    private string WriteDeconstruction(ExpressionSyntax left, ExpressionSyntax rightNode, string right)
    {
        // (name, declares a new variable) per element; name null = discard
        var targets = new List<(string? name, bool isNew)>();
        switch (left)
        {
            case DeclarationExpressionSyntax { Designation: ParenthesizedVariableDesignationSyntax pd }:
                foreach (var v in pd.Variables)
                    targets.Add(v is SingleVariableDesignationSyntax s ? (s.Identifier.Text, true) : (null, false));
                break;
            case TupleExpressionSyntax te:
                foreach (var a in te.Arguments)
                    targets.Add(a.Expression switch
                    {
                        DeclarationExpressionSyntax { Designation: SingleVariableDesignationSyntax s } => (s.Identifier.Text, true),
                        DeclarationExpressionSyntax => (null, false),
                        IdentifierNameSyntax { Identifier.Text: "_" } => (null, false),
                        var e => (Write(e), false),
                    });
                break;
            default:
                return Write(left) + " = " + right;
        }

        IReadOnlyList<ITypeSymbol>? elemTypes = null;
        try
        {
            if (_ctx.SemanticModel?.GetTypeInfo(rightNode).Type is INamedTypeSymbol { IsTupleType: true } tt)
                elemTypes = tt.TupleElements.Select(e => e.Type).ToList();
        }
        catch { }

        var tmp = _ctx.NextTmp("tup");
        _ctx.Out.WriteLine(_ctx.Tab + "__auto_type " + tmp + " = " + right + ";");
        for (int i = 0; i < targets.Count; i++)
        {
            var (name, isNew) = targets[i];
            if (name == null) continue;
            var item = tmp + ".item" + (i + 1);
            if (!isNew)
            {
                _ctx.Out.WriteLine(_ctx.Tab + name + " = " + item + ";");
                continue;
            }
            var csType = elemTypes != null && i < elemTypes.Count
                ? TranspilerContext.FormatTypeSymbol(elemTypes[i]) : null;
            string cType;
            if (csType == null) cType = "__auto_type";
            else
            {
                cType = TypeRegistry.MapType(csType);
                if (!cType.EndsWith("*") && CS2SX.Transpiler.CSharpToC.NeedsPtr(csType)) cType += "*";
            }
            _ctx.Out.WriteLine(_ctx.Tab + cType + " " + name + " = " + item + ";");
            _ctx.LocalTypes[name] = csType ?? "var";
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
        var code = WriteInvocationCore(inv);
        return inv.Expression is MemberAccessExpressionSyntax mem
            ? ReleaseAfterDispose(mem.Expression, mem.Name.Identifier.Text, inv.ArgumentList.Arguments.Count, code)
            : code;
    }

    /// <summary>
    /// <c>x.Dispose()</c> on a field/local/parameter of reference type also sets x to NULL.
    /// C# allows Dispose twice; in C the second free — by the app or by the class's
    /// generated _Free, which disposes owned fields — would be a double free.
    /// </summary>
    private string ReleaseAfterDispose(ExpressionSyntax receiver, string method, int argCount, string callCode)
    {
        if (method != "Dispose" || argCount != 0 || _ctx.SemanticModel == null) return callCode;
        if (receiver is not (IdentifierNameSyntax or MemberAccessExpressionSyntax)) return callCode;
        try
        {
            var sym = _ctx.SemanticModel.GetSymbolInfo(receiver).Symbol;
            var type = _ctx.SemanticModel.GetTypeInfo(receiver).Type;
            bool assignable = sym is IFieldSymbol { IsReadOnly: false, IsConst: false } or ILocalSymbol
                           || sym is IParameterSymbol { RefKind: RefKind.None };
            if (!assignable || type == null || !type.IsReferenceType) return callCode;
            return "(void)(" + callCode + ", " + Write(receiver) + " = NULL)";
        }
        catch { return callCode; }
    }

    private string WriteInvocationCore(InvocationExpressionSyntax inv)
    {
        // Nullable: x.GetValueOrDefault() / x.GetValueOrDefault(defVal)
        if (inv.Expression is MemberAccessExpressionSyntax nvMem
            && nvMem.Name.Identifier.Text == "GetValueOrDefault"
            && IsNullableExpr(nvMem.Expression))
        {
            var nvObj = Write(nvMem.Expression);
            // A null Nullable_T has value == 0 (zero-initialised), i.e. default(T).
            if (inv.ArgumentList.Arguments.Count > 0)
                return NullableHandler.WriteNullCoalescing(nvObj,
                    Write(inv.ArgumentList.Arguments[0].Expression), resultIsNullable: false);
            return NullableHandler.WriteGetValue(nvObj);
        }

        var result = _dispatcher.Dispatch(inv);
        if (result != null) return result;

        // FIX: base.Method(args) → BaseType_Method((BaseType*)self, args)
        // Zuvor: ((Control*)self)->methodname — hardcodet Control* und lowercaset den Namen
        if (inv.Expression is MemberAccessExpressionSyntax baseMem
            && baseMem.Expression is BaseExpressionSyntax)
        {
            var baseType = string.IsNullOrEmpty(_ctx.CurrentBaseType) ? "Control" : _ctx.CurrentBaseType;
            // Bind to the ancestor that actually implements the method — `base.M()`
            // in Square resolves to Shape.M when Rect (the direct base) doesn't override it.
            if (_ctx.SemanticModel != null)
            {
                try
                {
                    if (_ctx.SemanticModel.GetSymbolInfo(inv).Symbol is IMethodSymbol bm
                        && bm.ContainingType is { TypeKind: TypeKind.Class } owner
                        && TranspilerContext.IsUserCode(owner))
                        baseType = owner.Name;
                }
                catch { }
            }
            var methodName = baseMem.Name.Identifier.Text;
            var callArgs = WriteArguments(inv.ArgumentList.Arguments);
            var allArgs = new List<string> { "((" + baseType + "*)self)" };
            allArgs.AddRange(callArgs);
            var cName = CSharpToC.BuildCMethodName(baseType, methodName,
                inv.ArgumentList.Arguments.Count, _ctx.OverloadedMethods);
            return cName + "(" + string.Join(", ", allArgs) + ")";
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
        var calleeCode = inv.Expression is ElementAccessExpressionSyntax
                         || inv.Expression is MemberAccessExpressionSyntax
            ? Write(inv.Expression)
            : calleeStr;
        if (!IsSilentCall(calleeStr))
            _ctx.Warn(inv, $"unrecognized call '{calleeStr}' — passed through as-is; verify generated C compiles");
        return calleeCode + "(" + string.Join(", ", args) + ")";
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
            // The receiver is used twice (vtable, obj) — evaluate it once.
            var receiver = NullCheck.Wrap(mem.Expression, Write(mem.Expression), _ctx);
            var ifaceArgs = new List<string> { "_cs2sx_r->obj" };
            ifaceArgs.AddRange(callArgs);
            return "({ __auto_type _cs2sx_r = " + receiver + "; _cs2sx_r->vtable->" + methodName
                 + "(" + string.Join(", ", ifaceArgs) + "); })";
        }

        if (!_ctx.VTableTypes.Contains(receiverType)) return null;

        // Only route through the vtable if the method is an actual virtual slot.
        // A non-virtual method on a vtable-bearing class has no vtable member, so
        // dispatching it through ->vtable-> would emit invalid C. When slot info is
        // available and the method isn't a slot, fall through to a direct call.
        if (_ctx.VTableMethods.TryGetValue(receiverType, out var slots)
            && !slots.Contains(methodName))
            return null;

        var recvCode = NullCheck.Wrap(mem.Expression, Write(mem.Expression), _ctx);
        // Used twice (vtable lookup + self) — a non-trivial receiver is evaluated once.
        bool simpleRecv = mem.Expression is ThisExpressionSyntax or IdentifierNameSyntax
            && recvCode == Write(mem.Expression);
        var recv = simpleRecv ? recvCode : "_cs2sx_r";
        var vtableArgs = new List<string> { recv };
        vtableArgs.AddRange(callArgs);

        // The vtable pointer lives in the ROOT class of the hierarchy; a derived
        // class reaches it through its embedded base struct(s). Count the hops up
        // to the topmost vtable ancestor so we emit recv->base.[base.]*vtable.
        int vtHops = 0;
        if (_ctx.SemanticModel != null)
        {
            try
            {
                var ti = _ctx.SemanticModel.GetTypeInfo(mem.Expression);
                var rs = (ti.Type ?? ti.ConvertedType) as INamedTypeSymbol;
                while (rs != null)
                {
                    var b = rs.BaseType;
                    if (b != null && b.TypeKind == TypeKind.Class
                        && b.SpecialType == SpecialType.None
                        && _ctx.VTableTypes.Contains(b.Name))
                    { vtHops++; rs = b; }
                    else break;
                }
            }
            catch { }
        }
        var vtChain = string.Concat(Enumerable.Repeat("base.", vtHops));
        var call = recv + "->" + vtChain + "vtable->" + methodName
             + "(" + string.Join(", ", vtableArgs) + ")";
        return simpleRecv ? call : "({ __auto_type _cs2sx_r = " + recvCode + "; " + call + "; })";
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
        // Generic collection types must be handled by their dedicated handlers;
        // using them as-is would produce invalid C identifiers (e.g. "List<Foo>_Add").
        if (TypeRegistry.IsList(receiverType) || TypeRegistry.IsDictionary(receiverType)
            || TypeRegistry.IsStack(receiverType) || TypeRegistry.IsQueue(receiverType)
            || TypeRegistry.IsHashSet(receiverType)) return null;

        if (TypeRegistry.IsUserStruct(receiverType))
            return WriteStructMethodCall(mem, inv, receiverType);

        var callArgs = WriteArguments(inv.ArgumentList.Arguments);
        callArgs = ApplyUpcasts(inv, callArgs, receiverType);

        // Fill in default values for optional parameters omitted at the call site
        if (_ctx.SemanticModel != null)
        {
            try
            {
                var symInfo = _ctx.SemanticModel.GetSymbolInfo(inv);
                var methodSym = symInfo.Symbol as IMethodSymbol;
                if (methodSym != null && methodSym.Parameters.Length > callArgs.Count)
                {
                    for (int di = callArgs.Count; di < methodSym.Parameters.Length; di++)
                    {
                        var dParam = methodSym.Parameters[di];
                        if (!dParam.IsOptional) break;
                        callArgs.Add(FormatDefaultValue(dParam));
                    }
                }
            }
            catch { }
        }

        var recv = NullCheck.Wrap(mem.Expression, Write(mem.Expression), _ctx);

        // A non-virtual method inherited from a base class is emitted as
        // BaseType_Method, and the receiver must be cast to (BaseType*) (valid
        // because the base struct is the first member). Without this we'd emit a
        // nonexistent DerivedType_Method.
        // Generic instantiations are emitted under their mangled name: Box<int> → Box_int_Get.
        var methodOwner = receiverType.Contains('<') ? TypeRegistry.MapType(receiverType) : receiverType;
        if (_ctx.SemanticModel != null)
        {
            try
            {
                var ms = _ctx.SemanticModel.GetSymbolInfo(inv).Symbol as IMethodSymbol;
                var owner = ms?.ContainingType;
                if (ms != null && !ms.IsStatic && owner != null
                    && owner.TypeKind == TypeKind.Class
                    && owner.SpecialType == SpecialType.None
                    && !owner.IsGenericType
                    && owner.Name != receiverType)
                {
                    methodOwner = owner.Name;
                    recv = "(" + owner.Name + "*)" + recv;
                }
            }
            catch { }
        }

        var allArgs = new List<string> { recv };
        allArgs.AddRange(callArgs);
        var cName = CSharpToC.BuildCMethodName(methodOwner, methodName,
            inv.ArgumentList.Arguments.Count, _ctx.OverloadedMethods);
        return cName + "(" + string.Join(", ", allArgs) + ")";
    }

    /// <summary>
    /// Calls a method of a user value struct. Instance methods take <c>T* self</c>,
    /// so the receiver's address is passed; an rvalue receiver (e.g. <c>(a + b).Length()</c>)
    /// is materialized in a GNU statement-expression temporary first. Static methods
    /// (receiver is the type name) get no self.
    /// </summary>
    private string WriteStructMethodCall(MemberAccessExpressionSyntax mem,
        InvocationExpressionSyntax inv, string structType)
    {
        var cName = structType + "_" + mem.Name.Identifier.Text;
        var callArgs = WriteArguments(inv.ArgumentList.Arguments);

        bool isStatic = mem.Expression.ToString() == structType;
        if (!isStatic && _ctx.SemanticModel != null)
        {
            try { isStatic = _ctx.SemanticModel.GetSymbolInfo(mem.Expression).Symbol is ITypeSymbol; }
            catch { }
        }
        if (isStatic)
            return cName + "(" + string.Join(", ", callArgs) + ")";

        string self;
        if (mem.Expression is ThisExpressionSyntax)
            self = "self";
        else if (mem.Expression is IdentifierNameSyntax or MemberAccessExpressionSyntax
                 or ElementAccessExpressionSyntax)
            self = "&" + Write(mem.Expression);
        else
        {
            var tmp = _ctx.NextTmp("sv");
            var rest = callArgs.Count > 0 ? ", " + string.Join(", ", callArgs) : "";
            return "({ " + structType + " " + tmp + " = " + Write(mem.Expression) + "; "
                 + cName + "(&" + tmp + rest + "); })";
        }

        var all = new List<string> { self };
        all.AddRange(callArgs);
        return cName + "(" + string.Join(", ", all) + ")";
    }

    private List<string> ApplyUpcasts(InvocationExpressionSyntax inv, List<string> args, string receiverTypeName)
    {
        if (_ctx.SemanticModel == null) return args;
        if (inv.Expression is not MemberAccessExpressionSyntax macc) return args;

        var result = new List<string>(args);
        var invArgs = inv.ArgumentList.Arguments;
        var methodName = macc.Name.Identifier.Text;

        // Use the already-resolved receiver type name to find method parameters.
        IMethodSymbol? methodSym = null;
        try
        {
            var recvTypeSym = FindTypeInCompilation(_ctx.SemanticModel.Compilation, receiverTypeName);
            if (recvTypeSym != null)
            {
                methodSym = recvTypeSym
                    .GetMembers(methodName)
                    .OfType<IMethodSymbol>()
                    .FirstOrDefault(m => m.Parameters.Length == invArgs.Count);
            }
        }
        catch { }

        if (methodSym == null) return result;

        for (int i = 0; i < invArgs.Count && i < methodSym.Parameters.Length && i < result.Count; i++)
        {
            try
            {
                var paramType = methodSym.Parameters[i].Type as INamedTypeSymbol;
                if (paramType == null) continue;

                // Get actual argument type — prefer LocalTypes/FieldTypes, fall back to semantic model
                INamedTypeSymbol? actualType = null;
                var argExprStr = invArgs[i].Expression.ToString();
                var argKey = argExprStr.TrimStart('_');
                _ctx.LocalTypes.TryGetValue(argExprStr, out var argLt);
                _ctx.FieldTypes.TryGetValue(argKey, out var argFt);
                var argTypeName = (argLt ?? argFt ?? "").TrimEnd('*').Trim();
                if (!string.IsNullOrEmpty(argTypeName))
                    actualType = FindTypeInCompilation(_ctx.SemanticModel.Compilation, argTypeName);

                if (actualType == null)
                {
                    var argTypeInfo = _ctx.SemanticModel.GetTypeInfo(invArgs[i].Expression);
                    actualType = (argTypeInfo.Type ?? argTypeInfo.ConvertedType) as INamedTypeSymbol;
                }

                if (actualType != null
                    && actualType.Name != paramType.Name
                    && IsSubtypeOf(actualType, paramType))
                {
                    result[i] = "(" + paramType.Name + "*)" + result[i];
                }
            }
            catch { }
        }

        return result;
    }

    private static INamedTypeSymbol? FindTypeInCompilation(
        Microsoft.CodeAnalysis.Compilation compilation, string typeName)
    {
        return SearchNamespace(compilation.GlobalNamespace, typeName);

        static INamedTypeSymbol? SearchNamespace(INamespaceSymbol ns, string name)
        {
            var t = ns.GetTypeMembers(name).FirstOrDefault();
            if (t != null) return t;
            foreach (var child in ns.GetNamespaceMembers())
            {
                t = SearchNamespace(child, name);
                if (t != null) return t;
            }
            return null;
        }
    }

    private static string FormatDefaultValue(IParameterSymbol param)
    {
        if (!param.HasExplicitDefaultValue) return "0";
        return param.ExplicitDefaultValue switch
        {
            null => "NULL",
            true => "1",
            false => "0",
            string s => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"",
            var v => v.ToString() ?? "0"
        };
    }

    private static bool IsSubtypeOf(INamedTypeSymbol derived, INamedTypeSymbol baseType)
    {
        var t = derived.BaseType;
        while (t != null)
        {
            if (t.Name == baseType.Name) return true;
            t = t.BaseType;
        }
        return false;
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
        // Static field/property of a user class (Counter.Total += 5): a global, not
        // a member of an object — same C name as reading it.
        if (_ctx.SemanticModel != null)
        {
            try
            {
                var target = _ctx.SemanticModel.GetSymbolInfo(mem).Symbol;
                if (target is IFieldSymbol { IsStatic: true, IsConst: false } or IPropertySymbol { IsStatic: true }
                    && TranspilerContext.IsUserCode(target.ContainingType))
                    return Write(mem) + " " + op + " " + right;
            }
            catch { }
        }

        var obj = NullCheck.Wrap(mem.Expression, Write(mem.Expression), _ctx);
        var prop = mem.Name.Identifier.Text;
        var objRaw = mem.Expression.ToString();
        var objKey = objRaw.TrimStart('_');

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

        bool isStruct = (lt != null && IsStructType(lt))
                     || (ft != null && IsStructType(ft));
        var arrow = isStruct ? "." : "->";

        // CS2SX/LibNx value types: direct field access, no f_ prefix, no pointer
        if (isStruct)
            return obj + "." + prop + " " + op + " " + right;

        // Only use Label_SetText for the built-in runtime Label/Button types.
        // User-defined classes with a Text property use their own f_Text field.
        bool isBuiltinTextControl = (lt != null && TypeRegistry.IsBuiltinControlType(lt))
                                 || (ft != null && TypeRegistry.IsBuiltinControlType(ft));
        if (prop == "Text" && isBuiltinTextControl)
        {
            if (assign.Right is ConditionalExpressionSyntax cond)
                return "Label_SetText(" + obj + ", " + WriteConditional(cond) + ")";
            if (assign.Right is InterpolatedStringExpressionSyntax interp)
                return FormatStringBuilder.BuildLabelSetText(obj, interp, _ctx, Write);
            if (assign.Right is LiteralExpressionSyntax litStr
                && litStr.Token.IsKind(SyntaxKind.StringLiteralToken))
                return "Label_SetText(" + obj + ", \""
                     + StringEscaper.EscapeRaw(litStr.Token.ValueText) + "\")";
            return "Label_SetText(" + obj + ", " + right + ")";
        }

        // Button.OnClick is an Action delegate field of the runtime Button (no f_ prefix).
        if (prop == "OnClick" && (lt ?? ft) == "Button")
            return obj + "->OnClick " + op + " " + right;

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

        // Detect inherited properties via semantic model and route to ->base.
        if (_ctx.SemanticModel != null
            && assignReceiverType != null
            && !TypeRegistry.IsBuiltinControlType(assignReceiverType))
        {
            try
            {
                var typeInfo2 = _ctx.SemanticModel.GetTypeInfo(mem.Expression);
                var recvSym2 = (typeInfo2.ConvertedType ?? typeInfo2.Type) as INamedTypeSymbol;
                if (recvSym2 != null)
                {
                    // Walk for an inherited property OR field, counting base-levels.
                    INamedTypeSymbol? definingType2 = null;
                    var t = recvSym2;
                    int hops = 0;
                    while (t != null)
                    {
                        var members = t.GetMembers(prop);
                        if (members.OfType<IPropertySymbol>().Any()
                            || members.OfType<IFieldSymbol>().Any())
                        { definingType2 = t; break; }
                        t = t.BaseType; hops++;
                    }
                    if (definingType2 != null && hops > 0)
                    {
                        var baseChain = string.Concat(Enumerable.Repeat("base.", hops));
                        var pfx2 = TypeRegistry.HasNoPrefix(prop) ? "" : "f_";
                        cProp = baseChain + pfx2 + prop;
                    }
                }
            }
            catch { }
        }

        // Interpolated strings AND string concatenations (`"x" + y`) produce a
        // pooled/arena buffer that is recycled next frame → a dangling pointer if
        // stored in a field. Wrap with _cs2sx_heap_strdup() for a stable heap copy.
        // Only applies to user-class fields (Control.Text uses Label_SetText above).
        bool rhsIsTransientString =
            assign.Right is InterpolatedStringExpressionSyntax
            || (assign.Right is BinaryExpressionSyntax addBe
                && addBe.IsKind(SyntaxKind.AddExpression)
                && TypeInferrer.InferCSharpType(assign.Right, _ctx) == "string");

        var finalRight = right;
        if (rhsIsTransientString
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

        // Interface field assignment upcast: if property/field type is an interface,
        // wrap right-hand side with ClassName_as_IFace() conversion
        if (_ctx.SemanticModel != null)
        {
            try
            {
                // Get the declared type of the property being assigned
                var propSym = _ctx.SemanticModel.GetSymbolInfo(mem).Symbol;
                string? targetIfaceName = null;
                if (propSym is IPropertySymbol pSym)
                    targetIfaceName = TranspilerContext.FormatTypeSymbol(pSym.Type);
                else if (propSym is IFieldSymbol fSymProp)
                    targetIfaceName = TranspilerContext.FormatTypeSymbol(fSymProp.Type);
                if (targetIfaceName != null && TypeRegistry.IsRegisteredInterface(targetIfaceName))
                {
                    var rightTypeSym = _ctx.SemanticModel.GetTypeInfo(assign.Right).Type as INamedTypeSymbol;
                    if (rightTypeSym != null && !TypeRegistry.IsRegisteredInterface(rightTypeSym.Name)
                        && TypeRegistry.NeedsPointerSuffix(rightTypeSym.Name))
                        finalRight = rightTypeSym.Name + "_as_" + targetIfaceName + "(" + right + ")";
                }
            }
            catch { }
        }

        return obj + arrow + cProp + " " + op + " " + finalRight;
    }

    private string WriteIndexerAssignment(ElementAccessExpressionSyntax elemLeft, string op, string right)
    {
        // Arrays (bounds-checked, multi-dimensional): the element access is an lvalue.
        if (IsSingleDimArray(elemLeft.Expression) || elemLeft.ArgumentList.Arguments.Count > 1)
        {
            // Elements of reference-counted classes: the array holds a reference.
            if (op == "=" && ArcElementRoot(elemLeft) is { } elemRoot
                && elemLeft.Parent is AssignmentExpressionSyntax asg)
                return "CS2SX_ARC_STORE(" + elemRoot + ", " + WriteElementAccess(elemLeft) + ", "
                     + Arc.RetainIfShared(asg.Right, right, _ctx) + ")";
            return WriteElementAccess(elemLeft) + " " + op + " " + right;
        }

        var obj = Write(elemLeft.Expression);
        var key = Write(elemLeft.ArgumentList.Arguments[0].Expression);
        var objRaw = elemLeft.Expression.ToString();
        var objKey = objRaw.TrimStart('_');

        string? lt = null, ft = null;
        _ctx.LocalTypes.TryGetValue(objRaw, out lt);
        _ctx.FieldTypes.TryGetValue(objKey, out ft);
        // Any other receiver (Other.Map, obj.Items, a call result): its semantic type.
        if (lt == null && ft == null) lt = _ctx.GetSemanticType(elemLeft.Expression);

        bool isDict = (lt != null && TypeRegistry.IsDictionary(lt))
                   || (ft != null && TypeRegistry.IsDictionary(ft));
        if (isDict)
        {
            var dictType = lt ?? ft!;
            var types = TypeRegistry.GetDictionaryTypes(dictType)!.Value;
            var cKey = types.key == "string" ? "str" : TypeRegistry.MapType(types.key);
            var cVal = types.val == "string" ? "str" : TypeRegistry.MapType(types.val);
            var prefix = "Dict_" + cKey + "_" + cVal;
            return AccessorAssignment(prefix + "_Get", prefix + "_Set", obj, key, op, right);
        }

        bool isList = (lt != null && TypeRegistry.IsList(lt))
                   || (ft != null && TypeRegistry.IsList(ft));
        if (isList)
        {
            var listType = lt ?? ft!;
            var inner = TypeRegistry.GetListInnerType(listType) ?? "int";
            var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
            return AccessorAssignment("List_" + cInner + "_Get", "List_" + cInner + "_Set", obj, key, op, right);
        }

        // User-defined indexer setter → ClassName_set(obj, key, value)
        var objTypeName = (lt ?? ft ?? "").TrimEnd('*').Trim();
        if (!string.IsNullOrEmpty(objTypeName) && _ctx.IndexerClasses.Contains(objTypeName))
            return AccessorAssignment(objTypeName + "_get", objTypeName + "_set", obj, key, op, right);

        // Arrays (bounds-checked, multi-dimensional) and raw pointers: an lvalue.
        return WriteElementAccess(elemLeft) + " " + op + " " + right;
    }

    /// `c[k] = v` → set(c, k, v); compound `c[k] += v` → set(c, k, get(c, k) + v), with
    /// c and k evaluated once (the old code dropped the operator: `d[k] += 1` set 1).
    private static string AccessorAssignment(string get, string set, string obj, string key,
        string op, string right)
    {
        if (op == "=")
            return set + "(" + obj + ", " + key + ", " + right + ")";
        var binOp = op[..^1];
        return "({ __auto_type _cs2sx_c = " + obj + "; __auto_type _cs2sx_k = " + key + "; "
             + set + "(_cs2sx_c, _cs2sx_k, " + get + "(_cs2sx_c, _cs2sx_k) " + binOp + " (" + right + ")); })";
    }

    // ── MemberAccess ──────────────────────────────────────────────────────────

    /// Members System.Exception declares, on any exception (user subclasses included):
    /// e.Message → ((Exception*)(e))->f_Message, e.GetType().Name → CS2SX_Exc_TypeName(e).
    private string? TryWriteExceptionMember(MemberAccessExpressionSyntax mem)
    {
        if (_ctx.SemanticModel == null) return null;
        var member = mem.Name.Identifier.Text;
        try
        {
            if (member is "Name" or "FullName"
                && mem.Expression is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax getType }
                && getType.Name.Identifier.Text == "GetType"
                && TypeRegistry.IsExceptionType(_ctx.SemanticModel.GetTypeInfo(getType.Expression).Type))
            {
                return "CS2SX_Exc_TypeName((Exception*)(" + Write(getType.Expression) + "))";
            }

            if (_ctx.SemanticModel.GetSymbolInfo(mem).Symbol is not IPropertySymbol prop
                || prop.ContainingType?.ContainingNamespace?.ToDisplayString() is not ("System" or "System.IO" or "System.Collections.Generic")
                || !TypeRegistry.IsExceptionType(_ctx.SemanticModel.GetTypeInfo(mem.Expression).Type))
                return null;
            var recv = "((Exception*)(" + Write(mem.Expression) + "))";
            return member switch
            {
                "Message" => recv + "->f_Message",
                "InnerException" => recv + "->f_InnerException",
                "ParamName" => recv + "->f_ParamName",
                "StackTrace" or "Source" => "\"\"",
                "HResult" => "0",
                _ => null,
            };
        }
        catch { return null; }
    }

    private string WriteMemberAccess(MemberAccessExpressionSyntax mem)
    {
        var full = mem.ToString();

        // string.* Konstanten
        if (full == "string.Empty") return "\"\"";
        if (full == "string.IsNullOrEmpty") return "String_IsNullOrEmpty";
        if (full == "string.IsNullOrWhiteSpace") return "String_IsNullOrWhiteSpace";
        if (full == "int.MaxValue") return "INT_MAX";
        if (full == "int.MinValue") return "INT_MIN";

        // Type.StaticField on a user type → Type_StaticField (a C global).
        if (_ctx.SemanticModel != null)
        {
            try
            {
                if (_ctx.SemanticModel.GetSymbolInfo(mem).Symbol is IFieldSymbol { IsStatic: true, IsConst: false } sf
                    && sf.ContainingType is { } owner
                    && owner.TypeKind is TypeKind.Class or TypeKind.Struct
                    && TranspilerContext.IsUserCode(owner)
                    && _ctx.SemanticModel.GetSymbolInfo(mem.Expression).Symbol is ITypeSymbol)
                    return owner.Name + "_" + sf.Name.TrimStart('_');
            }
            catch { }
        }

        var exceptionMember = TryWriteExceptionMember(mem);
        if (exceptionMember != null) return exceptionMember;

        var storedMember = TryWriteUserClassStoredMember(mem) ?? TryWriteUserStructMember(mem);
        if (storedMember != null) return storedMember;

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

        // Qualified member access on known types: enum members and static-class const fields.
        // Semantic model resolves this reliably across files.
        if (_ctx.SemanticModel != null)
        {
            try
            {
                var sym = _ctx.SemanticModel.GetSymbolInfo(mem).Symbol;
                if (sym is Microsoft.CodeAnalysis.IFieldSymbol fld)
                {
                    // Enum member → global C constant (user enums are prefixed: Mode_None)
                    if (fld.ContainingType?.TypeKind == Microsoft.CodeAnalysis.TypeKind.Enum)
                        return TypeRegistry.IsUserDefinedEnum(fld.ContainingType.Name)
                            ? TypeRegistry.EnumMemberCName(fld.ContainingType.Name, fld.Name)
                            : fld.Name;
                    // const field via the type name (AppCategory.Tools, Stats.History) — of a
                    // static or an ordinary class/struct alike
                    if (fld.IsConst && fld.ContainingType != null && TranspilerContext.IsUserCode(fld.ContainingType))
                    {
                        // Inline the literal value when available — needed for C static array initializers
                        // which require compile-time constants (const char* variables are NOT constants in C)
                        if (fld.ConstantValue is string sv)
                            return "\"" + sv.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
                        if (fld.ConstantValue != null)
                            return CConstantLiteral(fld.ConstantValue);
                        return fld.ContainingType.Name + "_" + fld.Name;
                    }
                    // Static readonly field on a static class → ClassName_FieldName
                    if (fld.IsStatic && fld.ContainingType?.IsStatic == true)
                        return fld.ContainingType.Name + "_" + fld.Name;
                }
            }
            catch { }
        }
        // Fallback for check-mode without semantic model: if the property name is a known
        // enum member in the current translation unit, return it directly.
        // Fallback without semantic model: `Mode.None` on a registered user enum.
        {
            var receiverName = mem.Expression.ToString().Trim();
            if (TypeRegistry.IsUserDefinedEnum(receiverName))
                return TypeRegistry.EnumMemberCName(receiverName, mem.Name.Identifier.Text);
        }

        if (full.StartsWith("LibNX.", StringComparison.Ordinal))
            return mem.Name.Identifier.Text;

        var obj = NullCheck.Wrap(mem.Expression, Write(mem.Expression), _ctx);
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
            if (IsStringExpr(mem.Expression) || IsStringType(mem.Expression))
                return "strlen(" + obj + ")";
            var rk2 = mem.Expression.ToString();
            var rkey2 = rk2.TrimStart('_');
            if ((_ctx.LocalTypes.TryGetValue(rk2, out var rlt) && rlt is "string" or "char[]" or "const char*")
             || (_ctx.FieldTypes.TryGetValue(rkey2, out var rft) && rft is "string" or "char[]" or "const char*"))
                return "strlen(" + obj + ")";
            // Arrays: compile-time length if known in this method, params companion
            // count, otherwise the runtime length header (works for fields, parameters,
            // return values — anything).
            if (ArrayModel.IsArrayExpr(mem.Expression, _ctx))
                return ArrayModel.LengthOf(rk2, obj, _ctx);
        }
        if (prop == "Count" && IsListExpr(mem.Expression)) return obj + "->count";
        if (prop == "Count" && IsStackQueueHashSetExpr(mem.Expression)) return obj + "->count";
        if (prop == "Count" && IsDictExpr(mem.Expression)) return obj + "->count";
        // Chained access like sidebar.Items.Count — simple lookup misses the type; use SemanticModel.
        // Note: typeInfo.Type.ToString() returns fully qualified "System.Collections.Generic.List<T>",
        // so FormatTypeSymbol is required to get the short form "List<T>" that TypeRegistry understands.
        if (prop == "Count" && _ctx.SemanticModel != null)
        {
            try
            {
                var exprTypeSym = _ctx.SemanticModel.GetTypeInfo(mem.Expression).Type;
                if (exprTypeSym != null)
                {
                    var exprType = TranspilerContext.FormatTypeSymbol(exprTypeSym);
                    if (TypeRegistry.IsList(exprType) || TypeRegistry.IsDictionary(exprType)
                        || TypeRegistry.IsHashSet(exprType) || TypeRegistry.IsStack(exprType)
                        || TypeRegistry.IsQueue(exprType))
                        return obj + "->count";
                }
            }
            catch { }
        }
        if (prop == "Length" && IsStringBuilderExpr(mem.Expression)) return obj + "->length";
        if (prop == "HasValue" && IsNullableExpr(mem.Expression)) return NullableHandler.WriteHasValue(obj);
        if (prop == "Value" && IsNullableExpr(mem.Expression)) return NullableHandler.WriteGetValue(obj);
        if (prop is "Keys" or "Values" && IsDictExpr(mem.Expression)) return obj;

        var key = rawExpr.TrimStart('_');
        var receiverType = ResolveReceiverType(rawExpr, mem.Expression);

        // TouchState computed properties — CS2SX_TouchState is a plain C struct without methods
        if (receiverType == "TouchState" || obj.Contains("touch") || obj.Contains("Touch"))
        {
            if (prop == "IsTouched") return "(" + obj + ".count > 0)";
            if (prop == "X0") return "(" + obj + ".count > 0 ? " + obj + ".x[0] : 0)";
            if (prop == "Y0") return "(" + obj + ".count > 0 ? " + obj + ".y[0] : 0)";
        }

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
                    // CS2SX_LIST_DEFINE_PTR stores T* — element access returns pointer → use ->
                    bool tupleIsPtr = mem.Expression is ElementAccessExpressionSyntax;
                    var tupleAccessor = tupleIsPtr ? "->" : ".";
                    var elements = namedSym.TupleElements;
                    for (int ti = 0; ti < elements.Length; ti++)
                    {
                        var elem = elements[ti];
                        if (elem.Name == prop || elem.Name == "Item" + (ti + 1))
                        {
                            return obj + tupleAccessor + "item" + (ti + 1);
                        }
                    }
                }
            }
            catch { }
        }

        // When 'obj' is already a pointer access (contains '->'), the result is a pointer.
        // Using '.' on a pointer is invalid C — always use '->' in that case.
        bool objIsPointerResult = obj.Contains("->") || obj.Contains("self->") || obj.StartsWith("(") ;

        string? lt2 = null, ft2 = null;
        bool isStruct2 =
            (_ctx.LocalTypes.TryGetValue(rawExpr, out lt2) && IsStructType(lt2))
         || (_ctx.FieldTypes.TryGetValue(key, out ft2) && IsStructType(ft2));
        if (isStruct2)
        {
            if (objIsPointerResult && TypeRegistry.NeedsPointerSuffix(lt2 ?? ft2 ?? ""))
                return obj + "->" + prop;
            return obj + "." + prop;
        }

        string? vlt2 = null, vft2 = null;
        bool inValueTypeStructs =
            (_ctx.LocalTypes.TryGetValue(rawExpr, out vlt2) && _ctx.ValueTypeStructs.Contains(vlt2))
         || (_ctx.FieldTypes.TryGetValue(key, out vft2) && _ctx.ValueTypeStructs.Contains(vft2));
        if (inValueTypeStructs)
        {
            // If the field type is a class (needs pointer), use -> even if in ValueTypeStructs
            var recvT2 = vlt2 ?? vft2 ?? "";
            if (TypeRegistry.NeedsPointerSuffix(recvT2))
                return obj + "->" + (TypeRegistry.HasNoPrefix(prop) ? "" : "f_") + prop;
            return obj + "." + prop;
        }

        // SemanticModel: detect computed (non-auto) properties and inherited properties
        if (_ctx.SemanticModel != null && receiverType != null)
        {
            try
            {
                var typeInfo2 = _ctx.SemanticModel.GetTypeInfo(mem.Expression);
                var recvSym2 = (typeInfo2.ConvertedType ?? typeInfo2.Type) as INamedTypeSymbol;
                if (recvSym2 != null)
                {
                    // Walk up the type hierarchy to find the property, counting how
                    // many base-levels up it is declared (for nested `base.base.`).
                    IPropertySymbol? foundProp = null;
                    INamedTypeSymbol? definingType = null;
                    var t = recvSym2;
                    int propHops = 0;
                    while (t != null)
                    {
                        var p = t.GetMembers(prop).OfType<IPropertySymbol>().FirstOrDefault();
                        if (p != null) { foundProp = p; definingType = t; break; }
                        t = t.BaseType; propHops++;
                    }

                    if (foundProp != null && definingType != null)
                    {
                        bool isInherited = definingType.Name != recvSym2.Name;
                        // Use syntax-based auto-property detection.
                        // IsImplicitlyDeclared is false even for auto-property getters (get; is in source),
                        // so we must check whether the getter has an explicit body.
                        bool isComputed = false;
                        if (foundProp.GetMethod != null)
                        {
                            foreach (var synRef in foundProp.DeclaringSyntaxReferences)
                            {
                                if (synRef.GetSyntax() is PropertyDeclarationSyntax propSyn)
                                {
                                    isComputed = !PropertyWriter.IsAutoProperty(propSyn);
                                    break;
                                }
                            }
                        }

                        if (isComputed)
                        {
                            // Static property getter has no self parameter
                            if (foundProp.IsStatic)
                                return definingType.Name + "_get_" + prop + "()";
                            return definingType.Name + "_get_" + prop + "(" + obj + ")";
                        }

                        if (isInherited)
                        {
                            // Property defined in a base class → access via ->base.[base.]*
                            var baseChain = string.Concat(Enumerable.Repeat("base.", propHops));
                            var lower = prop.ToLowerInvariant();
                            if (TypeRegistry.IsBuiltinControlType(definingType.Name)
                                && TypeRegistry.ControlFields.Contains(lower))
                                return obj + "->" + baseChain + lower;
                            var pfx2 = TypeRegistry.HasNoPrefix(prop) ? "" : "f_";
                            return obj + "->" + baseChain + pfx2 + prop;
                        }
                    }

                    // Inherited FIELD (public int Legs;) — same base-chain routing.
                    {
                        var t2 = recvSym2;
                        int fieldHops = 0;
                        while (t2 != null)
                        {
                            var fsym = t2.GetMembers(prop).OfType<IFieldSymbol>().FirstOrDefault();
                            if (fsym != null)
                            {
                                if (fieldHops > 0)  // declared in a base class
                                {
                                    var baseChain = string.Concat(Enumerable.Repeat("base.", fieldHops));
                                    var pfx2 = TypeRegistry.HasNoPrefix(prop) ? "" : "f_";
                                    return obj + "->" + baseChain + pfx2 + prop;
                                }
                                break; // on receiver itself → normal fallback below
                            }
                            t2 = t2.BaseType; fieldHops++;
                        }
                    }
                }
            }
            catch { }
        }

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
            return cName + "_New" + CtorNaming.SuffixFor(ResolveCtor(obj))
                + "(" + string.Join(", ", ctorArgs) + ")";
        }
        return WriteObjectCreationFromTypeName(obj.Type.ToString(), obj.ArgumentList, obj.Initializer, obj);
    }

    private string WriteImplicitObjectCreation(ImplicitObjectCreationExpressionSyntax obj)
    {
        var typeName = _ctx.GetSemanticType(obj);
        if (typeName == null)
        {
            _ctx.Warn(obj, "new() — Zieltyp nicht bestimmbar (kein SemanticModel oder Typ-Fehler)");
            return "NULL /* new() — type not resolvable */";
        }
        return WriteObjectCreationFromTypeName(typeName, obj.ArgumentList, obj.Initializer, obj);
    }

    /// Arguments of a constructor call (`new T(...)`, `: base(...)`, `: this(...)`),
    /// with omitted optional parameters filled from their defaults.
    internal List<string> BuildCtorArgs(string typeName, ArgumentListSyntax? argList)
    {
        var supplied = argList?.Arguments.Select(a => Write(a.Expression)).ToList()
                       ?? new List<string>();

        if (_ctx.SemanticModel != null && argList?.Parent != null)
        {
            try
            {
                if (_ctx.SemanticModel.GetSymbolInfo(argList.Parent).Symbol is IMethodSymbol ctor)
                {
                    for (int i = supplied.Count; i < ctor.Parameters.Length; i++)
                    {
                        if (!ctor.Parameters[i].HasExplicitDefaultValue) break;
                        supplied.Add(DefaultValueLiteral(ctor.Parameters[i]));
                    }
                }
            }
            catch { }
        }

        return supplied;
    }

    /// C literal of an optional parameter's default value.
    internal static string DefaultValueLiteral(IParameterSymbol param)
    {
        if (!param.HasExplicitDefaultValue) return "0";
        var v = param.ExplicitDefaultValue;
        if (v != null && param.Type.TypeKind == TypeKind.Enum
            && TypeRegistry.IsUserDefinedEnum(param.Type.Name))
        {
            var member = param.Type.GetMembers().OfType<IFieldSymbol>()
                .FirstOrDefault(f => f.HasConstantValue && Equals(f.ConstantValue, v));
            if (member != null) return TypeRegistry.EnumMemberCName(param.Type.Name, member.Name);
        }
        return v switch
        {
            null => "NULL",
            string s => "\"" + StringEscaper.EscapeRaw(s) + "\"",
            bool b => b ? "1" : "0",
            float f => f.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "f",
            double d => d.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
            char c => ((int)c).ToString(),
            _ => Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture) ?? "0",
        };
    }

    /// A C# constant as a C literal, independent of the machine's culture
    /// (2.5f → "2.5f", not "2,5"; true → "1").
    internal static string CConstantLiteral(object value) => value switch
    {
        bool b => b ? "1" : "0",
        float f => f.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "f",
        double d => d.ToString("R", System.Globalization.CultureInfo.InvariantCulture),
        char c => ((int)c).ToString(System.Globalization.CultureInfo.InvariantCulture),
        long l => l.ToString(System.Globalization.CultureInfo.InvariantCulture) + "LL",
        ulong ul => ul.ToString(System.Globalization.CultureInfo.InvariantCulture) + "ULL",
        uint ui => ui.ToString(System.Globalization.CultureInfo.InvariantCulture) + "u",
        _ => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? "0",
    };

    /// The constructor a `new T(...)` resolves to (null without semantic model).
    private IMethodSymbol? ResolveCtor(SyntaxNode? creation)
    {
        if (creation == null || _ctx.SemanticModel == null) return null;
        try { return _ctx.SemanticModel.GetSymbolInfo(creation).Symbol as IMethodSymbol; }
        catch { return null; }
    }

    private string WriteObjectCreationFromTypeName(
        string typeName,
        ArgumentListSyntax? argList,
        InitializerExpressionSyntax? initializer,
        SyntaxNode? creation = null)
    {
        typeName = TypeRegistry.NestedToSimple(typeName);   // new Outer.Inner() → Inner_New()

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
            // Normalize cInner to a valid C identifier: void* → voidptr, spaces → _
            var cInner = inner == "string" ? "str"
                : TypeRegistry.MapType(inner).Replace("*", "ptr").Replace(" ", "_");
            var listCreate = "List_" + cInner + "_New()";

            // new List<T>(source) — copy from an existing List<T> or IEnumerable<T> source
            // e.g. new List<AppEntry>(_repository.GetFeatured())
            if (argList?.Arguments.Count == 1 && initializer == null)
            {
                var arg = argList.Arguments[0];
                var argTypeSem = _ctx.GetSemanticType(arg.Expression);
                bool srcIsList = argTypeSem != null && (TypeRegistry.IsList(argTypeSem)
                    || argTypeSem.StartsWith("IEnumerable<") || argTypeSem.StartsWith("IReadOnlyList<")
                    || argTypeSem.StartsWith("ICollection<"));
                if (srcIsList)
                {
                    var srcExpr = Write(arg.Expression);
                    var srcTmp = _ctx.NextTmp("src");
                    var tmp = _ctx.NextTmp("list");
                    _ctx.Out.WriteLine(_ctx.Tab + "List_" + cInner + "* " + srcTmp + " = " + srcExpr + ";");
                    _ctx.Out.WriteLine(_ctx.Tab + "List_" + cInner + "* " + tmp + " = " + listCreate + ";");
                    _ctx.Out.WriteLine(_ctx.Tab + "if (" + srcTmp + ") { for (int _cpi = 0; _cpi < " + srcTmp + "->count; _cpi++) List_" + cInner + "_Add(" + tmp + ", " + srcTmp + "->data[_cpi]); }");
                    return tmp;
                }
                // Single int argument is a capacity hint — ignore it, just create empty list
            }

            if (initializer?.Expressions.Count > 0)
            {
                var tmp = _ctx.NextTmp("list");
                _ctx.Out.WriteLine(_ctx.Tab + "List_" + cInner + "* " + tmp + " = " + listCreate + ";");
                foreach (var item in initializer.Expressions)
                {
                    var itemVal = Write(item);
                    // CS2SX_LIST_DEFINE_PTR stores T* — tuple compound literals MUST be heap-allocated.
                    // Using &(compound_literal) gives a pointer with automatic storage that becomes
                    // dangling when the enclosing function returns, corrupting the list later.
                    if (itemVal.StartsWith("(") && itemVal.Contains("item1"))
                    {
                        var tupleTmp = _ctx.NextTmp("t");
                        _ctx.Out.WriteLine(_ctx.Tab + cInner + "* " + tupleTmp + " = (" + cInner + "*)malloc(sizeof(" + cInner + "));");
                        _ctx.Out.WriteLine(_ctx.Tab + "if (" + tupleTmp + ") *" + tupleTmp + " = " + itemVal + ";");
                        _ctx.Out.WriteLine(_ctx.Tab + "List_" + cInner + "_Add(" + tmp + ", " + tupleTmp + ");");
                    }
                    else
                    {
                        _ctx.Out.WriteLine(_ctx.Tab + "List_" + cInner + "_Add(" + tmp + ", " + itemVal + ");");
                    }
                }
                return tmp;
            }
            return listCreate;
        }
        if (TypeRegistry.IsDictionary(typeName))
        {
            var types = TypeRegistry.GetDictionaryTypes(typeName)!.Value;
            var cKey = types.key == "string" ? "str" : TypeRegistry.MapType(types.key);
            var cVal = types.val == "string" ? "str" : TypeRegistry.MapType(types.val);
            var dictCreate = "Dict_" + cKey + "_" + cVal + "_New()";

            // new Dictionary<K,V> { { key1, val1 }, { key2, val2 }, ... }
            // Each initializer element is an InitializerExpressionSyntax with 2 sub-expressions.
            if (initializer?.Expressions.Count > 0)
            {
                var tmp = _ctx.NextTmp("dict");
                _ctx.Out.WriteLine(_ctx.Tab + "Dict_" + cKey + "_" + cVal + "* " + tmp + " = " + dictCreate + ";");
                foreach (var elem in initializer.Expressions)
                {
                    if (elem is InitializerExpressionSyntax pair && pair.Expressions.Count == 2)
                    {
                        var k = Write(pair.Expressions[0]);
                        var v = Write(pair.Expressions[1]);
                        _ctx.Out.WriteLine(_ctx.Tab + "Dict_" + cKey + "_" + cVal + "_Add(" + tmp + ", " + k + ", " + v + ");");
                    }
                }
                return tmp;
            }

            return dictCreate;
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
            return cName + "_New" + CtorNaming.SuffixFor(ResolveCtor(creation))
                + "(" + string.Join(", ", ctorArgs) + ")";
        }
        if (typeName == "Random")
            return "NULL /* Random — use CS2SX_Rand_Next() directly */";
        if (typeName == "Stopwatch")
            return "CS2SX_Stopwatch_New()";
        // Label/Button: constructor accepts optional text — default to "" when no arg given
        if (typeName is "Label" or "Button")
        {
            var txt = argList?.Arguments.Count > 0
                ? Write(argList.Arguments[0].Expression)
                : "\"\"";
            return typeName + "_New(" + txt + ")";
        }
        // CS2SX/LibNx value types not in ValueTypeStructs (they come from embedded stubs)
        if (typeName is "TouchState" or "StickPos" or "BatteryInfo" or "TimeInfo" or "MotionState")
        {
            var cType = TypeRegistry.MapType(typeName);
            return "(" + cType + "){0}";
        }
        if (_ctx.ValueTypeStructs.Contains(typeName) || TypeRegistry.IsUserStruct(typeName))
        {
            var cType = TypeRegistry.MapType(typeName);
            if (argList?.Arguments.Count > 0 && TypeRegistry.UserStructHasConstructor(typeName))
                return typeName + "_New(" + string.Join(", ",
                    argList.Arguments.Select(a => Write(a.Expression))) + ")";
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

        var args = BuildCtorArgs(typeName, argList);
        var ctorSym = ResolveCtor(creation);
        var newExpr = TypeRegistry.IsBuiltinException(typeName)
            ? ExceptionModel.NewExpr(typeName, ctorSym, args)
            : typeName + "_New" + CtorNaming.SuffixFor(ctorSym) + "(" + string.Join(", ", args) + ")";

        if (initializer?.Expressions.Count > 0)
        {
            var tmp = _ctx.NextTmp(typeName.ToLower());
            var cTypeName = TypeRegistry.MapType(typeName);
            _ctx.Out.WriteLine(_ctx.Tab + cTypeName + "* " + tmp + " = " + newExpr + ";");
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
                if (propName == "OnClick" && typeName == "Button")
                {
                    _ctx.Out.WriteLine(_ctx.Tab + tmp + "->OnClick = " + propVal + ";");
                    continue;
                }
                // For user-defined types use f_ prefix; MapProperty is for runtime/control types only
                bool isRuntimeInit = TypeRegistry.IsControlType(typeName)
                                  || TypeRegistry.IsBuiltinControlType(typeName)
                                  || TypeRegistry.IsLibNxStruct(typeName);
                var cp = isRuntimeInit
                    ? TypeRegistry.MapProperty(propName)
                    : (TypeRegistry.HasNoPrefix(propName) ? propName : "f_" + propName);
                if (OwningStoreRoot(asgn.Left) is { } initRoot)
                    _ctx.Out.WriteLine(_ctx.Tab + "CS2SX_ARC_STORE(" + initRoot + ", " + tmp + "->" + cp + ", "
                        + Arc.RetainIfShared(asgn.Right, propVal, _ctx) + ");");
                else
                    _ctx.Out.WriteLine(_ctx.Tab + tmp + "->" + cp + " = " + propVal + ";");
            }
            return tmp;
        }
        return newExpr;
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
        // An array created in an expression (field initializer, return, argument,
        // assignment) can outlive the current frame → always a heap array with header.
        // Elements are zero/NULL-initialised like in C#.
        var elemC = ArrayModel.ElemCType(arr.Type.ElementType.ToString().Trim());
        if (arr.Initializer != null && arr.Initializer.Expressions.Count > 0)
            return ArrayModel.FromInit(elemC, arr.Initializer.Expressions.Select(e => Write(e)).ToList());
        if (arr.Type.RankSpecifiers.Count > 0
            && arr.Type.RankSpecifiers[0].Sizes.Count > 0
            && arr.Type.RankSpecifiers[0].Sizes[0] is not OmittedArraySizeExpressionSyntax)
            return ArrayModel.NewHeap(elemC, Write(arr.Type.RankSpecifiers[0].Sizes[0]));
        return ArrayModel.NewHeap(elemC, "0");
    }

    private string WriteImplicitArrayCreation(ImplicitArrayCreationExpressionSyntax implArr)
    {
        var csElem = ArrayModel.ElementCsType(implArr, _ctx);
        if (csElem == null && implArr.Initializer?.Expressions.Count > 0)
            csElem = TypeInferrer.InferCSharpType(implArr.Initializer.Expressions[0], _ctx);
        var elems = implArr.Initializer?.Expressions.Select(e => Write(e)).ToList() ?? new List<string>();
        return ArrayModel.FromInit(ArrayModel.ElemCType(csElem ?? "int"), elems);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// <c>obj.Field</c> / <c>obj.AutoProp</c> where obj is a user CLASS instance, resolved
    /// through the semantic model so it works for any receiver expression
    /// (<c>list[0].Name</c>, <c>GetEnemy().Hp</c>, chains):
    /// <c>obj->f_Name</c>, or <c>obj->base.f_Name</c> for a member declared in a base class.
    /// Returns null (→ existing logic) for anything else: structs, computed properties,
    /// runtime/stub types, interfaces, static members.
    /// </summary>
    private string? TryWriteUserClassStoredMember(MemberAccessExpressionSyntax mem)
    {
        if (_ctx.SemanticModel == null) return null;
        if (mem.Expression is ThisExpressionSyntax or BaseExpressionSyntax) return null;
        var path = StoredMemberPath(mem.Expression, mem);
        return path == null ? null
            : NullCheck.Wrap(mem.Expression, Write(mem.Expression), _ctx) + "->" + path;
    }

    /// The C path from a user-class pointer to a stored member (field, auto-property,
    /// field-like event), through the embedded base structs: "base.f_Name".
    /// <paramref name="member"/> is the member access or `?.` member binding.
    private string? StoredMemberPath(ExpressionSyntax receiver, ExpressionSyntax member)
    {
        if (_ctx.SemanticModel == null) return null;
        try
        {
            var sym = _ctx.SemanticModel.GetSymbolInfo(member).Symbol;
            bool stored = sym switch
            {
                IFieldSymbol f => !f.IsStatic && !f.IsConst,
                IPropertySymbol p => !p.IsStatic && IsStoredProperty(p),
                IEventSymbol e => !e.IsStatic,   // field-like event: a delegate field
                _ => false,
            };
            if (!stored || !TranspilerContext.IsUserCode(sym!.ContainingType)) return null;
            if (sym.ContainingType.TypeKind != TypeKind.Class) return null;

            if (_ctx.SemanticModel.GetTypeInfo(receiver).Type is not INamedTypeSymbol recvType
                || recvType.TypeKind != TypeKind.Class
                || !TranspilerContext.IsUserCode(recvType))
                return null;

            int hops = 0;
            for (var t = recvType; t != null && !SymbolEqualityComparer.Default.Equals(
                     t.OriginalDefinition, sym.ContainingType.OriginalDefinition); t = t.BaseType)
                hops++;

            var name = sym.Name.TrimStart('_');
            var prefix = TypeRegistry.HasNoPrefix(name) ? "" : "f_";
            return string.Concat(Enumerable.Repeat("base.", hops)) + prefix + name;
        }
        catch { return null; }

        static bool IsStoredProperty(IPropertySymbol p)
        {
            foreach (var r in p.DeclaringSyntaxReferences)
            {
                var node = r.GetSyntax();
                if (node is ParameterSyntax) return true; // positional record member
                if (node is PropertyDeclarationSyntax pd) return PropertyWriter.IsAutoProperty(pd);
            }
            return false;
        }
    }

    /// <summary>
    /// <c>v.X</c> where v is a user STRUCT value from any expression (<c>box.Value.W</c>,
    /// <c>list[0].X</c>, <c>GetPos().Y</c>): struct members keep their C# names and are
    /// reached with '.', or '->' when the receiver is a <c>ref</c>/<c>out</c> parameter
    /// (passed as T*).
    /// </summary>
    private string? TryWriteUserStructMember(MemberAccessExpressionSyntax mem)
    {
        if (_ctx.SemanticModel == null || mem.Expression is ThisExpressionSyntax) return null;
        try
        {
            var sym = _ctx.SemanticModel.GetSymbolInfo(mem).Symbol;
            if (sym is not (IFieldSymbol { IsStatic: false } or IPropertySymbol { IsStatic: false })) return null;
            if (sym.ContainingType is not { TypeKind: TypeKind.Struct } st
                || !TypeRegistry.IsUserStruct(st.Name)) return null;
            if (sym is IPropertySymbol p && !p.DeclaringSyntaxReferences.Any(r =>
                    r.GetSyntax() is ParameterSyntax
                    || (r.GetSyntax() is PropertyDeclarationSyntax pd && PropertyWriter.IsAutoProperty(pd))))
                return null; // computed property — existing logic

            bool viaPointer = _ctx.SemanticModel.GetSymbolInfo(mem.Expression).Symbol
                is IParameterSymbol { RefKind: not RefKind.None };
            return Write(mem.Expression) + (viaPointer ? "->" : ".") + sym.Name;
        }
        catch { return null; }
    }

    /// <summary>
    /// <c>self-></c> followed by one <c>base.</c> per inheritance level between the
    /// current class and <paramref name="declaringType"/> (the class that declares the
    /// member), e.g. <c>self->base.base.</c> for a field two levels up.
    /// </summary>
    private string SelfBaseChain(SyntaxNode at, string declaringType)
    {
        int hops = 1;
        try
        {
            var encl = _ctx.SemanticModel?.GetEnclosingSymbol(at.SpanStart);
            var type = encl as INamedTypeSymbol ?? encl?.ContainingType;
            while (type != null && type.Name != _ctx.CurrentClass) type = type.ContainingType;
            int n = 0;
            for (var b = type?.BaseType; b != null; b = b.BaseType)
            {
                n++;
                if (b.Name == declaringType) { hops = n; break; }
            }
        }
        catch { }
        return "self->" + string.Concat(Enumerable.Repeat("base.", hops));
    }

    private static bool IsStructType(string csType) =>
        TypeRegistry.IsUserStruct(csType)
        || TypeRegistry.IsLibNxStruct(csType)
        || TypeRegistry.IsLibNxStruct(TypeRegistry.MapType(csType).TrimEnd('*'))
        || csType is "TouchState" or "StickPos" or "BatteryInfo" or "TimeInfo" or "MotionState";

    private string WriteConditional(ConditionalExpressionSyntax cond)
    {
        var c = Write(cond.Condition);
        // A branch may hoist statements (string building, temporaries) in front of the
        // current statement. Those must only run when their branch is taken — otherwise
        // `x != null ? "a" + x.Name : "-"` dereferences x before the check. Such a branch
        // becomes a GNU statement expression that holds its own hoisted statements.
        var t = WriteBranch(cond.WhenTrue);
        var f = WriteBranch(cond.WhenFalse);
        return "(" + c + " ? " + t + " : " + f + ")";
    }

    private string WriteBranch(ExpressionSyntax branch)
    {
        var sb = _ctx.Out.GetStringBuilder();
        var start = sb.Length;
        var expr = Write(branch);
        if (sb.Length == start) return expr;

        var hoisted = sb.ToString(start, sb.Length - start);
        var lines = hoisted.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
        // Preprocessor lines and line comments cannot be folded into one expression.
        if (lines.Any(l => l.StartsWith('#') || l.StartsWith("//"))) return expr;

        sb.Length = start;
        return "({ " + string.Join(" ", lines) + " " + expr + "; })";
    }

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
                return objExpr + "[(" + idx0 + ") * " + stride + " + (" + idx1 + ")]";
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
            else
            {
                _ctx.Warn($"^n index on '{objRaw}' whose length is unknown — emitting 0 (heap arrays have no sizeof length)", elem.ToString());
                lenExpr = "0 /* ^n on heap array: length unknown */";
            }
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
            return "Dict_" + cKey + "_" + cVal + "_Get(" + objExpr + ", " + index + ")";
        }

        bool isList = (lt != null && TypeRegistry.IsList(lt))
                   || (ft != null && TypeRegistry.IsList(ft));
        // SemanticModel fallback: detect List/IReadOnlyList via type symbol
        if (!isList && _ctx.SemanticModel != null)
        {
            try
            {
                var exprTypeSym = _ctx.SemanticModel.GetTypeInfo(elem.Expression).Type;
                if (exprTypeSym != null)
                {
                    var exprType = TranspilerContext.FormatTypeSymbol(exprTypeSym);
                    if (TypeRegistry.IsList(exprType))
                    {
                        isList = true;
                        lt = exprType;
                    }
                }
            }
            catch { }
        }
        if (isList)
        {
            var listType = lt ?? ft!;
            var inner = TypeRegistry.GetListInnerType(listType) ?? "int";
            var cInner = inner == "string" ? "str"
                : TypeRegistry.MapType(inner).Replace("*", "ptr").Replace(" ", "_");
            return "List_" + cInner + "_Get(" + objExpr + ", " + index + ")";
        }

        // User-defined indexer (this[T]) → ClassName_get(obj, index)
        var objTypeName = (lt ?? ft ?? "").TrimEnd('*').Trim();
        if (!string.IsNullOrEmpty(objTypeName) && _ctx.IndexerClasses.Contains(objTypeName))
            return objTypeName + "_get(" + objExpr + ", " + index + ")";

        // C# arrays are bounds-checked (IndexOutOfRange / NullReference), see CS2SX_AT.
        if (IsSingleDimArray(elem.Expression) && !IsRuntimeFixedArray(elem.Expression))
            return "CS2SX_AT(" + objExpr + ", " + index + ")";

        return objExpr + "[" + index + "]";
    }

    /// Integer division/modulo whose divisor is not a non-zero constant: C# throws
    /// DivideByZeroException, AArch64 silently yields 0 — so it gets checked.
    private bool NeedsZeroDivisorCheck(BinaryExpressionSyntax bin) =>
        IsIntegralWithRuntimeDivisor(bin, bin.Right);

    private bool IsIntegralWithRuntimeDivisor(ExpressionSyntax typed, ExpressionSyntax divisor)
    {
        if (_ctx.SemanticModel == null) return false;
        try
        {
            var t = _ctx.SemanticModel.GetTypeInfo(typed).Type;
            if (t == null || t.SpecialType is not (SpecialType.System_Int32 or SpecialType.System_UInt32
                    or SpecialType.System_Int64 or SpecialType.System_UInt64 or SpecialType.System_Int16
                    or SpecialType.System_UInt16 or SpecialType.System_Byte or SpecialType.System_SByte
                    or SpecialType.System_Char))
                return false;
            var c = _ctx.SemanticModel.GetConstantValue(divisor);
            return !(c.HasValue && c.Value != null && Convert.ToDecimal(c.Value) != 0);
        }
        catch { return false; }
    }

    private bool IsSingleDimArray(ExpressionSyntax expr)
    {
        if (_ctx.SemanticModel == null) return false;
        try { return _ctx.SemanticModel.GetTypeInfo(expr).Type is IArrayTypeSymbol { Rank: 1 }; }
        catch { return false; }
    }

    /// Array fields of SDK value structs (e.g. TouchState.x) are fixed-size C arrays
    /// without the CS2SX array header, so CS2SX_AT must not read one in front of them.
    private bool IsRuntimeFixedArray(ExpressionSyntax expr)
    {
        if (_ctx.SemanticModel == null) return false;
        try
        {
            var sym = _ctx.SemanticModel.GetSymbolInfo(expr).Symbol;
            var owner = (sym as IFieldSymbol)?.ContainingType ?? (sym as IPropertySymbol)?.ContainingType;
            return owner is { IsValueType: true } && !TranspilerContext.IsUserCode(owner);
        }
        catch { return false; }
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