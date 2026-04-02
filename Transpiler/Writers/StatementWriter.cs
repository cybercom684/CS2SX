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

    // ── Dispatch ──────────────────────────────────────────────────────────

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

    // ── Return ────────────────────────────────────────────────────────────

    private void WriteReturn(ReturnStatementSyntax ret)
    {
        if (ret.Expression == null)
            _ctx.WriteLine("return;");
        else
            _ctx.WriteLine("return " + _expr.Write(ret.Expression) + ";");
    }

    // ── Expression Statement ──────────────────────────────────────────────

    private void WriteExprStmt(ExpressionStatementSyntax expr)
    {
        var result = _expr.Write(expr.Expression);
        if (!string.IsNullOrEmpty(result))
            _ctx.WriteLine(result + ";");
    }

    // ── Lokale Variablen ──────────────────────────────────────────────────

    private void WriteLocal(LocalDeclarationStatementSyntax local)
    {
        var declType = local.Declaration.Type.ToString().Trim();

        // string[] mit new[] { } Initializer
        if (declType.EndsWith("[]") && local.Declaration.Variables.Count == 1)
        {
            var v = local.Declaration.Variables[0];
            if (v.Initializer?.Value is ImplicitArrayCreationExpressionSyntax implArr)
            {
                var initExprs = implArr.Initializer.Expressions
                    .Select(e => _expr.Write(e))
                    .ToList();
                _ctx.WriteLine("const char* " + v.Identifier + "[] = { "
                    + string.Join(", ", initExprs) + " };");
                _ctx.LocalTypes[v.Identifier.Text] = declType;
                return;
            }
        }

        foreach (var v in local.Declaration.Variables)
        {
            // libnx-Structs auf dem Stack
            if (TypeRegistry.IsLibNxStruct(declType))
            {
                var si = v.Initializer != null
                    ? " = " + _expr.Write(v.Initializer.Value)
                    : " = {0}";
                _ctx.WriteLine(TypeRegistry.MapType(declType) + " " + v.Identifier + si + ";");
                _ctx.LocalTypes[v.Identifier.Text] = declType;
                continue;
            }

            // Nullable-Typ: T? x = null → T* x = NULL; T? x = val → static T _tmp; T* x = &_tmp;
            if (NullableHandler.IsNullable(declType))
            {
                WriteNullableLocal(v, declType);
                continue;
            }

            // char-Puffer: string x = new string('\0', 256)
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

            // Split / GetFiles Sonderfall
            if (v.Initializer?.Value is InvocationExpressionSyntax splitInv
                && IsListStrCall(splitInv))
            {
                var initVal = _expr.Write(v.Initializer.Value);
                _ctx.WriteLine("List_str* " + v.Identifier + " = " + initVal + ";");
                _ctx.LocalTypes[v.Identifier.Text] = "List<string>";
                continue;
            }

            var (cType, isPtr) = InferLocalType(declType, v);
            if (string.IsNullOrWhiteSpace(cType)) cType = "int";

            var ptr = isPtr ? "*" : "";
            var init = BuildLocalInit(v, isPtr, declType, cType);

            _ctx.WriteLine(cType + ptr + " " + v.Identifier + init + ";");

            var registeredType = cType == "List_str"
                ? "List<string>"
                : (declType is "var" or "var?" ? cType : declType);
            _ctx.LocalTypes[v.Identifier.Text] = registeredType;
        }
    }

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
            if (IsSplitCall(inv))
                return ("List_str", true);

            var calleeStr = inv.Expression.ToString();

            if (calleeStr is "Directory.GetFiles"
                          or "CS2SX_Dir_GetFiles"
                          or "String_Split"
                          or "string.Split"
                          or "String.Split")
                return ("List_str", true);

            if (calleeStr is "string.Format" or "String.Format"
                           or "string.Concat" or "String.Concat")
                return ("const char", true);

            if (_ctx.MethodReturnTypes.TryGetValue(calleeStr, out var retType))
            {
                var cMapped = TypeRegistry.MapType(retType);
                var isPtr = !cMapped.EndsWith("*")
                           && (TypeRegistry.NeedsPointerSuffix(retType)
                            || TypeRegistry.IsStringBuilder(retType));
                return (cMapped.TrimEnd('*'), isPtr);
            }

            var inferred = TypeInferrer.InferCSharpType(v.Initializer!.Value, _ctx);
            if (inferred.EndsWith("*"))
                return (inferred.TrimEnd('*'), true);
            return (inferred, false);
        }

        var ct = TypeInferrer.InferCSharpType(v.Initializer?.Value, _ctx);
        if (ct.EndsWith("*"))
            return (ct.TrimEnd('*'), true);
        return (ct, false);
    }

    private static bool IsSplitCall(InvocationExpressionSyntax inv) =>
       IsListStrCall(inv);

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

    // ── If ────────────────────────────────────────────────────────────────

    private void WriteIf(IfStatementSyntax ifStmt)
    {
        // is-Pattern in if-Bedingung: if (obj is Dog d) { ... }
        // → Binding-Variable nach der Bedingung deklarieren
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

    private void WriteIfWithIsPattern(IfStatementSyntax ifStmt, IsPatternExpressionSyntax isPattern)
    {
        var cond = _expr.Write(isPattern);
        _ctx.WriteLine("if (" + cond + ")");
        _ctx.WriteLine("{");
        _ctx.Indent();

        // Binding-Variable innerhalb des Blocks deklarieren
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

    // ── For ───────────────────────────────────────────────────────────────

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

    // ── ForEach ───────────────────────────────────────────────────────────

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
        bool isString = colType is "string" or "char[]";

        var lenExpr = isList ? colExpr + "->count"
                    : isString ? "strlen(" + colExpr + ")"
                    : colExpr + "_count";

        var rawElemType = forEach.Type.ToString().Trim();
        if (rawElemType == "var")
        {
            if (isList) rawElemType = TypeRegistry.GetListInnerType(colType) ?? "int";
            else if (isString) rawElemType = "char";
        }

        _ctx.WriteLine("for (int " + idxVar + " = 0; "
                     + idxVar + " < (int)(" + lenExpr + "); "
                     + idxVar + "++)");
        _ctx.WriteLine("{");
        _ctx.Indent();

        WriteForEachLoopVar(varName, idxVar, rawElemType, colExpr, colType, isList, isString);
        _ctx.LocalTypes[varName] = rawElemType;

        var bodyStmts = forEach.Statement is BlockSyntax b
            ? b.Statements.Cast<StatementSyntax>()
            : new[] { forEach.Statement };

        foreach (var s in bodyStmts)
            Write(s);

        _ctx.Dedent();
        _ctx.WriteLine("}");
    }

    private void WriteForEachLoopVar(
        string varName, string idxVar,
        string rawElemType, string colExpr, string colType,
        bool isList, bool isString)
    {
        if (isString)
        {
            _ctx.WriteLine("char " + varName + " = " + colExpr + "[" + idxVar + "];");
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

    // ── While ─────────────────────────────────────────────────────────────

    private void WriteWhile(WhileStatementSyntax whileStmt)
    {
        _ctx.WriteLine("while (" + _expr.Write(whileStmt.Condition) + ")");
        WriteBlockOrStmt(whileStmt.Statement);
    }

    // ── Do ────────────────────────────────────────────────────────────────

    private void WriteDo(DoStatementSyntax doStmt)
    {
        _ctx.WriteLine("do");
        WriteBlockOrStmt(doStmt.Statement);
        _ctx.WriteLine("while (" + _expr.Write(doStmt.Condition) + ");");
    }

    // ── Switch ────────────────────────────────────────────────────────────

    private void WriteSwitch(SwitchStatementSyntax sw)
    {
        // Pattern-Switch (Arm hat DeclarationPattern/TypePattern) → if-else-Kette
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

    /// <summary>
    /// Pattern-basierten switch-Statement als if-else-Kette ausgeben.
    /// switch (x) { case Dog d: ... break; default: ... }
    /// → if (Dog_Is(x)) { Dog* d = (Dog*)x; ... } else { ... }
    /// </summary>
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

                    // Binding-Variable deklarieren wenn DeclarationPattern
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

    // ── Try/Catch ─────────────────────────────────────────────────────────

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

    // ── Throw ─────────────────────────────────────────────────────────────

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

    // ── Using ─────────────────────────────────────────────────────────────

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

    // ── Block-Hilfsmethoden ───────────────────────────────────────────────

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