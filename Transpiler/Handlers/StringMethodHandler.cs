using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;
using CS2SX.Transpiler.Writers;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Behandelt string-Methoden (statisch und Instanz).
///
/// Statisch:  string.IsNullOrEmpty, string.Contains, string.Format etc.
/// Instanz:   s.Contains("x"), s.StartsWith("x"), s.ToString(), x.ToString() etc.
///
/// Erweiterung: Neuen case in s_staticMethods oder s_instanceMethods ergänzen.
/// </summary>
public sealed class StringMethodHandler : IInvocationHandler
{
    private static readonly HashSet<string> s_staticMethods = new(StringComparer.Ordinal)
    {
        "string.IsNullOrEmpty", "string.IsNullOrWhiteSpace",
        "string.Contains", "string.StartsWith", "string.EndsWith",
        "string.Format", "string.Concat",
        "String.IsNullOrEmpty", "String.IsNullOrWhiteSpace",
    };

    private static readonly HashSet<string> s_instanceMethods = new(StringComparer.Ordinal)
    {
        "Contains", "StartsWith", "EndsWith", "Equals", "ToString",
    };

    public bool CanHandle(InvocationExpressionSyntax inv, string calleeStr, TranspilerContext ctx)
    {
        // Statische string-Methoden
        if (s_staticMethods.Contains(calleeStr)) return true;

        // Instanz-Methoden auf string/int/float
        if (inv.Expression is MemberAccessExpressionSyntax mem
            && s_instanceMethods.Contains(mem.Name.Identifier.Text))
        {
            var objStr  = mem.Expression.ToString();
            var objKey  = objStr.TrimStart('_');
            string? type = null;
            ctx.LocalTypes.TryGetValue(objStr, out type);
            if (type == null) ctx.FieldTypes.TryGetValue(objKey, out type);

            // Nicht übernehmen wenn es ein StringBuilder oder List ist (andere Handler)
            if (TypeRegistry.IsStringBuilder(type ?? "") || TypeRegistry.IsList(type ?? ""))
                return false;

            return true;
        }

        return false;
    }

    public string Handle(InvocationExpressionSyntax inv, string calleeStr, List<string> args,
        TranspilerContext ctx, Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr)
    {
        // Statische Methoden
        switch (calleeStr)
        {
            case "string.IsNullOrEmpty":
            case "string.IsNullOrWhiteSpace":
            case "String.IsNullOrEmpty":
            case "String.IsNullOrWhiteSpace":
                return "String_IsNullOrEmpty(" + (args.Count > 0 ? args[0] : "\"\"") + ")";

            case "string.Contains":
                return "String_Contains(" + args[0] + ", " + (args.Count > 1 ? args[1] : "\"\"") + ")";

            case "string.StartsWith":
                return "String_StartsWith(" + args[0] + ", " + (args.Count > 1 ? args[1] : "\"\"") + ")";

            case "string.EndsWith":
                return "String_EndsWith(" + args[0] + ", " + (args.Count > 1 ? args[1] : "\"\"") + ")";
        }

        // Instanz-Methoden
        if (inv.Expression is MemberAccessExpressionSyntax mem)
        {
            var receiver     = writeExpr(mem.Expression);
            var methodName   = mem.Name.Identifier.Text;
            var receiverType = TypeInferrer.InferCSharpType(mem.Expression, ctx);

            switch (methodName)
            {
                case "ToString":
                    return receiverType switch
                    {
                        "uint" or "u32" => "UInt_ToString((unsigned int)" + receiver + ")",
                        "float"         => "Float_ToString(" + receiver + ")",
                        _               => "Int_ToString((int)" + receiver + ")",
                    };

                case "Contains":
                    return "String_Contains(" + receiver + ", " + (args.Count > 0 ? args[0] : "\"\"") + ")";

                case "StartsWith":
                    return "String_StartsWith(" + receiver + ", " + (args.Count > 0 ? args[0] : "\"\"") + ")";

                case "EndsWith":
                    return "String_EndsWith(" + receiver + ", " + (args.Count > 0 ? args[0] : "\"\"") + ")";

                case "Equals":
                    return "strcmp(" + receiver + ", " + (args.Count > 0 ? args[0] : "\"\"") + ") == 0";
            }
        }

        return args.Count > 0 ? args[0] : "\"\"";
    }
}
