using Csxaml.Runtime;
using Microsoft.UI.Xaml;

namespace PackageHello;

public sealed partial class MainWindow : Window
{
    private readonly CsxamlHost _host;

    public MainWindow()
    {
        InitializeComponent();

        var card = new HelloCardComponent();
        card.SetProps(new HelloCardProps("Hello from packaged CSXAML"));

        _host = new CsxamlHost(ComponentHost, card);
        _host.Render();
    }
}
