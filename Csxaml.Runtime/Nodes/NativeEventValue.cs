namespace Csxaml.Runtime;

public sealed record NativeEventValue(
    string Name,
    Delegate Handler,
    ValueKindHint ValueKindHint)
{
    public NativeEventValue(string name, Delegate handler)
        : this(name, handler, ValueKindHint.Unknown)
    {
    }
}
