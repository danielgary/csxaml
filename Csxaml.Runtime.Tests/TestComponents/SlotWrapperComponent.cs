namespace Csxaml.Runtime.Tests;

internal sealed class SlotWrapperComponent : ComponentInstance<SlotWrapperProps>
{
    public override Node Render()
    {
        var children = new List<Node>
        {
            new NativeElementNode(
                "TextBlock",
                null,
                [new NativePropertyValue("Text", Props.Heading)],
                Array.Empty<NativeEventValue>(),
                Array.Empty<Node>())
        };

        children.AddRange(ChildContent);
        return new NativeElementNode(
            "StackPanel",
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeEventValue>(),
            children);
    }
}
