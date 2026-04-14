namespace Csxaml.Generator;

internal sealed record MarkupNode(
    MarkupTagName Tag,
    IReadOnlyList<PropertyNode> Properties,
    IReadOnlyList<ChildNode> Children,
    TextSpan Span) : ChildNode(Span)
{
    public string TagName => Tag.Text;
}
