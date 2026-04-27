namespace Csxaml.ControlMetadata;

/// <summary>
/// Describes the outcome of resolving an attached-property reference.
/// </summary>
public enum AttachedPropertyResolutionKind
{
    /// <summary>
    /// A single attached property matched the reference.
    /// </summary>
    Resolved,

    /// <summary>
    /// No attached property matched the reference.
    /// </summary>
    Unknown,

    /// <summary>
    /// More than one attached property matched the reference.
    /// </summary>
    Ambiguous
}
