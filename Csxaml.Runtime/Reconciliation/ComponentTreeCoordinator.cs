namespace Csxaml.Runtime;

public sealed class ComponentTreeCoordinator
{
    private readonly ComponentInstance _rootComponent;

    public ComponentTreeCoordinator(ComponentInstance rootComponent)
    {
        _rootComponent = rootComponent;
        _rootComponent.RequestRender = RequestRenderTree;
    }

    public event Action<NativeNode>? TreeUpdated;

    public NativeNode Render()
    {
        var tree = RenderComponent(_rootComponent);
        TreeUpdated?.Invoke(tree);
        return tree;
    }

    private void RequestRenderTree()
    {
        Render();
    }

    private NativeNode RenderComponent(ComponentInstance component)
    {
        component.ChildComponents.BeginRenderPass();
        var tree = ExpandNode(component, component.Render());
        component.ChildComponents.CompleteRenderPass();
        return tree;
    }

    private NativeNode ExpandNode(ComponentInstance owner, Node node)
    {
        return node switch
        {
            ComponentNode componentNode => RenderChildComponent(owner, componentNode),
            NativeElementNode nativeElementNode => ExpandNativeElement(owner, nativeElementNode),
            _ => throw new NotSupportedException(
                $"Unsupported node type '{node.GetType().Name}'.")
        };
    }

    private NativeNode RenderChildComponent(
        ComponentInstance owner,
        ComponentNode componentNode)
    {
        var child = owner.ChildComponents.Resolve(componentNode);
        child.RequestRender = RequestRenderTree;
        child.SetProps(componentNode.Props);
        child.SetChildContent(componentNode.ChildContent);
        return MergeAttachedProperties(RenderComponent(child), componentNode.AttachedProperties);
    }

    private NativeElementNode ExpandNativeElement(
        ComponentInstance owner,
        NativeElementNode nativeElementNode)
    {
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
            nativeElementNode.Events,
            children);
    }

    private static NativeElementNode MergeAttachedProperties(
        NativeNode renderedNode,
        IReadOnlyList<NativeAttachedPropertyValue> usageAttachedProperties)
    {
        if (renderedNode is not NativeElementNode nativeElementNode)
        {
            throw new NotSupportedException(
                $"Unsupported native node type '{renderedNode.GetType().Name}'.");
        }

        if (usageAttachedProperties.Count == 0)
        {
            return nativeElementNode;
        }

        var merged = nativeElementNode.AttachedProperties
            .Where(existing => usageAttachedProperties.All(candidate => !Matches(candidate, existing)))
            .Concat(usageAttachedProperties)
            .ToArray();

        return nativeElementNode with { AttachedProperties = merged };
    }

    private static bool Matches(
        NativeAttachedPropertyValue left,
        NativeAttachedPropertyValue right)
    {
        return string.Equals(left.OwnerName, right.OwnerName, StringComparison.Ordinal) &&
            string.Equals(left.PropertyName, right.PropertyName, StringComparison.Ordinal);
    }
}
