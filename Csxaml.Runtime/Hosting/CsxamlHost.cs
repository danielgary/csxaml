using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

public sealed class CsxamlHost : IDisposable, IAsyncDisposable
{
    private readonly Panel _hostPanel;
    private readonly ComponentTreeCoordinator _treeCoordinator;
    private readonly WinUiNodeRenderer _renderer;
    private bool _isDisposed;
    private UIElement? _rootElement;

    public CsxamlHost(Panel hostPanel, ComponentInstance rootComponent)
        : this(hostPanel, rootComponent, services: null, activator: null)
    {
    }

    public CsxamlHost(
        Panel hostPanel,
        ComponentInstance rootComponent,
        IServiceProvider? services)
        : this(hostPanel, rootComponent, services, activator: null)
    {
    }

    public CsxamlHost(
        Panel hostPanel,
        Type rootComponentType,
        IServiceProvider? services = null)
        : this(hostPanel, CreateRootActivation(rootComponentType, services))
    {
    }

    public void Render()
    {
        ThrowIfDisposed();
        _treeCoordinator.Render();
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _treeCoordinator.Dispose();
        _renderer.Dispose();
        _hostPanel.Children.Clear();
        _rootElement = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        await _treeCoordinator.DisposeAsync();
        _renderer.Dispose();
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
            return;
        }

        if (ReferenceEquals(_rootElement, element))
        {
            return;
        }

        _hostPanel.Children.Clear();
        _hostPanel.Children.Add(element);
        _rootElement = element;
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
