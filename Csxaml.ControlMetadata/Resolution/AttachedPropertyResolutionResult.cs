namespace Csxaml.ControlMetadata;

public sealed record AttachedPropertyResolutionResult(
    AttachedPropertyResolutionKind Kind,
    AttachedPropertyMetadata? Property);
