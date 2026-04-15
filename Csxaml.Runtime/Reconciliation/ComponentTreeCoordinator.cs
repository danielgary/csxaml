namespace Csxaml.Runtime;

public sealed class ComponentTreeCoordinator : IDisposable, IAsyncDisposable
{
    private readonly IComponentActivator _activator;
    private readonly ComponentContext _context;
    private readonly ComponentInstance _rootComponent;
    private bool _isDisposed;

    public ComponentTreeCoordinator(
        Type rootComponentType,
        IServiceProvider? services = null)
        : this(CreateRootActivation(rootComponentType, services))
    {
    }

    public ComponentTreeCoordinator(ComponentInstance rootComponent)
        : this(rootComponent, services: null, activator: null)
    {
    }

    public ComponentTreeCoordinator(
        ComponentInstance rootComponent,
        IServiceProvider? services)
        : this(rootComponent, services, activator: null)
    {
    }

    internal ComponentTreeCoordinator(
        ComponentInstance rootComponent,
        IServiceProvider? services,
        IComponentActivator? activator)
    {
        _activator = activator ?? new DefaultComponentActivator();
        _context = new ComponentContext(services);
        _rootComponent = rootComponent;
        _rootComponent.Initialize(_context);
        _rootComponent.RequestRender = RequestRenderTree;
    }

    private ComponentTreeCoordinator(RootActivation activation)
        : this(activation.RootComponent, activation.Services, activation.Activator)
    {
    }

    public event Action<NativeNode>? TreeUpdated;

    public NativeNode Render()
    {
        ThrowIfDisposed();

        try
        {
            var tree = RenderComponent(_rootComponent);
            TreeUpdated?.Invoke(tree);
            return tree;
        }
        catch (Exception exception)
        {
            throw CsxamlRuntimeExceptionBuilder.Wrap(
                exception,
                "root render",
                _rootComponent);
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        TreeUpdated = null;
        ComponentDisposer.Dispose(_rootComponent);
    }

    public ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return ValueTask.CompletedTask;
        }

        _isDisposed = true;
        TreeUpdated = null;
        return ComponentDisposer.DisposeAsync(_rootComponent);
    }

    private void RequestRenderTree()
    {
        if (_isDisposed)
        {
            return;
        }

        Render();
    }

    private NativeNode RenderComponent(ComponentInstance component)
    {
        component.ChildComponents.BeginRenderPass();
        try
        {
            var node = component.Render();
            var tree = ExpandNode(component, node);
            component.ChildComponents.CompleteRenderPass();
            component.MarkMounted();
            return tree;
        }
        catch (Exception exception)
        {
            component.ChildComponents.AbortRenderPass();
            throw CsxamlRuntimeExceptionBuilder.Wrap(
                exception,
                "component render",
                component);
        }
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
        try
        {
            var child = owner.ChildComponents.Resolve(componentNode, _context, _activator);
            child.RequestRender = RequestRenderTree;
            child.SetProps(componentNode.Props);
            child.SetChildContent(componentNode.ChildContent);
            return MergeAttachedProperties(RenderComponent(child), componentNode.AttachedProperties);
        }
        catch (Exception exception)
        {
            throw CsxamlRuntimeExceptionBuilder.Wrap(
                exception,
                "child component render",
                owner,
                componentNode.SourceInfo,
                $"while rendering '{componentNode.ComponentType.Name}'");
        }
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
            children,
            nativeElementNode.SourceInfo);
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

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(ComponentTreeCoordinator));
        }
    }

    private static RootActivation CreateRootActivation(
        Type rootComponentType,
        IServiceProvider? services)
    {
        var activator = new DefaultComponentActivator();
        return new RootActivation(
            activator.CreateComponent(rootComponentType, new ComponentContext(services)),
            services,
            activator);
    }

    private sealed record RootActivation(
        ComponentInstance RootComponent,
        IServiceProvider? Services,
        IComponentActivator Activator);
}
