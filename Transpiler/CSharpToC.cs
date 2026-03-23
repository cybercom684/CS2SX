using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler;

/// <summary>
/// Haupttranspiler: wandelt C#-Syntax-Tree in C-Header oder C-Implementierung um.
/// </summary>
public sealed class CSharpToC : CSharpSyntaxWalker
{
    private readonly StringWriter _out = new StringWriter();
    private int _indent = 0;
    private string _currentClass = string.Empty;

    private readonly Dictionary<string, string> _fieldTypes = new Dictionary<string, string>(StringComparer.Ordinal);
    private readonly MethodTranspiler _methodTranspiler;

    public enum TranspileMode
    {
        HeaderOnly, Implementation
    }
    private readonly TranspileMode _mode;

    private const string SwitchAppBase = "SwitchApp";

    public CSharpToC(TranspileMode mode = TranspileMode.Implementation)
    {
        _mode = mode;
        _methodTranspiler = new MethodTranspiler(
            _out,
            () => _indent,
            () => _indent++,
            () => _indent--,
            () => _currentClass,
            () => _fieldTypes
        );
    }

    // ── Öffentliche API ────────────────────────────────────────────────────────

    public string Transpile(string csharpSource)
    {
        var tree = CSharpSyntaxTree.ParseText(csharpSource);
        var diags = tree.GetDiagnostics()
            .Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
            .ToList();

        if (diags.Count > 0)
        {
            foreach (var d in diags)
                _out.WriteLine("/* PARSE ERROR: " + d.GetMessage() + " */");
        }

        Visit(tree.GetRoot());
        return _out.ToString();
    }

    // ── Visitor-Overrides ──────────────────────────────────────────────────────

    public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        => base.VisitNamespaceDeclaration(node);

    public override void VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
        => base.VisitFileScopedNamespaceDeclaration(node);

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        _currentClass = node.Identifier.Text;
        _fieldTypes.Clear();

        var baseType = node.BaseList?.Types.FirstOrDefault()?.ToString().Trim();
        var isSwitchAppChild = baseType == SwitchAppBase;

        if (_mode == TranspileMode.HeaderOnly)
        {
            WriteStructDefinition(node, baseType);

            if (isSwitchAppChild)
                WriteInitConstructor(node);
            else
                WriteNewConstructor(node);

            foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
                VisitMethodDeclaration(method);
        }
        else
        {
            // Implementation: _fieldTypes benoetigt MethodTranspiler,
            // aber KEINE Struct-Definition (steht bereits im Header via #include).
            CollectFieldTypes(node);

            if (isSwitchAppChild)
                WriteInitConstructor(node);
            else
                WriteNewConstructor(node);

            foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
                VisitMethodDeclaration(method);
        }

