using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler;

/// <summary>
/// Transpiliert C#-Methoden-Bodies (Statements und Expressions) nach C.
/// </summary>
public sealed class MethodTranspiler
{
    private readonly StringWriter _out;
    private readonly Func<int> _getIndent;
    private readonly Action _indent;
    private readonly Action _dedent;
    private readonly Func<string> _getCurrentClass;
    private readonly Func<Dictionary<string, string>> _getFieldTypes;

    private Dictionary<string, string> FieldTypes => _getFieldTypes();

    // Lokale Variablen der aktuellen Methode: Name → C#-Typ
    private readonly Dictionary<string, string> _localTypes = new Dictionary<string, string>(StringComparer.Ordinal);

    public MethodTranspiler(
        StringWriter out_,
        Func<int> getIndent,
        Action indent,
        Action dedent,
        Func<string> getCurrentClass,
        Func<Dictionary<string, string>> getFieldTypes)
    {
        _out = out_;
        _getIndent = getIndent;
        _indent = indent;
        _dedent = dedent;
        _getCurrentClass = getCurrentClass;
        _getFieldTypes = getFieldTypes;
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private string Tab => new string(' ', _getIndent() * 4);

    private bool IsFieldAccess(string name) =>
        name.StartsWith("_") && !string.IsNullOrEmpty(_getCurrentClass());

    // ── Statement-Dispatch ────────────────────────────────────────────────────

    public void WriteStatement(StatementSyntax stmt)
    {
        switch (stmt)
        {
            case ReturnStatementSyntax ret:
                WriteReturn(ret);
                break;

            case LocalDeclarationStatementSyntax local:
                WriteLocalDeclaration(local);
                break;

            case ExpressionStatementSyntax expr:
                _out.WriteLine(Tab + WriteExpression(expr.Expression) + ";");
                break;

            case IfStatementSyntax ifStmt:
                WriteIf(ifStmt);
                break;

            case BlockSyntax block:
                foreach (var s in block.Statements)
                    WriteStatement(s);
                break;

            case ForStatementSyntax forStmt:
                WriteFor(forStmt);
                break;

            case ForEachStatementSyntax forEach:
                WriteForEach(forEach);
                break;

            case WhileStatementSyntax whileStmt:
                _out.WriteLine(Tab + "while (" + WriteExpression(whileStmt.Condition) + ")");
                WriteBlock(whileStmt.Statement);
                break;

            case DoStatementSyntax doStmt:
                _out.WriteLine(Tab + "do");
                WriteBlock(doStmt.Statement);
                _out.WriteLine(Tab + "while (" + WriteExpression(doStmt.Condition) + ");");
                break;

            case BreakStatementSyntax:
                _out.WriteLine(Tab + "break;");
                break;

            case ContinueStatementSyntax:
                _out.WriteLine(Tab + "continue;");
                break;

            case SwitchStatementSyntax switchStmt:
                WriteSwitch(switchStmt);
                break;

            case EmptyStatementSyntax:
                break;

            case TryStatementSyntax tryStmt:
                WriteTryCatch(tryStmt);
                break;

            case ThrowStatementSyntax throwStmt:
                // throw in C → fehlerhafte Stelle markieren, Ausfuehrung stoppen
                if (throwStmt.Expression != null)
                    _out.WriteLine(Tab + "/* throw: " + throwStmt.Expression + " */");
                _out.WriteLine(Tab + "return;");
                break;

            default:
                _out.WriteLine(Tab + "/* UNSUPPORTED: " + stmt.GetType().Name + " */");
                break;
        }
    }

    // ── Statement-Writers ─────────────────────────────────────────────────────

    private void WriteReturn(ReturnStatementSyntax ret)
    {
        if (ret.Expression == null)
            _out.WriteLine(Tab + "return;");
        else
            _out.WriteLine(Tab + "return " + WriteExpression(ret.Expression) + ";");
    }

    private void WriteLocalDeclaration(LocalDeclarationStatementSyntax local)
    {
        var declType = local.Declaration.Type.ToString().Trim();

        foreach (var v in local.Declaration.Variables)
        {
            string cType;
            bool isPointer;

            if (declType == "var" || declType == "var?")
            {
                if (v.Initializer?.Value is ObjectCreationExpressionSyntax oc)
                {
                    cType = oc.Type.ToString();
                    isPointer = true;
                }
                else
                {
                    cType = InferCTypeFromExpr(v.Initializer?.Value);
                    isPointer = false;
                }
            }
            else
            {
                cType = TypeMapper.Map(declType);
                isPointer = !TypeMapper.IsPrimitive(declType)
                            && !TypeMapper.IsLibNxStruct(declType)
                            && !TypeMapper.IsList(declType)
                            && !TypeMapper.IsStringBuilder(declType)
                            && declType != "string"
                            && !declType.EndsWith("[]");
            }

            // libnx-Struct als Stack-Variable: FsDir dir = {0};
            if (TypeMapper.IsLibNxStruct(declType))
            {
                var structInit = v.Initializer != null
                    ? " = " + WriteExpression(v.Initializer.Value)
                    : " = {0}";
                _out.WriteLine(Tab + cType + " " + v.Identifier + structInit + ";");
                _localTypes[v.Identifier.Text] = declType;
                continue;
            }

            // Sonderfall: string buf = new string(' ', N) → char buf[N]; memset(buf, 0, N);
            // Wird fuer libnx-Ausgabepuffer verwendet (sd_list_dirs etc.)
            if ((declType == "string" || declType == "var") &&
                v.Initializer?.Value is ObjectCreationExpressionSyntax strNew &&
                strNew.Type.ToString() == "string" &&
                strNew.ArgumentList?.Arguments.Count == 2)
            {
                var sizeArg = WriteExpression(strNew.ArgumentList.Arguments[1].Expression);
                _out.WriteLine(Tab + "char " + v.Identifier + "[" + sizeArg + "];");
                _out.WriteLine(Tab + "memset(" + v.Identifier + ", 0, " + sizeArg + ");");
                _localTypes[v.Identifier.Text] = "char[]";
                continue;
            }

            var ptr = isPointer ? "*" : "";
            var init = v.Initializer != null ? " = " + WriteExpression(v.Initializer.Value) : "";

            _out.WriteLine(Tab + cType + ptr + " " + v.Identifier + init + ";");
            // Typ fuer InferFormatSpecifier merken
            _localTypes[v.Identifier.Text] = (declType == "var" || declType == "var?") ? cType : declType;
        }
    }

    private void WriteIf(IfStatementSyntax ifStmt)
    {
        _out.WriteLine(Tab + "if (" + WriteExpression(ifStmt.Condition) + ")");
        WriteBlock(ifStmt.Statement);

        if (ifStmt.Else != null)
        {
            if (ifStmt.Else.Statement is IfStatementSyntax nestedIf)
            {
                // else if: Tab bereits geschrieben, If schreibt eigenes Tab — daher direkt ausgeben
                _out.Write(Tab + "else ");
                WriteIfInline(nestedIf);
            }
            else
            {
                _out.WriteLine(Tab + "else");
                WriteBlock(ifStmt.Else.Statement);
            }
        }
    }

    private void WriteIfInline(IfStatementSyntax ifStmt)
    {
        _out.WriteLine("if (" + WriteExpression(ifStmt.Condition) + ")");
        WriteBlock(ifStmt.Statement);

        if (ifStmt.Else != null)
        {
            if (ifStmt.Else.Statement is IfStatementSyntax nestedIf)
            {
                _out.Write(Tab + "else ");
                WriteIfInline(nestedIf);
            }
            else
            {
                _out.WriteLine(Tab + "else");
                WriteBlock(ifStmt.Else.Statement);
            }
        }
    }

    private void WriteFor(ForStatementSyntax forStmt)
    {
        string init;
        if (forStmt.Declaration != null)
        {
            var typeName = TypeMapper.Map(forStmt.Declaration.Type.ToString().Trim());
            var vars = string.Join(", ", forStmt.Declaration.Variables.Select(v =>
            {
                var initExpr = v.Initializer != null ? " = " + WriteExpression(v.Initializer.Value) : "";
                return v.Identifier + initExpr;
            }));
            init = typeName + " " + vars;
        }
        else
        {
            init = string.Join(", ", forStmt.Initializers.Select(e => WriteExpression(e)));
        }

        var cond = forStmt.Condition != null ? WriteExpression(forStmt.Condition) : "";
        var incr = string.Join(", ", forStmt.Incrementors.Select(e => WriteExpression(e)));

        _out.WriteLine(Tab + "for (" + init + "; " + cond + "; " + incr + ")");
        WriteBlock(forStmt.Statement);
    }

    private void WriteForEach(ForEachStatementSyntax forEach)
    {
        var elemType = TypeMapper.Map(forEach.Type.ToString().Trim());
        var colExpr = WriteExpression(forEach.Expression);
        var varName = forEach.Identifier.Text;
        var idxVar = "_i_" + varName;

        // Heuristik: Kollektionstyp anhand des Ausdrucks bestimmen
        // Fall 1: Array-Feld / lokale Array-Variable  → for(int _i=0; _i < arr_len; _i++) elem = arr[_i]
        // Fall 2: string (char-Array)                 → for mit strlen
        // Fall 3: Unbekannt                           → for mit _count-Konvention

        var colRaw = forEach.Expression.ToString();

        // Laenge bestimmen
        string lenExpr;
        bool isString = false;

        // List<T> → ->count
        var colFieldKey = colRaw.TrimStart('_');
        _localTypes.TryGetValue(colRaw, out var colLt);
        FieldTypes.TryGetValue(colFieldKey, out var colFt);
        bool colIsList = (colLt != null && TypeMapper.IsList(colLt))
                      || (colFt != null && TypeMapper.IsList(colFt));

        if (colIsList)
        {
            lenExpr = colExpr + "->count";
        }
        else if (_localTypes.TryGetValue(colRaw, out var lt) && lt == "char[]")
        {
            lenExpr = "strlen(" + colExpr + ")";
            isString = true;
        }
        else if (colRaw.EndsWith(".Length") || colRaw.Contains(".Length"))
        {
            lenExpr = colExpr.Replace("->Length", "").Replace(".Length", "") + "_len";
        }
        else
        {
            // Konvention: <colExpr>_count oder sizeof/<colExpr>_len
            lenExpr = colExpr + "_count";
        }

        // Elementtyp: bei int[] → int, bei string[] → string etc.
        var rawElemType = forEach.Type.ToString().Trim();
        if (rawElemType == "var")
        {
            // Typ aus Kollektion ableiten
            var colTypeName = colRaw.TrimStart('_');
            if (FieldTypes.TryGetValue(colTypeName, out var ft) && ft.EndsWith("[]"))
                rawElemType = ft[..^2];
            else if (_localTypes.TryGetValue(colRaw, out var lt2) && lt2.EndsWith("[]"))
                rawElemType = lt2[..^2];
        }
        // Elementtyp bei List<T> aus dem List-Typ ableiten
        if (rawElemType == "var" && colIsList)
        {
            var listType = colLt ?? colFt ?? "";
            var inner = TypeMapper.GetListInnerType(listType);
            if (inner != null) rawElemType = inner;
        }
        elemType = TypeMapper.Map(rawElemType);

        _out.WriteLine(Tab + "for (int " + idxVar + " = 0; "
                           + idxVar + " < (int)(" + lenExpr + "); "
                           + idxVar + "++)");
        _out.WriteLine(Tab + "{");
        _indent();

        // List<T>: Zugriff über ->data[i]
        var dataAccess = colIsList ? colExpr + "->data[" + idxVar + "]"
                                   : colExpr + "[" + idxVar + "]";

        if (isString)
            _out.WriteLine(Tab + "char " + varName + " = " + dataAccess + ";");
        else if (TypeMapper.IsPrimitive(rawElemType))
            _out.WriteLine(Tab + elemType + " " + varName + " = " + dataAccess + ";");
        else
            _out.WriteLine(Tab + elemType + "* " + varName + " = " + dataAccess + ";");

        _localTypes[varName] = rawElemType;

        WriteStatement(forEach.Statement);
        _dedent();
        _out.WriteLine(Tab + "}");
    }

    private void WriteTryCatch(TryStatementSyntax tryStmt)
    {
        // try-Block direkt ausgeben (kein C-Äquivalent für Exceptions)
        _out.WriteLine(Tab + "/* try */");
        _out.WriteLine(Tab + "{");
        _indent();
        foreach (var s in tryStmt.Block.Statements)
            WriteStatement(s);
        _dedent();
        _out.WriteLine(Tab + "}");

        // catch-Klauseln als auskommentierte Blöcke (laufen nie)
        foreach (var catchClause in tryStmt.Catches)
        {
            var exType = catchClause.Declaration?.Type.ToString() ?? "Exception";
            var exVar = catchClause.Declaration?.Identifier.Text ?? "";
            _out.WriteLine(Tab + "/* catch (" + exType + " " + exVar + ") - ignored in C */");
        }

        // finally → immer ausführen
        if (tryStmt.Finally != null)
        {
            _out.WriteLine(Tab + "/* finally */");
            _out.WriteLine(Tab + "{");
            _indent();
            foreach (var s in tryStmt.Finally.Block.Statements)
                WriteStatement(s);
            _dedent();
            _out.WriteLine(Tab + "}");
        }
    }

    private void WriteSwitch(SwitchStatementSyntax switchStmt)
    {
        _out.WriteLine(Tab + "switch (" + WriteExpression(switchStmt.Expression) + ")");
        _out.WriteLine(Tab + "{");
        _indent();

        foreach (var section in switchStmt.Sections)
        {
            foreach (var label in section.Labels)
            {
                if (label is CaseSwitchLabelSyntax caseLabel)
                    _out.WriteLine(Tab + "case " + WriteExpression(caseLabel.Value) + ":");
                else if (label is DefaultSwitchLabelSyntax)
                    _out.WriteLine(Tab + "default:");
            }

            _indent();
            foreach (var s in section.Statements)
                WriteStatement(s);
            _dedent();
        }

        _dedent();
        _out.WriteLine(Tab + "}");
    }

    private void WriteBlock(StatementSyntax stmt)
    {
        _out.WriteLine(Tab + "{");
        _indent();

        if (stmt is BlockSyntax block)
        {
            foreach (var s in block.Statements)
                WriteStatement(s);
        }
        else
        {
            WriteStatement(stmt);
        }

        _dedent();
        _out.WriteLine(Tab + "}");
    }

    // ── Expression-Dispatch ───────────────────────────────────────────────────

    public string WriteExpression(SyntaxNode? expr)
    {
        if (expr == null) return "";

        switch (expr)
        {
            case BinaryExpressionSyntax bin:
                return WriteBinary(bin);

            case LiteralExpressionSyntax lit:
                return WriteLiteral(lit);

            case IdentifierNameSyntax id:
                return WriteIdentifier(id);

            case PrefixUnaryExpressionSyntax pre:
                return pre.OperatorToken.Text + WriteExpression(pre.Operand);

            case PostfixUnaryExpressionSyntax post:
                return WriteExpression(post.Operand) + post.OperatorToken.Text;

            case AssignmentExpressionSyntax assign:
                return WriteAssignment(assign);

            case MemberAccessExpressionSyntax mem:
                return WriteMemberAccess(mem);

            case InvocationExpressionSyntax inv:
                return WriteInvocation(inv);

            case InterpolatedStringExpressionSyntax interp:
                return BuildPrintf(interp, newline: false);

            case ArrayCreationExpressionSyntax arrCreate:
                return WriteArrayCreation(arrCreate);

            case ObjectCreationExpressionSyntax obj:
                return WriteObjectCreation(obj);

            case ParenthesizedExpressionSyntax par:
                return "(" + WriteExpression(par.Expression) + ")";

            case ConditionalExpressionSyntax cond:
                return "(" + WriteExpression(cond.Condition) + " ? "
                    + WriteExpression(cond.WhenTrue) + " : "
                    + WriteExpression(cond.WhenFalse) + ")";

            case CastExpressionSyntax cast:
                return "(" + TypeMapper.Map(cast.Type.ToString().Trim()) + ")"
                    + WriteExpression(cast.Expression);

            case ElementAccessExpressionSyntax elem:
                return WriteExpression(elem.Expression)
                    + "[" + WriteExpression(elem.ArgumentList.Arguments[0].Expression) + "]";

            case DefaultExpressionSyntax:
                return "NULL";

            case ThisExpressionSyntax:
                return "self";

            default:
                // Roslyn hat kein eigenes NullLiteralExpressionSyntax —
                // null-Literale kommen als LiteralExpressionSyntax mit Kind NullLiteralExpression
                return expr.ToString();
        }
    }

    // ── Expression-Writers ────────────────────────────────────────────────────

    private string WriteLiteral(LiteralExpressionSyntax lit)
    {
        // null, true, false
        if (lit.IsKind(SyntaxKind.NullLiteralExpression)) return "NULL";
        if (lit.IsKind(SyntaxKind.TrueLiteralExpression)) return "1";
        if (lit.IsKind(SyntaxKind.FalseLiteralExpression)) return "0";

        // String-Literale
        if (lit.Token.IsKind(SyntaxKind.StringLiteralToken))
            return "\"" + EscapeString(lit.Token.ValueText) + "\"";

        // Char-Literale
        if (lit.Token.IsKind(SyntaxKind.CharacterLiteralToken))
            return "'" + EscapeChar(lit.Token.ValueText) + "'";

        // Zahlen: C#-Suffixe anpassen
        var text = lit.Token.Text;
        if (text.EndsWith("f") || text.EndsWith("F"))
            return text.Substring(0, text.Length - 1) + "f";
        if (text.EndsWith("d") || text.EndsWith("D"))
            return text.Substring(0, text.Length - 1);
        if (text.EndsWith("m") || text.EndsWith("M"))
            return text.Substring(0, text.Length - 1);
        if (text.EndsWith("ul") || text.EndsWith("UL") || text.EndsWith("uL") || text.EndsWith("Ul"))
            return text.Substring(0, text.Length - 2) + "ULL";
        if (text.EndsWith("u") || text.EndsWith("U"))
            return text.Substring(0, text.Length - 1) + "U";
        if (text.EndsWith("l") || text.EndsWith("L"))
            return text.Substring(0, text.Length - 1) + "LL";

        return text;
    }

    private string WriteIdentifier(IdentifierNameSyntax id)
    {
        var name = id.Identifier.Text;
        var mapped = TypeMapper.MapEnum(name);
        if (mapped != name) return mapped;

        // Globale CS2SX-Variablen nie als Felder behandeln
        if (name == "_cs2sx_strbuf") return "_cs2sx_strbuf";

        if (IsFieldAccess(name))
            return "self->f_" + name.TrimStart('_');

        return name;
    }

    private string WriteBinary(BinaryExpressionSyntax bin)
    {
        var left = WriteExpression(bin.Left);
        var right = WriteExpression(bin.Right);
        var op = bin.OperatorToken.Text;

        // String-Vergleich == / != → strcmp
        if ((op == "==" || op == "!=") && IsStringExpression(bin.Left))
            return "strcmp(" + left + ", " + right + ") " + op + " 0";

        // is-Ausdruck → kein C-Äquivalent
        if (bin.IsKind(SyntaxKind.IsExpression))
            return "/* is-check: " + bin + " */ 1";

        return left + " " + op + " " + right;
    }

    private string WriteMemberAccess(MemberAccessExpressionSyntax mem)
    {
        var full = mem.ToString();
        var mapped = TypeMapper.MapEnum(full);
        if (mapped != full) return mapped;

        // Namespace-qualifizierter libnx-Aufruf: LibNX.Services.Psm.psmFoo → psmFoo
        // Wird als Invocation-Callee aufgerufen; letzten Bezeichner extrahieren.
        if (full.StartsWith("LibNX."))
            return mem.Name.Identifier.Text;

        var obj = WriteExpression(mem.Expression);
        var prop = mem.Name.Identifier.Text;
        var objExpr = mem.Expression.ToString().TrimStart('_');

        if (prop == "Length")
            return "strlen(" + obj + ")";

        // List<T>.Count → list->count
        if (prop == "Count")
        {
            var rawObj = mem.Expression.ToString();
            var keyObj = rawObj.TrimStart('_');
            if ((_localTypes.TryGetValue(rawObj, out var lt) && TypeMapper.IsList(lt)) ||
                (FieldTypes.TryGetValue(keyObj, out var ft) && TypeMapper.IsList(ft)))
                return obj + "->count";
        }

        // StringBuilder.Length → sb->length
        if (prop == "Length")
        {
            var rawObj = mem.Expression.ToString();
            var keyObj = rawObj.TrimStart('_');
            if ((_localTypes.TryGetValue(rawObj, out var sblt) && TypeMapper.IsStringBuilder(sblt)) ||
                (FieldTypes.TryGetValue(keyObj, out var sbft) && TypeMapper.IsStringBuilder(sbft)))
                return obj + "->length";
        }

        // Lokale libnx-Struct-Variable → Punkt-Zugriff (kein Pointer)
        if (_localTypes.TryGetValue(mem.Expression.ToString(), out var localT)
            && TypeMapper.IsLibNxStruct(localT))
            return obj + "." + prop;

        // Feld der eigenen Klasse das ein Struct ist → Punkt-Zugriff nach Dereferenz
        if (FieldTypes.TryGetValue(objExpr, out var fieldT)
            && TypeMapper.IsLibNxStruct(fieldT))
            return obj + "." + prop;

        return obj + "->" + prop;
    }

    private string WriteAssignment(AssignmentExpressionSyntax assign)
    {
        var op = assign.OperatorToken.Text;
        var right = WriteExpression(assign.Right);

        if (assign.Left is MemberAccessExpressionSyntax mem)
        {
            var obj = WriteExpression(mem.Expression);
            var prop = mem.Name.Identifier.Text;
            var cProp = MapPropertyName(prop);
            var objRaw = mem.Expression.ToString();
            var objKey = objRaw.TrimStart('_');

            // Struct-Zugriff: . statt ->
            bool isStruct = (_localTypes.TryGetValue(objRaw, out var lt) && TypeMapper.IsLibNxStruct(lt))
                         || (FieldTypes.TryGetValue(objKey, out var ft) && TypeMapper.IsLibNxStruct(ft));
            var arrow = isStruct ? "." : "->";

            // Text = $"..." → snprintf + Label_SetText
            if (prop == "Text" && assign.Right is InterpolatedStringExpressionSyntax interp)
            {
                var (fmt, args) = BuildFormatString(interp);
                if (args.Count == 0)
                    return "Label_SetText(" + obj + ", \"" + fmt + "\")";
                return "snprintf(_cs2sx_strbuf, sizeof(_cs2sx_strbuf), \""
                    + fmt + "\", " + string.Join(", ", args) + ");\n"
                    + Tab + "Label_SetText(" + obj + ", _cs2sx_strbuf)";
            }

            // Text = "literal string"
            if (prop == "Text" && assign.Right is LiteralExpressionSyntax litStr
                && litStr.Token.IsKind(SyntaxKind.StringLiteralToken))
                return "Label_SetText(" + obj + ", \"" + EscapeString(litStr.Token.ValueText) + "\")";

            // Text = anything else → Label_SetText (covers sb.ToString(), variables, etc.)
            if (prop == "Text")
                return "Label_SetText(" + obj + ", " + right + ")";

            // OnClick → Funktionszeiger
            if (prop == "OnClick")
            {
                var methodName = assign.Right.ToString().Trim();
                return obj + "->OnClick = (void(*)(void*))" + _getCurrentClass() + "_" + methodName;
            }

            return obj + arrow + cProp + " " + op + " " + right;
        }

        return WriteExpression(assign.Left) + " " + op + " " + right;
    }

    private string WriteArrayCreation(ArrayCreationExpressionSyntax arr)
    {
        // new int[5] → (int*)malloc(5 * sizeof(int))
        var elemType = arr.Type.ElementType.ToString().Trim();
        var cType = TypeMapper.Map(elemType);
        if (arr.Type.RankSpecifiers.Count > 0 &&
            arr.Type.RankSpecifiers[0].Sizes.Count > 0)
        {
            var size = WriteExpression(arr.Type.RankSpecifiers[0].Sizes[0]);
            return "(" + cType + "*)malloc(" + size + " * sizeof(" + cType + "))";
        }
        return "(" + cType + "*)malloc(sizeof(" + cType + "))";
    }

    private string WriteObjectCreation(ObjectCreationExpressionSyntax obj)
    {
        var typeName = obj.Type.ToString();

        // new StringBuilder(N) → StringBuilder_New(N)
        if (TypeMapper.IsStringBuilder(typeName))
        {
            var cap = obj.ArgumentList?.Arguments.Count > 0
                ? WriteExpression(obj.ArgumentList.Arguments[0].Expression)
                : "256";
            return "StringBuilder_New(" + cap + ")";
        }

        // new List<int>() → List_int_New()
        if (TypeMapper.IsList(typeName))
        {
            var inner = TypeMapper.GetListInnerType(typeName)!;
            var cInner = inner == "string" ? "char" : TypeMapper.Map(inner);
            return "List_" + cInner + "_New()";
        }

        var args = obj.ArgumentList?.Arguments.Select(a => WriteExpression(a.Expression))
                       ?? Enumerable.Empty<string>();

        var creation = typeName + "_New(" + string.Join(", ", args) + ")";

        // Object Initializer { X=5, Y=10, ... } → temp-Variable + Eigenschafts-Zuweisungen
        if (obj.Initializer != null && obj.Initializer.Expressions.Count > 0)
        {
            var tmp = "_tmp_" + typeName.ToLower() + "_" + (_tmpCounter++);
            _out.WriteLine(Tab + typeName + "* " + tmp + " = " + creation + ";");
            foreach (var expr in obj.Initializer.Expressions)
            {
                if (expr is AssignmentExpressionSyntax asgn)
                {
                    var prop = asgn.Left.ToString().Trim();
                    var val = WriteExpression(asgn.Right);
                    var cProp = MapPropertyName(prop);
                    _out.WriteLine(Tab + tmp + "->" + cProp + " = " + val + ";");
                }
            }
            return tmp;
        }

        return creation;
    }

    private int _tmpCounter = 0;

    private string WriteInvocation(InvocationExpressionSyntax inv)
    {
        var args = inv.ArgumentList.Arguments
            .Select(a =>
            {
                var expr = WriteExpression(a.Expression);
                // ref/out-Argument → Pointer-Uebergabe mit &
                if (a.RefKindKeyword.IsKind(SyntaxKind.RefKeyword) ||
                    a.RefKindKeyword.IsKind(SyntaxKind.OutKeyword))
                {
                    // char[]-Puffer verfaellt automatisch zum Pointer — kein & noetig
                    if (_localTypes.TryGetValue(a.Expression.ToString(), out var lt))
                    {
                        if (lt == "char[]") return expr;                    // char[] → kein &
                        if (TypeMapper.IsLibNxStruct(lt)) return "&" + expr; // Struct → &
                    }
                    var argName = a.Expression.ToString().TrimStart('_');
                    if (FieldTypes.TryGetValue(argName, out var ft) && ft == "string")
                        return expr;
                    return "&" + expr;
                }
                return expr;
            })
            .ToList();

        // LibNX-Namespace-Aufruf: LibNX.Services.Psm.psmFoo(...)
        // → letzten Bezeichner extrahieren, direkt als C-Funktion aufrufen
        if (inv.Expression is MemberAccessExpressionSyntax libNxAccess)
        {
            var fullName = inv.Expression.ToString();
            if (fullName.StartsWith("LibNX."))
            {
                var funcName = libNxAccess.Name.Identifier.Text;
                return funcName + "(" + string.Join(", ", args) + ")";
            }
        }

        // Feld-Methodenaufruf: _counter.Increment() → Counter_Increment(self->f_counter)
        if (inv.Expression is MemberAccessExpressionSyntax memberAccess)
        {
            var objStr = memberAccess.Expression.ToString();
            var methodName = memberAccess.Name.Identifier.Text;

            // Lokale List<T>-Variable: nums.Add(x) → List_int_Add(nums, x)
            var localObjKey = objStr.TrimStart('_');
            string? listFieldType = null;
            string? listObjExpr = null;

            if (_localTypes.TryGetValue(objStr, out var localT) && TypeMapper.IsList(localT))
            {
                listFieldType = localT;
                listObjExpr = objStr;
            }
            else if (objStr.StartsWith("_") && FieldTypes.TryGetValue(localObjKey, out var ft) && TypeMapper.IsList(ft))
            {
                listFieldType = ft;
                listObjExpr = "self->f_" + localObjKey;
            }

            if (listFieldType != null && listObjExpr != null)
            {
                var inner = TypeMapper.GetListInnerType(listFieldType)!;
                var cInner = inner == "string" ? "char" : TypeMapper.Map(inner);
                var cList = "List_" + cInner;
                switch (methodName)
                {
                    case "Add":
                        return cList + "_Add(" + listObjExpr + ", " + string.Join(", ", args) + ")";
                    case "Clear":
                        return cList + "_Clear(" + listObjExpr + ")";
                    case "RemoveAt":
                        return cList + "_Remove(" + listObjExpr + ", " + string.Join(", ", args) + ")";
                    case "Contains":
                        return cList + "_Contains(" + listObjExpr + ", " + string.Join(", ", args) + ")";
                }
            }

            // StringBuilder-Methoden: sb.Append(...) sb.AppendLine(...) sb.Clear() sb.ToString()
            string? sbObjExpr = null;
            if (_localTypes.TryGetValue(objStr, out var sbLt) && TypeMapper.IsStringBuilder(sbLt))
                sbObjExpr = objStr;
            else if (objStr.StartsWith("_") && FieldTypes.TryGetValue(objStr.TrimStart('_'), out var sbFt) && TypeMapper.IsStringBuilder(sbFt))
                sbObjExpr = "self->f_" + objStr.TrimStart('_');

            if (sbObjExpr != null)
            {
                if (methodName == "Clear")
                    return "StringBuilder_Clear(" + sbObjExpr + ")";
                if (methodName == "ToString")
                    return "StringBuilder_ToString(" + sbObjExpr + ")";
                if (methodName == "AppendLine")
                {
                    if (args.Count == 0)
                        return "StringBuilder_AppendChar(" + sbObjExpr + ", '\\n')";
                    var argExpr = inv.ArgumentList.Arguments[0].Expression;
                    var argType = InferCSharpType(argExpr);
                    return argType == "int" ? "StringBuilder_AppendLineInt(" + sbObjExpr + ", " + args[0] + ")"
                         : argType == "uint" ? "StringBuilder_AppendUInt(" + sbObjExpr + ", " + args[0] + "); StringBuilder_AppendChar(" + sbObjExpr + ", '\\n')"
                         : "StringBuilder_AppendLine(" + sbObjExpr + ", " + args[0] + ")";
                }
                if (methodName == "Append")
                {
                    if (args.Count == 0) return "";
                    var argExpr = inv.ArgumentList.Arguments[0].Expression;
                    var argType = InferCSharpType(argExpr);
                    return argType == "int" ? "StringBuilder_AppendInt(" + sbObjExpr + ", " + args[0] + ")"
                         : argType == "uint" ? "StringBuilder_AppendUInt(" + sbObjExpr + ", " + args[0] + ")"
                         : argType == "float" ? "StringBuilder_AppendFloat(" + sbObjExpr + ", " + args[0] + ")"
                         : argType == "char" ? "StringBuilder_AppendChar(" + sbObjExpr + ", " + args[0] + ")"
                         : "StringBuilder_AppendStr(" + sbObjExpr + ", " + args[0] + ")";
                }
            }

            // Instanz-Methoden auf string/int/float: x.ToString(), s.Contains(...) etc.
            var receiverExpr = WriteExpression(memberAccess.Expression);
            var receiverRaw = memberAccess.Expression.ToString();
            var receiverKey = receiverRaw.TrimStart('_');

            // Typ des Receivers bestimmen
            string receiverType = "";
            if (_localTypes.TryGetValue(receiverRaw, out var rlt)) receiverType = rlt;
            else if (FieldTypes.TryGetValue(receiverKey, out var rft)) receiverType = rft;

            // .ToString() auf Zahlen
            if (methodName == "ToString")
            {
                return receiverType == "uint" || receiverType == "u32"
                    ? "UInt_ToString((unsigned int)" + receiverExpr + ")"
                    : receiverType == "float"
                    ? "Float_ToString(" + receiverExpr + ")"
                    : "Int_ToString((int)" + receiverExpr + ")";
            }

            // string-Instanz-Methoden
            if (methodName == "Contains")
                return "String_Contains(" + receiverExpr + ", " + (args.Count > 0 ? args[0] : "\"\"") + ")";
            if (methodName == "StartsWith")
                return "String_StartsWith(" + receiverExpr + ", " + (args.Count > 0 ? args[0] : "\"\"") + ")";
            if (methodName == "EndsWith")
                return "String_EndsWith(" + receiverExpr + ", " + (args.Count > 0 ? args[0] : "\"\"") + ")";
            if (methodName == "Equals")
                return "strcmp(" + receiverExpr + ", " + (args.Count > 0 ? args[0] : "\"\"") + ") == 0";

            if (objStr.StartsWith("_"))
            {
                var fieldKey = objStr.TrimStart('_');
                if (FieldTypes.TryGetValue(fieldKey, out var fieldType))
                {
                    var allArgs = new List<string> { "self->f_" + fieldKey };
                    allArgs.AddRange(args);
                    return fieldType + "_" + methodName + "(" + string.Join(", ", allArgs) + ")";
                }
            }
        }

        var calleeStr = inv.Expression.ToString();

        switch (calleeStr)
        {
            case "Console.WriteLine":
                {
                    if (args.Count == 0) return "printf(\"\\n\")";
                    var firstArg = inv.ArgumentList.Arguments[0].Expression;
                    if (firstArg is LiteralExpressionSyntax lit && lit.Token.IsKind(SyntaxKind.StringLiteralToken))
                        return "printf(\"" + EscapeFormatString(lit.Token.ValueText) + "\\n\")";
                    if (firstArg is InterpolatedStringExpressionSyntax interp)
                        return BuildPrintf(interp, newline: true);
                    return "printf(\"%s\\n\", " + WriteExpression(firstArg) + ")";
                }

            case "Console.Write":
                {
                    if (args.Count == 0) return "printf(\"\")";
                    var firstArg = inv.ArgumentList.Arguments[0].Expression;
                    if (firstArg is LiteralExpressionSyntax lit && lit.Token.IsKind(SyntaxKind.StringLiteralToken))
                        return "printf(\"" + EscapeFormatString(lit.Token.ValueText) + "\")";
                    if (firstArg is InterpolatedStringExpressionSyntax interp)
                        return BuildPrintf(interp, newline: false);
                    return "printf(\"%s\", " + WriteExpression(firstArg) + ")";
                }

            case "Input.IsDown":
                return "(self->base.kDown & " + TypeMapper.MapEnum(args[0]) + ")";

            case "Input.IsHeld":
                return "(self->base.kHeld & " + TypeMapper.MapEnum(args[0]) + ")";

            case "Input.IsUp":
                return "(!(self->base.kHeld & " + TypeMapper.MapEnum(args[0]) + "))";

            case "Form.Add":
                return "SwitchApp_Add((SwitchApp*)self, (Control*)" + args[0] + ")";

            // string-Methoden
            case "string.IsNullOrEmpty":
                return "String_IsNullOrEmpty(" + args[0] + ")";
            case "string.IsNullOrWhiteSpace":
                return "String_IsNullOrEmpty(" + args[0] + ")";
            case "string.Contains":
                return "String_Contains(" + args[0] + ", " + (args.Count > 1 ? args[1] : "\"\"") + ")";
            case "string.StartsWith":
                return "String_StartsWith(" + args[0] + ", " + (args.Count > 1 ? args[1] : "\"\"") + ")";
            case "string.EndsWith":
                return "String_EndsWith(" + args[0] + ", " + (args.Count > 1 ? args[1] : "\"\"") + ")";

            // Math
            case "Math.Abs": return "abs(" + args[0] + ")";
            case "Math.Min": return "MIN(" + args[0] + ", " + args[1] + ")";
            case "Math.Max": return "MAX(" + args[0] + ", " + args[1] + ")";
            case "Math.Sqrt": return "sqrtf(" + args[0] + ")";
            case "Math.Floor": return "floorf(" + args[0] + ")";
            case "Math.Ceil": return "ceilf(" + args[0] + ")";
        }

        var callee = TypeMapper.MapMethod(calleeStr);

        // Methode der eigenen Klasse: CalcSum() → ClassName_CalcSum(self)
        if (inv.Expression is IdentifierNameSyntax ownMethod)
        {
            var methodName = ownMethod.Identifier.Text;
            // Nur wenn es kein bekannter C/libnx-Bezeichner ist
            if (!calleeStr.Contains(".") && char.IsUpper(methodName[0]))
            {
                var allArgs = new List<string> { "self" };
                allArgs.AddRange(args);
                return _getCurrentClass() + "_" + methodName + "(" + string.Join(", ", allArgs) + ")";
            }
        }

        return callee + "(" + string.Join(", ", args) + ")";
    }

    // ── Format-String-Helpers ─────────────────────────────────────────────────

    private string BuildPrintf(InterpolatedStringExpressionSyntax interp, bool newline)
    {
        var (fmt, args) = BuildFormatString(interp);
        var nl = newline ? "\\n" : "";
        if (args.Count == 0)
            return "printf(\"" + fmt + nl + "\")";
        return "printf(\"" + fmt + nl + "\", " + string.Join(", ", args) + ")";
    }

    private (string fmt, List<string> args) BuildFormatString(InterpolatedStringExpressionSyntax interp)
    {
        var fmt = new System.Text.StringBuilder();
        var args = new List<string>();

        foreach (var part in interp.Contents)
        {
            if (part is InterpolatedStringTextSyntax text)
            {
                fmt.Append(EscapeFormatString(text.TextToken.ValueText));
            }
            else if (part is InterpolationSyntax hole)
            {
                fmt.Append(InferFormatSpecifier(hole.Expression));
                args.Add(WriteExpression(hole.Expression));
            }
        }

        return (fmt.ToString(), args);
    }

    private string InferFormatSpecifier(ExpressionSyntax expr)
    {
        switch (expr)
        {
            case LiteralExpressionSyntax lit:
                if (lit.Token.Value is int || lit.Token.Value is long ||
                    lit.Token.Value is short || lit.Token.Value is byte || lit.Token.Value is sbyte)
                    return "%d";
                if (lit.Token.Value is uint || lit.Token.Value is ulong || lit.Token.Value is ushort)
                    return "%u";
                if (lit.Token.Value is float) return "%f";
                if (lit.Token.Value is double) return "%lf";
                return "%s";

            case IdentifierNameSyntax id:
                {
                    var fieldName = id.Identifier.Text.TrimStart('_');
                    if (FieldTypes.TryGetValue(fieldName, out var csType))
                        return TypeMapper.FormatSpecifier(TypeMapper.Map(csType));
                    // Lokale Variable: Typ aus _localTypes nachschlagen
                    if (_localTypes.TryGetValue(id.Identifier.Text, out var localCsType))
                        return TypeMapper.FormatSpecifier(TypeMapper.Map(localCsType));
                    if (id.Identifier.Text.StartsWith("_")) return "%d";
                    return "%s";
                }

            case CastExpressionSyntax cast:
                return TypeMapper.FormatSpecifier(TypeMapper.Map(cast.Type.ToString().Trim()));

            case MemberAccessExpressionSyntax:
                return "%d";

            case InvocationExpressionSyntax inv2:
                {
                    var callee = inv2.Expression.ToString();
                    if (callee.Contains("Get") || callee.Contains("Count") || callee.Contains("Length"))
                        return "%d";
                    return "%s";
                }

            case PostfixUnaryExpressionSyntax:
            case PrefixUnaryExpressionSyntax:
            case BinaryExpressionSyntax:
                return "%d";

            default:
                return "%s";
        }
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    private bool IsStringExpression(ExpressionSyntax expr)
    {
        if (expr is LiteralExpressionSyntax lit && lit.Token.IsKind(SyntaxKind.StringLiteralToken))
            return true;

        if (expr is IdentifierNameSyntax id)
        {
            var name = id.Identifier.Text.TrimStart('_');
            if (FieldTypes.TryGetValue(name, out var csType))
                return csType == "string";
        }

        return false;
    }

    private static string InferCTypeFromExpr(ExpressionSyntax? expr)
    {
        if (expr == null) return "int";

        switch (expr)
        {
            case LiteralExpressionSyntax lit:
                if (lit.Token.Value is int) return "int";
                if (lit.Token.Value is float) return "float";
                if (lit.Token.Value is double) return "double";
                if (lit.Token.Value is bool) return "bool";
                if (lit.Token.Value is string) return "const char*";
                return "int";
            default:
                return "int";
        }
    }

    private static string MapPropertyName(string prop)
    {
        switch (prop)
        {
            case "X": return "base.x";
            case "Y": return "base.y";
            case "Width": return "base.width";
            case "Height": return "base.height";
            case "Visible": return "base.visible";
            case "Text": return "text";
            case "Focused": return "focused";
            case "OnClick": return "OnClick";
            case "value": return "value";
            case "Value": return "value";
            case "width_chars": return "width_chars";
            case "WidthChars": return "width_chars";
            default: return "f_" + prop;
        }
    }

    /// Leitet den C#-Typ eines Ausdrucks her (fuer Append-Dispatch)
    private string InferCSharpType(ExpressionSyntax expr)
    {
        switch (expr)
        {
            case LiteralExpressionSyntax lit:
                if (lit.Token.Value is int) return "int";
                if (lit.Token.Value is uint) return "uint";
                if (lit.Token.Value is float) return "float";
                if (lit.Token.Value is double) return "double";
                if (lit.Token.Value is char) return "char";
                if (lit.Token.Value is string) return "string";
                return "int";
            case IdentifierNameSyntax id:
                {
                    var n = id.Identifier.Text.TrimStart('_');
                    if (_localTypes.TryGetValue(id.Identifier.Text, out var lt)) return lt;
                    if (FieldTypes.TryGetValue(n, out var ft)) return ft;
                    return "int";
                }
            case CastExpressionSyntax cast:
                return cast.Type.ToString().Trim();
            case InvocationExpressionSyntax:
                return "int";
            default:
                return "int";
        }
    }

    // Fuer normale String-Literale (kein %%-Escaping noetig)
    private static string EscapeString(string s)
    {
        return s.Replace("\\\\", "\\\\\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
    }

    // Fuer Format-Strings (snprintf/printf): % muss zu %% escaped werden
    private static string EscapeFormatString(string s)
    {
        return EscapeString(s).Replace("%", "%%");
    }

    private static string EscapeChar(string s)
    {
        return s.Replace("\\", "\\\\").Replace("'", "\\'");
    }
}