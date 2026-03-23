using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Interface für pluggable Methoden-Aufruf-Handler.
///
/// Jeder Handler ist für eine bestimmte Kategorie von Aufrufen zuständig:
/// LibNX, StringBuilder, List&lt;T&gt;, string-Methoden, Console, Input, Form etc.
///
/// Erweiterung: Neuen Handler implementieren + in InvocationDispatcher registrieren.
/// Keine Änderungen am bestehenden Code nötig.
/// </summary>
public interface IInvocationHandler
{
    /// <summary>
    /// True wenn dieser Handler den Aufruf behandeln kann.
    /// Wird von InvocationDispatcher in Reihenfolge der Priorität abgefragt.
    /// </summary>
    bool CanHandle(InvocationExpressionSyntax inv, string calleeStr, Core.TranspilerContext ctx);

    /// <summary>
    /// Transpiliert den Aufruf zu C-Code.
    /// Wird nur aufgerufen wenn CanHandle true zurückgegeben hat.
    /// </summary>
    string Handle(
        InvocationExpressionSyntax inv,
        string calleeStr,
        List<string> args,
        Core.TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr);
}
