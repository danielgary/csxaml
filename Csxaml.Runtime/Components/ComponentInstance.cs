namespace Csxaml.Runtime;

/// <summary>
/// Provides the base class for generated and hand-authored CSXAML components.
/// </summary>
public abstract class ComponentInstance
{
    private readonly ChildComponentStore _childComponents = new();
    private IReadOnlyList<Node> _childContent = Array.Empty<Node>();
    private bool _isDisposed;
    private bool _isMounted;
    private IServiceProvider? _services;
    private Action? _stateWriteValidator;

    /// <summary>
    /// Gets the display name used in CSXAML runtime diagnostics.
    /// </summary>
    public virtual string CsxamlComponentName => GetType().Name;

    /// <summary>
    /// Gets source-location metadata for the component declaration, when available.
    /// </summary>
    public virtual CsxamlSourceInfo? CsxamlSourceInfo => null;

    /// <summary>
    /// Gets or sets the callback used to request a render after component state changes.
    /// </summary>
    public Action? RequestRender { get; set; }

    internal ChildComponentStore ChildComponents => _childComponents;

    internal Action? StateWriteValidator
    {
        set => _stateWriteValidator = value;
    }

    /// <summary>
    /// Gets child content passed to the component by its parent.
    /// </summary>
    protected IReadOnlyList<Node> ChildContent => _childContent;

    internal bool IsDisposed => _isDisposed;

    /// <summary>
    /// Gets the service provider available to the component.
    /// </summary>
    protected IServiceProvider Services => _services ?? NullServiceProvider.Instance;

    /// <summary>
    /// Applies props supplied by a parent component.
    /// </summary>
    /// <param name="props">The props object produced by generated CSXAML code.</param>
    /// <exception cref="InvalidOperationException">Thrown when props are supplied to a component that does not accept props.</exception>
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
        _stateWriteValidator = null;
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
        _stateWriteValidator = null;
        return true;
    }

    /// <summary>
    /// Requests a render because component state changed.
    /// </summary>
    protected void InvalidateState()
    {
        RequestRender?.Invoke();
    }

    /// <summary>
    /// Verifies that a state write is allowed in the current runtime phase.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when state is written during render.</exception>
    protected void ValidateStateWrite()
    {
        _stateWriteValidator?.Invoke();
    }

    /// <summary>
    /// Resolves services required by the component before initialization.
    /// </summary>
    /// <param name="services">The service provider supplied to the runtime host.</param>
    protected virtual void ResolveInjectedServices(IServiceProvider services)
    {
    }

    /// <summary>
    /// Runs component initialization after services are available.
    /// </summary>
    protected virtual void InitializeComponent()
    {
    }

    /// <summary>
    /// Runs after the component is first mounted into the rendered tree.
    /// </summary>
    protected virtual void OnMounted()
    {
    }

    /// <summary>
    /// Renders the component into a CSXAML runtime node tree.
    /// </summary>
    /// <returns>The root node produced by the component.</returns>
    public abstract Node Render();
}
