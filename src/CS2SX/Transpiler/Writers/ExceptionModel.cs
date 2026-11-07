using CS2SX.Core;
using Microsoft.CodeAnalysis;

namespace CS2SX.Transpiler.Writers;

/// <summary>
/// How System exception types map onto the runtime (cs2sx_exc.h): every System
/// exception is the C struct <c>Exception</c> with its own type info, created by
/// <c>CS2SX_Exc_New(&amp;CS2SX_TI_T, message, paramName, inner)</c>. Constructor
/// arguments are matched by parameter name, since the overloads differ per type
/// (ArgumentNullException(string paramName) vs. Exception(string message)).
/// </summary>
internal static class ExceptionModel
{
    public static (string Message, string ParamName, string Inner) MapCtorArgs(
        IMethodSymbol? ctor, IReadOnlyList<string> args)
    {
        string msg = "NULL", param = "NULL", inner = "NULL";
        for (int i = 0; i < args.Count; i++)
        {
            var name = ctor != null && i < ctor.Parameters.Length ? ctor.Parameters[i].Name
                : i == 0 ? "message" : i == 1 ? "innerException" : "";
            switch (name)
            {
                case "message": msg = args[i]; break;
                case "paramName": param = args[i]; break;
                case "innerException": inner = "(Exception*)(" + args[i] + ")"; break;
            }
        }
        return (msg, param, inner);
    }

    public static string NewExpr(string typeName, IMethodSymbol? ctor, IReadOnlyList<string> args)
    {
        var (msg, param, inner) = MapCtorArgs(ctor, args);
        return $"CS2SX_Exc_New(&{TypeRegistry.ExceptionTypeInfo(typeName)}, {msg}, {param}, {inner})";
    }

    public static string InitCall(string selfExpr, IMethodSymbol? ctor, IReadOnlyList<string> args)
    {
        var (msg, param, inner) = MapCtorArgs(ctor, args);
        return $"CS2SX_Exc_Init((Exception*)({selfExpr}), {msg}, {param}, {inner})";
    }

    /// Type-info definition for a user exception class (in its .c file).
    public static string TypeInfoDefinition(string className, string baseName) =>
        $"const CS2SX_TypeInfo {TypeRegistry.ExceptionTypeInfo(className)} = "
        + $"{{ \"{className}\", &{TypeRegistry.ExceptionTypeInfo(baseName)}, "
        + $"\"Exception of type '{className}' was thrown.\" }};";
}
