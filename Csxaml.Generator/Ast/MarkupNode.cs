namespace Csxaml.Generator;

internal sealed record MarkupNode(
    MarkupTagName Tag,
    IReadOnlyList<PropertyNode> Properties,
    ElementRefNode? Ref,
    IReadOnlyList<PropertyContentNode> PropertyContent,
    IReadOnlyList<ChildNode> Children,
    TextSpan Span) : ChildNode(Span)
{
    public MarkupNode(
        MarkupTagName Tag,
        IReadOnlyList<PropertyNode> Properties,
        ElementRefNode? Ref,
        IReadOnlyList<ChildNode> Children,
        TextSpan Span)
        : this(
            Tag,
            Properties,
            Ref,
            Array.Empty<PropertyContentNode>(),
            Children,
            Span)
    {
    }

    public string TagName => Tag.Text;
}
