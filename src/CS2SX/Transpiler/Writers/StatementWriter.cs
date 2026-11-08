using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Writers;

public sealed class StatementWriter
{
    private readonly TranspilerContext _ctx;
    private readonly IExpressionWriter _expr;   // war: ExpressionWriter

    public StatementWriter(TranspilerContext ctx, IExpressionWriter expr)  // war: ExpressionWriter
    {
        _ctx = ctx;
        _expr = expr;
    }
    public void Write(StatementSyntax stmt)
    {
        // C# line for exception stack traces — only for statements in a block (an
        // unbraced `if (x) stmt;` must not end up guarding just this update).
        if (stmt is not (BlockSyntax or EmptyStatementSyntax or LocalFunctionStatementSyntax or LabeledStatementSyntax)
            && stmt.Parent is BlockSyntax or SwitchSectionSyntax)
            _ctx.WriteLine($"CS2SX_LINE({stmt.GetLocation().GetLineSpan().StartLinePosition.Line + 1});");

        switch (stmt)
        {
            case ReturnStatementSyntax ret: WriteReturn(ret); break;
            case LocalDeclarationStatementSyntax l: WriteLocal(l); break;
            case ExpressionStatementSyntax expr: WriteExprStmt(expr); break;
            case IfStatementSyntax ifStmt: WriteIf(ifStmt); break;
            case BlockSyntax block: WriteBlock(block); break;
            case ForStatementSyntax forStmt: InLoop(() => WriteFor(forStmt)); break;
            case ForEachStatementSyntax forEach: InLoop(() => WriteForEach(forEach)); break;
            case ForEachVariableStatementSyntax deconForeach: InLoop(() => WriteForEachDeconstruction(deconForeach)); break;
            case WhileStatementSyntax whileStmt: InLoop(() => WriteWhile(whileStmt)); break;
            case DoStatementSyntax doStmt: InLoop(() => WriteDo(doStmt)); break;
            case BreakStatementSyntax: WriteJump("break;", r => r.BreakDepth == _ctx.BreakDepth); break;
            case ContinueStatementSyntax: WriteJump("continue;", r => r.ContinueDepth == _ctx.ContinueDepth); break;
            case SwitchStatementSyntax sw:
                _ctx.BreakDepth++;
                try { WriteSwitch(sw); } finally { _ctx.BreakDepth--; }
                break;
            case TryStatementSyntax tryStmt: WriteTryCatch(tryStmt); break;
            case ThrowStatementSyntax throwStmt: WriteThrow(throwStmt); break;
            case UsingStatementSyntax usingStmt: WriteUsing(usingStmt); break;
            case LockStatementSyntax lockStmt: WriteLock(lockStmt); break;
            case EmptyStatementSyntax: break;
            case CheckedStatementSyntax checkedStmt:
                _ctx.Warn(checkedStmt, "checked/unchecked block — C has no overflow checking; body emitted as-is");
                _ctx.WriteLine("/* checked/unchecked — no overflow checking in C */");
                WriteBlockOrStmt(checkedStmt.Block);
                break;
            case YieldStatementSyntax yield:
                _ctx.Warn(yield, "yield return/break not supported — C has no generators; refactor to a List<T> or callback pattern");
                _ctx.WriteLine("/* yield not supported — refactor to List<T> or callback */");
                break;
            default:
                _ctx.Warn(stmt, $"unsupported statement '{stmt.GetType().Name}' — check generated C");
                _ctx.WriteLine("/* UNSUPPORTED: " + stmt.GetType().Name + " */");
                break;
        }
    }

    private void WriteReturn(ReturnStatementSyntax ret)
    {
        var line = ret.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        _ctx.CurrentLine = line;

        // Flush using-var cleanups before return (LIFO order)
        foreach (var cleanup in _ctx.PendingUsingVarCleanups)
            _ctx.WriteLine(cleanup);

        if (_ctx.TryRegions.Count > 0)
        {
            // Leaving try/catch blocks: the value is computed first (it may throw and
            // must still be caught), then frames are popped and finally blocks run.
            _ctx.WriteLine("{");
            _ctx.Indent();
            string? tmp = null;
            if (ret.Expression != null)
            {
                tmp = _ctx.NextTmp("ret");
                _ctx.WriteLine("__auto_type " + tmp + " = " + _expr.Write(ret.Expression) + ";");
            }
            EmitUnwind(0);
            _ctx.WriteLineWithMapping(tmp == null ? "return;" : "return " + tmp + ";", line,
                ret.ToString().Trim().Length > 60 ? ret.ToString().Trim()[..60] + "…" : ret.ToString().Trim());
            _ctx.Dedent();
            _ctx.WriteLine("}");
            return;
        }

        if (ret.Expression == null)
            _ctx.WriteLineWithMapping("return;", line, "return;");
        else
        {
            var val = _expr.Write(ret.Expression);
            _ctx.WriteLineWithMapping($"return {val};", line,
                ret.ToString().Trim().Length > 60
                    ? ret.ToString().Trim()[..60] + "…"
                    : ret.ToString().Trim());
        }
    }

    private void WriteExprStmt(ExpressionStatementSyntax expr)
    {
        var line = expr.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        _ctx.CurrentLine = line;

        // Reassigning a local array (`a = new int[10]`) invalidates the compile-time
        // length recorded at its declaration — from here on the runtime header counts.
        if (expr.Expression is AssignmentExpressionSyntax assign
            && assign.OperatorToken.IsKind(SyntaxKind.EqualsToken)
            && assign.Left is IdentifierNameSyntax leftId)
            _ctx.ArrayLengths.Remove(leftId.Identifier.Text);

        var result = _expr.Write(expr.Expression);
        if (!string.IsNullOrEmpty(result))
            _ctx.WriteLineWithMapping(result + ";", line,
                expr.ToString().Trim().Length > 60
                    ? expr.ToString().Trim()[..60] + "…"
                    : expr.ToString().Trim());
    }

