namespace Csxaml.Runtime.Tests;

internal sealed class AsyncDisposableProbeComponent : ComponentInstance, IAsyncDisposable
{
    public static int DisposeAsyncCount { get; private set; }

    public static void Reset()
    {
        DisposeAsyncCount = 0;
    }

    public ValueTask DisposeAsync()
    {
        DisposeAsyncCount++;
        return ValueTask.CompletedTask;
    }

    public override Node Render()
    {
        return new NativeElementNode(
            "TextBlock",
            null,
            [new NativePropertyValue("Text", "Async")],
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());
    }
}
