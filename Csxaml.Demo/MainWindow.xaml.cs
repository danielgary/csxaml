using System;
using Csxaml.Runtime;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace Csxaml.Demo;

public sealed partial class MainWindow : Window
{
    private readonly CsxamlHost _host;

    public MainWindow(IServiceProvider services)
    {
        InitializeComponent();
        ConfigureStartupWindow();

        _host = new CsxamlHost(ComponentHost, typeof(TodoBoardComponent), services);
        _host.Render();
    }

    private void ConfigureStartupWindow()
    {
        AppWindow.Title = "CSXAML Todo Board";
        AppWindow.Resize(new SizeInt32(1180, 780));
    }
}
