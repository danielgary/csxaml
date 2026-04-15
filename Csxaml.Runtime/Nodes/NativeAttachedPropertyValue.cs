namespace Csxaml.Runtime;

public sealed record NativeAttachedPropertyValue(
    string OwnerName,
    string PropertyName,
    object? Value,
    ValueKindHint ValueKindHint,
    CsxamlSourceInfo? SourceInfo = null)
{
    public string QualifiedName => $"{OwnerName}.{PropertyName}";
}
