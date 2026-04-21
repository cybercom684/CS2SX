// ============================================================================
// Transpiler/CSharpToC — Fix für return $"..." in string-Methoden
//
// Problem:
//   private string FormatFileSize(long bytes)
//   {
//       return $"{bytes} B";   // → snprintf(_sb0, ...) + return _sb0;
//   }                          //   _sb0 ist stack-lokal → UB!
//
// Fix:
// In VisitMethodDeclaration(), wenn die Methode "string" zurückgibt und
// mindestens ein return-Statement einen interpolierten String enthält →
// _static_ret_buf als statische Variable am Methodenanfang deklarieren,
// und alle BuildToBuffer-Aufrufe nutzen diesen Buffer.
//
// Umsetzung: In CSharpToC.VisitMethodDeclaration() nach der Signatur-Ausgabe,
// VOR dem Block-Inhalt:
//
//    if (csReturnType == "string" && HasInterpolatedReturn(node))
//    {
//        _ctx.WriteLine("static char _ret_buf[512];");
//        _ctx.CurrentReturnBuffer = "_ret_buf";
//    }
//
// Und in FormatStringBuilder.BuildToBuffer():
//    // Wenn ctx.CurrentReturnBuffer gesetzt ist → diesen Buffer nutzen
//    var buf = !string.IsNullOrEmpty(ctx.CurrentReturnBuffer)
//        ? ctx.CurrentReturnBuffer
//        : ctx.NextStringBuf();
//
// In TranspilerContext ergänzen:
//    public string? CurrentReturnBuffer { get; set; }
//
// Und in ClearMethodContext():
//    CurrentReturnBuffer = null;
// ============================================================================

// ── Vollständige Implementierung der Hilfsmethode ────────────────────────────

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace CS2SX.Transpiler;

public static class ReturnStringFixHelper
{
    /// <summary>
    /// Prüft ob eine Methode mindestens ein return-Statement mit interpoliertem String hat.
    /// Wenn ja, braucht sie einen statischen Rückgabepuffer.
    /// </summary>
    public static bool HasInterpolatedStringReturn(MethodDeclarationSyntax method)
    {
        var returnType = method.ReturnType.ToString().Trim();
        if (returnType != "string") return false;

        // Suche nach return $"..." oder return string.Format(...)
        return method.DescendantNodes()
            .OfType<ReturnStatementSyntax>()
            .Any(ret => ret.Expression is InterpolatedStringExpressionSyntax
                     || (ret.Expression is InvocationExpressionSyntax inv
                         && inv.Expression.ToString() is "string.Format" or "String.Format"));
    }
}