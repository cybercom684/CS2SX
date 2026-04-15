using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Writers;

public sealed class StatementWriter
{
    private readonly TranspilerContext _ctx;
    private readonly ExpressionWriter _expr;

    public StatementWriter(TranspilerContext ctx, ExpressionWriter expr)
    {
        _ctx = ctx;
        _expr = expr;
    }

    public void Write(StatementSyntax stmt)
    {
        switch (stmt)
        {
            case ReturnStatementSyntax ret: WriteReturn(ret); break;
            case LocalDeclarationStatementSyntax l: WriteLocal(l); break;
            case ExpressionStatementSyntax expr: WriteExprStmt(expr); break;
            case IfStatementSyntax ifStmt: WriteIf(ifStmt); break;
            case BlockSyntax block: WriteBlock(block); break;
            case ForStatementSyntax forStmt: WriteFor(forStmt); break;
            case ForEachStatementSyntax forEach: WriteForEach(forEach); break;
            case WhileStatementSyntax whileStmt: WriteWhile(whileStmt); break;
            case DoStatementSyntax doStmt: WriteDo(doStmt); break;
            case BreakStatementSyntax: _ctx.WriteLine("break;"); break;
            case ContinueStatementSyntax: _ctx.WriteLine("continue;"); break;
            case SwitchStatementSyntax sw: WriteSwitch(sw); break;
            case TryStatementSyntax tryStmt: WriteTryCatch(tryStmt); break;
            case ThrowStatementSyntax throwStmt: WriteThrow(throwStmt); break;
            case UsingStatementSyntax usingStmt: WriteUsing(usingStmt); break;
            case EmptyStatementSyntax: break;
            default:
                _ctx.WriteLine("/* UNSUPPORTED: " + stmt.GetType().Name + " */");
                break;
        }
    }

    private void WriteReturn(ReturnStatementSyntax ret)
    {
        if (ret.Expression == null)
            _ctx.WriteLine("return;");
        else
            _ctx.WriteLine("return " + _expr.Write(ret.Expression) + ";");
    }

    private void WriteExprStmt(ExpressionStatementSyntax expr)
    {
        var result = _expr.Write(expr.Expression);
        if (!string.IsNullOrEmpty(result))
            _ctx.WriteLine(result + ";");
    }

