using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

/// <summary>
/// Hosts a retained CSXAML component body inside a generated WinUI root.
/// </summary>
public sealed class CsxamlRootHost : IDisposable, IAsyncDisposable
{
    private readonly Func<object?> _getContent;
    private readonly Action<object?> _setContent;
    private readonly ComponentTreeCoordinator _treeCoordinator;
    private readonly WinUiNodeRenderer _renderer;
    private readonly WindowMouseWheelBridge? _mouseWheelBridge;
    private readonly ThreadMouseWheelBridge? _threadMouseWheelBridge;
    private RootPointerWheelBridge? _pointerWheelBridge;
    private bool _isDisposed;
    private FrameworkElement? _loadedElement;
    private UIElement? _rootElement;

    /// <summary>
    /// Initializes a host for a generated WinUI window.
    /// </summary>
    public CsxamlRootHost(
        Window window,
        ComponentInstance rootComponent,
        IServiceProvider? services = null)
        : this(() => window.Content, value => window.Content = (UIElement?)value, rootComponent, services)
    {
        _mouseWheelBridge = WindowMouseWheelBridge.Attach(window, () => _rootElement);
        _threadMouseWheelBridge = ThreadMouseWheelBridge.Attach(window, () => _rootElement);
        window.Activated += (_, _) => _mouseWheelBridge.RefreshTargets();
        window.Closed += (_, _) => Dispose();
    }

    /// <summary>
    /// Initializes a host for a generated WinUI page.
    /// </summary>
    public CsxamlRootHost(
        Page page,
        ComponentInstance rootComponent,
        IServiceProvider? services = null)
        : this(() => page.Content, value => page.Content = (UIElement?)value, rootComponent, services)
    {
        page.Unloaded += (_, _) => Dispose();
    }

    /// <summary>
    /// Creates a window host and renders its initial content immediately.
    /// </summary>
    public static CsxamlRootHost Mount(
        Window window,
        ComponentInstance rootComponent,
        IServiceProvider? services = null)
    {
        var host = new CsxamlRootHost(window, rootComponent, services);
        host.Render();
        return host;
    }

    /// <summary>
    /// Creates a page host and renders its initial content immediately.
    /// </summary>
    public static CsxamlRootHost Mount(
        Page page,
        ComponentInstance rootComponent,
        IServiceProvider? services = null)
    {
        var host = new CsxamlRootHost(page, rootComponent, services);
        host.Render();
        return host;
    }

    /// <summary>
    /// Renders the current component tree into the generated root.
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
        _mouseWheelBridge?.Dispose();
        _threadMouseWheelBridge?.Dispose();
        _pointerWheelBridge?.Dispose();
        ClearContent();
    }

    /// <summary>
    /// Asynchronously releases rendered elements and disposes the component tree.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        await _treeCoordinator.DisposeAsync();
        _renderer.Dispose();
        _mouseWheelBridge?.Dispose();
        _threadMouseWheelBridge?.Dispose();
        _pointerWheelBridge?.Dispose();
        ClearContent();
    }

    private CsxamlRootHost(
        Func<object?> getContent,
        Action<object?> setContent,
        ComponentInstance rootComponent,
        IServiceProvider? services)
    {
        _getContent = getContent;
        _setContent = setContent;
        _renderer = new WinUiNodeRenderer();
        _treeCoordinator = new ComponentTreeCoordinator(rootComponent, services);
        _treeCoordinator.TreeUpdated += UpdateContent;
    }

    private void UpdateContent(NativeNode tree)
    {
        var element = _renderer.Render(tree);
        if (ReferenceEquals(_rootElement, element))
        {
            RefreshMouseWheelTargets(element);
            return;
        }

        _pointerWheelBridge?.Dispose();
        DetachLoadedElement();
        _setContent(element);
        _rootElement = element;
        AttachLoadedElement(element);
        RefreshMouseWheelTargets(element);
        _pointerWheelBridge = RootPointerWheelBridge.Attach(element);
    }

    private void ClearContent()
    {
        DetachLoadedElement();
        if (ReferenceEquals(_getContent(), _rootElement))
        {
            _setContent(null);
        }

        _rootElement = null;
        _pointerWheelBridge = null;
    }

    private void AttachLoadedElement(UIElement element)
    {
        if (element is not FrameworkElement frameworkElement)
        {
            return;
        }

        _loadedElement = frameworkElement;
        _loadedElement.Loaded += RefreshMouseWheelTargets;
    }

    private void DetachLoadedElement()
    {
        if (_loadedElement is null)
        {
            return;
        }

        _loadedElement.Loaded -= RefreshMouseWheelTargets;
        _loadedElement = null;
    }

    private void RefreshMouseWheelTargets(object sender, RoutedEventArgs args)
    {
        if (sender is UIElement element)
        {
            RefreshMouseWheelTargets(element);
        }
    }

    private void RefreshMouseWheelTargets(UIElement element)
    {
        _mouseWheelBridge?.RefreshTargets();
        element.DispatcherQueue.TryEnqueue(() => _mouseWheelBridge?.RefreshTargets());
    }

    private void ThrowIfDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(CsxamlRootHost));
        }
    }
}
