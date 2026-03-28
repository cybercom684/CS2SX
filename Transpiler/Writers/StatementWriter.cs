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
            case TryStatementSyntax tryStmt:
                WriteTryCatch(tryStmt);
                break;
            case ThrowStatementSyntax throwStmt:
                WriteThrow(throwStmt);
                break;
            case EmptyStatementSyntax: break;
            case UsingStatementSyntax usingStmt:
                WriteUsing(usingStmt);
                break;
            default:
                _ctx.WriteLine("/* UNSUPPORTED: " + stmt.GetType().Name + " */");
                break;
        }
    }

    private void WriteUsing(UsingStatementSyntax usingStmt)
    {
        _ctx.WriteLine("{");
        _ctx.Indent();

        // Nur die erste Ressource unterstützen (bei using (var a = ..., b = ...) wird nur a behandelt)
        if (usingStmt.Declaration != null)
        {
            foreach (var varDecl in usingStmt.Declaration.Variables)
            {
                string typeName = usingStmt.Declaration.Type.ToString();
                string varName = varDecl.Identifier.Text;
                string init = varDecl.Initializer != null
                    ? _expr.Write(varDecl.Initializer.Value)
                    : "";

                // Bestimmen, ob es sich um einen Werttyp handelt (libnx‑Struct oder primitiv)
                bool isValueType = TypeRegistry.IsLibNxStruct(typeName) || TypeRegistry.IsPrimitive(typeName);
                string cType = TypeRegistry.MapType(typeName);
                string ptr = isValueType ? "" : "*";

                _ctx.WriteLine(cType + ptr + " " + varName + " = " + init + ";");

                // Body
                Write(usingStmt.Statement);

                // Dispose‑Aufruf – nur wenn der Typ die Methode besitzt
                if (TypeRegistry.IsDisposable(typeName))
                {
                    string disposeCall = isValueType ? varName + ".Dispose()" : varName + "->Dispose()";
                    _ctx.WriteLine("if (" + varName + ") " + disposeCall + ";");
                }
            }
        }
        else if (usingStmt.Expression != null)
        {
            // Fallback: Ausdruck ohne Variable (selten) – ignorieren
            _ctx.WriteLine("/* using expression not supported */");
            Write(usingStmt.Statement);
        }

        _ctx.Dedent();
        _ctx.WriteLine("}");
    }

    private void WriteReturn(ReturnStatementSyntax ret)
    {
        if (ret.Expression == null)
            _ctx.WriteLine("return;");
        else
            _ctx.WriteLine("return " + _expr.Write(ret.Expression) + ";");
    }

    private void WriteLocal(LocalDeclarationStatementSyntax local)
    {
        var declType = local.Declaration.Type.ToString().Trim();

        // params-Array (z.B. object[] args) → char*[] annähern oder überspringen
        if (declType.EndsWith("[]") && local.Declaration.Variables.Count == 1)
        {
            var v = local.Declaration.Variables[0];
            var innerType = declType[..^2].Trim();
            var cInner = TypeRegistry.MapType(innerType);

            if (v.Initializer?.Value is ImplicitArrayCreationExpressionSyntax implArr)
            {
                // new[] { a, b, c } → einzelne Variablen oder snprintf-Puffer
                // Für string[] machen wir ein const char*[] auf dem Stack
                var initExprs = implArr.Initializer.Expressions
                    .Select(e => _expr.Write(e))
                    .ToList();
                var arrName = v.Identifier.Text;
                _ctx.WriteLine("const char* " + arrName + "[] = { "
                    + string.Join(", ", initExprs) + " };");
                _ctx.LocalTypes[arrName] = declType;
                return;
            }
        }

        foreach (var v in local.Declaration.Variables)
        {
            if (TypeRegistry.IsLibNxStruct(declType))
            {
                var si = v.Initializer != null ? " = " + _expr.Write(v.Initializer.Value) : " = {0}";
                _ctx.WriteLine(TypeRegistry.MapType(declType) + " " + v.Identifier + si + ";");
                _ctx.LocalTypes[v.Identifier.Text] = declType;
                continue;
            }

            if ((declType is "string" or "var") &&
                v.Initializer?.Value is ObjectCreationExpressionSyntax strNew &&
                strNew.Type.ToString() == "string" &&
                strNew.ArgumentList?.Arguments.Count == 2)
            {
                var size = _expr.Write(strNew.ArgumentList.Arguments[1].Expression);
                _ctx.WriteLine("char " + v.Identifier + "[" + size + "];");
                _ctx.WriteLine("memset(" + v.Identifier + ", 0, " + size + ");");
                _ctx.LocalTypes[v.Identifier.Text] = "char[]";
                continue;
            }

            string cType;
            bool isPtr;

            if (declType is "var" or "var?")
            {
                if (v.Initializer?.Value is ObjectCreationExpressionSyntax oc)
                {
                    cType = oc.Type.ToString();
                    isPtr = true;
                }
                else if (v.Initializer?.Value is InvocationExpressionSyntax inv)
                {
                    var methodName = inv.Expression.ToString();
                    if (_ctx.MethodReturnTypes.TryGetValue(methodName, out var retType))
                    {
                        cType = retType;
                        isPtr = TypeRegistry.NeedsPointerSuffix(retType)
                             || TypeRegistry.IsStringBuilder(retType)
                             || TypeRegistry.IsList(retType)
                             || TypeRegistry.IsDictionary(retType);
                    }
                    else
                    {
                        cType = TypeInferrer.InferCSharpType(v.Initializer.Value, _ctx);
                        isPtr = false;
                    }
                }
                else
                {
                    cType = TypeInferrer.InferCSharpType(v.Initializer?.Value, _ctx);
                    isPtr = false;
                }
            }
            else
            {
                cType = TypeRegistry.MapType(declType);
                isPtr = TypeRegistry.NeedsPointerSuffix(declType)
                     || TypeRegistry.IsStringBuilder(declType)
                     || TypeRegistry.IsList(declType)
                     || TypeRegistry.IsDictionary(declType);
            }

            if (string.IsNullOrWhiteSpace(cType)) cType = "int";

            var ptr = isPtr ? "*" : "";

            string init;
            if (v.Initializer != null)
            {
                init = " = " + _expr.Write(v.Initializer.Value);
            }
            else if (!isPtr && TypeRegistry.IsPrimitive(declType is "var" or "var?" ? cType : declType))
            {
                init = " = 0";
            }
            else
            {
                init = "";
            }

            _ctx.WriteLine(cType + ptr + " " + v.Identifier + init + ";");
            _ctx.LocalTypes[v.Identifier.Text] = declType is "var" or "var?" ? cType : declType;
        }
    }

    private void WriteExprStmt(ExpressionStatementSyntax expr)
        => _ctx.WriteLine(_expr.Write(expr.Expression) + ";");

    private void WriteIf(IfStatementSyntax ifStmt)
    {
        _ctx.WriteLine("if (" + _expr.Write(ifStmt.Condition) + ")");
        WriteBlock(ifStmt.Statement);

        if (ifStmt.Else != null)
        {
            if (ifStmt.Else.Statement is IfStatementSyntax nested)
            {
                _ctx.Out.Write(_ctx.Tab + "else ");
                WriteIfInline(nested);
            }
            else
            {
                _ctx.WriteLine("else");
                WriteBlock(ifStmt.Else.Statement);
            }
        }
    }

    private void WriteIfInline(IfStatementSyntax ifStmt)
    {
        _ctx.Out.WriteLine("if (" + _expr.Write(ifStmt.Condition) + ")");
        WriteBlock(ifStmt.Statement);

        if (ifStmt.Else != null)
        {
            if (ifStmt.Else.Statement is IfStatementSyntax nested)
            {
                _ctx.Out.Write(_ctx.Tab + "else ");
                WriteIfInline(nested);
            }
            else
            {
                _ctx.WriteLine("else");
                WriteBlock(ifStmt.Else.Statement);
            }
        }
    }

    private void WriteFor(ForStatementSyntax forStmt)
    {
        string init;
        if (forStmt.Declaration != null)
        {
            var tName = TypeRegistry.MapType(forStmt.Declaration.Type.ToString().Trim());
            var vars = string.Join(", ", forStmt.Declaration.Variables.Select(v =>
            {
                var ie = v.Initializer != null ? " = " + _expr.Write(v.Initializer.Value) : "";
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
        WriteBlock(forStmt.Statement);
    }

    private void WriteForEach(ForEachStatementSyntax forEach)
    {
        var colRaw = forEach.Expression.ToString();
        var colKey = colRaw.TrimStart('_');
        var colExpr = _expr.Write(forEach.Expression);
        var varName = forEach.Identifier.Text;
        var idxVar = "_i_" + varName;

        string lenExpr;
        bool isString = false;

        _ctx.LocalTypes.TryGetValue(colRaw, out var colLt);
        _ctx.FieldTypes.TryGetValue(colKey, out var colFt);
        bool isList = (colLt != null && TypeRegistry.IsList(colLt))
                   || (colFt != null && TypeRegistry.IsList(colFt));

        if (isList)
        {
            lenExpr = colExpr + "->count";
        }
        else if (_ctx.LocalTypes.TryGetValue(colRaw, out var lt) && lt == "char[]")
        {
            lenExpr = "strlen(" + colExpr + ")";
            isString = true;
        }
        else if (colRaw.Contains(".Length"))
        {
            lenExpr = colExpr.Replace("->Length", "").Replace(".Length", "") + "_len";
        }
        else
        {
            lenExpr = colExpr + "_count";
        }

        var rawElemType = forEach.Type.ToString().Trim();

        if (rawElemType == "var")
        {
            if (_ctx.FieldTypes.TryGetValue(colKey, out var ft) && ft.EndsWith("[]"))
                rawElemType = ft[..^2];
            else if (_ctx.LocalTypes.TryGetValue(colRaw, out var lt2) && lt2.EndsWith("[]"))
                rawElemType = lt2[..^2];
        }

        if (rawElemType == "var" && isList)
        {
            var listType = colLt ?? colFt ?? "";
            var inner = TypeRegistry.GetListInnerType(listType);
            if (inner != null) rawElemType = inner;
        }

        var elemType = TypeRegistry.MapType(rawElemType);

        _ctx.WriteLine("for (int " + idxVar + " = 0; " + idxVar + " < (int)(" + lenExpr + "); " + idxVar + "++)");
        _ctx.WriteLine("{");
        _ctx.Indent();

        var dataAccess = isList ? colExpr + "->data[" + idxVar + "]" : colExpr + "[" + idxVar + "]";

        if (isString)
            _ctx.WriteLine("char " + varName + " = " + dataAccess + ";");
        else if (TypeRegistry.IsPrimitive(rawElemType))
            _ctx.WriteLine(elemType + " " + varName + " = " + dataAccess + ";");
        else
            _ctx.WriteLine(elemType + "* " + varName + " = " + dataAccess + ";");

        _ctx.LocalTypes[varName] = rawElemType;

        foreach (var s in (forEach.Statement is BlockSyntax b ? b.Statements.Cast<StatementSyntax>() : new[] { forEach.Statement }))
            Write(s);

        _ctx.Dedent();
        _ctx.WriteLine("}");
    }

    private void WriteWhile(WhileStatementSyntax whileStmt)
    {
        _ctx.WriteLine("while (" + _expr.Write(whileStmt.Condition) + ")");
        WriteBlock(whileStmt.Statement);
    }

    private void WriteDo(DoStatementSyntax doStmt)
    {
        _ctx.WriteLine("do");
        WriteBlock(doStmt.Statement);
        _ctx.WriteLine("while (" + _expr.Write(doStmt.Condition) + ");");
    }

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

    private void WriteTryCatch(TryStatementSyntax tryStmt)
    {
        // Eindeutiger Name für die jmp_buf‑Variable
        string jmpBufName = "_ex_buf_" + _ctx.NextTmp();
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

        // Nur den ersten catch‑Block unterstützen (mehrere wären möglich, aber für den Anfang reicht)
        if (tryStmt.Catches.Count > 0)
        {
            _ctx.WriteLine("else");
            _ctx.WriteLine("{");
            _ctx.Indent();
            var catchClause = tryStmt.Catches[0];
            // Parameter (z.B. Exception e) ignorieren
            foreach (var stmt in catchClause.Block.Statements)
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

    public void WriteBlock(StatementSyntax stmt)
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

    private void WriteBlock(BlockSyntax block)
    {
        _ctx.WriteLine("{");
        _ctx.Indent();
        foreach (var s in block.Statements) Write(s);
        _ctx.Dedent();
        _ctx.WriteLine("}");
    }
}