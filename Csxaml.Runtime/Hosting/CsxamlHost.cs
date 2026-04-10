using Microsoft.UI.Xaml.Controls;

namespace Csxaml.Runtime;

public sealed class CsxamlHost
{
    private readonly Panel _hostPanel;
    private readonly ComponentTreeCoordinator _treeCoordinator;
    private readonly WinUiNodeRenderer _renderer;

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
        _hostPanel.Children.Clear();
        _hostPanel.Children.Add(_renderer.Render(tree));
    }
}
