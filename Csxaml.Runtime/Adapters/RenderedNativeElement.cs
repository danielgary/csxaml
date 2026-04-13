namespace Csxaml.Runtime;

internal sealed class RenderedNativeElement : IDisposable
{
    private IReadOnlyList<RenderedNativeElement> _children = Array.Empty<RenderedNativeElement>();

    public RenderedNativeElement(
        string tagName,
        string? key,
        object element,
        INativeControlAdapter adapter)
    {
        TagName = tagName;
        Key = key;
        Element = element;
        Adapter = adapter;
    }

    public INativeControlAdapter Adapter { get; }

    public IReadOnlyList<RenderedNativeElement> Children => _children;

    public object Element { get; }

    public NativeEventBindingStore EventBindings { get; } = new();

    public string? Key { get; private set; }

    public string TagName { get; }

    public void Dispose()
    {
        EventBindings.Clear();
        foreach (var child in _children)
        {
            child.Dispose();
        }
    }

    public void ReplaceChildren(IReadOnlyList<RenderedNativeElement> children)
    {
        _children = children;
    }

    public void UpdateKey(string? key)
    {
        Key = key;
    }
}
