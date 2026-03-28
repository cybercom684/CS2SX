using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

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
        // Leerer Ausdruck (z.B. von ??=) → kein Statement ausgeben
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

            // ── Split-Sonderfall: immer zuerst prüfen ─────────────────────
            // Unabhängig von declType — var oder List<string>
            if (v.Initializer?.Value is InvocationExpressionSyntax splitInv
                && IsSplitCall(splitInv))
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
        // new T() → T*
        if (v.Initializer?.Value is ObjectCreationExpressionSyntax oc)
            return (oc.Type.ToString(), true);

        if (v.Initializer?.Value is InvocationExpressionSyntax inv)
        {
            // String.Split / .Split() → List_str* — ZUERST prüfen
            // isPtr = false weil "List_str" noch kein * hat und wir es einmal anhängen
            if (IsSplitCall(inv))
                return ("List_str", true);

            var calleeStr = inv.Expression.ToString();

            // String.Format / String.Concat → const char*
            if (calleeStr is "string.Format" or "String.Format"
                           or "string.Concat" or "String.Concat")
                return ("const char", true);

            // Bekannter Rückgabetyp aus MethodReturnTypes
            if (_ctx.MethodReturnTypes.TryGetValue(calleeStr, out var retType))
            {
                var cMapped = TypeRegistry.MapType(retType);
                // Wenn MapType bereits * enthält (List_T*, Dict_K_V*) → isPtr false
                var isPtr = !cMapped.EndsWith("*")
                           && (TypeRegistry.NeedsPointerSuffix(retType)
                            || TypeRegistry.IsStringBuilder(retType));
                return (cMapped.TrimEnd('*'), isPtr);
            }

            // Fallback — TypeInferrer gibt manchmal "List_str*" zurück
            var inferred = TypeInferrer.InferCSharpType(v.Initializer!.Value, _ctx);
            // Wenn bereits * im Typ → isPtr false, * trimmen
            if (inferred.EndsWith("*"))
                return (inferred.TrimEnd('*'), true);
            return (inferred, false);
        }

        var ct = TypeInferrer.InferCSharpType(v.Initializer?.Value, _ctx);
        if (ct.EndsWith("*"))
            return (ct.TrimEnd('*'), true);
        return (ct, false);
    }

    private static bool IsSplitCall(InvocationExpressionSyntax inv)
    {
        var calleeStr = inv.Expression.ToString();

        // string.Split("sep") oder String.Split(...)
        if (calleeStr is "string.Split" or "String.Split") return true;

        // "someString".Split(",") oder variable.Split(",")
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

    private void WriteIfInline(IfStatementSyntax ifStmt)
    {
        _ctx.Out.WriteLine("if (" + _expr.Write(ifStmt.Condition) + ")");
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
            // List<string> → const char* via List_str_Get
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
                        ? varName + ".Dispose()"
                        : varName + "->Dispose()";
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