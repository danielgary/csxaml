namespace Csxaml.Samples.FeatureGallery;

internal sealed class NativeWheelMessageProbeSession : IDisposable
{
    private readonly NativeWheelMessageCounter childCounter = new("child");
    private readonly NativeWheelMessageCounter rootCounter = new("root");
    private readonly NativeWheelWindowSubclass childSubclass;
    private readonly NativeWheelWindowSubclass rootSubclass;
    private readonly Action changed;

    public NativeWheelMessageProbeSession(
        IntPtr rootHwnd,
        IntPtr childHwnd,
        Action changed)
    {
        RootHwnd = rootHwnd;
        ChildHwnd = childHwnd;
        this.changed = changed;
        rootSubclass = new NativeWheelWindowSubclass(rootHwnd, RecordRootMessage);
        childSubclass = new NativeWheelWindowSubclass(childHwnd, RecordChildMessage);
    }

    public IntPtr RootHwnd { get; }

    public IntPtr ChildHwnd { get; }

    public string Status => $"Native wheel messages: {rootCounter}; {childCounter}; {DescribeLastMessage()}";

    public bool Matches(IntPtr rootHwnd, IntPtr childHwnd)
    {
        return RootHwnd == rootHwnd && ChildHwnd == childHwnd;
    }

    public void Reset()
    {
        rootCounter.Reset();
        childCounter.Reset();
    }

    public void Dispose()
    {
        childSubclass.Dispose();
        rootSubclass.Dispose();
    }

    private void RecordRootMessage(NativeWheelMessage message)
    {
        if (rootCounter.TryRecord(message))
        {
            changed();
        }
    }

    private void RecordChildMessage(NativeWheelMessage message)
    {
        if (childCounter.TryRecord(message))
        {
            changed();
        }
    }

    private string DescribeLastMessage()
    {
        var lastMessage = childCounter.LastMessage ?? rootCounter.LastMessage;
        return lastMessage is { } message
            ? NativeWheelWindowDiagnostics.Describe(message, RootHwnd, ChildHwnd)
            : "last=<none>";
    }
}
