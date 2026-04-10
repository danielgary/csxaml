using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

public sealed class CsxamlHost
{
    private readonly Panel _hostPanel;
    private readonly ComponentTreeCoordinator _treeCoordinator;
    private readonly WinUiNodeRenderer _renderer;
    private UIElement? _rootElement;

    public CsxamlHost(Panel hostPanel, ComponentInstance rootComponent)
    {
        _hostPanel = hostPanel;
        _renderer = new WinUiNodeRenderer();
        _treeCoordinator = new ComponentTreeCoordinator(rootComponent);
        _treeCoordinator.TreeUpdated += UpdateHostPanel;
    }

    public void Render()
    {
        _treeCoordinator.Render();
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
}
