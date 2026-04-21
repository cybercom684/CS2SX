// Datei: Transpiler/Handlers/DictionaryHandler.cs
// VOLLSTÄNDIGE DATEI ERSETZEN

using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

public sealed class DictionaryHandler : InvocationHandlerBase
{
    private static readonly HashSet<string> s_methods = new(StringComparer.Ordinal)
    {
        "Add", "Remove", "ContainsKey", "TryGetValue", "Clear",
    };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr, out string result)
    {
        if (inv.Expression is not MemberAccessExpressionSyntax mem
            || !s_methods.Contains(mem.Name.Identifier.Text))
            return NotHandled(out result);

        var objStr = mem.Expression.ToString();

        if (!TryResolveDict(objStr, ctx, out var dictType, out var dictExpr))
            return NotHandled(out result);

        var cDict = DictFuncPrefix(dictType);
        var method = mem.Name.Identifier.Text;

        if (method == "TryGetValue" && args.Count >= 2)
        {
            // FIX: out-Variable muss vorab deklariert werden, damit sie als Expression
            // (z.B. in return-Statement oder ternärem Ausdruck) funktioniert.
            // Wir ermitteln den Value-Typ aus dem Dict-Typ und emittieren eine lokale Variable.
            var types = TypeRegistry.GetDictionaryTypes(dictType);
            string outVarName;
            string outVarCType;

            // Prüfen ob das zweite Argument eine noch-nicht-deklarierte out-Variable ist
            var secondArg = inv.ArgumentList.Arguments.Count >= 2
                ? inv.ArgumentList.Arguments[1]
                : null;

            bool needsLocalDecl = false;
            if (secondArg != null
                && secondArg.RefKindKeyword.IsKind(
                    Microsoft.CodeAnalysis.CSharp.SyntaxKind.OutKeyword)
                && secondArg.Expression is
                    Microsoft.CodeAnalysis.CSharp.Syntax.DeclarationExpressionSyntax declExpr
                && declExpr.Designation is
                    Microsoft.CodeAnalysis.CSharp.Syntax.SingleVariableDesignationSyntax desig)
            {
                outVarName = desig.Identifier.Text;
                needsLocalDecl = true;

                // Typ aus dem Dict ableiten
                if (types.HasValue)
                {
                    var valCsType = types.Value.val;
                    outVarCType = valCsType == "string"
                        ? "const char*"
                        : TypeRegistry.MapType(valCsType);
                }
                else
                {
                    outVarCType = "void*";
                }

                // Lokale Variable deklarieren bevor sie als Ausdruck verwendet wird
                if (!ctx.LocalTypes.ContainsKey(outVarName))
                {
                    ctx.WriteLine(outVarCType + " " + outVarName + " = "
                        + (outVarCType == "const char*" ? "NULL" : "0") + ";");
                    ctx.LocalTypes[outVarName] = types.HasValue ? types.Value.val : "int";
                }

                result = cDict + "_TryGetValue("
                    + dictExpr + ", "
                    + args[0] + ", "
                    + "&" + outVarName + ")";
                return true;
            }

            // Normaler Fall: args[1] ist bereits ein Ausdruck (z.B. &myVar)
            var outPtr = args[1].StartsWith("&") ? args[1] : "&" + args[1];
            result = cDict + "_TryGetValue(" + dictExpr + ", " + args[0] + ", " + outPtr + ")";
            return true;
        }

        result = method switch
        {
            "Add" => cDict + "_Add(" + dictExpr + ", " + ArgAt(args, 0) + ", " + ArgAt(args, 1) + ")",
            "Remove" => cDict + "_Remove(" + dictExpr + ", " + ArgAt(args, 0) + ")",
            "ContainsKey" => cDict + "_ContainsKey(" + dictExpr + ", " + ArgAt(args, 0) + ")",
            "Clear" => cDict + "_Clear(" + dictExpr + ")",
            _ => dictExpr + "->" + method + "(" + JoinArgs(args) + ")",
        };
        return true;
    }
}