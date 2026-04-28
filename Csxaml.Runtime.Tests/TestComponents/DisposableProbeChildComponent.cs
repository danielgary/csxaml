namespace Csxaml.Runtime.Tests;

internal sealed class DisposableProbeChildComponent : ComponentInstance, IDisposable
{
    private static int _nextId;
    private static readonly List<int> DisposedIds = [];

    public DisposableProbeChildComponent()
    {
        InstanceId = ++_nextId;
        LastCreated = this;
        Count = new Csxaml.Runtime.State<int>(0, InvalidateState, ValidateStateWrite);
    }

    public static int DisposeCount { get; private set; }

    public static DisposableProbeChildComponent? LastCreated { get; private set; }

    public int InstanceId { get; }

    public Csxaml.Runtime.State<int> Count { get; }

    public static string DisposalTrace => string.Join(",", DisposedIds);

    public static void Reset()
    {
        _nextId = 0;
        DisposedIds.Clear();
        DisposeCount = 0;
        LastCreated = null;
    }

    public void Dispose()
    {
        DisposeCount++;
        DisposedIds.Add(InstanceId);
    }

    public override Node Render()
    {
        return new NativeElementNode(
            "TextBlock",
            null,
            [new NativePropertyValue("Text", $"Count:{Count.Value}")],
            Array.Empty<NativeEventValue>(),
            Array.Empty<Node>());
    }
}
