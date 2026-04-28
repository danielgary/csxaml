namespace Csxaml.Runtime;

/// <summary>
/// Represents a property assignment on a native runtime node.
/// </summary>
/// <param name="Name">The property name exposed to CSXAML markup.</param>
/// <param name="Value">The value to assign to the native property.</param>
/// <param name="ValueKindHint">A hint used when converting the value for WinUI.</param>
/// <param name="SourceInfo">Source-location metadata for the property assignment, when available.</param>
public sealed record NativePropertyValue(
    string Name,
    object? Value,
    ValueKindHint ValueKindHint,
    CsxamlSourceInfo? SourceInfo = null)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NativePropertyValue"/> class with an unknown value kind.
    /// </summary>
    /// <param name="name">The property name exposed to CSXAML markup.</param>
    /// <param name="value">The value to assign to the native property.</param>
    public NativePropertyValue(string name, object? value)
        : this(name, value, ValueKindHint.Unknown, null)
    {
    }
}
