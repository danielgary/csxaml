namespace Csxaml.Runtime;

internal interface IComponentActivator
{
    ComponentInstance CreateComponent(Type componentType, ComponentContext context);
}
