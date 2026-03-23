using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Writers;

/// <summary>
/// Transpiliert C#-Statements zu C-Code.
/// Alle Statement-Typen sind hier zentralisiert.
///
/// Erweiterung: Neuen case im WriteStatement-Switch ergänzen.
/// </summary>
public sealed class StatementWriter
{
    private readonly TranspilerContext _ctx;
    private readonly ExpressionWriter _expr;

    public StatementWriter(TranspilerContext ctx, ExpressionWriter expr)
    {
        _ctx = ctx;
        _expr = expr;
    }

    // ── Öffentliche API ───────────────────────────────────────────────────────

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
            case EmptyStatementSyntax: break;
            default:
                _ctx.WriteLine("/* UNSUPPORTED: " + stmt.GetType().Name + " */");
                break;
        }
    }

    // ── Statement-Writers ─────────────────────────────────────────────────────

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

        foreach (var v in local.Declaration.Variables)
        {
            // libnx-Struct als Stack-Variable: FsDir dir = {0};
            if (TypeRegistry.IsLibNxStruct(declType))
            {
                var si = v.Initializer != null ? " = " + _expr.Write(v.Initializer.Value) : " = {0}";
                _ctx.WriteLine(TypeRegistry.MapType(declType) + " " + v.Identifier + si + ";");
                _ctx.LocalTypes[v.Identifier.Text] = declType;
                continue;
            }

            // string buf = new string(' ', N) → char buf[N]; memset(buf, 0, N);
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
                    // StringBuilder und List<T> geben Pointer zurück → immer *
                    // Andere Objekte (Label, Button etc.) ebenfalls Pointer
                    isPtr = true;
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
                // StringBuilder und List sind Pointer-Typen (New() gibt Pointer zurück)
                isPtr = TypeRegistry.NeedsPointerSuffix(declType)
                     || TypeRegistry.IsStringBuilder(declType)
                     || TypeRegistry.IsList(declType)
                     || TypeRegistry.IsDictionary(declType);
            }

            var ptr = isPtr ? "*" : "";
            var init = v.Initializer != null ? " = " + _expr.Write(v.Initializer.Value) : "";

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

        // Länge bestimmen
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

        // Element-Typ bestimmen
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

        // for-Schleife schreiben
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
        _ctx.WriteLine("/* try */");
        _ctx.WriteLine("{");
        _ctx.Indent();
        foreach (var s in tryStmt.Block.Statements) Write(s);
        _ctx.Dedent();
        _ctx.WriteLine("}");

        foreach (var c in tryStmt.Catches)
        {
            var t = c.Declaration?.Type.ToString() ?? "Exception";
            var n = c.Declaration?.Identifier.Text ?? "";
            _ctx.WriteLine("/* catch (" + t + " " + n + ") - ignored in C */");
        }

        if (tryStmt.Finally != null)
        {
            _ctx.WriteLine("/* finally */");
            _ctx.WriteLine("{");
            _ctx.Indent();
            foreach (var s in tryStmt.Finally.Block.Statements) Write(s);
            _ctx.Dedent();
            _ctx.WriteLine("}");
        }
    }

    private void WriteThrow(ThrowStatementSyntax throwStmt)
    {
        if (throwStmt.Expression != null)
            _ctx.WriteLine("/* throw: " + throwStmt.Expression + " */");
        _ctx.WriteLine("return;");
    }

    // ── Block-Helper ──────────────────────────────────────────────────────────

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