    private void WriteLocal(LocalDeclarationStatementSyntax local)
    {
        bool isUsingDecl = local.UsingKeyword.IsKind(SyntaxKind.UsingKeyword);
        var declType = _ctx.ResolveAlias(local.Declaration.Type.ToString().Trim());

        if (TypeRegistry.IsDecimalType(declType))
            _ctx.Warn(local, "decimal is not supported — mapped to double (precision loss possible)");

        if (declType.Contains(",") && declType.Contains("[") && declType.Contains("]"))
        {
            WriteMultiDimArray(local, declType);
            return;
        }

        if (declType.EndsWith("[]") && local.Declaration.Variables.Count == 1)
        {
            var v = local.Declaration.Variables[0];
            if (v.Initializer?.Value != null)
            {
                WriteArrayWithInitializer(v, declType);
                return;
            }
        }

        foreach (var v in local.Declaration.Variables)
        {
            if (TypeRegistry.IsLibNxStruct(declType))
            {
                var si = v.Initializer != null
                    ? " = " + _expr.Write(v.Initializer.Value)
                    : " = {0}";
                _ctx.WriteLine(TypeRegistry.MapType(declType) + " " + v.Identifier + si + ";");
                _ctx.LocalTypes[v.Identifier.Text] = declType;
                if (isUsingDecl)
                    ScheduleUsingVarCleanup(v.Identifier.Text, declType);
                continue;
            }

            if (NullableHandler.IsNullable(declType))
            {
                WriteNullableLocal(v, declType);
                if (isUsingDecl) ScheduleUsingVarCleanup(v.Identifier.Text, declType);
                continue;
            }

            if (declType is "string" or "var"
                && v.Initializer?.Value is ObjectCreationExpressionSyntax strNew
                && strNew.Type.ToString() == "string"
                && strNew.ArgumentList?.Arguments.Count == 2)
            {
                var size = _expr.Write(strNew.ArgumentList.Arguments[1].Expression);
                _ctx.WriteLine("char " + v.Identifier + "[" + size + "];");
                _ctx.WriteLine("memset(" + v.Identifier + ", 0, " + size + ");");
                _ctx.LocalTypes[v.Identifier.Text] = "char[]";
                continue;
            }

            if (v.Initializer?.Value is InvocationExpressionSyntax splitInv
                && IsListStrCall(splitInv))
            {
                var initVal = _expr.Write(v.Initializer.Value);
                _ctx.WriteLine("List_str* " + v.Identifier + " = " + initVal + ";");
                _ctx.LocalTypes[v.Identifier.Text] = "List<string>";
                if (isUsingDecl) ScheduleUsingVarCleanup(v.Identifier.Text, "List<string>");
                continue;
            }

            if (IsExtensionStructType(declType))
            {
                var cType = TypeRegistry.MapType(declType);
                var initStr = v.Initializer != null
                    ? " = " + _expr.Write(v.Initializer.Value)
                    : " = {0}";
                _ctx.WriteLine(cType + " " + v.Identifier + initStr + ";");
                _ctx.LocalTypes[v.Identifier.Text] = declType;
                continue;
            }

            if (declType == "bool")
            {
                var initVal = v.Initializer != null
                    ? _expr.Write(v.Initializer.Value)
                    : "0";
                _ctx.WriteLine("int " + v.Identifier + " = " + initVal + ";");
                _ctx.LocalTypes[v.Identifier.Text] = "bool";
                continue;
            }

            if (_ctx.InterfaceTypes.Contains(declType) && v.Initializer != null)
            {
                // Interface values are IFace* (Class_as_IFace allocates the {vtable, obj} pair).
                var initExprRaw = v.Initializer.Value.ToString().Trim();
                var initCode = _expr.Write(v.Initializer.Value);
                var wrapped = TryWrapAsInterface(initExprRaw, initCode, declType, v.Initializer.Value);

                _ctx.WriteLine(declType + "* " + v.Identifier + " = " + (wrapped ?? initCode) + ";");
                _ctx.LocalTypes[v.Identifier.Text] = declType;
                continue;
            }

            if (_ctx.InterfaceTypes.Contains(declType))
            {
                _ctx.WriteLine(declType + "* " + v.Identifier + " = NULL;");
                _ctx.LocalTypes[v.Identifier.Text] = declType;
                continue;
            }

            if (declType.EndsWith("[]")
                && v.Initializer?.Value is ArrayCreationExpressionSyntax arrCreate)
            {
                WriteArrayAlloc(v, declType, arrCreate);
                continue;
            }

            // var x = new T[n]  →  treat as typed array allocation
            if ((declType is "var" or "var?")
                && v.Initializer?.Value is ArrayCreationExpressionSyntax varArrCreate)
            {
                var elemTypeName = varArrCreate.Type.ElementType.ToString().Trim();
                WriteArrayAlloc(v, elemTypeName + "[]", varArrCreate);
                continue;
            }

            // var x = new[] { a, b }  →  element type from the semantic model
            if (declType is "var" && v.Initializer?.Value is ImplicitArrayCreationExpressionSyntax)
            {
                WriteArrayWithInitializer(v, "var[]");
                continue;
            }

            var (cTypeFinal, isPtr) = InferLocalType(declType, v);
            if (string.IsNullOrWhiteSpace(cTypeFinal)) cTypeFinal = "int";
            // Inference can yield a C# keyword (e.g. `var s = c ? "a" : "b"` → "string").
            if (cTypeFinal is "string" or "bool") cTypeFinal = TypeRegistry.MapType(cTypeFinal);

            // Build the initializer first (this emits any LINQ/collection expansion
            // and returns the resulting expression — often a registered temp var).
            var init = BuildLocalInit(v, isPtr, declType, cTypeFinal);

            // Resolve the C# type of a `var` initializer. The semantic model often
            // can't type LINQ extension-method results, so prefer the type the
            // handler registered for the produced temp var; fall back to semantic.
            string? varCs = null;
            if (declType is "var" or "var?")
            {
                var rhsKey = init.StartsWith(" = ", StringComparison.Ordinal)
                    ? init.Substring(3).Trim() : "";
                if (rhsKey.Length > 0 && _ctx.LocalTypes.TryGetValue(rhsKey, out var rhsType))
                    varCs = rhsType;
                varCs ??= ResolveVarCsType(v);

                // Correct the C declaration type when `var` resolved to a collection
                // /reference type but InferVarType fell back to a scalar.
                if (varCs != null && cTypeFinal == "int"
                    && (TypeRegistry.IsList(varCs) || TypeRegistry.IsDictionary(varCs)
                        || TypeRegistry.IsStack(varCs) || TypeRegistry.IsQueue(varCs)
                        || TypeRegistry.IsHashSet(varCs) || TypeRegistry.IsStringBuilder(varCs)
                        || TypeRegistry.NeedsPointerSuffix(varCs)))
                {
                    var mapped = TypeRegistry.MapType(varCs);
                    cTypeFinal = mapped;
                    isPtr = !mapped.EndsWith("*");
                }
                // Inferred the right reference type but without the pointer (e.g. the
                // element type of `list.First()`): class instances are always T*.
                else if (varCs != null && !isPtr && !cTypeFinal.EndsWith("*")
                         && TypeRegistry.NeedsPointerSuffix(varCs)
                         && TypeRegistry.MapType(varCs) == cTypeFinal)
                {
                    isPtr = true;
                }
            }

            var ptr = isPtr ? "*" : "";
            _ctx.WriteLine(cTypeFinal + ptr + " " + v.Identifier + init + ";");

            // Register the C# type (not the C name) so collection/LINQ handlers,
            // which key off `List<T>` etc. via IsList, recognise `var` locals too.
            // (Previously a `var` local stored "List_int*", which IsList rejects, so
            // LINQ silently mangled to List_int_OrderBy.)
            string registeredType;
            if (cTypeFinal is "List_str" or "List_str*")
                registeredType = "List<string>";
            else if (declType is "var" or "var?")
                registeredType = varCs ?? cTypeFinal;
            else
                registeredType = declType;
            _ctx.LocalTypes[v.Identifier.Text] = registeredType;

            if (isUsingDecl) ScheduleUsingVarCleanup(v.Identifier.Text, registeredType);
        }
    }

    // Resolves the C# type of a `var` initializer (e.g. "List<int>", "Dog") so it
    // can be registered in LocalTypes in the form handlers expect. Returns null if
    // it can't be determined (caller falls back to the C type).
    private string? ResolveVarCsType(VariableDeclaratorSyntax v)
    {
        var init = v.Initializer?.Value;
        if (init == null) return null;
        if (init is ObjectCreationExpressionSyntax oc)
            return oc.Type.ToString().Trim();
        if (_ctx.SemanticModel != null)
        {
            try
            {
                var ti = _ctx.SemanticModel.GetTypeInfo(init);
                var sym = ti.Type ?? ti.ConvertedType;
                if (sym != null && sym is not IErrorTypeSymbol)
                {
                    var s = TranspilerContext.FormatTypeSymbol(sym);
                    if (!string.IsNullOrEmpty(s) && s != "?" && s != "object" && s != "var")
                        return s;
                }
            }
            catch { }
        }
        return null;
    }

    // Schedules a cleanup action for a "using var" declaration.
    // These are flushed (LIFO) before each return statement.
    private void ScheduleUsingVarCleanup(string varName, string csType)
    {
        string cleanup;
        if (TypeRegistry.IsStringBuilder(csType))
            cleanup = $"if ({varName}) StringBuilder_Free({varName});";
        else if (TypeRegistry.IsList(csType))
        {
            var inner = TypeRegistry.GetListInnerType(csType) ?? "int";
            var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
            cleanup = $"if ({varName}) List_{cInner}_Free({varName});";
        }
        else if (TypeRegistry.IsDictionary(csType))
        {
            var types = TypeRegistry.GetDictionaryTypes(csType);
            if (types.HasValue)
            {
                var ck = types.Value.key == "string" ? "str" : TypeRegistry.MapType(types.Value.key);
                var cv = types.Value.val == "string" ? "str" : TypeRegistry.MapType(types.Value.val);
                cleanup = $"if ({varName}) Dict_{ck}_{cv}_Free({varName});";
            }
            else cleanup = $"/* using var {varName}: free manually */";
        }
        else if (TypeRegistry.IsDisposable(csType))
            cleanup = $"if ({varName}) {csType}_Dispose({varName});";
        else if (!TypeRegistry.IsPrimitive(csType) && csType != "string"
                 && TypeRegistry.NeedsPointerSuffix(csType))
            cleanup = $"if ({varName}) {TypeRegistry.MapType(csType)}_Free({varName});";
        else
            return; // primitives don't need cleanup
        _ctx.PendingUsingVarCleanups.Push(cleanup);
    }

    private string? TryWrapAsInterface(string exprRaw, string exprCode, string targetIfaceName,
        Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax? exprSyn = null)
    {
        if (!_ctx.InterfaceTypes.Contains(targetIfaceName)) return null;
        var key = exprRaw.TrimStart('_');
        string? csType = null;
        _ctx.LocalTypes.TryGetValue(exprRaw, out csType);
        if (csType == null) _ctx.FieldTypes.TryGetValue(key, out csType);
        // SemanticModel fallback for function call expressions
        if (csType == null && exprSyn != null && _ctx.SemanticModel != null)
        {
            try
            {
                var sym = _ctx.SemanticModel.GetTypeInfo(exprSyn).Type as Microsoft.CodeAnalysis.INamedTypeSymbol;
                if (sym != null) csType = sym.Name;
            }
            catch { }
        }
        if (csType == null) return null;
        var bareType = csType.TrimEnd('*').Trim();
        if (bareType == targetIfaceName) return null;
        if (!TypeRegistry.NeedsPointerSuffix(bareType)) return null; // not a class
        return bareType + "_as_" + targetIfaceName + "(" + exprCode + ")";
    }

    private void WriteArrayAlloc(VariableDeclaratorSyntax v, string declType,
        ArrayCreationExpressionSyntax arrCreate)
    {
        var baseType = declType[..^2].Trim();
        var varName = v.Identifier.Text;
        var elemC = ArrayModel.ElemCType(baseType);

        List<string>? elems = arrCreate.Initializer?.Expressions.Select(e => _expr.Write(e)).ToList();

        string countExpr;
        int constCount;
        if (arrCreate.Type.RankSpecifiers.Count > 0
            && arrCreate.Type.RankSpecifiers[0].Sizes.Count > 0
            && arrCreate.Type.RankSpecifiers[0].Sizes[0] is not OmittedArraySizeExpressionSyntax)
        {
            var sizeNode = arrCreate.Type.RankSpecifiers[0].Sizes[0];
            countExpr = _expr.Write(sizeNode);
            constCount = ArrayModel.ConstantSize(sizeNode, _ctx);
        }
        else
        {
            constCount = elems?.Count ?? 0;
            countExpr = constCount.ToString();
        }

        WriteLocalArray(v, varName, elemC, countExpr, constCount, elems);
        _ctx.LocalTypes[varName] = declType;
    }

