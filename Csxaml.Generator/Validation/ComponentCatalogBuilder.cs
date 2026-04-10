namespace Csxaml.Generator;

internal static class ComponentCatalogBuilder
{
    public static ComponentCatalog Build(IReadOnlyList<ParsedComponent> components)
    {
        var entries = new Dictionary<string, ParsedComponent>(StringComparer.Ordinal);
        foreach (var component in components)
        {
            if (!entries.TryAdd(component.Definition.Name, component))
            {
                throw DiagnosticFactory.FromSpan(
                    component.Source,
                    component.Definition.Span,
                    $"component '{component.Definition.Name}' is defined more than once");
            }
        }

        return new ComponentCatalog(entries);
    }
}
