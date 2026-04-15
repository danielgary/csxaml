namespace Csxaml.Generator;

internal sealed record ForEachBlockNode(
    string ItemName,
    string CollectionExpression,
    TextSpan CollectionSpan,
    IReadOnlyList<ChildNode> Children,
    TextSpan Span) : ChildNode(Span);
