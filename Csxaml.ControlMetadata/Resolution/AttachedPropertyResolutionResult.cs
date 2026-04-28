namespace Csxaml.ControlMetadata;

/// <summary>
/// Represents the resolved metadata or failure state for an attached-property reference.
/// </summary>
/// <param name="Kind">The resolution outcome.</param>
/// <param name="Property">The matched attached property when resolution succeeded; otherwise, <see langword="null"/>.</param>
public sealed record AttachedPropertyResolutionResult(
    AttachedPropertyResolutionKind Kind,
    AttachedPropertyMetadata? Property);
