namespace Csxaml.Tooling.Core.Hover;

/// <summary>
/// Describes hover content and the source range it applies to.
/// </summary>
/// <param name="Start">The zero-based start offset of the hover range.</param>
/// <param name="Length">The length of the hover range.</param>
/// <param name="Markdown">The markdown content shown by the editor.</param>
public sealed record CsxamlHoverInfo(
    int Start,
    int Length,
    string Markdown);