    private void WriteLocal(LocalDeclarationStatementSyntax local)
    {
        var declType = local.Declaration.Type.ToString().Trim();

        // FIX 4: Mehrdimensionale Arrays (int[,]) → flaches Array
        if (declType.Contains(",") && declType.Contains("[") && declType.Contains("]"))
        {
            WriteMultiDimArray(local, declType);
            return;
        }

        // FIX 5+11: T[] mit Initializer → Stack-Array
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
            // LibNx Stack-Structs
            if (TypeRegistry.IsLibNxStruct(declType))
            {
                var si = v.Initializer != null
                    ? " = " + _expr.Write(v.Initializer.Value)
                    : " = {0}";
                _ctx.WriteLine(TypeRegistry.MapType(declType) + " " + v.Identifier + si + ";");
                _ctx.LocalTypes[v.Identifier.Text] = declType;
                continue;
            }

            if (NullableHandler.IsNullable(declType))
            {
                WriteNullableLocal(v, declType);
                continue;
            }

            // string mit new string(char, count) → char buf[N]
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

            // FIX 12: bool → int (0/1)
            if (declType == "bool")
            {
                var initVal = v.Initializer != null
                    ? _expr.Write(v.Initializer.Value)
                    : "0";
                _ctx.WriteLine("int " + v.Identifier + " = " + initVal + ";");
                _ctx.LocalTypes[v.Identifier.Text] = "bool";
                continue;
            }

            // ── Interface-Zuweisung ──────────────────────────────────────────
            // IRenderable r = button;  → IRenderable r = Button_as_IRenderable(button);
            //
            // Erkennung: deklarierter Typ ist ein bekanntes Interface aus dem Collector.
            // Der Initializer-Ausdruck hat einen anderen Typ → Wrapper-Funktion einsetzen.
            if (_ctx.InterfaceTypes.Contains(declType) && v.Initializer != null)
            {
                var initExprRaw = v.Initializer.Value.ToString().Trim();
                var initCode = _expr.Write(v.Initializer.Value);
                var wrapped = TryWrapAsInterface(initExprRaw, initCode, declType);

                if (wrapped != null)
                {
                    // IRenderable r = Button_as_IRenderable(button);
                    _ctx.WriteLine(declType + " " + v.Identifier + " = " + wrapped + ";");
                }
                else
                {
                    // Kein Wrapping möglich (Typ unbekannt oder bereits Interface)
                    _ctx.WriteLine(declType + " " + v.Identifier + " = " + initCode + ";");
                }
                _ctx.LocalTypes[v.Identifier.Text] = declType;
                continue;
            }
            // Wenn kein Initializer aber Interface-Typ: als lokale Variable
            if (_ctx.InterfaceTypes.Contains(declType))
            {
                _ctx.WriteLine(declType + " " + v.Identifier + ";");
                _ctx.LocalTypes[v.Identifier.Text] = declType;
                continue;
            }
            // ────────────────────────────────────────────────────────────────

            // FIX 1: T[] ohne Initializer aber MIT new T[N] → Stack oder Heap
            if (declType.EndsWith("[]")
                && v.Initializer?.Value is ArrayCreationExpressionSyntax arrCreate)
            {
                WriteArrayAlloc(v, declType, arrCreate);
                continue;
            }

            var (cTypeFinal, isPtr) = InferLocalType(declType, v);
            if (string.IsNullOrWhiteSpace(cTypeFinal)) cTypeFinal = "int";

            var ptr = isPtr ? "*" : "";
            var init = BuildLocalInit(v, isPtr, declType, cTypeFinal);

            _ctx.WriteLine(cTypeFinal + ptr + " " + v.Identifier + init + ";");

            var registeredType = cTypeFinal == "List_str"
                ? "List<string>"
                : (declType is "var" or "var?" ? cTypeFinal : declType);
            _ctx.LocalTypes[v.Identifier.Text] = registeredType;
        }
    }

    /// <summary>
    /// Prüft ob ein Initializer-Ausdruck in einen Interface-Wrapper eingepackt werden muss.
    ///
    /// IRenderable r = button;
    ///   targetIfaceName = "IRenderable"
    ///   exprRaw         = "button"    (Roslyn-Text)
    ///   exprCode        = "button"    (transpilierter C-Code)
    ///   → "Button_as_IRenderable(button)"
    ///
    /// Gibt null zurück wenn kein Wrapping nötig oder möglich ist.
    /// </summary>
    private string? TryWrapAsInterface(
        string exprRaw,
        string exprCode,
        string targetIfaceName)
    {
        if (!_ctx.InterfaceTypes.Contains(targetIfaceName)) return null;

        // Typ des Ausdrucks aus LocalTypes oder FieldTypes
        var key = exprRaw.TrimStart('_');
        string? csType = null;
        _ctx.LocalTypes.TryGetValue(exprRaw, out csType);
        if (csType == null) _ctx.FieldTypes.TryGetValue(key, out csType);

        // Typ unbekannt → kein Wrap (lassen wir den Compiler meckern)
        if (csType == null) return null;

        // Typ IST bereits das Interface → kein Wrap nötig
        var bareType = csType.TrimEnd('*').Trim();
        if (bareType == targetIfaceName) return null;

        // Wrapper: ClassName_as_InterfaceName(expr)
        return bareType + "_as_" + targetIfaceName + "(" + exprCode + ")";
    }

    // FIX 1: T[] mit new T[N] → Array-Allokation + Länge in ArrayLengths tracken
    private void WriteArrayAlloc(VariableDeclaratorSyntax v, string declType,
        ArrayCreationExpressionSyntax arrCreate)
    {
        var baseType = declType[..^2].Trim();
        var varName = v.Identifier.Text;
        var cType = baseType == "string" ? "const char*" : TypeRegistry.MapType(baseType);

        if (arrCreate.Type.RankSpecifiers.Count > 0
            && arrCreate.Type.RankSpecifiers[0].Sizes.Count > 0
            && arrCreate.Type.RankSpecifiers[0].Sizes[0] is not OmittedArraySizeExpressionSyntax)
        {
            var sizeExpr = _expr.Write(arrCreate.Type.RankSpecifiers[0].Sizes[0]);

            if (IsConstantSizeExpr(arrCreate.Type.RankSpecifiers[0].Sizes[0]))
            {
                _ctx.WriteLine(cType + " " + varName + "[" + sizeExpr + "];");
                _ctx.WriteLine("memset(" + varName + ", 0, sizeof(" + varName + "));");
            }
            else
            {
                _ctx.WriteLine(cType + "* " + varName
                    + " = (" + cType + "*)calloc(" + sizeExpr + ", sizeof(" + cType + "));");
            }
            _ctx.ArrayLengths[varName] = sizeExpr;
        }
        else if (arrCreate.Initializer != null)
        {
            var elems = arrCreate.Initializer.Expressions
                .Select(e => _expr.Write(e)).ToList();
            _ctx.WriteLine(cType + " " + varName + "[] = { " + string.Join(", ", elems) + " };");
            _ctx.ArrayLengths[varName] = elems.Count.ToString();
        }
        else
        {
            _ctx.WriteLine(cType + "* " + varName + " = NULL; /* empty array */");
        }

        _ctx.LocalTypes[varName] = declType;
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
            if (v.Initializer?.Value is ArrayCreationExpressionSyntax arr
                && arr.Initializer != null)
            {
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
                _ctx.WriteLine(cType + " " + v.Identifier + "[] = { "
                    + string.Join(", ", flatElems) + " };");
                _ctx.ArrayLengths[v.Identifier.Text] = flatElems.Count.ToString();
            }
            else
            {
                var dims = "";
                if (arr_RankSizes(v, out var sizes) && sizes.Count > 0)
                    dims = string.Join("*", sizes);
                else
                    dims = "1";
                _ctx.WriteLine(cType + " " + v.Identifier + "[" + dims + "];");
                _ctx.WriteLine("memset(" + v.Identifier + ", 0, sizeof(" + v.Identifier + "));");
            }
            _ctx.LocalTypes[v.Identifier.Text] = baseType + "[]";
        }
    }

    private static bool arr_RankSizes(VariableDeclaratorSyntax v, out List<string> sizes)
    {
        sizes = new();
        if (v.Initializer?.Value is ArrayCreationExpressionSyntax arr)
        {
            foreach (var rs in arr.Type.RankSpecifiers)
                foreach (var sz in rs.Sizes)
                    if (sz is not OmittedArraySizeExpressionSyntax)
                        sizes.Add(sz.ToString());
        }
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
            if (baseType == "string")
                _ctx.WriteLine("const char* " + varName + "[] = { " + string.Join(", ", elems) + " };");
            else
                _ctx.WriteLine(cType + " " + varName + "[] = { " + string.Join(", ", elems) + " };");

            _ctx.ArrayLengths[varName] = elems.Count.ToString();
            _ctx.LocalTypes[varName] = declType;
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
        csType is "TouchState" or "StickPos" or "BatteryInfo";

    private void WriteNullableLocal(VariableDeclaratorSyntax v, string declType)
    {
        var inner = NullableHandler.GetInnerType(declType);
        var innerC = TypeRegistry.MapType(inner);
        var varName = v.Identifier.Text;

        if (v.Initializer == null
            || v.Initializer.Value is LiteralExpressionSyntax lit
               && lit.IsKind(SyntaxKind.NullLiteralExpression))
        {
            _ctx.WriteLine(innerC + "* " + varName + " = NULL;");
        }
        else
        {
            var initVal = _expr.Write(v.Initializer.Value);
            var tmpName = "_nval_" + varName;
            _ctx.WriteLine(innerC + " " + tmpName + " = " + initVal + ";");
            _ctx.WriteLine(innerC + "* " + varName + " = &" + tmpName + ";");
        }

        _ctx.LocalTypes[varName] = declType;
    }

    private (string cType, bool isPtr) InferLocalType(
        string declType, VariableDeclaratorSyntax v)
    {
        if (declType is "var" or "var?")
            return InferVarType(v);

        if (declType == "bool") return ("int", false);

        var cType = TypeRegistry.MapType(declType);
        var isPtr = TypeRegistry.NeedsPointerSuffix(declType)
                 || TypeRegistry.IsStringBuilder(declType)
                 || TypeRegistry.IsList(declType)
                 || TypeRegistry.IsDictionary(declType);
        return (cType, isPtr);
    }

    private (string cType, bool isPtr) InferVarType(VariableDeclaratorSyntax v)
    {
        if (v.Initializer?.Value is ObjectCreationExpressionSyntax oc)
            return (oc.Type.ToString(), true);

        if (v.Initializer?.Value is InvocationExpressionSyntax inv)
        {
            if (IsSplitCall(inv)) return ("List_str", true);

            var calleeStr = inv.Expression.ToString();

            if (calleeStr is "Directory.GetFiles" or "CS2SX_Dir_GetFiles"
                          or "String_Split" or "string.Split" or "String.Split")
                return ("List_str", true);

            if (calleeStr is "string.Format" or "String.Format"
                           or "string.Concat" or "String.Concat")
                return ("const char", true);

            if (calleeStr is "Input.GetTouch" or "CS2SX_Input_GetTouch")
                return ("CS2SX_TouchState", false);

            if (calleeStr is "Input.GetStickLeft" or "_cs2sx_get_stick_left"
                           or "CS2SX_Input_GetStickLeft")
                return ("CS2SX_StickPos", false);

            if (calleeStr is "Input.GetStickRight" or "_cs2sx_get_stick_right"
                           or "CS2SX_Input_GetStickRight")
                return ("CS2SX_StickPos", false);

            if (calleeStr is "System.GetBattery" or "CS2SX_GetBattery")
                return ("CS2SX_BatteryInfo", false);

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

            if (TypeRegistry.IsList(inferred) || TypeRegistry.IsDictionary(inferred)
                || TypeRegistry.IsStringBuilder(inferred))
                return (TypeRegistry.MapType(inferred).TrimEnd('*'), true);

            if (inferred.EndsWith("*"))
                return (inferred.TrimEnd('*'), true);
            return (inferred, false);
        }

        var ct = TypeInferrer.InferCSharpType(v.Initializer?.Value, _ctx);

        if (ct is "TouchState") return ("CS2SX_TouchState", false);
        if (ct is "StickPos") return ("CS2SX_StickPos", false);
        if (ct is "BatteryInfo") return ("CS2SX_BatteryInfo", false);

        if (TypeRegistry.IsList(ct) || TypeRegistry.IsDictionary(ct)
            || TypeRegistry.IsStringBuilder(ct))
            return (TypeRegistry.MapType(ct).TrimEnd('*'), true);

        if (ct.EndsWith("*"))
            return (ct.TrimEnd('*'), true);
        return (ct, false);
    }

    private static bool IsSplitCall(InvocationExpressionSyntax inv) => IsListStrCall(inv);

    private static bool IsListStrCall(InvocationExpressionSyntax inv)
    {
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
            return " = " + _expr.Write(v.Initializer.Value);

        if (!isPtr && TypeRegistry.IsPrimitive(
                declType is "var" or "var?" ? cType : declType))
            return " = 0";

        return "";
    }

    // ── If ────────────────────────────────────────────────────────────────────

    private void WriteIf(IfStatementSyntax ifStmt)
    {
        if (TryExtractOutVarFromCondition(ifStmt.Condition, out var outVarDecls))
        {
            foreach (var (varName, varType) in outVarDecls)
            {
                var cType = TypeRegistry.MapType(varType);
                _ctx.WriteLine(cType + " " + varName + " = 0;");
                _ctx.LocalTypes[varName] = varType;
            }
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
        {
            foreach (var (varName, varType) in outVarDecls)
            {
                var cType = TypeRegistry.MapType(varType);
                _ctx.WriteLine(cType + " " + varName + " = 0;");
                _ctx.LocalTypes[varName] = varType;
            }
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

        string lenExpr;
        if (isList)
            lenExpr = colExpr + "->count";
        else if (isString)
            lenExpr = "strlen(" + colExpr + ")";
        else if (isRawArray)
            lenExpr = ResolveArrayLength(colRaw, colKey, colExpr);
        else
            lenExpr = colExpr + "_count";

        var rawElemType = forEach.Type.ToString().Trim();
        if (rawElemType == "var")
        {
            if (isList) rawElemType = TypeRegistry.GetListInnerType(colType) ?? "int";
            else if (isString) rawElemType = "char";
            else if (isRawArray) rawElemType = colType[..^2].Trim();
        }

        _ctx.WriteLine("for (int " + idxVar + " = 0; "
                     + idxVar + " < (int)(" + lenExpr + "); "
                     + idxVar + "++)");
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

        _ctx.WriteLine("for (int " + idxVar + " = 0; "
                     + idxVar + " < (int)(" + colExpr + "->count); "
                     + idxVar + "++)");
        _ctx.WriteLine("{");
        _ctx.Indent();

        _ctx.WriteLine(cKey + " " + varName + "_Key = " + colExpr + "->keys[" + idxVar + "];");
        _ctx.WriteLine(cVal + " " + varName + "_Value = " + colExpr + "->vals[" + idxVar + "];");

        _ctx.LocalTypes[varName + "_Key"] = types.key;
        _ctx.LocalTypes[varName + "_Value"] = types.val;
        _ctx.LocalTypes[varName] = "__kvp__" + varName;

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
        if (_ctx.ArrayLengths.TryGetValue(colRaw, out var knownLen))
            return knownLen;
        if (_ctx.ArrayLengths.TryGetValue(colKey, out var knownLenField))
            return knownLenField;

        if (_ctx.LocalTypes.ContainsKey(colRaw))
            return "(sizeof(" + colExpr + ") / sizeof(" + colExpr + "[0]))";

        return colExpr + "_count";
    }

    private void WriteForEachLoopVar(
        string varName, string idxVar,
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
        var isPrim = TypeRegistry.IsPrimitive(inner);
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
        if (sw.Sections.Any(s => s.Labels.OfType<CasePatternSwitchLabelSyntax>().Any()))
        {
            WritePatternSwitch(sw);
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

    private void WriteTryCatch(TryStatementSyntax tryStmt)
    {
        var jmpBufName = "_ex_buf_" + _ctx.NextTmp();
        _ctx.CurrentJumpBuf = jmpBufName;

        _ctx.WriteLine("jmp_buf " + jmpBufName + ";");
        _ctx.WriteLine("int _ex_val = setjmp(" + jmpBufName + ");");
        _ctx.WriteLine("if (_ex_val == 0)");
        _ctx.WriteLine("{");
        _ctx.Indent();
        foreach (var stmt in tryStmt.Block.Statements)
            Write(stmt);
        _ctx.Dedent();
        _ctx.WriteLine("}");

        if (tryStmt.Catches.Count > 0)
        {
            _ctx.WriteLine("else");
            _ctx.WriteLine("{");
            _ctx.Indent();
            foreach (var stmt in tryStmt.Catches[0].Block.Statements)
                Write(stmt);
            _ctx.Dedent();
            _ctx.WriteLine("}");
        }

        _ctx.CurrentJumpBuf = null;
    }

    private void WriteThrow(ThrowStatementSyntax throwStmt)
    {
        if (_ctx.CurrentJumpBuf != null)
        {
            _ctx.WriteLine("longjmp(" + _ctx.CurrentJumpBuf + ", 1);");
            _ctx.WriteLine("return;");
        }
        else
        {
            _ctx.WriteLine("/* throw ignored (no try/catch) */");
            _ctx.WriteLine("return;");
        }
    }

    private void WriteUsing(UsingStatementSyntax usingStmt)
    {
        _ctx.WriteLine("{");
        _ctx.Indent();

        if (usingStmt.Declaration != null)
        {
            foreach (var varDecl in usingStmt.Declaration.Variables)
            {
                var typeName = usingStmt.Declaration.Type.ToString();
                var varName = varDecl.Identifier.Text;
                var isValueType = TypeRegistry.IsLibNxStruct(typeName)
                               || TypeRegistry.IsPrimitive(typeName);
                var cType = TypeRegistry.MapType(typeName);
                var ptr = isValueType ? "" : "*";
                var initStr = varDecl.Initializer != null
                    ? _expr.Write(varDecl.Initializer.Value)
                    : "";

                _ctx.WriteLine(cType + ptr + " " + varName + " = " + initStr + ";");
                Write(usingStmt.Statement);

                if (TypeRegistry.IsDisposable(typeName))
                {
                    var disposeCall = isValueType
                        ? typeName + "_Dispose(&" + varName + ")"
                        : typeName + "_Dispose(" + varName + ")";
                    _ctx.WriteLine("if (" + varName + ") " + disposeCall + ";");
                }
            }
        }
        else if (usingStmt.Expression != null)
        {
            _ctx.WriteLine("/* using expression not supported */");
            Write(usingStmt.Statement);
        }

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
            Write(stmt);

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