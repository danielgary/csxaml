using Csxaml.Runtime;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace Csxaml.Samples.ExistingWinUI;

public sealed partial class MainWindow : Window
{
    private readonly CsxamlHost _host;

    public MainWindow()
    {
        InitializeComponent();
        ConfigureStartupWindow();

        _host = new CsxamlHost(ComponentHost, typeof(ExistingProjectPageComponent));
        _host.Render();
    }

    private void ConfigureStartupWindow()
    {
        AppWindow.Title = "Existing WinUI + CSXAML";
        AppWindow.Resize(new SizeInt32(720, 520));
    }
}
