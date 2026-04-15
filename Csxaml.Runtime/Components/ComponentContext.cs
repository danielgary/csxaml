namespace Csxaml.Runtime;

internal sealed class ComponentContext
{
    public ComponentContext(IServiceProvider? services = null)
    {
        Services = services ?? NullServiceProvider.Instance;
    }

    public IServiceProvider Services { get; }
}
