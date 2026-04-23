using Csxaml.Runtime;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace Csxaml.Starter;

public sealed partial class MainWindow : Window
{
    private readonly CsxamlHost _host;

    public MainWindow()
    {
        InitializeComponent();
        ConfigureStartupWindow();

        _host = new CsxamlHost(ComponentHost, typeof(StarterPageComponent));
        _host.Render();
    }

    private void ConfigureStartupWindow()
    {
        AppWindow.Title = "CSXAML Starter";
        AppWindow.Resize(new SizeInt32(720, 520));
    }
}
