namespace Csxaml.Generator;

internal sealed record PropertyNode(
    string Name,
    string? OwnerName,
    string PropertyName,
    PropertyValueKind ValueKind,
    string ValueText,
    TextSpan Span)
{
    public bool IsAttached => OwnerName is not null;
}
