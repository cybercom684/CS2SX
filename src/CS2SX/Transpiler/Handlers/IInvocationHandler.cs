using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Interface für pluggable Methoden-Aufruf-Handler.
///
/// TryHandle kombiniert CanHandle + Handle in einem Aufruf —
/// der Handler kann intern cachen was er beim Erkennen berechnet hat.
///
/// Erweiterung: Neuen Handler implementieren + in InvocationDispatcher registrieren.
/// </summary>
public interface IInvocationHandler
{
    /// <summary>
    /// Versucht den Aufruf zu behandeln.
    /// Gibt true zurück wenn der Handler zuständig ist, und setzt result auf den C-Code.
    /// Gibt false zurück wenn der Handler nicht zuständig ist — result ist dann null.
    /// </summary>
    bool TryHandle(
        InvocationExpressionSyntax inv,
        string calleeStr,
        List<string> args,
        TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr,
        out string result);
}