// Datei: Transpiler/Handlers/FileHandler.cs  — vollständig ersetzen

using Microsoft.CodeAnalysis.CSharp.Syntax;
using CS2SX.Core;

namespace CS2SX.Transpiler.Handlers;

public sealed class FileHandler : InvocationHandlerBase
{
    private static readonly Dictionary<string, string> s_map =
        new(StringComparer.Ordinal)
        {
            ["File.ReadAllText"] = "CS2SX_File_ReadAllText",
            ["File.WriteAllText"] = "CS2SX_File_WriteAllText",
            ["File.AppendAllText"] = "CS2SX_File_AppendAllText",
            ["File.Exists"] = "CS2SX_File_Exists",
            ["File.Delete"] = "CS2SX_File_Delete",
            ["File.Copy"] = "CS2SX_File_Copy",
            ["File.CopyBinary"] = "CS2SX_File_CopyBinary",
            ["File.ReadAllLines"] = "CS2SX_File_ReadAllLines",
            ["File.GetSize"] = "CS2SX_File_GetSize",
            ["File.GetModifiedTime"] = "CS2SX_File_GetMTime",
            ["File.GetModifiedString"] = "CS2SX_File_GetModifiedString",
            ["File.ReadHexDump"] = "CS2SX_File_ReadHexDump",
            ["File.CopyBegin"] = "CS2SX_File_CopyBegin",
            ["File.CopyStep"] = "CS2SX_File_CopyStep",
            ["File.CopyEnd"] = "CS2SX_File_CopyEnd",
            ["File.Rename"] = "CS2SX_File_Rename",

            ["CS2SX.Switch.File.ReadAllText"] = "CS2SX_File_ReadAllText",
            ["CS2SX.Switch.File.WriteAllText"] = "CS2SX_File_WriteAllText",
            ["CS2SX.Switch.File.AppendAllText"] = "CS2SX_File_AppendAllText",
            ["CS2SX.Switch.File.Exists"] = "CS2SX_File_Exists",
            ["CS2SX.Switch.File.Delete"] = "CS2SX_File_Delete",
            ["CS2SX.Switch.File.Copy"] = "CS2SX_File_Copy",
            ["CS2SX.Switch.File.ReadAllLines"] = "CS2SX_File_ReadAllLines",

            ["Directory.Exists"] = "CS2SX_Dir_Exists",
            ["Directory.CreateDirectory"] = "CS2SX_Dir_Create",
            ["Directory.Delete"] = "CS2SX_Dir_Delete",
            ["Directory.GetFiles"] = "CS2SX_Dir_GetFiles",
            ["Directory.GetCurrentDirectory"] = "CS2SX_Dir_GetCurrent",
            ["Directory.GetDirectories"] = "CS2SX_Dir_GetDirectories",
            ["Directory.GetEntries"] = "CS2SX_Dir_GetEntries",
            ["Directory.DeleteRecursive"] = "CS2SX_Dir_DeleteRecursive",
            ["Directory.Rename"] = "CS2SX_Dir_Rename",

            ["Filesystem.GetFreeSpace"] = "CS2SX_Fs_GetFreeSpace",
            ["Filesystem.GetTotalSpace"] = "CS2SX_Fs_GetTotalSpace",
            ["Filesystem.DirSize"] = "CS2SX_Fs_DirSize",

            ["Archive.Extract"] = "CS2SX_Zip_Extract",
            ["Archive.Compress"] = "CS2SX_Zip_Compress",
            ["Archive.BeginExtract"] = "CS2SX_Zip_BeginExtract",
            ["Archive.BeginCompress"] = "CS2SX_Zip_BeginCompress",
            ["Archive.Step"] = "CS2SX_Zip_Step",
            ["Archive.StepBudget"] = "CS2SX_Zip_StepBudget",
            ["Archive.Busy"] = "CS2SX_Zip_Busy",
            ["Archive.Result"] = "CS2SX_Zip_Result",
            ["Archive.Progress"] = "CS2SX_Zip_Progress",
            ["Archive.Total"] = "CS2SX_Zip_Total",
            ["Archive.Cancel"] = "CS2SX_Zip_Cancel",

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