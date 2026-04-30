namespace Csxaml.Generator;

internal sealed record PropertyContentNode(
    string OwnerName,
    string PropertyName,
    IReadOnlyList<PropertyNode> Properties,
    ElementRefNode? Ref,
    IReadOnlyList<ChildNode> Children,
    TextSpan Span) : ChildNode(Span)
{
    public string Name => $"{OwnerName}.{PropertyName}";
}
