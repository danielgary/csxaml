using System;
using Csxaml.Runtime;
using Microsoft.UI.Xaml;

namespace Csxaml.Demo;

public sealed partial class MainWindow : Window
{
    private readonly CsxamlHost _host;

    public MainWindow(IServiceProvider services)
    {
        InitializeComponent();

        _host = new CsxamlHost(ComponentHost, typeof(TodoBoardComponent), services);
        _host.Render();
    }
}
