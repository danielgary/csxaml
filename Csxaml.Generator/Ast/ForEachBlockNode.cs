namespace Csxaml.Generator;

internal sealed record ForEachBlockNode(
    string ItemName,
    string CollectionExpression,
    IReadOnlyList<ChildNode> Children,
    TextSpan Span) : ChildNode(Span);
