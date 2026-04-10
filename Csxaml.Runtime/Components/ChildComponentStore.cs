namespace Csxaml.Runtime;

internal sealed class ChildComponentStore
{
    private Dictionary<string, ComponentInstance> _current = new();
    private Dictionary<string, ComponentInstance> _previous = new();
    private Dictionary<string, int> _slotOccurrences = new();

    public void BeginRenderPass()
    {
        _current = new Dictionary<string, ComponentInstance>();
        _slotOccurrences = new Dictionary<string, int>();
    }

    public void CompleteRenderPass()
    {
        _previous = _current;
    }

    public ComponentInstance Resolve(ComponentNode node)
    {
        var slot = ComponentSlot.Create(node, _slotOccurrences);
        if (_current.ContainsKey(slot.MatchKey))
        {
            throw new InvalidOperationException(
                $"Duplicate child component identity '{slot.MatchKey}'.");
        }

        if (!_previous.TryGetValue(slot.MatchKey, out var instance))
        {
            instance = CreateComponentInstance(node.ComponentType);
        }

        _current[slot.MatchKey] = instance;
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
