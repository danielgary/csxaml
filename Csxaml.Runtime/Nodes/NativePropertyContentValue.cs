namespace Csxaml.Runtime;

/// <summary>
/// Represents child nodes assigned through native property-content syntax.
/// </summary>
/// <param name="Name">The native property name receiving the content.</param>
/// <param name="Children">The runtime nodes assigned to the property.</param>
/// <param name="SourceInfo">Source-location metadata for diagnostics, when available.</param>
public sealed record NativePropertyContentValue(
    string Name,
    IReadOnlyList<Node> Children,
    CsxamlSourceInfo? SourceInfo = null);
