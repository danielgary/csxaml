namespace Csxaml.Generator;

internal sealed class ComponentCatalog
{
    private readonly IReadOnlyList<ComponentCatalogEntry> _entries;
    private readonly IReadOnlyDictionary<string, IReadOnlyList<ComponentCatalogEntry>> _entriesByQualifiedName;
    private readonly IReadOnlyDictionary<string, IReadOnlyList<ComponentCatalogEntry>> _entriesBySimpleName;

    public ComponentCatalog(IReadOnlyList<ComponentCatalogEntry> entries)
    {
        _entries = entries;
        _entriesBySimpleName = entries
            .GroupBy(entry => entry.Name, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<ComponentCatalogEntry>)group.ToList(),
                StringComparer.Ordinal);
        _entriesByQualifiedName = entries
            .GroupBy(entry => $"{entry.NamespaceName}.{entry.Name}", StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<ComponentCatalogEntry>)group.ToList(),
                StringComparer.Ordinal);
    }

    public IReadOnlyList<ComponentCatalogEntry> FindByName(string componentName)
    {
        return _entriesBySimpleName.TryGetValue(componentName, out var entries)
            ? entries
            : Array.Empty<ComponentCatalogEntry>();
    }

    public IReadOnlyList<ComponentCatalogEntry> FindByNamespaceAndName(
        string namespaceName,
        string componentName)
    {
        return _entriesByQualifiedName.TryGetValue(
            $"{namespaceName}.{componentName}",
            out var entries)
            ? entries
            : Array.Empty<ComponentCatalogEntry>();
    }

    public IReadOnlyList<ComponentCatalogEntry> FindLocalComponents()
    {
        return _entries.Where(entry => entry.IsLocal).ToList();
    }
}
