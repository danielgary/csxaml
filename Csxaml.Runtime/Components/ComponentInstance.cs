namespace Csxaml.Runtime;

public abstract class ComponentInstance
{
    private readonly ChildComponentStore _childComponents = new();
    private IReadOnlyList<Node> _childContent = Array.Empty<Node>();
    private bool _isDisposed;
    private bool _isMounted;
    private IServiceProvider? _services;

    public virtual string CsxamlComponentName => GetType().Name;

    public virtual CsxamlSourceInfo? CsxamlSourceInfo => null;

    public Action? RequestRender { get; set; }

    internal ChildComponentStore ChildComponents => _childComponents;

    protected IReadOnlyList<Node> ChildContent => _childContent;

    internal bool IsDisposed => _isDisposed;

    protected IServiceProvider Services => _services ?? NullServiceProvider.Instance;

    public virtual void SetProps(object? props)
    {
        if (props is not null)
        {
            throw new InvalidOperationException(
                $"Component '{GetType().Name}' does not accept props.");
        }
    }

    internal void SetChildContent(IReadOnlyList<Node> childContent)
    {
        _childContent = childContent;
    }

    internal void Initialize(ComponentContext context)
    {
        if (_isDisposed)
        {
            throw new InvalidOperationException(
                $"Component '{GetType().Name}' is already disposed.");
        }

        if (_services is not null)
        {
            if (!ReferenceEquals(_services, context.Services))
            {
                throw new InvalidOperationException(
                    $"Component '{GetType().Name}' is already initialized with different services.");
            }

            return;
        }

        _services = context.Services;
        ResolveInjectedServices(context.Services);
        InitializeComponent();
    }

    internal void MarkMounted()
    {
        if (_isDisposed || _isMounted)
        {
            return;
        }

        _isMounted = true;
        OnMounted();
    }

    internal void MarkUnmounted()
    {
        if (_isDisposed)
        {
            return;
        }

        _isMounted = false;
        RequestRender = null;
    }

    internal bool TryBeginDispose()
    {
        if (_isDisposed)
        {
            return false;
        }

        _isDisposed = true;
        _isMounted = false;
        RequestRender = null;
        return true;
    }

    protected virtual void ResolveInjectedServices(IServiceProvider services)
    {
    }

    protected virtual void InitializeComponent()
    {
    }

    protected virtual void OnMounted()
    {
    }

    public abstract Node Render();
}
