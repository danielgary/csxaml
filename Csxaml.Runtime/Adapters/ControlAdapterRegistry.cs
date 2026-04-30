namespace Csxaml.Runtime;

internal sealed class ControlAdapterRegistry
{
    private readonly IReadOnlyDictionary<string, INativeControlAdapter> _adapters;
    private readonly Dictionary<string, INativeControlAdapter> _externalAdapters =
        new(StringComparer.Ordinal);
    private readonly object _gate = new();

    public ControlAdapterRegistry()
        : this(
        [
            new BorderControlAdapter(),
            new AutoSuggestBoxControlAdapter(),
            new ButtonControlAdapter(),
            new CanvasControlAdapter(),
            new CheckBoxControlAdapter(),
            new FrameControlAdapter(),
            new GridControlAdapter(),
            new ListViewControlAdapter(),
            new RelativePanelControlAdapter(),
            new ScrollViewerControlAdapter(),
            new SliderControlAdapter(),
            new StackPanelControlAdapter(),
            new TextBlockControlAdapter(),
            new TextBoxControlAdapter(),
            new VariableSizedWrapGridControlAdapter()
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
            return GetExternal(tagName);
        }

        return adapter;
    }

    private INativeControlAdapter GetExternal(string tagName)
    {
        lock (_gate)
        {
            if (_externalAdapters.TryGetValue(tagName, out var cached))
            {
                return cached;
            }

            if (!ExternalControlRegistry.TryGet(tagName, out var descriptor))
            {
                throw new InvalidOperationException(
                    $"Unsupported native control tag '{tagName}'.");
            }

            var adapter = new ExternalControlAdapter(descriptor!);
            _externalAdapters.Add(tagName, adapter);
            return adapter;
        }
    }
}
