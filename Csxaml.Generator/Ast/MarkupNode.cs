namespace Csxaml.Generator;

internal sealed record MarkupNode(
    string TagName,
    IReadOnlyList<PropertyNode> Properties,
    IReadOnlyList<ChildNode> Children,
    TextSpan Span) : ChildNode(Span);