    /// Declares a local array: on the stack when it is small, constant-sized and
    /// never leaves the method; otherwise on the heap. Both carry the length header.
    private void WriteLocalArray(VariableDeclaratorSyntax v, string varName, string elemC,
        string countExpr, int constCount, List<string>? elems)
    {
        if (constCount >= 0 && ArrayModel.CanLiveOnStack(v, constCount, elemC, _ctx))
            _ctx.WriteLine(ArrayModel.LocalDecl(elemC, varName, constCount.ToString(), elems));
        else if (elems is { Count: > 0 })
            _ctx.WriteLine(elemC + "* " + varName + " = " + ArrayModel.FromInit(elemC, elems) + ";");
        else
            _ctx.WriteLine(elemC + "* " + varName + " = " + ArrayModel.NewHeap(elemC, countExpr) + ";");

        if (constCount >= 0 && ArrayModel.IsNeverReassigned(v, _ctx))
            _ctx.ArrayLengths[varName] = constCount.ToString();
    }

    private static bool IsConstantSizeExpr(SyntaxNode node) =>
        node is LiteralExpressionSyntax
        || (node is BinaryExpressionSyntax bin
            && IsConstantSizeExpr(bin.Left)
            && IsConstantSizeExpr(bin.Right));

    private void WriteMultiDimArray(LocalDeclarationStatementSyntax local, string declType)
    {
        var baseType = declType[..declType.IndexOf('[')].Trim();
        var cType = TypeRegistry.MapType(baseType);

        foreach (var v in local.Declaration.Variables)
        {
            var varName = v.Identifier.Text;

            if (v.Initializer?.Value is ArrayCreationExpressionSyntax arr)
            {
                // Extract dimension sizes for stride calculation
                var dimSizes = new List<string>();
                if (arr.Type.RankSpecifiers.Count > 0)
                {
                    foreach (var size in arr.Type.RankSpecifiers[0].Sizes)
                    {
                        if (size is not OmittedArraySizeExpressionSyntax)
                            dimSizes.Add(_expr.Write(size));
                    }
                }

                if (arr.Initializer != null)
                {
                    // Flatten nested initializers: { {1,2}, {3,4} } → { 1, 2, 3, 4 }
                    var flatElems = new List<string>();
                    foreach (var row in arr.Initializer.Expressions)
                    {
                        if (row is ImplicitArrayCreationExpressionSyntax rowArr)
                            foreach (var elem in rowArr.Initializer.Expressions)
                                flatElems.Add(_expr.Write(elem));
                        else if (row is InitializerExpressionSyntax initRow)
                            foreach (var elem in initRow.Expressions)
                                flatElems.Add(_expr.Write(elem));
                        else
                            flatElems.Add(_expr.Write(row));
                    }

                    // Infer dimensions from initializer if not explicit
                    if (dimSizes.Count < 2 && arr.Initializer.Expressions.Count > 0)
                    {
                        var rows = arr.Initializer.Expressions.Count;
                        var cols = flatElems.Count / rows;
                        if (dimSizes.Count == 0)
                        {
                            dimSizes.Add(rows.ToString());
                            dimSizes.Add(cols.ToString());
                        }
                        else if (dimSizes.Count == 1)
                        {
                            dimSizes.Add(cols.ToString());
                        }
                    }

                    _ctx.WriteLine(cType + " " + varName + "[] = { " + string.Join(", ", flatElems) + " };");
                    _ctx.ArrayLengths[varName] = flatElems.Count.ToString();

                    // Store stride for [i,j] → [i*cols+j] rewriting
                    if (dimSizes.Count >= 2)
                        _ctx.LocalTypes["__stride__" + varName] = dimSizes[1]; // cols
                }
                else if (dimSizes.Count >= 2)
                {
                    var totalSize = string.Join("*", dimSizes);
                    _ctx.WriteLine(cType + "* " + varName +
                        " = (" + cType + "*)calloc(" + totalSize + ", sizeof(" + cType + "));");
                    _ctx.ArrayLengths[varName] = totalSize;
                    _ctx.LocalTypes["__stride__" + varName] = dimSizes[1];
                }
                else
                {
                    _ctx.WriteLine(cType + " " + varName + "[1]; /* multi-dim array, size unknown */");
                }
            }
            else
            {
                _ctx.WriteLine(cType + " " + varName + "[1]; /* multi-dim array */");
            }

            _ctx.LocalTypes[varName] = baseType + "[]";
        }
    }
    private static bool arr_RankSizes(VariableDeclaratorSyntax v, out List<string> sizes)
    {
        sizes = new();
        if (v.Initializer?.Value is ArrayCreationExpressionSyntax arr)
            foreach (var rs in arr.Type.RankSpecifiers)
                foreach (var sz in rs.Sizes)
                    if (sz is not OmittedArraySizeExpressionSyntax)
                        sizes.Add(sz.ToString());
        return sizes.Count > 0;
    }

    private void WriteArrayWithInitializer(VariableDeclaratorSyntax v, string declType)
    {
        var baseType = declType[..^2].Trim();
        var varName = v.Identifier.Text;
        SyntaxNode? initExpr = v.Initializer?.Value;

        List<string>? elems = null;
        string? cType = null;

        if (initExpr is ImplicitArrayCreationExpressionSyntax implArr)
        {
            elems = implArr.Initializer.Expressions.Select(e => _expr.Write(e)).ToList();
            cType = InferArrayElemType(implArr.Initializer.Expressions, baseType);
        }
        else if (initExpr is ArrayCreationExpressionSyntax arrCreation
                 && arrCreation.Initializer != null)
        {
            elems = arrCreation.Initializer.Expressions.Select(e => _expr.Write(e)).ToList();
            cType = TypeRegistry.MapType(baseType);
        }
        else if (initExpr is InitializerExpressionSyntax initList)
        {
            elems = initList.Expressions.Select(e => _expr.Write(e)).ToList();
            cType = TypeRegistry.MapType(baseType);
        }

        if (elems != null && cType != null)
        {
            // `var x = new[] {..}`: element type from the semantic model.
            var elemCs = baseType == "var"
                ? (ArrayModel.ElementCsType((ExpressionSyntax)initExpr!, _ctx) ?? "int")
                : baseType;
            WriteLocalArray(v, varName, ArrayModel.ElemCType(elemCs), elems.Count.ToString(), elems.Count, elems);
            _ctx.LocalTypes[varName] = elemCs + "[]";
            return;
        }

        var (cTypeFb, isPtr) = InferLocalType(declType, v);
        var ptr = isPtr ? "*" : "";
        var init2 = v.Initializer != null ? " = " + _expr.Write(v.Initializer.Value) : "";
        _ctx.WriteLine((cTypeFb ?? "int") + ptr + " " + varName + init2 + ";");
        _ctx.LocalTypes[varName] = declType;
    }

    private static string InferArrayElemType(
        Microsoft.CodeAnalysis.SeparatedSyntaxList<ExpressionSyntax> exprs,
        string hintBaseType)
    {
        if (hintBaseType != "var") return TypeRegistry.MapType(hintBaseType);
        if (exprs.Count == 0) return "int";
        var first = exprs[0];
        if (first is LiteralExpressionSyntax lit)
        {
            if (lit.Token.Value is string) return "const char*";
            if (lit.Token.Value is float) return "float";
            if (lit.Token.Value is double) return "double";
        }
        return "int";
    }

    private static bool IsExtensionStructType(string csType) =>
        csType is "TouchState" or "StickPos" or "BatteryInfo" or "TimeInfo" or "MotionState";

    private void WriteNullableLocal(VariableDeclaratorSyntax v, string declType)
    {
        // Nullable value type → a value struct on the stack. The initializer goes
        // through ExpressionWriter, which applies the T → T? / null → T? conversion.
        var nullableC = TypeRegistry.MapType(declType);
        var varName = v.Identifier.Text;
        var init = v.Initializer != null
            ? _expr.Write(v.Initializer.Value)
            : NullableHandler.Null(nullableC);
        _ctx.WriteLine(nullableC + " " + varName + " = " + init + ";");
        _ctx.LocalTypes[varName] = declType;
    }

    private (string cType, bool isPtr) InferLocalType(
        string declType, VariableDeclaratorSyntax v)
    {
        if (declType is "var" or "var?")
            return InferVarType(v);
        if (declType == "bool") return ("int", false);
        var cType = TypeRegistry.MapType(declType);
        // MapType already appends '*' for List<T>, Dictionary<K,V>, string (const char*) etc.
        // Returning isPtr=true on top of that would produce a double-pointer.
        if (cType.EndsWith("*"))
            return (cType, false);
        var isPtr = TypeRegistry.NeedsPointerSuffix(declType)
                 || TypeRegistry.IsStringBuilder(declType)
                 || TypeRegistry.IsList(declType)
                 || TypeRegistry.IsDictionary(declType);
        return (cType, isPtr);
    }

    // FIX: Directory.GetEntries und CS2SX_Dir_GetEntries als List_str* erkannt.

