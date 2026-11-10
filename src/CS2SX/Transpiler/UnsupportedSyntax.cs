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
        // async methods that could not be turned into state machines (AsyncLowering).
        if (model != null)
            foreach (var (node, message) in CS2SX.Build.AsyncLowering.Problems(root, model))
                error(node, message);
    }

    /// True inside an instance member — its C function has a parameter named `self`.
    private static bool InInstanceMember(SyntaxNode node) =>
        node.Ancestors().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault() is { } m
        && !m.Modifiers.Any(x => x.IsKind(SyntaxKind.StaticKeyword));

    private const string SelfName =
        "Der Name self ist in Instanzmethoden vergeben (so heißt this im erzeugten C) — die Variable umbenennen.";

    private static string? Check(SyntaxNode node, SemanticModel? model) => node switch
    {
        VariableDeclaratorSyntax { Identifier.Text: "self", Parent.Parent: LocalDeclarationStatementSyntax } v
            when InInstanceMember(v) => SelfName,
        ParameterSyntax { Identifier.Text: "self" } p when InInstanceMember(p) => SelfName,

        ForEachStatementSyntax { Expression: RangeExpressionSyntax } =>
            "foreach über einen Range wird nur mit Start und Ende ohne ^ unterstützt (foreach (var i in 0..10)) — sonst eine for-Schleife verwenden.",

        RangeExpressionSyntax r when r.Parent is not ForEachStatementSyntax =>
            "Dieser Range (a..b) wird nicht unterstützt — Ranges gehen bei Strings und Listen auf einer einfachen Variable/einem Feld (s[1..3], list[2..]); bei Arrays eine Schleife verwenden.",

        PrefixUnaryExpressionSyntax hat when hat.IsKind(SyntaxKind.IndexExpression)
                                             && hat.Parent is not RangeExpressionSyntax =>
            "Dieser Index vom Ende (^n) wird nicht unterstützt — ^ geht bei Strings, Listen und Arrays auf einer einfachen Variable/einem Feld (s[^1]); sonst s[s.Length - 1] schreiben.",

        WithExpressionSyntax =>
            "Dieser with-Ausdruck wird nicht unterstützt — with geht bei Records mit Positionsparametern auf einer einfachen Variable/einem Feld und nur mit diesen Parametern; sonst eine Kopie mit new anlegen: new Punkt(10, p.Y).",

        // Plain generic methods are specialised beforehand (GenericMethodMonomorphizer);
        // what reaches this point is in a generic class or interface.
        MethodDeclarationSyntax { TypeParameterList: not null } m =>
            $"Diese generische Methode ({m.Identifier.Text}<…>) wird nicht unterstützt — generische Methoden in generischen Klassen/Interfaces gehen nicht; die Methode in eine nicht-generische Klasse verschieben.",

        StructDeclarationSyntax { TypeParameterList: not null } s =>
            $"Dieser generische Struct ({s.Identifier.Text}<…>) wird nicht unterstützt — generische Structs innerhalb generischer Typen gehen nicht.",

        LocalFunctionStatementSyntax f =>
            $"Die lokale Funktion {f.Identifier.Text} verwendet lokale Variablen oder Parameter der umgebenden Methode (oder ist generisch) — das wird nicht unterstützt; die Werte als Parameter übergeben oder eine private Methode schreiben.",

        QueryExpressionSyntax =>
            "LINQ-Abfragesyntax (from … select) wird nicht unterstützt — die Methodenschreibweise verwenden: list.Where(x => …).Select(x => …).",

        DelegateDeclarationSyntax d =>
            $"Der delegate-Typ {d.Identifier.Text} wird nicht unterstützt (ref/out/params-Parameter oder generisch) — Action<…> oder Func<…> verwenden.",

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
            "Dieser Collection-Ausdruck wird nicht unterstützt — [a, b] geht für Arrays, List/HashSet (und leer für Stack/Queue), ohne ..spread; sonst new List<int> { 1, 2, 3 } verwenden.",

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
