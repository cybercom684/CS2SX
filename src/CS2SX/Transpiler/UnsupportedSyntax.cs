using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler;

/// C# constructs CS2SX cannot translate. Each one is reported as an error with the
/// C# line and a workaround, and the build stops before the C compiler runs —
/// instead of generating C that fails in GCC (or, worse, compiles and behaves
/// differently, like a dropped `goto`).
internal static class UnsupportedSyntax
{
    public static void Report(SyntaxNode root, SemanticModel? model, Action<SyntaxNode, string> error)
    {
        foreach (var node in root.DescendantNodes())
        {
            var message = Check(node, model);
            if (message != null) error(node, message);
        }
    }

    private static string? Check(SyntaxNode node, SemanticModel? model) => node switch
    {
        ForEachStatementSyntax { Expression: RangeExpressionSyntax } =>
            "foreach über einen Range (0..10) wird nicht unterstützt — eine for-Schleife verwenden: for (int i = 0; i < 10; i++).",

        RangeExpressionSyntax r when r.Parent is not ForEachStatementSyntax =>
            "Ranges (a..b) werden nicht unterstützt — bei Strings Substring(start, länge), bei Listen GetRange(start, anzahl) verwenden.",

        WithExpressionSyntax =>
            "with-Ausdrücke werden nicht unterstützt — eine Kopie mit new anlegen: new Punkt(10, p.Y).",

        // Plain generic methods are specialised beforehand (GenericMethodMonomorphizer);
        // what reaches this point is virtual/abstract, in an interface or a generic type.
        MethodDeclarationSyntax { TypeParameterList: not null } m =>
            $"Diese generische Methode ({m.Identifier.Text}<…>) wird nicht unterstützt — generische Methoden dürfen nicht virtual/abstract/override sein und nicht in Interfaces oder generischen Klassen stehen.",

        StructDeclarationSyntax { TypeParameterList: not null } s =>
            $"Generische Structs ({s.Identifier.Text}<…>) werden nicht unterstützt — eine generische Klasse verwenden.",

        LocalFunctionStatementSyntax f =>
            $"Lokale Funktionen ({f.Identifier.Text}) werden nicht unterstützt — als private Methode der Klasse schreiben.",

        QueryExpressionSyntax =>
            "LINQ-Abfragesyntax (from … select) wird nicht unterstützt — die Methodenschreibweise verwenden: list.Where(x => …).Select(x => …).",

        DelegateDeclarationSyntax d =>
            $"Eigene delegate-Typen ({d.Identifier.Text}) werden nicht unterstützt — Action<…> oder Func<…> verwenden.",

        GotoStatementSyntax =>
            "goto wird nicht unterstützt — mit Schleifen, break/continue oder einer Methode umbauen.",

        LabeledStatementSyntax l =>
            $"Sprungmarken ({l.Identifier.Text}:) werden nicht unterstützt — mit Schleifen oder einer Methode umbauen.",

        YieldStatementSyntax =>
            "yield return wird nicht unterstützt — die Werte in einer List<T> sammeln und zurückgeben.",

        StackAllocArrayCreationExpressionSyntax =>
            "stackalloc/Span<T> wird nicht unterstützt — ein normales Array verwenden: new int[n].",

        UnsafeStatementSyntax or PointerTypeSyntax =>
            "unsafe-Code und Zeiger werden nicht unterstützt.",

        CollectionExpressionSyntax =>
            "Collection-Ausdrücke ([1, 2, 3]) werden nicht unterstützt — new List<int> { 1, 2, 3 } bzw. new[] { 1, 2, 3 } verwenden.",

        BaseObjectCreationExpressionSyntax o when IsPlainObject(o, model) =>
            "new object() wird nicht unterstützt — eine eigene (leere) Klasse verwenden. Als Sperrobjekt ist es unnötig, lock wird ignoriert.",

        IdentifierNameSyntax { Identifier.Text: "dynamic" } id when IsDynamic(id, model) =>
            "dynamic wird nicht unterstützt — einen konkreten Typ verwenden.",

        InvocationExpressionSyntax
        {
            Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "GetType" } getType,
            ArgumentList.Arguments.Count: 0,
        } when !IsException(getType.Expression, model) =>
            "GetType() (Reflection) wird nur bei Exceptions unterstützt — mit 'is'/'switch' auf den Typ prüfen oder eine eigene Name-Eigenschaft verwenden.",

        _ => null,
    };

    /// Exceptions carry runtime type info, so e.GetType().Name works for them.
    private static bool IsException(ExpressionSyntax receiver, SemanticModel? model)
    {
        if (model == null) return true;   // unknown: don't block
        try
        {
            for (var t = model.GetTypeInfo(receiver).Type; t != null; t = t.BaseType)
                if (t.Name == "Exception" && t.ContainingNamespace?.ToDisplayString() == "System")
                    return true;
        }
        catch { return true; }
        return false;
    }

    private static bool IsPlainObject(BaseObjectCreationExpressionSyntax o, SemanticModel? model)
    {
        if (o is ObjectCreationExpressionSyntax { Type: PredefinedTypeSyntax p }
            && p.Keyword.IsKind(SyntaxKind.ObjectKeyword))
            return true;
        if (model == null) return false;
        try { return model.GetTypeInfo(o).Type?.SpecialType == SpecialType.System_Object; }
        catch { return false; }
    }

    private static bool IsDynamic(IdentifierNameSyntax id, SemanticModel? model)
    {
        if (model == null) return id.Parent is VariableDeclarationSyntax or ParameterSyntax;
        try { return model.GetTypeInfo(id).Type?.TypeKind == TypeKind.Dynamic; }
        catch { return false; }
    }
}
