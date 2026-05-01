namespace Csxaml.Runtime;

public sealed partial class WinUiNodeRenderer
{
    private static IReadOnlyDictionary<string, IReadOnlyList<object>> ProjectPropertyContent(
        IReadOnlyDictionary<string, IReadOnlyList<RenderedNativeElement>> propertyContent)
    {
        return propertyContent.ToDictionary(
            entry => entry.Key,
            entry => (IReadOnlyList<object>)ProjectChildElements(entry.Value),
            StringComparer.Ordinal);
    }

    private static List<NativeElementNode> GetNativeChildren(IReadOnlyList<Node> nodes)
    {
        var nativeChildren = new List<NativeElementNode>(nodes.Count);
        foreach (var child in nodes)
        {
            if (child is not NativeElementNode nativeChild)
            {
                throw new NotSupportedException(
                    $"Unsupported property-content node type '{child.GetType().Name}'.");
            }

            nativeChildren.Add(nativeChild);
        }

        return nativeChildren;
    }

    private void UpdatePropertyContent(RenderedNativeElement rendered, NativeElementNode node)
    {
        var updated = new Dictionary<string, IReadOnlyList<RenderedNativeElement>>(StringComparer.Ordinal);
        foreach (var propertyContent in node.PropertyContent)
        {
            updated[propertyContent.Name] = RenderPropertyContentChildren(rendered, propertyContent);
        }

        foreach (var entry in rendered.PropertyContent)
        {
            if (updated.ContainsKey(entry.Key))
            {
                continue;
            }

            foreach (var child in entry.Value)
            {
                child.Dispose();
            }

            updated[entry.Key] = Array.Empty<RenderedNativeElement>();
        }

        rendered.ReplacePropertyContent(updated);
        try
        {
            rendered.Adapter.SetPropertyContent(rendered.Element, ProjectPropertyContent(updated));
        }
        catch (Exception exception)
        {
            throw CsxamlRuntimeExceptionBuilder.Wrap(
                exception,
                "property-content projection",
                sourceInfo: node.SourceInfo,
                detail: node.TagName);
        }
    }

    private IReadOnlyList<RenderedNativeElement> RenderPropertyContentChildren(
        RenderedNativeElement rendered,
        NativePropertyContentValue propertyContent)
    {
        var nativeChildren = GetNativeChildren(propertyContent.Children);
        ValidateUniqueChildKeys(nativeChildren);

        var existing = rendered.PropertyContent.TryGetValue(propertyContent.Name, out var children)
            ? children
            : Array.Empty<RenderedNativeElement>();
        var matcher = new RenderedChildMatcher(existing);
        var updatedChildren = new List<RenderedNativeElement>(nativeChildren.Count);
        foreach (var child in nativeChildren)
        {
            updatedChildren.Add(RenderElement(matcher.TakeMatch(child), child));
        }

        matcher.DisposeUnmatched();
        return updatedChildren;
    }
}
