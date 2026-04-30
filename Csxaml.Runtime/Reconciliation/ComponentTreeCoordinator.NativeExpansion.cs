namespace Csxaml.Runtime;

public sealed partial class ComponentTreeCoordinator
{
    private NativeElementNode ExpandNativeElement(
        ComponentInstance owner,
        NativeElementNode nativeElementNode)
    {
        var propertyContent = ExpandPropertyContent(owner, nativeElementNode.PropertyContent);
        var children = new List<Node>(nativeElementNode.Children.Count);
        foreach (var child in nativeElementNode.Children)
        {
            children.Add(ExpandNode(owner, child));
        }

        return new NativeElementNode(
            nativeElementNode.TagName,
            nativeElementNode.Key,
            nativeElementNode.Properties,
            nativeElementNode.AttachedProperties,
            nativeElementNode.Ref,
            nativeElementNode.Events,
            propertyContent,
            children,
            nativeElementNode.SourceInfo);
    }

    private IReadOnlyList<NativePropertyContentValue> ExpandPropertyContent(
        ComponentInstance owner,
        IReadOnlyList<NativePropertyContentValue> propertyContent)
    {
        var expanded = new List<NativePropertyContentValue>(propertyContent.Count);
        foreach (var content in propertyContent)
        {
            expanded.Add(ExpandPropertyContent(owner, content));
        }

        return expanded;
    }

    private NativePropertyContentValue ExpandPropertyContent(
        ComponentInstance owner,
        NativePropertyContentValue propertyContent)
    {
        var children = new List<Node>(propertyContent.Children.Count);
        foreach (var child in propertyContent.Children)
        {
            children.Add(ExpandNode(owner, child));
        }

        return propertyContent with { Children = children };
    }
}
