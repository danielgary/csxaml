using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace Csxaml.Demo;

public partial class App : Application
{
    private readonly IServiceProvider _services;
    private Window? _window;

    public App()
    {
        InitializeComponent();
        _services = CreateServices();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow(_services);
        _window.Activate();
    }

    private static IServiceProvider CreateServices()
    {
        return new ServiceCollection()
            .AddSingleton<ITodoService, InMemoryTodoService>()
            .BuildServiceProvider();
    }
}