        _currentClass = string.Empty;
        _fieldTypes.Clear();
    }

    /// <summary>
    /// Befoellung von _fieldTypes ohne Output-Ausgabe.
    /// Benoetigt im Implementation-Mode damit der MethodTranspiler Feldtypen kennt.
    /// </summary>
    private void CollectFieldTypes(ClassDeclarationSyntax node)
    {
        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            var csType = field.Declaration.Type.ToString().Trim();
            foreach (var v in field.Declaration.Variables)
                _fieldTypes[v.Identifier.Text.TrimStart('_')] = csType;
        }
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var isStatic = node.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
        var returnType = TypeMapper.Map(node.ReturnType.ToString().Trim());

        var name = string.IsNullOrEmpty(_currentClass)
            ? node.Identifier.Text
            : _currentClass + "_" + node.Identifier.Text;

        var parameters = new List<string>();

        // self nur bei Instanzmethoden innerhalb einer Klasse
        if (!string.IsNullOrEmpty(_currentClass) && !isStatic)
            parameters.Add(_currentClass + "* self");

        foreach (var p in node.ParameterList.Parameters)
            parameters.Add(BuildParameterDecl(p));

        var sig = returnType + " " + name + "(" + string.Join(", ", parameters) + ")";

        if (_mode == TranspileMode.HeaderOnly)
        {
            _out.WriteLine(sig + ";");
            return;
        }

        _out.WriteLine(sig);
        _out.WriteLine("{");
        _indent++;

        if (node.Body != null)
        {
            foreach (var stmt in node.Body.Statements)
                _methodTranspiler.WriteStatement(stmt);
        }
        else if (node.ExpressionBody != null)
        {
            _out.WriteLine(Pad() + "return " + _methodTranspiler.WriteExpression(node.ExpressionBody.Expression) + ";");
        }

        _indent--;
        _out.WriteLine("}");
        _out.WriteLine();
    }

    // ── Private Hilfsmethoden ─────────────────────────────────────────────────

    private string Pad() => new string(' ', _indent * 4);

    private void WriteStructDefinition(ClassDeclarationSyntax node, string? baseType)
    {
        var name = node.Identifier.Text;

        _out.WriteLine("typedef struct " + name + " " + name + ";");
        _out.WriteLine("struct " + name);
        _out.WriteLine("{");
        _indent++;

        if (!string.IsNullOrEmpty(baseType))
            _out.WriteLine(Pad() + baseType + " base;");

        // Felder
        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            var csType = field.Declaration.Type.ToString().Trim();
            var cType = TypeMapper.Map(csType);
            // Array- und List-Typen mappen bereits zu Pointer-Typen
            // → kein zusaetzlicher * noetig
            var isArray = csType.EndsWith("[]");
            var isList = TypeMapper.IsList(csType);
            var isSb = TypeMapper.IsStringBuilder(csType);
            var isPrim = TypeMapper.IsPrimitive(csType) || TypeMapper.IsLibNxStruct(csType)
                          || csType == "string" || isArray || isList || isSb || csType == "Action";
            var ptr = isPrim ? "" : "*";

            foreach (var v in field.Declaration.Variables)
            {
                var fieldName = v.Identifier.Text.TrimStart('_');
                _fieldTypes[fieldName] = csType;
                _out.WriteLine(Pad() + cType + ptr + " f_" + fieldName + ";");
            }
        }

        // Properties als Felder
        foreach (var prop in node.Members.OfType<PropertyDeclarationSyntax>())
        {
            var csType = prop.Type.ToString().Trim();
            var cType = TypeMapper.Map(csType);
            _out.WriteLine(Pad() + cType + " f_" + prop.Identifier + ";");
        }

        _indent--;
        _out.WriteLine("};");
        _out.WriteLine();
    }

    private void WriteNewConstructor(ClassDeclarationSyntax node)
    {
        var name = node.Identifier.Text;
        var sig = name + "* " + name + "_New()";

        if (_mode == TranspileMode.HeaderOnly)
        {
            _out.WriteLine(sig + ";");
            return;
        }

        _out.WriteLine(sig);
        _out.WriteLine("{");
        _indent++;
        _out.WriteLine(Pad() + name + "* self = (" + name + "*)malloc(sizeof(" + name + "));");
        _out.WriteLine(Pad() + "memset(self, 0, sizeof(" + name + "));");

        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            foreach (var v in field.Declaration.Variables)
            {
                if (v.Initializer == null) continue;
                var fieldName = v.Identifier.Text.TrimStart('_');
                var init = _methodTranspiler.WriteExpression(v.Initializer.Value);
                _out.WriteLine(Pad() + "self->f_" + fieldName + " = " + init + ";");
            }
        }

        _out.WriteLine(Pad() + "return self;");
        _indent--;
        _out.WriteLine("}");
        _out.WriteLine();
    }

    private void WriteInitConstructor(ClassDeclarationSyntax node)
    {
        var name = node.Identifier.Text;
        var sig = "void " + name + "_Init(" + name + "* self)";

        if (_mode == TranspileMode.HeaderOnly)
        {
            _out.WriteLine(sig + ";");
            return;
        }

        _out.WriteLine(sig);
        _out.WriteLine("{");
        _indent++;

        // Lifecycle-Funktionszeiger verdrahten
        foreach (var method in node.Members.OfType<MethodDeclarationSyntax>())
        {
            var methodName = method.Identifier.Text;
            if (methodName == "OnInit" || methodName == "OnFrame" || methodName == "OnExit")
                _out.WriteLine(Pad() + "self->base." + methodName + " = (void(*)(SwitchApp*))" + name + "_" + methodName + ";");
        }

        // Feld-Initializer
        foreach (var field in node.Members.OfType<FieldDeclarationSyntax>())
        {
            foreach (var v in field.Declaration.Variables)
            {
                if (v.Initializer == null) continue;
                var fieldName = v.Identifier.Text.TrimStart('_');
                var init = _methodTranspiler.WriteExpression(v.Initializer.Value);
                _out.WriteLine(Pad() + "self->f_" + fieldName + " = " + init + ";");
            }
        }

        _indent--;
        _out.WriteLine("}");
        _out.WriteLine();
    }

    private static string BuildParameterDecl(ParameterSyntax p)
    {
        if (p.Type == null) return p.Identifier.Text;

        var csType = p.Type.ToString().Trim();
        var cType = TypeMapper.Map(csType);
        var isPrim = TypeMapper.IsPrimitive(csType) || csType == "string";

        var isRef = p.Modifiers.Any(m =>
            m.IsKind(SyntaxKind.RefKeyword) || m.IsKind(SyntaxKind.OutKeyword));

        var ptr = (!isPrim || isRef) ? "*" : "";
        return cType + ptr + " " + p.Identifier;
    }
}