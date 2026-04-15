namespace Csxaml.Runtime.Tests;

internal sealed class DisposableProbeHostComponent : ComponentInstance
{
    public bool ShowChild { get; set; } = true;

    public override Node Render()
    {
        var children = new List<Node>();
        if (ShowChild)
        {
            children.Add(new ComponentNode(typeof(DisposableProbeChildComponent), null, "disposable-child", "stable"));
        }

        return new NativeElementNode(
            "StackPanel",
            null,
            Array.Empty<NativePropertyValue>(),
            Array.Empty<NativeEventValue>(),
            children);
    }
}
