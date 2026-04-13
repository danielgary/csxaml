namespace Csxaml.Runtime;

internal sealed class ControlAdapterRegistry
{
    private readonly IReadOnlyDictionary<string, INativeControlAdapter> _adapters;

    public ControlAdapterRegistry()
        : this(
        [
            new BorderControlAdapter(),
            new ButtonControlAdapter(),
            new CheckBoxControlAdapter(),
            new StackPanelControlAdapter(),
            new TextBlockControlAdapter(),
            new TextBoxControlAdapter()
        ])
    {
    }

    internal ControlAdapterRegistry(IReadOnlyList<INativeControlAdapter> adapters)
    {
        _adapters = adapters.ToDictionary(adapter => adapter.TagName, StringComparer.Ordinal);
    }

    public INativeControlAdapter Get(string tagName)
    {
        if (!_adapters.TryGetValue(tagName, out var adapter))
        {
            throw new InvalidOperationException(
                $"Unsupported native control tag '{tagName}'.");
        }

        return adapter;
    }
}
