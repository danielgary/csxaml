namespace Csxaml.Runtime;

public sealed record NativePropertyValue(
    string Name,
    object? Value,
    ValueKindHint ValueKindHint)
{
    public NativePropertyValue(string name, object? value)
        : this(name, value, ValueKindHint.Unknown)
    {
    }
}
