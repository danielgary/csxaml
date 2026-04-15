namespace Csxaml.Generator;

internal sealed record IfBlockNode(
    string ConditionExpression,
    TextSpan ConditionSpan,
    IReadOnlyList<ChildNode> Children,
    TextSpan Span) : ChildNode(Span);
