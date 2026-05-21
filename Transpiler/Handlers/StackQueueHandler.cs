using CS2SX.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CS2SX.Transpiler.Handlers;

/// <summary>
/// Handles Stack&lt;T&gt;, Queue&lt;T&gt;, and HashSet&lt;T&gt; method calls.
///
/// Stack:
///   Push(x)       → Stack_T_Push(s, x)
///   Pop()         → Stack_T_Pop(s)
///   Peek()        → Stack_T_Peek(s)
///   Clear()       → Stack_T_Clear(s)
///   Count         → (property) s->count
///
/// Queue:
///   Enqueue(x)    → Queue_T_Enqueue(q, x)
///   Dequeue()     → Queue_T_Dequeue(q)
///   Peek()        → Queue_T_Peek(q)
///   Clear()       → Queue_T_Clear(q)
///   Count         → (property) q->count
///
/// HashSet:
///   Add(x)        → HashSet_T_Add(h, x)
///   Contains(x)   → HashSet_T_Contains(h, x)
///   Remove(x)     → HashSet_T_Remove(h, x)
///   Clear()       → HashSet_T_Clear(h)
///   Count         → (property) h->count
/// </summary>
public sealed class StackQueueHandler : InvocationHandlerBase
{
    private static readonly HashSet<string> s_stackMethods = new(StringComparer.Ordinal)
        { "Push", "Pop", "Peek", "Clear" };

    private static readonly HashSet<string> s_queueMethods = new(StringComparer.Ordinal)
        { "Enqueue", "Dequeue", "Peek", "Clear" };

    private static readonly HashSet<string> s_hashSetMethods = new(StringComparer.Ordinal)
        { "Add", "Contains", "Remove", "Clear", "UnionWith", "IntersectWith", "ExceptWith" };

    public override bool TryHandle(InvocationExpressionSyntax inv, string calleeStr,
        List<string> args, TranspilerContext ctx,
        Func<SyntaxNode?, string> writeExpr, out string result)
    {
        if (inv.Expression is not MemberAccessExpressionSyntax mem)
            return NotHandled(out result);

        var method = mem.Name.Identifier.Text;
        var rawReceiver = mem.Expression.ToString();
        var receiverKey = rawReceiver.TrimStart('_');

        string? collType = null;
        ctx.LocalTypes.TryGetValue(rawReceiver, out collType);
        if (collType == null) ctx.FieldTypes.TryGetValue(receiverKey, out collType);
        if (collType == null) collType = ctx.GetSemanticType(mem.Expression);

        if (collType == null) return NotHandled(out result);

        var objStr = writeExpr(mem.Expression);

        // ── Stack<T> ──────────────────────────────────────────────────────────
        if (TypeRegistry.IsStack(collType) && s_stackMethods.Contains(method))
        {
            var inner = TypeRegistry.GetStackInnerType(collType) ?? "int";
            var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
            var prefix = "Stack_" + cInner;
            // Emit bounds-check assertion for Pop and Peek
            if (method is "Pop" or "Peek")
            {
                ctx.Out.WriteLine(ctx.Tab + "if (!" + objStr + " || " + objStr + "->count == 0) { fprintf(stderr, \"Stack underflow\\n\"); abort(); }");
            }
            result = method switch
            {
                "Push"  => prefix + "_Push(" + objStr + ", " + JoinArgs(args) + ")",
                "Pop"   => prefix + "_Pop("  + objStr + ")",
                "Peek"  => prefix + "_Peek(" + objStr + ")",
                "Clear" => prefix + "_Clear(" + objStr + ")",
                _       => objStr + "->" + method + "()"
            };
            return true;
        }

        // ── Queue<T> ──────────────────────────────────────────────────────────
        if (TypeRegistry.IsQueue(collType) && s_queueMethods.Contains(method))
        {
            var inner = TypeRegistry.GetQueueInnerType(collType) ?? "int";
            var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
            var prefix = "Queue_" + cInner;
            // Emit bounds-check assertion for Dequeue and Peek
            if (method is "Dequeue" or "Peek")
            {
                ctx.Out.WriteLine(ctx.Tab + "if (!" + objStr + " || " + objStr + "->count == 0) { fprintf(stderr, \"Queue underflow\\n\"); abort(); }");
            }
            result = method switch
            {
                "Enqueue" => prefix + "_Enqueue(" + objStr + ", " + JoinArgs(args) + ")",
                "Dequeue" => prefix + "_Dequeue(" + objStr + ")",
                "Peek"    => prefix + "_Peek("    + objStr + ")",
                "Clear"   => prefix + "_Clear("   + objStr + ")",
                _         => objStr + "->" + method + "()"
            };
            return true;
        }

        // ── HashSet<T> ────────────────────────────────────────────────────────
        if (TypeRegistry.IsHashSet(collType) && s_hashSetMethods.Contains(method))
        {
            var inner = TypeRegistry.GetHashSetInnerType(collType) ?? "int";
            var cInner = inner == "string" ? "str" : TypeRegistry.MapType(inner);
            var prefix = "HashSet_" + cInner;
            result = method switch
            {
                "Add"           => prefix + "_Add("          + objStr + ", " + JoinArgs(args) + ")",
                "Contains"      => prefix + "_Contains("     + objStr + ", " + JoinArgs(args) + ")",
                "Remove"        => prefix + "_Remove("       + objStr + ", " + JoinArgs(args) + ")",
                "Clear"         => prefix + "_Clear("        + objStr + ")",
                "UnionWith"     => prefix + "_UnionWith("    + objStr + ", " + JoinArgs(args) + ")",
                "IntersectWith" => prefix + "_IntersectWith("+ objStr + ", " + JoinArgs(args) + ")",
                "ExceptWith"    => prefix + "_ExceptWith("   + objStr + ", " + JoinArgs(args) + ")",
                _               => objStr + "->" + method + "()"
            };
            return true;
        }

        return NotHandled(out result);
    }
}
