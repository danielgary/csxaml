using Csxaml.Runtime;
using GeneratedCsxaml;
using Microsoft.UI.Xaml;

namespace Csxaml.Demo;

public sealed partial class MainWindow : Window
{
    private readonly CsxamlHost _host;

    public MainWindow()
    {
        InitializeComponent();

        _host = new CsxamlHost(ComponentHost, new TodoBoardComponent());
        _host.Render();
    }
}
