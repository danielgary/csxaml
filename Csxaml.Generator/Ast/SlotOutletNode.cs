namespace Csxaml.Generator;

internal sealed record SlotOutletNode(
    IReadOnlyList<PropertyNode> Properties,
    IReadOnlyList<ChildNode> Children,
    TextSpan Span) : ChildNode(Span)
{
    public bool TryGetName(out string name)
    {
        var nameProperty = Properties.SingleOrDefault(
            property => string.Equals(property.Name, "Name", StringComparison.Ordinal));
        if (nameProperty is { ValueKind: PropertyValueKind.StringLiteral })
        {
            name = nameProperty.ValueText;
            return true;
        }

        name = string.Empty;
        return false;
    }
}
