namespace Csxaml.Runtime;

/// <summary>
/// Represents an attached property assignment on a native runtime node.
/// </summary>
/// <param name="OwnerName">The simple owner name used in markup.</param>
/// <param name="PropertyName">The unqualified attached property name.</param>
/// <param name="Value">The value to assign to the attached property.</param>
/// <param name="ValueKindHint">A hint used when converting the value for WinUI.</param>
/// <param name="SourceInfo">Source-location metadata for the assignment, when available.</param>
public sealed record NativeAttachedPropertyValue(
    string OwnerName,
    string PropertyName,
    object? Value,
    ValueKindHint ValueKindHint,
    CsxamlSourceInfo? SourceInfo = null)
{
    /// <summary>
    /// Gets the markup-qualified attached property name, such as <c>Grid.Row</c>.
    /// </summary>
    public string QualifiedName => $"{OwnerName}.{PropertyName}";
}
