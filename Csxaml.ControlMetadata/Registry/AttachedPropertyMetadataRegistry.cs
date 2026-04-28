namespace Csxaml.ControlMetadata;

/// <summary>
/// Provides lookup access to the generated attached-property metadata table.
/// </summary>
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

    /// <summary>
    /// Gets all attached properties known to the current metadata table.
    /// </summary>
    public static IReadOnlyList<AttachedPropertyMetadata> Properties => GeneratedAttachedPropertyMetadata.All;

    /// <summary>
    /// Gets all attached properties with the specified property name, regardless of owner.
    /// </summary>
    /// <param name="propertyName">The unqualified attached property name to search for.</param>
    /// <returns>The matching attached properties, or an empty list when none are known.</returns>
    public static IReadOnlyList<AttachedPropertyMetadata> GetPropertiesByName(string propertyName)
    {
        return PropertiesByName.TryGetValue(propertyName, out var properties)
            ? properties
            : EmptyProperties;
    }

    /// <summary>
    /// Gets metadata for the attached property with the specified owner and property name.
    /// </summary>
    /// <param name="ownerName">The simple owner name used in markup.</param>
    /// <param name="propertyName">The unqualified attached property name.</param>
    /// <returns>The matching attached property metadata.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the attached property is not known.</exception>
    public static AttachedPropertyMetadata GetProperty(string ownerName, string propertyName)
    {
        if (!TryGetProperty(ownerName, propertyName, out var property))
        {
            throw new InvalidOperationException(
                $"Unsupported attached property '{ownerName}.{propertyName}'.");
        }

        return property!;
    }

    /// <summary>
    /// Attempts to get metadata for the attached property with the specified owner and property name.
    /// </summary>
    /// <param name="ownerName">The simple owner name used in markup.</param>
    /// <param name="propertyName">The unqualified attached property name.</param>
    /// <param name="property">The matching attached property metadata when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> when the attached property is known; otherwise, <see langword="false"/>.</returns>
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
