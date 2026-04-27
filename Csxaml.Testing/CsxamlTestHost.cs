using Microsoft.Extensions.DependencyInjection;

namespace Csxaml.Testing;

/// <summary>
/// Creates in-memory CSXAML render sessions for component tests.
/// </summary>
public static class CsxamlTestHost
{
    /// <summary>
    /// Renders an existing root component instance.
    /// </summary>
    /// <param name="root">The root component instance to render.</param>
    /// <param name="services">The services available to the component tree.</param>
    /// <returns>A render result used to inspect and interact with the tree.</returns>
    public static CsxamlRenderResult Render(ComponentInstance root, IServiceProvider? services = null)
    {
        return new CsxamlRenderResult(new ComponentTreeCoordinator(root, services));
    }

    /// <summary>
    /// Activates and renders a root component type.
    /// </summary>
    /// <param name="rootComponentType">The component type to instantiate as the root.</param>
    /// <param name="services">The services available to the component tree.</param>
    /// <returns>A render result used to inspect and interact with the tree.</returns>
    public static CsxamlRenderResult Render(Type rootComponentType, IServiceProvider? services = null)
    {
        return new CsxamlRenderResult(new ComponentTreeCoordinator(rootComponentType, services));
    }

    /// <summary>
    /// Activates and renders a root component type.
    /// </summary>
    /// <typeparam name="TComponent">The component type to instantiate as the root.</typeparam>
    /// <param name="services">The services available to the component tree.</param>
    /// <returns>A render result used to inspect and interact with the tree.</returns>
    public static CsxamlRenderResult Render<TComponent>(IServiceProvider? services = null)
        where TComponent : ComponentInstance
    {
        return Render(typeof(TComponent), services);
    }

    /// <summary>
    /// Renders an existing root component instance with a configured service collection.
    /// </summary>
    /// <param name="root">The root component instance to render.</param>
    /// <param name="configureServices">A callback used to configure services for the component tree.</param>
    /// <returns>A render result used to inspect and interact with the tree.</returns>
    public static CsxamlRenderResult Render(ComponentInstance root, Action<ServiceCollection> configureServices)
    {
        return Render(root, BuildServices(configureServices));
    }

    /// <summary>
    /// Activates and renders a root component type with a configured service collection.
    /// </summary>
    /// <param name="rootComponentType">The component type to instantiate as the root.</param>
    /// <param name="configureServices">A callback used to configure services for the component tree.</param>
    /// <returns>A render result used to inspect and interact with the tree.</returns>
    public static CsxamlRenderResult Render(Type rootComponentType, Action<ServiceCollection> configureServices)
    {
        return Render(rootComponentType, BuildServices(configureServices));
    }

    /// <summary>
    /// Activates and renders a root component type with a configured service collection.
    /// </summary>
    /// <typeparam name="TComponent">The component type to instantiate as the root.</typeparam>
    /// <param name="configureServices">A callback used to configure services for the component tree.</param>
    /// <returns>A render result used to inspect and interact with the tree.</returns>
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
