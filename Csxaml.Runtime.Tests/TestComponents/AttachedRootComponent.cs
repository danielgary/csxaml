namespace Csxaml.Runtime.Tests;

internal sealed class AttachedRootComponent : ComponentInstance
{
    public override Node Render()
    {
        return new NativeElementNode(
            "Border",
            null,
            Array.Empty<NativePropertyValue>(),
            [
                new NativeAttachedPropertyValue(
                    "AutomationProperties",
                    "Name",
                    "Inner Editor",
                    ValueKindHint.String)
            ],
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());
    }
}
