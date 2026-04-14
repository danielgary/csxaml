namespace Csxaml.ControlMetadata;

public static class AttachedPropertyMetadataRegistry
{
    private static readonly IReadOnlyDictionary<string, AttachedPropertyMetadata> PropertiesByQualifiedName =
        GeneratedAttachedPropertyMetadata.All.ToDictionary(
            property => property.QualifiedName,
            StringComparer.Ordinal);

    public static IReadOnlyList<AttachedPropertyMetadata> Properties => GeneratedAttachedPropertyMetadata.All;

    public static AttachedPropertyMetadata GetProperty(string ownerName, string propertyName)
    {
        if (!TryGetProperty(ownerName, propertyName, out var property))
        {
            throw new InvalidOperationException(
                $"Unsupported attached property '{ownerName}.{propertyName}'.");
        }

        return property!;
    }

    public static bool TryGetProperty(
        string ownerName,
        string propertyName,
        out AttachedPropertyMetadata? property)
    {
        return PropertiesByQualifiedName.TryGetValue(
            BuildQualifiedName(ownerName, propertyName),
            out property);
    }

    private static string BuildQualifiedName(string ownerName, string propertyName)
    {
        return $"{ownerName}.{propertyName}";
    }
}
