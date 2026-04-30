namespace Csxaml.Runtime;

internal sealed class RenderedNativeElement : IDisposable
{
    private IReadOnlyList<RenderedNativeElement> _children = Array.Empty<RenderedNativeElement>();
    private IReadOnlyDictionary<string, IReadOnlyList<RenderedNativeElement>> _propertyContent =
        new Dictionary<string, IReadOnlyList<RenderedNativeElement>>(StringComparer.Ordinal);
    private NativeElementRefValue? _ref;

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

    public IReadOnlyDictionary<string, IReadOnlyList<RenderedNativeElement>> PropertyContent => _propertyContent;

    public string TagName { get; }

    public void Dispose()
    {
        EventBindings.Clear();
        _ref?.Reference.ClearIfCurrent(Element);
        _ref = null;
        foreach (var child in _children)
        {
            child.Dispose();
        }

        foreach (var children in _propertyContent.Values)
        {
            foreach (var child in children)
            {
                child.Dispose();
            }
        }
    }

    public void ApplyRef(NativeElementRefValue? nextRef)
    {
        if (!ReferenceEquals(_ref?.Reference, nextRef?.Reference))
        {
            _ref?.Reference.ClearIfCurrent(Element);
        }

        if (nextRef is null)
        {
            _ref = null;
            return;
        }

        if (!ReferenceEquals(nextRef.Reference.CurrentObject, Element))
        {
            nextRef.Reference.Assign(Element, nextRef.SourceInfo);
        }

        _ref = nextRef;
    }

    public void ReplaceChildren(IReadOnlyList<RenderedNativeElement> children)
    {
        _children = children;
    }

    public void ReplacePropertyContent(
        IReadOnlyDictionary<string, IReadOnlyList<RenderedNativeElement>> propertyContent)
    {
        _propertyContent = propertyContent;
    }

    public void UpdateKey(string? key)
    {
        Key = key;
    }
}
