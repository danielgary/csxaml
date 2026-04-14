namespace Csxaml.Runtime;

public sealed record NativeAttachedPropertyValue(
    string OwnerName,
    string PropertyName,
    object? Value,
    ValueKindHint ValueKindHint)
{
    public string QualifiedName => $"{OwnerName}.{PropertyName}";
}
