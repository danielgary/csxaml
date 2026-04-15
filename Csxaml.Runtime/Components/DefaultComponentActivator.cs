using Microsoft.Extensions.DependencyInjection;

namespace Csxaml.Runtime;

internal sealed class DefaultComponentActivator : IComponentActivator
{
    public ComponentInstance CreateComponent(Type componentType, ComponentContext context)
    {
        if (ActivatorUtilities.CreateInstance(context.Services, componentType) is not ComponentInstance instance)
        {
            throw new InvalidOperationException(
                $"Type '{componentType.FullName}' is not a component instance.");
        }

        instance.Initialize(context);
        return instance;
    }
}
