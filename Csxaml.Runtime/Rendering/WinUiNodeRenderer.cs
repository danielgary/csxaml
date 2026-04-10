using Microsoft.UI.Xaml;

namespace Csxaml.Runtime;

public sealed class WinUiNodeRenderer
{
    private readonly ControlAdapterRegistry _registry;
    private RenderedNativeElement? _root;

    public WinUiNodeRenderer()
        : this(new ControlAdapterRegistry())
    {
    }

    internal WinUiNodeRenderer(ControlAdapterRegistry registry)
    {
        _registry = registry;
    }

    public UIElement Render(NativeNode node)
    {
        return RenderProjectedRoot(node) as UIElement ??
            throw new InvalidOperationException(
                "The projected root must be a UIElement.");
    }

    internal object RenderProjectedRoot(NativeNode node)
    {
        if (node is not NativeElementNode nativeElement)
        {
            throw new NotSupportedException(
                $"Unsupported native node type '{node.GetType().Name}'.");
        }

        _root = RenderElement(_root, nativeElement);
        return _root.Element;
    }

    private RenderedNativeElement CreateElement(NativeElementNode node)
    {
        var adapter = _registry.Get(node.TagName);
        var element = adapter.Create();
        var rendered = new RenderedNativeElement(node.TagName, element, adapter);
        adapter.ApplyProperties(element, node);
        adapter.ApplyEvents(element, node, rendered.EventBindings);
        UpdateChildren(rendered, node);
        return rendered;
    }

    private RenderedNativeElement RenderElement(
        RenderedNativeElement? existing,
        NativeElementNode node)
    {
        if (existing is null ||
            !string.Equals(existing.TagName, node.TagName, StringComparison.Ordinal))
        {
            existing?.Dispose();
            return CreateElement(node);
        }

        existing.Adapter.ApplyProperties(existing.Element, node);
        existing.Adapter.ApplyEvents(existing.Element, node, existing.EventBindings);
        UpdateChildren(existing, node);
        return existing;
    }

    private void UpdateChildren(RenderedNativeElement rendered, NativeElementNode node)
    {
        var nativeChildren = node.Children
            .Select(child => child as NativeElementNode ??
                throw new NotSupportedException(
                    $"Unsupported child node type '{child.GetType().Name}'."))
            .ToList();

        var updatedChildren = new List<RenderedNativeElement>(nativeChildren.Count);
        for (var index = 0; index < nativeChildren.Count; index++)
        {
            var existingChild = index < rendered.Children.Count
                ? rendered.Children[index]
                : null;

            updatedChildren.Add(RenderElement(existingChild, nativeChildren[index]));
        }

        for (var index = nativeChildren.Count; index < rendered.Children.Count; index++)
        {
            rendered.Children[index].Dispose();
        }

        rendered.ReplaceChildren(updatedChildren);
        rendered.Adapter.SetChildren(
            rendered.Element,
            updatedChildren.Select(child => child.Element).ToList());
    }
}
