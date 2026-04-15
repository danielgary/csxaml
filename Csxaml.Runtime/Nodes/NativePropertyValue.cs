namespace Csxaml.Runtime;

public sealed record NativePropertyValue(
    string Name,
    object? Value,
    ValueKindHint ValueKindHint,
    CsxamlSourceInfo? SourceInfo = null)
{
    public NativePropertyValue(string name, object? value)
        : this(name, value, ValueKindHint.Unknown, null)
    {
    }
}
