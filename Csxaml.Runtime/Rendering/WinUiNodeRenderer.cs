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
        var rendered = new RenderedNativeElement(node.TagName, node.Key, element, adapter);
        ApplyElement(rendered, node);
        return rendered;
    }

    private RenderedNativeElement RenderElement(
        RenderedNativeElement? existing,
        NativeElementNode node)
    {
        if (!CanReuse(existing, node))
        {
            existing?.Dispose();
            return CreateElement(node);
        }

        var retained = existing ?? throw new InvalidOperationException(
            "Retained render path requires an existing native element.");
        retained.UpdateKey(node.Key);
        ApplyElement(retained, node);
        return retained;
    }

    private static bool CanReuse(RenderedNativeElement? existing, NativeElementNode node)
    {
        return existing is not null &&
            string.Equals(existing.TagName, node.TagName, StringComparison.Ordinal) &&
            string.Equals(existing.Key, node.Key, StringComparison.Ordinal);
    }

    private static List<NativeElementNode> GetNativeChildren(NativeElementNode node)
    {
        var nativeChildren = new List<NativeElementNode>(node.Children.Count);
        foreach (var child in node.Children)
        {
            if (child is not NativeElementNode nativeChild)
            {
                throw new NotSupportedException(
                    $"Unsupported child node type '{child.GetType().Name}'.");
            }

            nativeChildren.Add(nativeChild);
        }

        return nativeChildren;
    }

    private static List<object> ProjectChildElements(IReadOnlyList<RenderedNativeElement> children)
    {
        var projectedChildren = new List<object>(children.Count);
        foreach (var child in children)
        {
            projectedChildren.Add(child.Element);
        }

        return projectedChildren;
    }

    private static void ValidateUniqueChildKeys(IReadOnlyList<NativeElementNode> nativeChildren)
    {
        var seenKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var child in nativeChildren)
        {
            if (child.Key is null)
            {
                continue;
            }

            if (!seenKeys.Add(child.Key))
            {
                throw new InvalidOperationException(
                    $"Sibling native elements cannot share the key '{child.Key}'.");
            }
        }
    }

    private void ApplyElement(RenderedNativeElement rendered, NativeElementNode node)
    {
        rendered.Adapter.ApplyProperties(rendered.Element, node);
        rendered.Adapter.ApplyEvents(rendered.Element, node, rendered.EventBindings);
        UpdateChildren(rendered, node);
    }

    private void UpdateChildren(RenderedNativeElement rendered, NativeElementNode node)
    {
        var nativeChildren = GetNativeChildren(node);
        ValidateUniqueChildKeys(nativeChildren);

        var matcher = new RenderedChildMatcher(rendered.Children);
        var updatedChildren = new List<RenderedNativeElement>(nativeChildren.Count);
        foreach (var child in nativeChildren)
        {
            updatedChildren.Add(RenderElement(matcher.TakeMatch(child), child));
        }

        matcher.DisposeUnmatched();
        rendered.ReplaceChildren(updatedChildren);
        rendered.Adapter.SetChildren(rendered.Element, ProjectChildElements(updatedChildren));
    }
}
