using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

public sealed class FileHandler : InvocationHandlerBase
{
    private static readonly Dictionary<string, string> s_map =
        new(StringComparer.Ordinal)
        {
            // File — kurz und mit Namespace
            ["File.ReadAllText"] = "CS2SX_File_ReadAllText",
            ["File.WriteAllText"] = "CS2SX_File_WriteAllText",
            ["File.AppendAllText"] = "CS2SX_File_AppendAllText",
            ["File.Exists"] = "CS2SX_File_Exists",
            ["File.Delete"] = "CS2SX_File_Delete",
            ["File.Copy"] = "CS2SX_File_Copy",

            ["CS2SX.Switch.File.ReadAllText"] = "CS2SX_File_ReadAllText",
            ["CS2SX.Switch.File.WriteAllText"] = "CS2SX_File_WriteAllText",
            ["CS2SX.Switch.File.AppendAllText"] = "CS2SX_File_AppendAllText",
            ["CS2SX.Switch.File.Exists"] = "CS2SX_File_Exists",
            ["CS2SX.Switch.File.Delete"] = "CS2SX_File_Delete",
            ["CS2SX.Switch.File.Copy"] = "CS2SX_File_Copy",

            // Directory — kurz und mit Namespace
            ["Directory.Exists"] = "CS2SX_Dir_Exists",
            ["Directory.CreateDirectory"] = "CS2SX_Dir_Create",
            ["Directory.Delete"] = "CS2SX_Dir_Delete",
            ["Directory.GetFiles"] = "CS2SX_Dir_GetFiles",
            ["Directory.GetCurrentDirectory"] = "CS2SX_Dir_GetCurrent",

            ["CS2SX.Switch.Directory.Exists"] = "CS2SX_Dir_Exists",
            ["CS2SX.Switch.Directory.CreateDirectory"] = "CS2SX_Dir_Create",
            ["CS2SX.Switch.Directory.Delete"] = "CS2SX_Dir_Delete",
            ["CS2SX.Switch.Directory.GetFiles"] = "CS2SX_Dir_GetFiles",
            ["CS2SX.Switch.Directory.GetCurrentDirectory"] = "CS2SX_Dir_GetCurrent",
        };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<Microsoft.CodeAnalysis.SyntaxNode?, string> writeExpr, out string result)
    {
        if (!s_map.TryGetValue(calleeStr, out var cFunc))
            return NotHandled(out result);

        result = cFunc + "(" + JoinArgs(args) + ")";
        return true;
    }
}