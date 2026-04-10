namespace Csxaml.Generator;

internal sealed record IfBlockNode(
    string ConditionExpression,
    IReadOnlyList<ChildNode> Children,
    TextSpan Span) : ChildNode(Span);
