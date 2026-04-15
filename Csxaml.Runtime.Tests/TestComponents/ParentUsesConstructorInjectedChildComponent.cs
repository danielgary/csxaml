namespace Csxaml.Runtime.Tests;

internal sealed class ParentUsesConstructorInjectedChildComponent : ComponentInstance
{
    public override Node Render()
    {
        return new NativeElementNode(
            "StackPanel",
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeEventValue>(),
            [new ComponentNode(typeof(ConstructorInjectedChildComponent), null, "child", null)]);
    }
}