    private (string cType, bool isPtr) InferVarType(VariableDeclaratorSyntax v)
    {
        // User value structs are never pointers — resolve via the semantic model so
        // `var d = a - b` / `var p = Point.Origin()` work, not only `new Point(..)`.
        if (v.Initializer?.Value is { } initExpr)
        {
            var semType = _ctx.GetSemanticType(initExpr);
            if (semType != null && TypeRegistry.IsUserStruct(semType))
                return (semType, false);
            // T[] → element pointer (header array), e.g. `var parts = s.Split(',')`
            if (semType != null && semType.EndsWith("[]") && !semType.Contains(','))
                return (ArrayModel.ElemCType(semType[..^2]), true);
        }

        if (v.Initializer?.Value is ObjectCreationExpressionSyntax oc)
        {
            // Map through TypeRegistry so List<T>/Dict<K,V>/etc. get their C names.
            // MapType("List<RepoEntry>") → "List_RepoEntry*" (already carries *).
            var ocTypeName = oc.Type.ToString().Trim();
            var cMapped = TypeRegistry.MapType(ocTypeName);
            if (cMapped.EndsWith("*"))
                return (cMapped, false); // MapType already appended pointer
            return (cMapped, !TypeRegistry.IsUserStruct(ocTypeName));
        }

        if (v.Initializer?.Value is InvocationExpressionSyntax inv)
        {
            if (IsSplitCall(inv)) return ("List_str", true);

            var calleeStr = inv.Expression.ToString();

            // FIX: Alle Directory/File-Methoden die List_str* zurückgeben
            if (calleeStr is "Directory.GetFiles"
                          or "CS2SX_Dir_GetFiles"
                          or "Directory.GetDirectories"
                          or "CS2SX_Dir_GetDirectories"
                          or "Directory.GetEntries"        // NEU
                          or "CS2SX_Dir_GetEntries"        // NEU
                          or "String_Split"
                          or "string.Split"
                          or "String.Split"
                          or "CS2SX_File_ReadAllLines"
                          or "File.ReadAllLines")
                return ("List_str", true);

            if (calleeStr is "string.Format"
                          or "String.Format"
                          or "string.Concat"
                          or "String.Concat")
                return ("const char", true);

            if (calleeStr is "Input.GetTouch"
                          or "CS2SX_Input_GetTouch")
                return ("CS2SX_TouchState", false);

            if (calleeStr is "Input.GetStickLeft"
                          or "_cs2sx_get_stick_left")
                return ("CS2SX_StickPos", false);

            if (calleeStr is "Input.GetStickRight"
                          or "_cs2sx_get_stick_right")
                return ("CS2SX_StickPos", false);

            if (calleeStr is "System.GetBattery"
                          or "CS2SX_GetBattery"
                          or "NX.GetBattery")
                return ("CS2SX_BatteryInfo", false);

            if (calleeStr is "System.GetTime"
                          or "CS2SX_GetTime"
                          or "NX.GetTime")
                return ("CS2SX_TimeInfo", false);

            if (calleeStr is "Motion.Get" or "CS2SX_Motion_Get")
                return ("CS2SX_MotionState", false);

            if (calleeStr is "Stopwatch.StartNew"
                          or "CS2SX_Stopwatch_StartNew")
                return ("CS2SX_Stopwatch", true);

            if (_ctx.MethodReturnTypes.TryGetValue(calleeStr, out var retType))
            {
                var needsPtr = TypeRegistry.NeedsPointerSuffix(retType)
                            || TypeRegistry.IsStringBuilder(retType)
                            || TypeRegistry.IsList(retType)
                            || TypeRegistry.IsDictionary(retType)
                            || TypeRegistry.IsControlType(retType);
                var cMapped = TypeRegistry.MapType(retType).TrimEnd('*');
                return (cMapped, needsPtr);
            }

            var inferred = TypeInferrer.InferCSharpType(v.Initializer!.Value, _ctx);
            if (inferred is "TouchState") return ("CS2SX_TouchState", false);
            if (inferred is "StickPos") return ("CS2SX_StickPos", false);
            if (inferred is "BatteryInfo") return ("CS2SX_BatteryInfo", false);
            if (inferred is "TimeInfo") return ("CS2SX_TimeInfo", false);
            if (inferred is "MotionState") return ("CS2SX_MotionState", false);

            if (TypeRegistry.IsList(inferred)
                || TypeRegistry.IsDictionary(inferred)
                || TypeRegistry.IsStringBuilder(inferred)
                || TypeRegistry.IsStack(inferred)
                || TypeRegistry.IsQueue(inferred)
                || TypeRegistry.IsHashSet(inferred))
                return (TypeRegistry.MapType(inferred).TrimEnd('*'), true);

            if (inferred.EndsWith("*"))
                return (inferred.TrimEnd('*'), true);

            return (inferred, false);
        }

        var ct = TypeInferrer.InferCSharpType(v.Initializer?.Value, _ctx);
        if (ct is "TouchState") return ("CS2SX_TouchState", false);
        if (ct is "StickPos") return ("CS2SX_StickPos", false);
        if (ct is "BatteryInfo") return ("CS2SX_BatteryInfo", false);
        if (ct is "TimeInfo") return ("CS2SX_TimeInfo", false);

        if (TypeRegistry.IsList(ct)
            || TypeRegistry.IsDictionary(ct)
            || TypeRegistry.IsStringBuilder(ct)
            || TypeRegistry.IsStack(ct)
            || TypeRegistry.IsQueue(ct)
            || TypeRegistry.IsHashSet(ct))
            return (TypeRegistry.MapType(ct).TrimEnd('*'), true);

        if (ct.EndsWith("*"))
            return (ct.TrimEnd('*'), true);

        return (ct, false);
    }

    private bool IsSplitCall(InvocationExpressionSyntax inv) => IsListStrCall(inv);

    /// True when the call yields a List&lt;string&gt; in C#. Calls returning string[]
    /// (Split, GetFiles, ReadAllLines) are arrays — see InvocationDispatcher.AdaptStringArrayResult.
    private bool IsListStrCall(InvocationExpressionSyntax inv)
    {
        var sem = _ctx.GetSemanticType(inv);
        if (sem != null)
            return TypeRegistry.IsList(sem) && TypeRegistry.GetListInnerType(sem) == "string";

        var calleeStr = inv.Expression.ToString();
        if (calleeStr is "string.Split" or "String.Split") return true;
        if (calleeStr is "Directory.GetFiles") return true;
        if (inv.Expression is MemberAccessExpressionSyntax m
            && m.Name.Identifier.Text == "Split")
            return true;
        return false;
    }

    private string BuildLocalInit(VariableDeclaratorSyntax v,
        bool isPtr, string declType, string cType)
    {
        if (v.Initializer != null)
        {
            var rhs = _expr.Write(v.Initializer.Value);
            // Covariant assignment `Animal a = new Dog(...)`: the declared base type
            // and the derived RHS are distinct C struct pointers, so cast to the
            // base (valid — the base struct is the first member). GCC 14 treats the
            // mismatch as an error, not a warning.
            if (isPtr && declType is not ("var" or "var?")
                && TypeRegistry.NeedsPointerSuffix(declType)
                && NeedsCovariantCast(declType, v.Initializer.Value))
                rhs = "(" + declType + "*)(" + rhs + ")";
            return " = " + rhs;
        }
        if (!isPtr && TypeRegistry.IsPrimitive(
                declType is "var" or "var?" ? cType : declType))
            return " = 0";
        return "";
    }

    // True when the initializer's static type is a strict subclass of declType
    // (so the C pointer types differ and a cast is required).
    private bool NeedsCovariantCast(string declType, ExpressionSyntax init)
    {
        if (_ctx.SemanticModel == null) return false;
        try
        {
            var ti = _ctx.SemanticModel.GetTypeInfo(init);
            var rhsSym = (ti.Type ?? ti.ConvertedType) as INamedTypeSymbol;
            if (rhsSym == null || rhsSym.Name == declType) return false;
            var t = rhsSym.BaseType;
            while (t != null && t.SpecialType == SpecialType.None)
            {
                if (t.Name == declType) return true;
                t = t.BaseType;
            }
        }
        catch { }
        return false;
    }

    // ── If ────────────────────────────────────────────────────────────────────

    private void WriteIf(IfStatementSyntax ifStmt)
    {
        if (TryExtractOutVarFromCondition(ifStmt.Condition, out var outVarDecls))
            foreach (var (varName, varType) in outVarDecls)
            {
                var cType = TypeRegistry.MapType(varType);
                _ctx.WriteLine(cType + " " + varName + " = 0;");
                _ctx.LocalTypes[varName] = varType;
            }

        if (ifStmt.Condition is IsPatternExpressionSyntax isPattern)
        {
            WriteIfWithIsPattern(ifStmt, isPattern);
            return;
        }

        _ctx.WriteLine("if (" + _expr.Write(ifStmt.Condition) + ")");
        WriteBlockOrStmt(ifStmt.Statement);

        if (ifStmt.Else == null) return;

        if (ifStmt.Else.Statement is IfStatementSyntax nested)
        {
            _ctx.Out.Write(_ctx.Tab + "else ");
            WriteIfInline(nested);
        }
        else
        {
            _ctx.WriteLine("else");
            WriteBlockOrStmt(ifStmt.Else.Statement);
        }
    }

    private static bool TryExtractOutVarFromCondition(
        ExpressionSyntax condition,
        out List<(string name, string type)> outVars)
    {
        outVars = new();
        IEnumerable<ArgumentSyntax> args = condition switch
        {
            InvocationExpressionSyntax inv => inv.ArgumentList.Arguments,
            _ => Enumerable.Empty<ArgumentSyntax>()
        };
        foreach (var arg in args)
        {
            if (arg.RefKindKeyword.IsKind(SyntaxKind.OutKeyword)
                && arg.Expression is DeclarationExpressionSyntax decl
                && decl.Designation is SingleVariableDesignationSyntax desig)
            {
                var typeName = decl.Type.ToString().Trim() == "var"
                    ? InferOutVarType(condition, desig.Identifier.Text)
                    : decl.Type.ToString().Trim();
                outVars.Add((desig.Identifier.Text, typeName));
            }
        }
        return outVars.Count > 0;
    }

    private static string InferOutVarType(ExpressionSyntax condition, string varName)
    {
        if (condition is InvocationExpressionSyntax inv)
        {
            var callee = inv.Expression.ToString();
            if (callee is "int.TryParse" or "Int32.TryParse") return "int";
            if (callee is "float.TryParse" or "Single.TryParse") return "float";
            if (callee is "double.TryParse" or "Double.TryParse") return "double";
            if (callee is "long.TryParse" or "Int64.TryParse") return "long";
            if (callee is "uint.TryParse" or "UInt32.TryParse") return "uint";
            if (callee is "ulong.TryParse" or "UInt64.TryParse") return "ulong";
            if (callee is "short.TryParse" or "Int16.TryParse") return "short";
            if (callee is "ushort.TryParse" or "UInt16.TryParse") return "ushort";
            if (callee is "byte.TryParse" or "Byte.TryParse") return "byte";
            if (callee is "sbyte.TryParse" or "SByte.TryParse") return "sbyte";
            if (callee is "bool.TryParse" or "Boolean.TryParse") return "bool";
        }
        return "int";
    }

