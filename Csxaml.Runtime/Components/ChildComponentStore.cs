namespace Csxaml.Runtime;

internal sealed class ChildComponentStore
{
    private Dictionary<string, ComponentInstance> _current = new();
    private Dictionary<string, int> _positionOccurrences = new();
    private Dictionary<string, ComponentInstance> _previous = new();

    public void BeginRenderPass()
    {
        _current = new Dictionary<string, ComponentInstance>();
        _positionOccurrences = new Dictionary<string, int>();
    }

    public void CompleteRenderPass()
    {
        _previous = _current;
    }

    public ComponentInstance Resolve(ComponentNode node)
    {
        var matchKey = ComponentMatchKey.Create(node, _positionOccurrences);
        if (_current.ContainsKey(matchKey.Value))
        {
            throw new InvalidOperationException(
                $"Duplicate child component identity '{matchKey.Value}'.");
        }

        if (!_previous.TryGetValue(matchKey.Value, out var instance))
        {
            instance = CreateComponentInstance(node.ComponentType);
        }

        _current[matchKey.Value] = instance;
        return instance;
    }

    private static ComponentInstance CreateComponentInstance(Type componentType)
    {
        if (Activator.CreateInstance(componentType) is not ComponentInstance instance)
        {
            throw new InvalidOperationException(
                $"Type '{componentType.FullName}' is not a component instance.");
        }

        return instance;
    }
}
