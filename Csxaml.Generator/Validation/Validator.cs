namespace Csxaml.Generator;

internal sealed class Validator
{
    private readonly ComponentDefinitionValidator _componentDefinitionValidator = new();

    public ComponentCatalog Validate(IReadOnlyList<ParsedComponent> components)
    {
        var catalog = ComponentCatalogBuilder.Build(components);
        foreach (var component in components)
        {
            _componentDefinitionValidator.Validate(component, catalog);
        }

        return catalog;
    }
}