    private void WriteIfWithIsPattern(IfStatementSyntax ifStmt,
        IsPatternExpressionSyntax isPattern)
    {
        var cond = _expr.Write(isPattern);
        _ctx.WriteLine("if (" + cond + ")");
        _ctx.WriteLine("{");
        _ctx.Indent();
        if (isPattern.Pattern is DeclarationPatternSyntax dp
            && dp.Designation is SingleVariableDesignationSyntax desig)
        {
            var typeName = dp.Type.ToString().Trim();
            var cType = TypeRegistry.MapType(typeName);
            var subject = _expr.Write(isPattern.Expression);
            _ctx.WriteLine(cType + "* " + desig.Identifier.Text
                         + " = (" + cType + "*)" + subject + ";");
            _ctx.LocalTypes[desig.Identifier.Text] = typeName;
        }
        if (ifStmt.Statement is BlockSyntax block)
            foreach (var s in block.Statements) Write(s);
        else
            Write(ifStmt.Statement);
        _ctx.Dedent();
        _ctx.WriteLine("}");
        if (ifStmt.Else == null) return;
        if (ifStmt.Else.Statement is IfStatementSyntax nested)
        {
            _ctx.Out.Write(_ctx.Tab + "else ");
            WriteIfInline(nested);
        }
        else
        {
            _ctx.WriteLine("else");
            WriteBlockOrStmt(ifStmt.Else.Statement);
        }
    }

    private void WriteIfInline(IfStatementSyntax ifStmt)
    {
        if (TryExtractOutVarFromCondition(ifStmt.Condition, out var outVarDecls))
            foreach (var (varName, varType) in outVarDecls)
            {
                var cType = TypeRegistry.MapType(varType);
                _ctx.WriteLine(cType + " " + varName + " = 0;");
                _ctx.LocalTypes[varName] = varType;
            }

        if (ifStmt.Condition is IsPatternExpressionSyntax isPattern)
        {
            _ctx.Out.WriteLine("if (" + _expr.Write(isPattern) + ")");
            _ctx.WriteLine("{");
            _ctx.Indent();
            if (isPattern.Pattern is DeclarationPatternSyntax dp
                && dp.Designation is SingleVariableDesignationSyntax desig)
            {
                var typeName = dp.Type.ToString().Trim();
                var cType = TypeRegistry.MapType(typeName);
                var subject = _expr.Write(isPattern.Expression);
                _ctx.WriteLine(cType + "* " + desig.Identifier.Text
                             + " = (" + cType + "*)" + subject + ";");
                _ctx.LocalTypes[desig.Identifier.Text] = typeName;
            }
            if (ifStmt.Statement is BlockSyntax blk)
                foreach (var s in blk.Statements) Write(s);
            else
                Write(ifStmt.Statement);
            _ctx.Dedent();
            _ctx.WriteLine("}");
        }
        else
        {
            _ctx.Out.WriteLine("if (" + _expr.Write(ifStmt.Condition) + ")");
            WriteBlockOrStmt(ifStmt.Statement);
        }
        if (ifStmt.Else == null) return;
        if (ifStmt.Else.Statement is IfStatementSyntax nested)
        {
            _ctx.Out.Write(_ctx.Tab + "else ");
            WriteIfInline(nested);
        }
        else
        {
            _ctx.WriteLine("else");
            WriteBlockOrStmt(ifStmt.Else.Statement);
        }
    }

    // ── For ───────────────────────────────────────────────────────────────────

    private void WriteFor(ForStatementSyntax forStmt)
    {
        string init;
        if (forStmt.Declaration != null)
        {
            var tName = TypeRegistry.MapType(forStmt.Declaration.Type.ToString().Trim());
            var vars = string.Join(", ", forStmt.Declaration.Variables.Select(v =>
            {
                var ie = v.Initializer != null
                    ? " = " + _expr.Write(v.Initializer.Value)
                    : "";
                _ctx.LocalTypes[v.Identifier.Text] = forStmt.Declaration.Type.ToString().Trim();
                return v.Identifier + ie;
            }));
            init = tName + " " + vars;
        }
        else
        {
            init = string.Join(", ", forStmt.Initializers.Select(e => _expr.Write(e)));
        }
        var cond = forStmt.Condition != null ? _expr.Write(forStmt.Condition) : "";
        var incr = string.Join(", ", forStmt.Incrementors.Select(e => _expr.Write(e)));
        _ctx.WriteLine("for (" + init + "; " + cond + "; " + incr + ")");
        WriteBlockOrStmt(forStmt.Statement);
    }

    // ── ForEach ───────────────────────────────────────────────────────────────

    private void WriteForEach(ForEachStatementSyntax forEach)
    {
        var colRaw = forEach.Expression.ToString();
        var colKey = colRaw.TrimStart('_');
        var colExpr = _expr.Write(forEach.Expression);
        // foreach evaluates its collection exactly once. A call such as s.Split(' ')
        // or GetItems() would otherwise run again for the bound and every element.
        if (forEach.Expression is not (IdentifierNameSyntax or MemberAccessExpressionSyntax
                                       or ThisExpressionSyntax))
        {
            var colTmp = _ctx.NextTmp("fe");
            _ctx.WriteLine("__auto_type " + colTmp + " = " + colExpr + ";");
            colExpr = colTmp;
        }
        var varName = forEach.Identifier.Text;
        var idxVar = "_i_" + varName;

        _ctx.LocalTypes.TryGetValue(colRaw, out var colLt);
        _ctx.FieldTypes.TryGetValue(colKey, out var colFt);
        var colType = colLt ?? colFt ?? "";

        bool isList = TypeRegistry.IsList(colType);
        bool isDict = TypeRegistry.IsDictionary(colType);
        bool isString = colType is "string" or "char[]";
        bool isRawArray = colType.EndsWith("[]") && !isList;

        if (isDict)
        {
            WriteForEachDict(forEach, colExpr, colType, varName);
            return;
        }

        // Attempt SemanticModel lookup when colType is unknown
        if (string.IsNullOrEmpty(colType) && _ctx.SemanticModel != null)
        {
            try
            {
                var typeInfo = _ctx.SemanticModel.GetTypeInfo(forEach.Expression);
                var typeSymbol = typeInfo.Type ?? typeInfo.ConvertedType;
                if (typeSymbol != null && typeSymbol is not Microsoft.CodeAnalysis.IErrorTypeSymbol)
                {
                    colType = TranspilerContext.FormatTypeSymbol(typeSymbol);
                    isList = TypeRegistry.IsList(colType);
                    isDict = TypeRegistry.IsDictionary(colType);
                    isString = colType is "string" or "char[]";
                    isRawArray = colType.EndsWith("[]") && !isList;
                }
            }
            catch { }
        }

        // Still unknown after semantic lookup → emit a hard compile error so the user notices
        if (string.IsNullOrEmpty(colType) && !isList && !isDict && !isString && !isRawArray)
        {
            _ctx.Warn($"foreach: Collection-Typ von '{colRaw}' nicht bestimmbar. " +
                      $"Bitte expliziten Typ deklarieren (kein var). " +
                      $"Im generierten C wurde ein #error eingefügt.",
                      colRaw);
            _ctx.WriteLine($"#error \"CS2SX: foreach({varName} in {colRaw}) — collection type unknown. Declare '{colRaw}' with an explicit type.\"");
            return;
        }

        string lenExpr;
        if (isList)
            lenExpr = colExpr + "->count";
        else if (isString)
            lenExpr = "strlen(" + colExpr + ")";
        else if (isRawArray)
            lenExpr = ResolveArrayLength(colRaw, colKey, colExpr);
        else
            lenExpr = colExpr + "->count";

        var rawElemType = forEach.Type.ToString().Trim();
        if (rawElemType == "var")
        {
            if (isList) rawElemType = TypeRegistry.GetListInnerType(colType) ?? "int";
            else if (isString) rawElemType = "char";
            else if (isRawArray) rawElemType = colType[..^2].Trim();
            else rawElemType = "int";
        }

        _ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < (int)({lenExpr}); {idxVar}++)");
        _ctx.WriteLine("{");
        _ctx.Indent();

        WriteForEachLoopVar(varName, idxVar, rawElemType, colExpr, colType,
            isList, isString, isRawArray);
        _ctx.LocalTypes[varName] = rawElemType;

        var bodyStmts = forEach.Statement is BlockSyntax b
            ? b.Statements.Cast<StatementSyntax>()
            : new[] { forEach.Statement };
        foreach (var s in bodyStmts)
            Write(s);

