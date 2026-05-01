using Microsoft.UI.Xaml;

namespace Csxaml.Runtime;

/// <summary>
/// Projects CSXAML native runtime nodes into retained WinUI elements.
/// </summary>
public sealed partial class WinUiNodeRenderer : IDisposable
{
    private readonly ControlAdapterRegistry _registry;
    private RenderedNativeElement? _root;

    /// <summary>
    /// Initializes a new instance of the <see cref="WinUiNodeRenderer"/> class.
    /// </summary>
    public WinUiNodeRenderer()
        : this(new ControlAdapterRegistry())
    {
    }

    internal WinUiNodeRenderer(ControlAdapterRegistry registry)
    {
        _registry = registry;
    }

    /// <summary>
    /// Renders a native runtime node tree to a WinUI root element.
    /// </summary>
    /// <param name="node">The native runtime node tree to project.</param>
    /// <returns>The retained WinUI root element.</returns>
    public UIElement Render(NativeNode node)
    {
        try
        {
            return RenderProjectedRoot(node) as UIElement ??
                throw new InvalidOperationException(
                    "The projected root must be a UIElement.");
        }
        catch (Exception exception)
        {
            throw CsxamlRuntimeExceptionBuilder.Wrap(
                exception,
                "root projection",
                sourceInfo: (node as NativeElementNode)?.SourceInfo);
        }
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
        try
        {
            var adapter = _registry.Get(node.TagName);
            var element = adapter.Create();
            var rendered = new RenderedNativeElement(node.TagName, node.Key, element, adapter);
            ApplyElement(rendered, node);
            return rendered;
        }
        catch (Exception exception)
        {
            throw CsxamlRuntimeExceptionBuilder.Wrap(
                exception,
                "native element creation",
                sourceInfo: node.SourceInfo,
                detail: node.TagName);
        }
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
        try
        {
            ApplyElement(retained, node);
            return retained;
        }
        catch (Exception exception)
        {
            throw CsxamlRuntimeExceptionBuilder.Wrap(
                exception,
                "native element update",
                sourceInfo: node.SourceInfo,
                detail: node.TagName);
        }
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

    /// <summary>
    /// Releases retained native elements owned by the renderer.
    /// </summary>
    public void Dispose()
    {
        _root?.Dispose();
        _root = null;
    }

    private void ApplyElement(RenderedNativeElement rendered, NativeElementNode node)
    {
        rendered.Adapter.ApplyProperties(rendered.Element, node);
        ApplyAttachedProperties(rendered, node);
        rendered.Adapter.ApplyEvents(rendered.Element, node, rendered.EventBindings);
        UpdateChildren(rendered, node);
        UpdatePropertyContent(rendered, node);
        rendered.ApplyRef(node.Ref);
    }

    private static void ApplyAttachedProperties(RenderedNativeElement rendered, NativeElementNode node)
    {
        if (rendered.Element is not FrameworkElement frameworkElement)
        {
            if (node.AttachedProperties.Count == 0)
            {
                return;
            }

            throw new InvalidOperationException(
                $"Projected element for '{node.TagName}' must be a FrameworkElement.");
        }

        try
        {
            AttachedPropertyApplicator.Apply(frameworkElement, node);
        }
        catch (Exception exception)
        {
            throw CsxamlRuntimeExceptionBuilder.Wrap(
                exception,
                "attached property application",
                sourceInfo: node.SourceInfo,
                detail: node.TagName);
        }
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
        try
        {
            rendered.Adapter.SetChildren(rendered.Element, ProjectChildElements(updatedChildren));
        }
        catch (Exception exception)
        {
            throw CsxamlRuntimeExceptionBuilder.Wrap(
                exception,
                "child projection",
                sourceInfo: node.SourceInfo,
                detail: node.TagName);
        }
    }
}
