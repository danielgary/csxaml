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
        DisposeRemovedComponents();
        _previous = _current;
    }

    public void AbortRenderPass()
    {
        DisposeNewComponents();
        _current = new Dictionary<string, ComponentInstance>();
        _positionOccurrences = new Dictionary<string, int>();
    }

    public ComponentInstance Resolve(
        ComponentNode node,
        ComponentContext context,
        IComponentActivator activator)
    {
        var matchKey = ComponentMatchKey.Create(node, _positionOccurrences);
        if (_current.ContainsKey(matchKey.Value))
        {
            throw new InvalidOperationException(
                $"Duplicate child component identity '{matchKey.Value}'.");
        }

        if (!_previous.TryGetValue(matchKey.Value, out var instance))
        {
            instance = activator.CreateComponent(node.ComponentType, context);
        }

        instance.Initialize(context);
        _current[matchKey.Value] = instance;
        return instance;
    }

    public void DisposeAll()
    {
        foreach (var component in EnumerateDistinctComponents())
        {
            ComponentDisposer.Dispose(component);
        }

        _current = new Dictionary<string, ComponentInstance>();
        _previous = new Dictionary<string, ComponentInstance>();
        _positionOccurrences = new Dictionary<string, int>();
    }

    public async ValueTask DisposeAllAsync()
    {
        foreach (var component in EnumerateDistinctComponents())
        {
            await ComponentDisposer.DisposeAsync(component);
        }

        _current = new Dictionary<string, ComponentInstance>();
        _previous = new Dictionary<string, ComponentInstance>();
        _positionOccurrences = new Dictionary<string, int>();
    }

    private void DisposeRemovedComponents()
    {
        foreach (var pair in _previous)
        {
            if (_current.ContainsKey(pair.Key))
            {
                continue;
            }

            pair.Value.MarkUnmounted();
            ComponentDisposer.Dispose(pair.Value);
        }
    }

    private void DisposeNewComponents()
    {
        foreach (var pair in _current)
        {
            if (_previous.ContainsKey(pair.Key))
            {
                continue;
            }

            ComponentDisposer.Dispose(pair.Value);
        }
    }

    private IReadOnlyList<ComponentInstance> EnumerateDistinctComponents()
    {
        var seen = new HashSet<ComponentInstance>(ReferenceEqualityComparer.Instance);
        var values = new List<ComponentInstance>();

        foreach (var component in _previous.Values.Concat(_current.Values))
        {
            if (seen.Add(component))
            {
                values.Add(component);
            }
        }

        return values;
    }
}