        _ctx.Dedent();
        _ctx.WriteLine("}");
    }

    private void WriteForEachDict(ForEachStatementSyntax forEach,
        string colExpr, string colType, string varName)
    {
        var types = TypeRegistry.GetDictionaryTypes(colType)!.Value;
        var cKey = types.key == "string" ? "const char*" : TypeRegistry.MapType(types.key);
        var cVal = types.val == "string" ? "const char*" : TypeRegistry.MapType(types.val);
        var idxVar = "_i_" + varName;

        _ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < (int)({colExpr}->count); {idxVar}++)");
        _ctx.WriteLine("{");
        _ctx.Indent();

        // FIX: Wir registrieren BEIDE Varianten-Namen damit dot-access in jedem Fall klappt:
        //   foreach(var kv in dict) → kv.Key, kv.Value (MemberAccess)
        //   foreach(var (k, v) in dict) → k, v (Deconstruction — separat behandelt)
        //
        // Der MemberAccess-Handler in ExpressionWriter prüft auf __kvp__-Marker
        // und gibt dann varName_Key / varName_Value zurück.
        // Zusätzlich deklarieren wir die C-Variablen direkt damit der generierte Code
        // sie auch ohne MemberAccess-Rewrite nutzen kann.

        _ctx.WriteLine($"{cKey} {varName}_Key = {colExpr}->keys[{idxVar}];");
        _ctx.WriteLine($"{cVal} {varName}_Value = {colExpr}->vals[{idxVar}];");

        // Registrierung: sowohl "kv" als __kvp__ als auch "kv_Key"/"kv_Value" direkt
        _ctx.LocalTypes[$"{varName}_Key"] = types.key;
        _ctx.LocalTypes[$"{varName}_Value"] = types.val;

        // FIX: varName selbst als __kvp__varName registrieren damit WriteMemberAccess
        //      "kv.Key" → "kv_Key" und "kv.Value" → "kv_Value" umschreibt.
        _ctx.LocalTypes[varName] = $"__kvp__{varName}";

        var bodyStmts = forEach.Statement is BlockSyntax b
            ? b.Statements.Cast<StatementSyntax>()
            : new[] { forEach.Statement };
        foreach (var s in bodyStmts)
            Write(s);

        _ctx.Dedent();
        _ctx.WriteLine("}");
    }

    /// <summary>
    /// foreach(var (k, v) in dict) — Roslyn emits ForEachVariableStatementSyntax for deconstruction.
    /// Supports Dictionary<K,V> and List<(T1,T2)> (tuple list).
    /// </summary>
    private void WriteForEachDeconstruction(ForEachVariableStatementSyntax forEach)
    {
        var colExpr  = _expr.Write(forEach.Expression);
        var colRaw   = forEach.Expression.ToString().Trim();
        var colType  = _ctx.LocalTypes.TryGetValue(colRaw, out var t) ? t : "";

        // Extract deconstruction names
        var names = new List<string>();
        if (forEach.Variable is DeclarationExpressionSyntax decl
            && decl.Designation is ParenthesizedVariableDesignationSyntax paren)
        {
            foreach (var desig in paren.Variables)
                names.Add(desig is SingleVariableDesignationSyntax svd ? svd.Identifier.Text : "_");
        }

        var idxVar = "_i_decon_" + colRaw.Replace(".", "_");

        if (TypeRegistry.IsDictionary(colType) && names.Count >= 2)
        {
            var types = TypeRegistry.GetDictionaryTypes(colType)!.Value;
            var cKey = types.key == "string" ? "const char*" : TypeRegistry.MapType(types.key);
            var cVal = types.val == "string" ? "const char*" : TypeRegistry.MapType(types.val);

            _ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < (int)({colExpr}->count); {idxVar}++)");
            _ctx.WriteLine("{");
            _ctx.Indent();
            _ctx.WriteLine($"{cKey} {names[0]} = {colExpr}->keys[{idxVar}];");
            _ctx.WriteLine($"{cVal} {names[1]} = {colExpr}->vals[{idxVar}];");
            _ctx.LocalTypes[names[0]] = types.key;
            _ctx.LocalTypes[names[1]] = types.val;
        }
        else if (TypeRegistry.IsList(colType))
        {
            // List<(T1, T2)> — access via tuple struct pointer into data[]
            var innerType = TypeRegistry.GetListInnerType(colType) ?? "int";
            string tupleStructName;
            if (TypeRegistry.IsTuple(innerType))
                tupleStructName = TypeRegistry.GetTupleStructName(innerType);
            else
                tupleStructName = innerType == "string" ? "const char*" : TypeRegistry.MapType(innerType);

            _ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < (int)({colExpr}->count); {idxVar}++)");
            _ctx.WriteLine("{");
            _ctx.Indent();
            var elemVar = "_elem_" + idxVar;
            // Take address of the element in place — no void*, proper struct pointer
            _ctx.WriteLine($"{tupleStructName}* {elemVar} = &{colExpr}->data[{idxVar}];");
            for (int i = 0; i < names.Count; i++)
            {
                if (names[i] == "_") continue;
                _ctx.WriteLine($"__auto_type {names[i]} = {elemVar}->item{i + 1};");
            }
        }
        else
        {
            _ctx.Warn(forEach, $"foreach deconstruction on unsupported collection type '{colType}' — emitting index loop");
            _ctx.WriteLine($"for (int {idxVar} = 0; {idxVar} < (int)({colExpr}->count); {idxVar}++)");
            _ctx.WriteLine("{");
            _ctx.Indent();
        }

        var bodyStmts = forEach.Statement is BlockSyntax b
            ? b.Statements.Cast<StatementSyntax>()
            : new[] { forEach.Statement };
        foreach (var s in bodyStmts)
            Write(s);

        _ctx.Dedent();
        _ctx.WriteLine("}");
    }

    private string ResolveArrayLength(string colRaw, string colKey, string colExpr)
    {
        // C char buffers (string(char, n) locals) are real C arrays — sizeof works there.
        if (_ctx.LocalTypes.TryGetValue(colRaw, out var varType) && varType == "char[]")
            return "(sizeof(" + colExpr + ") / sizeof(" + colExpr + "[0]))";
        // Everything else is a C# array with a length header (or a params companion).
        return ArrayModel.LengthOf(colRaw, colExpr, _ctx);
    }

    private void WriteForEachLoopVar(string varName, string idxVar,
        string rawElemType, string colExpr, string colType,
        bool isList, bool isString, bool isRawArray)
    {
        if (isString)
        {
            _ctx.WriteLine("char " + varName + " = " + colExpr + "[" + idxVar + "];");
            return;
        }
        if (isRawArray)
        {
            var cType = TypeRegistry.MapType(rawElemType);
            if (rawElemType == "string")
                _ctx.WriteLine("const char* " + varName + " = " + colExpr + "[" + idxVar + "];");
            else
                _ctx.WriteLine(cType + " " + varName + " = " + colExpr + "[" + idxVar + "];");
            return;
        }
        if (!isList)
        {
            var cType = TypeRegistry.MapType(rawElemType);
            var ptr = TypeRegistry.IsPrimitive(rawElemType) ? "" : "*";
            _ctx.WriteLine(cType + ptr + " " + varName + " = "
                         + colExpr + "[" + idxVar + "];");
            return;
        }
        var inner = TypeRegistry.GetListInnerType(colType) ?? rawElemType;
        if (inner == "string")
        {
            _ctx.WriteLine("const char* " + varName
                         + " = List_str_Get(" + colExpr + ", " + idxVar + ");");
            return;
        }
        var cInner = TypeRegistry.MapType(inner);
        // Delegate elements are CS2SX_Delegate* values themselves (Func_int_t).
        var isPrim = TypeRegistry.IsPrimitive(inner) || TypeRegistry.IsDelegate(inner);
        var elemPtr = isPrim ? "" : "*";
        var listFunc = "List_" + cInner + "_Get";
        _ctx.WriteLine(cInner + elemPtr + " " + varName
                     + " = " + listFunc + "(" + colExpr + ", " + idxVar + ");");
    }

    private void WriteWhile(WhileStatementSyntax whileStmt)
    {
        _ctx.WriteLine("while (" + _expr.Write(whileStmt.Condition) + ")");
        WriteBlockOrStmt(whileStmt.Statement);
    }

    private void WriteDo(DoStatementSyntax doStmt)
    {
        _ctx.WriteLine("do");
        WriteBlockOrStmt(doStmt.Statement);
        _ctx.WriteLine("while (" + _expr.Write(doStmt.Condition) + ");");
    }

    private void WriteSwitch(SwitchStatementSyntax sw)
    {
        // Pattern and string switches are lowered to if/else chains. They sit inside
        // `switch (0) { default: … }` so a `break` nested in a case still leaves the
        // switch (not an enclosing loop), while `continue` keeps targeting the loop.
        if (sw.Sections.Any(s => s.Labels.OfType<CasePatternSwitchLabelSyntax>().Any()))
        {
            InBreakableBlock(() => WritePatternSwitch(sw));
            return;
        }

        // C `switch` only accepts integer subjects. A string switch must be
        // lowered to an if/else chain over CS2SX_strcmp_safe(...) == 0.
        if (TypeInferrer.InferCSharpType(sw.Expression, _ctx) == "string")
        {
            InBreakableBlock(() => WriteStringSwitch(sw));
            return;
        }

        _ctx.WriteLine("switch (" + _expr.Write(sw.Expression) + ")");
        _ctx.WriteLine("{");
        _ctx.Indent();
        foreach (var section in sw.Sections)
        {
            foreach (var label in section.Labels)
            {
                if (label is CaseSwitchLabelSyntax caseLabel)
                    _ctx.WriteLine("case " + _expr.Write(caseLabel.Value) + ":");
                else if (label is DefaultSwitchLabelSyntax)
                    _ctx.WriteLine("default:");
            }
            _ctx.Indent();
            foreach (var s in section.Statements) Write(s);
            _ctx.Dedent();
        }
        _ctx.Dedent();
        _ctx.WriteLine("}");
    }

    // Lowers `switch (str) { case "a": ...; default: ... }` to an if/else chain
    // using CS2SX_strcmp_safe, since C switch cannot branch on strings.
    private void WriteStringSwitch(SwitchStatementSyntax sw)
    {
        var subject = _expr.Write(sw.Expression);
        // Evaluate the subject once into a temp to avoid re-running side effects.
        var tmp = "_swstr" + _ctx.NextTmp();
        _ctx.WriteLine("const char* " + tmp + " = " + subject + ";");

        SwitchSectionSyntax? defaultSection = null;
        bool first = true;
        foreach (var section in sw.Sections)
        {
            var caseValues = section.Labels.OfType<CaseSwitchLabelSyntax>()
                .Select(l => _expr.Write(l.Value)).ToList();
            if (section.Labels.OfType<DefaultSwitchLabelSyntax>().Any())
            {
                defaultSection = section;
                if (caseValues.Count == 0) continue;
            }

            var cond = string.Join(" || ",
                caseValues.Select(v => "CS2SX_strcmp_safe(" + tmp + ", " + v + ") == 0"));
            _ctx.WriteLine((first ? "if (" : "else if (") + cond + ")");
            WriteStringSwitchBody(section);
            first = false;
        }

        if (defaultSection != null)
        {
            _ctx.WriteLine(first ? "" : "else");
            WriteStringSwitchBody(defaultSection);
        }
    }

    private void WriteStringSwitchBody(SwitchSectionSyntax section)
    {
        _ctx.WriteLine("{");
        _ctx.Indent();
        foreach (var s in section.Statements)
        {
            // Drop the trailing `break;` — in an if/else it would wrongly break an
            // enclosing loop. return/continue/throw are preserved.
            if (s is BreakStatementSyntax) continue;
            Write(s);
        }
        _ctx.Dedent();
        _ctx.WriteLine("}");
    }

    private void WritePatternSwitch(SwitchStatementSyntax sw)
    {
        var subject = _expr.Write(sw.Expression);
        bool first = true;
        foreach (var section in sw.Sections)
        {
            foreach (var label in section.Labels)
            {
                if (label is DefaultSwitchLabelSyntax)
                {
                    _ctx.WriteLine("else");
                    _ctx.WriteLine("{");
                    _ctx.Indent();
                    foreach (var s in section.Statements.Where(s => s is not BreakStatementSyntax))
                        Write(s);
                    _ctx.Dedent();
                    _ctx.WriteLine("}");
                    continue;
                }
                if (label is CaseSwitchLabelSyntax caseLabel)
                {
                    var kw = first ? "if" : "else if";
                    _ctx.WriteLine(kw + " (" + subject + " == " + _expr.Write(caseLabel.Value) + ")");
                    _ctx.WriteLine("{");
                    _ctx.Indent();
                    foreach (var s in section.Statements.Where(s => s is not BreakStatementSyntax))
                        Write(s);
                    _ctx.Dedent();
                    _ctx.WriteLine("}");
                    first = false;
                    continue;
                }
                if (label is CasePatternSwitchLabelSyntax patternLabel)
                {
                    var cond = PatternMatchingWriter.WritePattern(
                        patternLabel.Pattern, subject, _ctx, _expr.Write);
                    if (patternLabel.WhenClause != null)
                        cond = "(" + cond + " && " + _expr.Write(patternLabel.WhenClause.Condition) + ")";
                    var kw = first ? "if" : "else if";
                    _ctx.WriteLine(kw + " (" + cond + ")");
                    _ctx.WriteLine("{");
                    _ctx.Indent();
                    if (patternLabel.Pattern is DeclarationPatternSyntax dp
                        && dp.Designation is SingleVariableDesignationSyntax desig)
                    {
                        var typeName = dp.Type.ToString().Trim();
                        var cType = TypeRegistry.MapType(typeName);
                        _ctx.WriteLine(cType + "* " + desig.Identifier.Text
                                     + " = (" + cType + "*)" + subject + ";");
                        _ctx.LocalTypes[desig.Identifier.Text] = typeName;
                    }
                    foreach (var s in section.Statements.Where(s => s is not BreakStatementSyntax))
                        Write(s);
                    _ctx.Dedent();
                    _ctx.WriteLine("}");
                    first = false;
                }
            }
        }
    }

    private void WriteLock(LockStatementSyntax lockStmt)
    {
        // Switch is single-threaded from C# perspective; lock is a no-op
        _ctx.Warn(lockStmt, "lock statement: Switch homebrew is single-threaded — lock ignored");
        _ctx.WriteLine("/* lock(" + _expr.Write(lockStmt.Expression) + ") — no-op on Switch */");
        WriteBlockOrStmt(lockStmt.Statement);
    }

    // ── try / catch / finally / throw ─────────────────────────────────────────
    //
    //   try { A; }                         {
    //   catch (ParseError e) { B; }            Exception* _exf_0_pending = NULL;   (only with finally)
    //   finally { C; }                         CS2SX_ExFrame _exf_0; CS2SX_ExPush(&_exf_0);
    //                                          if (setjmp(_exf_0.buf) == 0) { A; CS2SX_ExPop(&_exf_0); }
    //                                          else {
    //                                              Exception* _exf_0_ex = _exf_0.ex;
    //                                              [catch bodies run under a second frame when
    //                                               there is a finally, so it runs for throws too]
    //                                              if (CS2SX_Exc_Is(_exf_0_ex, &CS2SX_TI_ParseError)) { … B; }
    //                                              else <rethrow, or pend it until after finally>
    //                                          }
    //                                          { C; }
    //                                          if (_exf_0_pending) CS2SX_Throw(_exf_0_pending);
    //                                      }
    // return/break/continue out of a region pop its frame and run its finally first
    // (EmitUnwind). The runtime side is cs2sx_exc.h.

    private void WriteTryCatch(TryStatementSyntax tryStmt)
    {
        var f = _ctx.NextTmp("exf");
        var pending = f + "_pending";
        var ex = f + "_ex";
        bool hasFinally = tryStmt.Finally != null;
        bool hasCatch = tryStmt.Catches.Count > 0;

        _ctx.WriteLine("{");
        _ctx.Indent();
        if (hasFinally) _ctx.WriteLine("Exception* " + pending + " = NULL;");
        _ctx.WriteLine("CS2SX_ExFrame " + f + ";");
        _ctx.WriteLine("CS2SX_ExPush(&" + f + ");");
        _ctx.WriteLine("if (setjmp(" + f + ".buf) == 0)");
        _ctx.WriteLine("{");
        _ctx.Indent();
        InTryRegion(f, tryStmt.Finally, () =>
        {
            foreach (var stmt in tryStmt.Block.Statements)
                Write(stmt);
        });
        _ctx.WriteLine("CS2SX_ExPop(&" + f + ");");
        _ctx.Dedent();
        _ctx.WriteLine("}");
        _ctx.WriteLine("else");
        _ctx.WriteLine("{");
        _ctx.Indent();
        _ctx.WriteLine("Exception* " + ex + " = " + f + ".ex;");
        if (!hasCatch)
        {
            _ctx.WriteLine(pending + " = " + ex + ";");
        }
        else if (hasFinally)
        {
            var fc = f + "_c";
            _ctx.WriteLine("CS2SX_ExFrame " + fc + ";");
            _ctx.WriteLine("CS2SX_ExPush(&" + fc + ");");
            _ctx.WriteLine("if (setjmp(" + fc + ".buf) == 0)");
            _ctx.WriteLine("{");
            _ctx.Indent();
            InTryRegion(fc, tryStmt.Finally, () => WriteCatchChain(tryStmt, ex, pending + " = " + ex + ";"));
            _ctx.WriteLine("CS2SX_ExPop(&" + fc + ");");
            _ctx.Dedent();
            _ctx.WriteLine("}");
            _ctx.WriteLine("else " + pending + " = " + fc + ".ex;");
        }
        else
        {
            WriteCatchChain(tryStmt, ex, "CS2SX_Throw(" + ex + ");");
        }
        _ctx.Dedent();
        _ctx.WriteLine("}");

        if (hasFinally)
        {
            WriteFinallyBlock(tryStmt.Finally!);
            _ctx.WriteLine("if (" + pending + ") CS2SX_Throw(" + pending + ");");
        }
        _ctx.Dedent();
        _ctx.WriteLine("}");
    }

    /// if / else-if chain over the catch clauses, matched by runtime type info.
    private void WriteCatchChain(TryStatementSyntax tryStmt, string ex, string unhandled)
    {
        bool first = true;
        bool catchesAll = false;
        foreach (var clause in tryStmt.Catches)
        {
            var typeName = CatchTypeName(clause);
            var varName = clause.Declaration?.Identifier.Text;
            if (string.IsNullOrEmpty(varName) || varName == "_") varName = null;
            var cType = TypeRegistry.MapType(typeName);
            var decl = varName != null ? cType + "* " + varName + " = (" + cType + "*)" + ex + ";" : null;
            if (varName != null) _ctx.LocalTypes[varName] = typeName;

            var cond = typeName == "Exception" ? "1"
                : "CS2SX_Exc_Is(" + ex + ", &" + TypeRegistry.ExceptionTypeInfo(typeName) + ")";
            if (clause.Filter != null)
            {
                var filter = _expr.Write(clause.Filter.FilterExpression);
                cond = (cond == "1" ? "" : cond + " && ")
                     + "({ " + (decl != null ? decl + " (void)" + varName + "; " : "") + "(" + filter + "); })";
            }

            _ctx.WriteLine((first ? "if (" : "else if (") + cond + ")");
            _ctx.WriteLine("{");
            _ctx.Indent();
            if (decl != null) _ctx.WriteLine(decl);
            _ctx.CatchExceptionVars.Push(ex);
            try
            {
                foreach (var stmt in clause.Block.Statements)
                    Write(stmt);
            }
            finally { _ctx.CatchExceptionVars.Pop(); }
            // The exception dies with the handler unless the variable escapes.
            if (!ExceptionVariableEscapes(clause, varName)
                && clause.Block.Statements.LastOrDefault() is not (ReturnStatementSyntax or ThrowStatementSyntax))
                _ctx.WriteLine("Exception_Free(" + ex + ");");
            _ctx.Dedent();
            _ctx.WriteLine("}");
            first = false;

            if (cond == "1") { catchesAll = true; break; }   // later clauses are unreachable
        }
        if (!catchesAll)
            _ctx.WriteLine("else " + unhandled);
    }

    /// Exception type a catch clause matches ("Exception" for a bare `catch`).
    private string CatchTypeName(CatchClauseSyntax clause)
    {
        if (clause.Declaration == null) return "Exception";
        if (_ctx.SemanticModel != null)
        {
            try
            {
                var t = _ctx.SemanticModel.GetTypeInfo(clause.Declaration.Type).Type;
                if (t != null && t is not IErrorTypeSymbol) return t.Name;
            }
            catch { }
        }
        var raw = clause.Declaration.Type.ToString().Trim();
        var dot = raw.LastIndexOf('.');
        return dot >= 0 ? raw[(dot + 1)..] : raw;
    }

    /// True when the handler may still reference the exception after its body: any use
    /// of the catch variable (even `e.Message` — the string belongs to the exception and
    /// may have been stored). Only handlers that never look at it free the exception.
    private static bool ExceptionVariableEscapes(CatchClauseSyntax clause, string? varName)
    {
        if (varName == null) return false;
        return clause.DescendantNodes().OfType<IdentifierNameSyntax>()
            .Any(id => id.Identifier.Text == varName);
    }

    private void WriteFinallyBlock(FinallyClauseSyntax fin)
    {
        _ctx.WriteLine("{");
        _ctx.Indent();
        foreach (var stmt in fin.Block.Statements)
            Write(stmt);
        _ctx.Dedent();
        _ctx.WriteLine("}");
    }

    private void InTryRegion(string frame, FinallyClauseSyntax? fin, Action body)
    {
        _ctx.TryRegions.Add(new TranspilerContext.TryRegion
        {
            Frame = frame,
            Finally = fin,
            BreakDepth = _ctx.BreakDepth,
            ContinueDepth = _ctx.ContinueDepth,
        });
        try { body(); }
        finally { _ctx.TryRegions.RemoveAt(_ctx.TryRegions.Count - 1); }
    }

    /// Cleanup for leaving the try regions from index <paramref name="from"/> inward:
    /// innermost first, pop its frame and run its finally block. A finally block is
    /// written outside its own region (a return inside it only unwinds outer ones).
    private void EmitUnwind(int from)
    {
        var saved = _ctx.TryRegions.ToList();
        try
        {
            for (int i = saved.Count - 1; i >= from; i--)
            {
                var r = saved[i];
                if (r.Frame != null) _ctx.WriteLine("CS2SX_ExPop(&" + r.Frame + ");");
                if (r.Finally != null)
                {
                    _ctx.TryRegions.RemoveRange(i, _ctx.TryRegions.Count - i);
                    WriteFinallyBlock(r.Finally);
                }
            }
        }
        finally
        {
            _ctx.TryRegions.Clear();
            _ctx.TryRegions.AddRange(saved);
        }
    }

    /// break/continue: leaving the try regions inside the targeted loop/switch first.
    private void WriteJump(string jump, Func<TranspilerContext.TryRegion, bool> insideTarget)
    {
        int from = _ctx.TryRegions.FindIndex(r => insideTarget(r));
        if (from < 0)
        {
            _ctx.WriteLine(jump);
            return;
        }
        _ctx.WriteLine("{");
        _ctx.Indent();
        EmitUnwind(from);
        _ctx.WriteLine(jump);
        _ctx.Dedent();
        _ctx.WriteLine("}");
    }

    private void InLoop(Action write)
    {
        _ctx.BreakDepth++;
        _ctx.ContinueDepth++;
        try { write(); }
        finally { _ctx.BreakDepth--; _ctx.ContinueDepth--; }
    }

    private void InBreakableBlock(Action write)
    {
        _ctx.WriteLine("switch (0) { default:");
        _ctx.Indent();
        write();
        _ctx.Dedent();
        _ctx.WriteLine("}");
    }

    private void WriteThrow(ThrowStatementSyntax throwStmt)
    {
        var line = throwStmt.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
        string ex;
        if (throwStmt.Expression == null)
        {
            // `throw;` rethrows the exception of the enclosing catch block.
            if (_ctx.CatchExceptionVars.Count == 0)
                _ctx.Warn(throwStmt, "`throw;` outside a catch block");
            ex = _ctx.CatchExceptionVars.Count > 0 ? _ctx.CatchExceptionVars.Peek() : "NULL";
        }
        else
        {
            ex = "(Exception*)(" + _expr.Write(throwStmt.Expression) + ")";
        }
        _ctx.WriteLineWithMapping("CS2SX_Throw(" + ex + ");", line, throwStmt.ToString().Trim());
    }

    private void WriteUsing(UsingStatementSyntax usingStmt)
    {
        _ctx.WriteLine("{");
        _ctx.Indent();

        var disposeActions = new List<string>();

        if (usingStmt.Declaration != null)
        {
            foreach (var varDecl in usingStmt.Declaration.Variables)
            {
                var typeName = usingStmt.Declaration.Type.ToString().Trim();
                var varName = varDecl.Identifier.Text;
                var isValueType = TypeRegistry.IsLibNxStruct(typeName)
                               || TypeRegistry.IsPrimitive(typeName);
                var cType = TypeRegistry.MapType(typeName);
                var ptr = isValueType ? "" : "*";
                var initStr = varDecl.Initializer != null
                    ? _expr.Write(varDecl.Initializer.Value)
                    : (isValueType ? "{0}" : "NULL");
                _ctx.WriteLine(cType + ptr + " " + varName + " = " + initStr + ";");
                _ctx.LocalTypes[varName] = typeName;

                // Build dispose action
                if (TypeRegistry.IsDisposable(typeName))
                {
                    var disposeCall = isValueType
                        ? typeName + "_Dispose(&" + varName + ")"
                        : typeName + "_Dispose(" + varName + ")";
                    disposeActions.Add("if (" + varName + ") " + disposeCall + ";");
                }
                else if (TypeRegistry.IsStringBuilder(typeName))
                {
                    disposeActions.Add("if (" + varName + ") StringBuilder_Free(" + varName + ");");
                }
                else if (TypeRegistry.IsList(typeName))
                {
                    var inner = TypeRegistry.GetListInnerType(typeName) ?? "int";
                    var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
                    disposeActions.Add("if (" + varName + ") List_" + cInner + "_Free(" + varName + ");");
                }
                else if (TypeRegistry.IsDictionary(typeName))
                {
                    var types = TypeRegistry.GetDictionaryTypes(typeName)!.Value;
                    var ck = types.key == "string" ? "str" : TypeRegistry.MapType(types.key);
                    var cv = types.val == "string" ? "str" : TypeRegistry.MapType(types.val);
                    disposeActions.Add("if (" + varName + ") Dict_" + ck + "_" + cv + "_Free(" + varName + ");");
                }
                else if (TypeRegistry.IsStack(typeName))
                {
                    var inner = TypeRegistry.GetStackInnerType(typeName) ?? "int";
                    var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
                    disposeActions.Add("if (" + varName + ") Stack_" + cInner + "_Free(" + varName + ");");
                }
                else if (TypeRegistry.IsQueue(typeName))
                {
                    var inner = TypeRegistry.GetQueueInnerType(typeName) ?? "int";
                    var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
                    disposeActions.Add("if (" + varName + ") Queue_" + cInner + "_Free(" + varName + ");");
                }
                else if (TypeRegistry.IsHashSet(typeName))
                {
                    var inner = TypeRegistry.GetHashSetInnerType(typeName) ?? "int";
                    var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
                    disposeActions.Add("if (" + varName + ") HashSet_" + cInner + "_Free(" + varName + ");");
                }
                else if (typeName == "Stopwatch")
                {
                    disposeActions.Add("if (" + varName + ") CS2SX_Stopwatch_Free(" + varName + ");");
                }
                else if (!isValueType && TypeRegistry.NeedsPointerSuffix(typeName))
                {
                    // Generic Dispose convention: TypeName_Free(ptr)
                    disposeActions.Add("if (" + varName + ") " + cType + "_Free(" + varName + ");");
                }
            }

            if (usingStmt.Statement != null)
                Write(usingStmt.Statement);
        }
        else if (usingStmt.Expression != null)
        {
            var exprCode = _expr.Write(usingStmt.Expression);
            _ctx.WriteLine("/* using(" + exprCode + ") */");
            if (usingStmt.Statement != null)
                Write(usingStmt.Statement);
        }

        // Emit dispose calls at end of scope (before closing brace)
        foreach (var action in disposeActions)
            _ctx.WriteLine(action);

        _ctx.Dedent();
        _ctx.WriteLine("}");
    }

    public void WriteBlockOrStmt(StatementSyntax stmt)
    {
        _ctx.WriteLine("{");
        _ctx.Indent();
        if (stmt is BlockSyntax block)
            foreach (var s in block.Statements) Write(s);
        else
        {
            // Unbraced body (`if (x) Call();`): inside these braces the line is safe.
            if (stmt is not (EmptyStatementSyntax or LocalFunctionStatementSyntax))
                _ctx.WriteLine($"CS2SX_LINE({stmt.GetLocation().GetLineSpan().StartLinePosition.Line + 1});");
            Write(stmt);
        }
        _ctx.Dedent();
        _ctx.WriteLine("}");
    }

    public void WriteBlock(StatementSyntax stmt) => WriteBlockOrStmt(stmt);

    private void WriteBlock(BlockSyntax block)
    {
        _ctx.WriteLine("{");
        _ctx.Indent();
        foreach (var s in block.Statements) Write(s);
        _ctx.Dedent();
        _ctx.WriteLine("}");
    }
}