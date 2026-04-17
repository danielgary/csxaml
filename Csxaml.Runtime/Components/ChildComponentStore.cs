namespace Csxaml.Runtime;

internal sealed class ChildComponentStore
{
    private Dictionary<string, ComponentInstance>? _current;
    private HashSet<string>? _explicitKeys;
    private Dictionary<string, int>? _positionOccurrences;
    private Dictionary<string, ComponentInstance>? _previous;

    public void BeginRenderPass()
    {
        _current?.Clear();
        _explicitKeys?.Clear();
        _positionOccurrences?.Clear();
    }

    public void CompleteRenderPass()
    {
        DisposeRemovedComponents();
        (_previous, _current) = (_current, _previous);
    }

    public void AbortRenderPass()
    {
        DisposeNewComponents();
        _current?.Clear();
        _explicitKeys?.Clear();
        _positionOccurrences?.Clear();
    }

    public ComponentInstance Resolve(
        ComponentNode node,
        ComponentContext context,
        IComponentActivator activator)
    {
        ValidateExplicitKey(node);

        var current = _current ??= new Dictionary<string, ComponentInstance>();
        var positionOccurrences = _positionOccurrences ??= new Dictionary<string, int>();
        var matchKey = ComponentMatchKey.Create(node, positionOccurrences);
        if (current.ContainsKey(matchKey.Value))
        {
            throw new InvalidOperationException(
                $"Duplicate child component identity '{matchKey.Value}'.");
        }

        if (_previous is null || !_previous.TryGetValue(matchKey.Value, out var instance))
        {
            instance = activator.CreateComponent(node.ComponentType, context);
        }

        instance.Initialize(context);
        current[matchKey.Value] = instance;
        return instance;
    }

    public void DisposeAll()
    {
        foreach (var component in EnumerateDistinctComponents())
        {
            ComponentDisposer.Dispose(component);
        }

        _current = null;
        _previous = null;
        _explicitKeys = null;
        _positionOccurrences = null;
    }

    public async ValueTask DisposeAllAsync()
    {
        foreach (var component in EnumerateDistinctComponents())
        {
            await ComponentDisposer.DisposeAsync(component);
        }

        _current = null;
        _previous = null;
        _explicitKeys = null;
        _positionOccurrences = null;
    }

    private void ValidateExplicitKey(ComponentNode node)
    {
        if (node.Key is null)
        {
            return;
        }

        _explicitKeys ??= new HashSet<string>(StringComparer.Ordinal);
        if (_explicitKeys.Add(node.Key))
        {
            return;
        }

        throw new InvalidOperationException(
            $"Sibling component elements cannot share the key '{node.Key}'.");
    }

    private void DisposeRemovedComponents()
    {
        if (_previous is null)
        {
            return;
        }

        foreach (var pair in _previous)
        {
            if (_current is not null && _current.ContainsKey(pair.Key))
            {
                continue;
            }

            pair.Value.MarkUnmounted();
            ComponentDisposer.Dispose(pair.Value);
        }
    }

    private void DisposeNewComponents()
    {
        if (_current is null)
        {
            return;
        }

        foreach (var pair in _current)
        {
            if (_previous is not null && _previous.ContainsKey(pair.Key))
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

        foreach (var component in EnumerateComponents())
        {
            if (seen.Add(component))
            {
                values.Add(component);
            }
        }

        return values;
    }

    private IEnumerable<ComponentInstance> EnumerateComponents()
    {
        if (_previous is not null)
        {
            foreach (var component in _previous.Values)
            {
                yield return component;
            }
        }

        if (_current is not null)
        {
            foreach (var component in _current.Values)
            {
                yield return component;
            }
        }
    }
}
