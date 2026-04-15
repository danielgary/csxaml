namespace Csxaml.Runtime.Tests;

internal sealed class MountedProbeHostComponent : ComponentInstance
{
    public MountedProbeHostComponent()
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
                new ComponentNode(typeof(MountedProbeChildComponent), null, "mounted-child", "stable")
            ]);
    }
}
