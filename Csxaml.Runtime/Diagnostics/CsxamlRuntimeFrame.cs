namespace Csxaml.Runtime;

/// <summary>
/// Describes one stage in a CSXAML runtime exception path.
/// </summary>
/// <param name="Stage">The runtime stage that was executing when the frame was added.</param>
/// <param name="ComponentName">The related component name, or <see langword="null"/> when the frame is not component-specific.</param>
/// <param name="SourceInfo">The source location related to the frame, when available.</param>
/// <param name="Detail">Additional concise diagnostic detail for the frame.</param>
public sealed record CsxamlRuntimeFrame(
    string Stage,
    string? ComponentName,
    CsxamlSourceInfo? SourceInfo,
    string? Detail);
