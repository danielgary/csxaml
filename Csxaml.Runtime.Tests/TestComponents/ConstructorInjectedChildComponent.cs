namespace Csxaml.Runtime.Tests;

internal sealed class ConstructorInjectedChildComponent : ComponentInstance
{
    private readonly GreetingService _greeting;

    public ConstructorInjectedChildComponent(GreetingService greeting)
    {
        _greeting = greeting;
    }

    public override Node Render()
    {
        return new NativeElementNode(
            "TextBlock",
            null,
            [new NativePropertyValue("Text", $"Child:{_greeting.Text}")],
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());
    }
}
