namespace Csxaml.Runtime;

internal sealed class NativeEventBindingStore
{
    private readonly Dictionary<string, EventBinding> _bindings = new(StringComparer.Ordinal);

    public void Clear()
    {
        foreach (var binding in _bindings.Values)
        {
            binding.Unbind();
        }

        _bindings.Clear();
    }

    public void Rebind<TDelegate>(
        string name,
        TDelegate? handler,
        Func<TDelegate, Action> bind)
        where TDelegate : Delegate
    {
        if (_bindings.TryGetValue(name, out var existing))
        {
            if (handler is not null && existing.Matches(handler))
            {
                return;
            }

            _bindings.Remove(name);
            existing.Unbind();
        }

        if (handler is null)
        {
            return;
        }

        _bindings[name] = new EventBinding(handler, bind(handler));
    }

    private sealed class EventBinding(Delegate handler, Action unbind)
    {
        public Action Unbind { get; } = unbind;

        public bool Matches(Delegate candidate)
        {
            return handler.Equals(candidate);
        }
    }
}
