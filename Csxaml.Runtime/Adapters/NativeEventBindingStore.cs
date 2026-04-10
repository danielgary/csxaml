namespace Csxaml.Runtime;

internal sealed class NativeEventBindingStore
{
    private readonly Dictionary<string, Action> _unbinders = new(StringComparer.Ordinal);

    public void Clear()
    {
        foreach (var unbind in _unbinders.Values)
        {
            unbind();
        }

        _unbinders.Clear();
    }

    public void Rebind<TDelegate>(
        string name,
        TDelegate? handler,
        Func<TDelegate, Action> bind)
        where TDelegate : Delegate
    {
        if (_unbinders.Remove(name, out var existing))
        {
            existing();
        }

        if (handler is null)
        {
            return;
        }

        _unbinders[name] = bind(handler);
    }
}
