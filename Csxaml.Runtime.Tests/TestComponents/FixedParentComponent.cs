namespace Csxaml.Runtime.Tests;

internal sealed class FixedParentComponent : ComponentInstance
{
    public FixedParentComponent()
    {
        Version = new Csxaml.Runtime.State<int>(0, () => RequestRender?.Invoke());
    }

    public Csxaml.Runtime.State<int> Version { get; }

    public override Node Render()
    {
        return new NativeElementNode(
            "StackPanel",
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeEventValue>(),
            [
                new NativeElementNode(
                    "TextBlock",
                    null,
                    [new NativePropertyValue("Text", $"Version:{Version.Value}")],
                    Array.Empty<NativeEventValue>(),
                    Array.Empty<Node>()),
                new ComponentNode(typeof(CardViewComponent), new CardViewProps("Fixed", false), "fixed-child", null)
            ]);
    }
}
