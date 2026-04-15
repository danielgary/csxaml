namespace Csxaml.Runtime.Tests;

internal sealed class MountedProbeChildComponent : ComponentInstance
{
    public MountedProbeChildComponent()
    {
        LastCreated = this;
    }

    public static MountedProbeChildComponent? LastCreated { get; private set; }

    public int MountCount { get; private set; }

    public static void Reset()
    {
        LastCreated = null;
    }

    protected override void OnMounted()
    {
        MountCount++;
    }

    public override Node Render()
    {
        return new NativeElementNode(
            "TextBlock",
            null,
            [new NativePropertyValue("Text", $"Mounted:{MountCount}")],
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());
    }
}
