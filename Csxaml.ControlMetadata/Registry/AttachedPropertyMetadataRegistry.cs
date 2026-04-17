namespace Csxaml.ControlMetadata;

public static class AttachedPropertyMetadataRegistry
{
    private static readonly IReadOnlyList<AttachedPropertyMetadata> EmptyProperties =
        Array.Empty<AttachedPropertyMetadata>();
    private static readonly IReadOnlyDictionary<string, AttachedPropertyMetadata> PropertiesByQualifiedName =
        GeneratedAttachedPropertyMetadata.All.ToDictionary(
            property => property.QualifiedName,
            StringComparer.Ordinal);
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<AttachedPropertyMetadata>> PropertiesByName =
        BuildPropertiesByName();

    public static IReadOnlyList<AttachedPropertyMetadata> Properties => GeneratedAttachedPropertyMetadata.All;

    public static IReadOnlyList<AttachedPropertyMetadata> GetPropertiesByName(string propertyName)
    {
        return PropertiesByName.TryGetValue(propertyName, out var properties)
            ? properties
            : EmptyProperties;
    }

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

    private static IReadOnlyDictionary<string, IReadOnlyList<AttachedPropertyMetadata>> BuildPropertiesByName()
    {
        var propertiesByName =
            new Dictionary<string, List<AttachedPropertyMetadata>>(StringComparer.Ordinal);
        foreach (var property in GeneratedAttachedPropertyMetadata.All)
        {
            if (!propertiesByName.TryGetValue(property.PropertyName, out var properties))
            {
                properties = new List<AttachedPropertyMetadata>();
                propertiesByName[property.PropertyName] = properties;
            }

            properties.Add(property);
        }

        var result =
            new Dictionary<string, IReadOnlyList<AttachedPropertyMetadata>>(StringComparer.Ordinal);
        foreach (var pair in propertiesByName)
        {
            result[pair.Key] = pair.Value;
        }

        return result;
    }
}
