namespace Csxaml.Runtime;

public sealed record NativeEventValue(
    string Name,
    Delegate Handler,
    ValueKindHint ValueKindHint,
    CsxamlSourceInfo? SourceInfo = null)
{
    public NativeEventValue(string name, Delegate handler)
        : this(name, handler, ValueKindHint.Unknown, null)
    {
    }
}
