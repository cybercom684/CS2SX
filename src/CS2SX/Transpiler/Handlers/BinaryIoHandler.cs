using CS2SX.Core;
using CS2SX.Transpiler.Writers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

/// BinaryWriter.Write has one overload per type; in C each is its own function
/// (BinaryWriter_WriteInt32, …), chosen from the overload C# picked — a value of
/// type bool and one of type int are both ints in C.
public sealed class BinaryIoHandler : InvocationHandlerBase
{
    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx, Func<SyntaxNode?, string> writeExpr, out string result)
    {
        result = "";
        if (ctx.SemanticModel == null || inv.Expression is not MemberAccessExpressionSyntax ma) return false;
        IMethodSymbol? m;
        try { m = ctx.SemanticModel.GetSymbolInfo(inv).Symbol as IMethodSymbol; }
        catch { return false; }
        if (m is not { Name: "Write", ContainingType.Name: "BinaryWriter" }
            || m.ContainingNamespace?.ToDisplayString() != "CS2SX.Switch")
            return false;

        string? suffix = m.Parameters.Length == 3 ? "BytesRange" : m.Parameters[0].Type switch
        {
            IArrayTypeSymbol => "Bytes",
            { SpecialType: SpecialType.System_Byte } => "Byte",
            { SpecialType: SpecialType.System_SByte } => "SByte",
            { SpecialType: SpecialType.System_Boolean } => "Boolean",
            { SpecialType: SpecialType.System_Int16 } => "Int16",
            { SpecialType: SpecialType.System_UInt16 } => "UInt16",
            { SpecialType: SpecialType.System_Int32 } => "Int32",
            { SpecialType: SpecialType.System_UInt32 } => "UInt32",
            { SpecialType: SpecialType.System_Int64 } => "Int64",
            { SpecialType: SpecialType.System_UInt64 } => "UInt64",
            { SpecialType: SpecialType.System_Single } => "Single",
            { SpecialType: SpecialType.System_Double } => "Double",
            { SpecialType: SpecialType.System_String } => "String",
            _ => null,
        };
        if (suffix == null) return false;
        var receiver = NullCheck.Wrap(ma.Expression, writeExpr(ma.Expression), ctx);
        result = "BinaryWriter_Write" + suffix + "(" + receiver + (args.Count > 0 ? ", " + JoinArgs(args) : "") + ")";
        return true;
    }
}
