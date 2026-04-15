using Microsoft.Extensions.DependencyInjection;

namespace Csxaml.Testing;

public static class CsxamlTestHost
{
    public static CsxamlRenderResult Render(ComponentInstance root, IServiceProvider? services = null)
    {
        return new CsxamlRenderResult(new ComponentTreeCoordinator(root, services));
    }

    public static CsxamlRenderResult Render(Type rootComponentType, IServiceProvider? services = null)
    {
        return new CsxamlRenderResult(new ComponentTreeCoordinator(rootComponentType, services));
    }

    public static CsxamlRenderResult Render<TComponent>(IServiceProvider? services = null)
        where TComponent : ComponentInstance
    {
        return Render(typeof(TComponent), services);
    }

    public static CsxamlRenderResult Render(ComponentInstance root, Action<ServiceCollection> configureServices)
    {
        return Render(root, BuildServices(configureServices));
    }

    public static CsxamlRenderResult Render(Type rootComponentType, Action<ServiceCollection> configureServices)
    {
        return Render(rootComponentType, BuildServices(configureServices));
    }

    public static CsxamlRenderResult Render<TComponent>(Action<ServiceCollection> configureServices)
        where TComponent : ComponentInstance
    {
        return Render(typeof(TComponent), BuildServices(configureServices));
    }

    private static IServiceProvider BuildServices(Action<ServiceCollection> configureServices)
    {
        var services = new ServiceCollection();
        configureServices(services);
        return services.BuildServiceProvider();
    }
}
