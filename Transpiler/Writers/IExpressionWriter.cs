using Microsoft.CodeAnalysis;

namespace CS2SX.Transpiler.Writers;

public interface IExpressionWriter
{
    string Write(SyntaxNode? node);
}