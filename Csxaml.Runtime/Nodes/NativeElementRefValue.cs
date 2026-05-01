namespace Csxaml.Runtime;

/// <summary>
/// Represents a reference assignment on a native runtime node.
/// </summary>
/// <param name="Reference">The element reference object to update.</param>
/// <param name="SourceInfo">Source-location metadata for the ref assignment, when available.</param>
public sealed record NativeElementRefValue(
    ElementRef Reference,
    CsxamlSourceInfo? SourceInfo = null);
