using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

/// <summary>
/// Hosts a CSXAML component tree inside a WinUI panel.
/// </summary>
public sealed class CsxamlHost : IDisposable, IAsyncDisposable
{
    private readonly Panel _hostPanel;
    private readonly ComponentTreeCoordinator _treeCoordinator;
    private readonly WinUiNodeRenderer _renderer;
    private RootPointerWheelBridge? _pointerWheelBridge;
    private bool _isDisposed;
    private UIElement? _rootElement;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsxamlHost"/> class with an existing root component.
    /// </summary>
    /// <param name="hostPanel">The panel that receives the rendered root element.</param>
    /// <param name="rootComponent">The root component instance to render.</param>
    public CsxamlHost(Panel hostPanel, ComponentInstance rootComponent)
        : this(hostPanel, rootComponent, services: null, activator: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsxamlHost"/> class with an existing root component and services.
    /// </summary>
    /// <param name="hostPanel">The panel that receives the rendered root element.</param>
    /// <param name="rootComponent">The root component instance to render.</param>
    /// <param name="services">The services available to the component tree.</param>
    public CsxamlHost(
        Panel hostPanel,
        ComponentInstance rootComponent,
        IServiceProvider? services)
        : this(hostPanel, rootComponent, services, activator: null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsxamlHost"/> class by activating the root component type.
    /// </summary>
    /// <param name="hostPanel">The panel that receives the rendered root element.</param>
    /// <param name="rootComponentType">The component type to instantiate as the root.</param>
    /// <param name="services">The services available to the component tree.</param>
    public CsxamlHost(
        Panel hostPanel,
        Type rootComponentType,
        IServiceProvider? services = null)
        : this(hostPanel, CreateRootActivation(rootComponentType, services))
    {
    }

    /// <summary>
    /// Renders the current component tree into the host panel.
    /// </summary>
    public void Render()
    {
        ThrowIfDisposed();
        _treeCoordinator.Render();
    }

    /// <summary>
    /// Releases rendered elements and disposes the component tree.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _treeCoordinator.Dispose();
        _renderer.Dispose();
        _pointerWheelBridge?.Dispose();
        _hostPanel.Children.Clear();
        _rootElement = null;
    }

    /// <summary>
    /// Asynchronously releases rendered elements and disposes the component tree.
    /// </summary>
    /// <returns>A task-like value that completes when asynchronous disposal has finished.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        await _treeCoordinator.DisposeAsync();
        _renderer.Dispose();
        _pointerWheelBridge?.Dispose();
        _hostPanel.Children.Clear();
        _rootElement = null;
    }

    private void UpdateHostPanel(NativeNode tree)
    {
        var element = _renderer.Render(tree);
        if (_hostPanel.Children.Count == 0)
        {
            _hostPanel.Children.Add(element);
            _rootElement = element;
            _pointerWheelBridge = RootPointerWheelBridge.Attach(element);
            return;
        }

        if (ReferenceEquals(_rootElement, element))
        {
            return;
        }

        _pointerWheelBridge?.Dispose();
        _hostPanel.Children.Clear();
        _hostPanel.Children.Add(element);
        _rootElement = element;
        _pointerWheelBridge = RootPointerWheelBridge.Attach(element);
    }

    private CsxamlHost(
        Panel hostPanel,
        RootActivation activation)
        : this(hostPanel, activation.RootComponent, activation.Services, activation.Activator)
    {
    }

    private CsxamlHost(
        Panel hostPanel,
        ComponentInstance rootComponent,
        IServiceProvider? services,
        IComponentActivator? activator)
    {
        _hostPanel = hostPanel;
        _renderer = new WinUiNodeRenderer();
        _treeCoordinator = new ComponentTreeCoordinator(rootComponent, services, activator);
        _treeCoordinator.TreeUpdated += UpdateHostPanel;
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

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(CsxamlHost));
        }
    }
}
