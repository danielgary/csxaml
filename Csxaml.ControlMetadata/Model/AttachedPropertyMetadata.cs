namespace Csxaml.ControlMetadata;

public sealed record AttachedPropertyMetadata(
    string OwnerName,
    string PropertyName,
    string ClrOwnerTypeName,
    string ClrTypeName,
    ValueKindHint ValueKindHint,
    string? RequiredParentTagName)
{
    public string QualifiedName => $"{OwnerName}.{PropertyName}";
}
