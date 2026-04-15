namespace Csxaml.Runtime.Tests;

internal sealed class ConstructorInjectedRootComponent : ComponentInstance
{
    private readonly GreetingService _greeting;

    public ConstructorInjectedRootComponent(GreetingService greeting)
    {
        _greeting = greeting;
    }

    public override Node Render()
    {
        return new NativeElementNode(
            "TextBlock",
            null,
            [new NativePropertyValue("Text", _greeting.Text)],
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());
    }
}
