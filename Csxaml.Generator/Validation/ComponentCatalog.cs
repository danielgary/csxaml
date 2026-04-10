namespace Csxaml.Generator;

internal sealed class ComponentCatalog
{
    private readonly Dictionary<string, ParsedComponent> _components;

    public ComponentCatalog(Dictionary<string, ParsedComponent> components)
    {
        _components = components;
    }

    public bool Contains(string componentName)
    {
        return _components.ContainsKey(componentName);
    }

    public ComponentDefinition GetComponent(string componentName)
    {
        return _components[componentName].Definition;
    }
}
