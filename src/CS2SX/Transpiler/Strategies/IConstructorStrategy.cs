using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using CS2SX.Transpiler.Writers;

namespace CS2SX.Transpiler.Strategies;

/// <summary>
/// Strategy-Pattern für die Konstruktor-Generierung.
/// Jede Strategie ist für eine bestimmte Klassen-Kategorie zuständig.
/// </summary>
public interface IConstructorStrategy
{
    /// <summary>
    /// True wenn diese Strategie für die gegebene Klasse zuständig ist.
    /// </summary>
    bool Matches(ClassDeclarationSyntax node, string baseType);

    /// <summary>
    /// Schreibt den vollständigen Konstruktor-Code für die Klasse.
    /// </summary>
    void Write(
        ClassDeclarationSyntax node,
        string name,
        string baseType,
        TranspilerContext ctx,
        ExpressionWriter exprWriter,
        CSharpToC transpiler);
}